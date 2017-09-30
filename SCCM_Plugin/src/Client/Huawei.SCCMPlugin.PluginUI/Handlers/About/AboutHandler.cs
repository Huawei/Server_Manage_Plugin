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
using System.Reflection;

namespace Huawei.SCCMPlugin.PluginUI.Handlers.About
{
    /// <summary>
    /// 关于
    /// </summary>
    [CefURL("huawei/about/about.html")]
    [Bound("NetBound")]
    public class AboutHandler: IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of About page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getVersion":
                        result = GetVersion(eventData);
                        break;
                    default: break;
                }
                return JsonUtil.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in About page fail.", ex);
            }
            catch(NullReferenceException ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in About page fail.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in About page fail.", ex);
            }
            return "";
        }

        private ApiResult<string> GetVersion(object eventData)
        {
            string version = "1.0.0.0";
            var ret = new ApiResult<string>(false, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", "", version);
            try
            {
                //1. 获取数据 
                string fileVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] arrVersion = fileVersion.Split('.');
                if (arrVersion.Length >= 3)
                {
                    version = string.Format("{0}.{1}.{2}", arrVersion[0], arrVersion[1], arrVersion[2]);
                }
                else
                {
                    version = fileVersion;
                }
                //2. 返回数据
                ret.Code = "0";
                ret.Success = true;
                ret.Data = version;

                LogUtil.HWLogger.UI.InfoFormat("Get version successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = string.Format("{0}{1}", ex.ErrorModel, ex.Code);
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = version;
                LogUtil.HWLogger.UI.Error("Get version failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                ret.Success = false;
                ret.ExceptionMsg = ex.Message;
                ret.Data = version;
                LogUtil.HWLogger.UI.Error("Get version failed: ", ex);
            }
            return ret;
        }
    }
}
