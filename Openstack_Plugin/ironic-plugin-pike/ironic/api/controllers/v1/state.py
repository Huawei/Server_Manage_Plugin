# Copyright 2013 Red Hat, Inc.
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

from wsme import types as wtypes

from ironic.api.controllers import base
from ironic.api.controllers import link


class State(base.APIBase):

    current = wtypes.text
    """The current state"""

    target = wtypes.text
    """The user modified desired state"""

    available = [wtypes.text]
    """A list of available states it is able to transition to"""

    links = [link.Link]
    """A list containing a self link and associated state links"""
