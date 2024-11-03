using Model;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

//Alvin 20230414
//Lumi 20231222

//重要问题

//有些型号有些设备版本没有SCT5参数,或者只有部分SCT5参数
//测试已经可靠容错

//RS485连接下,修改ID地址后,设备数据迁移,设备状态变更,其它界面下设备操作要正常
//部分实现

//RS485连接下,修改串口波特率等参数后,设备要重启,电脑串口要重开
//部分实现

//RS485连接下,如果总线上有多个设备,如何防止错误修改
//只检查已连接设备是否有重复ID
//未连接设备只能考虑增加检测按钮,探测一下目标地址是否有设备存在

//modbus TCP连接下，可以有重复id
//mMTCP的下标不是E_addr，不需要数据迁移

namespace Base.UI.MenuSet
{
    public partial class MenuParaRS485Form : Form
    {
        private XET actXET;     //需要操作的设备
        private byte newaddr;   //保存SCT初值

        //构造函数
        public MenuParaRS485Form()
        {
            InitializeComponent();
        }

        //加载窗口
        private void MenuParaRS485Form_Load(object sender, EventArgs e)
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
        private void MenuParaRS485Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            if (check_RS485UartChange() && (MyDevice.protocol.type == COMP.RS485))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("RS485总线配置已修改,请重新连接设备");
                }
                else
                {
                    MessageBox.Show("RS485 bus configuration has been modified, please reconnect the device");
                }

                if (MyDevice.protocol.IsOpen)
                {
                    //断开串口时，置位is_serial_closing标记更新
                    MyDevice.protocol.Is_serial_closing = true;

                    //关闭再打开串口
                    MyDevice.mePort_Open(MyDevice.protocol.portName,
                        Convert.ToInt32(comboBox1.Text),
                        (StopBits)(comboBox2.SelectedIndex + 1),
                        (Parity)comboBox3.SelectedIndex);
                }
            }
        }

        //停止位改变
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > 0)
            {
                comboBox3.SelectedIndex = 0;
            }
        }

        //校验位改变
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex > 0)
            {
                comboBox2.SelectedIndex = 0;
            }
        }

        //确定并写入设备
        private void button1_Click(object sender, EventArgs e)
        {
            //是否要迁移数据
            bool isCopy = false;

            //获取新地址
            Int32 dat = Convert.ToInt32(textBox1.Text.Trim());
            if (dat == 0) dat = 1;
            if (dat > 255) dat = 255;
            newaddr = (byte)dat;

            //发送
            if (MyDevice.protocol.IsOpen)
            {
                //改变站号
                if (MyDevice.protocol.addr != newaddr)
                {
                    if (MyDevice.protocol.type == COMP.RS485)
                    {
                        if (MyDevice.mBUS[newaddr].sTATE != STATE.INVALID)//目标站号存在设备
                        {
                            if (MyDevice.languageType == 0)
                            {
                                if (MessageBox.Show("如果总线中存在 " + newaddr.ToString() + " 站点,可能导致总线故障,是否继续修改?", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                if (MessageBox.Show("If the address is changed to " + newaddr.ToString() + " , it may cause a bus failure. Do you want to continue the change?", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                                {
                                    return;
                                }
                            }
                        }
                        actXET.sTATE = STATE.INVALID;//先将当前设备设为INVALID,让总线设备状态和devSum统计正确
                        isCopy = true;
                    }
                }

                //更新其它数据
                if (MyDevice.protocol.type != COMP.ModbusTCP)
                {
                    actXET.E_addr = (Byte)dat;
                }
                actXET.E_baud = (Byte)comboBox1.SelectedIndex;
                actXET.E_stopbit = (Byte)(comboBox2.SelectedIndex + 1);
                actXET.E_parity = (Byte)comboBox3.SelectedIndex;

                //复制数据到目标站号,WRX5时能isEQ通过
                if (isCopy)
                {
                    MyDevice.mBUS_DeepCopy(newaddr, MyDevice.protocol.addr);
                }

                //发送
                button1.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                if (check_RS485UartChange())
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

                //modbus TCP时，校验完bohrcode后再改E_addr
                if ((MyDevice.protocol.trTASK == TASKS.BCC) && MyDevice.protocol.type == COMP.ModbusTCP)
                {
                    actXET.E_addr = newaddr;
                }

                //写入并校验WRX5完成后再修改站号,并标记目标站点设为工作状态
                if (MyDevice.protocol.trTASK == TASKS.WRX5)
                {
                    //修改站号,后续指令能顺利通讯
                    MyDevice.protocol.addr = newaddr;

                    if (MyDevice.protocol.type == COMP.RS485)
                    {
                        //设置成功后,当前设备设为WORKING,让总线设备状态和devSum统计正确
                        MyDevice.mBUS[MyDevice.protocol.addr].sTATE = STATE.WORKING;
                    }
                }

                //继续写
                if (check_RS485UartChange())
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
                    
                    button1.HoverBackColor = Color.Green;

                    //modbus TCP 重启后需要3-4秒时间恢复，不直接关闭窗口
                    if (MyDevice.protocol.type != COMP.ModbusTCP)
                    {
                        this.Close();
                    }
                    else
                    {
                        button1.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    }
                }
                else if ((MyDevice.protocol.trTASK == TASKS.REST) && (MyDevice.mSUT.E_test < 0x58))
                {
                    //老版本重启指令无回复
                    button1.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    button1.HoverBackColor = Color.Green;
                }
            }
        }

        //串口调整
        private bool check_RS485UartChange()
        {
            //true = 需要重启初始化串口
            if (MyDevice.protocol.baudRate != Convert.ToInt32(comboBox1.Text)) return true;
            if ((int)MyDevice.protocol.stopBits != (comboBox2.SelectedIndex + 1)) return true;
            if ((int)MyDevice.protocol.parity != comboBox3.SelectedIndex) return true;
            return false;
        }

        //更新界面
        private void ui_UpdateForm()
        {
            //防错
            if (actXET.E_addr == 0) actXET.E_addr = 1;
            if (actXET.E_addr > 255) actXET.E_addr = 255;
            if (actXET.E_baud >= comboBox1.Items.Count) actXET.E_baud = (Byte)(comboBox1.Items.Count - 1);
            if (actXET.E_stopbit >= comboBox2.Items.Count) actXET.E_stopbit = 0;
            if (actXET.E_parity >= comboBox3.Items.Count) actXET.E_parity = 0;

            //更新界面参数
            textBox1.Text = actXET.E_addr.ToString();
            comboBox1.SelectedIndex = actXET.E_baud;
            comboBox2.SelectedIndex = actXET.E_stopbit - 1;
            comboBox3.SelectedIndex = actXET.E_parity;
        }
    }
}
