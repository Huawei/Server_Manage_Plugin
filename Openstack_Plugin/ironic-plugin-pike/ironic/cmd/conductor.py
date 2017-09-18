# -*- encoding: utf-8 -*-
#
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

"""
The Ironic Management Service
"""

import sys

from oslo_config import cfg
from oslo_log import log
from oslo_reports import guru_meditation_report as gmr
from oslo_service import service

from ironic.common import profiler as profiler
from ironic.common import rpc_service
from ironic.common import service as ironic_service
from ironic import version

CONF = cfg.CONF

LOG = log.getLogger(__name__)


def warn_about_unsafe_shred_parameters(conf):
    iterations = conf.deploy.shred_random_overwrite_iterations
    overwrite_with_zeros = conf.deploy.shred_final_overwrite_with_zeros
    if iterations == 0 and overwrite_with_zeros is False:
        LOG.warning('With shred_random_overwrite_iterations set to 0 and '
                    'shred_final_overwrite_with_zeros set to False, disks '
                    'may NOT be shredded at all, unless they support ATA '
                    'Secure Erase. This is a possible SECURITY ISSUE!')


def warn_about_missing_default_boot_option(conf):
    if not conf.deploy.default_boot_option:
        LOG.warning('The default value of default_boot_option '
                    'configuration will change eventually from '
                    '"netboot" to "local". It is recommended to set '
                    'an explicit value for it during the transition period')


def issue_startup_warnings(conf):
    warn_about_unsafe_shred_parameters(conf)
    warn_about_missing_default_boot_option(conf)


def main():
    # NOTE(lucasagomes): Safeguard to prevent 'ironic.conductor.manager'
    # from being imported prior to the configuration options being loaded.
    # If this happened, the periodic decorators would always use the
    # default values of the options instead of the configured ones. For
    # more information see: https://bugs.launchpad.net/ironic/+bug/1562258
    # and https://bugs.launchpad.net/ironic/+bug/1279774.
    assert 'ironic.conductor.manager' not in sys.modules

    # Parse config file and command line options, then start logging
    ironic_service.prepare_service(sys.argv)

    gmr.TextGuruMeditation.setup_autorun(version)

    mgr = rpc_service.RPCService(CONF.host,
                                 'ironic.conductor.manager',
                                 'ConductorManager')

    issue_startup_warnings(CONF)

    profiler.setup('ironic_conductor', CONF.host)

    launcher = service.launch(CONF, mgr)
    launcher.wait()


if __name__ == '__main__':
    sys.exit(main())
