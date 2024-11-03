using Model;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

//加载窗口时初始化页面

//开放给num使用的

//更新Bohrcode功能独立管理

//修改输出型号后需要更新设备,才能开始校准,做好防呆

namespace Base.UI.MenuSet
{
    public partial class MenuFacUserForm : Form
    {
        private XET actXET;//需要操作的设备

        //TASKS.BOR 更新Bohrcode
        //TASKS.WRX0 修改outype
        //TASKS.WRX4 校准后写入
        private TASKS uiTask;//按键指令


        //构造函数
        public MenuFacUserForm()
        {
            InitializeComponent();
        }

        //加载事件
        private void MenuFacUserForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //
            actXET = MyDevice.actDev;

            //错误信息提示
            label13.Text = "";
            label13.ForeColor = Color.Firebrick;
            bt_bohrcode.Visible = false;
            tabPage2.Enabled = false;

            //根据模拟量输出类型,调整comboBox1内容
            switch (actXET.S_DeviceType)
            {
                case TYPE.BS600H:
                case TYPE.TNP10:
                case TYPE.T4X600H:
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Output 0-5V");
                    comboBox1.Items.Add("Output 0-10V");
                    comboBox1.Items.Add("Output ±5V");
                    comboBox1.Items.Add("Output ±10V");
                    switch (actXET.S_OutType)
                    {
                        case OUT.UTP05: comboBox1.SelectedIndex = 0; break;
                        case OUT.UTP10: comboBox1.SelectedIndex = 1; break;
                        case OUT.UTN05: comboBox1.SelectedIndex = 2; break;
                        case OUT.UTN10: comboBox1.SelectedIndex = 3; break;
                    }
                    break;

                case TYPE.BS420H:
                case TYPE.T8X420H:
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Output 4-20mA");
                    comboBox1.Items.Add("Output 0-5V");
                    comboBox1.Items.Add("Output 0-10V");
                    switch (actXET.S_OutType)
                    {
                        case OUT.UT420: comboBox1.SelectedIndex = 0; break;
                        case OUT.UTP05: comboBox1.SelectedIndex = 1; break;
                        case OUT.UTP10: comboBox1.SelectedIndex = 2; break;
                    }
                    break;

                case TYPE.T420:
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Output 4-20mA");
                    comboBox1.SelectedIndex = 0;
                    break;

                case TYPE.BE30AH:
                case TYPE.TP10:
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Output 0-5V");
                    comboBox1.Items.Add("Output 0-10V");
                    switch (actXET.S_OutType)
                    {
                        case OUT.UTP05: comboBox1.SelectedIndex = 0; break;
                        case OUT.UTP10: comboBox1.SelectedIndex = 1; break;
                    }
                    break;

                case TYPE.TDES:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add("Output 4-20mA");
                        comboBox1.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add("Output 0-5V");
                        comboBox1.Items.Add("Output 0-10V");
                        switch (actXET.S_OutType)
                        {
                            case OUT.UTP05: comboBox1.SelectedIndex = 0; break;
                            case OUT.UTP10: comboBox1.SelectedIndex = 1; break;
                        }
                    }
                    break;

                case TYPE.TDSS:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add("Output 4-20mA");
                        comboBox1.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add("Output 0-5V");
                        comboBox1.Items.Add("Output 0-10V");
                        comboBox1.Items.Add("Output ±5V");
                        comboBox1.Items.Add("Output ±10V");
                        switch (actXET.S_OutType)
                        {
                            case OUT.UTP05: comboBox1.SelectedIndex = 0; break;
                            case OUT.UTP10: comboBox1.SelectedIndex = 1; break;
                            case OUT.UTN05: comboBox1.SelectedIndex = 2; break;
                            case OUT.UTN10: comboBox1.SelectedIndex = 3; break;
                        }
                    }
                    break;

                default:
                case TYPE.TD485:
                case TYPE.TCAN:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Output 数字量");
                    comboBox1.SelectedIndex = 0;
                    tabControl1.Enabled = false;
                    break;
            }

            //避免刚进窗口,初始化comboBox1后,comboBox1_SelectedIndexChanged将按钮置红色
            bt_outype.HoverBackColor = button1.HoverBackColor;
        }

        //关闭窗口
        private void MenuFacUserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            MyDevice.mePort_SendCOM(TASKS.DONE);//结束矫正
        }

        //BohrCode
        private void bt_bohrcode_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.BOR;//更新Bohrcode

            bt_bohrcode.HoverBackColor = Color.Firebrick;
            bt_outype.HoverBackColor = button1.HoverBackColor;
            bt_fact_write.HoverBackColor = button1.HoverBackColor;

            MyDevice.mePort_SendCOM(TASKS.BOR);
        }

        //Output type change
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label13.Text = "";
            textBox3.Text = "";

            switch (comboBox1.Text)
            {
                case "Output 4-20mA":
                    this.Text = "矫正4-20mA";
                    label1.Text = "Full: e_da_full_20ma =";
                    label2.Text = "Zero: e_da_zero_4ma =";
                    textBox1.Text = actXET.E_da_full_20ma.ToString();
                    textBox2.Text = actXET.E_da_zero_4ma.ToString();
                    if (actXET.S_OutType != OUT.UT420)
                    {
                        uiTask = TASKS.WRX0;
                        bt_outype.HoverBackColor = Color.Firebrick;
                    }
                    else
                    {
                        uiTask = TASKS.NULL;
                        bt_outype.HoverBackColor = button1.HoverBackColor;
                    }
                    break;

                case "Output 0-5V":
                    this.Text = "矫正0-5V";
                    label1.Text = "Full: e_da_full_05V =";
                    label2.Text = "Zero: e_da_zero_05V =";
                    textBox1.Text = actXET.E_da_full_05V.ToString();
                    textBox2.Text = actXET.E_da_zero_05V.ToString();
                    if (actXET.S_OutType != OUT.UTP05)
                    {
                        uiTask = TASKS.WRX0;
                        bt_outype.HoverBackColor = Color.Firebrick;
                    }
                    else
                    {
                        uiTask = TASKS.NULL;
                        bt_outype.HoverBackColor = button1.HoverBackColor;
                    }
                    break;

                case "Output 0-10V":
                    this.Text = "矫正0-10V";
                    label1.Text = "Full: e_da_full_10V =";
                    label2.Text = "Zero: e_da_zero_10V =";
                    textBox1.Text = actXET.E_da_full_10V.ToString();
                    textBox2.Text = actXET.E_da_zero_10V.ToString();
                    if (actXET.S_OutType != OUT.UTP10)
                    {
                        uiTask = TASKS.WRX0;
                        bt_outype.HoverBackColor = Color.Firebrick;
                    }
                    else
                    {
                        uiTask = TASKS.NULL;
                        bt_outype.HoverBackColor = button1.HoverBackColor;
                    }
                    break;

                case "Output ±5V":
                    this.Text = "矫正±5V";
                    label1.Text = "Full: e_da_full_P5 =";
                    label2.Text = "Zero: e_da_zero_N5 =";
                    textBox1.Text = actXET.E_da_full_P5.ToString();
                    textBox2.Text = actXET.E_da_zero_N5.ToString();
                    if (actXET.S_OutType != OUT.UTN05)
                    {
                        uiTask = TASKS.WRX0;
                        bt_outype.HoverBackColor = Color.Firebrick;
                    }
                    else
                    {
                        uiTask = TASKS.NULL;
                        bt_outype.HoverBackColor = button1.HoverBackColor;
                    }
                    break;

                case "Output ±10V":
                    this.Text = "矫正±10V";
                    label1.Text = "Full: e_da_full_P10 =";
                    label2.Text = "Zero: e_da_zero_N10 =";
                    textBox1.Text = actXET.E_da_full_P10.ToString();
                    textBox2.Text = actXET.E_da_zero_N10.ToString();
                    if (actXET.S_OutType != OUT.UTN10)
                    {
                        uiTask = TASKS.WRX0;
                        bt_outype.HoverBackColor = Color.Firebrick;
                    }
                    else
                    {
                        uiTask = TASKS.NULL;
                        bt_outype.HoverBackColor = button1.HoverBackColor;
                    }
                    break;
            }

            bt_caculate.HoverBackColor = button1.HoverBackColor;
            bt_fact_write.HoverBackColor = button1.HoverBackColor;
            Refresh();
        }

        //更新
        private void bt_outype_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.WRX0;//修改outype

            //根据comboBox1内容,更新模拟量输出类型
            switch (actXET.S_DeviceType)
            {
                case TYPE.BS600H:
                case TYPE.TNP10:
                case TYPE.T4X600H:
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0: actXET.S_OutType = OUT.UTP05; break;
                        case 1: actXET.S_OutType = OUT.UTP10; break;
                        case 2: actXET.S_OutType = OUT.UTN05; break;
                        case 3: actXET.S_OutType = OUT.UTN10; break;
                    }
                    break;

                case TYPE.BS420H:
                case TYPE.T8X420H:
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0: actXET.S_OutType = OUT.UT420; break;
                        case 1: actXET.S_OutType = OUT.UTP05; break;
                        case 2: actXET.S_OutType = OUT.UTP10; break;
                    }
                    break;

                case TYPE.T420:
                    actXET.S_OutType = OUT.UT420;
                    break;

                case TYPE.BE30AH:
                case TYPE.TP10:
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0: actXET.S_OutType = OUT.UTP05; break;
                        case 1: actXET.S_OutType = OUT.UTP10; break;
                    }
                    break;

                case TYPE.TDES:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        actXET.S_OutType = OUT.UT420;
                    }
                    else
                    {
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0: actXET.S_OutType = OUT.UTP05; break;
                            case 1: actXET.S_OutType = OUT.UTP10; break;
                        }
                    }
                    break;

                case TYPE.TDSS:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        actXET.S_OutType = OUT.UT420;
                    }
                    else
                    {
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0: actXET.S_OutType = OUT.UTP05; break;
                            case 1: actXET.S_OutType = OUT.UTP10; break;
                            case 2: actXET.S_OutType = OUT.UTN05; break;
                            case 3: actXET.S_OutType = OUT.UTN10; break;
                        }
                    }
                    break;

                default:
                case TYPE.TD485:
                case TYPE.TCAN:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    actXET.S_OutType = OUT.UMASK;
                    break;
            }

            //校准日期
            actXET.E_mfg_date = Convert.ToUInt32(System.DateTime.Now.ToString("yyMMddHHmm"));

            //
            if (MyDevice.protocol.IsOpen)
            {
                textBox3.Text = "";
                bt_outype.HoverBackColor = Color.Firebrick;
                bt_caculate.HoverBackColor = button1.HoverBackColor;
                bt_fact_write.HoverBackColor = button1.HoverBackColor;
                Refresh();

                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteTypTasks();
            }
        }

        //Full++
        private void button1_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GOUPF);
            }
        }

        //Full--
        private void button2_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GODMF);
            }
        }

        //Zero++
        private void button3_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GOUPZ);
            }
        }

        //Zero--
        private void button4_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GODMZ);
            }
        }

        //自动计算
        private void bt_caculate_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                if (!String.IsNullOrEmpty(textBox3.Text))
                {
                    Double dacset;
                    Double output;
                    Int32 correct;

                    //
                    try
                    {
                        dacset = Convert.ToDouble(textBox1.Text.Trim());
                        output = Convert.ToDouble(textBox3.Text.Trim());
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    //
                    if (output < 1.0f)
                    {
                        return;
                    }
                    else if (output > 25.0f)
                    {
                        return;
                    }

                    //
                    switch (actXET.S_OutType)
                    {
                        case OUT.UT420:
                            correct = (int)(dacset * 20.0f / output + 0.5f);
                            textBox1.Text = correct.ToString();
                            correct = (int)(dacset * 4.0f / output + 0.5f);
                            textBox2.Text = correct.ToString();
                            break;
                        case OUT.UTP05:
                            correct = (int)(dacset * 5.0f / output + 0.5f);
                            textBox1.Text = correct.ToString();
                            textBox2.Text = "0";
                            break;
                        case OUT.UTP10:
                            correct = (int)(dacset * 10.0f / output + 0.5f);
                            textBox1.Text = correct.ToString();
                            textBox2.Text = "0";
                            break;
                        case OUT.UTN05:
                            correct = (int)(((dacset - 524287.0f) * 5.0f / output) + 0.5f);
                            textBox1.Text = (0x7FFFF + correct).ToString();
                            textBox2.Text = (0x7FFFF - correct - 192).ToString();
                            break;
                        case OUT.UTN10:
                            correct = (int)(((dacset - 524287.0f) * 10.0f / output) + 0.5f);
                            textBox1.Text = (0x7FFFF + correct).ToString();
                            textBox2.Text = (0x7FFFF - correct - 192).ToString();
                            break;
                    }

                    //
                    bt_caculate.HoverBackColor = Color.Green;
                    bt_fact_write.HoverBackColor = Color.Firebrick;
                    Refresh();
                }
            }
        }

        //写入
        private void bt_fact_write_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.WRX4;//校准后写入

            //取得校准dac值
            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    actXET.E_da_zero_4ma = Convert.ToInt32(textBox2.Text);
                    actXET.E_da_full_20ma = Convert.ToInt32(textBox1.Text);
                    actXET.T_analog1 = "4.0";
                    actXET.T_analog2 = "8.0";
                    actXET.T_analog3 = "12.0";
                    actXET.T_analog4 = "16.0";
                    actXET.T_analog5 = "20.0";
                    break;
                case OUT.UTP05:
                    actXET.E_da_zero_05V = Convert.ToInt32(textBox2.Text);
                    actXET.E_da_full_05V = Convert.ToInt32(textBox1.Text);
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "1.25";
                    actXET.T_analog3 = "2.5";
                    actXET.T_analog4 = "3.75";
                    actXET.T_analog5 = "5.0";
                    break;
                case OUT.UTP10:
                    actXET.E_da_zero_10V = Convert.ToInt32(textBox2.Text);
                    actXET.E_da_full_10V = Convert.ToInt32(textBox1.Text);
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "2.5";
                    actXET.T_analog3 = "5.0";
                    actXET.T_analog4 = "7.5";
                    actXET.T_analog5 = "10.0";
                    break;
                case OUT.UTN05:
                    actXET.E_da_zero_N5 = Convert.ToInt32(textBox2.Text);
                    actXET.E_da_full_P5 = Convert.ToInt32(textBox1.Text);
                    actXET.T_analog1 = "0.0";
                    actXET.T_analog2 = "1.25";
                    actXET.T_analog3 = "2.5";
                    actXET.T_analog4 = "3.75";
                    actXET.T_analog5 = "5.0";
                    break;
                case OUT.UTN10:
                    actXET.E_da_zero_N10 = Convert.ToInt32(textBox2.Text);
                    actXET.E_da_full_P10 = Convert.ToInt32(textBox1.Text);
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
                textBox3.Text = "";
                bt_caculate.HoverBackColor = button1.HoverBackColor;
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.DONE);
                Thread.Sleep(50);
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteFacTasks();
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

                //
                switch (uiTask)
                {
                    //更新Bohrcode
                    case TASKS.BOR:
                        label13.Text = MyDevice.protocol.trTASK.ToString();
                        //成功
                        uiTask = TASKS.NULL;
                        bt_bohrcode.HoverBackColor = Color.Green;
                        break;

                    //修改outype
                    case TASKS.WRX0:
                        label13.Text = MyDevice.protocol.trTASK.ToString() + " " + MyDevice.protocol.isEqual.ToString() + " " + MyDevice.protocol.rxString;
                        //WRX0 -> RST
                        MyDevice.mePort_WriteTypTasks();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            uiTask = TASKS.NULL;
                            bt_outype.HoverBackColor = Color.Green;
                        }
                        break;

                    //校准后写入
                    case TASKS.WRX4:
                        label13.Text = MyDevice.protocol.trTASK.ToString() + " " + MyDevice.protocol.isEqual.ToString() + " " + MyDevice.protocol.rxString;
                        //BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst) -> NULL
                        MyDevice.mePort_WriteFacTasks();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            uiTask = TASKS.NULL;
                            bt_fact_write.HoverBackColor = Color.Green;
                        }
                        break;

                    default:
                        switch (MyDevice.protocol.trTASK)
                        {
                            //校准满点
                            case TASKS.GOUPF:
                            case TASKS.GODMF:
                                textBox1.Text = MyDevice.protocol.rxString;
                                bt_fact_write.HoverBackColor = Color.Firebrick;
                                break;

                            //校准零点
                            case TASKS.GOUPZ:
                            case TASKS.GODMZ:
                                textBox2.Text = MyDevice.protocol.rxString;
                                bt_fact_write.HoverBackColor = Color.Firebrick;
                                break;

                        }
                        break;
                }

                Refresh();
            }
        }
    }
}
