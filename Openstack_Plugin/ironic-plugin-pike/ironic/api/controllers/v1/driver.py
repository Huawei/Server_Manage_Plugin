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

from ironic_lib import metrics_utils
import pecan
from pecan import rest
from six.moves import http_client
import wsme
from wsme import types as wtypes

from ironic.api.controllers import base
from ironic.api.controllers import link
from ironic.api.controllers.v1 import types
from ironic.api.controllers.v1 import utils as api_utils
from ironic.api import expose
from ironic.common import exception
from ironic.common.i18n import _
from ironic.common import policy
from ironic.drivers import base as driver_base


METRICS = metrics_utils.get_metrics_logger(__name__)

# Property information for drivers:
#   key = driver name;
#   value = dictionary of properties of that driver:
#             key = property name.
#             value = description of the property.
# NOTE(rloo). This is cached for the lifetime of the API service. If one or
# more conductor services are restarted with new driver versions, the API
# service should be restarted.
_DRIVER_PROPERTIES = {}

# Vendor information for drivers:
#   key = driver name;
#   value = dictionary of vendor methods of that driver:
#             key = method name.
#             value = dictionary with the metadata of that method.
# NOTE(lucasagomes). This is cached for the lifetime of the API
# service. If one or more conductor services are restarted with new driver
# versions, the API service should be restarted.
_VENDOR_METHODS = {}

# RAID (logical disk) configuration information for drivers:
#   key = driver name;
#   value = dictionary of RAID configuration information of that driver:
#             key = property name.
#             value = description of the property
# NOTE(rloo). This is cached for the lifetime of the API service. If one or
# more conductor services are restarted with new driver versions, the API
# service should be restarted.
_RAID_PROPERTIES = {}


def hide_fields_in_newer_versions(obj):
    """This method hides fields that were added in newer API versions.

    Certain fields were introduced at certain API versions.
    These fields are only made available when the request's API version
    matches or exceeds the versions when these fields were introduced.
    """
    if not api_utils.allow_storage_interface():
        obj.default_storage_interface = wsme.Unset
        obj.enabled_storage_interfaces = wsme.Unset


class Driver(base.APIBase):
    """API representation of a driver."""

    name = wtypes.text
    """The name of the driver"""

    hosts = [wtypes.text]
    """A list of active conductors that support this driver"""

    type = wtypes.text
    """Whether the driver is classic or dynamic (hardware type)"""

    links = wsme.wsattr([link.Link], readonly=True)
    """A list containing self and bookmark links"""

    properties = wsme.wsattr([link.Link], readonly=True)
    """A list containing links to driver properties"""

    """Default interface for a hardware type"""
    default_boot_interface = wtypes.text
    default_console_interface = wtypes.text
    default_deploy_interface = wtypes.text
    default_inspect_interface = wtypes.text
    default_management_interface = wtypes.text
    default_network_interface = wtypes.text
    default_power_interface = wtypes.text
    default_raid_interface = wtypes.text
    default_storage_interface = wtypes.text
    default_vendor_interface = wtypes.text

    """A list of enabled interfaces for a hardware type"""
    enabled_boot_interfaces = [wtypes.text]
    enabled_console_interfaces = [wtypes.text]
    enabled_deploy_interfaces = [wtypes.text]
    enabled_inspect_interfaces = [wtypes.text]
    enabled_management_interfaces = [wtypes.text]
    enabled_network_interfaces = [wtypes.text]
    enabled_power_interfaces = [wtypes.text]
    enabled_raid_interfaces = [wtypes.text]
    enabled_storage_interfaces = [wtypes.text]
    enabled_vendor_interfaces = [wtypes.text]

    @staticmethod
    def convert_with_links(name, hosts, driver_type, detail=False,
                           interface_info=None):
        """Convert driver/hardware type info to an API-serializable object.

        :param name: name of driver or hardware type.
        :param hosts: list of conductor hostnames driver is active on.
        :param driver_type: 'classic' for classic drivers, 'dynamic' for
                            hardware types.
        :param detail: boolean, whether to include detailed info, such as
                       the 'type' field and default/enabled interfaces fields.
        :param interface_info: optional list of dicts of hardware interface
                               info.
        :returns: API-serializable driver object.
        """
        driver = Driver()
        driver.name = name
        driver.hosts = hosts
        driver.links = [
            link.Link.make_link('self',
                                pecan.request.public_url,
                                'drivers', name),
            link.Link.make_link('bookmark',
                                pecan.request.public_url,
                                'drivers', name,
                                bookmark=True)
        ]
        if api_utils.allow_links_node_states_and_driver_properties():
            driver.properties = [
                link.Link.make_link('self',
                                    pecan.request.public_url,
                                    'drivers', name + "/properties"),
                link.Link.make_link('bookmark',
                                    pecan.request.public_url,
                                    'drivers', name + "/properties",
                                    bookmark=True)
            ]

        if api_utils.allow_dynamic_drivers():
            driver.type = driver_type
            if driver_type == 'dynamic' and detail:
                if interface_info is None:
                    # TODO(jroll) objectify this
                    interface_info = (pecan.request.dbapi
                                      .list_hardware_type_interfaces([name]))
                for iface_type in driver_base.ALL_INTERFACES:
                    default = None
                    enabled = set()
                    for iface in interface_info:
                        if iface['interface_type'] == iface_type:
                            iface_name = iface['interface_name']
                            enabled.add(iface_name)
                            # NOTE(jroll) this assumes the default is the same
                            # on all conductors
                            if iface['default']:
                                default = iface_name

                    default_key = 'default_%s_interface' % iface_type
                    enabled_key = 'enabled_%s_interfaces' % iface_type
                    setattr(driver, default_key, default)
                    setattr(driver, enabled_key, list(enabled))

            elif detail:
                for iface_type in driver_base.ALL_INTERFACES:
                    # always return None for classic drivers
                    setattr(driver, 'default_%s_interface' % iface_type, None)
                    setattr(driver, 'enabled_%s_interfaces' % iface_type, None)

        hide_fields_in_newer_versions(driver)
        return driver

    @classmethod
    def sample(cls):
        attrs = {
            'name': 'sample-driver',
            'hosts': ['fake-host'],
            'type': 'classic',
        }
        for iface_type in driver_base.ALL_INTERFACES:
            attrs['default_%s_interface' % iface_type] = None
            attrs['enabled_%s_interfaces' % iface_type] = None

        sample = cls(**attrs)
        return sample


class DriverList(base.APIBase):
    """API representation of a list of drivers."""

    drivers = [Driver]
    """A list containing drivers objects"""

    @staticmethod
    def convert_with_links(drivers, hardware_types, detail=False):
        """Convert drivers and hardware types to an API-serializable object.

        :param drivers: dict mapping driver names to conductor hostnames.
        :param hardware_types: dict mapping hardware type names to conductor
                               hostnames.
        :param detail: boolean, whether to include detailed info, such as
                       the 'type' field and default/enabled interfaces fields.
        :returns: an API-serializable driver collection object.
        """
        collection = DriverList()
        collection.drivers = [
            Driver.convert_with_links(dname, list(drivers[dname]), 'classic',
                                      detail=detail)
            for dname in drivers]

        # NOTE(jroll) we return hardware types in all API versions,
        # but restrict type/default/enabled fields to 1.30.
        # This is checked in Driver.convert_with_links(), however also
        # checking here can save us a DB query.
        if api_utils.allow_dynamic_drivers() and detail:
            iface_info = pecan.request.dbapi.list_hardware_type_interfaces(
                list(hardware_types))
        else:
            iface_info = []

        for htname in hardware_types:
            interface_info = [i for i in iface_info
                              if i['hardware_type'] == htname]
            collection.drivers.append(
                Driver.convert_with_links(htname,
                                          list(hardware_types[htname]),
                                          'dynamic', detail=detail,
                                          interface_info=interface_info))
        return collection

    @classmethod
    def sample(cls):
        sample = cls()
        sample.drivers = [Driver.sample()]
        return sample


class DriverPassthruController(rest.RestController):
    """REST controller for driver passthru.

    This controller allow vendors to expose cross-node functionality in the
    Ironic API. Ironic will merely relay the message from here to the specified
    driver, no introspection will be made in the message body.
    """

    _custom_actions = {
        'methods': ['GET']
    }

    @METRICS.timer('DriverPassthruController.methods')
    @expose.expose(wtypes.text, wtypes.text)
    def methods(self, driver_name):
        """Retrieve information about vendor methods of the given driver.

        :param driver_name: name of the driver.
        :returns: dictionary with <vendor method name>:<method metadata>
                  entries.
        :raises: DriverNotFound if the driver name is invalid or the
                 driver cannot be loaded.
        """
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:vendor_passthru', cdict, cdict)

        if driver_name not in _VENDOR_METHODS:
            topic = pecan.request.rpcapi.get_topic_for_driver(driver_name)
            ret = pecan.request.rpcapi.get_driver_vendor_passthru_methods(
                pecan.request.context, driver_name, topic=topic)
            _VENDOR_METHODS[driver_name] = ret

        return _VENDOR_METHODS[driver_name]

    @METRICS.timer('DriverPassthruController._default')
    @expose.expose(wtypes.text, wtypes.text, wtypes.text,
                   body=wtypes.text)
    def _default(self, driver_name, method, data=None):
        """Call a driver API extension.

        :param driver_name: name of the driver to call.
        :param method: name of the method, to be passed to the vendor
                       implementation.
        :param data: body of data to supply to the specified method.
        """
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:vendor_passthru', cdict, cdict)

        topic = pecan.request.rpcapi.get_topic_for_driver(driver_name)
        return api_utils.vendor_passthru(driver_name, method, topic, data=data,
                                         driver_passthru=True)


class DriverRaidController(rest.RestController):

    _custom_actions = {
        'logical_disk_properties': ['GET']
    }

    @METRICS.timer('DriverRaidController.logical_disk_properties')
    @expose.expose(types.jsontype, wtypes.text)
    def logical_disk_properties(self, driver_name):
        """Returns the logical disk properties for the driver.

        :param driver_name: Name of the driver.
        :returns: A dictionary containing the properties that can be mentioned
            for logical disks and a textual description for them.
        :raises: UnsupportedDriverExtension if the driver doesn't
            support RAID configuration.
        :raises: NotAcceptable, if requested version of the API is less than
            1.12.
        :raises: DriverNotFound, if driver is not loaded on any of the
            conductors.
        """
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:get_raid_logical_disk_properties',
                         cdict, cdict)

        if not api_utils.allow_raid_config():
            raise exception.NotAcceptable()

        if driver_name not in _RAID_PROPERTIES:
            topic = pecan.request.rpcapi.get_topic_for_driver(driver_name)
            try:
                info = pecan.request.rpcapi.get_raid_logical_disk_properties(
                    pecan.request.context, driver_name, topic=topic)
            except exception.UnsupportedDriverExtension as e:
                # Change error code as 404 seems appropriate because RAID is a
                # standard interface and all drivers might not have it.
                e.code = http_client.NOT_FOUND
                raise

            _RAID_PROPERTIES[driver_name] = info
        return _RAID_PROPERTIES[driver_name]


class DriversController(rest.RestController):
    """REST controller for Drivers."""

    vendor_passthru = DriverPassthruController()

    raid = DriverRaidController()
    """Expose RAID as a sub-element of drivers"""

    _custom_actions = {
        'properties': ['GET'],
    }

    @METRICS.timer('DriversController.get_all')
    @expose.expose(DriverList, wtypes.text, types.boolean)
    def get_all(self, type=None, detail=None):
        """Retrieve a list of drivers."""
        # FIXME(deva): formatting of the auto-generated REST API docs
        #              will break from a single-line doc string.
        #              This is a result of a bug in sphinxcontrib-pecanwsme
        # https://github.com/dreamhost/sphinxcontrib-pecanwsme/issues/8
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:get', cdict, cdict)

        api_utils.check_allow_driver_detail(detail)
        api_utils.check_allow_filter_driver_type(type)
        if type not in (None, 'classic', 'dynamic'):
            raise exception.Invalid(_(
                '"type" filter must be one of "classic" or "dynamic", '
                'if specified.'))

        driver_list = {}
        hw_type_dict = {}
        if type is None or type == 'classic':
            driver_list = pecan.request.dbapi.get_active_driver_dict()
        if type is None or type == 'dynamic':
            hw_type_dict = pecan.request.dbapi.get_active_hardware_type_dict()
        return DriverList.convert_with_links(driver_list, hw_type_dict,
                                             detail=detail)

    @METRICS.timer('DriversController.get_one')
    @expose.expose(Driver, wtypes.text)
    def get_one(self, driver_name):
        """Retrieve a single driver."""
        # NOTE(russell_h): There is no way to make this more efficient than
        # retrieving a list of drivers using the current sqlalchemy schema, but
        # this path must be exposed for Pecan to route any paths we might
        # choose to expose below it.
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:get', cdict, cdict)

        def _find_driver(driver_dict, driver_type):
            for name, hosts in driver_dict.items():
                if name == driver_name:
                    return Driver.convert_with_links(name, list(hosts),
                                                     driver_type, detail=True)

        hw_type_dict = pecan.request.dbapi.get_active_hardware_type_dict()
        driver = _find_driver(hw_type_dict, 'dynamic')
        if driver:
            return driver
        driver_dict = pecan.request.dbapi.get_active_driver_dict()
        driver = _find_driver(driver_dict, 'classic')
        if driver:
            return driver

        raise exception.DriverNotFound(driver_name=driver_name)

    @METRICS.timer('DriversController.properties')
    @expose.expose(wtypes.text, wtypes.text)
    def properties(self, driver_name):
        """Retrieve property information of the given driver.

        :param driver_name: name of the driver.
        :returns: dictionary with <property name>:<property description>
                  entries.
        :raises: DriverNotFound (HTTP 404) if the driver name is invalid or
                 the driver cannot be loaded.
        """
        cdict = pecan.request.context.to_policy_values()
        policy.authorize('baremetal:driver:get_properties', cdict, cdict)

        if driver_name not in _DRIVER_PROPERTIES:
            topic = pecan.request.rpcapi.get_topic_for_driver(driver_name)
            properties = pecan.request.rpcapi.get_driver_properties(
                pecan.request.context, driver_name, topic=topic)
            _DRIVER_PROPERTIES[driver_name] = properties

        return _DRIVER_PROPERTIES[driver_name]
