using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Deploy
{
  [Serializable]
  public class DeployTask
  {
    /// <summary>
    /// 待配置的模板列表。可同时配置多个模板，每个之间以分号隔开，多个模板类型不能相同
    /// </summary>
    [JsonProperty(PropertyName = "templates")]
    public string Templates { get; set; }
    /// <summary>
    /// 要部署设备的DN，服务器唯一标识，例如："NE=xxx；NE=xxx"
    /// </summary>
    [JsonProperty(PropertyName = "deviceDn ")]
    public string DeviceDn { get; set; }
  }
}
