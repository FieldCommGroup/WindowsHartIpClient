namespace FieldCommGroup.HartIPClient
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MenuToolStrip = new System.Windows.Forms.ToolStrip();
            this.ToolBarImageList = new System.Windows.Forms.ImageList(this.components);
            this.NetConnectBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ParseResponsesBtn = new System.Windows.Forms.ToolStripButton();
            this.LogMgsBtn = new System.Windows.Forms.ToolStripButton();
            this.GetSubDeviceIdsBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutBtn = new System.Windows.Forms.ToolStripButton();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.ConnectStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.LogFileStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.ParseResponsesStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.DeviceList_cb = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ReqData_tb = new System.Windows.Forms.TextBox();
            this.SendCmd_btn = new System.Windows.Forms.Button();
            this.OutputMsg_lb = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ReqCmd_tb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DevUnivRev_lb = new System.Windows.Forms.Label();
            this.SendCmdToAll_btn = new System.Windows.Forms.Button();
            this.GetDeviceList_btn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxKeepAlive = new System.Windows.Forms.CheckBox();
            this.checkBoxSubscribeAll = new System.Windows.Forms.CheckBox();
            this.PublishedMsg_Lb = new System.Windows.Forms.ListBox();
            this.MenuToolStrip.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuToolStrip
            // 
            this.MenuToolStrip.AutoSize = false;
            this.MenuToolStrip.ImageList = this.ToolBarImageList;
            this.MenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NetConnectBtn,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.ParseResponsesBtn,
            this.LogMgsBtn,
            this.GetSubDeviceIdsBtn,
            this.toolStripSeparator2,
            this.AboutBtn});
            this.MenuToolStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuToolStrip.Name = "MenuToolStrip";
            this.MenuToolStrip.Size = new System.Drawing.Size(987, 77);
            this.MenuToolStrip.TabIndex = 1;
            this.MenuToolStrip.Text = "Menu";
            // 
            // ToolBarImageList
            // 
            this.ToolBarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolBarImageList.ImageStream")));
            this.ToolBarImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ToolBarImageList.Images.SetKeyName(0, "network-connect.png");
            this.ToolBarImageList.Images.SetKeyName(1, "network-disconnect.png");
            this.ToolBarImageList.Images.SetKeyName(2, "parse-responses.png");
            this.ToolBarImageList.Images.SetKeyName(3, "stop-parse-responses.png");
            this.ToolBarImageList.Images.SetKeyName(4, "log-file.png");
            this.ToolBarImageList.Images.SetKeyName(5, "log-pause.png");
            this.ToolBarImageList.Images.SetKeyName(6, "about.png");
            // 
            // NetConnectBtn
            // 
            this.NetConnectBtn.AutoSize = false;
            this.NetConnectBtn.CheckOnClick = true;
            this.NetConnectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NetConnectBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.NetConnectBtn.Image = ((System.Drawing.Image)(resources.GetObject("NetConnectBtn.Image")));
            this.NetConnectBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.NetConnectBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NetConnectBtn.Name = "NetConnectBtn";
            this.NetConnectBtn.Size = new System.Drawing.Size(68, 74);
            this.NetConnectBtn.Text = "Connect";
            this.NetConnectBtn.ToolTipText = "Connect to HART-IP Device";
            this.NetConnectBtn.Click += new System.EventHandler(this.NetConnectBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 77);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AutoSize = false;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(10, 22);
            // 
            // ParseResponsesBtn
            // 
            this.ParseResponsesBtn.CheckOnClick = true;
            this.ParseResponsesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ParseResponsesBtn.Image = ((System.Drawing.Image)(resources.GetObject("ParseResponsesBtn.Image")));
            this.ParseResponsesBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ParseResponsesBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ParseResponsesBtn.Name = "ParseResponsesBtn";
            this.ParseResponsesBtn.Size = new System.Drawing.Size(68, 74);
            this.ParseResponsesBtn.Text = "Parse Responses";
            this.ParseResponsesBtn.ToolTipText = "Parse HART Responses";
            this.ParseResponsesBtn.Click += new System.EventHandler(this.ParseResponsesBtn_Click);
            // 
            // LogMgsBtn
            // 
            this.LogMgsBtn.AutoSize = false;
            this.LogMgsBtn.CheckOnClick = true;
            this.LogMgsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.LogMgsBtn.Image = ((System.Drawing.Image)(resources.GetObject("LogMgsBtn.Image")));
            this.LogMgsBtn.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.LogMgsBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.LogMgsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LogMgsBtn.Name = "LogMgsBtn";
            this.LogMgsBtn.Size = new System.Drawing.Size(68, 74);
            this.LogMgsBtn.Text = "Log";
            this.LogMgsBtn.ToolTipText = "Log Messages";
            this.LogMgsBtn.Click += new System.EventHandler(this.LogFileBtn_Click);
            // 
            // GetSubDeviceIdsBtn
            // 
            this.GetSubDeviceIdsBtn.AutoSize = false;
            this.GetSubDeviceIdsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.GetSubDeviceIdsBtn.Enabled = false;
            this.GetSubDeviceIdsBtn.Image = ((System.Drawing.Image)(resources.GetObject("GetSubDeviceIdsBtn.Image")));
            this.GetSubDeviceIdsBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.GetSubDeviceIdsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GetSubDeviceIdsBtn.Name = "GetSubDeviceIdsBtn";
            this.GetSubDeviceIdsBtn.Size = new System.Drawing.Size(68, 74);
            this.GetSubDeviceIdsBtn.Text = "Get Sub-Device IDs";
            this.GetSubDeviceIdsBtn.ToolTipText = "Get Sub-Device IDs";
            this.GetSubDeviceIdsBtn.Click += new System.EventHandler(this.GetSubDevicesIds_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 77);
            // 
            // AboutBtn
            // 
            this.AboutBtn.AutoSize = false;
            this.AboutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AboutBtn.Image = ((System.Drawing.Image)(resources.GetObject("AboutBtn.Image")));
            this.AboutBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.AboutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AboutBtn.Name = "AboutBtn";
            this.AboutBtn.Size = new System.Drawing.Size(164, 74);
            this.AboutBtn.Text = "About";
            this.AboutBtn.Click += new System.EventHandler(this.AboutBtn_Click);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConnectStatus,
            this.LogFileStatus,
            this.ParseResponsesStatus});
            this.StatusStrip.Location = new System.Drawing.Point(0, 577);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(987, 22);
            this.StatusStrip.TabIndex = 2;
            // 
            // ConnectStatus
            // 
            this.ConnectStatus.Name = "ConnectStatus";
            this.ConnectStatus.Size = new System.Drawing.Size(77, 17);
            this.ConnectStatus.Text = "No Connection";
            // 
            // LogFileStatus
            // 
            this.LogFileStatus.Name = "LogFileStatus";
            this.LogFileStatus.Size = new System.Drawing.Size(114, 17);
            this.LogFileStatus.Text = "Stopped log Messages";
            // 
            // ParseResponsesStatus
            // 
            this.ParseResponsesStatus.Name = "ParseResponsesStatus";
            this.ParseResponsesStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Device Tag:";
            // 
            // DeviceList_cb
            // 
            this.DeviceList_cb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeviceList_cb.Enabled = false;
            this.DeviceList_cb.FormattingEnabled = true;
            this.DeviceList_cb.Location = new System.Drawing.Point(84, 87);
            this.DeviceList_cb.Name = "DeviceList_cb";
            this.DeviceList_cb.Size = new System.Drawing.Size(259, 21);
            this.DeviceList_cb.Sorted = true;
            this.DeviceList_cb.TabIndex = 0;
            this.DeviceList_cb.SelectedIndexChanged += new System.EventHandler(this.DeviceList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(197, 117);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Data (Hex):";
            // 
            // ReqData_tb
            // 
            this.ReqData_tb.Enabled = false;
            this.ReqData_tb.Location = new System.Drawing.Point(264, 114);
            this.ReqData_tb.MaxLength = 254;
            this.ReqData_tb.Name = "ReqData_tb";
            this.ReqData_tb.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.ReqData_tb.Size = new System.Drawing.Size(288, 20);
            this.ReqData_tb.TabIndex = 3;
            // 
            // SendCmd_btn
            // 
            this.SendCmd_btn.Enabled = false;
            this.SendCmd_btn.Location = new System.Drawing.Point(15, 139);
            this.SendCmd_btn.Name = "SendCmd_btn";
            this.SendCmd_btn.Size = new System.Drawing.Size(75, 23);
            this.SendCmd_btn.TabIndex = 4;
            this.SendCmd_btn.Text = "Send";
            this.SendCmd_btn.UseVisualStyleBackColor = true;
            this.SendCmd_btn.Click += new System.EventHandler(this.SendCmdBtn_Click);
            // 
            // OutputMsg_lb
            // 
            this.OutputMsg_lb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OutputMsg_lb.Location = new System.Drawing.Point(10, 169);
            this.OutputMsg_lb.Name = "OutputMsg_lb";
            this.OutputMsg_lb.ReadOnly = true;
            this.OutputMsg_lb.Size = new System.Drawing.Size(546, 395);
            this.OutputMsg_lb.TabIndex = 4;
            this.OutputMsg_lb.TabStop = false;
            this.OutputMsg_lb.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Command (Decimal):";
            // 
            // ReqCmd_tb
            // 
            this.ReqCmd_tb.Enabled = false;
            this.ReqCmd_tb.Location = new System.Drawing.Point(114, 114);
            this.ReqCmd_tb.Name = "ReqCmd_tb";
            this.ReqCmd_tb.Size = new System.Drawing.Size(67, 20);
            this.ReqCmd_tb.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(457, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Univ Rev:";
            // 
            // DevUnivRev_lb
            // 
            this.DevUnivRev_lb.AutoSize = true;
            this.DevUnivRev_lb.Location = new System.Drawing.Point(518, 89);
            this.DevUnivRev_lb.Name = "DevUnivRev_lb";
            this.DevUnivRev_lb.Size = new System.Drawing.Size(17, 13);
            this.DevUnivRev_lb.TabIndex = 22;
            this.DevUnivRev_lb.Text = "xx";
            // 
            // SendCmdToAll_btn
            // 
            this.SendCmdToAll_btn.Enabled = false;
            this.SendCmdToAll_btn.Location = new System.Drawing.Point(106, 139);
            this.SendCmdToAll_btn.Name = "SendCmdToAll_btn";
            this.SendCmdToAll_btn.Size = new System.Drawing.Size(75, 23);
            this.SendCmdToAll_btn.TabIndex = 5;
            this.SendCmdToAll_btn.Text = "Send To All";
            this.SendCmdToAll_btn.UseVisualStyleBackColor = true;
            this.SendCmdToAll_btn.Click += new System.EventHandler(this.SendCmdToAllBtn_Click);
            // 
            // GetDeviceList_btn
            // 
            this.GetDeviceList_btn.Enabled = false;
            this.GetDeviceList_btn.Location = new System.Drawing.Point(349, 86);
            this.GetDeviceList_btn.Name = "GetDeviceList_btn";
            this.GetDeviceList_btn.Size = new System.Drawing.Size(91, 23);
            this.GetDeviceList_btn.TabIndex = 1;
            this.GetDeviceList_btn.Text = "Get Device List";
            this.GetDeviceList_btn.UseVisualStyleBackColor = true;
            this.GetDeviceList_btn.Click += new System.EventHandler(this.GetDeviceListBtn_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(571, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Published Commands:";
            // 
            // checkBoxKeepAlive
            // 
            this.checkBoxKeepAlive.AutoSize = true;
            this.checkBoxKeepAlive.Checked = true;
            this.checkBoxKeepAlive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxKeepAlive.Location = new System.Drawing.Point(264, 143);
            this.checkBoxKeepAlive.Name = "checkBoxKeepAlive";
            this.checkBoxKeepAlive.Size = new System.Drawing.Size(161, 17);
            this.checkBoxKeepAlive.TabIndex = 25;
            this.checkBoxKeepAlive.Text = "Enable keep-alive messages";
            this.checkBoxKeepAlive.UseVisualStyleBackColor = true;
            // 
            // checkBoxSubscribeAll
            // 
            this.checkBoxSubscribeAll.AutoSize = true;
            this.checkBoxSubscribeAll.Enabled = false;
            this.checkBoxSubscribeAll.Location = new System.Drawing.Point(763, 146);
            this.checkBoxSubscribeAll.Name = "checkBoxSubscribeAll";
            this.checkBoxSubscribeAll.Size = new System.Drawing.Size(203, 17);
            this.checkBoxSubscribeAll.TabIndex = 26;
            this.checkBoxSubscribeAll.Text = "Subscribe to All Published Commands";
            this.checkBoxSubscribeAll.UseVisualStyleBackColor = true;
            this.checkBoxSubscribeAll.CheckedChanged += new System.EventHandler(this.checkBoxSubscribeAll_CheckedChanged);
            // 
            // PublishedMsg_Lb
            // 
            this.PublishedMsg_Lb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PublishedMsg_Lb.FormattingEnabled = true;
            this.PublishedMsg_Lb.Location = new System.Drawing.Point(574, 169);
            this.PublishedMsg_Lb.Name = "PublishedMsg_Lb";
            this.PublishedMsg_Lb.ScrollAlwaysVisible = true;
            this.PublishedMsg_Lb.Size = new System.Drawing.Size(401, 394);
            this.PublishedMsg_Lb.TabIndex = 27;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 599);
            this.Controls.Add(this.PublishedMsg_Lb);
            this.Controls.Add(this.checkBoxSubscribeAll);
            this.Controls.Add(this.checkBoxKeepAlive);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.GetDeviceList_btn);
            this.Controls.Add(this.SendCmdToAll_btn);
            this.Controls.Add(this.DevUnivRev_lb);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ReqCmd_tb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputMsg_lb);
            this.Controls.Add(this.SendCmd_btn);
            this.Controls.Add(this.ReqData_tb);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DeviceList_cb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.MenuToolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "HART-IP Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_OnClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MenuToolStrip.ResumeLayout(false);
            this.MenuToolStrip.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip MenuToolStrip;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ImageList ToolBarImageList;
        private System.Windows.Forms.ToolStripButton NetConnectBtn;
        private System.Windows.Forms.ToolStripButton LogMgsBtn;
        private System.Windows.Forms.ToolStripButton ParseResponsesBtn;
        private System.Windows.Forms.ToolStripStatusLabel ConnectStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox DeviceList_cb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ReqData_tb;
        private System.Windows.Forms.Button SendCmd_btn;
        private System.Windows.Forms.RichTextBox OutputMsg_lb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ReqCmd_tb;
        private System.Windows.Forms.ToolStripStatusLabel LogFileStatus;
        private System.Windows.Forms.ToolStripStatusLabel ParseResponsesStatus;
        private System.Windows.Forms.ToolStripButton AboutBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label DevUnivRev_lb;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button SendCmdToAll_btn;
        private System.Windows.Forms.ToolStripButton GetSubDeviceIdsBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.Button GetDeviceList_btn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxKeepAlive;
        private System.Windows.Forms.CheckBox checkBoxSubscribeAll;
        private System.Windows.Forms.ListBox PublishedMsg_Lb;
    }
}

