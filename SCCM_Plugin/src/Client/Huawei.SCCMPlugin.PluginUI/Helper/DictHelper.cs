using Huawei.SCCMPlugin.PluginUI.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Helper
{
    public sealed class DictHelper
    {
        /// <summary>
        /// 获取字典指定key的值
        /// </summary>
        /// <param name="dic">字典</param>
        /// <param name="key">指定key</param>
        /// <returns></returns>
        public static string GetDicValue(Dictionary<string, object> dic, string key)
        {
            if (dic.ContainsKey(key))//回调函数
            {
                return dic[key] as string;
            }
            return "";
        }

        /// <summary>
        /// 获取字典指定key的值
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="dic">字典</param>
        /// <param name="key">指定key</param>
        /// <returns></returns>
        public static T GetDicValue<T>(Dictionary<string, object> dic, string key)
        {
            if (dic.ContainsKey(key))//回调函数
            {
                return CommonUtil.CoreUtil.GetObjTranNull<T>(dic[key]);
            }
            return default(T);
        }
    }
}
