namespace NAudioDemo.AudioPlaybackDemo
{
    partial class AudioPlaybackPanel
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
            CloseWaveOut();
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
            this.groupBoxDriverModel = new System.Windows.Forms.GroupBox();
            this.panelOutputDeviceSettings = new System.Windows.Forms.Panel();
            this.comboBoxOutputDevice = new System.Windows.Forms.ComboBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.buttonPlay = new System.Windows.Forms.ToolStripButton();
            this.buttonPause = new System.Windows.Forms.ToolStripButton();
            this.buttonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.labelCurrentTime = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.labelTotalTime = new System.Windows.Forms.ToolStripLabel();
            this.trackBarPosition = new System.Windows.Forms.TrackBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBoxDriverModel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxDriverModel
            // 
            this.groupBoxDriverModel.Controls.Add(this.panelOutputDeviceSettings);
            this.groupBoxDriverModel.Controls.Add(this.comboBoxOutputDevice);
            this.groupBoxDriverModel.Location = new System.Drawing.Point(12, 30);
            this.groupBoxDriverModel.Name = "groupBoxDriverModel";
            this.groupBoxDriverModel.Size = new System.Drawing.Size(575, 288);
            this.groupBoxDriverModel.TabIndex = 13;
            this.groupBoxDriverModel.TabStop = false;
            this.groupBoxDriverModel.Text = "Output Driver";
            // 
            // panelOutputDeviceSettings
            // 
            this.panelOutputDeviceSettings.Location = new System.Drawing.Point(7, 52);
            this.panelOutputDeviceSettings.Name = "panelOutputDeviceSettings";
            this.panelOutputDeviceSettings.Size = new System.Drawing.Size(562, 236);
            this.panelOutputDeviceSettings.TabIndex = 1;
            // 
            // comboBoxOutputDevice
            // 
            this.comboBoxOutputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutputDevice.FormattingEnabled = true;
            this.comboBoxOutputDevice.Location = new System.Drawing.Point(6, 24);
            this.comboBoxOutputDevice.Name = "comboBoxOutputDevice";
            this.comboBoxOutputDevice.Size = new System.Drawing.Size(563, 21);
            this.comboBoxOutputDevice.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonPlay,
            this.buttonPause,
            this.buttonStop,
            this.toolStripLabel1,
            this.labelCurrentTime,
            this.toolStripLabel3,
            this.labelTotalTime});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(591, 25);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // buttonPlay
            // 
            this.buttonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(23, 22);
            this.buttonPlay.Text = "Play";
            this.buttonPlay.Click += new System.EventHandler(this.OnButtonPlayClick);
            // 
            // buttonPause
            // 
            this.buttonPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(23, 22);
            this.buttonPause.Text = "Pause";
            this.buttonPause.Click += new System.EventHandler(this.OnButtonPauseClick);
            // 
            // buttonStop
            // 
            this.buttonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(23, 22);
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.OnButtonStopClick);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(80, 22);
            this.toolStripLabel1.Text = "Current Time:";
            // 
            // labelCurrentTime
            // 
            this.labelCurrentTime.Name = "labelCurrentTime";
            this.labelCurrentTime.Size = new System.Drawing.Size(34, 22);
            this.labelCurrentTime.Text = "00:00";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(67, 22);
            this.toolStripLabel3.Text = "Total Time:";
            // 
            // labelTotalTime
            // 
            this.labelTotalTime.Name = "labelTotalTime";
            this.labelTotalTime.Size = new System.Drawing.Size(34, 22);
            this.labelTotalTime.Text = "00:00";
            // 
            // trackBarPosition
            // 
            this.trackBarPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarPosition.LargeChange = 10;
            this.trackBarPosition.Location = new System.Drawing.Point(18, 324);
            this.trackBarPosition.Maximum = 100;
            this.trackBarPosition.Name = "trackBarPosition";
            this.trackBarPosition.Size = new System.Drawing.Size(569, 45);
            this.trackBarPosition.TabIndex = 16;
            this.trackBarPosition.TickFrequency = 5;
            this.trackBarPosition.Scroll += new System.EventHandler(this.trackBarPosition_Scroll);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // AudioPlaybackPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trackBarPosition);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBoxDriverModel);
            this.Name = "AudioPlaybackPanel";
            this.Size = new System.Drawing.Size(591, 381);
            this.groupBoxDriverModel.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDriverModel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton buttonPlay;
        private System.Windows.Forms.ToolStripButton buttonPause;
        private System.Windows.Forms.ToolStripButton buttonStop;
        private System.Windows.Forms.TrackBar trackBarPosition;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenFile;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel labelCurrentTime;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripLabel labelTotalTime;
        private System.Windows.Forms.Panel panelOutputDeviceSettings;
        private System.Windows.Forms.ComboBox comboBoxOutputDevice;
    }
}