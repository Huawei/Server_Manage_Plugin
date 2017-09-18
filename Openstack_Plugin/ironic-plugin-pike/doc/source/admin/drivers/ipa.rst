.. _IPA:

===================
Ironic Python Agent
===================

Overview
========

*Ironic Python Agent* (also often called *IPA* or just *agent*) is a
Python-based agent which handles *ironic* bare metal nodes in a
variety of actions such as inspect, configure, clean and deploy images.
IPA is distributed over nodes and runs, inside of a ramdisk, the
process of booting this ramdisk on the node.

For more information see the `ironic-python-agent documentation
<https://docs.openstack.org/ironic-python-agent/latest>`_.

Drivers
=======

Starting with the Kilo release all drivers (except for fake ones) are using
IPA for deployment. There are two types of them, which can be distinguished
by prefix:

* For drivers with ``pxe_`` or ``iscsi_`` prefix IPA exposes the root hard
  drive as an iSCSI share and calls back to the ironic conductor. The
  conductor mounts the share and copies an image there. It then signals back
  to IPA for post-installation actions like setting up a bootloader for local
  boot support.

* For drivers with ``agent_`` prefix the conductor prepares a swift temporary
  URL for an image. IPA then handles the whole deployment process:
  downloading an image from swift, putting it on the machine and doing any
  post-deploy actions.

Which one to choose depends on your environment. iSCSI-based drivers put
higher load on conductors, agent-based drivers currently require the whole
image to fit in the node's memory.

.. todo: other differences?

.. todo: explain configuring swift for temporary URL's

Requirements
------------

Using IPA requires it to be present and configured on the deploy ramdisk, see
:ref:`deploy-ramdisk`

Using proxies for image download in agent drivers
=================================================

Overview
--------

IPA supports using proxies while downloading the user image. For example, this
could be used to speed up download by using caching proxy.

Steps to enable proxies
-----------------------

#. Configure the proxy server of your choice (for example
   `Squid <http://www.squid-cache.org/Doc/>`_,
   `Apache Traffic Server <https://docs.trafficserver.apache.org/en/latest/index.html>`_).
   This will probably require you to configure the proxy server to cache the
   content even if the requested URL contains a query, and to raise the maximum
   cached file size as images can be pretty big. If you have HTTPS enabled in
   swift (see `swift deployment guide <https://docs.openstack.org/swift/latest/deployment_guide.html>`_),
   it is possible to configure the proxy server to talk to swift via HTTPS
   to download the image, store it in the cache unencrypted and return it to
   the node via HTTPS again. Because the image will be stored unencrypted in
   the cache, this approach is recommended for images that do not contain
   sensitive information. Refer to your proxy server's documentation to
   complete this step.

#. Set ``[glance]swift_temp_url_cache_enabled`` in the ironic conductor config
   file to ``True``. The conductor will reuse the cached swift temporary URLs
   instead of generating new ones each time an image is requested, so that the
   proxy server does not create new cache entries for the same image, based on
   the query part of the URL (as it contains some query parameters that change
   each time it is regenerated).

#. Set ``[glance]swift_temp_url_expected_download_start_delay`` option in the
   ironic conductor config file to the value appropriate for your hardware.
   This is the delay (in seconds) from the time of the deploy request (when
   the swift temporary URL is generated) to when the URL is used for the image
   download. You can think of it as roughly the time needed for IPA ramdisk to
   startup and begin download. This value is used to check if the swift
   temporary URL duration is large enough to let the image download begin. Also
   if temporary URL caching is enabled, this will determine if a cached entry
   will still be valid when the download starts. It is used only if
   ``[glance]swift_temp_url_cache_enabled`` is ``True``.

#. Increase ``[glance]swift_temp_url_duration`` option in the ironic conductor
   config file, as only non-expired links to images will be returned from the
   swift temporary URLs cache. This means that if
   ``swift_temp_url_duration=1200`` then after 20 minutes a new image will be
   cached by the proxy server as the query in its URL will change. The value of
   this option must be greater than or equal to
   ``[glance]swift_temp_url_expected_download_start_delay``.

#. Add one or more of ``image_http_proxy``, ``image_https_proxy``,
   ``image_no_proxy`` to driver_info properties in each node that will use the
   proxy. Please refer to ``ironic driver-properties`` output of the
   ``agent_*`` driver you're using for descriptions of these properties.

Advanced configuration
======================

Out-of-band vs. in-band power off on deploy
-------------------------------------------

After deploying an image onto the node's hard disk, Ironic will reboot
the machine into the new image. By default this power action happens
``in-band``, meaning that the ironic-conductor will instruct the IPA
ramdisk to power itself off.

Some hardware may have a problem with the default approach and
would require Ironic to talk directly to the management controller
to switch the power off and on again. In order to tell Ironic to do
that, you have to update the node's ``driver_info`` field and set the
``deploy_forces_oob_reboot`` parameter with the value of **True**. For
example, the below command sets this configuration in a specific node::

  ironic node-update <UUID or name> add driver_info/deploy_forces_oob_reboot=True
