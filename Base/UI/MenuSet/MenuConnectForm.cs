using Model;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Library;
using Base.UI.MyControl;

//Lumi 20240428

//目前存在的问题

//需要测试多个连续地址设备,能够可靠连接
//站点和波特率组合扫描连接

//关闭扫描连接时直接操作mySelfUART/myRS485打开关闭串口
//串口打开成功后给protocol
//直接操作mySelfUART/myRS485发送连接或扫描指令
//定时器里扫描也直接直接操作myRS485发
//收到串口回复后用protocol完成SCT读出任务

//其它窗口任务中全部用protocol来操作通讯接口

namespace Base.UI.MenuSet
{
    public partial class MenuConnectForm : Form
    {
        Byte addr = 1;               //扫描站点1-255
        bool isScan = false;         //是否点击扫描
        bool reconnect = false;      //先连接115200再试19200波特率
        bool isConnectAlone = false; //是否是单连接
        bool isConnectAloneCAN = false;         //是否是单连接CAN                    

        List<Button> buttons = new List<Button>();                           //1-255站点按钮集合
        List<Button> buttonsCAN = new List<Button>();                        //1-127节点按钮集合
        Dictionary<uint, bool> canDeviceList = new Dictionary<uint, bool>(); //记录CANopen设备连接情况
        private AutoFormSize autoFormSize = new AutoFormSize();              //自适应屏幕分辨率
        private List<Client> clientsList = new List<Client>();               //设备列表
        private Queue<Client> clientsQueue = new Queue<Client>();            //设备队列  

        private bool dontShowAgain = false; // 用于存储用户是否选择不再提示

        //构造函数
        public MenuConnectForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConnectForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //界面随分辨率调整
            autoFormSize.UIComponetForm(this);

            if (this.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                double ratio = (double)this.Width / this.Height;
                this.Height = (int)(this.Height * (int)((double)Screen.PrimaryScreen.WorkingArea.Width / 1920));
                this.Width = (int)(ratio * this.Height);
            }
            else if (this.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                double ratio = (double)this.Width / this.Height;
                this.Height = (int)(this.Height * (double)Screen.PrimaryScreen.Bounds.Height / 1080);
                this.Width = (int)(ratio * this.Height);
            }

            checkBox1.Visible = false;
            checkBox2.Visible = false;
            checkBox3.Visible = false;
            checkBox6.Visible = false;

            //默认选中Self-UART模式
            treeViewEx1.Nodes[0].Expand();
            treeViewEx1.SelectedNode = treeViewEx1.Nodes[0].Nodes[0];

            //刷新串口
            buttonRefresh_Click(null, null);

            //界面参数
            comboBox2_BaudRate.Text = MyDevice.myRS485.baudRate.ToString();
            comboBox3_StopBits.SelectedIndex = (byte)MyDevice.myRS485.stopBits - 1;
            comboBox4_Parity.SelectedIndex = (byte)MyDevice.myRS485.parity;
            textBox1.Text = MyDevice.myRS485.addr.ToString();
            comboBox5_ChID.Text = "CH" + (MyDevice.myCANopen.channel + 1).ToString();
            comboBox6_BaudRate.Text = MyDevice.myCANopen.baudRate.ToString();
            textBox2.Text = MyDevice.myCANopen.addr.ToString();
            getLocalIPAddress();
            textBox4_port.Text = "5678";
            update_listBox1();
            update_listBox2();

            //加入按钮系列
            byte btnX = 255;
            foreach (Button btn in panel1.Controls)
            {
                if ((btn.Text != "扫描") && (btn.Text != "Scan"))
                {
                    btn.Text = btnX.ToString();
                    btn.Name = "btn" + btnX.ToString();
                    btnX--;//panel控件是倒序遍历
                    btn.Click += new EventHandler(btnX_Add_Click);
                    buttons.Add(btn);
                }
            }

            btnX = 127;
            foreach (Button btn in panel2.Controls)
            {
                if ((btn.Text != "侦测") && (btn.Text != "Detect"))
                {
                    btn.Text = btnX.ToString();
                    btn.Name = "btn" + btnX.ToString();
                    btnX--;//panel控件是倒序遍历
                    btn.Click += new EventHandler(btnX_Add_ClickCAN);
                    buttonsCAN.Add(btn);
                }
            }

            for (byte i = 1; i != 0; i++)
            {
                if (MyDevice.mBUS[i].sTATE == STATE.WORKING)
                {
                    buttons[255 - i].BackColor = Color.Green;
                }
            }
            for (byte i = 1; i < 128; i++)
            {
                if (MyDevice.mCAN[i].sTATE == STATE.WORKING)
                {
                    buttonsCAN[127 - i].BackColor = Color.Green;
                }
            }

            TCPServer.ClientsRefreshed += clients_Refresh;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConnectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
            //取消刷新连接设备事件
            TCPServer.ClientsRefreshed -= clients_Refresh;

            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;
            timeoutTimer.Enabled = false;
        }

        /// <summary>
        /// 界面随分辨率变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConnectForm_SizeChanged(object sender, EventArgs e)
        {
            autoFormSize.UIComponetForm_Resize(this);
        }

        /// <summary>
        /// 字体随分辨率变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuConnectForm_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Width < 800)
            {
                this.bt_send2.Font = new System.Drawing.Font("宋体", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                this.bt_send5.Font = new System.Drawing.Font("宋体", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            }
            else
            {
                this.bt_send2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                this.bt_send5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            }
        }

        /// <summary>
        /// 导航选择
        /// </summary>
        /// <param name="sender"></par
        /// <param name="e"></param>
        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //关闭侦测时钟
            timer2.Enabled = false;

            //接口上下框名称
            String groupBox1Name = "";
            String groupBox2Name = "";
            String groupBox3Name = "";
            String groupBox4Name = "";

            //
            switch (e.Node.Text.Trim())
            {
                //选择通讯协议
                case "Self-UART":
                    groupBox1Name = "Self-UART";
                    groupBox2Name = "";
                    groupBox3Name = "";
                    groupBox4Name = "";
                    break;

                case "RS485 MODBUS RTU":
                    groupBox1Name = "";
                    groupBox2Name = "RS485 MODBUS RTU";
                    groupBox3Name = "";
                    groupBox4Name = "";
                    break;

                case "RS232 MODBUS RTU":
                    groupBox1Name = "";
                    groupBox2Name = "RS232 MODBUS RTU";
                    groupBox3Name = "";
                    groupBox4Name = "";
                    break;

                case "CANopen":
                    groupBox1Name = "";
                    groupBox2Name = "";
                    groupBox3Name = "CANopen";
                    groupBox4Name = "";
                    break;

                case "MODBUS TCP":
                    groupBox1Name = "";
                    groupBox2Name = "";
                    groupBox3Name = "";
                    groupBox4Name = "MODBUS TCP";
                    break;
            }

            //接口上框调整
            if (groupBox1Name == "")
            {
                groupBox1.Visible = false;
                groupBox2.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                groupBox4.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);
                panel1.Visible = false;
                panel2.Visible = false;
            }
            else
            {
                label3.Text = "0";
                label4.Text = "0";
                groupBox1.Visible = true;
                groupBox1.Text = groupBox1Name;
                groupBox2.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y + groupBox1.Height + 10);
                groupBox3.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y + groupBox1.Height + 10);
                groupBox4.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y + groupBox1.Height + 10);
                panel1.Visible = false;
                panel2.Visible = false;
            }

            //接口下框调整
            if (groupBox2Name == "")
            {
                groupBox2.Visible = false;
            }
            else
            {
                label7.Text = "0";
                label8.Text = "0";
                textBox1.Text = "1";
                timer1.Enabled = false;
                timer3.Enabled = false;
                groupBox2.Visible = true;
                groupBox2.Text = groupBox2Name;
                panel1.Visible = groupBox2Name == "RS232 MODBUS RTU" ? false : true;
                btn_Scan.Visible = groupBox2Name == "RS232 MODBUS RTU" ? true : false;
                if (MyDevice.languageType == 0)
                {
                    bt_send3.Text = groupBox2Name == "RS232 MODBUS RTU" ? "连 接" : "单连接";
                }
                else
                {
                    bt_send3.Text = "Connect";
                }
                this.panel1.Location = new Point(this.groupBox2.Location.X, this.groupBox2.Location.Y + this.groupBox2.Height + 3);
            }

            //接口下框调整
            if (groupBox3Name == "")
            {
                groupBox3.Visible = false;
            }
            else
            {
                textBox2.Text = "1";
                timer1.Enabled = false;
                timer3.Enabled = false;
                groupBox3.Visible = true;
                groupBox3.Text = groupBox3Name;
                panel2.Visible = true;
                if (MyDevice.languageType == 0)
                {
                    bt_send4.Text = "单连接";
                }
                else
                {
                    bt_send4.Text = "Connect";
                }
                this.panel2.Location = new Point(this.groupBox3.Location.X, this.groupBox3.Location.Y + this.groupBox3.Height + 3);
            }

            //接口下框调整
            if (groupBox4Name == "")
            {
                groupBox4.Visible = false;
            }
            else
            {
                textBox3.Text = "1";
                timer1.Enabled = false;
                timer3.Enabled = false;
                groupBox4.Visible = true;
                groupBox4.Text = groupBox4Name;
            }
        }

        /// <summary>
        /// 停止位改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_StopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3_StopBits.SelectedIndex > 0)
            {
                comboBox4_Parity.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 校验位改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox4_Parity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4_Parity.SelectedIndex > 0)
            {
                comboBox3_StopBits.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 刷新按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            //刷串口
            comboBox0_port.Items.Clear();
            comboBox0_port.Items.AddRange(SerialPort.GetPortNames());
            comboBox1_port.Items.Clear();
            comboBox1_port.Items.AddRange(SerialPort.GetPortNames());
            //无串口
            if (comboBox0_port.Items.Count == 0 || comboBox1_port.Items.Count == 0)
            {
                comboBox0_port.Text = null;
                comboBox1_port.Text = null;
            }
            //有可用串口
            else
            {
                comboBox0_port.Text = MyDevice.mySelfUART.portName;
                comboBox1_port.Text = MyDevice.myRS485.portName;
                if (comboBox0_port.SelectedIndex < 0)
                {
                    comboBox0_port.SelectedIndex = 0;
                }
                if (comboBox1_port.SelectedIndex < 0)
                {
                    comboBox1_port.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1Close_Click(object sender, EventArgs e)
        {
            if (comboBox0_port.Text != null)
            {
                //名字是自定义串口的
                if (MyDevice.mySelfUART.portName == comboBox0_port.Text)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.mySelfUART.Is_serial_closing = true;

                    //处理当前在消息队列中的所有 Windows 消息
                    //防止界面停止响应
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    while (MyDevice.mySelfUART.Is_serial_listening)
                    {
                        Application.DoEvents();
                    }

                    //关闭接口
                    if (MyDevice.mySelfUART.Protocol_PortClose())
                    {
                        ui_Close_SelfUart();//button1
                    }

                    //变更接口的设备状态
                    if (!MyDevice.mySelfUART.IsOpen)
                    {
                        MyDevice.mSUT.sTATE = STATE.INVALID;
                    }
                }

                //名字是RS485接口的
                if (MyDevice.myRS485.portName == comboBox0_port.Text)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.myRS485.Is_serial_closing = true;

                    //处理当前在消息队列中的所有 Windows 消息
                    //防止界面停止响应
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    while (MyDevice.myRS485.Is_serial_listening)
                    {
                        Application.DoEvents();
                    }

                    //关闭接口
                    if (MyDevice.myRS485.Protocol_PortClose())
                    {
                        ui_Close_SelfUart();//button1
                    }

                    //变更接口的设备状态
                    if (!MyDevice.myRS485.IsOpen)
                    {
                        for (int i = 0; i <= 255; i++)
                        {
                            MyDevice.mBUS[i].sTATE = STATE.INVALID;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2Close_Click(object sender, EventArgs e)
        {
            if (comboBox1_port.Text != null)
            {
                //名字是自定义串口的
                if (MyDevice.mySelfUART.portName == comboBox1_port.Text)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.mySelfUART.Is_serial_closing = true;

                    //处理当前在消息队列中的所有 Windows 消息
                    //防止界面停止响应
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    while (MyDevice.mySelfUART.Is_serial_listening)
                    {
                        Application.DoEvents();
                    }

                    //关闭接口
                    if (MyDevice.mySelfUART.Protocol_PortClose())
                    {
                        ui_Close_RS485();//button2
                    }

                    //变更接口的设备状态
                    if (!MyDevice.mySelfUART.IsOpen)
                    {
                        MyDevice.mSUT.sTATE = STATE.INVALID;
                    }
                }

                //名字是RS485接口的
                if (MyDevice.myRS485.portName == comboBox1_port.Text)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.myRS485.Is_serial_closing = true;

                    //处理当前在消息队列中的所有 Windows 消息
                    //防止界面停止响应
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    while (MyDevice.myRS485.Is_serial_listening)
                    {
                        Application.DoEvents();
                    }

                    //关闭接口
                    if (MyDevice.myRS485.Protocol_PortClose())
                    {
                        ui_Close_RS485();//button2
                    }

                    //变更接口的设备状态
                    if (!MyDevice.myRS485.IsOpen)
                    {
                        for (int i = 0; i <= 255; i++)
                        {
                            MyDevice.mBUS[i].sTATE = STATE.INVALID;
                        }
                    }

                    //扫描1-255按钮初始化
                    for (byte i = 1; i != 0; i++)
                    {
                        buttons[255 - i].FlatStyle = FlatStyle.Standard;
                        buttons[255 - i].BackColor = Color.White;
                    }
                }
            }
        }

        /// <summary>
        /// 自定义协议接口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send1_Click(object sender, EventArgs e)
        {
            if (comboBox0_port.Text != null)
            {
                //切换自定义通讯
                MyDevice.mePort_ChangeProtocol(COMP.SelfUART);

                //打开串口
                MyDevice.mySelfUART.Protocol_PortOpen(comboBox0_port.Text, 115200, StopBits.One, Parity.None);

                //串口发送
                if (MyDevice.mySelfUART.IsOpen)
                {
                    //115200失败后再试19200
                    reconnect = true;
                    timer1.Interval = 200;
                    ui_Connect_SelfUart();

                    //读出SCT
                    MyDevice.mySelfUART.Protocol_ClearState();
                    MyDevice.mySelfUART.Protocol_mePort_ReadTasks();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
        }

        /// <summary>
        /// RS485协议接口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send3_Click(object sender, EventArgs e)
        {
            if (comboBox1_port.Text != null)
            {
                //防止连续按多次连接时连接不上
                if (MyDevice.myRS485.IsOpen)
                {
                    if (MyDevice.myRS485.Is_serial_listening)
                    {
                        return;
                    }
                    else
                    {
                        MyDevice.myRS485.Protocol_ClearState();
                        Thread.Sleep(60);
                    }
                }

                //不用扫描
                isScan = false;

                //单独连接
                isConnectAlone = true;

                //切换RS485通讯协议
                MyDevice.mePort_ChangeProtocol(COMP.RS485);

                //打开串口
                MyDevice.myRS485.Protocol_PortOpen(comboBox1_port.Text,
                    Convert.ToInt32(comboBox2_BaudRate.Text),
                    (StopBits)(comboBox3_StopBits.SelectedIndex + 1),
                    (Parity)comboBox4_Parity.SelectedIndex);

                //串口发送
                if (MyDevice.myRS485.IsOpen)
                {
                    //初始化设备连接状态
                    for (int i = 0; i <= 255; i++)
                    {
                        MyDevice.mBUS[i].sTATE = STATE.INVALID;
                    }

                    //
                    ui_Connect_RS485();

                    //站点地址
                    MyDevice.myRS485.addr = Convert.ToByte(textBox1.Text);

                    //读出SCT
                    MyDevice.myRS485.Protocol_ClearState();
                    MyDevice.myRS485.Protocol_mePort_ReadTasks();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
        }

        /// <summary>
        /// RS485协议接口扫描
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send2_Click(object sender, EventArgs e)
        {
            //点击扫描标志
            isScan = true;

            isConnectAlone = false;

            //扫描1-255按钮初始化
            for (byte i = 1; i != 0; i++)
            {
                buttons[255 - i].FlatStyle = FlatStyle.Standard;
            }

            //扫描中
            if (timer1.Enabled)
            {
                ui_Stop_Scan();
            }
            //停止中
            else
            {
                if (comboBox1_port.Text != null)
                {
                    //根据波特率选择扫描间隔
                    switch (comboBox2_BaudRate.Text)
                    {
                        case "1200": timer1.Interval = 200; break; //150
                        case "2400": timer1.Interval = 100; break; //75
                        case "4800": timer1.Interval = 50; break;  //38
                        case "9600": timer1.Interval = 40; break;  //19
                        case "14400": timer1.Interval = 17; break; //13
                        case "19200": timer1.Interval = 13; break; //10
                        case "38400": timer1.Interval = 7; break;  //5
                        case "57600": timer1.Interval = 5; break;  //4
                        case "115200": timer1.Interval = 2; break; //2
                    }

                    //切换RS485通讯协议
                    MyDevice.mePort_ChangeProtocol(COMP.RS485);

                    //打开串口
                    MyDevice.myRS485.Protocol_PortOpen(comboBox1_port.Text,
                        Convert.ToInt32(comboBox2_BaudRate.Text),
                        (StopBits)(comboBox3_StopBits.SelectedIndex + 1),
                        (Parity)comboBox4_Parity.SelectedIndex);

                    //串口有效
                    if (MyDevice.myRS485.IsOpen)
                    {
                        //扫描地址初始化
                        addr = 1;

                        //启动扫描
                        MyDevice.myRS485.Protocol_SendAddr(addr);
                        ui_Start_Scan();
                    }
                    else
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("串口未打开，检查串口是否被占用");
                        }
                        else
                        {
                            MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// RS485协议接口间隔扫描事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MyDevice.mySelfUART.Is_serial_listening == true) return;
            if (timeoutTimer.Enabled)
            {
                timeoutTimer.Stop();
            }
            if (reconnect)
            {
                //切换自定义通讯
                MyDevice.mePort_ChangeProtocol(COMP.SelfUART);

                //打开串口
                MyDevice.mySelfUART.Protocol_PortOpen(comboBox0_port.Text, 19200, StopBits.One, Parity.None);

                //再试19200
                reconnect = false;
                ui_Connect_SelfUart();

                //读出SCT
                MyDevice.mySelfUART.Protocol_ClearState();
                MyDevice.mySelfUART.Protocol_mePort_ReadTasks();
            }
            else if (MyDevice.myRS485.IsOpen)
            {
                //扫描地址1-255
                if ((++addr) != 0)
                {
                    //
                    ui_update_labelRxTx_RS485();

                    //
                    if (treeViewEx1.SelectedNode == treeViewEx1.Nodes[0].Nodes[1])
                    {
                        textBox1.Text = addr.ToString();
                    }
                    else
                    {
                        buttons[255 - addr].FlatStyle = FlatStyle.Flat;
                        buttons[255 - addr].FlatAppearance.BorderColor = Color.DarkGray;
                    }

                    MyDevice.myRS485.Protocol_SendAddr(addr);
                }
                else
                {
                    ui_Finish_Scan();
                }
            }
            else
            {
                //停止扫描
                ui_Stop_Scan();

                //
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("串口未打开，检查串口是否被占用");
                }
                else
                {
                    MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                }
            }
        }

        /// <summary>
        /// 扫描超时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeoutTimer_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        /// <summary>
        /// 加入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnX_Add_Click(object sender, EventArgs e)
        {
            Button buttonX = sender as Button;

            buttonX.BackColor = label1.BackColor;

            isConnectAlone = false;

            if (comboBox1_port.Text != null)
            {
                //防止连续按多次连接时连接不上
                if (MyDevice.myRS485.IsOpen)
                {
                    if (MyDevice.myRS485.Is_serial_listening)
                    {
                        return;
                    }
                    else
                    {
                        MyDevice.myRS485.Protocol_ClearState();
                        Thread.Sleep(60);
                    }
                }

                //不用扫描
                isScan = false;

                //切换RS485通讯协议
                MyDevice.mePort_ChangeProtocol(COMP.RS485);

                //打开串口
                MyDevice.myRS485.Protocol_PortOpen(comboBox1_port.Text,
                    Convert.ToInt32(comboBox2_BaudRate.Text),
                    (StopBits)(comboBox3_StopBits.SelectedIndex + 1),
                    (Parity)comboBox4_Parity.SelectedIndex);

                //串口发送
                if (MyDevice.myRS485.IsOpen)
                {
                    //初始化设备连接状态
                    MyDevice.mBUS[Convert.ToByte(buttonX.Text)].sTATE = STATE.INVALID;
                    //
                    ui_Connect_RS485();

                    //站点地址
                    MyDevice.myRS485.addr = Convert.ToByte(buttonX.Text);

                    //读出SCT
                    MyDevice.myRS485.Protocol_ClearState();
                    MyDevice.myRS485.Protocol_mePort_ReadTasks();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
        }

        /// <summary>
        /// 串口委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void update_FromUart()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(update_FromUart);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                Main.isConnected = false;
                if (timeoutTimer.Enabled)
                {
                    timeoutTimer.Stop();  //关闭超时时钟
                }

                switch (MyDevice.protocol.type)
                {
                    //扫描到设备启动读出SCT
                    case COMP.RS485:
                        if (MyDevice.myRS485.trTASK == TASKS.SCAN)
                        {
                            ui_Connect_RS485();
                            MyDevice.myRS485.Protocol_ClearState();
                        }
                        break;
                    case COMP.ModbusTCP:
                        if (MyDevice.myModbusTCP.trTASK == TASKS.SCAN)
                        {
                            ui_Connect_ModbusTCP();
                            MyDevice.myModbusTCP.Protocol_ClearState();
                        }
                        break;
                    //修改完心跳后读出SCT
                    case COMP.CANopen:
                        if (MyDevice.myCANopen.trTASK == TASKS.WHEART)
                        {
                            MyDevice.myCANopen.Protocol_ClearState();
                        }
                        break;
                    default:
                        break;
                }

                //继续读取
                MyDevice.protocol.Protocol_mePort_ReadTasks();

                //界面状态
                switch (MyDevice.protocol.type)
                {
                    case COMP.SelfUART:
                        ui_update_labelRxTx_SelfUart();
                        bt_send1.Text = MyDevice.protocol.trTASK.ToString();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            ui_Connected_SelfUart();
                            Main.isConnected = true;
                        }
                        break;
                    case COMP.RS485:
                        ui_update_labelRxTx_RS485();
                        if (isConnectAlone && ((!isScan) || (isScan && MyDevice.protocol.trTASK != TASKS.NULL)))
                        {
                            bt_send3.Text = MyDevice.protocol.trTASK.ToString();
                        }
                        switch (MyDevice.protocol.trTASK)
                        {
                            default:
                            case TASKS.SCAN:
                                break;
                            case TASKS.BOR:
                            case TASKS.RDX0:
                            case TASKS.RDX1:
                            case TASKS.RDX2:
                            case TASKS.RDX3:
                            case TASKS.RDX4:
                            case TASKS.RDX5:
                            case TASKS.RDX6:
                            case TASKS.RDX7:
                            case TASKS.RDX8:
                            case TASKS.RDX9:
                            case TASKS.RDFT:
                                if (isScan)
                                {
                                    timeoutTimer.Start();  //开启超时时钟
                                }
                                break;
                            case TASKS.NULL:
                                if (isScan)
                                {
                                    //继续扫描
                                    addr = addr > MyDevice.myRS485.addr ? MyDevice.myRS485.addr : addr;
                                    timer1.Enabled = true;
                                    if (treeViewEx1.SelectedNode == treeViewEx1.Nodes[0].Nodes[1])
                                    {
                                        if (MyDevice.languageType == 0)
                                        {
                                            bt_send3.Text = "站点" + addr + "已连接";
                                        }
                                        else
                                        {
                                            bt_send3.Text = "addr" + addr + " connected";
                                        }
                                        bt_send3.BackColor = Color.Green;
                                    }
                                    else
                                    {
                                        buttons[255 - addr].BackColor = Color.Green;
                                    }
                                }
                                else
                                {
                                    ui_Connected_RS485();
                                }
                                Main.isConnected = true;
                                break;
                        }
                        break;
                    case COMP.CANopen:
                        ui_update_labelRxTx_CANopen();
                        if (isConnectAloneCAN)
                        {
                            bt_send4.Text = MyDevice.protocol.trTASK.ToString();
                        }
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            ui_Connected_CANopen();
                            Main.isConnected = true;
                        }
                        break;
                    case COMP.ModbusTCP:
                        ui_update_labelRxTx_ModbusTCP();

                        if (!isScan)
                        {
                            bt_send6.Text = MyDevice.protocol.trTASK.ToString();
                        }

                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            update_listBox2();
                            Main.isConnected = true;
                            if (isScan)
                            {
                                //继续扫描
                                if (clientsList?.Count > 0)
                                {
                                    timer3.Enabled = true; //通过定时器，addr++
                                }
                            }
                            else
                            {
                                ui_Connected_ModbusTCP();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #region CANopen

        /// <summary>
        /// CAN通道号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox5_ChID_SelectedIndexChanged(object sender, EventArgs e)
        {
            //关闭侦测时钟
            timer2.Enabled = false;
        }

        /// <summary>
        /// CANopen 波特率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox6_BaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            //关闭侦测时钟
            timer2.Enabled = false;
        }

        /// <summary>
        /// CANopen关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5Close_Click(object sender, EventArgs e)
        {
            //关闭侦测时钟
            timer2.Enabled = false;

            //CANopen
            if (MyDevice.myCANopen.channel == comboBox5_ChID.SelectedIndex && MyDevice.myCANopen.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.myCANopen.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.myCANopen.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭接口
                if (MyDevice.myCANopen.Protocol_PortClose())
                {
                    ui_Close_CANopen();
                }

                //变更接口的设备状态
                if (!MyDevice.myCANopen.IsOpen)
                {
                    for (int i = 0; i <= 127; i++)
                    {
                        MyDevice.mCAN[i].sTATE = STATE.INVALID;
                    }
                }
            }
        }

        /// <summary>
        /// CANopen协议接口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send4_Click(object sender, EventArgs e)
        {
            if (!dontShowAgain)
            {
                // 创建一个带有复选框的自定义对话框
                CustomMessageBox customMessageBox = new CustomMessageBox
                {
                    MessageText = "CANopen连接将修改心跳间隔时间为2000ms，需要手动恢复，是否继续？"
                };

                if (customMessageBox.ShowDialog() != DialogResult.Yes)
                {
                    return;
                }

                // 更新不再提示的状态
                dontShowAgain = customMessageBox.DontShowAgain;
            }

            //关闭侦测时钟
            timer2.Enabled = false;

            if (comboBox1_port.Text != null)
            {
                //防止连续按多次连接时连接不上
                if (MyDevice.myCANopen.IsOpen)
                {
                    if (MyDevice.myCANopen.Is_serial_listening)
                    {
                        return;
                    }
                    else
                    {
                        MyDevice.myCANopen.Protocol_ClearState();
                        Thread.Sleep(60);
                    }
                }

                //不用扫描
                isScan = false;

                //单独连接
                isConnectAloneCAN = true;

                //切换CANopen通讯协议
                MyDevice.mePort_ChangeProtocol(COMP.CANopen);

                //打开串口
                MyDevice.myCANopen.Protocol_PortOpen(0, comboBox5_ChID.Text, Convert.ToInt32(comboBox6_BaudRate.Text));

                //串口发送
                if (MyDevice.myCANopen.IsOpen)
                {
                    //初始化设备连接状态
                    for (int i = 0; i <= 127; i++)
                    {
                        MyDevice.mCAN[i].sTATE = STATE.INVALID;
                    }

                    //
                    ui_Connect_CANopen();

                    //站点地址
                    MyDevice.myCANopen.addr = Convert.ToByte(textBox2.Text);

                    //修改心跳
                    MyDevice.myCANopen.Protocol_ClearState();
                    MyDevice.myCANopen.Protocol_SendHeartBeat(2000);
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("CAN通道未打开，检查是否被占用");
                    }
                    else
                    {
                        MessageBox.Show("The CAN channel is not enabled. Check whether the serial port is occupied.");
                    }
                }
            }
        }

        /// <summary>
        /// CANopen协议侦测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send5_Click(object sender, EventArgs e)
        {
            //防止连续按多次连接时连接不上
            if (MyDevice.myCANopen.IsOpen)
            {
                if (MyDevice.myCANopen.Is_serial_listening)
                {
                    return;
                }
                else
                {
                    MyDevice.myCANopen.Protocol_ClearState();
                    Thread.Sleep(60);
                }
            }

            //不用扫描
            isScan = false;

            //切换CANopen通讯协议
            MyDevice.mePort_ChangeProtocol(COMP.CANopen);

            //打开串口
            MyDevice.myCANopen.Protocol_PortOpen(0, comboBox5_ChID.Text, Convert.ToInt32(comboBox6_BaudRate.Text));

            //串口发送
            if (MyDevice.myCANopen.IsOpen)
            {
                // 初始化字典，将键为1到127的整数，值设为false
                canDeviceList.Clear();
                for (uint i = 1; i <= 127; i++)
                {
                    canDeviceList.Add(i, false);
                    buttonsCAN[127 - (int)i].BackColor = Color.White;
                }
                timer2.Enabled = true;
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("CAN通道未打开，检查是否被占用");
                }
                else
                {
                    MessageBox.Show("The CAN channel is not enabled. Check whether the serial port is occupied.");
                }
            }
        }

        /// <summary>
        /// CANopen协议侦测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (MyDevice.myCANopen.IsOpen)
            {
                uint id = MyDevice.myCANopen.Protocol_GetHeartBeatID();
                if (id > 0 && id <= 127)
                {
                    canDeviceList[id] = true;
                    buttonsCAN[127 - (int)id].BackColor = Color.Orange;
                }
            }
            else
            {
                //
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("CAN通道未打开，检查是否被占用");
                }
                else
                {
                    MessageBox.Show("The serial port is not enabled. Check whether the serial port is occupied.");
                }
            }
        }

        /// <summary>
        /// 加入事件CAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnX_Add_ClickCAN(object sender, EventArgs e)
        {
            if (!dontShowAgain)
            {
                // 创建一个带有复选框的自定义对话框
                CustomMessageBox customMessageBox = new CustomMessageBox
                {
                    MessageText = "CANopen连接将修改心跳间隔时间为2000ms，需要手动恢复，是否继续？"
                };

                if (customMessageBox.ShowDialog() != DialogResult.Yes)
                {
                    return;
                }

                // 更新不再提示的状态
                dontShowAgain = customMessageBox.DontShowAgain;
            }

            //关闭侦测时钟
            timer2.Enabled = false;

            Button buttonX = sender as Button;
            buttonX.BackColor = label1.BackColor;

            isConnectAloneCAN = false;

            if (comboBox1_port.Text != null)
            {
                //防止连续按多次连接时连接不上
                if (MyDevice.myCANopen.IsOpen)
                {
                    if (MyDevice.myCANopen.Is_serial_listening)
                    {
                        return;
                    }
                    else
                    {
                        MyDevice.myCANopen.Protocol_ClearState();
                        Thread.Sleep(60);
                    }
                }

                //不用扫描
                isScan = false;

                //切换RS485通讯协议
                MyDevice.mePort_ChangeProtocol(COMP.CANopen);

                //打开串口
                MyDevice.myCANopen.Protocol_PortOpen(0, comboBox5_ChID.Text, Convert.ToInt32(comboBox6_BaudRate.Text));

                //串口发送
                if (MyDevice.myCANopen.IsOpen)
                {
                    //初始化设备连接状态
                    MyDevice.mCAN[Convert.ToByte(buttonX.Text)].sTATE = STATE.INVALID;
                    //
                    ui_Connect_CANopen();

                    //站点地址
                    MyDevice.myCANopen.addr = Convert.ToByte(buttonX.Text);

                    //修改心跳
                    MyDevice.myCANopen.Protocol_ClearState();
                    MyDevice.myCANopen.Protocol_SendHeartBeat(2000);
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("CAN通道未打开");
                    }
                    else
                    {
                        MessageBox.Show("The CAN channel is not enabled.");
                    }
                }
            }
        }

        #endregion

        #region Modbus TCP

        /// <summary>
        /// Modbus TCP刷新主机地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            comboBox4_IP.Items.Clear();
            //获取本地的ip
            string str = comboBox4_IP.Text;
            getLocalIPAddress();
            if (str != comboBox4_IP.Text)
            {
                MyDevice.protocol.Protocol_PortClose();
            }
        }

        /// <summary>
        /// Modbus TCP打开主机端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            //切换Modbus TCP通讯协议
            MyDevice.mePort_ChangeProtocol(COMP.ModbusTCP);

            //打开端口
            MyDevice.myModbusTCP.Protocol_PortOpen(comboBox4_IP.Text, int.Parse(textBox4_port.Text));

            if (MyDevice.myModbusTCP.portName == comboBox4_IP.Text + ":" + textBox4_port.Text)  //打开成功
            {
                update_listBox1();
                button7_open.BackColor = Color.Green;
                button6_close.BackColor = label1.BackColor;

                //初始化设备连接状态
                for (int i = 0; i <= 255; i++)
                {
                    MyDevice.mMTCP[i].sTATE = STATE.INVALID;
                }
            }
            else
            {
                button7_open.BackColor = Color.Firebrick;
                button6_close.BackColor = label1.BackColor;
            }
        }

        /// <summary>
        /// Modbus TCP关闭主机端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_close_Click(object sender, EventArgs e)
        {
            if (comboBox4_IP.Text != null)
            {
                //名字是Modbus TCP接口的
                if (MyDevice.myModbusTCP.portName == comboBox4_IP.Text + ":" + textBox4_port.Text)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.myModbusTCP.Is_serial_closing = true;

                    //处理当前在消息队列中的所有 Windows 消息
                    //防止界面停止响应
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    while (MyDevice.myModbusTCP.Is_serial_listening)
                    {
                        Application.DoEvents();
                    }

                    //关闭接口
                    if (MyDevice.myModbusTCP.Protocol_PortClose())
                    {
                        listBox1.DataSource = null;//清空datasource
                        listBox2.DataSource = null;
                        ui_Close_ModbusTCP();
                    }

                    //变更接口的设备状态
                    if (!MyDevice.myModbusTCP.IsOpen)
                    {
                        for (int i = 0; i < MyDevice.mMTCP.Length; i++)
                        {
                            MyDevice.mMTCP[i].sTATE = STATE.INVALID;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// modbus-TCP协议接口扫描
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Scan2_Click(object sender, EventArgs e)
        {
            //点击扫描标志
            isScan = true;

            //扫描中
            if (timer3.Enabled)
            {
                ui_Stop_Scan_ModbusTCP();
            }
            //停止中
            else
            {
                if (comboBox4_IP.Text != null)
                {
                    //切换modbus-TCP通讯协议
                    MyDevice.mePort_ChangeProtocol(COMP.ModbusTCP);

                    //串口有效
                    if (MyDevice.myModbusTCP.IsOpen)
                    {
                        //扫描地址初始化
                        addr = 1;

                        //扫描设备(ip)列表初始化
                        if (clientsList?.Count < 1) return;
                        clientsQueue.Clear();
                        foreach (Client client in clientsList)
                        {
                            clientsQueue.Enqueue(client);    //初始化本轮扫描的队列
                        }
                        if (clientsQueue.Count > 0)
                        {
                            MyDevice.myModbusTCP.ipAddr = clientsQueue.Dequeue().ClientIP.ToString();
                            //启动扫描
                            MyDevice.myModbusTCP.Protocol_SendAddr(addr);
                        }

                        ui_Start_Scan_ModbusTCP();
                    }
                    else
                    {
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("端口未打开，检查串口是否被占用");
                        }
                        else
                        {
                            MessageBox.Show("The port is not enabled. Check whether the serial port is occupied.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Modbus TCP连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_send6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null && textBox3.Text != null)
            {
                //防止连续按多次连接时连接不上
                if (MyDevice.myModbusTCP.IsOpen)
                {
                    if (MyDevice.myModbusTCP.Is_serial_listening)
                    {
                        return;
                    }
                    else
                    {
                        MyDevice.myModbusTCP.Protocol_ClearState();
                        Thread.Sleep(60);
                    }
                }

                //不用扫描
                isScan = false;

                //切换MODBUS TCP通讯协议
                MyDevice.mePort_ChangeProtocol(COMP.ModbusTCP);

                //串口发送
                if (MyDevice.myModbusTCP.IsOpen)
                {
                    //
                    ui_Connect_ModbusTCP();

                    //由于ModbusTCP连接时，可能出现不同IP但设备id相同的项
                    //mMTCP的index不是设备id
                    //每次发送指令时，需要先确认IP地址，再匹配设备addr
                    MyDevice.myModbusTCP.ipAddr = listBox1.SelectedValue.ToString();
                    MyDevice.myModbusTCP.addr = Convert.ToByte(textBox3.Text);

                    //读出SCT
                    MyDevice.myModbusTCP.Protocol_ClearState();
                    MyDevice.myModbusTCP.Protocol_mePort_ReadTasks();
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("端口未打开");
                    }
                    else
                    {
                        MessageBox.Show("The port is not open");
                    }
                }
            }
        }

        /// <summary>
        /// Modbus-TCP协议接口间隔扫描事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (MyDevice.myModbusTCP.Is_serial_listening == true) return;
            if (MyDevice.myModbusTCP.IsOpen)
            {
                if (clientsList?.Count < 1) return;
                if (clientsQueue.Count > 0) //还有没扫描的设备
                {
                    MyDevice.myModbusTCP.ipAddr = clientsQueue.Dequeue().ClientIP.ToString();
                    MyDevice.myModbusTCP.Protocol_SendAddr(addr);
                }
                else//本轮所有设备已扫描
                {
                    //扫描地址1-255
                    if ((++addr) != 0)
                    {
                        textBox3.Text = addr.ToString();

                        ui_update_labelRxTx_ModbusTCP();

                        //扫描设备(ip)列表初始化
                        clientsQueue.Clear();
                        foreach (Client client in clientsList)
                        {
                            clientsQueue.Enqueue(client);    //初始化本轮扫描的队列
                        }
                        if (clientsQueue.Count > 0)
                        {
                            MyDevice.myModbusTCP.ipAddr = clientsQueue.Dequeue().ClientIP.ToString();
                            MyDevice.myModbusTCP.Protocol_SendAddr(addr);
                        }
                    }
                    else
                    {
                        ui_Finish_Scan_ModbusTCP();
                    }
                }
            }
            else
            {
                //停止扫描
                ui_Stop_Scan_ModbusTCP();

                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("端口未打开");
                }
                else
                {
                    MessageBox.Show("The port is not open");
                }
            }
        }

        /// <summary>
        /// Modbus TCP连接设备变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clients_Refresh(object sender, NetClientsRefreshEventArgs e)
        {
            Action action = () =>
                {
                    update_listBox1();
                    update_listBox2();
                };
            Invoke(action);
        }

        /// <summary>
        /// 更新listbox1
        /// </summary>
        private void update_listBox1()
        {
            Dictionary<IPAddress, Client> clients = TCPServer.MyClients;         //获取当前连接的客户端
            if (clients?.Count > 0)
            {
                var selectedValue = listBox1.SelectedItem;                       //选择的项

                clientsList = clients.Values.Where(item => item.IsConnected == true)       //只显示已连接的client
                                            .OrderBy(item => item.ConnectedTime).ToList(); //按连接时间排序

                var itemList = clients.Where(kv => kv.Value.IsConnected == true) //只显示已连接的client
                                      .OrderBy(kv => kv.Value.ConnectedTime)     //按连接时间排序
                                      .Select(kv => new { Key = kv.Value.ClientIP.ToString(), DisplayText = $"{kv.Value.ClientEndPoint}  设备TCP Client已连接" }).ToList();
                listBox1.DataSource = itemList;                                  //更新listbox
                listBox1.ValueMember = "Key";
                listBox1.DisplayMember = "DisplayText";

                if (selectedValue != null && listBox1.Items.Contains(selectedValue)) //恢复选择的项
                {
                    listBox1.SelectedItem = selectedValue;
                }
                else if (selectedValue == null && listBox1.Items.Count > 0)
                {
                    listBox1.SelectedIndex = 0;
                }
                else
                {
                    listBox1.SelectedItem = null;
                }
            }
            else
            {
                clientsList.Clear();
                listBox1.DataSource = null;
            }
        }

        /// <summary>
        /// 更新listbox2
        /// </summary>
        private void update_listBox2()
        {
            Dictionary<IPAddress, Client> clients = TCPServer.MyClients;              //获取当前连接的客户端
            if (clients?.Count > 0)
            {
                //在线客户端
                var onlineClients = clients.Where(kv => kv.Value.IsConnected == true) //只显示已连接的client
                                      .OrderBy(kv => kv.Value.ConnectedTime);         //按连接时间排序
                //查找mMTCP中TCP连接在线且已软件连接完成的设备
                var result = MyDevice.mMTCP.Where(item => onlineClients.Any(client => client.Value.ClientIP.ToString() == item.R_ipAddr && item.sTATE == STATE.WORKING)).ToList();
                //赋值给listbox2
                var itemList = result.Select(item => new { Key = item.E_addr + "-" + item.R_ipAddr, DisplayText = $"ID={item.E_addr} {item.R_ipAddr}:{TCPServer.FindPortByIP(item.R_ipAddr)}" }).ToList();
                listBox2.DataSource = itemList;                                      //更新listbox
                listBox2.ValueMember = "Key";
                listBox2.DisplayMember = "DisplayText";
            }
            else
            {
                listBox2.DataSource = null;
            }
        }

        #endregion

        #region 界面更新

        private void ui_Close_SelfUart()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            button1.BackColor = Color.Green;
            button2.BackColor = label1.BackColor;
            button5.BackColor = label1.BackColor;
            button6_close.BackColor = label1.BackColor;

            ui_reset_btnSendColor();    //发送键颜色复位
            ui_reset_btnSendText();     //发送键文字复位

            ui_reset_labelRxTx();       //将接受和发送的数字显示归零
        }

        private void ui_Close_RS485()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            button1.BackColor = label1.BackColor;
            button2.BackColor = Color.Green;
            button5.BackColor = label1.BackColor;
            button6_close.BackColor = label1.BackColor;

            ui_reset_btnSendColor();    //发送键颜色复位
            ui_reset_btnSendText();     //发送键文字复位

            ui_reset_labelRxTx();       //将接受和发送的数字显示归零
        }

        private void ui_Close_CANopen()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            button1.BackColor = label1.BackColor;
            button2.BackColor = label1.BackColor;
            button5.BackColor = Color.Green;
            button6_close.BackColor = label1.BackColor;

            ui_reset_btnSendColor();    //发送键颜色复位
            ui_reset_btnSendText();     //发送键文字复位

            ui_reset_labelRxTx();       //将接受和发送的数字显示归零
        }

        private void ui_Close_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            button1.BackColor = label1.BackColor;
            button2.BackColor = label1.BackColor;
            button5.BackColor = label1.BackColor;
            button6_close.BackColor = Color.Green;
            button7_open.BackColor = label1.BackColor;

            ui_reset_btnSendColor();    //发送键颜色复位
            ui_reset_btnSendText();     //发送键文字复位

            ui_reset_labelRxTx();       //将接受和发送的数字显示归零
        }

        private void ui_Connect_SelfUart()
        {
            timer1.Enabled = reconnect;//是否需要继续连接19200的老设备
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = Color.Firebrick;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_SelfUart();
        }

        private void ui_Connect_RS485()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = isConnectAlone ? Color.Firebrick : label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = isConnectAlone ? "连 接" : "单连接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_RS485();
        }

        private void ui_Connect_CANopen()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = isConnectAloneCAN ? Color.Firebrick : label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = isConnectAloneCAN ? "连 接" : "单连接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_CANopen();
        }

        private void ui_Connect_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位
            button7_open.BackColor = Color.Green;

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = isScan ? label1.BackColor : Color.Firebrick;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_ModbusTCP();
        }

        private void ui_Stop_Scan()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
            }

            ui_update_labelRxTx_RS485();
        }

        private void ui_Stop_Scan_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            ui_update_labelRxTx_ModbusTCP();
        }

        private void ui_Start_Scan()
        {
            reconnect = false;
            timer1.Enabled = true;//启动扫描
            timer3.Enabled = false;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "停止";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Stop";
            }

            ui_update_labelRxTx_RS485();

            foreach (Control control in this.panel1.Controls)
            {
                control.BackColor = label1.BackColor;
            }

            buttons[254].FlatStyle = FlatStyle.Flat;
            buttons[254].FlatAppearance.BorderColor = Color.DarkGray;
        }

        private void ui_Start_Scan_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//启动扫描
            timer3.Enabled = true;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            ui_update_labelRxTx_ModbusTCP();
        }

        private void ui_Finish_Scan()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "结束";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Finish";
            }

            ui_update_labelRxTx_RS485();
        }

        private void ui_Finish_Scan_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();
            ui_reset_btnSendColor();

            ui_update_labelRxTx_ModbusTCP();
        }

        private void ui_Connected_SelfUart()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = Color.Green;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "已连接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connected";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_SelfUart();
        }

        private void ui_Connected_RS485()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = isConnectAlone ? Color.Green : label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = isConnectAlone ? "已连接" : "单连接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = isConnectAlone ? "Connected" : "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_RS485();

            if (treeViewEx1.SelectedNode == treeViewEx1.Nodes[0].Nodes[2])
            {
                buttons[255 - MyDevice.protocol.addr].BackColor = Color.Green;
            }
        }

        private void ui_Connected_CANopen()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = isConnectAloneCAN ? Color.Green : label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = isConnectAloneCAN ? "已连接" : "单连接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = isConnectAlone ? "Connected" : "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }

            ui_update_labelRxTx_CANopen();

            if (treeViewEx1.SelectedNode == treeViewEx1.Nodes[0].Nodes[3])
            {
                buttonsCAN[127 - MyDevice.protocol.addr].BackColor = Color.Green;
            }
        }

        private void ui_Connected_ModbusTCP()
        {
            reconnect = false;
            timer1.Enabled = false;//关闭扫描
            timer3.Enabled = false;

            ui_reset_btnClose();//关闭键颜色复位
            button7_open.BackColor = Color.Green;

            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = Color.Green;

            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "已连接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connected";
            }

            ui_update_labelRxTx_ModbusTCP();
        }

        //更新SelfUart接受发送的数字
        private void ui_update_labelRxTx_SelfUart()
        {
            label3.Text = MyDevice.protocol.txCount.ToString();
            label4.Text = MyDevice.protocol.rxCount.ToString();
            label7.Text = "0";
            label8.Text = "0";
            label21.Text = "0";
            label22.Text = "0";
            label28.Text = "0";
            label29.Text = "0";
        }

        //更新RS485接受发送的数字
        private void ui_update_labelRxTx_RS485()
        {
            label3.Text = "0";
            label4.Text = "0";
            label7.Text = MyDevice.protocol.txCount.ToString();
            label8.Text = MyDevice.protocol.rxCount.ToString();
            label21.Text = "0";
            label22.Text = "0";
            label28.Text = "0";
            label29.Text = "0";
        }

        //更新CANopen接受发送的数字
        private void ui_update_labelRxTx_CANopen()
        {
            label3.Text = "0";
            label4.Text = "0";
            label7.Text = "0";
            label8.Text = "0";
            label21.Text = MyDevice.protocol.txCount.ToString();
            label22.Text = MyDevice.protocol.rxCount.ToString();
            label28.Text = "0";
            label29.Text = "0";
        }

        //更新ModbusTCP接受发送的数字
        private void ui_update_labelRxTx_ModbusTCP()
        {
            label3.Text = "0";
            label4.Text = "0";
            label7.Text = "0";
            label8.Text = "0";
            label21.Text = "0";
            label22.Text = "0";
            label28.Text = MyDevice.protocol.txCount.ToString();
            label29.Text = MyDevice.protocol.rxCount.ToString();
        }

        //将接受和发送的数字显示归零
        private void ui_reset_labelRxTx()
        {
            label3.Text = "0";  //SelfUart
            label4.Text = "0";  //SelfUart
            label7.Text = "0";  //RS485
            label8.Text = "0";  //RS485
            label21.Text = "0"; //CANopen
            label22.Text = "0"; //CANopen
            label28.Text = "0"; //ModbusTCP
            label29.Text = "0"; //ModbusTCP
        }

        //关闭键颜色复位
        private void ui_reset_btnClose()
        {
            button1.BackColor = label1.BackColor;
            button2.BackColor = label1.BackColor;
            button5.BackColor = label1.BackColor;
            button6_close.BackColor = label1.BackColor;
        }

        //发送键颜色复位
        private void ui_reset_btnSendColor()
        {
            bt_send1.BackColor = label1.BackColor;
            bt_send2.BackColor = label1.BackColor;
            bt_send3.BackColor = label1.BackColor;
            bt_send4.BackColor = label1.BackColor;
            bt_send5.BackColor = label1.BackColor;
            bt_send6.BackColor = label1.BackColor;
        }

        //发送键文字复位
        private void ui_reset_btnSendText()
        {
            if (MyDevice.languageType == 0)
            {
                bt_send1.Text = "连 接";
                bt_send2.Text = "扫描";
                bt_send3.Text = "连 接";
                bt_send4.Text = "连 接";
                bt_send5.Text = "侦测";
                bt_send6.Text = "连 接";
            }
            else
            {
                bt_send1.Text = "Connect";
                bt_send2.Text = "Scan";
                bt_send3.Text = "Connect";
                bt_send4.Text = "Connect";
                bt_send5.Text = "Detect";
                bt_send6.Text = "Connect";
            }
        }

        #endregion

        /// <summary>
        /// 获取本地的ip
        /// </summary>
        private void getLocalIPAddress()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    if (!comboBox4_IP.Items.Contains(AddressIP))
                    {
                        comboBox4_IP.Items.Add(AddressIP);
                    }
                }
            }
            comboBox4_IP.SelectedIndex = 0;
        }
    }
}

