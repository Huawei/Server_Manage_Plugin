# Copyright 2017 Red Hat, Inc.
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

import logging

from hwsushy import exceptions
from hwsushy.resources import base
from hwsushy.resources.system import constants as sys_cons
from hwsushy.resources.system import mappings as sys_maps
from hwsushy import utils

LOG = logging.getLogger(__name__)


class System(base.ResourceBase):

    asset_tag = None
    """The system asset tag"""

    bios_version = None
    """The system BIOS version"""

    boot = None
    """A dictionary containg the current boot device, frequency and mode"""

    description = None
    """The system description"""

    hostname = None
    """The system hostname"""

    identity = None
    """The system identity string"""

    # TODO(lucasagomes): Create mappings for the indicator_led
    indicator_led = None
    """Whether the indicator LED is lit or off"""

    manufacturer = None
    """The system manufacturer"""

    name = None
    """The system name"""

    part_number = None
    """The system part number"""

    power_state = None
    """The system power state"""

    serial_number = None
    """The system serial number"""

    sku = None
    """The system stock-keeping unit"""

    # TODO(lucasagomes): Create mappings for the system_type
    system_type = None
    """The system type"""

    uuid = None
    """The system UUID"""

    def __init__(self, connector, identity, redfish_version=None):
        """A class representing a ComputerSystem

        :param connector: A Connector instance
        :param identity: The identity of the System resource
        :param redfish_version: The version of RedFish. Used to construct
            the object according to schema of the given version.
        """
        super(System, self).__init__(connector, identity, redfish_version)

    def _parse_attributes(self):
        self.asset_tag = self.json.get('AssetTag')
        self.bios_version = self.json.get('BiosVersion')
        self.description = self.json.get('Description')
        self.hostname = self.json.get('HostName')
        self.identity = self.json.get('Id')
        self.indicator_led = self.json.get('IndicatorLED')
        self.manufacturer = self.json.get('Manufacturer')
        self.name = self.json.get('Name')
        self.part_number = self.json.get('PartNumber')
        self.serial_number = self.json.get('SerialNumber')
        self.sku = self.json.get('SKU')
        self.system_type = self.json.get('SystemType')
        self.uuid = self.json.get('UUID')
        self.power_state = sys_maps.SYSTEM_POWER_STATE_MAP.get(
            self.json.get('PowerState'))

        # Parse the boot attribute
        self.boot = {}
        boot_attr = self.json.get('Boot')
        if boot_attr is not None:
            self.boot['target'] = sys_maps.BOOT_SOURCE_TARGET_MAP.get(
                boot_attr.get('BootSourceOverrideTarget'))
            self.boot['enabled'] = sys_maps.BOOT_SOURCE_ENABLED_MAP.get(
                boot_attr.get('BootSourceOverrideEnabled'))
            self.boot['mode'] = sys_maps.BOOT_SOURCE_MODE_MAP.get(
                boot_attr.get('BootSourceOverrideMode'))

    def _get_reset_action_element(self):
        actions = self.json.get('Actions')
        if not actions:
            raise exceptions.MissingAttributeError(attribute='Actions',
                                                   resource=self._path)

        reset_action = actions.get('#ComputerSystem.Reset')
        if not reset_action:
            raise exceptions.MissingActionError(action='#ComputerSystem.Reset',
                                                resource=self._path)
        return reset_action

    def get_allowed_reset_system_values(self):
        """Get the allowed values for resetting the system.

        :returns: A set with the allowed values.
        """
        reset_action = self._get_reset_action_element()

        allowed_values = reset_action.get('ResetType@Redfish.AllowableValues')

        if not allowed_values:
            LOG.warning('Could not figure out the allowed values for the '
                        'reset system action for System %s', self.identity)
            return set(sys_maps.RESET_SYSTEM_VALUE_MAP_REV)

        allowed_values = set(allowed_values['ResetType'])

        return set([sys_maps.RESET_SYSTEM_VALUE_MAP[v] for v in
                    set(sys_maps.RESET_SYSTEM_VALUE_MAP).
                    intersection(allowed_values)])

    def _get_reset_system_path(self):
        reset_action = self._get_reset_action_element()

        target_uri = reset_action.get('target')
        if not target_uri:
            raise exceptions.MissingAttributeError(
                attribute='Actions/ComputerSystem.Reset/target',
                resource=self._path)

        return target_uri

    def reset_system(self, value):
        """Reset the system.

        :param value: The target value.
        :raises: InvalidParameterValueError, if the target value is not
            allowed.
        """
        valid_resets = self.get_allowed_reset_system_values()
        if value not in valid_resets:
            raise exceptions.InvalidParameterValueError(
                parameter='value', value=value, valid_values=valid_resets)

        value = sys_maps.RESET_SYSTEM_VALUE_MAP_REV[value]
        target_uri = self._get_reset_system_path()

        # TODO(lucasagomes): Check the return code and response body ?
        #                    Probably we should call refresh() as well.
        self._conn.post(target_uri, data={'ResetType': value})

    def get_allowed_system_boot_source_values(self):
        """Get the allowed values for changing the boot source.

        :returns: A set with the allowed values.
        """
        boot = self.json.get('Boot')
        if not boot:
            raise exceptions.MissingAttributeError(attribute='Boot',
                                                   resource=self._path)

        allowed_values = boot.get(
            'BootSourceOverrideTarget@Redfish.AllowableValues')

        if not allowed_values:
            LOG.warning('Could not figure out the allowed values for '
                        'configuring the boot source for System %s',
                        self.identity)
            return set(sys_maps.BOOT_SOURCE_TARGET_MAP_REV)

        return set([sys_maps.BOOT_SOURCE_TARGET_MAP[v] for v in
                    set(sys_maps.BOOT_SOURCE_TARGET_MAP).
                    intersection(allowed_values)])

    def set_system_boot_source(self, target,
                               enabled=sys_cons.BOOT_SOURCE_ENABLED_ONCE,
                               mode=None):
        """Set the boot source.

        Set the boot source to use on next reboot of the System.

        :param target: The target boot source.
        :param enabled: The frequency, whether to set it for the next
            reboot only (BOOT_SOURCE_ENABLED_ONCE) or persistent to all
            future reboots (BOOT_SOURCE_ENABLED_CONTINUOUS) or disabled
            (BOOT_SOURCE_ENABLED_DISABLED).
        :param mode: The boot mode, UEFI (BOOT_SOURCE_MODE_UEFI) or
            BIOS (BOOT_SOURCE_MODE_BIOS).
        :raises: InvalidParameterValueError, if any information passed is
            invalid.
        """
        valid_targets = self.get_allowed_system_boot_source_values()
        if target not in valid_targets:
            raise exceptions.InvalidParameterValueError(
                parameter='target', value=target, valid_values=valid_targets)

        if enabled not in sys_maps.BOOT_SOURCE_ENABLED_MAP_REV:
            raise exceptions.InvalidParameterValueError(
                parameter='enabled', value=enabled,
                valid_values=list(sys_maps.BOOT_SOURCE_TARGET_MAP_REV))

        data = {
            'Boot': {
                'BootSourceOverrideTarget':
                    sys_maps.BOOT_SOURCE_TARGET_MAP_REV[target],
                'BootSourceOverrideEnabled':
                    sys_maps.BOOT_SOURCE_ENABLED_MAP_REV[enabled]
            }
        }

        if mode is not None:
            if mode not in sys_maps.BOOT_SOURCE_MODE_MAP_REV:
                raise exceptions.InvalidParameterValueError(
                    parameter='mode', value=mode,
                    valid_values=list(sys_maps.BOOT_SOURCE_MODE_MAP_REV))

            data['Boot']['BootSourceOverrideMode'] = (
                sys_maps.BOOT_SOURCE_MODE_MAP_REV[mode])

        # TODO(lucasagomes): Check the return code and response body ?
        #                    Probably we should call refresh() as well.
        self._conn.patch(self.path, data=data, headers={'If-Match': True})

    # TODO(lucasagomes): All system have a Manager and Chassis object,
    # include a get_manager() and get_chassis() once we have an abstraction
    # for those resources.

    def get_interfaces(self):
        """get interfaces of a bmc.

        :returns: list of interfaces
        """
        path = self._path + '/EthernetInterfaces'
        resp = self._conn.get(path)
        ifs = utils.get_members_identities(resp.json().get('Members', []))
        return [interface.split('/')[-1] for interface in ifs]

    def get_mac(self, interface):
        """get mac address of a interface.

        :param interface: id of the bmc interface
        :returns: string value of mac address
        """
        path = self._path + '/EthernetInterfaces/' + interface
        resp = self._conn.get(path)
        return resp.json().get("PermanentMACAddress")


class SystemCollection(base.ResourceCollectionBase):

    @property
    def _resource_type(self):
        return System

    def __init__(self, connector, path, redfish_version=None):
        """A class representing a ComputerSystemCollection

        :param connector: A Connector instance
        :param path: The canonical path to the System collection resource
        :param redfish_version: The version of RedFish. Used to construct
            the object according to schema of the given version.
        """
        super(SystemCollection, self).__init__(connector, path,
                                               redfish_version)
