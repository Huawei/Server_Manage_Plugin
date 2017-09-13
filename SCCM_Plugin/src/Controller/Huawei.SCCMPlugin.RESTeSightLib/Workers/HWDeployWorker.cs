using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Deploy;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.DAO;
using Huawei.SCCMPlugin.Models.Softwares;
using CommonUtil;
using System.Web;
using Newtonsoft.Json;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers
{
    /// <summary>
    /// 模板部署业务类。
    /// </summary>
    public class HWDeployWorker : IHWDeployWorker
    {
        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        public IESSession ESSession
        {
            get; set;
        }
        string errorPix = "deploy.error.";
        /// <summary>
        /// 检测是否eSight返回错误。
        /// </summary>
        /// <param name="jsonData">eSight返回的Json对象</param>
        /// <param name="errorTip">错误标题头</param>
        private void CheckAndThrowException(JObject jsonData, string errorTip = "")
        {
            int retCode = 0;
            if (JsonUtil.GetJObjectPropVal<string>(jsonData, "code") == "0")
                retCode = 0;
            else
            {
                retCode = JsonUtil.GetJObjectPropVal<int>(jsonData, "code");
            }
            if (retCode != 0)
            {
                throw GetDeployException(retCode.ToString(), this, errorTip + JsonUtil.GetJObjectPropVal<string>(jsonData, "description"));
            }
        }
        private Exception GetDeployException(string code, IHWDeployWorker hwDeployWorker, string message)
        {
            return new DeployException(errorPix, code, hwDeployWorker, message);
        }
        /// <summary>
        /// 模板创建
        /// </summary>
        /// <param name="deployTemplate">模板参数</param>
        public void AddDeployTemplate(DeployTemplate deployTemplate)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DEPLOY_TEMPLATE);
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("templateName", deployTemplate.TemplateName));
            parameters.Add(new KeyValuePair<string, object>("templateType", deployTemplate.TemplateType));
            parameters.Add(new KeyValuePair<string, object>("templateDesc", deployTemplate.TemplateDesc));
            parameters.Add(new KeyValuePair<string, object>("templateProp", JsonUtil.SerializeObject(deployTemplate.TemplateProp)));
            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);
            CheckAndThrowException(jResult);
        }
        /// <summary>
        /// 查看模板详情
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns>模板详情</returns>
        public DeployTemplate QueryDeployTemplate(string templateName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DETAIL_TEMPLATE);
            sb.Append("?templateName=").Append(HttpUtility.UrlEncode(templateName, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryObjectResult<DeployTemplate> queryObjectResult = jResult.ToObject<QueryObjectResult<DeployTemplate>>(new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            return queryObjectResult.Data;
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateName">模板名称</param>
        public void DelDeployTemplate(string templateName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DELETE_DEPLOYTEMPLATE);
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("templateName", templateName));
            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);//不知道为什么华为这里提供的是POST接口。
            CheckAndThrowException(jResult);
        }
        /// <summary>
        /// 查询模板。
        /// deployTemplate.TemplateProp为null.
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="templateType">模板类型，可选，默认为ALL</param>
        /// <returns>模板分页对象</returns>
        public QueryLGListResult<DeployTemplate> QueryTemplatePage(int pageNo = -1, int pageSize = int.MaxValue, string templateType = "")
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PAGE_DEPLOY_TEMPLATE);
            StringBuilder paramBuild = new StringBuilder();
            if (pageNo != -1)
            {
                if (paramBuild.Length > 0) paramBuild.Append("&");
                paramBuild.Append("pageNo=").Append(pageNo);
            }
            if (pageSize != -1)
            {
                if (paramBuild.Length > 0) paramBuild.Append("&");
                paramBuild.Append("pageSize=").Append(pageSize);
            }
            if (!string.IsNullOrEmpty(templateType))
            {
                if (paramBuild.Length > 0) paramBuild.Append("&");
                paramBuild.Append("templateType=").Append(templateType);
            }
            if (paramBuild.Length > 0)
            {
                sb.Append("?").Append(paramBuild.ToString());
            }

            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryLGListResult<DeployTemplate> queryLGListResult = jResult.ToObject<QueryLGListResult<DeployTemplate>>();
            return queryLGListResult;
        }

        /// <summary>
        /// 添加部署任务
        /// </summary>
        /// <param name="taskSourceName">任务名，仅仅用做备注任务</param>
        /// <param name="deployTask">部署任务信息，包括模板类型和设备DN</param>
        /// <returns>返回eSight任务名</returns>
        public string AddDeployTask(string taskSourceName, DeployTask deployTask)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_TASK_DEPLOY);
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("templates", deployTask.Templates));
            parameters.Add(new KeyValuePair<string, object>("dn", deployTask.DeviceDn));

            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);
            CheckAndThrowException(jResult);
            HWESightTask eSightTask = new HWESightTask();
            QueryObjectResult<JObject> queryObjectResult = jResult.ToObject<QueryObjectResult<JObject>>();
            eSightTask.HWESightHostID = this.ESSession.HWESightHost.ID;
            eSightTask.TaskName = JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");
            eSightTask.SoftWareSourceName = taskSourceName;
            eSightTask.TaskStatus = ConstMgr.HWESightTask.TASK_STATUS_RUNNING;//初始化。
            eSightTask.TaskProgress = 0;
            eSightTask.TaskResult = "";
            eSightTask.ErrorDetail = "";
            eSightTask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
            eSightTask.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOY;
            eSightTask.LastModifyTime = System.DateTime.Now;
            eSightTask.CreateTime = System.DateTime.Now;
            int taskId = HWESightTaskDal.Instance.InsertEntity(eSightTask);
            eSightTask.ID = taskId;
            InsertHWTaskResourceList(eSightTask, deployTask.DeviceDn);

            LogUtil.HWLogger.API.InfoFormat("AddDeployTask：{0}", taskId);
            return JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");

        }
        /// <summary>
        /// 获得部署任务分页数据并返回列表。
        /// </summary>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        public QueryPageResult<HWESightTask> FindDeployTaskWithSync(QueryDeployParam queryDeployParam)
        {
            SyncTaskFromESight();
            //Search Again.
            QueryPageResult<HWESightTask> hwTaskPage = HWESightTaskDal.Instance.FindDeployTask(this.ESSession.HWESightHost.ID, queryDeployParam);
            hwTaskPage = FillDeviceDetails(hwTaskPage);
            return hwTaskPage;
        }
        /// <summary>
        /// 填充分页查询获得的任务里设备明细
        /// </summary>
        /// <param name="hwTaskPage">传入分页对象</param>
        /// <returns>填充好设备明细的任务分页</returns>
        private QueryPageResult<HWESightTask> FillDeviceDetails(QueryPageResult<HWESightTask> hwTaskPage)
        {
            //生成任务ID组成的字符串。方便后面sql一次查询
            //因为只是查询分页内的数据，所以数据不会很多。
            StringBuilder sb = new StringBuilder("-9999");
            foreach (HWESightTask hwESightTask in hwTaskPage.Data)
            {
                sb.Append(",").Append(hwESightTask.ID);
            }
            //获得任务列表
            IList<HWTaskResource> resourceList = HWTaskResourceDal.Instance.FindTaskResourceByTaskIds(sb.ToString());
            //填充资源数据
            foreach (HWESightTask hwESightTask in hwTaskPage.Data)
            {
                foreach (HWTaskResource hwTaskResource in resourceList)
                {
                    if (hwTaskResource.HWESightTaskID == hwESightTask.ID)
                    {
                        if (!string.IsNullOrEmpty(hwTaskResource.ErrorCode) && !string.Equals(hwTaskResource.ErrorCode, "0"))
                            hwTaskResource.ErrorCode = errorPix + hwTaskResource.ErrorCode;
                        hwESightTask.DeviceDetails.Add(hwTaskResource);
                    }
                }
            }
            return hwTaskPage;
        }
        /// <summary>
        /// 查找所有未完成任务。
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns>任务列表</returns>
        public IList<HWESightTask> FindUnFinishedTask()
        {
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindUnFinishedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_DEPLOY);
            return hwTaskList;
        }
        /// <summary>
        /// 根据eSight返回的状态，返回更新到数据库的状态。
        /// </summary>
        /// <param name="oldStatus">旧的数据库状态</param>
        /// <param name="taskStatus">eSight返回的taskStatus</param>
        /// <param name="taskResult">eSight返回的taskResult</param>
        /// <param name="taskCode">eSight返回的taskCode</param>
        /// <returns>判断后的任务状态</returns>
        private string GetTaskStatus(string oldStatus, string taskStatus, string taskResult, string taskCode)
        {
            if (taskResult == "Failed") return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;//失败优先返回
            if (string.Equals(taskResult, "Partion Success", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_HW_PFAILED;
            //部分成功
            //返回非0，默认返回错误。
            if (CoreUtil.GetObjTranNull<int>(taskCode) != 0) return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;
            //成功，返回finishe.
            if (taskResult == "Success") return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            //完成，返回finishe.
            if (taskStatus == "Complete") return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            //正在运行
            if (taskStatus == "Running") return ConstMgr.HWESightTask.SYNC_STATUS_CREATED;

            return oldStatus;
        }
        /// <summary>
        /// 同步任务状态到数据库。
        /// </summary>
        /// <param name="queryObjectResult">查询结果</param>
        /// <param name="taskName">任务名</param>
        private void SaveTaskProgressToDB(QueryObjectResult<DeployProgress> queryObjectResult, string taskName)
        {
            //sync to the database.
            DeployProgress deployProgress = queryObjectResult.Data;
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            hwtask.TaskStatus = deployProgress.TaskStatus;

            hwtask.TaskResult = deployProgress.TaskResult;
            hwtask.TaskCode =(!string.IsNullOrEmpty(deployProgress.TaskCode) && !string.Equals(deployProgress.TaskCode, "0")) ?
                (errorPix + deployProgress.TaskCode) : deployProgress.TaskCode;
            hwtask.ErrorDetail = deployProgress.ErrorDetail;

            hwtask.LastModifyTime = System.DateTime.Now;
            //find child device in database.
            int progress = UpdateHWTaskResourceList(hwtask, deployProgress);
            if (progress > 0)
                hwtask.TaskProgress = progress;// deployProgress.TaskProgress;
            else
                hwtask.TaskProgress = deployProgress.TaskProgress;
            if (progress == 100) hwtask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;

            hwtask.SyncStatus = GetTaskStatus(hwtask.SyncStatus, deployProgress.TaskStatus, deployProgress.TaskResult, deployProgress.TaskCode);
            HWESightTaskDal.Instance.UpdateEntity(hwtask);

        }
        /// <summary>
        /// 插入数据到任务资源表
        /// </summary>
        /// <param name="hwtask">任务对象</param>
        /// <param name="dnString">dn</param>
        private void InsertHWTaskResourceList(HWESightTask hwtask, string dnString)
        {
            string[] dns = dnString.Split(new char[] { ';' });
            foreach (string dn in dns)
            {
                HWTaskResource hwResource = new HWTaskResource();
                hwResource.HWESightTaskID = hwtask.ID;
                hwResource.DN = dn;
                hwResource.DeviceResult = "";
                hwResource.DeviceProgress = 0;
                hwResource.ErrorDetail = "";
                hwResource.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOY;
                hwResource.SyncStatus = hwtask.SyncStatus;
                hwResource.LastModifyTime = System.DateTime.Now;
                hwResource.CreateTime = System.DateTime.Now;
                HWTaskResourceDal.Instance.InsertEntity(hwResource);
            }
        }
        /// <summary>
        /// 更新任务资源表。 更新设备明细到数据库
        /// </summary>
        /// <param name="hwtask">模板部署任务</param>
        /// <param name="deployProgress">部署任务明细</param>
        /// <returns>完成的设备数量</returns>
        private int UpdateHWTaskResourceList(HWESightTask hwtask, DeployProgress deployProgress)
        {
            bool isFinsihed = false;
            int finishedCnt = 0;
            IList<HWTaskResource> resourceList = HWTaskResourceDal.Instance.FindTaskResourceByTaskId(hwtask.ID);
            Dictionary<string, DeviceProgress> dviDict = new Dictionary<string, DeviceProgress>();
            if (deployProgress.DeviceDetails == null) return 0;

            DeviceProgress emptyDevicProcess = null;
            foreach (DeviceProgress deviceProgress in deployProgress.DeviceDetails)
            {
                if (string.IsNullOrEmpty(deviceProgress.DeviceDn))//如果dn为空，设备关闭或者删除的时候。
                    emptyDevicProcess = deviceProgress;
                else
                    dviDict[deviceProgress.DeviceDn.ToUpper()] = deviceProgress;
            }

            foreach (HWTaskResource hwResource in resourceList)
            {
                DeviceProgress deviceProgress = null;
                if (dviDict.ContainsKey(hwResource.DN.ToUpper()))
                {
                    deviceProgress = dviDict[hwResource.DN.ToUpper()];

                    hwResource.DeviceResult = deviceProgress.DeviceResult;
                    hwResource.DeviceProgress = deviceProgress.Progress;
                    hwResource.ErrorCode = deviceProgress.ErrorCode;
                    hwResource.ErrorDetail = deviceProgress.ErrorDetail;

                    hwResource.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOY;
                    hwResource.SyncStatus = hwtask.SyncStatus;
                    hwResource.LastModifyTime = System.DateTime.Now;
                    if (deviceProgress.Progress == 100) finishedCnt++;

                }
                else
                {
                    if (emptyDevicProcess != null)
                    {
                        LogUtil.HWLogger.API.WarnFormat("System can't find this dn=[{0}], using the none dn message.", hwResource.DN);
                        hwResource.DeviceResult = emptyDevicProcess.DeviceResult;
                        hwResource.DeviceProgress = emptyDevicProcess.Progress;
                        hwResource.ErrorCode = emptyDevicProcess.ErrorCode;
                        hwResource.ErrorDetail = emptyDevicProcess.ErrorDetail;

                        hwResource.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOY;
                        hwResource.SyncStatus = hwtask.SyncStatus;
                        hwResource.LastModifyTime = System.DateTime.Now;
                        if (emptyDevicProcess.Progress == 100) finishedCnt++;
                    }
                    else
                    {
                        LogUtil.HWLogger.API.WarnFormat("System can't find this dn=[{0}]", hwResource.DN);
                    }
                }
                HWTaskResourceDal.Instance.UpdateEntity(hwResource);

            }
            if (resourceList.Count > 0)
                return (finishedCnt * 100) / resourceList.Count;
            else
                return 0;
        }
        /// <summary>
        /// 查询部署任务状态，从eSight
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <returns>部署任务明细对象</returns>
        private DeployProgress QueryDeployProcess(string taskName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_DEPLOY);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryObjectResult<DeployProgress> queryObjectResult = jResult.ToObject<QueryObjectResult<DeployProgress>>(new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            //sync to the database.
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            if (hwtask == null)//在数据库中没有找到，抛出错误。
            {
                throw new DeployException(ConstMgr.ErrorCode.DB_NOTFOUND, this, string.Format("Query the deployWork failed:System can not find this task in db.[{0}]", taskName));
            }
            else
            {
                SaveTaskProgressToDB(queryObjectResult, taskName);
            }
            return queryObjectResult.Data;
        }
        /// <summary>
        /// 从eSight同步所有任务。
        /// </summary>
        public void SyncTaskFromESight()
        {
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindCreatedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_DEPLOY);
            //Exception lastException = null;//修改为不抛出错误。
            foreach (HWESightTask hwTask in hwTaskList)
            {
                try
                {
                    QueryDeployProcess(hwTask.TaskName);
                }
                catch (Exception se)
                {
                    LogUtil.HWLogger.API.Error(se);
                    //lastException = se;//修改为不抛出错误。
                }
            }
            //if (lastException != null) throw lastException;//修改为不抛出错误。
        }
        /// <summary>
        /// 根据任务id,删除任务
        /// </summary>
        /// <param name="taskId">根据任务id</param>
        /// <returns>删除的任务数</returns>
        public int DeleteTask(int taskId)
        {
            return HWESightTaskDal.Instance.DeleteEntityById(taskId);
        }
    }
}
