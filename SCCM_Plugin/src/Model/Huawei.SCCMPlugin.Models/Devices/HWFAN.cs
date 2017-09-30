using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  [Serializable]
  public class HWFAN
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
    /// 转速
    /// </summary>
    [JsonProperty(PropertyName = "rotate")]
    public string Rotate { get; set; }
    /// <summary>
    /// 转百分比(%)
    /// </summary>
    [JsonProperty(PropertyName = "rotatePercent")]
    public int RotatePercent { get; set; }

    /// <summary>
    /// 风扇模式，含义如下：	“0”：自动	“1”：手动
    /// </summary>
    [JsonProperty(PropertyName = "controlModel")]
    public int ControlModel { get; set; }
    /// <summary>
    ///设备唯一标识符
    /// </summary>
    [JsonProperty(PropertyName = "moId")]
    public string MoId { get; set; }
  }
}
