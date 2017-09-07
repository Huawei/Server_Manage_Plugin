Configuring PXE and iPXE
========================

PXE setup
---------

If you will be using PXE, it needs to be set up on the Bare Metal service
node(s) where ``ironic-conductor`` is running.

#. Make sure the tftp root directory exist and can be written to by the
   user the ``ironic-conductor`` is running as. For example::

    sudo mkdir -p /tftpboot
    sudo chown -R ironic /tftpboot

#. Install tftp server and the syslinux package with the PXE boot images:

   Ubuntu (Up to and including 14.04)::

       sudo apt-get install xinetd tftpd-hpa syslinux-common syslinux

   Ubuntu (14.10 and after)::

       sudo apt-get install xinetd tftpd-hpa syslinux-common pxelinux

   Fedora 21/RHEL7/CentOS7::

       sudo yum install tftp-server syslinux-tftpboot xinetd

   Fedora 22 or higher::

       sudo dnf install tftp-server syslinux-tftpboot xinetd

   SUSE::

       sudo zypper install tftp syslinux xinetd

#. Using xinetd to provide a tftp server setup to serve ``/tftpboot``.
   Create or edit ``/etc/xinetd.d/tftp`` as below::

    service tftp
    {
      protocol        = udp
      port            = 69
      socket_type     = dgram
      wait            = yes
      user            = root
      server          = /usr/sbin/in.tftpd
      server_args     = -v -v -v -v -v --map-file /tftpboot/map-file /tftpboot
      disable         = no
      # This is a workaround for Fedora, where TFTP will listen only on
      # IPv6 endpoint, if IPv4 flag is not used.
      flags           = IPv4
    }

   and restart the ``xinetd`` service:

   Ubuntu::

       sudo service xinetd restart

   Fedora/RHEL7/CentOS7/SUSE::

       sudo systemctl restart xinetd

#. Copy the PXE image to ``/tftpboot``. The PXE image might be found at [1]_:

   Ubuntu (Up to and including 14.04)::

       sudo cp /usr/lib/syslinux/pxelinux.0 /tftpboot

   Ubuntu (14.10 and after)::

       sudo cp /usr/lib/PXELINUX/pxelinux.0 /tftpboot

   RHEL7/CentOS7/SUSE::

       sudo cp /usr/share/syslinux/pxelinux.0 /tftpboot

#. If whole disk images need to be deployed via PXE-netboot, copy the
   chain.c32 image to ``/tftpboot`` to support it:

   Ubuntu (Up to and including 14.04)::

       sudo cp /usr/lib/syslinux/chain.c32 /tftpboot

   Ubuntu (14.10 and after)::

       sudo cp /usr/lib/syslinux/modules/bios/chain.c32 /tftpboot

   Fedora::

       sudo cp /boot/extlinux/chain.c32 /tftpboot

   RHEL7/CentOS7/SUSE::

       sudo cp /usr/share/syslinux/chain.c32 /tftpboot/

#. If the version of syslinux is **greater than** 4 we also need to make sure
   that we copy the library modules into the ``/tftpboot`` directory [2]_
   [1]_. For example, for Ubuntu run::

       sudo cp /usr/lib/syslinux/modules/*/ldlinux.* /tftpboot

#. Create a map file in the tftp boot directory (``/tftpboot``)::

    echo 're ^(/tftpboot/) /tftpboot/\2' > /tftpboot/map-file
    echo 're ^/tftpboot/ /tftpboot/' >> /tftpboot/map-file
    echo 're ^(^/) /tftpboot/\1' >> /tftpboot/map-file
    echo 're ^([^/]) /tftpboot/\1' >> /tftpboot/map-file

.. [1] On **Fedora/RHEL** the ``syslinux-tftpboot`` package already install
       the library modules and PXE image at ``/tftpboot``. If the TFTP server
       is configured to listen to a different directory you should copy the
       contents of ``/tftpboot`` to the configured directory
.. [2] http://www.syslinux.org/wiki/index.php/Library_modules


PXE UEFI setup
--------------

If you want to deploy on a UEFI supported bare metal, perform these additional
steps on the ironic conductor node to configure the PXE UEFI environment.

#. Install Grub2 and shim packages:

   Ubuntu (14.04LTS and later)::

       sudo apt-get install grub-efi-amd64-signed shim-signed

   Fedora 21/RHEL7/CentOS7::

       sudo yum install grub2-efi shim

   Fedora 22 or higher::

       sudo dnf install grub2-efi shim

   SUSE::

       sudo zypper install grub2-x86_64-efi shim

#. Copy grub and shim boot loader images to ``/tftpboot`` directory:

   Ubuntu (14.04LTS and later)::

       sudo cp /usr/lib/shim/shim.efi.signed /tftpboot/bootx64.efi
       sudo cp /usr/lib/grub/x86_64-efi-signed/grubnetx64.efi.signed /tftpboot/grubx64.efi

   Fedora: (21 and later)::

       sudo cp /boot/efi/EFI/fedora/shim.efi /tftpboot/bootx64.efi
       sudo cp /boot/efi/EFI/fedora/grubx64.efi /tftpboot/grubx64.efi

   RHEL7/CentOS7::

       sudo cp /boot/efi/EFI/centos/shim.efi /tftpboot/bootx64.efi
       sudo cp /boot/efi/EFI/centos/grubx64.efi /tftpboot/grubx64.efi

   SUSE::

       sudo cp /usr/lib64/efi/shim.efi /tftpboot/bootx64.efi
       sudo cp /usr/lib/grub2/x86_64-efi/grub.efi /tftpboot/grubx64.efi

#. Create master grub.cfg:

   Ubuntu: Create grub.cfg under ``/tftpboot/grub`` directory::

       GRUB_DIR=/tftpboot/grub

   Fedora: Create grub.cfg under ``/tftpboot/EFI/fedora`` directory::

        GRUB_DIR=/tftpboot/EFI/fedora

   RHEL7/CentOS7: Create grub.cfg under ``/tftpboot/EFI/centos`` directory::

       GRUB_DIR=/tftpboot/EFI/centos

   SUSE: Create grub.cfg under ``/tftpboot/boot/grub`` directory::

       GRUB_DIR=/tftpboot/boot/grub

   Create directory ``GRUB_DIR``::

     sudo mkdir -p $GRUB_DIR

   This file is used to redirect grub to baremetal node specific config file.
   It redirects it to specific grub config file based on DHCP IP assigned to
   baremetal node.

   .. literalinclude:: ../../../ironic/drivers/modules/master_grub_cfg.txt

   Change the permission of grub.cfg::

    sudo chmod 644 $GRUB_DIR/grub.cfg

#. Update the bare metal node with ``boot_mode`` capability in node's properties
   field::

    ironic node-update <node-uuid> add properties/capabilities='boot_mode:uefi'

#. Make sure that bare metal node is configured to boot in UEFI boot mode and
   boot device is set to network/pxe.

   NOTE: ``pxe_ilo`` driver supports automatic setting of UEFI boot mode and
   boot device on the bare metal node. So this step is not required for
   ``pxe_ilo`` driver.

.. note::
  For more information on configuring boot modes, see :ref:`boot_mode_support`.


iPXE setup
----------

If you will be using iPXE to boot instead of PXE, iPXE needs to be set up
on the Bare Metal service node(s) where ``ironic-conductor`` is running.

#. Make sure these directories exist and can be written to by the user
   the ``ironic-conductor`` is running as. For example::

    sudo mkdir -p /tftpboot
    sudo mkdir -p /httpboot
    sudo chown -R ironic /tftpboot
    sudo chown -R ironic /httpboot

#. Create a map file in the tftp boot directory (``/tftpboot``)::

    echo 'r ^([^/]) /tftpboot/\1' > /tftpboot/map-file
    echo 'r ^(/tftpboot/) /tftpboot/\2' >> /tftpboot/map-file

   .. _HTTP server:

#. Set up TFTP and HTTP servers.

   These servers should be running and configured to use the local
   /tftpboot and /httpboot directories respectively, as their root
   directories. (Setting up these servers is outside the scope of this
   install guide.)

   These root directories need to be mounted locally to the
   ``ironic-conductor`` services, so that the services can access them.

   The Bare Metal service's configuration file (/etc/ironic/ironic.conf)
   should be edited accordingly to specify the TFTP and HTTP root
   directories and server addresses. For example:

   .. code-block:: ini

      [pxe]

      # Ironic compute node's tftp root path. (string value)
      tftp_root=/tftpboot

      # IP address of Ironic compute node's tftp server. (string
      # value)
      tftp_server=192.168.0.2

      [deploy]
      # Ironic compute node's http root path. (string value)
      http_root=/httpboot

      # Ironic compute node's HTTP server URL. Example:
      # http://192.1.2.3:8080 (string value)
      http_url=http://192.168.0.2:8080

#. Install the iPXE package with the boot images:

   Ubuntu::

       apt-get install ipxe

   Fedora 21/RHEL7/CentOS7::

       yum install ipxe-bootimgs

   Fedora 22 or higher::

       dnf install ipxe-bootimgs

   .. note::
      SUSE does not provide a package containing iPXE boot images. If you are
      using SUSE or if the packaged version of the iPXE boot image doesn't
      work, you can download a prebuilt one from http://boot.ipxe.org or build
      one image from source, see http://ipxe.org/download for more information.

#. Copy the iPXE boot image (``undionly.kpxe`` for **BIOS** and
   ``ipxe.efi`` for **UEFI**) to ``/tftpboot``. The binary might
   be found at:

   Ubuntu::

       cp /usr/lib/ipxe/{undionly.kpxe,ipxe.efi} /tftpboot

   Fedora/RHEL7/CentOS7::

       cp /usr/share/ipxe/{undionly.kpxe,ipxe.efi} /tftpboot

#. Enable/Configure iPXE in the Bare Metal Service's configuration file
   (/etc/ironic/ironic.conf):

   .. code-block:: ini

      [pxe]

      # Enable iPXE boot. (boolean value)
      ipxe_enabled=True

      # Neutron bootfile DHCP parameter. (string value)
      pxe_bootfile_name=undionly.kpxe

      # Bootfile DHCP parameter for UEFI boot mode. (string value)
      uefi_pxe_bootfile_name=ipxe.efi

      # Template file for PXE configuration. (string value)
      pxe_config_template=$pybasedir/drivers/modules/ipxe_config.template

      # Template file for PXE configuration for UEFI boot loader.
      # (string value)
      uefi_pxe_config_template=$pybasedir/drivers/modules/ipxe_config.template


#. It is possible to configure the Bare Metal service in such a way
   that nodes will boot into the deploy image directly from Object Storage.
   Doing this avoids having to cache the images on the ironic-conductor
   host and serving them via the ironic-conductor's `HTTP server`_.
   This can be done if:

   #. the Image Service is used for image storage;
   #. the images in the Image Service are internally stored in
      Object Storage;
   #. the Object Storage supports generating temporary URLs
      for accessing objects stored in it.
      Both the OpenStack Swift and RADOS Gateway provide support for this.

      * See :doc:`/admin/radosgw` on how to configure
        the Bare Metal Service with RADOS Gateway as the Object Storage.

   Configure this by setting the ``[pxe]/ipxe_use_swift`` configuration
   option to ``True`` as follows:

   .. code-block:: ini

      [pxe]

      # Download deploy images directly from swift using temporary
      # URLs. If set to false (default), images are downloaded to
      # the ironic-conductor node and served over its local HTTP
      # server. Applicable only when 'ipxe_enabled' option is set to
      # true. (boolean value)
      ipxe_use_swift=True

   Although the `HTTP server`_ still has to be deployed and configured
   (as it will serve iPXE boot script and boot configuration files for nodes),
   such configuration will shift some load from ironic-conductor hosts
   to the Object Storage service which can be scaled horizontally.

   Note that when SSL is enabled on the Object Storage service
   you have to ensure that iPXE firmware on the nodes can indeed
   boot from generated temporary URLs that use HTTPS protocol.

#. Restart the ``ironic-conductor`` process:

   Fedora/RHEL7/CentOS7/SUSE::

     sudo systemctl restart openstack-ironic-conductor

   Ubuntu::

     sudo service ironic-conductor restart


PXE multi-architecture setup
----------------------------

It is possible to deploy servers of different architecture by one conductor.
To use this feature, architecture-specific boot and template files must
be configured using the configuration options
``[pxe]pxe_bootfile_name_by_arch`` and ``[pxe]pxe_config_template_by_arch``
respectively, in the Bare Metal service's configuration file
(/etc/ironic/ironic.conf).

These two options are dictionary values; the key is the architecture and the
value is the boot (or config template) file. A node's ``cpu_arch`` property is
used as the key to get the appropriate boot file and template file. If the
node's ``cpu_arch`` is not in the dictionary, the configuration options (in
[pxe] group) ``pxe_bootfile_name``, ``pxe_config_template``,
``uefi_pxe_bootfile_name`` and ``uefi_pxe_config_template`` will be used
instead.

In the following example, since 'x86' and 'x86_64' keys are not in the
``pxe_bootfile_name_by_arch`` or ``pxe_config_template_by_arch`` options, x86
and x86_64 nodes will be deployed by 'pxelinux.0' or 'bootx64.efi', depending
on the node's ``boot_mode`` capability ('bios' or 'uefi'). However, aarch64
nodes will be deployed by 'grubaa64.efi', and ppc64 nodes by 'bootppc64'::

    [pxe]

    # Bootfile DHCP parameter. (string value)
    pxe_bootfile_name=pxelinux.0

    # On ironic-conductor node, template file for PXE
    # configuration. (string value)
    pxe_config_template = $pybasedir/drivers/modules/pxe_config.template

    # Bootfile DHCP parameter for UEFI boot mode. (string value)
    uefi_pxe_bootfile_name=bootx64.efi

    # On ironic-conductor node, template file for PXE
    # configuration for UEFI boot loader. (string value)
    uefi_pxe_config_template=$pybasedir/drivers/modules/pxe_grub_config.template

    # Bootfile DHCP parameter per node architecture. (dict value)
    pxe_bootfile_name_by_arch=aarch64:grubaa64.efi,ppc64:bootppc64

    # On ironic-conductor node, template file for PXE
    # configuration per node architecture. For example:
    # aarch64:/opt/share/grubaa64_pxe_config.template (dict value)
    pxe_config_template_by_arch=aarch64:pxe_grubaa64_config.template,ppc64:pxe_ppc64_config.template
