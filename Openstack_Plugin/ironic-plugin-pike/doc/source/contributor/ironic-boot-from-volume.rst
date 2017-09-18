=====================================
Ironic Boot-from-Volume with DevStack
=====================================

This guide shows how to setup DevStack for enabling boot-from-volume feature,
which has been supported from the Pike release.

This scenario shows how to setup DevStack to enable nodes to boot from volumes
managed by cinder with VMs as baremetal servers.

DevStack Configuration
======================

The following is ``local.conf`` that will setup DevStack with 3 VMs that are
registered in ironic. A volume connector with IQN is created for each node.
These connectors can be used to connect volumes created by cinder. The detailed
description for DevStack is at :ref:`deploy_devstack`.

::

    [[local|localrc]]

    enable_plugin ironic git://git.openstack.org/openstack/ironic

    IRONIC_STORAGE_INTERFACE=cinder

    # Credentials
    ADMIN_PASSWORD=password
    DATABASE_PASSWORD=password
    RABBIT_PASSWORD=password
    SERVICE_PASSWORD=password
    SERVICE_TOKEN=password
    SWIFT_HASH=password
    SWIFT_TEMPURL_KEY=password

    # Enable Neutron which is required by Ironic and disable nova-network.
    disable_service n-net
    disable_service n-novnc
    enable_service q-svc
    enable_service q-agt
    enable_service q-dhcp
    enable_service q-l3
    enable_service q-meta
    enable_service neutron

    # Enable Swift for agent_* drivers
    enable_service s-proxy
    enable_service s-object
    enable_service s-container
    enable_service s-account

    # Disable Horizon
    disable_service horizon

    # Disable Heat
    disable_service heat h-api h-api-cfn h-api-cw h-eng

    # Swift temp URL's are required for agent_* drivers.
    SWIFT_ENABLE_TEMPURLS=True

    # Create 3 virtual machines to pose as Ironic's baremetal nodes.
    IRONIC_VM_COUNT=3
    IRONIC_BAREMETAL_BASIC_OPS=True
    DEFAULT_INSTANCE_TYPE=baremetal

    # Enable Ironic drivers.
    IRONIC_ENABLED_DRIVERS=fake,agent_ipmitool,pxe_ipmitool

    # Change this to alter the default driver for nodes created by devstack.
    # This driver should be in the enabled list above.
    IRONIC_DEPLOY_DRIVER=agent_ipmitool

    # The parameters below represent the minimum possible values to create
    # functional nodes.
    IRONIC_VM_SPECS_RAM=1280
    IRONIC_VM_SPECS_DISK=10

    # Size of the ephemeral partition in GB. Use 0 for no ephemeral partition.
    IRONIC_VM_EPHEMERAL_DISK=0

    # To build your own IPA ramdisk from source, set this to True
    IRONIC_BUILD_DEPLOY_RAMDISK=False

    VIRT_DRIVER=ironic

    # By default, DevStack creates a 10.0.0.0/24 network for instances.
    # If this overlaps with the hosts network, you may adjust with the
    # following.
    NETWORK_GATEWAY=10.1.0.1
    FIXED_RANGE=10.1.0.0/24
    FIXED_NETWORK_SIZE=256

    # Log all output to files
    LOGFILE=$HOME/devstack.log
    LOGDIR=$HOME/logs
    IRONIC_VM_LOG_DIR=$HOME/ironic-bm-logs

After the environment is built, you can create a volume with cinder and request
an instance with the volume to nova::

    source ~/devstack/openrc

    # query the image id of the default cirros image
    image=$(openstack image show $DEFAULT_IMAGE_NAME -f value -c id)

    # create keypair
    ssh-keygen
    openstack keypair create --public-key ~/.ssh/id_rsa.pub default

    # create volume
    volume=$(openstack volume create --image $image --size 1 my-volume -f value -c id)

    # spawn instance
    openstack server create --flavor baremetal --volume $volume --key-name default testing

You can also run an integration test that an instance is booted from a remote
volume with tempest in the environment::

    cd /opt/stack/tempest
    tox -e all-plugin -- ironic_tempest_plugin.tests.scenario.test_baremetal_boot_from_volume
