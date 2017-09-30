using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Deploy
{
    [Serializable]
    public class DeviceProgress
    {
        /// <summary>
        /// “deviceDn” : “NE=123”,
        /// </summary>
        [JsonProperty(PropertyName = "dn")]
        public string DeviceDn { get; set; }

        /// <summary>
        /// “deviceResult”:” Success”,
        /// </summary>
        [JsonProperty(PropertyName = "deviceResult")]
        public string DeviceResult { get; set; }

        /// <summary>
        /// “deviceProgress”:”100”,
        /// </summary>
        [JsonProperty(PropertyName = "deviceProgress")]
        public int Progress { get; set; }
        /// <summary>
        /// “errorDetail”:””
        /// </summary>
        [JsonProperty(PropertyName = "errorDetail")]
        public string ErrorDetail { get; set; }
        /// <summary>
        /// errorCode
        /// </summary>
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }
    }
}
