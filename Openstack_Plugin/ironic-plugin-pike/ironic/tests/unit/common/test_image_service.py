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

import datetime
import os
import shutil

import mock
from oslo_config import cfg
import requests
import sendfile
import six
import six.moves.builtins as __builtin__
from six.moves import http_client

from ironic.common import exception
from ironic.common.glance_service.v1 import image_service as glance_v1_service
from ironic.common.glance_service.v2 import image_service as glance_v2_service
from ironic.common import image_service
from ironic.tests import base

if six.PY3:
    import io
    file = io.BytesIO


class HttpImageServiceTestCase(base.TestCase):
    def setUp(self):
        super(HttpImageServiceTestCase, self).setUp()
        self.service = image_service.HttpImageService()
        self.href = 'http://127.0.0.1:12345/fedora.qcow2'

    @mock.patch.object(requests, 'head', autospec=True)
    def test_validate_href(self, head_mock):
        response = head_mock.return_value
        response.status_code = http_client.OK
        self.service.validate_href(self.href)
        head_mock.assert_called_once_with(self.href)
        response.status_code = http_client.NO_CONTENT
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.validate_href,
                          self.href)
        response.status_code = http_client.BAD_REQUEST
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.validate_href,
                          self.href)

    @mock.patch.object(requests, 'head', autospec=True)
    def test_validate_href_error_code(self, head_mock):
        head_mock.return_value.status_code = http_client.BAD_REQUEST
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.validate_href, self.href)
        head_mock.assert_called_once_with(self.href)

    @mock.patch.object(requests, 'head', autospec=True)
    def test_validate_href_error(self, head_mock):
        head_mock.side_effect = requests.ConnectionError()
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.validate_href, self.href)
        head_mock.assert_called_once_with(self.href)

    @mock.patch.object(requests, 'head', autospec=True)
    def test_validate_href_error_with_secret_parameter(self, head_mock):
        head_mock.return_value.status_code = 204
        e = self.assertRaises(exception.ImageRefValidationFailed,
                              self.service.validate_href,
                              self.href,
                              True)
        self.assertIn('secreturl', six.text_type(e))
        self.assertNotIn(self.href, six.text_type(e))
        head_mock.assert_called_once_with(self.href)

    @mock.patch.object(requests, 'head', autospec=True)
    def _test_show(self, head_mock, mtime, mtime_date):
        head_mock.return_value.status_code = http_client.OK
        head_mock.return_value.headers = {
            'Content-Length': 100,
            'Last-Modified': mtime
        }
        result = self.service.show(self.href)
        head_mock.assert_called_once_with(self.href)
        self.assertEqual({'size': 100, 'updated_at': mtime_date,
                          'properties': {}}, result)

    def test_show_rfc_822(self):
        self._test_show(mtime='Tue, 15 Nov 2014 08:12:31 GMT',
                        mtime_date=datetime.datetime(2014, 11, 15, 8, 12, 31))

    def test_show_rfc_850(self):
        self._test_show(mtime='Tuesday, 15-Nov-14 08:12:31 GMT',
                        mtime_date=datetime.datetime(2014, 11, 15, 8, 12, 31))

    def test_show_ansi_c(self):
        self._test_show(mtime='Tue Nov 15 08:12:31 2014',
                        mtime_date=datetime.datetime(2014, 11, 15, 8, 12, 31))

    @mock.patch.object(requests, 'head', autospec=True)
    def test_show_no_content_length(self, head_mock):
        head_mock.return_value.status_code = http_client.OK
        head_mock.return_value.headers = {}
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.show, self.href)
        head_mock.assert_called_with(self.href)

    @mock.patch.object(shutil, 'copyfileobj', autospec=True)
    @mock.patch.object(requests, 'get', autospec=True)
    def test_download_success(self, req_get_mock, shutil_mock):
        response_mock = req_get_mock.return_value
        response_mock.status_code = http_client.OK
        response_mock.raw = mock.MagicMock(spec=file)
        file_mock = mock.Mock(spec=file)
        self.service.download(self.href, file_mock)
        shutil_mock.assert_called_once_with(
            response_mock.raw.__enter__(), file_mock,
            image_service.IMAGE_CHUNK_SIZE
        )
        req_get_mock.assert_called_once_with(self.href, stream=True)

    @mock.patch.object(requests, 'get', autospec=True)
    def test_download_fail_connerror(self, req_get_mock):
        req_get_mock.side_effect = requests.ConnectionError()
        file_mock = mock.Mock(spec=file)
        self.assertRaises(exception.ImageDownloadFailed,
                          self.service.download, self.href, file_mock)

    @mock.patch.object(shutil, 'copyfileobj', autospec=True)
    @mock.patch.object(requests, 'get', autospec=True)
    def test_download_fail_ioerror(self, req_get_mock, shutil_mock):
        response_mock = req_get_mock.return_value
        response_mock.status_code = http_client.OK
        response_mock.raw = mock.MagicMock(spec=file)
        file_mock = mock.Mock(spec=file)
        shutil_mock.side_effect = IOError
        self.assertRaises(exception.ImageDownloadFailed,
                          self.service.download, self.href, file_mock)
        req_get_mock.assert_called_once_with(self.href, stream=True)


class FileImageServiceTestCase(base.TestCase):
    def setUp(self):
        super(FileImageServiceTestCase, self).setUp()
        self.service = image_service.FileImageService()
        self.href = 'file:///home/user/image.qcow2'
        self.href_path = '/home/user/image.qcow2'

    @mock.patch.object(os.path, 'isfile', return_value=True, autospec=True)
    def test_validate_href(self, path_exists_mock):
        self.service.validate_href(self.href)
        path_exists_mock.assert_called_once_with(self.href_path)

    @mock.patch.object(os.path, 'isfile', return_value=False, autospec=True)
    def test_validate_href_path_not_found_or_not_file(self, path_exists_mock):
        self.assertRaises(exception.ImageRefValidationFailed,
                          self.service.validate_href, self.href)
        path_exists_mock.assert_called_once_with(self.href_path)

    @mock.patch.object(os.path, 'getmtime', return_value=1431087909.1641912,
                       autospec=True)
    @mock.patch.object(os.path, 'getsize', return_value=42, autospec=True)
    @mock.patch.object(image_service.FileImageService, 'validate_href',
                       autospec=True)
    def test_show(self, _validate_mock, getsize_mock, getmtime_mock):
        _validate_mock.return_value = self.href_path
        result = self.service.show(self.href)
        getsize_mock.assert_called_once_with(self.href_path)
        getmtime_mock.assert_called_once_with(self.href_path)
        _validate_mock.assert_called_once_with(mock.ANY, self.href)
        self.assertEqual({'size': 42,
                          'updated_at': datetime.datetime(2015, 5, 8,
                                                          12, 25, 9, 164191),
                          'properties': {}}, result)

    @mock.patch.object(os, 'link', autospec=True)
    @mock.patch.object(os, 'remove', autospec=True)
    @mock.patch.object(os, 'access', return_value=True, autospec=True)
    @mock.patch.object(os, 'stat', autospec=True)
    @mock.patch.object(image_service.FileImageService, 'validate_href',
                       autospec=True)
    def test_download_hard_link(self, _validate_mock, stat_mock, access_mock,
                                remove_mock, link_mock):
        _validate_mock.return_value = self.href_path
        stat_mock.return_value.st_dev = 'dev1'
        file_mock = mock.Mock(spec=file)
        file_mock.name = 'file'
        self.service.download(self.href, file_mock)
        _validate_mock.assert_called_once_with(mock.ANY, self.href)
        self.assertEqual(2, stat_mock.call_count)
        access_mock.assert_called_once_with(self.href_path, os.R_OK | os.W_OK)
        remove_mock.assert_called_once_with('file')
        link_mock.assert_called_once_with(self.href_path, 'file')

    @mock.patch.object(sendfile, 'sendfile', autospec=True)
    @mock.patch.object(os.path, 'getsize', return_value=42, autospec=True)
    @mock.patch.object(__builtin__, 'open', autospec=True)
    @mock.patch.object(os, 'access', return_value=False, autospec=True)
    @mock.patch.object(os, 'stat', autospec=True)
    @mock.patch.object(image_service.FileImageService, 'validate_href',
                       autospec=True)
    def test_download_copy(self, _validate_mock, stat_mock, access_mock,
                           open_mock, size_mock, copy_mock):
        _validate_mock.return_value = self.href_path
        stat_mock.return_value.st_dev = 'dev1'
        file_mock = mock.MagicMock(spec=file)
        file_mock.name = 'file'
        input_mock = mock.MagicMock(spec=file)
        open_mock.return_value = input_mock
        self.service.download(self.href, file_mock)
        _validate_mock.assert_called_once_with(mock.ANY, self.href)
        self.assertEqual(2, stat_mock.call_count)
        access_mock.assert_called_once_with(self.href_path, os.R_OK | os.W_OK)
        copy_mock.assert_called_once_with(file_mock.fileno(),
                                          input_mock.__enter__().fileno(),
                                          0, 42)
        size_mock.assert_called_once_with(self.href_path)

    @mock.patch.object(os, 'remove', side_effect=OSError, autospec=True)
    @mock.patch.object(os, 'access', return_value=True, autospec=True)
    @mock.patch.object(os, 'stat', autospec=True)
    @mock.patch.object(image_service.FileImageService, 'validate_href',
                       autospec=True)
    def test_download_hard_link_fail(self, _validate_mock, stat_mock,
                                     access_mock, remove_mock):
        _validate_mock.return_value = self.href_path
        stat_mock.return_value.st_dev = 'dev1'
        file_mock = mock.MagicMock(spec=file)
        file_mock.name = 'file'
        self.assertRaises(exception.ImageDownloadFailed,
                          self.service.download, self.href, file_mock)
        _validate_mock.assert_called_once_with(mock.ANY, self.href)
        self.assertEqual(2, stat_mock.call_count)
        access_mock.assert_called_once_with(self.href_path, os.R_OK | os.W_OK)

    @mock.patch.object(sendfile, 'sendfile', side_effect=OSError,
                       autospec=True)
    @mock.patch.object(os.path, 'getsize', return_value=42, autospec=True)
    @mock.patch.object(__builtin__, 'open', autospec=True)
    @mock.patch.object(os, 'access', return_value=False, autospec=True)
    @mock.patch.object(os, 'stat', autospec=True)
    @mock.patch.object(image_service.FileImageService, 'validate_href',
                       autospec=True)
    def test_download_copy_fail(self, _validate_mock, stat_mock, access_mock,
                                open_mock, size_mock, copy_mock):
        _validate_mock.return_value = self.href_path
        stat_mock.return_value.st_dev = 'dev1'
        file_mock = mock.MagicMock(spec=file)
        file_mock.name = 'file'
        input_mock = mock.MagicMock(spec=file)
        open_mock.return_value = input_mock
        self.assertRaises(exception.ImageDownloadFailed,
                          self.service.download, self.href, file_mock)
        _validate_mock.assert_called_once_with(mock.ANY, self.href)
        self.assertEqual(2, stat_mock.call_count)
        access_mock.assert_called_once_with(self.href_path, os.R_OK | os.W_OK)
        size_mock.assert_called_once_with(self.href_path)


class ServiceGetterTestCase(base.TestCase):

    @mock.patch.object(image_service, '_get_glance_session')
    @mock.patch.object(glance_v2_service.GlanceImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_glance_image_service(self, glance_service_mock,
                                      session_mock):
        image_href = 'image-uuid'
        self.context.auth_token = 'fake'
        image_service.get_image_service(image_href, context=self.context)
        glance_service_mock.assert_called_once_with(mock.ANY, None, 2,
                                                    self.context)
        self.assertFalse(session_mock.called)

    @mock.patch.object(image_service, '_get_glance_session')
    @mock.patch.object(glance_v1_service.GlanceImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_glance_image_service_default_v1(self, glance_service_mock,
                                                 session_mock):
        self.config(glance_api_version=1, group='glance')
        image_href = 'image-uuid'
        self.context.auth_token = 'fake'
        image_service.get_image_service(image_href, context=self.context)
        glance_service_mock.assert_called_once_with(mock.ANY, None, 1,
                                                    self.context)
        self.assertFalse(session_mock.called)

    @mock.patch.object(image_service, '_get_glance_session')
    @mock.patch.object(glance_v2_service.GlanceImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_glance_image_service_url(self, glance_service_mock,
                                          session_mock):
        image_href = 'glance://image-uuid'
        self.context.auth_token = 'fake'
        image_service.get_image_service(image_href, context=self.context)
        glance_service_mock.assert_called_once_with(mock.ANY, None, 2,
                                                    self.context)
        self.assertFalse(session_mock.called)

    @mock.patch.object(image_service, '_get_glance_session')
    @mock.patch.object(glance_v2_service.GlanceImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_glance_image_service_no_token(self, glance_service_mock,
                                               session_mock):
        image_href = 'image-uuid'
        self.context.auth_token = None
        sess = mock.Mock()
        sess.get_token.return_value = 'admin-token'
        session_mock.return_value = sess
        image_service.get_image_service(image_href, context=self.context)
        glance_service_mock.assert_called_once_with(mock.ANY, None, 2,
                                                    self.context)
        sess.get_token.assert_called_once_with()
        self.assertEqual('admin-token', self.context.auth_token)

    @mock.patch.object(image_service, '_get_glance_session')
    @mock.patch.object(glance_v2_service.GlanceImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_glance_image_service_token_not_needed(self,
                                                       glance_service_mock,
                                                       session_mock):
        image_href = 'image-uuid'
        self.context.auth_token = None
        self.config(auth_strategy='noauth', group='glance')
        image_service.get_image_service(image_href, context=self.context)
        glance_service_mock.assert_called_once_with(mock.ANY, None, 2,
                                                    self.context)
        self.assertFalse(session_mock.called)
        self.assertIsNone(self.context.auth_token)

    @mock.patch.object(image_service.HttpImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_http_image_service(self, http_service_mock):
        image_href = 'http://127.0.0.1/image.qcow2'
        image_service.get_image_service(image_href)
        http_service_mock.assert_called_once_with()

    @mock.patch.object(image_service.HttpImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_https_image_service(self, http_service_mock):
        image_href = 'https://127.0.0.1/image.qcow2'
        image_service.get_image_service(image_href)
        http_service_mock.assert_called_once_with()

    @mock.patch.object(image_service.FileImageService, '__init__',
                       return_value=None, autospec=True)
    def test_get_file_image_service(self, local_service_mock):
        image_href = 'file:///home/user/image.qcow2'
        image_service.get_image_service(image_href)
        local_service_mock.assert_called_once_with()

    def test_get_image_service_unknown_protocol(self):
        image_href = 'usenet://alt.binaries.dvd/image.qcow2'
        self.assertRaises(exception.ImageRefValidationFailed,
                          image_service.get_image_service, image_href)

    def test_out_range_auth_strategy(self):
        self.assertRaises(ValueError, cfg.CONF.set_override,
                          'auth_strategy', 'fake', 'glance')

    def test_out_range_glance_protocol(self):
        self.assertRaises(ValueError, cfg.CONF.set_override,
                          'glance_protocol', 'fake', 'glance')
