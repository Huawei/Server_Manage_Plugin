using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil.ModelHelper;
using Newtonsoft.Json;

namespace Huawei.SCCMPlugin.Models
{
    /// <summary>
    /// 任务资源表
    /// </summary>
    [DbTableName("HW_TASK_RESOURCE")]
    public class HWTaskResource : BaseModel
    {
        /// <summary>
        /// 对应任务表ID
        /// </summary>
        [JsonProperty(PropertyName = "hwESightTaskID")]
        [DbColumn("HW_ESIGHT_TASK_ID")]
        public int HWESightTaskID { get; set; }

        /// <summary>
        /// 服务器唯一标识，例如："NE=xxx"
        /// </summary>
        [JsonProperty(PropertyName = "dn")]
        [DbColumn("DN")]
        public string DN { get; set; }

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        [JsonProperty(PropertyName = "ipAddress")]
        [DbColumn("IP_ADDRESS")]
        public string IpAddress { get; set; }

        /// <summary>
        /// NONE FINISHED SYNC_FAILED HW_FAILED
        /// </summary>
        [JsonProperty(PropertyName = "syncStatus")]
        [DbColumn("SYNC_STATUS")]
        public string SyncStatus { get; set; }

        [JsonProperty(PropertyName = "deviceResult")]
        [DbColumn("DEVICE_RESULT")]
        public string DeviceResult { get; set; }

        [JsonProperty(PropertyName = "errorDetail")]
        [DbColumn("ERROR_DETAIL")]
        public string ErrorDetail { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        [DbColumn("ERROR_CODE")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "deviceProgress")]
        [DbColumn("DEVICE_PROGRESS")]
        public int DeviceProgress { get; set; }

        /// <summary>
        /// TASK_TYPE_POWER TASK_TYPE_OS TASK_TYPE_SOFTWARE 
        /// </summary>
        [JsonProperty(PropertyName = "taskType")]
        [DbColumn("TASK_TYPE")]
        public string TaskType { get; set; }

        /// <summary>
        /// 备用整型字段1
        /// </summary>
        [JsonProperty(PropertyName = "reservedInt1")]
        [DbColumn("RESERVED_INT1")]
        public int ReservediInt1 { get; set; }
        /// <summary>
        ///备用整型字段2
        /// </summary>
        [JsonProperty(PropertyName = "reservedInt2")]
        [DbColumn("RESERVED_INT2")]
        public int ReservediInt2 { get; set; }

        /// <summary>
        /// 备用字符字段1
        /// </summary>
        [JsonProperty(PropertyName = "reservedStr1")]
        [DbColumn("RESERVED_STR1")]
        public string ReservedStr1 { get; set; }
        /// <summary>
        ///备用字符字段2
        /// </summary>
        [JsonProperty(PropertyName = "reservedStr2")]
        [DbColumn("RESERVED_STR2")]
        public string ReservedStr2 { get; set; }

        /// <summary>
        /// 上次修改时间
        /// </summary>
        [JsonProperty(PropertyName = "lastModifyTime")]
        [DbColumn("LAST_MODIFY_TIME")]
        public DateTime LastModifyTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty(PropertyName = "createTime")]
        [DbColumn("CREATE_TIME")]
        public DateTime CreateTime { get; set; }
    }
}
