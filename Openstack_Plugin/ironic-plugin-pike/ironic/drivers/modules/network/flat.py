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
Flat network interface. Useful for shared, flat networks.
"""

from neutronclient.common import exceptions as neutron_exceptions
from oslo_config import cfg
from oslo_log import log

from ironic.common import exception
from ironic.common.i18n import _
from ironic.common import neutron
from ironic.drivers import base
from ironic.drivers.modules.network import common


LOG = log.getLogger(__name__)

CONF = cfg.CONF


class FlatNetwork(common.NeutronVIFPortIDMixin,
                  neutron.NeutronNetworkInterfaceMixin, base.NetworkInterface):
    """Flat network interface."""

    def __init__(self):
        cleaning_net = CONF.neutron.cleaning_network
        if not cleaning_net:
            LOG.warning(
                'Please specify a valid UUID or name for '
                '[neutron]/cleaning_network configuration option so that '
                'this interface is able to perform cleaning. Otherwise, '
                'cleaning operations will fail to start.')

    def validate(self, task):
        """Validates the network interface.

        :param task: a TaskManager instance.
        :raises: InvalidParameterValue, if the network interface configuration
            is invalid.
        :raises: MissingParameterValue, if some parameters are missing.
        """
        self.get_cleaning_network_uuid()

    def add_provisioning_network(self, task):
        """Add the provisioning network to a node.

        :param task: A TaskManager instance.
        :raises: NetworkError when failed to set binding:host_id
        """
        LOG.debug("Binding flat network ports")
        node = task.node
        host_id = node.instance_info.get('nova_host_id')
        if not host_id:
            return

        client = neutron.get_client()
        for port_like_obj in task.ports + task.portgroups:
            vif_port_id = (
                port_like_obj.internal_info.get(common.TENANT_VIF_KEY) or
                port_like_obj.extra.get('vif_port_id')
            )
            if not vif_port_id:
                continue
            body = {
                'port': {
                    'binding:host_id': host_id
                }
            }
            try:
                client.update_port(vif_port_id, body)
            except neutron_exceptions.NeutronClientException as e:
                msg = (_('Unable to set binding:host_id for '
                         'neutron port %(port_id)s. Error: '
                         '%(err)s') % {'port_id': vif_port_id, 'err': e})
                LOG.exception(msg)
                raise exception.NetworkError(msg)

    def remove_provisioning_network(self, task):
        """Remove the provisioning network from a node.

        :param task: A TaskManager instance.
        """
        pass

    def configure_tenant_networks(self, task):
        """Configure tenant networks for a node.

        :param task: A TaskManager instance.
        """
        pass

    def unconfigure_tenant_networks(self, task):
        """Unconfigure tenant networks for a node.

        :param task: A TaskManager instance.
        """
        pass

    def add_cleaning_network(self, task):
        """Add the cleaning network to a node.

        :param task: A TaskManager instance.
        :returns: a dictionary in the form {port.uuid: neutron_port['id']}
        :raises: NetworkError, InvalidParameterValue
        """
        # If we have left over ports from a previous cleaning, remove them
        neutron.rollback_ports(task, self.get_cleaning_network_uuid())
        LOG.info('Adding cleaning network to node %s', task.node.uuid)
        vifs = neutron.add_ports_to_network(
            task, self.get_cleaning_network_uuid())
        for port in task.ports:
            if port.uuid in vifs:
                internal_info = port.internal_info
                internal_info['cleaning_vif_port_id'] = vifs[port.uuid]
                port.internal_info = internal_info
                port.save()
        return vifs

    def remove_cleaning_network(self, task):
        """Remove the cleaning network from a node.

        :param task: A TaskManager instance.
        :raises: NetworkError
        """
        LOG.info('Removing ports from cleaning network for node %s',
                 task.node.uuid)
        neutron.remove_ports_from_network(task,
                                          self.get_cleaning_network_uuid())
        for port in task.ports:
            if 'cleaning_vif_port_id' in port.internal_info:
                internal_info = port.internal_info
                del internal_info['cleaning_vif_port_id']
                port.internal_info = internal_info
                port.save()
