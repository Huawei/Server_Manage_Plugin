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
    public class HWTaskResourceDal : BaseRepository<HWTaskResource>, IHWTaskResourceDal
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static HWTaskResourceDal Instance
        {
            get { return SingletonProvider<HWTaskResourceDal>.Instance; }
        }
        /// <summary>
        /// 查询一个任务对应的iBMC服务器。
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>对应的任务资源列表通过任务id</returns>
        public IList<HWTaskResource> FindTaskResourceByTaskId(int taskId)
        {
            IList<HWTaskResource> queryList = GetList(" HW_ESIGHT_TASK_ID=" + taskId);
            return queryList;
        }
        /// <summary>
        /// 查询任务对应的iBMC服务器。
        /// </summary>
        /// <param name="taskIds">任务ID组成的字符串 eg: 1,2,3,4</param>
        /// <returns>对应的任务资源列表</returns>
        public IList<HWTaskResource> FindTaskResourceByTaskIds(string taskIds)
        {
            IList<HWTaskResource> queryList = GetList(" HW_ESIGHT_TASK_ID in (" + taskIds+")");
            return queryList;
        }
    }
}
