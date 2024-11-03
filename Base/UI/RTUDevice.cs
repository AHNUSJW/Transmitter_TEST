using Library;
using Base.UI.MyControl;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Text;

//Lumi 20230926
//Lumi 20231106

namespace Base.UI
{
    public partial class RTUDevice : Form
    {
        #region 参数

        private XET actXET;     //需要操作的设备

        //激活的模块
        private enum ActiveGroupBox
        {
            connect,           //连接
            data,              //测量值，状态设置，数据曲线
            para,              //参数
            cal,               //标定
        }
        private ActiveGroupBox activeGroupBox = ActiveGroupBox.connect;   //先连接

        //连接
        private Byte addr = 1;                      //扫描站点1-255
        private bool isScan = false;                //是否点击扫描
        private bool isFormUartConnect = false;     //是否有连接委托
        private bool isFormUart = false;            //false:从未连接设备，true:连接过设备
        private int myDevSum = 0;                   //设备数量

        //测量值,状态设置
        private int comTicker;                                               //发送指令计时器
        private volatile int addrIndex;                                      //已连接设备的地址指针
        private List<Byte> mutiAddres = new List<Byte>();                    //存储已连接设备的地址
        private List<MutiDevice485> mutiDevices = new List<MutiDevice485>(); //多设备列表
        private TASKS nextTask;                                              //按键指令,TASKS.ZERO,TASKS.TARE,TASKS.BOR

        //数据曲线
        private DrawPicture drawPicture = new DrawPicture();    //绘图
        private int showTicker;             //控制显示速度
        private System.Timers.Timer timer;  //定时记录数据
        private DateTime stopTime;          //结束时间
        private Boolean isWrite;            //写入数据
        private int count;                  //记录数据列表数据个数

        //参数设置
        private byte oldOutype;     //影响模拟量DAC配置,影响da_point,影响斜率计算
        private byte oldAdspeed;    //影响CS1237初始化,影响灵敏度,影响下ad_point,影响斜率计算
        private byte oldCurve;      //影响ad_point和da_point,影响斜率计算
        private byte oldDecimal;    //影响da_point,影响斜率计算

        private string activeButton = "";    //当前按键，区分参数确认键buttonX_Para和标定写入键bt_Write

        //分配站点
        private byte oldAddr;    //被分配站点的设备的原站点
        private byte newAddr;    //被分配的新站点
        private uint devWeight;  //设定的重量

        //RTU参数设置


        #endregion

        public RTUDevice()
        {
            InitializeComponent();
        }

        // 界面加载
        private void RTUDevice_Load(object sender, EventArgs e)
        {
            //图标加载
            string logoIconPath = Path.Combine(Application.StartupPath, "pic", "logo.ico");
            if (File.Exists(logoIconPath))
            {
                this.Icon = new Icon(logoIconPath);
            }
            else
            {
                this.Icon = Properties.Resources.BCS16A;
            }

            //加载窗口标题Text
            load_FormText();

            //刷新串口
            buttonRefresh_Click(null, null);

            //初始化RTU设置
            load_RTUConfig();

            //全屏
            update_FullScreen();

            //全屏时，以管理员身份进入
            if (comboBox3_AutoStart.SelectedIndex == 1)
            {
                MyDevice.D_username = "admin";
                MyDevice.D_password = "123456";
            }

            //
            actXET = MyDevice.actDev;

            //界面参数
            comboBox2_BaudRate.Text = MyDevice.myRS485.baudRate.ToString();
            comboBox3_StopBits.SelectedIndex = (byte)MyDevice.myRS485.stopBits - 1;
            comboBox4_Parity.SelectedIndex = (byte)MyDevice.myRS485.parity;
            textBox1.Text = MyDevice.myRS485.addr.ToString();
            drawPicture = new DrawPicture(pictureBox1.Height, pictureBox1.Width);

            MyDevice.myUpdate += new freshHandler(update_FromUart);
            timer1.Enabled = true;

            //显示或隐藏连接以外的模块
            load_GroupBox(false);

            //依据分辨率调整groupbox的可见性和布局
            load_GroupboxLocation();

            //初始化状态设置
            load_SetState();

            //初始化参数设置
            load_SetPara();

            //初始化标定
            load_SetCal();

            //初始化数据表格
            load_ListView();

            //初始化分配站点
            load_AllocateAddr();

            //自动连接
            auto_Connect();
        }

        // 界面关闭
        private void RTUDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart_Connect);
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            timerConnect.Enabled = false;
            timer1.Enabled = false;
        }

        //界面大小变化
        private void RTUDevice_SizeChanged(object sender, EventArgs e)
        {
            //曲线窗口改变
            if (isFormUart)
            {
                drawPicture.Height = pictureBox1.Height;
                drawPicture.Width = pictureBox1.Width;
                pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();
            }
        }

        //显示或隐藏连接以外的模块
        private void load_GroupBox(bool isEnable)
        {
            groupBoxPara.Enabled = isEnable;
            groupBoxData.Enabled = isEnable;
            groupBoxSet.Enabled = isEnable;
            groupBoxAddr.Enabled = isEnable;
            groupBoxCurve.Enabled = isEnable;
            groupBoxCal.Enabled = isEnable;

            //控件可见
            switch (actXET.S_DeviceType)
            {
                default:
                    groupBoxAddr.Visible = false;
                    break;
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    groupBoxAddr.Visible = true;
                    break;
            }
        }

        //依据分辨率调整groupbox的可见性和布局
        private void load_GroupboxLocation()
        {
            if (this.Height < 841)
            {
                //隐藏参数设置
                groupBoxPara.Visible = false;
            }

            if (this.Width < 1041)
            {
                //隐藏曲线
                groupBoxCurve.Visible = false;
            }

            if (!this.groupBoxPara.Visible)
            {
                //groupBoxCal的位置修改为groupBoxPara的位置
                this.groupBoxCal.Location = this.groupBoxPara.Location;
                //groupBoxAddr的位置同步向上调整
                this.groupBoxAddr.Location = new Point(groupBoxCal.Location.X, groupBoxCal.Location.Y + groupBoxCal.Height + 6);
            }
        }

        //限制站点输入
        private void textBoxAddr_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text != "" && !byte.TryParse(textBox.Text, out byte b) || textBox.Text == "0")
            {
                textBox.Text = "";
            }
        }

        //限制站点输入
        private void textBox_Addr_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入正整数
            BoxRestrict.KeyPress_IntegerPositive_len4(sender, e);
        }

        #region 连接

        // 停止位改变
        private void comboBox3_StopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3_StopBits.SelectedIndex > 0)
            {
                comboBox4_Parity.SelectedIndex = 0;
            }
        }

        // 校验位改变
        private void omboBox4_Parity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4_Parity.SelectedIndex > 0)
            {
                comboBox3_StopBits.SelectedIndex = 0;
            }
        }

        //RS485协议接口扫描
        private void bt_send2_Click(object sender, EventArgs e)
        {
            activeGroupBox = ActiveGroupBox.connect;

            if (!isFormUartConnect)
            {
                MyDevice.myUpdate += new freshHandler(update_FromUart_Connect);
                isFormUartConnect = true;
            }

            //点击扫描标志
            isScan = true;

            //扫描中
            if (timerConnect.Enabled)
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
                        case "1200": timerConnect.Interval = 200; break; //150
                        case "2400": timerConnect.Interval = 100; break; //75
                        case "4800": timerConnect.Interval = 50; break;  //38
                        case "9600": timerConnect.Interval = 40; break;  //19
                        case "14400": timerConnect.Interval = 17; break; //13
                        case "19200": timerConnect.Interval = 13; break; //10
                        case "38400": timerConnect.Interval = 7; break;  //5
                        case "57600": timerConnect.Interval = 5; break;  //4
                        case "115200": timerConnect.Interval = 10; break; //2
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
                        MessageBox.Show("串口未打开，检查串口是否被占用");
                    }
                }
            }
        }

        // RS485协议接口连接
        private void bt_send3_Click(object sender, EventArgs e)
        {
            //检测该站点的设备是否已连接
            byte addrByte;
            if (mutiAddres != null)
            {
                if (byte.TryParse(textBox1.Text, out addrByte) && mutiAddres.Contains(addrByte))
                {
                    //该设备已经连接
                    return;
                }
            }

            activeGroupBox = ActiveGroupBox.connect;

            if (!isFormUartConnect)
            {
                MyDevice.myUpdate += new freshHandler(update_FromUart_Connect);
                isFormUartConnect = true;
            }

            if (comboBox1_port.Text != null)
            {
                MyDevice.myRS485.Protocol_ClearState();
                Thread.Sleep(60);

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
                    MessageBox.Show("串口未打开，检查串口是否被占用");
                }
            }
        }

        //刷新按钮
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            //刷串口
            comboBox1_port.Items.Clear();
            comboBox1_port.Items.AddRange(SerialPort.GetPortNames());
            //无串口
            if (comboBox1_port.Items.Count == 0)
            {
                comboBox1_port.Text = null;
            }
            //有可用串口
            else
            {
                comboBox1_port.Text = MyDevice.myRS485.portName;
                if (comboBox1_port.SelectedIndex < 0)
                {
                    comboBox1_port.SelectedIndex = 0;
                }
            }
        }

        // 关闭按钮
        private void button2Close_Click(object sender, EventArgs e)
        {
            if (comboBox1_port.Text != null)
            {
                activeGroupBox = ActiveGroupBox.connect;

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
                }

                load_DeviceFormActive();
                load_GroupBox(false);
            }
        }

        // 串口委托
        private void update_FromUart_Connect()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(update_FromUart_Connect);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                //扫描到设备启动读出SCT
                if (MyDevice.myRS485.trTASK == TASKS.SCAN)
                {
                    ui_Connect_RS485();
                    textBox1.Text = MyDevice.myRS485.addr.ToString();
                    MyDevice.myRS485.Protocol_ClearState();
                }

                //继续读取
                MyDevice.protocol.Protocol_mePort_ReadTasks();
                //界面状态
                if ((!isScan) || (isScan && MyDevice.protocol.trTASK != TASKS.NULL))
                {
                    bt_send3.Text = MyDevice.protocol.trTASK.ToString();
                }
                if (MyDevice.protocol.trTASK == TASKS.NULL)
                {
                    //保存RTU参数
                    save_RTUConfig(MyDevice.myRS485.addr);

                    if (isScan)
                    {
                        //继续扫描
                        addr = addr > MyDevice.myRS485.addr ? MyDevice.myRS485.addr : addr;
                        timerConnect.Enabled = true;
                        bt_send3.Text = "已连接";
                        bt_send3.BackColor = Color.Green;
                    }
                    else
                    {
                        ui_Connected_RS485();
                    }
                }
            }
        }

        // RS485协议接口间隔扫描事件
        private void timerConnect_Tick(object sender, EventArgs e)
        {
            if (MyDevice.mySelfUART.Is_serial_listening == true) return;
            if (MyDevice.myRS485.IsOpen)
            {
                //扫描地址1-255
                if ((++addr) != 1)
                {
                    //
                    textBox1.Text = addr.ToString();

                    //扫描
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
                MessageBox.Show("串口未打开，检查串口是否被占用");
            }
        }

        private void ui_Close_RS485()
        {
            timerConnect.Enabled = false;//关闭扫描

            button2.BackColor = Color.Green;
            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.CadetBlue;

            bt_send2.Text = "扫 描";
            bt_send3.Text = "连 接";
        }

        private void ui_Connect_RS485()
        {
            timerConnect.Enabled = false;//关闭扫描

            button2.BackColor = Color.CadetBlue;
            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.Firebrick;

            bt_send2.Text = "扫 描";
            bt_send3.Text = "连 接";
        }

        //停止扫描
        private void ui_Stop_Scan()
        {
            timerConnect.Enabled = false;//关闭扫描

            if (isFormUartConnect)
            {
                MyDevice.myUpdate -= new freshHandler(update_FromUart_Connect);
                isFormUartConnect = false;
            }
            isFormUart = true;

            button2.BackColor = Color.CadetBlue;
            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.CadetBlue;

            bt_send2.Text = "扫 描";
            bt_send3.Text = "连 接";

            DeviceFormActive();
            activeGroupBox = ActiveGroupBox.data;
        }

        private void ui_Start_Scan()
        {
            timerConnect.Enabled = true;//启动扫描
            ;
            button2.BackColor = Color.CadetBlue;
            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.CadetBlue;

            bt_send2.Text = "停 止";
            bt_send3.Text = "连 接";
        }

        //扫描完毕
        private void ui_Finish_Scan()
        {
            timerConnect.Enabled = false;//关闭扫描

            if (isFormUartConnect)
            {
                MyDevice.myUpdate -= new freshHandler(update_FromUart_Connect);
                isFormUartConnect = false;
            }
            isFormUart = true;

            button2.BackColor = Color.CadetBlue;
            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.CadetBlue;

            bt_send2.Text = "扫描完毕";
            bt_send3.Text = "连 接";

            DeviceFormActive();
            activeGroupBox = ActiveGroupBox.data;
        }

        //已连接
        private void ui_Connected_RS485()
        {
            timerConnect.Enabled = false;//关闭扫描

            if (isFormUartConnect)
            {
                MyDevice.myUpdate -= new freshHandler(update_FromUart_Connect);
                isFormUartConnect = false;
            }
            isFormUart = true;

            button2.BackColor = Color.CadetBlue;

            bt_send2.BackColor = Color.CadetBlue;
            bt_send3.BackColor = Color.Green;

            bt_send2.Text = "扫 描";
            bt_send3.Text = "已连接";

            DeviceFormActive();
            activeGroupBox = ActiveGroupBox.data;
        }

        #endregion

        #region 测量值

        //初始化设备数据显示窗口
        private void load_DeviceFormActive()
        {
            mutiAddres = new List<Byte>();
            mutiDevices = new List<MutiDevice485>();
            tableLayoutPanel1.Controls.Clear();

            //更新多选框
            comboBoxUnit.SelectedIndex = 0;
            comboBoxAddr.Items.Clear();
            comboBoxAddr.Items.Add("");
            comboBoxAddr.SelectedIndex = 0;

            //初始化绘图背景
            update_Picture();
        }

        //激活设备数据显示窗口
        private void DeviceFormActive()
        {
            //多设备显示窗口
            if (MyDevice.devSum > 0)
            {
                mutiAddres = new List<Byte>();
                mutiDevices = new List<MutiDevice485>();

                actXET = MyDevice.actDev;

                //将已连接设备的地址存入列表
                for (Byte i = 1; i != 0; i++)
                {
                    if (MyDevice.mBUS[i].sTATE == STATE.WORKING)
                    {
                        Byte mutiAddr = new Byte();

                        mutiAddr = i;
                        mutiAddres.Add(mutiAddr);
                    }
                }

                myDevSum = MyDevice.devSum > mutiAddres.Count ? mutiAddres.Count : MyDevice.devSum;

                //调整tableLayoutTable布局
                update_Layout(myDevSum);

                //清空多设备列表
                tableLayoutPanel1.Controls.Clear();

                //根据连接设备数增加MutiDevice控件
                for (int n = 1, j = 0; n <= myDevSum; n++)
                {
                    MyDevice.protocol.addr = mutiAddres[n - 1];
                    MutiDevice485 mutiDevice485 = new MutiDevice485();
                    mutiDevice485.DataFontSize = update_DeviceDataFontSize(myDevSum);
                    mutiDevice485.UnitFontSize = update_DeviceUnitFontSize(myDevSum);
                    mutiDevice485.AddrFontSize = update_DeviceAddrFontSize(myDevSum);
                    mutiDevices.Add(mutiDevice485);
                    tableLayoutPanel1.Controls.Add(mutiDevice485, (n - 1) % 4, j);
                    mutiDevice485.Dock = DockStyle.Fill;

                    mutiDevice485.Address = mutiAddres[n - 1].ToString();

                    //4个为一行
                    if (n % 4 == 0)
                    {
                        j++;
                    }
                }
                addrIndex = 0;

                //根据不同波特率有不同发送间隔
                switch (MyDevice.protocol.type)
                {
                    default:
                        break;

                    case COMP.RS485:
                        if (MyDevice.protocol.baudRate == 1200)
                        {
                            timer1.Interval = 250;
                        }
                        else if (MyDevice.protocol.baudRate == 2400)
                        {
                            timer1.Interval = 150;
                        }
                        else
                        {
                            timer1.Interval = 100;
                        }
                        break;
                }

                //groupbox显示
                load_GroupBox(true);
                //更新站点选择
                update_ComboBoxAddr();
                //更新单位选择
                update_ComboBoxUnit();
                //更新显示单位
                foreach (var item in mutiDevices)
                {
                    item.Unit = comboBoxUnit.Text;
                }

                //更新绘图背景
                update_Picture();

                //启动数据读取
                start_dataMonitor();
            }
        }

        //更新测量值窗口布局
        private void update_Layout(int devSum)
        {
            //调整tableLayoutTable布局
            if (devSum > 0)
            {
                int rows;
                int columns;

                switch (devSum)
                {
                    case int n when (n == 1):
                        rows = 1;
                        columns = 1;
                        break;
                    case int n when (n >= 2 && n <= 4):
                        rows = 2;
                        columns = 2;
                        break;
                    case int n when (n >= 5 && n <= 6):
                        rows = 3;
                        columns = 2;
                        break;
                    case int n when (n >= 7 && n <= 9):
                        rows = 3;
                        columns = 3;
                        break;
                    case int n when (n >= 10 && n <= 12):
                        rows = 4;
                        columns = 3;
                        break;
                    case int n when (n >= 13 && n <= 16):
                        rows = 4;
                        columns = 4;
                        break;
                    case int n when (n >= 17 && n <= 20):
                        rows = 5;
                        columns = 4;
                        break;
                    default:
                        rows = (devSum + 3) / 4;
                        columns = 4;
                        break;
                }

                tableLayoutPanel1.RowCount = rows;
                tableLayoutPanel1.ColumnCount = columns;

                // 设置列的百分比宽度
                tableLayoutPanel1.ColumnStyles.Clear();
                for (int i = 0; i < columns; i++)
                {
                    tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f / columns));
                }

                // 设置行高
                tableLayoutPanel1.RowStyles.Clear();
                if (devSum <= 20)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f / rows));
                    }
                }
                else
                {
                    for (int i = 0; i < rows; i++)
                    {
                        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 109));
                    }
                }
            }
        }

        //更新MutiDevice控件数值的字体大小
        private float update_DeviceDataFontSize(int devSum)
        {
            float fontsize = 25;

            switch (devSum)
            {
                case int n when (n == 1):
                    fontsize = 70;
                    break;
                case int n when (n >= 2 && n <= 6):
                    fontsize = 54;
                    break;
                case int n when (n >= 7 && n <= 12):
                    fontsize = 40;
                    break;
                case int n when (n >= 13 && n <= 20):
                    fontsize = 30;
                    break;
                case int n when (n >= 21):
                default:
                    break;
            }

            return fontsize;
        }

        //更新MutiDevice控件单位的字体大小
        private float update_DeviceUnitFontSize(int devSum)
        {
            float fontsize = 15;

            switch (devSum)
            {
                case int n when (n == 1):
                    fontsize = 36;
                    break;
                case int n when (n >= 2 && n <= 6):
                    fontsize = 30;
                    break;
                case int n when (n >= 7 && n <= 12):
                    fontsize = 24;
                    break;
                case int n when (n >= 13 && n <= 20):
                    fontsize = 18;
                    break;
                case int n when (n >= 21):
                default:
                    break;
            }

            return fontsize;
        }

        //更新MutiDevice控件地址的字体大小
        private float update_DeviceAddrFontSize(int devSum)
        {
            float fontsize = 11;

            switch (devSum)
            {
                case int n when (n == 1):
                    fontsize = 24;
                    break;
                case int n when (n >= 2 && n <= 6):
                    fontsize = 17;
                    break;
                case int n when (n >= 7 && n <= 12):
                    fontsize = 15;
                    break;
                case int n when (n >= 13 && n <= 20):
                    fontsize = 12.75f;
                    break;
                case int n when (n >= 21):
                default:
                    break;
            }

            return fontsize;
        }

        //更新显示参数
        private void update_OutText()
        {
            actXET = MyDevice.actDev;

            if (count % 10000 == 0)
            {
                dbListView1.Items.Clear();
            }

            //更新litView
            ListViewItem item = new ListViewItem();
            item.SubItems[0].Text = (count++).ToString();
            item.SubItems.Add(actXET.E_addr.ToString());
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss:fff"));

            //数值 & 超载
            if (actXET.R_overload)
            {
                mutiDevices[addrIndex].Data = "---OL---";
                mutiDevices[addrIndex].DataColor = Color.Red;

                //表格添加数据
                if (actXET.R_grossnet == "毛重")
                {
                    item.SubItems.Add("OG");
                }
                else
                {
                    item.SubItems.Add("ON");
                }
                item.SubItems.Add(mutiDevices[addrIndex].Data);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(actXET.R_weight)) return;

                //更新测量值和存储数据
                mutiDevices[addrIndex].DataColor = Color.Black;

                //单位
                string originalUnit;
                originalUnit = actXET.GetUnitUMASK();
                if (comboBoxUnit.SelectedItem == null || originalUnit == comboBoxUnit.Text)
                {
                    //不转换
                    mutiDevices[addrIndex].Unit = originalUnit;
                    mutiDevices[addrIndex].Data = actXET.R_weight;
                    //画曲线S(毛重净重状态下传入实时数据)
                    if (showTicker > 0)
                    {
                        if (mutiAddres[addrIndex].ToString().Equals(comboBoxAddr.Text))
                        {
                            showTicker = 0;
                            pictureBox1.Image = drawPicture.GetForegroundImageTypeOne(double.Parse(actXET.R_weight));
                        }
                    }
                }
                else
                {
                    //转换单位
                    mutiDevices[addrIndex].Unit = comboBoxUnit.Text == "无" ? "" : comboBoxUnit.Text;
                    mutiDevices[addrIndex].Data = UnitHelper.ConvertUnit(actXET.R_weight.Trim(), actXET.E_wt_decimal, originalUnit, comboBoxUnit.Text);
                    string dataStr = UnitHelper.ConvertUnit(actXET.R_weight.Trim(), actXET.E_wt_decimal, originalUnit, comboBoxUnit.Text, false);
                    //画曲线S(毛重净重状态下传入实时数据)
                    if (showTicker > 0)
                    {
                        if (mutiAddres[addrIndex].ToString().Equals(comboBoxAddr.Text))
                        {
                            showTicker = 0;
                            pictureBox1.Image = drawPicture.GetForegroundImageTypeOne(double.Parse(dataStr));
                        }
                    }
                }

                //更新数据加总
                Dictionary<string, double> WeightSums = new Dictionary<string, double>();
                foreach (MutiDevice485 dev in mutiDevices)
                {
                    if (double.TryParse(dev.Data, out double weight))
                    {
                        if (!WeightSums.ContainsKey(dev.Unit))
                        {
                            WeightSums[dev.Unit] = 0;
                        }
                        WeightSums[dev.Unit] += weight;
                    }
                }
                if (WeightSums.Count > 0)
                {
                    StringBuilder sumText = new StringBuilder("数据加总: ");
                    for (int i = 0; i < WeightSums.Count; i++)
                    {
                        var unitSum = WeightSums.ElementAt(i);
                        sumText.Append(unitSum.Value.ToString() + unitSum.Key);
                        if (i < WeightSums.Count - 1)
                        {
                            sumText.Append(", ");
                        }
                    }
                    label8_dataSum.Text = sumText.ToString();
                }
                else
                {
                    label8_dataSum.Text = "";
                }

                //稳定状态写入
                string status;
                if (actXET.R_stable) status = "S";
                else status = "U";

                if (actXET.R_grossnet == "毛重") status += "G";
                else status += "N";

                //表格添加数据
                item.SubItems.Add(status);
                item.SubItems.Add(mutiDevices[addrIndex].Data.PadLeft(9, ' '));
            }

            //表格添加数据单位
            item.SubItems.Add(mutiDevices[addrIndex].Unit);

            //将更新的listVew行添加进listView
            dbListView1.Items.Add(item);
            isWrite = true;//更新导入文件状态

            //稳定状态
            if (actXET.R_stable)
            {
                mutiDevices[addrIndex].LampColor = new Color[] { Color.Green };
            }
            else
            {
                mutiDevices[addrIndex].LampColor = new Color[] { Color.Black };
            }
        }

        //串口通讯响应
        public void update_FromUart()
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
            else
            {
                if (activeGroupBox == ActiveGroupBox.data)
                {
                    //检测外部EEPROM芯片
                    //有MyDevice.mePort_Write***()的WinForm,就会写SCT01235,就需要提示TEDS芯片插入
                    if (actXET.R_checklink != actXET.R_eeplink)
                    {
                        if (actXET.R_eeplink)
                        {
                            if (MessageBox.Show("检测到TEDS芯片插入, 确定将格式化并设置新的传感器数据, 可以取消并重新连接读出传感器数据?", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                        actXET.R_checklink = actXET.R_eeplink;
                    }

                    switch (nextTask)
                    {
                        case TASKS.QGROSS:
                        case TASKS.QNET:
                            if (activeGroupBox == ActiveGroupBox.data)
                            {
                                comTicker = 0;

                                //轮询发送问答指令
                                addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                MyDevice.protocol.addr = mutiAddres[addrIndex];

                                switch (mutiDevices[addrIndex].Outype)
                                {
                                    default:
                                    case "毛重":
                                        MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                        break;

                                    case "净重":
                                        MyDevice.mePort_SendCOM(TASKS.QNET);
                                        break;
                                }

                                //更新显示参数
                                update_OutText();
                            }
                            break;

                        //执行归零任务
                        case TASKS.ZERO:
                            nextTask = TASKS.NULL;
                            //指定要归零的设备地址
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_SendCOM(TASKS.ZERO);
                            break;

                        //执行扣重任务
                        case TASKS.TARE:
                            nextTask = TASKS.NULL;
                            //指定要扣重的设备地址
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_SendCOM(TASKS.TARE);
                            break;

                        //参数确认
                        case TASKS.WRX0:
                            nextTask = TASKS.NULL;
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_ClearState();
                            MyDevice.mePort_WriteAllTasks();
                            //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                            buttonX_Para.Text = MyDevice.protocol.trTASK.ToString();

                            //写完了
                            if (MyDevice.protocol.trTASK == TASKS.NULL)
                            {
                                buttonX_Para.HoverBackColor = Color.Green;
                                buttonX_Para.Text = "成 功";

                                Refresh();
                                //启动读数据
                                start_dataMonitor();
                            }
                            break;

                        //标定写入
                        case TASKS.BCC:
                            nextTask = TASKS.NULL;
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_ClearState();
                            MyDevice.mePort_WriteCalTasks();

                            //读结束
                            if (MyDevice.protocol.trTASK == TASKS.NULL)
                            {
                                bt_Write.HoverBackColor = Color.Green;
                                bt_Write.Text = "成 功";
                                Refresh();
                                //启动读数据
                                start_dataMonitor();
                            }
                            break;

                        //执行一次标定零点
                        case TASKS.ADCP1:
                            nextTask = TASKS.NULL;
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_SendCOM(TASKS.ADCP1);
                            break;

                        //执行一次标定满点
                        case TASKS.ADCP5:
                            nextTask = TASKS.NULL;
                            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                            MyDevice.mePort_SendCOM(TASKS.ADCP5);
                            break;

                        case TASKS.HWADDR:
                            nextTask = TASKS.NULL;
                            MyDevice.myRS485.Protocol_HwSetAddr(newAddr);
                            break;

                        case TASKS.SWADDR:
                            nextTask = TASKS.NULL;
                            MyDevice.myRS485.Protocol_SwSetAddr(newAddr, devWeight);
                            break;

                        default:
                            switch (MyDevice.protocol.trTASK)
                            {
                                case TASKS.ZERO:
                                    nextTask = TASKS.QGROSS;
                                    break;

                                case TASKS.TARE:
                                    //扣重后启动读数据
                                    start_dataMonitor_QNET();
                                    break;

                                //零点采集
                                case TASKS.ADCP1:
                                    actXET.RefreshRatio();
                                    bt_Zero.HoverBackColor = Color.Green;
                                    nextTask = TASKS.QGROSS;
                                    break;

                                //满点采集
                                case TASKS.ADCP5:
                                    actXET.RefreshRatio();
                                    bt_Max.HoverBackColor = Color.Green;
                                    nextTask = TASKS.QGROSS; ;
                                    break;

                                case TASKS.REST:
                                    //写入完毕,老版本重启指令无回复
                                    if (MyDevice.mSUT.E_test < 0x58)
                                    {
                                        if (activeButton == "buttonX_Para")
                                        {
                                            buttonX_Para.HoverBackColor = Color.Green;
                                            buttonX_Para.Text = "成 功";
                                        }
                                        else if (activeButton == "bt_Write")
                                        {
                                            bt_Write.HoverBackColor = Color.Green;
                                            bt_Write.Text = "成 功";
                                        }
                                        nextTask = TASKS.QGROSS;
                                    }
                                    break;

                                case TASKS.HWADDR:
                                case TASKS.SWADDR:
                                    actXET.RefreshRatio();
                                    buttonX_Addr.HoverBackColor = Color.Green;

                                    MyDevice.mBUS[oldAddr].sTATE = STATE.OFFLINE;
                                    MyDevice.mBUS_DeepCopy(newAddr, oldAddr);
                                    MyDevice.mBUS[newAddr].E_addr = newAddr;
                                    MyDevice.mBUS[newAddr].sTATE = STATE.WORKING;
                                    DeviceFormActive();
                                    break;

                                default:
                                    DTiws.View.ButtonX activeBtn = Controls.Find(activeButton, true).FirstOrDefault() as DTiws.View.ButtonX;
                                    if (activeBtn != null)
                                    {
                                        //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                                        activeBtn.Text = MyDevice.protocol.trTASK.ToString();
                                        //继续写
                                        if (activeButton == "buttonX_Para")
                                        {
                                            MyDevice.mePort_WriteAllTasks();
                                        }
                                        else if (activeButton == "bt_Write")
                                        {
                                            MyDevice.mePort_WriteCalTasks();
                                        }
                                        //读实时重量
                                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                                        {
                                            activeBtn.HoverBackColor = Color.Green;
                                            activeBtn.Text = "成 功";
                                            nextTask = TASKS.QGROSS;
                                        }
                                    }
                                    break;
                            }
                            Refresh();
                            break;
                    }
                }
            }
        }

        //超时监控
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (activeGroupBox == ActiveGroupBox.data)
            {
                showTicker++;

                switch (nextTask)
                {
                    case TASKS.QGROSS:
                    case TASKS.QNET:
                        switch (mutiDevices[addrIndex].Outype)
                        {
                            default:
                            case "毛重":
                                //超时询问毛重
                                if ((++comTicker) > 3)
                                {
                                    addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                    MyDevice.protocol.addr = mutiAddres[addrIndex];

                                    comTicker = 0;
                                    MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                }
                                break;

                            case "净重":
                                //超时询问净重
                                if ((++comTicker) > 3)
                                {
                                    addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                    MyDevice.protocol.addr = mutiAddres[addrIndex];

                                    comTicker = 0;
                                    MyDevice.mePort_SendCOM(TASKS.QNET);
                                }
                                break;
                        }
                        break;
                    case TASKS.ZERO:
                        if ((++comTicker) > 3)
                        {
                            comTicker = 0;
                            MyDevice.mePort_SendCOM(TASKS.ZERO);
                        }
                        break;

                    case TASKS.TARE:
                        if ((++comTicker) > 3)
                        {
                            comTicker = 0;
                            MyDevice.mePort_SendCOM(TASKS.TARE);
                        }
                        break;
                    default:
                        switch (MyDevice.protocol.trTASK)
                        {
                            case TASKS.HWADDR:
                            case TASKS.SWADDR:
                                //超时询问毛重
                                if ((++comTicker) > 5)
                                {
                                    comTicker = 0;
                                    nextTask = TASKS.QGROSS;
                                }
                                break;
                        }
                        break;
                }
            }
        }

        //启动数据读取
        private void start_dataMonitor()
        {
            comTicker = 0;
            nextTask = TASKS.QGROSS;
            MyDevice.protocol.trTASK = TASKS.QGROSS;
            MyDevice.mePort_ClearState();//清除数据确保R_eeplink不会02或03误设false

            foreach (MutiDevice485 device in mutiDevices)
            {
                device.Outype = "毛重";
            }

            buttonX_QGROSS.HoverBackColor = Color.Green;
            buttonX_QNET.HoverBackColor = Color.DarkGray;
            Refresh();
        }

        //启动数据读取,净重
        private void start_dataMonitor_QNET()
        {
            comTicker = 0;
            nextTask = TASKS.QNET;
            MyDevice.protocol.trTASK = TASKS.QNET;
            MyDevice.mePort_ClearState();//清除数据确保R_eeplink不会02或03误设false

            foreach (MutiDevice485 device in mutiDevices)
            {
                device.Outype = "净重";
            }

            buttonX_QGROSS.HoverBackColor = Color.DarkGray;
            buttonX_QNET.HoverBackColor = Color.Green;
            Refresh();
        }

        #endregion

        #region 状态设置

        //初始化状态设置
        private void load_SetState()
        {
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            || ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli"))
            || ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456")))
            {
                buttonX_ZERO.Enabled = true;
                buttonX_AZERO.Enabled = true;
            }
            else
            {
                buttonX_ZERO.Enabled = false;
                buttonX_AZERO.Enabled = false;
            }
        }

        //毛重净重panel的重绘
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel3.ClientRectangle,
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid, //左边
            Color.White, 1, ButtonBorderStyle.Solid, //上边
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid, //右边
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid);//底边
        }

        //归零全部归零panel的重绘
        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel4.ClientRectangle,
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid, //左边
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid, //上边
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid, //右边
            SystemColors.ControlLight, 1, ButtonBorderStyle.Solid);//底边
        }

        //更新ComboBoxUnit
        public void update_ComboBoxUnit()
        {
            //初始化comboBox
            string unit;
            string unitCategory = UnitHelper.GetUnitCategory((UNIT)actXET.E_wt_unit);
            comboBoxUnit.Items.Clear();
            switch (unitCategory)
            {
                case "重量":
                case "力":
                case "扭矩":
                case "压力":
                case "温度":
                    var unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, unitCategory);
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unit = UnitHelper.GetUnitDescription((UNIT)actXET.E_wt_unit);
                    break;
                case "无":
                    comboBoxUnit.Items.Add("无");
                    unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, "重量");
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, "力");
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, "扭矩");
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, "压力");
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, "温度");
                    foreach (var item in unitItems)
                    {
                        comboBoxUnit.Items.Add(item);
                    }
                    unit = "无";
                    break;
                case "其它":
                default:
                    //如果是老版本ct之后的单位，comboxbox的项为空
                    unit = UnitHelper.GetUnitAdjustedDescription((UNIT)actXET.E_wt_unit);
                    comboBoxUnit.Items.Add(unit);
                    break;
            }

            for (int i = 0; i < comboBoxUnit.Items?.Count; i++)
            {
                if (comboBoxUnit.Items[i].ToString() == unit)
                {
                    comboBoxUnit.SelectedIndex = i;
                    break;
                }
            }
        }

        //更新ComboBoxAddr表
        public void update_ComboBoxAddr()
        {
            //初始化comboBox
            comboBoxAddr.Items.Clear();
            foreach (byte address in mutiAddres)
            {
                comboBoxAddr.Items.Add(address);
            }

            if (comboBoxAddr.Items.Count > 0)
            {
                comboBoxAddr.SelectedIndex = 0;
            }
        }

        //毛重
        private void buttonX_QGROSS_Click(object sender, EventArgs e)
        {
            nextTask = TASKS.QGROSS;
            foreach (MutiDevice485 device in mutiDevices)
            {
                device.Outype = "毛重";
            }

            buttonX_QGROSS.HoverBackColor = Color.Green;
            buttonX_QNET.HoverBackColor = Color.DarkGray;
            Refresh();

            //曲线清空
            clear_Curve();
        }

        //净重
        private void buttonX_QNET_Click(object sender, EventArgs e)
        {
            nextTask = TASKS.QNET;
            foreach (MutiDevice485 device in mutiDevices)
            {
                device.Outype = "净重";
            }

            buttonX_QNET.HoverBackColor = Color.Green;
            buttonX_QGROSS.HoverBackColor = Color.DarkGray;
            Refresh();

            //曲线清空
            clear_Curve();
        }

        //全部归零，广播指令
        private void buttonX_AZERO_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("是否净重、毛重、皮重全部归零？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (res == DialogResult.OK)
            {
                MyDevice.mePort_SendCOM(TASKS.AZERO);

                //启动数据读取
                start_dataMonitor();

                //曲线清空
                clear_Curve();
            }
        }

        //单位
        private void comboBoxUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAddr.Text.Equals("")) return;
            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            //更新曲线 
            update_Picture();
        }

        //站点
        private void comboBoxAddr_SelectedIndexChanged(object sender, EventArgs e)
        {
            //选择的设备站点
            if (comboBoxAddr.Text.Equals("")) return;
            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            //更新单位选择
            update_ComboBoxUnit();

            //更新曲线 
            update_Picture();

            //更新参数设置
            update_groupBoxPara();

            //更新标定
            update_groupBoxCal();
        }

        //去皮
        private void buttonX_TARE_Click(object sender, EventArgs e)
        {
            nextTask = TASKS.TARE;
        }

        //归零
        private void buttonX_ZERO_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("是否净重、毛重、皮重全部归零？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (res == DialogResult.OK)
            {
                nextTask = TASKS.ZERO;

                clear_Curve();
            }
        }

        #endregion

        #region 数据曲线

        //初始化绘图
        private void update_Picture()
        {
            //寻找对应站点号的设备
            if (mutiDevices.Count < 1) return;
            var selectedDevice = mutiDevices.Where(device => device.Address == comboBoxAddr.Text);
            if (selectedDevice == null) return;
            List<string> sOutypeValue = selectedDevice.Select(device => device.S_Outype).ToList();
            if (sOutypeValue.Count < 1) return;

            int upper;
            int lower;

            drawPicture = new DrawPicture(pictureBox1.Height, pictureBox1.Width);

            switch (sOutypeValue[0])
            {
                default:
                case "UT420":
                    upper = 20;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = upper * 100;
                    drawPicture.LimitLowerLeftY = lower * 100;
                    break;

                case "UTP05":
                    upper = 5;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = upper * 200;
                    drawPicture.LimitLowerLeftY = lower * 200;
                    break;

                case "UTP10":
                    upper = 10;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = upper * 100;
                    drawPicture.LimitLowerLeftY = lower * 100;
                    break;

                case "UTN05":
                    upper = 5;
                    lower = -5;
                    drawPicture.LimitUpperLeftY = upper * 200;
                    drawPicture.LimitLowerLeftY = lower * 200;
                    break;

                case "UTN10":
                    upper = 10;
                    lower = -10;
                    drawPicture.LimitUpperLeftY = upper * 100;
                    drawPicture.LimitLowerLeftY = lower * 100;
                    break;

                case "UMASK":
                    upper = (int)(actXET.E_wt_full / Math.Pow(10, actXET.E_wt_decimal));
                    lower = 0;
                    drawPicture.LimitUpperLeftY = upper;
                    drawPicture.LimitLowerLeftY = lower;
                    break;
            }

            //选择的单位和设定的单位不一样，单位转换
            UNIT originalUnit = (UNIT)actXET.E_wt_unit;
            string UnitCategory = UnitHelper.GetUnitCategory(originalUnit);
            switch (UnitCategory)
            {
                case "重量":
                case "力":
                case "扭矩":
                case "压力":
                case "温度":
                    string originalUnitStr = actXET.GetUnitUMASK();
                    if (comboBoxUnit.SelectedItem != null)
                    {
                        if (originalUnitStr != comboBoxUnit.Text)
                        {
                            if (drawPicture.LimitUpperLeftY > 0)
                            {
                                drawPicture.LimitUpperLeftY = (int)double.Parse(UnitHelper.ConvertUnit(drawPicture.LimitUpperLeftY.ToString(), 0, originalUnit, comboBoxUnit.Text, false));
                                if (drawPicture.LimitUpperLeftY < 1) drawPicture.LimitUpperLeftY = 1;
                            }
                            if (drawPicture.LimitLowerLeftY > 0)
                            {
                                drawPicture.LimitLowerLeftY = (int)double.Parse(UnitHelper.ConvertUnit(drawPicture.LimitLowerLeftY.ToString(), 0, originalUnit, comboBoxUnit.Text, false));
                            }
                        }
                    }
                    break;
                case "无":
                case "其他":
                default:
                    break;
            }

            //画背景
            drawPicture.HorizontalAxisNum = 11;
            pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();
        }

        //初始化listView表格
        private void load_ListView()
        {
            //初始化combox2记录时间
            comboBox2.SelectedIndex = 0;

            //表头
            count = 1;
            dbListView1.Columns.Add("序号");
            dbListView1.Columns.Add("ID");
            dbListView1.Columns.Add("时间");
            dbListView1.Columns.Add("稳定状态");
            dbListView1.Columns.Add("数据");
            dbListView1.Columns.Add("单位");
            for (int i = 0; i < dbListView1.Columns.Count; i++)
            {
                dbListView1.Columns[i].Width = (int)(1.0 / dbListView1.Columns.Count * dbListView1.ClientRectangle.Width);
            }
        }

        //开始记录
        private void buttonX_Recoord_Click(object sender, EventArgs e)
        {
            if (buttonX_Recoord.Text == "开始记录")
            {
                //清空数据
                count = 1;
                isWrite = false;
                dbListView1.Items.Clear();
                //更新显示
                buttonX_Recoord.Text = "停止记录";
                label_Data.Text = "数据记录中";
                //记录中不允许更改时间
                comboBox2.Enabled = false;
                //记录结束时间
                stopTime = System.DateTime.Now.AddMinutes(Convert.ToInt32(comboBox2.Text));
                timer = new System.Timers.Timer();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                timer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                timer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
            else
            {
                buttonX_Recoord.Text = "开始记录";
                stopTime = DateTime.Now;
            }
        }

        //清空曲线
        private void buttonX_Clear_Click(object sender, EventArgs e)
        {
            clear_Curve();
        }

        //清空曲线
        private void clear_Curve()
        {
            //曲线清空
            if (drawPicture != null)
            {
                drawPicture.Data[0].Clear();
            }
        }

        //数据列表——保存实时数据
        private void save_Data(object sender, System.Timers.ElapsedEventArgs e)
        {
            int index;//实施记录数据下标

            //判断data目录是否存在
            if (!Directory.Exists(MyDevice.D_dataPath))
            {
                //不存在
                Directory.CreateDirectory(MyDevice.D_dataPath);
            }

            //文件路径
            string mePath = MyDevice.D_dataPath + @"\实时数据记录表_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
            FileInfo fileInfo = new FileInfo(mePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            //保存文件
            FileStream fs = new FileStream(mePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            //写入列名
            string data = "";
            for (int i = 0; i < dbListView1.Columns.Count; i++)
            {
                data += dbListView1.Columns[i].Text;
                if (i < dbListView1.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            //写入数据
            while (DateTime.Compare(DateTime.Now, stopTime) < 0)
            {
                if (isWrite)
                {
                    index = dbListView1.Items.Count - 1;
                    isWrite = false;
                    this.BeginInvoke(new System.Action(delegate
                    {
                        if (index > -1)
                        {
                            data = dbListView1.Items[index].SubItems[0].Text + "," + dbListView1.Items[index].SubItems[1].Text + "," +
                                   dbListView1.Items[index].SubItems[2].Text + "," + dbListView1.Items[index].SubItems[3].Text + "," +
                                   dbListView1.Items[index].SubItems[4].Text + "," + dbListView1.Items[index].SubItems[5].Text;

                            sw.WriteLine(data);
                        }
                    }));
                }
            }
            sw.Close();
            fs.Close();
            this.BeginInvoke(new System.Action(delegate
            {
                buttonX_Recoord.Text = "开始记录";
                label_Data.Text = "记录完成已自动保存！";
                comboBox2.Enabled = true;
            }));
        }

        #endregion

        #region 参数设置

        //初始化参数设置
        private void load_SetPara()
        {
            //可见性
            labelCal1.Visible = false;
            labelCal2.Visible = false;
            labelCal3.Visible = false;
            labelCal4.Visible = false;
            labelCal5.Visible = false;
            labelCal6.Visible = false;
            labelCal7.Visible = false;
            labelCal8.Visible = false;

            listBoxAutozero.Visible = false;
            listBoxAntivib.Visible = false;
            listBoxTkzerotime.Visible = false;
            listBoxTrackzero.Visible = false;
            listBoxTkdynatime.Visible = false;
            listBoxDynazero.Visible = false;
            listBoxFilterange.Visible = false;
            listBoxFiltertime.Visible = false;
            buttonX_Para.Visible = false;

            //权限
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            || ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli"))
            || ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            || ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456")))
            {
                labelCal1.Text = "上电归零范围";
                labelCal2.Text = "抗振动等级";
                labelCal3.Text = "零点跟踪时间";
                labelCal4.Text = "零点跟踪范围";
                labelCal5.Text = "蠕变跟踪时间";
                labelCal6.Text = "蠕变跟踪范围";
                labelCal7.Text = "判稳范围";
                labelCal8.Text = "判稳时间";

                listBoxAutozero.AdminMode = true;
                listBoxAntivib.AdminMode = true;
                listBoxTkzerotime.AdminMode = true;
                listBoxTrackzero.AdminMode = true;
                listBoxTkdynatime.AdminMode = true;
                listBoxDynazero.AdminMode = true;
                listBoxFilterange.AdminMode = true;
                listBoxFiltertime.AdminMode = true;
            }
            else
            {
                labelCal1.Text = "参数1";
                labelCal2.Text = "参数2";
                labelCal3.Text = "参数3";
                labelCal4.Text = "参数4";
                labelCal5.Text = "参数5";
                labelCal6.Text = "参数6";
                labelCal7.Text = "参数7";
                labelCal8.Text = "参数8";

                buttonX_Para.Enabled = false;
            }
        }

        //更新参数设置界面
        private void update_groupBoxPara()
        {
            buttonX_Para.HoverBackColor = Color.CadetBlue;
            buttonX_Para.Text = "确 定";

            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            //
            oldOutype = actXET.E_outype;
            oldCurve = actXET.E_curve;
            oldAdspeed = actXET.E_adspeed;
            oldDecimal = actXET.E_wt_decimal;

            //防错
            if (actXET.E_wt_division > 100) actXET.E_wt_division = 1;
            if (actXET.E_wt_division == 0) actXET.E_wt_division = 1;

            ////////////////////////////////////////////////////////////////////////////////

            //计算判稳范围
            updateFiltRange();

            //标定参数(上电归零范围)
            switch ((EATZ)actXET.E_autozero)
            {
                case EATZ.ATZ0: listBoxAutozero.SelectedIndex = 0; break;
                case EATZ.ATZ2: listBoxAutozero.SelectedIndex = 1; break;
                case EATZ.ATZ4: listBoxAutozero.SelectedIndex = 2; break;
                case EATZ.ATZ10: listBoxAutozero.SelectedIndex = 3; break;
                case EATZ.ATZ20: listBoxAutozero.SelectedIndex = 4; break;
                case EATZ.ATZ50: listBoxAutozero.SelectedIndex = 5; break;
                default: listBoxAutozero.SelectedIndex = 3; break;
            }

            //标定参数(抗振动等级)
            switch ((ELV)actXET.E_wt_antivib)
            {
                case ELV.LV0: listBoxAntivib.SelectedIndex = 0; break;
                case ELV.LV1: listBoxAntivib.SelectedIndex = 1; break;
                case ELV.LV2: listBoxAntivib.SelectedIndex = 2; break;
                case ELV.LV3: listBoxAntivib.SelectedIndex = 3; break;
                case ELV.LV4: listBoxAntivib.SelectedIndex = 4; break;
                case ELV.LV5: listBoxAntivib.SelectedIndex = 5; break;
                case ELV.LV6: listBoxAntivib.SelectedIndex = 6; break;
                case ELV.LV7: listBoxAntivib.SelectedIndex = 7; break;
                case ELV.LV8: listBoxAntivib.SelectedIndex = 8; break;
                case ELV.LV9: listBoxAntivib.SelectedIndex = 9; break;
                case ELV.LV10: listBoxAntivib.SelectedIndex = 10; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(滤波深度)
            switch ((ELV)actXET.E_wt_spfilt)
            {
                case ELV.LV0: listBoxFilterange.SelectedIndex = 0; break;
                case ELV.LV1: listBoxFilterange.SelectedIndex = 1; break;
                case ELV.LV2: listBoxFilterange.SelectedIndex = 2; break;
                case ELV.LV3: listBoxFilterange.SelectedIndex = 3; break;
                case ELV.LV4: listBoxFilterange.SelectedIndex = 4; break;
                case ELV.LV5: listBoxFilterange.SelectedIndex = 5; break;
                case ELV.LV6: listBoxFilterange.SelectedIndex = 6; break;
                case ELV.LV7: listBoxFilterange.SelectedIndex = 7; break;
                case ELV.LV8: listBoxFilterange.SelectedIndex = 8; break;
                case ELV.LV9: listBoxFilterange.SelectedIndex = 9; break;
                case ELV.LV10: listBoxFilterange.SelectedIndex = 10; break;
                default: listBoxFilterange.SelectedIndex = 0; break;
            }

            //标定参数(滤波时间)
            switch ((ELV)actXET.E_wt_sptime)
            {
                case ELV.LV0: listBoxFiltertime.SelectedIndex = 0; break;
                case ELV.LV1: listBoxFiltertime.SelectedIndex = 1; break;
                case ELV.LV2: listBoxFiltertime.SelectedIndex = 2; break;
                case ELV.LV3: listBoxFiltertime.SelectedIndex = 3; break;
                case ELV.LV4: listBoxFiltertime.SelectedIndex = 4; break;
                case ELV.LV5: listBoxFiltertime.SelectedIndex = 5; break;
                case ELV.LV6: listBoxFiltertime.SelectedIndex = 6; break;
                case ELV.LV7: listBoxFiltertime.SelectedIndex = 7; break;
                case ELV.LV8: listBoxFiltertime.SelectedIndex = 8; break;
                case ELV.LV9: listBoxFiltertime.SelectedIndex = 9; break;
                case ELV.LV10: listBoxFiltertime.SelectedIndex = 10; break;
                default: listBoxFiltertime.SelectedIndex = 0; break;
            }

            //标定参数(零点跟踪时间)
            switch ((TIM)actXET.E_tkzerotime)
            {
                case TIM.TIM0_1: listBoxTkzerotime.SelectedIndex = 0; break;
                case TIM.TIM0_2: listBoxTkzerotime.SelectedIndex = 1; break;
                case TIM.TIM0_3: listBoxTkzerotime.SelectedIndex = 2; break;
                case TIM.TIM0_4: listBoxTkzerotime.SelectedIndex = 3; break;
                case TIM.TIM0_5: listBoxTkzerotime.SelectedIndex = 4; break;
                case TIM.TIM0_6: listBoxTkzerotime.SelectedIndex = 5; break;
                case TIM.TIM0_7: listBoxTkzerotime.SelectedIndex = 6; break;
                case TIM.TIM0_8: listBoxTkzerotime.SelectedIndex = 7; break;
                case TIM.TIM0_9: listBoxTkzerotime.SelectedIndex = 8; break;
                case TIM.TIM1_0: listBoxTkzerotime.SelectedIndex = 9; break;
                case TIM.TIM1_1: listBoxTkzerotime.SelectedIndex = 10; break;
                case TIM.TIM1_2: listBoxTkzerotime.SelectedIndex = 11; break;
                case TIM.TIM1_3: listBoxTkzerotime.SelectedIndex = 12; break;
                case TIM.TIM1_4: listBoxTkzerotime.SelectedIndex = 13; break;
                case TIM.TIM1_5: listBoxTkzerotime.SelectedIndex = 14; break;
                case TIM.TIM1_6: listBoxTkzerotime.SelectedIndex = 15; break;
                case TIM.TIM1_7: listBoxTkzerotime.SelectedIndex = 16; break;
                case TIM.TIM1_8: listBoxTkzerotime.SelectedIndex = 17; break;
                case TIM.TIM1_9: listBoxTkzerotime.SelectedIndex = 18; break;
                case TIM.TIM2_0: listBoxTkzerotime.SelectedIndex = 19; break;
                case TIM.TIM2_1: listBoxTkzerotime.SelectedIndex = 20; break;
                case TIM.TIM2_2: listBoxTkzerotime.SelectedIndex = 21; break;
                case TIM.TIM2_3: listBoxTkzerotime.SelectedIndex = 22; break;
                case TIM.TIM2_4: listBoxTkzerotime.SelectedIndex = 23; break;
                case TIM.TIM2_5: listBoxTkzerotime.SelectedIndex = 24; break;
                case TIM.TIM2_6: listBoxTkzerotime.SelectedIndex = 25; break;
                case TIM.TIM2_7: listBoxTkzerotime.SelectedIndex = 26; break;
                case TIM.TIM2_8: listBoxTkzerotime.SelectedIndex = 27; break;
                case TIM.TIM2_9: listBoxTkzerotime.SelectedIndex = 28; break;
                case TIM.TIM3_0: listBoxTkzerotime.SelectedIndex = 29; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(零点跟踪范围)
            switch ((EATK)actXET.E_trackzero)
            {
                case EATK.TKZ00: listBoxTrackzero.SelectedIndex = 0; break;
                case EATK.TKZ5: listBoxTrackzero.SelectedIndex = 1; break;
                case EATK.TKZ10: listBoxTrackzero.SelectedIndex = 2; break;
                case EATK.TKZ20: listBoxTrackzero.SelectedIndex = 3; break;
                case EATK.TKZ30: listBoxTrackzero.SelectedIndex = 4; break;
                case EATK.TKZ40: listBoxTrackzero.SelectedIndex = 5; break;
                case EATK.TKZ50: listBoxTrackzero.SelectedIndex = 6; break;
                case EATK.TKZ60: listBoxTrackzero.SelectedIndex = 7; break;
                case EATK.TKZ70: listBoxTrackzero.SelectedIndex = 8; break;
                case EATK.TKZ80: listBoxTrackzero.SelectedIndex = 9; break;
                case EATK.TKZ90: listBoxTrackzero.SelectedIndex = 10; break;
                case EATK.TKZ100: listBoxTrackzero.SelectedIndex = 11; break;
                case EATK.TKZ200: listBoxTrackzero.SelectedIndex = 12; break;
                case EATK.TKZ300: listBoxTrackzero.SelectedIndex = 13; break;
                case EATK.TKZ400: listBoxTrackzero.SelectedIndex = 14; break;
                case EATK.TKZ500: listBoxTrackzero.SelectedIndex = 15; break;
                default: listBoxTrackzero.SelectedIndex = 1; break;
            }

            //标定参数(蠕变跟踪时间)
            switch ((TIM)actXET.E_tkdynatime)
            {
                case TIM.TIM0_1: listBoxTkdynatime.SelectedIndex = 0; break;
                case TIM.TIM0_2: listBoxTkdynatime.SelectedIndex = 1; break;
                case TIM.TIM0_3: listBoxTkdynatime.SelectedIndex = 2; break;
                case TIM.TIM0_4: listBoxTkdynatime.SelectedIndex = 3; break;
                case TIM.TIM0_5: listBoxTkdynatime.SelectedIndex = 4; break;
                case TIM.TIM0_6: listBoxTkdynatime.SelectedIndex = 5; break;
                case TIM.TIM0_7: listBoxTkdynatime.SelectedIndex = 6; break;
                case TIM.TIM0_8: listBoxTkdynatime.SelectedIndex = 7; break;
                case TIM.TIM0_9: listBoxTkdynatime.SelectedIndex = 8; break;
                case TIM.TIM1_0: listBoxTkdynatime.SelectedIndex = 9; break;
                case TIM.TIM1_1: listBoxTkdynatime.SelectedIndex = 10; break;
                case TIM.TIM1_2: listBoxTkdynatime.SelectedIndex = 11; break;
                case TIM.TIM1_3: listBoxTkdynatime.SelectedIndex = 12; break;
                case TIM.TIM1_4: listBoxTkdynatime.SelectedIndex = 13; break;
                case TIM.TIM1_5: listBoxTkdynatime.SelectedIndex = 14; break;
                case TIM.TIM1_6: listBoxTkdynatime.SelectedIndex = 15; break;
                case TIM.TIM1_7: listBoxTkdynatime.SelectedIndex = 16; break;
                case TIM.TIM1_8: listBoxTkdynatime.SelectedIndex = 17; break;
                case TIM.TIM1_9: listBoxTkdynatime.SelectedIndex = 18; break;
                case TIM.TIM2_0: listBoxTkdynatime.SelectedIndex = 19; break;
                case TIM.TIM2_1: listBoxTkdynatime.SelectedIndex = 20; break;
                case TIM.TIM2_2: listBoxTkdynatime.SelectedIndex = 21; break;
                case TIM.TIM2_3: listBoxTkdynatime.SelectedIndex = 22; break;
                case TIM.TIM2_4: listBoxTkdynatime.SelectedIndex = 23; break;
                case TIM.TIM2_5: listBoxTkdynatime.SelectedIndex = 24; break;
                case TIM.TIM2_6: listBoxTkdynatime.SelectedIndex = 25; break;
                case TIM.TIM2_7: listBoxTkdynatime.SelectedIndex = 26; break;
                case TIM.TIM2_8: listBoxTkdynatime.SelectedIndex = 27; break;
                case TIM.TIM2_9: listBoxTkdynatime.SelectedIndex = 28; break;
                case TIM.TIM3_0: listBoxTkdynatime.SelectedIndex = 29; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(蠕变跟踪范围)
            switch ((EATK)actXET.E_dynazero)
            {
                case EATK.TKZ00: listBoxDynazero.SelectedIndex = 0; break;
                case EATK.TKZ5: listBoxDynazero.SelectedIndex = 1; break;
                case EATK.TKZ10: listBoxDynazero.SelectedIndex = 2; break;
                case EATK.TKZ20: listBoxDynazero.SelectedIndex = 3; break;
                case EATK.TKZ30: listBoxDynazero.SelectedIndex = 4; break;
                case EATK.TKZ40: listBoxDynazero.SelectedIndex = 5; break;
                case EATK.TKZ50: listBoxDynazero.SelectedIndex = 6; break;
                case EATK.TKZ60: listBoxDynazero.SelectedIndex = 7; break;
                case EATK.TKZ70: listBoxDynazero.SelectedIndex = 8; break;
                case EATK.TKZ80: listBoxDynazero.SelectedIndex = 9; break;
                case EATK.TKZ90: listBoxDynazero.SelectedIndex = 10; break;
                case EATK.TKZ100: listBoxDynazero.SelectedIndex = 11; break;
                case EATK.TKZ200: listBoxDynazero.SelectedIndex = 12; break;
                case EATK.TKZ300: listBoxDynazero.SelectedIndex = 13; break;
                case EATK.TKZ400: listBoxDynazero.SelectedIndex = 14; break;
                case EATK.TKZ500: listBoxDynazero.SelectedIndex = 15; break;
                default: listBoxDynazero.SelectedIndex = 0; break;
            }

            ////////////////////////////////////////////////////////////////////////////////

            //控件使能
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            || ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli"))
            || ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            || ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456")))
            {
                switch (actXET.S_DeviceType)
                {
                    case TYPE.TDES:
                    case TYPE.TDSS:
                    case TYPE.T4X600H:
                        listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        break;

                    case TYPE.TD485:
                        listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxDynazero.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFilterange.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFiltertime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkzerotime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkdynatime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        break;
                    default:
                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        listBoxAntivib.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxDynazero.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFilterange.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxFiltertime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkzerotime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        listBoxTkdynatime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                        break;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////

            //控件可见
            switch (actXET.S_DeviceType)
            {
                case TYPE.TDES:
                case TYPE.TDSS:
                case TYPE.T4X600H:
                    //可见性
                    labelCal1.Visible = true;
                    labelCal2.Visible = false;
                    labelCal3.Visible = true;
                    labelCal4.Visible = true;
                    labelCal5.Visible = true;
                    labelCal6.Visible = true;
                    labelCal7.Visible = false;
                    labelCal8.Visible = false;

                    listBoxAutozero.Visible = true;
                    listBoxAntivib.Visible = false;
                    listBoxTkzerotime.Visible = true;
                    listBoxTrackzero.Visible = true;
                    listBoxTkdynatime.Visible = true;
                    listBoxDynazero.Visible = true;
                    listBoxFilterange.Visible = false;
                    listBoxFiltertime.Visible = false;
                    buttonX_Para.Visible = true;
                    break;
                default:
                case TYPE.TD485:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    //可见性
                    labelCal1.Visible = true;
                    labelCal2.Visible = true;
                    labelCal3.Visible = true;
                    labelCal4.Visible = true;
                    labelCal5.Visible = true;
                    labelCal6.Visible = true;
                    labelCal7.Visible = true;
                    labelCal8.Visible = true;

                    listBoxAutozero.Visible = true;
                    listBoxAntivib.Visible = true;
                    listBoxTkzerotime.Visible = true;
                    listBoxTrackzero.Visible = true;
                    listBoxTkdynatime.Visible = true;
                    listBoxDynazero.Visible = true;
                    listBoxFilterange.Visible = true;
                    listBoxFiltertime.Visible = true;
                    buttonX_Para.Visible = true;
                    break;
            }
        }

        //计算判稳范围,判稳时间
        private void updateFiltRange()
        {
            float ad_span = 0;

            //计算ad_span
            if ((ECVE)actXET.E_curve <= ECVE.CINTER)
            {
                ad_span = actXET.E_ad_point5 - actXET.E_ad_point1;
            }
            else
            {
                ad_span = actXET.E_ad_point11 - actXET.E_ad_point1;
            }

            //计算da_span,wt_span
            float da_span = actXET.E_da_full - actXET.E_da_zero;
            float wt_span = actXET.E_wt_full - actXET.E_wt_zero;

            //计算1个e对应的adcout值
            float e_data = ad_span * wt_span / da_span;
            e_data = e_data * actXET.E_wt_division / wt_span;
            if (e_data < 0)
            {
                e_data = -e_data;
            }

            //将滤波范围换算为n个e
            listBoxFilterange.Items.Clear();
            for (int i = 1; i <= 11; i++)
            {
                listBoxFilterange.Items.Add($"LV{i - 1}({(int)(actXET.E_filter * i / e_data)} e)");
            }

            //判稳时间
            switch ((ESPD)(actXET.E_adspeed & 0xF0))
            {
                case ESPD.CSF10:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("208 ms");
                    listBoxFiltertime.Items.Add("416 ms");
                    listBoxFiltertime.Items.Add("624 ms");
                    listBoxFiltertime.Items.Add("832 ms");
                    listBoxFiltertime.Items.Add("1040 ms");
                    listBoxFiltertime.Items.Add("1248 ms");
                    listBoxFiltertime.Items.Add("1456 ms");
                    listBoxFiltertime.Items.Add("1664 ms");
                    listBoxFiltertime.Items.Add("1872 ms");
                    listBoxFiltertime.Items.Add("2080 ms");
                    listBoxFiltertime.Items.Add("2288 ms");
                    break;
                case ESPD.CSF40:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
                case ESPD.CSF640:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("30 ms");
                    listBoxFiltertime.Items.Add("40 ms");
                    listBoxFiltertime.Items.Add("50 ms");
                    listBoxFiltertime.Items.Add("60 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("80 ms");
                    listBoxFiltertime.Items.Add("90 ms");
                    listBoxFiltertime.Items.Add("100 ms");
                    listBoxFiltertime.Items.Add("110 ms");
                    listBoxFiltertime.Items.Add("120 ms");
                    listBoxFiltertime.Items.Add("130 ms");
                    break;
                case ESPD.CSF1280:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("28 ms");
                    listBoxFiltertime.Items.Add("35 ms");
                    listBoxFiltertime.Items.Add("42 ms");
                    listBoxFiltertime.Items.Add("49 ms");
                    listBoxFiltertime.Items.Add("56 ms");
                    listBoxFiltertime.Items.Add("63 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("77 ms");
                    listBoxFiltertime.Items.Add("84 ms");
                    listBoxFiltertime.Items.Add("91 ms");
                    listBoxFiltertime.Items.Add("98 ms");
                    break;
                default:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
            }
        }

        //确定按键
        private void buttonX_Para_Click(object sender, EventArgs e)
        {
            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            //标定参数(上电归零范围)
            switch (listBoxAutozero.SelectedIndex)
            {
                case 0: actXET.E_autozero = (byte)EATZ.ATZ0; break;
                case 1: actXET.E_autozero = (byte)EATZ.ATZ2; break;
                case 2: actXET.E_autozero = (byte)EATZ.ATZ4; break;
                case 3: actXET.E_autozero = (byte)EATZ.ATZ10; break;
                case 4: actXET.E_autozero = (byte)EATZ.ATZ20; break;
                case 5: actXET.E_autozero = (byte)EATZ.ATZ50; break;
                default: actXET.E_autozero = (byte)EATZ.ATZ10; break;
            }

            //标定参数(抗振动等级)
            switch (listBoxAntivib.SelectedIndex)
            {
                case 0: actXET.E_wt_antivib = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_antivib = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_antivib = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_antivib = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_antivib = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_antivib = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_antivib = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_antivib = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_antivib = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_antivib = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_antivib = (byte)ELV.LV10; break;
                default: actXET.E_wt_antivib = (byte)ELV.LV0; break;
            }

            //标定参数(滤波深度)
            switch (listBoxFilterange.SelectedIndex)
            {
                case 0: actXET.E_wt_spfilt = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_spfilt = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_spfilt = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_spfilt = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_spfilt = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_spfilt = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_spfilt = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_spfilt = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_spfilt = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_spfilt = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_spfilt = (byte)ELV.LV10; break;
                default: actXET.E_wt_spfilt = (byte)ELV.LV0; break;
            }

            //标定参数(滤波时间)
            switch (listBoxFiltertime.SelectedIndex)
            {
                case 0: actXET.E_wt_sptime = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_sptime = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_sptime = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_sptime = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_sptime = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_sptime = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_sptime = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_sptime = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_sptime = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_sptime = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_sptime = (byte)ELV.LV10; break;
                default: actXET.E_wt_sptime = (byte)ELV.LV0; break;
            }

            //标定参数(零点跟踪时间)
            switch (listBoxTkzerotime.SelectedIndex)
            {
                case 0: actXET.E_tkzerotime = (byte)TIM.TIM0_1; break;
                case 1: actXET.E_tkzerotime = (byte)TIM.TIM0_2; break;
                case 2: actXET.E_tkzerotime = (byte)TIM.TIM0_3; break;
                case 3: actXET.E_tkzerotime = (byte)TIM.TIM0_4; break;
                case 4: actXET.E_tkzerotime = (byte)TIM.TIM0_5; break;
                case 5: actXET.E_tkzerotime = (byte)TIM.TIM0_6; break;
                case 6: actXET.E_tkzerotime = (byte)TIM.TIM0_7; break;
                case 7: actXET.E_tkzerotime = (byte)TIM.TIM0_8; break;
                case 8: actXET.E_tkzerotime = (byte)TIM.TIM0_9; break;
                case 9: actXET.E_tkzerotime = (byte)TIM.TIM1_0; break;
                case 10: actXET.E_tkzerotime = (byte)TIM.TIM1_1; break;
                case 11: actXET.E_tkzerotime = (byte)TIM.TIM1_2; break;
                case 12: actXET.E_tkzerotime = (byte)TIM.TIM1_3; break;
                case 13: actXET.E_tkzerotime = (byte)TIM.TIM1_4; break;
                case 14: actXET.E_tkzerotime = (byte)TIM.TIM1_5; break;
                case 15: actXET.E_tkzerotime = (byte)TIM.TIM1_6; break;
                case 16: actXET.E_tkzerotime = (byte)TIM.TIM1_7; break;
                case 17: actXET.E_tkzerotime = (byte)TIM.TIM1_8; break;
                case 18: actXET.E_tkzerotime = (byte)TIM.TIM1_9; break;
                case 19: actXET.E_tkzerotime = (byte)TIM.TIM2_0; break;
                case 20: actXET.E_tkzerotime = (byte)TIM.TIM2_1; break;
                case 21: actXET.E_tkzerotime = (byte)TIM.TIM2_2; break;
                case 22: actXET.E_tkzerotime = (byte)TIM.TIM2_3; break;
                case 23: actXET.E_tkzerotime = (byte)TIM.TIM2_4; break;
                case 24: actXET.E_tkzerotime = (byte)TIM.TIM2_5; break;
                case 25: actXET.E_tkzerotime = (byte)TIM.TIM2_6; break;
                case 26: actXET.E_tkzerotime = (byte)TIM.TIM2_7; break;
                case 27: actXET.E_tkzerotime = (byte)TIM.TIM2_8; break;
                case 28: actXET.E_tkzerotime = (byte)TIM.TIM2_9; break;
                case 29: actXET.E_tkzerotime = (byte)TIM.TIM3_0; break;
                default: actXET.E_tkzerotime = (byte)TIM.TIM0_1; break;
            }

            //标定参数(零点跟踪范围)
            switch (listBoxTrackzero.SelectedIndex)
            {
                case 0: actXET.E_trackzero = (byte)EATK.TKZ00; break;
                case 1: actXET.E_trackzero = (byte)EATK.TKZ5; break;
                case 2: actXET.E_trackzero = (byte)EATK.TKZ10; break;
                case 3: actXET.E_trackzero = (byte)EATK.TKZ20; break;
                case 4: actXET.E_trackzero = (byte)EATK.TKZ30; break;
                case 5: actXET.E_trackzero = (byte)EATK.TKZ40; break;
                case 6: actXET.E_trackzero = (byte)EATK.TKZ50; break;
                case 7: actXET.E_trackzero = (byte)EATK.TKZ60; break;
                case 8: actXET.E_trackzero = (byte)EATK.TKZ70; break;
                case 9: actXET.E_trackzero = (byte)EATK.TKZ80; break;
                case 10: actXET.E_trackzero = (byte)EATK.TKZ90; break;
                case 11: actXET.E_trackzero = (byte)EATK.TKZ100; break;
                case 12: actXET.E_trackzero = (byte)EATK.TKZ200; break;
                case 13: actXET.E_trackzero = (byte)EATK.TKZ300; break;
                case 14: actXET.E_trackzero = (byte)EATK.TKZ400; break;
                case 15: actXET.E_trackzero = (byte)EATK.TKZ500; break;
                default: actXET.E_trackzero = (byte)EATK.TKZ5; break;
            }

            //标定参数(蠕变跟踪时间)
            switch (listBoxTkdynatime.SelectedIndex)
            {
                case 0: actXET.E_tkdynatime = (byte)TIM.TIM0_1; break;
                case 1: actXET.E_tkdynatime = (byte)TIM.TIM0_2; break;
                case 2: actXET.E_tkdynatime = (byte)TIM.TIM0_3; break;
                case 3: actXET.E_tkdynatime = (byte)TIM.TIM0_4; break;
                case 4: actXET.E_tkdynatime = (byte)TIM.TIM0_5; break;
                case 5: actXET.E_tkdynatime = (byte)TIM.TIM0_6; break;
                case 6: actXET.E_tkdynatime = (byte)TIM.TIM0_7; break;
                case 7: actXET.E_tkdynatime = (byte)TIM.TIM0_8; break;
                case 8: actXET.E_tkdynatime = (byte)TIM.TIM0_9; break;
                case 9: actXET.E_tkdynatime = (byte)TIM.TIM1_0; break;
                case 10: actXET.E_tkdynatime = (byte)TIM.TIM1_1; break;
                case 11: actXET.E_tkdynatime = (byte)TIM.TIM1_2; break;
                case 12: actXET.E_tkdynatime = (byte)TIM.TIM1_3; break;
                case 13: actXET.E_tkdynatime = (byte)TIM.TIM1_4; break;
                case 14: actXET.E_tkdynatime = (byte)TIM.TIM1_5; break;
                case 15: actXET.E_tkdynatime = (byte)TIM.TIM1_6; break;
                case 16: actXET.E_tkdynatime = (byte)TIM.TIM1_7; break;
                case 17: actXET.E_tkdynatime = (byte)TIM.TIM1_8; break;
                case 18: actXET.E_tkdynatime = (byte)TIM.TIM1_9; break;
                case 19: actXET.E_tkdynatime = (byte)TIM.TIM2_0; break;
                case 20: actXET.E_tkdynatime = (byte)TIM.TIM2_1; break;
                case 21: actXET.E_tkdynatime = (byte)TIM.TIM2_2; break;
                case 22: actXET.E_tkdynatime = (byte)TIM.TIM2_3; break;
                case 23: actXET.E_tkdynatime = (byte)TIM.TIM2_4; break;
                case 24: actXET.E_tkdynatime = (byte)TIM.TIM2_5; break;
                case 25: actXET.E_tkdynatime = (byte)TIM.TIM2_6; break;
                case 26: actXET.E_tkdynatime = (byte)TIM.TIM2_7; break;
                case 27: actXET.E_tkdynatime = (byte)TIM.TIM2_8; break;
                case 28: actXET.E_tkdynatime = (byte)TIM.TIM2_9; break;
                case 29: actXET.E_tkdynatime = (byte)TIM.TIM3_0; break;
                default: actXET.E_tkdynatime = (byte)TIM.TIM0_1; break;
            }

            //标定参数(蠕变跟踪)
            switch (listBoxDynazero.SelectedIndex)
            {
                case 0: actXET.E_dynazero = (byte)EATK.TKZ00; break;
                case 1: actXET.E_dynazero = (byte)EATK.TKZ5; break;
                case 2: actXET.E_dynazero = (byte)EATK.TKZ10; break;
                case 3: actXET.E_dynazero = (byte)EATK.TKZ20; break;
                case 4: actXET.E_dynazero = (byte)EATK.TKZ30; break;
                case 5: actXET.E_dynazero = (byte)EATK.TKZ40; break;
                case 6: actXET.E_dynazero = (byte)EATK.TKZ50; break;
                case 7: actXET.E_dynazero = (byte)EATK.TKZ60; break;
                case 8: actXET.E_dynazero = (byte)EATK.TKZ70; break;
                case 9: actXET.E_dynazero = (byte)EATK.TKZ80; break;
                case 10: actXET.E_dynazero = (byte)EATK.TKZ90; break;
                case 11: actXET.E_dynazero = (byte)EATK.TKZ100; break;
                case 12: actXET.E_dynazero = (byte)EATK.TKZ200; break;
                case 13: actXET.E_dynazero = (byte)EATK.TKZ300; break;
                case 14: actXET.E_dynazero = (byte)EATK.TKZ400; break;
                case 15: actXET.E_dynazero = (byte)EATK.TKZ500; break;
                default: actXET.E_dynazero = (byte)EATK.TKZ5; break;
            }

            ////////////////////////////////////////////////////////////////////////////////

            //影响模拟量DAC配置,影响da_point,影响斜率计算
            if (oldOutype != actXET.E_outype)
            {
                actXET.RefreshOutypeChange(actXET.E_outype, oldOutype);

                actXET.RefreshRatio();
            }

            //影响斜率计算
            if (oldCurve != actXET.E_curve)
            {
                actXET.RefreshRatio();
            }

            //影响CS1237初始化,影响灵敏度,影响ad_point和da_point,影响斜率计算
            if (oldAdspeed != actXET.E_adspeed)
            {
                actXET.RefreshAdspeedChange();

                actXET.RefreshRatio();
            }

            //影响da_point,影响斜率计算
            if (oldDecimal != actXET.E_wt_decimal)
            {
                //小数点更新后重新计算da_point
                if (actXET.S_OutType == OUT.UMASK)
                {
                    actXET.T_analog1 = actXET.T_analog1;
                    actXET.T_analog2 = actXET.T_analog2;
                    actXET.T_analog3 = actXET.T_analog3;
                    actXET.T_analog4 = actXET.T_analog4;
                    actXET.T_analog5 = actXET.T_analog5;
                    actXET.T_analog6 = actXET.T_analog6;
                    actXET.T_analog7 = actXET.T_analog7;
                    actXET.T_analog8 = actXET.T_analog8;
                    actXET.T_analog9 = actXET.T_analog9;
                    actXET.T_analog10 = actXET.T_analog10;
                    actXET.T_analog11 = actXET.T_analog11;
                    actXET.RefreshRatio();
                }
            }

            //写入
            if (MyDevice.protocol.IsOpen)
            {
                activeButton = "buttonX_Para";
                buttonX_Para.HoverBackColor = Color.Firebrick;
                nextTask = TASKS.WRX0;
            }
        }

        #endregion

        #region 标定

        //初始化标定
        private void load_SetCal()
        {
            //权限
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
             || ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli"))
             || ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
             || ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456")))
            {
                bt_Zero.Enabled = true;
                bt_Max.Enabled = true;
                bt_Write.Enabled = true;
            }
            else
            {
                bt_Zero.Enabled = false;
                bt_Max.Enabled = false;
                bt_Write.Enabled = false;
            }
        }

        //更新标定界面
        private void update_groupBoxCal()
        {
            bt_Zero.HoverBackColor = Color.CadetBlue;
            bt_Max.HoverBackColor = Color.CadetBlue;
            bt_Write.HoverBackColor = Color.CadetBlue;
            bt_Write.Text = "写 入";

            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            //不为两点标定
            if ((ECVE)actXET.E_curve != ECVE.CTWOPT)
            {
                bt_Zero.Enabled = false;
                bt_Max.Enabled = false;
                bt_Write.Enabled = false;
                return;
            }

            //单位
            UNIT originalUnit = (UNIT)actXET.E_wt_unit;

            //输出
            textBox_Zero.Text = UnitHelper.ConvertUnit(actXET.T_analog1, actXET.E_wt_decimal, originalUnit, originalUnit);
            textBox_Max.Text = UnitHelper.ConvertUnit(actXET.T_analog5, actXET.E_wt_decimal, originalUnit, originalUnit);

            //mA,V,kg
            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    labeloutunit.Text = "范围: 4-20mA,±10V";
                    break;

                case OUT.UTP05:
                    labeloutunit.Text = "范围: 0-5V";
                    break;

                case OUT.UTP10:
                    labeloutunit.Text = "范围: 0-10";
                    break;

                case OUT.UTN05:
                    labeloutunit.Text = "范围: ±5V";
                    break;

                case OUT.UTN10:
                    labeloutunit.Text = "范围: ±10V";
                    break;

                case OUT.UMASK:
                    labeloutunit.Text = $"重量单位: {actXET.S_unit}";
                    break;

                default:
                    labeloutunit.Text = "";
                    break;
            }
        }

        //零点采集
        private void bt_Zero_Click(object sender, EventArgs e)
        {
            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            bt_Zero.HoverBackColor = Color.Firebrick;
            Refresh();
            nextTask = TASKS.ADCP1; ;
        }

        //满点采集
        private void bt_Max_Click(object sender, EventArgs e)
        {
            MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
            actXET = MyDevice.actDev;

            bt_Max.HoverBackColor = Color.Firebrick;
            Refresh();
            nextTask = TASKS.ADCP5;
        }

        //写入
        private void bt_Write_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                MyDevice.protocol.addr = byte.Parse(comboBoxAddr.Text);
                actXET = MyDevice.actDev;

                activeButton = "bt_Write";
                bt_Write.Text = "BCC";
                bt_Write.HoverBackColor = Color.Firebrick;
                nextTask = TASKS.BCC;
            }
        }

        #endregion

        #region 分配站点

        //初始化分配站点
        private void load_AllocateAddr()
        {
            //权限
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
             || ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli"))
             || ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
             || ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456")))
            {
                textBox_Addr.Enabled = true;
                textBox_Weight.Enabled = true;
                buttonX_Addr.Enabled = true;
            }
            else
            {
                textBox_Addr.Enabled = false;
                textBox_Weight.Enabled = false;
                buttonX_Addr.Enabled = false;
            }
        }

        //限制输入
        private void textBox_Weight_KeyPress(object sender, KeyPressEventArgs e)
        {
            BoxRestrict.KeyPress_RationalNumber(sender, e);
        }

        //分配站点
        private void buttonX_Addr_Click(object sender, EventArgs e)
        {
            if (!byte.TryParse(textBox_Addr.Text, out newAddr))
            {
                MessageBox.Show("输入的站点不正确");
                return;
            }
            if (mutiAddres.Contains(newAddr))
            {
                MessageBox.Show("该站点已被占用");
                return;
            }

            int acceptableDevSum = 0;   //符合重量条件的设备数量

            //获取符合要求的设备数量
            if (textBox_Weight.Text == "")
            {
                //有重量设置地址
                float devMaxFloat = float.Parse(textBox_Max.Text);
                foreach (MutiDevice485 dev in mutiDevices)
                {
                    if (float.TryParse(dev.Data, out float f))
                    {
                        if (Math.Abs(f) > Math.Abs(devMaxFloat * 0.01))
                        {
                            acceptableDevSum++;
                            oldAddr = byte.Parse(dev.Address);
                        }
                    }
                }
            }
            else
            {
                //检索重量设置地址
                float devWeightFloat = float.Parse(textBox_Weight.Text.Replace(".", ""));
                foreach (MutiDevice485 dev in mutiDevices)
                {
                    if (float.TryParse(dev.Data.Replace(".", ""), out float f))
                    {
                        if (f > devWeightFloat * 0.99 && f < devWeightFloat * 1.01)
                        {
                            acceptableDevSum++;
                            oldAddr = byte.Parse(dev.Address);
                        }
                    }
                }
            }

            if (acceptableDevSum > 1)
            {
                MessageBox.Show("有多台符合输入重量的设备");
                return;
            }
            else if (acceptableDevSum == 0)
            {
                MessageBox.Show("没有找到符合重量要求的设备");
                return;
            }

            //分配站点
            buttonX_Addr.HoverBackColor = Color.Firebrick;
            if (textBox_Weight.Text == "")
            {
                nextTask = TASKS.HWADDR;
            }
            else
            {
                devWeight = uint.Parse(textBox_Weight.Text.Replace(".", ""));

                buttonX_Addr.HoverBackColor = Color.Firebrick;
                nextTask = TASKS.SWADDR;
            }
        }

        #endregion

        #region RTU设置

        //双击label，且从未连接过，打开设置界面
        private void label17_DoubleClick(object sender, EventArgs e)
        {
            //连接过设备，可以关闭不能再次打开
            if (MyDevice.devSum > 0)
            {
                if (groupBoxConfig.Visible)
                {
                    groupBoxConfig.Visible = false;
                }
                return;
            }

            groupBoxConfig.Visible = !groupBoxConfig.Visible;

            label7_RTUConfig.Text = "";
        }

        //保存参数配置
        private void buttonX_Config_Click(object sender, EventArgs e)
        {
            if (byte.TryParse(textBox1.Text, out byte number) && number > 0)
            {
                //保存配置
                save_RTUConfig(number);

                //改注册表实现开机启动
                if (comboBox3_AutoStart.SelectedIndex == 1)
                {
                    AutoStart.AutoStartByRK("RTUAutoStart", true);
                }
                else
                {
                    AutoStart.AutoStartByRK("RTUAutoStart", false);
                }
            }
            else
            {
                // textBox1.Text 不是一个在1-255的整数
            }
        }

        //从RTUConfig.ini读取配置
        private void load_RTUConfig()
        {
            try
            {
                //读取文件
                if (File.Exists(MyDevice.D_datPath + "\\" + "RTUConfig.ini"))
                {
                    String[] meLines = File.ReadAllLines(MyDevice.D_datPath + "\\" + "RTUConfig.ini");
                    foreach (String line in meLines)
                    {
                        switch (line.Substring(0, line.IndexOf('=')))
                        {
                            case "AutoStart": comboBox3_AutoStart.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                            case "FullScreen": comboBox1_FullScreen.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                            case "AutoConnect": comboBox1_AutoConnect.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                        }
                    }
                    //只有当自动连接值为1时，才会读取通讯参数
                    if (comboBox1_AutoConnect.SelectedIndex == 1)
                    {
                        foreach (String line in meLines)
                        {
                            switch (line.Substring(0, line.IndexOf('=')))
                            {
                                case "PortName":
                                    string lastConnectCOM;
                                    lastConnectCOM = line.Split('=')[1];
                                    int index = comboBox1_port.Items.IndexOf(lastConnectCOM);
                                    if (index != -1)
                                    {
                                        comboBox1_port.SelectedIndex = index;
                                    }
                                    break;
                                case "BaudRate": comboBox2_BaudRate.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                                case "StopBits": comboBox3_StopBits.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                                case "Parity": comboBox4_Parity.SelectedIndex = Convert.ToInt32(line.Split('=')[1]); break;
                                case "Addr": textBox1.Text = Convert.ToByte(line.Split('=')[1]).ToString(); break;
                            }
                        }
                    }
                }
                else
                {
                    comboBox3_AutoStart.SelectedIndex = 0;
                    comboBox1_FullScreen.SelectedIndex = 0;
                    comboBox1_AutoConnect.SelectedIndex = 0;
                }
            }
            catch
            {
                comboBox3_AutoStart.SelectedIndex = 0;
                comboBox1_FullScreen.SelectedIndex = 0;
                comboBox1_AutoConnect.SelectedIndex = 0;
            }
        }

        //保存配置到RTUConfig.ini
        //参数设置点确定时保存,此时addr保存textbox里的值
        //设备最近一次连接完成时保存，此时addr保存MyDevice.myRS485.addr的值
        private void save_RTUConfig(byte addr = 1)
        {
            FileStream meFS = new FileStream(MyDevice.D_datPath + "\\" + "RTUConfig.ini", FileMode.OpenOrCreate, FileAccess.Write);
            TextWriter meWrite = new StreamWriter(meFS);
            meWrite.WriteLine("AutoStart=" + comboBox3_AutoStart.SelectedIndex);
            meWrite.WriteLine("FullScreen=" + comboBox1_FullScreen.SelectedIndex);
            meWrite.WriteLine("AutoConnect=" + comboBox1_AutoConnect.SelectedIndex);
            meWrite.WriteLine("PortName=" + comboBox1_port.Text);
            meWrite.WriteLine("BaudRate=" + comboBox2_BaudRate.SelectedIndex);
            meWrite.WriteLine("StopBits=" + comboBox3_StopBits.SelectedIndex);
            meWrite.WriteLine("Parity=" + comboBox4_Parity.SelectedIndex);
            meWrite.WriteLine("Addr=" + addr);
            meWrite.Close();
            meFS.Close();

            label7_RTUConfig.Text = "保存成功";
        }

        //全屏
        private void update_FullScreen()
        {
            if (comboBox1_FullScreen.SelectedIndex == 1)
            {
                //this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        //自动连接
        private void auto_Connect()
        {
            if (comboBox1_AutoConnect.SelectedIndex == 1)
            {
                bt_send3_Click(null, null);
            }
        }

        //读取窗口标题Text
        private void load_FormText()
        {
            try
            {
                //读取文件
                string logoIconPath = Path.Combine(Application.StartupPath, "pic", "FormText.ini");
                if (File.Exists(logoIconPath))
                {
                    String[] meLines = File.ReadAllLines(logoIconPath);
                    if (meLines.Length > 0)
                    {
                        this.Text = meLines[2].Split('=')[1];
                    }
                    else
                    {
                        this.Text = "RTU";
                    }
                }
                else
                {
                    this.Text = "RTU";
                }
            }
            catch
            {
                this.Text = "RTU";
            }
        }
        #endregion
    }
}