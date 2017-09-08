=====
Sushy
=====

Sushy is a Python library to communicate with `Redfish`_ based systems.

The goal of the library is to be extremely simple, small, have as few
dependencies as possible and be very conservative when dealing with BMCs
by issuing just enough requests to it (BMCs are very flaky).

Therefore, the scope of the library has been limited to what is supported
by the `OpenStack Ironic <https://wiki.openstack.org/wiki/Ironic>`_
project. As the project grows and more features from `Redfish`_ are
needed we can expand Sushy to fullfil those requirements.

* Free software: Apache license
* Documentation: http://sushy.rtfd.io
* Usage: http://sushy.readthedocs.io/en/latest/usage.html
* Source: http://git.openstack.org/cgit/openstack/sushy
* Bugs: http://bugs.launchpad.net/sushy

.. _Redfish: http://www.dmtf.org/standards/redfish
