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

import os

from oslo_log import log
from oslo_utils import excutils
from oslo_utils import importutils
from oslo_utils import strutils
import retrying
import rfc3986
import six
from six.moves import urllib

from ironic.common import exception
from ironic.common.i18n import _
from ironic.conf import CONF

sushy = importutils.try_import('sushy')

LOG = log.getLogger(__name__)

REQUIRED_PROPERTIES = {
    'redfish_address': _('The URL address to the Redfish controller. It '
                         'must include the authority portion of the URL. '
                         'If the scheme is missing, https is assumed. '
                         'For example: https://mgmt.vendor.com. Required'),
    'redfish_system_id': _('The canonical path to the ComputerSystem '
                           'resource that the driver will interact with. '
                           'It should include the root service, version and '
                           'the unique resource path to a ComputerSystem '
                           'within the same authority as the redfish_address '
                           'property. For example: /redfish/v1/Systems/1. '
                           'Required')
}

OPTIONAL_PROPERTIES = {
    'redfish_username': _('User account with admin/server-profile access '
                          'privilege. Although this property is not '
                          'mandatory it\'s highly recommended to set a '
                          'username. Optional'),
    'redfish_password': _('User account password. Although this property is '
                          'not mandatory, it\'s highly recommended to set a '
                          'password. Optional'),
    'redfish_verify_ca': _('Either a Boolean value, a path to a CA_BUNDLE '
                           'file or directory with certificates of trusted '
                           'CAs. If set to True the driver will verify the '
                           'host certificates; if False the driver will '
                           'ignore verifying the SSL certificate. If it\'s '
                           'a path the driver will use the specified '
                           'certificate or one of the certificates in the '
                           'directory. Defaults to True. Optional')
}

COMMON_PROPERTIES = REQUIRED_PROPERTIES.copy()
COMMON_PROPERTIES.update(OPTIONAL_PROPERTIES)


def parse_driver_info(node):
    """Parse the information required for Ironic to connect to Redfish.

    :param node: an Ironic node object
    :returns: dictionary of parameters
    :raises: InvalidParameterValue on malformed parameter(s)
    :raises: MissingParameterValue on missing parameter(s)
    """
    driver_info = node.driver_info or {}
    missing_info = [key for key in REQUIRED_PROPERTIES
                    if not driver_info.get(key)]
    if missing_info:
        raise exception.MissingParameterValue(_(
            'Missing the following Redfish properties in node '
            '%(node)s driver_info: %(info)s') % {'node': node.uuid,
                                                 'info': missing_info})

    # Validate the Redfish address
    address = driver_info['redfish_address']
    try:
        parsed = rfc3986.uri_reference(address)
    except TypeError:
        raise exception.InvalidParameterValue(
            _('Invalid Redfish address %(address)s set in '
              'driver_info/redfish_address on node %(node)s') %
            {'address': address, 'node': node.uuid})

    if not parsed.scheme or not parsed.authority:
        address = 'https://%s' % address
        parsed = rfc3986.uri_reference(address)
    # TODO(vdrok): Workaround this check, in py3 we need to use validator class
    if not parsed.is_valid(require_scheme=True, require_authority=True):
        raise exception.InvalidParameterValue(
            _('Invalid Redfish address %(address)s set in '
              'driver_info/redfish_address on node %(node)s') %
            {'address': address, 'node': node.uuid})

    try:
        system_id = urllib.parse.quote(driver_info['redfish_system_id'])
    except (TypeError, AttributeError):
        raise exception.InvalidParameterValue(
            _('Invalid value "%(value)s" set in '
              'driver_info/redfish_system_id on node %(node)s. '
              'The value should be a path (string) to the resource '
              'that the driver will interact with. For example: '
              '/redfish/v1/Systems/1') %
            {'value': driver_info['redfish_system_id'], 'node': node.uuid})

    # Check if verify_ca is a Boolean or a file/directory in the file-system
    verify_ca = driver_info.get('redfish_verify_ca', True)
    if isinstance(verify_ca, six.string_types):
        if os.path.isdir(verify_ca) or os.path.isfile(verify_ca):
            pass
        else:
            try:
                verify_ca = strutils.bool_from_string(verify_ca, strict=True)
            except ValueError:
                raise exception.InvalidParameterValue(
                    _('Invalid value type set in driver_info/'
                      'redfish_verify_ca on node %(node)s. '
                      'The value should be a Boolean or the path '
                      'to a file/directory, not "%(value)s"'
                      ) % {'value': verify_ca, 'node': node.uuid})
    elif isinstance(verify_ca, bool):
        # If it's a boolean it's grand, we don't need to do anything
        pass
    else:
        raise exception.InvalidParameterValue(
            _('Invalid value type set in driver_info/redfish_verify_ca '
              'on node %(node)s. The value should be a Boolean or the path '
              'to a file/directory, not "%(value)s"') % {'value': verify_ca,
                                                         'node': node.uuid})

    return {'address': address,
            'system_id': system_id,
            'username': driver_info.get('redfish_username'),
            'password': driver_info.get('redfish_password'),
            'verify_ca': verify_ca,
            'node_uuid': node.uuid}


def get_system(node):
    """Get a Redfish System that represents a node.

    :param node: an Ironic node object
    :raises: RedfishConnectionError when it fails to connect to Redfish
    :raises: RedfishError if the System is not registered in Redfish
    """
    driver_info = parse_driver_info(node)
    address = driver_info['address']
    system_id = driver_info['system_id']

    @retrying.retry(
        retry_on_exception=(
            lambda e: isinstance(e, exception.RedfishConnectionError)),
        stop_max_attempt_number=CONF.redfish.connection_attempts,
        wait_fixed=CONF.redfish.connection_retry_interval * 1000)
    def _get_system():
        try:
            # TODO(lucasagomes): We should look into a mechanism to
            # cache the connections (and maybe even system's instances)
            # to avoid unnecessary requests to the Redfish controller
            conn = sushy.Sushy(address, username=driver_info['username'],
                               password=driver_info['password'],
                               verify=driver_info['verify_ca'])
            return conn.get_system(system_id)
        except sushy.exceptions.ResourceNotFoundError as e:
            LOG.error('The Redfish System "%(system)s" was not found for '
                      'node %(node)s. Error %(error)s',
                      {'system': system_id, 'node': node.uuid, 'error': e})
            raise exception.RedfishError(error=e)
        # TODO(lucasagomes): We should look at other types of
        # ConnectionError such as AuthenticationError or SSLError and stop
        # retrying on them
        except sushy.exceptions.ConnectionError as e:
            LOG.warning('For node %(node)s, got a connection error from '
                        'Redfish at address "%(address)s" when fetching '
                        'System "%(system)s". Error: %(error)s',
                        {'system': system_id, 'address': address,
                         'node': node.uuid, 'error': e})
            raise exception.RedfishConnectionError(node=node.uuid, error=e)

    try:
        return _get_system()
    except exception.RedfishConnectionError as e:
        with excutils.save_and_reraise_exception():
            LOG.error('Failed to connect to Redfish at %(address)s for '
                      'node %(node)s. Error: %(error)s',
                      {'address': address, 'node': node.uuid, 'error': e})
