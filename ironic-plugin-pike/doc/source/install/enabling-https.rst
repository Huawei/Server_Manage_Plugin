.. _enabling-https:

Enabling HTTPS
--------------

.. _EnableHTTPSinSwift:

Enabling HTTPS in Swift
=======================

The drivers using virtual media use swift for storing boot images
and node configuration information (contains sensitive information for Ironic
conductor to provision bare metal hardware).  By default, HTTPS is not enabled
in swift. HTTPS is required to encrypt all communication between swift and Ironic
conductor and swift and bare metal (via virtual media).  It can be enabled in one
of the following ways:

* `Using an SSL termination proxy
  <https://docs.openstack.org/security-guide/secure-communication/tls-proxies-and-http-services.html>`_

* `Using native SSL support in swift
  <https://docs.openstack.org/swift/pike/deployment_guide.html>`_
  (recommended only for testing purpose by swift).

.. _EnableHTTPSinGlance:

Enabling HTTPS in Image service
===============================

Ironic drivers usually use Image service during node provisioning. By default,
image service does not use HTTPS, but it is required for secure communication.
It can be enabled by making the following changes to ``/etc/glance/glance-api.conf``:

#. `Configuring SSL support
   <https://docs.openstack.org/glance/pike/configuration/configuring.html#configuring-ssl-support>`_

#. Restart the glance-api service::

    Fedora/RHEL7/CentOS7/SUSE:
        sudo systemctl restart openstack-glance-api

    Debian/Ubuntu:
        sudo service glance-api restart

See the `Glance <https://docs.openstack.org/glance/pike/>`_ documentation,
for more details on the Image service.

Enabling HTTPS communication between Image service and Object storage
=====================================================================

This section describes the steps needed to enable secure HTTPS communication between
Image service and Object storage when Object storage is used as the Backend.

To enable secure HTTPS communication between Image service and Object storage follow these steps:

#. :ref:`EnableHTTPSinSwift`

#.  `Configure Swift Storage Backend
    <https://docs.openstack.org/glance/pike/configuration/configuring.html#configuring-the-swift-storage-backend>`_

#. :ref:`EnableHTTPSinGlance`

Enabling HTTPS communication between Image service and Bare Metal service
=========================================================================

This section describes the steps needed to enable secure HTTPS communication between
Image service and Bare Metal service.

To enable secure HTTPS communication between Bare Metal service and Image service follow these steps:

#. Edit ``/etc/ironic/ironic.conf``::

    [glance]
    ...
    glance_cafile=/path/to/certfile
    glance_protocol=https
    glance_api_insecure=False

   .. note::
      'glance_cafile' is a optional path to a CA certificate bundle to be used to validate the SSL certificate
      served by Image service.

#. Restart ironic-conductor service::

    Fedora/RHEL7/CentOS7/SUSE:
        sudo systemctl restart openstack-ironic-conductor

    Debian/Ubuntu:
        sudo service ironic-conductor restart
