# Copyright 2013 Hewlett-Packard Development Company, L.P.
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

"""Tests for manipulating Conductors via the DB API"""

import datetime

import mock
import oslo_db
from oslo_db import exception as db_exc
from oslo_db import sqlalchemy
from oslo_utils import timeutils

from ironic.common import exception
from ironic.tests.unit.db import base
from ironic.tests.unit.db import utils


class DbConductorTestCase(base.DbTestCase):

    def test_register_conductor_existing_fails(self):
        c = utils.get_test_conductor()
        self.dbapi.register_conductor(c)
        self.assertRaises(
            exception.ConductorAlreadyRegistered,
            self.dbapi.register_conductor,
            c)

    def test_register_conductor_override(self):
        c = utils.get_test_conductor()
        self.dbapi.register_conductor(c)
        self.dbapi.register_conductor(c, update_existing=True)

    def _create_test_cdr(self, hardware_types=None, **kwargs):
        hardware_types = hardware_types or []
        c = utils.get_test_conductor(**kwargs)
        cdr = self.dbapi.register_conductor(c)
        for ht in hardware_types:
            self.dbapi.register_conductor_hardware_interfaces(cdr.id, ht,
                                                              'power',
                                                              ['ipmi', 'fake'],
                                                              'ipmi')
        return cdr

    def test_register_conductor_hardware_interfaces(self):
        c = self._create_test_cdr()
        interfaces = ['direct', 'iscsi']
        self.dbapi.register_conductor_hardware_interfaces(c.id, 'generic',
                                                          'deploy', interfaces,
                                                          'iscsi')
        ifaces = self.dbapi.list_conductor_hardware_interfaces(c.id)
        ci1, ci2 = ifaces
        self.assertEqual(2, len(ifaces))
        self.assertEqual('generic', ci1.hardware_type)
        self.assertEqual('generic', ci2.hardware_type)
        self.assertEqual('deploy', ci1.interface_type)
        self.assertEqual('deploy', ci2.interface_type)
        self.assertEqual('direct', ci1.interface_name)
        self.assertEqual('iscsi', ci2.interface_name)
        self.assertFalse(ci1.default)
        self.assertTrue(ci2.default)

    def test_register_conductor_hardware_interfaces_duplicate(self):
        c = self._create_test_cdr()
        interfaces = ['direct', 'iscsi']
        self.dbapi.register_conductor_hardware_interfaces(c.id, 'generic',
                                                          'deploy', interfaces,
                                                          'iscsi')
        ifaces = self.dbapi.list_conductor_hardware_interfaces(c.id)
        ci1, ci2 = ifaces
        self.assertEqual(2, len(ifaces))

        # do it again for the duplicates
        self.assertRaises(
            exception.ConductorHardwareInterfacesAlreadyRegistered,
            self.dbapi.register_conductor_hardware_interfaces,
            c.id, 'generic', 'deploy', interfaces, 'iscsi')

    def test_unregister_conductor_hardware_interfaces(self):
        c = self._create_test_cdr()
        interfaces = ['direct', 'iscsi']
        self.dbapi.register_conductor_hardware_interfaces(c.id, 'generic',
                                                          'deploy', interfaces,
                                                          'iscsi')
        self.dbapi.unregister_conductor_hardware_interfaces(c.id)

        ifaces = self.dbapi.list_conductor_hardware_interfaces(c.id)
        self.assertEqual([], ifaces)

    def test_get_conductor(self):
        c1 = self._create_test_cdr()
        c2 = self.dbapi.get_conductor(c1.hostname)
        self.assertEqual(c1.id, c2.id)

    def test_get_conductor_not_found(self):
        self._create_test_cdr()
        self.assertRaises(
            exception.ConductorNotFound,
            self.dbapi.get_conductor,
            'bad-hostname')

    def test_unregister_conductor(self):
        c = self._create_test_cdr()
        self.dbapi.unregister_conductor(c.hostname)
        self.assertRaises(
            exception.ConductorNotFound,
            self.dbapi.unregister_conductor,
            c.hostname)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_touch_conductor(self, mock_utcnow):
        test_time = datetime.datetime(2000, 1, 1, 0, 0)
        mock_utcnow.return_value = test_time
        c = self._create_test_cdr()
        self.assertEqual(test_time, timeutils.normalize_time(c.updated_at))

        test_time = datetime.datetime(2000, 1, 1, 0, 1)
        mock_utcnow.return_value = test_time
        self.dbapi.touch_conductor(c.hostname)
        c = self.dbapi.get_conductor(c.hostname)
        self.assertEqual(test_time, timeutils.normalize_time(c.updated_at))

    @mock.patch.object(oslo_db.api.time, 'sleep', autospec=True)
    @mock.patch.object(sqlalchemy.orm.Query, 'update', autospec=True)
    def test_touch_conductor_deadlock(self, mock_update, mock_sleep):
        mock_sleep.return_value = None
        mock_update.side_effect = [db_exc.DBDeadlock(), None]
        c = self._create_test_cdr()
        self.dbapi.touch_conductor(c.hostname)
        self.assertEqual(2, mock_update.call_count)
        self.assertEqual(2, mock_sleep.call_count)

    def test_touch_conductor_not_found(self):
        # A conductor's heartbeat will not create a new record,
        # it will only update existing ones
        self._create_test_cdr()
        self.assertRaises(
            exception.ConductorNotFound,
            self.dbapi.touch_conductor,
            'bad-hostname')

    def test_touch_offline_conductor(self):
        # Ensure that a conductor's periodic heartbeat task can make the
        # conductor visible again, even if it was spuriously marked offline
        c = self._create_test_cdr()
        self.dbapi.unregister_conductor(c.hostname)
        self.assertRaises(
            exception.ConductorNotFound,
            self.dbapi.get_conductor,
            c.hostname)
        self.dbapi.touch_conductor(c.hostname)
        self.dbapi.get_conductor(c.hostname)

    def test_clear_node_reservations_for_conductor(self):
        node1 = self.dbapi.create_node({'reservation': 'hostname1'})
        node2 = self.dbapi.create_node({'reservation': 'hostname2'})
        node3 = self.dbapi.create_node({'reservation': None})
        self.dbapi.clear_node_reservations_for_conductor('hostname1')
        node1 = self.dbapi.get_node_by_id(node1.id)
        node2 = self.dbapi.get_node_by_id(node2.id)
        node3 = self.dbapi.get_node_by_id(node3.id)
        self.assertIsNone(node1.reservation)
        self.assertEqual('hostname2', node2.reservation)
        self.assertIsNone(node3.reservation)

    def test_clear_node_target_power_state(self):
        node1 = self.dbapi.create_node({'reservation': 'hostname1',
                                        'target_power_state': 'power on'})
        node2 = self.dbapi.create_node({'reservation': 'hostname2',
                                        'target_power_state': 'power on'})
        node3 = self.dbapi.create_node({'reservation': None,
                                        'target_power_state': 'power on'})
        self.dbapi.clear_node_target_power_state('hostname1')
        node1 = self.dbapi.get_node_by_id(node1.id)
        node2 = self.dbapi.get_node_by_id(node2.id)
        node3 = self.dbapi.get_node_by_id(node3.id)
        self.assertIsNone(node1.target_power_state)
        self.assertIn('power operation was aborted', node1.last_error)
        self.assertEqual('power on', node2.target_power_state)
        self.assertIsNone(node2.last_error)
        self.assertEqual('power on', node3.target_power_state)
        self.assertIsNone(node3.last_error)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_one_host_no_driver(self, mock_utcnow):
        h = 'fake-host'
        expected = {}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[])
        result = self.dbapi.get_active_driver_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_one_host_one_driver(self, mock_utcnow):
        h = 'fake-host'
        d = 'fake-driver'
        expected = {d: {h}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[d])
        result = self.dbapi.get_active_driver_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_one_host_many_drivers(self, mock_utcnow):
        h = 'fake-host'
        d1 = 'driver-one'
        d2 = 'driver-two'
        expected = {d1: {h}, d2: {h}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[d1, d2])
        result = self.dbapi.get_active_driver_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_many_hosts_one_driver(self, mock_utcnow):
        h1 = 'host-one'
        h2 = 'host-two'
        d = 'fake-driver'
        expected = {d: {h1, h2}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(id=1, hostname=h1, drivers=[d])
        self._create_test_cdr(id=2, hostname=h2, drivers=[d])
        result = self.dbapi.get_active_driver_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_many_hosts_and_drivers(self, mock_utcnow):
        h1 = 'host-one'
        h2 = 'host-two'
        h3 = 'host-three'
        d1 = 'driver-one'
        d2 = 'driver-two'
        expected = {d1: {h1, h2}, d2: {h2, h3}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(id=1, hostname=h1, drivers=[d1])
        self._create_test_cdr(id=2, hostname=h2, drivers=[d1, d2])
        self._create_test_cdr(id=3, hostname=h3, drivers=[d2])
        result = self.dbapi.get_active_driver_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_driver_dict_with_old_conductor(self, mock_utcnow):
        past = datetime.datetime(2000, 1, 1, 0, 0)
        present = past + datetime.timedelta(minutes=2)

        d = 'common-driver'

        h1 = 'old-host'
        d1 = 'old-driver'
        mock_utcnow.return_value = past
        self._create_test_cdr(id=1, hostname=h1, drivers=[d, d1])

        h2 = 'new-host'
        d2 = 'new-driver'
        mock_utcnow.return_value = present
        self._create_test_cdr(id=2, hostname=h2, drivers=[d, d2])

        # verify that old-host does not show up in current list
        one_minute = 60
        expected = {d: {h2}, d2: {h2}}
        result = self.dbapi.get_active_driver_dict(interval=one_minute)
        self.assertEqual(expected, result)

        # change the interval, and verify that old-host appears
        two_minute = one_minute * 2
        expected = {d: {h1, h2}, d1: {h1}, d2: {h2}}
        result = self.dbapi.get_active_driver_dict(interval=two_minute)
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_one_host_no_ht(self, mock_utcnow):
        h = 'fake-host'
        expected = {}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[], hardware_types=[])
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_one_host_one_ht(self, mock_utcnow):
        h = 'fake-host'
        ht = 'hardware-type'
        expected = {ht: {h}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[], hardware_types=[ht])
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_one_host_many_ht(self, mock_utcnow):
        h = 'fake-host'
        ht1 = 'hardware-type'
        ht2 = 'another-hardware-type'
        expected = {ht1: {h}, ht2: {h}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(hostname=h, drivers=[],
                              hardware_types=[ht1, ht2])
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_many_host_one_ht(self, mock_utcnow):
        h1 = 'host-one'
        h2 = 'host-two'
        ht = 'hardware-type'
        expected = {ht: {h1, h2}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(id=1, hostname=h1, drivers=[],
                              hardware_types=[ht])
        self._create_test_cdr(id=2, hostname=h2, drivers=[],
                              hardware_types=[ht])
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_many_host_many_ht(self,
                                                             mock_utcnow):
        h1 = 'host-one'
        h2 = 'host-two'
        ht1 = 'hardware-type'
        ht2 = 'another-hardware-type'
        expected = {ht1: {h1, h2}, ht2: {h1, h2}}

        mock_utcnow.return_value = datetime.datetime.utcnow()
        self._create_test_cdr(id=1, hostname=h1, drivers=[],
                              hardware_types=[ht1, ht2])
        self._create_test_cdr(id=2, hostname=h2, drivers=[],
                              hardware_types=[ht1, ht2])
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_active_hardware_type_dict_with_old_conductor(self,
                                                              mock_utcnow):
        past = datetime.datetime(2000, 1, 1, 0, 0)
        present = past + datetime.timedelta(minutes=2)

        ht = 'hardware-type'

        h1 = 'old-host'
        ht1 = 'old-hardware-type'
        mock_utcnow.return_value = past
        self._create_test_cdr(id=1, hostname=h1, drivers=[],
                              hardware_types=[ht, ht1])

        h2 = 'new-host'
        ht2 = 'new-hardware-type'
        mock_utcnow.return_value = present
        self._create_test_cdr(id=2, hostname=h2, drivers=[],
                              hardware_types=[ht, ht2])

        # verify that old-host does not show up in current list
        self.config(heartbeat_timeout=60, group='conductor')
        expected = {ht: {h2}, ht2: {h2}}
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

        # change the heartbeat timeout, and verify that old-host appears
        self.config(heartbeat_timeout=120, group='conductor')
        expected = {ht: {h1, h2}, ht1: {h1}, ht2: {h2}}
        result = self.dbapi.get_active_hardware_type_dict()
        self.assertEqual(expected, result)

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_get_offline_conductors(self, mock_utcnow):
        self.config(heartbeat_timeout=60, group='conductor')
        time_ = datetime.datetime(2000, 1, 1, 0, 0)

        mock_utcnow.return_value = time_
        c = self._create_test_cdr()

        # Only 30 seconds passed since last heartbeat, it's still
        # considered alive
        mock_utcnow.return_value = time_ + datetime.timedelta(seconds=30)
        self.assertEqual([], self.dbapi.get_offline_conductors())

        # 61 seconds passed since last heartbeat, it's dead
        mock_utcnow.return_value = time_ + datetime.timedelta(seconds=61)
        self.assertEqual([c.hostname], self.dbapi.get_offline_conductors())

    @mock.patch.object(timeutils, 'utcnow', autospec=True)
    def test_list_hardware_type_interfaces(self, mock_utcnow):
        self.config(heartbeat_timeout=60, group='conductor')
        time_ = datetime.datetime(2000, 1, 1, 0, 0)
        h = 'fake-host'
        ht1 = 'hw-type-1'
        ht2 = 'hw-type-2'

        mock_utcnow.return_value = time_
        self._create_test_cdr(hostname=h, hardware_types=[ht1, ht2])

        expected = [
            {
                'hardware_type': ht1,
                'interface_type': 'power',
                'interface_name': 'ipmi',
                'default': True,
            },
            {
                'hardware_type': ht1,
                'interface_type': 'power',
                'interface_name': 'fake',
                'default': False,
            },
            {
                'hardware_type': ht2,
                'interface_type': 'power',
                'interface_name': 'ipmi',
                'default': True,
            },
            {
                'hardware_type': ht2,
                'interface_type': 'power',
                'interface_name': 'fake',
                'default': False,
            },
        ]

        def _verify(expected, result):
            for expected_row, row in zip(expected, result):
                for k, v in expected_row.items():
                    self.assertEqual(v, getattr(row, k))

        # with both hw types
        result = self.dbapi.list_hardware_type_interfaces([ht1, ht2])
        _verify(expected, result)

        # with one hw type
        result = self.dbapi.list_hardware_type_interfaces([ht1])
        _verify(expected[:2], result)

        # 61 seconds passed since last heartbeat, it's dead
        mock_utcnow.return_value = time_ + datetime.timedelta(seconds=61)
        result = self.dbapi.list_hardware_type_interfaces([ht1, ht2])
        self.assertEqual([], result)
