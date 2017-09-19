# coding=utf-8
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

"""Central place for handling Keystone authorization and service lookup."""

from keystoneauth1 import exceptions as kaexception
from keystoneauth1 import loading as kaloading
from oslo_log import log as logging
import six

from ironic.common import exception
from ironic.conf import CONF


LOG = logging.getLogger(__name__)


def ks_exceptions(f):
    """Wraps keystoneclient functions and centralizes exception handling."""
    @six.wraps(f)
    def wrapper(*args, **kwargs):
        try:
            return f(*args, **kwargs)
        except kaexception.EndpointNotFound:
            service_type = kwargs.get('service_type', 'baremetal')
            endpoint_type = kwargs.get('endpoint_type', 'internal')
            raise exception.CatalogNotFound(
                service_type=service_type, endpoint_type=endpoint_type)
        except (kaexception.Unauthorized, kaexception.AuthorizationFailure):
            raise exception.KeystoneUnauthorized()
        except (kaexception.NoMatchingPlugin,
                kaexception.MissingRequiredOptions) as e:
            raise exception.ConfigInvalid(six.text_type(e))
        except Exception as e:
            LOG.exception('Keystone request failed: %(msg)s',
                          {'msg': six.text_type(e)})
            raise exception.KeystoneFailure(six.text_type(e))
    return wrapper


@ks_exceptions
def get_session(group, **session_kwargs):
    """Loads session object from options in a configuration file section.

    The session_kwargs will be passed directly to keystoneauth1 Session
    and will override the values loaded from config.
    Consult keystoneauth1 docs for available options.

    :param group: name of the config section to load session options from

    """
    return kaloading.load_session_from_conf_options(
        CONF, group, **session_kwargs)


@ks_exceptions
def get_auth(group, **auth_kwargs):
    """Loads auth plugin from options in a configuration file section.

    The auth_kwargs will be passed directly to keystoneauth1 auth plugin
    and will override the values loaded from config.
    Note that the accepted kwargs will depend on auth plugin type as defined
    by [group]auth_type option.
    Consult keystoneauth1 docs for available auth plugins and their options.

    :param group: name of the config section to load auth plugin options from

    """
    try:
        auth = kaloading.load_auth_from_conf_options(CONF, group,
                                                     **auth_kwargs)
    except kaexception.MissingRequiredOptions:
        LOG.error('Failed to load auth plugin from group %s', group)
        raise
    return auth


# NOTE(pas-ha) Used by neutronclient and resolving ironic API only
# FIXME(pas-ha) remove this while moving to kesytoneauth adapters
@ks_exceptions
def get_service_url(session, **kwargs):
    """Find endpoint for given service in keystone catalog.

    If 'interface' is provided, fetches service url of this interface.
    Otherwise, first tries to fetch 'internal' endpoint,
    and then the 'public' one.

    :param session: keystoneauth Session object
    :param kwargs: any other arguments accepted by Session.get_endpoint method

    """

    if 'interface' in kwargs:
        return session.get_endpoint(**kwargs)
    try:
        return session.get_endpoint(interface='internal', **kwargs)
    except kaexception.EndpointNotFound:
        return session.get_endpoint(interface='public', **kwargs)
