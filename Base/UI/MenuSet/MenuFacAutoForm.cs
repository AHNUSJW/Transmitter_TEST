using DMM6500;
using Library;
using Model;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using Tooling;

//Tong Ziyun 20230705
//Lumi 20231222

namespace Base.UI.MenuSet
{
    public partial class MenuFacAutoForm : Form
    {
        private XET actXET;                 //需要操作的设备
        private Int32 step = 0;             //调试数据步骤

        private Thread RefreshDMMValueth;   // 显示更新DMM值
        private Boolean isExit = false;

        private TASKS nextTask = TASKS.NULL;//按键指令
        private int comTicker;              //发送指令计时器
        private volatile bool isTimer;      //是否执行定时器任务

        private Int32 da_full;              //记录调整数值 例：e_da_full_20ma
        private Int32 da_zero;              //记录调整数值 例：e_da_zero_20ma
        private Double limitFull = 0;       //记录数据调整的范围的上限0-10...
        private Double limitzZero = 0;      //记录数据调整范围的下限

        public MenuFacAutoForm()
        {
            InitializeComponent();

            // 读取初始化串口，DM6500等信息
            MyDefine.Dm6500 = new Dmm6500();
            MyDefine.InitConfig();

            #region 连接万用表并赋值

            //实时读取万用表值线程
            RefreshDMMValueth = new Thread(new ThreadStart(RefreshDMMValueTH));
            RefreshDMMValueth.IsBackground = true;
            RefreshDMMValueth.Start();

            //打开万用表
            if (!MyDefine.Dm6500._IsOpen)
            {
                RefreshListBoxCheckMsg("Start Check DMM6500...");
                toolStripLabelDMMconnectStuts.Text = "未连接";
                toolStripLabelDMMconnectStuts.ForeColor = Color.Red;
                //尝试连接DMM
                if (MyDefine.Dm6500.OpenDmm(MyDefine.DmmUsbTring))
                {
                    if (MyDefine.Dm6500._IsOpen)
                    {
                        toolStripLabelDMMconnectStuts.Text = "已连接";
                        RefreshListBoxCheckMsg("OK: DMM Connect Successful...");
                    }
                }
                else
                {
                    RefreshListBoxCheckMsg("Error: DMM 6500 Open failed.");
                    return;
                }
            }
            #endregion
        }

        private void MenuFacAutoForm_Load(object sender, EventArgs e)
        {
            actXET = MyDevice.actDev;

            //更新界面显示
            switch (actXET.S_DeviceType)
            {
                default:
                case TYPE.BE30AH:
                case TYPE.BS420H:
                case TYPE.T8X420H:
                case TYPE.BS600H:
                    //刷新串口
                    bt_Refresh_Click(sender, e);
                    groupBox3.Visible = true;
                    break;
                case TYPE.T420:
                case TYPE.TNP10:
                case TYPE.TP10:
                case TYPE.TDES:
                case TYPE.TDSS:
                case TYPE.T4X600H:
                    groupBox3.Visible = false;
                    break;
            }

            //显示设备参数
            RefreshListBoxConfig();

            #region 校验连接DMM6500
            if (!string.IsNullOrEmpty((MyDefine.DmmUsbTring)))
            {
                if (!MyDefine.Dm6500._IsOpen)
                {
                    toolStripLabelDMMconnectStuts.Text = MyDevice.languageType == 0 ? "未连接" : "Unconnect";
                    toolStripLabelDMMconnectStuts.ForeColor = Color.Red;
                }
                if (MyDefine.Dm6500.OpenDmm(MyDefine.DmmUsbTring))
                {
                    if (MyDefine.Dm6500._IsOpen)
                    {
                        toolStripLabelDMMconnectStuts.Text = MyDevice.languageType == 0 ? "已连接" : "Connected";
                        toolStripLabelDMMconnectStuts.ForeColor = Color.Green;
                    }
                }
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("DMM配置连接参数不能为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("The DMM configuration connection parameter cannot be null！", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            RefreshListBoxCheckMsg("OK Check DMM Successful...");
            #endregion

            MyDevice.myUpdate += new Model.freshHandler(update_FromUart);
        }

        private void MenuFacAutoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            RefreshDMMValueth.Abort();

            MyDevice.myUpdate -= new Model.freshHandler(update_FromUart);

            //线程退出标识
            isExit = true;

            //关闭销毁DMM6500
            MyDefine.Dm6500.CloseDmm();
        }

        /// <summary>
        /// 刷新串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Refresh_Click(object sender, EventArgs e)
        {
            //刷串口
            comboBox_port.Items.Clear();
            comboBox_port.Items.AddRange(SerialPort.GetPortNames());
            //无可用串口
            if (comboBox_port.Items.Count < 2)
            {
                comboBox_port.Text = null;
            }
            else
            {
                //刷新串口，去除通讯串口
                for (int i = 0; i < comboBox_port.Items.Count; i++)
                {
                    if (comboBox_port.Items[i].ToString() == MyDevice.protocol.portName)
                    {
                        comboBox_port.Items.RemoveAt(i);
                    }
                }
                //初始化combox串口选择
                comboBox_port.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 连接治具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Connect_Click(object sender, EventArgs e)
        {
            if (comboBox_port.Text != null)
            {
                //打开串口
                MyDefine.myJIG.PortOpen(comboBox_port.Text);

                //串口发送
                if (MyDefine.myJIG.IsOpen)
                {
                    bt_Connect.BackColor = Color.Red;
                    MyDefine.myUpdate += new Tooling.freshHandler(update_ToolingUart);

                    //打开治具
                    send_Machine(Tooling.Constants.OPEN);
                }
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_close_Click(object sender, EventArgs e)
        {
            if (comboBox_port.Text != null)
            {
                bt_Connect.BackColor = Color.White;
                if (MyDefine.myJIG.PortClose())
                {
                    bt_close.BackColor = Color.Green;
                }
                else
                {
                    bt_close.BackColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// 治具开关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //开
                send_Machine(Tooling.Constants.OPEN);
            }
            else if (e.Button == MouseButtons.Right)
            {
                //关
                send_Machine(Tooling.Constants.CLOSE);
            }
        }

        /// <summary>
        /// 切换治具电流电压档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //切换到电流档
                send_Machine(Tooling.Constants.CURRENT);
            }
            else if (e.Button == MouseButtons.Right)
            {
                //切换到电压档
                send_Machine(Tooling.Constants.VOLTAGE);
            }
        }

        /// <summary>
        /// 切换治具零满点采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //切换到采零点
                send_Machine(Tooling.Constants.ZERO);
            }
            else if (e.Button == MouseButtons.Right)
            {
                //切换到采满点
                send_Machine(Tooling.Constants.FULL);
            }
        }

        /// <summary>
        /// 开始自动校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_start_Click(object sender, EventArgs e)
        {
            #region 数据初始化
            //清空校验信息列表
            listBoxCheckMsg.Items.Clear();
            //更新界面校验信息
            RefreshListBoxCheckMsg("Start Auto Running...");
            //初始化校验步骤
            step = 0;
            #endregion

            #region 校验  DMM6500
            //开始检测设备连接状态
            if (string.IsNullOrEmpty((MyDefine.DmmUsbTring)))
            {
                RefreshListBoxCheckMsg("Error DMM configuration connection parameter cannot be empty.");
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("DMM配置连接参数不能为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("The DMM configuration connection parameter cannot be null！", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            if (!MyDefine.Dm6500._IsOpen)
            {
                RefreshListBoxCheckMsg("Start Check DMM6500...");
                toolStripLabelDMMconnectStuts.Text = MyDevice.languageType == 0 ? "未连接" : "Unconnect";
                toolStripLabelDMMconnectStuts.ForeColor = Color.Red;
                //尝试连接DMM
                if (MyDefine.Dm6500.OpenDmm(MyDefine.DmmUsbTring))
                {
                    if (MyDefine.Dm6500._IsOpen)
                    {
                        toolStripLabelDMMconnectStuts.Text = MyDevice.languageType == 0 ? "已连接" : "Connected";
                        RefreshListBoxCheckMsg("OK: DMM Connect Successful...");
                    }
                }
                else
                {
                    RefreshListBoxCheckMsg("Error: DMM 6500 Open failed.");
                    return;
                }
            }
            #endregion

            //更新指示灯状态
            ucSignalLamp1.LampColor[0] = Color.Red;
            Refresh();

            //更新界面校验信息
            timer1.Enabled = true;
            RefreshListBoxCheckMsg("------ Start Read Device Info ------");

            nextTask = TASKS.BCC;//读出所有SCT

            RefreshListBoxCheckMsg("----Start Read SCT----");
            if (!MyDevice.mySelfUART.IsOpen)
            {
                //打开串口
                MyDevice.mySelfUART.Protocol_PortOpen(MyDevice.mySelfUART.portName, 115200, StopBits.One, Parity.None);
            }
            MyDevice.mePort_ClearState();
            MyDevice.mePort_ReadTasks();
        }

        /// <summary>
        /// 停止自动校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_stop_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            nextTask = TASKS.NULL;
            //治具断电
            send_Machine(Tooling.Constants.CLOSE);
        }

        /// <summary>
        /// 零点采样
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_zero_sample_Click(object sender, EventArgs e)
        {
            nextTask = TASKS.NULL;
            bt_zero_sample.HoverBackColor = Color.Firebrick;
            Refresh();
            MyDevice.mePort_SendCOM(TASKS.ADCP1);
        }

        /// <summary>
        /// 满点采样
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_full_sample_Click(object sender, EventArgs e)
        {
            nextTask = TASKS.NULL;
            bt_full_sample.HoverBackColor = Color.Firebrick;
            Refresh();
            MyDevice.mePort_SendCOM(TASKS.ADCP5);
        }

        //矫正
        private void fresh_UartDataTasks()
        {
            isTimer = false;

            switch (nextTask)
            {
                //读SCT： BOR->RDX0->RDX1->RDX2->RDX3->RDX4->RDX5->RDX6->RDX7->RDX8
                case TASKS.BCC:
                    MyDevice.mePort_ReadTasks();
                    if (MyDevice.protocol.trTASK == TASKS.NULL)
                    {
                        nextTask = TASKS.BOR;
                        MyDevice.mePort_SendCOM(TASKS.BOR);
                    }
                    break;

                //更新Bohrcode
                case TASKS.BOR:
                    //刷新界面
                    RefreshListBoxConfig();
                    //更新输出outType
                    update_OutType();
                    break;

                //切换不同的输出（outType）后更新到下位机
                case TASKS.REST:
                    if (MyDevice.protocol.IsOpen)
                    {
                        RefreshListBoxCheckMsg("----Start Update----");
                        nextTask = TASKS.WRX0;
                        MyDevice.mePort_ClearState();
                        MyDevice.mePort_WriteTypTasks();
                    }
                    break;

                //发送重启后触发更新万用表状态
                case TASKS.WRX0:
                    //WRX0 -> RST
                    MyDevice.mePort_WriteTypTasks();
                    if ((MyDevice.protocol.trTASK == TASKS.NULL))
                    {
                        nextTask = TASKS.WRX1;
                        RefreshListBoxConfig();
                        MyDevice.mePort_SendCOM(TASKS.GOUPF);//Full+
                    }
                    break;

                //粗调--写入
                case TASKS.WRX1:
                    nextTask = TASKS.WRX2;
                    RefreshListBoxCheckMsg("----Start Coarse Tuning----");
                    //粗调
                    if (caculate())
                    {
                        RefreshListBoxCheckMsg("----Start Write----");
                        fact_Write();//写入
                    }
                    else
                    {
                        timer1.Enabled = false;
                        nextTask = TASKS.NULL;
                    }
                    break;

                //写入完成后触发更新万用表状态
                case TASKS.WRX2:
                    //BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst) -> NULL
                    MyDevice.mePort_WriteFacTasks();
                    if (MyDevice.protocol.trTASK == TASKS.NULL)//老版本重启指令无回复(不考虑)
                    {
                        RefreshListBoxConfig();
                        nextTask = TASKS.WRX3;
                        //触发更新万用表
                        MyDevice.mePort_SendCOM(TASKS.GOUPF);//Full+
                    }
                    break;

                //检验粗调的值是否合理
                case TASKS.WRX3:
                    //若不合理，则重新粗调
                    if (Math.Abs(limitFull - Dmm6500_value()) > 0.01)
                    {
                        nextTask = TASKS.WRX2;
                        RefreshListBoxCheckMsg("----Coarse Tuning Again----");
                        //粗调
                        if (caculate())
                        {
                            RefreshListBoxCheckMsg("----Start Write----");
                            fact_Write();//写入
                        }
                        else
                        {
                            timer1.Enabled = false;
                            nextTask = TASKS.NULL;
                        }
                    }
                    else
                    {
                        nextTask = TASKS.GOUPF;
                        RefreshListBoxCheckMsg("----Start Debugging The Zero Full Point----");
                        //触发
                        MyDevice.mePort_SendCOM(TASKS.GOUPF);//Full+
                    }
                    break;

                //调满点
                case TASKS.GOUPF:
                    caculateFull();
                    break;

                //调零点
                case TASKS.GOUPZ:
                    caculateZero();
                    break;

                case TASKS.WRX4://写入
                    //BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst) -> NULL
                    MyDevice.mePort_WriteFacTasks();
                    if (MyDevice.protocol.trTASK == TASKS.NULL)//老版本重启指令无回复
                    {
                        //检测需要校准的outType是否校准完成
                        if (++step < actXET.S_listPlan.Count)
                        {
                            //更新输出outTyepe
                            update_OutType();
                        }
                        else
                        {
                            //校准完成后初始化步骤
                            step = 0;
                            //结束校准
                            MyDevice.mePort_SendCOM(TASKS.DONE);
                            Thread.Sleep(50);

                            //零满点采集
                            switch (actXET.S_DeviceType)
                            {
                                case TYPE.TNP10:
                                case TYPE.TDSS:
                                case TYPE.TDES:
                                case TYPE.TP10:
                                case TYPE.T420:
                                case TYPE.T4X600H:
                                    //采满点
                                    nextTask = TASKS.DONE;
                                    RefreshListBoxCheckMsg("----Start mining full----");
                                    MyDevice.mePort_SendCOM(TASKS.ADCP5);
                                    break;
                                case TYPE.BE30AH:
                                case TYPE.T8X420H:
                                case TYPE.BS420H:
                                case TYPE.BS600H:
                                    nextTask = TASKS.ADCP5;
                                    //切换到采满点
                                    send_Machine(Tooling.Constants.FULL);
                                    break;
                            }
                        }
                    }
                    break;

                //采满点
                case TASKS.ADCP5:
                    nextTask = TASKS.DONE;
                    RefreshListBoxCheckMsg("----Start mining full----");
                    MyDevice.mePort_SendCOM(TASKS.ADCP5);
                    break;

                //判断采集值，结束校准
                case TASKS.DONE:
                    nextTask = TASKS.NULL;
                    timer1.Enabled = false;

                    //治具断电
                    send_Machine(Tooling.Constants.CLOSE);

                    //参数更新后重新计算ratio和AD值
                    actXET.RefreshRatio();
                    RefreshListBoxConfig();

                    //判断值是否在范围内
                    if (actXET.E_ad_full < 23911 && actXET.E_ad_full > -2080)
                    {
                        //更新指示灯状态
                        ucSignalLamp1.LampColor[0] = Color.Green;
                        RefreshListBoxCheckMsg("finished.");
                        Refresh();
                    }
                    else
                    {
                        RefreshListBoxCheckMsg("error.");
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("CS1237故障", "error");
                        }
                        else
                        {
                            MessageBox.Show("CS1237 failure", "error");
                        }
                    }
                    break;
            }

            comTicker = 0;
            isTimer = true;
        }

        // 切换outtype
        // 更新万用表状态
        // 更新治具状态
        // 更新调整值
        // 更新限定值
        private void update_OutType()
        {
            nextTask = TASKS.REST;

            //更新模拟量输出类型
            actXET.S_OutType = actXET.S_listPlan[step];

            //校准日期
            actXET.E_mfg_date = Convert.ToUInt32(System.DateTime.Now.ToString("yyMMddHHmm"));

            switch (actXET.S_listPlan[step])
            {
                case OUT.UT420:
                    RefreshListBoxCheckMsg("----Start regulating the current----");
                    //切换到DCI
                    MyDefine.Dm6500.SwitchScanTypeDCI();
                    //切换到电流档
                    send_Machine(Tooling.Constants.CURRENT);
                    //更新调整值
                    da_full = actXET.E_da_full_20ma;
                    da_zero = actXET.E_da_zero_4ma;
                    //更新数据调整的范围4 - 20...
                    limitFull = 20.0;
                    limitzZero = 4.0;
                    break;

                case OUT.UTP05:
                    RefreshListBoxCheckMsg("----Start adjusting the voltage(0-5V)----");
                    //切换到DCV
                    MyDefine.Dm6500.SwitchScanTypeDCV();
                    //切换到电压档
                    send_Machine(Tooling.Constants.VOLTAGE);
                    //更新调整值
                    da_full = actXET.E_da_full_05V;
                    da_zero = actXET.E_da_zero_05V;
                    //更新数据调整的范围0 - 5...
                    limitFull = 5.0;
                    limitzZero = 0;
                    break;

                case OUT.UTP10:
                    RefreshListBoxCheckMsg("----Start adjusting the voltage(0-10v)----");
                    //切换到DCV
                    MyDefine.Dm6500.SwitchScanTypeDCV();
                    //切换到电压档
                    send_Machine(Tooling.Constants.VOLTAGE);
                    //更新调整值
                    da_full = actXET.E_da_full_10V;
                    da_zero = actXET.E_da_zero_10V;
                    //更新数据调整的范围0 - 10...
                    limitFull = 10.0;
                    limitzZero = 0;
                    break;

                case OUT.UTN05:
                    RefreshListBoxCheckMsg("----Start adjusting the voltage(±5V)----");
                    //切换到DCV
                    MyDefine.Dm6500.SwitchScanTypeDCV();
                    //切换到电压档
                    send_Machine(Tooling.Constants.VOLTAGE);
                    //更新调整值
                    da_full = actXET.E_da_full_P5;
                    da_zero = actXET.E_da_zero_N5;
                    //更新数据调整的范围0 - 5...
                    limitFull = 5.0;
                    limitzZero = -5.0;
                    break;

                case OUT.UTN10:
                    RefreshListBoxCheckMsg("----Start adjusting the voltage(±10V)----");
                    //切换到DCV
                    MyDefine.Dm6500.SwitchScanTypeDCV();
                    //切换到电压档
                    send_Machine(Tooling.Constants.VOLTAGE);
                    //更新调整值
                    da_full = actXET.E_da_full_P10;
                    da_zero = actXET.E_da_zero_N10;
                    //更新数据调整的范围0 - 10...
                    limitFull = 10.0;
                    limitzZero = -10.0;
                    break;
            }
        }

        // 更新万用表读数
        private void RefreshDMMValueTH()
        {
            while (!isExit)
            {
                double dValue;
                if (MyDefine.Dm6500._IsOpen)
                {
                    dValue = 0.00000;

                    //读取DMM6500值
                    MyDefine.Dm6500.Query(out dValue);
                    //其它线程的操作请求
                    if (lbValue.InvokeRequired)
                    {
                        try
                        {
                            lbValue.Invoke(new Action<string>(S =>
                            {
                                lbValue.Text = S.ToString();
                            }), dValue.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        lbValue.Text = dValue.ToString();
                    }
                }
            }
        }

        // 更新设备参数
        private void RefreshListBoxConfig()
        {
            //其它线程的操作请求
            this.BeginInvoke(new System.Action(delegate
            {
                byte deci = actXET.S_decimal;
                string unit = " " + actXET.S_unit;
                string unitUmask = actXET.GetUnitUMASK();
                string sens = actXET.RefreshSens();//灵敏度

                listBoxConfig.Items.Clear();
                listBoxConfig.Items.Add("----------------");
                listBoxConfig.Items.Add("TYPE = " + actXET.S_DeviceType.ToString());
                listBoxConfig.Items.Add("OUT  = " + actXET.S_OutType.ToString());
                listBoxConfig.Items.Add("----------------");
                listBoxConfig.Items.Add("[0]e_test      = 0x" + actXET.E_test.ToString("X2"));
                listBoxConfig.Items.Add("[0]e_outype    = 0x" + actXET.E_outype.ToString("X2"));
                listBoxConfig.Items.Add("[0]e_curve     = 0x" + actXET.E_curve.ToString("X2"));
                listBoxConfig.Items.Add("[0]e_adspeed   = 0x" + actXET.E_adspeed.ToString("X2"));
                listBoxConfig.Items.Add("[0]e_mfg_date  = " + actXET.E_mfg_date.ToString());
                listBoxConfig.Items.Add("[0]e_bohrcode  = " + actXET.E_bohrcode.ToString("X8"));
                listBoxConfig.Items.Add("[0]e_protype   = " + actXET.E_protype.ToString());
                listBoxConfig.Items.Add("----------------");
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CTWOPT:
                        listBoxConfig.Items.Add("[2]e_input1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_analog1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("e_sens         = " + sens);
                        listBoxConfig.Items.Add("[1]e_ad_point1 = " + actXET.E_ad_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point5 = " + actXET.E_ad_point5.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point1 = " + actXET.E_da_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point5 = " + actXET.E_da_point5.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_zero   = " + actXET.E_ad_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_full   = " + actXET.E_ad_full.ToString());
                        listBoxConfig.Items.Add("[3]e_da_zero   = " + actXET.E_da_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_da_full   = " + actXET.E_da_full.ToString());
                        listBoxConfig.Items.Add("[3]e_vtio      = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBoxConfig.Items.Add("----------------");
                        break;
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                        listBoxConfig.Items.Add("[2]e_input1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_analog1 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog2 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog3 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog4 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog5 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("e_sens       = " + sens);
                        listBoxConfig.Items.Add("----------------");
                        listBoxConfig.Items.Add("[1]e_ad_point1 = " + actXET.E_ad_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point2 = " + actXET.E_ad_point2.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point3 = " + actXET.E_ad_point3.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point4 = " + actXET.E_ad_point4.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point5 = " + actXET.E_ad_point5.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point1 = " + actXET.E_da_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point2 = " + actXET.E_da_point2.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point3 = " + actXET.E_da_point3.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point4 = " + actXET.E_da_point4.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point5 = " + actXET.E_da_point5.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_zero   = " + actXET.E_ad_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_full   = " + actXET.E_ad_full.ToString());
                        listBoxConfig.Items.Add("[3]e_da_zero   = " + actXET.E_da_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_da_full   = " + actXET.E_da_full.ToString());
                        listBoxConfig.Items.Add("----------------");
                        listBoxConfig.Items.Add("e_vtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBoxConfig.Items.Add("e_wtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBoxConfig.Items.Add("e_atio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBoxConfig.Items.Add("e_btio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBoxConfig.Items.Add("e_ctio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBoxConfig.Items.Add("e_dtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBoxConfig.Items.Add("----------------");
                        break;
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        listBoxConfig.Items.Add("[2]e_input1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input2   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input3   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input4   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[2]e_input5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[7]e_input6   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[7]e_input7   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[7]e_input8   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[7]e_input9   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[7]e_input10  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V");
                        listBoxConfig.Items.Add("[8]e_input11  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V");
                        listBoxConfig.Items.Add("----------------");
                        listBoxConfig.Items.Add("[2]e_analog1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[2]e_analog5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[7]e_analog6  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[7]e_analog7  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[7]e_analog8  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[7]e_analog9  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[7]e_analog10 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("[8]e_analog11 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
                        listBoxConfig.Items.Add("e_sens        = " + sens);
                        listBoxConfig.Items.Add("----------------");
                        listBoxConfig.Items.Add("[1]e_ad_point1  = " + actXET.E_ad_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point2  = " + actXET.E_ad_point2.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point3  = " + actXET.E_ad_point3.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point4  = " + actXET.E_ad_point4.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point5  = " + actXET.E_ad_point5.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point6  = " + actXET.E_ad_point6.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point7  = " + actXET.E_ad_point7.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point8  = " + actXET.E_ad_point8.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point9  = " + actXET.E_ad_point9.ToString());
                        listBoxConfig.Items.Add("[1]e_ad_point10 = " + actXET.E_ad_point10.ToString());
                        listBoxConfig.Items.Add("[8]e_ad_point11 = " + actXET.E_ad_point11.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point1  = " + actXET.E_da_point1.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point2  = " + actXET.E_da_point2.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point3  = " + actXET.E_da_point3.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point4  = " + actXET.E_da_point4.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point5  = " + actXET.E_da_point5.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point6  = " + actXET.E_da_point6.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point7  = " + actXET.E_da_point7.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point8  = " + actXET.E_da_point8.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point9  = " + actXET.E_da_point9.ToString());
                        listBoxConfig.Items.Add("[1]e_da_point10 = " + actXET.E_da_point10.ToString());
                        listBoxConfig.Items.Add("[8]e_da_point11 = " + actXET.E_da_point11.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_zero    = " + actXET.E_ad_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_ad_full    = " + actXET.E_ad_full.ToString());
                        listBoxConfig.Items.Add("[3]e_da_zero    = " + actXET.E_da_zero.ToString());
                        listBoxConfig.Items.Add("[3]e_da_full    = " + actXET.E_da_full.ToString());
                        listBoxConfig.Items.Add("----------------");
                        listBoxConfig.Items.Add("e_vtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBoxConfig.Items.Add("e_wtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBoxConfig.Items.Add("e_atio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBoxConfig.Items.Add("e_btio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBoxConfig.Items.Add("e_ctio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBoxConfig.Items.Add("e_dtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBoxConfig.Items.Add("e_etio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_etio).ToString());
                        listBoxConfig.Items.Add("e_ftio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ftio).ToString());
                        listBoxConfig.Items.Add("e_gtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_gtio).ToString());
                        listBoxConfig.Items.Add("e_htio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_htio).ToString());
                        listBoxConfig.Items.Add("e_itio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_itio).ToString());
                        listBoxConfig.Items.Add("e_jtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_jtio).ToString());
                        listBoxConfig.Items.Add("----------------");
                        break;
                }
                listBoxConfig.Items.Add("[4]e_da_zero_4ma  = " + actXET.E_da_zero_4ma.ToString());
                listBoxConfig.Items.Add("[4]e_da_full_20ma = " + actXET.E_da_full_20ma.ToString());
                listBoxConfig.Items.Add("[4]e_da_zero_05V  = " + actXET.E_da_zero_05V.ToString());
                listBoxConfig.Items.Add("[4]e_da_full_05V  = " + actXET.E_da_full_05V.ToString());
                listBoxConfig.Items.Add("[4]e_da_zero_10V  = " + actXET.E_da_zero_10V.ToString());
                listBoxConfig.Items.Add("[4]e_da_full_10V  = " + actXET.E_da_full_10V.ToString());
                listBoxConfig.Items.Add("[4]e_da_zero_N5   = " + actXET.E_da_zero_N5.ToString());
                listBoxConfig.Items.Add("[4]e_da_full_P5   = " + actXET.E_da_full_P5.ToString());
                listBoxConfig.Items.Add("[4]e_da_zero_N10  = " + actXET.E_da_zero_N10.ToString());
                listBoxConfig.Items.Add("[4]e_da_full_P10  = " + actXET.E_da_full_P10.ToString());
                listBoxConfig.Items.Add("----------------");
                listBoxConfig.Items.Add("[5]e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
                listBoxConfig.Items.Add("[5]e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                listBoxConfig.Items.Add("----------------");
            }));
        }

        // 更新界面校验信息
        private void RefreshListBoxCheckMsg(string msg)
        {
            listBoxCheckMsg.Items.Insert(0, $"{System.DateTime.Now.ToString("MM-dd HH:mm:ss:fff")}    {msg}");
        }

        // 粗调
        private Boolean caculate()
        {
            Double dacset;
            Double output;
            Int32 correct;

            //
            try
            {
                dacset = da_full;
                //获取万用表值
                output = Dmm6500_value();
            }
            catch (Exception)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("数据错误,请点击停止按钮后再次点击开始按钮");
                }
                else
                {
                    MessageBox.Show("Data error, please click the stop button and click the start button again");
                }
                return false;
            }

            //
            if (output < 1.0f || output > 25.0f)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("数据异常，请点击停止按钮后，重新放板子，再测一次", "Error");
                }
                else
                {
                    MessageBox.Show("Data is abnormal, please click the stop button, put the board again, and test again", "Error");
                }
                return false;
            }

            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    da_full = (int)(dacset * 20.0f / output + 0.5f);
                    da_zero = (int)(dacset * 4.0f / output + 0.5f);
                    break;
                case OUT.UTP05:
                    da_full = (int)(dacset * 5.0f / output + 0.5f);
                    da_zero = 0;
                    break;
                case OUT.UTP10:
                    da_full = (int)(dacset * 10.0f / output + 0.5f);
                    da_zero = 0;
                    break;
                case OUT.UTN05:
                    correct = (int)(((dacset - 524287.0f) * 5.0f / output) + 0.5f);
                    da_full = 0x7FFFF + correct;
                    da_zero = 0x7FFFF - correct - 192;
                    break;
                case OUT.UTN10:
                    correct = (int)(((dacset - 524287.0f) * 10.0f / output) + 0.5f);
                    da_full = 0x7FFFF + correct;
                    da_zero = 0x7FFFF - correct - 192;
                    break;
            }

            return true;
        }

        // 调满点
        private void caculateFull()
        {
            if (Math.Abs(limitFull - Dmm6500_value()) <= actXET.S_limitNum)
            {
                da_full = Convert.ToInt32(MyDevice.protocol.rxString);
                switch (actXET.S_OutType)
                {
                    default:
                    case OUT.UTP05:
                    case OUT.UTP10: //不校验零点
                        RefreshListBoxCheckMsg("----Start Write----");
                        nextTask = TASKS.WRX4;
                        fact_Write();//写入
                        break;
                    case OUT.UT420:
                    case OUT.UTN05:
                    case OUT.UTN10:
                        nextTask = TASKS.GOUPZ;
                        //触发s
                        MyDevice.mePort_SendCOM(TASKS.GOUPZ);//Zero+
                        break;
                }
            }
            else if ((limitFull - Dmm6500_value()) > actXET.S_limitNum)
            {
                MyDevice.mePort_SendCOM(TASKS.GOUPF);//Full+
            }
            else
            {
                MyDevice.mePort_SendCOM(TASKS.GODMF);//Full-
            }
        }

        // 调零点
        private void caculateZero()
        {
            if (Math.Abs(limitzZero - Dmm6500_value()) <= actXET.S_limitNum)
            {
                da_zero = Convert.ToInt32(MyDevice.protocol.rxString);

                RefreshListBoxCheckMsg("----Start Write----");
                nextTask = TASKS.WRX4;
                fact_Write();//写入
            }
            else if ((limitzZero - Dmm6500_value()) > actXET.S_limitNum)
            {
                MyDevice.mePort_SendCOM(TASKS.GOUPZ);//Zero+
            }
            else
            {
                MyDevice.mePort_SendCOM(TASKS.GODMZ);//Zero-
            }
        }

        // 写入
        private void fact_Write()
        {
            //取得校准dac值
            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    actXET.E_da_zero_4ma = da_zero;
                    actXET.E_da_full_20ma = da_full;
                    actXET.T_analog1 = "4.0";
                    actXET.T_analog2 = "8.0";
                    actXET.T_analog3 = "12.0";
                    actXET.T_analog4 = "16.0";
                    actXET.T_analog5 = "20.0";
                    break;
                case OUT.UTP05:
                    actXET.E_da_zero_05V = da_zero;
                    actXET.E_da_full_05V = da_full;
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "1.25";
                    actXET.T_analog3 = "2.5";
                    actXET.T_analog4 = "3.75";
                    actXET.T_analog5 = "5.0";
                    break;
                case OUT.UTP10:
                    actXET.E_da_zero_10V = da_zero;
                    actXET.E_da_full_10V = da_full;
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "2.5";
                    actXET.T_analog3 = "5.0";
                    actXET.T_analog4 = "7.5";
                    actXET.T_analog5 = "10.0";
                    break;
                case OUT.UTN05:
                    actXET.E_da_zero_N5 = da_zero;
                    actXET.E_da_full_P5 = da_full;
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "1.25";
                    actXET.T_analog3 = "2.5";
                    actXET.T_analog4 = "3.75";
                    actXET.T_analog5 = "5.0";
                    break;
                case OUT.UTN10:
                    actXET.E_da_zero_N10 = da_zero;
                    actXET.E_da_full_P10 = da_full;
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "2.5";
                    actXET.T_analog3 = "5.0";
                    actXET.T_analog4 = "7.5";
                    actXET.T_analog5 = "10.0";
                    break;
            }

            //更新斜率
            actXET.RefreshRatio();

            //写入SCT
            if (MyDevice.protocol.IsOpen)
            {
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteFacTasks();
            }
        }

        // 获取电压表值
        private double Dmm6500_value()
        {
            double output;

            //获取万用表数值
            MyDefine.Dm6500.Query(out output);

            //获取万用表值
            if (actXET.S_OutType == OUT.UT420)
            {
                //电流值需要放大1000
                output *= 1000;
            }
            return output;
        }

        // 发送指令给治具
        private void send_Machine(Byte bt_state)
        {
            //只有型号为BE30AH、BS420H、BS600H才与治具通讯
            switch (actXET.S_DeviceType)
            {
                case TYPE.BE30AH:
                case TYPE.BS420H:
                case TYPE.T8X420H:
                case TYPE.BS600H:
                    MyDefine.myJIG.SendCOM(bt_state);
                    break;
                default:
                    fresh_UartDataTasks();
                    break;
            }
        }

        // 治具委托串口通讯
        private void update_ToolingUart()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    Tooling.freshHandler meDelegate = new Tooling.freshHandler(update_ToolingUart);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                bt_Connect.BackColor = Color.Green;
                MyDefine.myUpdate -= new Tooling.freshHandler(update_ToolingUart);
                MyDefine.myUpdate += new Tooling.freshHandler(update_FromUart);
            }
        }

        //串口通讯响应
        private void update_FromUart()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    Model.freshHandler meDelegate = new Model.freshHandler(update_FromUart);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                if (nextTask != TASKS.NULL)
                {
                    fresh_UartDataTasks();
                }
                else
                {
                    switch (MyDevice.protocol.trTASK)
                    {
                        //零点采样
                        case TASKS.ADCP1:
                            actXET.RefreshRatio();
                            RefreshListBoxConfig();
                            bt_zero_sample.HoverBackColor = Color.Green;
                            break;

                        //满点采样
                        case TASKS.ADCP5:
                            actXET.RefreshRatio();
                            RefreshListBoxConfig();
                            bt_full_sample.HoverBackColor = Color.Green;
                            break;
                    }
                }
            }
        }

        //超时监控
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Main.ActiveForm.Contains("SetFacAuto") && isTimer)
            {
                if (MyDevice.protocol.Is_serial_listening) return;

                if (++comTicker > 5)
                {
                    comTicker = 0;
                    switch (nextTask)
                    {
                        //读SCT任务
                        case TASKS.BCC:
                            MyDevice.mePort_ReadTasks();
                            break;

                        //更新BOR
                        case TASKS.BOR:
                            MyDevice.mePort_SendCOM(TASKS.BOR);
                            break;

                        //切换不同的outType
                        case TASKS.REST:
                            update_OutType();
                            break;

                        //切换不同的输出（outType）后更新到下位机
                        case TASKS.WRX0:
                            MyDevice.mePort_WriteTypTasks();
                            break;

                        //触发更新万用表状态
                        case TASKS.WRX1:
                        case TASKS.WRX3:
                        case TASKS.GOUPF:
                            MyDevice.mePort_SendCOM(TASKS.GOUPF);//Full+
                            break;

                        //粗调--写入
                        case TASKS.WRX2:
                        case TASKS.WRX4:
                            MyDevice.mePort_WriteFacTasks();
                            break;

                        //调零点
                        case TASKS.GOUPZ:
                            MyDevice.mePort_SendCOM(TASKS.GOUPZ);//Zero+
                            break;

                        //采满点
                        case TASKS.ADCP5:
                        case TASKS.DONE:
                            MyDevice.mePort_SendCOM(TASKS.ADCP5);
                            break;
                    }
                }
            }
        }
    }
}
