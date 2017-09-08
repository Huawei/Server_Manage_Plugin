namespace UnitTestSampleOnUI
{
  partial class Form1
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
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.lstData = new System.Windows.Forms.ListBox();
      this.txtData = new System.Windows.Forms.TextBox();
      this.btnAdd = new System.Windows.Forms.Button();
      this.btnRemove = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lstData
      // 
      this.lstData.FormattingEnabled = true;
      this.lstData.Location = new System.Drawing.Point(2, 3);
      this.lstData.Name = "lstData";
      this.lstData.Size = new System.Drawing.Size(203, 303);
      this.lstData.TabIndex = 0;
      // 
      // txtData
      // 
      this.txtData.Location = new System.Drawing.Point(212, 3);
      this.txtData.Name = "txtData";
      this.txtData.Size = new System.Drawing.Size(136, 20);
      this.txtData.TabIndex = 1;
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(212, 43);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(136, 48);
      this.btnAdd.TabIndex = 2;
      this.btnAdd.Text = "添加";
      this.btnAdd.UseVisualStyleBackColor = true;
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnRemove
      // 
      this.btnRemove.Location = new System.Drawing.Point(212, 97);
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.Size = new System.Drawing.Size(136, 48);
      this.btnRemove.TabIndex = 3;
      this.btnRemove.Text = "移除";
      this.btnRemove.UseVisualStyleBackColor = true;
      this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(353, 310);
      this.Controls.Add(this.btnRemove);
      this.Controls.Add(this.btnAdd);
      this.Controls.Add(this.txtData);
      this.Controls.Add(this.lstData);
      this.Name = "Form1";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "UIUnitTestSample";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox lstData;
    private System.Windows.Forms.TextBox txtData;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRemove;
  }
}

