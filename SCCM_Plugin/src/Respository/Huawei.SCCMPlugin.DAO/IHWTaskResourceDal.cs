using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// 查询任务资源表-数据库业务类。
    /// </summary>
    public interface IHWTaskResourceDal : IBaseRepository<HWTaskResource>
    {
        /// <summary>
        /// 查询一个任务对应的iBMC服务器。
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>对应的任务资源列表通过任务id</returns>
        IList<HWTaskResource> FindTaskResourceByTaskId(int taskId);
        /// <summary>
        /// 查询任务对应的iBMC服务器。
        /// </summary>
        /// <param name="taskIds">任务ID组成的字符串 eg: 1,2,3,4</param>
        /// <returns>对应的任务资源列表</returns>
        IList<HWTaskResource> FindTaskResourceByTaskIds(string taskIds);
    }
}
