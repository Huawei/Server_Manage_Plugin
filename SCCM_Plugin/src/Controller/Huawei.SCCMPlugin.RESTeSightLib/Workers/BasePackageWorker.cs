using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Const;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.DAO;
using CommonUtil;
using Huawei.SCCMPlugin.Models.Firmware;
using Huawei.SCCMPlugin.Models.Deploy;
using System.Web;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers
{
    /// <summary>
    /// 固件上传
    /// </summary>
    public class BasePackageWorker : IBasePackageWorker
    {
        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        public IESSession ESSession
        {
            get; set;
        }
        string errorPix = "firmware.error.";
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
        private Exception GetDeployException(string code, IBasePackageWorker basePackageWorker, string message)
        {
            return new BasePackageExpceion(errorPix, code, basePackageWorker, message);
        }
        #region 上传固件任务
        /// <summary>
        /// 上传固件任务。
        /// </summary>
        /// <param name="basePackage"></param>
        /// <returns></returns>
        public string UploadBasePackage(BasePackage basePackage)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_UPLOADE_BASEPACKAGE);
            int retCode = 0;
            JObject jResult = ESSession.HCPost(sb.ToString(), basePackage);
            CheckAndThrowException(jResult);
            HWESightTask eSightTask = HWESightTaskDal.Instance.FindTaskByBasepackageName(this.ESSession.HWESightHost.ID, basePackage.BasepackageName);
            if (eSightTask == null)
                eSightTask = new HWESightTask();
            else
                LogUtil.HWLogger.API.WarnFormat("Find same package name in the database, the data will be overwrite=[{0}]", basePackage.BasepackageName);
            string taskName = "";
            QueryObjectResult<JObject> queryObjectResult = jResult.ToObject<QueryObjectResult<JObject>>();
            if (queryObjectResult.Code == 0)
            {
                taskName = JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");
                //Save to database.
                eSightTask.HWESightHostID = this.ESSession.HWESightHost.ID;
                eSightTask.TaskName = taskName;
                eSightTask.SoftWareSourceName = basePackage.BasepackageName;
                eSightTask.ReservedStr1 = basePackage.BasepackageType;
                eSightTask.TaskStatus = ConstMgr.HWESightTask.TASK_STATUS_RUNNING;//初始化。
                eSightTask.TaskProgress = 0;
                eSightTask.TaskResult = "";
                eSightTask.ErrorDetail = "";
                eSightTask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
                eSightTask.TaskType = ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE;
                eSightTask.LastModifyTime = System.DateTime.Now;
                eSightTask.CreateTime = System.DateTime.Now;
                int taskId = HWESightTaskDal.Instance.InsertEntity(eSightTask);
                LogUtil.HWLogger.API.InfoFormat("add task：{0}", taskId);
            }
            return taskName;
        }

        public BasePackageProgress QueryBasePackageProcess(string taskName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_BASEPACKAGE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult, "Find package progress filed.");
            QueryObjectResult<BasePackageProgress> queryObjectResult = jResult.ToObject<QueryObjectResult<BasePackageProgress>>();
            //sync to the database.
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            if (hwtask == null)
            {
                throw new BasePackageExpceion(ConstMgr.ErrorCode.DB_NOTFOUND, this, string.Format("Find package progress filed: System can not find this task in the database[{0}]", taskName));
            }
            else
            {
                SaveUploadProgressToDB(queryObjectResult, taskName);
            }
            return queryObjectResult.Data;
        }

        /// <summary>
        /// 删除升级包
        /// </summary>
        /// <param name="packageName">升级包名字</param>
        public void DeleteBasePackage(string packageName) {

            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DELETE_BASEPACKAGE);
            
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("basepackageName", packageName));
            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);

            CheckAndThrowException(jResult);
            DeleteTaskProgressFromDBByName(packageName);
        }
        /// <summary>
        /// 删除软件源，从数据库。
        /// </summary>
        /// <param name="basepackageName">软件源名称，eSight 返回的任务名</param>
        private void DeleteTaskProgressFromDBByName(string basepackageName)
        {
            HWESightTask hwTask = HWESightTaskDal.Instance.FindTaskBySourceName(this.ESSession.HWESightHost.ID, basepackageName);
            if (hwTask != null)
            {
                HWESightTaskDal.Instance.DeleteEntityById(hwTask.ID);
            }
            else
            {
                LogUtil.HWLogger.API.WarnFormat("System can't find this package in the database=[{0}]", basepackageName);
            }
        }

        private void SaveUploadProgressToDB(QueryObjectResult<BasePackageProgress> queryObjectResult, string taskName)
        {

            //sync to the database.
            BasePackageProgress basePackageProgess = queryObjectResult.Data;
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            if (hwtask == null)
            {
                throw new BasePackageExpceion(ConstMgr.ErrorCode.DB_NOTFOUND, this, string.Format("Find upload package progress filed: System can not find this task in the database[{0}]", taskName));
            }
            else
            {
                hwtask.TaskStatus = basePackageProgess.TaskStatus;
                hwtask.TaskProgress = basePackageProgess.TaskProgress;
                hwtask.TaskResult = basePackageProgess.TaskResult;
                hwtask.TaskCode = (!string.IsNullOrEmpty(basePackageProgess.TaskCode) && !string.Equals(basePackageProgess.TaskCode, "0"))?
                (errorPix+ basePackageProgess.TaskCode):basePackageProgess.TaskCode;
                hwtask.ErrorDetail = basePackageProgess.ErrorDetail;
                hwtask.SyncStatus = GetTaskStatus(hwtask.SyncStatus, basePackageProgess.TaskStatus, basePackageProgess.TaskResult);
                hwtask.LastModifyTime = System.DateTime.Now;
                HWESightTaskDal.Instance.UpdateEntity(hwtask);
            }
        }

        private void SyncUploadPackageFromESight()
        {
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindCreatedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE);
            Exception lastException = null;
            foreach (HWESightTask hwTask in hwTaskList)
            {
                try
                {
                    QueryBasePackageProcess(hwTask.TaskName);
                }
                catch (Exception se)
                {
                    LogUtil.HWLogger.API.Error(se);
                    //lastException = se;
                }
            }
           // if (lastException != null) throw lastException;
        }

        public IList<HWESightTask> FindUnFinishedUploadPackageTask()
        {
            SyncUploadPackageFromESight();
            //Search Again.
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindUnFinishedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE);
            return hwTaskList;
        }
        #endregion 上传固件任务

        #region 查询固件
        public QueryLGListResult<BasePackageItem> QueryBasePackagePage(int pageNo = -1, int pageSize = int.MaxValue)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PAGE_BASEPACKAGE);
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
            if (paramBuild.Length > 0)
            {
                sb.Append("?").Append(paramBuild.ToString());
            }

            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryLGListResult<BasePackageItem> queryLGListResult = jResult.ToObject<QueryLGListResult<BasePackageItem>>();
            return queryLGListResult;
        }
        public QueryObjectResult<BasePackageDetail> QueryBasePackageDetail(string basepackageName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DETAIL_BASEPACKAGE);
            StringBuilder paramBuild = new StringBuilder();
            if (!string.IsNullOrEmpty(basepackageName))
            {
                //if (paramBuild.Length > 0) paramBuild.Append("&");
                paramBuild.Append("basepackageName=").Append(HttpUtility.UrlEncode(basepackageName, Encoding.UTF8));
            }
            if (paramBuild.Length > 0)
            {
                sb.Append("?").Append(paramBuild.ToString());
            }

            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryObjectResult<BasePackageDetail> queryObjectResult = jResult.ToObject<QueryObjectResult<BasePackageDetail>>();
            return queryObjectResult;
        }
        #endregion 查询固件

        #region 部署固件任务
        /// <summary>
        /// 添加部署任务
        /// </summary>
        /// <param name="taskSourceName"></param>
        /// <param name="deployTask"></param>
        /// <returns>返回eSight任务名</returns>
        public string AddDeployTask(DeployPackageTask deployTask)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_TASK_BASEPACKAGE);
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("basepackageName", deployTask.BasepackageName));
            parameters.Add(new KeyValuePair<string, object>("firmwareList", deployTask.FirmwareList));
            parameters.Add(new KeyValuePair<string, object>("dn", deployTask.DeviceDn));
            parameters.Add(new KeyValuePair<string, object>("isforceupgrade", deployTask.IsForceUpgrade));
            parameters.Add(new KeyValuePair<string, object>("effectiveMethod", deployTask.EffectiveMethod));
            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);
            CheckAndThrowException(jResult);
            HWESightTask eSightTask = new HWESightTask();
            QueryObjectResult<JObject> queryObjectResult = jResult.ToObject<QueryObjectResult<JObject>>();
            if (queryObjectResult.Code != 0)
            {
                throw GetDeployException(queryObjectResult.Code.ToString(), this, queryObjectResult.Description);//"添加部署任务出错:" + 
            }
            else
            {//Save to database.
                eSightTask.HWESightHostID = this.ESSession.HWESightHost.ID;
                eSightTask.TaskName = JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");
                eSightTask.SoftWareSourceName = deployTask.BasepackageName;

                eSightTask.TaskStatus = ConstMgr.HWESightTask.TASK_STATUS_RUNNING;//初始化。
                eSightTask.TaskProgress = 0;
                eSightTask.TaskResult = "";
                eSightTask.ErrorDetail = "";
                eSightTask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
                eSightTask.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE;//部署固件任务
                eSightTask.DeviceIp = deployTask.DeviceDn;
                eSightTask.ReservedStr1 = deployTask.FirmwareList;
                eSightTask.LastModifyTime = System.DateTime.Now;
                eSightTask.CreateTime = System.DateTime.Now;
                int taskId = HWESightTaskDal.Instance.InsertEntity(eSightTask);
                eSightTask.ID = taskId;
                InsertHWTaskResourceList(eSightTask, deployTask.DeviceDn);

                LogUtil.HWLogger.API.InfoFormat("add task:{0}", taskId);
            }
            return JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");

        }
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
                hwResource.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE;
                hwResource.SyncStatus = hwtask.SyncStatus;
                hwResource.LastModifyTime = System.DateTime.Now;
                hwResource.CreateTime = System.DateTime.Now;
                HWTaskResourceDal.Instance.InsertEntity(hwResource);
            }
        }

        public DeployPackageTaskDetail QueryDeployTaskProcess(string taskName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_TASK_PROGRESS_BASEPACKAGE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryObjectResult<DeployPackageTaskDetail> queryObjectResult = jResult.ToObject<QueryObjectResult<DeployPackageTaskDetail>>();
            //sync to the database.
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            if (hwtask == null)
            {
                throw new BasePackageExpceion(ConstMgr.ErrorCode.DB_NOTFOUND, this, string.Format("Find task failed, system can't find this task.[{0}]", taskName));
            }
            else
            {
                SaveDeployProgressToDB(queryObjectResult, taskName);
            }
            return queryObjectResult.Data;
        }
        public QueryObjectResult<BasePackageDNProgress> QueryDeployTaskDNProcess(string taskName, string dn)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_TASK_DN_PROGRESS_BASEPACKAGE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8))
               .Append("&dn=").Append(HttpUtility.UrlEncode(dn, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            var queryObjectResult = jResult.ToObject<QueryObjectResult<BasePackageDNProgress>>();
            foreach (FirmwarelistProgress firmlistProgress in queryObjectResult.Data.Firmwarelist) {
                if (!string.IsNullOrEmpty(firmlistProgress.Details) && !string.Equals(firmlistProgress.Details, "0"))
                    firmlistProgress.Details = errorPix + firmlistProgress.Details;
            }
            return queryObjectResult;
        }

        private void SaveDeployProgressToDB(QueryObjectResult<DeployPackageTaskDetail> queryObjectResult, string taskName)
        {
            //sync to the database.
            DeployPackageTaskDetail deployProgress = queryObjectResult.Data;
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);

            hwtask.TaskStatus = deployProgress.TaskStatus;

            hwtask.TaskResult = deployProgress.TaskResult;
            //hwtask.TaskCode = deployProgress.TaskCode;
            //hwtask.ErrorDetail = deployProgress.ErrorDetail;
            
            hwtask.LastModifyTime = System.DateTime.Now;

            //find child device in database.
            int progress = UpdateHWTaskResourceList(hwtask, deployProgress);
            if (progress > 0)
                hwtask.TaskProgress = progress;// deployProgress.TaskProgress;
            if (deployProgress.TaskProgress > 0)
                hwtask.TaskProgress = deployProgress.TaskProgress;
            if (progress == 100) hwtask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;

            hwtask.SyncStatus = GetTaskStatus(hwtask.SyncStatus, deployProgress.TaskStatus, deployProgress.TaskResult);
            HWESightTaskDal.Instance.UpdateEntity(hwtask);
        }
        private int UpdateHWTaskResourceList(HWESightTask hwtask, DeployPackageTaskDetail deployProgress)
        {
            /*
            bool isFinsihed = false;
            int finishedCnt = 0;
            IList<HWTaskResource> resourceList = HWTaskResourceDal.Instance.FindTaskResourceByTaskId(hwtask.ID);
            Dictionary<string, DeviceProgress> dviDict = new Dictionary<string, DeviceProgress>();
            foreach (DeviceProgress deviceProgress in deployProgress.DeviceDetails)
            {
              dviDict[deviceProgress.DeviceDn.ToUpper()] = deviceProgress;
            }
            foreach (HWTaskResource hwResource in resourceList)
            {
              DeviceProgress deviceProgress = dviDict[hwResource.DN.ToUpper()];
              hwResource.DeviceResult = deviceProgress.DeviceResult;
              hwResource.DeviceProgress = deviceProgress.Progress;
              hwResource.ErrorDetail = deviceProgress.ErrorDetail;
              hwResource.TaskType = ConstMgr.HWESightTask.TASK_TYPE_DEPLOY;
              hwResource.SyncStatus = hwtask.SyncStatus;
              hwResource.LastModifyTime = System.DateTime.Now;
              HWTaskResourceDal.Instance.UpdateEntity(hwResource);
              if (deviceProgress.Progress == 100) finishedCnt++;
            }
            if (resourceList.Count > 0)
              return (finishedCnt * 100) / resourceList.Count;
            else
              return 0;*/
            return 0;
        }


        public string GetTaskStatus(string oldStatus, string taskStatus, string taskResult)
        {

            if (string.Equals(taskResult, "Failed", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;
            if (string.Equals(taskResult, "Partion Success", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_HW_PFAILED;
            if (string.Equals(taskStatus, "Failed", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;
            if (string.Equals(taskResult, "Success", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            if (string.Equals(taskStatus, "Running", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
            //if (string.Equals(taskStatus, "Complete", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            //if (string.Equals(taskStatus, "Success", StringComparison.OrdinalIgnoreCase)) return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            
            // if (taskCode != "0") return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;

            return oldStatus;
        }


        public void SyncDeployTaskFromESight()
        {
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindCreatedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE);
            Exception lastException = null;
            foreach (HWESightTask hwTask in hwTaskList)
            {
                try
                {
                    QueryDeployTaskProcess(hwTask.TaskName);
                }
                catch (Exception se)
                {
                    LogUtil.HWLogger.API.Error(se);
                    //lastException = se;
                }
            }
            //if (lastException != null) throw lastException;
        }

        /*
        private void DeleteTaskProgressFromDBByName(string basePackagename)
        {
            HWESightTask hwTask = HWESightTaskDal.Instance.FindTaskByBasepackageName(this.ESSession.HWESightHost.ID, basePackagename);
            if (hwTask != null)
            {
                HWESightTaskDal.Instance.DeleteEntityById(hwTask.ID);
            }
            else
            {
                LogUtil.HWLogger.API.WarnFormat("删除时，在数据库中没有找到此升级包数据=[{0}]", basePackagename);
            }
        }*/
        public int DeleteTask(int taskId)
        {
            return HWESightTaskDal.Instance.DeleteEntityById(taskId);
        }
        public IList<HWESightTask> FindUnFinishedDeployTask()
        {
            SyncDeployTaskFromESight();
            //Search Again.
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindUnFinishedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE);
            return hwTaskList;
        }
        public QueryPageResult<HWESightTask> FindDeployTaskWithSync(QueryDeployPackageParam queryDeployParam)
        {
            SyncDeployTaskFromESight();
            //Search Again.
            QueryPageResult<HWESightTask> hwTaskPage = HWESightTaskDal.Instance.FindDeployPackageTask(this.ESSession.HWESightHost.ID, queryDeployParam);
            return hwTaskPage;
        }

        #endregion 部署固件任务
    }


}
