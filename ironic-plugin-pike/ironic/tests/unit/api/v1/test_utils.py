# -*- encoding: utf-8 -*-
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

import mock
from oslo_config import cfg
from oslo_utils import uuidutils
import pecan
from six.moves import http_client
from webob import static
import wsme

from ironic.api.controllers.v1 import node as api_node
from ironic.api.controllers.v1 import utils
from ironic.common import exception
from ironic import objects
from ironic.tests import base
from ironic.tests.unit.api import utils as test_api_utils

CONF = cfg.CONF


class TestApiUtils(base.TestCase):

    def test_validate_limit(self):
        limit = utils.validate_limit(10)
        self.assertEqual(10, limit)

        # max limit
        limit = utils.validate_limit(999999999)
        self.assertEqual(CONF.api.max_limit, limit)

        # negative
        self.assertRaises(wsme.exc.ClientSideError, utils.validate_limit, -1)

        # zero
        self.assertRaises(wsme.exc.ClientSideError, utils.validate_limit, 0)

    def test_validate_sort_dir(self):
        sort_dir = utils.validate_sort_dir('asc')
        self.assertEqual('asc', sort_dir)

        # invalid sort_dir parameter
        self.assertRaises(wsme.exc.ClientSideError,
                          utils.validate_sort_dir,
                          'fake-sort')

    def test_get_patch_values_no_path(self):
        patch = [{'path': '/name', 'op': 'update', 'value': 'node-0'}]
        path = '/invalid'
        values = utils.get_patch_values(patch, path)
        self.assertEqual([], values)

    def test_get_patch_values_remove(self):
        patch = [{'path': '/name', 'op': 'remove'}]
        path = '/name'
        values = utils.get_patch_values(patch, path)
        self.assertEqual([], values)

    def test_get_patch_values_success(self):
        patch = [{'path': '/name', 'op': 'replace', 'value': 'node-x'}]
        path = '/name'
        values = utils.get_patch_values(patch, path)
        self.assertEqual(['node-x'], values)

    def test_get_patch_values_multiple_success(self):
        patch = [{'path': '/name', 'op': 'replace', 'value': 'node-x'},
                 {'path': '/name', 'op': 'replace', 'value': 'node-y'}]
        path = '/name'
        values = utils.get_patch_values(patch, path)
        self.assertEqual(['node-x', 'node-y'], values)

    def test_is_path_removed_success(self):
        patch = [{'path': '/name', 'op': 'remove'}]
        path = '/name'
        value = utils.is_path_removed(patch, path)
        self.assertTrue(value)

    def test_is_path_removed_subpath_success(self):
        patch = [{'path': '/local_link_connection/switch_id', 'op': 'remove'}]
        path = '/local_link_connection'
        value = utils.is_path_removed(patch, path)
        self.assertTrue(value)

    def test_is_path_removed_similar_subpath(self):
        patch = [{'path': '/local_link_connection_info/switch_id',
                  'op': 'remove'}]
        path = '/local_link_connection'
        value = utils.is_path_removed(patch, path)
        self.assertFalse(value)

    def test_is_path_removed_replace(self):
        patch = [{'path': '/name', 'op': 'replace', 'value': 'node-x'}]
        path = '/name'
        value = utils.is_path_removed(patch, path)
        self.assertFalse(value)

    def test_is_path_updated_success(self):
        patch = [{'path': '/name', 'op': 'remove'}]
        path = '/name'
        value = utils.is_path_updated(patch, path)
        self.assertTrue(value)

    def test_is_path_updated_subpath_success(self):
        patch = [{'path': '/properties/switch_id', 'op': 'add', 'value': 'id'}]
        path = '/properties'
        value = utils.is_path_updated(patch, path)
        self.assertTrue(value)

    def test_is_path_updated_similar_subpath(self):
        patch = [{'path': '/properties2/switch_id',
                  'op': 'replace', 'value': 'spam'}]
        path = '/properties'
        value = utils.is_path_updated(patch, path)
        self.assertFalse(value)

    def test_check_for_invalid_fields(self):
        requested = ['field_1', 'field_3']
        supported = ['field_1', 'field_2', 'field_3']
        utils.check_for_invalid_fields(requested, supported)

    def test_check_for_invalid_fields_fail(self):
        requested = ['field_1', 'field_4']
        supported = ['field_1', 'field_2', 'field_3']
        self.assertRaises(exception.InvalidParameterValue,
                          utils.check_for_invalid_fields,
                          requested, supported)

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_fields(self, mock_request):
        mock_request.version.minor = 8
        self.assertIsNone(utils.check_allow_specify_fields(['foo']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_fields_fail(self, mock_request):
        mock_request.version.minor = 7
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_specify_fields, ['foo'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_fields_network_interface(self, mock_request):
        mock_request.version.minor = 20
        self.assertIsNone(
            utils.check_allowed_fields(['network_interface']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_fields_network_interface_fail(self, mock_request):
        mock_request.version.minor = 19
        self.assertRaises(
            exception.NotAcceptable,
            utils.check_allowed_fields,
            ['network_interface'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_fields_resource_class(self, mock_request):
        mock_request.version.minor = 21
        self.assertIsNone(
            utils.check_allowed_fields(['resource_class']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_fields_resource_class_fail(self, mock_request):
        mock_request.version.minor = 20
        self.assertRaises(
            exception.NotAcceptable,
            utils.check_allowed_fields,
            ['resource_class'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_portgroup_fields_mode_properties(self,
                                                            mock_request):
        mock_request.version.minor = 26
        self.assertIsNone(
            utils.check_allowed_portgroup_fields(['mode']))
        self.assertIsNone(
            utils.check_allowed_portgroup_fields(['properties']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allowed_portgroup_fields_mode_properties_fail(self,
                                                                 mock_request):
        mock_request.version.minor = 25
        self.assertRaises(
            exception.NotAcceptable,
            utils.check_allowed_portgroup_fields,
            ['mode'])
        self.assertRaises(
            exception.NotAcceptable,
            utils.check_allowed_portgroup_fields,
            ['properties'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_driver(self, mock_request):
        mock_request.version.minor = 16
        self.assertIsNone(utils.check_allow_specify_driver(['fake']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_driver_fail(self, mock_request):
        mock_request.version.minor = 15
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_specify_driver, ['fake'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_resource_class(self, mock_request):
        mock_request.version.minor = 21
        self.assertIsNone(utils.check_allow_specify_resource_class(['foo']))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_specify_resource_class_fail(self, mock_request):
        mock_request.version.minor = 20
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_specify_resource_class, ['foo'])

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_filter_driver_type(self, mock_request):
        mock_request.version.minor = 30
        self.assertIsNone(utils.check_allow_filter_driver_type('classic'))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_filter_driver_type_none(self, mock_request):
        mock_request.version.minor = 29
        self.assertIsNone(utils.check_allow_filter_driver_type(None))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_filter_driver_type_fail(self, mock_request):
        mock_request.version.minor = 29
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_filter_driver_type, 'classic')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_driver_detail(self, mock_request):
        mock_request.version.minor = 30
        self.assertIsNone(utils.check_allow_driver_detail(True))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_driver_detail_false(self, mock_request):
        mock_request.version.minor = 30
        self.assertIsNone(utils.check_allow_driver_detail(False))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_driver_detail_none(self, mock_request):
        mock_request.version.minor = 29
        self.assertIsNone(utils.check_allow_driver_detail(None))

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_driver_detail_fail(self, mock_request):
        mock_request.version.minor = 29
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_driver_detail, True)

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_manage_verbs(self, mock_request):
        mock_request.version.minor = 4
        utils.check_allow_management_verbs('manage')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_manage_verbs_fail(self, mock_request):
        mock_request.version.minor = 3
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'manage')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_provide_verbs(self, mock_request):
        mock_request.version.minor = 4
        utils.check_allow_management_verbs('provide')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_provide_verbs_fail(self, mock_request):
        mock_request.version.minor = 3
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'provide')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_inspect_verbs(self, mock_request):
        mock_request.version.minor = 6
        utils.check_allow_management_verbs('inspect')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_inspect_verbs_fail(self, mock_request):
        mock_request.version.minor = 5
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'inspect')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_abort_verbs(self, mock_request):
        mock_request.version.minor = 13
        utils.check_allow_management_verbs('abort')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_abort_verbs_fail(self, mock_request):
        mock_request.version.minor = 12
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'abort')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_clean_verbs(self, mock_request):
        mock_request.version.minor = 15
        utils.check_allow_management_verbs('clean')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_clean_verbs_fail(self, mock_request):
        mock_request.version.minor = 14
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'clean')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_unknown_verbs(self, mock_request):
        utils.check_allow_management_verbs('rebuild')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_inject_nmi(self, mock_request):
        mock_request.version.minor = 29
        self.assertTrue(utils.allow_inject_nmi())
        mock_request.version.minor = 28
        self.assertFalse(utils.allow_inject_nmi())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_links_node_states_and_driver_properties(self, mock_request):
        mock_request.version.minor = 14
        self.assertTrue(utils.allow_links_node_states_and_driver_properties())
        mock_request.version.minor = 10
        self.assertFalse(utils.allow_links_node_states_and_driver_properties())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_adopt_verbs_fail(self, mock_request):
        mock_request.version.minor = 16
        self.assertRaises(exception.NotAcceptable,
                          utils.check_allow_management_verbs, 'adopt')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_check_allow_adopt_verbs(self, mock_request):
        mock_request.version.minor = 17
        utils.check_allow_management_verbs('adopt')

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_port_internal_info(self, mock_request):
        mock_request.version.minor = 18
        self.assertTrue(utils.allow_port_internal_info())
        mock_request.version.minor = 17
        self.assertFalse(utils.allow_port_internal_info())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_port_advanced_net_fields(self, mock_request):
        mock_request.version.minor = 19
        self.assertTrue(utils.allow_port_advanced_net_fields())
        mock_request.version.minor = 18
        self.assertFalse(utils.allow_port_advanced_net_fields())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_network_interface(self, mock_request):
        mock_request.version.minor = 20
        self.assertTrue(utils.allow_network_interface())
        mock_request.version.minor = 19
        self.assertFalse(utils.allow_network_interface())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_resource_class(self, mock_request):
        mock_request.version.minor = 21
        self.assertTrue(utils.allow_resource_class())
        mock_request.version.minor = 20
        self.assertFalse(utils.allow_resource_class())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_ramdisk_endpoints(self, mock_request):
        mock_request.version.minor = 22
        self.assertTrue(utils.allow_ramdisk_endpoints())
        mock_request.version.minor = 21
        self.assertFalse(utils.allow_ramdisk_endpoints())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_portgroups(self, mock_request):
        mock_request.version.minor = 23
        self.assertTrue(utils.allow_portgroups())
        mock_request.version.minor = 22
        self.assertFalse(utils.allow_portgroups())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_portgroups_subcontrollers(self, mock_request):
        mock_request.version.minor = 24
        self.assertTrue(utils.allow_portgroups_subcontrollers())
        mock_request.version.minor = 23
        self.assertFalse(utils.allow_portgroups_subcontrollers())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_remove_chassis_uuid(self, mock_request):
        mock_request.version.minor = 25
        self.assertTrue(utils.allow_remove_chassis_uuid())
        mock_request.version.minor = 24
        self.assertFalse(utils.allow_remove_chassis_uuid())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_portgroup_mode_properties(self, mock_request):
        mock_request.version.minor = 26
        self.assertTrue(utils.allow_portgroup_mode_properties())
        mock_request.version.minor = 25
        self.assertFalse(utils.allow_portgroup_mode_properties())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_dynamic_drivers(self, mock_request):
        mock_request.version.minor = 30
        self.assertTrue(utils.allow_dynamic_drivers())
        mock_request.version.minor = 29
        self.assertFalse(utils.allow_dynamic_drivers())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_volume(self, mock_request):
        mock_request.version.minor = 32
        self.assertTrue(utils.allow_volume())
        mock_request.version.minor = 31
        self.assertFalse(utils.allow_volume())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    def test_allow_storage_interface(self, mock_request):
        mock_request.version.minor = 33
        self.assertTrue(utils.allow_storage_interface())
        mock_request.version.minor = 32
        self.assertFalse(utils.allow_storage_interface())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    @mock.patch.object(objects.Port, 'supports_physical_network')
    def test_allow_port_physical_network_no_pin(self, mock_spn, mock_request):
        mock_spn.return_value = True
        mock_request.version.minor = 34
        self.assertTrue(utils.allow_port_physical_network())
        mock_request.version.minor = 33
        self.assertFalse(utils.allow_port_physical_network())

    @mock.patch.object(pecan, 'request', spec_set=['version'])
    @mock.patch.object(objects.Port, 'supports_physical_network')
    def test_allow_port_physical_network_pin(self, mock_spn, mock_request):
        mock_spn.return_value = False
        mock_request.version.minor = 34
        self.assertFalse(utils.allow_port_physical_network())
        mock_request.version.minor = 33
        self.assertFalse(utils.allow_port_physical_network())


class TestNodeIdent(base.TestCase):

    def setUp(self):
        super(TestNodeIdent, self).setUp()
        self.valid_name = 'my-host'
        self.valid_uuid = uuidutils.generate_uuid()
        self.invalid_name = 'Mr Plow'
        self.node = test_api_utils.post_get_test_node()

    @mock.patch.object(pecan, 'request')
    def test_allow_node_logical_names_pre_name(self, mock_pecan_req):
        mock_pecan_req.version.minor = 1
        self.assertFalse(utils.allow_node_logical_names())

    @mock.patch.object(pecan, 'request')
    def test_allow_node_logical_names_post_name(self, mock_pecan_req):
        mock_pecan_req.version.minor = 5
        self.assertTrue(utils.allow_node_logical_names())

    @mock.patch("pecan.request")
    def test_is_valid_node_name(self, mock_pecan_req):
        mock_pecan_req.version.minor = 10
        self.assertTrue(utils.is_valid_node_name(self.valid_name))
        self.assertFalse(utils.is_valid_node_name(self.invalid_name))
        self.assertFalse(utils.is_valid_node_name(self.valid_uuid))

    @mock.patch.object(pecan, 'request')
    @mock.patch.object(utils, 'allow_node_logical_names')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(objects.Node, 'get_by_name')
    def test_get_rpc_node_expect_uuid(self, mock_gbn, mock_gbu, mock_anln,
                                      mock_pr):
        mock_anln.return_value = True
        self.node['uuid'] = self.valid_uuid
        mock_gbu.return_value = self.node
        self.assertEqual(self.node, utils.get_rpc_node(self.valid_uuid))
        self.assertEqual(1, mock_gbu.call_count)
        self.assertEqual(0, mock_gbn.call_count)

    @mock.patch.object(pecan, 'request')
    @mock.patch.object(utils, 'allow_node_logical_names')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(objects.Node, 'get_by_name')
    def test_get_rpc_node_expect_name(self, mock_gbn, mock_gbu, mock_anln,
                                      mock_pr):
        mock_pr.version.minor = 10
        mock_anln.return_value = True
        self.node['name'] = self.valid_name
        mock_gbn.return_value = self.node
        self.assertEqual(self.node, utils.get_rpc_node(self.valid_name))
        self.assertEqual(0, mock_gbu.call_count)
        self.assertEqual(1, mock_gbn.call_count)

    @mock.patch.object(pecan, 'request')
    @mock.patch.object(utils, 'allow_node_logical_names')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(objects.Node, 'get_by_name')
    def test_get_rpc_node_invalid_name(self, mock_gbn, mock_gbu,
                                       mock_anln, mock_pr):
        mock_pr.version.minor = 10
        mock_anln.return_value = True
        self.assertRaises(exception.InvalidUuidOrName,
                          utils.get_rpc_node,
                          self.invalid_name)

    @mock.patch.object(pecan, 'request')
    @mock.patch.object(utils, 'allow_node_logical_names')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(objects.Node, 'get_by_name')
    def test_get_rpc_node_by_uuid_no_logical_name(self, mock_gbn, mock_gbu,
                                                  mock_anln, mock_pr):
        # allow_node_logical_name() should have no effect
        mock_anln.return_value = False
        self.node['uuid'] = self.valid_uuid
        mock_gbu.return_value = self.node
        self.assertEqual(self.node, utils.get_rpc_node(self.valid_uuid))
        self.assertEqual(1, mock_gbu.call_count)
        self.assertEqual(0, mock_gbn.call_count)

    @mock.patch.object(pecan, 'request')
    @mock.patch.object(utils, 'allow_node_logical_names')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(objects.Node, 'get_by_name')
    def test_get_rpc_node_by_name_no_logical_name(self, mock_gbn, mock_gbu,
                                                  mock_anln, mock_pr):
        mock_anln.return_value = False
        self.node['name'] = self.valid_name
        mock_gbn.return_value = self.node
        self.assertRaises(exception.NodeNotFound,
                          utils.get_rpc_node,
                          self.valid_name)


class TestVendorPassthru(base.TestCase):

    def test_method_not_specified(self):
        self.assertRaises(wsme.exc.ClientSideError,
                          utils.vendor_passthru, 'fake-ident',
                          None, 'fake-topic', data='fake-data')

    @mock.patch.object(pecan, 'request',
                       spec_set=['method', 'context', 'rpcapi'])
    def _vendor_passthru(self, mock_request, async=True,
                         driver_passthru=False):
        return_value = {'return': 'SpongeBob', 'async': async, 'attach': False}
        mock_request.method = 'post'
        mock_request.context = 'fake-context'

        passthru_mock = None
        if driver_passthru:
            passthru_mock = mock_request.rpcapi.driver_vendor_passthru
        else:
            passthru_mock = mock_request.rpcapi.vendor_passthru
        passthru_mock.return_value = return_value

        response = utils.vendor_passthru('fake-ident', 'squarepants',
                                         'fake-topic', data='fake-data',
                                         driver_passthru=driver_passthru)

        passthru_mock.assert_called_once_with(
            'fake-context', 'fake-ident', 'squarepants', 'POST',
            'fake-data', 'fake-topic')
        self.assertIsInstance(response, wsme.api.Response)
        self.assertEqual('SpongeBob', response.obj)
        self.assertEqual(response.return_type, wsme.types.Unset)
        sc = http_client.ACCEPTED if async else http_client.OK
        self.assertEqual(sc, response.status_code)

    def test_vendor_passthru_async(self):
        self._vendor_passthru()

    def test_vendor_passthru_sync(self):
        self._vendor_passthru(async=False)

    def test_driver_vendor_passthru_async(self):
        self._vendor_passthru(driver_passthru=True)

    def test_driver_vendor_passthru_sync(self):
        self._vendor_passthru(async=False, driver_passthru=True)

    @mock.patch.object(pecan, 'response', spec_set=['app_iter'])
    @mock.patch.object(pecan, 'request',
                       spec_set=['method', 'context', 'rpcapi'])
    def _test_vendor_passthru_attach(self, return_value, expct_return_value,
                                     mock_request, mock_response):
        return_ = {'return': return_value, 'async': False, 'attach': True}
        mock_request.method = 'get'
        mock_request.context = 'fake-context'
        mock_request.rpcapi.driver_vendor_passthru.return_value = return_
        response = utils.vendor_passthru('fake-ident', 'bar',
                                         'fake-topic', data='fake-data',
                                         driver_passthru=True)
        mock_request.rpcapi.driver_vendor_passthru.assert_called_once_with(
            'fake-context', 'fake-ident', 'bar', 'GET',
            'fake-data', 'fake-topic')

        # Assert file was attached to the response object
        self.assertIsInstance(mock_response.app_iter, static.FileIter)
        self.assertEqual(expct_return_value,
                         mock_response.app_iter.file.read())
        # Assert response message is none
        self.assertIsInstance(response, wsme.api.Response)
        self.assertIsNone(response.obj)
        self.assertIsNone(response.return_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_vendor_passthru_attach(self):
        self._test_vendor_passthru_attach('foo', b'foo')

    def test_vendor_passthru_attach_unicode_to_byte(self):
        self._test_vendor_passthru_attach(u'não', b'n\xc3\xa3o')

    def test_vendor_passthru_attach_byte_to_byte(self):
        self._test_vendor_passthru_attach(b'\x00\x01', b'\x00\x01')

    def test_get_controller_reserved_names(self):
        expected = ['maintenance', 'management', 'states',
                    'vendor_passthru', 'validate', 'detail']
        self.assertEqual(sorted(expected),
                         sorted(utils.get_controller_reserved_names(
                                api_node.NodesController)))


class TestPortgroupIdent(base.TestCase):
    def setUp(self):
        super(TestPortgroupIdent, self).setUp()
        self.valid_name = 'my-portgroup'
        self.valid_uuid = uuidutils.generate_uuid()
        self.invalid_name = 'My Portgroup'
        self.portgroup = test_api_utils.post_get_test_portgroup()

    @mock.patch.object(pecan, 'request', spec_set=["context"])
    @mock.patch.object(objects.Portgroup, 'get_by_name')
    def test_get_rpc_portgroup_name(self, mock_gbn, mock_pr):
        mock_gbn.return_value = self.portgroup
        self.assertEqual(self.portgroup, utils.get_rpc_portgroup(
            self.valid_name))
        mock_gbn.assert_called_once_with(mock_pr.context, self.valid_name)

    @mock.patch.object(pecan, 'request', spec_set=["context"])
    @mock.patch.object(objects.Portgroup, 'get_by_uuid')
    def test_get_rpc_portgroup_uuid(self, mock_gbu, mock_pr):
        self.portgroup['uuid'] = self.valid_uuid
        mock_gbu.return_value = self.portgroup
        self.assertEqual(self.portgroup, utils.get_rpc_portgroup(
            self.valid_uuid))
        mock_gbu.assert_called_once_with(mock_pr.context, self.valid_uuid)

    def test_get_rpc_portgroup_invalid_name(self):
        self.assertRaises(exception.InvalidUuidOrName,
                          utils.get_rpc_portgroup,
                          self.invalid_name)
