using Model;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Base.UI.MenuSet
{
    public partial class MenuSetCorrForm : Form
    {
        private XET actXET;//需要操作的设备
        private string zero;//标定零点
        private string full;//标定满点

        public MenuSetCorrForm()
        {
            InitializeComponent();
        }

        private void MenuSetCorrForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            actXET = MyDevice.actDev;

            ui_UpdateForm();
        }

        private void MenuSetCorrForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            double analog1;
            double analog5;
            double measure;

            //多点修正方法
            //将页面调整为多点的输入框
            //根据修正的区间来计算ad_point值
            //再更新斜率
            if (actXET.E_curve != (byte)ECVE.CTWOPT)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("只做两点标定的满点修正");
                }
                else
                {
                    MessageBox.Show("Only two-point calibration can use this function");
                }
                return;
            }

            //(E_ad_point1, E_da_point1, E_analog1)
            //(E_ad_point5, E_da_point5, E_analog5)
            //实际analog和标定E_analog5有误差
            //用ad_point和analog直线计算E_ad_point5
            //再ad_point和da_point直线计算和修正E_vtio
            analog1 = Convert.ToDouble(actXET.T_analog1);
            analog5 = Convert.ToDouble(actXET.T_analog5);
            measure = Convert.ToDouble(textBox1.Text.Trim());

            //更新E_ad_point5和E_ad_full和E_input5
            actXET.E_ad_full = (int)((measure - analog1) * (actXET.E_ad_point5 - actXET.E_ad_point1) / (analog5 - analog1) + actXET.E_ad_point1);
            actXET.E_ad_point5 = actXET.E_ad_full;
            actXET.T_input5 = (actXET.E_ad_point5 / actXET.S_MVDV).ToString();

            //更新斜率
            actXET.RefreshRatio();

            //写入
            if (MyDevice.protocol.IsOpen)
            {
                bt_ok.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteCalTasks();
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
                MyDevice.mePort_WriteCalTasks();

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

        //更新界面
        private void ui_UpdateForm()
        {
            //零点和满点
            zero = actXET.T_analog1;
            if (actXET.S_ElevenType)
            {
                full = actXET.T_analog11;
            }
            else
            {
                full = actXET.T_analog5;
            }

            //界面单位
            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                    label2.Text = "mA";
                    break;

                case OUT.UTP05:
                case OUT.UTP10:
                case OUT.UTN05:
                case OUT.UTN10:
                    label2.Text = "V";
                    break;

                case OUT.UMASK:
                    switch (actXET.E_wt_unit)
                    {
                        case 0: label2.Text = ""; break;
                        case 1: label2.Text = "kg"; break;
                        case 2: label2.Text = "lb"; break;
                        case 3: label2.Text = "oz"; break;
                        case 4: label2.Text = "g"; break;
                        case 5: label2.Text = "mg"; break;
                        case 6: label2.Text = "t"; break;
                        case 7: label2.Text = "ct"; break;
                        case 8: label2.Text = "N"; break;
                        case 9: label2.Text = "kN"; break;
                        case 10: label2.Text = "N·m"; break;
                        case 11: label2.Text = "lbf·in"; break;
                        case 12: label2.Text = "lbf·ft"; break;
                        case 13: label2.Text = "kgf·cm"; break;
                        case 14: label2.Text = "kgf·m"; break;
                        case 15: label2.Text = "mV/V"; break;
                        case 16: label2.Text = "内码"; break;
                        case 17: label2.Text = "其它"; break;
                    }
                    break;
            }

            //修正的数据
            label5.Text = full + label2.Text;
        }
    }
}
