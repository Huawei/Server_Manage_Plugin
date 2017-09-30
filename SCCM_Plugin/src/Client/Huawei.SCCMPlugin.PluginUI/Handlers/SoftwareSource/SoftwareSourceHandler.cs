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

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// 软件源管理
    /// </summary>
    [CefURL("huawei/softwareManager/softManager.html")]
    [Bound("NetBound")]
    public class SoftwareSourceHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Software Source page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getSoftwareSourceTaskList":
                        result = GetSoftwareSourceTaskList(eventData);
                        break;
                    case "getSoftwareSourceList":
                        result = GetSoftwareSourceList(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    case "delete":
                        result = Delete(eventData);
                        break;
                    case "deleteTask":
                        result = DeleteTask(eventData);
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
                LogUtil.HWLogger.UI.Error("Execute the method in Software Source page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Software Source page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Software Source page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载软件源任务列表
        /// </summary>
        private WebReturnResult<List<HWESightTask>> GetSoftwareSourceTaskList(object eventData)
        {
            var ret = new WebReturnResult<List<HWESightTask>>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "" };
            try
            {
                LogUtil.HWLogger.UI.InfoFormat("Querying software source task list...");
                //1.
                ESightEngine.Instance.InitESSessions();   
                //2. 获取软件源任务数据 
                List<HWESightTask> hwESightTaskList = new List<HWESightTask>(ESightEngine.Instance.FindAllSoftwareSourceTaskWithSync());
                //3. 组织数据
                ret.Code        = 0;
                ret.Data        = hwESightTaskList;
                ret.Description = "";
                LogUtil.HWLogger.UI.InfoFormat("Querying software source task list successful, the ret is: [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying software source task list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying software source task list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 加载软件源列表
        /// </summary>
        private WebReturnLGResult<SourceItem> GetSoftwareSourceList(object eventData)
        {
            return CommonBLLMethodHelper.GetSoftwareList(eventData);
        }

        /// <summary>
        /// 加载eSight
        /// </summary>
        private ApiListResult<HWESightHost> LoadESightList(object eventData)
        {
            return CommonBLLMethodHelper.LoadESightList();
        }
        
        /// <summary>
        /// 删除软件源
        /// </summary>
        private WebReturnResult<int> Delete(object eventData)
        {
            var ret = new WebReturnResult<int>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "", Data = 0};
            try
            {
                //1. 获取UI传来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<string>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Deleting software source..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 删除模板
                esSession.SoftSourceWorker.DeleteSoftwareSource(webOneESightParam.Param);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = 1;
                ret.Description = "";
                LogUtil.HWLogger.UI.InfoFormat("Deleting software source successful! the ret is: [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting software source failed: ", ex);
                ret.Code         = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel   = ex.ErrorModel;
                ret.Data         = 0;
                ret.Description  = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting software source failed: ", ex);
                ret.Code         = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Data         = 0;
                ret.Description  = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        }

        /// <summary>
        /// 删除失败的软件源任务
        /// </summary>
        private WebReturnResult<int> DeleteTask(object eventData)
        {
            var ret = new WebReturnResult<int>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "", Data = 0};
            try
            {
                LogUtil.HWLogger.UI.Info("Deleting failed software source task...");
                int deleteTaskResult = ESightEngine.Instance.ClearAllFailedSoftwareSourceTask();

                ret.Code        = 0;
                ret.Data        = deleteTaskResult;
                ret.Description = "Deleting failed software source task successful!";
                LogUtil.HWLogger.UI.InfoFormat("Deleting failed software source task successful! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting failed software source task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Data        = 0;
                ret.Description = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting failed software source task failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Data        = 0;
                ret.Description = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        }

        #region 保存软件源
        /// <summary>
        /// 保存软件源
        /// </summary>
        private WebReturnResult<List<WebReturnESightResult>> Save(object eventData)
        {
            var ret = new WebReturnResult<List<WebReturnESightResult>>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "" };
            try
            {
                //1. 获取UI传来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving software source..., the param is [{0}]", newJsData);
                var webMutilESightsParam = JsonUtil.DeserializeObject<WebMutilESightsParam<SoftwareSource>>(jsData);
                //2. 根据HostIP获取IESSession
                List<WebReturnESightResult> webReturnESightResultList = new List<WebReturnESightResult>();
                int eSightCount   = webMutilESightsParam.ESights.Count;
                int errorCount    = 0;
                int errorCode     = 0;
                string errorModel = "";
                foreach (string hostIP in webMutilESightsParam.ESights)
                {
                    IESSession esSession = null;
                    //3. 保存软件源
                    WebReturnESightResult webReturnESightResult = new WebReturnESightResult();
                    try
                    {
                        esSession = ESSessionHelper.GetESSession(hostIP);
                        esSession.SoftSourceWorker.UploadSoftwareSource(webMutilESightsParam.Data);

                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = 0;
                        webReturnESightResult.Description = "successful";
                        LogUtil.HWLogger.UI.InfoFormat("Saving software source successful on eSight [{0}]", webReturnESightResult.ESightIp);
                    }
                    catch (BaseException ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ex.Code);
                        webReturnESightResult.ErrorModel  = ex.ErrorModel;
                        webReturnESightResult.Description = ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ex.Code, ex.ErrorModel, ref errorCode, ref errorModel);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving software source failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    catch (Exception ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                        webReturnESightResult.Description = ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", ref errorCode, ref errorModel);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving software source failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    webReturnESightResultList.Add(webReturnESightResult);
                }
                SetSaveResultWhenComplete(errorCount, eSightCount, errorCode, errorModel, ref ret);
                ret.Description = "";
                ret.Data        = webReturnESightResultList;
                LogUtil.HWLogger.UI.InfoFormat("Saving software source completed! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving software source failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving software source failed: ", ex);
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
            }
            return ret;
        }

        private void SetErrorCode(int errorCount, string innerErrorCode, string innerErrorModel, ref int errorCode, ref string errorModel)
        {
            if (errorCount == 1)
            {
                errorCode = CoreUtil.GetObjTranNull<int>(innerErrorCode);
                errorModel = innerErrorModel;
            }
        }

        private void SetSaveResultWhenComplete(int errorCount, int eSightCount, int errorCode, string errorModel, ref WebReturnResult<List<WebReturnESightResult>> ret)
        {
            if (errorCount == 0)
            {
                ret.Code = 0;
            }
            else if (errorCount == eSightCount)
            {
                ret.Code = errorCode;
            }
            else
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_PART_ERR);
            }
            ret.ErrorModel = errorModel;
        }
        #endregion

    }
}
