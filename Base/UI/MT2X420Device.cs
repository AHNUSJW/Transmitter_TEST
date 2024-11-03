using Model;
using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

//Tong Ziyun 20230608

namespace Base.UI
{
    public partial class MT2X420Device : Form
    {
        private XET actXET1;        //需要操作的设备
        private XET actXET2;        //需要操作的设备

        private volatile TASKS nextTask1;    //按键指令,TASKS.ZERO,TASKS.TARE,TASKS.BOR6
        private volatile TASKS nextTask2;    //按键指令,TASKS.ZERO,TASKS.TARE,TASKS.BOR

        private volatile int comTicker1;     //串口一发送指令计时器
        private volatile int comTicker2;     //串口二发送指令计时器

        private Int32 leftFull;     //串口一Full校准
        private Int32 leftZero;     //串口一Zero校准
        private Int32 rightFull;    //串口二Full校准
        private Int32 rightZero;    //串口二Zero校准

        private Color[] status1 = new Color[2] { Color.Red, Color.Green };      //指示灯状态点击按钮控件后通讯灯变为红绿闪状态
        private Color[] status2 = new Color[2] { Color.Green, Color.Green };    //指示灯状态在连接成功后变为常绿状态，在读DAC时也为常绿状态
        private Color[] status3 = new Color[2] { Color.Gray, Color.Gray };      //指示灯在未连接的状态下为长灰状态

        public MT2X420Device()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 界面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MT2X420Device_Load(object sender, EventArgs e)
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

            //加载界面文本
            load_FormText();

            //初始化设备连接接口
            MyDevice.protocol = MyDevice.mySelfUART;
            MyDevice.protocos = MyDevice.mySecondUART;

            //初始化设备数据
            actXET1 = MyDevice.mSUT;
            actXET2 = MyDevice.mSXT;

            //刷新串口
            bt_refresh_Click(sender, e);
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MT2X420Device_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart1);
            MyDevice.mySecondeUpdate -= new freshHandler(update_FromUart2);

            MyDevice.mePort_StopDacout(MyDevice.protocol);
            MyDevice.mePort_StopDacout(MyDevice.protocos);

            timer1.Enabled = false;
            timer2.Enabled = false;
        }

        /// <summary>
        /// 刷新串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_refresh_Click(object sender, EventArgs e)
        {
            if (!MyDevice.protocol.IsOpen)
            {
                //刷串口
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox1.Items.Count < 1)
                {
                    comboBox1.Text = null;
                }
                //有可用串口
                else
                {
                    //通道一串口下拉框初始化
                    if (comboBox1.Items[0].ToString() == comboBox2.Text && comboBox1.Items.Count > 1)
                    {
                        comboBox1.SelectedIndex = 1;
                    }
                    else
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                }
            }
            if (!MyDevice.protocos.IsOpen)
            {
                //刷串口
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox2.Items.Count < 1)
                {
                    comboBox2.Text = null;
                }
                //有可用串口
                else
                {
                    //通道二串口下拉框初始化
                    if (comboBox2.Items[0].ToString() == comboBox1.Text && comboBox2.Items.Count > 1)
                    {
                        comboBox2.SelectedIndex = 1;
                    }
                    else
                    {
                        comboBox2.SelectedIndex = 0;
                    }
                }
            }

            //更新按钮状态
            bt_refresh.BackColor = Color.Green;
        }

        /// <summary>
        /// 连接通道一
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_connect1_Click(object sender, EventArgs e)
        {
            if (!MyDevice.protocol.IsOpen)
            {
                //打开串口
                MyDevice.protocol.Protocol_PortOpen(comboBox1.Text, 115200, StopBits.One, Parity.None);

                //串口发送
                if (MyDevice.protocol.IsOpen)
                {
                    MyDevice.myUpdate -= new freshHandler(update_FromUart1);
                    MyDevice.myUpdate += new freshHandler(update_FromUart1);

                    //更新按钮状态
                    bt_connect1.BackColor = Color.Green;

                    //更新按键指令状态
                    nextTask1 = TASKS.BOR;

                    //读出SCT
                    MyDevice.protocol.Protocol_ClearState();
                    MyDevice.protocol.Protocol_mePort_ReadTasks();

                    //初始化发送指令计时器
                    comTicker1 = 0;

                    //启动定时器
                    timer1.Enabled = true;
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
            else
            {

                MyDevice.myUpdate -= new freshHandler(update_FromUart1);

                if (MyDevice.protocol.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                    //清空串口
                    MyDevice.protocol.Protocol_ClearState();
                }

                //更新按键指令状态
                nextTask1 = TASKS.NULL;

                //关闭定时器
                timer1.Enabled = false;

                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocol.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocol.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                if (MyDevice.protocol.Protocol_PortClose())
                {
                    //更新界面
                    ucSignalLamp1.LampColor = status3;
                    bt_connect1.Text = "连 接";
                    bt_connect1.HoverBackColor = Color.LightSteelBlue;
                    Refresh();

                }
            }
        }

        /// <summary>
        /// 连接通道二
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_connect2_Click(object sender, EventArgs e)
        {
            if (!MyDevice.protocos.IsOpen)
            {
                //打开串口
                MyDevice.protocos.Protocol_PortOpen(comboBox2.Text, 115200, StopBits.One, Parity.None);

                //串口发送
                if (MyDevice.protocos.IsOpen)
                {

                    MyDevice.mySecondeUpdate -= new freshHandler(update_FromUart2);
                    MyDevice.mySecondeUpdate += new freshHandler(update_FromUart2);

                    //更新按钮状态
                    bt_connect2.BackColor = Color.Green;

                    //更新按键指令状态
                    nextTask2 = TASKS.BOR;

                    //读出SCT
                    MyDevice.protocos.Protocol_ClearState();
                    MyDevice.protocos.Protocol_mePort_ReadTasks();

                    //初始化发送指令计时器
                    comTicker2 = 0;
                    //启动定时器
                    timer2.Enabled = true;
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
            else
            {
                MyDevice.mySecondeUpdate -= new freshHandler(update_FromUart2);

                if (MyDevice.protocos.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocos);
                }

                //关闭定时器
                timer2.Enabled = false;

                //更新按键指令状态
                nextTask2 = TASKS.NULL;

                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocos.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocos.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                if (MyDevice.protocos.Protocol_PortClose())
                {
                    //更新界面
                    ucSignalLamp2.LampColor = status3;
                    bt_connect2.Text = "连 接";
                    bt_connect2.HoverBackColor = Color.LightSteelBlue;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// 归零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX3_Click(object sender, EventArgs e)
        {
            nextTask1 = TASKS.ZERO;
            nextTask2 = TASKS.ZERO;
        }

        /// <summary>
        /// 零点标定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                nextTask1 = TASKS.ADCP1;
            }
            if (textBox5.Text != "")
            {
                nextTask2 = TASKS.ADCP1;
            }
        }

        /// <summary>
        /// 满点标定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                nextTask1 = TASKS.ADCP5;
            }
            if (textBox6.Text != "")
            {
                nextTask2 = TASKS.ADCP5;
            }
        }

        /// <summary>
        /// 数据离开检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void output_Leave(object sender, EventArgs e)
        {
            TextBox text = (TextBox)sender;
            string str = text.Text;
            try
            {
                if (str.EndsWith("."))
                {
                    str = str.Remove(str.Length - 1, 1);
                }

                if (Convert.ToSingle(str) > 21f)
                {
                    str = "20";
                }
                else if (Convert.ToSingle(str) < 0f)
                {
                    str = "4";
                }
                text.Text = str;
            }
            catch
            {
                text.Text = "4";
            }
        }

        #region 串口一校准
        //串口一Full++
        private void bt_LeftFullAdd_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask1 = TASKS.GOUPF;
            }
            else
            {
                MyDevice.protocol.Protocol_SendCOM(TASKS.GOUPF);
            }
        }

        //串口一Full--
        private void bt_LeftFullDec_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask1 = TASKS.GODMF;
            }
            else
            {
                MyDevice.protocol.Protocol_SendCOM(TASKS.GODMF);
            }
        }

        //串口一Zero++
        private void bt_LeftZeroAdd_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask1 = TASKS.GOUPZ;
            }
            else
            {
                MyDevice.protocol.Protocol_SendCOM(TASKS.GOUPZ);
            }
        }

        //串口一Zero--
        private void bt_LeftZeroDec_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask1 = TASKS.GODMZ;
            }
            else
            {
                MyDevice.protocol.Protocol_SendCOM(TASKS.GODMZ);
            }
        }

        //串口一写入
        private void bt_LeftSure_Click(object sender, EventArgs e)
        {
            //取得校准dac值
            switch (actXET1.S_OutType)
            {
                case OUT.UT420:
                    actXET1.E_da_zero_4ma = leftZero;
                    actXET1.E_da_full_20ma = leftFull;
                    actXET1.T_analog1 = "4.0";
                    actXET1.T_analog2 = "8.0";
                    actXET1.T_analog3 = "12.0";
                    actXET1.T_analog4 = "16.0";
                    actXET1.T_analog5 = "20.0";
                    break;
                case OUT.UTP05:
                    actXET1.E_da_zero_05V = leftZero;
                    actXET1.E_da_full_05V = leftFull;
                    actXET1.T_analog1 = "0.0";
                    actXET1.T_analog2 = "1.25";
                    actXET1.T_analog3 = "2.5";
                    actXET1.T_analog4 = "3.75";
                    actXET1.T_analog5 = "5.0";
                    break;
                case OUT.UTP10:
                    actXET1.E_da_zero_10V = leftZero;
                    actXET1.E_da_full_10V = leftFull;
                    actXET1.T_analog1 = "0.0";
                    actXET1.T_analog2 = "2.5";
                    actXET1.T_analog3 = "5.0";
                    actXET1.T_analog4 = "7.5";
                    actXET1.T_analog5 = "10.0";
                    break;
                case OUT.UTN05:
                    actXET1.E_da_zero_N5 = leftZero;
                    actXET1.E_da_full_P5 = leftFull;
                    actXET1.T_analog1 = "0.0";
                    actXET1.T_analog2 = "1.25";
                    actXET1.T_analog3 = "2.5";
                    actXET1.T_analog4 = "3.75";
                    actXET1.T_analog5 = "5.0";
                    break;
                case OUT.UTN10:
                    actXET1.E_da_zero_N10 = leftZero;
                    actXET1.E_da_full_P10 = leftFull;
                    actXET1.T_analog1 = "0.0";
                    actXET1.T_analog2 = "2.5";
                    actXET1.T_analog3 = "5.0";
                    actXET1.T_analog4 = "7.5";
                    actXET1.T_analog5 = "10.0";
                    break;
            }

            //更新斜率
            actXET1.RefreshRatio();

            //写入SCT
            if (MyDevice.protocol.IsOpen)
            {
                bt_LeftSure.HoverBackColor = Color.Firebrick;
                Refresh();

                if (MyDevice.protocol.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                    //清空串口
                    MyDevice.protocol.Protocol_ClearState();
                }
                MyDevice.protocol.Protocol_ClearState();
                MyDevice.protocol.Protocol_mePort_WriteFacTasks();

                //更新按键指令状态
                nextTask1 = TASKS.WRX4;

            }
        }
        #endregion

        #region 串口二校准
        //串口二Full++
        private void bt_RightFullAdd_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocos.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask2 = TASKS.GOUPF;
            }
            else
            {
                MyDevice.protocos.Protocol_SendCOM(TASKS.GOUPF);
            }
        }

        //串口二Full--
        private void bt_RightFullDec_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocos.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask2 = TASKS.GODMF;
            }
            else
            {
                MyDevice.protocos.Protocol_SendCOM(TASKS.GODMF);
            }
        }

        //串口二Zero++
        private void bt_RightZeroAdd_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocos.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask2 = TASKS.GOUPZ;
            }
            else
            {
                MyDevice.protocos.Protocol_SendCOM(TASKS.GOUPZ);
            }

        }

        //串口二Zero--
        private void bt_RightZeroDec_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocos.trTASK == TASKS.DAC)
            {
                //更新按键指令状态
                nextTask2 = TASKS.GODMZ;
            }
            else
            {
                MyDevice.protocos.Protocol_SendCOM(TASKS.GODMZ);
            }
        }

        //串口二写入
        private void bt_RightSure_Click(object sender, EventArgs e)
        {
            //取得校准dac值
            switch (actXET2.S_OutType)
            {
                case OUT.UT420:
                    actXET2.E_da_zero_4ma = rightZero;
                    actXET2.E_da_full_20ma = rightFull;
                    actXET2.T_analog1 = "4.0";
                    actXET2.T_analog2 = "8.0";
                    actXET2.T_analog3 = "12.0";
                    actXET2.T_analog4 = "16.0";
                    actXET2.T_analog5 = "20.0";
                    break;
                case OUT.UTP05:
                    actXET2.E_da_zero_05V = rightZero;
                    actXET2.E_da_full_05V = rightFull;
                    actXET2.T_analog1 = "0.0";
                    actXET2.T_analog2 = "1.25";
                    actXET2.T_analog3 = "2.5";
                    actXET2.T_analog4 = "3.75";
                    actXET2.T_analog5 = "5.0";
                    break;
                case OUT.UTP10:
                    actXET2.E_da_zero_10V = rightZero;
                    actXET2.E_da_full_10V = rightFull;
                    actXET2.T_analog1 = "0.0";
                    actXET2.T_analog2 = "2.5";
                    actXET2.T_analog3 = "5.0";
                    actXET2.T_analog4 = "7.5";
                    actXET2.T_analog5 = "10.0";
                    break;
                case OUT.UTN05:
                    actXET2.E_da_zero_N5 = rightZero;
                    actXET2.E_da_full_P5 = rightFull;
                    actXET2.T_analog1 = "0.0";
                    actXET2.T_analog2 = "1.25";
                    actXET2.T_analog3 = "2.5";
                    actXET2.T_analog4 = "3.75";
                    actXET2.T_analog5 = "5.0";
                    break;
                case OUT.UTN10:
                    actXET2.E_da_zero_N10 = rightZero;
                    actXET2.E_da_full_P10 = rightFull;
                    actXET2.T_analog1 = "0.0";
                    actXET2.T_analog2 = "2.5";
                    actXET2.T_analog3 = "5.0";
                    actXET2.T_analog4 = "7.5";
                    actXET2.T_analog5 = "10.0";
                    break;
            }

            //更新斜率
            actXET2.RefreshRatio();

            //写入SCT
            if (MyDevice.protocos.IsOpen)
            {
                bt_RightSure.HoverBackColor = Color.Firebrick;
                Refresh();

                if (MyDevice.protocos.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocos);
                    //清空串口
                    MyDevice.protocos.Protocol_ClearState();
                }
                MyDevice.protocos.Protocol_ClearState();
                MyDevice.protocos.Protocol_mePort_WriteFacTasks();

                //更新按键指令状态
                nextTask2 = TASKS.WRX4;
            }
        }
        #endregion

        #region 串口一通讯
        // 串口一委托
        private void update_FromUart1()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(update_FromUart1);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                //发送指令计时器刷新
                comTicker1 = 0;

                //更新界面参数显示
                signalOutput1.Text = actXET1.R_output;

                switch (nextTask1)
                {
                    //连接读SCT
                    case TASKS.BOR:
                        //继续读取
                        MyDevice.protocol.Protocol_mePort_ReadTasks();
                        //界面状态更新
                        bt_connect1.Text = MyDevice.protocol.trTASK.ToString();

                        //所有流程读取完成后执行
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //初始化校准数据
                            leftFull = actXET1.E_da_full_20ma;
                            leftZero = actXET1.E_da_zero_4ma;
                            //更新界面
                            bt_connect1.HoverBackColor = Color.Green;
                            bt_connect1.Text = "连接";
                            ucSignalLamp1.LampColor = status2;
                            //更新按键指令状态
                            nextTask1 = TASKS.DAC;
                        }
                        break;

                    //校准写入
                    case TASKS.WRX4:
                        //继续写入
                        MyDevice.protocol.Protocol_mePort_WriteFacTasks();
                        //界面状态
                        bt_LeftSure.Text = MyDevice.protocol.trTASK.ToString();

                        //所有流程读取完成后执行
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //刷新界面按钮控件
                            bt_LeftSure.HoverBackColor = Color.Green;
                            bt_LeftSure.Text = "完成";
                            Refresh();
                            //启动读数据
                            start_dataMonitor1();
                        }
                        break;

                    //归零后重新读取SCT3
                    case TASKS.RDX3:
                        //只读SCT3
                        if (MyDevice.protocol.trTASK == TASKS.RDX3)
                        {
                            //刷新界面状态灯
                            ucSignalLamp1.LampColor = status2;
                            //读SCT3后启动读数据
                            start_dataMonitor1();
                        }
                        break;

                    //采零满点后执行写入
                    case TASKS.BCC:
                        //继续写
                        MyDevice.protocol.Protocol_mePort_WriteCalTasks();
                        //读结束
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //刷新界面状态灯
                            ucSignalLamp1.LampColor = status2;
                            Refresh();
                            //启动读数据
                            start_dataMonitor1();
                        }
                        break;

                    //执行一次归零任务
                    case TASKS.ZERO:
                        //更新按键指令状态
                        nextTask1 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp1.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        MyDevice.protocol.Protocol_SendCOM(TASKS.ZERO);
                        break;

                    //执行一次标定零点
                    case TASKS.ADCP1:
                        //更新按键指令状态
                        nextTask1 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp1.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        actXET1.T_analog1 = textBox3.Text;
                        MyDevice.protocol.Protocol_SendCOM(TASKS.ADCP1);
                        break;

                    //执行一次标定满点
                    case TASKS.ADCP5:
                        //更新按键指令状态
                        nextTask1 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp1.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        actXET1.T_analog5 = textBox4.Text;
                        MyDevice.protocol.Protocol_SendCOM(TASKS.ADCP5);
                        break;

                    //校准Full++、Full--、Zero++、Zero--
                    case TASKS.GODMF:
                    case TASKS.GODMZ:
                    case TASKS.GOUPF:
                    case TASKS.GOUPZ:
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        MyDevice.protocol.Protocol_SendCOM(nextTask1);
                        //更新按键指令状态
                        nextTask1 = TASKS.NULL;
                        break;

                    default:
                        switch (MyDevice.protocol.trTASK)
                        {
                            //归零后读SCT3
                            case TASKS.ZERO:
                                nextTask1 = TASKS.RDX3;
                                MyDevice.protocol.Protocol_SendCOM(TASKS.RDX3);
                                break;

                            //采零满点
                            case TASKS.ADCP1:
                            case TASKS.ADCP5:
                                actXET1.RefreshRatio();
                                nextTask1 = TASKS.BCC;
                                //写零满点
                                MyDevice.protocol.Protocol_ClearState();
                                MyDevice.protocol.Protocol_mePort_WriteCalTasks();
                                break;

                            //校准满点
                            case TASKS.GOUPF:
                            case TASKS.GODMF:
                                leftFull = Convert.ToInt32(MyDevice.protocol.rxString);
                                bt_LeftSure.HoverBackColor = Color.Firebrick;
                                Refresh();
                                break;

                            //校准零点
                            case TASKS.GOUPZ:
                            case TASKS.GODMZ:
                                leftZero = Convert.ToInt32(MyDevice.protocol.rxString);
                                bt_LeftSure.HoverBackColor = Color.Firebrick;
                                Refresh();
                                break;
                        }
                        break;
                }
            }
        }

        // 超时监控串口一
        private void timer1_Tick(object sender, EventArgs e)
        {
            //如果串口正在接受数据，则发生超时监控
            if (MyDevice.protocol.Is_serial_listening) return;

            switch (nextTask1)
            {
                //连接串口后读SCT任务监控
                case TASKS.BOR:
                    //如果超时需要重新读
                    if ((++comTicker1) > 5)
                    {
                        comTicker1 = 0;
                        MyDevice.protocol.Protocol_ChangeEQ();
                        MyDevice.protocol.Protocol_mePort_ReadTasks();
                    }
                    break;

                //监控任务
                case TASKS.DAC:
                    //如果没有收到02 80 80 80则comTicker计时器每300ms发送一次DACO指令
                    //如果有接收到02 80 80 80则comTicker计时器在接收委托中清零
                    if ((++comTicker1) > 5)
                    {
                        comTicker1 = 0;
                        MyDevice.protocol.Protocol_ChangeEQ();
                        MyDevice.protocol.Protocol_SendCOM(TASKS.DAC);
                    }
                    break;

                //校准写入后任务监控
                case TASKS.WRX4:
                    //如果超时需要重新读
                    if ((++comTicker1) > 5)
                    {
                        comTicker1 = 0;
                        MyDevice.protocol.Protocol_ChangeEQ();
                        MyDevice.protocol.Protocol_mePort_WriteFacTasks();
                    }
                    break;

                default:
                    break;
            }
        }

        //启动串口一数据读取
        private void start_dataMonitor1()
        {
            //发送指令计时器刷新
            comTicker1 = 0;
            //更新按键指令状态
            nextTask1 = TASKS.DAC;
            //清除数据确保R_eeplink不会02或03误设false
            MyDevice.protocol.Protocol_ClearState();
        }
        #endregion

        #region 串口二通讯
        // 串口二委托
        private void update_FromUart2()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(update_FromUart2);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                //发送指令计时器刷新
                comTicker2 = 0;

                //更新界面参数显示
                signalOutput2.Text = actXET2.R_output;

                switch (nextTask2)
                {
                    //连接读SCT
                    case TASKS.BOR:
                        //继续读取
                        MyDevice.protocos.Protocol_mePort_ReadTasks();
                        //界面状态更新
                        bt_connect2.Text = MyDevice.protocos.trTASK.ToString();

                        //所有流程读取完成后执行
                        if (MyDevice.protocos.trTASK == TASKS.NULL)
                        {
                            //初始化校准数据
                            rightFull = actXET2.E_da_full_20ma;
                            rightZero = actXET2.E_da_zero_4ma;
                            //更新界面
                            bt_connect2.HoverBackColor = Color.Green;
                            bt_connect2.Text = "连接";
                            ucSignalLamp2.LampColor = status2;
                            //更新按键指令状态
                            nextTask2 = TASKS.DAC;
                        }
                        break;

                    //校准写入
                    case TASKS.WRX4:
                        //继续写入
                        MyDevice.protocos.Protocol_mePort_WriteFacTasks();
                        //界面状态
                        bt_RightSure.Text = MyDevice.protocos.trTASK.ToString();

                        //所有流程读取完成后执行
                        if (MyDevice.protocos.trTASK == TASKS.NULL)
                        {
                            //刷新界面按钮控件
                            bt_RightSure.HoverBackColor = Color.Green;
                            bt_RightSure.Text = "完成";
                            Refresh();
                            //启动读数据
                            start_dataMonitor2();
                        }
                        break;

                    //归零后重新读取SCT3
                    case TASKS.RDX3:
                        //只读SCT3
                        if (MyDevice.protocos.trTASK == TASKS.RDX3)
                        {
                            //刷新界面状态灯
                            ucSignalLamp2.LampColor = status2;
                            Refresh();
                            //读SCT3后启动读数据
                            start_dataMonitor2();
                        }
                        break;

                    //采零满点后执行写入
                    case TASKS.BCC:
                        //继续写
                        MyDevice.protocos.Protocol_mePort_WriteCalTasks();
                        //读结束
                        if (MyDevice.protocos.trTASK == TASKS.NULL)
                        {
                            //刷新界面状态灯
                            ucSignalLamp2.LampColor = status2;
                            //启动读数据
                            start_dataMonitor2();
                        }
                        break;

                    //执行一次归零任务
                    case TASKS.ZERO:
                        //更新按键指令状态
                        nextTask2 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp2.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocos.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocos);
                        }
                        MyDevice.protocos.Protocol_SendCOM(TASKS.ZERO);
                        break;

                    //执行一次标定零点
                    case TASKS.ADCP1:
                        //更新按键指令状态
                        nextTask2 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp2.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocos.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocos);
                        }
                        actXET2.T_analog1 = textBox5.Text;
                        MyDevice.protocos.Protocol_SendCOM(TASKS.ADCP1);
                        break;

                    //执行一次标定满点
                    case TASKS.ADCP5:
                        //更新按键指令状态
                        nextTask2 = TASKS.NULL;
                        //刷新界面状态灯
                        ucSignalLamp2.LampColor = status1;
                        //如果在读DAC，则先停止DAC发送
                        if (MyDevice.protocos.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocos);
                        }
                        actXET2.T_analog5 = textBox6.Text;
                        MyDevice.protocos.Protocol_SendCOM(TASKS.ADCP5);
                        break;

                    //校准Full++、Full--、Zero++、Zero--
                    case TASKS.GODMF:
                    case TASKS.GODMZ:
                    case TASKS.GOUPF:
                    case TASKS.GOUPZ:
                        if (MyDevice.protocos.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocos);
                        }
                        MyDevice.protocos.Protocol_SendCOM(nextTask2);
                        //更新按键指令状态
                        nextTask2 = TASKS.NULL;
                        break;

                    default:
                        switch (MyDevice.protocos.trTASK)
                        {
                            //归零后读SCT3
                            case TASKS.ZERO:
                                nextTask2 = TASKS.RDX3;
                                MyDevice.protocos.Protocol_SendCOM(TASKS.RDX3);
                                break;

                            //采零满点
                            case TASKS.ADCP1:
                            case TASKS.ADCP5:
                                actXET2.RefreshRatio();
                                nextTask2 = TASKS.BCC;
                                //写零满点
                                MyDevice.protocos.Protocol_ClearState();
                                MyDevice.protocos.Protocol_mePort_WriteCalTasks();
                                break;

                            //校准满点
                            case TASKS.GOUPF:
                            case TASKS.GODMF:
                                rightFull = Convert.ToInt32(MyDevice.protocos.rxString);
                                bt_RightSure.HoverBackColor = Color.Firebrick;
                                Refresh();
                                break;

                            //校准零点
                            case TASKS.GOUPZ:
                            case TASKS.GODMZ:
                                rightZero = Convert.ToInt32(MyDevice.protocos.rxString);
                                bt_RightSure.HoverBackColor = Color.Firebrick;
                                Refresh();
                                break;
                        }
                        break;
                }
            }
        }

        // 超时监控串口二
        private void timer2_Tick(object sender, EventArgs e)
        {
            //如果串口正在接收数据，则不发生超时监控
            if (MyDevice.protocos.Is_serial_listening) return;

            switch (nextTask2)
            {
                //连接串口后读SCT任务监控
                case TASKS.BOR:
                    //如果超时需要重新读
                    if ((++comTicker2) > 5)
                    {
                        comTicker2 = 0;
                        MyDevice.protocos.Protocol_ChangeEQ();
                        MyDevice.protocos.Protocol_mePort_ReadTasks();
                    }
                    break;

                //监控任务
                case TASKS.DAC:
                    //如果没有收到02 80 80 80则comTicker计时器每300ms发送一次DACO指令
                    //如果有接收到02 80 80 80则comTicker计时器在接收委托中清零
                    if ((++comTicker2) > 5)
                    {
                        comTicker2 = 0;
                        MyDevice.protocos.Protocol_ChangeEQ();
                        MyDevice.protocos.Protocol_SendCOM(TASKS.DAC);
                    }
                    break;

                //校准写入后任务监控
                case TASKS.WRX4:
                    //如果超时需要重新读
                    if ((++comTicker2) > 5)
                    {
                        comTicker2 = 0;
                        MyDevice.protocos.Protocol_ChangeEQ();
                        MyDevice.protocos.Protocol_mePort_WriteFacTasks();
                    }
                    break;

                default:
                    break;
            }
        }

        //启动串口二数据读取
        private void start_dataMonitor2()
        {
            //发送指令计时器刷新
            comTicker2 = 0;
            //更新按键指令状态
            nextTask2 = TASKS.DAC;
            //清除数据确保R_eeplink不会02或03误设false
            MyDevice.protocos.Protocol_ClearState();
        }
        #endregion

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
                        this.Text = meLines[3].Split('=')[1];
                    }
                    else
                    {
                        this.Text = "MT2X420Device";
                    }
                }
                else
                {
                    this.Text = "MT2X420Device";
                }
            }
            catch
            {
                this.Text = "MT2X420Device";
            }
        }
    }
}
