using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Deploy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.IWorkers
{
    /// <summary>
    /// 模板部署业务类。
    /// </summary>
    public interface IHWDeployWorker
    {
        /// <summary>
        /// 模板创建
        /// </summary>
        /// <param name="deployTemplate">模板参数</param>
        void AddDeployTemplate(DeployTemplate deployTemplate);
        /// <summary>
        /// 查看模板详情
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns>模板详情</returns>
        DeployTemplate QueryDeployTemplate(string templateName);
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateName">模板名称</param>
        void DelDeployTemplate(string templateName);

        /// <summary>
        /// 查询模板。
        /// deployTemplate.TemplateProp为null.
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="templateType">模板类型，可选，默认为ALL</param>
        /// <returns>模板分页对象</returns>
        QueryLGListResult<DeployTemplate> QueryTemplatePage(int pageNo = -1, int pageSize = int.MaxValue, string templateType = "");


        //任务--------------
        /// <summary>
        /// 部署任务
        /// </summary>
        /// <param name="taskSourceName"></param>
        /// <param name="deployTask"></param>
        /// <returns>返回eSight任务名</returns>
        string AddDeployTask(string taskSourceName, DeployTask deployTask);


        /// <summary>
        /// 同步未完成任务。
        /// </summary>
        void SyncTaskFromESight();

        /// <summary>
        /// 获得部署任务分页数据并返回列表。
        /// </summary>
        /// <param name="queryDeployParam"></param>
        /// <returns>分页对象</returns>
        QueryPageResult<HWESightTask> FindDeployTaskWithSync(QueryDeployParam queryDeployParam);

        /// <summary>
        /// 查找所有未完成任务。
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns>任务列表</returns>
        IList<HWESightTask> FindUnFinishedTask();
        /// <summary>
        /// 删除部署任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        int DeleteTask(int taskId);
        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        IESSession ESSession { get; set; }
    }
}
