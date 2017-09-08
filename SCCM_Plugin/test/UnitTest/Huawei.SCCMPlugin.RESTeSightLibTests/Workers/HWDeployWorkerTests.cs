using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models.Deploy;
using Newtonsoft.Json;
using CommonUtil;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers.Tests
{
    [TestClass()]
    public class HWDeployWorkerTests
    {
        static HWESightHost _hwESightHost = null;
        static IESSession _esSession = null;

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
        public void QueryTemplatePageTest()
        {
            _esSession.Open();
            WebOneESightParam<Object> webQueryParam = new WebOneESightParam<Object>();
            webQueryParam.ESightIP = "127.0.0.1";
            webQueryParam.Param = null;
            LogUtil.HWLogger.API.Info("QueryTemplatePageToJsonTest Param:" + JsonUtil.SerializeObject(webQueryParam));


            QueryLGListResult<DeployTemplate> templatePage = _esSession.DeployWorker.QueryTemplatePage(1, 20, "OS");
            LogUtil.HWLogger.API.Info("template list:" + JsonUtil.SerializeObject(templatePage));
            Assert.IsNotNull(templatePage);
        }
        /*
         * 

        [TestMethod()]
        public void AddDeployTemplateTest()
        {
          Assert.Fail();
        }
        */
        [TestMethod()]
        public void AddPowerDeployTemplateTest()
        {
            _esSession.Open();
            JObject jObject = JsonUtil.DeserializeObject<JObject>(@"{ 
          ""powerPolicy"":""1""         // 必选，电源策略，可选值0/1/2 
        } 
        ");
            DeployTemplate deployTemplate = new DeployTemplate();
            deployTemplate.TemplateName = "上电模板";
            deployTemplate.TemplateType = "POWER";
            deployTemplate.TemplateDesc = "this is a power on template";
            deployTemplate.TemplateProp = jObject;
            WebMutilESightsParam<DeployTemplate> webPostParam = new WebMutilESightsParam<DeployTemplate>();
            webPostParam.ESights = new List<string>() { "127.0.0.1", "192.168.1.1" };
            webPostParam.Data = deployTemplate;
            LogUtil.HWLogger.API.Info("AddDeployTemplateTest Param:" + JsonUtil.SerializeObject(webPostParam));
            _esSession.DeployWorker.AddDeployTemplate(webPostParam.Data);
        }

        [TestMethod()]
        public void DelDeployTemplateTest()
        {
            _esSession.Open();
            WebOneESightParam<string> postParam = new WebOneESightParam<string>();
            postParam.ESightIP = "127.0.0.1";
            postParam.Param = "template1";
            LogUtil.HWLogger.API.Info("DelDeployTemplateTest Param:" + JsonUtil.SerializeObject(postParam));

            _esSession.DeployWorker.DelDeployTemplate(postParam.Param);

        }


        [TestMethod()]
        public void AddDeployTaskTest()
        {
            _esSession.Open();
            string basepackageName = "test1";
            DeployTask deployTask = new DeployTask();
            deployTask.DeviceDn = "NE=123;NE=1234";
            deployTask.Templates = "template1,poweron1";
            var newSource = new { deployTaskName = basepackageName, deviceDn = deployTask.DeviceDn, templates = deployTask.Templates };
            WebOneESightParam<dynamic> postParam = new WebOneESightParam<dynamic>();

            postParam.ESightIP = "127.0.0.1";
            postParam.Param = newSource;

            LogUtil.HWLogger.API.Info("AddDeployTaskTest postParam:" + JsonUtil.SerializeObject(postParam));

            string taskName = _esSession.DeployWorker.AddDeployTask(basepackageName, deployTask);
            JObject taskObject = new JObject();
            taskObject.Add("taskName", taskName);
            WebReturnResult<JObject> taskResult = new WebReturnResult<JObject>();
            taskResult.Data = taskObject;
            taskResult.Description = "";
            LogUtil.HWLogger.API.Info("AddDeployTaskTest Result:" + JsonUtil.SerializeObject(taskResult));
        }

        [TestMethod()]
        public void FindDeployTaskWithSyncTest()
        {
            _esSession.Open();
            AddDeployTaskTest();
            QueryDeployParam queryDeployParam = new QueryDeployParam();
            queryDeployParam.PageNo = 1;
            queryDeployParam.PageSize = 20;
            queryDeployParam.TaskStatus = "";//ConstMgr.HWESightTask.TASK_STATUS_RUNNING;
            queryDeployParam.TaskSourceName = "t";
            queryDeployParam.Order = "taskName";
            queryDeployParam.OrderDesc = true;
            WebOneESightParam<QueryDeployParam> queryParam = new WebOneESightParam<QueryDeployParam>();
            queryParam.ESightIP = _esSession.HWESightHost.HostIP;
            queryParam.Param = queryDeployParam;
            LogUtil.HWLogger.API.Info("FindDeployTaskWithSyncTest queryParam:" + JsonUtil.SerializeObject(queryParam));

            QueryPageResult<HWESightTask> taskResult = _esSession.DeployWorker.FindDeployTaskWithSync(queryDeployParam);
            LogUtil.HWLogger.API.Info("FindDeployTaskWithSyncTest QueryPageResult:" + JsonUtil.SerializeObject(taskResult));
        }

        [TestMethod()]
        public void FindUnFinishedTaskTest()
        {
            _esSession.Open();
            _esSession.DeployWorker.FindUnFinishedTask();
        }



        [TestMethod()]
        public void SyncTaskFromESightTest()
        {
            _esSession.Open();
            _esSession.DeployWorker.SyncTaskFromESight();
        }

        [TestMethod()]
        public void DeleteTaskTest()
        {
            _esSession.DeployWorker.DeleteTask(-1);
        }
    }
}