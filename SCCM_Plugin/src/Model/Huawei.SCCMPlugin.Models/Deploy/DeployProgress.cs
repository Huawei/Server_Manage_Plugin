using Huawei.SCCMPlugin.Models.Deploy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Deploy
{
  [Serializable]
  public class DeployProgress
  {
    /// <summary>
    /// 接口自动生成的任务名称，任务的唯一标识
    /// </summary>
    [JsonProperty(PropertyName = "taskName")]
    public string TaskName { get; set; }

    /// <summary>
    /// 模板的列表
    /// </summary>
    [JsonProperty(PropertyName = "templates")]
    public string Templates { get; set; }

    /// <summary>
    /// ConstMgr.HWESightTask.TASK_STATUS_RUNNING;
    /// 任务状态，取值范围：Complete or Running
    /// </summary>
    [JsonProperty(PropertyName = "taskStatus")]
    public string TaskStatus { get; set; }

    /// <summary>
    /// 任务执行进度，取值范围：0 to 100 -int
    /// </summary>
    [JsonProperty(PropertyName = "taskProgress")]
    public int TaskProgress { get; set; }
    /// <summary>
    /// ConstMgr.HWESightTask.TASK_RESULT_SUCCESS
    /// 任务执行结果，任务状态为Running时，为空；任务状态为Complete时，取值范围为：Success or Failed
    /// </summary>
    [JsonProperty(PropertyName = "taskResult")]
    public string TaskResult { get; set; }
    /// <summary>
    /// 任务执行结果，任务状态为Running时，为空；任务状态为Complete时，取值范围为：0代表任务成功，其余代表失败
    /// </summary>
    [JsonProperty(PropertyName = "taskCode")]
    public string TaskCode { get; set; }


    /// <summary>
    /// 任务执行结果，任务状态为Running时，为空；任务状态为Complete时，取值范围为：成功时为空，失败时为失败具体原因
    /// </summary>
    [JsonProperty(PropertyName = "errorDetail")]
    public string ErrorDetail { get; set; }

    [JsonProperty(PropertyName = "deviceDetails")]
    public IList<DeviceProgress> DeviceDetails { get; set; }
  }
}
