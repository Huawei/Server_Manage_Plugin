# Copyright 2010 United States Government as represented by the
# Administrator of the National Aeronautics and Space Administration.
# Copyright 2011 Justin Santa Barbara
# Copyright (c) 2012 NTT DOCOMO, INC.
# All Rights Reserved.
#
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

"""Utilities and helper functions."""

import contextlib
import datetime
import errno
import hashlib
import os
import re
import shutil
import tempfile

import jinja2
from oslo_concurrency import processutils
from oslo_log import log as logging
from oslo_utils import netutils
from oslo_utils import timeutils
import pytz
import six

from ironic.common import exception
from ironic.common.i18n import _
from ironic.conf import CONF

LOG = logging.getLogger(__name__)

warn_deprecated_extra_vif_port_id = False


def _get_root_helper():
    # NOTE(jlvillal): This function has been moved to ironic-lib. And is
    # planned to be deleted here. If need to modify this function, please
    # also do the same modification in ironic-lib
    return 'sudo ironic-rootwrap %s' % CONF.rootwrap_config


def execute(*cmd, **kwargs):
    """Convenience wrapper around oslo's execute() method.

    :param cmd: Passed to processutils.execute.
    :param use_standard_locale: True | False. Defaults to False. If set to
                                True, execute command with standard locale
                                added to environment variables.
    :returns: (stdout, stderr) from process execution
    :raises: UnknownArgumentError
    :raises: ProcessExecutionError
    """

    use_standard_locale = kwargs.pop('use_standard_locale', False)
    if use_standard_locale:
        env = kwargs.pop('env_variables', os.environ.copy())
        env['LC_ALL'] = 'C'
        kwargs['env_variables'] = env
    if kwargs.get('run_as_root') and 'root_helper' not in kwargs:
        kwargs['root_helper'] = _get_root_helper()
    result = processutils.execute(*cmd, **kwargs)
    LOG.debug('Execution completed, command line is "%s"',
              ' '.join(map(str, cmd)))
    LOG.debug('Command stdout is: "%s"', result[0])
    LOG.debug('Command stderr is: "%s"', result[1])
    return result


def is_valid_datapath_id(datapath_id):
    """Verify the format of an OpenFlow datapath_id.

    Check if a datapath_id is valid and contains 16 hexadecimal digits.
    Datapath ID format: the lower 48-bits are for a MAC address,
    while the upper 16-bits are implementer-defined.

    :param datapath_id: OpenFlow datapath_id to be validated.
    :returns: True if valid. False if not.

    """
    m = "^[0-9a-f]{16}$"
    return (isinstance(datapath_id, six.string_types) and
            re.match(m, datapath_id.lower()))


_is_valid_logical_name_re = re.compile(r'^[A-Z0-9-._~]+$', re.I)

# old is_hostname_safe() regex, retained for backwards compat
_is_hostname_safe_re = re.compile(r"""^
[a-z0-9]([a-z0-9\-]{0,61}[a-z0-9])?  # host
(\.[a-z0-9\-_]{0,62}[a-z0-9])*       # domain
\.?                                  # trailing dot
$""", re.X)


def is_valid_logical_name(hostname):
    """Determine if a logical name is valid.

    The logical name may only consist of RFC3986 unreserved
    characters, to wit:

        ALPHA / DIGIT / "-" / "." / "_" / "~"
    """
    if not isinstance(hostname, six.string_types) or len(hostname) > 255:
        return False

    return _is_valid_logical_name_re.match(hostname) is not None


def is_hostname_safe(hostname):
    """Old check for valid logical node names.

    Retained for compatibility with REST API < 1.10.

    Nominally, checks that the supplied hostname conforms to:
        * http://en.wikipedia.org/wiki/Hostname
        * http://tools.ietf.org/html/rfc952
        * http://tools.ietf.org/html/rfc1123

    In practice, this check has several shortcomings and errors that
    are more thoroughly documented in bug #1468508.

    :param hostname: The hostname to be validated.
    :returns: True if valid. False if not.
    """
    if not isinstance(hostname, six.string_types) or len(hostname) > 255:
        return False

    return _is_hostname_safe_re.match(hostname) is not None


def is_valid_no_proxy(no_proxy):
    """Check no_proxy validity

    Check if no_proxy value that will be written to environment variable by
    ironic-python-agent is valid.

    :param no_proxy: the value that requires validity check. Expected to be a
        comma-separated list of host names, IP addresses and domain names
        (with optional :port).
    :returns: True if no_proxy is valid, False otherwise.
    """
    if not isinstance(no_proxy, six.string_types):
        return False
    hostname_re = re.compile('(?!-)[A-Z\d-]{1,63}(?<!-)$', re.IGNORECASE)
    for hostname in no_proxy.split(','):
        hostname = hostname.strip().split(':')[0]
        if not hostname:
            continue
        max_length = 253
        if hostname.startswith('.'):
            # It is allowed to specify a dot in the beginning of the value to
            # indicate that it is a domain name, which means there will be at
            # least one additional character in full hostname. *. is also
            # possible but may be not supported by some clients, so is not
            # considered valid here.
            hostname = hostname[1:]
            max_length = 251

        if len(hostname) > max_length:
            return False

        if not all(hostname_re.match(part) for part in hostname.split('.')):
            return False

    return True


def validate_and_normalize_mac(address):
    """Validate a MAC address and return normalized form.

    Checks whether the supplied MAC address is formally correct and
    normalize it to all lower case.

    :param address: MAC address to be validated and normalized.
    :returns: Normalized and validated MAC address.
    :raises: InvalidMAC If the MAC address is not valid.

    """
    if not netutils.is_valid_mac(address):
        raise exception.InvalidMAC(mac=address)
    return address.lower()


def validate_and_normalize_datapath_id(datapath_id):
    """Validate an OpenFlow datapath_id and return normalized form.

    Checks whether the supplied OpenFlow datapath_id is formally correct and
    normalize it to all lower case.

    :param datapath_id: OpenFlow datapath_id to be validated and normalized.
    :returns: Normalized and validated OpenFlow datapath_id.
    :raises: InvalidDatapathID If an OpenFlow datapath_id is not valid.

    """

    if not is_valid_datapath_id(datapath_id):
        raise exception.InvalidDatapathID(datapath_id=datapath_id)
    return datapath_id.lower()


def _get_hash_object(hash_algo_name):
    """Create a hash object based on given algorithm.

    :param hash_algo_name: name of the hashing algorithm.
    :raises: InvalidParameterValue, on unsupported or invalid input.
    :returns: a hash object based on the given named algorithm.
    """
    algorithms = (hashlib.algorithms_guaranteed if six.PY3
                  else hashlib.algorithms)
    if hash_algo_name not in algorithms:
        msg = (_("Unsupported/Invalid hash name '%s' provided.")
               % hash_algo_name)
        LOG.error(msg)
        raise exception.InvalidParameterValue(msg)

    return getattr(hashlib, hash_algo_name)()


def hash_file(file_like_object, hash_algo='md5'):
    """Generate a hash for the contents of a file.

    It returns a hash of the file object as a string of double length,
    containing only hexadecimal digits. It supports all the algorithms
    hashlib does.
    :param file_like_object: file like object whose hash to be calculated.
    :param hash_algo: name of the hashing strategy, default being 'md5'.
    :raises: InvalidParameterValue, on unsupported or invalid input.
    :returns: a condensed digest of the bytes of contents.
    """
    checksum = _get_hash_object(hash_algo)
    while True:
        chunk = file_like_object.read(32768)
        if not chunk:
            break
        encoded_chunk = (chunk.encode(encoding='utf-8')
                         if isinstance(chunk, six.string_types) else chunk)
        checksum.update(encoded_chunk)
    return checksum.hexdigest()


def file_has_content(path, content, hash_algo='md5'):
    """Checks that content of the file is the same as provided reference.

    :param path: path to file
    :param content: reference content to check against
    :param hash_algo: hashing algo from hashlib to use, default is 'md5'
    :returns: True if the hash of reference content is the same as
        the hash of file's content, False otherwise
    """
    with open(path, 'rb') as existing:
        file_hash_hex = hash_file(existing, hash_algo=hash_algo)
    ref_hash = _get_hash_object(hash_algo)
    encoded_content = (content.encode(encoding='utf-8')
                       if isinstance(content, six.string_types) else content)
    ref_hash.update(encoded_content)
    return file_hash_hex == ref_hash.hexdigest()


@contextlib.contextmanager
def tempdir(**kwargs):
    tempfile.tempdir = CONF.tempdir
    tmpdir = tempfile.mkdtemp(**kwargs)
    try:
        yield tmpdir
    finally:
        try:
            shutil.rmtree(tmpdir)
        except OSError as e:
            LOG.error('Could not remove tmpdir: %s', e)


def rmtree_without_raise(path):
    try:
        if os.path.isdir(path):
            shutil.rmtree(path)
    except OSError as e:
        LOG.warning("Failed to remove dir %(path)s, error: %(e)s",
                    {'path': path, 'e': e})


def write_to_file(path, contents):
    with open(path, 'w') as f:
        f.write(contents)


def create_link_without_raise(source, link):
    try:
        os.symlink(source, link)
    except OSError as e:
        if e.errno == errno.EEXIST:
            return
        else:
            LOG.warning("Failed to create symlink from "
                        "%(source)s to %(link)s, error: %(e)s",
                        {'source': source, 'link': link, 'e': e})


def safe_rstrip(value, chars=None):
    """Removes trailing characters from a string if that does not make it empty

    :param value: A string value that will be stripped.
    :param chars: Characters to remove.
    :return: Stripped value.

    """
    if not isinstance(value, six.string_types):
        LOG.warning("Failed to remove trailing character. Returning "
                    "original object. Supplied object is not a string: "
                    "%s,", value)
        return value

    return value.rstrip(chars) or value


def mount(src, dest, *args):
    """Mounts a device/image file on specified location.

    :param src: the path to the source file for mounting
    :param dest: the path where it needs to be mounted.
    :param args: a tuple containing the arguments to be
        passed to mount command.
    :raises: processutils.ProcessExecutionError if it failed
        to run the process.
    """
    args = ('mount', ) + args + (src, dest)
    execute(*args, run_as_root=True, check_exit_code=[0])


def umount(loc, *args):
    """Umounts a mounted location.

    :param loc: the path to be unmounted.
    :param args: a tuple containing the argumnets to be
        passed to the umount command.
    :raises: processutils.ProcessExecutionError if it failed
        to run the process.
    """
    args = ('umount', ) + args + (loc, )
    execute(*args, run_as_root=True, check_exit_code=[0])


def check_dir(directory_to_check=None, required_space=1):
    """Check a directory is usable.

    This function can be used by drivers to check that directories
    they need to write to are usable. This should be called from the
    drivers init function. This function checks that the directory
    exists and then calls check_dir_writable and check_dir_free_space.
    If directory_to_check is not provided the default is to use the
    temp directory.

    :param directory_to_check: the directory to check.
    :param required_space: amount of space to check for in MiB.
    :raises: PathNotFound if directory can not be found
    :raises: DirectoryNotWritable if user is unable to write to the
             directory
    :raises InsufficientDiskSpace: if free space is < required space
    """
    # check if directory_to_check is passed in, if not set to tempdir
    if directory_to_check is None:
        directory_to_check = CONF.tempdir

    LOG.debug("checking directory: %s", directory_to_check)

    if not os.path.exists(directory_to_check):
        raise exception.PathNotFound(dir=directory_to_check)

    _check_dir_writable(directory_to_check)
    _check_dir_free_space(directory_to_check, required_space)


def _check_dir_writable(chk_dir):
    """Check that the chk_dir is able to be written to.

    :param chk_dir: Directory to check
    :raises: DirectoryNotWritable if user is unable to write to the
             directory
    """
    is_writable = os.access(chk_dir, os.W_OK)
    if not is_writable:
        raise exception.DirectoryNotWritable(dir=chk_dir)


def _check_dir_free_space(chk_dir, required_space=1):
    """Check that directory has some free space.

    :param chk_dir: Directory to check
    :param required_space: amount of space to check for in MiB.
    :raises InsufficientDiskSpace: if free space is < required space
    """
    # check that we have some free space
    stat = os.statvfs(chk_dir)
    # get dir free space in MiB.
    free_space = float(stat.f_bsize * stat.f_bavail) / 1024 / 1024
    # check for at least required_space MiB free
    if free_space < required_space:
        raise exception.InsufficientDiskSpace(path=chk_dir,
                                              required=required_space,
                                              actual=free_space)


def get_updated_capabilities(current_capabilities, new_capabilities):
    """Returns an updated capability string.

    This method updates the original (or current) capabilities with the new
    capabilities. The original capabilities would typically be from a node's
    properties['capabilities']. From new_capabilities, any new capabilities
    are added, and existing capabilities may have their values updated. This
    updated capabilities string is returned.

    :param current_capabilities: Current capability string
    :param new_capabilities: the dictionary of capabilities to be updated.
    :returns: An updated capability string.
        with new_capabilities.
    :raises: ValueError, if current_capabilities is malformed or
        if new_capabilities is not a dictionary
    """
    if not isinstance(new_capabilities, dict):
        raise ValueError(
            _("Cannot update capabilities. The new capabilities should be in "
              "a dictionary. Provided value is %s") % new_capabilities)

    cap_dict = {}
    if current_capabilities:
        try:
            cap_dict = dict(x.split(':', 1)
                            for x in current_capabilities.split(','))
        except ValueError:
            # Capabilities can be filled by operator.  ValueError can
            # occur in malformed capabilities like:
            # properties/capabilities='boot_mode:bios,boot_option'.
            raise ValueError(
                _("Invalid capabilities string '%s'.") % current_capabilities)

    cap_dict.update(new_capabilities)
    return ','.join('%(key)s:%(value)s' % {'key': key, 'value': value}
                    for key, value in cap_dict.items())


def is_regex_string_in_file(path, string):
    with open(path, 'r') as inf:
        return any(re.search(string, line) for line in inf.readlines())


def unix_file_modification_datetime(file_name):
    return timeutils.normalize_time(
        # normalize time to be UTC without timezone
        datetime.datetime.fromtimestamp(
            # fromtimestamp will return local time by default, make it UTC
            os.path.getmtime(file_name), tz=pytz.utc
        )
    )


def validate_network_port(port, port_name="Port"):
    """Validates the given port.

    :param port: TCP/UDP port.
    :param port_name: Name of the port.
    :returns: An integer port number.
    :raises: InvalidParameterValue, if the port is invalid.
    """
    try:
        port = int(port)
    except ValueError:
        raise exception.InvalidParameterValue(_(
            '%(port_name)s "%(port)s" is not a valid integer.') %
            {'port_name': port_name, 'port': port})
    if port < 1 or port > 65535:
        raise exception.InvalidParameterValue(_(
            '%(port_name)s "%(port)s" is out of range. Valid port '
            'numbers must be between 1 and 65535.') %
            {'port_name': port_name, 'port': port})
    return port


def render_template(template, params, is_file=True):
    """Renders Jinja2 template file with given parameters.

    :param template: full path to the Jinja2 template file
    :param params: dictionary with parameters to use when rendering
    :param is_file: whether template is file or string with template itself
    :returns: the rendered template as a string
    """
    if is_file:
        tmpl_path, tmpl_name = os.path.split(template)
        loader = jinja2.FileSystemLoader(tmpl_path)
    else:
        tmpl_name = 'template'
        loader = jinja2.DictLoader({tmpl_name: template})
    env = jinja2.Environment(loader=loader)
    tmpl = env.get_template(tmpl_name)
    return tmpl.render(params)


def warn_about_deprecated_extra_vif_port_id():
    global warn_deprecated_extra_vif_port_id
    if not warn_deprecated_extra_vif_port_id:
        warn_deprecated_extra_vif_port_id = True
        LOG.warning("Attaching VIF to a port/portgroup via "
                    "extra['vif_port_id'] is deprecated and will not "
                    "be supported in Pike release. API endpoint "
                    "v1/nodes/<node>/vifs should be used instead.")
