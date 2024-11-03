using Library;
using Model;
using System;
using System.Drawing;
using System.Windows.Forms;

//Alvin 20230414
//Lumi 20240102

//重要问题

//有些型号有些设备版本没有SCT5参数,或者只有部分SCT5参数
//测试已经可靠容错

//CANopen连接下,修改ID地址后,设备数据迁移,设备状态变更,其它界面下设备操作要正常
//未测试

//CANopen连接下,修改节点波特率等参数后,设备要重启,电脑串口要重开
//未测试

//CANopen连接下,如果总线上有多个设备,如何防止错误修改
//只检查已连接设备是否有重复ID
//未连接设备只能考虑增加检测按钮,探测一下目标地址是否有设备存在

namespace Base.UI.MenuSet
{
    public partial class MenuParaCANopenForm : Form
    {
        private XET actXET;     //需要操作的设备
        private byte newaddr;   //保存SCT初值

        //构造函数
        public MenuParaCANopenForm()
        {
            InitializeComponent();
        }

        //加载窗口
        private void MenuParaCANopenForm_Load(object sender, EventArgs e)
        {
            //加载接收触发
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //
            actXET = MyDevice.actDev;

            //错误信息提示
            label13.Text = "";
            label13.ForeColor = Color.Firebrick;

            //初始化更新界面
            ui_UpdateForm();
        }

        //释放窗口
        private void MenuParaCANopenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            //
            if ((button1.Text == "成功" || button1.Text == "Success") && (MyDevice.protocol.type == COMP.CANopen))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("CANopen总线配置已修改,请重新连接设备");
                }
                else
                {
                    MessageBox.Show("CANopen bus configuration has been modified, please reconnect the device");
                }

                if (MyDevice.protocol.IsOpen)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.protocol.Is_serial_closing = true;

                    //关闭再打开串口
                    MyDevice.myCANopen.Protocol_PortOpen(0, "CH" + (MyDevice.myCANopen.channel + 1), Convert.ToInt32(comboBox1.Text.Replace(" kbps", "")));
                }
            }
        }

        #region textBox限制

        //SCT时间
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入正整数,长度限制5
            BoxRestrict.KeyPress_IntegerPositive_len5(sender, e);
        }

        //COB ID
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入十六进制数,长度限制3
            BoxRestrict.KeyPress_HEX_len3(sender, e);
        }

        //超载百分比报警
        private void textBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入正整数,长度限制3
            BoxRestrict.KeyPress_IntegerPositive_len3(sender, e);
        }

        //目标报警值、区间低报警值、区间高报警值
        private void alarmValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入有理数
            BoxRestrict.KeyPress_RationalNumber(sender, e);
        }

        //鼠标点击消除小数点准备输入
        private void alarmValue_MouseClick(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Text = Convert.ToDouble(textBox.Text).ToString();
        }

        //离开数字量自动加小数点
        private void alarmValue_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Text = Convert.ToDouble(textBox.Text).ToString("f" + actXET.E_wt_decimal);
        }

        #endregion

        //同步和异步模式切换
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                label10.Text = MyDevice.languageType == 0 ? "同步模式接收SYNC个数(1-240)" : "SYNC start value(1-240)";
                if (actXET.E_typeTPDO0 > 240)
                {
                    textBox4.Text = "240";
                }
                else
                {
                    textBox4.Text = actXET.E_typeTPDO0.ToString();
                }
            }
            else
            {
                label10.Text = MyDevice.languageType == 0 ? "异步模式时间间隔(1-65535ms)" : "Event timer(1-65535ms)";
                textBox4.Text = actXET.E_evenTPDO0.ToString();
            }
        }

        //确定并写入设备 CANopen通讯参数
        private void button1_Click(object sender, EventArgs e)
        {
            //是否要迁移数据
            bool isCopy = false;

            //获取新地址
            Int32 dat = Convert.ToInt32(textBox1.Text.Trim());
            if (dat == 0) dat = 1;
            if (dat > 127) dat = 127;
            newaddr = (byte)dat;

            //校验safety参数
            if (!restrict_safetyParaInput()) return;

            //发送
            if (MyDevice.protocol.IsOpen)
            {
                //通讯参数
                //改变站号
                if (MyDevice.protocol.addr != newaddr)
                {
                    if (MyDevice.protocol.type == COMP.CANopen)
                    {
                        if (MyDevice.mCAN[newaddr].sTATE != STATE.INVALID)//目标站号存在设备
                        {
                            if (MyDevice.languageType == 0)
                            {
                                if (MessageBox.Show("如果总线中存在 " + newaddr.ToString() + " 节点,可能导致总线故障,是否继续修改?", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                if (MessageBox.Show("If the node ID is changed to " + newaddr.ToString() + " , it may cause a bus failure. Do you want to continue the change?", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                                {
                                    return;
                                }
                            }
                        }
                        actXET.sTATE = STATE.INVALID;//先将当前设备设为INVALID,让总线设备状态和devSum统计正确
                        isCopy = true;
                    }
                }

                //更新其它数据zhoup
                actXET.E_nodeID = (Byte)dat;
                actXET.E_nodeBaud = (Byte)comboBox1.SelectedIndex;
                actXET.E_heartBeat = Convert.ToUInt16(textBox3.Text);
                if (comboBox3.SelectedIndex == 0)
                {
                    //同步模式
                    dat = Convert.ToInt32(textBox4.Text.Trim());
                    if (dat > 240) dat = 240;
                    actXET.E_typeTPDO0 = (Byte)dat;
                }
                else
                {
                    //异步模式
                    dat = Convert.ToInt32(textBox4.Text.Trim());
                    if (dat > 65535) dat = 65535;
                    actXET.E_typeTPDO0 = 0xFE;
                    actXET.E_evenTPDO0 = (UInt16)dat;
                }

                //防错
                if (actXET.E_nodeID == 0) actXET.E_nodeID = 1;
                if (actXET.E_nodeID > 127) actXET.E_nodeID = 127;
                if (actXET.E_heartBeat == 0) actXET.E_heartBeat = 1;//1-65535
                if (actXET.E_typeTPDO0 == 0) actXET.E_typeTPDO0 = 1;
                if (actXET.E_typeTPDO0 > 0xF0) actXET.E_typeTPDO0 = 0xFE;//0x01—0xF0,0xFE
                if (actXET.E_evenTPDO0 == 0) actXET.E_evenTPDO0 = 1;//1-65535

                //safety参数
                actXET.E_enGFC = (byte)comboBox4.SelectedIndex;
                actXET.E_enSRDO = (byte)comboBox5.SelectedIndex;
                actXET.E_SCT_time = Convert.ToUInt16(textBox5.Text);
                actXET.E_COB_ID1 = Convert.ToUInt16(textBox6.Text, 16);
                actXET.E_COB_ID2 = Convert.ToUInt16(textBox7.Text, 16);
                actXET.E_enOL = (byte)comboBox6.SelectedIndex;
                actXET.E_overload = Convert.ToByte(textBox8.Text);
                actXET.E_alarmMode = (byte)comboBox7.SelectedIndex;
                actXET.T_wetTarget = textBox9.Text;
                actXET.T_wetLow = textBox10.Text;
                actXET.T_wetHigh = textBox11.Text;

                //复制数据到目标站号,WRX5时能isEQ通过
                if (isCopy)
                {
                    MyDevice.mCAN_DeepCopy(newaddr, MyDevice.protocol.addr);
                }

                //发送
                button1.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                if (check_CANopenChange())
                {
                    MyDevice.mePort_WriteBusTasks();
                }
                else
                {
                    MyDevice.mePort_WriteParTasks();
                }
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
                    this.BeginInvoke(meDelegate, new object[] { });
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

                //页面
                ui_UpdateForm();

                //写入并校验WRX5完成后再修改站号,并标记目标站点设为工作状态
                if ((MyDevice.protocol.type == COMP.CANopen) && (MyDevice.protocol.trTASK == TASKS.WRX5))
                {
                    //修改站号,后续指令能顺利通讯
                    MyDevice.protocol.addr = newaddr;

                    //设置成功后,当前设备设为WORKING,让总线设备状态和devSum统计正确
                    MyDevice.mCAN[MyDevice.protocol.addr].sTATE = STATE.WORKING;
                }

                //继续写
                if (check_CANopenChange())
                {
                    MyDevice.mePort_WriteBusTasks();
                }
                else
                {
                    MyDevice.mePort_WriteParTasks();
                }

                //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                button1.Text = MyDevice.protocol.trTASK.ToString();
                label13.Text = MyDevice.protocol.rxString;

                //写完了
                if (MyDevice.protocol.trTASK == TASKS.NULL)
                {
                    button1.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    button1.HoverBackColor = Color.Green;
                    this.Close();
                }
                else if ((MyDevice.protocol.trTASK == TASKS.REST) && (MyDevice.mSUT.E_test < 0x58))
                {
                    //老版本重启指令无回复
                    button1.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    button1.HoverBackColor = Color.Green;
                }
            }
        }

        //接口调整
        private bool check_CANopenChange()
        {
            //true = 需要重启初始化CAN
            if (MyDevice.protocol.baudRate == Convert.ToInt32(comboBox1.Text.Replace(" kbps", ""))) return false;
            return true;
        }

        //更新界面
        private void ui_UpdateForm()
        {
            if (actXET.E_test > 0x58)
            {
                groupBox1.Enabled = true;
            }
            else //老版本不能使用修改safety参数功能
            {
                groupBox1.Enabled = false;
            }

            //防错
            //通讯参数
            if (actXET.E_nodeID == 0) actXET.E_nodeID = 1;
            if (actXET.E_nodeID > 127) actXET.E_nodeID = 127;
            if (actXET.E_nodeBaud >= comboBox1.Items.Count) actXET.E_nodeBaud = (Byte)(comboBox1.Items.Count - 1);
            if (actXET.E_heartBeat == 0) actXET.E_heartBeat = 1;//1-65535
            if (actXET.E_typeTPDO0 == 0) actXET.E_typeTPDO0 = 1;
            if (actXET.E_typeTPDO0 > 0xF0) actXET.E_typeTPDO0 = 0xFE;//0x01—0xF0,0xFE
            if (actXET.E_evenTPDO0 == 0) actXET.E_evenTPDO0 = 1;//1-65535

            //safety参数
            if (actXET.E_SCT_time < 1 || actXET.E_SCT_time > 65535) actXET.E_SCT_time = 0x19;//1-65535
            if (!IsOdd(actXET.E_COB_ID1)) actXET.E_COB_ID1 = 0x109;//仅奇数
            if (actXET.E_COB_ID1 < 0x101 || actXET.E_COB_ID1 > 0x13F) actXET.E_COB_ID1 = 0x109;//0x101—0x13F
            if (IsOdd(actXET.E_COB_ID2)) actXET.E_COB_ID2 = 0x10A;//仅偶数
            if (actXET.E_COB_ID2 < 0x102 || actXET.E_COB_ID2 > 0x140) actXET.E_COB_ID2 = 0x10A;//0x102—0x140
            if (actXET.E_overload < 100 || actXET.E_overload > 200) actXET.E_overload = 120;//100-200

            //更新界面参数
            //通讯参数
            textBox1.Text = actXET.E_nodeID.ToString();
            comboBox1.SelectedIndex = actXET.E_nodeBaud;
            textBox3.Text = actXET.E_heartBeat.ToString();
            if (actXET.E_typeTPDO0 != 0xFE)
            {
                comboBox3.SelectedIndex = 0;//同步模式
            }
            else
            {
                comboBox3.SelectedIndex = 1;//异步模式
            }
            textBox4.Text = actXET.T_timeTPDO;

            //safety参数
            comboBox4.SelectedIndex = actXET.E_enGFC;
            comboBox5.SelectedIndex = actXET.E_enSRDO;
            textBox5.Text = actXET.E_SCT_time.ToString();
            textBox6.Text = actXET.E_COB_ID1.ToString("X2");
            textBox7.Text = actXET.E_COB_ID2.ToString("X2");
            comboBox6.SelectedIndex = actXET.E_enOL;
            textBox8.Text = actXET.E_overload.ToString();
            comboBox7.SelectedIndex = actXET.E_alarmMode;
            textBox9.Text = actXET.T_wetTarget;
            textBox10.Text = actXET.T_wetLow;
            textBox11.Text = actXET.T_wetHigh;
        }

        //校验Safety参数
        private bool restrict_safetyParaInput()
        {
            //防错
            if (int.Parse(textBox5.Text) < 1 || int.Parse(textBox5.Text) > 65535)
            {
                label13.Text = "SCT时间需要在范围1-65535";
                return false;
            }
            else if (!IsOdd(Convert.ToUInt16(textBox6.Text, 16)))
            {
                label13.Text = "COB-ID 1只能是奇数";
                return false;
            }
            else if (Convert.ToUInt16(textBox6.Text, 16) < 0x101 || Convert.ToUInt16(textBox6.Text) > 0x13F)
            {
                label13.Text = "COB-ID 1需要在范围0x101-0x13F";
                return false;
            }
            else if (IsOdd(Convert.ToUInt16(textBox7.Text, 16)))
            {
                label13.Text = "COB-ID 1只能是偶数";
                return false;
            }
            else if (Convert.ToUInt16(textBox7.Text, 16) < 0x102 || Convert.ToUInt16(textBox7.Text, 16) > 0x140)
            {
                label13.Text = "COB-ID 2需要在范围0x102—0x140";
                return false;
            }
            else if (int.Parse(textBox8.Text) < 100 || int.Parse(textBox8.Text) > 200)
            {
                label13.Text = "超载百分比报警需要在范围100-200";
                return false;
            }
            else if (double.Parse(textBox9.Text) < 0 || double.Parse(textBox9.Text) > 60000)
            {
                label13.Text = "目标报警值需要在范围0-60000";
                return false;
            }
            else if (double.Parse(textBox10.Text) < 0 || double.Parse(textBox10.Text) > 60000)
            {
                label13.Text = "区间低报警值需要在范围0-60000";
                return false;
            }
            else if (double.Parse(textBox11.Text) < 0 || double.Parse(textBox11.Text) > 60000)
            {
                label13.Text = "区间高报警值需要在范围0-60000";
                return false;
            }
            else
            {
                label13.Text = "";
                return true;
            }
        }

        //奇偶数判断
        public static bool IsOdd(ushort n)
        {
            return (n % 2 == 1) ? true : false;
        }
    }
}
