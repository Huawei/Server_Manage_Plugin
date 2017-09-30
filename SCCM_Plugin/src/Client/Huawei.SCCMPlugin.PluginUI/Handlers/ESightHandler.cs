using Huawei.SCCMPlugin.PluginUI.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCM.WebApp.Plugin;
using Huawei.SCCMPlugin.PluginUI.Attributes;

namespace Huawei.SCCMPlugin.PluginUI.Handlers
{
  [CefURL("test/test.html")]
  public class ESightHandler : IWebHandler
  {
    /*
     * 1. Init Page.
     * 2. Get Ajax, 获得一些下拉数据。
     * 3. 提交。
     * 3.1 添加 删除 修改。（拿数据）
     */
    public string JavascriptEventArrived(CefBrowser cefBrowser, string eventName, string postData)
    {
      //init
      throw new NotImplementedException();
    }
  }
}
