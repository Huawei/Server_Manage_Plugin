# coding=utf-8
#
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

import datetime

import mock
from oslo_config import cfg
from testtools import matchers

from ironic.common import exception
from ironic import objects
from ironic.objects import base as obj_base
from ironic.tests.unit.db import base as db_base
from ironic.tests.unit.db import utils as db_utils
from ironic.tests.unit.objects import utils as obj_utils

CONF = cfg.CONF


class TestPortObject(db_base.DbTestCase, obj_utils.SchemasTestMixIn):

    def setUp(self):
        super(TestPortObject, self).setUp()
        self.fake_port = db_utils.get_test_port()

    def test_get_by_id(self):
        port_id = self.fake_port['id']
        with mock.patch.object(self.dbapi, 'get_port_by_id',
                               autospec=True) as mock_get_port:
            mock_get_port.return_value = self.fake_port

            port = objects.Port.get(self.context, port_id)

            mock_get_port.assert_called_once_with(port_id)
            self.assertEqual(self.context, port._context)

    def test_get_by_uuid(self):
        uuid = self.fake_port['uuid']
        with mock.patch.object(self.dbapi, 'get_port_by_uuid',
                               autospec=True) as mock_get_port:
            mock_get_port.return_value = self.fake_port

            port = objects.Port.get(self.context, uuid)

            mock_get_port.assert_called_once_with(uuid)
            self.assertEqual(self.context, port._context)

    def test_get_by_address(self):
        address = self.fake_port['address']
        with mock.patch.object(self.dbapi, 'get_port_by_address',
                               autospec=True) as mock_get_port:
            mock_get_port.return_value = self.fake_port

            port = objects.Port.get(self.context, address)

            mock_get_port.assert_called_once_with(address)
            self.assertEqual(self.context, port._context)

    def test_get_bad_id_and_uuid_and_address(self):
        self.assertRaises(exception.InvalidIdentity,
                          objects.Port.get, self.context, 'not-a-uuid')

    def test_create(self):
        port = objects.Port(self.context, **self.fake_port)
        with mock.patch.object(self.dbapi, 'create_port',
                               autospec=True) as mock_create_port:
            mock_create_port.return_value = db_utils.get_test_port()

            port.create()

            args, _kwargs = mock_create_port.call_args
            self.assertEqual(objects.Port.VERSION, args[0]['version'])

    def test_save(self):
        uuid = self.fake_port['uuid']
        address = "b2:54:00:cf:2d:40"
        test_time = datetime.datetime(2000, 1, 1, 0, 0)
        with mock.patch.object(self.dbapi, 'get_port_by_uuid',
                               autospec=True) as mock_get_port:
            mock_get_port.return_value = self.fake_port
            with mock.patch.object(self.dbapi, 'update_port',
                                   autospec=True) as mock_update_port:
                mock_update_port.return_value = (
                    db_utils.get_test_port(address=address,
                                           updated_at=test_time))
                p = objects.Port.get_by_uuid(self.context, uuid)
                p.address = address
                p.save()

                mock_get_port.assert_called_once_with(uuid)
                mock_update_port.assert_called_once_with(
                    uuid, {'version': objects.Port.VERSION,
                           'address': "b2:54:00:cf:2d:40"})
                self.assertEqual(self.context, p._context)
                res_updated_at = (p.updated_at).replace(tzinfo=None)
                self.assertEqual(test_time, res_updated_at)

    def test_refresh(self):
        uuid = self.fake_port['uuid']
        returns = [self.fake_port,
                   db_utils.get_test_port(address="c3:54:00:cf:2d:40")]
        expected = [mock.call(uuid), mock.call(uuid)]
        with mock.patch.object(self.dbapi, 'get_port_by_uuid',
                               side_effect=returns,
                               autospec=True) as mock_get_port:
            p = objects.Port.get_by_uuid(self.context, uuid)
            self.assertEqual("52:54:00:cf:2d:31", p.address)
            p.refresh()
            self.assertEqual("c3:54:00:cf:2d:40", p.address)

            self.assertEqual(expected, mock_get_port.call_args_list)
            self.assertEqual(self.context, p._context)

    def test_save_after_refresh(self):
        # Ensure that it's possible to do object.save() after object.refresh()
        address = "b2:54:00:cf:2d:40"
        db_node = db_utils.create_test_node()
        db_port = db_utils.create_test_port(node_id=db_node.id)
        p = objects.Port.get_by_uuid(self.context, db_port.uuid)
        p_copy = objects.Port.get_by_uuid(self.context, db_port.uuid)
        p.address = address
        p.save()
        p_copy.refresh()
        p_copy.address = 'aa:bb:cc:dd:ee:ff'
        # Ensure this passes and an exception is not generated
        p_copy.save()

    def test_list(self):
        with mock.patch.object(self.dbapi, 'get_port_list',
                               autospec=True) as mock_get_list:
            mock_get_list.return_value = [self.fake_port]
            ports = objects.Port.list(self.context)
            self.assertThat(ports, matchers.HasLength(1))
            self.assertIsInstance(ports[0], objects.Port)
            self.assertEqual(self.context, ports[0]._context)

    @mock.patch.object(obj_base.IronicObject, 'supports_version')
    def test_supports_physical_network_supported(self, mock_sv):
        mock_sv.return_value = True
        self.assertTrue(objects.Port.supports_physical_network())
        mock_sv.assert_called_once_with((1, 7))

    @mock.patch.object(obj_base.IronicObject, 'supports_version')
    def test_supports_physical_network_unsupported(self, mock_sv):
        mock_sv.return_value = False
        self.assertFalse(objects.Port.supports_physical_network())
        mock_sv.assert_called_once_with((1, 7))

    def test_payload_schemas(self):
        self._check_payload_schemas(objects.port, objects.Port.fields)


class TestConvertToVersion(db_base.DbTestCase):

    def setUp(self):
        super(TestConvertToVersion, self).setUp()
        self.fake_port = db_utils.get_test_port()

    def test_physnet_supported_missing(self):
        # Physical network not set, should be set to default.
        port = objects.Port(self.context, **self.fake_port)
        delattr(port, 'physical_network')
        port.obj_reset_changes()
        port._convert_to_version("1.7")
        self.assertIsNone(port.physical_network)
        self.assertEqual({'physical_network': None}, port.obj_get_changes())

    def test_physnet_supported_set(self):
        # Physical network set, no change required.
        port = objects.Port(self.context, **self.fake_port)
        port.physical_network = 'physnet1'
        port.obj_reset_changes()
        port._convert_to_version("1.7")
        self.assertEqual('physnet1', port.physical_network)
        self.assertEqual({}, port.obj_get_changes())

    def test_physnet_unsupported_missing(self):
        # Physical network not set, no change required.
        port = objects.Port(self.context, **self.fake_port)
        delattr(port, 'physical_network')
        port.obj_reset_changes()
        port._convert_to_version("1.6")
        self.assertNotIn('physical_network', port)
        self.assertEqual({}, port.obj_get_changes())

    def test_physnet_unsupported_set_remove(self):
        # Physical network set, should be removed.
        port = objects.Port(self.context, **self.fake_port)
        port.physical_network = 'physnet1'
        port.obj_reset_changes()
        port._convert_to_version("1.6")
        self.assertNotIn('physical_network', port)
        self.assertEqual({}, port.obj_get_changes())

    def test_physnet_unsupported_set_no_remove_non_default(self):
        # Physical network set, should be set to default.
        port = objects.Port(self.context, **self.fake_port)
        port.physical_network = 'physnet1'
        port.obj_reset_changes()
        port._convert_to_version("1.6", False)
        self.assertIsNone(port.physical_network)
        self.assertEqual({'physical_network': None}, port.obj_get_changes())

    def test_physnet_unsupported_set_no_remove_default(self):
        # Physical network set, no change required.
        port = objects.Port(self.context, **self.fake_port)
        port.physical_network = None
        port.obj_reset_changes()
        port._convert_to_version("1.6", False)
        self.assertIsNone(port.physical_network)
        self.assertEqual({}, port.obj_get_changes())
