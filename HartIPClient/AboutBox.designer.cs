namespace FieldCommGroup.HartIPClient
{
  partial class AboutBox
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
            this.ProductName = new System.Windows.Forms.Label();
            this.CompanyName = new System.Windows.Forms.Label();
            this.Version = new System.Windows.Forms.Label();
            this.Copyright = new System.Windows.Forms.Label();
            this.About_OKBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ProductName
            // 
            this.ProductName.AutoSize = true;
            this.ProductName.Location = new System.Drawing.Point(12, 10);
            this.ProductName.Name = "ProductName";
            this.ProductName.Size = new System.Drawing.Size(78, 13);
            this.ProductName.TabIndex = 0;
            this.ProductName.Text = "Product Name:";
            // 
            // CompanyName
            // 
            this.CompanyName.AutoSize = true;
            this.CompanyName.Location = new System.Drawing.Point(12, 37);
            this.CompanyName.Name = "CompanyName";
            this.CompanyName.Size = new System.Drawing.Size(85, 13);
            this.CompanyName.TabIndex = 1;
            this.CompanyName.Text = "Company Name:";
            // 
            // Version
            // 
            this.Version.AutoSize = true;
            this.Version.Location = new System.Drawing.Point(178, 10);
            this.Version.Name = "Version";
            this.Version.Size = new System.Drawing.Size(27, 13);
            this.Version.TabIndex = 3;
            this.Version.Text = "xxxx";
            // 
            // Copyright
            // 
            this.Copyright.AutoSize = true;
            this.Copyright.Location = new System.Drawing.Point(12, 64);
            this.Copyright.Name = "Copyright";
            this.Copyright.Size = new System.Drawing.Size(51, 13);
            this.Copyright.TabIndex = 4;
            this.Copyright.Text = "Copyright";
            // 
            // About_OKBtn
            // 
            this.About_OKBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.About_OKBtn.Location = new System.Drawing.Point(181, 86);
            this.About_OKBtn.Name = "About_OKBtn";
            this.About_OKBtn.Size = new System.Drawing.Size(75, 23);
            this.About_OKBtn.TabIndex = 5;
            this.About_OKBtn.Text = "OK";
            this.About_OKBtn.UseVisualStyleBackColor = true;
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 113);
            this.Controls.Add(this.About_OKBtn);
            this.Controls.Add(this.Copyright);
            this.Controls.Add(this.Version);
            this.Controls.Add(this.CompanyName);
            this.Controls.Add(this.ProductName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private new System.Windows.Forms.Label ProductName;
    private new System.Windows.Forms.Label CompanyName;
    private System.Windows.Forms.Label Version;
    private System.Windows.Forms.Label Copyright;
    private System.Windows.Forms.Button About_OKBtn;

  }
}
