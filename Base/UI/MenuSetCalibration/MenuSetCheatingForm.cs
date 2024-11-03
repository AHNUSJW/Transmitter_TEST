using Model;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Base.UI.MenuSet
{
    public partial class MenuSetCheatingForm : Form
    {
        private XET actXET;//需要操作的设备

        public MenuSetCheatingForm()
        {
            InitializeComponent();
        }

        private void MenuSetCheatingForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            actXET = MyDevice.actDev;

            comboBox1.SelectedIndex = actXET.E_cheatype;
            comboBox2.SelectedIndex = actXET.E_thmax - 1;//外阈值1-9
            comboBox3.SelectedIndex = actXET.E_thmin;//內阈值0-8
        }

        private void MenuSetCheatingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //内阈值不能大于外阈值
            if (comboBox3.SelectedIndex >= comboBox2.SelectedIndex)
            {
                comboBox3.SelectedIndex = comboBox2.SelectedIndex;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //外阈值不能小于内阈值
            if (comboBox2.SelectedIndex <= comboBox3.SelectedIndex)
            {
                comboBox2.SelectedIndex = comboBox3.SelectedIndex;
            }
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            //更新
            actXET.E_cheatype = (Byte)comboBox1.SelectedIndex;
            actXET.E_thmax = Convert.ToByte(comboBox2.Text);
            actXET.E_thmin = Convert.ToByte(comboBox3.Text);

            //写入
            if (MyDevice.protocol.IsOpen)
            {
                bt_ok.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteParTasks();
            }
        }

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

                //继续写
                MyDevice.mePort_WriteParTasks();

                //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                bt_ok.Text = MyDevice.protocol.trTASK.ToString();

                //写完了
                if (MyDevice.protocol.trTASK == TASKS.NULL)
                {
                    bt_ok.HoverBackColor = Color.Green;
                    this.Close();
                }
                else if ((MyDevice.protocol.trTASK == TASKS.REST) && (MyDevice.mSUT.E_test < 0x58))
                {
                    //老版本重启指令无回复
                    bt_ok.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    bt_ok.HoverBackColor = Color.Green;
                }
            }
        }
    }
}