# Copyright 2017 Red Hat, Inc.
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

import json
import logging

import requests
from six.moves.urllib import parse

from hwsushy import exceptions

LOG = logging.getLogger(__name__)

SESSION_URL = '/redfish/v1/SessionService/Sessions'


class Connector(object):

    def __init__(self, url, username=None, password=None, verify=True):
        self._url = url
        self._username = username
        self._password = password
        self._verify = verify
        self._etag = None
        self._session_id = None
        self._auth_token = self._get_auth_token()

    def _get_auth_token(self):
        """Get session first"""
        data = {"UserName": self._username,
                "Password": self._password}

        resp = self._op('POST', self._url + SESSION_URL, data)

        if 'X-Auth-Token' not in resp.headers:
            raise exceptions.ConnectionError(url=self._url, error='no auth')

        location = resp.headers['Location'].split('/')
        self._session_id = location.pop()

        return resp.headers['X-Auth-Token']

    def _op(self, method, path='', data=None, headers=None):
        """Generic RESTful request handler.

        :param method: The HTTP method to be used, e.g: GET, POST,
            PUT, PATCH, etc...
        :param path: The sub-URI path to the resource.
        :param data: Optional JSON data.
        :param headers: Optional dictionary of headers.
        :returns: The response object from the requests library.
        :raises: ConnectionError
        :raises: HTTPError
        """
        if headers is None:
            headers = {}

        headers['Content-Type'] = 'application/json'

        if hasattr(self, '_auth_token'):
            headers['X-Auth-Token'] = self._auth_token

        if 'If-Match' in headers and self._etag:
            headers['If-Match'] = self._etag

        with requests.Session() as session:
            session.headers = headers
            session.verify = self._verify

            if data is not None:
                data = json.dumps(data)

            url = parse.urljoin(self._url, path)
            # TODO(lucasagomes): We should mask the data to remove sensitive
            # information
            LOG.debug('Issuing a HTTP %(method)s request at %(url)s with '
                      'the headers "%(headers)s" and data "%(data)s"',
                      {'method': method, 'url': url, 'headers': headers,
                       'data': data or ''})
            try:
                response = session.request(method, url, data=data)
            except requests.ConnectionError as e:
                raise exceptions.ConnectionError(url=url, error=e)

            LOG.debug('Response: Status code: %d', response.status_code)
            try:
                response.raise_for_status()
            except requests.HTTPError as e:
                raise exceptions.HTTPError(
                    method=method, url=url, error=e,
                    status_code=e.response.status_code)

            # Notes: keep Etag
            if 'ETag' in response.headers:
                self._etag = response.headers['Etag']

            return response

    def get(self, path='', data=None, headers=None):
        """HTTP GET method.

        :param path: Optional sub-URI path to the resource.
        :param data: Optional JSON data.
        :param headers: Optional dictionary of headers.
        :returns: The response object from the requests library.
        :raises: ConnectionError
        :raises: HTTPError
        """
        return self._op('GET', path, data, headers)

    def post(self, path='', data=None, headers=None):
        """HTTP POST method.

        :param path: Optional sub-URI path to the resource.
        :param data: Optional JSON data.
        :param headers: Optional dictionary of headers.
        :returns: The response object from the requests library.
        :raises: ConnectionError
        :raises: HTTPError
        """
        return self._op('POST', path, data, headers)

    def patch(self, path='', data=None, headers=None):
        """HTTP PATCH method.

        :param path: Optional sub-URI path to the resource.
        :param data: Optional JSON data.
        :param headers: Optional dictionary of headers.
        :returns: The response object from the requests library.
        :raises: ConnectionError
        :raises: HTTPError
        """
        return self._op('PATCH', path, data, headers)

    def delete(self, path='', headers=None):
        """HTTP DELETE method.

        :param path: Optional sub-URI path to the resource.
        :param headers: Optional dictionary of headers.
        :returns: The response object from the requests library.
        :raises: ConnectionError
        :raises: HTTPError
        """
        return self._op('DELETE', path, headers)

    def logoff(self):
        """logoff method, logoff bmc.

        :returns: None
        :raises: ConnectionError
        :raises: HTTPError

        """
        return self.delete(SESSION_URL + '/' + self._session_id)
