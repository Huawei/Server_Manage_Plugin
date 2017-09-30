using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// 前台错误代码，由后台返回
        /// 只有一些后台抛出错误代码定义。
        /// </summary>
        public class ErrorCode : ConstBase
        {
            /// <summary>
            /// 华为鉴权失败，返回正文如下
            /// {"description":"Auth failed","data":null,"code":1204}
            /// </summary>
            public const string HW_LOGIN_AUTH = "1204";


            /// <summary>
            /// 仅做判断使用，当多个eSight有成功，有失败。 前台判断这个错误代码。来具体显示不同eSight对应的错误。
            /// </summary>
            public const string SYS_UNKNOWN_PART_ERR = "-100000";
            /// <summary>
            ///  系统未知错误，API使用
            /// </summary>
            public const string SYS_UNKNOWN_ERR = "-99999"; //未知错误

            //系统内部错误 9xxxx
            /// <summary>
            /// 请先初始化
            /// </summary>
            public const string SYS_UNINIT = "-90001";  

            /// <summary>
            /// 请先配置eSight
            /// </summary>
            public const string SYS_NO_ESIGHT = "-90002"; 
            /// <summary>
            /// 当前没有失败任务   
            /// </summary>
            public const string SYS_NO_FAILEDTASK = "-90004";
            /// <summary>
            /// eSight用户名或者密码错误
            /// </summary>
            public const string SYS_USER_LOGING = "-90005";

            //网络错误  8xxxx
            /// <summary>
            /// String.Format("Accessing[{0}] , Error occurred: {1}", url, hrm.StatusCode)// 访问esighthttp错误。
            /// </summary>
            public const string NET_EISGHT_HTTP = "-80009";
            /// <summary>
            /// No connection could be made because the target machine actively refused.
            /// </summary>
            public const string NET_SOCKET_REFUSED = "-80010";
            /// <summary>
            /// Network connection error.
            /// </summary>
            public const string NET_SOCKET_UNKNOWN = "-81001";
            /// <summary>
            /// 没有找到对应的eSight.
            /// </summary>
            public const string NET_ESIGHT_NOFOUND = "-80011";

            //数据库错误 7xxxx
            /// <summary>
            /// 数据库没有找到对应的数据。[{0}]
            /// </summary>
            public const string DB_NOTFOUND = "-70001";
        }
    }
}
