using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Firmware
{
    /// <summary>
    /// 设备固件升级详情。
    /// </summary>
    [Serializable]
    public class FirmwarelistProgress
    {
        /// <summary>
        /// 固件类型。
        /// </summary>
        [JsonProperty(PropertyName = "firmwareType")]
        public string FirmwareType { get; set; }
        /// <summary>
        /// 当前固件版本。
        /// </summary>
        [JsonProperty(PropertyName = "currentVersion")]
        public string CurrentVersion { get; set; }
        /// <summary>
        /// 固件升级版本。
        /// </summary>
        [JsonProperty(PropertyName = "upgradeVersion")]
        public string UpgradeVersion { get; set; }
        /// <summary>
        /// 固件升级进度，取值范围0-100。
        /// </summary>
        [JsonProperty(PropertyName = "firmwareProgress")]
        public int FirmwareProgress { get; set; }
        /// <summary>
        /// 升级结果，取值范围：
        ///	Success：成功
        ///	Failed ：失败
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
        /// <summary>
        /// 升级结果细节，成功时为空，失败时返回错误码。
        /// </summary>
        [JsonProperty(PropertyName = "details")]
        public string Details { get; set; }
    }
}
