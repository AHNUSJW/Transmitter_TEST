using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

//Alvin 20230409
//Junzhe 20230927
//Lumi 20240604

//各种工作情况下的wt_full和wt_decimal关联性
//输入wt_full和离开输入自动显示小数点效果实现
//输入wt_full的文本框,点击后鼠标不在文本最后,需要改进

namespace Base.UI.MenuSet
{
    public partial class MenuSetParaForm : Form
    {
        private XET actXET;         //需要操作的设备
        private byte oldOutype;     //影响模拟量DAC配置,影响da_point,影响斜率计算
        private byte oldAdspeed;    //影响CS1237初始化,影响灵敏度,影响下ad_point,影响斜率计算
        private byte oldCurve;      //影响ad_point和da_point,影响斜率计算
        private byte oldDecimal;    //影响da_point,影响斜率计算
        private DTiws.View.ButtonX curBT; //按键记录

        //构造函数
        public MenuSetParaForm()
        {
            InitializeComponent();
        }

        //加载
        private void MenuMenuSetCalForm_Load(object sender, EventArgs e)
        {
            //加载接收触发
            MyDevice.myUpdate += new freshHandler(update_FromUart);
            textBox4.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive);
            textBox5.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive);

            //
            actXET = MyDevice.actDev;

            //错误信息提示
            label13.Text = "";
            label13.ForeColor = Color.Firebrick;

            //更新界面
            ui_InitCombox6();
            ui_UpdateForm();
            ui_UpdateByAccount();
        }

        //关闭
        private void MenuMenuSetCalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消接收触发
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        //提示零点跟踪效果
        private void listBoxTrackzero_SelectedIndexChanged(object sender, EventArgs e)
        {
            float e_data;
            float e_track;

            switch (listBoxTrackzero.SelectedIndex)
            {
                default:
                case 0: e_track = (byte)EATK.TKZ00; break;
                case 1: e_track = (byte)EATK.TKZ5; break;
                case 2: e_track = (byte)EATK.TKZ10; break;
                case 3: e_track = (byte)EATK.TKZ20; break;
                case 4: e_track = (byte)EATK.TKZ30; break;
                case 5: e_track = (byte)EATK.TKZ40; break;
                case 6: e_track = (byte)EATK.TKZ50; break;
                case 7: e_track = (byte)EATK.TKZ60; break;
                case 8: e_track = (byte)EATK.TKZ70; break;
                case 9: e_track = (byte)EATK.TKZ80; break;
                case 10: e_track = (byte)EATK.TKZ90; break;
                case 11: e_track = (byte)EATK.TKZ100; break;
                case 12: e_track = (byte)EATK.TKZ200; break;
                case 13: e_track = (byte)EATK.TKZ300; break;
                case 14: e_track = (byte)EATK.TKZ400; break;
                case 15: e_track = (byte)EATK.TKZ500; break;
            }

            if (listBoxTrackzero.SelectedIndex > 0)
            {
                //智能检查故障
                actXET.RefreshCalInfo();

                //零点跟踪计算
                switch (actXET.S_DeviceType)
                {
                    case TYPE.BE30AH:
                    case TYPE.T420:
                    case TYPE.TP10:
                    case TYPE.TDES:
                        if ((Math.Abs(actXET.E_ad_point5 - actXET.E_ad_point1) * e_track / 8192) < 1)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label13.Text = actXET.R_resolution + ", 零点跟踪无效果";
                            }
                            else
                            {
                                label13.Text = actXET.R_resolution + ", Zero tracking has no effect";
                            }
                        }
                        else
                        {
                            label13.Text = "";
                        }
                        break;

                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.TNP10:
                    case TYPE.TDSS:
                    case TYPE.T4X600H:
                        if ((Math.Abs(actXET.E_ad_point5 - actXET.E_ad_point1) * e_track / 131072) < 1)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label13.Text = actXET.R_resolution + ", 零点跟踪无效果";
                            }
                            else
                            {
                                label13.Text = actXET.R_resolution + ", Zero tracking has no effect";
                            }
                        }
                        else
                        {
                            label13.Text = "";
                        }
                        break;

                    case TYPE.TCAN:
                        //1个e对应的adcout
                        e_data = (float)((actXET.E_ad_point5 - actXET.E_ad_point1) * actXET.E_wt_division * e_track) * 0.5f / (float)(actXET.E_da_point5 - actXET.E_da_point1);
                        //
                        if (Math.Abs(e_data) < 1)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label13.Text = actXET.R_resolution + ", 零点跟踪无效果";
                            }
                            else
                            {
                                label13.Text = actXET.R_resolution + ", Zero tracking has no effect";
                            }
                        }
                        else
                        {
                            label13.Text = "";
                        }
                        break;

                    case TYPE.TD485:
                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        if (!actXET.S_ElevenType)
                        {
                            //1个e对应的adcout
                            e_data = (float)((actXET.E_ad_point5 - actXET.E_ad_point1) * actXET.E_wt_division * e_track) * 0.5f / (float)(actXET.E_da_point5 - actXET.E_da_point1);
                        }
                        else
                        {
                            //1个e对应的adcout
                            e_data = (float)((actXET.E_ad_point11 - actXET.E_ad_point1) * actXET.E_wt_division * e_track) * 0.5f / (float)(actXET.E_da_point11 - actXET.E_da_point1);
                        }
                        //
                        if (Math.Abs(e_data) < 1)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label13.Text = actXET.R_resolution + ", 零点跟踪无效果";
                            }
                            else
                            {
                                label13.Text = actXET.R_resolution + ", Zero tracking has no effect";
                            }
                        }
                        else
                        {
                            label13.Text = "";
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                label13.Text = "";
            }
        }

        //提示蠕变跟踪效果
        private void listBoxDynazero_SelectedIndexChanged(object sender, EventArgs e)
        {
            float e_data;
            float e_track;

            switch (listBoxDynazero.SelectedIndex)
            {
                default:
                case 0: e_track = (byte)EATK.TKZ00; break;
                case 1: e_track = (byte)EATK.TKZ5; break;
                case 2: e_track = (byte)EATK.TKZ10; break;
                case 3: e_track = (byte)EATK.TKZ20; break;
                case 4: e_track = (byte)EATK.TKZ30; break;
                case 5: e_track = (byte)EATK.TKZ40; break;
                case 6: e_track = (byte)EATK.TKZ50; break;
                case 7: e_track = (byte)EATK.TKZ60; break;
                case 8: e_track = (byte)EATK.TKZ70; break;
                case 9: e_track = (byte)EATK.TKZ80; break;
                case 10: e_track = (byte)EATK.TKZ90; break;
                case 11: e_track = (byte)EATK.TKZ100; break;
                case 12: e_track = (byte)EATK.TKZ200; break;
                case 13: e_track = (byte)EATK.TKZ300; break;
                case 14: e_track = (byte)EATK.TKZ400; break;
                case 15: e_track = (byte)EATK.TKZ500; break;
            }

            if (listBoxDynazero.SelectedIndex > 0)
            {
                //智能检查故障
                actXET.RefreshCalInfo();

                //零点跟踪计算
                switch (actXET.S_DeviceType)
                {
                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        if (!actXET.S_ElevenType)
                        {
                            //1个e对应的adcout
                            e_data = (float)((actXET.E_ad_point5 - actXET.E_ad_point1) * actXET.E_wt_division * e_track) * 0.5f / (float)(actXET.E_da_point5 - actXET.E_da_point1);
                        }
                        else
                        {
                            //1个e对应的adcout
                            e_data = (float)((actXET.E_ad_point11 - actXET.E_ad_point1) * actXET.E_wt_division * e_track) * 0.5f / (float)(actXET.E_da_point11 - actXET.E_da_point1);
                        }
                        //
                        if (Math.Abs(e_data) < 1)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label13.Text = actXET.R_resolution + ", 蠕变跟踪无效果";
                            }
                            else
                            {
                                label13.Text = actXET.R_resolution + ", Creep tracking has no effect";
                            }
                        }
                        else
                        {
                            label13.Text = "";
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                label13.Text = "";
            }
        }

        //鼠标点击消除小数点准备输入
        private void textBox4_enter(object sender, MouseEventArgs e)
        {
            if (double.TryParse(textBox4.Text, out double value))
            {
                textBox4.Text = value.ToString();
            }
        }

        //离开数字量自动加小数点
        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex <= 0)
            {
                if (double.TryParse(textBox4.Text, out double value))
                {
                    textBox4.Text = Convert.ToDouble(value).ToString();
                }
            }
            else
            {
                if (double.TryParse(textBox4.Text, out double value))
                {
                    textBox4.Text = value.ToString("f" + comboBox4.SelectedIndex);
                }
            }
        }

        //数字量小数点改变
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
            {
                if (double.TryParse(textBox4.Text, out double value))
                {
                    textBox4.Text = value.ToString();
                }
            }
            else
            {
                if (double.TryParse(textBox4.Text, out double value))
                {
                    textBox4.Text = value.ToString("f" + comboBox4.SelectedIndex);
                }
            }
        }

        //Save
        private void button1_Click(object sender, EventArgs e)
        {
            //校验
            if (!double.TryParse(textBox4.Text, out _))
            {
                MessageBox.Show("数字量小数点不能为空,请重新输入");
                return;
            }
            if (!byte.TryParse(textBox5.Text, out _))
            {
                MessageBox.Show("稳定数字量范围只能是0-255的整数");
                return;
            }

            //标定方法
            if (radioButton1.Checked) { actXET.E_curve = (byte)ECVE.CTWOPT; }
            else if (radioButton2.Checked) { actXET.E_curve = (byte)ECVE.CFITED; }
            else if (radioButton3.Checked) { actXET.E_curve = (byte)ECVE.CINTER; }
            else if (radioButton4.Checked) { actXET.E_curve = (byte)ECVE.CELTED; }
            else if (radioButton5.Checked) { actXET.E_curve = (byte)ECVE.CELTER; }
            else { actXET.E_curve = (byte)ECVE.CTWOPT; }

            //标定参数(信号范围)
            switch (listBoxPGA.SelectedIndex)
            {
                case 0: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0xF0) + (byte)EPGA.ADPGA1); break;
                case 1: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0xF0) + (byte)EPGA.ADPGA2); break;
                case 2: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0xF0) + (byte)EPGA.ADPGA64); break;
                case 3: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0xF0) + (byte)EPGA.ADPGA128); break;
                default: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0xF0) + (byte)EPGA.ADPGA128); break;
            }

            //标定参数(采集速率)
            switch (listBoxSpeed.SelectedIndex)
            {
                case 0: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0x0F) + (byte)ESPD.CSF10); break;
                case 1: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0x0F) + (byte)ESPD.CSF40); break;
                case 2: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0x0F) + (byte)ESPD.CSF640); break;
                case 3: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0x0F) + (byte)ESPD.CSF1280); break;
                default: actXET.E_adspeed = (byte)((actXET.E_adspeed & 0x0F) + (byte)ESPD.CSF1280); break;
            }

            //标定参数(上电归零范围)
            switch (listBoxAutozero.SelectedIndex)
            {
                case 0: actXET.E_autozero = (byte)EATZ.ATZ0; break;
                case 1: actXET.E_autozero = (byte)EATZ.ATZ2; break;
                case 2: actXET.E_autozero = (byte)EATZ.ATZ4; break;
                case 3: actXET.E_autozero = (byte)EATZ.ATZ10; break;
                case 4: actXET.E_autozero = (byte)EATZ.ATZ20; break;
                case 5: actXET.E_autozero = (byte)EATZ.ATZ50; break;
                default: actXET.E_autozero = (byte)EATZ.ATZ10; break;
            }

            //标定参数(抗振动等级)
            switch (listBoxAntivib.SelectedIndex)
            {
                case 0: actXET.E_wt_antivib = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_antivib = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_antivib = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_antivib = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_antivib = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_antivib = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_antivib = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_antivib = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_antivib = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_antivib = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_antivib = (byte)ELV.LV10; break;
                default: actXET.E_wt_antivib = (byte)ELV.LV0; break;
            }

            //标定参数(滤波深度)
            switch (listBoxFilterange.SelectedIndex)
            {
                case 0: actXET.E_wt_spfilt = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_spfilt = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_spfilt = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_spfilt = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_spfilt = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_spfilt = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_spfilt = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_spfilt = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_spfilt = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_spfilt = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_spfilt = (byte)ELV.LV10; break;
                default: actXET.E_wt_spfilt = (byte)ELV.LV0; break;
            }

            //标定参数(滤波时间)
            switch (listBoxFiltertime.SelectedIndex)
            {
                case 0: actXET.E_wt_sptime = (byte)ELV.LV0; break;
                case 1: actXET.E_wt_sptime = (byte)ELV.LV1; break;
                case 2: actXET.E_wt_sptime = (byte)ELV.LV2; break;
                case 3: actXET.E_wt_sptime = (byte)ELV.LV3; break;
                case 4: actXET.E_wt_sptime = (byte)ELV.LV4; break;
                case 5: actXET.E_wt_sptime = (byte)ELV.LV5; break;
                case 6: actXET.E_wt_sptime = (byte)ELV.LV6; break;
                case 7: actXET.E_wt_sptime = (byte)ELV.LV7; break;
                case 8: actXET.E_wt_sptime = (byte)ELV.LV8; break;
                case 9: actXET.E_wt_sptime = (byte)ELV.LV9; break;
                case 10: actXET.E_wt_sptime = (byte)ELV.LV10; break;
                default: actXET.E_wt_sptime = (byte)ELV.LV0; break;
            }

            //标定参数(零点跟踪时间)
            switch (listBoxTkzerotime.SelectedIndex)
            {
                case 0: actXET.E_tkzerotime = (byte)TIM.TIM0_1; break;
                case 1: actXET.E_tkzerotime = (byte)TIM.TIM0_2; break;
                case 2: actXET.E_tkzerotime = (byte)TIM.TIM0_3; break;
                case 3: actXET.E_tkzerotime = (byte)TIM.TIM0_4; break;
                case 4: actXET.E_tkzerotime = (byte)TIM.TIM0_5; break;
                case 5: actXET.E_tkzerotime = (byte)TIM.TIM0_6; break;
                case 6: actXET.E_tkzerotime = (byte)TIM.TIM0_7; break;
                case 7: actXET.E_tkzerotime = (byte)TIM.TIM0_8; break;
                case 8: actXET.E_tkzerotime = (byte)TIM.TIM0_9; break;
                case 9: actXET.E_tkzerotime = (byte)TIM.TIM1_0; break;
                case 10: actXET.E_tkzerotime = (byte)TIM.TIM1_1; break;
                case 11: actXET.E_tkzerotime = (byte)TIM.TIM1_2; break;
                case 12: actXET.E_tkzerotime = (byte)TIM.TIM1_3; break;
                case 13: actXET.E_tkzerotime = (byte)TIM.TIM1_4; break;
                case 14: actXET.E_tkzerotime = (byte)TIM.TIM1_5; break;
                case 15: actXET.E_tkzerotime = (byte)TIM.TIM1_6; break;
                case 16: actXET.E_tkzerotime = (byte)TIM.TIM1_7; break;
                case 17: actXET.E_tkzerotime = (byte)TIM.TIM1_8; break;
                case 18: actXET.E_tkzerotime = (byte)TIM.TIM1_9; break;
                case 19: actXET.E_tkzerotime = (byte)TIM.TIM2_0; break;
                case 20: actXET.E_tkzerotime = (byte)TIM.TIM2_1; break;
                case 21: actXET.E_tkzerotime = (byte)TIM.TIM2_2; break;
                case 22: actXET.E_tkzerotime = (byte)TIM.TIM2_3; break;
                case 23: actXET.E_tkzerotime = (byte)TIM.TIM2_4; break;
                case 24: actXET.E_tkzerotime = (byte)TIM.TIM2_5; break;
                case 25: actXET.E_tkzerotime = (byte)TIM.TIM2_6; break;
                case 26: actXET.E_tkzerotime = (byte)TIM.TIM2_7; break;
                case 27: actXET.E_tkzerotime = (byte)TIM.TIM2_8; break;
                case 28: actXET.E_tkzerotime = (byte)TIM.TIM2_9; break;
                case 29: actXET.E_tkzerotime = (byte)TIM.TIM3_0; break;
                default: actXET.E_tkzerotime = (byte)TIM.TIM0_1; break;
            }

            //标定参数(零点跟踪范围)
            switch (listBoxTrackzero.SelectedIndex)
            {
                case 0: actXET.E_trackzero = (byte)EATK.TKZ00; break;
                case 1: actXET.E_trackzero = (byte)EATK.TKZ5; break;
                case 2: actXET.E_trackzero = (byte)EATK.TKZ10; break;
                case 3: actXET.E_trackzero = (byte)EATK.TKZ20; break;
                case 4: actXET.E_trackzero = (byte)EATK.TKZ30; break;
                case 5: actXET.E_trackzero = (byte)EATK.TKZ40; break;
                case 6: actXET.E_trackzero = (byte)EATK.TKZ50; break;
                case 7: actXET.E_trackzero = (byte)EATK.TKZ60; break;
                case 8: actXET.E_trackzero = (byte)EATK.TKZ70; break;
                case 9: actXET.E_trackzero = (byte)EATK.TKZ80; break;
                case 10: actXET.E_trackzero = (byte)EATK.TKZ90; break;
                case 11: actXET.E_trackzero = (byte)EATK.TKZ100; break;
                case 12: actXET.E_trackzero = (byte)EATK.TKZ200; break;
                case 13: actXET.E_trackzero = (byte)EATK.TKZ300; break;
                case 14: actXET.E_trackzero = (byte)EATK.TKZ400; break;
                case 15: actXET.E_trackzero = (byte)EATK.TKZ500; break;
                default: actXET.E_trackzero = (byte)EATK.TKZ5; break;
            }

            //标定参数(蠕变跟踪时间)
            switch (listBoxTkdynatime.SelectedIndex)
            {
                case 0: actXET.E_tkdynatime = (byte)TIM.TIM0_1; break;
                case 1: actXET.E_tkdynatime = (byte)TIM.TIM0_2; break;
                case 2: actXET.E_tkdynatime = (byte)TIM.TIM0_3; break;
                case 3: actXET.E_tkdynatime = (byte)TIM.TIM0_4; break;
                case 4: actXET.E_tkdynatime = (byte)TIM.TIM0_5; break;
                case 5: actXET.E_tkdynatime = (byte)TIM.TIM0_6; break;
                case 6: actXET.E_tkdynatime = (byte)TIM.TIM0_7; break;
                case 7: actXET.E_tkdynatime = (byte)TIM.TIM0_8; break;
                case 8: actXET.E_tkdynatime = (byte)TIM.TIM0_9; break;
                case 9: actXET.E_tkdynatime = (byte)TIM.TIM1_0; break;
                case 10: actXET.E_tkdynatime = (byte)TIM.TIM1_1; break;
                case 11: actXET.E_tkdynatime = (byte)TIM.TIM1_2; break;
                case 12: actXET.E_tkdynatime = (byte)TIM.TIM1_3; break;
                case 13: actXET.E_tkdynatime = (byte)TIM.TIM1_4; break;
                case 14: actXET.E_tkdynatime = (byte)TIM.TIM1_5; break;
                case 15: actXET.E_tkdynatime = (byte)TIM.TIM1_6; break;
                case 16: actXET.E_tkdynatime = (byte)TIM.TIM1_7; break;
                case 17: actXET.E_tkdynatime = (byte)TIM.TIM1_8; break;
                case 18: actXET.E_tkdynatime = (byte)TIM.TIM1_9; break;
                case 19: actXET.E_tkdynatime = (byte)TIM.TIM2_0; break;
                case 20: actXET.E_tkdynatime = (byte)TIM.TIM2_1; break;
                case 21: actXET.E_tkdynatime = (byte)TIM.TIM2_2; break;
                case 22: actXET.E_tkdynatime = (byte)TIM.TIM2_3; break;
                case 23: actXET.E_tkdynatime = (byte)TIM.TIM2_4; break;
                case 24: actXET.E_tkdynatime = (byte)TIM.TIM2_5; break;
                case 25: actXET.E_tkdynatime = (byte)TIM.TIM2_6; break;
                case 26: actXET.E_tkdynatime = (byte)TIM.TIM2_7; break;
                case 27: actXET.E_tkdynatime = (byte)TIM.TIM2_8; break;
                case 28: actXET.E_tkdynatime = (byte)TIM.TIM2_9; break;
                case 29: actXET.E_tkdynatime = (byte)TIM.TIM3_0; break;
                default: actXET.E_tkdynatime = (byte)TIM.TIM0_1; break;
            }

            //标定参数(蠕变跟踪)
            switch (listBoxDynazero.SelectedIndex)
            {
                case 0: actXET.E_dynazero = (byte)EATK.TKZ00; break;
                case 1: actXET.E_dynazero = (byte)EATK.TKZ5; break;
                case 2: actXET.E_dynazero = (byte)EATK.TKZ10; break;
                case 3: actXET.E_dynazero = (byte)EATK.TKZ20; break;
                case 4: actXET.E_dynazero = (byte)EATK.TKZ30; break;
                case 5: actXET.E_dynazero = (byte)EATK.TKZ40; break;
                case 6: actXET.E_dynazero = (byte)EATK.TKZ50; break;
                case 7: actXET.E_dynazero = (byte)EATK.TKZ60; break;
                case 8: actXET.E_dynazero = (byte)EATK.TKZ70; break;
                case 9: actXET.E_dynazero = (byte)EATK.TKZ80; break;
                case 10: actXET.E_dynazero = (byte)EATK.TKZ90; break;
                case 11: actXET.E_dynazero = (byte)EATK.TKZ100; break;
                case 12: actXET.E_dynazero = (byte)EATK.TKZ200; break;
                case 13: actXET.E_dynazero = (byte)EATK.TKZ300; break;
                case 14: actXET.E_dynazero = (byte)EATK.TKZ400; break;
                case 15: actXET.E_dynazero = (byte)EATK.TKZ500; break;
                default: actXET.E_dynazero = (byte)EATK.TKZ5; break;
            }

            //检重报警
            actXET.E_checkhigh = Convert.ToInt32(textBox1.Text.Trim());
            actXET.E_checklow = Convert.ToInt32(textBox2.Text.Trim());

            //产品批号
            actXET.E_mfg_srno = Convert.ToInt32(textBox3.Text.Trim());

            //按键标定是否锁定
            if (radioButton6.Checked) { actXET.E_enspan = 1; }
            else if (radioButton7.Checked) { actXET.E_enspan = 0; }
            else { actXET.E_enspan = 0; }

            ////////////////////////////////////////////////////////////////////////////////

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

                case TYPE.TP10:
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0: actXET.S_OutType = OUT.UTP05; break;
                        case 1: actXET.S_OutType = OUT.UTP10; break;
                    }
                    break;

                case TYPE.BE30AH:
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

            //更新其它数据
            actXET.E_wt_decimal = (Byte)comboBox4.SelectedIndex;
            actXET.E_wt_division = Convert.ToByte(comboBox5.Text);
            actXET.E_wt_unit = (Byte)comboBox6.SelectedValue;
            actXET.E_wt_ascii = (Byte)comboBox7.SelectedIndex;
            actXET.E_stabletime = (Byte)(comboBox8.SelectedIndex + 1);
            actXET.E_stablerange = Convert.ToByte(textBox5.Text);

            //更新decimal后再更新wt_full
            actXET.E_wt_zero = 0;
            actXET.T_wt_full = textBox4.Text;

            ////////////////////////////////////////////////////////////////////////////////

            //影响模拟量DAC配置,影响da_point,影响斜率计算
            if (oldOutype != actXET.E_outype)
            {
                actXET.RefreshOutypeChange(actXET.E_outype, oldOutype);

                actXET.RefreshRatio();
            }

            //影响斜率计算
            if (oldCurve != actXET.E_curve)
            {
                actXET.RefreshRatio();
            }

            //影响CS1237初始化,影响灵敏度,影响ad_point和da_point,影响斜率计算
            if (oldAdspeed != actXET.E_adspeed)
            {
                actXET.RefreshAdspeedChange();

                actXET.RefreshRatio();
            }

            //影响da_point,影响斜率计算
            if (oldDecimal != actXET.E_wt_decimal)
            {
                //小数点更新后重新计算da_point
                if (actXET.S_OutType == OUT.UMASK)
                {
                    actXET.T_analog1 = actXET.T_analog1;
                    actXET.T_analog2 = actXET.T_analog2;
                    actXET.T_analog3 = actXET.T_analog3;
                    actXET.T_analog4 = actXET.T_analog4;
                    actXET.T_analog5 = actXET.T_analog5;
                    actXET.T_analog6 = actXET.T_analog6;
                    actXET.T_analog7 = actXET.T_analog7;
                    actXET.T_analog8 = actXET.T_analog8;
                    actXET.T_analog9 = actXET.T_analog9;
                    actXET.T_analog10 = actXET.T_analog10;
                    actXET.T_analog11 = actXET.T_analog11;
                    actXET.RefreshRatio();
                }
            }

            //写入
            if (MyDevice.protocol.IsOpen)
            {
                curBT = button1;
                button1.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteAllTasks();
            }
        }

        //重置
        private void button2_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                if (MessageBox.Show("为恢复故障设备,【确定】将重置设备参数(包括所有标定参数和传感器参数和通讯参数),恢复后需重设参数和标定！", "Warning", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }

                actXET.ResetDeviceSct();

                this.ui_UpdateForm();

                //写入
                curBT = button2;
                button2.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteAllTasks();
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

                //继续写
                MyDevice.mePort_WriteAllTasks();

                //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                curBT.Text = MyDevice.protocol.trTASK.ToString();
                label13.Text = MyDevice.protocol.rxString;

                //写完了
                if (MyDevice.protocol.trTASK == TASKS.NULL)
                {
                    curBT.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    curBT.HoverBackColor = Color.Green;
                    //modbus TCP 重启后需要3-4秒时间恢复，不直接关闭窗口
                    //if (MyDevice.protocol.type != COMP.ModbusTCP)
                    //{
                    this.Close();
                    // }
                }
                else if ((MyDevice.protocol.trTASK == TASKS.REST) && (MyDevice.mSUT.E_test < 0x58))
                {
                    //老版本重启指令无回复
                    curBT.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                    curBT.HoverBackColor = Color.Green;
                }
            }
        }

        //计算判稳范围
        private void updateFiltRange()
        {
            float ad_span = 0;

            //计算ad_span
            if ((ECVE)actXET.E_curve <= ECVE.CINTER)
            {
                ad_span = actXET.E_ad_point5 - actXET.E_ad_point1;
            }
            else
            {
                ad_span = actXET.E_ad_point11 - actXET.E_ad_point1;
            }

            //计算da_span,wt_span
            float da_span = actXET.E_da_full - actXET.E_da_zero;
            float wt_span = actXET.E_wt_full - actXET.E_wt_zero;

            //计算1个e对应的adcout值
            float e_data = ad_span * wt_span / da_span;
            e_data = e_data * actXET.E_wt_division / wt_span;
            if (e_data < 0)
            {
                e_data = -e_data;
            }

            //将滤波范围换算为n个e
            listBoxFilterange.Items.Clear();
            for (int i = 1; i <= 11; i++)
            {
                listBoxFilterange.Items.Add($"LV{i - 1}({(int)(actXET.E_filter * i / e_data)} e)");
            }
        }

        //更新界面
        private void ui_UpdateForm()
        {
            //
            oldOutype = actXET.E_outype;
            oldCurve = actXET.E_curve;
            oldAdspeed = actXET.E_adspeed;
            oldDecimal = actXET.E_wt_decimal;

            //防错
            if (actXET.E_wt_decimal >= comboBox4.Items.Count) actXET.E_wt_decimal = 0;
            if (!comboBox6.Items.Cast<dynamic>().Any(item => item.Value == actXET.E_wt_unit)) actXET.E_wt_unit = 0;
            if (actXET.E_wt_ascii >= comboBox7.Items.Count) actXET.E_wt_ascii = 0;
            if (actXET.E_wt_division > 100) actXET.E_wt_division = 1;
            if (actXET.E_wt_division == 0) actXET.E_wt_division = 1;
            if (actXET.E_wt_antivib >= comboBox8.Items.Count) actXET.E_wt_antivib = 0;
            if (actXET.E_stabletime > 29) actXET.E_stabletime = 0;
            if (actXET.E_stablerange > 255) actXET.E_stablerange = 0;

            ////////////////////////////////////////////////////////////////////////////////

            //计算判稳范围
            updateFiltRange();

            //标定方法
            switch ((ECVE)actXET.E_curve)
            {
                case ECVE.CTWOPT: radioButton1.Checked = true; break;
                case ECVE.CFITED: radioButton2.Checked = true; break;
                case ECVE.CINTER: radioButton3.Checked = true; break;
                case ECVE.CELTED: radioButton4.Checked = true; break;
                case ECVE.CELTER: radioButton5.Checked = true; break;
                default: radioButton1.Checked = true; break;
            }

            //标定参数(信号范围)
            switch ((EPGA)(actXET.E_adspeed & 0x0F))
            {
                case EPGA.ADPGA1: listBoxPGA.SelectedIndex = 0; break;
                case EPGA.ADPGA2: listBoxPGA.SelectedIndex = 1; break;
                case EPGA.ADPGA64: listBoxPGA.SelectedIndex = 2; break;
                case EPGA.ADPGA128: listBoxPGA.SelectedIndex = 3; break;
                default: listBoxPGA.SelectedIndex = 3; break;
            }

            //标定参数(采集速率)
            switch ((ESPD)(actXET.E_adspeed & 0xF0))
            {
                case ESPD.CSF10:
                    listBoxSpeed.SelectedIndex = 0;
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("208 ms");
                    listBoxFiltertime.Items.Add("416 ms");
                    listBoxFiltertime.Items.Add("624 ms");
                    listBoxFiltertime.Items.Add("832 ms");
                    listBoxFiltertime.Items.Add("1040 ms");
                    listBoxFiltertime.Items.Add("1248 ms");
                    listBoxFiltertime.Items.Add("1456 ms");
                    listBoxFiltertime.Items.Add("1664 ms");
                    listBoxFiltertime.Items.Add("1872 ms");
                    listBoxFiltertime.Items.Add("2080 ms");
                    listBoxFiltertime.Items.Add("2288 ms");
                    break;
                case ESPD.CSF40:
                    listBoxSpeed.SelectedIndex = 1;
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
                case ESPD.CSF640:
                    listBoxSpeed.SelectedIndex = 2;
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("30 ms");
                    listBoxFiltertime.Items.Add("40 ms");
                    listBoxFiltertime.Items.Add("50 ms");
                    listBoxFiltertime.Items.Add("60 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("80 ms");
                    listBoxFiltertime.Items.Add("90 ms");
                    listBoxFiltertime.Items.Add("100 ms");
                    listBoxFiltertime.Items.Add("110 ms");
                    listBoxFiltertime.Items.Add("120 ms");
                    listBoxFiltertime.Items.Add("130 ms");
                    break;
                case ESPD.CSF1280:
                    listBoxSpeed.SelectedIndex = 3;
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("28 ms");
                    listBoxFiltertime.Items.Add("35 ms");
                    listBoxFiltertime.Items.Add("42 ms");
                    listBoxFiltertime.Items.Add("49 ms");
                    listBoxFiltertime.Items.Add("56 ms");
                    listBoxFiltertime.Items.Add("63 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("77 ms");
                    listBoxFiltertime.Items.Add("84 ms");
                    listBoxFiltertime.Items.Add("91 ms");
                    listBoxFiltertime.Items.Add("98 ms");
                    break;
                default:
                    listBoxSpeed.SelectedIndex = 1;
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
            }

            //标定参数(上电归零范围)
            switch ((EATZ)actXET.E_autozero)
            {
                case EATZ.ATZ0: listBoxAutozero.SelectedIndex = 0; break;
                case EATZ.ATZ2: listBoxAutozero.SelectedIndex = 1; break;
                case EATZ.ATZ4: listBoxAutozero.SelectedIndex = 2; break;
                case EATZ.ATZ10: listBoxAutozero.SelectedIndex = 3; break;
                case EATZ.ATZ20: listBoxAutozero.SelectedIndex = 4; break;
                case EATZ.ATZ50: listBoxAutozero.SelectedIndex = 5; break;
                default: listBoxAutozero.SelectedIndex = 3; break;
            }

            //标定参数(抗振动等级)
            switch ((ELV)actXET.E_wt_antivib)
            {
                case ELV.LV0: listBoxAntivib.SelectedIndex = 0; break;
                case ELV.LV1: listBoxAntivib.SelectedIndex = 1; break;
                case ELV.LV2: listBoxAntivib.SelectedIndex = 2; break;
                case ELV.LV3: listBoxAntivib.SelectedIndex = 3; break;
                case ELV.LV4: listBoxAntivib.SelectedIndex = 4; break;
                case ELV.LV5: listBoxAntivib.SelectedIndex = 5; break;
                case ELV.LV6: listBoxAntivib.SelectedIndex = 6; break;
                case ELV.LV7: listBoxAntivib.SelectedIndex = 7; break;
                case ELV.LV8: listBoxAntivib.SelectedIndex = 8; break;
                case ELV.LV9: listBoxAntivib.SelectedIndex = 9; break;
                case ELV.LV10: listBoxAntivib.SelectedIndex = 10; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(滤波深度)
            switch ((ELV)actXET.E_wt_spfilt)
            {
                case ELV.LV0: listBoxFilterange.SelectedIndex = 0; break;
                case ELV.LV1: listBoxFilterange.SelectedIndex = 1; break;
                case ELV.LV2: listBoxFilterange.SelectedIndex = 2; break;
                case ELV.LV3: listBoxFilterange.SelectedIndex = 3; break;
                case ELV.LV4: listBoxFilterange.SelectedIndex = 4; break;
                case ELV.LV5: listBoxFilterange.SelectedIndex = 5; break;
                case ELV.LV6: listBoxFilterange.SelectedIndex = 6; break;
                case ELV.LV7: listBoxFilterange.SelectedIndex = 7; break;
                case ELV.LV8: listBoxFilterange.SelectedIndex = 8; break;
                case ELV.LV9: listBoxFilterange.SelectedIndex = 9; break;
                case ELV.LV10: listBoxFilterange.SelectedIndex = 10; break;
                default: listBoxFilterange.SelectedIndex = 0; break;
            }

            //标定参数(滤波时间)
            switch ((ELV)actXET.E_wt_sptime)
            {
                case ELV.LV0: listBoxFiltertime.SelectedIndex = 0; break;
                case ELV.LV1: listBoxFiltertime.SelectedIndex = 1; break;
                case ELV.LV2: listBoxFiltertime.SelectedIndex = 2; break;
                case ELV.LV3: listBoxFiltertime.SelectedIndex = 3; break;
                case ELV.LV4: listBoxFiltertime.SelectedIndex = 4; break;
                case ELV.LV5: listBoxFiltertime.SelectedIndex = 5; break;
                case ELV.LV6: listBoxFiltertime.SelectedIndex = 6; break;
                case ELV.LV7: listBoxFiltertime.SelectedIndex = 7; break;
                case ELV.LV8: listBoxFiltertime.SelectedIndex = 8; break;
                case ELV.LV9: listBoxFiltertime.SelectedIndex = 9; break;
                case ELV.LV10: listBoxFiltertime.SelectedIndex = 10; break;
                default: listBoxFiltertime.SelectedIndex = 0; break;
            }

            //标定参数(零点跟踪时间)
            switch ((TIM)actXET.E_tkzerotime)
            {
                case TIM.TIM0_1: listBoxTkzerotime.SelectedIndex = 0; break;
                case TIM.TIM0_2: listBoxTkzerotime.SelectedIndex = 1; break;
                case TIM.TIM0_3: listBoxTkzerotime.SelectedIndex = 2; break;
                case TIM.TIM0_4: listBoxTkzerotime.SelectedIndex = 3; break;
                case TIM.TIM0_5: listBoxTkzerotime.SelectedIndex = 4; break;
                case TIM.TIM0_6: listBoxTkzerotime.SelectedIndex = 5; break;
                case TIM.TIM0_7: listBoxTkzerotime.SelectedIndex = 6; break;
                case TIM.TIM0_8: listBoxTkzerotime.SelectedIndex = 7; break;
                case TIM.TIM0_9: listBoxTkzerotime.SelectedIndex = 8; break;
                case TIM.TIM1_0: listBoxTkzerotime.SelectedIndex = 9; break;
                case TIM.TIM1_1: listBoxTkzerotime.SelectedIndex = 10; break;
                case TIM.TIM1_2: listBoxTkzerotime.SelectedIndex = 11; break;
                case TIM.TIM1_3: listBoxTkzerotime.SelectedIndex = 12; break;
                case TIM.TIM1_4: listBoxTkzerotime.SelectedIndex = 13; break;
                case TIM.TIM1_5: listBoxTkzerotime.SelectedIndex = 14; break;
                case TIM.TIM1_6: listBoxTkzerotime.SelectedIndex = 15; break;
                case TIM.TIM1_7: listBoxTkzerotime.SelectedIndex = 16; break;
                case TIM.TIM1_8: listBoxTkzerotime.SelectedIndex = 17; break;
                case TIM.TIM1_9: listBoxTkzerotime.SelectedIndex = 18; break;
                case TIM.TIM2_0: listBoxTkzerotime.SelectedIndex = 19; break;
                case TIM.TIM2_1: listBoxTkzerotime.SelectedIndex = 20; break;
                case TIM.TIM2_2: listBoxTkzerotime.SelectedIndex = 21; break;
                case TIM.TIM2_3: listBoxTkzerotime.SelectedIndex = 22; break;
                case TIM.TIM2_4: listBoxTkzerotime.SelectedIndex = 23; break;
                case TIM.TIM2_5: listBoxTkzerotime.SelectedIndex = 24; break;
                case TIM.TIM2_6: listBoxTkzerotime.SelectedIndex = 25; break;
                case TIM.TIM2_7: listBoxTkzerotime.SelectedIndex = 26; break;
                case TIM.TIM2_8: listBoxTkzerotime.SelectedIndex = 27; break;
                case TIM.TIM2_9: listBoxTkzerotime.SelectedIndex = 28; break;
                case TIM.TIM3_0: listBoxTkzerotime.SelectedIndex = 29; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(零点跟踪范围)
            switch ((EATK)actXET.E_trackzero)
            {
                case EATK.TKZ00: listBoxTrackzero.SelectedIndex = 0; break;
                case EATK.TKZ5: listBoxTrackzero.SelectedIndex = 1; break;
                case EATK.TKZ10: listBoxTrackzero.SelectedIndex = 2; break;
                case EATK.TKZ20: listBoxTrackzero.SelectedIndex = 3; break;
                case EATK.TKZ30: listBoxTrackzero.SelectedIndex = 4; break;
                case EATK.TKZ40: listBoxTrackzero.SelectedIndex = 5; break;
                case EATK.TKZ50: listBoxTrackzero.SelectedIndex = 6; break;
                case EATK.TKZ60: listBoxTrackzero.SelectedIndex = 7; break;
                case EATK.TKZ70: listBoxTrackzero.SelectedIndex = 8; break;
                case EATK.TKZ80: listBoxTrackzero.SelectedIndex = 9; break;
                case EATK.TKZ90: listBoxTrackzero.SelectedIndex = 10; break;
                case EATK.TKZ100: listBoxTrackzero.SelectedIndex = 11; break;
                case EATK.TKZ200: listBoxTrackzero.SelectedIndex = 12; break;
                case EATK.TKZ300: listBoxTrackzero.SelectedIndex = 13; break;
                case EATK.TKZ400: listBoxTrackzero.SelectedIndex = 14; break;
                case EATK.TKZ500: listBoxTrackzero.SelectedIndex = 15; break;
                default: listBoxTrackzero.SelectedIndex = 1; break;
            }

            //标定参数(蠕变跟踪时间)
            switch ((TIM)actXET.E_tkdynatime)
            {
                case TIM.TIM0_1: listBoxTkdynatime.SelectedIndex = 0; break;
                case TIM.TIM0_2: listBoxTkdynatime.SelectedIndex = 1; break;
                case TIM.TIM0_3: listBoxTkdynatime.SelectedIndex = 2; break;
                case TIM.TIM0_4: listBoxTkdynatime.SelectedIndex = 3; break;
                case TIM.TIM0_5: listBoxTkdynatime.SelectedIndex = 4; break;
                case TIM.TIM0_6: listBoxTkdynatime.SelectedIndex = 5; break;
                case TIM.TIM0_7: listBoxTkdynatime.SelectedIndex = 6; break;
                case TIM.TIM0_8: listBoxTkdynatime.SelectedIndex = 7; break;
                case TIM.TIM0_9: listBoxTkdynatime.SelectedIndex = 8; break;
                case TIM.TIM1_0: listBoxTkdynatime.SelectedIndex = 9; break;
                case TIM.TIM1_1: listBoxTkdynatime.SelectedIndex = 10; break;
                case TIM.TIM1_2: listBoxTkdynatime.SelectedIndex = 11; break;
                case TIM.TIM1_3: listBoxTkdynatime.SelectedIndex = 12; break;
                case TIM.TIM1_4: listBoxTkdynatime.SelectedIndex = 13; break;
                case TIM.TIM1_5: listBoxTkdynatime.SelectedIndex = 14; break;
                case TIM.TIM1_6: listBoxTkdynatime.SelectedIndex = 15; break;
                case TIM.TIM1_7: listBoxTkdynatime.SelectedIndex = 16; break;
                case TIM.TIM1_8: listBoxTkdynatime.SelectedIndex = 17; break;
                case TIM.TIM1_9: listBoxTkdynatime.SelectedIndex = 18; break;
                case TIM.TIM2_0: listBoxTkdynatime.SelectedIndex = 19; break;
                case TIM.TIM2_1: listBoxTkdynatime.SelectedIndex = 20; break;
                case TIM.TIM2_2: listBoxTkdynatime.SelectedIndex = 21; break;
                case TIM.TIM2_3: listBoxTkdynatime.SelectedIndex = 22; break;
                case TIM.TIM2_4: listBoxTkdynatime.SelectedIndex = 23; break;
                case TIM.TIM2_5: listBoxTkdynatime.SelectedIndex = 24; break;
                case TIM.TIM2_6: listBoxTkdynatime.SelectedIndex = 25; break;
                case TIM.TIM2_7: listBoxTkdynatime.SelectedIndex = 26; break;
                case TIM.TIM2_8: listBoxTkdynatime.SelectedIndex = 27; break;
                case TIM.TIM2_9: listBoxTkdynatime.SelectedIndex = 28; break;
                case TIM.TIM3_0: listBoxTkdynatime.SelectedIndex = 29; break;
                default: listBoxAntivib.SelectedIndex = 0; break;
            }

            //标定参数(蠕变跟踪范围)
            switch ((EATK)actXET.E_dynazero)
            {
                case EATK.TKZ00: listBoxDynazero.SelectedIndex = 0; break;
                case EATK.TKZ5: listBoxDynazero.SelectedIndex = 1; break;
                case EATK.TKZ10: listBoxDynazero.SelectedIndex = 2; break;
                case EATK.TKZ20: listBoxDynazero.SelectedIndex = 3; break;
                case EATK.TKZ30: listBoxDynazero.SelectedIndex = 4; break;
                case EATK.TKZ40: listBoxDynazero.SelectedIndex = 5; break;
                case EATK.TKZ50: listBoxDynazero.SelectedIndex = 6; break;
                case EATK.TKZ60: listBoxDynazero.SelectedIndex = 7; break;
                case EATK.TKZ70: listBoxDynazero.SelectedIndex = 8; break;
                case EATK.TKZ80: listBoxDynazero.SelectedIndex = 9; break;
                case EATK.TKZ90: listBoxDynazero.SelectedIndex = 10; break;
                case EATK.TKZ100: listBoxDynazero.SelectedIndex = 11; break;
                case EATK.TKZ200: listBoxDynazero.SelectedIndex = 12; break;
                case EATK.TKZ300: listBoxDynazero.SelectedIndex = 13; break;
                case EATK.TKZ400: listBoxDynazero.SelectedIndex = 14; break;
                case EATK.TKZ500: listBoxDynazero.SelectedIndex = 15; break;
                default: listBoxDynazero.SelectedIndex = 0; break;
            }

            //检重报警
            this.textBox1.Text = actXET.E_checkhigh.ToString();
            this.textBox2.Text = actXET.E_checklow.ToString();

            //产品批号
            this.textBox3.Text = actXET.E_mfg_srno.ToString();

            //按键标定是否锁定
            if (actXET.E_enspan > 0)
            {
                radioButton6.Checked = true;
            }
            else
            {
                radioButton7.Checked = true;
            }

            ////////////////////////////////////////////////////////////////////////////////

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
                    break;
            }

            //数字量参数
            textBox4.Text = actXET.T_wt_full;
            comboBox4.SelectedIndex = actXET.E_wt_decimal;
            comboBox6.SelectedValue = actXET.E_wt_unit;
            comboBox7.SelectedIndex = actXET.E_wt_ascii;
            comboBox5.Text = actXET.E_wt_division.ToString();
            comboBox8.SelectedIndex = actXET.E_stabletime - 1;
            textBox5.Text = actXET.E_stablerange.ToString();

            ////////////////////////////////////////////////////////////////////////////////

            //控件使能
            switch (actXET.S_DeviceType)
            {
                default:
                case TYPE.BS420H:
                case TYPE.T8X420H:
                case TYPE.BS600H:
                    comboBox1.Enabled = true;//模拟量输出类型
                    radioButton4.Enabled = false;//11点拟合
                    radioButton5.Enabled = false;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = true;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    groupBox7.Visible = false;//没有数字量
                    break;
                case TYPE.BE30AH:
                    comboBox1.Enabled = true;//模拟量输出类型
                    radioButton4.Enabled = false;//11点拟合
                    radioButton5.Enabled = false;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = true;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    groupBox7.Visible = true;//有数字量
                    label15.Enabled = false;//没有分度值
                    comboBox5.Enabled = false;//没有分度值
                    label14.Enabled = false;//没有数字量稳定时间
                    comboBox8.Enabled = false;//没有数字量稳定时间
                    textBox5.Enabled = false;//没有数字量稳定范围
                    break;
                case TYPE.T420:
                case TYPE.TNP10:
                case TYPE.TP10:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        comboBox1.Enabled = false;//模拟量输出类型
                    }
                    else
                    {
                        comboBox1.Enabled = true;//模拟量输出类型
                    }
                    radioButton4.Enabled = false;//11点拟合
                    radioButton5.Enabled = false;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    groupBox7.Visible = false;//没有数字量
                    break;

                case TYPE.TDES:
                case TYPE.TDSS:
                case TYPE.T4X600H:
                    if (actXET.S_OutType == OUT.UT420)
                    {
                        comboBox1.Enabled = false;//模拟量输出类型
                    }
                    else
                    {
                        comboBox1.Enabled = true;//模拟量输出类型
                    }
                    radioButton4.Enabled = false;//11点拟合
                    radioButton5.Enabled = false;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    label15.Enabled = false;//没有分度值
                    comboBox5.Enabled = false;//没有分度值
                    label14.Enabled = false;//没有数字量稳定时间
                    comboBox8.Enabled = false;//没有数字量稳定时间
                    textBox5.Enabled = false;//没有数字量稳定范围
                    break;

                case TYPE.TD485:
                    groupBox6.Enabled = false;//模拟量输出类型
                    radioButton4.Enabled = true;//11点拟合
                    radioButton5.Enabled = true;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    label15.Enabled = true;//有分度值
                    comboBox5.Enabled = true;//有分度值
                    label14.Enabled = true;//有数字量稳定时间
                    comboBox8.Enabled = true;//有数字量稳定时间
                    textBox5.Enabled = false;//没有数字量稳定范围
                    break;

                case TYPE.TCAN:
                    groupBox6.Enabled = false;//模拟量输出类型
                    if (actXET.E_test > 0x58) //老版本没有11点标定
                    {
                        radioButton4.Enabled = true;//11点拟合
                        radioButton5.Enabled = true;//11点插值
                    }
                    else
                    {
                        radioButton4.Enabled = false;//11点拟合
                        radioButton5.Enabled = false;//11点插值
                    }
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = false;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    label15.Enabled = true;//有分度值
                    comboBox5.Enabled = true;//有分度值
                    label14.Enabled = false;//没有数字量稳定时间
                    comboBox8.Enabled = false;//没有数字量稳定时间
                    textBox5.Enabled = false;//没有数字量稳定范围
                    break;

                case TYPE.iBus:
                    groupBox6.Enabled = false;//模拟量输出类型
                    radioButton4.Enabled = true;//11点拟合
                    radioButton5.Enabled = true;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    label15.Enabled = true;//有分度值
                    comboBox5.Enabled = true;//有分度值
                    label14.Enabled = true;//有数字量稳定时间
                    comboBox8.Enabled = true;//有数字量稳定时间
                    textBox5.Enabled = true;//有数字量稳定范围
                    break;

                case TYPE.iNet:
                case TYPE.iStar:
                    groupBox6.Enabled = false;//模拟量输出类型
                    radioButton4.Enabled = true;//11点拟合
                    radioButton5.Enabled = true;//11点插值
                    groupBox3.Enabled = false;//检重报警
                    groupBox5.Enabled = false;//按键锁
                    listBoxAntivib.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxDynazero.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFilterange.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxFiltertime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkzerotime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    listBoxTkdynatime.Enabled = true;//蠕变跟踪,滤波范围,稳定次数,抗振动等级,蠕变跟踪时间,零点跟踪时间iBus
                    label15.Enabled = true;//有分度值
                    comboBox5.Enabled = true;//有分度值
                    label14.Enabled = true;//有数字量稳定时间
                    comboBox8.Enabled = true;//有数字量稳定时间
                    textBox5.Enabled = false;//没有数字量稳定范围
                    break;
            }

            ////////////////////////////////////////////////////////////////////////////////

            //控件隐藏
            switch (actXET.S_DeviceType)
            {
                default:
                case TYPE.BS420H:
                case TYPE.T8X420H:
                case TYPE.BS600H:
                    label20.Visible = false;//隐藏抗振动等级
                    listBoxAntivib.Visible = false;//隐藏抗振动等级
                    label9.Visible = false;//隐藏滤波深度
                    listBoxFilterange.Visible = false;//隐藏滤波深度
                    label10.Visible = false;//隐藏滤波时间
                    listBoxFiltertime.Visible = false;//隐藏滤波时间
                    groupBox7.Visible = false;//隐藏数字量参数
                    break;

                case TYPE.BE30AH:
                    label20.Visible = false;//隐藏抗振动等级
                    listBoxAntivib.Visible = false;//隐藏抗振动等级
                    label9.Visible = false;//隐藏滤波深度
                    listBoxFilterange.Visible = false;//隐藏滤波深度
                    label10.Visible = false;//隐藏滤波时间
                    listBoxFiltertime.Visible = false;//隐藏滤波时间
                    break;

                case TYPE.T420:
                case TYPE.TNP10:
                case TYPE.TP10:
                    label20.Visible = false;//隐藏抗振动等级
                    listBoxAntivib.Visible = false;//隐藏抗振动等级
                    label9.Visible = false;//隐藏滤波深度
                    listBoxFilterange.Visible = false;//隐藏滤波深度
                    label10.Visible = false;//隐藏滤波时间
                    listBoxFiltertime.Visible = false;//隐藏滤波时间
                    groupBox5.Visible = false;//隐藏按键锁
                    groupBox7.Visible = false;//隐藏数字量参数
                    break;

                case TYPE.TDES:
                case TYPE.TDSS:
                case TYPE.T4X600H:
                    label20.Visible = false;//隐藏抗振动等级
                    listBoxAntivib.Visible = false;//隐藏抗振动等级
                    label9.Visible = false;//隐藏滤波深度
                    listBoxFilterange.Visible = false;//隐藏滤波深度
                    label10.Visible = false;//隐藏滤波时间
                    listBoxFiltertime.Visible = false;//隐藏滤波时间
                    groupBox5.Visible = false;//隐藏按键锁
                    label14.Visible = false;//隐藏数字量稳定时间
                    comboBox8.Visible = false;//隐藏数字量稳定时间
                    label25.Visible = false;//隐藏数字量稳定范围
                    textBox5.Visible = false;//隐藏数字量稳定范围
                    break;

                case TYPE.TCAN:
                    label20.Visible = false;//隐藏抗振动等级
                    listBoxAntivib.Visible = false;//隐藏抗振动等级
                    label9.Visible = false;//隐藏滤波深度
                    listBoxFilterange.Visible = false;//隐藏滤波深度
                    label10.Visible = false;//隐藏滤波时间
                    listBoxFiltertime.Visible = false;//隐藏滤波时间
                    groupBox5.Visible = false;//隐藏按键锁
                    label19.Visible = false;//隐藏连续发送格式
                    comboBox7.Visible = false;//隐藏连续发送格式
                    label14.Visible = false;//隐藏数字量稳定时间
                    comboBox8.Visible = false;//隐藏数字量稳定时间
                    label25.Visible = false;//隐藏数字量稳定范围
                    textBox5.Visible = false;//隐藏数字量稳定范围
                    break;

                case TYPE.TD485:
                case TYPE.iStar:
                case TYPE.iNet:
                    label25.Visible = false;//隐藏数字量稳定范围
                    textBox5.Visible = false;//隐藏数字量稳定范围
                    groupBox5.Visible = false;//隐藏按键锁
                    break;

                case TYPE.iBus:
                    groupBox5.Visible = false;//隐藏按键锁
                    break;
            }
        }

        //初始化数字量单位选择
        private void ui_InitCombox6()
        {
            var unitList = Enum.GetValues(typeof(UNIT))
                               .Cast<UNIT>()
                               .Select(e => new
                               {
                                   Value = (byte)e,
                                   Description = UnitHelper.GetUnitAdjustedDescription(e)
                               })
                               .ToList();

            comboBox6.DataSource = unitList;
            comboBox6.DisplayMember = "Description";
            comboBox6.ValueMember = "Value";
        }

        //依据账户权限更新见面
        private void ui_UpdateByAccount()
        {
            if (((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc")) || (MyDevice.D_username == "fac") && (MyDevice.D_password == "woli") || (MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            {
                button2.Visible = true;

                label16.Enabled = true;
                textBox4.Enabled = true;
                label17.Enabled = true;
                comboBox4.Enabled = true;
                label15.Enabled = true;
                comboBox5.Enabled = true;
            }
            else
            {
                button2.Visible = false;

                label16.Enabled = false;
                textBox4.Enabled = false;
                label17.Enabled = false;
                comboBox4.Enabled = false;
                label15.Enabled = false;
                comboBox5.Enabled = false;
            }
        }

        //更换采样频率
        private void listBoxSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (listBoxSpeed.SelectedIndex)
            {
                case 0:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("208 ms");
                    listBoxFiltertime.Items.Add("416 ms");
                    listBoxFiltertime.Items.Add("624 ms");
                    listBoxFiltertime.Items.Add("832 ms");
                    listBoxFiltertime.Items.Add("1040 ms");
                    listBoxFiltertime.Items.Add("1248 ms");
                    listBoxFiltertime.Items.Add("1456 ms");
                    listBoxFiltertime.Items.Add("1664 ms");
                    listBoxFiltertime.Items.Add("1872 ms");
                    listBoxFiltertime.Items.Add("2080 ms");
                    listBoxFiltertime.Items.Add("2288 ms");
                    break;
                case 1:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
                case 2:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("30 ms");
                    listBoxFiltertime.Items.Add("40 ms");
                    listBoxFiltertime.Items.Add("50 ms");
                    listBoxFiltertime.Items.Add("60 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("80 ms");
                    listBoxFiltertime.Items.Add("90 ms");
                    listBoxFiltertime.Items.Add("100 ms");
                    listBoxFiltertime.Items.Add("110 ms");
                    listBoxFiltertime.Items.Add("120 ms");
                    listBoxFiltertime.Items.Add("130 ms");
                    break;
                case 3:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("28 ms");
                    listBoxFiltertime.Items.Add("35 ms");
                    listBoxFiltertime.Items.Add("42 ms");
                    listBoxFiltertime.Items.Add("49 ms");
                    listBoxFiltertime.Items.Add("56 ms");
                    listBoxFiltertime.Items.Add("63 ms");
                    listBoxFiltertime.Items.Add("70 ms");
                    listBoxFiltertime.Items.Add("77 ms");
                    listBoxFiltertime.Items.Add("84 ms");
                    listBoxFiltertime.Items.Add("91 ms");
                    listBoxFiltertime.Items.Add("98 ms");
                    break;
                default:
                    listBoxFiltertime.Items.Clear();
                    listBoxFiltertime.Items.Add("108 ms");
                    listBoxFiltertime.Items.Add("162 ms");
                    listBoxFiltertime.Items.Add("216 ms");
                    listBoxFiltertime.Items.Add("270 ms");
                    listBoxFiltertime.Items.Add("324 ms");
                    listBoxFiltertime.Items.Add("378 ms");
                    listBoxFiltertime.Items.Add("432 ms");
                    listBoxFiltertime.Items.Add("486 ms");
                    listBoxFiltertime.Items.Add("540 ms");
                    listBoxFiltertime.Items.Add("594 ms");
                    listBoxFiltertime.Items.Add("648 ms");
                    break;
            }

            //标定参数(滤波时间)
            switch ((ELV)actXET.E_wt_sptime)
            {
                case ELV.LV0: listBoxFiltertime.SelectedIndex = 0; break;
                case ELV.LV1: listBoxFiltertime.SelectedIndex = 1; break;
                case ELV.LV2: listBoxFiltertime.SelectedIndex = 2; break;
                case ELV.LV3: listBoxFiltertime.SelectedIndex = 3; break;
                case ELV.LV4: listBoxFiltertime.SelectedIndex = 4; break;
                case ELV.LV5: listBoxFiltertime.SelectedIndex = 5; break;
                case ELV.LV6: listBoxFiltertime.SelectedIndex = 6; break;
                case ELV.LV7: listBoxFiltertime.SelectedIndex = 7; break;
                case ELV.LV8: listBoxFiltertime.SelectedIndex = 8; break;
                case ELV.LV9: listBoxFiltertime.SelectedIndex = 9; break;
                case ELV.LV10: listBoxFiltertime.SelectedIndex = 10; break;
                default: listBoxFiltertime.SelectedIndex = 0; break;
            }
        }
    }
}
