# Copyright 2016 Intel Corporation
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

import os

from oslo_config import cfg

from ironic.common.i18n import _

opts = [
    cfg.StrOpt('pxe_append_params',
               default='nofb nomodeset vga=normal',
               help=_('Additional append parameters for baremetal PXE boot.')),
    cfg.StrOpt('default_ephemeral_format',
               default='ext4',
               help=_('Default file system format for ephemeral partition, '
                      'if one is created.')),
    cfg.StrOpt('images_path',
               default='/var/lib/ironic/images/',
               help=_('On the ironic-conductor node, directory where images '
                      'are stored on disk.')),
    cfg.StrOpt('instance_master_path',
               default='/var/lib/ironic/master_images',
               help=_('On the ironic-conductor node, directory where master '
                      'instance images are stored on disk. '
                      'Setting to <None> disables image caching.')),
    cfg.IntOpt('image_cache_size',
               default=20480,
               help=_('Maximum size (in MiB) of cache for master images, '
                      'including those in use.')),
    # 10080 here is 1 week - 60*24*7. It is entirely arbitrary in the absence
    # of a facility to disable the ttl entirely.
    cfg.IntOpt('image_cache_ttl',
               default=10080,
               help=_('Maximum TTL (in minutes) for old master images in '
                      'cache.')),
    cfg.StrOpt('pxe_config_template',
               default=os.path.join(
                   '$pybasedir', 'drivers/modules/pxe_config.template'),
               help=_('On ironic-conductor node, template file for PXE '
                      'configuration.')),
    cfg.StrOpt('uefi_pxe_config_template',
               default=os.path.join(
                   '$pybasedir',
                   'drivers/modules/pxe_grub_config.template'),
               help=_('On ironic-conductor node, template file for PXE '
                      'configuration for UEFI boot loader.')),
    cfg.DictOpt('pxe_config_template_by_arch',
                default={},
                help=_('On ironic-conductor node, template file for PXE '
                       'configuration per node architecture. '
                       'For example: '
                       'aarch64:/opt/share/grubaa64_pxe_config.template')),
    cfg.StrOpt('tftp_server',
               default='$my_ip',
               help=_("IP address of ironic-conductor node's TFTP server.")),
    cfg.StrOpt('tftp_root',
               default='/tftpboot',
               help=_("ironic-conductor node's TFTP root path. The "
                      "ironic-conductor must have read/write access to this "
                      "path.")),
    cfg.StrOpt('tftp_master_path',
               default='/tftpboot/master_images',
               help=_('On ironic-conductor node, directory where master TFTP '
                      'images are stored on disk. '
                      'Setting to <None> disables image caching.')),
    cfg.IntOpt('dir_permission',
               help=_("The permission that will be applied to the TFTP "
                      "folders upon creation. This should be set to the "
                      "permission such that the tftpserver has access to "
                      "read the contents of the configured TFTP folder. This "
                      "setting is only required when the operating system's "
                      "umask is restrictive such that ironic-conductor is "
                      "creating files that cannot be read by the TFTP server. "
                      "Setting to <None> will result in the operating "
                      "system's umask to be utilized for the creation of new "
                      "tftp folders. It is recommended that an octal "
                      "representation is specified. For example: 0o755")),
    cfg.StrOpt('pxe_bootfile_name',
               default='pxelinux.0',
               help=_('Bootfile DHCP parameter.')),
    cfg.StrOpt('uefi_pxe_bootfile_name',
               default='bootx64.efi',
               help=_('Bootfile DHCP parameter for UEFI boot mode.')),
    cfg.DictOpt('pxe_bootfile_name_by_arch',
                default={},
                help=_('Bootfile DHCP parameter per node architecture. '
                       'For example: aarch64:grubaa64.efi')),
    cfg.BoolOpt('ipxe_enabled',
                default=False,
                help=_('Enable iPXE boot.')),
    cfg.StrOpt('ipxe_boot_script',
               default=os.path.join(
                   '$pybasedir', 'drivers/modules/boot.ipxe'),
               help=_('On ironic-conductor node, the path to the main iPXE '
                      'script file.')),
    cfg.IntOpt('ipxe_timeout',
               default=0,
               help=_('Timeout value (in seconds) for downloading an image '
                      'via iPXE. Defaults to 0 (no timeout)')),
    cfg.StrOpt('ip_version',
               default='4',
               choices=['4', '6'],
               help=_('The IP version that will be used for PXE booting. '
                      'Defaults to 4. EXPERIMENTAL')),
    cfg.BoolOpt('ipxe_use_swift',
                default=False,
                help=_("Download deploy images directly from swift using "
                       "temporary URLs. "
                       "If set to false (default), images are downloaded "
                       "to the ironic-conductor node and served over its "
                       "local HTTP server. "
                       "Applicable only when 'ipxe_enabled' option is "
                       "set to true.")),

]


def register_opts(conf):
    conf.register_opts(opts, group='pxe')
