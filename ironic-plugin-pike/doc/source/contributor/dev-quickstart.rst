.. _dev-quickstart:

=====================
Developer Quick-Start
=====================

This is a quick walkthrough to get you started developing code for Ironic.
This assumes you are already familiar with submitting code reviews to
an OpenStack project.

The gate currently runs the unit tests under Python 2.7 and Python 3.5. It
is strongly encouraged to run the unit tests locally prior to submitting a
patch.

.. note::
    Do not run unit tests on the same environment as devstack due to
    conflicting configuration with system dependencies.

.. note::
    This document is compatible with Python (3.5), Ubuntu (16.04) and Fedora (24).
    When referring to different versions of Python and OS distributions, this
    is explicitly stated.

.. seealso::

    https://docs.openstack.org/infra/manual/developers.html#development-workflow

Prepare Development System
==========================

System Prerequisites
--------------------

The following packages cover the prerequisites for a local development
environment on most current distributions. Instructions for getting set up with
non-default versions of Python and on older distributions are included below as
well.

- Ubuntu/Debian::

    sudo apt-get install build-essential python-dev libssl-dev python-pip libmysqlclient-dev libxml2-dev libxslt-dev libpq-dev git git-review libffi-dev gettext ipmitool psmisc graphviz libjpeg-dev

- Fedora 21/RHEL7/CentOS7::

    sudo yum install python-devel openssl-devel python-pip mysql-devel libxml2-devel libxslt-devel postgresql-devel git git-review libffi-devel gettext ipmitool psmisc graphviz gcc libjpeg-turbo-devel

  If using RHEL and yum reports "No package python-pip available" and "No
  package git-review available", use the EPEL software repository.
  Instructions can be found at `<https://fedoraproject.org/wiki/EPEL/FAQ#howtouse>`_.

- Fedora 22 or higher::

    sudo dnf install python-devel openssl-devel python-pip mysql-devel libxml2-devel libxslt-devel postgresql-devel git git-review libffi-devel gettext ipmitool psmisc graphviz gcc libjpeg-turbo-devel

  Additionally, if using Fedora 23, ``redhat-rpm-config`` package should be
  installed so that development virtualenv can be built successfully.

- openSUSE/SLE 12::

    sudo zypper install git git-review libffi-devel libmysqlclient-devel libopenssl-devel libxml2-devel libxslt-devel postgresql-devel python-devel python-nose python-pip gettext-runtime psmisc

  Graphviz is only needed for generating the state machine diagram. To install it
  on openSUSE or SLE 12, see
  `<https://software.opensuse.org/download.html?project=graphics&package=graphviz-plugins>`_.


Python Prerequisites
--------------------

If your distro has at least tox 1.8, use similar command to install
``python-tox`` package. Otherwise install this on all distros::

    sudo pip install -U tox

You may need to explicitly upgrade virtualenv if you've installed the one
from your OS distribution and it is too old (tox will complain). You can
upgrade it individually, if you need to::

    sudo pip install -U virtualenv


Running Unit Tests Locally
==========================

If you haven't already, Ironic source code should be pulled directly from git::

    # from your home or source directory
    cd ~
    git clone https://git.openstack.org/openstack/ironic
    cd ironic

Running Unit and Style Tests
----------------------------

All unit tests should be run using tox. To run Ironic's entire test suite::

    # to run the py27, py35 unit tests, and the style tests
    tox

To run a specific test or tests, use the "-e" option followed by the tox target
name. For example::

    # run the unit tests under py27 and also run the pep8 tests
    tox -epy27 -epep8

.. note::
    If tests are run under py27 and then run under py35 the following error may occur::

      db type could not be determined
      ERROR: InvocationError: '/home/ubuntu/ironic/.tox/py35/bin/ostestr'

    To overcome this error remove the file `.testrepository/times.dbm`
    and then run the py35 test.

You may pass options to the test programs using positional arguments.
To run a specific unit test, this passes the -r option and desired test
(regex string) to `os-testr <https://pypi.python.org/pypi/os-testr>`_::

    # run a specific test for Python 2.7
    tox -epy27 -- -r test_conductor

Debugging unit tests
--------------------

In order to break into the debugger from a unit test we need to insert
a breaking point to the code:

.. code-block:: python

  import pdb; pdb.set_trace()

Then run ``tox`` with the debug environment as one of the following::

  tox -e debug
  tox -e debug test_file_name
  tox -e debug test_file_name.TestClass
  tox -e debug test_file_name.TestClass.test_name

For more information see the `oslotest documentation
<https://docs.openstack.org/oslotest/latest/user/features.html#debugging-with-oslo-debug-helper>`_.

Database Setup
--------------

The unit tests need a local database setup, you can use
``tools/test-setup.sh`` to set up the database the same way as setup
in the OpenStack test systems.

Additional Tox Targets
----------------------

There are several additional tox targets not included in the default list, such
as the target which builds the documentation site.   See the ``tox.ini`` file
for a complete listing of tox targets. These can be run directly by specifying
the target name::

    # generate the documentation pages locally
    tox -edocs

    # generate the sample configuration file
    tox -egenconfig


Exercising the Services Locally
===============================

In addition to running automated tests, sometimes it can be helpful to actually
run the services locally, without needing a server in a remote datacenter.

If you would like to exercise the Ironic services in isolation within your local
environment, you can do this without starting any other OpenStack services. For
example, this is useful for rapidly prototyping and debugging interactions over
the RPC channel, testing database migrations, and so forth.

Here we describe two ways to install and configure the dependencies, either run
directly on your local machine or encapsulated in a virtual machine or
container.

Step 1: Create a Python virtualenv
----------------------------------

#. If you haven't already downloaded the source code, do that first::

    cd ~
    git clone https://git.openstack.org/openstack/ironic
    cd ironic

#. Create the Python virtualenv::

    tox -evenv --notest --develop -r

#. Activate the virtual environment::

    source .tox/venv/bin/activate

#. Install the ironic client::

    pip install python-ironicclient

   .. note:: You can install python-ironicclient from source by cloning the git
             repository and running `pip install .` while in the root of the
             cloned repository.

#. Export some ENV vars so the client will connect to the local services
   that you'll start in the next section::

    export OS_AUTH_TOKEN=fake-token
    export IRONIC_URL=http://localhost:6385/

Next, install and configure system dependencies. Two different approaches are
described below; you should only do one of these.

Step 2a: System Dependencies In A Virtual Machine
-------------------------------------------------

This option requires `virtualbox <https://www.virtualbox.org>`_,
`vagrant <https://www.vagrantup.com>`_, and
`ansible <https://www.ansible.com>`_. You may install these using your
favorite package manager, or by downloading from the provided links.

#. Let vagrant do the work::

    vagrant up

This will create a VM available to your local system at `192.168.99.11`,
will install all the necessary service dependencies,
and configure some default users. It will also generate
`./etc/ironic/ironic.conf.local` preconfigured for local dev work.
We recommend you compare and familiarize yourself with the settings in
`./etc/ironic/ironic.conf.sample` so you can adjust it to meet your own needs.

Step 2b: Install System Dependencies Locally
--------------------------------------------

This option will install RabbitMQ and MySQL on your local system. This may not
be desirable in some situations (eg, you're developing from a laptop and do not
want to run a MySQL server on it all the time).

#. Install rabbitmq-server::

    # install rabbit message broker
    # Ubuntu/Debian:
    sudo apt-get install rabbitmq-server

    # Fedora 21/RHEL7/CentOS7:
    sudo yum install rabbitmq-server
    sudo systemctl start rabbitmq-server.service

    # Fedora 22 or higher:
    sudo dnf install rabbitmq-server
    sudo systemctl start rabbitmq-server.service

    # openSUSE/SLE 12:
    sudo zypper install rabbitmq-server
    sudo systemctl start rabbitmq-server.service

#. Install mysql-server::

    # Ubuntu/Debian:
    # sudo apt-get install mysql-server

    # Fedora 21/RHEL7/CentOS7:
    # sudo yum install mariadb mariadb-server
    # sudo systemctl start mariadb.service

    # Fedora 22 or higher:
    # sudo dnf install mariadb mariadb-server
    # sudo systemctl start mariadb.service

    # openSUSE/SLE 12:
    # sudo zypper install mariadb
    # sudo systemctl start mysql.service

    # If using MySQL, you need to create the initial database
    mysql -u root -pMYSQL_ROOT_PWD -e "create schema ironic"

   .. note:: if you choose not to install mysql-server, ironic will default to
            using a local sqlite database.

#. Create a configuration file within the ironic source directory::

    # copy sample config and modify it as necessary
    cp etc/ironic/ironic.conf.sample etc/ironic/ironic.conf.local

    # disable auth since we are not running keystone here
    sed -i "s/#auth_strategy = keystone/auth_strategy = noauth/" etc/ironic/ironic.conf.local

    # Use the 'fake_ipmitool' test driver
    sed -i "s/#enabled_drivers = pxe_ipmitool/enabled_drivers = fake_ipmitool/" etc/ironic/ironic.conf.local

    # set a fake host name [useful if you want to test multiple services on the same host]
    sed -i "s/#host = .*/host = test-host/" etc/ironic/ironic.conf.local

    # change the periodic sync_power_state_interval to a week, to avoid getting NodeLocked exceptions
    sed -i "s/#sync_power_state_interval = 60/sync_power_state_interval = 604800/" etc/ironic/ironic.conf.local

    # if you opted to install mysql-server, switch the DB connection from sqlite to mysql
    sed -i "s/#connection = .*/connection = mysql\+pymysql:\/\/root:MYSQL_ROOT_PWD@localhost\/ironic/" etc/ironic/ironic.conf.local

Step 3: Start the Services
--------------------------

From within the python virtualenv, run the following command to prepare the
database before you start the ironic services::

    # initialize the database for ironic
    ironic-dbsync --config-file etc/ironic/ironic.conf.local create_schema

Next, open two new terminals for this section, and run each of the examples
here in a separate terminal. In this way, the services will *not* be run as
daemons; you can observe their output and stop them with Ctrl-C at any time.

#. Start the API service in debug mode and watch its output::

    cd ~/ironic
    source .tox/venv/bin/activate
    ironic-api -d --config-file etc/ironic/ironic.conf.local

#. Start the Conductor service in debug mode and watch its output::

    cd ~/ironic
    source .tox/venv/bin/activate
    ironic-conductor -d --config-file etc/ironic/ironic.conf.local

Step 4: Interact with the running services
------------------------------------------

You should now be able to interact with ironic via the python client, which is
present in the python virtualenv, and observe both services' debug outputs in
the other two windows. This is a good way to test new features or play with the
functionality without necessarily starting DevStack.

To get started, list the available commands and resources::

    # get a list of available commands
    ironic help

    # get the list of drivers currently supported by the available conductor(s)
    ironic driver-list

    # get a list of nodes (should be empty at this point)
    ironic node-list

Here is an example walkthrough of creating a node::

    MAC="aa:bb:cc:dd:ee:ff"   # replace with the MAC of a data port on your node
    IPMI_ADDR="1.2.3.4"       # replace with a real IP of the node BMC
    IPMI_USER="admin"         # replace with the BMC's user name
    IPMI_PASS="pass"          # replace with the BMC's password

    # enroll the node with the "fake" deploy driver and the "ipmitool" power driver
    # Note that driver info may be added at node creation time with "-i"
    NODE=$(ironic node-create -d fake_ipmitool -i ipmi_address=$IPMI_ADDR -i ipmi_username=$IPMI_USER | grep ' uuid ' | awk '{print $4}')

    # driver info may also be added or updated later on
    ironic node-update $NODE add driver_info/ipmi_password=$IPMI_PASS

    # add a network port
    ironic port-create -n $NODE -a $MAC

    # view the information for the node
    ironic node-show $NODE

    # request that the node's driver validate the supplied information
    ironic node-validate $NODE

    # you have now enrolled a node sufficiently to be able to control
    # its power state from ironic!
    ironic node-set-power-state $NODE on

If you make some code changes and want to test their effects, simply stop the
services with Ctrl-C and restart them.

Step 5: Fixing your test environment
------------------------------------

If you are testing changes that add or remove python entrypoints, or making
significant changes to ironic's python modules, or simply keep the virtualenv
around for a long time, your development environment may reach an inconsistent
state. It may help to delete cached ".pyc" files, update dependencies,
reinstall ironic, or even recreate the virtualenv. The following commands may
help with that, but are not an exhaustive troubleshooting guide::

  # clear cached pyc files
  cd ~/ironic/ironic
  find ./ -name '*.pyc' | xargs rm

  # reinstall ironic modules
  cd ~/ironic
  source .tox/venv/bin/activate
  pip uninstall ironic
  pip install -e .

  # install and upgrade ironic and all python dependencies
  cd ~/ironic
  source .tox/venv/bin/activate
  pip install -U -e .


.. _`deploy_devstack`:

Deploying Ironic with DevStack
==============================

DevStack may be configured to deploy Ironic, setup Nova to use the Ironic
driver and provide hardware resources (network, baremetal compute nodes)
using a combination of OpenVSwitch and libvirt.  It is highly recommended
to deploy on an expendable virtual machine and not on your personal work
station. Deploying Ironic with DevStack requires a machine running Ubuntu
16.04 (or later) or Fedora 24 (or later). Make sure your machine is fully
up to date and has the latest packages installed before beginning this process.

.. seealso::

    https://docs.openstack.org/devstack/latest/

.. note::
    The devstack "demo" tenant is now granted the "baremetal_observer" role
    and thereby has read-only access to ironic's API. This is sufficient for
    all the examples below. Should you want to create or modify bare metal
    resources directly (ie. through ironic rather than through nova) you will
    need to use the devstack "admin" tenant.


Devstack will no longer create the user 'stack' with the desired
permissions, but does provide a script to perform the task::

    git clone https://git.openstack.org/openstack-dev/devstack.git devstack
    sudo ./devstack/tools/create-stack-user.sh

Switch to the stack user and clone DevStack::

    sudo su - stack
    git clone https://git.openstack.org/openstack-dev/devstack.git devstack

Create devstack/local.conf with minimal settings required to enable Ironic.
You can use either of two drivers for deploy: agent\_\* or pxe\_\*, see :ref:`IPA`
for explanation. An example local.conf that enables both types of drivers
and uses the ``agent_ipmitool`` driver by default::

    cd devstack
    cat >local.conf <<END
    [[local|localrc]]
    # Credentials
    ADMIN_PASSWORD=password
    DATABASE_PASSWORD=password
    RABBIT_PASSWORD=password
    SERVICE_PASSWORD=password
    SERVICE_TOKEN=password
    SWIFT_HASH=password
    SWIFT_TEMPURL_KEY=password

    # Enable Ironic plugin
    enable_plugin ironic git://git.openstack.org/openstack/ironic

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

    # Disable Cinder
    disable_service cinder c-sch c-api c-vol

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

    END

.. note::
    Git protocol requires access to port 9418, which is not a standard port that
    corporate firewalls always allow. If you are behind a firewall or on a proxy that
    blocks Git protocol, modify the ``enable_plugin`` line to use ``https://`` instead
    of ``git://`` and add ``GIT_BASE=https://git.openstack.org`` to the credentials::

      GIT_BASE=https://git.openstack.org

      # Enable Ironic plugin
      enable_plugin ironic https://git.openstack.org/openstack/ironic

.. note::
    When a \*_ipmitool driver is set and IRONIC_IS_HARDWARE variable is false devstack
    will automatically set up `VirtualBMC <https://github.com/openstack/virtualbmc>`_
    to control the power state of the virtual baremetal nodes.

.. note::
    When running QEMU as non-root user (e.g. ``qemu`` on Fedora or ``libvirt-qemu`` on Ubuntu),
    make sure ``IRONIC_VM_LOG_DIR`` points to a directory where QEMU will be able to write.
    You can verify this with, for example::

      # on Fedora
      sudo -u qemu touch $HOME/ironic-bm-logs/test.log
      # on Ubuntu
      sudo -u libvirt-qemu touch $HOME/ironic-bm-logs/test.log

.. note::
    To check out an in-progress patch for testing, you can add a Git ref to the ``enable_plugin`` line. For instance::

      enable_plugin ironic git://git.openstack.org/openstack/ironic refs/changes/46/295946/15

    For a patch in review, you can find the ref to use by clicking the
    "Download" button in Gerrit. You can also specify a different git repo, or
    a branch or tag::

      enable_plugin ironic https://github.com/openstack/ironic stable/kilo

    For more details, see the
    `devstack plugin interface documentation
    <https://docs.openstack.org/devstack/latest/plugins.html#plugin-interface>`_.

Run stack.sh::

    ./stack.sh

Source credentials, create a key, and spawn an instance as the ``demo`` user::

    source ~/devstack/openrc

    # query the image id of the default cirros image
    image=$(openstack image show $DEFAULT_IMAGE_NAME -f value -c id)

    # create keypair
    ssh-keygen
    openstack keypair create --public-key ~/.ssh/id_rsa.pub default

    # spawn instance
    openstack server create --flavor baremetal --image $image --key-name default testing

.. note::
    Because devstack create multiple networks, we need to pass an additional parameter
    ``--nic net-id`` to the nova boot command when using the admin account, for example::

      net_id=$(openstack network list | egrep "$PRIVATE_NETWORK_NAME"'[^-]' | awk '{ print $2 }')

      openstack server create --flavor baremetal --nic net-id=$net_id --image $image --key-name default testing

You should now see a Nova instance building::

    openstack server list --long
    +----------+---------+--------+------------+-------------+----------+------------+----------+-------------------+------+------------+
    | ID       | Name    | Status | Task State | Power State | Networks | Image Name | Image ID | Availability Zone | Host | Properties |
    +----------+---------+--------+------------+-------------+----------+------------+----------+-------------------+------+------------+
    | a2c7f812 | testing | BUILD  | spawning   | NOSTATE     |          | cirros-0.3 | 44d4092a | nova              |      |            |
    | -e386-4a |         |        |            |             |          | .5-x86_64- | -51ac-47 |                   |      |            |
    | 22-b393- |         |        |            |             |          | disk       | 51-9c50- |                   |      |            |
    | fe1802ab |         |        |            |             |          |            | fd6e2050 |                   |      |            |
    | d56e     |         |        |            |             |          |            | faa1     |                   |      |            |
    +----------+---------+--------+------------+-------------+----------+------------+----------+-------------------+------+------------+

Nova will be interfacing with Ironic conductor to spawn the node.  On the
Ironic side, you should see an Ironic node associated with this Nova instance.
It should be powered on and in a 'wait call-back' provisioning state::

    openstack baremetal node list
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+
    | UUID                                 | Name   | Instance UUID                        | Power State | Provisioning State | Maintenance |
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+
    | 9e592cbe-e492-4e4f-bf8f-4c9e0ad1868f | node-0 | None                                 | power off   | None               | False       |
    | ec0c6384-cc3a-4edf-b7db-abde1998be96 | node-1 | None                                 | power off   | None               | False       |
    | 4099e31c-576c-48f8-b460-75e1b14e497f | node-2 | a2c7f812-e386-4a22-b393-fe1802abd56e | power on    | wait call-back     | False       |
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+

At this point, Ironic conductor has called to libvirt (via virtualbmc) to
power on a virtual machine, which will PXE + TFTP boot from the conductor node and
progress through the Ironic provisioning workflow.  One libvirt domain should
be active now::

    sudo virsh list --all
     Id    Name                           State
    ----------------------------------------------------
     2     node-2                         running
     -     node-0                         shut off
     -     node-1                         shut off

This provisioning process may take some time depending on the performance of
the host system, but Ironic should eventually show the node as having an
'active' provisioning state::

    openstack baremetal node list
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+
    | UUID                                 | Name   | Instance UUID                        | Power State | Provisioning State | Maintenance |
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+
    | 9e592cbe-e492-4e4f-bf8f-4c9e0ad1868f | node-0 | None                                 | power off   | None               | False       |
    | ec0c6384-cc3a-4edf-b7db-abde1998be96 | node-1 | None                                 | power off   | None               | False       |
    | 4099e31c-576c-48f8-b460-75e1b14e497f | node-2 | a2c7f812-e386-4a22-b393-fe1802abd56e | power on    | active             | False       |
    +--------------------------------------+--------+--------------------------------------+-------------+--------------------+-------------+

This should also be reflected in the Nova instance state, which at this point
should be ACTIVE, Running and an associated private IP::

    openstack server list --long
    +----------+---------+--------+------------+-------------+---------------+------------+----------+-------------------+------+------------+
    | ID       | Name    | Status | Task State | Power State | Networks      | Image Name | Image ID | Availability Zone | Host | Properties |
    +----------+---------+--------+------------+-------------+---------------+------------+----------+-------------------+------+------------+
    | a2c7f812 | testing | ACTIVE | none       | Running     | private=10.1. | cirros-0.3 | 44d4092a | nova              |      |            |
    | -e386-4a |         |        |            |             | 0.4, fd7d:1f3 | .5-x86_64- | -51ac-47 |                   |      |            |
    | 22-b393- |         |        |            |             | c:4bf1:0:f816 | disk       | 51-9c50- |                   |      |            |
    | fe1802ab |         |        |            |             | :3eff:f39d:6d |            | fd6e2050 |                   |      |            |
    | d56e     |         |        |            |             | 94            |            | faa1     |                   |      |            |
    +----------+---------+--------+------------+-------------+---------------+------------+----------+-------------------+------+------------+

The server should now be accessible via SSH::

    ssh cirros@10.1.0.4
    $

Running Tempest tests
=====================

After `Deploying Ironic with DevStack`_ one might want to run integration
tests against the running cloud. The Tempest project is the project that
offers an integration test suite for OpenStack.

First, navigate to Tempest directory::

  cd /opt/stack/tempest

To run all tests from the `Ironic plugin
<https://git.openstack.org/cgit/openstack/ironic/tree/ironic_tempest_plugin?h=master>`_,
execute the following command::

  tox -e all-plugin -- ironic

To limit the amount of tests that you would like to run, you can use
a regex. For instance, to limit the run to a single test file, the
following command can be used::

  tox -e all-plugin -- ironic_tempest_plugin.tests.scenario.test_baremetal_basic_ops


Debugging Tempest tests
-----------------------

It is sometimes useful to step through the test code, line by line,
especially when the error output is vague. This can be done by
running the tests in debug mode and using a debugger such as `pdb
<https://docs.python.org/2/library/pdb.html>`_.

For example, after editing the *test_baremetal_basic_ops* file and
setting up the pdb traces you can invoke the ``run_tempest.sh`` script
in the Tempest directory with the following parameters::

  ./run_tempest.sh -N -d ironic_tempest_plugin.tests.scenario.test_baremetal_basic_ops

* The *-N* parameter tells the script to run the tests in the local
  environment (without a virtualenv) so it can find the Ironic tempest
  plugin.

* The *-d* parameter enables the debug mode, allowing it to be used
  with pdb.

For more information about the supported parameters see::

  ./run_tempest.sh --help

.. note::
   Always be careful when running debuggers in time sensitive code,
   they may cause timeout errors that weren't there before.


OSProfiler Tracing in Ironic
============================

OSProfiler is an OpenStack cross-project profiling library. It is being
used among OpenStack projects to look at performance issues and detect
bottlenecks. For details on how OSProfiler works and how to use it in ironic,
please refer to `OSProfiler Support Documentation <osprofiler-support>`_.


Building developer documentation
================================

If you would like to build the documentation locally, eg. to test your
documentation changes before uploading them for review, run these
commands to build the documentation set:

- On your local machine::

    # activate your development virtualenv
    source .tox/venv/bin/activate

    # build the docs
    tox -edocs

    #Now use your browser to open the top-level index.html located at:

    ironic/doc/build/html/index.html


- On a remote machine::

    # Go to the directory that contains the docs
    cd ~/ironic/doc/source/

    # Build the docs
    tox -edocs

    # Change directory to the newly built HTML files
    cd ~/ironic/doc/build/html/

    # Create a server using python on port 8000
    python -m SimpleHTTPServer 8000

    #Now use your browser to open the top-level index.html located at:

    http://your_ip:8000
