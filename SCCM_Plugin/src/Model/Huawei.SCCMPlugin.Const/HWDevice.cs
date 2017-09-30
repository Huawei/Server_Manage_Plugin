using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// eSight服务器-常量类
        /// </summary>
        public class HWDevice : ConstBase
        {
            private static HWDevice _instance = null;
 
            /// <summary>
            /// 状态,NONE,ONLINE,DISCONNECT,FAILED
            /// </summary>
            public const string AS_STATUS = "STATUS";

            public const string STATUS_NORMAL = "0";
            public const string STATUS_OFFLINE = "-1";
            public const string STATUS_UNKNOWN = "-2";
            public const string STATUS_OTHER = "OTHER";

            public const string AS_SERVER_TYPE = "SERVER_TYPE";
            /// <summary>
            /// 机架服务器
            /// </summary>
            public const string SERVER_TYPE_RACK = "rack";
            /// <summary>
            /// 刀片服务器
            /// </summary>
            public const string SERVER_TYPE_BLADE = "blade";
            //public const string SERVER_TYPE_HIGHDENSITY= "highdensity";
            //public const string SERVER_TYPE_STORAGENODE= "storagenode";
            //public const string SERVER_TYPE_THIRDPARTYSERVER= "thirdpartyserver";

            public HWDevice()
            {
                Dictionary<string, string> statusDict = new Dictionary<string, string>();
                statusDict.Add(STATUS_NORMAL, "正常");
                statusDict.Add(STATUS_OFFLINE, "离线");
                statusDict.Add(STATUS_UNKNOWN, "未知");
                statusDict.Add(STATUS_OTHER, "故障");

                this.keyTable[AS_STATUS] = statusDict;

                Dictionary<string, string> serverTypeDict = new Dictionary<string, string>();

                serverTypeDict.Add(SERVER_TYPE_RACK, "机架服务器");
                serverTypeDict.Add(SERVER_TYPE_BLADE, "刀片服务器");
                //serverTypeDict.Add(SERVER_TYPE_HIGHDENSITY, "高密服务器");
                //serverTypeDict.Add(SERVER_TYPE_STORAGENODE, "存储性服务器");
                //serverTypeDict.Add(SERVER_TYPE_THIRDPARTYSERVER, "第三方服务器");

                this.keyTable[AS_SERVER_TYPE] = serverTypeDict;
            }
            /// <summary>
            /// 单例方法
            /// </summary>
            public static HWDevice Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new HWDevice();
                    }
                    return _instance;
                }
            }
        }
    }
}
