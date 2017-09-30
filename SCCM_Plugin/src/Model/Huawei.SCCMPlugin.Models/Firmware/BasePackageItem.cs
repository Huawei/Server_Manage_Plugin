using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Huawei.SCCMPlugin.Models.Firmware
{
    [Serializable]
    public class BasePackageItem
    {
        /// <summary>
        /// 升级包名称
        /// </summary>
        [JsonProperty(PropertyName = "basepackageName")]
        public string BasepackageName { get; set; }

        /// <summary>
        /// 升级包描述
        /// </summary>
        [JsonProperty(PropertyName = "basepackageDesc")]
        public string BasepackageDesc { get; set; }

        /// <summary>
        /// 升级包类型
        /// </summary>
        [JsonProperty(PropertyName = "basepackageType")]
        public string BasepackageType { get; set; }
    }
}
