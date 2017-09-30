using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models.Deploy;
using Huawei.SCCMPlugin.Models.Firmware;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// eSight任务数据库管理类 软件源+OS模板+上下电 任务表
    /// </summary>
    public class HWESightTaskDal : BaseRepository<HWESightTask>, IHWESightTaskDal
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static HWESightTaskDal Instance
        {
            get { return SingletonProvider<HWESightTaskDal>.Instance; }
        }
        /// <summary>
        /// 根据任务名，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskName">eSight返回的任务名</param>
        /// <returns>对应的任务</returns>
        public HWESightTask FindTaskByName(int eSightID, string taskName)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and TASK_NAME='" + ReplaceQuote(taskName) + "'");
            if (queryList.Count > 0)
            {
                return queryList[0];
            }
            else
                return null;
        }
        /// <summary>
        /// 根据软件源名，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskName">eSight返回的任务名</param>
        /// <returns>对应的任务</returns>
        public HWESightTask FindTaskBySourceName(int eSightID, string sourceName)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and SOFTWARE_SOURCE_NAME='" + ReplaceQuote(sourceName) + "' and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE + "'");
            if (queryList.Count > 0)
            {
                return queryList[0];
            }
            else
                return null;
        }
        /// <summary>
        /// 根据软件源名，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskName">eSight返回的任务名</param>
        /// <returns>对应的任务</returns>
        public HWESightTask FindTaskByBasepackageName(int eSightID, string basepackageName)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and SOFTWARE_SOURCE_NAME='" + ReplaceQuote(basepackageName) + "' and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE + "'");
            if (queryList.Count > 0)
            {
                return queryList[0];
            }
            else
                return null;
        }
        /// <summary>
        /// 根据任务类型，查询对应的eSight任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>任务列表</returns>
        public IList<HWESightTask> FindTaskByType(int eSightID, string taskType)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and TASK_TYPE='" + taskType + "'");
            return queryList;
        }
        /// <summary>
        /// 根据记录数和分页大小，计算有多少页。
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <param name="recordCount">记录总数</param>
        /// <returns></returns>
        private int GetPageCount(int pageSize, int recordCount)
        {
            int pageCount = recordCount % pageSize == 0 ? recordCount / pageSize : recordCount / pageSize + 1;
            if (pageCount < 1) pageCount = 1;
            return pageCount;
        }
        /// <summary>
        ///  查询部署任务分页
        /// </summary>
        /// <param name="eSightID"></param>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        public QueryPageResult<HWESightTask> FindDeployTask(int eSightID, QueryDeployParam queryDeployParam)
        {
            QueryPageResult<HWESightTask> queryPageResult = new QueryPageResult<HWESightTask>();
            int totalCount = 0;
            string sql = " HW_ESIGHT_HOST_ID=" + eSightID;
            if (!string.IsNullOrWhiteSpace(queryDeployParam.TaskSourceName))
                sql += " and SOFTWARE_SOURCE_NAME like '%" + this.ReplaceQuote(queryDeployParam.TaskSourceName.Trim()) + "%'";

            if (!string.IsNullOrWhiteSpace(queryDeployParam.TaskStatus))
                sql += " and SYNC_STATUS ='" + this.ReplaceQuote(queryDeployParam.TaskStatus.Trim()) + "'";

            queryPageResult.Data = GetList(out totalCount, sql
              + " and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_DEPLOY + "'",
              queryDeployParam.PageSize, queryDeployParam.PageNo,queryDeployParam.Order, queryDeployParam.OrderDesc);
            queryPageResult.TotalSize = totalCount;
            queryPageResult.TotalPage = GetPageCount(queryDeployParam.PageSize, totalCount);
            return queryPageResult;
        }
        /// <summary>
        ///  查询模板部署任务分页
        /// </summary>
        /// <param name="eSightID"></param>
        /// <param name="queryDeployParam">查询参数</param>
        /// <returns>分页对象</returns>
        public QueryPageResult<HWESightTask> FindDeployPackageTask(int eSightID, QueryDeployPackageParam queryDeployParam)
        {
            QueryPageResult<HWESightTask> queryPageResult = new QueryPageResult<HWESightTask>();
            int totalCount = 0;
            string sql = " HW_ESIGHT_HOST_ID=" + eSightID;
            if (!string.IsNullOrWhiteSpace(queryDeployParam.TaskeName))
                sql += " and TASK_NAME like '%" + this.ReplaceQuote(queryDeployParam.TaskeName.Trim()) + "%'";

            if (!string.IsNullOrWhiteSpace(queryDeployParam.TaskStatus))
                sql += " and SYNC_STATUS ='" + this.ReplaceQuote(queryDeployParam.TaskStatus.Trim()) + "'";

            queryPageResult.Data = GetList(out totalCount, sql
              + " and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_DEPLOYFIRMWARE + "'",
              queryDeployParam.PageSize, queryDeployParam.PageNo, queryDeployParam.Order, queryDeployParam.OrderDesc);
            queryPageResult.TotalSize = totalCount;
            queryPageResult.TotalPage = GetPageCount(queryDeployParam.PageSize, totalCount);
            return queryPageResult;
        }
        /// <summary>
        /// 返回未完成的任务，包括错误的任务，查询对应的eSight运行中的任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>未完成的任务列表</returns>
        public IList<HWESightTask> FindUnFinishedTaskByType(int eSightID, string taskType)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and TASK_TYPE='" + taskType + "' and SYNC_STATUS!='" + ConstMgr.HWESightTask.SYNC_STATUS_FINISHED + "'");
            return queryList;
        }
        /// <summary>
        /// 根据任务类型，查询对应的eSight运行中的任务。
        /// </summary>
        /// <param name="eSightID">对应的eSight ID</param>
        /// <param name="taskType">任务类型</param>
        /// <returns>运行中的任务列表</returns>
        public IList<HWESightTask> FindCreatedTaskByType(int eSightID, string taskType)
        {
            IList<HWESightTask> queryList = GetList(" HW_ESIGHT_HOST_ID=" + eSightID + " and TASK_TYPE='" + taskType + "' and SYNC_STATUS='" + ConstMgr.HWESightTask.SYNC_STATUS_CREATED + "'");
            return queryList;
        }
        /// <summary>
        /// 清除失败任务
        /// </summary>
        /// <returns>清除的数量</returns>
        public int ClearFailedPackageTask() {
            return ExecuteSql("delete from HW_ESIGHT_TASK where (SYNC_STATUS = '" + ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED +
                "' or SYNC_STATUS='" + ConstMgr.HWESightTask.SYNC_STATUS_SYNC_FAILED + "'" +
                " or SYNC_STATUS='" + ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED + "')" +
                " and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_FIRMWARE + "'");
        }
        /// <summary>
        /// 清除失败软件源任务
        /// </summary>
        /// <returns>清除的数量</returns>
        public int ClearFailedSoftwareSourceTask()
        {
            return ExecuteSql("delete from HW_ESIGHT_TASK where (SYNC_STATUS = '" + ConstMgr.HWESightTask.TASK_RESULT_FAILED +
                "' or SYNC_STATUS='" + ConstMgr.HWESightTask.SYNC_STATUS_SYNC_FAILED + "'" +
                " or SYNC_STATUS='" + ConstMgr.HWESightTask.SYNC_STATUS_HW_FAILED + "')" +
                " and TASK_TYPE='" + ConstMgr.HWESightTask.TASK_TYPE_SOFTWARE + "'");
        }
        /// <summary>
        /// 替换SQL单引号
        /// </summary>
        /// <param name="sql">运行脚本</param>
        /// <returns>替换后的脚本。</returns>
        public string ReplaceQuote(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}
