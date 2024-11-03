using Library;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//Junzhe 20231124
//Lumi 20240228

namespace Base.UI.MenuToolConfig
{
    public partial class MenuToolEepromCfg : Form
    {
        private XET actXET;//需要操作的设备

        private Point label5_point;//存储控件点位信息
        private Point label10_point;
        private Point label22_point;
        private Point label23_point;
        private Point textBox5_point;
        private Point textBox10_point;

        public MenuToolEepromCfg()
        {
            InitializeComponent();
        }

        //加载
        private void MenuToolEepromCfg_Load(object sender, EventArgs e)
        {
            //委托
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //限制输入
            textBox1.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox2.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox3.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox4.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox5.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox6.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox7.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox8.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox9.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox10.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            textBox13.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive);

            //需要操作的设备
            actXET = MyDevice.actDev;

            //存储控件位置
            label5_point = label5.Location;
            label10_point = label10.Location;
            label22_point = label22.Location;
            label23_point = label23.Location;
            textBox5_point = textBox5.Location;
            textBox10_point = textBox10.Location;
        }

        //关闭
        private void MenuToolEepromCfg_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消接收触发
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        //读出参数
        private void buttonX1_Click(object sender, EventArgs e)
        {
            buttonX1.HoverBackColor = Color.Firebrick;
            MyDevice.protocol.Protocol_SendCOM(TASKS.REPRM);
        }

        //写入参数
        private void buttonX2_Click(object sender, EventArgs e)
        {
            //防呆
            if (buttonX1.HoverBackColor != Color.Green)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先读出参数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                else
                {
                    MessageBox.Show("Read out the parameters first!", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            if (textBox1.Text == "" ||
                textBox2.Text == "" ||
                textBox3.Text == "" ||
                textBox4.Text == "" ||
                textBox5.Text == "" ||
                textBox6.Text == "" ||
                textBox7.Text == "" ||
                textBox8.Text == "" ||
                textBox9.Text == "" ||
                textBox10.Text == "" ||
                textBox11.Text == "" ||
                textBox12.Text == "" ||
                textBox13.Text == "")
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("参数不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); return;
                }
                else
                {
                    MessageBox.Show("Parameter cannot be empty!", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }

            //参数
            actXET.Ep_input1 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox1.Text));
            actXET.Ep_input2 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox2.Text));
            actXET.Ep_input3 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox3.Text));
            actXET.Ep_input4 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox4.Text));
            actXET.Ep_input5 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox5.Text));
            actXET.Ep_analog1 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox6.Text));
            actXET.Ep_analog2 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox7.Text));
            actXET.Ep_analog3 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox8.Text));
            actXET.Ep_analog4 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox9.Text));
            actXET.Ep_analog5 = MyDevice.myUIT.ConvertFloatToInt32((float)Convert.ToDouble(textBox10.Text));
            if (Byte.TryParse(textBox11.Text, out Byte version))
            {
                actXET.Ep_version = version;
            }
            else
            {
                actXET.Ep_version = 6; //固定为6
            }

            actXET.Ep_wt_decimal = (Byte)comboBox1.SelectedIndex;
            actXET.Ep_wt_full = (int)(Convert.ToDouble(textBox13.Text) * Math.Pow(10, actXET.Ep_wt_decimal));
            actXET.Ep_wt_unit = (Byte)comboBox2.SelectedIndex;
            actXET.Ep_curve = (Byte)(comboBox3.SelectedIndex + 2);

            //编号
            string[] text_HEX = MyDevice.myUIT.ConvertAsciiToHex(textBox12.Text).Split(' ');
            for (int i = 0; i < text_HEX.Length; i++)
            {
                actXET.Ep_text[i] = Convert.ToByte(text_HEX[i]);
            }
            for (int i = text_HEX.Length; i < 40; i++)
            {
                actXET.Ep_text[i] = 0;
            }

            //写入
            buttonX2.HoverBackColor = Color.Firebrick;
            MyDevice.protocol.Protocol_SendEeprom();
        }

        //更新Eeprom参数
        private void update_EepromPara()
        {
            //灵敏度
            textBox1.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_input1).ToString();
            textBox2.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_input2).ToString();
            textBox3.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_input3).ToString();
            textBox4.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_input4).ToString();
            textBox5.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_input5).ToString();
            //输出
            textBox6.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_analog1).ToString();
            textBox7.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_analog2).ToString();
            textBox8.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_analog3).ToString();
            textBox9.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_analog4).ToString();
            textBox10.Text = MyDevice.myUIT.ConvertInt32ToFloat(actXET.Ep_analog5).ToString();
            //版本
            textBox11.Text = actXET.Ep_version.ToString();
            //满点
            textBox13.Text = (actXET.Ep_wt_full / Math.Pow(10, actXET.Ep_wt_decimal)).ToString("f" + actXET.Ep_wt_decimal).ToString();

            //ASCII
            string item_ASCII;
            List<string> text_ASCII = new List<string>();
            foreach (byte item in actXET.Ep_text)
            {
                if (item != 0)
                {
                    item_ASCII = MyDevice.myUIT.ConvertHexToAscii(item.ToString("X2"));
                    text_ASCII.Add(item_ASCII);
                }
            }

            //编码
            textBox12.Text = string.Join("", text_ASCII);

            //小数点
            switch (actXET.Ep_wt_decimal)
            {
                default:
                case 0:
                    comboBox1.SelectedIndex = 0;
                    break;
                case 1:
                    comboBox1.SelectedIndex = 1;
                    break;
                case 2:
                    comboBox1.SelectedIndex = 2;
                    break;
                case 3:
                    comboBox1.SelectedIndex = 3;
                    break;
                case 4:
                    comboBox1.SelectedIndex = 4;
                    break;
                case 5:
                    comboBox1.SelectedIndex = 5;
                    break;
                case 6:
                    comboBox1.SelectedIndex = 6;
                    break;
            }

            //单位
            switch (actXET.Ep_wt_unit)
            {
                default:
                case 0:
                    comboBox2.SelectedIndex = 0;
                    break;
                case 1:
                    comboBox2.SelectedIndex = 1;
                    break;
                case 2:
                    comboBox2.SelectedIndex = 2;
                    break;
                case 3:
                    comboBox2.SelectedIndex = 3;
                    break;
                case 4:
                    comboBox2.SelectedIndex = 4;
                    break;
                case 5:
                    comboBox2.SelectedIndex = 5;
                    break;
                case 6:
                    comboBox2.SelectedIndex = 6;
                    break;
                case 7:
                    comboBox2.SelectedIndex = 7;
                    break;
                case 8:
                    comboBox2.SelectedIndex = 8;
                    break;
                case 9:
                    comboBox2.SelectedIndex = 9;
                    break;
            }

            //标定方式
            switch (actXET.Ep_curve)
            {
                default:
                case 2:
                    comboBox3.SelectedIndex = 0;
                    break;
                case 3:
                    comboBox3.SelectedIndex = 1;
                    break;
                case 4:
                    comboBox3.SelectedIndex = 2;
                    break;
            }

            //根据小数点改变数字量满点显示
            if (comboBox1.SelectedIndex == 0)
            {
                textBox13.Text = Convert.ToDouble(textBox13.Text).ToString();
            }
            else
            {
                textBox13.Text = Convert.ToDouble(textBox13.Text).ToString("f" + comboBox1.SelectedIndex);
            }
        }

        //根据标定方式改变点位显示
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex)
            {
                default:
                case 0:
                case 1:
                    if (MyDevice.languageType == 0)
                    {
                        label1.Text = "零点灵敏度: ";
                        label5.Text = "满点灵敏度: ";
                        label6.Text = "零点输出: ";
                        label10.Text = "满点输出: ";
                    }
                    else
                    {
                        label1.Text = "Zero point sensitivity: ";
                        label5.Text = "Full point sensitivity: ";
                        label6.Text = "Zero point output: ";
                        label10.Text = "Full point output: ";
                    }

                    label2.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    label7.Visible = false;
                    label8.Visible = false;
                    label9.Visible = false;
                    label13.Visible = false;
                    label14.Visible = false;
                    label15.Visible = false;
                    textBox2.Visible = false;
                    textBox3.Visible = false;
                    textBox4.Visible = false;
                    textBox7.Visible = false;
                    textBox8.Visible = false;
                    textBox9.Visible = false;

                    label5.Location = label2.Location;
                    label10.Location = label7.Location;
                    label22.Location = label3.Location;
                    label23.Location = label8.Location;
                    textBox5.Location = textBox2.Location;
                    textBox10.Location = textBox7.Location;
                    break;
                case 2:
                    if (MyDevice.languageType == 0)
                    {
                        label1.Text = "点1灵敏度: ";
                        label5.Text = "点5灵敏度: ";
                        label6.Text = "点1输出: ";
                        label10.Text = "点5输出: ";
                    }
                    else
                    {
                        label1.Text = "Point 1 sensitivity: ";
                        label5.Text = "Point 5 sensitivity: ";
                        label6.Text = "Point 1 output: ";
                        label10.Text = "Point 5 output: ";
                    }

                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label13.Visible = true;
                    label14.Visible = true;
                    label15.Visible = true;
                    textBox2.Visible = true;
                    textBox3.Visible = true;
                    textBox4.Visible = true;
                    textBox7.Visible = true;
                    textBox8.Visible = true;
                    textBox9.Visible = true;

                    label5.Location = label5_point;
                    label10.Location = label10_point;
                    label22.Location = label22_point;
                    label23.Location = label23_point;
                    textBox5.Location = textBox5_point;
                    textBox10.Location = textBox10_point;
                    break;
            }
        }

        //根据小数点改变数字量满点显示
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (buttonX1.HoverBackColor != Color.Green || textBox13.Text == "") { return; }

            if (comboBox1.SelectedIndex <= 0)
            {
                textBox13.Text = Convert.ToDouble(textBox13.Text).ToString();
            }
            else
            {
                textBox13.Text = Convert.ToDouble(textBox13.Text).ToString("f" + comboBox1.SelectedIndex);
            }
        }

        //串口通讯委托回调响应
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
                update_EepromPara();

                if (MyDevice.protocol.trTASK == TASKS.REPRM)
                {
                    buttonX1.HoverBackColor = Color.Green;
                }
                if (MyDevice.protocol.trTASK == TASKS.WEPRM)
                {
                    buttonX2.HoverBackColor = Color.Green;
                }
            }
        }
    }
}
