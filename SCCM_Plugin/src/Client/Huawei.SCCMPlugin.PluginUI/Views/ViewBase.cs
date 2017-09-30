using Microsoft.ConfigurationManagement.AdminConsole.Views.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace Huawei.SCCMPlugin.PluginUI.Views
{
    public class ViewBase : OverviewControllerBase
    {

        public void InitByForm(System.Windows.Forms.Form frm)
        {
            try
            {
                WindowsFormsHost wfr = new WindowsFormsHost();

                frm.TopLevel = false;
                frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                frm.Dock = System.Windows.Forms.DockStyle.Fill;
                wfr.Child = frm;
                base.Content = wfr;
            }
            catch (ApplicationException ae)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when InitByForm(ApplicationException): ", ae);
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.UI.Error(se);
                MessageBox.Show(se.Message);
            }
        }

        public ViewBase() : base()
        {

        }

        public override void EndInit()
        {
            base.EndInit();
            //this.Content = new Label() { Content = "My Content" };
        }

    }
}
