.. _troubleshooting:

======================
Troubleshooting Ironic
======================

Nova returns "No valid host was found" Error
============================================

Sometimes Nova Conductor log file "nova-conductor.log" or a message returned
from Nova API contains the following error::

    NoValidHost: No valid host was found. There are not enough hosts available.

"No valid host was found" means that the Nova Scheduler could not find a bare
metal node suitable for booting the new instance.

This in turn usually means some mismatch between resources that Nova expects
to find and resources that Ironic advertised to Nova.

A few things should be checked in this case:

#. Make sure that enough nodes are in ``available`` state, not in
   maintenance mode and not already used by an existing instance.
   Check with the following command::

       ironic node-list --provision-state available --maintenance false --associated false

   If this command does not show enough nodes, use generic ``ironic
   node-list`` to check other nodes. For example, nodes in ``manageable`` state
   should be made available::

       ironic node-set-provision-state <IRONIC NODE> provide

   The Bare metal service automatically puts a node in maintenance mode if
   there are issues with accessing its management interface. Check the power
   credentials (e.g. ``ipmi_address``, ``ipmi_username`` and ``ipmi_password``)
   and then move the node out of maintenance mode::

       ironic node-set-maintenance <IRONIC NODE> off

   The ``node-validate`` command can be used to verify that all required fields
   are present. The following command should not return anything::

       ironic node-validate baremetal-0 | grep -E '(power|management)\W*False'

   Maintenance mode will be also set on a node if automated cleaning has
   failed for it previously.

#. Starting with the Pike release, check that all your nodes have the
   ``resource_class`` field set using the following command::

       openstack --os-baremetal-api-version 1.21 baremetal node list --fields uuid name resource_class

   Then check that the flavor(s) are configured to request these resource
   classes via their properties::

       openstack flavor show <FLAVOR NAME> -f value -c properties

   For example, if your node has resource class ``baremetal-large``, it will
   be matched by a flavor with property ``resources:CUSTOM_BAREMETAL_LARGE``
   set to ``1``. See :doc:`/install/configure-nova-flavors` for more
   details on the correct configuration.

#. If you do not use scheduling based on resource classes, then the node's
   properties must have been set either manually or via inspection.
   For each node with ``available`` state check that the ``properties``
   JSON field has valid values for the keys ``cpus``, ``cpu_arch``,
   ``memory_mb`` and ``local_gb``. Example of valid properties::

        $ ironic node-show <IRONIC NODE> --fields properties
        +------------+------------------------------------------------------------------------------------+
        | Property   | Value                                                                              |
        +------------+------------------------------------------------------------------------------------+
        | properties | {u'memory_mb': u'8192', u'cpu_arch': u'x86_64', u'local_gb': u'41', u'cpus': u'4'} |
        +------------+------------------------------------------------------------------------------------+

   .. warning::
       If you're using exact match filters in the Nova Scheduler, make sure
       the flavor and the node properties match exactly.

#. The Nova flavor that you are using does not match any properties of the
   available Ironic nodes. Use
   ::

        openstack flavor show <FLAVOR NAME>

   to compare. The extra specs in your flavor starting with ``capability:``
   should match ones in ``node.properties['capabilities']``.

   .. note::
      The format of capabilities is different in Nova and Ironic.
      E.g. in Nova flavor::

        $ openstack flavor show <FLAVOR NAME> -c properties
        +------------+----------------------------------+
        | Field      | Value                            |
        +------------+----------------------------------+
        | properties | capabilities:boot_option='local' |
        +------------+----------------------------------+

      But in Ironic node::

        $ ironic node-show <IRONIC NODE> --fields properties
        +------------+-----------------------------------------+
        | Property   | Value                                   |
        +------------+-----------------------------------------+
        | properties | {u'capabilities': u'boot_option:local'} |
        +------------+-----------------------------------------+

#. After making changes to nodes in Ironic, it takes time for those changes
   to propagate from Ironic to Nova. Check that
   ::

        openstack hypervisor stats show

   correctly shows total amount of resources in your system. You can also
   check ``openstack hypervisor show <IRONIC NODE>`` to see the status of
   individual Ironic nodes as reported to Nova.

   .. TODO(dtantsur): explain inspecting the placement API

#. Figure out which Nova Scheduler filter ruled out your nodes. Check the
   ``nova-scheduler`` logs for lines containing something like::

        Filter ComputeCapabilitiesFilter returned 0 hosts

   The name of the filter that removed the last hosts may give some hints on
   what exactly was not matched. See `Nova filters documentation
   <https://docs.openstack.org/nova/latest/filter_scheduler.html>`_ for more
   details.

#. If none of the above helped, check Ironic conductor log carefully to see
   if there are any conductor-related errors which are the root cause for
   "No valid host was found". If there are any "Error in deploy of node
   <IRONIC-NODE-UUID>: [Errno 28] ..." error messages in Ironic conductor
   log, it means the conductor run into a special error during deployment.
   So you can check the log carefully to fix or work around and then try
   again.

Patching the Deploy Ramdisk
===========================

When debugging a problem with deployment and/or inspection you may want to
quickly apply a change to the ramdisk to see if it helps. Of course you can
inject your code and/or SSH keys during the ramdisk build (depends on how
exactly you've built your ramdisk). But it's also possible to quickly modify
an already built ramdisk.

Create an empty directory and unpack the ramdisk content there::

    mkdir unpack
    cd unpack
    gzip -dc /path/to/the/ramdisk | cpio -id

The last command will result in the whole Linux file system tree unpacked in
the current directory. Now you can modify any files you want. The actual
location of the files will depend on the way you've built the ramdisk.

After you've done the modifications, pack the whole content of the current
directory back::

    find . | cpio -H newc -o > /path/to/the/new/ramdisk

.. note:: You don't need to modify the kernel (e.g.
          ``tinyipa-master.vmlinuz``), only the ramdisk part.

.. note:: For CoreOS-based ramdisk you also need to unpack and pack back the
          squashfs archive inside the unpacked ramdisk.

API Errors
==========

The `debug_tracebacks_in_api` config option may be set to return tracebacks
in the API response for all 4xx and 5xx errors.

Retrieving logs from the deploy ramdisk
=======================================

When troubleshooting deployments (specially in case of a deploy failure)
it's important to have access to the deploy ramdisk logs to be able to
identify the source of the problem. By default, Ironic will retrieve the
logs from the deploy ramdisk when the deployment fails and save it on the
local filesystem at ``/var/log/ironic/deploy``.

To change this behavior, operators can make the following changes to
``/etc/ironic/ironic.conf`` under the ``[agent]`` group:

* ``deploy_logs_collect``:  Whether Ironic should collect the deployment
  logs on deployment. Valid values for this option are:

  * ``on_failure`` (**default**): Retrieve the deployment logs upon a
    deployment failure.

  * ``always``: Always retrieve the deployment logs, even if the
    deployment succeed.

  * ``never``: Disable retrieving the deployment logs.

* ``deploy_logs_storage_backend``: The name of the storage backend where
  the logs will be stored. Valid values for this option are:

  * ``local`` (**default**): Store the logs in the local filesystem.

  * ``swift``: Store the logs in Swift.

* ``deploy_logs_local_path``: The path to the directory where the
  logs should be stored, used when the ``deploy_logs_storage_backend``
  is configured to ``local``. By default logs will be stored at
  **/var/log/ironic/deploy**.

* ``deploy_logs_swift_container``: The name of the Swift container to
  store the logs, used when the deploy_logs_storage_backend is configured to
  "swift". By default **ironic_deploy_logs_container**.

* ``deploy_logs_swift_days_to_expire``: Number of days before a log object
  is marked as expired in Swift. If None, the logs will be kept forever
  or until manually deleted. Used when the deploy_logs_storage_backend is
  configured to "swift". By default **30** days.

When the logs are collected, Ironic will store a *tar.gz* file containing
all the logs according to the ``deploy_logs_storage_backend``
configuration option. All log objects will be named with the following
pattern::

  <node-uuid>[_<instance-uuid>]_<timestamp yyyy-mm-dd-hh:mm:ss>.tar.gz

.. note::
   The *instance_uuid* field is not required for deploying a node when
   Ironic is configured to be used in standalone mode. If present it
   will be appended to the name.


Accessing the log data
----------------------

When storing in the local filesystem
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

When storing the logs in the local filesystem, the log files can
be found at the path configured in the ``deploy_logs_local_path``
configuration option. For example, to find the logs from the node
``5e9258c4-cfda-40b6-86e2-e192f523d668``:

.. code-block:: bash

   $ ls /var/log/ironic/deploy | grep 5e9258c4-cfda-40b6-86e2-e192f523d668
   5e9258c4-cfda-40b6-86e2-e192f523d668_88595d8a-6725-4471-8cd5-c0f3106b6898_2016-08-08-13:52:12.tar.gz
   5e9258c4-cfda-40b6-86e2-e192f523d668_db87f2c5-7a9a-48c2-9a76-604287257c1b_2016-08-08-14:07:25.tar.gz

.. note::
   When saving the logs to the filesystem, operators may want to enable
   some form of rotation for the logs to avoid disk space problems.


When storing in Swift
~~~~~~~~~~~~~~~~~~~~~

When using Swift, operators can associate the objects in the
container with the nodes in Ironic and search for the logs for the node
``5e9258c4-cfda-40b6-86e2-e192f523d668`` using the **prefix** parameter.
For example:

.. code-block:: bash

  $ swift list ironic_deploy_logs_container -p 5e9258c4-cfda-40b6-86e2-e192f523d668
  5e9258c4-cfda-40b6-86e2-e192f523d668_88595d8a-6725-4471-8cd5-c0f3106b6898_2016-08-08-13:52:12.tar.gz
  5e9258c4-cfda-40b6-86e2-e192f523d668_db87f2c5-7a9a-48c2-9a76-604287257c1b_2016-08-08-14:07:25.tar.gz

To download a specific log from Swift, do:

.. code-block:: bash

   $ swift download ironic_deploy_logs_container "5e9258c4-cfda-40b6-86e2-e192f523d668_db87f2c5-7a9a-48c2-9a76-604287257c1b_2016-08-08-14:07:25.tar.gz"
   5e9258c4-cfda-40b6-86e2-e192f523d668_db87f2c5-7a9a-48c2-9a76-604287257c1b_2016-08-08-14:07:25.tar.gz [auth 0.341s, headers 0.391s, total 0.391s, 0.531 MB/s]

The contents of the log file
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The log is just a ``.tar.gz`` file that can be extracted as:

.. code-block:: bash

   $ tar xvf <file path>


The contents of the file may differ slightly depending on the distribution
that the deploy ramdisk is using:

* For distributions using ``systemd`` there will be a file called
  **journal** which contains all the system logs collected via the
  ``journalctl`` command.

* For other distributions, the ramdisk will collect all the contents of
  the ``/var/log`` directory.

For all distributions, the log file will also contain the output of
the following commands (if present): ``ps``, ``df``, ``ip addr`` and
``iptables``.

Here's one example when extracting the content of a log file for a
distribution that uses ``systemd``:

.. code-block:: bash

   $ tar xvf 5e9258c4-cfda-40b6-86e2-e192f523d668_88595d8a-6725-4471-8cd5-c0f3106b6898_2016-08-08-13:52:12.tar.gz
   df
   ps
   journal
   ip_addr
   iptables

DHCP during PXE or iPXE is inconsistent or unreliable
=====================================================

This can be caused by the spanning tree protocol delay on some switches. The
delay prevents the switch port moving to forwarding mode during the nodes
attempts to PXE, so the packets never make it to the DHCP server. To resolve
this issue you should set the switch port that connects to your baremetal nodes
as an edge or PortFast type port. Configured in this way the switch port will
move to forwarding mode as soon as the link is established. An example on how to
do that for a Cisco Nexus switch is:

.. code-block:: bash

    $ config terminal
    $ (config) interface eth1/11
    $ (config-if) spanning-tree port type edge
