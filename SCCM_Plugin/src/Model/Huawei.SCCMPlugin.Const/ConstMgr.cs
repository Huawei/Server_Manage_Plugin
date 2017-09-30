using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huawei.SCCMPlugin.Const
{
    /// <summary>
    /// 常量类<para />
    /// partial class片段类关键字。c#语法。
    /// 通过内部类和片段类的方式，将一个类写成类似包的样子，以达到可以看到引用常量的全路径的目的。
    /// </summary>
    public partial class ConstMgr
    {
        /// <summary>
        /// 常量类的基类，提供一些常量类的基础方法。
        /// </summary>
        public class ConstBase
        {
            /// <summary>
            /// 由一组 AS_KEY组成的Dictionary<para />
            /// 常量类的基本结构：<para />
            /// 业务实体类 如：HOST实体类<para />
            /// --keyTable( AS_KEY组成的Dictionary) 通常是某个字段的一个常量如 AS_STATUS 对应STATUS字段。<para />
            /// ----keyTable 里的Dictionary， 由一组类似KEY/VALUE的组成的对应常量， 类似下拉列表。<para />
            /// </summary>
            protected Dictionary<string, Dictionary<string, string>> keyTable = new Dictionary<string, Dictionary<string, string>>();
            /// <summary>
            /// 获取一个常量类型的常量值对列表。<para />
            /// 如：ConstMgr.HWESightHost.Instance.GetConstKeyByAsKey(ConstMgr.HWESightHost.AS_LATEST_STATUS);
            /// </summary>
            /// <param name="asKey">通常是某个字段的一个常量如 AS_STATUS 对应STATUS字段</param>
            /// <returns></returns>
            public Dictionary<string, string> GetConstKeyByAsKey(string asKey)
            {
                return keyTable[asKey];
            }
            /// <summary>
            /// 获取一个常量类型的常量 key 列表。<para />
            /// 如：ConstMgr.HWESightHost.Instance.MaskKeysByAsKey(ConstMgr.HWESightHost.AS_LATEST_STATUS);
            /// </summary>
            /// <param name="asKey">通常是某个字段的一个常量如 AS_STATUS 对应STATUS字段</param>
            /// <returns></returns>
            public string[] MaskKeysByAsKey(string asKey)
            {
                Dictionary<string, string> roleTalbe = keyTable[asKey];
                return roleTalbe.Keys.ToArray();
            }
            /// <summary>
            /// 获取一个常量类型的常量 值 列表。<para />
            /// 如：ConstMgr.HWESightHost.Instance.MaskKeysByAsKey(ConstMgr.HWESightHost.AS_LATEST_STATUS); 
            /// </summary>
            /// <param name="asKey">通常是某个字段的一个常量如 AS_STATUS 对应STATUS字段</param>
            /// <returns></returns>
            public string[] MaskValueByAsKey(string asKey)
            {
                Dictionary<string, string> roleTalbe = keyTable[asKey];
                return roleTalbe.Values.ToArray();
            }

            /// <summary>
            /// 获取一个常量对应的值<para />
            /// 如：ConstMgr.HWESightHost.Instance.MaskKeysByAsKey(ConstMgr.HWESightHost.AS_LATEST_STATUS); 
            /// </summary>
            /// <param name="asKey">通常是某个字段的一个常量如 AS_STATUS 对应STATUS字段</param>
            /// <param name="findKey">常量的key</param>
            /// <returns></returns>
            public string ValueOfKey(string asKey, string findKey)
            {
                Dictionary<string, string> roleTalbe = GetConstKeyByAsKey(asKey);
                if (roleTalbe.ContainsKey(findKey))
                {
                    return roleTalbe[findKey];
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
