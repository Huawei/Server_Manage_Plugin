========================
REST API Version History
========================

**1.34** (Pike)

    Adds a ``physical_network`` field to the port object. All ports in a
    portgroup must have the same value in their ``physical_network`` field.

**1.33** (Pike)

    Added ``storage_interface`` field to the node object to allow getting and
    setting the interface.
    Also added ``default_storage_interface`` and ``enabled_storage_interfaces``
    fields to the driver object to show the information.

**1.32** (Pike)

    Added new endpoints for remote volume configuration:

    * GET /v1/volume as a root for volume resources
    * GET /v1/volume/connectors for listing volume connectors
    * POST /v1/volume/connectors for creating a volume connector
    * GET /v1/volume/connectors/<UUID> for showing a volume connector
    * PATCH /v1/volume/connectors/<UUID> for updating a volume connector
    * DELETE /v1/volume/connectors/<UUID> for deleting a volume connector
    * GET /v1/volume/targets for listing volume targets
    * POST /v1/volume/targets for creating a volume target
    * GET /v1/volume/targets/<UUID> for showing a volume target
    * PATCH /v1/volume/targets/<UUID> for updating a volume target
    * DELETE /v1/volume/targets/<UUID> for deleting a volume target

    Volume resources also can be listed as sub resources of nodes:

    * GET /v1/nodes/<node identifier>/volume
    * GET /v1/nodes/<node identifier>/volume/connectors
    * GET /v1/nodes/<node identifier>/volume/targets

**1.31** (Ocata)

    Added the following fields to the node object, to allow getting and
    setting interfaces for a dynamic driver:

    * boot_interface
    * console_interface
    * deploy_interface
    * inspect_interface
    * management_interface
    * power_interface
    * raid_interface
    * vendor_interface

**1.30** (Ocata)

    Added dynamic driver APIs.

    * GET /v1/drivers now accepts a ``type`` parameter (optional, one of
      ``classic`` or ``dynamic``), to limit the result to only classic drivers
      or dynamic drivers (hardware types). Without this parameter, both
      classic and dynamic drivers are returned.

    * GET /v1/drivers now accepts a ``detail`` parameter (optional, one of
      ``True`` or ``False``), to show all fields for a driver. Defaults to
      ``False``.

    * GET /v1/drivers now returns an additional ``type`` field to show if the
      driver is classic or dynamic.

    * GET /v1/drivers/<name> now returns an additional ``type`` field to show
      if the driver is classic or dynamic.

    * GET /v1/drivers/<name> now returns additional fields that are null for
      classic drivers, and set as following for dynamic drivers:

        * The value of the default_<interface-type>_interface is the entrypoint
          name of the calculated default interface for that type:

            * default_boot_interface
            * default_console_interface
            * default_deploy_interface
            * default_inspect_interface
            * default_management_interface
            * default_network_interface
            * default_power_interface
            * default_raid_interface
            * default_vendor_interface

        * The value of the enabled_<interface-type>_interfaces is a list of
          entrypoint names of the enabled interfaces for that type:

            * enabled_boot_interfaces
            * enabled_console_interfaces
            * enabled_deploy_interfaces
            * enabled_inspect_interfaces
            * enabled_management_interfaces
            * enabled_network_interfaces
            * enabled_power_interfaces
            * enabled_raid_interfaces
            * enabled_vendor_interfaces

**1.29** (Ocata)

    Add a new management API to support inject NMI,
    'PUT /v1/nodes/(node_ident)/management/inject_nmi'.

**1.28** (Ocata)

    Add '/v1/nodes/<node identifier>/vifs' endpoint for attach, detach and list of VIFs.

**1.27** (Ocata)

    Add ``soft rebooting`` and ``soft power off`` as possible values
    for the ``target`` field of the power state change payload, and
    also add ``timeout`` field to it.

**1.26** (Ocata)

    Add portgroup ``mode`` and ``properties`` fields.

**1.25** (Ocata)

    Add possibility to unset chassis_uuid from a node.

**1.24** (Ocata)

    Added new endpoints '/v1/nodes/<node>/portgroups' and '/v1/portgroups/<portgroup>/ports'.
    Added new field ``port.portgroup_uuid``.

**1.23** (Ocata)

    Added '/v1/portgroups/ endpoint.

**1.22** (Newton, 6.1.0)

    Added endpoints for deployment ramdisks.

**1.21** (Newton, 6.1.0)

    Add node ``resource_class`` field.

**1.20** (Newton, 6.1.0)

    Add node ``network_interface`` field.

**1.19** (Newton, 6.1.0)

    Add ``local_link_connection`` and ``pxe_enabled`` fields to the port object.

**1.18** (Newton, 6.1.0)

    Add ``internal_info`` readonly field to the port object, that will be used
    by ironic to store internal port-related information.

**1.17** (Newton, 6.0.0)

    Addition of provision_state verb ``adopt`` which allows an operator
    to move a node from ``manageable`` state to ``active`` state without
    performing a deployment operation on the node. This is intended for
    nodes that have already been deployed by external means.

**1.16** (Mitaka, 5.0.0)

    Add ability to filter nodes by driver.

**1.15** (Mitaka, 5.0.0)

    Add ability to do manual cleaning when a node is in the manageable
    provision state via PUT v1/nodes/<identifier>/states/provision,
    target:clean, clean_steps:[...].

**1.14** (Liberty, 4.2.0)

    Make the following endpoints discoverable via Ironic API:

    * '/v1/nodes/<UUID or logical name>/states'
    * '/v1/drivers/<driver name>/properties'

**1.13** (Liberty, 4.2.0)

    Add a new verb ``abort`` to the API used to abort nodes in
    ``CLEANWAIT`` state.

**1.12** (Liberty, 4.2.0)

    This API version adds the following abilities:

    * Get/set ``node.target_raid_config`` and to get
      ``node.raid_config``.
    * Retrieve the logical disk properties for the driver.

**1.11** (Liberty, 4.0.0, breaking change)

    Newly registered nodes begin in the ``enroll`` provision state by default,
    instead of ``available``. To get them to the ``available`` state,
    the ``manage`` action must first be run to verify basic hardware control.
    On success the node moves to ``manageable`` provision state. Then the
    ``provide`` action must be run. Automated cleaning of the node is done and
    the node is made ``available``.

**1.10** (Liberty, 4.0.0)

    Logical node names support all RFC 3986 unreserved characters.
    Previously only valid fully qualified domain names could be used.

**1.9** (Liberty, 4.0.0)

    Add ability to filter nodes by provision state.

**1.8** (Liberty, 4.0.0)

    Add ability to return a subset of resource fields.

**1.7** (Liberty, 4.0.0)

    Add node ``clean_step`` field.

**1.6** (Kilo)

    Add :ref:`inspection` process: introduce ``inspecting`` and ``inspectfail``
    provision states, and ``inspect`` action that can be used when a node is in
    ``manageable`` provision state.

**1.5** (Kilo)

    Add logical node names that can be used to address a node in addition to
    the node UUID. Name is expected to be a valid `fully qualified domain
    name`_ in this version of API.

**1.4** (Kilo)

    Add ``manageable`` state and ``manage`` transition, which can be used to
    move a node to ``manageable`` state from ``available``.
    The node cannot be deployed in ``manageable`` state.
    This change is mostly a preparation for future inspection work
    and introduction of ``enroll`` provision state.

**1.3** (Kilo)

    Add node ``driver_internal_info`` field.

**1.2** (Kilo, breaking change)

    Renamed NOSTATE (``None`` in Python, ``null`` in JSON) node state to
    ``available``. This is needed to reduce confusion around ``None`` state,
    especially when future additions to the state machine land.

**1.1** (Kilo)

    This was the initial version when API versioning was introduced.
    Includes the following changes from Kilo release cycle:

    * Add node ``maintenance_reason`` field and an API endpoint to
      set/unset the node maintenance mode.

    * Add sync and async support for vendor passthru methods.

    * Vendor passthru endpoints support different HTTP methods, not only
      ``POST``.

    * Make vendor methods discoverable via the Ironic API.

    * Add logic to store the config drive passed by Nova.

    This has been the minimum supported version since versioning was
    introduced.

**1.0** (Juno)

    This version denotes Juno API and was never explicitly supported, as API
    versioning was not implemented in Juno, and **1.1** became the minimum
    supported version in Kilo.

.. _fully qualified domain name: https://en.wikipedia.org/wiki/Fully_qualified_domain_name
