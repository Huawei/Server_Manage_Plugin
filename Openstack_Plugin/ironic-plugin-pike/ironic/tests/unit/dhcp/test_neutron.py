#
# Copyright 2014 OpenStack Foundation
# All Rights Reserved
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

from neutronclient.common import exceptions as neutron_client_exc
from oslo_utils import uuidutils

from ironic.common import dhcp_factory
from ironic.common import exception
from ironic.common import pxe_utils
from ironic.conductor import task_manager
from ironic.dhcp import neutron
from ironic.tests.unit.conductor import mgr_utils
from ironic.tests.unit.db import base as db_base
from ironic.tests.unit.objects import utils as object_utils


class TestNeutron(db_base.DbTestCase):

    def setUp(self):
        super(TestNeutron, self).setUp()
        mgr_utils.mock_the_extension_manager(driver='fake')
        self.config(
            cleaning_network='00000000-0000-0000-0000-000000000000',
            group='neutron')
        self.config(enabled_drivers=['fake'])
        self.config(dhcp_provider='neutron',
                    group='dhcp')
        self.node = object_utils.create_test_node(self.context)
        self.ports = [
            object_utils.create_test_port(
                self.context, node_id=self.node.id, id=2,
                uuid='1be26c0b-03f2-4d2e-ae87-c02d7f33c782',
                address='52:54:00:cf:2d:32')]
        # Very simple neutron port representation
        self.neutron_port = {'id': '132f871f-eaec-4fed-9475-0d54465e0f00',
                             'mac_address': '52:54:00:cf:2d:32'}

        dhcp_factory.DHCPFactory._dhcp_provider = None

    @mock.patch('ironic.common.neutron.get_client', autospec=True)
    def test_update_port_dhcp_opts(self, client_mock):
        opts = [{'opt_name': 'bootfile-name',
                 'opt_value': 'pxelinux.0'},
                {'opt_name': 'tftp-server',
                 'opt_value': '1.1.1.1'},
                {'opt_name': 'server-ip-address',
                 'opt_value': '1.1.1.1'}]
        port_id = 'fake-port-id'
        expected = {'port': {'extra_dhcp_opts': opts}}

        api = dhcp_factory.DHCPFactory()
        api.provider.update_port_dhcp_opts(port_id, opts)
        client_mock.return_value.update_port.assert_called_once_with(
            port_id, expected)

    @mock.patch('ironic.common.neutron.get_client', autospec=True)
    def test_update_port_dhcp_opts_with_exception(self, client_mock):
        opts = [{}]
        port_id = 'fake-port-id'
        client_mock.return_value.update_port.side_effect = (
            neutron_client_exc.NeutronClientException())

        api = dhcp_factory.DHCPFactory()
        self.assertRaises(
            exception.FailedToUpdateDHCPOptOnPort,
            api.provider.update_port_dhcp_opts,
            port_id, opts)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi.update_port_dhcp_opts')
    @mock.patch('ironic.common.network.get_node_vif_ids')
    def test_update_dhcp(self, mock_gnvi, mock_updo):
        mock_gnvi.return_value = {'ports': {'port-uuid': 'vif-uuid'},
                                  'portgroups': {}}
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            opts = pxe_utils.dhcp_options_for_instance(task)
            api = dhcp_factory.DHCPFactory()
            api.update_dhcp(task, opts)
        mock_updo.assert_called_once_with('vif-uuid', opts)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi.update_port_dhcp_opts')
    @mock.patch('ironic.common.network.get_node_vif_ids')
    def test_update_dhcp_no_vif_data(self, mock_gnvi, mock_updo):
        mock_gnvi.return_value = {'portgroups': {}, 'ports': {}}
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory()
            self.assertRaises(exception.FailedToUpdateDHCPOptOnPort,
                              api.update_dhcp, task, self.node)
        self.assertFalse(mock_updo.called)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi.update_port_dhcp_opts')
    @mock.patch('ironic.common.network.get_node_vif_ids')
    def test_update_dhcp_some_failures(self, mock_gnvi, mock_updo):
        # confirm update is called twice, one fails, but no exception raised
        mock_gnvi.return_value = {'ports': {'p1': 'v1', 'p2': 'v2'},
                                  'portgroups': {}}
        exc = exception.FailedToUpdateDHCPOptOnPort('fake exception')
        mock_updo.side_effect = [None, exc]
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory()
            api.update_dhcp(task, self.node)
            mock_gnvi.assert_called_once_with(task)
        self.assertEqual(2, mock_updo.call_count)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi.update_port_dhcp_opts')
    @mock.patch('ironic.common.network.get_node_vif_ids')
    def test_update_dhcp_fails(self, mock_gnvi, mock_updo):
        # confirm update is called twice, both fail, and exception is raised
        mock_gnvi.return_value = {'ports': {'p1': 'v1', 'p2': 'v2'},
                                  'portgroups': {}}
        exc = exception.FailedToUpdateDHCPOptOnPort('fake exception')
        mock_updo.side_effect = [exc, exc]
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory()
            self.assertRaises(exception.FailedToUpdateDHCPOptOnPort,
                              api.update_dhcp,
                              task, self.node)
            mock_gnvi.assert_called_once_with(task)
        self.assertEqual(2, mock_updo.call_count)

    @mock.patch.object(neutron, 'LOG', autospec=True)
    @mock.patch('time.sleep', autospec=True)
    @mock.patch.object(neutron.NeutronDHCPApi, 'update_port_dhcp_opts',
                       autospec=True)
    @mock.patch('ironic.common.network.get_node_vif_ids', autospec=True)
    def test_update_dhcp_set_sleep_and_fake(self, mock_gnvi, mock_updo,
                                            mock_ts, mock_log):
        mock_gnvi.return_value = {'ports': {'port-uuid': 'vif-uuid'},
                                  'portgroups': {}}
        self.config(port_setup_delay=30, group='neutron')
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            opts = pxe_utils.dhcp_options_for_instance(task)
            api = dhcp_factory.DHCPFactory()
            api.update_dhcp(task, opts)
            mock_log.debug.assert_called_once_with(
                "Waiting %d seconds for Neutron.", 30)
            mock_ts.assert_called_with(30)
        mock_updo.assert_called_once_with(mock.ANY, 'vif-uuid', opts)

    @mock.patch.object(neutron, 'LOG', autospec=True)
    @mock.patch.object(neutron.NeutronDHCPApi, 'update_port_dhcp_opts',
                       autospec=True)
    @mock.patch('ironic.common.network.get_node_vif_ids', autospec=True)
    def test_update_dhcp_unset_sleep_and_fake(self, mock_gnvi, mock_updo,
                                              mock_log):
        mock_gnvi.return_value = {'ports': {'port-uuid': 'vif-uuid'},
                                  'portgroups': {}}
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            opts = pxe_utils.dhcp_options_for_instance(task)
            api = dhcp_factory.DHCPFactory()
            api.update_dhcp(task, opts)
            mock_log.debug.assert_not_called()
        mock_updo.assert_called_once_with(mock.ANY, 'vif-uuid', opts)

    def test__get_fixed_ip_address(self):
        port_id = 'fake-port-id'
        expected = "192.168.1.3"
        api = dhcp_factory.DHCPFactory().provider
        port_data = {
            "id": port_id,
            "network_id": "3cb9bc59-5699-4588-a4b1-b87f96708bc6",
            "admin_state_up": True,
            "status": "ACTIVE",
            "mac_address": "fa:16:3e:4c:2c:30",
            "fixed_ips": [
                {
                    "ip_address": "192.168.1.3",
                    "subnet_id": "f8a6e8f8-c2ec-497c-9f23-da9616de54ef"
                }
            ],
            "device_id": 'bece68a3-2f8b-4e66-9092-244493d6aba7',
        }
        fake_client = mock.Mock()
        fake_client.show_port.return_value = {'port': port_data}
        result = api._get_fixed_ip_address(port_id, fake_client)
        self.assertEqual(expected, result)
        fake_client.show_port.assert_called_once_with(port_id)

    def test__get_fixed_ip_address_invalid_ip(self):
        port_id = 'fake-port-id'
        api = dhcp_factory.DHCPFactory().provider
        port_data = {
            "id": port_id,
            "network_id": "3cb9bc59-5699-4588-a4b1-b87f96708bc6",
            "admin_state_up": True,
            "status": "ACTIVE",
            "mac_address": "fa:16:3e:4c:2c:30",
            "fixed_ips": [
                {
                    "ip_address": "invalid.ip",
                    "subnet_id": "f8a6e8f8-c2ec-497c-9f23-da9616de54ef"
                }
            ],
            "device_id": 'bece68a3-2f8b-4e66-9092-244493d6aba7',
        }
        fake_client = mock.Mock()
        fake_client.show_port.return_value = {'port': port_data}
        self.assertRaises(exception.InvalidIPv4Address,
                          api._get_fixed_ip_address,
                          port_id, fake_client)
        fake_client.show_port.assert_called_once_with(port_id)

    def test__get_fixed_ip_address_with_exception(self):
        port_id = 'fake-port-id'
        api = dhcp_factory.DHCPFactory().provider

        fake_client = mock.Mock()
        fake_client.show_port.side_effect = (
            neutron_client_exc.NeutronClientException())
        self.assertRaises(exception.FailedToGetIPAddressOnPort,
                          api._get_fixed_ip_address, port_id, fake_client)
        fake_client.show_port.assert_called_once_with(port_id)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def _test__get_port_ip_address(self, mock_gfia, network):
        expected = "192.168.1.3"
        fake_vif = 'test-vif-%s' % network
        port = object_utils.create_test_port(
            self.context, node_id=self.node.id, address='aa:bb:cc:dd:ee:ff',
            uuid=uuidutils.generate_uuid(),
            internal_info={
                'cleaning_vif_port_id': (fake_vif if network == 'cleaning'
                                         else None),
                'provisioning_vif_port_id': (fake_vif
                                             if network == 'provisioning'
                                             else None),
                'tenant_vif_port_id': (fake_vif if network == 'tenant'
                                       else None),
            }
        )
        mock_gfia.return_value = expected
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            result = api._get_port_ip_address(task, port,
                                              mock.sentinel.client)
        self.assertEqual(expected, result)
        mock_gfia.assert_called_once_with(fake_vif, mock.sentinel.client)

    def test__get_port_ip_address_tenant(self):
        self._test__get_port_ip_address(network='tenant')

    def test__get_port_ip_address_cleaning(self):
        self._test__get_port_ip_address(network='cleaning')

    def test__get_port_ip_address_provisioning(self):
        self._test__get_port_ip_address(network='provisioning')

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def test__get_port_ip_address_for_portgroup(self, mock_gfia):
        expected = "192.168.1.3"
        pg = object_utils.create_test_portgroup(
            self.context, node_id=self.node.id, address='aa:bb:cc:dd:ee:ff',
            uuid=uuidutils.generate_uuid(),
            internal_info={'tenant_vif_port_id': 'test-vif-A'})
        mock_gfia.return_value = expected
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            result = api._get_port_ip_address(task, pg,
                                              mock.sentinel.client)
        self.assertEqual(expected, result)
        mock_gfia.assert_called_once_with('test-vif-A', mock.sentinel.client)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def test__get_port_ip_address_with_exception(self, mock_gfia):
        expected = "192.168.1.3"
        port = object_utils.create_test_port(self.context,
                                             node_id=self.node.id,
                                             address='aa:bb:cc:dd:ee:ff',
                                             uuid=uuidutils.generate_uuid())
        mock_gfia.return_value = expected
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            self.assertRaises(exception.FailedToGetIPAddressOnPort,
                              api._get_port_ip_address, task, port,
                              mock.sentinel.client)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def test__get_port_ip_address_for_portgroup_with_exception(
            self, mock_gfia):
        expected = "192.168.1.3"
        pg = object_utils.create_test_portgroup(self.context,
                                                node_id=self.node.id,
                                                address='aa:bb:cc:dd:ee:ff',
                                                uuid=uuidutils.generate_uuid())
        mock_gfia.return_value = expected
        with task_manager.acquire(self.context,
                                  self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            self.assertRaises(exception.FailedToGetIPAddressOnPort,
                              api._get_port_ip_address, task, pg,
                              mock.sentinel.client)

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def _test__get_ip_addresses_ports(self, key, mock_gfia):
        if key == "extra":
            kwargs1 = {key: {'vif_port_id': 'test-vif-A'}}
        else:
            kwargs1 = {key: {'tenant_vif_port_id': 'test-vif-A'}}
        ip_address = '10.10.0.1'
        expected = [ip_address]
        port = object_utils.create_test_port(self.context,
                                             node_id=self.node.id,
                                             address='aa:bb:cc:dd:ee:ff',
                                             uuid=uuidutils.generate_uuid(),
                                             **kwargs1)
        mock_gfia.return_value = ip_address
        with task_manager.acquire(self.context, self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            result = api._get_ip_addresses(task, [port],
                                           mock.sentinel.client)
        self.assertEqual(expected, result)

    def test__get_ip_addresses_ports_extra(self):
        self._test__get_ip_addresses_ports('extra')

    def test__get_ip_addresses_ports_int_info(self):
        self._test__get_ip_addresses_ports('internal_info')

    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_fixed_ip_address')
    def _test__get_ip_addresses_portgroup(self, key, mock_gfia):
        if key == "extra":
            kwargs1 = {key: {'vif_port_id': 'test-vif-A'}}
        else:
            kwargs1 = {key: {'tenant_vif_port_id': 'test-vif-A'}}
        ip_address = '10.10.0.1'
        expected = [ip_address]
        pg = object_utils.create_test_portgroup(
            self.context, node_id=self.node.id,
            address='aa:bb:cc:dd:ee:ff', uuid=uuidutils.generate_uuid(),
            **kwargs1)
        mock_gfia.return_value = ip_address
        with task_manager.acquire(self.context, self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            result = api._get_ip_addresses(task, [pg], mock.sentinel.client)
        self.assertEqual(expected, result)

    def test__get_ip_addresses_portgroup_extra(self):
        self._test__get_ip_addresses_portgroup('extra')

    def test__get_ip_addresses_portgroup_int_info(self):
        self._test__get_ip_addresses_portgroup('internal_info')

    @mock.patch('ironic.common.neutron.get_client', autospec=True)
    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_port_ip_address')
    def test_get_ip_addresses(self, get_ip_mock, client_mock):
        ip_address = '10.10.0.1'
        expected = [ip_address]

        get_ip_mock.return_value = ip_address

        with task_manager.acquire(self.context, self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            result = api.get_ip_addresses(task)
            get_ip_mock.assert_called_once_with(task, task.ports[0],
                                                client_mock.return_value)
        self.assertEqual(expected, result)

    @mock.patch('ironic.common.neutron.get_client', autospec=True)
    @mock.patch('ironic.dhcp.neutron.NeutronDHCPApi._get_port_ip_address')
    def test_get_ip_addresses_for_port_and_portgroup(self, get_ip_mock,
                                                     client_mock):
        object_utils.create_test_portgroup(
            self.context, node_id=self.node.id, address='aa:bb:cc:dd:ee:ff',
            uuid=uuidutils.generate_uuid(),
            internal_info={'tenant_vif_port_id': 'test-vif-A'})

        with task_manager.acquire(self.context, self.node.uuid) as task:
            api = dhcp_factory.DHCPFactory().provider
            api.get_ip_addresses(task)
            get_ip_mock.assert_has_calls(
                [mock.call(task, task.ports[0], client_mock.return_value),
                 mock.call(task, task.portgroups[0], client_mock.return_value)]
            )
