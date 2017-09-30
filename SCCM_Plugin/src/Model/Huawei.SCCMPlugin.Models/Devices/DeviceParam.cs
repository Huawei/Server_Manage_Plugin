using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Devices
{
  [Serializable]
  public class DeviceParam
  {
    /// <summary>
    /// 服务器类型，范围如下：
    /// 说明
    /// “rack”：机架服务器
    /// “blade”：刀片服务器
    /// “highdensity”：高密服务器
    /// “storagenode”：存储型服务器
    /// “thirdpartyserver”：第三方服务器
    /// </summary>
    [JsonProperty(PropertyName = "servertype")]
    public string ServerType { get; set; }

    /// <summary>
    /// 可选
    /// 页查询的第几页，从1开始，默认取第1页。
    ///说明
    ///pageNo大于查询到条数的总页数时，默认取最后一页。
    /// </summary>
    [JsonProperty(PropertyName = "start")]
    public int StartPage { get; set; }
    /// <summary>
    /// 可选
    ///分页查询的每页记录数，支持1～100条，默认值20条。
    ///说明
    ///pageSize小于1或大于100时，使用默认值20。
    /// </summary>
    [JsonProperty(PropertyName = "size")]
    public int PageSize { get; set; }
    /// <summary>
    /// 可选
    ///指定查询结果集采用的排序字段。仅指定一个字段
    /// 缺省排序字段是dn。
    /// 可指定的排序字段包括：dn、ipAddress、serverName
    /// </summary>
    [JsonProperty(PropertyName = "orderby")]
    public string PageOrder { get; set; }

    /// <summary>
    /// 可选
    ///指定查询结果是否按降序排序。缺省值是false。
    ///说明：
    ///此请求参数只有指定了“orderby”请求参数后才有效。
    /// </summary>
    [JsonProperty(PropertyName = "desc")]
    public bool OrderDesc { get; set; }
  }
}
