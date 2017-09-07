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

"""The Ironic Service API."""

import sys

from oslo_config import cfg
from oslo_reports import guru_meditation_report as gmr

from ironic.common import profiler
from ironic.common import service as ironic_service
from ironic.common import wsgi_service
from ironic.objects import base
from ironic.objects import indirection
from ironic import version

CONF = cfg.CONF


def main():
    # Parse config file and command line options, then start logging
    ironic_service.prepare_service(sys.argv)

    gmr.TextGuruMeditation.setup_autorun(version)

    # Enable object backporting via the conductor
    base.IronicObject.indirection_api = (
        indirection.IronicObjectIndirectionAPI())

    profiler.setup('ironic_api', CONF.host)

    # Build and start the WSGI app
    launcher = ironic_service.process_launcher()
    server = wsgi_service.WSGIService('ironic_api', CONF.api.enable_ssl_api)
    launcher.launch_service(server, workers=server.workers)
    launcher.wait()

if __name__ == '__main__':
    sys.exit(main())
