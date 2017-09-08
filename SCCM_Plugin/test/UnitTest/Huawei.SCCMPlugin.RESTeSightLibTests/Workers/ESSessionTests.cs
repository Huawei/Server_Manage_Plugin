using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Huawei.SCCMPlugin.Const;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using CommonUtil;
using Newtonsoft.Json;
using Huawei.SCCMPlugin.Models.Devices;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Moq;
using System.Net;
using System.IO;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers.Tests
{
    [TestClass()]
    public class ESSessionTests
    {
        static HWESightHost _hwESightHost = null;
        static ESSession _esSession = null;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            _hwESightHost = new HWESightHost();
            _hwESightHost.HostIP = "127.0.0.1";
            _hwESightHost.HostPort = 32102;
            _hwESightHost.LoginAccount = "test";
            _hwESightHost.LoginPwd = EncryptUtil.EncryptPwd("test");
            _esSession = new ESSession();
            _esSession.SetHttpMode(true);
            _esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);

        }

        [TestMethod()]
        public void InitESightTest()
        {
            _esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
        }

        [TestMethod()]
        public void InitESightTest1()
        {
            _esSession.InitESight(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd
              , ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
        }
        [TestMethod()]
        public void IsConnectedTest()
        {
            bool isConnect = _esSession.IsConnected();
            Assert.AreEqual(String.IsNullOrEmpty(_esSession.OpenID), !_esSession.IsConnected());
        }

        [TestMethod()]
        public void IsTimeoutTest()
        {
            ESSession esSession = new ESSession();
            esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
            Assert.AreEqual(true, esSession.IsTimeout());

        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void ChkOpenResultTest()
        {
            ESSession esSession = new ESSession();
            HWESightHost hwESightHost = new HWESightHost();
            hwESightHost.HostIP = "127.0.0.1";
            hwESightHost.HostPort = 32102;
            hwESightHost.LoginAccount = "test";
            hwESightHost.LoginPwd = "test";
            esSession = new ESSession();
            esSession.SetHttpMode(true);
            esSession.InitESight(hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);            
            JObject result = JsonUtil.DeserializeObject<JObject>("{\"code\":1,\"data\":\"bfec0163 - dd56 - 473e-b11f - 6d5845e1b684\", \"description\":\"Operation success.\"}");
            esSession.ChkOpenResult(result);
        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void HandleException1Test()
        {
            ESSession esSession = new ESSession();
            AggregateException ae = new AggregateException("1");
            esSession.HandleException(ae);
            List<Exception> exs = new List<Exception>();
        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void HandleException2Test()
        {
            ESSession esSession = new ESSession();
            var expected = "response content";
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new Mock<HttpWebResponse>();
            response.Setup(c => c.GetResponseStream()).Returns(responseStream);

            WebException we1 = new WebException("tset", new Exception("xx"), WebExceptionStatus.ConnectionClosed, response.Object);

            WebException we2 = new WebException("tset2");
            AggregateException ae = new AggregateException("1",we1,we2);
            esSession.HandleException(ae);
           
        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void HCCheckResultTestErr()
        {
            HttpResponseMessage rsp = new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway);
            rsp.Content =
            new StringContent("{\"code\":0,\"data\":\"bfec0163 - dd56 - 473e-b11f - 6d5845e1b684\", \"description\":\"Operation success.\"}"
              , Encoding.UTF8, "application/json");
            JObject jObject = _esSession.HCCheckResult("https://127.0.0.1", rsp);
            Console.WriteLine(jObject);
           // Assert.AreEqual(CoreUtil.GetObjTranNull<int>(jObject.Property("code")), 0);
        }
        [TestMethod()]
        public void HCCheckResultTest()
        {
            //HttpResponseMessage hrm = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            /*
             objectContent = new StringContent(content);
      objectContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpClientExtensions.PostAsJsonAsync<T>:

      await httpClient.PostAsJsonAsync(url, json);
             */
            HttpResponseMessage rsp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            //rsp.Content.Headers.Add("application/json");
            //rsp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            rsp.Content =
            new StringContent("{\"code\":0,\"data\":\"bfec0163 - dd56 - 473e-b11f - 6d5845e1b684\", \"description\":\"Operation success.\"}"
              , Encoding.UTF8, "application/json");
            JObject jObject = _esSession.HCCheckResult("https://127.0.0.1", rsp);
            Console.WriteLine(jObject);
            Assert.AreEqual(CoreUtil.GetObjTranNull<int>(jObject.Property("code")), 0);
        }
   
        private JObject GetLoginParam()
        {
            JObject paramJson = new JObject();
            paramJson.Add("userid", _hwESightHost.LoginAccount);
            paramJson.Add("value", _hwESightHost.LoginPwd);
            paramJson.Add("ipaddr", SystemUtil.GetLocalhostIP());
            return paramJson;
        }
        [TestMethod()]
        public void HCPostTest()
        {
            _esSession.Open();
            JObject result = _esSession.HCPut(ConstMgr.HWESightHost.URL_LOGIN, GetLoginParam());
            Console.WriteLine(result);
            Assert.AreEqual(CoreUtil.GetObjTranNull<int>(result.Property("code")), 0);
        }

        [TestMethod()]
        public void HCPutTest()
        {
            _esSession.Open();
            JObject result = _esSession.HCPut(ConstMgr.HWESightHost.URL_LOGIN, GetLoginParam());
            Console.WriteLine(result);
            Assert.AreEqual(CoreUtil.GetObjTranNull<int>(result.Property("code")), 0);
        }

        [TestMethod()]
        public void HCGetTest()
        {
            _esSession.Open();
            JObject result = _esSession.HCGet(ConstMgr.HWESightHost.URL_LOGIN);
            Console.WriteLine(result);
            Assert.AreEqual(CoreUtil.GetObjTranNull<int>(result.Property("code")), 0);
        }

        [TestMethod()]
        public void HCDeleteTest()
        {
            _esSession.Open();
            JObject result = _esSession.HCDelete(ConstMgr.HWESightHost.URL_LOGIN);
            Console.WriteLine(result);
            Assert.AreEqual(CoreUtil.GetObjTranNull<int>(result.Property("code")), 0);
        }

        [TestMethod()]
        public void OpenTest()
        {
            _esSession.Open();
            Assert.IsTrue(_esSession.IsConnected());
            Assert.IsTrue(!string.IsNullOrEmpty(_esSession.OpenID));
        }

        [TestMethod()]
        public void QueryHWPageTest()
        {
            _esSession.Open();
            DeviceParam queryDeviceParam = new DeviceParam();
            queryDeviceParam.ServerType = ConstMgr.HWDevice.SERVER_TYPE_BLADE;
            queryDeviceParam.StartPage = 1;
            queryDeviceParam.PageSize = 100;
            queryDeviceParam.PageOrder = "ipAddress";
            queryDeviceParam.OrderDesc = true;
            QueryPageResult<HWDevice> hwDevicePageResult = _esSession.QueryHWPage(queryDeviceParam);
            LogUtil.HWLogger.DEFAULT.Info(hwDevicePageResult);
            Assert.IsNotNull(hwDevicePageResult);
        }


        [TestMethod()]
        public void SetHttpModeTest()
        {
            _esSession.SetHttpMode(true);
        }



        [TestMethod()]
        public void QueryForDeviceDetailTest()
        {
            _esSession.Open();
            QueryListResult<HWDeviceDetail> hwDeviceDetails= _esSession.DviWorker.QueryForDeviceDetail("NE=34603409");
            LogUtil.HWLogger.DEFAULT.Info(JsonUtil.SerializeObject(hwDeviceDetails));
            Assert.IsNotNull(hwDeviceDetails);
        }

        [TestMethod()]
        public void CloseTest()
        {
            _esSession.Close();
        }


        [TestMethod()]
        public void SaveToDBTest()
        {

            using (ESSession esSession = new ESSession())
            {
                esSession.SetHttpMode(true);
                esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
                bool isSccu = esSession.SaveToDB();
                Assert.IsTrue(isSccu);
                Assert.IsTrue(esSession.HWESightHost.ID > 0);
            }
        }
        [TestMethod()]
        public void OpenHttpsTest()
        {
            //ESSession esSession = new ESSession();
            //HWESightHost hwESightHost = new HWESightHost();
            //hwESightHost.HostIP = "58.251.166.178";
            //hwESightHost.HostPort = 32102;// 31943;
            //hwESightHost.LoginAccount = "admin";
            //hwESightHost.LoginPwd = "esight@123";
            //esSession.SetHttpMode(true);
            //esSession.InitESight(hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
            //esSession.Open();
            //Assert.AreEqual(true, esSession.IsTimeout());

        }
        [TestMethod()]
        public void DeleteESightTest()
        {
            bool isSccu = _esSession.SaveToDB();
            Assert.IsTrue(isSccu);
            isSccu = _esSession.DeleteESight();
            Assert.IsTrue(isSccu);
        }
        [TestMethod()]
        public void DeleteESightTestZero()
        {
            ESSession esSession = new ESSession();
            HWESightHost hwESightHost = new HWESightHost();
            hwESightHost.HostIP = "127.0.0.1";
            hwESightHost.HostPort = 32102;
            hwESightHost.LoginAccount = "test";
            hwESightHost.LoginPwd = "test";
            esSession = new ESSession();
            esSession.SetHttpMode(true);
            esSession.InitESight(hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);

            bool isSccu = esSession.SaveToDB();
            Assert.IsTrue(isSccu);
            int oldId = esSession.HWESightHost.ID;
            esSession.HWESightHost.ID = 0;
            isSccu = esSession.DeleteESight();
            Assert.IsTrue(isSccu);
            esSession.HWESightHost.ID = oldId;
            esSession.DeleteESight();
        }
        [TestMethod()]
        public void DisposeESightTest()
        {
            ESSession esSession = new ESSession();
            HWESightHost hwESightHost = new HWESightHost();
            hwESightHost.HostIP = "127.0.0.1";
            hwESightHost.HostPort = 32102;
            hwESightHost.LoginAccount = "test";
            hwESightHost.LoginPwd = "test";
            esSession = new ESSession();
            esSession.SetHttpMode(true);
            esSession.InitESight(hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);
            esSession.Open();
            bool isSccu = esSession.SaveToDB();
            Assert.IsTrue(isSccu);

            esSession.HClient = null;
            //esSession.HWESightHost.HostIP = "xxx.xxx.xxx";
            
            esSession.Dispose();
        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void ChkWeb404()
        {
            _esSession.TryOpen();
            _esSession.HCGet("xxsdfsdf");
        }
        [TestMethod()]
        [ExpectedException(typeof(ESSessionExpceion))]
        public void ChkUnInitESightTest()
        {
            ESSession esSession = new ESSession();
            esSession.TryOpen();
        }
        [TestMethod()]
        public void DisposeTest()
        {
            _esSession.Dispose();
        }

        [TestMethod()]
        public void QueryHWPageJsonTest()
        {
            _esSession.Open();
            DeviceParam queryDeviceParam = new DeviceParam();
            queryDeviceParam.ServerType = ConstMgr.HWDevice.SERVER_TYPE_BLADE;
            queryDeviceParam.StartPage = 1;
            queryDeviceParam.PageSize = 100;
            queryDeviceParam.PageOrder = "ipAddress";
            queryDeviceParam.OrderDesc = true;
            WebOneESightParam<DeviceParam> webQueryParam = new WebOneESightParam<DeviceParam>();
            webQueryParam.ESightIP = "127.0.0.1";
            webQueryParam.Param = queryDeviceParam;
            LogUtil.HWLogger.API.Info("queryDeviceParam:" + JsonUtil.SerializeObject(webQueryParam));

            QueryPageResult<HWDevice> hwDevicePageResult = _esSession.QueryHWPage(queryDeviceParam);
            LogUtil.HWLogger.API.Info("hwDevicePageResult:" + JsonUtil.SerializeObject(hwDevicePageResult));
            Assert.IsNotNull(hwDevicePageResult);
        }


    }
}