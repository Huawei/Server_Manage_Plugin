using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Firmware
{
    [Serializable]
    public class DeployPackageTask
    {
        /// <summary>
        /// 待升级的升级包名称。一个升级任务只能使用一个升级包。
        /// </summary>
        [JsonProperty(PropertyName = "basepackageName")]
        public string BasepackageName { get; set; }
        /// <summary>
        /// 升级部件列表。可同时升级IBMC、BIOS、RAID卡固件、RAID卡驱动等
        /// RAID,CNA
        /// </summary>
        [JsonProperty(PropertyName = "firmwareList")]
        public string FirmwareList { get; set; }
        /// <summary>
        /// 设备IP。机架/高密服务器填写机架BMC的IP，机框填写E9000@HMM的IP，刀片服务器填写" blade槽位@HMM的IP"，交换板填写"switch槽位@HMM的IP"
        /// </summary>
        [JsonProperty(PropertyName = "dn")]
        public string DeviceDn { get; set; }

        [JsonProperty(PropertyName = "isforceupgrade")]
        public bool IsForceUpgrade { get; set; }
        /// <summary>
        /// effectiveMethod
        /// </summary>
        [JsonProperty(PropertyName = "effectiveMethod")]
        public int EffectiveMethod { get; set; }
    }
}
