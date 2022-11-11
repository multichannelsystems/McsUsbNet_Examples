
namespace MEA2100_Recording_and_Stimulation
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btRecordingStart = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btRecordingStop = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btStimulatinStop = new System.Windows.Forms.Button();
            this.btStimulationStart = new System.Windows.Forms.Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbDeviceList = new System.Windows.Forms.ComboBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btRecordingStart
            // 
            this.btRecordingStart.Location = new System.Drawing.Point(6, 19);
            this.btRecordingStart.Name = "btRecordingStart";
            this.btRecordingStart.Size = new System.Drawing.Size(75, 23);
            this.btRecordingStart.TabIndex = 0;
            this.btRecordingStart.Text = "Start";
            this.btRecordingStart.UseVisualStyleBackColor = true;
            this.btRecordingStart.Click += new System.EventHandler(this.btRecordingStart_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btRecordingStop);
            this.groupBox1.Controls.Add(this.btRecordingStart);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(172, 51);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Recording";
            // 
            // btRecordingStop
            // 
            this.btRecordingStop.Location = new System.Drawing.Point(87, 19);
            this.btRecordingStop.Name = "btRecordingStop";
            this.btRecordingStop.Size = new System.Drawing.Size(75, 23);
            this.btRecordingStop.TabIndex = 1;
            this.btRecordingStop.Text = "Stop";
            this.btRecordingStop.UseVisualStyleBackColor = true;
            this.btRecordingStop.Click += new System.EventHandler(this.btRecordingStop_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btStimulatinStop);
            this.groupBox2.Controls.Add(this.btStimulationStart);
            this.groupBox2.Location = new System.Drawing.Point(190, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(171, 51);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Stimulate";
            // 
            // btStimulatinStop
            // 
            this.btStimulatinStop.Location = new System.Drawing.Point(87, 19);
            this.btStimulatinStop.Name = "btStimulatinStop";
            this.btStimulatinStop.Size = new System.Drawing.Size(75, 23);
            this.btStimulatinStop.TabIndex = 3;
            this.btStimulatinStop.Text = "Stop";
            this.btStimulatinStop.UseVisualStyleBackColor = true;
            this.btStimulatinStop.Click += new System.EventHandler(this.btStimulatinStop_Click);
            // 
            // btStimulationStart
            // 
            this.btStimulationStart.Location = new System.Drawing.Point(6, 19);
            this.btStimulationStart.Name = "btStimulationStart";
            this.btStimulationStart.Size = new System.Drawing.Size(75, 23);
            this.btStimulationStart.TabIndex = 2;
            this.btStimulationStart.Text = "Start";
            this.btStimulationStart.UseVisualStyleBackColor = true;
            this.btStimulationStart.Click += new System.EventHandler(this.btStimulationStart_Click);
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Location = new System.Drawing.Point(12, 69);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.Name = "Series2";
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(776, 369);
            this.chart1.TabIndex = 3;
            this.chart1.Text = "chart1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cbDeviceList);
            this.groupBox3.Location = new System.Drawing.Point(367, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 51);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Device";
            // 
            // cbDeviceList
            // 
            this.cbDeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDeviceList.FormattingEnabled = true;
            this.cbDeviceList.Location = new System.Drawing.Point(6, 21);
            this.cbDeviceList.Name = "cbDeviceList";
            this.cbDeviceList.Size = new System.Drawing.Size(188, 21);
            this.cbDeviceList.TabIndex = 0;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(794, 69);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(220, 368);
            this.listBox1.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1026, 450);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btRecordingStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btRecordingStop;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btStimulatinStop;
        private System.Windows.Forms.Button btStimulationStart;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cbDeviceList;
        private System.Windows.Forms.ListBox listBox1;
    }
}

