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
    /// 模板管理
    /// </summary>
    [CefURL("huawei/template/list.html")]
    [Bound("NetBound")]
    public class TemplateListHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Template List page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getList":
                        result = GetList(eventData);
                        break;
                    case "getTemplateDetail":
                        result = GetTemplateDetail(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    case "delete":
                        result = Delete(eventData);
                        break;
                    case "saveTemplate":
                        result = SaveTemplate(eventData);
                        break;
                    case "getSoftwareList":
                        result = GetSoftwareList(eventData);
                        break;
                    default: break;
                }
                return JsonConvert.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template List page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template List page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Template List page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载模板列表
        /// </summary>
        private WebReturnLGResult<DeployTemplate> GetList(object eventData)
        {
            return CommonBLLMethodHelper.GetTemplateList(eventData);
        }

        private WebReturnResult<DeployTemplate> GetTemplateDetail(object eventData)
        {
            var ret = new WebReturnResult<DeployTemplate>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOfQueryTemplateDetail>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying template detail, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取升级任务详情    
                string templateName = webOneESightParam.Param.TemplateName;
                var templateDetail = esSession.DeployWorker.QueryDeployTemplate(templateName);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = templateDetail;
                ret.Description = "Operation success.";
                LogUtil.HWLogger.UI.InfoFormat("Querying template detail successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying template detail failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying template detail failed: ", ex);
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
        /// 删除模板
        /// </summary>
        private ApiResult Delete(object eventData)
        {
            ApiResult ret = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                LogUtil.HWLogger.UI.InfoFormat("Deleting template..., the param is [{0}]", JsonUtil.SerializeObject(eventData));
                //1. 获取UI传来的参数
                var jsData          = eventData as Dictionary<string, object>;
                string hostIP       = DictHelper.GetDicValue(jsData, "hostIP");
                string templateName = DictHelper.GetDicValue(jsData, "templateName");
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(hostIP);
                //3. 删除模板
                esSession.DeployWorker.DelDeployTemplate(templateName);
                LogUtil.HWLogger.UI.Info("Deleting template successful!");

                ret.Code    = "0";
                ret.Success = true;
                ret.Msg     = "Deleting template successful!";
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting template failed: ", ex);
                ret.Code         = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success      = false;
                ret.ExceptionMsg = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting template failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        }

        #region 保存模板
        /// <summary>
        /// 保存模板
        /// </summary>
        private ApiListResult<WebReturnESightResult> SaveTemplate(object eventData)
        {
            var ret = new ApiListResult<WebReturnESightResult>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                //1. 获取UI传来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                WebMutilESightsParam<DeployTemplate> webMutilESightsParam = JsonUtil.DeserializeObject<WebMutilESightsParam<DeployTemplate>>(jsData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving [{0}] template..., the param is [{1}]", webMutilESightsParam.Data.TemplateType, newJsData);
                //2. 根据HostIP获取IESSession
                IList<WebReturnESightResult> webReturnESightResultList = new List<WebReturnESightResult>();
                int eSightCount  = webMutilESightsParam.ESights.Count;
                int errorCount   = 0;
                string errorCode = "";
                foreach (string hostIP in webMutilESightsParam.ESights)
                {
                    IESSession esSession = null;
                    //3. 保存上下电模板
                    WebReturnESightResult webReturnESightResult = new WebReturnESightResult();
                    try
                    {
                        esSession = ESSessionHelper.GetESSession(hostIP);
                        esSession.DeployWorker.AddDeployTemplate(webMutilESightsParam.Data);

                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = 0;
                        webReturnESightResult.Description = "successful";
                        LogUtil.HWLogger.UI.InfoFormat("Saving template successful on eSight [{0}]", webReturnESightResult.ESightIp);
                    }
                    catch (BaseException ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ex.Code);
                        webReturnESightResult.ErrorModel  = ex.ErrorModel;
                        webReturnESightResult.Description = ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ex.Code, ex.ErrorModel, ref errorCode);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving template failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    catch (Exception ex)
                    {
                        webReturnESightResult.ESightIp    = hostIP;
                        webReturnESightResult.Code        = CommonUtil.CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                        webReturnESightResult.Description = ex.InnerException.Message ?? ex.Message;
                        errorCount++;
                        SetErrorCode(errorCount, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", ref errorCode);
                        LogUtil.HWLogger.UI.Error(string.Format("Saving template failed on eSight [{0}]: ", webReturnESightResult.ESightIp), ex);
                    }
                    webReturnESightResultList.Add(webReturnESightResult);
                }
                SetSaveResultWhenComplete(errorCount, eSightCount, errorCode, ref ret);
                ret.Data = webReturnESightResultList;
                LogUtil.HWLogger.UI.InfoFormat("Saving template completed! the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving template failed: ", ex);
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = null;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving template failed: ", ex);
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

        private WebReturnLGResult<SourceItem> GetSoftwareList(object eventData)
        {
            return CommonBLLMethodHelper.GetSoftwareList(eventData);
        }

        #region Inner class
        private class ParamOfQueryTemplateDetail
        {
            [JsonProperty("templateName")]
            public string TemplateName { get; set; }
        }
        #endregion
    }
}
