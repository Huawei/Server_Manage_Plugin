using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Softwares
{
  [Serializable]
  public class SoftwareSourceParam
  {
    /// <summary>
    /// 可选
    /// 页查询的第几页，从1开始，默认取第1页。
    ///说明
    ///pageNo大于查询到条数的总页数时，默认取最后一页。
    /// </summary>
    [JsonProperty(PropertyName = "pageNo")]
    public int PageNo { get; set; }
    /// <summary>
    /// 可选
    ///分页查询的每页记录数，支持1～100条，默认值20条。
    ///说明
    ///pageSize小于1或大于100时，使用默认值20。
    /// </summary>
    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; }
  }
}
