using Microsoft.ConfigurationManagement.AdminConsole;
using Microsoft.ConfigurationManagement.AdminConsole.Schema;
using Microsoft.ConfigurationManagement.AdminConsole.Views.Common;
using Microsoft.EnterpriseManagement.UI.WpfViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Huawei.SCCMPlugin.PluginUI.Views
{
  public class ViewBaseDescription : IConsoleView2, IConsoleView
  {
    // Fields
    //private IContainer components;

    // Methods
    public bool TryConfigure(ref XmlElement persistedConfigurationData)
    {
      return false;
    }

    public bool TryInitialize(ScopeNode scopeNode, AssemblyDescription resourceAssembly, ViewAssemblyDescription viewAssemblyDescription)
    {
      return true;
    }

    // Properties
    public Type TypeOfView
    {
      get
      {
        return typeof(Overview);
      }
    }

    public Type TypeOfViewController
    {
      get
      {
        return typeof(ViewBase);
      }
    }
  }
}
