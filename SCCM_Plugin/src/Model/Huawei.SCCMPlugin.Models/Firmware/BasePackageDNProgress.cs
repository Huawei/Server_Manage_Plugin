using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Firmware
{
    /// <summary>
    /// 6.2.11.7 查询升级任务中指定设备的固件升级详情
    /// </summary>
    [Serializable]
    public class BasePackageDNProgress
    {
        /// <summary>
        /// 接口自动生成的任务名称，任务的唯一标识。
        /// </summary>
        [JsonProperty(PropertyName = "taskName")]
        public string Taskname { get; set; }

        /// <summary>
        /// 服务器唯一标识，例如："NE=xxx"。
        /// </summary>
        [JsonProperty(PropertyName = "dn")]
        public string DN { get; set; }
        /// <summary>
        /// 设备升级任务状态。取值范围：
        ///Idle：任务未开始运行
        ///Running：任务正在运行
        ///Complete：任务已运行结束
        /// </summary>
        [JsonProperty(PropertyName = "deviceTaskStatus")]
        public string DeviceTaskStatus { get; set; }
        /// <summary>
        /// 设备升级任务进度。任务执行进度，取值范围：0-100。
        /// </summary>
        [JsonProperty(PropertyName = "deviceTaskProgress")]
        public int DeviceTaskProgress { get; set; }

        /// <summary>
        /// 设备升级任务结果。任务状态为Idel/Running时，为空；任务状态为Complete时，取值范围：
        ///	Success：成功
        ///	Failed ：失败
        ///	Partion Success ：部分成功
        /// </summary>
        [JsonProperty(PropertyName = "deviceTaskResult")]
        public string DeviceTaskResult { get; set; }

        /// <summary>
        /// 设备固件升级详情。
        /// </summary>
        [JsonProperty(PropertyName = "firmwarelist")]
        public List<FirmwarelistProgress> Firmwarelist { get; set; }
    }
}
