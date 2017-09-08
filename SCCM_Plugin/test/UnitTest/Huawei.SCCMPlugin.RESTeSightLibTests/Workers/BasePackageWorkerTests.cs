using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using CommonUtil;
using Huawei.SCCMPlugin.PluginUI.Entitys;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.Models.Firmware;
using Huawei.SCCMPlugin.Models.Deploy;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Moq;
using System.Web;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers.Tests
{

    [TestClass()]
    public class BasePackageWorkerTests
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
        public void UploadBasePackageTest()
        {
            _esSession.Open();
            BasePackage basePackage = new BasePackage();
            basePackage.BasepackageName = "basepackage1";
            basePackage.BasepackageDescription = "This is a basepackage.";
            basePackage.BasepackageType = ConstMgr.HWBasePackage.PACKAGE_TYPE_FIRMWARE;
            basePackage.FileList = "iBMC.zip,iBMC.zip.asc";
            basePackage.SftpserverIP = "188.10.18.188";
            basePackage.SftpUserName = "itSftpUser";
            basePackage.SftpPassword = "Huawei@123";
            string taskName = _esSession.BasePackageWorker.UploadBasePackage(basePackage);
            Console.WriteLine(taskName);
            Assert.IsTrue(!string.IsNullOrEmpty(taskName));
        }
        [TestMethod()]
        public void UploadBasePackageJsonTest()
        {
            _esSession.Open();
            BasePackage basePackage = new BasePackage();
            basePackage.BasepackageName = "basepackage1";
            basePackage.BasepackageDescription = "This is a basepackage.";
            basePackage.BasepackageType = ConstMgr.HWBasePackage.PACKAGE_TYPE_FIRMWARE;
            basePackage.FileList = "iBMC.zip,iBMC.zip.asc";
            basePackage.SftpserverIP = "188.10.18.188";
            basePackage.SftpUserName = "itSftpUser";
            basePackage.SftpPassword = "Huawei@123";
            WebMutilESightsParam<BasePackage> postESightParam = new WebMutilESightsParam<BasePackage>();
            postESightParam.ESights = new List<string>() { "127.0.0.1" };
            postESightParam.Data = basePackage;

            LogUtil.HWLogger.API.Info("UploadBasePackageJsonTest Params:" + JsonUtil.SerializeObject(postESightParam));

            string taskName = _esSession.BasePackageWorker.UploadBasePackage(basePackage);


            JObject taskObject = new JObject();
            taskObject.Add("taskName", taskName);
            WebReturnResult<JObject> taskResult = new WebReturnResult<JObject>();
            taskResult.Data = taskObject;
            taskResult.Description = "";
            LogUtil.HWLogger.API.Info("UploadBasePackageJsonTest Result:" + JsonUtil.SerializeObject(taskResult));


            Assert.IsTrue(!string.IsNullOrEmpty(taskName));
        }
        [TestMethod()]
        public void FindUnFinishedUploadPackageTask()
        {
            _esSession.Open();
            List<HWESightTask> hwTaskList = new List<HWESightTask>(_esSession.BasePackageWorker.FindUnFinishedUploadPackageTask());
            Assert.IsNotNull(hwTaskList);
            UploadBasePackageJsonTest();
            hwTaskList = new List<HWESightTask>(_esSession.BasePackageWorker.FindUnFinishedUploadPackageTask());
            LogUtil.HWLogger.API.Info("FindUnFinishedTaskToJson:" + JsonUtil.SerializeObject(hwTaskList));
            WebReturnResult<List<HWESightTask>> ret = new WebReturnResult<List<HWESightTask>>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = hwTaskList;
            LogUtil.HWLogger.API.Info("FindUnFinishedTaskToJson:" + JsonUtil.SerializeObject(ret));
            Assert.IsTrue(hwTaskList.Count >= 0);
        }


        [TestMethod()]
        public void QueryBasePackageProcessTest()
        {
            _esSession.Open();
            var packageTask = new { taskName = "API@Task_1456209500919" };
            WebOneESightParam<dynamic> packageParam = new WebOneESightParam<dynamic>();
            packageParam.ESightIP = "127.0.0.1";
            packageParam.Param = packageTask;
            LogUtil.HWLogger.API.Info("QueryBasePackageProcessTest packageParam:" + JsonUtil.SerializeObject(packageParam));

            BasePackageProgress basePackageProcess = _esSession.BasePackageWorker.QueryBasePackageProcess(packageTask.taskName);
            WebReturnResult<BasePackageProgress> ret = new WebReturnResult<BasePackageProgress>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = basePackageProcess;
            LogUtil.HWLogger.API.Info("QueryBasePackageProcessTest basePackageProcess:" + JsonUtil.SerializeObject(ret));
        }
        [TestMethod()]
        [ExpectedException(typeof(BasePackageExpceion))]
        public void QueryBasePackageProcessFailTest()
        {
            var mock = new Mock<IESSession>();
            string taskName = "API@Task_1456209500919";

            IBasePackageWorker worker = new BasePackageWorker();
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_BASEPACKAGE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\" : -4011, \"data\":{ \"taskName\":\"API@Task_1456209500919\"},\"description\" : \"任务正在运行.\"}");



            mock.Setup(foo => foo.HCGet(sb.ToString())).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            //mock.Setup(foo => foo.SoftSourceWorker)).Returns(jResult);
            worker.ESSession = mock.Object;
            BasePackageProgress basePackageProcess = worker.QueryBasePackageProcess(taskName);
        }
        [TestMethod()]
        [ExpectedException(typeof(BasePackageExpceion))]
        public void QueryBasePackageProcessFail2Test()
        {
            _esSession.Open();
            string taskName = "API@Task_1456209500919";

            _esSession.BasePackageWorker.QueryBasePackageProcess("ssss");
        }

        [TestMethod()]
        public void GetTaskStatus1Test()
        {
            BasePackageWorker worker = new BasePackageWorker();
            string status = "";
            status = worker.GetTaskStatus("", "", "Success");
            Assert.AreEqual(status, ConstMgr.HWESightTask.SYNC_STATUS_FINISHED);
            status = worker.GetTaskStatus("", "Failed", "");
            Assert.AreEqual(status, ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED);
            status = worker.GetTaskStatus("", "Running", "");
            Assert.AreEqual(status, ConstMgr.HWESightTask.SYNC_STATUS_CREATED);
            status = worker.GetTaskStatus("test", "", "");
            Assert.AreEqual(status, "test");
        }
        [TestMethod()]
        public void DelTaskTest()
        {
            _esSession.Open();
            _esSession.BasePackageWorker.DeleteTask(0);
        }
        [TestMethod()]
        public void QueryBasePackagePageTest()
        {
            _esSession.Open();
            var queryParam = new { };
            WebOneESightParam<dynamic> packageParam = new WebOneESightParam<dynamic>();
            packageParam.ESightIP = "127.0.0.1";
            packageParam.Param = queryParam;
            LogUtil.HWLogger.API.Info("QueryBasePackagePageTest packageParam:" + JsonUtil.SerializeObject(packageParam));

            QueryLGListResult<BasePackageItem> queryLGListResult = _esSession.BasePackageWorker.QueryBasePackagePage(1, 20);
            WebReturnLGResult<BasePackageItem> ret = new WebReturnLGResult<BasePackageItem>(queryLGListResult);

            LogUtil.HWLogger.API.Info("QueryBasePackagePageTest queryLGListResult:" + JsonUtil.SerializeObject(ret));
        }

        [TestMethod()]
        public void QueryBasePackageDetailTest()
        {
            _esSession.Open();
            var queryParam = new { basepackageName = "basepackage1" };
            WebOneESightParam<dynamic> packageParam = new WebOneESightParam<dynamic>();
            packageParam.ESightIP = "127.0.0.1";
            packageParam.Param = queryParam;
            LogUtil.HWLogger.API.Info("QueryBasePackageDetailTest packageParam:" + JsonUtil.SerializeObject(packageParam));

            QueryObjectResult<BasePackageDetail> queryLGListResult = _esSession.BasePackageWorker.QueryBasePackageDetail(queryParam.basepackageName);
            //QueryObjectResult<BasePackageDetail> ret = new QueryObjectResult<BasePackageDetail>();
            //ret.Code = 0;
            //ret.Description = "";
            // ret.Data = queryLGListResult;

            LogUtil.HWLogger.API.Info("QueryBasePackageDetailTest queryLGListResult:" + JsonUtil.SerializeObject(queryLGListResult));
        }

        [TestMethod()]
        public void AddDeployTaskTest()
        {
            _esSession.Open();
            string basepackageName = "test1";
            DeployPackageTask deployTask = new DeployPackageTask();
            deployTask.BasepackageName = "basepackage1";
            deployTask.DeviceDn = "NE=123;NE=1234";
            deployTask.FirmwareList = "CAN,SSD";

            WebOneESightParam<dynamic> postParam = new WebOneESightParam<dynamic>();

            postParam.ESightIP = "127.0.0.1";
            postParam.Param = deployTask;

            LogUtil.HWLogger.API.Info("AddDeployTaskTest postParam:" + JsonUtil.SerializeObject(postParam));

            string taskName = _esSession.BasePackageWorker.AddDeployTask(deployTask);

            JObject taskObject = new JObject();
            taskObject.Add("taskName", taskName);
            WebReturnResult<JObject> taskResult = new WebReturnResult<JObject>();
            taskResult.Data = taskObject;
            taskResult.Description = "";
            LogUtil.HWLogger.API.Info("AddDeployTaskTest Result:" + JsonUtil.SerializeObject(taskResult));
        }

        [TestMethod()]
        public void QueryDeployTaskProcessTest()
        {
            _esSession.Open();
            var packageTask = new { taskName = "API@Task_1456209500919" };
            WebOneESightParam<dynamic> packageParam = new WebOneESightParam<dynamic>();
            packageParam.ESightIP = "127.0.0.1";
            packageParam.Param = packageTask;
            LogUtil.HWLogger.API.Info("QueryDeployTaskProcessTest packageParam:" + JsonUtil.SerializeObject(packageParam));

            DeployPackageTaskDetail taskDetail = _esSession.BasePackageWorker.QueryDeployTaskProcess(packageTask.taskName);
            WebReturnResult<DeployPackageTaskDetail> ret = new WebReturnResult<DeployPackageTaskDetail>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = taskDetail;
            LogUtil.HWLogger.API.Info("QueryDeployTaskProcessTest basePackageProcess:" + JsonUtil.SerializeObject(ret));
        }

        [TestMethod()]
        public void SyncDeployTaskFromESightTest()
        {
            _esSession.Open();
            _esSession.BasePackageWorker.SyncDeployTaskFromESight();
        }

        [TestMethod()]
        public void FindUnFinishedDeployTaskTest()
        {
            _esSession.Open();
            _esSession.BasePackageWorker.FindUnFinishedDeployTask();
        }

        [TestMethod()]
        public void FindDeployTaskWithSyncTest()
        {
            _esSession.Open();
            AddDeployTaskTest();
            QueryDeployPackageParam queryDeployParam = new QueryDeployPackageParam();
            queryDeployParam.PageNo = 1;
            queryDeployParam.PageSize = 20;
            queryDeployParam.TaskStatus = "";//ConstMgr.HWESightTask.TASK_STATUS_RUNNING;
            queryDeployParam.TaskeName = "t";
            queryDeployParam.Order = "taskName";
            queryDeployParam.OrderDesc = false;
            WebOneESightParam<QueryDeployPackageParam> queryParam = new WebOneESightParam<QueryDeployPackageParam>();
            queryParam.ESightIP = _esSession.HWESightHost.HostIP;
            queryParam.Param = queryDeployParam;
            LogUtil.HWLogger.API.Info("FindDeployTaskWithSyncTest queryParam:" + JsonUtil.SerializeObject(queryParam));

            QueryPageResult<HWESightTask> taskResult = _esSession.BasePackageWorker.FindDeployTaskWithSync(queryDeployParam);

            WebReturnResult<QueryPageResult<HWESightTask>> ret = new WebReturnResult<QueryPageResult<HWESightTask>>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = taskResult;

            LogUtil.HWLogger.API.Info("FindDeployTaskWithSyncTest QueryPageResult:" + JsonUtil.SerializeObject(ret));
        }
        [TestMethod()]
        public void DeleteTaskTest()
        {
            var delTask = new { taskId = 1 };
            WebOneESightParam<dynamic> packageParam = new WebOneESightParam<dynamic>();
            packageParam.ESightIP = "127.0.0.1";
            packageParam.Param = delTask;
            LogUtil.HWLogger.API.Info("DeleteTaskTest packageParam:" + JsonUtil.SerializeObject(packageParam));

            WebReturnResult<int> ret = new WebReturnResult<int>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = _esSession.BasePackageWorker.DeleteTask(delTask.taskId);

            LogUtil.HWLogger.API.Info("DeleteTaskTest QueryPageResult:" + JsonUtil.SerializeObject(ret));
        }

        [TestMethod()]
        public void ClearFailedTask()
        {
            var delTask = new { };
            LogUtil.HWLogger.API.Info("ClearFailedTask packageParam:" + JsonUtil.SerializeObject(delTask));

            WebReturnResult<int> ret = new WebReturnResult<int>();
            ret.Code = 0;
            ret.Description = "";
            ret.Data = ESightEngine.Instance.ClearAllFailedPackageTask();

            LogUtil.HWLogger.API.Info("ClearFailedTask QueryPageResult:" + JsonUtil.SerializeObject(ret));
        }

        [TestMethod()]
        public void QueryDeployTaskDNProcessTest()
        {
            _esSession.Open();
            string taskName = "API@FirmWareTask_1503898934595";
            string dn = "NE=34603009";
            var firmwareTaskDetail = _esSession.BasePackageWorker.QueryDeployTaskDNProcess(taskName, dn);
            var ret = new WebReturnResult<BasePackageDNProgress>();
            ret.Code        = firmwareTaskDetail.Code;
            ret.Data        = firmwareTaskDetail.Data;
            ret.Description = firmwareTaskDetail.Description;
            LogUtil.HWLogger.API.InfoFormat("QueryDeployTaskDNProcess QueryPageResult: {0}", JsonUtil.SerializeObject(ret));
            Assert.IsNotNull(ret.Data);
        }
    }
}