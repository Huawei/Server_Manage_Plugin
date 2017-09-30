using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Deploy;
using Huawei.SCCMPlugin.Models.Firmware;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// eSight任务数据库管理类 软件源+OS模板+上下电 任务表
    /// </summary>
    public interface IHWESightTaskDal : IBaseRepository<HWESightTask>
    {
        /// <summary>
        /// 根据任务名，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskName">eSight返回的任务名</param>
        /// <returns>对应的任务</returns>
        HWESightTask FindTaskByName(int eSightID, string taskName);
        /// <summary>
        /// 根据软件源名，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskName">eSight返回的任务名</param>
        /// <returns>对应的任务</returns>
        HWESightTask FindTaskBySourceName(int eSightID, string sourceName);
        /// <summary>
        /// 根据任务类型，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>任务列表</returns>
        IList<HWESightTask> FindTaskByType(int eSightID, string taskType);
        /// <summary>
        /// 根据任务类型，查询对应的eSight运行中的任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>运行中的任务列表</returns>
        IList<HWESightTask> FindCreatedTaskByType(int eSightID, string taskType);
        /// <summary>
        /// 返回未完成的任务，包括错误的任务，查询对应的eSight运行中的任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>未完成的任务列表</returns>
        IList<HWESightTask> FindUnFinishedTaskByType(int eSightID, string taskType);
        /// <summary>
        ///  查询模板部署任务分页
        /// </summary>
        /// <param name="eSightID"></param>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        QueryPageResult<HWESightTask> FindDeployTask(int eSightID, QueryDeployParam queryDeployParam);
        /// <summary>
        /// 清除失败任务
        /// </summary>
        /// <returns>清除的数量</returns>
        int ClearFailedPackageTask();
        /// <summary>
        /// 查询部署任务分页。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        QueryPageResult<HWESightTask> FindDeployPackageTask(int eSightID, QueryDeployPackageParam queryDeployParam);
    }
}
