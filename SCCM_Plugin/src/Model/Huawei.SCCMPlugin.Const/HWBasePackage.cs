using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// 固件升级包-常量类
        /// </summary>
        public class HWBasePackage : ConstBase
        {
            private static HWBasePackage _instance = null;
            /// <summary>
            /// 升级包类型
            /// </summary>
            public const string AS_PACKAGE_TYPE = "PACKAGE_TYPE";
            /// <summary>
            /// 固件包
            /// </summary>
            public const string PACKAGE_TYPE_FIRMWARE = "Firmware";
            /// <summary>
            /// 驱动包
            /// </summary>
            public const string PACKAGE_TYPE_DRIVER = "Driver";
            /// <summary>
            /// 基线包
            /// </summary>
            public const string PACKAGE_TYPE_BUNDLE = "Bundle";
            /// <summary>
            /// 构造方法
            /// </summary>
            public HWBasePackage()
            {
                Dictionary<string, string> packageDict = new Dictionary<string, string>();
                packageDict.Add(PACKAGE_TYPE_FIRMWARE, "Firmware");
                packageDict.Add(PACKAGE_TYPE_FIRMWARE, "Driver");
                packageDict.Add(PACKAGE_TYPE_FIRMWARE, "Bundle");

                this.keyTable[AS_PACKAGE_TYPE] = packageDict;
            }
            /// <summary>
            /// 单例方法
            /// </summary>
            public static HWBasePackage Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new HWBasePackage();
                    }
                    return _instance;
                }
            }
        }
    }
}
