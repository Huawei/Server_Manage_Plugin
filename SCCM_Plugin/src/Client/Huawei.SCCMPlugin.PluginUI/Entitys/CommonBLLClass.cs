using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Entitys
{
    /// <summary>
    /// 删除任务API参数对象，用于升级任务、模板任务
    /// </summary>
    public class ParamOfDeleteTask
    {
        [JsonProperty("taskId")]
        public int TaskId { get; set; }
    }

    /// <summary>
    /// 分页参数对象，用于只有页码和页大小两个参数的UI
    /// </summary>
    public class ParamOnlyPagingInfo
    {
        [JsonProperty("pageNo")]
        public int PageNo { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 分页参数对象，用于查询模板列表
    /// </summary>
    public class ParamPagingOfQueryTemplate
    {
        [JsonProperty("pageNo")]
        public int PageNo { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("templateType")]
        public string TemplateType { get; set; }
    }

    /// <summary>
    /// 分页参数对象，用于查询eSight列表
    /// </summary>
    public class ParamPagingOfQueryESight
    {
        [JsonProperty("pageNo")]
        public int PageNo { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("hostIp")]
        public string HostIp { get; set; }
    }

    /// <summary>
    /// 用于升级任务详情查询
    /// </summary>
    public class ParamOfQueryFirmewareDetail
    {
        [JsonProperty("taskName")]
        public string TaskName { get; set; }

        [JsonProperty("dn")]
        public string DN { get; set; }
    }

    /// <summary>
    /// 用于服务器详细信息查询
    /// </summary>
    public class ParamOfQueryDeviceDetail
    {
        [JsonProperty("ip")]
        public string ESightIP { get; set; }

        [JsonProperty("dn")]
        public string DN { get; set; }

        [JsonProperty("serverType")]
        public string ServerType { get; set; }
    }
}
