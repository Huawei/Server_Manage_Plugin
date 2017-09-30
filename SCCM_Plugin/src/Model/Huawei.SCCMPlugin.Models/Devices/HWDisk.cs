using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  [Serializable]
  public class HWDisk
  {
    /// <summary>
    /// 名称，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
    /// <summary>
    /// 服务器状态，含义如下：
    ///	“0”：正常
    ///	“-1”：离线
    ///	“-2”：未知
    ///	其他：故障
    /// </summary>
    [JsonProperty(PropertyName = "healthState")]
    public string HealthState { get; set; }
    /// <summary>
    /// 槽位信息
    /// </summary>
    [JsonProperty(PropertyName = "location")]
    public int Location { get; set; }
    /// <summary>
    ///设备唯一标识符
    /// </summary>
    [JsonProperty(PropertyName = "moId")]
    public string MoId { get; set; }
  }
}
