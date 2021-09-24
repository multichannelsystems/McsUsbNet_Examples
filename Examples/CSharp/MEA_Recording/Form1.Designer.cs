namespace MEA_Recording
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btMeaDevice_present = new System.Windows.Forms.Button();
            this.tbDeviceInfo = new System.Windows.Forms.TextBox();
            this.cbDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btStart = new System.Windows.Forms.Button();
            this.btStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbChannel = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxDataSelectionMethod = new System.Windows.Forms.ComboBox();
            this.checkBoxCommonThreshold = new System.Windows.Forms.CheckBox();
            this.checkBoxPollForData = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.listBoxErrors = new System.Windows.Forms.ListBox();
            this.comboBoxDataMode = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // btMeaDevice_present
            // 
            this.btMeaDevice_present.Location = new System.Drawing.Point(12, 26);
            this.btMeaDevice_present.Name = "btMeaDevice_present";
            this.btMeaDevice_present.Size = new System.Drawing.Size(138, 23);
            this.btMeaDevice_present.TabIndex = 0;
            this.btMeaDevice_present.Text = "MEA Device present?";
            this.btMeaDevice_present.UseVisualStyleBackColor = true;
            this.btMeaDevice_present.Click += new System.EventHandler(this.BtMeaDevicePresentClick);
            // 
            // tbDeviceInfo
            // 
            this.tbDeviceInfo.Location = new System.Drawing.Point(389, 12);
            this.tbDeviceInfo.Multiline = true;
            this.tbDeviceInfo.Name = "tbDeviceInfo";
            this.tbDeviceInfo.Size = new System.Drawing.Size(282, 146);
            this.tbDeviceInfo.TabIndex = 1;
            // 
            // cbDevices
            // 
            this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevices.FormattingEnabled = true;
            this.cbDevices.Location = new System.Drawing.Point(156, 28);
            this.cbDevices.Name = "cbDevices";
            this.cbDevices.Size = new System.Drawing.Size(227, 21);
            this.cbDevices.TabIndex = 2;
            this.cbDevices.SelectedIndexChanged += new System.EventHandler(this.CbDevicesSelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(156, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Devices";
            // 
            // btStart
            // 
            this.btStart.Enabled = false;
            this.btStart.Location = new System.Drawing.Point(156, 55);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(75, 23);
            this.btStart.TabIndex = 4;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.BtStartClick);
            // 
            // btStop
            // 
            this.btStop.Enabled = false;
            this.btStop.Location = new System.Drawing.Point(240, 55);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(75, 23);
            this.btStop.TabIndex = 5;
            this.btStop.Text = "Stop";
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.Click += new System.EventHandler(this.BtStopClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(321, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Sampling";
            // 
            // cbChannel
            // 
            this.cbChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChannel.FormattingEnabled = true;
            this.cbChannel.Location = new System.Drawing.Point(301, 170);
            this.cbChannel.Name = "cbChannel";
            this.cbChannel.Size = new System.Drawing.Size(70, 21);
            this.cbChannel.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(249, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Channel";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(390, 169);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 85);
            this.panel1.TabIndex = 9;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1Paint);
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Controls.Add(this.label5);
            this.groupBoxSettings.Controls.Add(this.comboBoxDataMode);
            this.groupBoxSettings.Controls.Add(this.label4);
            this.groupBoxSettings.Controls.Add(this.comboBoxDataSelectionMethod);
            this.groupBoxSettings.Controls.Add(this.checkBoxCommonThreshold);
            this.groupBoxSettings.Controls.Add(this.checkBoxPollForData);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 104);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(166, 157);
            this.groupBoxSettings.TabIndex = 10;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Data Selection Method";
            // 
            // comboBoxDataSelectionMethod
            // 
            this.comboBoxDataSelectionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataSelectionMethod.FormattingEnabled = true;
            this.comboBoxDataSelectionMethod.Location = new System.Drawing.Point(6, 32);
            this.comboBoxDataSelectionMethod.Name = "comboBoxDataSelectionMethod";
            this.comboBoxDataSelectionMethod.Size = new System.Drawing.Size(150, 21);
            this.comboBoxDataSelectionMethod.TabIndex = 12;
            this.comboBoxDataSelectionMethod.SelectedIndexChanged += new System.EventHandler(this.comboBoxDataSelectionMethod_SelectedIndexChanged);
            // 
            // checkBoxCommonThreshold
            // 
            this.checkBoxCommonThreshold.AutoSize = true;
            this.checkBoxCommonThreshold.Location = new System.Drawing.Point(6, 122);
            this.checkBoxCommonThreshold.Name = "checkBoxCommonThreshold";
            this.checkBoxCommonThreshold.Size = new System.Drawing.Size(117, 17);
            this.checkBoxCommonThreshold.TabIndex = 3;
            this.checkBoxCommonThreshold.Text = "Common Threshold";
            this.toolTip.SetToolTip(this.checkBoxCommonThreshold, "If active, data will be retrieved by (timer-controlled) polling.\r\nIf incative, da" +
        "ta will be retrieved by callbacks.");
            this.checkBoxCommonThreshold.UseVisualStyleBackColor = true;
            this.checkBoxCommonThreshold.CheckedChanged += new System.EventHandler(this.checkBoxCommonThreshold_CheckedChanged);
            // 
            // checkBoxPollForData
            // 
            this.checkBoxPollForData.AutoSize = true;
            this.checkBoxPollForData.Location = new System.Drawing.Point(6, 99);
            this.checkBoxPollForData.Name = "checkBoxPollForData";
            this.checkBoxPollForData.Size = new System.Drawing.Size(87, 17);
            this.checkBoxPollForData.TabIndex = 2;
            this.checkBoxPollForData.Text = "Poll For Data";
            this.toolTip.SetToolTip(this.checkBoxPollForData, "If active, data will be retrieved by (timer-controlled) polling.\r\nIf incative, da" +
        "ta will be retrieved by callbacks.");
            this.checkBoxPollForData.UseVisualStyleBackColor = true;
            this.checkBoxPollForData.CheckedChanged += new System.EventHandler(this.OnCheckBoxPollForDataCheckedChanged);
            // 
            // listBoxErrors
            // 
            this.listBoxErrors.FormattingEnabled = true;
            this.listBoxErrors.Location = new System.Drawing.Point(677, 12);
            this.listBoxErrors.Name = "listBoxErrors";
            this.listBoxErrors.Size = new System.Drawing.Size(170, 251);
            this.listBoxErrors.TabIndex = 11;
            // 
            // comboBoxDataMode
            // 
            this.comboBoxDataMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataMode.FormattingEnabled = true;
            this.comboBoxDataMode.Location = new System.Drawing.Point(6, 72);
            this.comboBoxDataMode.Name = "comboBoxDataMode";
            this.comboBoxDataMode.Size = new System.Drawing.Size(121, 21);
            this.comboBoxDataMode.TabIndex = 12;
            this.comboBoxDataMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxDataMode_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "DataMode";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 273);
            this.Controls.Add(this.listBoxErrors);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbChannel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btStop);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbDevices);
            this.Controls.Add(this.tbDeviceInfo);
            this.Controls.Add(this.btMeaDevice_present);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "MEA Recording Example";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1FormClosed);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btMeaDevice_present;
        private System.Windows.Forms.TextBox tbDeviceInfo;
        private System.Windows.Forms.ComboBox cbDevices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.Button btStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbChannel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.CheckBox checkBoxPollForData;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ListBox listBoxErrors;
        private System.Windows.Forms.CheckBox checkBoxCommonThreshold;
        private System.Windows.Forms.ComboBox comboBoxDataSelectionMethod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxDataMode;
        private System.Windows.Forms.Label label5;
    }
}

