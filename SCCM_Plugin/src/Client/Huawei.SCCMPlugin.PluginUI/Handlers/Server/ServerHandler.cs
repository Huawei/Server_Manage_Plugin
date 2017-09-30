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
using Huawei.SCCMPlugin.Models.Devices;

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
    /// <summary>
    /// 服务器管理
    /// </summary>
    [CefURL("huawei/serverList/serverList.html")]
    [Bound("NetBound")]
    public class ServerHandler : IWebHandler
    {
        public string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Server page...");
                object result = new ApiResult(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "getList":
                        result = GetList(eventData);
                        break;
                    case "getDeviceDetail":
                        result = GetDeviceDetail(eventData);
                        break;
                    case "loadESightList":
                        result = LoadESightList(eventData);
                        break;
                    default: break;
                }
                return JsonConvert.SerializeObject(result);
            }
            catch(JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Call JsonConvert.SerializeObject failed.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Server page failed.", ex);
            }
            return "";
        }

        /// <summary>
        /// 加载服务器列表
        /// </summary>
        private QueryPageResult<HWDevice> GetList(object eventData)
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
        /// 加载eSight
        /// </summary>
        private ApiListResult<HWESightHost> LoadESightList(object eventData)
        {
            return CommonBLLMethodHelper.LoadESightList();
        }
    }
}
