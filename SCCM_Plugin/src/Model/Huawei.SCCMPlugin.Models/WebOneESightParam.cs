using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
  public class WebOneESightParam<T>
  {
    [JsonProperty(PropertyName = "esight")]
    public string ESightIP { get; set; }

    [JsonProperty(PropertyName = "param")]
    public T Param { get; set; }
  }
}
