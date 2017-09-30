using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Huawei.SCCMPlugin.Models.Firmware
{
    [Serializable]
    public class BasePackageDetail
    {
    /// <summary>
    /// 升级包名称，必选，可由大小写字母、数字或- _构成的6-32字符。
    /// </summary>
    [JsonProperty(PropertyName = "basepackageName")]
    public string BasepackageName { get; set; }

    /// <summary>
    /// 升级包描述，必选，0-128字符。
    /// </summary>
    [JsonProperty(PropertyName = "basepackageDesc")]
    public string BasepackageDesc { get; set; }

    /// <summary>
    /// 升级包类型，必选，”Firmware”,”Driver”,”Bundle”
    /// </summary>
    [JsonProperty(PropertyName = "basepackageType")]
    public string BasepackageType { get; set; }
    /// <summary>
    /// 详情属性
    /// </summary>
    [JsonProperty(PropertyName = "basepackageProp")]
    public JArray BasepackageProp { get; set; }

  }
}
