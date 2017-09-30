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
using Huawei.SCCMPlugin.Models.Firmware;
using Huawei.SCCMPlugin.Models.Devices;

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// 升级任务管理
    /// </summary>
    [CefURL("huawei/firmware/listFirmwareTask.html")]
    [Bound("NetBound")]
    public class FirmwareTaskHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Firmware Task page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getTaskList":
                        result = GetTaskList(eventData);
                        break;
                    case "getTaskDetail":
                        result = GetTaskDetail(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    case "getFirmwareList":
                        result = GetFirmwareList(eventData);
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
                LogUtil.HWLogger.UI.Error("Execute the method in Firmware Task page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Firmware Task page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Firmware Task page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载升级任务列表
        /// </summary>
        private QueryPageResult<HWESightTask> GetTaskList(object eventData)
        {
            var ret = new QueryPageResult<HWESightTask>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<QueryDeployPackageParam>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying firmware task list, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取升级任务列表数据    
                var firmwareTaskList = esSession.BasePackageWorker.FindDeployTaskWithSync(webOneESightParam.Param);
                //4. 返回数据
                ret = firmwareTaskList;
                LogUtil.HWLogger.UI.InfoFormat("Querying firmware task list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data = null;
                ret.TotalSize = 0;
                ret.TotalPage = 0;
                LogUtil.HWLogger.UI.Error("Querying firmware task list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
                ret.TotalSize = 0;
                ret.TotalPage = 0;
                LogUtil.HWLogger.UI.Error("Querying firmware task list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 查询升级任务详情
        /// </summary>
        /// <param name="eventData">JS参数</param>
        /// <returns></returns>
        private WebReturnResult<BasePackageDNProgress> GetTaskDetail(object eventData)
        {
            var ret = new WebReturnResult<BasePackageDNProgress>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfQueryFirmewareDetail>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying firmware task detail, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取升级任务详情    
                string taskName = webOneESightParam.Param.TaskName;
                string dn = webOneESightParam.Param.DN;
                var firmwareTaskDetail = esSession.BasePackageWorker.QueryDeployTaskDNProcess(taskName, dn);
                //4. 返回数据
                ret.Code        = firmwareTaskDetail.Code;
                ret.Data        = firmwareTaskDetail.Data;
                ret.Description = firmwareTaskDetail.Description;
                LogUtil.HWLogger.UI.InfoFormat("Querying firmware task detail successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying firmware task detail failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying firmware task detail failed: ", ex);
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
        private WebReturnLGResult<BasePackageItem> GetFirmwareList(object eventData)
        {
            return CommonBLLMethodHelper.GetFirmwareList(eventData);
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
        /// 删除升级任务
        /// </summary>
        private WebReturnResult<int> Delete(object eventData)
        {
            var ret = new WebReturnResult<int>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 获取UI传来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfDeleteTask>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Deleting firmware task..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 删除升级任务
                int deleteResult = esSession.BasePackageWorker.DeleteTask(webOneESightParam.Param.TaskId);
                //4. 返回数据
                ret.Code        = 0;
                ret.Description = "Deleting firmware task successful!";
                ret.Data        = deleteResult;
                LogUtil.HWLogger.UI.InfoFormat("Deleting firmware task successful! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting firmware task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = 0;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting firmware task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = 0;
            }
            return ret;
        }

        /// <summary>
        /// 添加升级任务
        /// </summary>
        private WebReturnResult<JObject> Save(object eventData)
        {
            var ret = new WebReturnResult<JObject>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 获取UI传来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<DeployPackageTask>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving firmware task..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 保存
                string taskName = esSession.BasePackageWorker.AddDeployTask(webOneESightParam.Param);
                //4. 返回数据
                JObject taskObject = new JObject();
                taskObject.Add("taskName", taskName);
                ret.Code        = 0;
                ret.Description = "";
                ret.Data        = taskObject;
                LogUtil.HWLogger.UI.InfoFormat("Saving firmware task completed! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving firmware task failed: ", ex);
                ret.Code = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data = null;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving firmware task failed: ", ex);
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
            }
            return ret;
        }
    }
}
