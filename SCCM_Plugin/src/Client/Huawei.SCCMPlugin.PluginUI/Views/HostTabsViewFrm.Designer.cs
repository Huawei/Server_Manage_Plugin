namespace Huawei.SCCMPlugin.PluginUI.Views
{
  partial class HostTabsViewFrm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      //LogUtil.HWLogger.UI.Info("despose cef...");
      //if (eSightBrowser != null) eSightBrowser.di;
      //LogUtil.HWLogger.UI.Info("desposed cef.");
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.panel3 = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(840, 2);
            this.panel3.TabIndex = 1;
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 2);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.TabIndex = 5;
            // 
            // HostTabsViewFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(840, 439);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.panel3);
            this.Name = "HostTabsViewFrm";
            this.Text = "HomeFrm";
            this.Load += new System.EventHandler(this.HostTabsViewFrm_Load);
            this.ResumeLayout(false);

    }

    #endregion
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnlContent;
    }
}