using Library;
using Base.UI.MyControl;

namespace Base.UI
{
    partial class MeasureDevice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MeasureDevice));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dbListView1 = new Base.UI.MyControl.DoubleBufferListView();
            this.textBoxEx_timeInterval = new HZH_Controls.Controls.TextBoxEx();
            this.label_s = new System.Windows.Forms.Label();
            this.label_timeInterval = new System.Windows.Forms.Label();
            this.enTimeInterval = new System.Windows.Forms.CheckBox();
            this.enAutoRecord = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonX6 = new DTiws.View.ButtonX();
            this.buttonX7 = new DTiws.View.ButtonX();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.buttonX1 = new DTiws.View.ButtonX();
            this.label11 = new System.Windows.Forms.Label();
            this.tbLSL = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbUSL = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonX3 = new DTiws.View.ButtonX();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonX4 = new DTiws.View.ButtonX();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.signalStable1 = new HZH_Controls.Controls.UCSignalLamp();
            this.signalUnit1 = new System.Windows.Forms.Label();
            this.comboBox_unit = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_tare = new DTiws.View.ButtonX();
            this.button_zero = new DTiws.View.ButtonX();
            this.signalOutput1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            this.splitContainer2.Panel1.Controls.Add(this.dbListView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.textBoxEx_timeInterval);
            this.splitContainer2.Panel2.Controls.Add(this.label_s);
            this.splitContainer2.Panel2.Controls.Add(this.label_timeInterval);
            this.splitContainer2.Panel2.Controls.Add(this.enTimeInterval);
            this.splitContainer2.Panel2.Controls.Add(this.enAutoRecord);
            this.splitContainer2.Panel2.Controls.Add(this.label8);
            this.splitContainer2.Panel2.Controls.Add(this.comboBox2);
            this.splitContainer2.Panel2.Controls.Add(this.label7);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.buttonX6);
            this.splitContainer2.Panel2.Controls.Add(this.buttonX7);
            // 
            // dbListView1
            // 
            this.dbListView1.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.dbListView1, "dbListView1");
            this.dbListView1.GridLines = true;
            this.dbListView1.HideSelection = false;
            this.dbListView1.Name = "dbListView1";
            this.dbListView1.UseCompatibleStateImageBehavior = false;
            this.dbListView1.View = System.Windows.Forms.View.Details;
            // 
            // textBoxEx_timeInterval
            // 
            this.textBoxEx_timeInterval.DecLength = 2;
            this.textBoxEx_timeInterval.InputType = HZH_Controls.TextInputType.PositiveInteger;
            resources.ApplyResources(this.textBoxEx_timeInterval, "textBoxEx_timeInterval");
            this.textBoxEx_timeInterval.MaxValue = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.textBoxEx_timeInterval.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.textBoxEx_timeInterval.MyRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.textBoxEx_timeInterval.Name = "textBoxEx_timeInterval";
            this.textBoxEx_timeInterval.OldText = null;
            this.textBoxEx_timeInterval.PromptColor = System.Drawing.Color.Gray;
            this.textBoxEx_timeInterval.PromptFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxEx_timeInterval.PromptText = "";
            this.textBoxEx_timeInterval.RegexPattern = "";
            this.textBoxEx_timeInterval.TextChanged += new System.EventHandler(this.textBoxEx_timeInterval_TextChanged);
            // 
            // label_s
            // 
            resources.ApplyResources(this.label_s, "label_s");
            this.label_s.Name = "label_s";
            // 
            // label_timeInterval
            // 
            resources.ApplyResources(this.label_timeInterval, "label_timeInterval");
            this.label_timeInterval.Name = "label_timeInterval";
            // 
            // enTimeInterval
            // 
            resources.ApplyResources(this.enTimeInterval, "enTimeInterval");
            this.enTimeInterval.Name = "enTimeInterval";
            this.enTimeInterval.UseVisualStyleBackColor = true;
            this.enTimeInterval.Click += new System.EventHandler(this.enTimeInterval_Click);
            // 
            // enAutoRecord
            // 
            resources.ApplyResources(this.enAutoRecord, "enAutoRecord");
            this.enAutoRecord.Name = "enAutoRecord";
            this.enAutoRecord.UseVisualStyleBackColor = true;
            this.enAutoRecord.Click += new System.EventHandler(this.enAutoRecord_Click);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // comboBox2
            // 
            resources.ApplyResources(this.comboBox2, "comboBox2");
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            resources.GetString("comboBox2.Items"),
            resources.GetString("comboBox2.Items1"),
            resources.GetString("comboBox2.Items2")});
            this.comboBox2.Name = "comboBox2";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // buttonX6
            // 
            this.buttonX6.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX6.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX6, "buttonX6");
            this.buttonX6.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX6.HoverForeColor = System.Drawing.Color.White;
            this.buttonX6.Name = "buttonX6";
            this.buttonX6.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX6.PressForeColor = System.Drawing.Color.White;
            this.buttonX6.Radius = 6;
            this.buttonX6.UseVisualStyleBackColor = true;
            this.buttonX6.Click += new System.EventHandler(this.buttonX6_Click);
            // 
            // buttonX7
            // 
            this.buttonX7.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX7.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX7, "buttonX7");
            this.buttonX7.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX7.HoverForeColor = System.Drawing.Color.White;
            this.buttonX7.Name = "buttonX7";
            this.buttonX7.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX7.PressForeColor = System.Drawing.Color.White;
            this.buttonX7.Radius = 6;
            this.buttonX7.UseVisualStyleBackColor = true;
            this.buttonX7.Click += new System.EventHandler(this.buttonX7_Click);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listBox2);
            this.splitContainer1.Panel2.Controls.Add(this.buttonX1);
            this.splitContainer1.Panel2.Controls.Add(this.label11);
            this.splitContainer1.Panel2.Controls.Add(this.tbLSL);
            this.splitContainer1.Panel2.Controls.Add(this.label9);
            this.splitContainer1.Panel2.Controls.Add(this.tbUSL);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.buttonX3);
            this.splitContainer1.Panel2.Controls.Add(this.textBox2);
            this.splitContainer1.Panel2.Controls.Add(this.buttonX2);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.buttonX4);
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Panel2.Controls.Add(this.label10);
            this.splitContainer1.Panel2.Controls.Add(this.label12);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column4,
            this.Column5,
            this.Column3});
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.Column1, "Column1");
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.Column2, "Column2");
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.Column4, "Column4");
            this.Column4.Name = "Column4";
            // 
            // Column5
            // 
            resources.ApplyResources(this.Column5, "Column5");
            this.Column5.Name = "Column5";
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.Column3, "Column3");
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // listBox2
            // 
            this.listBox2.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.listBox2, "listBox2");
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Name = "listBox2";
            // 
            // buttonX1
            // 
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX1, "buttonX1");
            this.buttonX1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.UseVisualStyleBackColor = true;
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // tbLSL
            // 
            resources.ApplyResources(this.tbLSL, "tbLSL");
            this.tbLSL.Name = "tbLSL";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // tbUSL
            // 
            resources.ApplyResources(this.tbUSL, "tbUSL");
            this.tbUSL.Name = "tbUSL";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // buttonX3
            // 
            this.buttonX3.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX3.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX3, "buttonX3");
            this.buttonX3.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX3.HoverForeColor = System.Drawing.Color.White;
            this.buttonX3.Name = "buttonX3";
            this.buttonX3.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX3.PressForeColor = System.Drawing.Color.White;
            this.buttonX3.Radius = 6;
            this.buttonX3.UseVisualStyleBackColor = true;
            this.buttonX3.Click += new System.EventHandler(this.buttonX3_Click);
            // 
            // textBox2
            // 
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.Name = "textBox2";
            // 
            // buttonX2
            // 
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX2, "buttonX2");
            this.buttonX2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 6;
            this.buttonX2.UseVisualStyleBackColor = true;
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // buttonX4
            // 
            this.buttonX4.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX4.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX4, "buttonX4");
            this.buttonX4.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX4.HoverForeColor = System.Drawing.Color.White;
            this.buttonX4.Name = "buttonX4";
            this.buttonX4.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX4.PressForeColor = System.Drawing.Color.White;
            this.buttonX4.Radius = 6;
            this.buttonX4.UseVisualStyleBackColor = true;
            this.buttonX4.Click += new System.EventHandler(this.buttonX4_Click);
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.Leave += new System.EventHandler(this.textBox1_Leave);
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // listBox1
            // 
            resources.ApplyResources(this.listBox1, "listBox1");
            this.listBox1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Name = "listBox1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.signalStable1);
            this.panel1.Controls.Add(this.signalUnit1);
            this.panel1.Controls.Add(this.comboBox_unit);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.button_tare);
            this.panel1.Controls.Add(this.button_zero);
            this.panel1.Controls.Add(this.signalOutput1);
            this.panel1.Name = "panel1";
            this.panel1.SizeChanged += new System.EventHandler(this.signalOutput1_SizeChanged);
            // 
            // signalStable1
            // 
            this.signalStable1.IsHighlight = true;
            this.signalStable1.IsShowBorder = false;
            this.signalStable1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))))};
            resources.ApplyResources(this.signalStable1, "signalStable1");
            this.signalStable1.Name = "signalStable1";
            this.signalStable1.TwinkleSpeed = 0;
            // 
            // signalUnit1
            // 
            resources.ApplyResources(this.signalUnit1, "signalUnit1");
            this.signalUnit1.Name = "signalUnit1";
            // 
            // comboBox_unit
            // 
            this.comboBox_unit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comboBox_unit, "comboBox_unit");
            this.comboBox_unit.FormattingEnabled = true;
            this.comboBox_unit.Items.AddRange(new object[] {
            resources.GetString("comboBox_unit.Items"),
            resources.GetString("comboBox_unit.Items1"),
            resources.GetString("comboBox_unit.Items2"),
            resources.GetString("comboBox_unit.Items3"),
            resources.GetString("comboBox_unit.Items4"),
            resources.GetString("comboBox_unit.Items5"),
            resources.GetString("comboBox_unit.Items6")});
            this.comboBox_unit.Name = "comboBox_unit";
            this.comboBox_unit.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // button_tare
            // 
            resources.ApplyResources(this.button_tare, "button_tare");
            this.button_tare.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button_tare.EnterForeColor = System.Drawing.Color.White;
            this.button_tare.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button_tare.HoverForeColor = System.Drawing.Color.White;
            this.button_tare.Name = "button_tare";
            this.button_tare.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button_tare.PressForeColor = System.Drawing.Color.White;
            this.button_tare.Radius = 6;
            this.button_tare.UseVisualStyleBackColor = true;
            this.button_tare.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_zero
            // 
            resources.ApplyResources(this.button_zero, "button_zero");
            this.button_zero.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button_zero.EnterForeColor = System.Drawing.Color.White;
            this.button_zero.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button_zero.HoverForeColor = System.Drawing.Color.White;
            this.button_zero.Name = "button_zero";
            this.button_zero.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button_zero.PressForeColor = System.Drawing.Color.White;
            this.button_zero.Radius = 6;
            this.button_zero.UseVisualStyleBackColor = true;
            this.button_zero.Click += new System.EventHandler(this.button4_Click);
            // 
            // signalOutput1
            // 
            resources.ApplyResources(this.signalOutput1, "signalOutput1");
            this.signalOutput1.ForeColor = System.Drawing.Color.Black;
            this.signalOutput1.Name = "signalOutput1";
            this.signalOutput1.SizeChanged += new System.EventHandler(this.signalOutput1_SizeChanged);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pictureBox1);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.splitContainer2);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer1);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // MeasureDevice
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listBox1);
            this.Name = "MeasureDevice";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MeasureDevice_FormClosed);
            this.Load += new System.EventHandler(this.MeasureDevice_Load);
            this.SizeChanged += new System.EventHandler(this.MeasureDevice_SizeChanged);
            this.Resize += new System.EventHandler(this.MeasureDevice_Resize);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel1;
        private DTiws.View.ButtonX button_zero;
        private System.Windows.Forms.Label label1;
        private DTiws.View.ButtonX button_tare;
        private System.Windows.Forms.Label signalOutput1;
        private System.Windows.Forms.Label signalUnit1;
        private HZH_Controls.Controls.UCSignalLamp signalStable1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DTiws.View.ButtonX buttonX4;
        private DTiws.View.ButtonX buttonX3;
        private DTiws.View.ButtonX buttonX2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private DTiws.View.ButtonX buttonX6;
        private DTiws.View.ButtonX buttonX7;
        private System.Windows.Forms.Label label7;
        private DoubleBufferListView dbListView1;
        private DTiws.View.ButtonX buttonX1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbLSL;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbUSL;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.ComboBox comboBox_unit;
        private System.Windows.Forms.CheckBox enAutoRecord;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.CheckBox enTimeInterval;
        private HZH_Controls.Controls.TextBoxEx textBoxEx_timeInterval;
        private System.Windows.Forms.Label label_s;
        private System.Windows.Forms.Label label_timeInterval;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label6;
    }
}