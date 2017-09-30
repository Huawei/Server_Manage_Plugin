using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
  /// <summary>
  /// 大列表查询，带页码的
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public class QueryLGListResult<T>
  {
    /// <summary>
    /// 操作返回码。可以是如下值之一：
    ///	0：成功
    ///	非0：失败
    /// </summary>
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }

    /// <summary>
    /// 软件源总数
    /// </summary>
    [JsonProperty(PropertyName = "totalNum")]
    public int TotalNum { get; set; }

    /// <summary>
    /// 软件源列表数据
    /// </summary>
    [JsonProperty(PropertyName = "data")]
    public List<T> Data { get; set; }

    /// <summary>
    /// 接口调用结果的描述信息。
    /// </summary>
    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }
  }
}
