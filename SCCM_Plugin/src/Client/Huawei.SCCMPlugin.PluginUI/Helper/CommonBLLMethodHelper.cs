using CommonUtil;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Deploy;
using Huawei.SCCMPlugin.Models.Devices;
using Huawei.SCCMPlugin.Models.Firmware;
using Huawei.SCCMPlugin.Models.Softwares;
using Huawei.SCCMPlugin.PluginUI.Entitys;
using Huawei.SCCMPlugin.RESTeSightLib;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Huawei.SCCMPlugin.PluginUI.Helper
{
    public sealed class CommonBLLMethodHelper
    {
        /// <summary>
        /// 加载eSight数据，用于UI eSight下拉框
        /// </summary>
        /// <returns></returns>
        public static ApiListResult<HWESightHost> LoadESightList()
        {
            var ret = new ApiListResult<HWESightHost>(false, ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, "", "", null);
            LogUtil.HWLogger.UI.InfoFormat("Loading eSight list...");
            ESightEngine.Instance.InitESSessions();   
            List<HWESightHost> hwESightHostList = ESSessionHelper.GethwESightHostList();
            if (hwESightHostList.Count == 0)
            {
                ret.Code = ConstMgr.ErrorCode.SYS_NO_ESIGHT;
                ret.Success = false;
                ret.Data = hwESightHostList;
            }
            else
            {
                ret.Code = "0";
                ret.Success = true;
                ret.Data = hwESightHostList;
            }
            LogUtil.HWLogger.UI.InfoFormat("Loading eSight list completed, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            return ret;
        }

        /// <summary>
        /// 加载已上传的固件升级包列表, 用于固件升级 UI
        /// </summary>
        public static WebReturnLGResult<BasePackageItem> GetFirmwareList(object eventData)
        {
            var ret = new WebReturnLGResult<BasePackageItem>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "" };
            try
            {
                //1. 解析js传过来的参数
                var jsData           = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOnlyPagingInfo>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage list, the param is [{0}]...", jsData);
                int pageSize               = webOneESightParam.Param.PageSize;
                int pageNo                 = webOneESightParam.Param.PageNo;
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取已上传的固件升级包列表数据    
                var basePackageItemList = esSession.BasePackageWorker.QueryBasePackagePage(pageNo, pageSize);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = basePackageItemList.Data;
                ret.TotalNum    = basePackageItemList.TotalNum;
                ret.Description = "";
                LogUtil.HWLogger.UI.InfoFormat("Querying basepackage list successful, the ret is [{0}]...", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying basepackage list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying basepackage list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 加载服务器列表, 用于服务器列表UI、添加升级任务UI、添加模板任务UI
        /// </summary>
        public static QueryPageResult<HWDevice> GetServerList(object eventData)
        {
            var ret = new QueryPageResult<HWDevice>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<DeviceParam>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying server list, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取服务器列表数据    
                var hwDevicePageResult = esSession.QueryHWPage(webOneESightParam.Param);
                //4. 返回数据
                ret = hwDevicePageResult;
                LogUtil.HWLogger.UI.InfoFormat("Querying server list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data = null;
                ret.TotalSize = 0;
                ret.TotalPage = 0;
                LogUtil.HWLogger.UI.Error("Querying server list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data = null;
                ret.TotalSize = 0;
                ret.TotalPage = 0;
                LogUtil.HWLogger.UI.Error("Querying server list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 查询服务器详细信息
        /// </summary>
        /// <param name="eventData">JS传递的参数</param>
        /// <returns></returns>
        public static WebReturnResult<QueryListResult<HWDeviceDetail>> GetDeviceDetail(object eventData)
        {
            var ret = new WebReturnResult<QueryListResult<HWDeviceDetail>>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData            = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<ParamOfQueryDeviceDetail>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying server detail, the param is [{0}]", jsData);
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取升级任务详情    
                string dn = webOneESightParam.DN;
                var hwDeviceDetailList = esSession.DviWorker.QueryForDeviceDetail(dn);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = hwDeviceDetailList;
                ret.Description = "Succeeded in querying server details.";
                LogUtil.HWLogger.UI.InfoFormat("Querying server detail successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying server detail failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                LogUtil.HWLogger.UI.Error("Querying server detail failed: ", ex);
            }
            return ret;
        }
        
        /// <summary>
        /// 获取已上传软件源列表，用于软件源管理UI，添加OS模板中的查询软件源UI
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public static WebReturnLGResult<SourceItem> GetSoftwareList(object eventData)
        {
            var ret = new WebReturnLGResult<SourceItem>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR), Description = "" };
            try
            {
                //1. 解析js传过来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamOnlyPagingInfo>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying software list, the param is [{0}]", jsData);
                int pageSize               = webOneESightParam.Param.PageSize;
                int pageNo                 = webOneESightParam.Param.PageNo;
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取软件源列表数据    
                var softwareSourceList = esSession.SoftSourceWorker.QuerySoftwarePage(pageNo, pageSize);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = softwareSourceList.Data;
                ret.TotalNum    = softwareSourceList.TotalNum;
                ret.Description = "";

                LogUtil.HWLogger.UI.InfoFormat("Querying software list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying software list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying software list failed: ", ex);
            }
            return ret;
        }
        
        /// <summary>
        /// 加载模板列表, 用于模板列表UI、添加模板任务UI
        /// </summary>
        public static WebReturnLGResult<DeployTemplate> GetTemplateList(object eventData)
        {
            var ret = new WebReturnLGResult<DeployTemplate>() { Code = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR) };
            try
            {
                //1. 解析js传过来的参数
                var jsData = JsonUtil.SerializeObject(eventData);
                var webOneESightParam = JsonUtil.DeserializeObject<WebOneESightParam<ParamPagingOfQueryTemplate>>(jsData);
                LogUtil.HWLogger.UI.InfoFormat("Querying template list, the param is [{0}]", jsData);
                int pageNo = -1;
                int pageSize = int.MaxValue;
                string templateType = "";
                if (webOneESightParam.Param != null)
                {
                    pageSize     = webOneESightParam.Param.PageSize;
                    pageNo       = webOneESightParam.Param.PageNo;
                    templateType = webOneESightParam.Param.TemplateType;
                }
                
                //2. 根据HostIP获取IESSession
                IESSession esSession = ESSessionHelper.GetESSession(webOneESightParam.ESightIP);
                //3. 获取模板列表数据    
                var deployTemplate = esSession.DeployWorker.QueryTemplatePage(pageNo, pageSize, templateType);
                //4. 返回数据
                ret.Code        = 0;
                ret.Data        = deployTemplate.Data;
                ret.TotalNum    = deployTemplate.TotalNum;
                ret.Description = "";
                LogUtil.HWLogger.UI.InfoFormat("Querying template list successful, the ret is [{0}]", JsonUtil.SerializeObject(ret));
            }
            catch (BaseException ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ex.Code);
                ret.ErrorModel  = ex.ErrorModel;
                ret.Description = ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying template list failed: ", ex);
            }
            catch (Exception ex)
            {
                ret.Code        = CoreUtil.GetObjTranNull<int>(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR);
                ret.Description = ex.InnerException.Message ?? ex.Message;
                ret.Data        = null;
                ret.TotalNum    = 0;
                LogUtil.HWLogger.UI.Error("Querying template list failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 隐藏Json字符串中的密码
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static string HidePassword(string jsonData)
        {
            string newJsonData = null;
            string replacement = "\"${str}\":\"********\"";
            string pattern1 = "\"(?<str>([A-Za-z0-9_]*)password)\":\"(.*?)\"";
            newJsonData = Regex.Replace(jsonData, pattern1, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            string pattern2 = "\"(?<str>([A-Za-z0-9_]*)pwd)\":\"(.*?)\"";
            newJsonData = Regex.Replace(newJsonData, pattern2, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            return newJsonData;
        }
    }
}
