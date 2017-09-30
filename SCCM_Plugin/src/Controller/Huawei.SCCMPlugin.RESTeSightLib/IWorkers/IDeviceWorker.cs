using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.IWorkers
{
    /// <summary>
    /// eSight iBMC 服务器 业务类
    /// </summary>
    public interface IDeviceWorker
    {
       
        IESSession ESSession { get; set; }

        /// <summary>
        /// 查询设备明细，返回华为结果.
        /// </summary>
        /// <param name="dn">设备DN</param>
        /// <returns>设备明细</returns>
        QueryListResult<HWDeviceDetail> QueryForDeviceDetail(string dn);
    }
}
