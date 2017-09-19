# Copyright 2016 Red Hat, Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License"); you may
# not use this file except in compliance with the License. You may obtain
# a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations
# under the License.

"""
Generic hardware types.
"""

from ironic.drivers import hardware_type
from ironic.drivers.modules import agent
from ironic.drivers.modules import fake
from ironic.drivers.modules import inspector
from ironic.drivers.modules import iscsi_deploy
from ironic.drivers.modules.network import flat as flat_net
from ironic.drivers.modules.network import neutron
from ironic.drivers.modules.network import noop as noop_net
from ironic.drivers.modules import noop
from ironic.drivers.modules import pxe
from ironic.drivers.modules.storage import cinder
from ironic.drivers.modules.storage import noop as noop_storage


class GenericHardware(hardware_type.AbstractHardwareType):
    """Abstract base class representing generic hardware.

    This class provides reasonable defaults for boot, deploy, inspect, network
    and raid interfaces.
    """

    @property
    def supported_boot_interfaces(self):
        """List of supported boot interfaces."""
        return [pxe.PXEBoot]

    @property
    def supported_deploy_interfaces(self):
        """List of supported deploy interfaces."""
        return [iscsi_deploy.ISCSIDeploy, agent.AgentDeploy]

    @property
    def supported_inspect_interfaces(self):
        """List of supported inspect interfaces."""
        # Inspector support should be the default if it's enabled by an
        # operator (implying that the service is installed).
        return [inspector.Inspector, noop.NoInspect]

    @property
    def supported_network_interfaces(self):
        """List of supported network interfaces."""
        return [flat_net.FlatNetwork, neutron.NeutronNetwork,
                noop_net.NoopNetwork]

    @property
    def supported_raid_interfaces(self):
        """List of supported raid interfaces."""
        # AgentRAID requires RAID bits on the IPA image that are not shipped by
        # default. Hence, even if AgentRAID is enabled, NoRAID is the default.
        return [noop.NoRAID, agent.AgentRAID]

    @property
    def supported_storage_interfaces(self):
        """List of supported storage interfaces."""
        return [noop_storage.NoopStorage, cinder.CinderStorage]


class ManualManagementHardware(GenericHardware):
    """Hardware type that uses manual power and boot management.

    Using this hardware type assumes that an operator manages reboot
    and setting boot devices manually. This hardware type should only be used
    when no suitable hardware type exists in ironic, or the existing hardware
    type misbehaves for any reason.
    """

    @property
    def supported_management_interfaces(self):
        """List of supported management interfaces."""
        return [fake.FakeManagement]

    @property
    def supported_power_interfaces(self):
        """List of supported power interfaces."""
        return [fake.FakePower]

    @property
    def supported_vendor_interfaces(self):
        """List of supported vendor interfaces."""
        return [noop.NoVendor]
