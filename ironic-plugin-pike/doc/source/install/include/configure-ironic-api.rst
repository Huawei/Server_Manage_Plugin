Configuring ironic-api service
------------------------------

#. The Bare Metal service stores information in a database. This guide uses the
   MySQL database that is used by other OpenStack services.

   Configure the location of the database via the ``connection`` option. In the
   following, replace ``IRONIC_DBPASSWORD`` with the password of your
   ``ironic`` user, and replace ``DB_IP`` with the IP address where the DB
   server is located:

   .. code-block:: ini

      [database]

      # The SQLAlchemy connection string used to connect to the
      # database (string value)
      connection=mysql+pymysql://ironic:IRONIC_DBPASSWORD@DB_IP/ironic?charset=utf8

#. Configure the ironic-api service to use the RabbitMQ message broker by
   setting the following option. Replace ``RPC_*`` with appropriate
   address details and credentials of RabbitMQ server:

   .. code-block:: ini

      [DEFAULT]

      # A URL representing the messaging driver to use and its full
      # configuration. (string value)
      transport_url = rabbit://RPC_USER:RPC_PASSWORD@RPC_HOST:RPC_PORT/

#. Configure the ironic-api service to use these credentials with the Identity
   service. Replace ``PUBLIC_IDENTITY_IP`` with the public IP of the Identity
   server, ``PRIVATE_IDENTITY_IP`` with the private IP of the Identity server
   and replace ``IRONIC_PASSWORD`` with the password you chose for the
   ``ironic`` user in the Identity service:

   .. code-block:: ini

      [DEFAULT]

      # Authentication strategy used by ironic-api: one of
      # "keystone" or "noauth". "noauth" should not be used in a
      # production environment because all authentication will be
      # disabled. (string value)
      auth_strategy=keystone

      [keystone_authtoken]

      # Authentication type to load (string value)
      auth_type=password

      # Complete public Identity API endpoint (string value)
      auth_uri=http://PUBLIC_IDENTITY_IP:5000

      # Complete admin Identity API endpoint. (string value)
      auth_url=http://PRIVATE_IDENTITY_IP:35357

      # Service username. (string value)
      username=ironic

      # Service account password. (string value)
      password=IRONIC_PASSWORD

      # Service tenant name. (string value)
      project_name=service

      # Domain name containing project (string value)
      project_domain_name=Default

      # User's domain name (string value)
      user_domain_name=Default

#. Create the Bare Metal service database tables:

   .. code-block:: bash

      $ ironic-dbsync --config-file /etc/ironic/ironic.conf create_schema

#. Restart the ironic-api service:

   .. TODO(mmitchell): Split this based on operating system
   .. code-block:: console

      Fedora/RHEL7/CentOS7/SUSE:
        sudo systemctl restart openstack-ironic-api

      Ubuntu:
        sudo service ironic-api restart
