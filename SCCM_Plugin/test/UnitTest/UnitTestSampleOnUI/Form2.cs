using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Huawei.SCCMPlugin.PluginUI.Views;

namespace UnitTestSampleOnUI
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //eSight配置
        private void button1_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/eSight/eSightConfig.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //服务器列表
        private void button2_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/serverList/serverList.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //关于
        private void button3_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/about/about.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //软件源管理
        private void button4_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/softwareManager/softManager.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //模板管理
        private void button5_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/template/list.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //任务管理
        private void button6_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/task/list.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //固件升级 - 升级包管理
        private void button7_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/firmware/listFirmware.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //固件升级 - 升级任务管理
        private void button8_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("huawei/firmware/listFirmwareTask.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }
    }
}
