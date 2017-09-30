using Microsoft.ConfigurationManagement.AdminConsole.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Huawei.SCCMPlugin.PluginUI.Views
{
  public class HostTabsViewController : ViewBase
  {
    HostTabsViewFrm _frm = null;
    public HostTabsViewController() : base(){
     
    }
    public override void EndInit()
    {
      base.EndInit();
      string meanu = GetMenuIndexNodeVal(base.RootNodeProviderConfiguration.ConsoleRootObject.ViewAssemblyDescription[0]);
      _frm = new HostTabsViewFrm(meanu);
      base.InitByForm(_frm);
    }
    private string GetMenuIndexNodeVal(ViewAssemblyDescription viewAssemblyDescription)
    {
      string retVal = "";
      try
      {
        XmlElement rootNode = viewAssemblyDescription.CustomData;
        if (rootNode != null)
        {
          retVal = rootNode.InnerText;
        }
      }
      catch (Exception se)
      {
        LogUtil.HWLogger.UI.Error(se);
        throw;
      }
      return retVal;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!_frm.IsDisposed) _frm.Close();
    }
  }
}
