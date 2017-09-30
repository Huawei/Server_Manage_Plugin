using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  /// <summary>
  /// 板信息，刀片服务器：交换板；机架、高密服务器、刀片：主板；
  /// </summary>
  [Serializable]
  public class HWBoard
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
    /// 单板类型，含义如下：	“0”：主板	“1”：交换板
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public int BoardType { get; set; }
    /// <summary>
    /// 单板序列号，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "sn")]
    public string SN { get; set; }

    /// <summary>
    /// 单板部件号，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "partNumber")]
    public string PartNumber { get; set; }

    /// <summary>
    /// 厂商，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "manufacturer")]
    public string Manufacturer { get; set; }

    /// <summary>
    /// 制造日期，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "manuTime")]
    public string ManuTime { get; set; }

    /// <summary>
    ///设备唯一标识符
    /// </summary>
    [JsonProperty(PropertyName = "moId")]
    public string MoId { get; set; }
  }
}
