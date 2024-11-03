
using Library;
using System;
using System.Windows.Forms;

namespace Base.UI
{
    partial class RTUDevice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RTUDevice));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxData = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label8_dataSum = new System.Windows.Forms.Label();
            this.groupBoxCurve = new System.Windows.Forms.GroupBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dbListView1 = new Base.UI.MyControl.DoubleBufferListView();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label_Data = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonX_Recoord = new DTiws.View.ButtonX();
            this.buttonX_Clear = new DTiws.View.ButtonX();
            this.groupBoxAddr = new System.Windows.Forms.GroupBox();
            this.textBox_Addr = new System.Windows.Forms.TextBox();
            this.buttonX_Addr = new DTiws.View.ButtonX();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.textBox_Weight = new System.Windows.Forms.TextBox();
            this.groupBoxSet = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonX_QNET = new DTiws.View.ButtonX();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonX_QGROSS = new DTiws.View.ButtonX();
            this.comboBoxUnit = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBoxAddr = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonX_AZERO = new DTiws.View.ButtonX();
            this.buttonX_TARE = new DTiws.View.ButtonX();
            this.buttonX_ZERO = new DTiws.View.ButtonX();
            this.panel4 = new System.Windows.Forms.Panel();
            this.groupBoxCal = new System.Windows.Forms.GroupBox();
            this.labeloutunit = new System.Windows.Forms.Label();
            this.bt_Write = new DTiws.View.ButtonX();
            this.bt_Max = new DTiws.View.ButtonX();
            this.bt_Zero = new DTiws.View.ButtonX();
            this.label5 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.textBox_Zero = new System.Windows.Forms.TextBox();
            this.textBox_Max = new System.Windows.Forms.TextBox();
            this.groupBoxPara = new System.Windows.Forms.GroupBox();
            this.buttonX_Para = new DTiws.View.ButtonX();
            this.labelCal5 = new System.Windows.Forms.Label();
            this.listBoxTkdynatime = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal3 = new System.Windows.Forms.Label();
            this.listBoxTkzerotime = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal2 = new System.Windows.Forms.Label();
            this.listBoxAntivib = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal6 = new System.Windows.Forms.Label();
            this.listBoxDynazero = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal8 = new System.Windows.Forms.Label();
            this.listBoxFiltertime = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal7 = new System.Windows.Forms.Label();
            this.listBoxFilterange = new Base.UI.MyControl.LimitedSelectionListBox();
            this.labelCal1 = new System.Windows.Forms.Label();
            this.labelCal4 = new System.Windows.Forms.Label();
            this.listBoxAutozero = new Base.UI.MyControl.LimitedSelectionListBox();
            this.listBoxTrackzero = new Base.UI.MyControl.LimitedSelectionListBox();
            this.groupBoxConnect = new System.Windows.Forms.GroupBox();
            this.groupBoxConfig = new System.Windows.Forms.GroupBox();
            this.comboBox1_FullScreen = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label7_RTUConfig = new System.Windows.Forms.Label();
            this.buttonX_Config = new System.Windows.Forms.Button();
            this.comboBox1_AutoConnect = new System.Windows.Forms.ComboBox();
            this.comboBox3_AutoStart = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bt_send3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.bt_send2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.comboBox4_Parity = new System.Windows.Forms.ComboBox();
            this.comboBox3_StopBits = new System.Windows.Forms.ComboBox();
            this.comboBox2_BaudRate = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.comboBox1_port = new System.Windows.Forms.ComboBox();
            this.timerConnect = new System.Windows.Forms.Timer(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.groupBoxData.SuspendLayout();
            this.panel5.SuspendLayout();
            this.groupBoxCurve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxAddr.SuspendLayout();
            this.groupBoxSet.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBoxCal.SuspendLayout();
            this.groupBoxPara.SuspendLayout();
            this.groupBoxConnect.SuspendLayout();
            this.groupBoxConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.Controls.Add(this.groupBoxData);
            this.panel1.Controls.Add(this.groupBoxCurve);
            this.panel1.Controls.Add(this.groupBoxAddr);
            this.panel1.Controls.Add(this.groupBoxSet);
            this.panel1.Controls.Add(this.groupBoxCal);
            this.panel1.Controls.Add(this.groupBoxPara);
            this.panel1.Controls.Add(this.groupBoxConnect);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1904, 1011);
            this.panel1.TabIndex = 0;
            // 
            // groupBoxData
            // 
            this.groupBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxData.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxData.Controls.Add(this.panel5);
            this.groupBoxData.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxData.Location = new System.Drawing.Point(423, 4);
            this.groupBoxData.Name = "groupBoxData";
            this.groupBoxData.Size = new System.Drawing.Size(1478, 675);
            this.groupBoxData.TabIndex = 178;
            this.groupBoxData.TabStop = false;
            this.groupBoxData.Text = "测量值";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1472, 616);
            this.tableLayoutPanel1.TabIndex = 141;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.Controls.Add(this.label8_dataSum);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(3, 632);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1472, 40);
            this.panel5.TabIndex = 142;
            // 
            // label8_dataSum
            // 
            this.label8_dataSum.AutoSize = true;
            this.label8_dataSum.Font = new System.Drawing.Font("宋体", 12F);
            this.label8_dataSum.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8_dataSum.Location = new System.Drawing.Point(15, 12);
            this.label8_dataSum.Name = "label8_dataSum";
            this.label8_dataSum.Size = new System.Drawing.Size(87, 16);
            this.label8_dataSum.TabIndex = 139;
            this.label8_dataSum.Text = "数据加总：";
            // 
            // groupBoxCurve
            // 
            this.groupBoxCurve.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurve.Controls.Add(this.splitContainer2);
            this.groupBoxCurve.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxCurve.Location = new System.Drawing.Point(424, 685);
            this.groupBoxCurve.Name = "groupBoxCurve";
            this.groupBoxCurve.Size = new System.Drawing.Size(1009, 322);
            this.groupBoxCurve.TabIndex = 180;
            this.groupBoxCurve.TabStop = false;
            this.groupBoxCurve.Text = "数据曲线";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 19);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.splitContainer2.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.AliceBlue;
            this.splitContainer2.Panel2.Controls.Add(this.dbListView1);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.comboBox2);
            this.splitContainer2.Panel2.Controls.Add(this.label_Data);
            this.splitContainer2.Panel2.Controls.Add(this.label11);
            this.splitContainer2.Panel2.Controls.Add(this.buttonX_Recoord);
            this.splitContainer2.Panel2.Controls.Add(this.buttonX_Clear);
            this.splitContainer2.Size = new System.Drawing.Size(1003, 300);
            this.splitContainer2.SplitterDistance = 719;
            this.splitContainer2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(715, 296);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // dbListView1
            // 
            this.dbListView1.HideSelection = false;
            this.dbListView1.Location = new System.Drawing.Point(137, 234);
            this.dbListView1.Name = "dbListView1";
            this.dbListView1.Size = new System.Drawing.Size(67, 44);
            this.dbListView1.TabIndex = 129;
            this.dbListView1.UseCompatibleStateImageBehavior = false;
            this.dbListView1.View = System.Windows.Forms.View.Details;
            this.dbListView1.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 15.75F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(133, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 21);
            this.label6.TabIndex = 128;
            this.label6.Text = "min";
            // 
            // comboBox2
            // 
            this.comboBox2.Font = new System.Drawing.Font("宋体", 14.25F);
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "30",
            "60",
            "120"});
            this.comboBox2.Location = new System.Drawing.Point(49, 66);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(78, 27);
            this.comboBox2.TabIndex = 127;
            // 
            // label_Data
            // 
            this.label_Data.AutoSize = true;
            this.label_Data.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label_Data.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_Data.Location = new System.Drawing.Point(48, 253);
            this.label_Data.Name = "label_Data";
            this.label_Data.Size = new System.Drawing.Size(77, 14);
            this.label_Data.TabIndex = 126;
            this.label_Data.Text = "未记录数据";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold);
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(43, 30);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(120, 21);
            this.label11.TabIndex = 1;
            this.label11.Text = "记录时间：";
            // 
            // buttonX_Recoord
            // 
            this.buttonX_Recoord.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_Recoord.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_Recoord.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_Recoord.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_Recoord.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_Recoord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_Recoord.Location = new System.Drawing.Point(49, 125);
            this.buttonX_Recoord.Name = "buttonX_Recoord";
            this.buttonX_Recoord.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_Recoord.PressForeColor = System.Drawing.Color.White;
            this.buttonX_Recoord.Radius = 6;
            this.buttonX_Recoord.Size = new System.Drawing.Size(115, 38);
            this.buttonX_Recoord.TabIndex = 11;
            this.buttonX_Recoord.Text = "开始记录";
            this.buttonX_Recoord.UseVisualStyleBackColor = true;
            this.buttonX_Recoord.Click += new System.EventHandler(this.buttonX_Recoord_Click);
            // 
            // buttonX_Clear
            // 
            this.buttonX_Clear.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_Clear.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_Clear.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_Clear.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_Clear.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_Clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_Clear.Location = new System.Drawing.Point(49, 180);
            this.buttonX_Clear.Name = "buttonX_Clear";
            this.buttonX_Clear.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_Clear.PressForeColor = System.Drawing.Color.White;
            this.buttonX_Clear.Radius = 6;
            this.buttonX_Clear.Size = new System.Drawing.Size(115, 38);
            this.buttonX_Clear.TabIndex = 10;
            this.buttonX_Clear.Text = "清空曲线";
            this.buttonX_Clear.UseVisualStyleBackColor = true;
            this.buttonX_Clear.Click += new System.EventHandler(this.buttonX_Clear_Click);
            // 
            // groupBoxAddr
            // 
            this.groupBoxAddr.Controls.Add(this.textBox_Addr);
            this.groupBoxAddr.Controls.Add(this.buttonX_Addr);
            this.groupBoxAddr.Controls.Add(this.label23);
            this.groupBoxAddr.Controls.Add(this.label24);
            this.groupBoxAddr.Controls.Add(this.textBox_Weight);
            this.groupBoxAddr.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAddr.Location = new System.Drawing.Point(3, 879);
            this.groupBoxAddr.Name = "groupBoxAddr";
            this.groupBoxAddr.Size = new System.Drawing.Size(414, 128);
            this.groupBoxAddr.TabIndex = 179;
            this.groupBoxAddr.TabStop = false;
            this.groupBoxAddr.Text = "分配站点";
            // 
            // textBox_Addr
            // 
            this.textBox_Addr.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox_Addr.Location = new System.Drawing.Point(136, 77);
            this.textBox_Addr.Name = "textBox_Addr";
            this.textBox_Addr.Size = new System.Drawing.Size(100, 23);
            this.textBox_Addr.TabIndex = 139;
            this.textBox_Addr.TextChanged += new System.EventHandler(this.textBoxAddr_TextChanged);
            this.textBox_Addr.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_Addr_KeyPress);
            // 
            // buttonX_Addr
            // 
            this.buttonX_Addr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX_Addr.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_Addr.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_Addr.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_Addr.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_Addr.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_Addr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_Addr.Location = new System.Drawing.Point(271, 73);
            this.buttonX_Addr.Name = "buttonX_Addr";
            this.buttonX_Addr.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_Addr.PressForeColor = System.Drawing.Color.White;
            this.buttonX_Addr.Radius = 6;
            this.buttonX_Addr.Size = new System.Drawing.Size(86, 32);
            this.buttonX_Addr.TabIndex = 138;
            this.buttonX_Addr.Text = "确  定";
            this.buttonX_Addr.UseVisualStyleBackColor = true;
            this.buttonX_Addr.Click += new System.EventHandler(this.buttonX_Addr_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label23.Location = new System.Drawing.Point(66, 37);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(49, 14);
            this.label23.TabIndex = 26;
            this.label23.Text = "重量：";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label24.Location = new System.Drawing.Point(66, 81);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(49, 14);
            this.label24.TabIndex = 27;
            this.label24.Text = "站点：";
            // 
            // textBox_Weight
            // 
            this.textBox_Weight.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox_Weight.Location = new System.Drawing.Point(136, 34);
            this.textBox_Weight.Name = "textBox_Weight";
            this.textBox_Weight.Size = new System.Drawing.Size(100, 23);
            this.textBox_Weight.TabIndex = 28;
            this.textBox_Weight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_Weight_KeyPress);
            // 
            // groupBoxSet
            // 
            this.groupBoxSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSet.Controls.Add(this.panel3);
            this.groupBoxSet.Controls.Add(this.panel2);
            this.groupBoxSet.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxSet.Location = new System.Drawing.Point(1441, 685);
            this.groupBoxSet.Name = "groupBoxSet";
            this.groupBoxSet.Size = new System.Drawing.Size(459, 323);
            this.groupBoxSet.TabIndex = 177;
            this.groupBoxSet.TabStop = false;
            this.groupBoxSet.Text = "状态设置";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonX_QNET);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.buttonX_QGROSS);
            this.panel3.Controls.Add(this.comboBoxUnit);
            this.panel3.Location = new System.Drawing.Point(0, 22);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(459, 143);
            this.panel3.TabIndex = 140;
            this.panel3.Paint += new System.Windows.Forms.PaintEventHandler(this.panel3_Paint);
            // 
            // buttonX_QNET
            // 
            this.buttonX_QNET.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_QNET.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_QNET.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_QNET.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_QNET.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_QNET.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_QNET.Location = new System.Drawing.Point(173, 79);
            this.buttonX_QNET.Name = "buttonX_QNET";
            this.buttonX_QNET.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_QNET.PressForeColor = System.Drawing.Color.White;
            this.buttonX_QNET.Radius = 6;
            this.buttonX_QNET.Size = new System.Drawing.Size(115, 38);
            this.buttonX_QNET.TabIndex = 135;
            this.buttonX_QNET.Text = "净  重";
            this.buttonX_QNET.UseVisualStyleBackColor = true;
            this.buttonX_QNET.Click += new System.EventHandler(this.buttonX_QNET_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 12F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(43, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 16);
            this.label4.TabIndex = 138;
            this.label4.Text = "单位:";
            // 
            // buttonX_QGROSS
            // 
            this.buttonX_QGROSS.BackColor = System.Drawing.Color.White;
            this.buttonX_QGROSS.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_QGROSS.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_QGROSS.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_QGROSS.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_QGROSS.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_QGROSS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_QGROSS.Location = new System.Drawing.Point(41, 79);
            this.buttonX_QGROSS.Name = "buttonX_QGROSS";
            this.buttonX_QGROSS.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_QGROSS.PressForeColor = System.Drawing.Color.White;
            this.buttonX_QGROSS.Radius = 6;
            this.buttonX_QGROSS.Size = new System.Drawing.Size(115, 38);
            this.buttonX_QGROSS.TabIndex = 134;
            this.buttonX_QGROSS.Text = "毛  重";
            this.buttonX_QGROSS.UseVisualStyleBackColor = false;
            this.buttonX_QGROSS.Click += new System.EventHandler(this.buttonX_QGROSS_Click);
            // 
            // comboBoxUnit
            // 
            this.comboBoxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUnit.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBoxUnit.FormattingEnabled = true;
            this.comboBoxUnit.Items.AddRange(new object[] {
            "无"});
            this.comboBoxUnit.Location = new System.Drawing.Point(118, 28);
            this.comboBoxUnit.Name = "comboBoxUnit";
            this.comboBoxUnit.Size = new System.Drawing.Size(124, 27);
            this.comboBoxUnit.TabIndex = 137;
            this.comboBoxUnit.SelectedIndexChanged += new System.EventHandler(this.comboBoxUnit_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboBoxAddr);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.buttonX_AZERO);
            this.panel2.Controls.Add(this.buttonX_TARE);
            this.panel2.Controls.Add(this.buttonX_ZERO);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Location = new System.Drawing.Point(3, 164);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(453, 153);
            this.panel2.TabIndex = 139;
            // 
            // comboBoxAddr
            // 
            this.comboBoxAddr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAddr.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBoxAddr.FormattingEnabled = true;
            this.comboBoxAddr.Location = new System.Drawing.Point(118, 28);
            this.comboBoxAddr.Name = "comboBoxAddr";
            this.comboBoxAddr.Size = new System.Drawing.Size(124, 27);
            this.comboBoxAddr.TabIndex = 133;
            this.comboBoxAddr.SelectedIndexChanged += new System.EventHandler(this.comboBoxAddr_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(43, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 16);
            this.label1.TabIndex = 136;
            this.label1.Text = "站点:";
            // 
            // buttonX_AZERO
            // 
            this.buttonX_AZERO.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_AZERO.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_AZERO.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_AZERO.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_AZERO.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_AZERO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_AZERO.Location = new System.Drawing.Point(301, 82);
            this.buttonX_AZERO.Name = "buttonX_AZERO";
            this.buttonX_AZERO.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_AZERO.PressForeColor = System.Drawing.Color.White;
            this.buttonX_AZERO.Radius = 6;
            this.buttonX_AZERO.Size = new System.Drawing.Size(115, 38);
            this.buttonX_AZERO.TabIndex = 14;
            this.buttonX_AZERO.Text = "全部归零";
            this.buttonX_AZERO.UseVisualStyleBackColor = true;
            this.buttonX_AZERO.Click += new System.EventHandler(this.buttonX_AZERO_Click);
            // 
            // buttonX_TARE
            // 
            this.buttonX_TARE.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_TARE.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_TARE.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_TARE.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_TARE.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_TARE.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_TARE.Location = new System.Drawing.Point(38, 82);
            this.buttonX_TARE.Name = "buttonX_TARE";
            this.buttonX_TARE.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_TARE.PressForeColor = System.Drawing.Color.White;
            this.buttonX_TARE.Radius = 6;
            this.buttonX_TARE.Size = new System.Drawing.Size(115, 38);
            this.buttonX_TARE.TabIndex = 12;
            this.buttonX_TARE.Text = "去  皮";
            this.buttonX_TARE.UseVisualStyleBackColor = true;
            this.buttonX_TARE.Click += new System.EventHandler(this.buttonX_TARE_Click);
            // 
            // buttonX_ZERO
            // 
            this.buttonX_ZERO.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_ZERO.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_ZERO.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_ZERO.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_ZERO.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_ZERO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_ZERO.Location = new System.Drawing.Point(170, 82);
            this.buttonX_ZERO.Name = "buttonX_ZERO";
            this.buttonX_ZERO.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_ZERO.PressForeColor = System.Drawing.Color.White;
            this.buttonX_ZERO.Radius = 6;
            this.buttonX_ZERO.Size = new System.Drawing.Size(115, 38);
            this.buttonX_ZERO.TabIndex = 13;
            this.buttonX_ZERO.Text = "归  零";
            this.buttonX_ZERO.UseVisualStyleBackColor = true;
            this.buttonX_ZERO.Click += new System.EventHandler(this.buttonX_ZERO_Click);
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(163, 72);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(263, 57);
            this.panel4.TabIndex = 137;
            this.panel4.Paint += new System.Windows.Forms.PaintEventHandler(this.panel4_Paint);
            // 
            // groupBoxCal
            // 
            this.groupBoxCal.Controls.Add(this.labeloutunit);
            this.groupBoxCal.Controls.Add(this.bt_Write);
            this.groupBoxCal.Controls.Add(this.bt_Max);
            this.groupBoxCal.Controls.Add(this.bt_Zero);
            this.groupBoxCal.Controls.Add(this.label5);
            this.groupBoxCal.Controls.Add(this.label18);
            this.groupBoxCal.Controls.Add(this.textBox_Zero);
            this.groupBoxCal.Controls.Add(this.textBox_Max);
            this.groupBoxCal.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxCal.Location = new System.Drawing.Point(3, 710);
            this.groupBoxCal.Name = "groupBoxCal";
            this.groupBoxCal.Size = new System.Drawing.Size(414, 163);
            this.groupBoxCal.TabIndex = 176;
            this.groupBoxCal.TabStop = false;
            this.groupBoxCal.Text = "标 定";
            // 
            // labeloutunit
            // 
            this.labeloutunit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labeloutunit.AutoSize = true;
            this.labeloutunit.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labeloutunit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labeloutunit.Location = new System.Drawing.Point(40, 128);
            this.labeloutunit.Name = "labeloutunit";
            this.labeloutunit.Size = new System.Drawing.Size(77, 12);
            this.labeloutunit.TabIndex = 139;
            this.labeloutunit.Text = "重量单位: kg";
            // 
            // bt_Write
            // 
            this.bt_Write.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_Write.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_Write.EnterForeColor = System.Drawing.Color.White;
            this.bt_Write.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_Write.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.bt_Write.HoverForeColor = System.Drawing.Color.White;
            this.bt_Write.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_Write.Location = new System.Drawing.Point(271, 117);
            this.bt_Write.Name = "bt_Write";
            this.bt_Write.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_Write.PressForeColor = System.Drawing.Color.White;
            this.bt_Write.Radius = 6;
            this.bt_Write.Size = new System.Drawing.Size(86, 32);
            this.bt_Write.TabIndex = 138;
            this.bt_Write.Text = "写  入";
            this.bt_Write.UseVisualStyleBackColor = true;
            this.bt_Write.Click += new System.EventHandler(this.bt_Write_Click);
            // 
            // bt_Max
            // 
            this.bt_Max.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_Max.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_Max.EnterForeColor = System.Drawing.Color.White;
            this.bt_Max.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_Max.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.bt_Max.HoverForeColor = System.Drawing.Color.White;
            this.bt_Max.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_Max.Location = new System.Drawing.Point(271, 73);
            this.bt_Max.Name = "bt_Max";
            this.bt_Max.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_Max.PressForeColor = System.Drawing.Color.White;
            this.bt_Max.Radius = 6;
            this.bt_Max.Size = new System.Drawing.Size(86, 32);
            this.bt_Max.TabIndex = 129;
            this.bt_Max.Text = "满点采集";
            this.bt_Max.UseVisualStyleBackColor = true;
            this.bt_Max.Click += new System.EventHandler(this.bt_Max_Click);
            // 
            // bt_Zero
            // 
            this.bt_Zero.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_Zero.BackColor = System.Drawing.Color.White;
            this.bt_Zero.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_Zero.EnterForeColor = System.Drawing.Color.White;
            this.bt_Zero.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_Zero.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.bt_Zero.HoverForeColor = System.Drawing.Color.White;
            this.bt_Zero.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_Zero.Location = new System.Drawing.Point(271, 28);
            this.bt_Zero.Name = "bt_Zero";
            this.bt_Zero.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_Zero.PressForeColor = System.Drawing.Color.White;
            this.bt_Zero.Radius = 6;
            this.bt_Zero.Size = new System.Drawing.Size(86, 32);
            this.bt_Zero.TabIndex = 128;
            this.bt_Zero.Text = "零点采集";
            this.bt_Zero.UseVisualStyleBackColor = false;
            this.bt_Zero.Click += new System.EventHandler(this.bt_Zero_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(38, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 14);
            this.label5.TabIndex = 26;
            this.label5.Text = "零点重量：";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label18.Location = new System.Drawing.Point(38, 83);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(77, 14);
            this.label18.TabIndex = 27;
            this.label18.Text = "满点重量：";
            // 
            // textBox_Zero
            // 
            this.textBox_Zero.Enabled = false;
            this.textBox_Zero.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox_Zero.Location = new System.Drawing.Point(136, 35);
            this.textBox_Zero.Name = "textBox_Zero";
            this.textBox_Zero.Size = new System.Drawing.Size(100, 23);
            this.textBox_Zero.TabIndex = 28;
            // 
            // textBox_Max
            // 
            this.textBox_Max.Enabled = false;
            this.textBox_Max.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox_Max.Location = new System.Drawing.Point(136, 79);
            this.textBox_Max.Name = "textBox_Max";
            this.textBox_Max.Size = new System.Drawing.Size(100, 23);
            this.textBox_Max.TabIndex = 29;
            // 
            // groupBoxPara
            // 
            this.groupBoxPara.Controls.Add(this.buttonX_Para);
            this.groupBoxPara.Controls.Add(this.labelCal5);
            this.groupBoxPara.Controls.Add(this.listBoxTkdynatime);
            this.groupBoxPara.Controls.Add(this.labelCal3);
            this.groupBoxPara.Controls.Add(this.listBoxTkzerotime);
            this.groupBoxPara.Controls.Add(this.labelCal2);
            this.groupBoxPara.Controls.Add(this.listBoxAntivib);
            this.groupBoxPara.Controls.Add(this.labelCal6);
            this.groupBoxPara.Controls.Add(this.listBoxDynazero);
            this.groupBoxPara.Controls.Add(this.labelCal8);
            this.groupBoxPara.Controls.Add(this.listBoxFiltertime);
            this.groupBoxPara.Controls.Add(this.labelCal7);
            this.groupBoxPara.Controls.Add(this.listBoxFilterange);
            this.groupBoxPara.Controls.Add(this.labelCal1);
            this.groupBoxPara.Controls.Add(this.labelCal4);
            this.groupBoxPara.Controls.Add(this.listBoxAutozero);
            this.groupBoxPara.Controls.Add(this.listBoxTrackzero);
            this.groupBoxPara.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxPara.ForeColor = System.Drawing.Color.Black;
            this.groupBoxPara.Location = new System.Drawing.Point(3, 260);
            this.groupBoxPara.Name = "groupBoxPara";
            this.groupBoxPara.Size = new System.Drawing.Size(414, 448);
            this.groupBoxPara.TabIndex = 167;
            this.groupBoxPara.TabStop = false;
            this.groupBoxPara.Text = "参数设置";
            // 
            // buttonX_Para
            // 
            this.buttonX_Para.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX_Para.BackColor = System.Drawing.Color.White;
            this.buttonX_Para.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX_Para.EnterForeColor = System.Drawing.Color.White;
            this.buttonX_Para.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_Para.HoverBackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_Para.HoverForeColor = System.Drawing.Color.White;
            this.buttonX_Para.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_Para.Location = new System.Drawing.Point(271, 399);
            this.buttonX_Para.Name = "buttonX_Para";
            this.buttonX_Para.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX_Para.PressForeColor = System.Drawing.Color.White;
            this.buttonX_Para.Radius = 6;
            this.buttonX_Para.Size = new System.Drawing.Size(86, 32);
            this.buttonX_Para.TabIndex = 129;
            this.buttonX_Para.Text = "确  定";
            this.buttonX_Para.UseVisualStyleBackColor = false;
            this.buttonX_Para.Visible = false;
            this.buttonX_Para.Click += new System.EventHandler(this.buttonX_Para_Click);
            // 
            // labelCal5
            // 
            this.labelCal5.AutoSize = true;
            this.labelCal5.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal5.Location = new System.Drawing.Point(37, 213);
            this.labelCal5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCal5.Name = "labelCal5";
            this.labelCal5.Size = new System.Drawing.Size(42, 14);
            this.labelCal5.TabIndex = 49;
            this.labelCal5.Text = "参数5";
            this.labelCal5.Visible = false;
            // 
            // listBoxTkdynatime
            // 
            this.listBoxTkdynatime.AdminMode = false;
            this.listBoxTkdynatime.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxTkdynatime.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxTkdynatime.ForeColor = System.Drawing.Color.Black;
            this.listBoxTkdynatime.FormattingEnabled = true;
            this.listBoxTkdynatime.ItemHeight = 14;
            this.listBoxTkdynatime.Items.AddRange(new object[] {
            "0.1 s",
            "0.2 s",
            "0.3 s",
            "0.4 s",
            "0.5 s",
            "0.6 s",
            "0.7 s",
            "0.8 s",
            "0.9 s",
            "1.0 s",
            "1.1 s",
            "1.2 s",
            "1.3 s",
            "1.4 s",
            "1.5 s",
            "1.6 s",
            "1.7 s",
            "1.8 s",
            "1.9 s",
            "2.0 s",
            "2.1 s",
            "2.2 s",
            "2.3 s",
            "2.4 s",
            "2.5 s",
            "2.6 s",
            "2.7 s",
            "2.8 s",
            "2.9 s",
            "3.0 s"});
            this.listBoxTkdynatime.Location = new System.Drawing.Point(40, 228);
            this.listBoxTkdynatime.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxTkdynatime.Name = "listBoxTkdynatime";
            this.listBoxTkdynatime.Size = new System.Drawing.Size(115, 60);
            this.listBoxTkdynatime.TabIndex = 50;
            this.listBoxTkdynatime.Visible = false;
            // 
            // labelCal3
            // 
            this.labelCal3.AutoSize = true;
            this.labelCal3.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal3.Location = new System.Drawing.Point(35, 119);
            this.labelCal3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCal3.Name = "labelCal3";
            this.labelCal3.Size = new System.Drawing.Size(42, 14);
            this.labelCal3.TabIndex = 47;
            this.labelCal3.Text = "参数3";
            this.labelCal3.Visible = false;
            // 
            // listBoxTkzerotime
            // 
            this.listBoxTkzerotime.AdminMode = false;
            this.listBoxTkzerotime.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxTkzerotime.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxTkzerotime.ForeColor = System.Drawing.Color.Black;
            this.listBoxTkzerotime.FormattingEnabled = true;
            this.listBoxTkzerotime.ItemHeight = 14;
            this.listBoxTkzerotime.Items.AddRange(new object[] {
            "0.1 s",
            "0.2 s",
            "0.3 s",
            "0.4 s",
            "0.5 s",
            "0.6 s",
            "0.7 s",
            "0.8 s",
            "0.9 s",
            "1.0 s",
            "1.1 s",
            "1.2 s",
            "1.3 s",
            "1.4 s",
            "1.5 s",
            "1.6 s",
            "1.7 s",
            "1.8 s",
            "1.9 s",
            "2.0 s",
            "2.1 s",
            "2.2 s",
            "2.3 s",
            "2.4 s",
            "2.5 s",
            "2.6 s",
            "2.7 s",
            "2.8 s",
            "2.9 s",
            "3.0 s"});
            this.listBoxTkzerotime.Location = new System.Drawing.Point(38, 136);
            this.listBoxTkzerotime.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxTkzerotime.Name = "listBoxTkzerotime";
            this.listBoxTkzerotime.Size = new System.Drawing.Size(115, 60);
            this.listBoxTkzerotime.TabIndex = 48;
            this.listBoxTkzerotime.Visible = false;
            // 
            // labelCal2
            // 
            this.labelCal2.AutoSize = true;
            this.labelCal2.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal2.Location = new System.Drawing.Point(206, 27);
            this.labelCal2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCal2.Name = "labelCal2";
            this.labelCal2.Size = new System.Drawing.Size(42, 14);
            this.labelCal2.TabIndex = 19;
            this.labelCal2.Text = "参数2";
            this.labelCal2.Visible = false;
            // 
            // listBoxAntivib
            // 
            this.listBoxAntivib.AdminMode = false;
            this.listBoxAntivib.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxAntivib.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxAntivib.ForeColor = System.Drawing.Color.Black;
            this.listBoxAntivib.FormattingEnabled = true;
            this.listBoxAntivib.ItemHeight = 14;
            this.listBoxAntivib.Items.AddRange(new object[] {
            "LV 0",
            "LV 1",
            "LV 2",
            "LV 3",
            "LV 4",
            "LV 5",
            "LV 6",
            "LV 7",
            "LV 8",
            "LV 9",
            "LV 10"});
            this.listBoxAntivib.Location = new System.Drawing.Point(209, 43);
            this.listBoxAntivib.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxAntivib.Name = "listBoxAntivib";
            this.listBoxAntivib.Size = new System.Drawing.Size(115, 60);
            this.listBoxAntivib.TabIndex = 20;
            this.listBoxAntivib.Visible = false;
            // 
            // labelCal6
            // 
            this.labelCal6.AutoSize = true;
            this.labelCal6.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal6.Location = new System.Drawing.Point(208, 213);
            this.labelCal6.Name = "labelCal6";
            this.labelCal6.Size = new System.Drawing.Size(42, 14);
            this.labelCal6.TabIndex = 15;
            this.labelCal6.Text = "参数6";
            this.labelCal6.Visible = false;
            // 
            // listBoxDynazero
            // 
            this.listBoxDynazero.AdminMode = false;
            this.listBoxDynazero.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxDynazero.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxDynazero.ForeColor = System.Drawing.Color.Black;
            this.listBoxDynazero.FormattingEnabled = true;
            this.listBoxDynazero.ItemHeight = 14;
            this.listBoxDynazero.Items.AddRange(new object[] {
            "0 e",
            "0.5 e",
            "1 e",
            "2 e",
            "3 e",
            "4 e",
            "5 e",
            "6 e",
            "7 e",
            "8 e",
            "9 e",
            "10 e",
            "20 e",
            "30 e"});
            this.listBoxDynazero.Location = new System.Drawing.Point(211, 228);
            this.listBoxDynazero.Name = "listBoxDynazero";
            this.listBoxDynazero.Size = new System.Drawing.Size(115, 60);
            this.listBoxDynazero.TabIndex = 16;
            this.listBoxDynazero.Visible = false;
            // 
            // labelCal8
            // 
            this.labelCal8.AutoSize = true;
            this.labelCal8.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal8.Location = new System.Drawing.Point(208, 303);
            this.labelCal8.Name = "labelCal8";
            this.labelCal8.Size = new System.Drawing.Size(42, 14);
            this.labelCal8.TabIndex = 13;
            this.labelCal8.Text = "参数8";
            this.labelCal8.Visible = false;
            // 
            // listBoxFiltertime
            // 
            this.listBoxFiltertime.AdminMode = false;
            this.listBoxFiltertime.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxFiltertime.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxFiltertime.ForeColor = System.Drawing.Color.Black;
            this.listBoxFiltertime.FormattingEnabled = true;
            this.listBoxFiltertime.ItemHeight = 14;
            this.listBoxFiltertime.Items.AddRange(new object[] {
            "208 ms",
            "416 ms",
            "624 ms",
            "832 ms",
            "1040 ms",
            "1248 ms",
            "1456 ms",
            "1664 ms",
            "1872 ms",
            "2080 ms",
            "2288 ms"});
            this.listBoxFiltertime.Location = new System.Drawing.Point(211, 319);
            this.listBoxFiltertime.Name = "listBoxFiltertime";
            this.listBoxFiltertime.Size = new System.Drawing.Size(115, 60);
            this.listBoxFiltertime.TabIndex = 14;
            this.listBoxFiltertime.Visible = false;
            // 
            // labelCal7
            // 
            this.labelCal7.AutoSize = true;
            this.labelCal7.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal7.Location = new System.Drawing.Point(39, 303);
            this.labelCal7.Name = "labelCal7";
            this.labelCal7.Size = new System.Drawing.Size(42, 14);
            this.labelCal7.TabIndex = 11;
            this.labelCal7.Text = "参数7";
            this.labelCal7.Visible = false;
            // 
            // listBoxFilterange
            // 
            this.listBoxFilterange.AdminMode = false;
            this.listBoxFilterange.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxFilterange.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxFilterange.ForeColor = System.Drawing.Color.Black;
            this.listBoxFilterange.FormattingEnabled = true;
            this.listBoxFilterange.ItemHeight = 14;
            this.listBoxFilterange.Items.AddRange(new object[] {
            "0 e",
            "1 e",
            "2 e",
            "3 e",
            "4 e",
            "5 e",
            "6 e",
            "7 e",
            "8 e",
            "9 e",
            "10 e"});
            this.listBoxFilterange.Location = new System.Drawing.Point(40, 319);
            this.listBoxFilterange.Name = "listBoxFilterange";
            this.listBoxFilterange.Size = new System.Drawing.Size(115, 60);
            this.listBoxFilterange.TabIndex = 12;
            this.listBoxFilterange.Visible = false;
            // 
            // labelCal1
            // 
            this.labelCal1.AutoSize = true;
            this.labelCal1.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal1.Location = new System.Drawing.Point(35, 27);
            this.labelCal1.Name = "labelCal1";
            this.labelCal1.Size = new System.Drawing.Size(42, 14);
            this.labelCal1.TabIndex = 6;
            this.labelCal1.Text = "参数1";
            this.labelCal1.Visible = false;
            // 
            // labelCal4
            // 
            this.labelCal4.AutoSize = true;
            this.labelCal4.Font = new System.Drawing.Font("宋体", 10F);
            this.labelCal4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCal4.Location = new System.Drawing.Point(206, 119);
            this.labelCal4.Name = "labelCal4";
            this.labelCal4.Size = new System.Drawing.Size(42, 14);
            this.labelCal4.TabIndex = 7;
            this.labelCal4.Text = "参数4";
            this.labelCal4.Visible = false;
            // 
            // listBoxAutozero
            // 
            this.listBoxAutozero.AdminMode = false;
            this.listBoxAutozero.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxAutozero.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxAutozero.ForeColor = System.Drawing.Color.Black;
            this.listBoxAutozero.FormattingEnabled = true;
            this.listBoxAutozero.ItemHeight = 14;
            this.listBoxAutozero.Items.AddRange(new object[] {
            "0%  Range",
            "2%  Range",
            "4%  Range",
            "10% Range",
            "20% Range",
            "50% Range"});
            this.listBoxAutozero.Location = new System.Drawing.Point(38, 43);
            this.listBoxAutozero.Name = "listBoxAutozero";
            this.listBoxAutozero.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listBoxAutozero.Size = new System.Drawing.Size(115, 60);
            this.listBoxAutozero.TabIndex = 9;
            this.listBoxAutozero.Visible = false;
            // 
            // listBoxTrackzero
            // 
            this.listBoxTrackzero.AdminMode = false;
            this.listBoxTrackzero.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxTrackzero.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxTrackzero.ForeColor = System.Drawing.Color.Black;
            this.listBoxTrackzero.FormattingEnabled = true;
            this.listBoxTrackzero.ItemHeight = 14;
            this.listBoxTrackzero.Items.AddRange(new object[] {
            "0 e",
            "0.5 e",
            "1 e",
            "2 e",
            "3 e",
            "4 e",
            "5 e",
            "6 e",
            "7 e",
            "8 e",
            "9 e",
            "10 e",
            "20 e",
            "30 e",
            "40 e",
            "50 e"});
            this.listBoxTrackzero.Location = new System.Drawing.Point(209, 136);
            this.listBoxTrackzero.Name = "listBoxTrackzero";
            this.listBoxTrackzero.Size = new System.Drawing.Size(115, 60);
            this.listBoxTrackzero.TabIndex = 10;
            this.listBoxTrackzero.Visible = false;
            // 
            // groupBoxConnect
            // 
            this.groupBoxConnect.Controls.Add(this.groupBoxConfig);
            this.groupBoxConnect.Controls.Add(this.bt_send3);
            this.groupBoxConnect.Controls.Add(this.textBox1);
            this.groupBoxConnect.Controls.Add(this.label17);
            this.groupBoxConnect.Controls.Add(this.button2);
            this.groupBoxConnect.Controls.Add(this.bt_send2);
            this.groupBoxConnect.Controls.Add(this.button4);
            this.groupBoxConnect.Controls.Add(this.comboBox4_Parity);
            this.groupBoxConnect.Controls.Add(this.comboBox3_StopBits);
            this.groupBoxConnect.Controls.Add(this.comboBox2_BaudRate);
            this.groupBoxConnect.Controls.Add(this.label13);
            this.groupBoxConnect.Controls.Add(this.label14);
            this.groupBoxConnect.Controls.Add(this.label15);
            this.groupBoxConnect.Controls.Add(this.label16);
            this.groupBoxConnect.Controls.Add(this.comboBox1_port);
            this.groupBoxConnect.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxConnect.Location = new System.Drawing.Point(3, 3);
            this.groupBoxConnect.Name = "groupBoxConnect";
            this.groupBoxConnect.Size = new System.Drawing.Size(414, 255);
            this.groupBoxConnect.TabIndex = 166;
            this.groupBoxConnect.TabStop = false;
            this.groupBoxConnect.Text = "设备连接";
            // 
            // groupBoxConfig
            // 
            this.groupBoxConfig.Controls.Add(this.comboBox1_FullScreen);
            this.groupBoxConfig.Controls.Add(this.label7);
            this.groupBoxConfig.Controls.Add(this.label7_RTUConfig);
            this.groupBoxConfig.Controls.Add(this.buttonX_Config);
            this.groupBoxConfig.Controls.Add(this.comboBox1_AutoConnect);
            this.groupBoxConfig.Controls.Add(this.comboBox3_AutoStart);
            this.groupBoxConfig.Controls.Add(this.label2);
            this.groupBoxConfig.Controls.Add(this.label3);
            this.groupBoxConfig.Location = new System.Drawing.Point(199, 69);
            this.groupBoxConfig.Name = "groupBoxConfig";
            this.groupBoxConfig.Size = new System.Drawing.Size(200, 127);
            this.groupBoxConfig.TabIndex = 59;
            this.groupBoxConfig.TabStop = false;
            this.groupBoxConfig.Text = "软件设置";
            this.groupBoxConfig.Visible = false;
            // 
            // comboBox1_FullScreen
            // 
            this.comboBox1_FullScreen.BackColor = System.Drawing.Color.Snow;
            this.comboBox1_FullScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1_FullScreen.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1_FullScreen.FormattingEnabled = true;
            this.comboBox1_FullScreen.Items.AddRange(new object[] {
            "关闭",
            "开启"});
            this.comboBox1_FullScreen.Location = new System.Drawing.Point(81, 69);
            this.comboBox1_FullScreen.Name = "comboBox1_FullScreen";
            this.comboBox1_FullScreen.Size = new System.Drawing.Size(104, 22);
            this.comboBox1_FullScreen.TabIndex = 56;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(7, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 14);
            this.label7.TabIndex = 55;
            this.label7.Text = "全 屏:";
            // 
            // label7_RTUConfig
            // 
            this.label7_RTUConfig.AutoSize = true;
            this.label7_RTUConfig.Location = new System.Drawing.Point(13, 101);
            this.label7_RTUConfig.Name = "label7_RTUConfig";
            this.label7_RTUConfig.Size = new System.Drawing.Size(0, 17);
            this.label7_RTUConfig.TabIndex = 54;
            // 
            // buttonX_Config
            // 
            this.buttonX_Config.BackColor = System.Drawing.Color.CadetBlue;
            this.buttonX_Config.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX_Config.ForeColor = System.Drawing.Color.White;
            this.buttonX_Config.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonX_Config.Location = new System.Drawing.Point(119, 93);
            this.buttonX_Config.Name = "buttonX_Config";
            this.buttonX_Config.Size = new System.Drawing.Size(66, 34);
            this.buttonX_Config.TabIndex = 53;
            this.buttonX_Config.Text = "确 定";
            this.buttonX_Config.UseVisualStyleBackColor = false;
            this.buttonX_Config.Click += new System.EventHandler(this.buttonX_Config_Click);
            // 
            // comboBox1_AutoConnect
            // 
            this.comboBox1_AutoConnect.BackColor = System.Drawing.Color.Snow;
            this.comboBox1_AutoConnect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1_AutoConnect.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1_AutoConnect.FormattingEnabled = true;
            this.comboBox1_AutoConnect.Items.AddRange(new object[] {
            "关闭",
            "开启"});
            this.comboBox1_AutoConnect.Location = new System.Drawing.Point(81, 43);
            this.comboBox1_AutoConnect.Name = "comboBox1_AutoConnect";
            this.comboBox1_AutoConnect.Size = new System.Drawing.Size(104, 22);
            this.comboBox1_AutoConnect.TabIndex = 51;
            // 
            // comboBox3_AutoStart
            // 
            this.comboBox3_AutoStart.BackColor = System.Drawing.Color.Snow;
            this.comboBox3_AutoStart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3_AutoStart.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox3_AutoStart.FormattingEnabled = true;
            this.comboBox3_AutoStart.Items.AddRange(new object[] {
            "关闭",
            "开启"});
            this.comboBox3_AutoStart.Location = new System.Drawing.Point(81, 16);
            this.comboBox3_AutoStart.Name = "comboBox3_AutoStart";
            this.comboBox3_AutoStart.Size = new System.Drawing.Size(104, 22);
            this.comboBox3_AutoStart.TabIndex = 52;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(7, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 14);
            this.label2.TabIndex = 49;
            this.label2.Text = "开机启动:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(7, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 14);
            this.label3.TabIndex = 50;
            this.label3.Text = "自动连接:";
            // 
            // bt_send3
            // 
            this.bt_send3.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_send3.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_send3.ForeColor = System.Drawing.Color.White;
            this.bt_send3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_send3.Location = new System.Drawing.Point(269, 28);
            this.bt_send3.Name = "bt_send3";
            this.bt_send3.Size = new System.Drawing.Size(62, 38);
            this.bt_send3.TabIndex = 58;
            this.bt_send3.Text = "连 接";
            this.bt_send3.UseVisualStyleBackColor = false;
            this.bt_send3.Click += new System.EventHandler(this.bt_send3_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(121, 205);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(64, 23);
            this.textBox1.TabIndex = 53;
            this.textBox1.TextChanged += new System.EventHandler(this.textBoxAddr_TextChanged);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_Addr_KeyPress);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label17.Location = new System.Drawing.Point(15, 209);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(91, 14);
            this.label17.TabIndex = 49;
            this.label17.Text = "站点(1-255):";
            this.label17.DoubleClick += new System.EventHandler(this.label17_DoubleClick);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.CadetBlue;
            this.button2.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button2.Location = new System.Drawing.Point(339, 28);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(62, 38);
            this.button2.TabIndex = 50;
            this.button2.Text = "关 闭";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2Close_Click);
            // 
            // bt_send2
            // 
            this.bt_send2.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_send2.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_send2.ForeColor = System.Drawing.Color.White;
            this.bt_send2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_send2.Location = new System.Drawing.Point(199, 197);
            this.bt_send2.Name = "bt_send2";
            this.bt_send2.Size = new System.Drawing.Size(86, 38);
            this.bt_send2.TabIndex = 51;
            this.bt_send2.Text = "扫 描";
            this.bt_send2.UseVisualStyleBackColor = false;
            this.bt_send2.Click += new System.EventHandler(this.bt_send2_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.CadetBlue;
            this.button4.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button4.Location = new System.Drawing.Point(199, 28);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(62, 38);
            this.button4.TabIndex = 52;
            this.button4.Text = "刷 新";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // comboBox4_Parity
            // 
            this.comboBox4_Parity.BackColor = System.Drawing.Color.Snow;
            this.comboBox4_Parity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox4_Parity.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox4_Parity.FormattingEnabled = true;
            this.comboBox4_Parity.Items.AddRange(new object[] {
            "None",
            "Odd(奇校验)",
            "Even(偶校验)",
            "Mark",
            "Space"});
            this.comboBox4_Parity.Location = new System.Drawing.Point(81, 161);
            this.comboBox4_Parity.Name = "comboBox4_Parity";
            this.comboBox4_Parity.Size = new System.Drawing.Size(104, 22);
            this.comboBox4_Parity.TabIndex = 47;
            this.comboBox4_Parity.SelectedIndexChanged += new System.EventHandler(this.omboBox4_Parity_SelectedIndexChanged);
            // 
            // comboBox3_StopBits
            // 
            this.comboBox3_StopBits.BackColor = System.Drawing.Color.Snow;
            this.comboBox3_StopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3_StopBits.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox3_StopBits.FormattingEnabled = true;
            this.comboBox3_StopBits.Items.AddRange(new object[] {
            "1",
            "2"});
            this.comboBox3_StopBits.Location = new System.Drawing.Point(81, 118);
            this.comboBox3_StopBits.Name = "comboBox3_StopBits";
            this.comboBox3_StopBits.Size = new System.Drawing.Size(104, 22);
            this.comboBox3_StopBits.TabIndex = 48;
            this.comboBox3_StopBits.SelectedIndexChanged += new System.EventHandler(this.comboBox3_StopBits_SelectedIndexChanged);
            // 
            // comboBox2_BaudRate
            // 
            this.comboBox2_BaudRate.BackColor = System.Drawing.Color.Snow;
            this.comboBox2_BaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2_BaudRate.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox2_BaudRate.FormattingEnabled = true;
            this.comboBox2_BaudRate.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBox2_BaudRate.Location = new System.Drawing.Point(81, 76);
            this.comboBox2_BaudRate.Name = "comboBox2_BaudRate";
            this.comboBox2_BaudRate.Size = new System.Drawing.Size(104, 22);
            this.comboBox2_BaudRate.TabIndex = 46;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(29, 41);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 14);
            this.label13.TabIndex = 41;
            this.label13.Text = "串口：";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label14.Location = new System.Drawing.Point(15, 80);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 14);
            this.label14.TabIndex = 42;
            this.label14.Text = "波特率：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(15, 122);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 14);
            this.label15.TabIndex = 43;
            this.label15.Text = "停止位：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label16.Location = new System.Drawing.Point(15, 165);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(63, 14);
            this.label16.TabIndex = 44;
            this.label16.Text = "校验位：";
            // 
            // comboBox1_port
            // 
            this.comboBox1_port.BackColor = System.Drawing.Color.Snow;
            this.comboBox1_port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1_port.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1_port.FormattingEnabled = true;
            this.comboBox1_port.Location = new System.Drawing.Point(81, 37);
            this.comboBox1_port.Name = "comboBox1_port";
            this.comboBox1_port.Size = new System.Drawing.Size(104, 22);
            this.comboBox1_port.TabIndex = 45;
            // 
            // timerConnect
            // 
            this.timerConnect.Tick += new System.EventHandler(this.timerConnect_Tick);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // RTUDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1904, 1011);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RTUDevice";
            this.Text = "RTU";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RTUDevice_FormClosed);
            this.Load += new System.EventHandler(this.RTUDevice_Load);
            this.SizeChanged += new System.EventHandler(this.RTUDevice_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.groupBoxData.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.groupBoxCurve.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxAddr.ResumeLayout(false);
            this.groupBoxAddr.PerformLayout();
            this.groupBoxSet.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBoxCal.ResumeLayout(false);
            this.groupBoxCal.PerformLayout();
            this.groupBoxPara.ResumeLayout(false);
            this.groupBoxPara.PerformLayout();
            this.groupBoxConnect.ResumeLayout(false);
            this.groupBoxConnect.PerformLayout();
            this.groupBoxConfig.ResumeLayout(false);
            this.groupBoxConfig.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion
        private Panel panel1;
        private GroupBox groupBoxConnect;
        private GroupBox groupBoxPara;
        private Label labelCal5;
        private Label labelCal3;
        private MyControl.LimitedSelectionListBox listBoxTkzerotime;
        private Label labelCal2;
        private MyControl.LimitedSelectionListBox listBoxAntivib;
        private Label labelCal6;
        private MyControl.LimitedSelectionListBox listBoxDynazero;
        private Label labelCal8;
        private MyControl.LimitedSelectionListBox listBoxFiltertime;
        private Label labelCal7;
        private MyControl.LimitedSelectionListBox listBoxFilterange;
        private Label labelCal1;
        private Label labelCal4;
        private MyControl.LimitedSelectionListBox listBoxAutozero;
        private MyControl.LimitedSelectionListBox listBoxTrackzero;
        private SplitContainer splitContainer2;
        private Label label6;
        private ComboBox comboBox2;
        private Label label_Data;
        private Label label11;
        private DTiws.View.ButtonX buttonX_Recoord;
        private DTiws.View.ButtonX buttonX_Clear;
        private GroupBox groupBoxCal;
        private TextBox textBox1;
        private Label label17;
        private Button button2;
        private Button bt_send2;
        private Button button4;
        private ComboBox comboBox4_Parity;
        private ComboBox comboBox3_StopBits;
        private ComboBox comboBox2_BaudRate;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private ComboBox comboBox1_port;
        private Button bt_send3;
        private GroupBox groupBoxSet;
        private DTiws.View.ButtonX buttonX_AZERO;
        private DTiws.View.ButtonX buttonX_ZERO;
        private DTiws.View.ButtonX buttonX_TARE;
        private ComboBox comboBoxAddr;
        private Label label1;
        private DTiws.View.ButtonX buttonX_QNET;
        private DTiws.View.ButtonX buttonX_QGROSS;
        private GroupBox groupBoxData;
        private Label label4;
        private ComboBox comboBoxUnit;
        private Label label5;
        private Label label18;
        private TextBox textBox_Zero;
        private TextBox textBox_Max;
        private DTiws.View.ButtonX bt_Max;
        private DTiws.View.ButtonX bt_Zero;
        private DTiws.View.ButtonX bt_Write;
        private GroupBox groupBoxAddr;
        private DTiws.View.ButtonX buttonX_Addr;
        private Label label23;
        private Label label24;
        private TextBox textBox_Weight;
        private Label labeloutunit;
        private DTiws.View.ButtonX buttonX_Para;
        private PictureBox pictureBox1;
        private Panel panel3;
        private Panel panel2;
        private Timer timerConnect;
        private TableLayoutPanel tableLayoutPanel1;
        private Timer timer1;
        private MyControl.DoubleBufferListView dbListView1;
        private GroupBox groupBoxCurve;
        private TextBox textBox_Addr;
        private MyControl.LimitedSelectionListBox listBoxTkdynatime;
        private GroupBox groupBoxConfig;
        private Button buttonX_Config;
        private ComboBox comboBox1_AutoConnect;
        private ComboBox comboBox3_AutoStart;
        private Label label2;
        private Label label3;
        private Label label7_RTUConfig;
        private ComboBox comboBox1_FullScreen;
        private Label label7;
        private Panel panel4;
        private Panel panel5;
        private Label label8_dataSum;
    }
}