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
bmc redfish Driver for Huawei redfish driver.
"""

from ironic.drivers import base
from ironic.drivers.modules import agent
from ironic.drivers.modules import iscsi_deploy
from ironic.drivers.modules import pxe
from ironic.drivers import generic
from ironic.drivers.modules.bmc_redfish import management
from ironic.drivers.modules.bmc_redfish import power

class BMCRedfishHardware(generic.GenericHardware):
    """Redfish hardware type."""

    @property
    def supported_management_interfaces(self):
        """List of supported management interfaces."""
        return [management.BMCRedfishManagement]

    @property
    def supported_power_interfaces(self):
        """List of supported power interfaces."""
        return [power.BMCRedfishPower]

class AgentAndBMCRedfish(base.BaseDriver):
    """Redfish driver for huawei bmc server.

    This driver implements the `core` functionality using
    :class: `ironic.drivers.modules.bmc_redfish.BMCRedfishManagement
    :class: `ironic.drivers.modules.bmc_redfish.BMCRedfishPower
    """

    def __init__(self):
        self.power = power.BMCRedfishPower()
        self.boot = pxe.PXEBoot()
        self.deploy = agent.AgentDeploy()
        self.management = management.BMCRedfishManagement()

class PXEAndBMCRedfish(base.BaseDriver):
    """PXE + huawei bmc driver.

    This driver implements the `core` functionality, combining

    :class: `ironic.drivers.modules.bmc_redfish.BMCRedfishManagement
    :class: `ironic.drivers.modules.bmc_redfish.BMCRedfishPower
    """
    def __init__(self):
        self.power = power.BMCRedfishPower()
        self.boot = pxe.PXEBoot()
        self.deploy = iscsi_deploy.ISCSIDeploy()
        self.management = management.BMCRedfishManagement()
