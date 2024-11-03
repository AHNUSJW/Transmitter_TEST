using Library;
using Model;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

//Lumi 20231222

//加载窗口时初始化页面

//手动校准要实现生产所有的操作,避免来回切换页面浪费时间

//更新Bohrcode功能独立管理

//修改输出型号后需要更新设备,才能开始校准,做好防呆

namespace Base.UI.MenuSet
{
    public partial class MenuFacManualForm : Form
    {
        private XET actXET;//需要操作的设备

        //TASKS.RDX0 读出所有SCT
        //TASKS.BOR 更新Bohrcode
        //TASKS.WRX0 修改outype
        //TASKS.WRX1 标定后写入
        //TASKS.WRX4 校准后写入
        private TASKS uiTask;//按键指令

        //构造函数
        public MenuFacManualForm()
        {
            InitializeComponent();
        }

        //加载事件
        private void MenuFacManualForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //
            actXET = MyDevice.actDev;

            //错误信息提示
            label13.Text = "";
            label13.ForeColor = Color.Firebrick;

            //更新表格
            paralist_Checking();

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
        private void MenuFacManualForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            MyDevice.mePort_SendCOM(TASKS.DONE);//结束矫正
        }

        //更新表格
        private void paralist_Checking()
        {
            byte deci = actXET.S_decimal;
            string unit = " " + actXET.S_unit;
            string unitUmask = actXET.GetUnitUMASK();

            //控件使能
            if (actXET.S_HalfCal)
            {
                //有12mA中点校准
                label5.Enabled = true;
                textBox4.Enabled = true;
                buttonX1.Enabled = true;
                buttonX2.Enabled = true;
            }
            else
            {
                //无12mA中点校准
                label5.Enabled = false;
                textBox4.Enabled = false;
                buttonX1.Enabled = false;
                buttonX2.Enabled = false;
            }

            switch (actXET.S_DeviceType)
            {
                case TYPE.BE30AH:
                    label4.Text = "BE30AH";
                    break;
                case TYPE.BS420H:
                    label4.Text = "BS420H";
                    break;
                case TYPE.T8X420H:
                    label4.Text = "T8X420H";
                    break;
                case TYPE.BS600H:
                    label4.Text = "BS600H";
                    break;
                case TYPE.T4X600H:
                    label4.Text = "T4X600H";
                    break;
                case TYPE.T420:
                    label4.Text = "T420";
                    break;
                case TYPE.TNP10:
                    label4.Text = "TNP10";
                    break;
                case TYPE.TP10:
                    label4.Text = "TP10";
                    break;

                case TYPE.TDES:
                    switch (actXET.S_OutType)
                    {
                        case OUT.UT420:
                            label4.Text = "TDES-420";
                            break;

                        case OUT.UTP05:
                        case OUT.UTP10:
                            label4.Text = "TDES-10";
                            break;

                        default:
                            label4.Text = "";
                            break;
                    }
                    break;

                case TYPE.TDSS:
                    switch (actXET.S_OutType)
                    {
                        case OUT.UT420:
                            label4.Text = "TDSS-420";
                            break;

                        case OUT.UTN05:
                        case OUT.UTN10:
                            label4.Text = "TDSS-NP10";
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                case TYPE.TD485:
                case TYPE.TCAN:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    label4.Text = "";
                    break;
            }

            string sens = actXET.RefreshSens();//灵敏度

            //
            listBox1.Items.Clear();
            listBox1.Items.Add("----------------");
            listBox1.Items.Add("TYPE = " + actXET.S_DeviceType.ToString());
            listBox1.Items.Add("OUT  = " + actXET.S_OutType.ToString());
            listBox1.Items.Add("----------------");
            listBox1.Items.Add("[0]e_test      = 0x" + actXET.E_test.ToString("X2"));
            listBox1.Items.Add("[0]e_outype    = 0x" + actXET.E_outype.ToString("X2"));
            listBox1.Items.Add("[0]e_curve     = 0x" + actXET.E_curve.ToString("X2"));
            listBox1.Items.Add("[0]e_adspeed   = 0x" + actXET.E_adspeed.ToString("X2"));
            listBox1.Items.Add("[0]e_mfg_date  = " + actXET.E_mfg_date.ToString());
            listBox1.Items.Add("[0]e_bohrcode  = " + actXET.E_bohrcode.ToString("X8"));
            listBox1.Items.Add("[0]e_protype   = " + actXET.E_protype.ToString());
            listBox1.Items.Add("----------------");
            switch ((ECVE)actXET.E_curve)
            {
                case ECVE.CTWOPT:
                    listBox1.Items.Add("[2]e_input1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_analog1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                    listBox1.Items.Add("e_sens         = " + sens);
                    listBox1.Items.Add("[1]e_ad_point1 = " + actXET.E_ad_point1.ToString());
                    listBox1.Items.Add("[1]e_ad_point5 = " + actXET.E_ad_point5.ToString());
                    listBox1.Items.Add("[1]e_da_point1 = " + actXET.E_da_point1.ToString());
                    listBox1.Items.Add("[1]e_da_point5 = " + actXET.E_da_point5.ToString());
                    listBox1.Items.Add("[3]e_ad_zero   = " + actXET.E_ad_zero.ToString());
                    listBox1.Items.Add("[3]e_ad_full   = " + actXET.E_ad_full.ToString());
                    listBox1.Items.Add("[3]e_da_zero   = " + actXET.E_da_zero.ToString());
                    listBox1.Items.Add("[3]e_da_full   = " + actXET.E_da_full.ToString());
                    listBox1.Items.Add("[3]e_vtio      = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                    listBox1.Items.Add("----------------");
                    break;
                case ECVE.CFITED:
                case ECVE.CINTER:
                    listBox1.Items.Add("[2]e_input1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_analog1 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog2 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog3 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog4 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog5 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                    listBox1.Items.Add("e_sens       = " + sens);
                    listBox1.Items.Add("----------------");
                    listBox1.Items.Add("[1]e_ad_point1 = " + actXET.E_ad_point1.ToString());
                    listBox1.Items.Add("[1]e_ad_point2 = " + actXET.E_ad_point2.ToString());
                    listBox1.Items.Add("[1]e_ad_point3 = " + actXET.E_ad_point3.ToString());
                    listBox1.Items.Add("[1]e_ad_point4 = " + actXET.E_ad_point4.ToString());
                    listBox1.Items.Add("[1]e_ad_point5 = " + actXET.E_ad_point5.ToString());
                    listBox1.Items.Add("[1]e_da_point1 = " + actXET.E_da_point1.ToString());
                    listBox1.Items.Add("[1]e_da_point2 = " + actXET.E_da_point2.ToString());
                    listBox1.Items.Add("[1]e_da_point3 = " + actXET.E_da_point3.ToString());
                    listBox1.Items.Add("[1]e_da_point4 = " + actXET.E_da_point4.ToString());
                    listBox1.Items.Add("[1]e_da_point5 = " + actXET.E_da_point5.ToString());
                    listBox1.Items.Add("[3]e_ad_zero   = " + actXET.E_ad_zero.ToString());
                    listBox1.Items.Add("[3]e_ad_full   = " + actXET.E_ad_full.ToString());
                    listBox1.Items.Add("[3]e_da_zero   = " + actXET.E_da_zero.ToString());
                    listBox1.Items.Add("[3]e_da_full   = " + actXET.E_da_full.ToString());
                    listBox1.Items.Add("----------------");
                    listBox1.Items.Add("e_vtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                    listBox1.Items.Add("e_wtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                    listBox1.Items.Add("e_atio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                    listBox1.Items.Add("e_btio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                    listBox1.Items.Add("e_ctio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                    listBox1.Items.Add("e_dtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                    listBox1.Items.Add("----------------");
                    break;
                case ECVE.CELTED:
                case ECVE.CELTER:
                    listBox1.Items.Add("[2]e_input1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input2   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input3   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input4   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                    listBox1.Items.Add("[2]e_input5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                    listBox1.Items.Add("[7]e_input6   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V");
                    listBox1.Items.Add("[7]e_input7   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V");
                    listBox1.Items.Add("[7]e_input8   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V");
                    listBox1.Items.Add("[7]e_input9   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V");
                    listBox1.Items.Add("[7]e_input10  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V");
                    listBox1.Items.Add("[8]e_input11  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V");
                    listBox1.Items.Add("----------------");
                    listBox1.Items.Add("[2]e_analog1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[2]e_analog5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[7]e_analog6  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[7]e_analog7  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[7]e_analog8  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[7]e_analog9  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[7]e_analog10 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                    listBox1.Items.Add("[8]e_analog11 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
                    listBox1.Items.Add("e_sens        = " + sens);
                    listBox1.Items.Add("----------------");
                    listBox1.Items.Add("[1]e_ad_point1  = " + actXET.E_ad_point1.ToString());
                    listBox1.Items.Add("[1]e_ad_point2  = " + actXET.E_ad_point2.ToString());
                    listBox1.Items.Add("[1]e_ad_point3  = " + actXET.E_ad_point3.ToString());
                    listBox1.Items.Add("[1]e_ad_point4  = " + actXET.E_ad_point4.ToString());
                    listBox1.Items.Add("[1]e_ad_point5  = " + actXET.E_ad_point5.ToString());
                    listBox1.Items.Add("[1]e_ad_point6  = " + actXET.E_ad_point6.ToString());
                    listBox1.Items.Add("[1]e_ad_point7  = " + actXET.E_ad_point7.ToString());
                    listBox1.Items.Add("[1]e_ad_point8  = " + actXET.E_ad_point8.ToString());
                    listBox1.Items.Add("[1]e_ad_point9  = " + actXET.E_ad_point9.ToString());
                    listBox1.Items.Add("[1]e_ad_point10 = " + actXET.E_ad_point10.ToString());
                    listBox1.Items.Add("[8]e_ad_point11 = " + actXET.E_ad_point11.ToString());
                    listBox1.Items.Add("[1]e_da_point1  = " + actXET.E_da_point1.ToString());
                    listBox1.Items.Add("[1]e_da_point2  = " + actXET.E_da_point2.ToString());
                    listBox1.Items.Add("[1]e_da_point3  = " + actXET.E_da_point3.ToString());
                    listBox1.Items.Add("[1]e_da_point4  = " + actXET.E_da_point4.ToString());
                    listBox1.Items.Add("[1]e_da_point5  = " + actXET.E_da_point5.ToString());
                    listBox1.Items.Add("[1]e_da_point6  = " + actXET.E_da_point6.ToString());
                    listBox1.Items.Add("[1]e_da_point7  = " + actXET.E_da_point7.ToString());
                    listBox1.Items.Add("[1]e_da_point8  = " + actXET.E_da_point8.ToString());
                    listBox1.Items.Add("[1]e_da_point9  = " + actXET.E_da_point9.ToString());
                    listBox1.Items.Add("[1]e_da_point10 = " + actXET.E_da_point10.ToString());
                    listBox1.Items.Add("[8]e_da_point11 = " + actXET.E_da_point11.ToString());
                    listBox1.Items.Add("[3]e_ad_zero    = " + actXET.E_ad_zero.ToString());
                    listBox1.Items.Add("[3]e_ad_full    = " + actXET.E_ad_full.ToString());
                    listBox1.Items.Add("[3]e_da_zero    = " + actXET.E_da_zero.ToString());
                    listBox1.Items.Add("[3]e_da_full    = " + actXET.E_da_full.ToString());
                    listBox1.Items.Add("----------------");
                    listBox1.Items.Add("e_vtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                    listBox1.Items.Add("e_wtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                    listBox1.Items.Add("e_atio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                    listBox1.Items.Add("e_btio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                    listBox1.Items.Add("e_ctio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                    listBox1.Items.Add("e_dtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                    listBox1.Items.Add("e_etio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_etio).ToString());
                    listBox1.Items.Add("e_ftio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ftio).ToString());
                    listBox1.Items.Add("e_gtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_gtio).ToString());
                    listBox1.Items.Add("e_htio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_htio).ToString());
                    listBox1.Items.Add("e_itio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_itio).ToString());
                    listBox1.Items.Add("e_jtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_jtio).ToString());
                    listBox1.Items.Add("----------------");
                    break;
            }
            listBox1.Items.Add("[4]e_da_zero_4ma  = " + actXET.E_da_zero_4ma.ToString());
            listBox1.Items.Add("[4]e_da_full_20ma = " + actXET.E_da_full_20ma.ToString());
            listBox1.Items.Add("[4]e_da_zero_05V  = " + actXET.E_da_zero_05V.ToString());
            listBox1.Items.Add("[4]e_da_full_05V  = " + actXET.E_da_full_05V.ToString());
            listBox1.Items.Add("[4]e_da_zero_10V  = " + actXET.E_da_zero_10V.ToString());
            listBox1.Items.Add("[4]e_da_full_10V  = " + actXET.E_da_full_10V.ToString());
            listBox1.Items.Add("[4]e_da_zero_N5   = " + actXET.E_da_zero_N5.ToString());
            listBox1.Items.Add("[4]e_da_full_P5   = " + actXET.E_da_full_P5.ToString());
            listBox1.Items.Add("[4]e_da_zero_N10  = " + actXET.E_da_zero_N10.ToString());
            listBox1.Items.Add("[4]e_da_full_P10  = " + actXET.E_da_full_P10.ToString());
            listBox1.Items.Add("----------------");
            listBox1.Items.Add("[5]e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
            listBox1.Items.Add("[5]e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
            listBox1.Items.Add("----------------");
        }

        //恢复出厂
        private void bt_reset_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.REST;

            bt_reset.HoverBackColor = Color.Firebrick;
            bt_bohrcode.HoverBackColor = button1.HoverBackColor;
            bt_outype.HoverBackColor = button1.HoverBackColor;
            bt_fact_write.HoverBackColor = button1.HoverBackColor;
            bt_zero_sample.HoverBackColor = button1.HoverBackColor;
            bt_full_sample.HoverBackColor = button1.HoverBackColor;
            bt_cal_write.HoverBackColor = button1.HoverBackColor;
            bt_cal_write.Text = MyDevice.languageType == 0 ? "写 入" : "Write";

            actXET.E_test = 0xFF;
            actXET.E_mfg_date = Convert.ToUInt32(System.DateTime.Now.ToString("yyMMddHHmm"));

            MyDevice.mePort_ClearState();
            MyDevice.mePort_WriteTypTasks();
        }

        //RDX+BOR
        private void bt_bohrcode_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.RDX0;//读出所有SCT

            bt_reset.HoverBackColor = button1.HoverBackColor;
            bt_bohrcode.HoverBackColor = Color.Firebrick;
            bt_outype.HoverBackColor = button1.HoverBackColor;
            bt_fact_write.HoverBackColor = button1.HoverBackColor;
            bt_zero_sample.HoverBackColor = button1.HoverBackColor;
            bt_full_sample.HoverBackColor = button1.HoverBackColor;
            bt_cal_write.HoverBackColor = button1.HoverBackColor;
            bt_cal_write.Text = MyDevice.languageType == 0 ? "写 入" : "Write";

            MyDevice.mePort_ClearState();
            MyDevice.mePort_ReadTasks();
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

                    if (actXET.S_HalfCal)
                    {
                        textBox4.Text = actXET.E_da_zero_05V.ToString();
                    }

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

        //Mid++
        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GOUPM);
            }
        }

        //Mid--
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (uiTask == TASKS.NULL)
            {
                bt_fact_write.HoverBackColor = Color.Firebrick;
                Refresh();

                MyDevice.mePort_SendCOM(TASKS.GODMM);
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
                            //T420型号 中点校准
                            if (actXET.S_HalfCal)
                            {
                                correct = (int)(dacset * 12.0f / output + 0.5f);
                                textBox4.Text = correct.ToString();
                            }
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
                    //T420型号 中点校准 使用E_da_zero_05V
                    if (actXET.S_HalfCal)
                    {
                        actXET.E_da_zero_05V = Convert.ToInt32(textBox4.Text);
                    }
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

        //零点采样
        private void bt_zero_sample_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.NULL;
            bt_zero_sample.HoverBackColor = Color.Firebrick;
            Refresh();
            MyDevice.mePort_SendCOM(TASKS.ADCP1);
        }

        //满点采样
        private void bt_full_sample_Click(object sender, EventArgs e)
        {
            uiTask = TASKS.NULL;
            bt_full_sample.HoverBackColor = Color.Firebrick;
            Refresh();
            MyDevice.mePort_SendCOM(TASKS.ADCP5);
        }

        //标定写入
        private void bt_cal_write_Click(object sender, EventArgs e)
        {
            if (actXET.E_bohrcode == -1)
            {
                MessageBox.Show("工厂未校准!");
            }
            else
            {
                uiTask = TASKS.WRX1;//标定后写入
                bt_cal_write.Text = "BCC";
                bt_cal_write.HoverBackColor = Color.Firebrick;
                MyDevice.mePort_ClearState();
                MyDevice.mePort_WriteCalTasks();
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
                    case TASKS.REST:
                        label13.Text = MyDevice.protocol.trTASK.ToString() + " " + MyDevice.protocol.isEqual.ToString() + " " + MyDevice.protocol.rxString;
                        //WRX0 -> RST
                        MyDevice.mePort_WriteTypTasks();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            uiTask = TASKS.RDX0;
                            bt_reset.HoverBackColor = Color.Green;
                        }
                        break;

                    //读出所有SCT
                    case TASKS.RDX0:
                        label13.Text = MyDevice.protocol.trTASK.ToString();
                        //BOR -> RDX0 -> RDX1 -> RDX2 -> RDX3 -> RDX4 -> RDX5 -> RDX6 -> RDX7 -> RDX8
                        MyDevice.mePort_ReadTasks();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            uiTask = TASKS.BOR;
                            bt_bohrcode.HoverBackColor = Color.Firebrick;
                            MyDevice.mePort_SendCOM(TASKS.BOR);
                        }
                        break;

                    //更新Bohrcode
                    case TASKS.BOR:
                        label13.Text = MyDevice.protocol.trTASK.ToString();
                        //成功
                        uiTask = TASKS.NULL;
                        bt_bohrcode.HoverBackColor = Color.Green;
                        paralist_Checking();
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
                            paralist_Checking();
                        }
                        break;

                    //标定后写入
                    case TASKS.WRX1:
                        label13.Text = MyDevice.protocol.trTASK.ToString() + " " + MyDevice.protocol.isEqual.ToString() + " " + MyDevice.protocol.rxString;
                        //BCC -> WRX1 -> WRX2 -> WRX3 -> WRX6 -> WRX7 -> WRX8-> (rst) -> NULL
                        MyDevice.mePort_WriteCalTasks();
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            uiTask = TASKS.NULL;
                            bt_cal_write.Text = "成 功";
                            bt_cal_write.HoverBackColor = Color.Green;
                            paralist_Checking();
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
                            paralist_Checking();
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

                            //校准中点(仅T420型号)
                            case TASKS.GOUPM:
                            case TASKS.GODMM:
                                textBox4.Text = MyDevice.protocol.rxString;
                                bt_fact_write.HoverBackColor = Color.Firebrick;
                                break;

                            //校准零点
                            case TASKS.GOUPZ:
                            case TASKS.GODMZ:
                                textBox2.Text = MyDevice.protocol.rxString;
                                bt_fact_write.HoverBackColor = Color.Firebrick;
                                break;

                            //零点采样
                            case TASKS.ADCP1:
                                actXET.RefreshRatio();
                                paralist_Checking();
                                bt_zero_sample.HoverBackColor = Color.Green;
                                break;

                            //满点采样
                            case TASKS.ADCP5:
                                actXET.RefreshRatio();
                                paralist_Checking();
                                bt_full_sample.HoverBackColor = Color.Green;
                                break;
                        }
                        break;
                }

                Refresh();
            }
        }
    }
}
