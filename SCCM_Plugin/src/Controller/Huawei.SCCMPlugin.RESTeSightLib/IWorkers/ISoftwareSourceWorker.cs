using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Softwares;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.IWorkers
{
    /// <summary>
    /// 上传软件源 业务类
    /// </summary>
    public interface ISoftwareSourceWorker
    {
        /// <summary>
        /// 上传软件源，直接返回结果。
        /// 不抛出错误
        /// </summary>
        /// <param name="softwareSource">软件源参数</param>
        /// <returns>QueryObjectResult[taskname]</returns>
        QueryObjectResult<JObject> UploadSoftwareSourceWithResult(SoftwareSource softwareSource);
        /// <summary>
        /// 上传软件源
        /// </summary>
        /// <param name="softwareSource">软件源参数</param>
        /// <returns>返回任务名</returns>
        string UploadSoftwareSource(SoftwareSource softwareSource);
        /// <summary>
        /// 查询上传软件源进度
        /// </summary>
        /// <param name="taskName">eSight 返回的任务名</param>
        /// <returns>上传软件源进度对象</returns>
        SourceProgress QuerySoftwareProcess(string taskName);
        /// <summary>
        /// 查询软件源分页。
        /// </summary>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns>分页对象</returns>
        QueryLGListResult<SourceItem> QuerySoftwarePage(int pageNo = -1, int pageSize = -1);
        /// <summary>
        /// 删除软件源
        /// </summary>
        /// <param name="softwaresourceName">软件源名称，eSight 返回的任务名</param>
        void DeleteSoftwareSource(string softwaresourceName);
        /// <summary>
        /// 同步未完成任务。
        /// </summary>
        void SyncTaskFromESight();
        /// <summary>
        /// 获得同步软件源任务列表并返回列表。
        /// </summary>
        /// <returns>软件源任务列表</returns>
        IList<HWESightTask> FindSoftwareTaskWithSync();
        /// <summary>
        /// 查找所有未完成任务。
        /// </summary>
        /// <returns>任务列表</returns>
        IList<HWESightTask> FindUnFinishedTask();
        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        IESSession ESSession { get; set; }
    }
}
