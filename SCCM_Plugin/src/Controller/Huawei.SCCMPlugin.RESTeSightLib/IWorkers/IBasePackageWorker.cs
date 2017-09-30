using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Deploy;
using Huawei.SCCMPlugin.Models.Firmware;
using Huawei.SCCMPlugin.Models.Softwares;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.IWorkers
{
    /// <summary>
    /// 升级包业务类
    /// </summary>
    public interface IBasePackageWorker
    {
        /// <summary>
        /// 升级包上传
        /// </summary>
        /// <param name="basePackage">升级包参数</param>
        /// <returns>返回任务名</returns>
        string UploadBasePackage(BasePackage basePackage);
        /// <summary>
        /// 升级文件上传进度查询
        /// </summary>
        /// <param name="taskName">任务名，eSight返回的</param>
        /// <returns>进度对象</returns>
        BasePackageProgress QueryBasePackageProcess(string taskName);

        /// <summary>
        /// 删除升级包
        /// </summary>
        /// <param name="packageName">升级包名字</param>
        void DeleteBasePackage(string packageName);

        /// <summary>
        /// 查询升级包上传任务。
        /// </summary>
        /// <returns>任务列表</returns>
        IList<HWESightTask> FindUnFinishedUploadPackageTask();


        /// <summary>
        /// 升级包列表查询。
        /// </summary>
        /// <param name="pageNo">当前页</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns>分页对象</returns>
        QueryLGListResult<BasePackageItem> QueryBasePackagePage(int pageNo = -1, int pageSize = int.MaxValue);
        /// <summary>
        /// 查询固件明细
        /// </summary>
        /// <param name="basepackageName"></param>
        /// <returns></returns>
        QueryObjectResult<BasePackageDetail> QueryBasePackageDetail(string basepackageName);

        /// <summary>
        /// 添加固件部署任务
        /// </summary>
        /// <param name="deployTask">部署任务对象</param>
        /// <returns>eSight返回的的任务名</returns>
        string AddDeployTask(DeployPackageTask deployTask);
        /// <summary>
        /// 查询固件部署任务列表
        /// </summary>
        /// <returns></returns>
        IList<HWESightTask> FindUnFinishedDeployTask();
        /// <summary>
        /// 查询固件部署任务进度
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        DeployPackageTaskDetail QueryDeployTaskProcess(string taskName);
        /// <summary>
        /// 查询固件部署任务，设备执行的明细
        /// </summary>
        /// <param name="taskName">任务的标示，eSight返回</param>
        /// <param name="dn">dn</param>
        /// <returns>JObject,因为层次比较多，而且后台也不做保存所以直接传JObject。</returns>
        QueryObjectResult<BasePackageDNProgress> QueryDeployTaskDNProcess(string taskName,string dn);
        /// <summary>
        /// 同步未完成任务。
        /// 在固件升级，实际上存在两种任务，一种类似软件源上传，另一种类似模板部署。
        /// ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE
        /// ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE
        /// </summary>
        void SyncDeployTaskFromESight();
        /// <summary>
        /// 查询所有任务，并且分页。
        /// </summary>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        QueryPageResult<HWESightTask> FindDeployTaskWithSync(QueryDeployPackageParam queryDeployParam);
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>删除的数量</returns>
        int DeleteTask(int taskId);
        /// <summary>
        /// 当前的eSession连接对象
        /// </summary>
        IESSession ESSession { get; set; }

       
    }
}
