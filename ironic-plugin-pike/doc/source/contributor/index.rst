Developer's Guide
=================

Getting Started
---------------

If you are new to ironic, this section contains information that should help
you get started as a developer working on the project or contributing to the
project.

.. toctree::
  :maxdepth: 1

  Developer Contribution Guide <code-contribution-guide>
  Setting Up Your Development Environment <dev-quickstart>
  Frequently Asked Questions <faq>

The following pages describe the architecture of the Bare Metal service
and may be helpful to anyone working on or with the service, but are written
primarily for developers.

.. toctree::
  :maxdepth: 1

  Ironic System Architecture <architecture>
  Provisioning State Machine <states>
  Developing New Notifications <notifications>
  OSProfiler Tracing <osprofiler-support>

These pages contain information for PTLs, cross-project liaisons, and core
reviewers.

.. toctree::
  :maxdepth: 1

  Releasing Ironic Projects <releasing>
  Ironic Governance Structure <governance>


Writing Drivers
---------------

Ironic's community includes many hardware vendors who contribute drivers that
enable more advanced functionality when Ironic is used in conjunction with that
hardware. To do this, the Ironic developer community is committed to
standardizing on a `Python Driver API <api/ironic.drivers.base.html>`_ that
meets the common needs of all hardware vendors, and evolving this API without
breaking backwards compatibility. However, it is sometimes necessary for driver
authors to implement functionality - and expose it through the REST API - that
can not be done through any existing API.

To facilitate that, we also provide the means for API calls to be "passed
through" ironic and directly to the driver. Some guidelines on how to implement
this are provided below. Driver authors are strongly encouraged to talk with
the developer community about any implementation using this functionality.

.. toctree::
  :maxdepth: 1

  Driver Overview <drivers>
  Driver Base Class Definition <api/ironic.drivers.base>
  Writing "vendor_passthru" methods <vendor-passthru>
  Third party continuous integration testing <third-party-ci>

Testing Network Integration
---------------------------

In order to test the integration between the Bare Metal and Networking
services, support has been added to `devstack <https://launchpad.net/devstack>`_
to mimic an external physical switch.  Here we include a recommended
configuration for devstack to bring up this environment.

.. toctree::
  :maxdepth: 1

  Configuring Devstack for multitenant network testing <ironic-multitenant-networking>

Testing Boot-from-Volume
------------------------

Starting with the Pike release, it is also possible to use DevStack for testing
booting from Cinder volumes with VMs.

.. toctree::
  :maxdepth: 1

  Configuring Devstack for boot-from-volume testing <ironic-boot-from-volume>

Full Ironic Server Python API Reference
---------------------------------------

* :ref:`modindex`

.. # api/autoindex is hidden since it's in the modindex link above.
.. toctree::
  :hidden:

  api/autoindex
