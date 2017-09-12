using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Softwares;
using Huawei.SCCMPlugin.Const;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.DAO;
using System.Web;
using CommonUtil;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers
{
    /// <summary>
    /// 上传软件源 业务类
    /// </summary>
    public class SoftwareSourceWorker : ISoftwareSourceWorker
    {
        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        public IESSession ESSession
        {
            get; set;
        }
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
        private Exception GetDeployException(string code, ISoftwareSourceWorker softwareSource, string message) {
             return new SoftwareSourceExpceion("deploy.error.", code, softwareSource, message);
        }
        /// <summary>
        /// 上传软件源，直接返回结果。
        /// 不抛出错误
        /// </summary>
        /// <param name="softwareSource">软件源参数</param>
        /// <returns>QueryObjectResult[taskname]</returns>
        public QueryObjectResult<JObject> UploadSoftwareSourceWithResult(SoftwareSource softwareSource)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_UPLOADE_SOFTWARESOURCE);
            JObject jResult = ESSession.HCPost(sb.ToString(), softwareSource);//提交eSight服务器
            CheckAndThrowException(jResult);//检测提交错误。
            HWESightTask eSightTask = HWESightTaskDal.Instance.FindTaskBySourceName(this.ESSession.HWESightHost.ID, softwareSource.SoftwareName);
            if (eSightTask == null)//不是重复任务，创建任务对象。
                eSightTask = new HWESightTask();
            else//重复的任务。
                LogUtil.HWLogger.API.WarnFormat("Find same source in the database, the data will be overwite=[{0}]", softwareSource.SoftwareName);
            string taskName = "";//eSight返回的任务名。
            QueryObjectResult<JObject> queryObjectResult = jResult.ToObject<QueryObjectResult<JObject>>();//转换Json对象为期待的序列化对象。
            if (queryObjectResult.Code == 0)//成功
            {
                taskName = JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");
                //Save to database.
                eSightTask.HWESightHostID = this.ESSession.HWESightHost.ID;
                eSightTask.TaskName = taskName;
                eSightTask.SoftWareSourceName = softwareSource.SoftwareName;
                eSightTask.TaskStatus = ConstMgr.HWESightTask.TASK_STATUS_RUNNING;//初始化。
                eSightTask.TaskProgress = 0;
                eSightTask.TaskResult = "";
                eSightTask.ErrorDetail = "";
                eSightTask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
                eSightTask.TaskType = ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE;
                eSightTask.LastModifyTime = System.DateTime.Now;
                eSightTask.CreateTime = System.DateTime.Now;
                int taskId = HWESightTaskDal.Instance.InsertEntity(eSightTask);
                LogUtil.HWLogger.API.InfoFormat("Add task：{0}", taskId);
            }
            return queryObjectResult;
        }
        /// <summary>
        /// 上传软件源
        /// </summary>
        /// <param name="softwareSource">软件源参数</param>
        /// <returns>返回任务名</returns>
        public string UploadSoftwareSource(SoftwareSource softwareSource)
        {
            QueryObjectResult<JObject> queryObjectResult = UploadSoftwareSourceWithResult(softwareSource);
            if (queryObjectResult.Code != 0)
            {
                throw GetDeployException(queryObjectResult.Code.ToString(), this, queryObjectResult.Description);//"上传软件源出错:" +
            }
            string taskName = JsonUtil.GetJObjectPropVal<string>(queryObjectResult.Data, "taskName");
            return taskName;
        }
        /// <summary>
        /// 保存上传软件源进度到数据。
        /// </summary>
        /// <param name="queryObjectResult">返回的软件源进度对戏</param>
        /// <param name="taskName">eSight对应的任务名。</param>
        private void SaveTaskProgressToDB(QueryObjectResult<SourceProgress> queryObjectResult, string taskName)
        {
            //sync to the database.
            SourceProgress sourceProgress = queryObjectResult.Data;
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            hwtask.TaskStatus = sourceProgress.TaskStatus;
            hwtask.TaskProgress = sourceProgress.TaskProgress;
            hwtask.TaskResult = sourceProgress.TaskResult;
            hwtask.TaskCode =
              (!string.IsNullOrEmpty(sourceProgress.TaskCode) && !string.Equals(sourceProgress.TaskCode, "0")) ?
                ("deploy.error." + sourceProgress.TaskCode) : sourceProgress.TaskCode;

            hwtask.ErrorDetail = sourceProgress.ErrorDetail;
            hwtask.SyncStatus = GetTaskStatus(hwtask.SyncStatus, sourceProgress.TaskStatus, sourceProgress.TaskResult, sourceProgress.TaskCode);
            hwtask.LastModifyTime = System.DateTime.Now;
            HWESightTaskDal.Instance.UpdateEntity(hwtask);
            /* if (hwtask == null)
             {
                 throw new SoftwareSourceExpceion(ConstMgr.ErrorCode.DB_NOTFOUND, this, string.Format("查询软件源上传进度出错:数据库没有找到对应的任务。[{0}]", taskName));
             }
             else
             {
                 hwtask.TaskStatus = sourceProgress.TaskStatus;
                 hwtask.TaskProgress = sourceProgress.TaskProgress;
                 hwtask.TaskResult = sourceProgress.TaskResult;
                 hwtask.TaskCode = sourceProgress.TaskCode;
                 hwtask.ErrorDetail = sourceProgress.ErrorDetail;
                 hwtask.SyncStatus = ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
                 hwtask.LastModifyTime = System.DateTime.Now;
                 HWESightTaskDal.Instance.UpdateEntity(hwtask);
             }*/

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
            if (taskResult == "Failed") return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;
            if (CoreUtil.GetObjTranNull<int>(taskCode) != 0) return ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED;

            if (taskResult == "Success") return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            if (taskStatus == "Complete") return ConstMgr.HWESightTask.SYNC_STATUS_FINISHED;
            if (taskStatus == "Running") return ConstMgr.HWESightTask.SYNC_STATUS_CREATED;
            

            return oldStatus;
        }
        /// <summary>
        /// 查询上传软件源进度
        /// </summary>
        /// <param name="taskName">eSight 返回的任务名</param>
        /// <returns>上传软件源进度对象</returns>
        public SourceProgress QuerySoftwareProcess(string taskName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PROGRESS_SOFTWARESOURCE);
            sb.Append("?taskName=").Append(HttpUtility.UrlEncode(taskName, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString());
            CheckAndThrowException(jResult);
            QueryObjectResult<SourceProgress> queryObjectResult = jResult.ToObject<QueryObjectResult<SourceProgress>>();
            //sync to the database.
            HWESightTask hwtask = HWESightTaskDal.Instance.FindTaskByName(this.ESSession.HWESightHost.ID, taskName);
            SaveTaskProgressToDB(queryObjectResult, taskName);
            return queryObjectResult.Data;
        }
        /// <summary>
        /// 删除软件源，从数据库。
        /// </summary>
        /// <param name="softwaresourceName">软件源名称，eSight 返回的任务名</param>
        private void DeleteTaskProgressFromDBByName(string softwaresourceName)
        {
            HWESightTask hwTask = HWESightTaskDal.Instance.FindTaskBySourceName(this.ESSession.HWESightHost.ID, softwaresourceName);
            if (hwTask != null)
            {
                HWESightTaskDal.Instance.DeleteEntityById(hwTask.ID);
            }
            else
            {
                LogUtil.HWLogger.API.WarnFormat("System can't find this source in the database=[{0}]", softwaresourceName);
            }
        }
        /// <summary>
        /// 删除软件源
        /// </summary>
        /// <param name="softwaresourceName">软件源名称，eSight 返回的任务名</param>
        public void DeleteSoftwareSource(string softwaresourceName)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DELETE_SOFTWARESOURCE);
            //删除软件源需要把参数放在Body,是POST by Jacky 2017-6-30
            IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("softwareName", softwaresourceName));
            JObject jResult = ESSession.HCPostForm(sb.ToString(), parameters);

            CheckAndThrowException(jResult);
            QueryObjectResult<string> queryObjectResult = jResult.ToObject<QueryObjectResult<string>>();
            DeleteTaskProgressFromDBByName(softwaresourceName);
        }
        /// <summary>
        /// 查询软件源分页。
        /// </summary>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns>分页对象</returns>
        public QueryLGListResult<SourceItem> QuerySoftwarePage(int pageNo = -1, int pageSize = int.MaxValue)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_PAGE_SOFTWARESOURCE);
            StringBuilder paramBuild = new StringBuilder();
            if (pageNo != -1)
            {
                //if (paramBuild.Length > 0) paramBuild.Append("&");
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
            QueryLGListResult<SourceItem> queryLGListResult = jResult.ToObject<QueryLGListResult<SourceItem>>();
            return queryLGListResult;
        }
        /// <summary>
        /// 同步未完成任务。
        /// </summary>
        public void SyncTaskFromESight()
        {
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindCreatedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE);
            Exception lastException = null;
            foreach (HWESightTask hwTask in hwTaskList)
            {
                try
                {
                    QuerySoftwareProcess(hwTask.TaskName);
                }
                catch (Exception se)
                {
                    LogUtil.HWLogger.API.Error(se);
                    //lastException = se;
                    //不抛出错误，防止前台弹出异常。
                }
            }
            //if (lastException != null) throw lastException;
        }
        /// <summary>
        /// 获得同步软件源任务列表并返回列表。
        /// </summary>
        /// <returns>软件源任务列表</returns>
        public IList<HWESightTask> FindSoftwareTaskWithSync()
        {
            //Search Again.
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE);
            return hwTaskList;
        }
        /// <summary>
        /// 查找所有未完成任务。
        /// </summary>
        /// <returns>任务列表</returns>
        public IList<HWESightTask> FindUnFinishedTask()
        {
            SyncTaskFromESight();
            //Search Again.
            IList<HWESightTask> hwTaskList = HWESightTaskDal.Instance.FindUnFinishedTaskByType(this.ESSession.HWESightHost.ID, ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE);
            return hwTaskList;
        }

    }


}
