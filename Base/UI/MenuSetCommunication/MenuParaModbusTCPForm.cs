using Library;
using Model;
using System;
using System.Drawing;
using System.Windows.Forms;

//Lumi 20240314

namespace Base.UI.MenuSet
{
    public partial class MenuParaModbusTCPForm : Form
    {
        private XET actXET;     //需要操作的设备

        public MenuParaModbusTCPForm()
        {
            InitializeComponent();
        }

        //加载窗口
        private void MenuParaModbusTCPForm_Load(object sender, EventArgs e)
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
        private void MenuParaModbusTCPForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            //
            if ((button1.Text == "成功" || button1.Text == "Success") && (MyDevice.protocol.type == COMP.ModbusTCP))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("Modbus TCP配置已修改,请重新连接设备");
                }
                else
                {
                    MessageBox.Show("Modbus TCP configuration has been modified, please reconnect the device");
                }
            }
        }

        //端口号
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入正整数,长度限制5
            BoxRestrict.KeyPress_IntegerPositive_len5(sender, e);
        }

        //DHCP修改
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DHCP_SelectedIndexChanged();
        }

        //DHCP修改
        private void DHCP_SelectedIndexChanged()
        {
            //DHCP 关闭时，开放设置设备ip地址、设备网关地址、设备子网掩码
            if (comboBox1.SelectedIndex == 0)
            {
                ipAddrTextbox2.Enabled = true;
                ipAddrTextbox3.Enabled = true;
                ipAddrTextbox4.Enabled = true;
            }
            else
            {
                ipAddrTextbox2.Enabled = false;
                ipAddrTextbox3.Enabled = false;
                ipAddrTextbox4.Enabled = false;
            }
        }

        //Scan修改
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Scan_SelectedIndexChanged();
        }

        //Scan修改
        private void Scan_SelectedIndexChanged()
        {
            //scan 关闭时，开放设置设置主机ip地址
            if (comboBox2.SelectedIndex == 0)
            {
                ipAddrTextbox1.Enabled = true;
            }
            else
            {
                ipAddrTextbox1.Enabled = false;
            }
        }

        //确定键
        private void button1_Click(object sender, EventArgs e)
        {
            //校验参数
            if (!restrict_ParaInput()) return;

            //发送
            if (MyDevice.protocol.IsOpen)
            {
                //更新参数
                actXET.E_netServicePort = Convert.ToUInt16(textBox1.Text);
                byte[] ipArray = actXET.GetIpAddressFromString(ipAddrTextbox1.IPAddrStr);
                actXET.E_netServiceIP[0] = ipArray[0];
                actXET.E_netServiceIP[1] = ipArray[1];
                actXET.E_netServiceIP[2] = ipArray[2];
                actXET.E_netServiceIP[3] = ipArray[3];
                ipArray = actXET.GetIpAddressFromString(ipAddrTextbox2.IPAddrStr);
                actXET.E_netClientIP[0] = ipArray[0];
                actXET.E_netClientIP[1] = ipArray[1];
                actXET.E_netClientIP[2] = ipArray[2];
                actXET.E_netClientIP[3] = ipArray[3];
                ipArray = actXET.GetIpAddressFromString(ipAddrTextbox3.IPAddrStr);
                actXET.E_netGatIP[0] = ipArray[0];
                actXET.E_netGatIP[1] = ipArray[1];
                actXET.E_netGatIP[2] = ipArray[2];
                actXET.E_netGatIP[3] = ipArray[3];
                ipArray = actXET.GetIpAddressFromString(ipAddrTextbox4.IPAddrStr);
                actXET.E_netMaskIP[0] = ipArray[0];
                actXET.E_netMaskIP[1] = ipArray[1];
                actXET.E_netMaskIP[2] = ipArray[2];
                actXET.E_netMaskIP[3] = ipArray[3];
                actXET.E_useDHCP = (byte)comboBox1.SelectedIndex;
                actXET.E_useScan = (byte)comboBox2.SelectedIndex;

                //发送
                button1.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteParTasks();
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

                //继续写
                MyDevice.mePort_WriteParTasks();

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
            }
        }

        //更新界面
        private void ui_UpdateForm()
        {
            //更新界面参数
            textBox1.Text = actXET.E_netServicePort.ToString();
            ipAddrTextbox1.SetIPAddrStr(actXET.GetIpAddressFromArray(actXET.E_netServiceIP));
            ipAddrTextbox2.SetIPAddrStr(actXET.GetIpAddressFromArray(actXET.E_netClientIP));
            ipAddrTextbox3.SetIPAddrStr(actXET.GetIpAddressFromArray(actXET.E_netGatIP));
            ipAddrTextbox4.SetIPAddrStr(actXET.GetIpAddressFromArray(actXET.E_netMaskIP));
            comboBox1.SelectedIndex = actXET.E_useDHCP;
            comboBox2.SelectedIndex = actXET.E_useScan;


            DHCP_SelectedIndexChanged();
            Scan_SelectedIndexChanged();
        }

        //校验参数
        private bool restrict_ParaInput()
        {
            //防错
            if (int.Parse(textBox1.Text) < 1 || int.Parse(textBox1.Text) > 65535)
            {
                label13.Text = "主机端口号填写有误";
                return false;
            }
            else
            {
                label13.Text = "";
                return true;
            }
        }
    }
}
