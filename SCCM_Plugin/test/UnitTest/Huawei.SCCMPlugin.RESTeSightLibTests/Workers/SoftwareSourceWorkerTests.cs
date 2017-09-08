using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Softwares;
using Newtonsoft.Json;
using Huawei.SCCMPlugin.PluginUI.Entitys;
using CommonUtil;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Moq;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System.Web;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers.Tests
{
    [TestClass()]
    public class SoftwareSourceWorkerTests
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
        [ExpectedException(typeof(SoftwareSourceExpceion), "任务正在运行.")]
        public void TestFailToUploadSoft()
        {
            var mock = new Mock<IESSession>();
            SoftwareSource softwareSource = new SoftwareSource();
            softwareSource.SoftwareName = "OS_Software1";
            softwareSource.SoftwareDescription = "“This is a OS template.";
            softwareSource.SoftwareType = "Windows";
            softwareSource.SoftwareVersion = "Windows Server 2008 R2 x64";
            softwareSource.SoftwareLanguage = "Chinese";
            softwareSource.SourceName = "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso";
            softwareSource.SftpserverIP = "188.10.18.188";
            softwareSource.SftpUserName = "itSftpUser";
            softwareSource.SftpPassword = "Huawei@123";

            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_UPLOADE_SOFTWARESOURCE);
            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\" : -4011, \"data\":{ \"taskName\":\"API@Task_1456209500919\"},\"description\" : \"任务正在运行.\"}");
            ISoftwareSourceWorker worker = new SoftwareSourceWorker();

            mock.Setup(foo => foo.HCPost(sb.ToString(), softwareSource)).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            //mock.Setup(foo => foo.SoftSourceWorker)).Returns(jResult);
            worker.ESSession = mock.Object;
            worker.UploadSoftwareSource(softwareSource);
        }
        [TestMethod()]
        [ExpectedException(typeof(SoftwareSourceExpceion), "任务正在运行.")]
        public void TestFailToSyncTaskProgress() {
            var mock = new Mock<IESSession>();
            string taskName = "API@Task_1456209500919";
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_SOFTWARESOURCE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\":1,\"data\":{\"taskName\":\"API@Task_1456209500919\","+
                "\"softwaresourceName\":\"OS_Software1\",\"taskStatus\":\"Complete\",\"taskProgress\":\"100\",\"taskResult\":\"Success\","+
                "\"taskCode\":0,\"errorDetail\":\"软件源上传失败，传输中断\"},\"description\":\"server error.\"}");
            ISoftwareSourceWorker worker = new SoftwareSourceWorker();
            
            mock.Setup(foo => foo.HCGet(sb.ToString())).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            //mock.Setup(foo => foo.SoftSourceWorker)).Returns(jResult);
            worker.ESSession = mock.Object;
            worker.QuerySoftwareProcess(taskName);
        }
        [TestMethod()]
        public void TestFailToSyncTaskFromESights()
        {
            var mock = new Mock<IESSession>();
            string taskName = "API@Task_1456209500919";
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_SOFTWARESOURCE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\":1,\"data\":{\"taskName\":\"API@Task_1456209500919\"," +
                "\"softwaresourceName\":\"OS_Software1\",\"taskStatus\":\"Complete\",\"taskProgress\":\"100\",\"taskResult\":\"Success\"," +
                "\"taskCode\":0,\"errorDetail\":\"软件源上传失败，传输中断\"},\"description\":\"server error.\"}");
            ISoftwareSourceWorker worker = new SoftwareSourceWorker();
            LogUtil.HWLogger.API.InfoFormat("TestFailToSyncTaskFromESights started..");
            mock.Setup(foo => foo.HCGet(sb.ToString())).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            //mock.Setup(foo => foo.SoftSourceWorker)).Returns(jResult);
            worker.ESSession = mock.Object;
            UploadSoftwareSourceTest();
            worker.SyncTaskFromESight();
            LogUtil.HWLogger.API.InfoFormat("TestFailToSyncTaskFromESights finished..");
        }
        [TestMethod()]
        public void UploadSoftwareSourceTest()
        {
            _esSession.Open();
            SoftwareSource softwareSource = new SoftwareSource();
            softwareSource.SoftwareName = "OS_Software1";
            softwareSource.SoftwareDescription = "“This is a OS template.";
            softwareSource.SoftwareType = "Windows";
            softwareSource.SoftwareVersion = "Windows Server 2008 R2 x64";
            softwareSource.SoftwareLanguage = "Chinese";
            softwareSource.SourceName = "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso";
            softwareSource.SftpserverIP = "188.10.18.188";
            softwareSource.SftpUserName = "itSftpUser";
            softwareSource.SftpPassword = "Huawei@123";
            string taskName = _esSession.SoftSourceWorker.UploadSoftwareSource(softwareSource);
            Console.WriteLine(taskName);
            Assert.IsTrue(!string.IsNullOrEmpty(taskName));
        }
        [TestMethod()]
        public void UploadSoftwareSourceTest2()
        {
            Random random = new Random();
            
            _esSession.Open();
            SoftwareSource softwareSource = new SoftwareSource();
            softwareSource.SoftwareName = "OS_Software3331"+ random.Next();
            softwareSource.SoftwareDescription = "“This is a OS template.";
            softwareSource.SoftwareType = "Windows";
            softwareSource.SoftwareVersion = "Windows Server 2008 R2 x64";
            softwareSource.SoftwareLanguage = "Chinese";
            softwareSource.SourceName = "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso";
            softwareSource.SftpserverIP = "188.10.18.188";
            softwareSource.SftpUserName = "itSftpUser";
            softwareSource.SftpPassword = "Huawei@123";
            string taskName = _esSession.SoftSourceWorker.UploadSoftwareSource(softwareSource);
            Console.WriteLine(taskName);
            Assert.IsTrue(!string.IsNullOrEmpty(taskName));
        }
        [TestMethod()]
        public void UploadSoftwareSourceTest1()
        {
            _esSession.Open();
            SoftwareSource softwareSource = new SoftwareSource();
            softwareSource.SoftwareName = "OS_Software1111";
            softwareSource.SoftwareDescription = "“This is a OS template.";
            softwareSource.SoftwareType = "Windows";
            softwareSource.SoftwareVersion = "Windows Server 2008 R2 x64";
            softwareSource.SoftwareLanguage = "Chinese";
            softwareSource.SourceName = "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso";
            softwareSource.SftpserverIP = "188.10.18.188";
            softwareSource.SftpUserName = "itSftpUser";
            softwareSource.SftpPassword = "Huawei@123";
            QueryObjectResult < JObject > sourceResult = _esSession.SoftSourceWorker.UploadSoftwareSourceWithResult(softwareSource);
            Assert.IsNotNull(sourceResult);
        }
        [TestMethod()]
        public void UploadSoftwareSourceJsonTest()
        {
            _esSession.Open();
            SoftwareSource softwareSource = new SoftwareSource();
            softwareSource.SoftwareName = "OS_Software1";
            softwareSource.SoftwareDescription = "“This is a OS template.";
            softwareSource.SoftwareType = "Windows";
            softwareSource.SoftwareVersion = "Windows Server 2008 R2 x64";
            softwareSource.SoftwareLanguage = "Chinese";
            softwareSource.SourceName = "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso";
            softwareSource.SftpserverIP = "188.10.18.188";
            softwareSource.SftpUserName = "itSftpUser";
            softwareSource.SftpPassword = "Huawei@123";
            string taskName = _esSession.SoftSourceWorker.UploadSoftwareSource(softwareSource);
            WebMutilESightsParam<SoftwareSource> postESightParam = new WebMutilESightsParam<SoftwareSource>();
            postESightParam.ESights = new List<string>() { "127.0.0.1" };
            postESightParam.Data = softwareSource;
            Console.WriteLine(taskName);

            LogUtil.HWLogger.API.Info("UploadSoftwareSourceJsonTest Params:" + JsonUtil.SerializeObject(postESightParam));
            JObject taskObject = new JObject();
            taskObject.Add("taskName", taskName);
            QueryObjectResult<JObject> taskResult = new QueryObjectResult<JObject>();
            taskResult.Data = taskObject;

            LogUtil.HWLogger.API.Info("UploadSoftwareSourceJsonTest Result:" + JsonUtil.SerializeObject(taskResult));

            Assert.IsTrue(!string.IsNullOrEmpty(taskName));
        }

        [TestMethod()]
        public void QuerySoftwareProcessTest()
        {
            _esSession.Open();
            SourceProgress sourceProgress = _esSession.SoftSourceWorker.QuerySoftwareProcess("API@Task_1456209500919");
            Assert.IsNotNull(sourceProgress);
        }
        //[TestMethod()]
        //public void UploadSoftwareSourceWithResultTest()
        //{
        //    _esSession.Open();
        //    SourceProgress sourceProgress = _esSession.SoftSourceWorker.QuerySoftwareProcess("API@Task_145620950091955555");
        //    Assert.IsNotNull(sourceProgress);
        //}

        [TestMethod()]
        public void DeleteSoftwareSourceTest()
        {
            _esSession.Open();
            _esSession.SoftSourceWorker.DeleteSoftwareSource("softwaresource1");
        }
        [TestMethod()]
        public void DeleteSoftwareSourceJsonTest()
        {
            _esSession.Open();
            string[] softwarenames = new string[] { "softwaresource1", "softwaresource2" };
            WebOneESightParam<string> uiPostParam = new WebOneESightParam<string>();
            uiPostParam.ESightIP = "127.0.0.1";
            uiPostParam.Param = "softwaresource1";
            LogUtil.HWLogger.API.InfoFormat("DeleteSoftwareSourceTest postParams:{0}",JsonConvert.SerializeObject(uiPostParam));
            _esSession.SoftSourceWorker.DeleteSoftwareSource("softwaresource1");

            //WebReturnESightResult<string>
            //LogUtil.HWLogger.API.InfoFormat("DeleteSoftwareSourceTest postParams:{0}", JsonConvert.SerializeObject(uiPostParam));

        }
        [TestMethod()]
        public void DeleteSoftwareSourceTestWithMatch()
        {
            _esSession.Open();
            UploadSoftwareSourceTest();
            _esSession.SoftSourceWorker.DeleteSoftwareSource("OS_Software1");
        }
        [TestMethod()]
        [ExpectedException(typeof(SoftwareSourceExpceion), "Delete softwaresource failed(test).")]
        public void DeleteSoftwareSourcFailedTest()
        {
            var mock = new Mock<IESSession>();
            string softwaresourceName = "OS_Software1";
            
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DELETE_SOFTWARESOURCE);
            //sb.Append("?softwareName=").Append(HttpUtility.UrlEncode(softwaresourceName, Encoding.UTF8));

            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\":-777, \"description\":\"Delete softwaresource failed(test).\"}");
            ISoftwareSourceWorker worker = new SoftwareSourceWorker();
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("softwareName", softwaresourceName));

            mock.Setup(foo => foo.HCPostForm(sb.ToString(), parameters)).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            //mock.Setup(foo => foo.SoftSourceWorker)).Returns(jResult);
            worker.ESSession = mock.Object;
            worker.DeleteSoftwareSource(softwaresourceName);
        }
        [TestMethod()]
        [ExpectedException(typeof(SoftwareSourceExpceion), "find softwaresource list failed(test)")]
        public void FailedQuerySoftwarePageTest()
        {
            var mock = new Mock<IESSession>();

            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PAGE_SOFTWARESOURCE);

            JObject jResult = JsonUtil.DeserializeObject<JObject>("{\"code\":1,\"totalNum\":3,\"data\":[],\"description\":\"find softwaresource list failed(test)\"}");
            ISoftwareSourceWorker worker = new SoftwareSourceWorker();

            mock.Setup(foo => foo.HCGet(sb.ToString())).Returns(jResult);
            mock.Setup(foo => foo.HWESightHost).Returns(_hwESightHost);
            worker.ESSession = mock.Object;
            QueryLGListResult<SourceItem> queryLgListResult = worker.QuerySoftwarePage();
            Assert.IsNotNull(queryLgListResult);
        }
        [TestMethod()]
        public void QuerySoftwarePageTest()
        {
            _esSession.Open();
            QueryLGListResult<SourceItem> queryLgListResult = _esSession.SoftSourceWorker.QuerySoftwarePage();
            Assert.IsNotNull(queryLgListResult);
        }
        [TestMethod()]
        public void FailSyncTaskFromESightTest()
        {
            _esSession.Open();
            QueryLGListResult<SourceItem> queryLgListResult = _esSession.SoftSourceWorker.QuerySoftwarePage();
            Assert.IsNotNull(queryLgListResult);
        }
        

        [TestMethod()]
        public void QuerySoftwarePageToJsonTest()
        {
            _esSession.Open();
            QueryLGListResult<SourceItem> queryLgListResult = _esSession.SoftSourceWorker.QuerySoftwarePage(1, 20);
            WebReturnLGResult<SourceItem> retResult = new WebReturnLGResult<SourceItem>(queryLgListResult);
            LogUtil.HWLogger.API.Info("QuerySoftwarePageToJsonTest:" + JsonUtil.SerializeObject(retResult));
            Assert.IsNotNull(queryLgListResult);
        }
        [TestMethod()]
        public void FindSoftwareTaskWithSyncTest()
        {
            _esSession.Open();
            IList<HWESightTask> hwTaskList = _esSession.SoftSourceWorker.FindSoftwareTaskWithSync();
            Assert.IsNotNull(hwTaskList);
            UploadSoftwareSourceTest();
            hwTaskList = _esSession.SoftSourceWorker.FindSoftwareTaskWithSync();
            Assert.IsTrue(hwTaskList.Count > 0);
        }

        [TestMethod()]
        public void FindUnFinishedTask()
        {
            _esSession.Open();
            IList<HWESightTask> hwTaskList = _esSession.SoftSourceWorker.FindSoftwareTaskWithSync();
            Assert.IsNotNull(hwTaskList);
            UploadSoftwareSourceTest();
            hwTaskList = _esSession.SoftSourceWorker.FindUnFinishedTask();
            Assert.IsTrue(hwTaskList.Count >= 0);
        }
        [TestMethod()]
        public void FindUnFinishedTaskToJson()
        {
            _esSession.Open();
            IList<HWESightTask> hwTaskList = _esSession.SoftSourceWorker.FindSoftwareTaskWithSync();
            Assert.IsNotNull(hwTaskList);
            UploadSoftwareSourceTest();
            hwTaskList = _esSession.SoftSourceWorker.FindUnFinishedTask();
            LogUtil.HWLogger.API.Info("FindUnFinishedTaskToJson:" + JsonUtil.SerializeObject(hwTaskList));
            Assert.IsTrue(hwTaskList.Count >= 0);
        }

        [TestMethod()]
        public void SyncTaskFromESightTest()
        {
            _esSession.Open();
            _esSession.SoftSourceWorker.SyncTaskFromESight();
        }
    }
}