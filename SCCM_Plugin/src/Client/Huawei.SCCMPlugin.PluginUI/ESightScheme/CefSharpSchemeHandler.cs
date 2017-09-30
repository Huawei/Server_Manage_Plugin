using System;
using System.IO;
using System.Net;
using CefSharp;
using LogUtil;
using Cookie = CefSharp.Cookie;

namespace Huawei.SCCMPlugin.PluginUI.ESightScheme
{
    /// <summary>
    /// 实现对CefSharp请求资源处理
    /// </summary>
    internal class CefSharpSchemeHandler : IResourceHandler
    {

        private string mimeType;
        private MemoryStream stream;

        bool IResourceHandler.ProcessRequest(IRequest request, ICallback callback)
        {
            var uri = new Uri(request.Url);
            string fullPath = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, uri.Authority + uri.AbsolutePath);
            if (File.Exists(fullPath))
            {
                stream = new MemoryStream(File.ReadAllBytes(fullPath));
                var fileExtension = Path.GetExtension(uri.AbsolutePath);
                mimeType = ResourceHandler.GetMimeType(fileExtension);
                callback.Continue();
                return true;
            }
            else
            {
                HWLogger.DEFAULT.Error("error:can not find filename:" + Path.GetFullPath(fullPath));
                callback.Dispose();
                return false;
            }
        }

        void IResourceHandler.GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            responseLength = stream == null ? 0 : stream.Length;
            redirectUrl = null;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusText = "OK";
            response.MimeType = mimeType;
        }

        bool IResourceHandler.ReadResponse(Stream dataOut, out int bytesRead, ICallback callback)
        {
            //Dispose the callback as it's an unmanaged resource, we don't need it in this case
            callback.Dispose();

            if (stream == null)
            {
                bytesRead = 0;
                return false;
            }

            //Data out represents an underlying buffer (typically 32kb in size).
            var buffer = new byte[dataOut.Length];
            bytesRead = stream.Read(buffer, 0, buffer.Length);

            dataOut.Write(buffer, 0, buffer.Length);

            return bytesRead > 0;
        }

        bool IResourceHandler.CanGetCookie(Cookie cookie)
        {
            return true;
        }

        bool IResourceHandler.CanSetCookie(Cookie cookie)
        {
            return true;
        }

        void IResourceHandler.Cancel()
        {

        }

    }
}
