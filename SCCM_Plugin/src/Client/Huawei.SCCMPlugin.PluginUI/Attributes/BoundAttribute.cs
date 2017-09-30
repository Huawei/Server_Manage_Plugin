using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Attributes
{
    /// <summary>
    /// 定义Bound特性，用于绑定html页面
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class BoundAttribute : Attribute
    {
        /// <summary>
        /// Bound名称
        /// </summary>
        public string BoundName { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="boundName">Bound名称</param>
        public BoundAttribute(string boundName)
        {
            BoundName = boundName;
        }
    }
}
