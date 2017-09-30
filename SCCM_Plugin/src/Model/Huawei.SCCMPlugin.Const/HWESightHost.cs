using System.Collections.Generic;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// eSight主机
        /// </summary>
        public class HWESightHost : ConstBase
        {
            private static HWESightHost _instance = null;
            /// <summary>
            /// eSight服务器上传连接状体
            /// 状态,NONE,ONLINE,DISCONNECT,FAILED
            /// </summary>
            public const string AS_LATEST_STATUS = "LATEST_STATUS";

            public const string LATEST_STATUS_NONE = "NONE";
            public const string LATEST_STATUS_ONLINE = "ONLINE";
            public const string LATEST_STATUS_DISCONNECT = "DISCONNECT";
            public const string LATEST_STATUS_FAILED = "FAILED";

            /// <summary>
            /// eSight服务器,http请求默认超时时间。
            /// </summary>
            public const int DEFAULT_TIMEOUT_SEC = 30 * 60;
            /// <summary>
            /// http请求URL的pattern
            /// </summary>
            public const string BASE_URI_FROMATE = "{0}://{1}:{2}/";
            /// <summary>
            /// 登陆eSight REST URL
            /// </summary>
            public const string URL_LOGIN = "rest/openapi/sm/session";
            /// <summary>
            ///服务器查询 REST URL
            /// </summary
            public const string URL_DEVICEPAGE = "rest/openapi/server/device";
            /// <summary>
            ///服务器明细查询 REST URL
            /// </summary
            public const string URL_DEVICEDETAIL = "rest/openapi/server/device/detail";

            /// <summary>
            ///上传软件源 REST URL
            /// </summary
            public const string URL_UPLOADE_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/upload";
            /// <summary>
            ///上传软件源进度查询 REST URL
            /// </summary
            public const string URL_PROGRESS_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/progress";
            /// <summary>
            ///删除软件源 REST URL
            /// </summary
            public const string URL_DELETE_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/delete";
            /// <summary>
            ///软件源列表查询 REST URL
            /// </summary
            public const string URL_PAGE_SOFTWARESOURCE = "rest/openapi/server/deploy/software/list";

            /// <summary>
            ///部署模板查询 REST URL
            /// </summary
            public const string URL_PAGE_DEPLOY_TEMPLATE = "rest/openapi/server/deploy/template/list";
            /// <summary>
            ///添加部署模板 REST URL
            /// </summary
            public const string URL_DEPLOY_TEMPLATE = "rest/openapi/server/deploy/template";
            /// <summary>
            ///模板详情 REST URL
            /// </summary
            public const string URL_DETAIL_TEMPLATE = "rest/openapi/server/deploy/template/detail";
            /// <summary>
            ///删除部署模板 REST URL
            /// </summary
            public const string URL_DELETE_DEPLOYTEMPLATE = "rest/openapi/server/deploy/template/del";
            /// <summary>
            ///部署模板到服务器 REST URL
            /// </summary
            public const string URL_TASK_DEPLOY = "rest/openapi/server/deploy/task";
            /// <summary>
            ///部署模板到服务器进度 REST URL
            /// </summary
            public const string URL_PROGRESS_DEPLOY = "rest/openapi/server/deploy/task/detail";
                        
            /// <summary>
            ///固件上传 REST URL
            /// </summary
            public const string URL_UPLOADE_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/upload";
            /// <summary>
            ///固件上传进度 REST URL
            /// </summary
            public const string URL_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/progress";
            /// <summary>
            /// 固件-删除升级包
            /// </summary>
            public const string URL_DELETE_BASEPACKAGE = "rest/openapi/server/firmware/basepackage/del";
            /// <summary>
            ///固件列表查询 REST URL
            /// </summary
            public const string URL_PAGE_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/list";
            /// <summary>
            ///固件列表查询-明细 REST URL
            /// </summary
            public const string URL_DETAIL_BASEPACKAGE = "rest/openapi/server/firmware/basepackage/detail";
 
            /// <summary>
            ///固件升级部署任务 REST URL
            /// </summary                                        
            public const string URL_TASK_BASEPACKAGE = "rest/openapi/server/firmware/task";
            /// <summary>
            ///固件升级部署任务-详情 REST URL
            /// </summary           
            public const string URL_TASK_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/taskdetail";

            public const string URL_TASK_DN_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/taskdevicedetail";

            public HWESightHost()
            {
                Dictionary<string, string> latestStatusDict = new Dictionary<string, string>();
                latestStatusDict.Add(LATEST_STATUS_NONE, "初始");
                latestStatusDict.Add(LATEST_STATUS_ONLINE, "在线");
                latestStatusDict.Add(LATEST_STATUS_DISCONNECT, "断开");
                latestStatusDict.Add(LATEST_STATUS_FAILED, "连接失败");

                this.keyTable[AS_LATEST_STATUS] = latestStatusDict;
            }

            public static HWESightHost Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new HWESightHost();
                    }
                    return _instance;
                }
            }
        }
    }
}
