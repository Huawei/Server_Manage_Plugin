using CommonUtil;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Devices;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers
{
    /// <summary>
    /// 服务器 业务类
    /// </summary>
    public class DeviceWorker : IDeviceWorker
    {
        string errorPix = "device.error.";

        /// <summary>
        /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
        /// </summary>
        public IESSession ESSession
        {
            get; set;
        }
        private void CheckAndThrowException(JObject jsonData, string errorTip = "")
        {
            int retCode = 0;
            retCode = JsonUtil.GetJObjectPropVal<int>(jsonData, "code");
            if (retCode != 0)
            {
                throw new DeviceExpceion(errorPix,retCode.ToString(), this, errorTip + JsonUtil.GetJObjectPropVal<string>(jsonData, "description"));
            }
        }
        /// <summary>
        /// 查询服务器详情
        /// </summary>
        /// <param name="dn">设备DN</param>
        /// <returns>详细信息</returns>
        public QueryListResult<HWDeviceDetail> QueryForDeviceDetail(string dn)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DEVICEDETAIL);
            sb.Append("?dn=").Append(HttpUtility.UrlEncode(dn, Encoding.UTF8));
            JObject jResult = ESSession.HCGet(sb.ToString(),false);
            CheckAndThrowException(jResult);
            QueryListResult<HWDeviceDetail> queryListResult = jResult.ToObject<QueryListResult<HWDeviceDetail>>();
            //HWDeviceDetail hwDeviceDetail = queryListResult.Data[0];
            return queryListResult;
        }

    }
}
