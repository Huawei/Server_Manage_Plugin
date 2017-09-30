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
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.Const;
using CommonUtil;

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// eSight配置
    /// </summary>
    [CefURL("huawei/eSight/eSightConfig.html")]
    [Bound("NetBound")]
    public class ESightsettingHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of ESightsetting page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getList":
                        result = GetList(eventData);
                        break;
                    case "test":
                        result = Test(eventData);
                        break;
                    case "save":
                        result = Save(eventData);
                        break;
                    case "saveNoCert":
                        result = SaveNoCert(eventData);
                        break;
                    case "delete":
                        result = Delete(eventData);
                        break;
                    default: break;
                }
                return JsonUtil.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in ESightsetting page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in ESightsetting page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in ESightsetting page fail.", ex);
            }
            return "";
        }

        /// <summary>
        /// 获取eSight列表数据
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        private WebReturnLGResult<HWESightHost> GetList(object eventData)
        {
            var ret = new WebReturnLGResult<HWESightHost>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "" };
            try
            {
                //1. 处理参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamPagingOfQueryESight>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying eSight list, the param is [{0}]", jsData);
                int pageSize          = webOneESightParam.Param.PageSize;
                int pageNo            = webOneESightParam.Param.PageNo;
                string hostIp         = webOneESightParam.Param.HostIp;
                //2. 获取数据 
                ESightEngine.Instance.InitESSessions();   
                List<HWESightHost> hwESightHostList = ESSessionHelper.GethwESightHostList();
                var filterList = hwESightHostList.Where(x => x.HostIP.Contains(hostIp)).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                //3. 返回数据
                ret.Code        = 0;
                ret.Data        = filterList;
                ret.TotalNum    = hwESightHostList.Count();
                ret.Description = "";

                LogUtil.HWLogger.UI.InfoFormat("Querying eSight list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying eSight list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying eSight list failed: ", ex);
            }
            return ret;
        }

        private ApiResult Test(object eventData)
        {
            ApiResult ret = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                var jsData = JsonUtil.SerializeObject(eventData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Testing eSight connect..., the param is [{0}]", newJsData);

                HWESightHost hwESightHost = GetESightConfigFromUI(jsData);
                string testResult = ESightEngine.Instance.TestESSession(hwESightHost.HostIP, hwESightHost.HostPort, hwESightHost.LoginAccount, hwESightHost.LoginPwd);
                if (string.IsNullOrEmpty(testResult))
                {
                    LogUtil.HWLogger.UI.Info("Testing eSight connect successful!");
                    ret.Code    = "0";
                    ret.Success = true;
                    ret.Msg     = "Testing eSight connect successful!";
                }
                else
                {
                    LogUtil.HWLogger.UI.Info("Testing eSight connect failed!");
                    ret.Code         = testResult;
                    ret.Success      = false;
                    ret.ExceptionMsg = "Testing eSight connect failed!";
                }
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Testing eSight connect failed: ", ex);
                ret.Success = false;
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.ExceptionMsg = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Testing eSight connect failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.Msg          = "Testing eSight connect failed!";
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        } 

        private ApiResult Save(object eventData)
        {
            ApiResult ret = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                var jsData = JsonUtil.SerializeObject(eventData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving eSight configuration..., the param is [{0}]", newJsData);
                HWESightHost hwESightHost = GetESightConfigFromUI(jsData);
                var eSSession = ESightEngine.Instance.SaveNewESSession(hwESightHost.HostIP, hwESightHost.AliasName, hwESightHost.HostPort, hwESightHost.LoginAccount, hwESightHost.LoginPwd);
                LogUtil.HWLogger.UI.Info("Saving eSight configuration successful!");

                ret.Code    = "0";
                ret.Success = true;
                ret.Msg     = "Saving eSight configuration successful!";
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving eSight configuration failed: ", ex);
                ret.Code         = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success      = false;
                ret.ExceptionMsg = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving eSight configuration failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        } 

        private ApiResult SaveNoCert(object eventData)
        {
            ApiResult ret = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                var jsData = JsonUtil.SerializeObject(eventData);
                var newJsData = CommonBLLMethodHelper.HidePassword(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Saving eSight configuration(No Cert)..., the param is [{0}]", newJsData);

                HWESightHost hwESightHost = GetESightConfigFromUI(jsData);
                var eSSession = ESightEngine.Instance.SaveESSession(hwESightHost.HostIP, hwESightHost.AliasName, hwESightHost.HostPort);
                LogUtil.HWLogger.UI.Info("Saving eSight configuration(No Cert) successful!");

                ret.Code    = "0";
                ret.Success = true;
                ret.Msg     = "Saving eSight configuration successful!";
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Saving eSight configuration failed: ", ex);
                ret.Code         = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success      = false;
                ret.ExceptionMsg = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Saving eSight configuration failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        } 

        private HWESightHost GetESightConfigFromUI(string jsData)
        {
            var hwESightHost = JsonUtil.DeserializeObject<HWESightHost>(jsData);
            if (!string.IsNullOrWhiteSpace(hwESightHost.LoginPwd))
            {
                string loginPwd       = hwESightHost.LoginPwd;
                hwESightHost.LoginPwd = EncryptUtil.EncryptPwd(loginPwd);
            }
            return hwESightHost;
        }

        private ApiResult Delete(object eventData)
        {
            ApiResult ret = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "");
            try
            {
                var jsData = JsonUtil.SerializeObject(eventData);
                LogUtil.HWLogger.UI.InfoFormat("Deleting eSight configuration..., the param is [{0}]", jsData);
                var deleteParam = JsonUtil.DeserializeObject<ParamOfDelete>(jsData);
                ESightEngine.Instance.RemoveESSession(deleteParam.ESightIDList);
                LogUtil.HWLogger.UI.Info("Deleting eSight configuration successful!");

                ret.Code    = "0";
                ret.Success = true;
                ret.Msg     = "Deleting eSight configuration successful!";
            }
            catch (BaseException ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting eSight configuration failed: ", ex);
                ret.Code         = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success      = false;
                ret.ExceptionMsg = ex.Message;
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Deleting eSight configuration failed: ", ex);
                ret.Code         = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success      = false;
                ret.ExceptionMsg = ex.InnerException.Message ?? ex.Message;
            }
            return ret;
        }

        #region Inner class
        private class ParamOfDelete
        {
            [JsonProperty("ids")]
            public int[] ESightIDList { get; set; }
        }
        #endregion
    }
}
