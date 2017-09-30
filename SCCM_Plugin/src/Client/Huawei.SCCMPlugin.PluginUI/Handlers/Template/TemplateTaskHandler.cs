using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.PluginUI.Attributes;
using Huawei.SCCMPlugin.PluginUI.Helper;
using Newtonsoft.Json;
using Huawei.SCCMPlugin.PluginUI.Interface;
using Huawei.SCCMPlugin.PluginUI.Entitys;
using Huawei.SCCMPlugin.RESTeSightLib;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.Models.Deploy;
using CommonUtil;
using Newtonsoft.Json.Linq;
using Huawei.SCCMPlugin.Models.Softwares;
using Huawei.SCCMPlugin.Models.Devices;

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// 模板部署任务管理
    /// </summary>
    [CefURL("huawei/task/list.html")]
    [Bound("NetBound")]
    public class TemplateTaskHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Template Task page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getTaskList":
                        result = GetTaskList(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    case "getTemplateList":
                        result = GetTemplateList(eventData);
                        break;
                    case "getServerList":
                        result = GetServerList(eventData);
                        break;
                    case "getDeviceDetail":
                        result = GetDeviceDetail(eventData);
                        break;
                    case "delete":
                        result = Delete(eventData);
                        break;
                    case "save":
                        result = Save(eventData);
                        break;
                    default: break;
                }
                return JsonConvert.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template Task page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template Task page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template Task page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载模板任务列表
        /// </summary>
        private QueryPageResult<HWESightTask> GetTaskList(object eventData)
        {
            var ret = new QueryPageResult<HWESightTask>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<QueryDeployParam>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying template task list, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取模板列表数据    
                QueryPageResult<HWESightTask> templateTaskList = esSession.DeployWorker.FindDeployTaskWithSync(webOneESightParam.Param);
                //4. 返回数据
                ret = templateTaskList;
                LogUtil.HWLogger.UI.InfoFormat("Querying template task list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data = null;
                ret.TotalPage = 0;
                ret.TotalSize = 0;
                LogUtil.HWLogger.UI.Error("Querying template task list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
                ret.TotalPage = 0;
                ret.TotalSize = 0;
                LogUtil.HWLogger.UI.Error("Querying template task list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 加载eSight
        /// </summary>
        private ApiListResult<HWESightHost> LoadESightList(object eventData)
        {
            return CommonBLLMethodHelper.LoadESightList();
        }

        /// <summary>
        /// 加载已上传的固件升级包列表
        /// </summary>
        private WebReturnLGResult<DeployTemplate> GetTemplateList(object eventData)
        {
            return CommonBLLMethodHelper.GetTemplateList(eventData);
        }

        /// <summary>
        /// 加载服务器列表
        /// </summary>
        private QueryPageResult<HWDevice> GetServerList(object eventData)
        {
            return CommonBLLMethodHelper.GetServerList(eventData);
        }

        /// <summary>
        /// 查询服务器详细信息
        /// </summary>
        /// <param name="eventData">JS传递的参数</param>
        /// <returns></returns>
        private WebReturnResult<QueryListResult<HWDeviceDetail>> GetDeviceDetail(object eventData)
        {
            return CommonBLLMethodHelper.GetDeviceDetail(eventData);
        }

        /// <summary>
        /// 删除模板任务
        /// </summary>
        private WebReturnResult<int> Delete(object eventData)
        {
            var ret = new WebReturnResult<int>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 获取UI传来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfDeleteTask>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Deleting template task..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 删除模板任务
                int deleteResult = esSession.DeployWorker.DeleteTask(webOneESightParam.Param.TaskId);
                //4. 返回数据
                ret.Code        = 0;
                ret.Description = "Deleting template task successful!";
                ret.Data        = deleteResult;
                LogUtil.HWLogger.UI.InfoFormat("Deleting template task successful! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting template task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = 0;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting template task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = 0;
            }
            return ret;
        }

        /// <summary>
        /// 添加模板任务
        /// </summary>
        private WebReturnResult<JObject> Save(object eventData)
        {
            var ret = new WebReturnResult<JObject>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 获取UI传来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfAddDeployTask>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving template task..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 保存
                DeployTask deployTask = new DeployTask();
                deployTask.DeviceDn   = webOneESightParam.Param.DeviceDn;
                deployTask.Templates  = webOneESightParam.Param.Templates;
                string taskName = esSession.DeployWorker.AddDeployTask(webOneESightParam.Param.TaskSourceName, deployTask);
                //4. 返回数据
                JObject taskObject = new JObject();
                taskObject.Add("taskName", taskName);
                ret.Code        = 0;
                ret.Description = "";
                ret.Data        = taskObject;
                LogUtil.HWLogger.UI.InfoFormat("Saving template task completed! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving template task failed: ", ex);
                ret.Code = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data = null;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving template task failed: ", ex);
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
            }
            return ret;
        }

        #region Inner class
        private class ParamOfAddDeployTask
        {
            [JsonProperty("deployTaskName")]
            public string TaskSourceName { get; set; }

            [JsonProperty(PropertyName = "templates")]
            public string Templates { get; set; }

            [JsonProperty(PropertyName = "dn")]
            public string DeviceDn { get; set; }
        }
        #endregion
    }
}
