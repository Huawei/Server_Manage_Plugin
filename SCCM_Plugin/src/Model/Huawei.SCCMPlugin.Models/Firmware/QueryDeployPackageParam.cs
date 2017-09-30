using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Firmware
{
  [Serializable]
  public class QueryDeployPackageParam
  {
    [JsonProperty(PropertyName = "taskName")]
    public string TaskeName { get; set; }

    [JsonProperty(PropertyName = "taskStatus")]
    public string TaskStatus { get; set; }

    /// <summary>
    /// PageNo
    /// </summary>
    [JsonProperty(PropertyName = "pageNo")]
    public int PageNo { get; set; }
    /// <summary>
    /// pageSize
    /// </summary>
    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; }

    [JsonProperty(PropertyName = "order")]
    public string Order { get; set; }

    private bool _isDesc = false;
    [JsonProperty(PropertyName = "orderDesc", NullValueHandling=NullValueHandling.Ignore)]
    public bool OrderDesc {
            get { return _isDesc; }
            set { _isDesc = value; }
        }
    }
}
