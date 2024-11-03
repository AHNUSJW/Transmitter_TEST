
namespace Base.UI.MenuSet
{
    partial class MenuFacAutoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuFacAutoForm));
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBoxConfig = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listBoxCheckMsg = new System.Windows.Forms.ListBox();
            this.ucSignalLamp1 = new HZH_Controls.Controls.UCSignalLamp();
            this.lbValue = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.bt_Refresh = new System.Windows.Forms.Button();
            this.bt_close = new System.Windows.Forms.Button();
            this.bt_Connect = new System.Windows.Forms.Button();
            this.comboBox_port = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.VAValue = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelComConStatus = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabelDMMconnectStuts = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabelTimeNow = new System.Windows.Forms.ToolStripLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.bt_full_sample = new DTiws.View.ButtonX();
            this.bt_zero_sample = new DTiws.View.ButtonX();
            this.bt_stop = new DTiws.View.ButtonX();
            this.bt_start = new DTiws.View.ButtonX();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer3
            // 
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainer1.Panel2.Controls.Add(this.ucSignalLamp1);
            this.splitContainer1.Panel2.Controls.Add(this.lbValue);
            this.splitContainer1.Panel2.Controls.Add(this.bt_full_sample);
            this.splitContainer1.Panel2.Controls.Add(this.bt_zero_sample);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel2.Controls.Add(this.VAValue);
            this.splitContainer1.Panel2.Controls.Add(this.bt_stop);
            this.splitContainer1.Panel2.Controls.Add(this.bt_start);
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.listBoxConfig);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // listBoxConfig
            // 
            this.listBoxConfig.BackColor = System.Drawing.Color.LightSteelBlue;
            this.listBoxConfig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.listBoxConfig, "listBoxConfig");
            this.listBoxConfig.FormattingEnabled = true;
            this.listBoxConfig.Name = "listBoxConfig";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.White;
            this.groupBox2.Controls.Add(this.listBoxCheckMsg);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // listBoxCheckMsg
            // 
            this.listBoxCheckMsg.BackColor = System.Drawing.Color.LightSteelBlue;
            this.listBoxCheckMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.listBoxCheckMsg, "listBoxCheckMsg");
            this.listBoxCheckMsg.FormattingEnabled = true;
            this.listBoxCheckMsg.Name = "listBoxCheckMsg";
            // 
            // ucSignalLamp1
            // 
            this.ucSignalLamp1.IsHighlight = true;
            this.ucSignalLamp1.IsShowBorder = false;
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))))};
            resources.ApplyResources(this.ucSignalLamp1, "ucSignalLamp1");
            this.ucSignalLamp1.Name = "ucSignalLamp1";
            this.ucSignalLamp1.TwinkleSpeed = 0;
            // 
            // lbValue
            // 
            resources.ApplyResources(this.lbValue, "lbValue");
            this.lbValue.Name = "lbValue";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.bt_Refresh);
            this.groupBox3.Controls.Add(this.bt_close);
            this.groupBox3.Controls.Add(this.bt_Connect);
            this.groupBox3.Controls.Add(this.comboBox_port);
            this.groupBox3.Controls.Add(this.label13);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.button3_MouseClick);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.button2_MouseClick);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.button1_MouseClick);
            // 
            // bt_Refresh
            // 
            resources.ApplyResources(this.bt_Refresh, "bt_Refresh");
            this.bt_Refresh.Name = "bt_Refresh";
            this.bt_Refresh.UseVisualStyleBackColor = false;
            this.bt_Refresh.Click += new System.EventHandler(this.bt_Refresh_Click);
            // 
            // bt_close
            // 
            resources.ApplyResources(this.bt_close, "bt_close");
            this.bt_close.Name = "bt_close";
            this.bt_close.UseVisualStyleBackColor = false;
            this.bt_close.Click += new System.EventHandler(this.bt_close_Click);
            // 
            // bt_Connect
            // 
            resources.ApplyResources(this.bt_Connect, "bt_Connect");
            this.bt_Connect.Name = "bt_Connect";
            this.bt_Connect.UseVisualStyleBackColor = false;
            this.bt_Connect.Click += new System.EventHandler(this.bt_Connect_Click);
            // 
            // comboBox_port
            // 
            this.comboBox_port.BackColor = System.Drawing.Color.Snow;
            this.comboBox_port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comboBox_port, "comboBox_port");
            this.comboBox_port.FormattingEnabled = true;
            this.comboBox_port.Name = "comboBox_port";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // VAValue
            // 
            resources.ApplyResources(this.VAValue, "VAValue");
            this.VAValue.BackColor = System.Drawing.Color.White;
            this.VAValue.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.VAValue.Name = "VAValue";
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelComConStatus,
            this.toolStripLabelDMMconnectStuts,
            this.toolStripLabel2,
            this.toolStripLabelTimeNow});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripLabelComConStatus
            // 
            this.toolStripLabelComConStatus.ActiveLinkColor = System.Drawing.Color.Firebrick;
            resources.ApplyResources(this.toolStripLabelComConStatus, "toolStripLabelComConStatus");
            this.toolStripLabelComConStatus.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.toolStripLabelComConStatus.Name = "toolStripLabelComConStatus";
            // 
            // toolStripLabelDMMconnectStuts
            // 
            this.toolStripLabelDMMconnectStuts.ForeColor = System.Drawing.Color.Red;
            this.toolStripLabelDMMconnectStuts.Name = "toolStripLabelDMMconnectStuts";
            resources.ApplyResources(this.toolStripLabelDMMconnectStuts, "toolStripLabelDMMconnectStuts");
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            // 
            // toolStripLabelTimeNow
            // 
            this.toolStripLabelTimeNow.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabelTimeNow.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.toolStripLabelTimeNow.Name = "toolStripLabelTimeNow";
            resources.ApplyResources(this.toolStripLabelTimeNow, "toolStripLabelTimeNow");
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // bt_full_sample
            // 
            this.bt_full_sample.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_full_sample.EnterForeColor = System.Drawing.Color.White;
            this.bt_full_sample.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_full_sample.HoverForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bt_full_sample, "bt_full_sample");
            this.bt_full_sample.Name = "bt_full_sample";
            this.bt_full_sample.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_full_sample.PressForeColor = System.Drawing.Color.White;
            this.bt_full_sample.Radius = 6;
            this.bt_full_sample.UseVisualStyleBackColor = true;
            this.bt_full_sample.Click += new System.EventHandler(this.bt_full_sample_Click);
            // 
            // bt_zero_sample
            // 
            this.bt_zero_sample.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_zero_sample.EnterForeColor = System.Drawing.Color.White;
            this.bt_zero_sample.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_zero_sample.HoverForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bt_zero_sample, "bt_zero_sample");
            this.bt_zero_sample.Name = "bt_zero_sample";
            this.bt_zero_sample.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_zero_sample.PressForeColor = System.Drawing.Color.White;
            this.bt_zero_sample.Radius = 6;
            this.bt_zero_sample.UseVisualStyleBackColor = true;
            this.bt_zero_sample.Click += new System.EventHandler(this.bt_zero_sample_Click);
            // 
            // bt_stop
            // 
            this.bt_stop.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_stop.EnterForeColor = System.Drawing.Color.White;
            this.bt_stop.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_stop.HoverForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bt_stop, "bt_stop");
            this.bt_stop.Name = "bt_stop";
            this.bt_stop.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_stop.PressForeColor = System.Drawing.Color.White;
            this.bt_stop.Radius = 6;
            this.bt_stop.UseVisualStyleBackColor = true;
            this.bt_stop.Click += new System.EventHandler(this.bt_stop_Click);
            // 
            // bt_start
            // 
            this.bt_start.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_start.EnterForeColor = System.Drawing.Color.White;
            this.bt_start.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_start.HoverForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bt_start, "bt_start");
            this.bt_start.Name = "bt_start";
            this.bt_start.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_start.PressForeColor = System.Drawing.Color.White;
            this.bt_start.Radius = 6;
            this.bt_start.UseVisualStyleBackColor = true;
            this.bt_start.Click += new System.EventHandler(this.bt_start_Click);
            // 
            // MenuFacAutoForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.splitContainer3);
            this.Name = "MenuFacAutoForm";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuFacAutoForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuFacAutoForm_Load);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBoxConfig;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox listBoxCheckMsg;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button bt_Refresh;
        private System.Windows.Forms.Button bt_close;
        private System.Windows.Forms.Button bt_Connect;
        private System.Windows.Forms.ComboBox comboBox_port;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label VAValue;
        private DTiws.View.ButtonX bt_stop;
        private DTiws.View.ButtonX bt_start;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelComConStatus;
        private System.Windows.Forms.ToolStripLabel toolStripLabelDMMconnectStuts;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel toolStripLabelTimeNow;
        private DTiws.View.ButtonX bt_full_sample;
        private DTiws.View.ButtonX bt_zero_sample;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lbValue;
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp1;
    }
}