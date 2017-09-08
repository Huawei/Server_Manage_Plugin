using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Devices;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers.Tests
{
    [TestClass()]
    public class DeviceWorkerTests
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
            _hwESightHost.LoginPwd = "test";
            _esSession = new ESSession();
            _esSession.SetHttpMode(true);
            _esSession.InitESight(_hwESightHost, ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC);

        }
        [TestMethod()]
        public void InstallOSTest()
        {
            //Assert.Fail();
        }

        [TestMethod()]
        public void PowerDownTest()
        {
            // Assert.Fail();
        }

        [TestMethod()]
        public void PowerOnTest()
        {
            // Assert.Fail();
        }


        [TestMethod()]
        public void QueryForDeviceDetailTest()
        {
            _esSession.Open();
            QueryListResult<HWDeviceDetail> hwDetails = _esSession.DviWorker.QueryForDeviceDetail("test");
            Assert.IsNotNull(hwDetails);
        }
    }
}