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

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// 升级包管理
    /// </summary>
    [CefURL("huawei/firmware/listFirmware.html")]
    [Bound("NetBound")]
    public class BasepackageHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Basepackage page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getList":
                        result = GetList(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    case "delete":
                        result = Delete(eventData);
                        break;
                    case "save":
                        result = Save(eventData);
                        break;
                    case "getFirmwareList":
                        result = GetFirmwareList(eventData);
                        break;
                    case "getFirmwareDetails":
                        result = GetFirmwareDetails(eventData);
                        break;
                    case "deleteBasepackage":
                        result = DeleteBasepackage(eventData);
                        break;
                    default: break;
                }
                return JsonConvert.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Basepackage page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Basepackage page fail.", ex);;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Basepackage page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载升级包任务列表
        /// </summary>
        private ApiListResult<HWESightTask> GetList(object eventData)
        {
            var ret = new ApiListResult<HWESightTask>(false, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", "", null);
            try
            {
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage task list...");
                //1.
                ESightEngine.Instance.InitESSessions();   
                //2. 获取升级包任务数据    
                IList<HWESightTask> hwESightTaskList = ESightEngine.Instance.FindAllBasePackageTaskWithSync();
                //3. 组织数据
                ret.Code    = "0";
                ret.Success = true;
                ret.Data    = hwESightTaskList;
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage task list successful, the ret is: [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = null;
                LogUtil.HWLogger.UI.Error("Querying basepackage task list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
                LogUtil.HWLogger.UI.Error("Querying basepackage task list failed: ", ex);
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
        /// 加载固件升级包明细列表
        /// </summary>
        private ApiResult<BasePackageDetail> GetFirmwareDetails(object eventData)
        {
            var ret = new ApiResult<BasePackageDetail>(false, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", "", null);
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfQueryBasePackageDetail>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage detail, the param is: [{0}] [{1}]", webOneESightParam.ESightIP, webOneESightParam.Param.BasepackageName);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取固件升级包明细数据    
                QueryObjectResult<BasePackageDetail> basePackageDetail = esSession.BasePackageWorker.QueryBasePackageDetail(webOneESightParam.Param.BasepackageName);
                //4. 组织数据
                ret.Code    = basePackageDetail.Code.ToString();
                ret.Success = basePackageDetail.Code == 0 ? true : false;
                ret.Data    = basePackageDetail.Data;
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage detail successful, the ret is: [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = null;
                LogUtil.HWLogger.UI.Error("Querying basepackage detail failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
                LogUtil.HWLogger.UI.Error("Querying basepackage detail failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 删除失败的升级包任务
        /// </summary>
        private ApiResult<int> Delete(object eventData)
        {
            var ret = new ApiResult<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                LogUtil.HWLogger.UI.Info("Deleting failed basepackage task...");
                var deleteResult = ESightEngine.Instance.ClearAllFailedPackageTask();

                if (deleteResult == 0)
                {
                    ret.Code    = ConstMgr.ErrorCode.SYS_NO_FAILEDTASK;
                    ret.Success = false;
                    ret.Msg     = "Now no failed basepackage task!";
                    ret.Data    = deleteResult;
                }
                else
                {
                    ret.Code    = "0";
                    ret.Success = true;
                    ret.Msg     = "Deleting failed basepackage task successful!";
                    ret.Data    = deleteResult;
                }
                
                LogUtil.HWLogger.UI.InfoFormat("Deleting failed basepackage task successful! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting failed basepackage task failed: ", ex);
                ret.Code         = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success      = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = 0;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting failed basepackage task failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
                ret.Data = 0;
            }
            return ret;
        }

        /// <summary>
        /// 删除已上传的升级包
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        private WebReturnResult<int> DeleteBasepackage(object eventData)
        {
            var ret = new WebReturnResult<int>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfQueryBasePackageDetail>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Deleting basepackage..., the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取升级任务详情    
                string basepackageName = webOneESightParam.Param.BasepackageName;
                esSession.BasePackageWorker.DeleteBasePackage(basepackageName);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = 1;
                ret.Description = "delete FMBasePackage File success.";
                LogUtil.HWLogger.UI.InfoFormat("Deleting basepackage successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = 0;
                LogUtil.HWLogger.UI.Error("Deleting basepackage failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = 0;
                LogUtil.HWLogger.UI.Error("Deleting basepackage failed: ", ex);
            }
            return ret;
        }

        #region Save
        /// <summary>
        /// 添加升级包
        /// </summary>
        private ApiListResult<WebReturnESightResult> Save(object eventData)
        {
            var ret = new ApiListResult<WebReturnESightResult>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                //1. 获取UI传来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving basepackage..., the param is [{0}]", newJsData);
                WebMutilESightsParam<BasePackage> webMutilESightsParam = JsonUtil.DeserializeObject<WebMutilESightsParam<BasePackage>>(jsData);
                //2. 根据HostIP获取IESSession
                IList<WebReturnESightResult> webReturnESightResultList = new List<WebReturnESightResult>();
                int eSightCount  = webMutilESightsParam.ESights.Count;
                int errorCount   = 0;
                string errorCode = "";
                foreach (string hostIP in webMutilESightsParam.ESights)
                {
                    IESSession esSession = null;
                    //3. 保存
                    WebReturnESightResult webReturnESightResult = new WebReturnESightResult();
                    try
                    {
                        esSession = ESSessionHelper.GetESSession(hostIP);
                        esSession.BasePackageWorker.UploadBasePackage(webMutilESightsParam.Data);

                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = 0;
                        webReturnESightResult.Description = "successful";
                        LogUtil.HWLogger.UI.InfoFormat("Saving basepackage successful on eSight [{0}]", webReturnESightResult.ESightIp);
                    }
                    catch (BaseException ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ex.Code);
                        webReturnESightResult.ErrorModel  = ex.ErrorModel;
                        webReturnESightResult.Description = ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ex.Code, ex.ErrorModel, ref errorCode);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving basepackage failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    catch (Exception ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                        webReturnESightResult.Description = ex.InnerException.Message ?? ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", ref errorCode);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving basepackage failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    webReturnESightResultList.Add(webReturnESightResult);
                }
                SetSaveResultWhenComplete(errorCount, eSightCount, errorCode, ref ret);
                ret.Data = webReturnESightResultList;
                LogUtil.HWLogger.UI.InfoFormat("Saving basepackage completed! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving basepackage failed: ", ex);
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = null;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving basepackage failed: ", ex);
                ret.Code = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
            }
            return ret;
        }

        private void SetErrorCode(int errorCount, string innerErrorCode, string innerErrorModel, ref string errorCode)
        {
            if (errorCount == 1)
            {
                errorCode = string.Format("{0}{1}", innerErrorModel, innerErrorCode);
            }
        }

        private void SetSaveResultWhenComplete(int errorCount, int eSightCount, string errorCode, ref ApiListResult<WebReturnESightResult> ret)
        {
            if (errorCount == 0)
            {
                ret.Code = "0";
                ret.Success = true;
            }
            else if (errorCount == eSightCount)
            {
                ret.Code = errorCode;
                ret.Success = false;
            }
            else
            {
                ret.Code = ConstMgr.ErrorCode.SYS_UNKNOWN_PART_ERR;
                ret.Success = false;
            }
        }
        #endregion

        #region Inner class
        private class ParamOfQueryBasePackageDetail
        {
            [JsonProperty("basepackageName")]
            public string BasepackageName { get; set; }
        }
        #endregion
    }
}
