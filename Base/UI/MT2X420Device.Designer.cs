
using Library;
using System;
using System.Windows.Forms;

namespace Base.UI
{
    partial class MT2X420Device
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bt_refresh = new DTiws.View.ButtonX();
            this.buttonX1 = new DTiws.View.ButtonX();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.buttonX3 = new DTiws.View.ButtonX();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ucSignalLamp2 = new HZH_Controls.Controls.UCSignalLamp();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.bt_RightFullDec = new DTiws.View.ButtonX();
            this.bt_RightZeroDec = new DTiws.View.ButtonX();
            this.bt_RightSure = new DTiws.View.ButtonX();
            this.bt_RightZeroAdd = new DTiws.View.ButtonX();
            this.bt_RightFullAdd = new DTiws.View.ButtonX();
            this.bt_connect2 = new DTiws.View.ButtonX();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.signalOutput2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bt_connect1 = new DTiws.View.ButtonX();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.ucSignalLamp1 = new HZH_Controls.Controls.UCSignalLamp();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bt_LeftFullDec = new DTiws.View.ButtonX();
            this.bt_LeftZeroDec = new DTiws.View.ButtonX();
            this.bt_LeftSure = new DTiws.View.ButtonX();
            this.bt_LeftZeroAdd = new DTiws.View.ButtonX();
            this.bt_LeftFullAdd = new DTiws.View.ButtonX();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.signalOutput1 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1534, 859);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel2.Controls.Add(this.bt_refresh);
            this.panel2.Controls.Add(this.buttonX1);
            this.panel2.Controls.Add(this.buttonX2);
            this.panel2.Controls.Add(this.buttonX3);
            this.panel2.Location = new System.Drawing.Point(654, 14);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(193, 833);
            this.panel2.TabIndex = 164;
            // 
            // bt_refresh
            // 
            this.bt_refresh.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bt_refresh.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_refresh.EnterForeColor = System.Drawing.Color.White;
            this.bt_refresh.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_refresh.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_refresh.HoverForeColor = System.Drawing.Color.White;
            this.bt_refresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_refresh.Location = new System.Drawing.Point(13, 33);
            this.bt_refresh.Name = "bt_refresh";
            this.bt_refresh.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_refresh.PressForeColor = System.Drawing.Color.White;
            this.bt_refresh.Radius = 6;
            this.bt_refresh.Size = new System.Drawing.Size(166, 60);
            this.bt_refresh.TabIndex = 158;
            this.bt_refresh.Text = "刷  新";
            this.bt_refresh.UseVisualStyleBackColor = true;
            this.bt_refresh.Click += new System.EventHandler(this.bt_refresh_Click);
            // 
            // buttonX1
            // 
            this.buttonX1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            this.buttonX1.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX1.Location = new System.Drawing.Point(13, 283);
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.Size = new System.Drawing.Size(166, 62);
            this.buttonX1.TabIndex = 155;
            this.buttonX1.Text = "标定零点";
            this.buttonX1.UseVisualStyleBackColor = true;
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // buttonX2
            // 
            this.buttonX2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            this.buttonX2.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX2.Location = new System.Drawing.Point(13, 418);
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 6;
            this.buttonX2.Size = new System.Drawing.Size(166, 62);
            this.buttonX2.TabIndex = 156;
            this.buttonX2.Text = "标定满点";
            this.buttonX2.UseVisualStyleBackColor = true;
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // buttonX3
            // 
            this.buttonX3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonX3.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX3.EnterForeColor = System.Drawing.Color.White;
            this.buttonX3.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX3.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX3.HoverForeColor = System.Drawing.Color.White;
            this.buttonX3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX3.Location = new System.Drawing.Point(13, 150);
            this.buttonX3.Name = "buttonX3";
            this.buttonX3.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX3.PressForeColor = System.Drawing.Color.White;
            this.buttonX3.Radius = 6;
            this.buttonX3.Size = new System.Drawing.Size(166, 62);
            this.buttonX3.TabIndex = 157;
            this.buttonX3.Text = "归  零";
            this.buttonX3.UseVisualStyleBackColor = true;
            this.buttonX3.Click += new System.EventHandler(this.buttonX3_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.ucSignalLamp2);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.bt_connect2);
            this.groupBox3.Controls.Add(this.textBox6);
            this.groupBox3.Controls.Add(this.comboBox2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.textBox5);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.signalOutput2);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.Location = new System.Drawing.Point(898, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(599, 833);
            this.groupBox3.TabIndex = 163;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "通道二";
            // 
            // ucSignalLamp2
            // 
            this.ucSignalLamp2.IsHighlight = true;
            this.ucSignalLamp2.IsShowBorder = false;
            this.ucSignalLamp2.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.Gray,
        System.Drawing.Color.Gray};
            this.ucSignalLamp2.Location = new System.Drawing.Point(70, 159);
            this.ucSignalLamp2.Name = "ucSignalLamp2";
            this.ucSignalLamp2.Size = new System.Drawing.Size(49, 49);
            this.ucSignalLamp2.TabIndex = 162;
            this.ucSignalLamp2.TwinkleSpeed = 100;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.bt_RightFullDec);
            this.groupBox4.Controls.Add(this.bt_RightZeroDec);
            this.groupBox4.Controls.Add(this.bt_RightSure);
            this.groupBox4.Controls.Add(this.bt_RightZeroAdd);
            this.groupBox4.Controls.Add(this.bt_RightFullAdd);
            this.groupBox4.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox4.Location = new System.Drawing.Point(139, 514);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(389, 252);
            this.groupBox4.TabIndex = 159;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "校准";
            // 
            // bt_RightFullDec
            // 
            this.bt_RightFullDec.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_RightFullDec.EnterForeColor = System.Drawing.Color.White;
            this.bt_RightFullDec.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_RightFullDec.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_RightFullDec.HoverForeColor = System.Drawing.Color.White;
            this.bt_RightFullDec.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_RightFullDec.Location = new System.Drawing.Point(210, 113);
            this.bt_RightFullDec.Name = "bt_RightFullDec";
            this.bt_RightFullDec.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_RightFullDec.PressForeColor = System.Drawing.Color.White;
            this.bt_RightFullDec.Radius = 6;
            this.bt_RightFullDec.Size = new System.Drawing.Size(161, 52);
            this.bt_RightFullDec.TabIndex = 147;
            this.bt_RightFullDec.Text = "满点校准 -";
            this.bt_RightFullDec.UseVisualStyleBackColor = true;
            this.bt_RightFullDec.Click += new System.EventHandler(this.bt_RightFullDec_Click);
            // 
            // bt_RightZeroDec
            // 
            this.bt_RightZeroDec.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_RightZeroDec.EnterForeColor = System.Drawing.Color.White;
            this.bt_RightZeroDec.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_RightZeroDec.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_RightZeroDec.HoverForeColor = System.Drawing.Color.White;
            this.bt_RightZeroDec.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_RightZeroDec.Location = new System.Drawing.Point(20, 113);
            this.bt_RightZeroDec.Name = "bt_RightZeroDec";
            this.bt_RightZeroDec.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_RightZeroDec.PressForeColor = System.Drawing.Color.White;
            this.bt_RightZeroDec.Radius = 6;
            this.bt_RightZeroDec.Size = new System.Drawing.Size(161, 52);
            this.bt_RightZeroDec.TabIndex = 180;
            this.bt_RightZeroDec.Text = "零点校准 -";
            this.bt_RightZeroDec.UseVisualStyleBackColor = true;
            this.bt_RightZeroDec.Click += new System.EventHandler(this.bt_LeftZeroDec_Click);
            // 
            // bt_RightSure
            // 
            this.bt_RightSure.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_RightSure.EnterForeColor = System.Drawing.Color.White;
            this.bt_RightSure.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_RightSure.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_RightSure.HoverForeColor = System.Drawing.Color.White;
            this.bt_RightSure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_RightSure.Location = new System.Drawing.Point(139, 186);
            this.bt_RightSure.Name = "bt_RightSure";
            this.bt_RightSure.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_RightSure.PressForeColor = System.Drawing.Color.White;
            this.bt_RightSure.Radius = 6;
            this.bt_RightSure.Size = new System.Drawing.Size(114, 46);
            this.bt_RightSure.TabIndex = 151;
            this.bt_RightSure.Text = "确 定";
            this.bt_RightSure.UseVisualStyleBackColor = true;
            this.bt_RightSure.Click += new System.EventHandler(this.bt_RightSure_Click);
            // 
            // bt_RightZeroAdd
            // 
            this.bt_RightZeroAdd.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_RightZeroAdd.EnterForeColor = System.Drawing.Color.White;
            this.bt_RightZeroAdd.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_RightZeroAdd.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_RightZeroAdd.HoverForeColor = System.Drawing.Color.White;
            this.bt_RightZeroAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_RightZeroAdd.Location = new System.Drawing.Point(20, 41);
            this.bt_RightZeroAdd.Name = "bt_RightZeroAdd";
            this.bt_RightZeroAdd.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_RightZeroAdd.PressForeColor = System.Drawing.Color.White;
            this.bt_RightZeroAdd.Radius = 6;
            this.bt_RightZeroAdd.Size = new System.Drawing.Size(161, 52);
            this.bt_RightZeroAdd.TabIndex = 179;
            this.bt_RightZeroAdd.Text = "零点校准 +";
            this.bt_RightZeroAdd.UseVisualStyleBackColor = true;
            this.bt_RightZeroAdd.Click += new System.EventHandler(this.bt_RightZeroAdd_Click);
            // 
            // bt_RightFullAdd
            // 
            this.bt_RightFullAdd.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_RightFullAdd.EnterForeColor = System.Drawing.Color.White;
            this.bt_RightFullAdd.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_RightFullAdd.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_RightFullAdd.HoverForeColor = System.Drawing.Color.White;
            this.bt_RightFullAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_RightFullAdd.Location = new System.Drawing.Point(210, 41);
            this.bt_RightFullAdd.Name = "bt_RightFullAdd";
            this.bt_RightFullAdd.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_RightFullAdd.PressForeColor = System.Drawing.Color.White;
            this.bt_RightFullAdd.Radius = 6;
            this.bt_RightFullAdd.Size = new System.Drawing.Size(161, 52);
            this.bt_RightFullAdd.TabIndex = 146;
            this.bt_RightFullAdd.Text = "满点校准 +";
            this.bt_RightFullAdd.UseVisualStyleBackColor = true;
            this.bt_RightFullAdd.Click += new System.EventHandler(this.bt_RightFullAdd_Click);
            // 
            // bt_connect2
            // 
            this.bt_connect2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_connect2.EnterForeColor = System.Drawing.Color.White;
            this.bt_connect2.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_connect2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_connect2.HoverForeColor = System.Drawing.Color.White;
            this.bt_connect2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_connect2.Location = new System.Drawing.Point(403, 41);
            this.bt_connect2.Name = "bt_connect2";
            this.bt_connect2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_connect2.PressForeColor = System.Drawing.Color.White;
            this.bt_connect2.Radius = 6;
            this.bt_connect2.Size = new System.Drawing.Size(113, 44);
            this.bt_connect2.TabIndex = 159;
            this.bt_connect2.Text = "连 接";
            this.bt_connect2.UseVisualStyleBackColor = true;
            this.bt_connect2.Click += new System.EventHandler(this.bt_connect2_Click);
            // 
            // textBox6
            // 
            this.textBox6.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox6.Location = new System.Drawing.Point(151, 427);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(297, 50);
            this.textBox6.TabIndex = 193;
            this.textBox6.Text = "20";
            this.textBox6.Leave += new System.EventHandler(this.output_Leave);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(151, 41);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(210, 45);
            this.comboBox2.TabIndex = 187;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label1.Location = new System.Drawing.Point(471, 430);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 37);
            this.label1.TabIndex = 185;
            this.label1.Text = "mA";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(146, 377);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 27);
            this.label2.TabIndex = 184;
            this.label2.Text = "通道二满点";
            // 
            // textBox5
            // 
            this.textBox5.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox5.Location = new System.Drawing.Point(151, 292);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(297, 50);
            this.textBox5.TabIndex = 180;
            this.textBox5.Text = "4";
            this.textBox5.Leave += new System.EventHandler(this.output_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label3.Location = new System.Drawing.Point(471, 295);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 37);
            this.label3.TabIndex = 179;
            this.label3.Text = "mA";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(146, 245);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 27);
            this.label5.TabIndex = 178;
            this.label5.Text = "通道二零点";
            // 
            // signalOutput2
            // 
            this.signalOutput2.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.signalOutput2.Location = new System.Drawing.Point(151, 159);
            this.signalOutput2.Name = "signalOutput2";
            this.signalOutput2.Size = new System.Drawing.Size(297, 50);
            this.signalOutput2.TabIndex = 174;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label6.Location = new System.Drawing.Point(471, 163);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 37);
            this.label6.TabIndex = 164;
            this.label6.Text = "mA";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(146, 115);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(208, 27);
            this.label9.TabIndex = 163;
            this.label9.Text = "通道二实际输出";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.bt_connect1);
            this.groupBox2.Controls.Add(this.textBox4);
            this.groupBox2.Controls.Add(this.ucSignalLamp1);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.signalOutput1);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(12, 14);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(599, 833);
            this.groupBox2.TabIndex = 162;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "通道一";
            // 
            // bt_connect1
            // 
            this.bt_connect1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_connect1.EnterForeColor = System.Drawing.Color.White;
            this.bt_connect1.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_connect1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_connect1.HoverForeColor = System.Drawing.Color.White;
            this.bt_connect1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_connect1.Location = new System.Drawing.Point(382, 41);
            this.bt_connect1.Name = "bt_connect1";
            this.bt_connect1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_connect1.PressForeColor = System.Drawing.Color.White;
            this.bt_connect1.Radius = 6;
            this.bt_connect1.Size = new System.Drawing.Size(113, 44);
            this.bt_connect1.TabIndex = 159;
            this.bt_connect1.Text = "连 接";
            this.bt_connect1.UseVisualStyleBackColor = true;
            this.bt_connect1.Click += new System.EventHandler(this.bt_connect1_Click);
            // 
            // textBox4
            // 
            this.textBox4.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox4.Location = new System.Drawing.Point(141, 427);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(297, 50);
            this.textBox4.TabIndex = 193;
            this.textBox4.Text = "4";
            this.textBox4.Leave += new System.EventHandler(this.output_Leave);
            // 
            // ucSignalLamp1
            // 
            this.ucSignalLamp1.IsHighlight = true;
            this.ucSignalLamp1.IsShowBorder = false;
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.Gray,
        System.Drawing.Color.Gray};
            this.ucSignalLamp1.Location = new System.Drawing.Point(64, 160);
            this.ucSignalLamp1.Name = "ucSignalLamp1";
            this.ucSignalLamp1.Size = new System.Drawing.Size(49, 49);
            this.ucSignalLamp1.TabIndex = 161;
            this.ucSignalLamp1.TwinkleSpeed = 100;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(141, 40);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(210, 45);
            this.comboBox1.TabIndex = 187;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label15.Location = new System.Drawing.Point(461, 430);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(57, 37);
            this.label15.TabIndex = 185;
            this.label15.Text = "mA";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bt_LeftFullDec);
            this.groupBox1.Controls.Add(this.bt_LeftZeroDec);
            this.groupBox1.Controls.Add(this.bt_LeftSure);
            this.groupBox1.Controls.Add(this.bt_LeftZeroAdd);
            this.groupBox1.Controls.Add(this.bt_LeftFullAdd);
            this.groupBox1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(127, 514);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(391, 252);
            this.groupBox1.TabIndex = 154;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "校准";
            // 
            // bt_LeftFullDec
            // 
            this.bt_LeftFullDec.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_LeftFullDec.EnterForeColor = System.Drawing.Color.White;
            this.bt_LeftFullDec.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_LeftFullDec.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_LeftFullDec.HoverForeColor = System.Drawing.Color.White;
            this.bt_LeftFullDec.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_LeftFullDec.Location = new System.Drawing.Point(210, 113);
            this.bt_LeftFullDec.Name = "bt_LeftFullDec";
            this.bt_LeftFullDec.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_LeftFullDec.PressForeColor = System.Drawing.Color.White;
            this.bt_LeftFullDec.Radius = 6;
            this.bt_LeftFullDec.Size = new System.Drawing.Size(161, 52);
            this.bt_LeftFullDec.TabIndex = 147;
            this.bt_LeftFullDec.Text = "满点校准 -";
            this.bt_LeftFullDec.UseVisualStyleBackColor = true;
            this.bt_LeftFullDec.Click += new System.EventHandler(this.bt_LeftFullDec_Click);
            // 
            // bt_LeftZeroDec
            // 
            this.bt_LeftZeroDec.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_LeftZeroDec.EnterForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroDec.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_LeftZeroDec.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_LeftZeroDec.HoverForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroDec.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_LeftZeroDec.Location = new System.Drawing.Point(18, 113);
            this.bt_LeftZeroDec.Name = "bt_LeftZeroDec";
            this.bt_LeftZeroDec.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_LeftZeroDec.PressForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroDec.Radius = 6;
            this.bt_LeftZeroDec.Size = new System.Drawing.Size(161, 52);
            this.bt_LeftZeroDec.TabIndex = 180;
            this.bt_LeftZeroDec.Text = "零点校准 -";
            this.bt_LeftZeroDec.UseVisualStyleBackColor = true;
            this.bt_LeftZeroDec.Click += new System.EventHandler(this.bt_LeftZeroDec_Click);
            // 
            // bt_LeftSure
            // 
            this.bt_LeftSure.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_LeftSure.EnterForeColor = System.Drawing.Color.White;
            this.bt_LeftSure.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_LeftSure.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_LeftSure.HoverForeColor = System.Drawing.Color.White;
            this.bt_LeftSure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_LeftSure.Location = new System.Drawing.Point(138, 186);
            this.bt_LeftSure.Name = "bt_LeftSure";
            this.bt_LeftSure.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_LeftSure.PressForeColor = System.Drawing.Color.White;
            this.bt_LeftSure.Radius = 6;
            this.bt_LeftSure.Size = new System.Drawing.Size(114, 46);
            this.bt_LeftSure.TabIndex = 151;
            this.bt_LeftSure.Text = "确 定";
            this.bt_LeftSure.UseVisualStyleBackColor = true;
            this.bt_LeftSure.Click += new System.EventHandler(this.bt_LeftSure_Click);
            // 
            // bt_LeftZeroAdd
            // 
            this.bt_LeftZeroAdd.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_LeftZeroAdd.EnterForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroAdd.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_LeftZeroAdd.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_LeftZeroAdd.HoverForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_LeftZeroAdd.Location = new System.Drawing.Point(18, 41);
            this.bt_LeftZeroAdd.Name = "bt_LeftZeroAdd";
            this.bt_LeftZeroAdd.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_LeftZeroAdd.PressForeColor = System.Drawing.Color.White;
            this.bt_LeftZeroAdd.Radius = 6;
            this.bt_LeftZeroAdd.Size = new System.Drawing.Size(161, 52);
            this.bt_LeftZeroAdd.TabIndex = 179;
            this.bt_LeftZeroAdd.Text = "零点校准 +";
            this.bt_LeftZeroAdd.UseVisualStyleBackColor = true;
            this.bt_LeftZeroAdd.Click += new System.EventHandler(this.bt_LeftZeroAdd_Click);
            // 
            // bt_LeftFullAdd
            // 
            this.bt_LeftFullAdd.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_LeftFullAdd.EnterForeColor = System.Drawing.Color.White;
            this.bt_LeftFullAdd.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_LeftFullAdd.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_LeftFullAdd.HoverForeColor = System.Drawing.Color.White;
            this.bt_LeftFullAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_LeftFullAdd.Location = new System.Drawing.Point(210, 41);
            this.bt_LeftFullAdd.Name = "bt_LeftFullAdd";
            this.bt_LeftFullAdd.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_LeftFullAdd.PressForeColor = System.Drawing.Color.White;
            this.bt_LeftFullAdd.Radius = 6;
            this.bt_LeftFullAdd.Size = new System.Drawing.Size(161, 52);
            this.bt_LeftFullAdd.TabIndex = 146;
            this.bt_LeftFullAdd.Text = "满点校准 +";
            this.bt_LeftFullAdd.UseVisualStyleBackColor = true;
            this.bt_LeftFullAdd.Click += new System.EventHandler(this.bt_LeftFullAdd_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label16.Location = new System.Drawing.Point(136, 380);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(152, 27);
            this.label16.TabIndex = 184;
            this.label16.Text = "通道一满点";
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox3.Location = new System.Drawing.Point(141, 292);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(297, 50);
            this.textBox3.TabIndex = 180;
            this.textBox3.Text = "20";
            this.textBox3.Leave += new System.EventHandler(this.output_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label7.Location = new System.Drawing.Point(461, 295);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 37);
            this.label7.TabIndex = 179;
            this.label7.Text = "mA";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(136, 247);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(152, 27);
            this.label8.TabIndex = 178;
            this.label8.Text = "通道一零点";
            // 
            // signalOutput1
            // 
            this.signalOutput1.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.signalOutput1.Location = new System.Drawing.Point(141, 159);
            this.signalOutput1.Name = "signalOutput1";
            this.signalOutput1.Size = new System.Drawing.Size(297, 50);
            this.signalOutput1.TabIndex = 174;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label11.Location = new System.Drawing.Point(461, 162);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 37);
            this.label11.TabIndex = 164;
            this.label11.Text = "mA";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(136, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(208, 27);
            this.label4.TabIndex = 163;
            this.label4.Text = "通道一实际输出";
            // 
            // MT2X420Device
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1534, 859);
            this.Controls.Add(this.panel1);
            this.Name = "MT2X420Device";
            this.Text = "MT2X420Device";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MT2X420Device_FormClosed);
            this.Load += new System.EventHandler(this.MT2X420Device_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private Timer timer1;
        private Timer timer2;
        private Panel panel1;
        private Panel panel2;
        private DTiws.View.ButtonX bt_refresh;
        private DTiws.View.ButtonX buttonX1;
        private DTiws.View.ButtonX buttonX2;
        private DTiws.View.ButtonX buttonX3;
        private GroupBox groupBox3;
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp2;
        private GroupBox groupBox4;
        private DTiws.View.ButtonX bt_RightFullDec;
        private DTiws.View.ButtonX bt_RightZeroDec;
        private DTiws.View.ButtonX bt_RightSure;
        private DTiws.View.ButtonX bt_RightZeroAdd;
        private DTiws.View.ButtonX bt_RightFullAdd;
        private DTiws.View.ButtonX bt_connect2;
        private TextBox textBox6;
        private ComboBox comboBox2;
        private Label label1;
        private Label label2;
        private TextBox textBox5;
        private Label label3;
        private Label label5;
        private TextBox signalOutput2;
        private Label label6;
        private Label label9;
        private GroupBox groupBox2;
        private DTiws.View.ButtonX bt_connect1;
        private TextBox textBox4;
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp1;
        private ComboBox comboBox1;
        private Label label15;
        private DTiws.View.ButtonX bt_LeftFullDec;
        private DTiws.View.ButtonX bt_LeftZeroDec;
        private DTiws.View.ButtonX bt_LeftSure;
        private DTiws.View.ButtonX bt_LeftZeroAdd;
        private DTiws.View.ButtonX bt_LeftFullAdd;
        private Label label16;
        private TextBox textBox3;
        private Label label7;
        private Label label8;
        private TextBox signalOutput1;
        private Label label11;
        private Label label4;
        private GroupBox groupBox1;
    }
}