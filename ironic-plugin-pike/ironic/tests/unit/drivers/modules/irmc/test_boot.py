# Copyright 2015 FUJITSU LIMITED
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
Test class for iRMC Boot Driver
"""

import os
import shutil
import tempfile

from ironic_lib import utils as ironic_utils
import mock
from oslo_config import cfg
from oslo_utils import uuidutils
import six

from ironic.common import boot_devices
from ironic.common import exception
from ironic.common.glance_service import service_utils
from ironic.common.i18n import _
from ironic.common import images
from ironic.common import states
from ironic.conductor import task_manager
from ironic.conductor import utils as manager_utils
from ironic.drivers.modules import deploy_utils
from ironic.drivers.modules.irmc import boot as irmc_boot
from ironic.drivers.modules.irmc import common as irmc_common
from ironic.drivers.modules.irmc import management as irmc_management
from ironic.drivers.modules import pxe
from ironic.tests.unit.conductor import mgr_utils
from ironic.tests.unit.db import base as db_base
from ironic.tests.unit.db import utils as db_utils
from ironic.tests.unit.drivers import third_party_driver_mock_specs \
    as mock_specs
from ironic.tests.unit.objects import utils as obj_utils

if six.PY3:
    import io
    file = io.BytesIO


INFO_DICT = db_utils.get_test_irmc_info()
CONF = cfg.CONF
PARSED_IFNO = {
    'irmc_address': '1.2.3.4',
    'irmc_port': 80,
    'irmc_username': 'admin0',
    'irmc_password': 'fake0',
    'irmc_auth_method': 'digest',
    'irmc_client_timeout': 60,
    'irmc_snmp_community': 'public',
    'irmc_snmp_port': 161,
    'irmc_snmp_version': 'v2c',
    'irmc_snmp_security': None,
    'irmc_sensor_method': 'ipmitool',
}


class IRMCDeployPrivateMethodsTestCase(db_base.DbTestCase):

    def setUp(self):
        irmc_boot.check_share_fs_mounted_patcher.start()
        self.addCleanup(irmc_boot.check_share_fs_mounted_patcher.stop)
        super(IRMCDeployPrivateMethodsTestCase, self).setUp()
        mgr_utils.mock_the_extension_manager(driver='iscsi_irmc')
        self.node = obj_utils.create_test_node(
            self.context, driver='iscsi_irmc', driver_info=INFO_DICT)

        CONF.irmc.remote_image_share_root = '/remote_image_share_root'
        CONF.irmc.remote_image_server = '10.20.30.40'
        CONF.irmc.remote_image_share_type = 'NFS'
        CONF.irmc.remote_image_share_name = 'share'
        CONF.irmc.remote_image_user_name = 'admin'
        CONF.irmc.remote_image_user_password = 'admin0'
        CONF.irmc.remote_image_user_domain = 'local'

    @mock.patch.object(os.path, 'isdir', spec_set=True, autospec=True)
    def test__parse_config_option(self, isdir_mock):
        isdir_mock.return_value = True

        result = irmc_boot._parse_config_option()

        isdir_mock.assert_called_once_with('/remote_image_share_root')
        self.assertIsNone(result)

    @mock.patch.object(os.path, 'isdir', spec_set=True, autospec=True)
    def test__parse_config_option_non_existed_root(self, isdir_mock):
        CONF.irmc.remote_image_share_root = '/non_existed_root'
        isdir_mock.return_value = False

        self.assertRaises(exception.InvalidParameterValue,
                          irmc_boot._parse_config_option)
        isdir_mock.assert_called_once_with('/non_existed_root')

    @mock.patch.object(os.path, 'isfile', spec_set=True, autospec=True)
    def test__parse_driver_info_in_share(self, isfile_mock):
        """With required 'irmc_deploy_iso' in share."""
        isfile_mock.return_value = True
        self.node.driver_info['irmc_deploy_iso'] = 'deploy.iso'
        driver_info_expected = {'irmc_deploy_iso': 'deploy.iso'}

        driver_info_actual = irmc_boot._parse_driver_info(self.node)

        isfile_mock.assert_called_once_with(
            '/remote_image_share_root/deploy.iso')
        self.assertEqual(driver_info_expected, driver_info_actual)

    @mock.patch.object(service_utils, 'is_image_href_ordinary_file_name',
                       spec_set=True, autospec=True)
    def test__parse_driver_info_not_in_share(
            self, is_image_href_ordinary_file_name_mock):
        """With required 'irmc_deploy_iso' not in share."""
        self.node.driver_info[
            'irmc_deploy_iso'] = 'bc784057-a140-4130-add3-ef890457e6b3'
        driver_info_expected = {'irmc_deploy_iso':
                                'bc784057-a140-4130-add3-ef890457e6b3'}
        is_image_href_ordinary_file_name_mock.return_value = False

        driver_info_actual = irmc_boot._parse_driver_info(self.node)

        self.assertEqual(driver_info_expected, driver_info_actual)

    @mock.patch.object(os.path, 'isfile', spec_set=True, autospec=True)
    def test__parse_driver_info_with_deploy_iso_invalid(self, isfile_mock):
        """With required 'irmc_deploy_iso' non existed."""
        isfile_mock.return_value = False

        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.node.driver_info['irmc_deploy_iso'] = 'deploy.iso'
            error_msg = (_("Deploy ISO file, %(deploy_iso)s, "
                           "not found for node: %(node)s.") %
                         {'deploy_iso': '/remote_image_share_root/deploy.iso',
                          'node': task.node.uuid})

            e = self.assertRaises(exception.InvalidParameterValue,
                                  irmc_boot._parse_driver_info,
                                  task.node)
            self.assertEqual(error_msg, str(e))

    def test__parse_driver_info_with_deploy_iso_missing(self):
        """With required 'irmc_deploy_iso' empty."""
        self.node.driver_info['irmc_deploy_iso'] = None

        error_msg = ("Error validating iRMC virtual media deploy. Some"
                     " parameters were missing in node's driver_info."
                     " Missing are: ['irmc_deploy_iso']")
        e = self.assertRaises(exception.MissingParameterValue,
                              irmc_boot._parse_driver_info,
                              self.node)
        self.assertEqual(error_msg, str(e))

    def test__parse_instance_info_with_boot_iso_file_name_ok(self):
        """With optional 'irmc_boot_iso' file name."""
        CONF.irmc.remote_image_share_root = '/etc'
        self.node.instance_info['irmc_boot_iso'] = 'hosts'
        instance_info_expected = {'irmc_boot_iso': 'hosts'}
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    def test__parse_instance_info_without_boot_iso_ok(self):
        """With optional no 'irmc_boot_iso' file name."""
        CONF.irmc.remote_image_share_root = '/etc'

        self.node.instance_info['irmc_boot_iso'] = None
        instance_info_expected = {}
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    def test__parse_instance_info_with_boot_iso_uuid_ok(self):
        """With optional 'irmc_boot_iso' glance uuid."""
        self.node.instance_info[
            'irmc_boot_iso'] = 'bc784057-a140-4130-add3-ef890457e6b3'
        instance_info_expected = {'irmc_boot_iso':
                                  'bc784057-a140-4130-add3-ef890457e6b3'}
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    def test__parse_instance_info_with_boot_iso_glance_ok(self):
        """With optional 'irmc_boot_iso' glance url."""
        self.node.instance_info['irmc_boot_iso'] = (
            'glance://bc784057-a140-4130-add3-ef890457e6b3')
        instance_info_expected = {
            'irmc_boot_iso': 'glance://bc784057-a140-4130-add3-ef890457e6b3',
        }
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    def test__parse_instance_info_with_boot_iso_http_ok(self):
        """With optional 'irmc_boot_iso' http url."""
        self.node.driver_info[
            'irmc_deploy_iso'] = 'http://irmc_boot_iso'
        driver_info_expected = {'irmc_deploy_iso': 'http://irmc_boot_iso'}
        driver_info_actual = irmc_boot._parse_driver_info(self.node)

        self.assertEqual(driver_info_expected, driver_info_actual)

    def test__parse_instance_info_with_boot_iso_https_ok(self):
        """With optional 'irmc_boot_iso' https url."""
        self.node.instance_info[
            'irmc_boot_iso'] = 'https://irmc_boot_iso'
        instance_info_expected = {'irmc_boot_iso': 'https://irmc_boot_iso'}
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    def test__parse_instance_info_with_boot_iso_file_url_ok(self):
        """With optional 'irmc_boot_iso' file url."""
        self.node.instance_info[
            'irmc_boot_iso'] = 'file://irmc_boot_iso'
        instance_info_expected = {'irmc_boot_iso': 'file://irmc_boot_iso'}
        instance_info_actual = irmc_boot._parse_instance_info(self.node)

        self.assertEqual(instance_info_expected, instance_info_actual)

    @mock.patch.object(os.path, 'isfile', spec_set=True, autospec=True)
    def test__parse_instance_info_with_boot_iso_invalid(self, isfile_mock):
        CONF.irmc.remote_image_share_root = '/etc'
        isfile_mock.return_value = False

        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.node.instance_info['irmc_boot_iso'] = 'hosts~non~existed'

            error_msg = (_("Boot ISO file, %(boot_iso)s, "
                           "not found for node: %(node)s.") %
                         {'boot_iso': '/etc/hosts~non~existed',
                          'node': task.node.uuid})

            e = self.assertRaises(exception.InvalidParameterValue,
                                  irmc_boot._parse_instance_info,
                                  task.node)
            self.assertEqual(error_msg, str(e))

    @mock.patch.object(deploy_utils, 'get_image_instance_info',
                       spec_set=True, autospec=True)
    @mock.patch('os.path.isfile', autospec=True)
    def test_parse_deploy_info_ok(self, mock_isfile,
                                  get_image_instance_info_mock):
        CONF.irmc.remote_image_share_root = '/etc'
        get_image_instance_info_mock.return_value = {'a': 'b'}
        driver_info_expected = {'a': 'b',
                                'irmc_deploy_iso': 'hosts',
                                'irmc_boot_iso': 'fstab'}

        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.node.driver_info['irmc_deploy_iso'] = 'hosts'
            task.node.instance_info['irmc_boot_iso'] = 'fstab'
            driver_info_actual = irmc_boot._parse_deploy_info(task.node)
            self.assertEqual(driver_info_expected, driver_info_actual)
            boot_iso_path = os.path.join(
                CONF.irmc.remote_image_share_root,
                task.node.instance_info['irmc_boot_iso']
            )
            mock_isfile.assert_any_call(boot_iso_path)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_vmedia_for_boot', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'fetch', spec_set=True,
                       autospec=True)
    def test__setup_deploy_iso_with_file(self,
                                         fetch_mock,
                                         setup_vmedia_mock,
                                         set_boot_device_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.driver_info['irmc_deploy_iso'] = 'deploy_iso_filename'
            ramdisk_opts = {'a': 'b'}
            irmc_boot._setup_deploy_iso(task, ramdisk_opts)

            self.assertFalse(fetch_mock.called)

            setup_vmedia_mock.assert_called_once_with(
                task,
                'deploy_iso_filename',
                ramdisk_opts)
            set_boot_device_mock.assert_called_once_with(task,
                                                         boot_devices.CDROM)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_vmedia_for_boot', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'fetch', spec_set=True,
                       autospec=True)
    def test_setup_deploy_iso_with_image_service(
            self,
            fetch_mock,
            setup_vmedia_mock,
            set_boot_device_mock):
        CONF.irmc.remote_image_share_root = '/'

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.driver_info['irmc_deploy_iso'] = 'glance://deploy_iso'
            ramdisk_opts = {'a': 'b'}
            irmc_boot._setup_deploy_iso(task, ramdisk_opts)

            fetch_mock.assert_called_once_with(
                task.context,
                'glance://deploy_iso',
                "/deploy-%s.iso" % self.node.uuid)

            setup_vmedia_mock.assert_called_once_with(
                task,
                "deploy-%s.iso" % self.node.uuid,
                ramdisk_opts)
            set_boot_device_mock.assert_called_once_with(
                task, boot_devices.CDROM)

    def test__get_deploy_iso_name(self):
        actual = irmc_boot._get_deploy_iso_name(self.node)
        expected = "deploy-%s.iso" % self.node.uuid
        self.assertEqual(expected, actual)

    def test__get_boot_iso_name(self):
        actual = irmc_boot._get_boot_iso_name(self.node)
        expected = "boot-%s.iso" % self.node.uuid
        self.assertEqual(expected, actual)

    @mock.patch.object(images, 'create_boot_iso', spec_set=True, autospec=True)
    @mock.patch.object(deploy_utils, 'get_boot_mode_for_deploy', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'get_image_properties', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'fetch', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    def test__prepare_boot_iso_file(self,
                                    deploy_info_mock,
                                    fetch_mock,
                                    image_props_mock,
                                    boot_mode_mock,
                                    create_boot_iso_mock):
        deploy_info_mock.return_value = {'irmc_boot_iso': 'irmc_boot.iso'}
        with task_manager.acquire(self.context, self.node.uuid) as task:
            irmc_boot._prepare_boot_iso(task, 'root-uuid')

            deploy_info_mock.assert_called_once_with(task.node)
            self.assertFalse(fetch_mock.called)
            self.assertFalse(image_props_mock.called)
            self.assertFalse(boot_mode_mock.called)
            self.assertFalse(create_boot_iso_mock.called)
            task.node.refresh()
            self.assertEqual('irmc_boot.iso',
                             task.node.driver_internal_info['irmc_boot_iso'])

    @mock.patch.object(images, 'create_boot_iso', spec_set=True, autospec=True)
    @mock.patch.object(deploy_utils, 'get_boot_mode_for_deploy', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'get_image_properties', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'fetch', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    @mock.patch.object(service_utils, 'is_image_href_ordinary_file_name',
                       spec_set=True, autospec=True)
    def test__prepare_boot_iso_fetch_ok(self,
                                        is_image_href_ordinary_file_name_mock,
                                        deploy_info_mock,
                                        fetch_mock,
                                        image_props_mock,
                                        boot_mode_mock,
                                        create_boot_iso_mock):

        CONF.irmc.remote_image_share_root = '/'
        image = '733d1c44-a2ea-414b-aca7-69decf20d810'
        is_image_href_ordinary_file_name_mock.return_value = False
        deploy_info_mock.return_value = {'irmc_boot_iso': image}

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.instance_info['irmc_boot_iso'] = image
            irmc_boot._prepare_boot_iso(task, 'root-uuid')

            deploy_info_mock.assert_called_once_with(task.node)
            fetch_mock.assert_called_once_with(
                task.context,
                image,
                "/boot-%s.iso" % self.node.uuid)
            self.assertFalse(image_props_mock.called)
            self.assertFalse(boot_mode_mock.called)
            self.assertFalse(create_boot_iso_mock.called)
            task.node.refresh()
            self.assertEqual("boot-%s.iso" % self.node.uuid,
                             task.node.driver_internal_info['irmc_boot_iso'])

    @mock.patch.object(images, 'create_boot_iso', spec_set=True, autospec=True)
    @mock.patch.object(deploy_utils, 'get_boot_mode_for_deploy', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'get_image_properties', spec_set=True,
                       autospec=True)
    @mock.patch.object(images, 'fetch', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    def test__prepare_boot_iso_create_ok(self,
                                         deploy_info_mock,
                                         fetch_mock,
                                         image_props_mock,
                                         boot_mode_mock,
                                         create_boot_iso_mock):
        CONF.pxe.pxe_append_params = 'kernel-params'

        deploy_info_mock.return_value = {'image_source': 'image-uuid'}
        image_props_mock.return_value = {'kernel_id': 'kernel_uuid',
                                         'ramdisk_id': 'ramdisk_uuid'}

        CONF.irmc.remote_image_share_name = '/remote_image_share_root'
        boot_mode_mock.return_value = 'uefi'

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._prepare_boot_iso(task, 'root-uuid')

            self.assertFalse(fetch_mock.called)
            deploy_info_mock.assert_called_once_with(task.node)
            image_props_mock.assert_called_once_with(
                task.context, 'image-uuid', ['kernel_id', 'ramdisk_id'])
            create_boot_iso_mock.assert_called_once_with(
                task.context,
                '/remote_image_share_root/' +
                "boot-%s.iso" % self.node.uuid,
                'kernel_uuid', 'ramdisk_uuid',
                'file:///remote_image_share_root/' +
                "deploy-%s.iso" % self.node.uuid,
                'root-uuid', 'kernel-params', 'uefi')
            task.node.refresh()
            self.assertEqual("boot-%s.iso" % self.node.uuid,
                             task.node.driver_internal_info['irmc_boot_iso'])

    def test__get_floppy_image_name(self):
        actual = irmc_boot._get_floppy_image_name(self.node)
        expected = "image-%s.img" % self.node.uuid
        self.assertEqual(expected, actual)

    @mock.patch.object(shutil, 'copyfile', spec_set=True, autospec=True)
    @mock.patch.object(images, 'create_vfat_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(tempfile, 'NamedTemporaryFile', spec_set=True,
                       autospec=True)
    def test__prepare_floppy_image(self,
                                   tempfile_mock,
                                   create_vfat_image_mock,
                                   copyfile_mock):
        mock_image_file_handle = mock.MagicMock(spec=file)
        mock_image_file_obj = mock.MagicMock()
        mock_image_file_obj.name = 'image-tmp-file'
        mock_image_file_handle.__enter__.return_value = mock_image_file_obj
        tempfile_mock.side_effect = [mock_image_file_handle]

        deploy_args = {'arg1': 'val1', 'arg2': 'val2'}
        CONF.irmc.remote_image_share_name = '/remote_image_share_root'

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._prepare_floppy_image(task, deploy_args)

        create_vfat_image_mock.assert_called_once_with(
            'image-tmp-file', parameters=deploy_args)
        copyfile_mock.assert_called_once_with(
            'image-tmp-file',
            '/remote_image_share_root/' + "image-%s.img" % self.node.uuid)

    @mock.patch.object(shutil, 'copyfile', spec_set=True, autospec=True)
    @mock.patch.object(images, 'create_vfat_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(tempfile, 'NamedTemporaryFile', spec_set=True,
                       autospec=True)
    def test__prepare_floppy_image_exception(self,
                                             tempfile_mock,
                                             create_vfat_image_mock,
                                             copyfile_mock):
        mock_image_file_handle = mock.MagicMock(spec=file)
        mock_image_file_obj = mock.MagicMock()
        mock_image_file_obj.name = 'image-tmp-file'
        mock_image_file_handle.__enter__.return_value = mock_image_file_obj
        tempfile_mock.side_effect = [mock_image_file_handle]

        deploy_args = {'arg1': 'val1', 'arg2': 'val2'}
        CONF.irmc.remote_image_share_name = '/remote_image_share_root'
        copyfile_mock.side_effect = IOError("fake error")

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            self.assertRaises(exception.IRMCOperationError,
                              irmc_boot._prepare_floppy_image,
                              task,
                              deploy_args)

        create_vfat_image_mock.assert_called_once_with(
            'image-tmp-file', parameters=deploy_args)
        copyfile_mock.assert_called_once_with(
            'image-tmp-file',
            '/remote_image_share_root/' + "image-%s.img" % self.node.uuid)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_vmedia_for_boot', spec_set=True,
                       autospec=True)
    def test_attach_boot_iso_if_needed(
            self,
            setup_vmedia_mock,
            set_boot_device_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=True) as task:
            task.node.provision_state = states.ACTIVE
            task.node.driver_internal_info['irmc_boot_iso'] = 'boot-iso'
            irmc_boot.attach_boot_iso_if_needed(task)
            setup_vmedia_mock.assert_called_once_with(task, 'boot-iso')
            set_boot_device_mock.assert_called_once_with(
                task, boot_devices.CDROM)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_vmedia_for_boot', spec_set=True,
                       autospec=True)
    def test_attach_boot_iso_if_needed_on_rebuild(
            self,
            setup_vmedia_mock,
            set_boot_device_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=True) as task:
            task.node.provision_state = states.DEPLOYING
            task.node.driver_internal_info['irmc_boot_iso'] = 'boot-iso'
            irmc_boot.attach_boot_iso_if_needed(task)
            self.assertFalse(setup_vmedia_mock.called)
            self.assertFalse(set_boot_device_mock.called)

    @mock.patch.object(irmc_boot, '_attach_virtual_cd', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_attach_virtual_fd', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_prepare_floppy_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_fd', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_cd', spec_set=True,
                       autospec=True)
    def test__setup_vmedia_for_boot_with_parameters(self,
                                                    _detach_virtual_cd_mock,
                                                    _detach_virtual_fd_mock,
                                                    _prepare_floppy_image_mock,
                                                    _attach_virtual_fd_mock,
                                                    _attach_virtual_cd_mock):
        parameters = {'a': 'b'}
        iso_filename = 'deploy_iso_or_boot_iso'
        _prepare_floppy_image_mock.return_value = 'floppy_file_name'

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._setup_vmedia_for_boot(task, iso_filename, parameters)

            _detach_virtual_cd_mock.assert_called_once_with(task.node)
            _detach_virtual_fd_mock.assert_called_once_with(task.node)
            _prepare_floppy_image_mock.assert_called_once_with(task,
                                                               parameters)
            _attach_virtual_fd_mock.assert_called_once_with(task.node,
                                                            'floppy_file_name')
            _attach_virtual_cd_mock.assert_called_once_with(task.node,
                                                            iso_filename)

    @mock.patch.object(irmc_boot, '_attach_virtual_cd', autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_fd', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_cd', spec_set=True,
                       autospec=True)
    def test__setup_vmedia_for_boot_without_parameters(
            self,
            _detach_virtual_cd_mock,
            _detach_virtual_fd_mock,
            _attach_virtual_cd_mock):

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._setup_vmedia_for_boot(task, 'bootable_iso_filename')

            _detach_virtual_cd_mock.assert_called_once_with(task.node)
            _detach_virtual_fd_mock.assert_called_once_with(task.node)
            _attach_virtual_cd_mock.assert_called_once_with(
                task.node,
                'bootable_iso_filename')

    @mock.patch.object(irmc_boot, '_get_deploy_iso_name', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_get_floppy_image_name', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_remove_share_file', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_fd', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_detach_virtual_cd', spec_set=True,
                       autospec=True)
    def test__cleanup_vmedia_boot_ok(self,
                                     _detach_virtual_cd_mock,
                                     _detach_virtual_fd_mock,
                                     _remove_share_file_mock,
                                     _get_floppy_image_name_mock,
                                     _get_deploy_iso_name_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._cleanup_vmedia_boot(task)

            _detach_virtual_cd_mock.assert_called_once_with(task.node)
            _detach_virtual_fd_mock.assert_called_once_with(task.node)
            _get_floppy_image_name_mock.assert_called_once_with(task.node)
            _get_deploy_iso_name_mock.assert_called_once_with(task.node)
            self.assertTrue(_remove_share_file_mock.call_count, 2)
            _remove_share_file_mock.assert_has_calls(
                [mock.call(_get_floppy_image_name_mock(task.node)),
                 mock.call(_get_deploy_iso_name_mock(task.node))])

    @mock.patch.object(ironic_utils, 'unlink_without_raise', spec_set=True,
                       autospec=True)
    def test__remove_share_file(self, unlink_without_raise_mock):
        CONF.irmc.remote_image_share_name = '/'

        irmc_boot._remove_share_file("boot.iso")

        unlink_without_raise_mock.assert_called_once_with('/boot.iso')

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__attach_virtual_cd_ok(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_boot.scci.get_virtual_cd_set_params_cmd = (
            mock.MagicMock(sepc_set=[]))
        cd_set_params = (irmc_boot.scci
                         .get_virtual_cd_set_params_cmd.return_value)

        CONF.irmc.remote_image_server = '10.20.30.40'
        CONF.irmc.remote_image_user_domain = 'local'
        CONF.irmc.remote_image_share_type = 'NFS'
        CONF.irmc.remote_image_share_name = 'share'
        CONF.irmc.remote_image_user_name = 'admin'
        CONF.irmc.remote_image_user_password = 'admin0'

        irmc_boot.scci.get_share_type.return_value = 0

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._attach_virtual_cd(task.node, 'iso_filename')

            get_irmc_client_mock.assert_called_once_with(task.node)
            (irmc_boot.scci.get_virtual_cd_set_params_cmd
             .assert_called_once_with)('10.20.30.40',
                                       'local',
                                       0,
                                       'share',
                                       'iso_filename',
                                       'admin',
                                       'admin0')
            irmc_client.assert_has_calls(
                [mock.call(cd_set_params, async=False),
                 mock.call(irmc_boot.scci.MOUNT_CD, async=False)])

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__attach_virtual_cd_fail(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_client.side_effect = Exception("fake error")
        irmc_boot.scci.SCCIClientError = Exception

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            e = self.assertRaises(exception.IRMCOperationError,
                                  irmc_boot._attach_virtual_cd,
                                  task.node,
                                  'iso_filename')
            get_irmc_client_mock.assert_called_once_with(task.node)
            self.assertEqual("iRMC Inserting virtual cdrom failed. " +
                             "Reason: fake error", str(e))

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__detach_virtual_cd_ok(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._detach_virtual_cd(task.node)

            irmc_client.assert_called_once_with(irmc_boot.scci.UNMOUNT_CD)

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__detach_virtual_cd_fail(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_client.side_effect = Exception("fake error")
        irmc_boot.scci.SCCIClientError = Exception

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            e = self.assertRaises(exception.IRMCOperationError,
                                  irmc_boot._detach_virtual_cd,
                                  task.node)
            self.assertEqual("iRMC Ejecting virtual cdrom failed. " +
                             "Reason: fake error", str(e))

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__attach_virtual_fd_ok(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_boot.scci.get_virtual_fd_set_params_cmd = (
            mock.MagicMock(sepc_set=[]))
        fd_set_params = (irmc_boot.scci
                         .get_virtual_fd_set_params_cmd.return_value)

        CONF.irmc.remote_image_server = '10.20.30.40'
        CONF.irmc.remote_image_user_domain = 'local'
        CONF.irmc.remote_image_share_type = 'NFS'
        CONF.irmc.remote_image_share_name = 'share'
        CONF.irmc.remote_image_user_name = 'admin'
        CONF.irmc.remote_image_user_password = 'admin0'

        irmc_boot.scci.get_share_type.return_value = 0

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._attach_virtual_fd(task.node,
                                         'floppy_image_filename')

            get_irmc_client_mock.assert_called_once_with(task.node)
            (irmc_boot.scci.get_virtual_fd_set_params_cmd
             .assert_called_once_with)('10.20.30.40',
                                       'local',
                                       0,
                                       'share',
                                       'floppy_image_filename',
                                       'admin',
                                       'admin0')
            irmc_client.assert_has_calls(
                [mock.call(fd_set_params, async=False),
                 mock.call(irmc_boot.scci.MOUNT_FD, async=False)])

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__attach_virtual_fd_fail(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_client.side_effect = Exception("fake error")
        irmc_boot.scci.SCCIClientError = Exception

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            e = self.assertRaises(exception.IRMCOperationError,
                                  irmc_boot._attach_virtual_fd,
                                  task.node,
                                  'iso_filename')
            get_irmc_client_mock.assert_called_once_with(task.node)
            self.assertEqual("iRMC Inserting virtual floppy failed. " +
                             "Reason: fake error", str(e))

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__detach_virtual_fd_ok(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            irmc_boot._detach_virtual_fd(task.node)

            irmc_client.assert_called_once_with(irmc_boot.scci.UNMOUNT_FD)

    @mock.patch.object(irmc_common, 'get_irmc_client', spec_set=True,
                       autospec=True)
    def test__detach_virtual_fd_fail(self, get_irmc_client_mock):
        irmc_client = get_irmc_client_mock.return_value
        irmc_client.side_effect = Exception("fake error")
        irmc_boot.scci.SCCIClientError = Exception

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            e = self.assertRaises(exception.IRMCOperationError,
                                  irmc_boot._detach_virtual_fd,
                                  task.node)
            self.assertEqual("iRMC Ejecting virtual floppy failed. "
                             "Reason: fake error", str(e))

    @mock.patch.object(irmc_boot, '_parse_config_option', spec_set=True,
                       autospec=True)
    def test_check_share_fs_mounted_ok(self, parse_conf_mock):
        # Note(naohirot): mock.patch.stop() and mock.patch.start() don't work.
        # therefor monkey patching is used to
        # irmc_boot.check_share_fs_mounted.
        # irmc_boot.check_share_fs_mounted is mocked in
        # third_party_driver_mocks.py.
        # irmc_boot.check_share_fs_mounted_orig is the real function.
        CONF.irmc.remote_image_share_root = '/'
        CONF.irmc.remote_image_share_type = 'nfs'
        result = irmc_boot.check_share_fs_mounted_orig()

        parse_conf_mock.assert_called_once_with()
        self.assertIsNone(result)

    @mock.patch.object(irmc_boot, '_parse_config_option', spec_set=True,
                       autospec=True)
    def test_check_share_fs_mounted_exception(self, parse_conf_mock):
        # Note(naohirot): mock.patch.stop() and mock.patch.start() don't work.
        # therefor monkey patching is used to
        # irmc_boot.check_share_fs_mounted.
        # irmc_boot.check_share_fs_mounted is mocked in
        # third_party_driver_mocks.py.
        # irmc_boot.check_share_fs_mounted_orig is the real function.
        CONF.irmc.remote_image_share_root = '/etc'
        CONF.irmc.remote_image_share_type = 'cifs'

        self.assertRaises(exception.IRMCSharedFileSystemNotMounted,
                          irmc_boot.check_share_fs_mounted_orig)
        parse_conf_mock.assert_called_once_with()


class IRMCVirtualMediaBootTestCase(db_base.DbTestCase):

    def setUp(self):
        irmc_boot.check_share_fs_mounted_patcher.start()
        self.addCleanup(irmc_boot.check_share_fs_mounted_patcher.stop)
        super(IRMCVirtualMediaBootTestCase, self).setUp()
        mgr_utils.mock_the_extension_manager(driver="iscsi_irmc")
        self.node = obj_utils.create_test_node(
            self.context, driver='iscsi_irmc', driver_info=INFO_DICT)

    @mock.patch.object(deploy_utils, 'validate_image_properties',
                       spec_set=True, autospec=True)
    @mock.patch.object(service_utils, 'is_glance_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, 'check_share_fs_mounted', spec_set=True,
                       autospec=True)
    def test_validate_whole_disk_image(self,
                                       check_share_fs_mounted_mock,
                                       deploy_info_mock,
                                       is_glance_image_mock,
                                       validate_prop_mock):
        d_info = {'image_source': '733d1c44-a2ea-414b-aca7-69decf20d810'}
        deploy_info_mock.return_value = d_info
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.driver_internal_info = {'is_whole_disk_image': True}
            task.driver.boot.validate(task)

            check_share_fs_mounted_mock.assert_called_once_with()
            deploy_info_mock.assert_called_once_with(task.node)
            self.assertFalse(is_glance_image_mock.called)
            validate_prop_mock.assert_called_once_with(task.context,
                                                       d_info, [])

    @mock.patch.object(deploy_utils, 'validate_image_properties',
                       spec_set=True, autospec=True)
    @mock.patch.object(service_utils, 'is_glance_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, 'check_share_fs_mounted', spec_set=True,
                       autospec=True)
    def test_validate_glance_image(self,
                                   check_share_fs_mounted_mock,
                                   deploy_info_mock,
                                   is_glance_image_mock,
                                   validate_prop_mock):
        d_info = {'image_source': '733d1c44-a2ea-414b-aca7-69decf20d810'}
        deploy_info_mock.return_value = d_info
        is_glance_image_mock.return_value = True
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.validate(task)

            check_share_fs_mounted_mock.assert_called_once_with()
            deploy_info_mock.assert_called_once_with(task.node)
            validate_prop_mock.assert_called_once_with(
                task.context, d_info, ['kernel_id', 'ramdisk_id'])

    @mock.patch.object(deploy_utils, 'validate_image_properties',
                       spec_set=True, autospec=True)
    @mock.patch.object(service_utils, 'is_glance_image', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_parse_deploy_info', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, 'check_share_fs_mounted', spec_set=True,
                       autospec=True)
    def test_validate_non_glance_image(self,
                                       check_share_fs_mounted_mock,
                                       deploy_info_mock,
                                       is_glance_image_mock,
                                       validate_prop_mock):
        d_info = {'image_source': '733d1c44-a2ea-414b-aca7-69decf20d810'}
        deploy_info_mock.return_value = d_info
        is_glance_image_mock.return_value = False
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.validate(task)

            check_share_fs_mounted_mock.assert_called_once_with()
            deploy_info_mock.assert_called_once_with(task.node)
            validate_prop_mock.assert_called_once_with(
                task.context, d_info, ['kernel', 'ramdisk'])

    @mock.patch.object(irmc_management, 'backup_bios_config', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_deploy_iso',
                       spec_set=True, autospec=True)
    @mock.patch.object(deploy_utils, 'get_single_nic_with_vif_port_id',
                       spec_set=True, autospec=True)
    def _test_prepare_ramdisk(self,
                              get_single_nic_with_vif_port_id_mock,
                              _setup_deploy_iso_mock,
                              mock_backup_bios):
        instance_info = self.node.instance_info
        instance_info['irmc_boot_iso'] = 'glance://abcdef'
        instance_info['image_source'] = '6b2f0c0c-79e8-4db6-842e-43c9764204af'
        self.node.instance_info = instance_info
        self.node.save()

        ramdisk_params = {'a': 'b'}
        get_single_nic_with_vif_port_id_mock.return_value = '12:34:56:78:90:ab'
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_ramdisk(task, ramdisk_params)

            expected_ramdisk_opts = {'a': 'b', 'BOOTIF': '12:34:56:78:90:ab'}
            get_single_nic_with_vif_port_id_mock.assert_called_once_with(
                task)
            _setup_deploy_iso_mock.assert_called_once_with(
                task, expected_ramdisk_opts)
            self.assertEqual('glance://abcdef',
                             self.node.instance_info['irmc_boot_iso'])
            provision_state = task.node.provision_state
            self.assertEqual(1 if provision_state == states.DEPLOYING else 0,
                             mock_backup_bios.call_count)

    def test_prepare_ramdisk_glance_image_deploying(self):
        self.node.provision_state = states.DEPLOYING
        self.node.save()
        self._test_prepare_ramdisk()

    def test_prepare_ramdisk_glance_image_cleaning(self):
        self.node.provision_state = states.CLEANING
        self.node.save()
        self._test_prepare_ramdisk()

    @mock.patch.object(irmc_boot, '_setup_deploy_iso', spec_set=True,
                       autospec=True)
    def test_prepare_ramdisk_not_deploying_not_cleaning(self, mock_is_image):
        """Ensure deploy ops are blocked when not deploying and not cleaning"""

        for state in states.STABLE_STATES:
            mock_is_image.reset_mock()
            self.node.provision_state = state
            self.node.save()
            with task_manager.acquire(self.context, self.node.uuid,
                                      shared=False) as task:
                self.assertIsNone(
                    task.driver.boot.prepare_ramdisk(task, None))
                self.assertFalse(mock_is_image.called)

    @mock.patch.object(irmc_boot, '_cleanup_vmedia_boot', spec_set=True,
                       autospec=True)
    def test_clean_up_ramdisk(self, _cleanup_vmedia_boot_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.clean_up_ramdisk(task)
            _cleanup_vmedia_boot_mock.assert_called_once_with(task)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_cleanup_vmedia_boot', spec_set=True,
                       autospec=True)
    def _test_prepare_instance_whole_disk_image(
            self, _cleanup_vmedia_boot_mock, set_boot_device_mock):
        self.node.driver_internal_info = {'is_whole_disk_image': True}
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_instance(task)

            _cleanup_vmedia_boot_mock.assert_called_once_with(task)
            set_boot_device_mock.assert_called_once_with(task,
                                                         boot_devices.DISK,
                                                         persistent=True)

    def test_prepare_instance_whole_disk_image_local(self):
        self.node.instance_info = {'capabilities': '{"boot_option": "local"}'}
        self.node.save()
        self._test_prepare_instance_whole_disk_image()

    def test_prepare_instance_whole_disk_image(self):
        self._test_prepare_instance_whole_disk_image()

    @mock.patch.object(irmc_boot.IRMCVirtualMediaBoot,
                       '_configure_vmedia_boot', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_cleanup_vmedia_boot', spec_set=True,
                       autospec=True)
    def test_prepare_instance_partition_image(
            self, _cleanup_vmedia_boot_mock, _configure_vmedia_mock):
        self.node.driver_internal_info = {'root_uuid_or_disk_id': "some_uuid"}
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_instance(task)

            _cleanup_vmedia_boot_mock.assert_called_once_with(task)
            _configure_vmedia_mock.assert_called_once_with(mock.ANY, task,
                                                           "some_uuid")

    @mock.patch.object(irmc_boot, '_cleanup_vmedia_boot', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_remove_share_file', spec_set=True,
                       autospec=True)
    def test_clean_up_instance(self, _remove_share_file_mock,
                               _cleanup_vmedia_boot_mock):
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.instance_info['irmc_boot_iso'] = 'glance://deploy_iso'
            task.node.driver_internal_info['irmc_boot_iso'] = 'irmc_boot.iso'
            task.node.driver_internal_info = {'root_uuid_or_disk_id': (
                "12312642-09d3-467f-8e09-12385826a123")}

            task.driver.boot.clean_up_instance(task)

            _remove_share_file_mock.assert_called_once_with(
                irmc_boot._get_boot_iso_name(task.node))
            self.assertNotIn('irmc_boot_iso',
                             task.node.driver_internal_info)
            self.assertNotIn('root_uuid_or_disk_id',
                             task.node.driver_internal_info)
            _cleanup_vmedia_boot_mock.assert_called_once_with(task)

    @mock.patch.object(manager_utils, 'node_set_boot_device', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_setup_vmedia_for_boot', spec_set=True,
                       autospec=True)
    @mock.patch.object(irmc_boot, '_prepare_boot_iso', spec_set=True,
                       autospec=True)
    def test__configure_vmedia_boot(self,
                                    _prepare_boot_iso_mock,
                                    _setup_vmedia_for_boot_mock,
                                    node_set_boot_device):
        root_uuid_or_disk_id = {'root uuid': 'root_uuid'}

        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.node.driver_internal_info['irmc_boot_iso'] = 'boot.iso'
            task.driver.boot._configure_vmedia_boot(
                task, root_uuid_or_disk_id)

            _prepare_boot_iso_mock.assert_called_once_with(
                task, root_uuid_or_disk_id)
            _setup_vmedia_for_boot_mock.assert_called_once_with(
                task, 'boot.iso')
            node_set_boot_device.assert_called_once_with(
                task, boot_devices.CDROM, persistent=True)

    def test_remote_image_share_type_values(self):
        cfg.CONF.set_override('remote_image_share_type', 'cifs', 'irmc')
        cfg.CONF.set_override('remote_image_share_type', 'nfs', 'irmc')
        self.assertRaises(ValueError, cfg.CONF.set_override,
                          'remote_image_share_type', 'fake', 'irmc')


class IRMCPXEBootTestCase(db_base.DbTestCase):

    def setUp(self):
        super(IRMCPXEBootTestCase, self).setUp()
        mgr_utils.mock_the_extension_manager(driver="pxe_irmc")
        self.node = obj_utils.create_test_node(
            self.context, driver='pxe_irmc', driver_info=INFO_DICT)

    @mock.patch.object(irmc_management, 'backup_bios_config', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'prepare_ramdisk', spec_set=True,
                       autospec=True)
    def test_prepare_ramdisk_with_backup_bios(self, mock_parent_prepare,
                                              mock_backup_bios):
        self.node.provision_state = states.DEPLOYING
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_ramdisk(task, {})
            mock_backup_bios.assert_called_once_with(task)
            mock_parent_prepare.assert_called_once_with(
                task.driver.boot, task, {})

    @mock.patch.object(irmc_management, 'backup_bios_config', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'prepare_ramdisk', spec_set=True,
                       autospec=True)
    def test_prepare_ramdisk_without_backup_bios(self, mock_parent_prepare,
                                                 mock_backup_bios):
        self.node.provision_state = states.CLEANING
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_ramdisk(task, {})
            self.assertFalse(mock_backup_bios.called)
            mock_parent_prepare.assert_called_once_with(
                task.driver.boot, task, {})

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'prepare_instance', spec_set=True,
                       autospec=True)
    def test_prepare_instance_with_secure_boot(self, mock_prepare_instance,
                                               mock_set_secure_boot_mode):
        self.node.provision_state = states.DEPLOYING
        self.node.target_provision_state = states.ACTIVE
        self.node.instance_info = {
            'capabilities': {
                "secure_boot": "true"
            }
        }
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_instance(task)
            mock_set_secure_boot_mode.assert_called_once_with(task.node,
                                                              enable=True)
            mock_prepare_instance.assert_called_once_with(
                task.driver.boot, task)

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'prepare_instance', spec_set=True,
                       autospec=True)
    def test_prepare_instance_with_secure_boot_false(
            self, mock_prepare_instance, mock_set_secure_boot_mode):
        self.node.provision_state = states.DEPLOYING
        self.node.target_provision_state = states.ACTIVE
        self.node.instance_info = {
            'capabilities': {
                "secure_boot": "false"
            }
        }
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_instance(task)
            self.assertFalse(mock_set_secure_boot_mode.called)
            mock_prepare_instance.assert_called_once_with(
                task.driver.boot, task)

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'prepare_instance', spec_set=True,
                       autospec=True)
    def test_prepare_instance_without_secure_boot(self, mock_prepare_instance,
                                                  mock_set_secure_boot_mode):
        self.node.provision_state = states.DEPLOYING
        self.node.target_provision_state = states.ACTIVE
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.prepare_instance(task)
            self.assertFalse(mock_set_secure_boot_mode.called)
            mock_prepare_instance.assert_called_once_with(
                task.driver.boot, task)

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'clean_up_instance', spec_set=True,
                       autospec=True)
    def test_clean_up_instance_with_secure_boot(self, mock_clean_up_instance,
                                                mock_set_secure_boot_mode):
        self.node.provision_state = states.CLEANING
        self.node.target_provision_state = states.AVAILABLE
        self.node.instance_info = {
            'capabilities': {
                "secure_boot": "true"
            }
        }
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.clean_up_instance(task)
            mock_set_secure_boot_mode.assert_called_once_with(task.node,
                                                              enable=False)
            mock_clean_up_instance.assert_called_once_with(
                task.driver.boot, task)

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'clean_up_instance', spec_set=True,
                       autospec=True)
    def test_clean_up_instance_secure_boot_false(self, mock_clean_up_instance,
                                                 mock_set_secure_boot_mode):
        self.node.provision_state = states.CLEANING
        self.node.target_provision_state = states.AVAILABLE
        self.node.instance_info = {
            'capabilities': {
                "secure_boot": "false"
            }
        }
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.clean_up_instance(task)
            self.assertFalse(mock_set_secure_boot_mode.called)
            mock_clean_up_instance.assert_called_once_with(
                task.driver.boot, task)

    @mock.patch.object(irmc_common, 'set_secure_boot_mode', spec_set=True,
                       autospec=True)
    @mock.patch.object(pxe.PXEBoot, 'clean_up_instance', spec_set=True,
                       autospec=True)
    def test_clean_up_instance_without_secure_boot(
            self, mock_clean_up_instance, mock_set_secure_boot_mode):
        self.node.provision_state = states.CLEANING
        self.node.target_provision_state = states.AVAILABLE
        self.node.save()
        with task_manager.acquire(self.context, self.node.uuid,
                                  shared=False) as task:
            task.driver.boot.clean_up_instance(task)
            self.assertFalse(mock_set_secure_boot_mode.called)
            mock_clean_up_instance.assert_called_once_with(
                task.driver.boot, task)


@mock.patch.object(irmc_boot, 'viom',
                   spec_set=mock_specs.SCCICLIENT_VIOM_SPEC)
class IRMCVirtualMediaBootWithVolumeTestCase(db_base.DbTestCase):

    def setUp(self):
        super(IRMCVirtualMediaBootWithVolumeTestCase, self).setUp()
        irmc_boot.check_share_fs_mounted_patcher.start()
        self.addCleanup(irmc_boot.check_share_fs_mounted_patcher.stop)
        self.config(enabled_hardware_types=['irmc'],
                    enabled_boot_interfaces=['irmc-virtual-media'],
                    enabled_deploy_interfaces=['direct'],
                    enabled_power_interfaces=['irmc'],
                    enabled_management_interfaces=['irmc'],
                    enabled_storage_interfaces=['cinder'])
        driver_info = INFO_DICT
        d_in_info = dict(boot_from_volume='volume-uuid')
        self.node = obj_utils.create_test_node(self.context,
                                               driver='irmc',
                                               driver_info=driver_info,
                                               storage_interface='cinder',
                                               driver_internal_info=d_in_info)

    def _create_mock_conf(self, mock_viom):
        mock_conf = mock.Mock(spec_set=mock_specs.SCCICLIENT_VIOM_CONF_SPEC)
        mock_viom.VIOMConfiguration.return_value = mock_conf
        return mock_conf

    def _add_pci_physical_id(self, uuid, physical_id):
        driver_info = self.node.driver_info
        ids = driver_info.get('irmc_pci_physical_ids', {})
        ids[uuid] = physical_id
        driver_info['irmc_pci_physical_ids'] = ids
        self.node.driver_info = driver_info
        self.node.save()

    def _create_port(self, physical_id='LAN0-1', **kwargs):
        uuid = uuidutils.generate_uuid()
        obj_utils.create_test_port(self.context,
                                   uuid=uuid,
                                   node_id=self.node.id,
                                   **kwargs)
        if physical_id:
            self._add_pci_physical_id(uuid, physical_id)

    def _create_iscsi_iqn_connector(self, physical_id='CNA1-1'):
        uuid = uuidutils.generate_uuid()
        obj_utils.create_test_volume_connector(
            self.context,
            uuid=uuid,
            type='iqn',
            node_id=self.node.id,
            connector_id='iqn.initiator')
        if physical_id:
            self._add_pci_physical_id(uuid, physical_id)

    def _create_iscsi_ip_connector(self, physical_id=None, network_size='24'):
        uuid = uuidutils.generate_uuid()
        obj_utils.create_test_volume_connector(
            self.context,
            uuid=uuid,
            type='ip',
            node_id=self.node.id,
            connector_id='192.168.11.11')
        if physical_id:
            self._add_pci_physical_id(uuid, physical_id)
        if network_size:
            driver_info = self.node.driver_info
            driver_info['irmc_storage_network_size'] = network_size
            self.node.driver_info = driver_info
            self.node.save()

    def _create_iscsi_target(self, target_info=None, boot_index=0, **kwargs):
        target_properties = {
            'target_portal': '192.168.22.22:3260',
            'target_iqn': 'iqn.target',
            'target_lun': 1,
        }
        if target_info:
            target_properties.update(target_info)
        obj_utils.create_test_volume_target(
            self.context,
            volume_type='iscsi',
            node_id=self.node.id,
            boot_index=boot_index,
            properties=target_properties,
            **kwargs)

    def _create_iscsi_resources(self):
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector()
        self._create_iscsi_target()

    def _create_fc_connector(self):
        uuid = uuidutils.generate_uuid()
        obj_utils.create_test_volume_connector(
            self.context,
            uuid=uuid,
            type='wwnn',
            node_id=self.node.id,
            connector_id='11:22:33:44:55')
        self._add_pci_physical_id(uuid, 'FC2-1')
        obj_utils.create_test_volume_connector(
            self.context,
            uuid=uuidutils.generate_uuid(),
            type='wwpn',
            node_id=self.node.id,
            connector_id='11:22:33:44:56')

    def _create_fc_target(self):
        target_properties = {
            'target_wwn': 'aa:bb:cc:dd:ee',
            'target_lun': 2,
        }
        obj_utils.create_test_volume_target(
            self.context,
            volume_type='fibre_channel',
            node_id=self.node.id,
            boot_index=0,
            properties=target_properties)

    def _create_fc_resources(self):
        self._create_fc_connector()
        self._create_fc_target()

    def _call_validate(self):
        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.driver.boot.validate(task)

    def test_validate_iscsi(self, mock_viom):
        self._create_port()
        self._create_iscsi_resources()
        self._call_validate()
        self.assertEqual([mock.call('LAN0-1'), mock.call('CNA1-1')],
                         mock_viom.validate_physical_port_id.call_args_list)

    def test_validate_no_physical_id_in_lan_port(self, mock_viom):
        self._create_port(physical_id=None)
        self._create_iscsi_resources()
        self.assertRaises(exception.MissingParameterValue,
                          self._call_validate)

    @mock.patch.object(irmc_boot, 'scci',
                       spec_set=mock_specs.SCCICLIENT_IRMC_SCCI_SPEC)
    def test_validate_invalid_physical_id_in_lan_port(self, mock_scci,
                                                      mock_viom):
        self._create_port(physical_id='wrong-id')
        self._create_iscsi_resources()

        mock_viom.validate_physical_port_id.side_effect = (
            Exception('fake error'))
        mock_scci.SCCIInvalidInputError = Exception
        self.assertRaises(exception.InvalidParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_no_ip(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_target()

        self.assertRaises(exception.MissingParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_no_iqn(self, mock_viom):
        self._create_port()
        self._create_iscsi_ip_connector(physical_id='CNA1-1')
        self._create_iscsi_target()

        self.assertRaises(exception.MissingParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_no_netmask(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector(network_size=None)
        self._create_iscsi_target()

        self.assertRaises(exception.MissingParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_invalid_netmask(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector(network_size='worng-netmask')
        self._create_iscsi_target()

        self.assertRaises(exception.InvalidParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_too_small_netmask(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector(network_size='0')
        self._create_iscsi_target()

        self.assertRaises(exception.InvalidParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_too_large_netmask(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector(network_size='32')
        self._create_iscsi_target()

        self.assertRaises(exception.InvalidParameterValue,
                          self._call_validate)

    def test_validate_iscsi_connector_no_physical_id(self, mock_viom):
        self._create_port()
        self._create_iscsi_iqn_connector(physical_id=None)
        self._create_iscsi_ip_connector()
        self._create_iscsi_target()

        self.assertRaises(exception.MissingParameterValue,
                          self._call_validate)

    @mock.patch.object(deploy_utils, 'get_single_nic_with_vif_port_id')
    def test_prepare_ramdisk_skip(self, mock_nic, mock_viom):
        self._create_iscsi_resources()
        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.node.provision_state = states.DEPLOYING
            task.driver.boot.prepare_ramdisk(task, {})
            mock_nic.assert_not_called()

    @mock.patch.object(irmc_boot, '_cleanup_vmedia_boot')
    def test_prepare_instance(self, mock_clean, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_iscsi_resources()

        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.driver.boot.prepare_instance(task)
            mock_clean.assert_not_called()

        mock_conf.set_iscsi_volume.assert_called_once_with(
            'CNA1-1',
            'iqn.initiator',
            initiator_ip='192.168.11.11',
            initiator_netmask=24,
            target_iqn='iqn.target',
            target_ip='192.168.22.22',
            target_port='3260',
            target_lun=1,
            boot_prio=1,
            chap_user=None,
            chap_secret=None)
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('CNA1-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def _call__configure_boot_from_volume(self):
        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.driver.boot._configure_boot_from_volume(task)

    def _assert_viom_apply(self, mock_viom, mock_conf):
        mock_conf.apply.assert_called_once_with()
        mock_conf.dump_json.assert_called_once_with()
        mock_viom.VIOMConfiguration.assert_called_once_with(
            PARSED_IFNO, identification=self.node.uuid)

    def test__configure_boot_from_volume_iscsi(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_iscsi_resources()

        self._call__configure_boot_from_volume()

        mock_conf.set_iscsi_volume.assert_called_once_with(
            'CNA1-1',
            'iqn.initiator',
            initiator_ip='192.168.11.11',
            initiator_netmask=24,
            target_iqn='iqn.target',
            target_ip='192.168.22.22',
            target_port='3260',
            target_lun=1,
            boot_prio=1,
            chap_user=None,
            chap_secret=None)
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('CNA1-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def test__configure_boot_from_volume_multi_lan_ports(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_port(physical_id='LAN0-2',
                          address='52:54:00:cf:2d:32')
        self._create_iscsi_resources()

        self._call__configure_boot_from_volume()

        mock_conf.set_iscsi_volume.assert_called_once_with(
            'CNA1-1',
            'iqn.initiator',
            initiator_ip='192.168.11.11',
            initiator_netmask=24,
            target_iqn='iqn.target',
            target_ip='192.168.22.22',
            target_port='3260',
            target_lun=1,
            boot_prio=1,
            chap_user=None,
            chap_secret=None)
        self.assertEqual([mock.call('LAN0-1'), mock.call('LAN0-2')],
                         mock_conf.set_lan_port.call_args_list)
        mock_viom.validate_physical_port_id.assert_called_once_with('CNA1-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def test__configure_boot_from_volume_iscsi_no_portal_port(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector()
        self._create_iscsi_target(
            target_info=dict(target_portal='192.168.22.23'))

        self._call__configure_boot_from_volume()

        mock_conf.set_iscsi_volume.assert_called_once_with(
            'CNA1-1',
            'iqn.initiator',
            initiator_ip='192.168.11.11',
            initiator_netmask=24,
            target_iqn='iqn.target',
            target_ip='192.168.22.23',
            target_port=None,
            target_lun=1,
            boot_prio=1,
            chap_user=None,
            chap_secret=None)
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('CNA1-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def test__configure_boot_from_volume_iscsi_chap(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_iscsi_iqn_connector()
        self._create_iscsi_ip_connector()
        self._create_iscsi_target(
            target_info=dict(auth_method='CHAP',
                             auth_username='chapuser',
                             auth_password='chappass'))

        self._call__configure_boot_from_volume()

        mock_conf.set_iscsi_volume.assert_called_once_with(
            'CNA1-1',
            'iqn.initiator',
            initiator_ip='192.168.11.11',
            initiator_netmask=24,
            target_iqn='iqn.target',
            target_ip='192.168.22.22',
            target_port='3260',
            target_lun=1,
            boot_prio=1,
            chap_user='chapuser',
            chap_secret='chappass')
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('CNA1-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def test__configure_boot_from_volume_fc(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_fc_connector()
        self._create_fc_target()

        self._call__configure_boot_from_volume()

        mock_conf.set_fc_volume.assert_called_once_with(
            'FC2-1',
            'aa:bb:cc:dd:ee',
            2,
            boot_prio=1)
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('FC2-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    @mock.patch.object(irmc_boot, 'scci',
                       spec_set=mock_specs.SCCICLIENT_IRMC_SCCI_SPEC)
    def test__configure_boot_from_volume_apply_error(self, mock_scci,
                                                     mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        self._create_port()
        self._create_fc_connector()
        self._create_fc_target()
        mock_conf.apply.side_effect = Exception('fake scci error')
        mock_scci.SCCIError = Exception

        with task_manager.acquire(self.context, self.node.uuid) as task:
            self.assertRaises(exception.IRMCOperationError,
                              task.driver.boot._configure_boot_from_volume,
                              task)

        mock_conf.set_fc_volume.assert_called_once_with(
            'FC2-1',
            'aa:bb:cc:dd:ee',
            2,
            boot_prio=1)
        mock_conf.set_lan_port.assert_called_once_with('LAN0-1')
        mock_viom.validate_physical_port_id.assert_called_once_with('FC2-1')
        self._assert_viom_apply(mock_viom, mock_conf)

    def test_clean_up_instance(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.driver.boot.clean_up_instance(task)

        mock_viom.VIOMConfiguration.assert_called_once_with(PARSED_IFNO,
                                                            self.node.uuid)
        mock_conf.terminate.assert_called_once_with(reboot=False)

    def test_clean_up_instance_error(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        mock_conf.terminate.side_effect = Exception('fake error')
        irmc_boot.scci.SCCIError = Exception
        with task_manager.acquire(self.context, self.node.uuid) as task:
            self.assertRaises(exception.IRMCOperationError,
                              task.driver.boot.clean_up_instance,
                              task)

        mock_viom.VIOMConfiguration.assert_called_once_with(PARSED_IFNO,
                                                            self.node.uuid)
        mock_conf.terminate.assert_called_once_with(reboot=False)

    def test__cleanup_boot_from_volume(self, mock_viom):
        mock_conf = self._create_mock_conf(mock_viom)
        with task_manager.acquire(self.context, self.node.uuid) as task:
            task.driver.boot._cleanup_boot_from_volume(task)

        mock_viom.VIOMConfiguration.assert_called_once_with(PARSED_IFNO,
                                                            self.node.uuid)
        mock_conf.terminate.assert_called_once_with(reboot=False)
