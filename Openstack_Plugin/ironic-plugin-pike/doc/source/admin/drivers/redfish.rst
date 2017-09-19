==============
Redfish driver
==============

Overview
========

The ``redfish`` driver enables managing servers compliant with the
Redfish_ protocol.

Prerequisites
=============

* The Sushy_ library should be installed on the ironic conductor node(s).

  For example, it can be installed with ``pip``::

      sudo pip install sushy

Enabling the Redfish driver
===========================

#. Add ``redfish`` to the list of ``enabled_hardware_types``,
   ``enabled_power_interfaces`` and ``enabled_management_interfaces``
   in ``/etc/ironic/ironic.conf``. For example::

    [DEFAULT]
    ...
    enabled_hardware_types = ipmi,redfish
    enabled_power_interfaces = ipmitool,redfish
    enabled_management_interfaces = ipmitool,redfish

#. Restart the ironic conductor service::

    sudo service ironic-conductor restart

    # Or, for RDO:
    sudo systemctl restart openstack-ironic-conductor

Registering a node with the Redfish driver
===========================================

Nodes configured to use the driver should have the ``driver`` property
set to ``redfish``.

The following properties are specified in the node's ``driver_info``
field:

- ``redfish_address``: The URL address to the Redfish controller. It must
                       include the authority portion of the URL, and can
                       optionally include the scheme. If the scheme is
                       missing, https is assumed.
                       For example: https://mgmt.vendor.com. This is required.

- ``redfish_system_id``: The canonical path to the System resource that
                         the driver will interact with. It should include
                         the root service, version and the unique
                         resource path to the System. For example:
                         /redfish/v1/Systems/1. This is required.

- ``redfish_username``: User account with admin/server-profile access
                        privilege. Although not required, it is highly
                        recommended.

- ``redfish_password``: User account password. Although not required, it is
                        highly recommended.

- ``redfish_verify_ca``: If redfish_address has the **https** scheme, the
                         driver will use a secure (TLS_) connection when
                         talking to the Redfish controller. By default
                         (if this is not set or set to True), the driver
                         will try to verify the host certificates. This
                         can be set to the path of a certificate file or
                         directory with trusted certificates that the
                         driver will use for verification. To disable
                         verifying TLS_, set this to False. This is optional.

The ``openstack baremetal node create`` command can be used to enroll
a node with the ``redfish`` driver. For example:

.. code-block:: bash

  openstack baremetal node create --driver redfish --driver-info \
    redfish_address=https://example.com --driver-info \
    redfish_system_id=/redfish/v1/Systems/CX34R87 --driver-info \
    redfish_username=admin --driver-info redfish_password=password

For more information about enrolling nodes see :ref:`enrollment`
in the install guide.

.. _Redfish: http://redfish.dmtf.org/
.. _Sushy: https://git.openstack.org/cgit/openstack/sushy
.. _TLS: https://en.wikipedia.org/wiki/Transport_Layer_Security
