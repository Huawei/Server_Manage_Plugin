using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
    [Serializable]
    public class Blade
    {
        /// <summary>
        /// 刀片唯一标识，例如："NE=xxx"
        /// </summary>
        [JsonProperty(PropertyName = "dn")]
        public string DN { get; set; }
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        [JsonProperty(PropertyName = "ipAddress")]
        public string IpAddress { get; set; }

        /// <summary>
        /// 服务器位置信息，属性字符串直接显示，非枚举值
        /// </summary>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// BMC版本信息。
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
        /// <summary>
        /// 子刀片名称。
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
