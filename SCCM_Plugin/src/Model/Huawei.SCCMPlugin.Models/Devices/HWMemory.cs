using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  [Serializable]
  public class HWMemory
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
    /// 容量，已经带有单位，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "capacity")]
    public string Capacity { get; set; }

    /// <summary>
    /// 厂家，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "manufacture")]
    public string Manufacture { get; set; }
    
    /// <summary>
    /// 主频，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "frequency")]
    public string Frequency { get; set; }

    /// <summary>
    ///设备唯一标识符
    /// </summary>
    [JsonProperty(PropertyName = "moId")]
    public string MoId { get; set; }
  }
}
