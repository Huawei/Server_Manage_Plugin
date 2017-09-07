# Copyright 2013 Red Hat, Inc.
# All Rights Reserved.
#
#    Licensed under the Apache License, Version 2.0 (the "License"); you may
#    not use this file except in compliance with the License. You may obtain
#    a copy of the License at
#
#         http://www.apache.org/licenses/LICENSE-2.0
#
#    Unless required by applicable law or agreed to in writing, software
#    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
#    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
#    License for the specific language governing permissions and limitations
#    under the License.

import json

import mock
from oslo_config import cfg
from six.moves import http_client
from testtools import matchers

from ironic.api.controllers import base as api_base
from ironic.api.controllers.v1 import driver
from ironic.common import exception
from ironic.conductor import rpcapi
from ironic.drivers import base as driver_base
from ironic.tests.unit.api import base


class TestListDrivers(base.BaseApiTest):
    d1 = 'fake-driver1'
    d2 = 'fake-driver2'
    d3 = 'fake-hardware-type'
    h1 = 'fake-host1'
    h2 = 'fake-host2'

    def register_fake_conductors(self):
        c1 = self.dbapi.register_conductor({
            'hostname': self.h1,
            'drivers': [self.d1, self.d2],
        })
        c2 = self.dbapi.register_conductor({
            'hostname': self.h2,
            'drivers': [self.d2],
        })
        for c in (c1, c2):
            self.dbapi.register_conductor_hardware_interfaces(
                c.id, self.d3, 'deploy', ['iscsi', 'direct'], 'direct')

    def _test_drivers(self, use_dynamic, detail=False, storage_if=False):
        self.register_fake_conductors()
        headers = {}
        expected = [
            {'name': self.d1, 'hosts': [self.h1], 'type': 'classic'},
            {'name': self.d2, 'hosts': [self.h1, self.h2], 'type': 'classic'},
            {'name': self.d3, 'hosts': [self.h1, self.h2], 'type': 'dynamic'},
        ]
        expected = sorted(expected, key=lambda d: d['name'])
        if use_dynamic:
            if storage_if:
                headers[api_base.Version.string] = '1.33'
            else:
                headers[api_base.Version.string] = '1.30'

        path = '/drivers'
        if detail:
            path += '?detail=True'
        data = self.get_json(path, headers=headers)

        self.assertEqual(len(expected), len(data['drivers']))
        drivers = sorted(data['drivers'], key=lambda d: d['name'])
        for i in range(len(expected)):
            d = drivers[i]
            e = expected[i]

            self.assertEqual(e['name'], d['name'])
            self.assertEqual(sorted(e['hosts']), sorted(d['hosts']))
            self.validate_link(d['links'][0]['href'])
            self.validate_link(d['links'][1]['href'])

            if use_dynamic:
                self.assertEqual(e['type'], d['type'])

            # NOTE(jroll) we don't test detail=True with use_dynamic=False
            # as this case can't actually happen.
            if detail:
                self.assertIn('default_deploy_interface', d)
                if storage_if:
                    self.assertIn('default_storage_interface', d)
                    self.assertIn('enabled_storage_interfaces', d)
                else:
                    self.assertNotIn('default_storage_interface', d)
                    self.assertNotIn('enabled_storage_interfaces', d)
            else:
                # ensure we don't spill these fields into driver listing
                # one should be enough
                self.assertNotIn('default_deploy_interface', d)

    def test_drivers(self):
        self._test_drivers(False)

    def test_drivers_with_dynamic(self):
        self._test_drivers(True)

    def _test_drivers_with_dynamic_detailed(self, storage_if=False):
        with mock.patch.object(self.dbapi, 'list_hardware_type_interfaces',
                               autospec=True) as mock_hw:
            mock_hw.return_value = [
                {
                    'hardware_type': self.d3,
                    'interface_type': 'deploy',
                    'interface_name': 'iscsi',
                    'default': False,
                },
                {
                    'hardware_type': self.d3,
                    'interface_type': 'deploy',
                    'interface_name': 'direct',
                    'default': True,
                },
            ]

            self._test_drivers(True, detail=True, storage_if=storage_if)

    def test_drivers_with_dynamic_detailed(self):
        self._test_drivers_with_dynamic_detailed()

    def test_drivers_with_dynamic_detailed_storage_interface(self):
        self._test_drivers_with_dynamic_detailed(storage_if=True)

    def _test_drivers_type_filter(self, requested_type):
        self.register_fake_conductors()
        headers = {api_base.Version.string: '1.30'}
        data = self.get_json('/drivers?type=%s' % requested_type,
                             headers=headers)
        for d in data['drivers']:
            # just check it's the right type, other tests handle the rest
            self.assertEqual(requested_type, d['type'])

    def test_drivers_type_filter_classic(self):
        self._test_drivers_type_filter('classic')

    def test_drivers_type_filter_dynamic(self):
        self._test_drivers_type_filter('dynamic')

    def test_drivers_type_filter_bad_version(self):
        headers = {api_base.Version.string: '1.29'}
        data = self.get_json('/drivers?type=classic',
                             headers=headers,
                             expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, data.status_code)

    def test_drivers_type_filter_bad_value(self):
        headers = {api_base.Version.string: '1.30'}
        data = self.get_json('/drivers?type=working',
                             headers=headers,
                             expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, data.status_code)

    def test_drivers_detail_bad_version(self):
        headers = {api_base.Version.string: '1.29'}
        data = self.get_json('/drivers?detail=True',
                             headers=headers,
                             expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, data.status_code)

    def test_drivers_detail_bad_version_false(self):
        headers = {api_base.Version.string: '1.29'}
        data = self.get_json('/drivers?detail=False',
                             headers=headers,
                             expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, data.status_code)

    def test_drivers_no_active_conductor(self):
        data = self.get_json('/drivers')
        self.assertThat(data['drivers'], matchers.HasLength(0))
        self.assertEqual([], data['drivers'])

    @mock.patch.object(rpcapi.ConductorAPI, 'get_driver_properties')
    def _test_drivers_get_one_ok(self, use_dynamic, mock_driver_properties,
                                 storage_if=False):
        # get_driver_properties mock is required by validate_link()
        self.register_fake_conductors()

        if use_dynamic:
            driver = self.d3
            driver_type = 'dynamic'
            hosts = [self.h1, self.h2]
        else:
            driver = self.d1
            driver_type = 'classic'
            hosts = [self.h1]

        headers = {}
        if storage_if:
            headers[api_base.Version.string] = '1.33'
        else:
            headers[api_base.Version.string] = '1.30'

        data = self.get_json('/drivers/%s' % driver,
                             headers=headers)

        self.assertEqual(driver, data['name'])
        self.assertEqual(sorted(hosts), sorted(data['hosts']))
        self.assertIn('properties', data)
        self.assertEqual(driver_type, data['type'])

        if use_dynamic:
            for iface in driver_base.ALL_INTERFACES:
                if storage_if or iface != 'storage':
                    self.assertIn('default_%s_interface' % iface, data)
                    self.assertIn('enabled_%s_interfaces' % iface, data)
            self.assertIsNotNone(data['default_deploy_interface'])
            self.assertIsNotNone(data['enabled_deploy_interfaces'])
        else:
            self.assertIsNone(data['default_deploy_interface'])
            self.assertIsNone(data['enabled_deploy_interfaces'])

        self.validate_link(data['links'][0]['href'])
        self.validate_link(data['links'][1]['href'])
        self.validate_link(data['properties'][0]['href'])
        self.validate_link(data['properties'][1]['href'])

    def test_drivers_get_one_ok_classic(self):
        self._test_drivers_get_one_ok(False)

    def _test_drivers_get_one_ok_dynamic(self, storage_if=False):
        with mock.patch.object(self.dbapi, 'list_hardware_type_interfaces',
                               autospec=True) as mock_hw:
            mock_hw.return_value = [
                {
                    'hardware_type': self.d3,
                    'interface_type': 'deploy',
                    'interface_name': 'iscsi',
                    'default': False,
                },
                {
                    'hardware_type': self.d3,
                    'interface_type': 'deploy',
                    'interface_name': 'direct',
                    'default': True,
                },
            ]

            self._test_drivers_get_one_ok(True, storage_if=storage_if)
            mock_hw.assert_called_once_with([self.d3])

    def test_drivers_get_one_ok_dynamic(self):
        self._test_drivers_get_one_ok_dynamic()

    def test_drivers_get_one_ok_dynamic_storage_interface(self):
        self._test_drivers_get_one_ok_dynamic(storage_if=True)

    def test_driver_properties_hidden_in_lower_version(self):
        self.register_fake_conductors()
        data = self.get_json('/drivers/%s' % self.d1,
                             headers={api_base.Version.string: '1.8'})
        self.assertNotIn('properties', data)

    def test_driver_type_hidden_in_lower_version(self):
        self.register_fake_conductors()
        data = self.get_json('/drivers/%s' % self.d1,
                             headers={api_base.Version.string: '1.14'})
        self.assertNotIn('type', data)

    def test_drivers_get_one_not_found(self):
        response = self.get_json('/drivers/%s' % self.d1, expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def _test_links(self, public_url=None):
        cfg.CONF.set_override('public_endpoint', public_url, 'api')
        self.register_fake_conductors()
        data = self.get_json('/drivers/%s' % self.d1)
        self.assertIn('links', data)
        self.assertEqual(2, len(data['links']))
        self.assertIn(self.d1, data['links'][0]['href'])
        for l in data['links']:
            bookmark = l['rel'] == 'bookmark'
            self.assertTrue(self.validate_link(l['href'], bookmark=bookmark))

        if public_url is not None:
            expected = [{'href': '%s/v1/drivers/%s' % (public_url, self.d1),
                         'rel': 'self'},
                        {'href': '%s/drivers/%s' % (public_url, self.d1),
                         'rel': 'bookmark'}]
            for i in expected:
                self.assertIn(i, data['links'])

    def test_links(self):
        self._test_links()

    def test_links_public_url(self):
        self._test_links(public_url='http://foo')

    @mock.patch.object(rpcapi.ConductorAPI, 'driver_vendor_passthru')
    def test_driver_vendor_passthru_sync(self, mocked_driver_vendor_passthru):
        self.register_fake_conductors()
        mocked_driver_vendor_passthru.return_value = {
            'return': {'return_key': 'return_value'},
            'async': False,
            'attach': False}
        response = self.post_json(
            '/drivers/%s/vendor_passthru/do_test' % self.d1,
            {'test_key': 'test_value'})
        self.assertEqual(http_client.OK, response.status_int)
        self.assertEqual(mocked_driver_vendor_passthru.return_value['return'],
                         response.json)

    @mock.patch.object(rpcapi.ConductorAPI, 'driver_vendor_passthru')
    def test_driver_vendor_passthru_async(self, mocked_driver_vendor_passthru):
        self.register_fake_conductors()
        mocked_driver_vendor_passthru.return_value = {'return': None,
                                                      'async': True,
                                                      'attach': False}
        response = self.post_json(
            '/drivers/%s/vendor_passthru/do_test' % self.d1,
            {'test_key': 'test_value'})
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertIsNone(mocked_driver_vendor_passthru.return_value['return'])

    @mock.patch.object(rpcapi.ConductorAPI, 'driver_vendor_passthru')
    def test_driver_vendor_passthru_put(self, mocked_driver_vendor_passthru):
        self.register_fake_conductors()
        return_value = {'return': None, 'async': True, 'attach': False}
        mocked_driver_vendor_passthru.return_value = return_value
        response = self.put_json(
            '/drivers/%s/vendor_passthru/do_test' % self.d1,
            {'test_key': 'test_value'})
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(return_value['return'], response.json)

    @mock.patch.object(rpcapi.ConductorAPI, 'driver_vendor_passthru')
    def test_driver_vendor_passthru_get(self, mocked_driver_vendor_passthru):
        self.register_fake_conductors()
        return_value = {'return': 'foo', 'async': False, 'attach': False}
        mocked_driver_vendor_passthru.return_value = return_value
        response = self.get_json(
            '/drivers/%s/vendor_passthru/do_test' % self.d1)
        self.assertEqual(return_value['return'], response)

    @mock.patch.object(rpcapi.ConductorAPI, 'driver_vendor_passthru')
    def test_driver_vendor_passthru_delete(self, mock_driver_vendor_passthru):
        self.register_fake_conductors()
        return_value = {'return': None, 'async': True, 'attach': False}
        mock_driver_vendor_passthru.return_value = return_value
        response = self.delete(
            '/drivers/%s/vendor_passthru/do_test' % self.d1)
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(return_value['return'], response.json)

    def test_driver_vendor_passthru_driver_not_found(self):
        # tests when given driver is not found
        # e.g. get_topic_for_driver fails to find the driver
        response = self.post_json(
            '/drivers/%s/vendor_passthru/do_test' % self.d1,
            {'test_key': 'test_value'},
            expect_errors=True)

        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_driver_vendor_passthru_method_not_found(self):
        response = self.post_json(
            '/drivers/%s/vendor_passthru' % self.d1,
            {'test_key': 'test_value'},
            expect_errors=True)

        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        error = json.loads(response.json['error_message'])
        self.assertEqual('Missing argument: "method"',
                         error['faultstring'])

    @mock.patch.object(rpcapi.ConductorAPI,
                       'get_driver_vendor_passthru_methods')
    def test_driver_vendor_passthru_methods(self, get_methods_mock):
        self.register_fake_conductors()
        return_value = {'foo': 'bar'}
        get_methods_mock.return_value = return_value
        path = '/drivers/%s/vendor_passthru/methods' % self.d1

        data = self.get_json(path)
        self.assertEqual(return_value, data)
        get_methods_mock.assert_called_once_with(mock.ANY, self.d1,
                                                 topic=mock.ANY)

        # Now let's test the cache: Reset the mock
        get_methods_mock.reset_mock()

        # Call it again
        data = self.get_json(path)
        self.assertEqual(return_value, data)
        # Assert RPC method wasn't called this time
        self.assertFalse(get_methods_mock.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'get_raid_logical_disk_properties')
    def test_raid_logical_disk_properties(self, disk_prop_mock):
        driver._RAID_PROPERTIES = {}
        self.register_fake_conductors()
        properties = {'foo': 'description of foo'}
        disk_prop_mock.return_value = properties
        path = '/drivers/%s/raid/logical_disk_properties' % self.d1
        data = self.get_json(path,
                             headers={api_base.Version.string: "1.12"})
        self.assertEqual(properties, data)
        disk_prop_mock.assert_called_once_with(mock.ANY, self.d1,
                                               topic=mock.ANY)

    @mock.patch.object(rpcapi.ConductorAPI, 'get_raid_logical_disk_properties')
    def test_raid_logical_disk_properties_older_version(self, disk_prop_mock):
        driver._RAID_PROPERTIES = {}
        self.register_fake_conductors()
        properties = {'foo': 'description of foo'}
        disk_prop_mock.return_value = properties
        path = '/drivers/%s/raid/logical_disk_properties' % self.d1
        ret = self.get_json(path,
                            headers={api_base.Version.string: "1.4"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'get_raid_logical_disk_properties')
    def test_raid_logical_disk_properties_cached(self, disk_prop_mock):
        # only one RPC-conductor call will be made and the info cached
        # for subsequent requests
        driver._RAID_PROPERTIES = {}
        self.register_fake_conductors()
        properties = {'foo': 'description of foo'}
        disk_prop_mock.return_value = properties
        path = '/drivers/%s/raid/logical_disk_properties' % self.d1
        for i in range(3):
            data = self.get_json(path,
                                 headers={api_base.Version.string: "1.12"})
            self.assertEqual(properties, data)
        disk_prop_mock.assert_called_once_with(mock.ANY, self.d1,
                                               topic=mock.ANY)
        self.assertEqual(properties, driver._RAID_PROPERTIES[self.d1])

    @mock.patch.object(rpcapi.ConductorAPI, 'get_raid_logical_disk_properties')
    def test_raid_logical_disk_properties_iface_not_supported(
            self, disk_prop_mock):
        driver._RAID_PROPERTIES = {}
        self.register_fake_conductors()
        disk_prop_mock.side_effect = exception.UnsupportedDriverExtension(
            extension='raid', driver='fake')
        path = '/drivers/%s/raid/logical_disk_properties' % self.d1
        ret = self.get_json(path,
                            headers={api_base.Version.string: "1.12"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        disk_prop_mock.assert_called_once_with(mock.ANY, self.d1,
                                               topic=mock.ANY)


@mock.patch.object(rpcapi.ConductorAPI, 'get_driver_properties')
@mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for_driver')
class TestDriverProperties(base.BaseApiTest):

    def test_driver_properties_fake(self, mock_topic, mock_properties):
        # Can get driver properties for fake driver.
        driver._DRIVER_PROPERTIES = {}
        driver_name = 'fake'
        mock_topic.return_value = 'fake_topic'
        mock_properties.return_value = {'prop1': 'Property 1. Required.'}
        data = self.get_json('/drivers/%s/properties' % driver_name)
        self.assertEqual(mock_properties.return_value, data)
        mock_topic.assert_called_once_with(driver_name)
        mock_properties.assert_called_once_with(mock.ANY, driver_name,
                                                topic=mock_topic.return_value)
        self.assertEqual(mock_properties.return_value,
                         driver._DRIVER_PROPERTIES[driver_name])

    def test_driver_properties_hw_type(self, mock_topic, mock_properties):
        # Can get driver properties for manual-management hardware type
        driver._DRIVER_PROPERTIES = {}
        driver_name = 'manual-management'
        mock_topic.return_value = 'fake_topic'
        mock_properties.return_value = {'prop1': 'Property 1. Required.'}

        with mock.patch.object(self.dbapi, 'get_active_hardware_type_dict',
                               autospec=True) as mock_hw_type:
            mock_hw_type.return_value = {driver_name: 'fake_topic'}
            data = self.get_json('/drivers/%s/properties' % driver_name)

        self.assertEqual(mock_properties.return_value, data)
        mock_topic.assert_called_once_with(driver_name)
        mock_properties.assert_called_once_with(mock.ANY, driver_name,
                                                topic=mock_topic.return_value)
        self.assertEqual(mock_properties.return_value,
                         driver._DRIVER_PROPERTIES[driver_name])

    def test_driver_properties_cached(self, mock_topic, mock_properties):
        # only one RPC-conductor call will be made and the info cached
        # for subsequent requests
        driver._DRIVER_PROPERTIES = {}
        driver_name = 'fake'
        mock_topic.return_value = 'fake_topic'
        mock_properties.return_value = {'prop1': 'Property 1. Required.'}
        data = self.get_json('/drivers/%s/properties' % driver_name)
        data = self.get_json('/drivers/%s/properties' % driver_name)
        data = self.get_json('/drivers/%s/properties' % driver_name)
        self.assertEqual(mock_properties.return_value, data)
        mock_topic.assert_called_once_with(driver_name)
        mock_properties.assert_called_once_with(mock.ANY, driver_name,
                                                topic=mock_topic.return_value)
        self.assertEqual(mock_properties.return_value,
                         driver._DRIVER_PROPERTIES[driver_name])

    def test_driver_properties_invalid_driver_name(self, mock_topic,
                                                   mock_properties):
        # Cannot get driver properties for an invalid driver; no RPC topic
        # exists for it.
        driver._DRIVER_PROPERTIES = {}
        driver_name = 'bad_driver'
        mock_topic.side_effect = exception.DriverNotFound(
            driver_name=driver_name)
        mock_properties.return_value = {'prop1': 'Property 1. Required.'}
        ret = self.get_json('/drivers/%s/properties' % driver_name,
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_int)
        mock_topic.assert_called_once_with(driver_name)
        self.assertFalse(mock_properties.called)

    def test_driver_properties_cannot_load(self, mock_topic, mock_properties):
        # Cannot get driver properties for the driver. Although an RPC topic
        # exists for it, the conductor wasn't able to load it.
        driver._DRIVER_PROPERTIES = {}
        driver_name = 'driver'
        mock_topic.return_value = 'driver_topic'
        mock_properties.side_effect = exception.DriverNotFound(
            driver_name=driver_name)
        ret = self.get_json('/drivers/%s/properties' % driver_name,
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_int)
        mock_topic.assert_called_once_with(driver_name)
        mock_properties.assert_called_once_with(mock.ANY, driver_name,
                                                topic=mock_topic.return_value)
