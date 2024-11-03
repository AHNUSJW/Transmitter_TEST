using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//Lumi 20240311

//暂只开放通信信道
//其他设置visible = false

namespace Base.UI.MenuSetCommunication
{
    public partial class MenuParaWirelessForm : Form
    {
        private XET actXET;     //需操作的设备

        public MenuParaWirelessForm()
        {
            InitializeComponent();
        }

        //窗体加载
        private void MenuParaWirelessForm_Load(object sender, EventArgs e)
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

        //窗体关闭
        private void MenuParaWirelessForm_FormClosing(object sender, FormClosingEventArgs e)
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

        //校验ADDL
        private void textBox1_ADDL_Leave(object sender, EventArgs e)
        {
            if (tb_ADDL.Text == "")
            {
                tb_ADDL.Text = actXET.E_addrRF[1].ToString();
                return;
            }

            if (Convert.ToUInt16(tb_ADDL.Text) < 1 || Convert.ToUInt16(tb_ADDL.Text) > 255)
            {
                MessageBox.Show("接收器ID不得超出 1 - 255 的范围");
                return;
            }
        }

        //确认
        private void buttonX1_Click(object sender, EventArgs e)
        {
            //发送
            if (MyDevice.protocol.IsOpen)
            {
                byte parity = Convert.ToByte(cb_rfCheck.Source[cb_rfCheck.SelectedIndex].Key);
                byte baudrate = Convert.ToByte(cb_rfBaud.Source[cb_rfBaud.SelectedIndex].Key);
                byte airrate = Convert.ToByte(cb_rfRate.Source[cb_rfRate.SelectedIndex].Key);
                byte channel = Convert.ToByte(cb_rfChan.Source[cb_rfChan.SelectedIndex].Key, 16);

                //更新参数
                actXET.E_addrRF[1] = Convert.ToByte(tb_ADDL.Text);
                if (actXET.S_parityRF != parity) //校验位修改
                {
                    actXET.E_spedRF = (byte)((actXET.E_spedRF & 0b00111111) | (parity << 6));
                }
                if (actXET.S_baudrateRF != Convert.ToInt16(baudrate)) //波特率位修改
                {
                    actXET.E_spedRF = (byte)((actXET.E_spedRF & 0b11000111) | (baudrate << 3));
                }
                if (actXET.S_baudrateRF != Convert.ToInt16(airrate)) //空中速率修改
                {
                    actXET.E_spedRF = (byte)((actXET.E_spedRF & 0b11111000) | (airrate));
                }
                if (actXET.S_channelRF != channel) //通信信道修改
                {
                    actXET.E_chanRF = (byte)((actXET.E_chanRF & 0b11100000) | (channel));
                }

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

        //更新界面参数
        private void ui_UpdateForm()
        {
            //校验位
            cb_rfCheck.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("00", "8N1"),
                new KeyValuePair<string, string>("01", "8O1"),
                new KeyValuePair<string, string>("10", "8E1")
            };

            //串口速率（波特率）
            cb_rfBaud.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("000", "1200"),
                new KeyValuePair<string, string>("001", "2400"),
                new KeyValuePair<string, string>("010", "4800"),
                new KeyValuePair<string, string>("011", "9600"),
                new KeyValuePair<string, string>("100", "19200"),
                new KeyValuePair<string, string>("101", "38400"),
                new KeyValuePair<string, string>("110", "57600"),
                new KeyValuePair<string, string>("111", "115200")
            };

            //空中速率
            cb_rfRate.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("000", "2.5k"),
                new KeyValuePair<string, string>("001", "5k"),
                new KeyValuePair<string, string>("010", "12k"),
                new KeyValuePair<string, string>("011", "28k"),
                new KeyValuePair<string, string>("100", "64k"),
                new KeyValuePair<string, string>("101", "168k")  //110 111也是168k
            };

            //通信信道
            cb_rfChan.Source = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("0x00", "431"),
                new KeyValuePair<string, string>("0x01", "431.5"),
                new KeyValuePair<string, string>("0x02", "432"),
                new KeyValuePair<string, string>("0x03", "432.5"),
                new KeyValuePair<string, string>("0x04", "433"),
                new KeyValuePair<string, string>("0x05", "433.5"),
                new KeyValuePair<string, string>("0x06", "434"),
                new KeyValuePair<string, string>("0x07", "434.5"),
                new KeyValuePair<string, string>("0x08", "435"),
                new KeyValuePair<string, string>("0x09", "435.5"),
                new KeyValuePair<string, string>("0x0A", "436"),
                new KeyValuePair<string, string>("0x0B", "436.5"),
                new KeyValuePair<string, string>("0x0C", "437"),
                new KeyValuePair<string, string>("0x0D", "437.5"),
                new KeyValuePair<string, string>("0x0E", "438"),
                new KeyValuePair<string, string>("0x0F", "438.5"),
                new KeyValuePair<string, string>("0x10", "439"),
                new KeyValuePair<string, string>("0x11", "439.5"),
                new KeyValuePair<string, string>("0x12", "440"),
                new KeyValuePair<string, string>("0x13", "440.5"),
                new KeyValuePair<string, string>("0x14", "441"),
                new KeyValuePair<string, string>("0x15", "441.5"),
                new KeyValuePair<string, string>("0x16", "442"),
                new KeyValuePair<string, string>("0x17", "442.5"),
                new KeyValuePair<string, string>("0x18", "443"),
                new KeyValuePair<string, string>("0x19", "443.5"),
                new KeyValuePair<string, string>("0x1A", "444"),
                new KeyValuePair<string, string>("0x1B", "444.5"),
                new KeyValuePair<string, string>("0x1C", "445"),
                new KeyValuePair<string, string>("0x1D", "445.5"),
                new KeyValuePair<string, string>("0x1E", "446"),
                new KeyValuePair<string, string>("0x1F", "446.5"),
            };

            //ADDL
            tb_ADDL.Text = actXET.E_addrRF[1].ToString();

            //校验位
            cb_rfCheck.SelectedIndex = 0;
            for (int i = 0; i < cb_rfCheck.Source.Count; i++)
            {
                if (Convert.ToByte(cb_rfCheck.Source[i].Key, 2) == actXET.S_parityRF)
                {
                    cb_rfCheck.SelectedIndex = i;
                    break;
                }
            }

            //波特率
            cb_rfBaud.SelectedIndex = 0;
            for (int i = 0; i < cb_rfBaud.Source.Count; i++)
            {
                if (Convert.ToByte(cb_rfBaud.Source[i].Key, 2) == actXET.S_baudrateRF)
                {
                    cb_rfBaud.SelectedIndex = i;
                    break;
                }
            }

            //空中速率
            cb_rfRate.SelectedIndex = 0;
            for (int i = 0; i < cb_rfRate.Source.Count; i++)
            {
                if (Convert.ToByte(cb_rfRate.Source[i].Key, 2) == actXET.S_airrateRF)
                {
                    cb_rfRate.SelectedIndex = i;
                    break;
                }
            }
            if (actXET.S_airrateRF == 0b110 || actXET.S_airrateRF == 0b111)  //110 111也是168k
            {
                cb_rfRate.SelectedIndex = cb_rfRate.Source.Count - 1;
            }

            //通信信道
            cb_rfChan.SelectedIndex = 0;
            for (int i = 0; i < cb_rfChan.Source.Count; i++)
            {
                if (Convert.ToByte(cb_rfChan.Source[i].Key, 16) == actXET.S_channelRF)
                {
                    cb_rfChan.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}
