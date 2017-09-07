# Copyright 2014 Rackspace, Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from ironic_lib import metrics_utils
from ironic_lib import utils as il_utils
from oslo_log import log
from oslo_utils import units
import six.moves.urllib_parse as urlparse

from ironic.common import dhcp_factory
from ironic.common import exception
from ironic.common.glance_service import service_utils
from ironic.common.i18n import _
from ironic.common import images
from ironic.common import raid
from ironic.common import states
from ironic.common import utils
from ironic.conductor import task_manager
from ironic.conductor import utils as manager_utils
from ironic.conf import CONF
from ironic.drivers import base
from ironic.drivers.modules import agent_base_vendor
from ironic.drivers.modules import deploy_utils


LOG = log.getLogger(__name__)

METRICS = metrics_utils.get_metrics_logger(__name__)

REQUIRED_PROPERTIES = {
    'deploy_kernel': _('UUID (from Glance) of the deployment kernel. '
                       'Required.'),
    'deploy_ramdisk': _('UUID (from Glance) of the ramdisk with agent that is '
                        'used at deploy time. Required.'),
}

OPTIONAL_PROPERTIES = {
    'image_http_proxy': _('URL of a proxy server for HTTP connections. '
                          'Optional.'),
    'image_https_proxy': _('URL of a proxy server for HTTPS connections. '
                           'Optional.'),
    'image_no_proxy': _('A comma-separated list of host names, IP addresses '
                        'and domain names (with optional :port) that will be '
                        'excluded from proxying. To denote a doman name, use '
                        'a dot to prefix the domain name. This value will be '
                        'ignored if ``image_http_proxy`` and '
                        '``image_https_proxy`` are not specified. Optional.'),
}

COMMON_PROPERTIES = REQUIRED_PROPERTIES.copy()
COMMON_PROPERTIES.update(OPTIONAL_PROPERTIES)
COMMON_PROPERTIES.update(agent_base_vendor.VENDOR_PROPERTIES)

PARTITION_IMAGE_LABELS = ('kernel', 'ramdisk', 'root_gb', 'root_mb', 'swap_mb',
                          'ephemeral_mb', 'ephemeral_format', 'configdrive',
                          'preserve_ephemeral', 'image_type',
                          'deploy_boot_mode')


@METRICS.timer('check_image_size')
def check_image_size(task, image_source):
    """Check if the requested image is larger than the ram size.

    :param task: a TaskManager instance containing the node to act on.
    :param image_source: href of the image.
    :raises: InvalidParameterValue if size of the image is greater than
        the available ram size.
    """
    node = task.node
    properties = node.properties
    # skip check if 'memory_mb' is not defined
    if 'memory_mb' not in properties:
        LOG.warning('Skip the image size check as memory_mb is not '
                    'defined in properties on node %s.', node.uuid)
        return

    image_show = images.image_show(task.context, image_source)
    if CONF.agent.stream_raw_images and image_show.get('disk_format') == 'raw':
        LOG.debug('Skip the image size check since the image is going to be '
                  'streamed directly onto the disk for node %s', node.uuid)
        return

    memory_size = int(properties.get('memory_mb'))
    image_size = int(image_show['size'])
    reserved_size = CONF.agent.memory_consumed_by_agent
    if (image_size + (reserved_size * units.Mi)) > (memory_size * units.Mi):
        msg = (_('Memory size is too small for requested image, if it is '
                 'less than (image size + reserved RAM size), will break '
                 'the IPA deployments. Image size: %(image_size)d MiB, '
                 'Memory size: %(memory_size)d MiB, Reserved size: '
                 '%(reserved_size)d MiB.')
               % {'image_size': image_size / units.Mi,
                  'memory_size': memory_size,
                  'reserved_size': reserved_size})
        raise exception.InvalidParameterValue(msg)


@METRICS.timer('validate_image_proxies')
def validate_image_proxies(node):
    """Check that the provided proxy parameters are valid.

    :param node: an Ironic node.
    :raises: InvalidParameterValue if any of the provided proxy parameters are
        incorrect.
    """
    invalid_proxies = {}
    for scheme in ('http', 'https'):
        proxy_param = 'image_%s_proxy' % scheme
        proxy = node.driver_info.get(proxy_param)
        if proxy:
            chunks = urlparse.urlparse(proxy)
            # NOTE(vdrok) If no scheme specified, this is still a valid
            # proxy address. It is also possible for a proxy to have a
            # scheme different from the one specified in the image URL,
            # e.g. it is possible to use https:// proxy for downloading
            # http:// image.
            if chunks.scheme not in ('', 'http', 'https'):
                invalid_proxies[proxy_param] = proxy
    msg = ''
    if invalid_proxies:
        msg += _("Proxy URL should either have HTTP(S) scheme "
                 "or no scheme at all, the following URLs are "
                 "invalid: %s.") % invalid_proxies
    no_proxy = node.driver_info.get('image_no_proxy')
    if no_proxy is not None and not utils.is_valid_no_proxy(no_proxy):
        msg += _(
            "image_no_proxy should be a list of host names, IP addresses "
            "or domain names to exclude from proxying, the specified list "
            "%s is incorrect. To denote a domain name, prefix it with a dot "
            "(instead of e.g. '.*').") % no_proxy
    if msg:
        raise exception.InvalidParameterValue(msg)


class AgentDeployMixin(agent_base_vendor.AgentDeployMixin):

    @METRICS.timer('AgentDeployMixin.deploy_has_started')
    def deploy_has_started(self, task):
        commands = self._client.get_commands_status(task.node)

        for command in commands:
            if command['command_name'] == 'prepare_image':
                # deploy did start at some point
                return True
        return False

    @METRICS.timer('AgentDeployMixin.deploy_is_done')
    def deploy_is_done(self, task):
        commands = self._client.get_commands_status(task.node)
        if not commands:
            return False

        last_command = commands[-1]

        if last_command['command_name'] != 'prepare_image':
            # catches race condition where prepare_image is still processing
            # so deploy hasn't started yet
            return False

        if last_command['command_status'] != 'RUNNING':
            return True

        return False

    @METRICS.timer('AgentDeployMixin.continue_deploy')
    @task_manager.require_exclusive_lock
    def continue_deploy(self, task):
        task.process_event('resume')
        node = task.node
        image_source = node.instance_info.get('image_source')
        LOG.debug('Continuing deploy for node %(node)s with image %(img)s',
                  {'node': node.uuid, 'img': image_source})

        image_info = {
            'id': image_source.split('/')[-1],
            'urls': [node.instance_info['image_url']],
            'checksum': node.instance_info['image_checksum'],
            # NOTE(comstud): Older versions of ironic do not set
            # 'disk_format' nor 'container_format', so we use .get()
            # to maintain backwards compatibility in case code was
            # upgraded in the middle of a build request.
            'disk_format': node.instance_info.get('image_disk_format'),
            'container_format': node.instance_info.get(
                'image_container_format'),
            'stream_raw_images': CONF.agent.stream_raw_images,
        }

        proxies = {}
        for scheme in ('http', 'https'):
            proxy_param = 'image_%s_proxy' % scheme
            proxy = node.driver_info.get(proxy_param)
            if proxy:
                proxies[scheme] = proxy
        if proxies:
            image_info['proxies'] = proxies
            no_proxy = node.driver_info.get('image_no_proxy')
            if no_proxy is not None:
                image_info['no_proxy'] = no_proxy

        image_info['node_uuid'] = node.uuid
        iwdi = node.driver_internal_info.get('is_whole_disk_image')
        if not iwdi:
            for label in PARTITION_IMAGE_LABELS:
                image_info[label] = node.instance_info.get(label)
            boot_option = deploy_utils.get_boot_option(node)
            boot_mode = deploy_utils.get_boot_mode_for_deploy(node)
            if boot_mode:
                image_info['deploy_boot_mode'] = boot_mode
            else:
                image_info['deploy_boot_mode'] = 'bios'
            image_info['boot_option'] = boot_option
            disk_label = deploy_utils.get_disk_label(node)
            if disk_label is not None:
                image_info['disk_label'] = disk_label

        # Tell the client to download and write the image with the given args
        self._client.prepare_image(node, image_info)

        task.process_event('wait')

    def _get_uuid_from_result(self, task, type_uuid):
        command = self._client.get_commands_status(task.node)[-1]

        if command['command_result'] is not None:
            words = command['command_result']['result'].split()
            for word in words:
                if type_uuid in word:
                    result = word.split('=')[1]
                    if not result:
                        msg = (_('Command result did not return %(type_uuid)s '
                                 'for node %(node)s. The version of the IPA '
                                 'ramdisk used in the deployment might not '
                                 'have support for provisioning of '
                                 'partition images.') %
                               {'type_uuid': type_uuid,
                                'node': task.node.uuid})
                        LOG.error(msg)
                        deploy_utils.set_failed_state(task, msg)
                        return
                    return result

    @METRICS.timer('AgentDeployMixin.check_deploy_success')
    def check_deploy_success(self, node):
        # should only ever be called after we've validated that
        # the prepare_image command is complete
        command = self._client.get_commands_status(node)[-1]
        if command['command_status'] == 'FAILED':
            return command['command_error']

    @METRICS.timer('AgentDeployMixin.reboot_to_instance')
    def reboot_to_instance(self, task):
        task.process_event('resume')
        node = task.node
        iwdi = task.node.driver_internal_info.get('is_whole_disk_image')
        error = self.check_deploy_success(node)
        if error is not None:
            # TODO(jimrollenhagen) power off if using neutron dhcp to
            #                      align with pxe driver?
            msg = (_('node %(node)s command status errored: %(error)s') %
                   {'node': node.uuid, 'error': error})
            LOG.error(msg)
            deploy_utils.set_failed_state(task, msg)
            return
        if not iwdi:
            root_uuid = self._get_uuid_from_result(task, 'root_uuid')
            if deploy_utils.get_boot_mode_for_deploy(node) == 'uefi':
                efi_sys_uuid = (
                    self._get_uuid_from_result(task,
                                               'efi_system_partition_uuid'))
            else:
                efi_sys_uuid = None
            driver_internal_info = task.node.driver_internal_info
            driver_internal_info['root_uuid_or_disk_id'] = root_uuid
            task.node.driver_internal_info = driver_internal_info
            task.node.save()
            self.prepare_instance_to_boot(task, root_uuid, efi_sys_uuid)
        LOG.info('Image successfully written to node %s', node.uuid)
        LOG.debug('Rebooting node %s to instance', node.uuid)
        if iwdi:
            manager_utils.node_set_boot_device(task, 'disk', persistent=True)

        self.reboot_and_finish_deploy(task)

        # NOTE(TheJulia): If we deployed a whole disk image, we
        # should expect a whole disk image and clean-up the tftp files
        # on-disk incase the node is disregarding the boot preference.
        # TODO(rameshg87): Not all in-tree drivers using reboot_to_instance
        # have a boot interface. So include a check for now. Remove this
        # check once all in-tree drivers have a boot interface.
        if task.driver.boot and iwdi:
            task.driver.boot.clean_up_ramdisk(task)


class AgentDeploy(AgentDeployMixin, base.DeployInterface):
    """Interface for deploy-related actions."""

    def get_properties(self):
        """Return the properties of the interface.

        :returns: dictionary of <property name>:<property description> entries.
        """
        return COMMON_PROPERTIES

    @METRICS.timer('AgentDeploy.validate')
    def validate(self, task):
        """Validate the driver-specific Node deployment info.

        This method validates whether the properties of the supplied node
        contain the required information for this driver to deploy images to
        the node.

        :param task: a TaskManager instance
        :raises: MissingParameterValue, if any of the required parameters are
            missing.
        :raises: InvalidParameterValue, if any of the parameters have invalid
            value.
        """
        if CONF.agent.manage_agent_boot:
            task.driver.boot.validate(task)

        node = task.node

        # Validate node capabilities
        deploy_utils.validate_capabilities(node)

        if not task.driver.storage.should_write_image(task):
            # NOTE(TheJulia): There is no reason to validate
            # image properties if we will not be writing an image
            # in a boot from volume case. As such, return to the caller.
            LOG.debug('Skipping complete deployment interface validation '
                      'for node %s as it is set to boot from a remote '
                      'volume.', node.uuid)
            return

        params = {}
        image_source = node.instance_info.get('image_source')
        params['instance_info.image_source'] = image_source
        error_msg = _('Node %s failed to validate deploy image info. Some '
                      'parameters were missing') % node.uuid

        deploy_utils.check_for_missing_params(params, error_msg)

        if not service_utils.is_glance_image(image_source):
            if not node.instance_info.get('image_checksum'):
                raise exception.MissingParameterValue(_(
                    "image_source's image_checksum must be provided in "
                    "instance_info for node %s") % node.uuid)

        check_image_size(task, image_source)
        # Validate the root device hints
        try:
            root_device = node.properties.get('root_device')
            il_utils.parse_root_device_hints(root_device)
        except ValueError as e:
            raise exception.InvalidParameterValue(
                _('Failed to validate the root device hints for node '
                  '%(node)s. Error: %(error)s') % {'node': node.uuid,
                                                   'error': e})

        validate_image_proxies(node)

    @METRICS.timer('AgentDeploy.deploy')
    @task_manager.require_exclusive_lock
    def deploy(self, task):
        """Perform a deployment to a node.

        Perform the necessary work to deploy an image onto the specified node.
        This method will be called after prepare(), which may have already
        performed any preparatory steps, such as pre-caching some data for the
        node.

        :param task: a TaskManager instance.
        :returns: status of the deploy. One of ironic.common.states.
        """
        if task.driver.storage.should_write_image(task):
            manager_utils.node_power_action(task, states.REBOOT)
            return states.DEPLOYWAIT
        else:
            # TODO(TheJulia): At some point, we should de-dupe this code
            # as it is nearly identical to the iscsi deploy interface.
            # This is not being done now as it is expected to be
            # refactored in the near future.
            manager_utils.node_power_action(task, states.POWER_OFF)
            task.driver.network.remove_provisioning_network(task)
            task.driver.network.configure_tenant_networks(task)
            task.driver.boot.prepare_instance(task)
            manager_utils.node_power_action(task, states.POWER_ON)
            LOG.info('Deployment to node %s done', task.node.uuid)
            return states.DEPLOYDONE

    @METRICS.timer('AgentDeploy.tear_down')
    @task_manager.require_exclusive_lock
    def tear_down(self, task):
        """Tear down a previous deployment on the task's node.

        :param task: a TaskManager instance.
        :returns: status of the deploy. One of ironic.common.states.
        :raises: NetworkError if the cleaning ports cannot be removed.
        :raises: InvalidParameterValue when the wrong power state is specified
             or the wrong driver info is specified for power management.
        :raises: StorageError when the storage interface attached volumes fail
             to detach.
        :raises: other exceptions by the node's power driver if something
             wrong occurred during the power action.
        """
        manager_utils.node_power_action(task, states.POWER_OFF)
        task.driver.storage.detach_volumes(task)
        deploy_utils.tear_down_storage_configuration(task)
        task.driver.network.unconfigure_tenant_networks(task)

        return states.DELETED

    @METRICS.timer('AgentDeploy.prepare')
    @task_manager.require_exclusive_lock
    def prepare(self, task):
        """Prepare the deployment environment for this node.

        :param task: a TaskManager instance.
        :raises: NetworkError: if the previous cleaning ports cannot be removed
            or if new cleaning ports cannot be created.
        :raises: InvalidParameterValue when the wrong power state is specified
            or the wrong driver info is specified for power management.
        :raises: StorageError If the storage driver is unable to attach the
            configured volumes.
        :raises: other exceptions by the node's power driver if something
            wrong occurred during the power action.
        :raises: exception.ImageRefValidationFailed if image_source is not
            Glance href and is not HTTP(S) URL.
        :raises: any boot interface's prepare_ramdisk exceptions.
        """
        node = task.node
        deploy_utils.populate_storage_driver_internal_info(task)
        if node.provision_state == states.DEPLOYING:
            # Adding the node to provisioning network so that the dhcp
            # options get added for the provisioning port.
            manager_utils.node_power_action(task, states.POWER_OFF)
            if task.driver.storage.should_write_image(task):
                # NOTE(vdrok): in case of rebuild, we have tenant network
                # already configured, unbind tenant ports if present
                task.driver.network.unconfigure_tenant_networks(task)
                task.driver.network.add_provisioning_network(task)
            # Signal to storage driver to attach volumes
            task.driver.storage.attach_volumes(task)
            if not task.driver.storage.should_write_image(task):
                # We have nothing else to do as this is handled in the
                # backend storage system, and we can return to the caller
                # as we do not need to boot the agent to deploy.
                return
        if node.provision_state == states.ACTIVE:
            # Call is due to conductor takeover
            task.driver.boot.prepare_instance(task)
        elif node.provision_state != states.ADOPTING:
            node.instance_info = deploy_utils.build_instance_info_for_deploy(
                task)
            node.save()
            if CONF.agent.manage_agent_boot:
                deploy_opts = deploy_utils.build_agent_options(node)
                task.driver.boot.prepare_ramdisk(task, deploy_opts)

    @METRICS.timer('AgentDeploy.clean_up')
    @task_manager.require_exclusive_lock
    def clean_up(self, task):
        """Clean up the deployment environment for this node.

        If preparation of the deployment environment ahead of time is possible,
        this method should be implemented by the driver. It should erase
        anything cached by the `prepare` method.

        If implemented, this method must be idempotent. It may be called
        multiple times for the same node on the same conductor, and it may be
        called by multiple conductors in parallel. Therefore, it must not
        require an exclusive lock.

        This method is called before `tear_down`.

        :param task: a TaskManager instance.
        """
        if CONF.agent.manage_agent_boot:
            task.driver.boot.clean_up_ramdisk(task)
        task.driver.boot.clean_up_instance(task)
        provider = dhcp_factory.DHCPFactory()
        provider.clean_dhcp(task)

    def take_over(self, task):
        """Take over management of this node from a dead conductor.

        :param task: a TaskManager instance.
        """
        pass

    @METRICS.timer('AgentDeploy.get_clean_steps')
    def get_clean_steps(self, task):
        """Get the list of clean steps from the agent.

        :param task: a TaskManager object containing the node
        :raises NodeCleaningFailure: if the clean steps are not yet
            available (cached), for example, when a node has just been
            enrolled and has not been cleaned yet.
        :returns: A list of clean step dictionaries
        """
        new_priorities = {
            'erase_devices': CONF.deploy.erase_devices_priority,
            'erase_devices_metadata':
                CONF.deploy.erase_devices_metadata_priority,
        }
        return deploy_utils.agent_get_clean_steps(
            task, interface='deploy',
            override_priorities=new_priorities)

    @METRICS.timer('AgentDeploy.execute_clean_step')
    def execute_clean_step(self, task, step):
        """Execute a clean step asynchronously on the agent.

        :param task: a TaskManager object containing the node
        :param step: a clean step dictionary to execute
        :raises: NodeCleaningFailure if the agent does not return a command
            status
        :returns: states.CLEANWAIT to signify the step will be completed async
        """
        return deploy_utils.agent_execute_clean_step(task, step)

    @METRICS.timer('AgentDeploy.prepare_cleaning')
    def prepare_cleaning(self, task):
        """Boot into the agent to prepare for cleaning.

        :param task: a TaskManager object containing the node
        :raises: NodeCleaningFailure, NetworkError if the previous cleaning
            ports cannot be removed or if new cleaning ports cannot be created.
        :raises: InvalidParameterValue if cleaning network UUID config option
            has an invalid value.
        :returns: states.CLEANWAIT to signify an asynchronous prepare
        """
        return deploy_utils.prepare_inband_cleaning(
            task, manage_boot=CONF.agent.manage_agent_boot)

    @METRICS.timer('AgentDeploy.tear_down_cleaning')
    def tear_down_cleaning(self, task):
        """Clean up the PXE and DHCP files after cleaning.

        :param task: a TaskManager object containing the node
        :raises: NodeCleaningFailure, NetworkError if the cleaning ports cannot
            be removed
        """
        deploy_utils.tear_down_inband_cleaning(
            task, manage_boot=CONF.agent.manage_agent_boot)


class AgentRAID(base.RAIDInterface):
    """Implementation of RAIDInterface which uses agent ramdisk."""

    def get_properties(self):
        """Return the properties of the interface."""
        return {}

    @METRICS.timer('AgentRAID.create_configuration')
    @base.clean_step(priority=0)
    def create_configuration(self, task,
                             create_root_volume=True,
                             create_nonroot_volumes=True):
        """Create a RAID configuration on a bare metal using agent ramdisk.

        This method creates a RAID configuration on the given node.

        :param task: a TaskManager instance.
        :param create_root_volume: If True, a root volume is created
            during RAID configuration. Otherwise, no root volume is
            created. Default is True.
        :param create_nonroot_volumes: If True, non-root volumes are
            created. If False, no non-root volumes are created. Default
            is True.
        :returns: states.CLEANWAIT if operation was successfully invoked.
        :raises: MissingParameterValue, if node.target_raid_config is missing
            or was found to be empty after skipping root volume and/or non-root
            volumes.
        """
        node = task.node
        LOG.debug("Agent RAID create_configuration invoked for node %(node)s "
                  "with create_root_volume=%(create_root_volume)s and "
                  "create_nonroot_volumes=%(create_nonroot_volumes)s with the "
                  "following target_raid_config: %(target_raid_config)s.",
                  {'node': node.uuid,
                   'create_root_volume': create_root_volume,
                   'create_nonroot_volumes': create_nonroot_volumes,
                   'target_raid_config': node.target_raid_config})

        if not node.target_raid_config:
            raise exception.MissingParameterValue(
                _("Node %s has no target RAID configuration.") % node.uuid)

        target_raid_config = node.target_raid_config.copy()

        error_msg_list = []
        if not create_root_volume:
            target_raid_config['logical_disks'] = [
                x for x in target_raid_config['logical_disks']
                if not x.get('is_root_volume')]
            error_msg_list.append(_("skipping root volume"))

        if not create_nonroot_volumes:
            error_msg_list.append(_("skipping non-root volumes"))

            target_raid_config['logical_disks'] = [
                x for x in target_raid_config['logical_disks']
                if x.get('is_root_volume')]

        if not target_raid_config['logical_disks']:
            error_msg = _(' and ').join(error_msg_list)
            raise exception.MissingParameterValue(
                _("Node %(node)s has empty target RAID configuration "
                  "after %(msg)s.") % {'node': node.uuid, 'msg': error_msg})

        # Rewrite it back to the node object, but no need to save it as
        # we need to just send this to the agent ramdisk.
        node.driver_internal_info['target_raid_config'] = target_raid_config

        LOG.debug("Calling agent RAID create_configuration for node %(node)s "
                  "with the following target RAID configuration: %(target)s",
                  {'node': node.uuid, 'target': target_raid_config})
        step = node.clean_step
        return deploy_utils.agent_execute_clean_step(task, step)

    @staticmethod
    @agent_base_vendor.post_clean_step_hook(
        interface='raid', step='create_configuration')
    def _create_configuration_final(task, command):
        """Clean step hook after a RAID configuration was created.

        This method is invoked as a post clean step hook by the Ironic
        conductor once a create raid configuration is completed successfully.
        The node (properties, capabilities, RAID information) will be updated
        to reflect the actual RAID configuration that was created.

        :param task: a TaskManager instance.
        :param command: A command result structure of the RAID operation
            returned from agent ramdisk on query of the status of command(s).
        :raises: InvalidParameterValue, if 'current_raid_config' has more than
            one root volume or if node.properties['capabilities'] is malformed.
        :raises: IronicException, if clean_result couldn't be found within
            the 'command' argument passed.
        """
        try:
            clean_result = command['command_result']['clean_result']
        except KeyError:
            raise exception.IronicException(
                _("Agent ramdisk didn't return a proper command result while "
                  "cleaning %(node)s. It returned '%(result)s' after command "
                  "execution.") % {'node': task.node.uuid,
                                   'result': command})

        raid.update_raid_info(task.node, clean_result)

    @METRICS.timer('AgentRAID.delete_configuration')
    @base.clean_step(priority=0)
    def delete_configuration(self, task):
        """Deletes RAID configuration on the given node.

        :param task: a TaskManager instance.
        :returns: states.CLEANWAIT if operation was successfully invoked
        """
        LOG.debug("Agent RAID delete_configuration invoked for node %s.",
                  task.node.uuid)
        step = task.node.clean_step
        return deploy_utils.agent_execute_clean_step(task, step)

    @staticmethod
    @agent_base_vendor.post_clean_step_hook(
        interface='raid', step='delete_configuration')
    def _delete_configuration_final(task, command):
        """Clean step hook after RAID configuration was deleted.

        This method is invoked as a post clean step hook by the Ironic
        conductor once a delete raid configuration is completed successfully.
        It sets node.raid_config to empty dictionary.

        :param task: a TaskManager instance.
        :param command: A command result structure of the RAID operation
            returned from agent ramdisk on query of the status of command(s).
        :returns: None
        """
        task.node.raid_config = {}
        task.node.save()
