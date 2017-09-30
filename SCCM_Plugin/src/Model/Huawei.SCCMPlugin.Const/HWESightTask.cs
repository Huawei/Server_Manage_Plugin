using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// eSight任务表-常量类
        /// </summary>
        public class HWESightTask : ConstBase
        {
            private static HWESightTask _instance = null;

            /// <summary>
            /// 任务状态，取值范围：Complete or Running
            /// </summary>
            public const string AS_TASK_STATUS = "TASK_STATUS";
            /// <summary>
            /// 正在运行
            /// </summary>
            public const string TASK_STATUS_RUNNING = "Running";
            /// <summary>
            /// 完成
            /// </summary>
            public const string TASK_STATUS_COMPLATE = "Complete";

            
            /// <summary>
            /// eSight返回状态
            ///任务执行结果，任务状态为
            ///Running时，为空；任务状态
            ///为Complete时，取值范围为：
            ///Success or Failed
            /// </summary>
            public const string AS_TASK_RESULT = "TASK_RESULT";
            public const string TASK_RESULT_SUCCESS = "Success";
            public const string TASK_RESULT_FAILED = "Failed";

            /// <summary>
            /// 同步eSight返回的结果状态
            /// </summary>
            public const string AS_SYNC_STATUS = "SYNC_STATUS";
            /// <summary>
            /// 创建初始 相当于running
            /// </summary>
            public const string SYNC_STATUS_CREATED = "CREATED";
            /// <summary>
            /// 完成
            /// </summary>
            public const string SYNC_STATUS_FINISHED = "FINISHED";
            /// <summary>
            /// plugin自身错误引起的同步失败
            /// </summary>
            public const string SYNC_STATUS_SYNC_FAILED = "SYNC_FAILED";
            /// <summary>
            /// Partion Success,部分成功。
            /// </summary>
            public const string SYNC_STATUS_HW_PFAILED = "PARTION_FAILED";
            /// <summary>
            /// eSight返回失败。
            /// </summary>
            public const string SYNC_STATUS_HW_FAILED = "HW_FAILED";

            /// <summary>
            /// 任务类型
            /// </summary>
            public const string AS_TASK_TYPE = "TASK_TYPE";
            /// <summary>
            /// 模板部署任务
            /// </summary>
            public const string TASK_TYPE_DEPLOY = "TASK_TYPE_DEPLOY";
            /// <summary>
            /// 上传软件源任务
            /// </summary>
            public const string TASK_TYPE_SOFTWARE = "TASK_TYPE_SOFTWARE";
            /// <summary>
            /// 上传固件升级包任务
            /// </summary>
            public const string TASK_TYPE_FIRMWARE = "TASK_TYPE_FIRMWARE";
            /// <summary>
            /// 部署升级包任务
            /// </summary>
            public const string TASK_TYPE_DEPLOYFIRMWARE = "TASK_TYPE_DEPLOYFIRMWARE";

            public HWESightTask()
            {
                Dictionary<string, string> taskStatusDict = new Dictionary<string, string>();
                taskStatusDict.Add(TASK_STATUS_RUNNING, "运行中");
                taskStatusDict.Add(TASK_STATUS_COMPLATE, "完成");

                this.keyTable[AS_TASK_STATUS] = taskStatusDict;

                Dictionary<string, string> taskResultDict = new Dictionary<string, string>();
                taskResultDict.Add(TASK_RESULT_SUCCESS, "成功");
                taskResultDict.Add(TASK_RESULT_FAILED, "失败");

                this.keyTable[AS_TASK_RESULT] = taskResultDict;

                Dictionary<string, string> syncStatusDict = new Dictionary<string, string>();
                syncStatusDict.Add(SYNC_STATUS_CREATED, "已创建");
                syncStatusDict.Add(SYNC_STATUS_FINISHED, "已完成");
                syncStatusDict.Add(SYNC_STATUS_SYNC_FAILED, "同步失败");
                syncStatusDict.Add(SYNC_STATUS_HW_PFAILED, "部分成功");                
                syncStatusDict.Add(SYNC_STATUS_HW_FAILED, "同步eSight失败");

                this.keyTable[AS_SYNC_STATUS] = syncStatusDict;

                Dictionary<string, string> taskTypeDict = new Dictionary<string, string>();
                taskTypeDict.Add(TASK_TYPE_DEPLOY, "部署任务");
                taskTypeDict.Add(TASK_TYPE_SOFTWARE, "软件源任务");
                taskTypeDict.Add(TASK_TYPE_SOFTWARE, "固件上传任务");
                taskTypeDict.Add(TASK_TYPE_DEPLOYFIRMWARE, "固件部署任务");

                this.keyTable[AS_TASK_TYPE] = taskTypeDict;
            }

            public static HWESightTask Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new HWESightTask();
                    }
                    return _instance;
                }
            }
        }
    }
}

