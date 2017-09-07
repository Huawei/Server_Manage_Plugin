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
"""
Tests for the API /nodes/ methods.
"""

import datetime
import json

import mock
from oslo_config import cfg
from oslo_utils import timeutils
from oslo_utils import uuidutils
import six
from six.moves import http_client
from six.moves.urllib import parse as urlparse
from testtools import matchers
from wsme import types as wtypes

from ironic.api.controllers import base as api_base
from ironic.api.controllers import v1 as api_v1
from ironic.api.controllers.v1 import node as api_node
from ironic.api.controllers.v1 import notification_utils
from ironic.api.controllers.v1 import utils as api_utils
from ironic.api.controllers.v1 import versions
from ironic.common import boot_devices
from ironic.common import driver_factory
from ironic.common import exception
from ironic.common import states
from ironic.conductor import rpcapi
from ironic import objects
from ironic.objects import fields as obj_fields
from ironic.tests import base
from ironic.tests.unit.api import base as test_api_base
from ironic.tests.unit.api import utils as test_api_utils
from ironic.tests.unit.objects import utils as obj_utils


class TestNodeObject(base.TestCase):

    def test_node_init(self):
        node_dict = test_api_utils.node_post_data()
        del node_dict['instance_uuid']
        node = api_node.Node(**node_dict)
        self.assertEqual(wtypes.Unset, node.instance_uuid)


class TestListNodes(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestListNodes, self).setUp()
        self.chassis = obj_utils.create_test_chassis(self.context)
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)

    def _create_association_test_nodes(self):
        # create some unassociated nodes
        unassociated_nodes = []
        for id in range(3):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            unassociated_nodes.append(node.uuid)

        # created some associated nodes
        associated_nodes = []
        for id in range(4):
            node = obj_utils.create_test_node(
                self.context, uuid=uuidutils.generate_uuid(),
                instance_uuid=uuidutils.generate_uuid())
            associated_nodes.append(node.uuid)
        return {'associated': associated_nodes,
                'unassociated': unassociated_nodes}

    def test_empty(self):
        data = self.get_json('/nodes')
        self.assertEqual([], data['nodes'])

    def test_one(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        data = self.get_json(
            '/nodes', headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertIn('instance_uuid', data['nodes'][0])
        self.assertIn('maintenance', data['nodes'][0])
        self.assertIn('power_state', data['nodes'][0])
        self.assertIn('provision_state', data['nodes'][0])
        self.assertIn('uuid', data['nodes'][0])
        self.assertEqual(node.uuid, data['nodes'][0]["uuid"])
        self.assertNotIn('driver', data['nodes'][0])
        self.assertNotIn('driver_info', data['nodes'][0])
        self.assertNotIn('driver_internal_info', data['nodes'][0])
        self.assertNotIn('extra', data['nodes'][0])
        self.assertNotIn('properties', data['nodes'][0])
        self.assertNotIn('chassis_uuid', data['nodes'][0])
        self.assertNotIn('reservation', data['nodes'][0])
        self.assertNotIn('console_enabled', data['nodes'][0])
        self.assertNotIn('target_power_state', data['nodes'][0])
        self.assertNotIn('target_provision_state', data['nodes'][0])
        self.assertNotIn('provision_updated_at', data['nodes'][0])
        self.assertNotIn('maintenance_reason', data['nodes'][0])
        self.assertNotIn('clean_step', data['nodes'][0])
        self.assertNotIn('raid_config', data['nodes'][0])
        self.assertNotIn('target_raid_config', data['nodes'][0])
        self.assertNotIn('network_interface', data['nodes'][0])
        self.assertNotIn('resource_class', data['nodes'][0])
        for field in api_utils.V31_FIELDS:
            self.assertNotIn(field, data['nodes'][0])
        self.assertNotIn('storage_interface', data['nodes'][0])
        # never expose the chassis_id
        self.assertNotIn('chassis_id', data['nodes'][0])

    def test_get_one(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(node.uuid, data['uuid'])
        self.assertIn('driver', data)
        self.assertIn('driver_info', data)
        self.assertEqual('******', data['driver_info']['fake_password'])
        self.assertEqual('bar', data['driver_info']['foo'])
        self.assertIn('driver_internal_info', data)
        self.assertIn('instance_info', data)
        self.assertEqual('******', data['instance_info']['configdrive'])
        self.assertEqual('******', data['instance_info']['image_url'])
        self.assertEqual('bar', data['instance_info']['foo'])
        self.assertIn('extra', data)
        self.assertIn('properties', data)
        self.assertIn('chassis_uuid', data)
        self.assertIn('reservation', data)
        self.assertIn('maintenance_reason', data)
        self.assertIn('name', data)
        self.assertIn('inspection_finished_at', data)
        self.assertIn('inspection_started_at', data)
        self.assertIn('clean_step', data)
        self.assertIn('states', data)
        self.assertIn('network_interface', data)
        self.assertIn('resource_class', data)
        for field in api_utils.V31_FIELDS:
            self.assertIn(field, data)
        self.assertIn('storage_interface', data)
        # never expose the chassis_id
        self.assertNotIn('chassis_id', data)

    def test_node_states_field_hidden_in_lower_version(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: '1.8'})
        self.assertNotIn('states', data)

    def test_node_interface_fields_hidden_in_lower_version(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: '1.30'})
        for field in api_utils.V31_FIELDS:
            self.assertNotIn(field, data)

    def test_node_storage_interface_hidden_in_lower_version(self):
        node = obj_utils.create_test_node(self.context,
                                          storage_interface='cinder')
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: '1.32'})
        self.assertNotIn('storage_interface', data)

    def test_get_one_custom_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'extra,instance_info'
        data = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        # We always append "links"
        self.assertItemsEqual(['extra', 'instance_info', 'links'], data)

    def test_get_collection_custom_fields(self):
        fields = 'uuid,instance_info'
        for i in range(3):
            obj_utils.create_test_node(self.context,
                                       uuid=uuidutils.generate_uuid(),
                                       instance_uuid=uuidutils.generate_uuid())

        data = self.get_json(
            '/nodes?fields=%s' % fields,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})

        self.assertEqual(3, len(data['nodes']))
        for node in data['nodes']:
            # We always append "links"
            self.assertItemsEqual(['uuid', 'instance_info', 'links'], node)

    def test_get_custom_fields_invalid_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'uuid,spongebob'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MAX_VER)},
            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertIn('spongebob', response.json['error_message'])

    def test_get_custom_fields_invalid_api_version(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'uuid,extra'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_get_one_custom_fields_show_password(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id,
                                          driver_info={'fake_password': 'bar'})
        fields = 'driver_info'
        data = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        # We always append "links"
        self.assertItemsEqual(['driver_info', 'links'], data)
        self.assertEqual('******', data['driver_info']['fake_password'])

    def test_get_network_interface_fields_invalid_api_version(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'network_interface'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_get_network_interface_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'network_interface'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertIn('network_interface', response)

    def test_get_all_interface_fields_invalid_api_version(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields_arg = ','.join(api_utils.V31_FIELDS)
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields_arg),
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_get_all_interface_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields_arg = ','.join(api_utils.V31_FIELDS)
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields_arg),
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        for field in api_utils.V31_FIELDS:
            self.assertIn(field, response)

    def test_get_storage_interface_fields_invalid_api_version(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'storage_interface'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_get_storage_interface_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        fields = 'storage_interface'
        response = self.get_json(
            '/nodes/%s?fields=%s' % (node.uuid, fields),
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertIn('storage_interface', response)

    def test_detail(self):
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id)
        data = self.get_json(
            '/nodes/detail',
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(node.uuid, data['nodes'][0]["uuid"])
        self.assertIn('name', data['nodes'][0])
        self.assertIn('driver', data['nodes'][0])
        self.assertIn('driver_info', data['nodes'][0])
        self.assertIn('extra', data['nodes'][0])
        self.assertIn('properties', data['nodes'][0])
        self.assertIn('chassis_uuid', data['nodes'][0])
        self.assertIn('reservation', data['nodes'][0])
        self.assertIn('maintenance', data['nodes'][0])
        self.assertIn('console_enabled', data['nodes'][0])
        self.assertIn('target_power_state', data['nodes'][0])
        self.assertIn('target_provision_state', data['nodes'][0])
        self.assertIn('provision_updated_at', data['nodes'][0])
        self.assertIn('inspection_finished_at', data['nodes'][0])
        self.assertIn('inspection_started_at', data['nodes'][0])
        self.assertIn('raid_config', data['nodes'][0])
        self.assertIn('target_raid_config', data['nodes'][0])
        self.assertIn('network_interface', data['nodes'][0])
        self.assertIn('resource_class', data['nodes'][0])
        for field in api_utils.V31_FIELDS:
            self.assertIn(field, data['nodes'][0])
        self.assertIn('storage_interface', data['nodes'][0])
        # never expose the chassis_id
        self.assertNotIn('chassis_id', data['nodes'][0])

    def test_detail_against_single(self):
        node = obj_utils.create_test_node(self.context)
        response = self.get_json('/nodes/%s/detail' % node.uuid,
                                 expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_mask_available_state(self):
        node = obj_utils.create_test_node(self.context,
                                          provision_state=states.AVAILABLE)

        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MIN_VER)})
        self.assertEqual(states.NOSTATE, data['provision_state'])

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.2"})
        self.assertEqual(states.AVAILABLE, data['provision_state'])

    def test_hide_fields_in_newer_versions_driver_internal(self):
        node = obj_utils.create_test_node(self.context,
                                          driver_internal_info={"foo": "bar"})
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MIN_VER)})
        self.assertNotIn('driver_internal_info', data)

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.3"})
        self.assertEqual({"foo": "bar"}, data['driver_internal_info'])

    def test_hide_fields_in_newer_versions_name(self):
        node = obj_utils.create_test_node(self.context,
                                          name="fish")
        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.4"})
        self.assertNotIn('name', data)

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.5"})
        self.assertEqual('fish', data['name'])

    def test_hide_fields_in_newer_versions_inspection(self):
        some_time = datetime.datetime(2015, 3, 18, 19, 20)
        node = obj_utils.create_test_node(self.context,
                                          inspection_started_at=some_time)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MIN_VER)})
        self.assertNotIn('inspection_finished_at', data)
        self.assertNotIn('inspection_started_at', data)

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.6"})
        started = timeutils.parse_isotime(
            data['inspection_started_at']).replace(tzinfo=None)
        self.assertEqual(some_time, started)
        self.assertIsNone(data['inspection_finished_at'])

    def test_hide_fields_in_newer_versions_clean_step(self):
        node = obj_utils.create_test_node(self.context,
                                          clean_step={"foo": "bar"})
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MIN_VER)})
        self.assertNotIn('clean_step', data)

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.7"})
        self.assertEqual({"foo": "bar"}, data['clean_step'])

    def test_hide_fields_in_newer_versions_network_interface(self):
        node = obj_utils.create_test_node(self.context,
                                          network_interface='flat')
        data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.19'})
        self.assertNotIn('network_interface', data['nodes'][0])
        new_data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.20'})
        self.assertEqual(node.network_interface,
                         new_data['nodes'][0]["network_interface"])

    def test_hide_fields_in_newer_versions_resource_class(self):
        node = obj_utils.create_test_node(self.context,
                                          resource_class='foo')
        data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.20'})
        self.assertNotIn('resource_class', data['nodes'][0])
        new_data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.21'})
        self.assertEqual(node.resource_class,
                         new_data['nodes'][0]["resource_class"])

    def test_hide_fields_in_newer_versions_interface_fields(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.30'})
        for field in api_utils.V31_FIELDS:
            self.assertNotIn(field, data['nodes'][0])
        new_data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.31'})
        for field in api_utils.V31_FIELDS:
            self.assertEqual(getattr(node, field),
                             new_data['nodes'][0][field])

    def test_hide_fields_in_newer_versions_volume(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: '1.31'})
        self.assertNotIn('volume', data)

        data = self.get_json('/nodes/%s' % node.uuid,
                             headers={api_base.Version.string: "1.32"})
        self.assertIn('volume', data)

    def test_hide_fields_in_newer_versions_storage_interface(self):
        node = obj_utils.create_test_node(self.context,
                                          storage_interface='cinder')
        data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.32'})
        self.assertNotIn('storage_interface', data['nodes'][0])
        new_data = self.get_json(
            '/nodes/detail', headers={api_base.Version.string: '1.33'})
        self.assertEqual(node.storage_interface,
                         new_data['nodes'][0]["storage_interface"])

    def test_many(self):
        nodes = []
        for id in range(5):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            nodes.append(node.uuid)
        data = self.get_json('/nodes')
        self.assertEqual(len(nodes), len(data['nodes']))

        uuids = [n['uuid'] for n in data['nodes']]
        self.assertEqual(sorted(nodes), sorted(uuids))

    def test_many_have_names(self):
        nodes = []
        node_names = []
        for id in range(5):
            name = 'node-%s' % id
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid(),
                                              name=name)
            nodes.append(node.uuid)
            node_names.append(name)
        data = self.get_json('/nodes',
                             headers={api_base.Version.string: "1.5"})
        names = [n['name'] for n in data['nodes']]
        self.assertEqual(len(nodes), len(data['nodes']))
        self.assertEqual(sorted(node_names), sorted(names))

    def _test_links(self, public_url=None):
        cfg.CONF.set_override('public_endpoint', public_url, 'api')
        uuid = uuidutils.generate_uuid()
        obj_utils.create_test_node(self.context, uuid=uuid)
        data = self.get_json('/nodes/%s' % uuid)
        self.assertIn('links', data)
        self.assertEqual(2, len(data['links']))
        self.assertIn(uuid, data['links'][0]['href'])
        for l in data['links']:
            bookmark = l['rel'] == 'bookmark'
            self.assertTrue(self.validate_link(l['href'], bookmark=bookmark))

        if public_url is not None:
            expected = [{'href': '%s/v1/nodes/%s' % (public_url, uuid),
                         'rel': 'self'},
                        {'href': '%s/nodes/%s' % (public_url, uuid),
                         'rel': 'bookmark'}]
            for i in expected:
                self.assertIn(i, data['links'])

    def test_links(self):
        self._test_links()

    def test_links_public_url(self):
        self._test_links(public_url='http://foo')

    def test_collection_links(self):
        nodes = []
        for id in range(5):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            nodes.append(node.uuid)
        data = self.get_json('/nodes/?limit=3')
        self.assertEqual(3, len(data['nodes']))

        next_marker = data['nodes'][-1]['uuid']
        self.assertIn(next_marker, data['next'])

    def test_collection_links_default_limit(self):
        cfg.CONF.set_override('max_limit', 3, 'api')
        nodes = []
        for id in range(5):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            nodes.append(node.uuid)
        data = self.get_json('/nodes')
        self.assertEqual(3, len(data['nodes']))

        next_marker = data['nodes'][-1]['uuid']
        self.assertIn(next_marker, data['next'])

    def test_sort_key(self):
        nodes = []
        for id in range(3):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            nodes.append(node.uuid)
        data = self.get_json('/nodes?sort_key=uuid')
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertEqual(sorted(nodes), uuids)

    def test_sort_key_invalid(self):
        invalid_keys_list = ['foo', 'properties', 'driver_info', 'extra',
                             'instance_info', 'driver_internal_info',
                             'clean_step']
        for invalid_key in invalid_keys_list:
            response = self.get_json('/nodes?sort_key=%s' % invalid_key,
                                     expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, response.status_int)
            self.assertEqual('application/json', response.content_type)
            self.assertIn(invalid_key, response.json['error_message'])

    def _test_sort_key_allowed(self, detail=False):
        node_uuids = []
        for id in range(3, 0, -1):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid(),
                                              resource_class='rc_%s' % id)
            node_uuids.append(node.uuid)
        node_uuids.reverse()
        headers = {'X-OpenStack-Ironic-API-Version': '1.21'}
        detail_str = '/detail' if detail else ''
        data = self.get_json('/nodes%s?sort_key=resource_class' % detail_str,
                             headers=headers)
        data_uuids = [n['uuid'] for n in data['nodes']]
        self.assertEqual(node_uuids, data_uuids)

    def test_sort_key_allowed(self):
        self._test_sort_key_allowed()

    def test_detail_sort_key_allowed(self):
        self._test_sort_key_allowed(detail=True)

    def _test_sort_key_not_allowed(self, detail=False):
        headers = {'X-OpenStack-Ironic-API-Version': '1.20'}
        detail_str = '/detail' if detail else ''
        resp = self.get_json('/nodes%s?sort_key=resource_class' % detail_str,
                             headers=headers, expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, resp.status_int)
        self.assertEqual('application/json', resp.content_type)

    def test_sort_key_not_allowed(self):
        self._test_sort_key_not_allowed()

    def test_detail_sort_key_not_allowed(self):
        self._test_sort_key_not_allowed(detail=True)

    def test_ports_subresource_link(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json('/nodes/%s' % node.uuid)
        self.assertIn('ports', data)

    def test_portgroups_subresource(self):
        node = obj_utils.create_test_node(self.context)
        headers = {'X-OpenStack-Ironic-API-Version': '1.24'}
        for id_ in range(2):
            obj_utils.create_test_portgroup(self.context, node_id=node.id,
                                            name="pg-%s" % id_,
                                            uuid=uuidutils.generate_uuid(),
                                            address='52:54:00:cf:2d:3%s' % id_)

        data = self.get_json('/nodes/%s/portgroups' % node.uuid,
                             headers=headers)
        self.assertEqual(2, len(data['portgroups']))
        self.assertNotIn('next', data)

        # Test collection pagination
        data = self.get_json('/nodes/%s/portgroups?limit=1' % node.uuid,
                             headers=headers)
        self.assertEqual(1, len(data['portgroups']))
        self.assertIn('next', data)

    def test_portgroups_subresource_link(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={'X-OpenStack-Ironic-API-Version': '1.24'})
        self.assertIn('portgroups', data.keys())

    def test_portgroups_subresource_link_hidden_for_older_versions(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={'X-OpenStack-Ironic-API-Version': '1.20'})
        self.assertNotIn('portgroups', data.keys())

    def test_portgroups_subresource_old_api_version(self):
        node = obj_utils.create_test_node(self.context)
        response = self.get_json(
            '/nodes/%s/portgroups' % node.uuid, expect_errors=True,
            headers={'X-OpenStack-Ironic-API-Version': '1.23'})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_ports_subresource(self):
        node = obj_utils.create_test_node(self.context)

        for id_ in range(2):
            obj_utils.create_test_port(self.context, node_id=node.id,
                                       uuid=uuidutils.generate_uuid(),
                                       address='52:54:00:cf:2d:3%s' % id_)

        data = self.get_json('/nodes/%s/ports' % node.uuid)
        self.assertEqual(2, len(data['ports']))
        self.assertNotIn('next', data)

        # Test collection pagination
        data = self.get_json('/nodes/%s/ports?limit=1' % node.uuid)
        self.assertEqual(1, len(data['ports']))
        self.assertIn('next', data)

    def test_ports_subresource_noid(self):
        node = obj_utils.create_test_node(self.context)
        obj_utils.create_test_port(self.context, node_id=node.id)
        # No node id specified
        response = self.get_json('/nodes/ports', expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_ports_subresource_node_not_found(self):
        non_existent_uuid = 'eeeeeeee-cccc-aaaa-bbbb-cccccccccccc'
        response = self.get_json('/nodes/%s/ports' % non_existent_uuid,
                                 expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_ports_subresource_invalid_ident(self):
        invalid_ident = '123~123'
        response = self.get_json('/nodes/%s/ports' % invalid_ident,
                                 expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertIn('Expected a logical name or UUID',
                      response.json['error_message'])

    def test_ports_subresource_via_portgroups_subres_not_allowed(self):
        node = obj_utils.create_test_node(self.context)
        pg = obj_utils.create_test_portgroup(self.context,
                                             node_id=node.id)
        response = self.get_json('/nodes/%s/portgroups/%s/ports' % (
            node.uuid, pg.uuid), expect_errors=True,
            headers={api_base.Version.string: '1.24'})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_volume_subresource_link(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: '1.32'})
        self.assertIn('volume', data)

    def test_volume_subresource(self):
        node = obj_utils.create_test_node(self.context)
        data = self.get_json('/nodes/%s/volume' % node.uuid,
                             headers={api_base.Version.string: '1.32'})
        self.assertIn('connectors', data)
        self.assertIn('targets', data)
        self.assertIn('/volume/connectors',
                      data['connectors'][0]['href'])
        self.assertIn('/volume/connectors',
                      data['connectors'][1]['href'])
        self.assertIn('/volume/targets',
                      data['targets'][0]['href'])
        self.assertIn('/volume/targets',
                      data['targets'][1]['href'])

    def test_volume_subresource_invalid_api_version(self):
        node = obj_utils.create_test_node(self.context)
        response = self.get_json('/nodes/%s/volume' % node.uuid,
                                 headers={api_base.Version.string: '1.31'},
                                 expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_volume_connectors_subresource(self):
        node = obj_utils.create_test_node(self.context)

        for id_ in range(2):
            obj_utils.create_test_volume_connector(
                self.context, node_id=node.id, uuid=uuidutils.generate_uuid(),
                connector_id='test-connector_id-%s' % id_)

        data = self.get_json(
            '/nodes/%s/volume/connectors' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(2, len(data['connectors']))
        self.assertNotIn('next', data)

        # Test collection pagination
        data = self.get_json(
            '/nodes/%s/volume/connectors?limit=1' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(1, len(data['connectors']))
        self.assertIn('next', data)

    def test_volume_connectors_subresource_noid(self):
        node = obj_utils.create_test_node(self.context)
        obj_utils.create_test_volume_connector(self.context, node_id=node.id)
        # No node_id specified.
        response = self.get_json(
            '/nodes/volume/connectors',
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_volume_connectors_subresource_node_not_found(self):
        non_existent_uuid = 'eeeeeeee-cccc-aaaa-bbbb-cccccccccccc'
        response = self.get_json(
            '/nodes/%s/volume/connectors' % non_existent_uuid,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_volume_targets_subresource(self):
        node = obj_utils.create_test_node(self.context)

        for id_ in range(2):
            obj_utils.create_test_volume_target(
                self.context, node_id=node.id, uuid=uuidutils.generate_uuid(),
                boot_index=id_)

        data = self.get_json(
            '/nodes/%s/volume/targets' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(2, len(data['targets']))
        self.assertNotIn('next', data)

        # Test collection pagination
        data = self.get_json(
            '/nodes/%s/volume/targets?limit=1' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(1, len(data['targets']))
        self.assertIn('next', data)

    def test_volume_targets_subresource_noid(self):
        node = obj_utils.create_test_node(self.context)
        obj_utils.create_test_volume_target(self.context, node_id=node.id)
        # No node_id specified.
        response = self.get_json(
            '/nodes/volume/targets',
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_volume_targets_subresource_node_not_found(self):
        non_existent_uuid = 'eeeeeeee-cccc-aaaa-bbbb-cccccccccccc'
        response = self.get_json(
            '/nodes/%s/volume/targets' % non_existent_uuid,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    @mock.patch.object(timeutils, 'utcnow')
    def _test_node_states(self, mock_utcnow, api_version=None):
        fake_state = 'fake-state'
        fake_error = 'fake-error'
        fake_config = '{"foo": "bar"}'
        test_time = datetime.datetime(2000, 1, 1, 0, 0)
        mock_utcnow.return_value = test_time
        node = obj_utils.create_test_node(self.context,
                                          power_state=fake_state,
                                          target_power_state=fake_state,
                                          provision_state=fake_state,
                                          target_provision_state=fake_state,
                                          provision_updated_at=test_time,
                                          raid_config=fake_config,
                                          target_raid_config=fake_config,
                                          last_error=fake_error)
        headers = {}
        if api_version:
            headers = {api_base.Version.string: api_version}
        data = self.get_json('/nodes/%s/states' % node.uuid, headers=headers)
        self.assertEqual(fake_state, data['power_state'])
        self.assertEqual(fake_state, data['target_power_state'])
        self.assertEqual(fake_state, data['provision_state'])
        self.assertEqual(fake_state, data['target_provision_state'])
        prov_up_at = timeutils.parse_isotime(
            data['provision_updated_at']).replace(tzinfo=None)
        self.assertEqual(test_time, prov_up_at)
        self.assertEqual(fake_error, data['last_error'])
        self.assertFalse(data['console_enabled'])
        return data

    def test_node_states(self):
        self._test_node_states()

    def test_node_states_raid(self):
        data = self._test_node_states(api_version="1.12")
        self.assertEqual({'foo': 'bar'}, data['raid_config'])
        self.assertEqual({'foo': 'bar'}, data['target_raid_config'])

    @mock.patch.object(timeutils, 'utcnow')
    def test_node_states_by_name(self, mock_utcnow):
        fake_state = 'fake-state'
        fake_error = 'fake-error'
        test_time = datetime.datetime(1971, 3, 9, 0, 0)
        mock_utcnow.return_value = test_time
        node = obj_utils.create_test_node(self.context,
                                          name='eggs',
                                          power_state=fake_state,
                                          target_power_state=fake_state,
                                          provision_state=fake_state,
                                          target_provision_state=fake_state,
                                          provision_updated_at=test_time,
                                          last_error=fake_error)
        data = self.get_json('/nodes/%s/states' % node.name,
                             headers={api_base.Version.string: "1.5"})
        self.assertEqual(fake_state, data['power_state'])
        self.assertEqual(fake_state, data['target_power_state'])
        self.assertEqual(fake_state, data['provision_state'])
        self.assertEqual(fake_state, data['target_provision_state'])
        prov_up_at = timeutils.parse_isotime(
            data['provision_updated_at']).replace(tzinfo=None)
        self.assertEqual(test_time, prov_up_at)
        self.assertEqual(fake_error, data['last_error'])
        self.assertFalse(data['console_enabled'])

    def test_node_by_instance_uuid(self):
        node = obj_utils.create_test_node(
            self.context,
            uuid=uuidutils.generate_uuid(),
            instance_uuid=uuidutils.generate_uuid())
        instance_uuid = node.instance_uuid

        data = self.get_json('/nodes?instance_uuid=%s' % instance_uuid,
                             headers={api_base.Version.string: "1.5"})

        self.assertThat(data['nodes'], matchers.HasLength(1))
        self.assertEqual(node['instance_uuid'],
                         data['nodes'][0]["instance_uuid"])

    def test_node_by_instance_uuid_wrong_uuid(self):
        obj_utils.create_test_node(
            self.context, uuid=uuidutils.generate_uuid(),
            instance_uuid=uuidutils.generate_uuid())
        wrong_uuid = uuidutils.generate_uuid()

        data = self.get_json('/nodes?instance_uuid=%s' % wrong_uuid)

        self.assertThat(data['nodes'], matchers.HasLength(0))

    def test_node_by_instance_uuid_invalid_uuid(self):
        response = self.get_json('/nodes?instance_uuid=fake',
                                 expect_errors=True)

        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)

    def test_associated_nodes_insensitive(self):
        associated_nodes = (self
                            ._create_association_test_nodes()['associated'])

        data = self.get_json('/nodes?associated=true')
        data1 = self.get_json('/nodes?associated=True')

        uuids = [n['uuid'] for n in data['nodes']]
        uuids1 = [n['uuid'] for n in data1['nodes']]
        self.assertEqual(sorted(associated_nodes), sorted(uuids1))
        self.assertEqual(sorted(associated_nodes), sorted(uuids))

    def test_associated_nodes_error(self):
        self._create_association_test_nodes()
        response = self.get_json('/nodes?associated=blah', expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_unassociated_nodes_insensitive(self):
        unassociated_nodes = (
            self._create_association_test_nodes()['unassociated'])

        data = self.get_json('/nodes?associated=false')
        data1 = self.get_json('/nodes?associated=FALSE')

        uuids = [n['uuid'] for n in data['nodes']]
        uuids1 = [n['uuid'] for n in data1['nodes']]
        self.assertEqual(sorted(unassociated_nodes), sorted(uuids1))
        self.assertEqual(sorted(unassociated_nodes), sorted(uuids))

    def test_unassociated_nodes_with_limit(self):
        unassociated_nodes = (
            self._create_association_test_nodes()['unassociated'])

        data = self.get_json('/nodes?associated=False&limit=2')

        self.assertThat(data['nodes'], matchers.HasLength(2))
        self.assertIn(data['nodes'][0]['uuid'], unassociated_nodes)

    def test_next_link_with_association(self):
        self._create_association_test_nodes()
        data = self.get_json('/nodes/?limit=3&associated=True')
        self.assertThat(data['nodes'], matchers.HasLength(3))
        self.assertIn('associated=True', data['next'])

    def test_detail_with_association_filter(self):
        associated_nodes = (self
                            ._create_association_test_nodes()['associated'])
        data = self.get_json('/nodes/detail?associated=true')
        self.assertIn('driver', data['nodes'][0])
        self.assertEqual(len(associated_nodes), len(data['nodes']))

    def test_next_link_with_association_with_detail(self):
        self._create_association_test_nodes()
        data = self.get_json('/nodes/detail?limit=3&associated=true')
        self.assertThat(data['nodes'], matchers.HasLength(3))
        self.assertIn('driver', data['nodes'][0])
        self.assertIn('associated=True', data['next'])

    def test_detail_with_instance_uuid(self):
        node = obj_utils.create_test_node(
            self.context,
            uuid=uuidutils.generate_uuid(),
            instance_uuid=uuidutils.generate_uuid(),
            chassis_id=self.chassis.id)
        instance_uuid = node.instance_uuid

        data = self.get_json('/nodes/detail?instance_uuid=%s' % instance_uuid)

        self.assertEqual(node['instance_uuid'],
                         data['nodes'][0]["instance_uuid"])
        self.assertIn('driver', data['nodes'][0])
        self.assertIn('driver_info', data['nodes'][0])
        self.assertIn('extra', data['nodes'][0])
        self.assertIn('properties', data['nodes'][0])
        self.assertIn('chassis_uuid', data['nodes'][0])
        # never expose the chassis_id
        self.assertNotIn('chassis_id', data['nodes'][0])

    def test_maintenance_nodes(self):
        nodes = []
        for id in range(5):
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid(),
                                              maintenance=id % 2)
            nodes.append(node)

        data = self.get_json('/nodes?maintenance=true')
        uuids = [n['uuid'] for n in data['nodes']]
        test_uuids_1 = [n.uuid for n in nodes if n.maintenance]
        self.assertEqual(sorted(test_uuids_1), sorted(uuids))

        data = self.get_json('/nodes?maintenance=false')
        uuids = [n['uuid'] for n in data['nodes']]
        test_uuids_0 = [n.uuid for n in nodes if not n.maintenance]
        self.assertEqual(sorted(test_uuids_0), sorted(uuids))

    def test_maintenance_nodes_error(self):
        response = self.get_json('/nodes?associated=true&maintenance=blah',
                                 expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_maintenance_nodes_associated(self):
        self._create_association_test_nodes()
        node = obj_utils.create_test_node(
            self.context,
            instance_uuid=uuidutils.generate_uuid(),
            maintenance=True)

        data = self.get_json('/nodes?associated=true&maintenance=false')
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertNotIn(node.uuid, uuids)
        data = self.get_json('/nodes?associated=true&maintenance=true')
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node.uuid, uuids)
        data = self.get_json('/nodes?associated=true&maintenance=TruE')
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node.uuid, uuids)

    def test_get_nodes_by_provision_state(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          provision_state=states.AVAILABLE)
        node1 = obj_utils.create_test_node(self.context,
                                           uuid=uuidutils.generate_uuid(),
                                           provision_state=states.DEPLOYING)

        data = self.get_json('/nodes?provision_state=available',
                             headers={api_base.Version.string: "1.9"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node.uuid, uuids)
        self.assertNotIn(node1.uuid, uuids)
        data = self.get_json('/nodes?provision_state=deploying',
                             headers={api_base.Version.string: "1.9"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node1.uuid, uuids)
        self.assertNotIn(node.uuid, uuids)

    def test_get_nodes_by_invalid_provision_state(self):
        response = self.get_json('/nodes?provision_state=test',
                                 headers={api_base.Version.string: "1.9"},
                                 expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_get_nodes_by_provision_state_not_allowed(self):
        response = self.get_json('/nodes?provision_state=test',
                                 headers={api_base.Version.string: "1.8"},
                                 expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_get_nodes_by_driver(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          driver='pxe_ipmitool')
        node1 = obj_utils.create_test_node(self.context,
                                           uuid=uuidutils.generate_uuid(),
                                           driver='fake')

        data = self.get_json('/nodes?driver=pxe_ipmitool',
                             headers={api_base.Version.string: "1.16"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node.uuid, uuids)
        self.assertNotIn(node1.uuid, uuids)
        data = self.get_json('/nodes?driver=fake',
                             headers={api_base.Version.string: "1.16"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node1.uuid, uuids)
        self.assertNotIn(node.uuid, uuids)

    def test_get_nodes_by_invalid_driver(self):
        data = self.get_json('/nodes?driver=test',
                             headers={api_base.Version.string: "1.16"})
        self.assertEqual(0, len(data['nodes']))

    def test_get_nodes_by_driver_invalid_api_version(self):
        response = self.get_json(
            '/nodes?driver=fake',
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)
        self.assertTrue(response.json['error_message'])

    def _test_get_nodes_by_resource_class(self, detail=False):
        if detail:
            base_url = '/nodes/detail?resource_class=%s'
        else:
            base_url = '/nodes?resource_class=%s'

        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          driver='fake',
                                          resource_class='foo')
        node1 = obj_utils.create_test_node(self.context,
                                           uuid=uuidutils.generate_uuid(),
                                           driver='fake',
                                           resource_class='bar')

        data = self.get_json(base_url % 'foo',
                             headers={api_base.Version.string: "1.21"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node.uuid, uuids)
        self.assertNotIn(node1.uuid, uuids)
        data = self.get_json(base_url % 'bar',
                             headers={api_base.Version.string: "1.21"})
        uuids = [n['uuid'] for n in data['nodes']]
        self.assertIn(node1.uuid, uuids)
        self.assertNotIn(node.uuid, uuids)

    def test_get_nodes_by_resource_class(self):
        self._test_get_nodes_by_resource_class(detail=False)

    def test_get_nodes_by_resource_class_detail(self):
        self._test_get_nodes_by_resource_class(detail=True)

    def _test_get_nodes_by_invalid_resource_class(self, detail=False):
        if detail:
            base_url = '/nodes/detail?resource_class=%s'
        else:
            base_url = '/nodes?resource_class=%s'

        data = self.get_json(base_url % 'test',
                             headers={api_base.Version.string: "1.21"})
        self.assertEqual(0, len(data['nodes']))

    def test_get_nodes_by_invalid_resource_class(self):
        self._test_get_nodes_by_invalid_resource_class(detail=False)

    def test_get_nodes_by_invalid_resource_class_detail(self):
        self._test_get_nodes_by_invalid_resource_class(detail=True)

    def _test_get_nodes_by_resource_class_invalid_api_version(self,
                                                              detail=False):
        if detail:
            base_url = '/nodes/detail?resource_class=%s'
        else:
            base_url = '/nodes?resource_class=%s'

        response = self.get_json(
            base_url % 'fake',
            headers={api_base.Version.string: str(api_v1.MIN_VER)},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_get_nodes_by_resource_class_invalid_api_version(self):
        self._test_get_nodes_by_resource_class_invalid_api_version(
            detail=False)

    def test_get_nodes_by_resource_class_invalid_api_version_detail(self):
        self._test_get_nodes_by_resource_class_invalid_api_version(detail=True)

    def test_get_console_information(self):
        node = obj_utils.create_test_node(self.context)
        expected_console_info = {'test': 'test-data'}
        expected_data = {'console_enabled': True,
                         'console_info': expected_console_info}
        with mock.patch.object(rpcapi.ConductorAPI,
                               'get_console_information') as mock_gci:
            mock_gci.return_value = expected_console_info
            data = self.get_json('/nodes/%s/states/console' % node.uuid)
            self.assertEqual(expected_data, data)
            mock_gci.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_console_information')
    def test_get_console_information_by_name(self, mock_gci):
        node = obj_utils.create_test_node(self.context, name='spam')
        expected_console_info = {'test': 'test-data'}
        expected_data = {'console_enabled': True,
                         'console_info': expected_console_info}
        mock_gci.return_value = expected_console_info
        data = self.get_json('/nodes/%s/states/console' % node.name,
                             headers={api_base.Version.string: "1.5"})
        self.assertEqual(expected_data, data)
        mock_gci.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    def test_get_console_information_console_disabled(self):
        node = obj_utils.create_test_node(self.context)
        expected_data = {'console_enabled': False,
                         'console_info': None}
        with mock.patch.object(rpcapi.ConductorAPI,
                               'get_console_information') as mock_gci:
            mock_gci.side_effect = (
                exception.NodeConsoleNotEnabled(node=node.uuid))
            data = self.get_json('/nodes/%s/states/console' % node.uuid)
            self.assertEqual(expected_data, data)
            mock_gci.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    def test_get_console_information_not_supported(self):
        node = obj_utils.create_test_node(self.context)
        with mock.patch.object(rpcapi.ConductorAPI,
                               'get_console_information') as mock_gci:
            mock_gci.side_effect = exception.UnsupportedDriverExtension(
                extension='console', driver='test-driver')
            ret = self.get_json('/nodes/%s/states/console' % node.uuid,
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
            mock_gci.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_boot_device')
    def test_get_boot_device(self, mock_gbd):
        node = obj_utils.create_test_node(self.context)
        expected_data = {'boot_device': boot_devices.PXE, 'persistent': True}
        mock_gbd.return_value = expected_data
        data = self.get_json('/nodes/%s/management/boot_device' % node.uuid)
        self.assertEqual(expected_data, data)
        mock_gbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_boot_device')
    def test_get_boot_device_by_name(self, mock_gbd):
        node = obj_utils.create_test_node(self.context, name='spam')
        expected_data = {'boot_device': boot_devices.PXE, 'persistent': True}
        mock_gbd.return_value = expected_data
        data = self.get_json('/nodes/%s/management/boot_device' % node.name,
                             headers={api_base.Version.string: "1.5"})
        self.assertEqual(expected_data, data)
        mock_gbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_boot_device')
    def test_get_boot_device_iface_not_supported(self, mock_gbd):
        node = obj_utils.create_test_node(self.context)
        mock_gbd.side_effect = exception.UnsupportedDriverExtension(
            extension='management', driver='test-driver')
        ret = self.get_json('/nodes/%s/management/boot_device' % node.uuid,
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        mock_gbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_supported_boot_devices')
    def test_get_supported_boot_devices(self, mock_gsbd):
        mock_gsbd.return_value = [boot_devices.PXE]
        node = obj_utils.create_test_node(self.context)
        data = self.get_json('/nodes/%s/management/boot_device/supported'
                             % node.uuid)
        expected_data = {'supported_boot_devices': [boot_devices.PXE]}
        self.assertEqual(expected_data, data)
        mock_gsbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_supported_boot_devices')
    def test_get_supported_boot_devices_by_name(self, mock_gsbd):
        mock_gsbd.return_value = [boot_devices.PXE]
        node = obj_utils.create_test_node(self.context, name='spam')
        data = self.get_json(
            '/nodes/%s/management/boot_device/supported' % node.name,
            headers={api_base.Version.string: "1.5"})
        expected_data = {'supported_boot_devices': [boot_devices.PXE]}
        self.assertEqual(expected_data, data)
        mock_gsbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'get_supported_boot_devices')
    def test_get_supported_boot_devices_iface_not_supported(self, mock_gsbd):
        node = obj_utils.create_test_node(self.context)
        mock_gsbd.side_effect = exception.UnsupportedDriverExtension(
            extension='management', driver='test-driver')
        ret = self.get_json('/nodes/%s/management/boot_device/supported' %
                            node.uuid, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        mock_gsbd.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'validate_driver_interfaces')
    def test_validate_by_uuid_using_deprecated_interface(self, mock_vdi):
        # Note(mrda): The 'node_uuid' interface is deprecated in favour
        # of the 'node' interface
        node = obj_utils.create_test_node(self.context)
        self.get_json('/nodes/validate?node_uuid=%s' % node.uuid)
        mock_vdi.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'validate_driver_interfaces')
    def test_validate_by_uuid(self, mock_vdi):
        node = obj_utils.create_test_node(self.context)
        self.get_json('/nodes/validate?node=%s' % node.uuid,
                      headers={api_base.Version.string: "1.5"})
        mock_vdi.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'validate_driver_interfaces')
    def test_validate_by_name_unsupported(self, mock_vdi):
        node = obj_utils.create_test_node(self.context, name='spam')
        ret = self.get_json('/nodes/validate?node=%s' % node.name,
                            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, ret.status_code)
        self.assertFalse(mock_vdi.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'validate_driver_interfaces')
    def test_validate_by_name(self, mock_vdi):
        node = obj_utils.create_test_node(self.context, name='spam')
        self.get_json('/nodes/validate?node=%s' % node.name,
                      headers={api_base.Version.string: "1.5"})
        # note that this should be node.uuid here as we get that from the
        # rpc_node lookup and pass that downwards
        mock_vdi.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    def test_ssh_creds_masked(self):
        driver_info = {"ssh_password": "password", "ssh_key_contents": "key"}
        node = obj_utils.create_test_node(self.context,
                                          chassis_id=self.chassis.id,
                                          driver_info=driver_info)
        data = self.get_json(
            '/nodes/%s' % node.uuid,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})

        self.assertEqual("******", data["driver_info"]["ssh_password"])
        self.assertEqual("******", data["driver_info"]["ssh_key_contents"])


class TestPatch(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestPatch, self).setUp()
        self.chassis = obj_utils.create_test_chassis(self.context)
        self.node = obj_utils.create_test_node(self.context, name='node-57',
                                               chassis_id=self.chassis.id)
        self.node_no_name = obj_utils.create_test_node(
            self.context, uuid='deadbeef-0000-1111-2222-333333333333',
            chassis_id=self.chassis.id)
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'update_node')
        self.mock_update_node = p.start()
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'change_node_power_state')
        self.mock_cnps = p.start()
        self.addCleanup(p.stop)

    @mock.patch.object(notification_utils, '_emit_api_notification')
    def test_update_ok(self, mock_notify):
        self.mock_update_node.return_value = self.node
        (self
         .mock_update_node
         .return_value
         .updated_at) = "2013-12-03T06:20:41.184720+00:00"
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/instance_uuid',
                                     'value':
                                     'aaaaaaaa-1111-bbbb-2222-cccccccccccc',
                                     'op': 'replace'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)
        self.assertEqual(self.mock_update_node.return_value.updated_at,
                         timeutils.parse_isotime(response.json['updated_at']))
        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY, 'update',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.START,
                                      chassis_uuid=self.chassis.uuid),
                                      mock.call(mock.ANY, mock.ANY, 'update',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.END,
                                      chassis_uuid=self.chassis.uuid)])

    def test_update_by_name_unsupported(self):
        self.mock_update_node.return_value = self.node
        (self
         .mock_update_node
         .return_value
         .updated_at) = "2013-12-03T06:20:41.184720+00:00"
        response = self.patch_json(
            '/nodes/%s' % self.node.name,
            [{'path': '/instance_uuid',
              'value': 'aaaaaaaa-1111-bbbb-2222-cccccccccccc',
              'op': 'replace'}],
            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_code)
        self.assertFalse(self.mock_update_node.called)

    def test_update_ok_by_name(self):
        self.mock_update_node.return_value = self.node
        (self
         .mock_update_node
         .return_value
         .updated_at) = "2013-12-03T06:20:41.184720+00:00"
        response = self.patch_json(
            '/nodes/%s' % self.node.name,
            [{'path': '/instance_uuid',
              'value': 'aaaaaaaa-1111-bbbb-2222-cccccccccccc',
              'op': 'replace'}],
            headers={api_base.Version.string: "1.5"})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)
        self.assertEqual(self.mock_update_node.return_value.updated_at,
                         timeutils.parse_isotime(response.json['updated_at']))
        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_update_state(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'power_state': 'new state'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    @mock.patch.object(notification_utils, '_emit_api_notification')
    def test_update_fails_bad_driver_info(self, mock_notify):
        fake_err = 'Fake Error Message'
        self.mock_update_node.side_effect = (
            exception.InvalidParameterValue(fake_err))

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/driver_info/this',
                                     'value': 'foo',
                                     'op': 'add'},
                                    {'path': '/driver_info/that',
                                     'value': 'bar',
                                     'op': 'add'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)

        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY, 'update',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.START,
                                      chassis_uuid=self.chassis.uuid),
                                      mock.call(mock.ANY, mock.ANY, 'update',
                                      obj_fields.NotificationLevel.ERROR,
                                      obj_fields.NotificationStatus.ERROR,
                                      chassis_uuid=self.chassis.uuid)])

    def test_update_fails_bad_driver(self):
        self.mock_gtf.side_effect = exception.NoValidHost('Fake Error')

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/driver',
                                     'value': 'bad-driver',
                                     'op': 'replace'}],
                                   expect_errors=True)

        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)

    def test_add_ok(self):
        self.mock_update_node.return_value = self.node

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/extra/foo',
                                     'value': 'bar',
                                     'op': 'add'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_add_root(self):
        self.mock_update_node.return_value = self.node
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/instance_uuid',
                                     'value':
                                     'aaaaaaaa-1111-bbbb-2222-cccccccccccc',
                                     'op': 'add'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)
        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_add_root_non_existent(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/foo', 'value': 'bar',
                                     'op': 'add'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_remove_ok(self):
        self.mock_update_node.return_value = self.node

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/extra',
                                     'op': 'remove'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_remove_non_existent_property_fail(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/extra/non-existent',
                                     'op': 'remove'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_update_allowed_in_power_transition(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          target_power_state=states.POWER_OFF)
        self.mock_update_node.return_value = node
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/extra/foo',
                                     'value': 'bar',
                                     'op': 'add'}])
        self.assertEqual(http_client.OK, response.status_code)

    def test_update_allowed_in_maintenance(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          target_power_state=states.POWER_OFF,
                                          maintenance=True)
        self.mock_update_node.return_value = node
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/instance_uuid',
                                     'op': 'remove'}])
        self.assertEqual(http_client.OK, response.status_code)

    def test_add_state_in_deployfail(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid(),
                                          provision_state=states.DEPLOYFAIL,
                                          target_provision_state=states.ACTIVE)
        self.mock_update_node.return_value = node
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/extra/foo', 'value': 'bar',
                                     'op': 'add'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)
        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_patch_ports_subresource_no_port_id(self):
        response = self.patch_json('/nodes/%s/ports' % self.node.uuid,
                                   [{'path': '/extra/foo', 'value': 'bar',
                                     'op': 'add'}], expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_patch_ports_subresource(self):
        response = self.patch_json(
            '/nodes/%s/ports/9bb50f13-0b8d-4ade-ad2d-d91fefdef9cc' %
            self.node.uuid,
            [{'path': '/extra/foo', 'value': 'bar',
              'op': 'add'}], expect_errors=True)
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_patch_portgroups_subresource(self):
        response = self.patch_json(
            '/nodes/%s/portgroups/9bb50f13-0b8d-4ade-ad2d-d91fefdef9cc' %
            self.node.uuid,
            [{'path': '/extra/foo', 'value': 'bar',
              'op': 'add'}], expect_errors=True,
            headers={'X-OpenStack-Ironic-API-Version': '1.24'})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_patch_volume_connectors_subresource_no_connector_id(self):
        response = self.patch_json(
            '/nodes/%s/volume/connectors' % self.node.uuid,
            [{'path': '/extra/foo', 'value': 'bar', 'op': 'add'}],
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_patch_volume_connectors_subresource(self):
        connector = (
            obj_utils.create_test_volume_connector(self.context,
                                                   node_id=self.node.id))
        response = self.patch_json(
            '/nodes/%s/volume/connectors/%s' % (self.node.uuid,
                                                connector.uuid),
            [{'path': '/extra/foo', 'value': 'bar', 'op': 'add'}],
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_patch_volume_targets_subresource(self):
        target = obj_utils.create_test_volume_target(self.context,
                                                     node_id=self.node.id)
        response = self.patch_json(
            '/nodes/%s/volume/targets/%s' % (self.node.uuid,
                                             target.uuid),
            [{'path': '/extra/foo', 'value': 'bar', 'op': 'add'}],
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_remove_uuid(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/uuid', 'op': 'remove'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_add_state_in_cleaning(self):
        node = obj_utils.create_test_node(
            self.context,
            uuid=uuidutils.generate_uuid(),
            provision_state=states.CLEANING,
            target_provision_state=states.AVAILABLE)
        self.mock_update_node.return_value = node
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/extra/foo', 'value': 'bar',
                                     'op': 'add'}], expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.CONFLICT, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_remove_mandatory_field(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/driver', 'op': 'remove'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_chassis_uuid(self):
        self.mock_update_node.return_value = self.node
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_uuid',
                                     'value': self.chassis.uuid,
                                     'op': 'replace'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_add_chassis_uuid(self):
        self.mock_update_node.return_value = self.node
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_uuid',
                                     'value': self.chassis.uuid,
                                     'op': 'add'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_remove_chassis_uuid(self):
        self.mock_update_node.return_value = self.node
        headers = {api_base.Version.string: "1.25"}
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_uuid',
                                     'op': 'remove'}],
                                   headers=headers)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_remove_chassis_uuid_invalid_api_version(self):
        self.mock_update_node.return_value = self.node
        headers = {api_base.Version.string: "1.24"}
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_uuid',
                                     'op': 'remove'}],
                                   headers=headers,
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)
        self.assertTrue(response.json['error_message'])

    @mock.patch("pecan.request")
    def test__update_changed_fields_remove_chassis_uuid(self, mock_pecan_req):
        mock_pecan_req.version.minor = versions.MINOR_MAX_VERSION
        controller = api_node.NodesController()

        node_dict = self.node.as_dict()
        del node_dict['chassis_id']
        node_no_chassis = api_node.Node(**node_dict)

        controller._update_changed_fields(node_no_chassis, self.node)
        self.assertIsNone(self.node.chassis_id)

    def test_add_chassis_id(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_id',
                                     'value': '1',
                                     'op': 'add'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_chassis_id(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_id',
                                     'value': '1',
                                     'op': 'replace'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_remove_chassis_id(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_id',
                                     'op': 'remove'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_non_existent_chassis_uuid(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/chassis_uuid',
                                     'value':
                                     'eeeeeeee-dddd-cccc-bbbb-aaaaaaaaaaaa',
                                     'op': 'replace'}], expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_remove_internal_field(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/last_error', 'op': 'remove'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_internal_field(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/power_state', 'op': 'replace',
                                     'value': 'fake-state'}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_maintenance(self):
        self.mock_update_node.return_value = self.node

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/maintenance', 'op': 'replace',
                                     'value': 'true'}])
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_replace_maintenance_by_name(self):
        self.mock_update_node.return_value = self.node

        response = self.patch_json(
            '/nodes/%s' % self.node.name,
            [{'path': '/maintenance', 'op': 'replace',
              'value': 'true'}],
            headers={api_base.Version.string: "1.5"})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

        self.mock_update_node.assert_called_once_with(
            mock.ANY, mock.ANY, 'test-topic')

    def test_replace_consoled_enabled(self):
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/console_enabled',
                                     'op': 'replace', 'value': True}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_replace_provision_updated_at(self):
        test_time = '2000-01-01 00:00:00'
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/provision_updated_at',
                                     'op': 'replace', 'value': test_time}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_patch_add_name_ok(self):
        self.mock_update_node.return_value = self.node_no_name
        test_name = 'guido-van-rossum'
        response = self.patch_json('/nodes/%s' % self.node_no_name.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def _patch_add_name_invalid_or_reserved(self, name):
        self.mock_update_node.return_value = self.node_no_name
        response = self.patch_json('/nodes/%s' % self.node_no_name.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': name}],
                                   headers={api_base.Version.string: "1.10"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_patch_add_name_invalid(self):
        self._patch_add_name_invalid_or_reserved('i am invalid')

    def test_patch_add_name_reserved(self):
        reserved_names = api_utils.get_controller_reserved_names(
            api_node.NodesController)
        for name in reserved_names:
            self._patch_add_name_invalid_or_reserved(name)

    def test_patch_add_name_empty_invalid(self):
        test_name = ''
        response = self.patch_json('/nodes/%s' % self.node_no_name.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_patch_add_name_empty_not_acceptable(self):
        test_name = ''
        response = self.patch_json('/nodes/%s' % self.node_no_name.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': test_name}],
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_patch_name_replace_ok(self):
        self.mock_update_node.return_value = self.node
        test_name = 'guido-van-rossum'
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/name',
                                     'op': 'replace',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_patch_add_replace_invalid(self):
        self.mock_update_node.return_value = self.node_no_name
        test_name = 'Guido Van Error'
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/name',
                                     'op': 'replace',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_patch_update_name_twice_both_invalid(self):
        test_name_1 = 'Windows ME'
        test_name_2 = 'Guido Van Error'
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': test_name_1},
                                    {'path': '/name',
                                     'op': 'replace',
                                     'value': test_name_2}],
                                   headers={api_base.Version.string: "1.5"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertIn(test_name_1, response.json['error_message'])

    def test_patch_update_name_twice_second_invalid(self):
        test_name = 'Guido Van Error'
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/name',
                                     'op': 'add',
                                     'value': 'node-0'},
                                    {'path': '/name',
                                     'op': 'replace',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertIn(test_name, response.json['error_message'])

    def test_patch_duplicate_name(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        test_name = "this-is-my-node"
        self.mock_update_node.side_effect = exception.DuplicateName(test_name)
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/name',
                                     'op': 'replace',
                                     'value': test_name}],
                                   headers={api_base.Version.string: "1.5"},
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.CONFLICT, response.status_code)
        self.assertTrue(response.json['error_message'])

    @mock.patch.object(api_node.NodesController, '_check_names_acceptable')
    def test_patch_name_remove_ok(self, cna_mock):
        self.mock_update_node.return_value = self.node
        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/name',
                                     'op': 'remove'}],
                                   headers={api_base.Version.string:
                                            "1.5"})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)
        self.assertFalse(cna_mock.called)

    @mock.patch.object(api_utils, 'get_rpc_node')
    def test_patch_update_drive_console_enabled(self, mock_rpc_node):
        self.node.console_enabled = True
        mock_rpc_node.return_value = self.node

        response = self.patch_json('/nodes/%s' % self.node.uuid,
                                   [{'path': '/driver',
                                     'value': 'foo',
                                     'op': 'add'}],
                                   expect_errors=True)
        mock_rpc_node.assert_called_once_with(self.node.uuid)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.CONFLICT, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_update_in_UPDATE_ALLOWED_STATES(self):
        for state in states.UPDATE_ALLOWED_STATES:
            node = obj_utils.create_test_node(
                self.context,
                uuid=uuidutils.generate_uuid(),
                provision_state=state,
                target_provision_state=states.AVAILABLE)

            self.mock_update_node.return_value = node
            response = self.patch_json('/nodes/%s' % node.uuid,
                                       [{'path': '/extra/foo', 'value': 'bar',
                                         'op': 'add'}])
            self.assertEqual('application/json', response.content_type)
            self.assertEqual(http_client.OK, response.status_code)

    def test_update_network_interface(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        network_interface = 'flat'
        headers = {api_base.Version.string: str(api_v1.MAX_VER)}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/network_interface',
                                     'value': network_interface,
                                     'op': 'add'}],
                                   headers=headers)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_update_network_interface_old_api(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        network_interface = 'flat'
        headers = {api_base.Version.string: '1.15'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/network_interface',
                                     'value': network_interface,
                                     'op': 'add'}],
                                   headers=headers,
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)

    def test_update_resource_class(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        resource_class = 'foo'
        headers = {api_base.Version.string: '1.21'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/resource_class',
                                     'value': resource_class,
                                     'op': 'add'}],
                                   headers=headers)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_update_resource_class_old_api(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        resource_class = 'foo'
        headers = {api_base.Version.string: '1.20'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/resource_class',
                                     'value': resource_class,
                                     'op': 'add'}],
                                   headers=headers,
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)

    def test_update_resource_class_max_length(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        resource_class = 'f' * 80
        headers = {api_base.Version.string: '1.21'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/resource_class',
                                     'value': resource_class,
                                     'op': 'add'}],
                                   headers=headers)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_update_resource_class_too_long(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        resource_class = 'f' * 81
        headers = {api_base.Version.string: '1.21'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/resource_class',
                                     'value': resource_class,
                                     'op': 'add'}],
                                   headers=headers,
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)

    def test_update_interface_fields(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        headers = {api_base.Version.string: str(api_v1.MAX_VER)}
        for field in api_utils.V31_FIELDS:
            response = self.patch_json('/nodes/%s' % node.uuid,
                                       [{'path': '/%s' % field,
                                         'value': 'fake',
                                         'op': 'add'}],
                                       headers=headers)
            self.assertEqual('application/json', response.content_type)
            self.assertEqual(http_client.OK, response.status_code)

    def test_update_interface_fields_bad_version(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        headers = {api_base.Version.string: '1.30'}
        for field in api_utils.V31_FIELDS:
            response = self.patch_json('/nodes/%s' % node.uuid,
                                       [{'path': '/%s' % field,
                                         'value': 'fake',
                                         'op': 'add'}],
                                       headers=headers,
                                       expect_errors=True)
            self.assertEqual('application/json', response.content_type)
            self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)

    def test_update_classic_driver_interface_fields(self):
        headers = {api_base.Version.string: '1.31'}
        self.mock_update_node.side_effect = (
            exception.MustBeNone('error'))
        for field in api_utils.V31_FIELDS:
            node = obj_utils.create_test_node(self.context,
                                              uuid=uuidutils.generate_uuid())
            response = self.patch_json('/nodes/%s' % node.uuid,
                                       [{'path': '/%s' % field,
                                         'value': 'fake',
                                         'op': 'add'}],
                                       headers=headers,
                                       expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, response.status_int)
            self.assertEqual('application/json', response.content_type)

    def test_update_storage_interface(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        storage_interface = 'cinder'
        headers = {api_base.Version.string: str(api_v1.MAX_VER)}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/storage_interface',
                                     'value': storage_interface,
                                     'op': 'add'}],
                                   headers=headers)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.OK, response.status_code)

    def test_update_storage_interface_old_api(self):
        node = obj_utils.create_test_node(self.context,
                                          uuid=uuidutils.generate_uuid())
        self.mock_update_node.return_value = node
        storage_interface = 'cinder'
        headers = {api_base.Version.string: '1.32'}
        response = self.patch_json('/nodes/%s' % node.uuid,
                                   [{'path': '/storage_interface',
                                     'value': storage_interface,
                                     'op': 'add'}],
                                   headers=headers,
                                   expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_code)


def _create_node_locally(node):
    driver_factory.check_and_update_node_interfaces(node)
    node.create()
    return node


@mock.patch.object(rpcapi.ConductorAPI, 'create_node',
                   lambda _api, _ctx, node, _topic: _create_node_locally(node))
class TestPost(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestPost, self).setUp()
        self.config(enabled_drivers=['fake'])
        self.chassis = obj_utils.create_test_chassis(self.context)
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)

    @mock.patch.object(timeutils, 'utcnow')
    def _test_create_node(self, mock_utcnow, headers=None,
                          remove_chassis_uuid=False, **kwargs):
        headers = headers or {}
        ndict = test_api_utils.post_get_test_node(**kwargs)
        if remove_chassis_uuid:
            del ndict['chassis_uuid']
        test_time = datetime.datetime(2000, 1, 1, 0, 0)
        mock_utcnow.return_value = test_time
        response = self.post_json('/nodes', ndict,
                                  headers=headers)
        self.assertEqual(http_client.CREATED, response.status_int)
        result = self.get_json('/nodes/%s' % ndict['uuid'],
                               headers=headers)
        self.assertEqual(ndict['uuid'], result['uuid'])
        self.assertFalse(result['updated_at'])
        return_created_at = timeutils.parse_isotime(
            result['created_at']).replace(tzinfo=None)
        self.assertEqual(test_time, return_created_at)
        # Check location header
        self.assertIsNotNone(response.location)
        expected_location = '/v1/nodes/%s' % ndict['uuid']
        self.assertEqual(urlparse.urlparse(response.location).path,
                         expected_location)
        return result

    @mock.patch.object(notification_utils.LOG, 'exception', autospec=True)
    @mock.patch.object(notification_utils.LOG, 'warning', autospec=True)
    def test_create_node(self, mock_warning, mock_exception):
        self._test_create_node()
        self.assertFalse(mock_warning.called)
        self.assertFalse(mock_exception.called)

    def test_create_node_chassis_uuid_always_in_response(self):
        result = self._test_create_node(chassis_uuid=None)
        self.assertIsNone(result['chassis_uuid'])
        result = self._test_create_node(uuid=uuidutils.generate_uuid(),
                                        remove_chassis_uuid=True)
        self.assertIsNone(result['chassis_uuid'])

    def test_create_node_invalid_chassis(self):
        ndict = test_api_utils.post_get_test_node(chassis_uuid=0)
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])

    def test_create_node_explicit_network_interface(self):
        headers = {api_base.Version.string: '1.20'}
        result = self._test_create_node(headers=headers,
                                        network_interface='neutron')
        self.assertEqual('neutron', result['network_interface'])

    def test_create_node_specify_interfaces(self):
        headers = {api_base.Version.string: '1.31'}
        for field in api_utils.V31_FIELDS:
            cfg.CONF.set_override('enabled_%ss' % field, ['fake'])
        for field in api_utils.V31_FIELDS:
            node = {
                'uuid': uuidutils.generate_uuid(),
                field: 'fake',
                'driver': 'fake-hardware'
            }
            result = self._test_create_node(headers=headers, **node)
            self.assertEqual('fake', result[field])

    def test_create_node_specify_interfaces_bad_version(self):
        headers = {api_base.Version.string: '1.30'}
        for field in api_utils.V31_FIELDS:
            ndict = test_api_utils.post_get_test_node(**{field: 'fake'})
            response = self.post_json('/nodes', ndict, headers=headers,
                                      expect_errors=True)
            self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_create_node_classic_driver_specify_interface(self):
        headers = {api_base.Version.string: '1.31'}
        for field in api_utils.V31_FIELDS:
            node = {
                'uuid': uuidutils.generate_uuid(),
                field: 'fake',
            }
            ndict = test_api_utils.post_get_test_node(**node)
            response = self.post_json('/nodes', ndict,
                                      headers=headers,
                                      expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, response.status_int)
            self.assertEqual('application/json', response.content_type)
            self.assertTrue(response.json['error_message'])

    def test_create_node_explicit_storage_interface(self):
        headers = {api_base.Version.string: '1.33'}
        result = self._test_create_node(headers=headers,
                                        storage_interface='cinder')
        self.assertEqual('cinder', result['storage_interface'])

    def test_create_node_name_empty_invalid(self):
        ndict = test_api_utils.post_get_test_node(name='')
        response = self.post_json('/nodes', ndict,
                                  headers={api_base.Version.string: "1.10"},
                                  expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])

    def test_create_node_name_empty_not_acceptable(self):
        ndict = test_api_utils.post_get_test_node(name='')
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])

    def test_create_node_reserved_name(self):
        reserved_names = api_utils.get_controller_reserved_names(
            api_node.NodesController)
        for name in reserved_names:
            ndict = test_api_utils.post_get_test_node(name=name)
            response = self.post_json(
                '/nodes', ndict, headers={api_base.Version.string: "1.10"},
                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, response.status_int)
            self.assertEqual('application/json', response.content_type)
            self.assertTrue(response.json['error_message'])

    def test_create_node_default_state_none(self):
        ndict = test_api_utils.post_get_test_node()
        response = self.post_json('/nodes', ndict,
                                  headers={api_base.Version.string: "1.10"})
        self.assertEqual(http_client.CREATED, response.status_int)

        # default state remains NONE/AVAILABLE
        result = self.get_json('/nodes/%s' % ndict['uuid'])
        self.assertEqual(states.NOSTATE, result['provision_state'])
        result = self.get_json('/nodes/%s' % ndict['uuid'],
                               headers={api_base.Version.string: "1.10"})
        self.assertEqual(ndict['uuid'], result['uuid'])
        self.assertEqual(states.AVAILABLE, result['provision_state'])

    def test_create_node_default_state_enroll(self):
        ndict = test_api_utils.post_get_test_node()
        response = self.post_json('/nodes', ndict,
                                  headers={api_base.Version.string: "1.11"})
        self.assertEqual(http_client.CREATED, response.status_int)

        # default state is ENROLL
        result = self.get_json('/nodes/%s' % ndict['uuid'])
        self.assertEqual(ndict['uuid'], result['uuid'])
        self.assertEqual(states.ENROLL, result['provision_state'])

    def test_create_node_doesnt_contain_id(self):
        # FIXME(comstud): I'd like to make this test not use the
        # dbapi, however, no matter what I do when trying to mock
        # Node.create(), the API fails to convert the objects.Node
        # into the API Node object correctly (it leaves all fields
        # as Unset).
        with mock.patch.object(self.dbapi, 'create_node',
                               wraps=self.dbapi.create_node) as cn_mock:
            ndict = test_api_utils.post_get_test_node(extra={'foo': 123})
            self.post_json('/nodes', ndict)
            result = self.get_json('/nodes/%s' % ndict['uuid'])
            self.assertEqual(ndict['extra'], result['extra'])
            cn_mock.assert_called_once_with(mock.ANY)
            # Check that 'id' is not in first arg of positional args
            self.assertNotIn('id', cn_mock.call_args[0][0])

    def _test_jsontype_attributes(self, attr_name):
        kwargs = {attr_name: {'str': 'foo', 'int': 123, 'float': 0.1,
                              'bool': True, 'list': [1, 2], 'none': None,
                              'dict': {'cat': 'meow'}}}
        ndict = test_api_utils.post_get_test_node(**kwargs)
        self.post_json('/nodes', ndict)
        result = self.get_json('/nodes/%s' % ndict['uuid'])
        self.assertEqual(ndict[attr_name], result[attr_name])

    def test_create_node_valid_extra(self):
        self._test_jsontype_attributes('extra')

    def test_create_node_valid_properties(self):
        self._test_jsontype_attributes('properties')

    def test_create_node_valid_driver_info(self):
        self._test_jsontype_attributes('driver_info')

    def test_create_node_valid_instance_info(self):
        self._test_jsontype_attributes('instance_info')

    def _test_vendor_passthru_ok(self, mock_vendor, return_value=None,
                                 is_async=True):
        expected_status = http_client.ACCEPTED if is_async else http_client.OK
        expected_return_value = json.dumps(return_value)
        if six.PY3:
            expected_return_value = expected_return_value.encode('utf-8')

        node = obj_utils.create_test_node(self.context)
        info = {'foo': 'bar'}
        mock_vendor.return_value = {'return': return_value,
                                    'async': is_async,
                                    'attach': False}
        response = self.post_json('/nodes/%s/vendor_passthru/test' % node.uuid,
                                  info)
        mock_vendor.assert_called_once_with(
            mock.ANY, node.uuid, 'test', 'POST', info, 'test-topic')
        self.assertEqual(expected_return_value, response.body)
        self.assertEqual(expected_status, response.status_code)

    def _test_vendor_passthru_ok_by_name(self, mock_vendor, return_value=None,
                                         is_async=True):
        expected_status = http_client.ACCEPTED if is_async else http_client.OK
        expected_return_value = json.dumps(return_value)
        if six.PY3:
            expected_return_value = expected_return_value.encode('utf-8')

        node = obj_utils.create_test_node(self.context, name='node-109')
        info = {'foo': 'bar'}
        mock_vendor.return_value = {'return': return_value,
                                    'async': is_async,
                                    'attach': False}
        response = self.post_json('/nodes/%s/vendor_passthru/test' % node.name,
                                  info,
                                  headers={api_base.Version.string: "1.5"})
        mock_vendor.assert_called_once_with(
            mock.ANY, node.uuid, 'test', 'POST', info, 'test-topic')
        self.assertEqual(expected_return_value, response.body)
        self.assertEqual(expected_status, response.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_async(self, mock_vendor):
        self._test_vendor_passthru_ok(mock_vendor)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_sync(self, mock_vendor):
        return_value = {'cat': 'meow'}
        self._test_vendor_passthru_ok(mock_vendor, return_value=return_value,
                                      is_async=False)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_put(self, mocked_vendor_passthru):
        node = obj_utils.create_test_node(self.context)
        return_value = {'return': None, 'async': True, 'attach': False}
        mocked_vendor_passthru.return_value = return_value
        response = self.put_json(
            '/nodes/%s/vendor_passthru/do_test' % node.uuid,
            {'test_key': 'test_value'})
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(return_value['return'], response.json)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_by_name(self, mock_vendor):
        self._test_vendor_passthru_ok_by_name(mock_vendor)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_get(self, mocked_vendor_passthru):
        node = obj_utils.create_test_node(self.context)
        return_value = {'return': 'foo', 'async': False, 'attach': False}
        mocked_vendor_passthru.return_value = return_value
        response = self.get_json(
            '/nodes/%s/vendor_passthru/do_test' % node.uuid)
        self.assertEqual(return_value['return'], response)

    @mock.patch.object(rpcapi.ConductorAPI, 'vendor_passthru')
    def test_vendor_passthru_delete(self, mock_vendor_passthru):
        node = obj_utils.create_test_node(self.context)
        return_value = {'return': None, 'async': True, 'attach': False}
        mock_vendor_passthru.return_value = return_value
        response = self.delete(
            '/nodes/%s/vendor_passthru/do_test' % node.uuid)
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(return_value['return'], response.json)

    def test_vendor_passthru_no_such_method(self):
        node = obj_utils.create_test_node(self.context)
        uuid = node.uuid
        info = {'foo': 'bar'}

        with mock.patch.object(
                rpcapi.ConductorAPI, 'vendor_passthru') as mock_vendor:
            mock_vendor.side_effect = exception.UnsupportedDriverExtension(
                **{'driver': node.driver, 'node': uuid, 'extension': 'test'})
            response = self.post_json('/nodes/%s/vendor_passthru/test' % uuid,
                                      info, expect_errors=True)
            mock_vendor.assert_called_once_with(
                mock.ANY, uuid, 'test', 'POST', info, 'test-topic')
            self.assertEqual(http_client.BAD_REQUEST, response.status_code)

    def test_vendor_passthru_without_method(self):
        node = obj_utils.create_test_node(self.context)
        response = self.post_json('/nodes/%s/vendor_passthru' % node.uuid,
                                  {'foo': 'bar'}, expect_errors=True)
        self.assertEqual('application/json', response.content_type, )
        self.assertEqual(http_client.BAD_REQUEST, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_post_ports_subresource_no_node_id(self):
        node = obj_utils.create_test_node(self.context)
        pdict = test_api_utils.port_post_data(node_id=None)
        pdict['node_uuid'] = node.uuid
        response = self.post_json('/nodes/ports', pdict,
                                  expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_post_ports_subresource(self):
        node = obj_utils.create_test_node(self.context)
        pdict = test_api_utils.port_post_data(node_id=None)
        pdict['node_uuid'] = node.uuid
        response = self.post_json('/nodes/%s/ports' % node.uuid, pdict,
                                  expect_errors=True)
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_post_portgroups_subresource(self):
        node = obj_utils.create_test_node(self.context)
        pgdict = test_api_utils.portgroup_post_data(node_id=None)
        pgdict['node_uuid'] = node.uuid
        response = self.post_json(
            '/nodes/%s/portgroups' % node.uuid, pgdict, expect_errors=True,
            headers={'X-OpenStack-Ironic-API-Version': '1.24'})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_post_volume_connectors_subresource_no_node_id(self):
        node = obj_utils.create_test_node(self.context)
        pdict = test_api_utils.volume_connector_post_data(node_id=None)
        pdict['node_uuid'] = node.uuid
        response = self.post_json(
            '/nodes/volume/connectors', pdict,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.NOT_FOUND, response.status_int)

    def test_post_volume_connectors_subresource(self):
        node = obj_utils.create_test_node(self.context)
        pdict = test_api_utils.volume_connector_post_data(node_id=None)
        pdict['node_uuid'] = node.uuid
        response = self.post_json(
            '/nodes/%s/volume/connectors' % node.uuid, pdict,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_post_volume_targets_subresource(self):
        node = obj_utils.create_test_node(self.context)
        pdict = test_api_utils.volume_target_post_data(node_id=None)
        pdict['node_uuid'] = node.uuid
        response = self.post_json(
            '/nodes/%s/volume/targets' % node.uuid, pdict,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_create_node_no_mandatory_field_driver(self):
        ndict = test_api_utils.post_get_test_node()
        del ndict['driver']
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])

    def test_create_node_invalid_driver(self):
        ndict = test_api_utils.post_get_test_node()
        self.mock_gtf.side_effect = exception.NoValidHost('Fake Error')
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])

    def test_create_node_no_chassis_uuid(self):
        ndict = test_api_utils.post_get_test_node()
        del ndict['chassis_uuid']
        response = self.post_json('/nodes', ndict)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.CREATED, response.status_int)
        # Check location header
        self.assertIsNotNone(response.location)
        expected_location = '/v1/nodes/%s' % ndict['uuid']
        self.assertEqual(urlparse.urlparse(response.location).path,
                         expected_location)

    @mock.patch.object(notification_utils, '_emit_api_notification')
    def test_create_node_with_chassis_uuid(self, mock_notify):
        ndict = test_api_utils.post_get_test_node(
            chassis_uuid=self.chassis.uuid)
        response = self.post_json('/nodes', ndict)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.CREATED, response.status_int)
        result = self.get_json('/nodes/%s' % ndict['uuid'])
        self.assertEqual(ndict['chassis_uuid'], result['chassis_uuid'])
        # Check location header
        self.assertIsNotNone(response.location)
        expected_location = '/v1/nodes/%s' % ndict['uuid']
        self.assertEqual(urlparse.urlparse(response.location).path,
                         expected_location)
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY, 'create',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.START,
                                      chassis_uuid=self.chassis.uuid),
                                      mock.call(mock.ANY, mock.ANY, 'create',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.END,
                                      chassis_uuid=self.chassis.uuid)])

    def test_create_node_chassis_uuid_not_found(self):
        ndict = test_api_utils.post_get_test_node(
            chassis_uuid='1a1a1a1a-2b2b-3c3c-4d4d-5e5e5e5e5e5e')
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertTrue(response.json['error_message'])

    def test_create_node_with_internal_field(self):
        ndict = test_api_utils.post_get_test_node()
        ndict['reservation'] = 'fake'
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)
        self.assertTrue(response.json['error_message'])

    @mock.patch.object(rpcapi.ConductorAPI, 'get_node_vendor_passthru_methods')
    def test_vendor_passthru_methods(self, get_methods_mock):
        return_value = {'foo': 'bar'}
        get_methods_mock.return_value = return_value
        node = obj_utils.create_test_node(self.context)
        path = '/nodes/%s/vendor_passthru/methods' % node.uuid

        data = self.get_json(path)
        self.assertEqual(return_value, data)
        get_methods_mock.assert_called_once_with(mock.ANY, node.uuid,
                                                 topic=mock.ANY)

        # Now let's test the cache: Reset the mock
        get_methods_mock.reset_mock()

        # Call it again
        data = self.get_json(path)
        self.assertEqual(return_value, data)
        # Assert RPC method wasn't called this time
        self.assertFalse(get_methods_mock.called)

    def test_create_node_network_interface(self):
        ndict = test_api_utils.post_get_test_node(
            network_interface='flat')
        response = self.post_json('/nodes', ndict,
                                  headers={api_base.Version.string:
                                           str(api_v1.MAX_VER)})
        self.assertEqual(http_client.CREATED, response.status_int)
        result = self.get_json('/nodes/%s' % ndict['uuid'],
                               headers={api_base.Version.string:
                                        str(api_v1.MAX_VER)})
        self.assertEqual('flat', result['network_interface'])

    def test_create_node_network_interface_old_api_version(self):
        ndict = test_api_utils.post_get_test_node(
            network_interface='flat')
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_create_node_invalid_network_interface(self):
        ndict = test_api_utils.post_get_test_node(
            network_interface='foo')
        response = self.post_json('/nodes', ndict, expect_errors=True,
                                  headers={api_base.Version.string:
                                           str(api_v1.MAX_VER)})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_create_node_resource_class(self):
        ndict = test_api_utils.post_get_test_node(
            resource_class='foo')
        response = self.post_json('/nodes', ndict,
                                  headers={api_base.Version.string:
                                           str(api_v1.MAX_VER)})
        self.assertEqual(http_client.CREATED, response.status_int)
        result = self.get_json('/nodes/%s' % ndict['uuid'],
                               headers={api_base.Version.string:
                                        str(api_v1.MAX_VER)})
        self.assertEqual('foo', result['resource_class'])

    def test_create_node_resource_class_old_api_version(self):
        ndict = test_api_utils.post_get_test_node(
            resource_class='foo')
        response = self.post_json('/nodes', ndict, expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_create_node_storage_interface_old_api_version(self):
        headers = {api_base.Version.string: '1.32'}
        ndict = test_api_utils.post_get_test_node(storage_interface='cinder')
        response = self.post_json('/nodes', ndict, headers=headers,
                                  expect_errors=True)
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.NOT_ACCEPTABLE, response.status_int)

    def test_create_node_invalid_storage_interface(self):
        ndict = test_api_utils.post_get_test_node(storage_interface='foo')
        response = self.post_json('/nodes', ndict, expect_errors=True,
                                  headers={api_base.Version.string:
                                           str(api_v1.MAX_VER)})
        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)


class TestDelete(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestDelete, self).setUp()
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)

    @mock.patch.object(notification_utils, '_emit_api_notification')
    @mock.patch.object(rpcapi.ConductorAPI, 'destroy_node')
    def test_delete_node(self, mock_dn, mock_notify):
        node = obj_utils.create_test_node(self.context)
        self.delete('/nodes/%s' % node.uuid)
        mock_dn.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY, 'delete',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.START,
                                      chassis_uuid=None),
                                      mock.call(mock.ANY, mock.ANY, 'delete',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.END,
                                      chassis_uuid=None)])

    @mock.patch.object(rpcapi.ConductorAPI, 'destroy_node')
    def test_delete_node_by_name_unsupported(self, mock_dn):
        node = obj_utils.create_test_node(self.context, name='foo')
        self.delete('/nodes/%s' % node.name,
                    expect_errors=True)
        self.assertFalse(mock_dn.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'destroy_node')
    def test_delete_node_by_name(self, mock_dn):
        node = obj_utils.create_test_node(self.context, name='foo')
        self.delete('/nodes/%s' % node.name,
                    headers={api_base.Version.string: "1.5"})
        mock_dn.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')

    @mock.patch.object(objects.Node, 'get_by_uuid')
    def test_delete_node_not_found(self, mock_gbu):
        node = obj_utils.get_test_node(self.context)
        mock_gbu.side_effect = exception.NodeNotFound(node=node.uuid)

        response = self.delete('/nodes/%s' % node.uuid, expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])
        mock_gbu.assert_called_once_with(mock.ANY, node.uuid)

    @mock.patch.object(objects.Node, 'get_by_name')
    def test_delete_node_not_found_by_name_unsupported(self, mock_gbn):
        node = obj_utils.get_test_node(self.context, name='foo')
        mock_gbn.side_effect = exception.NodeNotFound(node=node.name)

        response = self.delete('/nodes/%s' % node.name,
                               expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)
        self.assertFalse(mock_gbn.called)

    @mock.patch.object(objects.Node, 'get_by_name')
    def test_delete_node_not_found_by_name(self, mock_gbn):
        node = obj_utils.get_test_node(self.context, name='foo')
        mock_gbn.side_effect = exception.NodeNotFound(node=node.name)

        response = self.delete('/nodes/%s' % node.name,
                               headers={api_base.Version.string: "1.5"},
                               expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_int)
        self.assertEqual('application/json', response.content_type)
        self.assertTrue(response.json['error_message'])
        mock_gbn.assert_called_once_with(mock.ANY, node.name)

    def test_delete_ports_subresource_no_port_id(self):
        node = obj_utils.create_test_node(self.context)
        response = self.delete('/nodes/%s/ports' % node.uuid,
                               expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_delete_ports_subresource(self):
        node = obj_utils.create_test_node(self.context)
        port = obj_utils.create_test_port(self.context, node_id=node.id)
        response = self.delete(
            '/nodes/%(node_uuid)s/ports/%(port_uuid)s' %
            {'node_uuid': node.uuid, 'port_uuid': port.uuid},
            expect_errors=True)
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_delete_portgroup_subresource(self):
        node = obj_utils.create_test_node(self.context)
        pg = obj_utils.create_test_portgroup(self.context, node_id=node.id)
        response = self.delete(
            '/nodes/%(node_uuid)s/portgroups/%(pg_uuid)s' %
            {'node_uuid': node.uuid, 'pg_uuid': pg.uuid},
            expect_errors=True,
            headers={'X-OpenStack-Ironic-API-Version': '1.24'})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_delete_volume_connectors_subresource_no_connector_id(self):
        node = obj_utils.create_test_node(self.context)
        response = self.delete(
            '/nodes/%s/volume/connectors' % node.uuid,
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.BAD_REQUEST, response.status_int)

    def test_delete_volume_connectors_subresource(self):
        node = obj_utils.create_test_node(self.context)
        connector = obj_utils.create_test_volume_connector(self.context,
                                                           node_id=node.id)
        response = self.delete(
            '/nodes/%s/volume/connectors/%s' % (node.uuid, connector.uuid),
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    def test_delete_volume_targets_subresource(self):
        node = obj_utils.create_test_node(self.context)
        target = obj_utils.create_test_volume_target(self.context,
                                                     node_id=node.id)
        response = self.delete(
            '/nodes/%s/volume/targets/%s' % (node.uuid, target.uuid),
            expect_errors=True,
            headers={api_base.Version.string: str(api_v1.MAX_VER)})
        self.assertEqual(http_client.FORBIDDEN, response.status_int)

    @mock.patch.object(notification_utils, '_emit_api_notification')
    @mock.patch.object(rpcapi.ConductorAPI, 'destroy_node')
    def test_delete_associated(self, mock_dn, mock_notify):
        node = obj_utils.create_test_node(
            self.context,
            instance_uuid='aaaaaaaa-1111-bbbb-2222-cccccccccccc')
        mock_dn.side_effect = exception.NodeAssociated(
            node=node.uuid, instance=node.instance_uuid)

        response = self.delete('/nodes/%s' % node.uuid, expect_errors=True)
        self.assertEqual(http_client.CONFLICT, response.status_int)
        mock_dn.assert_called_once_with(mock.ANY, node.uuid, 'test-topic')
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY, 'delete',
                                      obj_fields.NotificationLevel.INFO,
                                      obj_fields.NotificationStatus.START,
                                      chassis_uuid=None),
                                      mock.call(mock.ANY, mock.ANY, 'delete',
                                      obj_fields.NotificationLevel.ERROR,
                                      obj_fields.NotificationStatus.ERROR,
                                      chassis_uuid=None)])

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_delete_node_maintenance_mode(self, mock_update, mock_get):
        node = obj_utils.create_test_node(self.context, maintenance=True,
                                          maintenance_reason='blah')
        mock_get.return_value = node
        response = self.delete('/nodes/%s/maintenance' % node.uuid)
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(b'', response.body)
        self.assertFalse(node.maintenance)
        self.assertIsNone(node.maintenance_reason)
        mock_get.assert_called_once_with(mock.ANY, node.uuid)
        mock_update.assert_called_once_with(mock.ANY, mock.ANY,
                                            topic='test-topic')

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_delete_node_maintenance_mode_by_name(self, mock_update,
                                                  mock_get):
        node = obj_utils.create_test_node(self.context, maintenance=True,
                                          maintenance_reason='blah',
                                          name='foo')
        mock_get.return_value = node
        response = self.delete('/nodes/%s/maintenance' % node.name,
                               headers={api_base.Version.string: "1.5"})
        self.assertEqual(http_client.ACCEPTED, response.status_int)
        self.assertEqual(b'', response.body)
        self.assertFalse(node.maintenance)
        self.assertIsNone(node.maintenance_reason)
        mock_get.assert_called_once_with(mock.ANY, node.name)
        mock_update.assert_called_once_with(mock.ANY, mock.ANY,
                                            topic='test-topic')


class TestPut(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestPut, self).setUp()
        self.node = obj_utils.create_test_node(
            self.context,
            provision_state=states.AVAILABLE, name='node-39')
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'change_node_power_state')
        self.mock_cnps = p.start()
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'do_node_deploy')
        self.mock_dnd = p.start()
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'do_node_tear_down')
        self.mock_dntd = p.start()
        self.addCleanup(p.stop)
        p = mock.patch.object(rpcapi.ConductorAPI, 'inspect_hardware')
        self.mock_dnih = p.start()
        self.addCleanup(p.stop)

    def _test_power_state_success(self, target_state, timeout, api_version):
        if timeout is None:
            body = {'target': target_state}
        else:
            body = {'target': target_state, 'timeout': timeout}

        if api_version is None:
            response = self.put_json(
                '/nodes/%s/states/power' % self.node.uuid, body)
        else:
            response = self.put_json(
                '/nodes/%s/states/power' % self.node.uuid, body,
                headers={api_base.Version.string: api_version})

        self.assertEqual(http_client.ACCEPTED, response.status_code)
        self.assertEqual(b'', response.body)
        self.mock_cnps.assert_called_once_with(mock.ANY,
                                               self.node.uuid,
                                               target_state,
                                               timeout=timeout,
                                               topic='test-topic')
        # Check location header
        self.assertIsNotNone(response.location)
        expected_location = '/v1/nodes/%s/states' % self.node.uuid
        self.assertEqual(urlparse.urlparse(response.location).path,
                         expected_location)

    def _test_power_state_failure(self, target_state, http_status_code,
                                  timeout, api_version):
        if timeout is None:
            body = {'target': target_state}
        else:
            body = {'target': target_state, 'timeout': timeout}

        if api_version is None:
            response = self.put_json(
                '/nodes/%s/states/power' % self.node.uuid, body,
                expect_errors=True)
        else:
            response = self.put_json(
                '/nodes/%s/states/power' % self.node.uuid, body,
                headers={api_base.Version.string: api_version},
                expect_errors=True)

        self.assertEqual('application/json', response.content_type)
        self.assertEqual(http_status_code, response.status_code)
        self.assertTrue(response.json['error_message'])

    def test_power_state_power_on_no_timeout_no_ver(self):
        self._test_power_state_success(states.POWER_ON, None, None)

    def test_power_state_power_on_no_timeout_valid_soft_ver(self):
        self._test_power_state_success(states.POWER_ON, None, "1.27")

    def test_power_state_power_on_no_timeout_invalid_soft_ver(self):
        self._test_power_state_success(states.POWER_ON, None, "1.26")

    def test_power_state_power_on_valid_timeout_no_ver(self):
        self._test_power_state_failure(
            states.POWER_ON, http_client.NOT_ACCEPTABLE, 2, None)

    def test_power_state_power_on_valid_timeout_valid_soft_ver(self):
        self._test_power_state_success(states.POWER_ON, 2, "1.27")

    def test_power_state_power_on_valid_timeout_invalid_soft_ver(self):
        self._test_power_state_failure(
            states.POWER_ON, http_client.NOT_ACCEPTABLE, 2, "1.26")

    def test_power_state_power_on_invalid_timeout_no_ver(self):
        self._test_power_state_failure(
            states.POWER_ON, http_client.BAD_REQUEST, 0, None)

    def test_power_state_power_on_invalid_timeout_valid_soft_ver(self):
        self._test_power_state_failure(
            states.POWER_ON, http_client.BAD_REQUEST, 0, "1.27")

    def test_power_state_power_on_invalid_timeout_invalid_soft_ver(self):
        self._test_power_state_failure(
            states.POWER_ON, http_client.BAD_REQUEST, 0, "1.26")

    def test_power_state_soft_power_off_no_timeout_no_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, None, None)

    def test_power_state_soft_power_off_no_timeout_valid_soft_ver(self):
        self._test_power_state_success(states.SOFT_POWER_OFF, None, "1.27")

    def test_power_state_soft_power_off_no_timeout_invalid_soft_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, None, "1.26")

    def test_power_state_soft_power_off_valid_timeout_no_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, 2, None)

    def test_power_state_soft_power_off_valid_timeout_valid_soft_ver(self):
        self._test_power_state_success(states.SOFT_POWER_OFF, 2, "1.27")

    def test_power_state_soft_power_off_valid_timeout_invalid_soft_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, 2, "1.26")

    def test_power_state_soft_power_off_invalid_timeout_no_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, 0, None)

    def test_power_state_soft_power_off_invalid_timeout_valid_soft_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.BAD_REQUEST, 0, "1.27")

    def test_power_state_soft_power_off_invalid_timeout_invalid_soft_ver(self):
        self._test_power_state_failure(
            states.SOFT_POWER_OFF, http_client.NOT_ACCEPTABLE, 0, "1.26")

    def test_power_state_by_name_unsupported(self):
        response = self.put_json('/nodes/%s/states/power' % self.node.name,
                                 {'target': states.POWER_ON},
                                 expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, response.status_code)

    def test_power_state_by_name(self):
        response = self.put_json('/nodes/%s/states/power' % self.node.name,
                                 {'target': states.POWER_ON},
                                 headers={api_base.Version.string: "1.5"})
        self.assertEqual(http_client.ACCEPTED, response.status_code)
        self.assertEqual(b'', response.body)
        self.mock_cnps.assert_called_once_with(mock.ANY,
                                               self.node.uuid,
                                               states.POWER_ON,
                                               timeout=None,
                                               topic='test-topic')
        # Check location header
        self.assertIsNotNone(response.location)
        expected_location = '/v1/nodes/%s/states' % self.node.name
        self.assertEqual(urlparse.urlparse(response.location).path,
                         expected_location)

    def test_power_invalid_state_request(self):
        ret = self.put_json('/nodes/%s/states/power' % self.node.uuid,
                            {'target': 'not-supported'}, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_power_change_when_being_cleaned(self):
        for state in (states.CLEANING, states.CLEANWAIT):
            self.node.provision_state = state
            self.node.save()
            ret = self.put_json('/nodes/%s/states/power' % self.node.uuid,
                                {'target': states.POWER_OFF},
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_provision_invalid_state_request(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': 'not-supported'}, expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_provision_with_deploy(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.ACTIVE})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dnd.assert_called_once_with(
            mock.ANY, self.node.uuid, False, None, 'test-topic')
        # Check location header
        self.assertIsNotNone(ret.location)
        expected_location = '/v1/nodes/%s/states' % self.node.uuid
        self.assertEqual(urlparse.urlparse(ret.location).path,
                         expected_location)

    def test_provision_by_name_unsupported(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.name,
                            {'target': states.ACTIVE},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)

    def test_provision_by_name(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.name,
                            {'target': states.ACTIVE},
                            headers={api_base.Version.string: "1.5"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dnd.assert_called_once_with(
            mock.ANY, self.node.uuid, False, None, 'test-topic')

    def test_provision_with_deploy_configdrive(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.ACTIVE, 'configdrive': 'foo'})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dnd.assert_called_once_with(
            mock.ANY, self.node.uuid, False, 'foo', 'test-topic')
        # Check location header
        self.assertIsNotNone(ret.location)
        expected_location = '/v1/nodes/%s/states' % self.node.uuid
        self.assertEqual(urlparse.urlparse(ret.location).path,
                         expected_location)

    def test_provision_with_configdrive_not_active(self):
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.DELETED, 'configdrive': 'foo'},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_provision_with_tear_down(self):
        node = self.node
        node.provision_state = states.ACTIVE
        node.target_provision_state = states.NOSTATE
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.DELETED})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dntd.assert_called_once_with(
            mock.ANY, node.uuid, 'test-topic')
        # Check location header
        self.assertIsNotNone(ret.location)
        expected_location = '/v1/nodes/%s/states' % node.uuid
        self.assertEqual(urlparse.urlparse(ret.location).path,
                         expected_location)

    def test_provision_already_in_progress(self):
        node = self.node
        node.provision_state = states.DEPLOYING
        node.target_provision_state = states.ACTIVE
        node.reservation = 'fake-host'
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.ACTIVE},
                            expect_errors=True)
        self.assertEqual(http_client.CONFLICT, ret.status_code)  # Conflict
        self.assertFalse(self.mock_dnd.called)

    def test_provision_locked_with_correct_state(self):
        node = self.node
        node.provision_state = states.AVAILABLE
        node.target_provision_state = states.NOSTATE
        node.reservation = 'fake-host'
        node.save()
        self.mock_dnd.side_effect = exception.NodeLocked(node='', host='')
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.ACTIVE},
                            expect_errors=True)
        self.assertEqual(http_client.CONFLICT, ret.status_code)  # Conflict
        self.assertTrue(self.mock_dnd.called)

    def test_provision_with_tear_down_in_progress_deploywait(self):
        node = self.node
        node.provision_state = states.DEPLOYWAIT
        node.target_provision_state = states.ACTIVE
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.DELETED})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dntd.assert_called_once_with(
            mock.ANY, node.uuid, 'test-topic')
        # Check location header
        self.assertIsNotNone(ret.location)
        expected_location = '/v1/nodes/%s/states' % node.uuid
        self.assertEqual(urlparse.urlparse(ret.location).path,
                         expected_location)

    # NOTE(deva): this test asserts API funcionality which is not part of
    # the new-ironic-state-machine in Kilo. It is retained for backwards
    # compatibility with Juno.
    # TODO(deva): add a deprecation-warning to the REST result
    # and check for it here.
    def test_provision_with_deploy_after_deployfail(self):
        node = self.node
        node.provision_state = states.DEPLOYFAIL
        node.target_provision_state = states.ACTIVE
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.ACTIVE})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.mock_dnd.assert_called_once_with(
            mock.ANY, node.uuid, False, None, 'test-topic')
        # Check location header
        self.assertIsNotNone(ret.location)
        expected_location = '/v1/nodes/%s/states' % node.uuid
        self.assertEqual(expected_location,
                         urlparse.urlparse(ret.location).path)

    def test_provision_already_in_state(self):
        self.node.provision_state = states.ACTIVE
        self.node.save()
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.ACTIVE},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_provide_from_manage(self, mock_dpa):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['provide']},
                            headers={api_base.Version.string: "1.4"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['provide'],
                                         'test-topic')

    def test_inspect_already_in_progress(self):
        node = self.node
        node.provision_state = states.INSPECTING
        node.target_provision_state = states.MANAGEABLE
        node.reservation = 'fake-host'
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': states.MANAGEABLE},
                            expect_errors=True)
        self.assertEqual(http_client.CONFLICT, ret.status_code)  # Conflict

    def test_inspect_validation_failed_status_code(self):
        self.mock_dnih.side_effect = exception.InvalidParameterValue(
            err='Failed to validate inspection or power info.')
        node = self.node
        node.provision_state = states.MANAGEABLE
        node.reservation = 'fake-host'
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': 'inspect'},
                            headers={api_base.Version.string: "1.6"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_inspect_validation_failed_missing_parameter_value(self):
        self.mock_dnih.side_effect = exception.MissingParameterValue(
            err='Failed to validate inspection or power info.')
        node = self.node
        node.provision_state = states.MANAGEABLE
        node.reservation = 'fake-host'
        node.save()
        ret = self.put_json('/nodes/%s/states/provision' % node.uuid,
                            {'target': 'inspect'},
                            headers={api_base.Version.string: "1.6"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_manage_from_available(self, mock_dpa):
        self.node.provision_state = states.AVAILABLE
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['manage']},
                            headers={api_base.Version.string: "1.4"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['manage'],
                                         'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_bad_requests_in_managed_state(self, mock_dpa):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()

        for state in [states.ACTIVE, states.REBUILD, states.DELETED]:
            ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                                {'target': states.ACTIVE},
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertEqual(0, mock_dpa.call_count)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_abort_cleanwait(self, mock_dpa):
        self.node.provision_state = states.CLEANWAIT
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['abort']},
                            headers={api_base.Version.string: "1.13"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['abort'],
                                         'test-topic')

    def test_abort_invalid_state(self):
        # "abort" is only valid for nodes in CLEANWAIT
        self.node.provision_state = states.CLEANING
        self.node.save()
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['abort']},
                            headers={api_base.Version.string: "1.13"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_provision_with_cleansteps_not_clean(self):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['provide'],
                             'clean_steps': 'foo'},
                            headers={api_base.Version.string: "1.4"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    def test_clean_no_cleansteps(self):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['clean']},
                            headers={api_base.Version.string: "1.15"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_node_clean')
    @mock.patch.object(api_node, '_check_clean_steps')
    def test_clean_check_steps_fail(self, mock_check, mock_rpcapi):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()
        mock_check.side_effect = exception.InvalidParameterValue('bad')
        clean_steps = [{"step": "upgrade_firmware", "interface": "deploy"}]
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['clean'],
                             'clean_steps': clean_steps},
                            headers={api_base.Version.string: "1.15"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        mock_check.assert_called_once_with(clean_steps)
        self.assertFalse(mock_rpcapi.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_node_clean')
    @mock.patch.object(api_node, '_check_clean_steps')
    def test_clean(self, mock_check, mock_rpcapi):
        self.node.provision_state = states.MANAGEABLE
        self.node.save()
        clean_steps = [{"step": "upgrade_firmware", "interface": "deploy"}]
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['clean'],
                             'clean_steps': clean_steps},
                            headers={api_base.Version.string: "1.15"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_check.assert_called_once_with(clean_steps)
        mock_rpcapi.assert_called_once_with(mock.ANY, self.node.uuid,
                                            clean_steps, 'test-topic')

    def test_adopt_raises_error_before_1_17(self):
        """Test that a lower API client cannot use the adopt verb"""
        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['adopt']},
                            headers={api_base.Version.string: "1.16"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_adopt_from_manage(self, mock_dpa):
        """Test that a node can be adopted from the manageable state"""
        self.node.provision_state = states.MANAGEABLE
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['adopt']},
                            headers={api_base.Version.string: "1.17"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['adopt'],
                                         'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_adopt_from_adoptfail(self, mock_dpa):
        """Test that a node in ADOPTFAIL can be adopted"""
        self.node.provision_state = states.ADOPTFAIL
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['adopt']},
                            headers={api_base.Version.string: "1.17"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['adopt'],
                                         'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_adopt_from_active_fails(self, mock_dpa):
        """Test that an ACTIVE node cannot be adopted"""
        self.node.provision_state = states.ACTIVE
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['adopt']},
                            headers={api_base.Version.string: "1.17"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertEqual(0, mock_dpa.call_count)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_manage_from_adoptfail(self, mock_dpa):
        """Test that a node can be sent to MANAGEABLE from ADOPTFAIL"""
        self.node.provision_state = states.ADOPTFAIL
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.VERBS['manage']},
                            headers={api_base.Version.string: "1.17"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_dpa.assert_called_once_with(mock.ANY, self.node.uuid,
                                         states.VERBS['manage'],
                                         'test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_bad_requests_in_adopting_state(self, mock_dpa):
        """Test that a node in ADOPTING fails with invalid requests

        Verify that an API request fails if the ACTIVE, REBUILD, or DELETED
        state is requested by an API client when the node is in ADOPTING
        state.
        """
        self.node.provision_state = states.ADOPTING
        self.node.save()

        for state in [states.ACTIVE, states.REBUILD, states.DELETED]:
            ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                                {'target': state},
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertEqual(0, mock_dpa.call_count)

    @mock.patch.object(rpcapi.ConductorAPI, 'do_provisioning_action')
    def test_bad_requests_in_adoption_failed_state(self, mock_dpa):
        """Test that a node in ADOPTFAIL fails with invalid requests

        Verify that an API request fails if the ACTIVE, REBUILD, or DELETED
        state is requested by an API client when the node is in ADOPTFAIL
        state.
        """
        self.node.provision_state = states.ADOPTFAIL
        self.node.save()

        for state in [states.ACTIVE, states.REBUILD, states.DELETED]:
            ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                                {'target': state},
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertEqual(0, mock_dpa.call_count)

    def test_set_console_mode_enabled(self):
        with mock.patch.object(rpcapi.ConductorAPI,
                               'set_console_mode') as mock_scm:
            ret = self.put_json('/nodes/%s/states/console' % self.node.uuid,
                                {'enabled': "true"})
            self.assertEqual(http_client.ACCEPTED, ret.status_code)
            self.assertEqual(b'', ret.body)
            mock_scm.assert_called_once_with(mock.ANY, self.node.uuid,
                                             True, 'test-topic')
            # Check location header
            self.assertIsNotNone(ret.location)
            expected_location = '/v1/nodes/%s/states/console' % self.node.uuid
            self.assertEqual(urlparse.urlparse(ret.location).path,
                             expected_location)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_console_mode')
    def test_set_console_by_name_unsupported(self, mock_scm):
        ret = self.put_json('/nodes/%s/states/console' % self.node.name,
                            {'enabled': "true"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_console_mode')
    def test_set_console_by_name(self, mock_scm):
        ret = self.put_json('/nodes/%s/states/console' % self.node.name,
                            {'enabled': "true"},
                            headers={api_base.Version.string: "1.5"})
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_scm.assert_called_once_with(mock.ANY, self.node.uuid,
                                         True, 'test-topic')

    def test_set_console_mode_disabled(self):
        with mock.patch.object(rpcapi.ConductorAPI,
                               'set_console_mode') as mock_scm:
            ret = self.put_json('/nodes/%s/states/console' % self.node.uuid,
                                {'enabled': "false"})
            self.assertEqual(http_client.ACCEPTED, ret.status_code)
            self.assertEqual(b'', ret.body)
            mock_scm.assert_called_once_with(mock.ANY, self.node.uuid,
                                             False, 'test-topic')
            # Check location header
            self.assertIsNotNone(ret.location)
            expected_location = '/v1/nodes/%s/states/console' % self.node.uuid
            self.assertEqual(urlparse.urlparse(ret.location).path,
                             expected_location)

    def test_set_console_mode_bad_request(self):
        with mock.patch.object(rpcapi.ConductorAPI,
                               'set_console_mode') as mock_scm:
            ret = self.put_json('/nodes/%s/states/console' % self.node.uuid,
                                {'enabled': "invalid-value"},
                                expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
            # assert set_console_mode wasn't called
            assert not mock_scm.called

    def test_set_console_mode_bad_request_missing_parameter(self):
        with mock.patch.object(rpcapi.ConductorAPI,
                               'set_console_mode') as mock_scm:
            ret = self.put_json('/nodes/%s/states/console' % self.node.uuid,
                                {}, expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
            # assert set_console_mode wasn't called
            assert not mock_scm.called

    def test_set_console_mode_console_not_supported(self):
        with mock.patch.object(rpcapi.ConductorAPI,
                               'set_console_mode') as mock_scm:
            mock_scm.side_effect = exception.UnsupportedDriverExtension(
                extension='console', driver='test-driver')
            ret = self.put_json('/nodes/%s/states/console' % self.node.uuid,
                                {'enabled': "true"}, expect_errors=True)
            self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
            mock_scm.assert_called_once_with(mock.ANY, self.node.uuid,
                                             True, 'test-topic')

    def test_provision_node_in_maintenance_fail(self):
        self.node.maintenance = True
        self.node.save()

        ret = self.put_json('/nodes/%s/states/provision' % self.node.uuid,
                            {'target': states.ACTIVE},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])

    @mock.patch.object(rpcapi.ConductorAPI, 'set_target_raid_config',
                       autospec=True)
    def test_put_raid(self, set_raid_config_mock):
        raid_config = {'logical_disks': [{'size_gb': 100, 'raid_level': 1}]}
        ret = self.put_json(
            '/nodes/%s/states/raid' % self.node.uuid, raid_config,
            headers={api_base.Version.string: "1.12"})
        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        self.assertEqual(b'', ret.body)
        set_raid_config_mock.assert_called_once_with(
            mock.ANY, mock.ANY, self.node.uuid, raid_config, topic=mock.ANY)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_target_raid_config',
                       autospec=True)
    def test_put_raid_older_version(self, set_raid_config_mock):
        raid_config = {'logical_disks': [{'size_gb': 100, 'raid_level': 1}]}
        ret = self.put_json(
            '/nodes/%s/states/raid' % self.node.uuid, raid_config,
            headers={api_base.Version.string: "1.5"},
            expect_errors=True)
        self.assertEqual(http_client.NOT_ACCEPTABLE, ret.status_code)
        self.assertFalse(set_raid_config_mock.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_target_raid_config',
                       autospec=True)
    def test_put_raid_iface_not_supported(self, set_raid_config_mock):
        raid_config = {'logical_disks': [{'size_gb': 100, 'raid_level': 1}]}
        set_raid_config_mock.side_effect = (
            exception.UnsupportedDriverExtension(extension='raid',
                                                 driver='fake'))
        ret = self.put_json(
            '/nodes/%s/states/raid' % self.node.uuid, raid_config,
            headers={api_base.Version.string: "1.12"},
            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        set_raid_config_mock.assert_called_once_with(
            mock.ANY, mock.ANY, self.node.uuid, raid_config, topic=mock.ANY)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_target_raid_config',
                       autospec=True)
    def test_put_raid_invalid_parameter_value(self, set_raid_config_mock):
        raid_config = {'logical_disks': [{'size_gb': 100, 'raid_level': 1}]}
        set_raid_config_mock.side_effect = exception.InvalidParameterValue(
            'foo')
        ret = self.put_json(
            '/nodes/%s/states/raid' % self.node.uuid, raid_config,
            headers={api_base.Version.string: "1.12"},
            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        set_raid_config_mock.assert_called_once_with(
            mock.ANY, mock.ANY, self.node.uuid, raid_config, topic=mock.ANY)

    @mock.patch.object(rpcapi.ConductorAPI, 'set_boot_device')
    def test_set_boot_device(self, mock_sbd):
        device = boot_devices.PXE
        ret = self.put_json('/nodes/%s/management/boot_device'
                            % self.node.uuid, {'boot_device': device})
        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_sbd.assert_called_once_with(mock.ANY, self.node.uuid,
                                         device, persistent=False,
                                         topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'set_boot_device')
    def test_set_boot_device_by_name(self, mock_sbd):
        device = boot_devices.PXE
        ret = self.put_json('/nodes/%s/management/boot_device'
                            % self.node.name, {'boot_device': device},
                            headers={api_base.Version.string: "1.5"})
        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_sbd.assert_called_once_with(mock.ANY, self.node.uuid,
                                         device, persistent=False,
                                         topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'set_boot_device')
    def test_set_boot_device_not_supported(self, mock_sbd):
        mock_sbd.side_effect = exception.UnsupportedDriverExtension(
            extension='management', driver='test-driver')
        device = boot_devices.PXE
        ret = self.put_json('/nodes/%s/management/boot_device'
                            % self.node.uuid, {'boot_device': device},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        mock_sbd.assert_called_once_with(mock.ANY, self.node.uuid,
                                         device, persistent=False,
                                         topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'set_boot_device')
    def test_set_boot_device_persistent(self, mock_sbd):
        device = boot_devices.PXE
        ret = self.put_json('/nodes/%s/management/boot_device?persistent=True'
                            % self.node.uuid, {'boot_device': device})
        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_sbd.assert_called_once_with(mock.ANY, self.node.uuid,
                                         device, persistent=True,
                                         topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'set_boot_device')
    def test_set_boot_device_persistent_invalid_value(self, mock_sbd):
        device = boot_devices.PXE
        ret = self.put_json('/nodes/%s/management/boot_device?persistent=blah'
                            % self.node.uuid, {'boot_device': device},
                            expect_errors=True)
        self.assertEqual('application/json', ret.content_type)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)

    @mock.patch.object(rpcapi.ConductorAPI, 'inject_nmi')
    def test_inject_nmi(self, mock_inject_nmi):
        ret = self.put_json('/nodes/%s/management/inject_nmi'
                            % self.node.uuid, {},
                            headers={api_base.Version.string: "1.29"})
        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        self.assertEqual(b'', ret.body)
        mock_inject_nmi.assert_called_once_with(mock.ANY, self.node.uuid,
                                                topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'inject_nmi')
    def test_inject_nmi_not_allowed(self, mock_inject_nmi):
        ret = self.put_json('/nodes/%s/management/inject_nmi'
                            % self.node.uuid, {},
                            headers={api_base.Version.string: "1.28"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        self.assertFalse(mock_inject_nmi.called)

    @mock.patch.object(rpcapi.ConductorAPI, 'inject_nmi')
    def test_inject_nmi_not_supported(self, mock_inject_nmi):
        mock_inject_nmi.side_effect = exception.UnsupportedDriverExtension(
            extension='management', driver='test-driver')
        ret = self.put_json('/nodes/%s/management/inject_nmi'
                            % self.node.uuid, {},
                            headers={api_base.Version.string: "1.29"},
                            expect_errors=True)
        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        mock_inject_nmi.assert_called_once_with(mock.ANY, self.node.uuid,
                                                topic='test-topic')

    def _test_set_node_maintenance_mode(self, mock_update, mock_get, reason,
                                        node_ident, is_by_name=False):
        request_body = {}
        if reason:
            request_body['reason'] = reason

        self.node.maintenance = False
        mock_get.return_value = self.node
        if is_by_name:
            headers = {api_base.Version.string: "1.5"}
        else:
            headers = {}
        ret = self.put_json('/nodes/%s/maintenance' % node_ident,
                            request_body, headers=headers)
        self.assertEqual(http_client.ACCEPTED, ret.status_code)
        self.assertEqual(b'', ret.body)
        self.assertTrue(self.node.maintenance)
        self.assertEqual(reason, self.node.maintenance_reason)
        mock_get.assert_called_once_with(mock.ANY, node_ident)
        mock_update.assert_called_once_with(mock.ANY, mock.ANY,
                                            topic='test-topic')

    @mock.patch.object(notification_utils, '_emit_api_notification')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_set_node_maintenance_mode(self, mock_update, mock_get,
                                       mock_notify):
        self._test_set_node_maintenance_mode(mock_update, mock_get,
                                             'fake_reason', self.node.uuid)
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY,
                                                'maintenance_set',
                                     obj_fields.NotificationLevel.INFO,
                                     obj_fields.NotificationStatus.START),
                                     mock.call(mock.ANY, mock.ANY,
                                               'maintenance_set',
                                     obj_fields.NotificationLevel.INFO,
                                     obj_fields.NotificationStatus.END)])

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_set_node_maintenance_mode_no_reason(self, mock_update, mock_get):
        self._test_set_node_maintenance_mode(mock_update, mock_get, None,
                                             self.node.uuid)

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_set_node_maintenance_mode_by_name(self, mock_update, mock_get):
        self._test_set_node_maintenance_mode(mock_update, mock_get,
                                             'fake_reason', self.node.name,
                                             is_by_name=True)

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_set_node_maintenance_mode_no_reason_by_name(self, mock_update,
                                                         mock_get):
        self._test_set_node_maintenance_mode(mock_update, mock_get, None,
                                             self.node.name, is_by_name=True)

    @mock.patch.object(notification_utils, '_emit_api_notification')
    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'update_node')
    def test_set_node_maintenance_mode_error(self, mock_update, mock_get,
                                             mock_notify):
        mock_get.return_value = self.node
        mock_update.side_effect = Exception()
        self.put_json('/nodes/%s/maintenance' % self.node.uuid,
                      {'reason': 'fake'}, expect_errors=True)
        mock_notify.assert_has_calls([mock.call(mock.ANY, mock.ANY,
                                                'maintenance_set',
                                     obj_fields.NotificationLevel.INFO,
                                     obj_fields.NotificationStatus.START),
                                     mock.call(mock.ANY, mock.ANY,
                                               'maintenance_set',
                                     obj_fields.NotificationLevel.ERROR,
                                     obj_fields.NotificationStatus.ERROR)])


class TestCheckCleanSteps(base.TestCase):
    def test__check_clean_steps_not_list(self):
        clean_steps = {"step": "upgrade_firmware", "interface": "deploy"}
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               "not of type 'array'",
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_not_dict(self):
        clean_steps = ['clean step']
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               "not of type 'object'",
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_key_invalid(self):
        clean_steps = [{"step": "upgrade_firmware", "interface": "deploy",
                        "unknown": "upgrade_firmware"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'unexpected',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_missing_interface(self):
        clean_steps = [{"step": "upgrade_firmware"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'interface',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_missing_step_key(self):
        clean_steps = [{"interface": "deploy"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'step',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_missing_step_value(self):
        clean_steps = [{"step": None, "interface": "deploy"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               "not of type 'string'",
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_min_length_step_value(self):
        clean_steps = [{"step": "", "interface": "deploy"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'is too short',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_interface_value_invalid(self):
        clean_steps = [{"step": "upgrade_firmware", "interface": "not"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'is not one of',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_step_args_value_invalid(self):
        clean_steps = [{"step": "upgrade_firmware", "interface": "deploy",
                        "args": "invalid args"}]
        self.assertRaisesRegex(exception.InvalidParameterValue,
                               'args',
                               api_node._check_clean_steps, clean_steps)

    def test__check_clean_steps_valid(self):
        clean_steps = [{"step": "upgrade_firmware", "interface": "deploy"}]
        api_node._check_clean_steps(clean_steps)

        step1 = {"step": "upgrade_firmware", "interface": "deploy",
                 "args": {"arg1": "value1", "arg2": "value2"}}
        api_node._check_clean_steps([step1])

        step2 = {"step": "configure raid", "interface": "raid"}
        api_node._check_clean_steps([step1, step2])


class TestAttachDetachVif(test_api_base.BaseApiTest):

    def setUp(self):
        super(TestAttachDetachVif, self).setUp()
        self.vif_version = "1.28"
        self.node = obj_utils.create_test_node(
            self.context,
            provision_state=states.AVAILABLE, name='node-39')
        p = mock.patch.object(rpcapi.ConductorAPI, 'get_topic_for')
        self.mock_gtf = p.start()
        self.mock_gtf.return_value = 'test-topic'
        self.addCleanup(p.stop)

    @mock.patch.object(objects.Node, 'get_by_uuid')
    def test_vif_subcontroller_old_version(self, mock_get):
        mock_get.return_value = self.node
        ret = self.get_json('/nodes/%s/vifs' % self.node.uuid,
                            headers={api_base.Version.string: "1.26"},
                            expect_errors=True)
        self.assertEqual(http_client.NOT_FOUND, ret.status_code)

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_list')
    def test_vif_list(self, mock_list, mock_get):
        mock_get.return_value = self.node
        self.get_json('/nodes/%s/vifs' % self.node.uuid,
                      headers={api_base.Version.string:
                               self.vif_version})

        mock_get.assert_called_once_with(mock.ANY, self.node.uuid)
        mock_list.assert_called_once_with(mock.ANY, self.node.uuid,
                                          topic='test-topic')

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach(self, mock_attach, mock_get):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'id': vif_id
        }

        mock_get.return_value = self.node

        ret = self.post_json('/nodes/%s/vifs' % self.node.uuid,
                             request_body,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        mock_get.assert_called_once_with(mock.ANY, self.node.uuid)
        mock_attach.assert_called_once_with(mock.ANY, self.node.uuid,
                                            vif_info=request_body,
                                            topic='test-topic')

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_by_node_name(self, mock_attach, mock_get):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'id': vif_id
        }

        mock_get.return_value = self.node

        ret = self.post_json('/nodes/%s/vifs' % self.node.name,
                             request_body,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        mock_get.assert_called_once_with(mock.ANY, self.node.name)
        mock_attach.assert_called_once_with(mock.ANY, self.node.uuid,
                                            vif_info=request_body,
                                            topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_node_not_found(self, mock_attach):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'id': vif_id
        }

        ret = self.post_json('/nodes/doesntexist/vifs',
                             request_body, expect_errors=True,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.NOT_FOUND, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        self.assertFalse(mock_attach.called)

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_conductor_unavailable(self, mock_attach, mock_get):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'id': vif_id
        }
        mock_get.return_value = self.node
        self.mock_gtf.side_effect = exception.NoValidHost('boom')
        ret = self.post_json('/nodes/%s/vifs' % self.node.name,
                             request_body, expect_errors=True,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        self.assertFalse(mock_attach.called)

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_no_vif_id(self, mock_attach, mock_get):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'bad_id': vif_id
        }

        mock_get.return_value = self.node

        ret = self.post_json('/nodes/%s/vifs' % self.node.uuid,
                             request_body, expect_errors=True,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_invalid_vif_id(self, mock_attach, mock_get):
        request_body = {
            'id': "invalid%id^"
        }

        mock_get.return_value = self.node

        ret = self.post_json('/nodes/%s/vifs' % self.node.uuid,
                             request_body, expect_errors=True,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.BAD_REQUEST, ret.status_code)
        self.assertTrue(ret.json['error_message'])

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_attach')
    def test_vif_attach_node_locked(self, mock_attach, mock_get):
        vif_id = uuidutils.generate_uuid()
        request_body = {
            'id': vif_id
        }

        mock_get.return_value = self.node
        mock_attach.side_effect = exception.NodeLocked(node='', host='')

        ret = self.post_json('/nodes/%s/vifs' % self.node.uuid,
                             request_body, expect_errors=True,
                             headers={api_base.Version.string:
                                      self.vif_version})

        self.assertEqual(http_client.CONFLICT, ret.status_code)
        self.assertTrue(ret.json['error_message'])

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_detach')
    def test_vif_detach(self, mock_detach, mock_get):
        vif_id = uuidutils.generate_uuid()

        mock_get.return_value = self.node

        ret = self.delete('/nodes/%s/vifs/%s' % (self.node.uuid, vif_id),
                          headers={api_base.Version.string:
                                   self.vif_version})

        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        mock_get.assert_called_once_with(mock.ANY, self.node.uuid)
        mock_detach.assert_called_once_with(mock.ANY, self.node.uuid,
                                            vif_id=vif_id,
                                            topic='test-topic')

    @mock.patch.object(objects.Node, 'get_by_name')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_detach')
    def test_vif_detach_by_node_name(self, mock_detach, mock_get):
        vif_id = uuidutils.generate_uuid()

        mock_get.return_value = self.node

        ret = self.delete('/nodes/%s/vifs/%s' % (self.node.name, vif_id),
                          headers={api_base.Version.string: self.vif_version})

        self.assertEqual(http_client.NO_CONTENT, ret.status_code)
        mock_get.assert_called_once_with(mock.ANY, self.node.name)
        mock_detach.assert_called_once_with(mock.ANY, self.node.uuid,
                                            vif_id=vif_id,
                                            topic='test-topic')

    @mock.patch.object(rpcapi.ConductorAPI, 'vif_detach')
    def test_vif_detach_node_not_found(self, mock_detach):
        vif_id = uuidutils.generate_uuid()

        ret = self.delete('/nodes/doesntexist/vifs/%s' % vif_id,
                          headers={api_base.Version.string: self.vif_version},
                          expect_errors=True)

        self.assertEqual(http_client.NOT_FOUND, ret.status_code)
        self.assertTrue(ret.json['error_message'])
        self.assertFalse(mock_detach.called)

    @mock.patch.object(objects.Node, 'get_by_uuid')
    @mock.patch.object(rpcapi.ConductorAPI, 'vif_detach')
    def test_vif_detach_node_locked(self, mock_detach, mock_get):
        vif_id = uuidutils.generate_uuid()

        mock_get.return_value = self.node
        mock_detach.side_effect = exception.NodeLocked(node='', host='')

        ret = self.delete('/nodes/%s/vifs/%s' % (self.node.uuid, vif_id),
                          headers={api_base.Version.string: self.vif_version},
                          expect_errors=True)

        self.assertEqual(http_client.CONFLICT, ret.status_code)
        self.assertTrue(ret.json['error_message'])
