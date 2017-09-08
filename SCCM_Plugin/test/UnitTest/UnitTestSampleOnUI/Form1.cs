using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnitTestSampleOnUI
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
      if (txtData.Text.Trim() == "")
      {
        MessageBox.Show("请输入数据！");
        return;
      }

      string myData = txtData.Text.Trim();
      lstData.Items.Add(myData);

      txtData.Text = "";
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
      if (lstData.Items.Count == 0)
      {
        MessageBox.Show("没有数据，无法移除！");
        return;
      }

      if (lstData.SelectedIndex == -1)
      {
        MessageBox.Show("请先选择数据！");
        return;
      }

      lstData.Items.Remove(lstData.SelectedItem);
    }
  }
}
