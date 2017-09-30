using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Attributes
{
    /// <summary>
    /// 定义CefSharp Browser将打开的URL
    /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  public sealed class CefURLAttribute : Attribute
  {
    private readonly string _url;

        /// <summary>
        /// SCCM Plugin UI Url
        /// </summary>
    public string URL
    {
      get { return _url; }
    }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">SCCM Plugin UI Url</param>
    public CefURLAttribute(string url)
    {
      _url = url;
    }
  }
}
