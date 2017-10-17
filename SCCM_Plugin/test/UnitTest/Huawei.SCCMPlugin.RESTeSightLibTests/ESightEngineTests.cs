﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using Huawei.SCCMPlugin.Models.Devices;
using Newtonsoft.Json;
using CommonUtil;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.Models.Softwares;

namespace Huawei.SCCMPlugin.RESTeSightLib.Tests
{
    [TestClass()]
    public class ESightEngineTests
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
            _hwESightHost.LoginPwd =EncryptUtil.EncryptPwd("test");
            _esSession = new ESSession();
            _esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);

            //ESightEngine.Instance.SetHttpMode(true);
        }

        [TestMethod()]
        public void InitESSessionsTest()
        {
            ESightEngine.Instance.InitESSessions();
        }

        [TestMethod()]
        public void ListESSessionsTest()
        {

            IList<IESSession> list = ESightEngine.Instance.ListESSessions();
            Assert.IsNotNull(list);
        }
        [TestMethod()]
        public void ListESSessionsToJsonTest()
        {
            SaveNewESSessionTest();
            IList<HWESightHost> list = ESightEngine.Instance.ListESHost();
            IList<HWESightHost> retlist = new List<HWESightHost>();
            foreach (HWESightHost host in list)
            {
                host.LoginPwd = "";
                retlist.Add(host);
            }
            LogUtil.HWLogger.API.Info("tttt:" + JsonUtil.SerializeObject(list));
            Assert.IsNotNull(list);
        }
        [TestMethod()]
        public void SaveNewESSessionTest()
        {
            IESSession iSession = ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            Assert.IsNotNull(iSession);
            iSession = ESightEngine.Instance.SaveESSession(_hwESightHost.HostIP,"Tom", _hwESightHost.HostPort);
            Assert.IsNotNull(iSession);
        }
        [TestMethod()]
        [ExpectedException(typeof(BaseException))]
        public void SaveESSessionFaileTest()
        {
            IESSession iSession = ESightEngine.Instance.SaveESSession("133.300.621.118", "Tom",1433);

        }
        [TestMethod()]
        public void RemoveESSessionTest()
        {

            IESSession iSession = ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            Assert.IsNotNull(iSession);
            bool removed = ESightEngine.Instance.RemoveESSession(new string[] { _hwESightHost.HostIP });
            Assert.IsTrue(removed);
        }



        [TestMethod()]
        public void TestESSessionTest()
        {
            string retMsg = ESightEngine.Instance.TestESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            LogUtil.HWLogger.API.Error(retMsg);
            Assert.IsTrue(string.IsNullOrEmpty(retMsg));
        }

        [TestMethod()]
        public void FindESSessionTest()
        {
            IESSession iSession = ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            Assert.IsNotNull(iSession);
            iSession = ESightEngine.Instance.FindESSession(_hwESightHost.HostIP);
            Assert.IsNotNull(iSession);
            iSession = ESightEngine.Instance.FindESSession("xxxx");
            Assert.IsNull(iSession);
        }

        [TestMethod()]
        public void FindAndOpenESSessionTest()
        {
            IESSession iSession = ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            Assert.IsNotNull(iSession);
            iSession = ESightEngine.Instance.FindAndOpenESSession(_hwESightHost.HostIP);
            Assert.IsNotNull(iSession);
            Assert.IsTrue(iSession.IsConnected());
        }
        [TestMethod()]
        public void TestOpenId()
        {
            ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            IESSession iSession = ESightEngine.Instance.FindAndOpenESSession(_hwESightHost.HostIP);
            DeviceParam queryDeviceParam = new DeviceParam();
            queryDeviceParam.ServerType = ConstMgr.HWDevice.SERVER_TYPE_BLADE;
            queryDeviceParam.StartPage = 1;
            queryDeviceParam.PageSize = 100;
            queryDeviceParam.PageOrder = "ipAddress";
            queryDeviceParam.OrderDesc = true;
            QueryPageResult<HWDevice> hwDevicePageResult = iSession.QueryHWPage(queryDeviceParam);
            LogUtil.HWLogger.DEFAULT.Info(hwDevicePageResult);
            Assert.IsTrue(!string.IsNullOrEmpty(iSession.OpenID));
        }
        [TestMethod()]
        public void TestJsonResult()
        {
            List<WebReturnESightResult> list = new List<WebReturnESightResult>();
            WebReturnESightResult webReturnESightResult1 = new WebReturnESightResult();
            webReturnESightResult1.ESightIp = "127.0.0.1";
            webReturnESightResult1.Code = 0;
            webReturnESightResult1.Description = "成功";
            list.Add(webReturnESightResult1);
            WebReturnESightResult webReturnESightResult2 = new WebReturnESightResult();
            webReturnESightResult2.ESightIp = "127.0.0.3";
            webReturnESightResult2.Code = 51001;
            webReturnESightResult2.Description = "模板名称重复，模板已存在";
            list.Add(webReturnESightResult2);
            WebReturnResult<List<WebReturnESightResult>> taskResult = new WebReturnResult<List<WebReturnESightResult>>();
            taskResult.Data = list;
            taskResult.Code = 51001;
            taskResult.Description = "模板名称重复，模板已存在";
            LogUtil.HWLogger.API.Info("TestJsonResult Result:" + JsonUtil.SerializeObject(taskResult));
        }

        [TestMethod()]
        public void FindAllBasePackageTaskWithSyncTest()
        {
            IList<HWESightTask> taskList = ESightEngine.Instance.FindAllBasePackageTaskWithSync();
            LogUtil.HWLogger.API.InfoFormat("FindAllBasePackageTaskWithSyncTest Result:" + JsonUtil.SerializeObject(taskList));
        }

        [TestMethod()]
        public void RemoveESSessionTest1()
        {
            IESSession iSession = ESightEngine.Instance.SaveNewESSession(_hwESightHost.HostIP, _hwESightHost.HostPort, _hwESightHost.LoginAccount, _hwESightHost.LoginPwd);
            Assert.IsNotNull(iSession);
            iSession = ESightEngine.Instance.FindESSession(_hwESightHost.HostIP);
            ESightEngine.Instance.RemoveESSession(new int[] { iSession.HWESightHost.ID });            
        }
        [TestMethod()]
        public void FindAllSoftwareSourceTaskWithSyncTest()
        {

            List<HWESightTask> taskList = new List<HWESightTask>(ESightEngine.Instance.FindAllSoftwareSourceTaskWithSync());
            WebReturnResult<List<HWESightTask>> ret = new WebReturnResult<List<HWESightTask>>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = taskList;

            LogUtil.HWLogger.API.InfoFormat("FindAllSoftwareSourceTaskWithSyncTest Result:" + JsonUtil.SerializeObject(taskList));
        }
        [TestMethod()]
        public void CleaSoftwareFailedTask()
        {
            var delTask = new { };
            LogUtil.HWLogger.API.Info("ClearFailedTask packageParam:" + JsonUtil.SerializeObject(delTask));

            WebReturnResult<int> ret = new WebReturnResult<int>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = ESightEngine.Instance.ClearAllFailedSoftwareSourceTask();

            LogUtil.HWLogger.API.Info("ClearFailedTask QueryPageResult:" + JsonUtil.SerializeObject(ret));
        }
        [TestMethod()]
        public void RemoveESSessionTest2()
        {
            ESightEngine.Instance.RemoveESSession(-1);
        }
        [TestMethod()]
        public void EncryptPwd() {

            string testPwd1 = "中文";
            string testPwdEn = EncryptUtil.EncryptPwd(testPwd1);
            Console.WriteLine(testPwdEn);
            string testPwd2 = EncryptUtil.DecryptPwd(testPwdEn);
            Console.WriteLine(testPwd2);

            string pwd3 = "frnAd1kWhLS3CVRCshbo0TGqSc6irpTdZDzb0k0IXV8=";
            string testPwd3 = EncryptUtil.DecryptPwd(pwd3);
            Console.WriteLine(testPwd3);
            string testPwd4 = EncryptUtil.DecryptPwd(testPwd3);
            Console.WriteLine(testPwd4);
            Assert.IsTrue(testPwd1.Equals(testPwd2));
            
        }
        [TestMethod()]
        public void RefreshPwdsTest()
        {
            ESightEngine.Instance.SaveNewESSession(_esSession.HWESightHost.HostIP, _esSession.HWESightHost.HostPort, _esSession.HWESightHost.LoginAccount, _esSession.HWESightHost.LoginPwd);
           
            ESightEngine.Instance.RefreshPwds();
            IList<HWESightHost>  hostlist=ESightEngine.Instance.ListESHost();
            foreach (HWESightHost host in hostlist) {
                LogUtil.HWLogger.API.Info(EncryptUtil.DecryptPwd(host.LoginPwd));
            }
        }
        private static string XOrTwoString(string str1, string str2)
        {
            byte[] aBytes = strToToHexByte(str1);
            byte[] bytes = strToToHexByte(str2);

            byte[] rBytes = new byte[aBytes.Length];
            for (int i = 0; i < aBytes.Length; i++)
            {
                rBytes[i] = (byte)(aBytes[i] ^ bytes[i]);
            }
            string result = System.Text.Encoding.Unicode.GetString(rBytes);
            return result;
        }
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        [TestMethod()]
        public void GenPwdTest()
        {
            // string key = "";
            //string randKey=""
            string plainText = "75730F7107706007C077700057D05"; //XOrTwoString(key, randKey);  // original plaintext

            //  LogUtil.HWLogger.DEFAULT.Info("plainText..." +Encoding.Unicode.GetBytes(plainText)+":"
            //+BitConverter.ToString(Encoding.Unicode.GetBytes(plainText)).Replace("-", String.Empty));


            string saltValue = "668DAFB758034A97";        // can be any string
            string hashAlgorithm = "SHA256";             // can be "MD5"
            int passwordIterations = 10000;                // can be any number
            int keySize = 128;                // can be 192 or 128

            /*
             string trueEnKey = GenRootKey(plainText, saltValue,
                     hashAlgorithm, passwordIterations, keySize);*/
            
            string trueEnKey = EncryptUtil.GenRootKey(plainText, saltValue, passwordIterations, keySize);

            byte[] keyBytes = new byte[32];
            byte[] saltValueBytes= strToToHexByte(saltValue);
            EncryptUtil.PKCS5_PBKDF2_HMAC(plainText, 16, saltValueBytes, 16, passwordIterations, EncryptUtil.EVP_sha256(), 32, keyBytes);
            trueEnKey=BitConverter.ToString(keyBytes).Replace("-", String.Empty);


            LogUtil.HWLogger.DEFAULT.Info("trueEnKey..." + trueEnKey);
            Console.WriteLine(trueEnKey);

        }

        [TestMethod()]
        public void LogTest1()
        {
            LogUtil.HWLogger.API.Error("test", new Exception("sdfasdfasfasd"));
        }
        [TestMethod()]
        public void LogTest2()
        {
            LogUtil.HWLogger.API.Error("中文标题", new Exception("中文错误"));
        }
    }
}