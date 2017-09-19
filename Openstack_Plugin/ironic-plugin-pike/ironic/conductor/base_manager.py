#    Licensed under the Apache License, Version 2.0 (the "License"); you may
#    not use this file except in compliance with the License. You may obtain
#    a copy of the License at
#
#         http://www.apache.org/licenses/LICENSE-2.0
#
#    Unless required by applicable law or agreed to in writing, software
#    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
#    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
#    License for the specific language governing permissions and limitations
#    under the License.

"""Base conductor manager functionality."""

import inspect
import threading

import eventlet
import futurist
from futurist import periodics
from futurist import rejection
from oslo_db import exception as db_exception
from oslo_log import log
from oslo_utils import excutils

from ironic.common import context as ironic_context
from ironic.common import driver_factory
from ironic.common import exception
from ironic.common import hash_ring
from ironic.common.i18n import _
from ironic.common import rpc
from ironic.common import states
from ironic.conductor import notification_utils as notify_utils
from ironic.conductor import task_manager
from ironic.conf import CONF
from ironic.db import api as dbapi
from ironic.drivers import base as driver_base
from ironic import objects
from ironic.objects import fields as obj_fields


LOG = log.getLogger(__name__)


def _check_enabled_interfaces():
    """Sanity-check enabled_*_interfaces configs.

    We do this before we even bother to try to load up drivers. If we have any
    dynamic drivers enabled, then we need interfaces enabled as well.

    :raises: ConfigInvalid if an enabled interfaces config option is empty.
    """
    if CONF.enabled_hardware_types:
        empty_confs = []
        iface_types = ['enabled_%s_interfaces' % i
                       for i in driver_base.ALL_INTERFACES]
        for iface_type in iface_types:
            conf_value = getattr(CONF, iface_type)
            if not conf_value:
                empty_confs.append(iface_type)
        if empty_confs:
            msg = (_('Configuration options %s cannot be an empty list.') %
                   ', '.join(empty_confs))
            raise exception.ConfigInvalid(error_msg=msg)


class BaseConductorManager(object):

    def __init__(self, host, topic):
        super(BaseConductorManager, self).__init__()
        if not host:
            host = CONF.host
        self.host = host
        self.topic = topic
        self.sensors_notifier = rpc.get_sensors_notifier()
        self._started = False
        self._shutdown = None

    def init_host(self, admin_context=None):
        """Initialize the conductor host.

        :param admin_context: the admin context to pass to periodic tasks.
        :raises: RuntimeError when conductor is already running.
        :raises: NoDriversLoaded when no drivers are enabled on the conductor.
        :raises: DriverNotFound if a driver is enabled that does not exist.
        :raises: DriverLoadError if an enabled driver cannot be loaded.
        :raises: DriverNameConflict if a classic driver and a dynamic driver
                 are both enabled and have the same name.
        :raises: ConfigInvalid if required config options for connection with
                 radosgw are missing while storing config drive.
        """
        if self._started:
            raise RuntimeError(_('Attempt to start an already running '
                                 'conductor manager'))
        self._shutdown = False

        self.dbapi = dbapi.get_instance()

        self._keepalive_evt = threading.Event()
        """Event for the keepalive thread."""

        # TODO(dtantsur): make the threshold configurable?
        rejection_func = rejection.reject_when_reached(
            CONF.conductor.workers_pool_size)
        self._executor = futurist.GreenThreadPoolExecutor(
            max_workers=CONF.conductor.workers_pool_size,
            check_and_reject=rejection_func)
        """Executor for performing tasks async."""

        self.ring_manager = hash_ring.HashRingManager()
        """Consistent hash ring which maps drivers to conductors."""

        _check_enabled_interfaces()

        # NOTE(deva): these calls may raise DriverLoadError or DriverNotFound
        # NOTE(vdrok): Instantiate network and storage interface factory on
        # startup so that all the interfaces are loaded at the very
        # beginning, and failures prevent the conductor from starting.
        drivers = driver_factory.drivers()
        hardware_types = driver_factory.hardware_types()
        driver_factory.NetworkInterfaceFactory()
        driver_factory.StorageInterfaceFactory()

        # NOTE(jroll) this is passed to the dbapi, which requires a list, not
        # a generator (which keys() returns in py3)
        driver_names = list(drivers)
        hardware_type_names = list(hardware_types)

        # check that at least one driver is loaded, whether classic or dynamic
        if not driver_names and not hardware_type_names:
            msg = ("Conductor %s cannot be started because no drivers "
                   "were loaded. This could be because no classic drivers "
                   "were specified in the 'enabled_drivers' config option "
                   "and no dynamic drivers were specified in the "
                   "'enabled_hardware_types' config option.")
            LOG.error(msg, self.host)
            raise exception.NoDriversLoaded(conductor=self.host)

        # check for name clashes between classic and dynamic drivers
        name_clashes = set(driver_names).intersection(hardware_type_names)
        if name_clashes:
            name_clashes = ', '.join(name_clashes)
            msg = ("Conductor %(host)s cannot be started because there is "
                   "one or more name conflicts between classic drivers and "
                   "dynamic drivers (%(names)s). Check any external driver "
                   "plugins and the 'enabled_drivers' and "
                   "'enabled_hardware_types' config options.")
            LOG.error(msg, {'host': self.host, 'names': name_clashes})
            raise exception.DriverNameConflict(names=name_clashes)

        # Collect driver-specific periodic tasks.
        # Conductor periodic tasks accept context argument, driver periodic
        # tasks accept this manager and context. We have to ensure that the
        # same driver interface class is not traversed twice, otherwise
        # we'll have several instances of the same task.
        LOG.debug('Collecting periodic tasks')
        self._periodic_task_callables = []
        periodic_task_classes = set()
        self._collect_periodic_tasks(self, (admin_context,))
        for driver_obj in drivers.values():
            for iface_name in driver_obj.all_interfaces:
                iface = getattr(driver_obj, iface_name, None)
                if iface and iface.__class__ not in periodic_task_classes:
                    self._collect_periodic_tasks(iface, (self, admin_context))
                    periodic_task_classes.add(iface.__class__)

        if (len(self._periodic_task_callables) >
                CONF.conductor.workers_pool_size):
            LOG.warning('This conductor has %(tasks)d periodic tasks '
                        'enabled, but only %(workers)d task workers '
                        'allowed by [conductor]workers_pool_size option',
                        {'tasks': len(self._periodic_task_callables),
                         'workers': CONF.conductor.workers_pool_size})

        self._periodic_tasks = periodics.PeriodicWorker(
            self._periodic_task_callables,
            executor_factory=periodics.ExistingExecutor(self._executor))

        # Check for required config options if object_store_endpoint_type is
        # radosgw
        if (CONF.deploy.configdrive_use_object_store and
            CONF.deploy.object_store_endpoint_type == "radosgw"):
            if (None in (CONF.swift.auth_url, CONF.swift.username,
                         CONF.swift.password)):
                msg = _("Parameters missing to make a connection with "
                        "radosgw. Ensure that [swift]/auth_url, "
                        "[swift]/username, and [swift]/password are all "
                        "configured.")
                raise exception.ConfigInvalid(msg)

        # clear all target_power_state with locks by this conductor
        self.dbapi.clear_node_target_power_state(self.host)
        # clear all locks held by this conductor before registering
        self.dbapi.clear_node_reservations_for_conductor(self.host)
        try:
            # Register this conductor with the cluster
            self.conductor = objects.Conductor.register(
                admin_context, self.host, driver_names)
        except exception.ConductorAlreadyRegistered:
            # This conductor was already registered and did not shut down
            # properly, so log a warning and update the record.
            LOG.warning("A conductor with hostname %(hostname)s was "
                        "previously registered. Updating registration",
                        {'hostname': self.host})
            self.conductor = objects.Conductor.register(
                admin_context, self.host, driver_names, update_existing=True)

        # register hardware types and interfaces supported by this conductor
        # and validate them against other conductors
        try:
            self._register_and_validate_hardware_interfaces(hardware_types)
        except (exception.DriverLoadError, exception.DriverNotFound,
                exception.ConductorHardwareInterfacesAlreadyRegistered,
                exception.InterfaceNotFoundInEntrypoint,
                exception.NoValidDefaultForInterface) as e:
            with excutils.save_and_reraise_exception():
                LOG.error('Failed to register hardware types. %s', e)
                self.del_host()

        # Start periodic tasks
        self._periodic_tasks_worker = self._executor.submit(
            self._periodic_tasks.start, allow_empty=True)
        self._periodic_tasks_worker.add_done_callback(
            self._on_periodic_tasks_stop)

        # NOTE(lucasagomes): If the conductor server dies abruptly
        # mid deployment (OMM Killer, power outage, etc...) we
        # can not resume the deployment even if the conductor
        # comes back online. Cleaning the reservation of the nodes
        # (dbapi.clear_node_reservations_for_conductor) is not enough to
        # unstick it, so let's gracefully fail the deployment so the node
        # can go through the steps (deleting & cleaning) to make itself
        # available again.
        filters = {'reserved': False,
                   'provision_state': states.DEPLOYING}
        last_error = (_("The deployment can't be resumed by conductor "
                        "%s. Moving to fail state.") % self.host)
        self._fail_if_in_state(ironic_context.get_admin_context(), filters,
                               states.DEPLOYING, 'provision_updated_at',
                               last_error=last_error)

        # Start consoles if it set enabled in a greenthread.
        try:
            self._spawn_worker(self._start_consoles,
                               ironic_context.get_admin_context())
        except exception.NoFreeConductorWorker:
            LOG.warning('Failed to start worker for restarting consoles.')

        # Spawn a dedicated greenthread for the keepalive
        try:
            self._spawn_worker(self._conductor_service_record_keepalive)
            LOG.info('Successfully started conductor with hostname '
                     '%(hostname)s.',
                     {'hostname': self.host})
        except exception.NoFreeConductorWorker:
            with excutils.save_and_reraise_exception():
                LOG.critical('Failed to start keepalive')
                self.del_host()

        self._started = True

    def del_host(self, deregister=True):
        # Conductor deregistration fails if called on non-initialized
        # conductor (e.g. when rpc server is unreachable).
        if not hasattr(self, 'conductor'):
            return
        self._shutdown = True
        self._keepalive_evt.set()
        if deregister:
            try:
                # Inform the cluster that this conductor is shutting down.
                # Note that rebalancing will not occur immediately, but when
                # the periodic sync takes place.
                self.conductor.unregister()
                LOG.info('Successfully stopped conductor with hostname '
                         '%(hostname)s.',
                         {'hostname': self.host})
            except exception.ConductorNotFound:
                pass
        else:
            LOG.info('Not deregistering conductor with hostname %(hostname)s.',
                     {'hostname': self.host})
        # Waiting here to give workers the chance to finish. This has the
        # benefit of releasing locks workers placed on nodes, as well as
        # having work complete normally.
        self._periodic_tasks.stop()
        self._periodic_tasks.wait()
        self._executor.shutdown(wait=True)
        self._started = False

    def _register_and_validate_hardware_interfaces(self, hardware_types):
        """Register and validate hardware interfaces for this conductor.

        Registers a row in the database for each combination of
        (hardware type, interface type, interface) that is supported and
        enabled.

        TODO: Validates against other conductors to check if the
        set of registered hardware interfaces for a given hardware type is the
        same, and warns if not (we can't error out, otherwise all conductors
        must be restarted at once to change configuration).

        :param hardware_types: Dictionary mapping hardware type name to
                               hardware type object.
        :raises: ConductorHardwareInterfacesAlreadyRegistered
        :raises: InterfaceNotFoundInEntrypoint
        :raises: NoValidDefaultForInterface if the default value cannot be
                 calculated and is not provided in the configuration
        """
        # first unregister, in case we have cruft laying around
        self.conductor.unregister_all_hardware_interfaces()

        for ht_name, ht in hardware_types.items():
            interface_map = driver_factory.enabled_supported_interfaces(ht)
            for interface_type, interface_names in interface_map.items():
                default_interface = driver_factory.default_interface(
                    ht, interface_type, driver_name=ht_name)
                self.conductor.register_hardware_interfaces(ht_name,
                                                            interface_type,
                                                            interface_names,
                                                            default_interface)

        # TODO(jroll) validate against other conductor, warn if different
        # how do we do this performantly? :|

    def _collect_periodic_tasks(self, obj, args):
        """Collect periodic tasks from a given object.

        Populates self._periodic_task_callables with tuples
        (callable, args, kwargs).

        :param obj: object containing periodic tasks as methods
        :param args: tuple with arguments to pass to every task
        """
        for name, member in inspect.getmembers(obj):
            if periodics.is_periodic(member):
                LOG.debug('Found periodic task %(owner)s.%(member)s',
                          {'owner': obj.__class__.__name__,
                           'member': name})
                self._periodic_task_callables.append((member, args, {}))

    def _on_periodic_tasks_stop(self, fut):
        try:
            fut.result()
        except Exception as exc:
            LOG.critical('Periodic tasks worker has failed: %s', exc)
        else:
            LOG.info('Successfully shut down periodic tasks')

    def iter_nodes(self, fields=None, **kwargs):
        """Iterate over nodes mapped to this conductor.

        Requests node set from and filters out nodes that are not
        mapped to this conductor.

        Yields tuples (node_uuid, driver, ...) where ... is derived from
        fields argument, e.g.: fields=None means yielding ('uuid', 'driver'),
        fields=['foo'] means yielding ('uuid', 'driver', 'foo').

        :param fields: list of fields to fetch in addition to uuid and driver
        :param kwargs: additional arguments to pass to dbapi when looking for
                       nodes
        :return: generator yielding tuples of requested fields
        """
        columns = ['uuid', 'driver'] + list(fields or ())
        node_list = self.dbapi.get_nodeinfo_list(columns=columns, **kwargs)
        for result in node_list:
            if self._shutdown:
                break
            if self._mapped_to_this_conductor(*result[:2]):
                yield result

    def _spawn_worker(self, func, *args, **kwargs):

        """Create a greenthread to run func(*args, **kwargs).

        Spawns a greenthread if there are free slots in pool, otherwise raises
        exception. Execution control returns immediately to the caller.

        :returns: Future object.
        :raises: NoFreeConductorWorker if worker pool is currently full.

        """
        try:
            return self._executor.submit(func, *args, **kwargs)
        except futurist.RejectedSubmission:
            raise exception.NoFreeConductorWorker()

    def _conductor_service_record_keepalive(self):
        while not self._keepalive_evt.is_set():
            try:
                self.conductor.touch()
            except db_exception.DBConnectionError:
                LOG.warning('Conductor could not connect to database '
                            'while heartbeating.')
            self._keepalive_evt.wait(CONF.conductor.heartbeat_interval)

    def _mapped_to_this_conductor(self, node_uuid, driver):
        """Check that node is mapped to this conductor.

        Note that because mappings are eventually consistent, it is possible
        for two conductors to simultaneously believe that a node is mapped to
        them. Any operation that depends on exclusive control of a node should
        take out a lock.
        """
        try:
            ring = self.ring_manager[driver]
        except exception.DriverNotFound:
            return False

        return self.host in ring.get_nodes(
            node_uuid.encode('utf-8'),
            replicas=CONF.hash_distribution_replicas)

    def _fail_if_in_state(self, context, filters, provision_state,
                          sort_key, callback_method=None,
                          err_handler=None, last_error=None,
                          keep_target_state=False):
        """Fail nodes that are in specified state.

        Retrieves nodes that satisfy the criteria in 'filters'.
        If any of these nodes is in 'provision_state', it has failed
        in whatever provisioning activity it was currently doing.
        That failure is processed here.

        :param: context: request context
        :param: filters: criteria (as a dictionary) to get the desired
                         list of nodes that satisfy the filter constraints.
                         For example, if filters['provisioned_before'] = 60,
                         this would process nodes whose provision_updated_at
                         field value was 60 or more seconds before 'now'.
        :param: provision_state: provision_state that the node is in,
                                 for the provisioning activity to have failed.
        :param: sort_key: the nodes are sorted based on this key.
        :param: callback_method: the callback method to be invoked in a
                                 spawned thread, for a failed node. This
                                 method must take a :class:`TaskManager` as
                                 the first (and only required) parameter.
        :param: err_handler: for a failed node, the error handler to invoke
                             if an error occurs trying to spawn an thread
                             to do the callback_method.
        :param: last_error: the error message to be updated in node.last_error
        :param: keep_target_state: if True, a failed node will keep the same
                                   target provision state it had before the
                                   failure. Otherwise, the node's target
                                   provision state will be determined by the
                                   fsm.

        """
        node_iter = self.iter_nodes(filters=filters,
                                    sort_key=sort_key,
                                    sort_dir='asc')

        workers_count = 0
        for node_uuid, driver in node_iter:
            try:
                with task_manager.acquire(context, node_uuid,
                                          purpose='node state check') as task:
                    if (task.node.maintenance or
                            task.node.provision_state != provision_state):
                        continue

                    target_state = (None if not keep_target_state else
                                    task.node.target_provision_state)

                    # timeout has been reached - process the event 'fail'
                    if callback_method:
                        task.process_event('fail',
                                           callback=self._spawn_worker,
                                           call_args=(callback_method, task),
                                           err_handler=err_handler,
                                           target_state=target_state)
                    else:
                        task.node.last_error = last_error
                        task.process_event('fail', target_state=target_state)
            except exception.NoFreeConductorWorker:
                break
            except (exception.NodeLocked, exception.NodeNotFound):
                continue
            workers_count += 1
            if workers_count >= CONF.conductor.periodic_max_workers:
                break

    def _start_consoles(self, context):
        """Start consoles if set enabled.

        :param: context: request context
        """
        filters = {'console_enabled': True}

        node_iter = self.iter_nodes(filters=filters)

        for node_uuid, driver in node_iter:
            try:
                with task_manager.acquire(context, node_uuid, shared=False,
                                          purpose='start console') as task:
                    notify_utils.emit_console_notification(
                        task, 'console_restore',
                        obj_fields.NotificationStatus.START)
                    try:
                        LOG.debug('Trying to start console of node %(node)s',
                                  {'node': node_uuid})
                        task.driver.console.start_console(task)
                        LOG.info('Successfully started console of node '
                                 '%(node)s', {'node': node_uuid})
                        notify_utils.emit_console_notification(
                            task, 'console_restore',
                            obj_fields.NotificationStatus.END)
                    except Exception as err:
                        msg = (_('Failed to start console of node %(node)s '
                                 'while starting the conductor, so changing '
                                 'the console_enabled status to False, error: '
                                 '%(err)s')
                               % {'node': node_uuid, 'err': err})
                        LOG.error(msg)
                        # If starting console failed, set node console_enabled
                        # back to False and set node's last error.
                        task.node.last_error = msg
                        task.node.console_enabled = False
                        task.node.save()
                        notify_utils.emit_console_notification(
                            task, 'console_restore',
                            obj_fields.NotificationStatus.ERROR)
            except exception.NodeLocked:
                LOG.warning('Node %(node)s is locked while trying to '
                            'start console on conductor startup',
                            {'node': node_uuid})
                continue
            except exception.NodeNotFound:
                LOG.warning("During starting console on conductor "
                            "startup, node %(node)s was not found",
                            {'node': node_uuid})
                continue
            finally:
                # Yield on every iteration
                eventlet.sleep(0)
