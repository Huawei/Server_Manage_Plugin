using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil.ModelHelper;
using Newtonsoft.Json;
using Huawei.SCCMPlugin.Models.Deploy;

namespace Huawei.SCCMPlugin.Models
{
    /// <summary>
    /// 软件源+OS模板+上下电 任务表
    /// </summary>
    [DbTableName("HW_ESIGHT_TASK")]
    public class HWESightTask : BaseModel
    {

        /// <summary>
        /// ESIGHT的ID 
        /// </summary>
        [JsonProperty(PropertyName = "hwEsighthostId")]
        [DbColumn("HW_ESIGHT_HOST_ID")]
        public int HWESightHostID { get; set; }

        /// <summary>
        /// 对应taskName
        /// </summary>
        [JsonProperty(PropertyName = "taskName")]
        [DbColumn("TASK_NAME")]
        public string TaskName { get; set; }

        /// <summary>
        /// softwaresourceName
        /// </summary>
        [JsonProperty(PropertyName = "softwareSourceName")]
        [DbColumn("SOFTWARE_SOURCE_NAME")]
        public string SoftWareSourceName { get; set; }
        /// <summary>
        /// templates    任务中添加的模板，模板中间以分号隔开
        /// </summary>
        [JsonProperty(PropertyName = "templates")]
        [DbColumn("TEMPLATES")]
        public string Templates { get; set; }
        /// <summary>
        /// deviceIP, 部署的设备IP
        /// </summary>
        [JsonProperty(PropertyName = "deviceIp")]
        [DbColumn("DEVICE_IP")]
        public string DeviceIp { get; set; }
        /// <summary>
        /// taskStatus
        /// </summary>
        [JsonProperty(PropertyName = "taskStatus")]
        [DbColumn("TASK_STATUS")]
        public string TaskStatus { get; set; }

        /// <summary>
        /// taskProgress
        /// </summary>
        [JsonProperty(PropertyName = "taskProgress")]
        [DbColumn("TASK_PROGRESS")]
        public int TaskProgress { get; set; }

        /// <summary>
        /// taskResult 任务执行结果，任务状态为 Running时，为空；任务状态 为Complete时，取值范围为：Success or Failed
        /// </summary>
        [JsonProperty(PropertyName = "taskResult")]
        [DbColumn("TASK_RESULT")]
        public string TaskResult { get; set; }


        /// <summary>
        /// taskResult任务执行结果，任务状态为Running时，为空；任 务状态为Complete时，取值范围为：0代表任务成功，其余代表失败
        /// </summary>
        [DbColumn("TASK_CODE")]
        [JsonProperty(PropertyName = "taskCode")]
        public string TaskCode { get; set; }

        /// <summary>
        /// taskResult任务执行结果，任务状态 为Running时，为空；任务状态为Complete时，取值范围为：成功时为空，失败时为失败具体原因超过2000时自动截取
        /// </summary>
        [JsonProperty(PropertyName = "errorDetail")]
        [DbColumn("ERROR_DETAIL")]
        public string ErrorDetail { get; set; }

        /// <summary>
        /// NONE FINISHED SYNC_FAILED HW_FAILED
        /// </summary>
        [JsonProperty(PropertyName = "syncStatus")]
        [DbColumn("SYNC_STATUS")]
        public string SyncStatus { get; set; }

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
        [JsonConverter(typeof(CustomJsonDateTimeConverter))]
        [JsonProperty(PropertyName = "lastModify")]
        [DbColumn("LAST_MODIFY_TIME")]
        public DateTime LastModifyTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonConverter(typeof(CustomJsonDateTimeConverter))]
        [JsonProperty(PropertyName = "createTime")]
        [DbColumn("CREATE_TIME")]
        public DateTime CreateTime { get; set; }

        IList<HWTaskResource> deviceDetails = new List<HWTaskResource>();
        /// <summary>
        /// 资源设备列表。
        /// </summary>
        [JsonProperty(PropertyName = "deviceDetails")]
        public IList<HWTaskResource> DeviceDetails {
            get { return deviceDetails; }
            set { deviceDetails = value; }
        }
    }
}
