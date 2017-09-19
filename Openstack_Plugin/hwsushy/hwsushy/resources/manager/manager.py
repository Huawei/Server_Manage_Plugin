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

from hwsushy.resources import base

LOG = logging.getLogger(__name__)


class Manager(base.ResourceBase):

    ManagerType = None
    """The system UUID"""

    def __init__(self, connector, identity, redfish_version=None):
        """A class representing a Manager

        :param connector: A Connector instance
        :param identity: The identity of the System resource
        :param redfish_version: The version of RedFish. Used to construct
            the object according to schema of the given version.
        """
        super(Manager, self).__init__(connector, identity, redfish_version)

    def _parse_attributes(self):
        self.ManagerType = self.json.get('ManagerType')


class ManagerCollection(base.ResourceCollectionBase):

    @property
    def _resource_type(self):
        return Manager

    def __init__(self, connector, path, redfish_version=None):
        """A class representing a ComputerSystemCollection

        :param connector: A Connector instance
        :param path: The canonical path to the System collection resource
        :param redfish_version: The version of RedFish. Used to construct
            the object according to schema of the given version.
        """
        super(ManagerCollection, self).__init__(connector, path,
                                                redfish_version)
