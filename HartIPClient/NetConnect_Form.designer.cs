namespace FieldCommGroup.HartIPClient
{
  partial class NetConnect_Form
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
            this.label1 = new System.Windows.Forms.Label();
            this.IPAdd_tb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Port_tb = new System.Windows.Forms.TextBox();
            this.Tcp_RBtn = new System.Windows.Forms.RadioButton();
            this.Udp_RBtn = new System.Windows.Forms.RadioButton();
            this.OK_btn = new System.Windows.Forms.Button();
            this.Cancel_btn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.DrDelayBase_tb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DrRetries_tb = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SocketTimeout_tb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "HART-IP Device IP Address:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // IPAdd_tb
            // 
            this.IPAdd_tb.Location = new System.Drawing.Point(181, 12);
            this.IPAdd_tb.Name = "IPAdd_tb";
            this.IPAdd_tb.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.IPAdd_tb.Size = new System.Drawing.Size(217, 20);
            this.IPAdd_tb.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(404, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port:";
            // 
            // Port_tb
            // 
            this.Port_tb.Location = new System.Drawing.Point(439, 12);
            this.Port_tb.Name = "Port_tb";
            this.Port_tb.Size = new System.Drawing.Size(56, 20);
            this.Port_tb.TabIndex = 1;
            this.Port_tb.Text = "5094";
            // 
            // Tcp_RBtn
            // 
            this.Tcp_RBtn.AutoSize = true;
            this.Tcp_RBtn.Checked = true;
            this.Tcp_RBtn.Location = new System.Drawing.Point(77, 19);
            this.Tcp_RBtn.Name = "Tcp_RBtn";
            this.Tcp_RBtn.Size = new System.Drawing.Size(44, 17);
            this.Tcp_RBtn.TabIndex = 1;
            this.Tcp_RBtn.TabStop = true;
            this.Tcp_RBtn.Text = "Tcp";
            this.Tcp_RBtn.UseVisualStyleBackColor = true;
            this.Tcp_RBtn.CheckedChanged += new System.EventHandler(this.Tcp_RBtn_CheckedChanged);
            // 
            // Udp_RBtn
            // 
            this.Udp_RBtn.AutoSize = true;
            this.Udp_RBtn.Location = new System.Drawing.Point(16, 19);
            this.Udp_RBtn.Name = "Udp_RBtn";
            this.Udp_RBtn.Size = new System.Drawing.Size(45, 17);
            this.Udp_RBtn.TabIndex = 0;
            this.Udp_RBtn.Text = "Udp";
            this.Udp_RBtn.UseVisualStyleBackColor = true;
            // 
            // OK_btn
            // 
            this.OK_btn.Location = new System.Drawing.Point(156, 168);
            this.OK_btn.Name = "OK_btn";
            this.OK_btn.Size = new System.Drawing.Size(75, 23);
            this.OK_btn.TabIndex = 5;
            this.OK_btn.Text = "OK";
            this.OK_btn.UseVisualStyleBackColor = true;
            this.OK_btn.Click += new System.EventHandler(this.OK_btn_Click);
            // 
            // Cancel_btn
            // 
            this.Cancel_btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_btn.Location = new System.Drawing.Point(275, 168);
            this.Cancel_btn.Name = "Cancel_btn";
            this.Cancel_btn.Size = new System.Drawing.Size(75, 23);
            this.Cancel_btn.TabIndex = 6;
            this.Cancel_btn.Text = "Cancel";
            this.Cancel_btn.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "DR Retry Delay Base (ms):";
            // 
            // DrDelayBase_tb
            // 
            this.DrDelayBase_tb.Location = new System.Drawing.Point(145, 23);
            this.DrDelayBase_tb.Name = "DrDelayBase_tb";
            this.DrDelayBase_tb.Size = new System.Drawing.Size(86, 20);
            this.DrDelayBase_tb.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(287, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "DR Retries:";
            // 
            // DrRetries_tb
            // 
            this.DrRetries_tb.Location = new System.Drawing.Point(356, 23);
            this.DrRetries_tb.Name = "DrRetries_tb";
            this.DrRetries_tb.Size = new System.Drawing.Size(66, 20);
            this.DrRetries_tb.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.DrRetries_tb);
            this.groupBox1.Controls.Add(this.DrDelayBase_tb);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(10, 104);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(485, 58);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Request Retry Delay:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.SocketTimeout_tb);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.Udp_RBtn);
            this.groupBox2.Controls.Add(this.Tcp_RBtn);
            this.groupBox2.Location = new System.Drawing.Point(11, 43);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(484, 48);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connection Type:";
            // 
            // SocketTimeout_tb
            // 
            this.SocketTimeout_tb.Location = new System.Drawing.Point(390, 19);
            this.SocketTimeout_tb.Name = "SocketTimeout_tb";
            this.SocketTimeout_tb.Size = new System.Drawing.Size(88, 20);
            this.SocketTimeout_tb.TabIndex = 4;
            this.SocketTimeout_tb.Text = "30";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(194, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(193, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Session Timeout (seconds of inactivity):";
            // 
            // NetConnect_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(507, 205);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Cancel_btn);
            this.Controls.Add(this.OK_btn);
            this.Controls.Add(this.Port_tb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.IPAdd_tb);
            this.Controls.Add(this.label1);
            this.Name = "NetConnect_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Network Connection";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox IPAdd_tb;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox Port_tb;
    private System.Windows.Forms.RadioButton Tcp_RBtn;
    private System.Windows.Forms.RadioButton Udp_RBtn;
    private System.Windows.Forms.Button OK_btn;
    private System.Windows.Forms.Button Cancel_btn;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox DrDelayBase_tb;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox DrRetries_tb;
	private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TextBox SocketTimeout_tb;
    private System.Windows.Forms.Label label5;
    }
}