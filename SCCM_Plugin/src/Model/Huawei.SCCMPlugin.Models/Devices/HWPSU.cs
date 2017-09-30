using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  [Serializable]
  public class HWPSU
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
    /// 功耗
    /// </summary>
    [JsonProperty(PropertyName = "inputPower")]
    public string InputPower { get; set; }

    /// <summary>
    /// 厂家，属性字符串直接显示，非枚举值
    /// </summary>
    [JsonProperty(PropertyName = "manufacture")]
    public string Manufacture { get; set; }

    /// <summary>
    /// 电源版本信息，属性字符串直接显示，非枚举值；    备注：有的电源没有版本信息；
    /// </summary>
    [JsonProperty(PropertyName = "version")]
    public string Version { get; set; }


    /// <summary>
    /// 输入电源模式：acInput(1) dcInput(2) acInputDcInput(3)
    /// </summary>
    [JsonProperty(PropertyName = "inputMode")]
    public string InputMode { get; set; }

    /// <summary>
    ///设备唯一标识符
    /// </summary>
    [JsonProperty(PropertyName = "moId")]
    public string MoId { get; set; }
  }
}
