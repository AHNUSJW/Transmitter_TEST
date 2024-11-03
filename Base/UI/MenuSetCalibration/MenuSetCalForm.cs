using Library;
using Model;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//Alvin 20230414
//Ricardo 20230704
//Junzhe 20230926
//Lumi 20231222

//step1
//连接后读出 -> input_Checking和output_Checking -> 更新TextBox.text=T_input/T_analog

//step2
//模拟量输出修改或标定加载值修改 -> 更新T_analog=TextBox.text -> 更新E_analogX

//step3
//采集 -> 串口ReceiveLong -> 更新E_ad_pointX和E_inputX -> 更新TextBox.text=T_input
//灵敏度输入 -> 更新T_input=TextBox.text -> 更新E_ad_pointX和E_inputX

//step4
//input_Leave和output_Leave -> 计算斜率

//step5
//写入并读出校验

namespace Base.UI.MenuSet
{
    public partial class MenuSetCalForm : Form
    {
        private AutoFormSize autoFormSize = new AutoFormSize();              //自适应屏幕分辨率

        private XET actXET;//需要操作的设备
        private TASKS nextTask;//按键指令

        //进程保存的ad_point值
        private Int32 ui_ad_point_f1;
        private Int32 ui_ad_point_f2;
        private Int32 ui_ad_point_f3;
        private Int32 ui_ad_point_f4;
        private Int32 ui_ad_point_f5;
        private Int32 ui_ad_point_f6;
        private Int32 ui_ad_point_f7;
        private Int32 ui_ad_point_f8;
        private Int32 ui_ad_point_f9;
        private Int32 ui_ad_point_f10;
        private Int32 ui_ad_point_f11;

        //返程保存的ad_ponit值
        private Int32 ui_ad_point_b1;
        private Int32 ui_ad_point_b2;
        private Int32 ui_ad_point_b3;
        private Int32 ui_ad_point_b4;
        private Int32 ui_ad_point_b5;
        private Int32 ui_ad_point_b6;
        private Int32 ui_ad_point_b7;
        private Int32 ui_ad_point_b8;
        private Int32 ui_ad_point_b9;
        private Int32 ui_ad_point_b10;
        private Int32 ui_ad_point_b11;

        //进程保存的input值
        private Int32 ui_input_f1;
        private Int32 ui_input_f2;
        private Int32 ui_input_f3;
        private Int32 ui_input_f4;
        private Int32 ui_input_f5;
        private Int32 ui_input_f6;
        private Int32 ui_input_f7;
        private Int32 ui_input_f8;
        private Int32 ui_input_f9;
        private Int32 ui_input_f10;
        private Int32 ui_input_f11;

        //返程保存的input值
        private Int32 ui_input_b1;
        private Int32 ui_input_b2;
        private Int32 ui_input_b3;
        private Int32 ui_input_b4;
        private Int32 ui_input_b5;
        private Int32 ui_input_b6;
        private Int32 ui_input_b7;
        private Int32 ui_input_b8;
        private Int32 ui_input_b9;
        private Int32 ui_input_b10;
        private Int32 ui_input_b11;

        public MenuSetCalForm()
        {
            InitializeComponent();
            //
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        #region 窗口控制

        /// <summary>
        /// 加载先登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMenuSetCalForm_Load(object sender, EventArgs e)
        {
            //界面随分辨率调整
            autoFormSize.UIComponetForm(this);

            if (this.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                double ratio = (double)this.Width / this.Height;
                this.Height = (int)(this.Height * (int)((double)Screen.PrimaryScreen.WorkingArea.Width / 1920));
                this.Width = (int)(ratio * this.Height);
            }
            else if (this.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                double ratio = (double)this.Width / this.Height;
                this.Height = (int)(this.Height * (double)Screen.PrimaryScreen.Bounds.Height / 1080);
                this.Width = (int)(ratio * this.Height);
            }

            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //需要标定的设备
            actXET = MyDevice.actDev;

            //智能检查故障
            actXET.RefreshCalInfo();

            //调整界面内容
            curve_Checking();

            //连接后读出 -> input_Checking和output_Checking -> 更新TextBox.text=T_input/T_analog
            input_Checking();
            output_Checking();

            //更新列表参数
            paralist_Checking();

            //错误信息提示
            label34.ForeColor = Color.Firebrick;
            label34.Text = actXET.R_errSensor;
            label35.Text = actXET.R_resolution;

            switch (MyDevice.protocol.type)
            {
                default:
                case COMP.SelfUART:
                case COMP.CANopen:
                    panel6.Visible = false;
                    panel7.Location = panel6.Location;
                    break;

                case COMP.RS485:
                case COMP.ModbusTCP:
                    if (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重")
                    {
                        //显示实时重量
                        Disp_Weight();
                        //获取实时数据
                        MyDevice.mePort_SendCOM(TASKS.QGROSS);
                    }
                    else
                    {
                        panel6.Visible = false;
                        panel7.Location = panel6.Location;
                    }
                    break;
            }

            if (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重")
            {
                panel6.BackColor = Color.White;
            }
            else
            {
                panel6.BackColor = SystemColors.ControlLight;
                signalStable1.LampColor = new Color[] { Color.Black };
                signalStable1.BackColor = SystemColors.ControlLight;
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMenuSetCalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        /// <summary>
        /// 限制最小窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMenuSetCalForm_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Width < 957) this.Width = 960;
            if (this.Height < 710) this.Height = 710;
        }

        #endregion

        #region 界面更新

        //两点法-加载采集标定法
        private void show_TwoAcq()
        {
            //
            this.panel1.Visible = false; //加载（第1~11个点）
            this.labelload.Visible = false; //灵敏度
            this.labelloadunit.Visible = false; //范围: ±3.5mV/V

            //
            this.panel2.Visible = true; //
            this.label6.Visible = true; //第一点输出：
            this.label7.Visible = false; //第二点输出：
            this.label8.Visible = false; //第三点输出：
            this.label9.Visible = false; //第四点输出：
            this.label10.Visible = true; //第五点输出：
            this.textBox12.Visible = true; //output1
            this.textBox13.Visible = false; //output2
            this.textBox14.Visible = false; //output3
            this.textBox15.Visible = false; //output4
            this.textBox16.Visible = true; //output5
            this.panel4.Visible = false; //输出（第6~11个点）
            this.labelout.Visible = true; //设置输出
            this.labeloutunit.Visible = true; //范围: 4-20mA,±10V

            //
            this.label11.Visible = false; //< =>
            this.label12.Visible = false; //< =>
            this.label13.Visible = false; //< =>
            this.label14.Visible = false; //< =>
            this.label15.Visible = false; //< =>
            this.label28.Visible = false; //< =>
            this.label29.Visible = false; //< =>
            this.label30.Visible = false; //< =>
            this.label31.Visible = false; //< =>
            this.label32.Visible = false; //< =>
            this.label33.Visible = false; //< =>

            //
            this.button1.Visible = true; //采 集 1
            this.button2.Visible = false; //采 集 2
            this.button3.Visible = false; //采 集 3
            this.button4.Visible = false; //采 集 4
            this.button5.Visible = true; //采 集 5
            this.button6.Visible = false; //采 集 6
            this.button7.Visible = false; //采 集 7
            this.button8.Visible = false; //采 集 8
            this.button9.Visible = false; //采 集 9
            this.button10.Visible = false; //采 集 10
            this.button11.Visible = false; //采 集 11
            this.bt_Write.Visible = true; //写 入
            this.bt_InputProtected.Visible = false; //保 护
            this.bt_OutputProtected.Visible = true; //保 护

            //
            this.panel5.Visible = false; //单程进程返程
        }

        //五点法-加载采集标定法
        private void show_FiveAcq()
        {
            //
            this.panel1.Visible = false; // //加载（第1~11个点）
            this.labelload.Visible = false; //灵敏度
            this.labelloadunit.Visible = false; //范围: ±3.5mV/V
            //
            this.panel2.Visible = true; //
            this.label6.Visible = true; //第一点输出：
            this.label7.Visible = true; //第二点输出：
            this.label8.Visible = true; //第三点输出：
            this.label9.Visible = true; //第四点输出：
            this.label10.Visible = true; //第五点输出：
            this.textBox12.Visible = true; //output1
            this.textBox13.Visible = true; //output2
            this.textBox14.Visible = true; //output3
            this.textBox15.Visible = true; //output4
            this.textBox16.Visible = true; //output5
            this.panel4.Visible = false; //输出（第6~11个点）
            this.labelout.Visible = true; //设置输出
            this.labeloutunit.Visible = true; //范围: 4-20mA,±10V
            //
            this.label11.Visible = false; //< =>
            this.label12.Visible = false; //< =>
            this.label13.Visible = false; //< =>
            this.label14.Visible = false; //< =>
            this.label15.Visible = false; //< =>
            this.label28.Visible = false; //< =>
            this.label29.Visible = false; //< =>
            this.label30.Visible = false; //< =>
            this.label31.Visible = false; //< =>
            this.label32.Visible = false; //< =>
            this.label33.Visible = false; //< =>
            //
            this.button1.Visible = true; //采 集 1
            this.button2.Visible = true; //采 集 2
            this.button3.Visible = true; //采 集 3
            this.button4.Visible = true; //采 集 4
            this.button5.Visible = true; //采 集 5
            this.button6.Visible = false; //采 集 6
            this.button7.Visible = false; //采 集 7
            this.button8.Visible = false; //采 集 8
            this.button9.Visible = false; //采 集 9
            this.button10.Visible = false; //采 集 10
            this.button11.Visible = false; //采 集 11
            //
            this.bt_InputProtected.Visible = false; //保 护
            this.bt_OutputProtected.Visible = true; //保 护
            //
            //仅型号为TP10时才显示单程进程返程
            if (actXET.S_DeviceType == TYPE.TP10)
            {
                this.panel5.Visible = true; //显示单程进程返程
                this.radioButtonSingle.Checked = true;

                actXET.ecveType = ECVEType.Single;//默认初始状态
            }
            else
            {
                this.panel5.Visible = false; //非TP10型号不显示单程进程返程
            }
        }

        //十一点法-加载采集标定法
        private void show_ElevenAcq()
        {
            //调用五点法-加载采集标定法
            show_FiveAcq();
            //在五点法基础上修改界面
            this.panel4.Visible = true; //输出（第6~11个点）
            //
            this.button6.Visible = true; //采 集 6
            this.button7.Visible = true; //采 集 7
            this.button8.Visible = true; //采 集 8
            this.button9.Visible = true; //采 集 9
            this.button10.Visible = true; //采 集 10
            this.button11.Visible = true; //采 集 11
        }

        //两点法-免标定直接输入
        private void show_TwoInput()
        {
            //
            this.panel1.Visible = true; //
            this.label1.Visible = true; //第一点加载：
            this.label2.Visible = false; //第二点加载：
            this.label3.Visible = false; //第三点加载：
            this.label4.Visible = false; //第四点加载：
            this.label5.Visible = true; //第五点加载：
            this.textBox1.Visible = true; //input1
            this.textBox2.Visible = false; //input2
            this.textBox3.Visible = false; //input3
            this.textBox4.Visible = false; //input4
            this.textBox5.Visible = true; //input5
            this.panel3.Visible = false; //加载（第6~11个点）
            this.labelload.Visible = true; //灵敏度
            this.labelloadunit.Visible = true; //范围: ±3.5mV/V
            //
            this.panel2.Visible = true; //
            this.label6.Visible = true; //第一点输出：
            this.label7.Visible = false; //第二点输出：
            this.label8.Visible = false; //第三点输出：
            this.label9.Visible = false; //第四点输出：
            this.label10.Visible = true; //第五点输出：
            this.textBox12.Visible = true; //output1
            this.textBox13.Visible = false; //output2
            this.textBox14.Visible = false; //output3
            this.textBox15.Visible = false; //output4
            this.textBox16.Visible = true; //output5
            this.panel4.Visible = false; //输出（第6~11个点）
            this.labelout.Visible = true; //设置输出
            this.labeloutunit.Visible = true; //范围: 4-20mA,±10V
            //
            this.label11.Visible = true; //< =>
            this.label12.Visible = false; //< =>
            this.label13.Visible = false; //< =>
            this.label14.Visible = false; //< =>
            this.label15.Visible = true; //< =>
            this.label28.Visible = false; //< =>
            this.label29.Visible = false; //< =>
            this.label30.Visible = false; //< =>
            this.label31.Visible = false; //< =>
            this.label32.Visible = false; //< =>
            this.label33.Visible = false; //< =>
            //
            this.button1.Visible = false; //采 集 1
            this.button2.Visible = false; //采 集 2
            this.button3.Visible = false; //采 集 3
            this.button4.Visible = false; //采 集 4
            this.button5.Visible = false; //采 集 5
            this.button6.Visible = false; //采 集 6
            this.button7.Visible = false; //采 集 7
            this.button8.Visible = false; //采 集 8
            this.button9.Visible = false; //采 集 9
            this.button10.Visible = false; //采 集 10
            this.button11.Visible = false; //采 集 11
            this.bt_Write.Visible = true; //写 入
            this.bt_InputProtected.Visible = true; //保 护
            this.bt_OutputProtected.Visible = true; //保 护
            //
            this.panel5.Visible = false; //单程进程返程
        }

        //五点法-免标定直接输入
        private void show_FiveInput()
        {
            //
            this.panel1.Visible = true; //
            this.label1.Visible = true; //第一点加载：
            this.label2.Visible = true; //第二点加载：
            this.label3.Visible = true; //第三点加载：
            this.label4.Visible = true; //第四点加载：
            this.label5.Visible = true; //第五点加载：
            this.textBox1.Visible = true; //input1
            this.textBox2.Visible = true; //input2
            this.textBox3.Visible = true; //input3
            this.textBox4.Visible = true; //input4
            this.textBox5.Visible = true; //input5
            this.panel3.Visible = false; //加载（第6~11个点）
            this.labelload.Visible = true; //灵敏度
            this.labelloadunit.Visible = true; //范围: ±3.5mV/V
            //
            this.panel2.Visible = true; //
            this.label6.Visible = true; //第一点输出：
            this.label7.Visible = true; //第二点输出：
            this.label8.Visible = true; //第三点输出：
            this.label9.Visible = true; //第四点输出：
            this.label10.Visible = true; //第五点输出：
            this.textBox12.Visible = true; //output1
            this.textBox13.Visible = true; //output2
            this.textBox14.Visible = true; //output3
            this.textBox15.Visible = true; //output4
            this.textBox16.Visible = true; //output5
            this.panel4.Visible = false; //输出（6~11个点）
            this.labelout.Visible = true; //设置输出
            this.labeloutunit.Visible = true; //范围: 4-20mA,±10V
            //
            this.label11.Visible = true; //< =>
            this.label12.Visible = true; //< =>
            this.label13.Visible = true; //< =>
            this.label14.Visible = true; //< =>
            this.label15.Visible = true; //< =>
            this.label28.Visible = false; //< =>
            this.label29.Visible = false; //< =>
            this.label30.Visible = false; //< =>
            this.label31.Visible = false; //< =>
            this.label32.Visible = false; //< =>
            this.label33.Visible = false; //< =>
            //
            this.button1.Visible = false; //采 集 1
            this.button2.Visible = false; //采 集 2
            this.button3.Visible = false; //采 集 3
            this.button4.Visible = false; //采 集 4
            this.button5.Visible = false; //采 集 5
            this.button6.Visible = false; //采 集 6
            this.button7.Visible = false; //采 集 7
            this.button8.Visible = false; //采 集 8
            this.button9.Visible = false; //采 集 9
            this.button10.Visible = false; //采 集 10
            this.button11.Visible = false; //采 集 11
            this.bt_Write.Visible = true; //写 入
            this.bt_InputProtected.Visible = true; //保 护
            this.bt_OutputProtected.Visible = true; //保 护
            //
            this.panel5.Visible = false; //单程进程返程
        }

        //十一点法-免标定直接输入
        private void show_ElevenInput()
        {
            //调用五点法-免标定直接输入
            show_FiveInput();
            //在五点法基础上修改界面
            this.panel3.Visible = true; //加载（第6~11个点）
            this.panel3.BringToFront();
            this.panel4.Visible = true; //输出（第6~11个点）
            this.panel4.BringToFront();
            //
            this.label28.Visible = true; //< =>
            this.label29.Visible = true; //< =>
            this.label30.Visible = true; //< =>
            this.label31.Visible = true; //< =>
            this.label32.Visible = true; //< =>
            this.label33.Visible = true; //< =>
            //
            this.panel5.Visible = false; //单程进程返程
        }

        //两点法-加载采集标定法
        private void local_TwoPoint()
        {
            int space = button2.Location.Y - button1.Location.Y;
            //
            //this.panel1.Size = new System.Drawing.Size(200, space * 2);
            this.label5.Location = new Point(label2.Location.X, label2.Location.Y); //第五点加载：
            this.textBox5.Location = new Point(textBox2.Location.X, textBox2.Location.Y); //input5
            this.labelloadunit.Location = new Point(labelloadunit.Location.X, button4.Location.Y - 35); //范围: ±3.5mV/V
            //
            //this.panel2.Size = new System.Drawing.Size(200, space * 2);
            this.label10.Location = new Point(label7.Location.X, label7.Location.Y); //第五点输出：
            this.textBox16.Location = new Point(textBox13.Location.X, textBox13.Location.Y); //output5
            this.labeloutunit.Location = new Point(labeloutunit.Location.X, button4.Location.Y - 35); //范围: 4-20mA,±10V
            //
            this.label15.Location = new Point(label15.Location.X, label12.Location.Y);//< =>
            //
            this.button5.Location = new Point(button5.Location.X, button2.Location.Y); //采 集 5
            this.bt_Write.Location = new Point(bt_Write.Location.X, button3.Location.Y); //写入
        }

        //五点法-加载采集标定法
        private void local_FivePoint()
        {
            int space = button2.Location.Y - button1.Location.Y;
            //
            //this.panel1.Size = new System.Drawing.Size(200, space * 5);
            this.label5.Location = new Point(label1.Location.X, label4.Location.Y + space); //第五点加载：
            this.textBox5.Location = new Point(textBox1.Location.X, textBox4.Location.Y + space); //input5
            this.labelloadunit.Location = new Point(labelloadunit.Location.X, button6.Location.Y); //范围: ±3.5mV/V
            //
            //this.panel2.Size = new System.Drawing.Size(200, space * 5);
            this.label10.Location = new Point(label6.Location.X, label9.Location.Y + space); //第五点输出：
            this.textBox16.Location = new Point(textBox12.Location.X, textBox15.Location.Y + space); //output5
            this.labeloutunit.Location = new Point(labeloutunit.Location.X, button6.Location.Y); //范围: 4-20mA,±10V
            //
            this.label15.Location = new Point(label15.Location.X, label14.Location.Y + space);//< =>
            //
            this.button5.Location = new Point(button5.Location.X, button4.Location.Y + space); //采 集 5
            this.bt_Write.Location = new Point(bt_Write.Location.X, button6.Location.Y); //写入
        }

        //十一点法-加载采集标定法
        private void local_ElevenPoint()
        {
            int space = button2.Location.Y - button1.Location.Y;
            //
            //this.panel1.Size = new System.Drawing.Size(200, space * 5);
            this.label5.Location = new Point(label1.Location.X, label4.Location.Y + space); //第五点加载：
            this.textBox5.Location = new Point(textBox1.Location.X, textBox4.Location.Y + space); //input5
            this.labelloadunit.Location = new Point(labelloadunit.Location.X, button11.Location.Y + space); //范围: ±3.5mV/V
            //
            //this.panel2.Size = new System.Drawing.Size(200, space * 5);
            this.label10.Location = new Point(label6.Location.X, label9.Location.Y + space); //第五点输出：
            this.textBox16.Location = new Point(textBox12.Location.X, textBox15.Location.Y + space); //output5
            this.labeloutunit.Location = new Point(labeloutunit.Location.X, button11.Location.Y + space); //范围: 4-20mA,±10V
            //
            this.label15.Location = new Point(label15.Location.X, label14.Location.Y + space);//< =>
            //
            this.button5.Location = new Point(button5.Location.X, button4.Location.Y + space); //采 集 5
            this.bt_Write.Location = new Point(bt_Write.Location.X, button11.Location.Y + space); //写入
        }

        //两点标定界面文字
        private void fresh_ZFform()
        {
            if (MyDevice.languageType == 0)//语言 0：中文   1：英文
            {
                label1.Text = "零点加载：";
                label5.Text = "满点加载：";
                label6.Text = "零点输出：";
                label10.Text = "满点输出：";
                button1.Text = "零点采集";
                button5.Text = "满点采集";
            }
            else
            {
                label1.Text = "       Zero loading:";
                label5.Text = "       Full loading:";
                label6.Text = "       Zero output:";
                label10.Text = "       Full output:";
                button1.Text = "Zero";
                button5.Text = "Full";
            }
        }

        //多点标定界面文字
        private void fresh_RecoverForm()
        {
            if (MyDevice.languageType == 0)//语言 0：中文   1：英文
            {
                label1.Text = "第一点加载：";
                label5.Text = "第五点加载：";
                label6.Text = "第一点输出：";
                label10.Text = "第五点输出：";
                button1.Text = "采集1";
                button5.Text = "采集5";
            }
            else
            {
                label1.Text = "First point loading:";
                label5.Text = "Fifth point loading:";
                label6.Text = "First point output:";
                label10.Text = "Fifth point output:";
                button1.Text = "Collect1";
                button5.Text = "Collect5";
            }
        }

        //根据标定算法调整界面
        private void curve_Checking()
        {
            switch ((ECVE)actXET.E_curve)
            {
                case ECVE.CTWOPT:
                    //更新界面
                    fresh_ZFform();
                    //
                    local_TwoPoint();
                    //加载采集标定法
                    if (radioButton1.Checked == true)
                    {
                        show_TwoAcq();
                    }
                    //免标定直接输入
                    else
                    {
                        show_TwoInput();
                    }
                    break;
                case ECVE.CFITED:
                case ECVE.CINTER:
                    //更新界面
                    fresh_RecoverForm();
                    //
                    local_FivePoint();
                    //加载采集标定法
                    if (radioButton1.Checked == true)
                    {
                        show_FiveAcq();
                    }
                    //免标定直接输入
                    else
                    {
                        show_FiveInput();
                    }
                    break;
                case ECVE.CELTED:
                case ECVE.CELTER:
                    //更新界面
                    fresh_RecoverForm();
                    //
                    local_ElevenPoint();
                    //加载采集标定法
                    if (radioButton1.Checked == true)
                    {
                        show_ElevenAcq();
                    }
                    //免标定直接输入
                    else
                    {
                        show_ElevenInput();
                    }
                    break;
                default:
                    //更新界面
                    fresh_RecoverForm();
                    //
                    local_FivePoint();
                    //加载采集标定法
                    if (radioButton1.Checked == true)
                    {
                        show_FiveAcq();
                    }
                    //免标定直接输入
                    else
                    {
                        show_FiveInput();
                    }
                    break;
            }
        }

        //更新界面参数
        private void input_Checking()
        {
            //灵敏度
            textBox1.Text = actXET.T_input1;
            textBox2.Text = actXET.T_input2;
            textBox3.Text = actXET.T_input3;
            textBox4.Text = actXET.T_input4;
            textBox5.Text = actXET.T_input5;
            textBox6.Text = actXET.T_input6;
            textBox7.Text = actXET.T_input7;
            textBox8.Text = actXET.T_input8;
            textBox9.Text = actXET.T_input9;
            textBox10.Text = actXET.T_input10;
            textBox11.Text = actXET.T_input11;

            //mV/V
            if (MyDevice.languageType == 0)
            {
                switch (actXET.E_adspeed & 0x0F)
                {
                    case (Byte)EPGA.ADPGA1: labelloadunit.Text = "范围: ±400mV/V"; break;
                    case (Byte)EPGA.ADPGA2: labelloadunit.Text = "范围: ±200mV/V"; break;
                    case (Byte)EPGA.ADPGA64: labelloadunit.Text = "范围: ±7.0mV/V"; break;
                    case (Byte)EPGA.ADPGA128: labelloadunit.Text = "范围: ±3.5mV/V"; break;
                    default: labelloadunit.Text = "范围: ±3.5mV/V"; break;
                }

            }
            else
            {
                switch (actXET.E_adspeed & 0x0F)
                {
                    case (Byte)EPGA.ADPGA1: labelloadunit.Text = "range: ±400mV/V"; break;
                    case (Byte)EPGA.ADPGA2: labelloadunit.Text = "range: ±200mV/V"; break;
                    case (Byte)EPGA.ADPGA64: labelloadunit.Text = "range: ±7.0mV/V"; break;
                    case (Byte)EPGA.ADPGA128: labelloadunit.Text = "range: ±3.5mV/V"; break;
                    default: labelloadunit.Text = "range: ±3.5mV/V"; break;
                }
            }
        }

        //更新界面参数
        private void output_Checking()
        {
            //输出
            textBox12.Text = actXET.T_analog1;
            textBox13.Text = actXET.T_analog2;
            textBox14.Text = actXET.T_analog3;
            textBox15.Text = actXET.T_analog4;
            textBox16.Text = actXET.T_analog5;
            textBox17.Text = actXET.T_analog6;
            textBox18.Text = actXET.T_analog7;
            textBox19.Text = actXET.T_analog8;
            textBox20.Text = actXET.T_analog9;
            textBox21.Text = actXET.T_analog10;
            textBox22.Text = actXET.T_analog11;

            //mA,V,kg
            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "范围: 4-20mA,±10V" : "range: 4-20mA,±10V";
                    break;

                case OUT.UTP05:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "范围: 0-5V" : "range: 0-5V";
                    break;

                case OUT.UTP10:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "范围: 0-10V" : "range: 0-10V";
                    break;

                case OUT.UTN05:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "范围: ±5V" : "range: ±5V";
                    break;

                case OUT.UTN10:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "范围: ±10V" : "range: ±10V";
                    break;

                case OUT.UMASK:
                    string unitStr;
                    unitStr = MyDevice.languageType == 0 ? "单位" : "Unit";
                    labeloutunit.Text = $"{unitStr} : {actXET.S_unit}";
                    break;

                default:
                    labeloutunit.Text = MyDevice.languageType == 0 ? "输出: mA,V" : "output: mA,V";
                    break;
            }
        }

        //更新显示参数
        private void paralist_Checking()
        {
            byte deci;
            string unit;
            string unitUmask = actXET.GetUnitUMASK();
            string unitUmaskSolo = UnitHelper.GetUnitAdjustedDescription((UNIT)actXET.E_wt_unit);//使数字量单位列不显示""
            string sens = actXET.RefreshSens();//灵敏度

            //超级账户
            if ((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            {
                deci = actXET.S_decimal;
                unit = " " + actXET.S_unit + ">";

                listBox1.Items.Clear();
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("TYPE = " + actXET.S_DeviceType.ToString());
                listBox1.Items.Add("OUT  = " + actXET.S_OutType.ToString());
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("[0]e_test      = 0x" + actXET.E_test.ToString("X2"));
                listBox1.Items.Add("[0]e_outype    = 0x" + actXET.E_outype.ToString("X2"));
                listBox1.Items.Add("[0]e_curve     = 0x" + actXET.E_curve.ToString("X2"));
                listBox1.Items.Add("[0]e_adspeed   = 0x" + actXET.E_adspeed.ToString("X2"));
                listBox1.Items.Add("[0]e_autozero  = 0x" + actXET.E_autozero.ToString("X2"));
                listBox1.Items.Add("[0]e_trackzero = 0x" + actXET.E_trackzero.ToString("X2"));
                listBox1.Items.Add("[0]e_checkhigh = " + actXET.E_checkhigh.ToString());
                listBox1.Items.Add("[0]e_checklow  = " + actXET.E_checklow.ToString());
                listBox1.Items.Add("[0]e_mfg_date  = " + actXET.E_mfg_date.ToString());
                listBox1.Items.Add("[0]e_mfg_srno  = " + actXET.E_mfg_srno.ToString());
                listBox1.Items.Add("[0]e_tmp_min   = " + actXET.S_tmp_min);
                listBox1.Items.Add("[0]e_tmp_max   = " + actXET.S_tmp_max);
                listBox1.Items.Add("[0]e_tmp_cal   = " + actXET.E_tmp_cal.ToString());
                listBox1.Items.Add("[0]e_bohrcode  = " + actXET.R_bohrcode_long.ToString("X8"));
                listBox1.Items.Add("[0]e_enspan    = " + actXET.E_enspan.ToString());
                listBox1.Items.Add("[0]e_protype   = " + actXET.E_protype.ToString());
                listBox1.Items.Add("----------------");
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CTWOPT:
                        listBox1.Items.Add("[2]e_input1    = " + actXET.E_input1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input5    = " + actXET.E_input5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_analog1   = " + actXET.E_analog1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog5   = " + actXET.E_analog5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
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
                        listBox1.Items.Add("[2]e_input1  = " + actXET.E_input1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input2  = " + actXET.E_input2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input3  = " + actXET.E_input3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input4  = " + actXET.E_input4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input5  = " + actXET.E_input5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_analog1 = " + actXET.E_analog1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog2 = " + actXET.E_analog2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog3 = " + actXET.E_analog3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog4 = " + actXET.E_analog4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog5 = " + actXET.E_analog5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
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
                        listBox1.Items.Add("[2]e_input1   = " + actXET.E_input1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input2   = " + actXET.E_input2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input3   = " + actXET.E_input3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input4   = " + actXET.E_input4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V>");
                        listBox1.Items.Add("[2]e_input5   = " + actXET.E_input5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V>");
                        listBox1.Items.Add("[7]e_input6   = " + actXET.E_input6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V>");
                        listBox1.Items.Add("[7]e_input7   = " + actXET.E_input7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V>");
                        listBox1.Items.Add("[7]e_input8   = " + actXET.E_input8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V>");
                        listBox1.Items.Add("[7]e_input9   = " + actXET.E_input9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V>");
                        listBox1.Items.Add("[7]e_input10  = " + actXET.E_input10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V>");
                        listBox1.Items.Add("[8]e_input11  = " + actXET.E_input11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V>");
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("[2]e_analog1  = " + actXET.E_analog1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog2  = " + actXET.E_analog2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog3  = " + actXET.E_analog3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog4  = " + actXET.E_analog4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[2]e_analog5  = " + actXET.E_analog5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[7]e_analog6  = " + actXET.E_analog6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[7]e_analog7  = " + actXET.E_analog7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[7]e_analog8  = " + actXET.E_analog8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[7]e_analog9  = " + actXET.E_analog9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[7]e_analog10 = " + actXET.E_analog10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                        listBox1.Items.Add("[8]e_analog11 = " + actXET.E_analog11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
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
                listBox1.Items.Add("[5]e_corr   = " + actXET.E_corr.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_corr).ToString() + ">");
                listBox1.Items.Add("[5]e_mark   = 0x" + actXET.E_mark.ToString("X2"));
                listBox1.Items.Add("[5]e_sign   = 0x" + actXET.E_sign.ToString("X2"));
                listBox1.Items.Add("[5]e_addr   = 0x" + actXET.E_addr.ToString("X2"));
                switch (actXET.E_baud)
                {
                    case 0: listBox1.Items.Add("[5]e_baud        = 1200"); break;
                    case 1: listBox1.Items.Add("[5]e_baud        = 2400"); break;
                    case 2: listBox1.Items.Add("[5]e_baud        = 4800"); break;
                    case 3: listBox1.Items.Add("[5]e_baud        = 9600"); break;
                    case 4: listBox1.Items.Add("[5]e_baud        = 14400"); break;
                    case 5: listBox1.Items.Add("[5]e_baud        = 19200"); break;
                    case 6: listBox1.Items.Add("[5]e_baud        = 38400"); break;
                    case 7: listBox1.Items.Add("[5]e_baud        = 57600"); break;
                    case 8: listBox1.Items.Add("[5]e_baud        = 115200"); break;
                    case 9: listBox1.Items.Add("[5]e_baud        = 230400"); break;
                    case 10: listBox1.Items.Add("[5]e_baud        = 256000"); break;
                }
                listBox1.Items.Add("[5]e_stopbit     = " + ((StopBits)actXET.E_stopbit).ToString());
                listBox1.Items.Add("[5]e_parity      = " + ((Parity)actXET.E_parity).ToString());
                listBox1.Items.Add("[5]e_wt_zero     = " + actXET.T_wt_zero + " " + unitUmask);
                listBox1.Items.Add("[5]e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
                listBox1.Items.Add("[5]e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                listBox1.Items.Add("[5]e_wt_unit     = " + unitUmaskSolo);
                listBox1.Items.Add("[5]e_wt_ascii    = " + actXET.E_wt_ascii.ToString());
                listBox1.Items.Add("[5]e_wt_sptime   = " + actXET.E_wt_sptime.ToString());
                listBox1.Items.Add("[5]e_wt_spfilt   = " + actXET.E_wt_spfilt.ToString());
                listBox1.Items.Add("[5]e_wt_division = " + actXET.E_wt_division.ToString() + "e");
                listBox1.Items.Add("[5]e_wt_antivib  = " + actXET.E_wt_antivib.ToString());
                listBox1.Items.Add("[5]e_heartBeat   = " + actXET.E_heartBeat.ToString());
                listBox1.Items.Add("[5]e_typeTPDO0   = " + actXET.E_typeTPDO0.ToString());
                listBox1.Items.Add("[5]e_evenTPDO0   = " + actXET.E_evenTPDO0.ToString());
                listBox1.Items.Add("[5]e_nodeID      = " + actXET.E_nodeID.ToString());
                listBox1.Items.Add("[5]e_nodeBaud    = " + actXET.E_nodeBaud.ToString());
                listBox1.Items.Add("[5]e_dynazero    = " + actXET.E_dynazero.ToString());
                listBox1.Items.Add("[5]e_cheatype    = " + actXET.E_cheatype.ToString());
                listBox1.Items.Add("[5]e_thmax       = " + actXET.E_thmax.ToString());
                listBox1.Items.Add("[5]e_thmin       = " + actXET.E_thmin.ToString());
                listBox1.Items.Add("[5]e_stablerange  = " + actXET.E_stablerange.ToString());
                listBox1.Items.Add("[5]e_stabletime  = " + actXET.E_stabletime.ToString());
                listBox1.Items.Add("[5]e_tkzerotime  = " + actXET.E_tkzerotime.ToString());
                listBox1.Items.Add("[5]e_tkdynatime  = " + actXET.E_tkdynatime.ToString());
                listBox1.Items.Add("----------------");
            }
            //普通账户
            else
            {
                deci = actXET.S_decimal;
                unit = " " + actXET.S_unit;

                listBox1.Items.Clear();
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("TYPE = " + actXET.S_DeviceType.ToString());
                listBox1.Items.Add("OUT = " + actXET.S_OutType.ToString());
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("e_test      = 0x" + actXET.E_test.ToString("X2"));
                listBox1.Items.Add("e_outype    = 0x" + actXET.E_outype.ToString("X2"));
                listBox1.Items.Add("e_curve     = 0x" + actXET.E_curve.ToString("X2"));
                listBox1.Items.Add("e_adspeed   = 0x" + actXET.E_adspeed.ToString("X2"));
                listBox1.Items.Add("e_autozero  = 0x" + actXET.E_autozero.ToString("X2"));
                listBox1.Items.Add("e_trackzero = 0x" + actXET.E_trackzero.ToString("X2"));
                listBox1.Items.Add("e_mfg_date  = " + actXET.E_mfg_date.ToString());
                listBox1.Items.Add("e_mfg_srno  = " + actXET.E_mfg_srno.ToString());
                listBox1.Items.Add("e_bohrcode  = " + actXET.R_bohrcode_long.ToString("X8"));
                listBox1.Items.Add("e_enspan    = " + actXET.E_enspan.ToString());
                listBox1.Items.Add("e_protype   = " + actXET.E_protype.ToString());
                listBox1.Items.Add("----------------");
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CTWOPT:
                        listBox1.Items.Add("e_inputz    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("e_inputf    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("e_analogz   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analogf   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_sens      = " + sens);
                        listBox1.Items.Add("e_ad_point1 = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("e_ad_point5 = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("e_da_point1 = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("e_da_point5 = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("e_ad_zero   = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("e_ad_full   = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("e_da_zero   = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("e_da_full   = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("e_vtio      = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        break;
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                        listBox1.Items.Add("e_input1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("e_input2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("e_input3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("e_input4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("e_input5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("e_analog1 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog2 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog3 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog4 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog5 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_sens    = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_ad_point1 = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("e_ad_point2 = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("e_ad_point3 = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("e_ad_point4 = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("e_ad_point5 = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("e_da_point1 = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("e_da_point2 = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("e_da_point3 = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("e_da_point4 = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("e_da_point5 = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("e_ad_zero   = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("e_ad_full   = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("e_da_zero   = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("e_da_full   = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_vtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("e_wtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBox1.Items.Add("e_atio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBox1.Items.Add("e_btio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBox1.Items.Add("e_ctio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBox1.Items.Add("e_dtio = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        break;
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        listBox1.Items.Add("e_input1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("e_input2   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("e_input3   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("e_input4   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("e_input5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("e_input6   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V");
                        listBox1.Items.Add("e_input7   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V");
                        listBox1.Items.Add("e_input8   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V");
                        listBox1.Items.Add("e_input9   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V");
                        listBox1.Items.Add("e_input10  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V");
                        listBox1.Items.Add("e_input11  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V");
                        listBox1.Items.Add("e_analog1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog6  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog7  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog8  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog9  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog10 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_analog11 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
                        listBox1.Items.Add("e_sens     = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_ad_point1  = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("e_ad_point2  = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("e_ad_point3  = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("e_ad_point4  = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("e_ad_point5  = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("e_ad_point6  = " + actXET.E_ad_point6.ToString());
                        listBox1.Items.Add("e_ad_point7  = " + actXET.E_ad_point7.ToString());
                        listBox1.Items.Add("e_ad_point8  = " + actXET.E_ad_point8.ToString());
                        listBox1.Items.Add("e_ad_point9  = " + actXET.E_ad_point9.ToString());
                        listBox1.Items.Add("e_ad_point10 = " + actXET.E_ad_point10.ToString());
                        listBox1.Items.Add("e_ad_point11 = " + actXET.E_ad_point11.ToString());
                        listBox1.Items.Add("e_da_point1  = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("e_da_point2  = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("e_da_point3  = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("e_da_point4  = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("e_da_point5  = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("e_da_point6  = " + actXET.E_da_point6.ToString());
                        listBox1.Items.Add("e_da_point7  = " + actXET.E_da_point7.ToString());
                        listBox1.Items.Add("e_da_point8  = " + actXET.E_da_point8.ToString());
                        listBox1.Items.Add("e_da_point9  = " + actXET.E_da_point9.ToString());
                        listBox1.Items.Add("e_da_point10 = " + actXET.E_da_point10.ToString());
                        listBox1.Items.Add("e_da_point11 = " + actXET.E_da_point11.ToString());
                        listBox1.Items.Add("e_ad_zero    = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("e_ad_full    = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("e_da_zero    = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("e_da_full    = " + actXET.E_da_full.ToString());
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
                        break;
                }
                listBox1.Items.Add("----------------");
                switch (actXET.S_DeviceType)
                {
                    case TYPE.BE30AH:
                    case TYPE.TDES:
                    case TYPE.TDSS:
                    case TYPE.TD485:
                    case TYPE.T4X600H:
                        listBox1.Items.Add("e_sign        = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("e_addr        = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("e_baud        = 1200"); break;
                            case 1: listBox1.Items.Add("e_baud        = 2400"); break;
                            case 2: listBox1.Items.Add("e_baud        = 4800"); break;
                            case 3: listBox1.Items.Add("e_baud        = 9600"); break;
                            case 4: listBox1.Items.Add("e_baud        = 14400"); break;
                            case 5: listBox1.Items.Add("e_baud        = 19200"); break;
                            case 6: listBox1.Items.Add("e_baud        = 38400"); break;
                            case 7: listBox1.Items.Add("e_baud        = 57600"); break;
                            case 8: listBox1.Items.Add("e_baud        = 115200"); break;
                            case 9: listBox1.Items.Add("e_baud        = 230400"); break;
                            case 10: listBox1.Items.Add("e_baud        = 256000"); break;
                        }
                        listBox1.Items.Add("e_stopbit     = " + ((StopBits)actXET.E_stopbit).ToString());
                        listBox1.Items.Add("e_parity      = " + ((Parity)actXET.E_parity).ToString());
                        listBox1.Items.Add("e_wt_zero     = " + actXET.T_wt_zero.ToString() + " " + unitUmask);
                        listBox1.Items.Add("e_wt_full     = " + actXET.T_wt_full.ToString() + " " + unitUmask);
                        listBox1.Items.Add("e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("e_wt_unit     = " + unitUmaskSolo);
                        listBox1.Items.Add("e_wt_ascii    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iBus:
                        listBox1.Items.Add("e_sign        = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("e_addr        = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("e_baud        = 1200"); break;
                            case 1: listBox1.Items.Add("e_baud        = 2400"); break;
                            case 2: listBox1.Items.Add("e_baud        = 4800"); break;
                            case 3: listBox1.Items.Add("e_baud        = 9600"); break;
                            case 4: listBox1.Items.Add("e_baud        = 14400"); break;
                            case 5: listBox1.Items.Add("e_baud        = 19200"); break;
                            case 6: listBox1.Items.Add("e_baud        = 38400"); break;
                            case 7: listBox1.Items.Add("e_baud        = 57600"); break;
                            case 8: listBox1.Items.Add("e_baud        = 115200"); break;
                            case 9: listBox1.Items.Add("e_baud        = 230400"); break;
                            case 10: listBox1.Items.Add("e_baud        = 256000"); break;
                        }
                        listBox1.Items.Add("e_stopbit     = " + ((StopBits)actXET.E_stopbit).ToString());
                        listBox1.Items.Add("e_parity      = " + ((Parity)actXET.E_parity).ToString());
                        listBox1.Items.Add("e_wt_zero     = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("e_wt_unit     = " + unitUmaskSolo);
                        listBox1.Items.Add("e_wt_ascii    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("e_wt_sptime   = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("e_wt_spfilt   = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("e_wt_division = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("e_wt_antivib  = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("e_stablerange  = " + actXET.E_stablerange.ToString());
                        listBox1.Items.Add("e_stabletime  = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("e_tkzerotime  = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("e_tkdynatime  = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iNet:
                    case TYPE.iStar:
                        listBox1.Items.Add("e_sign        = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("e_addr        = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("e_baud        = 1200"); break;
                            case 1: listBox1.Items.Add("e_baud        = 2400"); break;
                            case 2: listBox1.Items.Add("e_baud        = 4800"); break;
                            case 3: listBox1.Items.Add("e_baud        = 9600"); break;
                            case 4: listBox1.Items.Add("e_baud        = 14400"); break;
                            case 5: listBox1.Items.Add("e_baud        = 19200"); break;
                            case 6: listBox1.Items.Add("e_baud        = 38400"); break;
                            case 7: listBox1.Items.Add("e_baud        = 57600"); break;
                            case 8: listBox1.Items.Add("e_baud        = 115200"); break;
                            case 9: listBox1.Items.Add("e_baud        = 230400"); break;
                            case 10: listBox1.Items.Add("e_baud        = 256000"); break;
                        }
                        listBox1.Items.Add("e_stopbit     = " + ((StopBits)actXET.E_stopbit).ToString());
                        listBox1.Items.Add("e_parity      = " + ((Parity)actXET.E_parity).ToString());
                        listBox1.Items.Add("e_wt_zero     = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("e_wt_unit     = " + unitUmaskSolo);
                        listBox1.Items.Add("e_wt_ascii    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("e_wt_sptime   = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("e_wt_spfilt   = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("e_wt_division = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("e_wt_antivib  = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("e_stabletime  = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("e_tkzerotime  = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("e_tkdynatime  = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.TCAN:
                        listBox1.Items.Add("e_wt_zero     = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("e_wt_full     = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("e_wt_decimal  = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("e_wt_unit     = " + unitUmaskSolo);
                        listBox1.Items.Add("e_wt_sptime   = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("e_wt_spfilt   = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("e_wt_division = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("e_heartBeat   = " + actXET.E_heartBeat.ToString());
                        listBox1.Items.Add("e_typeTPDO0   = " + actXET.E_typeTPDO0.ToString());
                        listBox1.Items.Add("e_evenTPDO0   = " + actXET.E_evenTPDO0.ToString());
                        listBox1.Items.Add("e_nodeID      = " + actXET.E_nodeID.ToString());
                        listBox1.Items.Add("e_nodeBaud    = " + actXET.E_nodeBaud.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    default:
                        break;
                }
            }
        }

        //更新实时重量
        private void Disp_Weight()
        {
            //更新实时重量
            signalOutput1.Text = actXET.R_weight;

            //稳定状态
            if (actXET.R_stable)
            {
                signalStable1.LampColor = new Color[] { Color.Green };
            }
            else
            {
                signalStable1.LampColor = new Color[] { Color.Black };
            }

            //单位
            signalUnit1.Text = actXET.GetUnitUMASK();
        }

        #endregion

        #region 输入检查

        /// <summary>
        /// input检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_Leave(object sender, EventArgs e)
        {
            //灵敏度输入 -> 更新T_input=TextBox.text -> 更新E_ad_pointX和E_inputX
            if (radioButton2.Checked == true)
            {
                actXET.T_input1 = textBox1.Text;
                actXET.T_input2 = textBox2.Text;
                actXET.T_input3 = textBox3.Text;
                actXET.T_input4 = textBox4.Text;
                actXET.T_input5 = textBox5.Text;
                actXET.T_input6 = textBox6.Text;
                actXET.T_input7 = textBox7.Text;
                actXET.T_input8 = textBox8.Text;
                actXET.T_input9 = textBox9.Text;
                actXET.T_input10 = textBox10.Text;
                actXET.T_input11 = textBox11.Text;

                actXET.RefreshRatio();

                paralist_Checking();

                label34.Text = actXET.R_errSensor;
                label35.Text = actXET.R_resolution;
            }
        }

        /// <summary>
        /// output检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void output_Leave(object sender, EventArgs e)
        {
            //模拟量输出修改或标定加载值修改->更新T_analog = TextBox.text->更新E_analogX

            actXET.T_analog1 = textBox12.Text;
            actXET.T_analog2 = textBox13.Text;
            actXET.T_analog3 = textBox14.Text;
            actXET.T_analog4 = textBox15.Text;
            actXET.T_analog5 = textBox16.Text;
            actXET.T_analog6 = textBox17.Text;
            actXET.T_analog7 = textBox18.Text;
            actXET.T_analog8 = textBox19.Text;
            actXET.T_analog9 = textBox20.Text;
            actXET.T_analog10 = textBox21.Text;
            actXET.T_analog11 = textBox22.Text;

            actXET.RefreshRatio();

            paralist_Checking();

            label34.Text = actXET.R_errSensor;
            label35.Text = actXET.R_resolution;
        }

        #endregion

        #region 其它操作控件

        /// <summary>
        /// 加载采集标定法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            curve_Checking();
        }

        /// <summary>
        /// 免标定直接输入法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            curve_Checking();
        }

        /// <summary>
        /// 点击v按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonVS_Click(object sender, EventArgs e)
        {
            //
            MyDevice.myUpdate -= update_FromUart;
            actXET.RefreshRatio();
            this.Hide();

            //
            MenuCalScopeForm myMenuCalScopeForm = new MenuCalScopeForm();
            myMenuCalScopeForm.StartPosition = FormStartPosition.CenterParent;
            myMenuCalScopeForm.ShowDialog();

            //
            this.Show();
            this.BringToFront();
            MyDevice.myUpdate += update_FromUart;
        }

        /// <summary>
        /// 点击F按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFX_Click(object sender, EventArgs e)
        {
            //
            actXET.RefreshRatio();
            this.Hide();

            //
            MenuCalCurveForm myMenuCalCurveForm = new MenuCalCurveForm();
            myMenuCalCurveForm.StartPosition = FormStartPosition.CenterParent;
            myMenuCalCurveForm.ShowDialog();

            //
            this.Show();
            this.BringToFront();
        }

        /// <summary>
        /// input输入保护
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_InputProtected_Click(object sender, EventArgs e)
        {
            if (bt_InputProtected.Text == "保 护" || bt_InputProtected.Text == "Protected")
            {
                bt_InputProtected.Text = MyDevice.languageType == 0 ? "编 辑" : "Edit";
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
                textBox5.ReadOnly = true;
                textBox6.ReadOnly = true;
                textBox7.ReadOnly = true;
                textBox8.ReadOnly = true;
                textBox9.ReadOnly = true;
                textBox10.ReadOnly = true;
                textBox11.ReadOnly = true;

                actXET.RefreshRatio();

                paralist_Checking();

                label34.Text = actXET.R_errSensor;
                label35.Text = actXET.R_resolution;
            }
            else
            {
                bt_InputProtected.Text = MyDevice.languageType == 0 ? "保 护" : "Protected";
                textBox1.ReadOnly = false;
                textBox2.ReadOnly = false;
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = false;
                textBox5.ReadOnly = false;
                textBox6.ReadOnly = false;
                textBox7.ReadOnly = false;
                textBox8.ReadOnly = false;
                textBox9.ReadOnly = false;
                textBox10.ReadOnly = false;
                textBox11.ReadOnly = false;
            }

            //
            if (textBox2.BackColor != Color.Firebrick) { while (textBox2.BackColor != textBox1.BackColor) textBox2.BackColor = textBox1.BackColor; }
            if (textBox3.BackColor != Color.Firebrick) { while (textBox3.BackColor != textBox1.BackColor) textBox3.BackColor = textBox1.BackColor; }
            if (textBox4.BackColor != Color.Firebrick) { while (textBox4.BackColor != textBox1.BackColor) textBox4.BackColor = textBox1.BackColor; }
            if (textBox5.BackColor != Color.Firebrick) { while (textBox5.BackColor != textBox1.BackColor) textBox5.BackColor = textBox1.BackColor; }
            if (textBox6.BackColor != Color.Firebrick) { while (textBox6.BackColor != textBox1.BackColor) textBox6.BackColor = textBox1.BackColor; }
            if (textBox7.BackColor != Color.Firebrick) { while (textBox7.BackColor != textBox1.BackColor) textBox7.BackColor = textBox1.BackColor; }
            if (textBox8.BackColor != Color.Firebrick) { while (textBox8.BackColor != textBox1.BackColor) textBox8.BackColor = textBox1.BackColor; }
            if (textBox9.BackColor != Color.Firebrick) { while (textBox9.BackColor != textBox1.BackColor) textBox9.BackColor = textBox1.BackColor; }
            if (textBox10.BackColor != Color.Firebrick) { while (textBox10.BackColor != textBox1.BackColor) textBox10.BackColor = textBox1.BackColor; }
            if (textBox11.BackColor != Color.Firebrick) { while (textBox11.BackColor != textBox1.BackColor) textBox11.BackColor = textBox1.BackColor; }
            //
            if (textBox13.BackColor != Color.Firebrick) { while (textBox13.BackColor != textBox12.BackColor) textBox13.BackColor = textBox12.BackColor; }
            if (textBox14.BackColor != Color.Firebrick) { while (textBox14.BackColor != textBox12.BackColor) textBox14.BackColor = textBox12.BackColor; }
            if (textBox15.BackColor != Color.Firebrick) { while (textBox15.BackColor != textBox12.BackColor) textBox15.BackColor = textBox12.BackColor; }
            if (textBox16.BackColor != Color.Firebrick) { while (textBox16.BackColor != textBox12.BackColor) textBox16.BackColor = textBox12.BackColor; }
            if (textBox17.BackColor != Color.Firebrick) { while (textBox17.BackColor != textBox12.BackColor) textBox17.BackColor = textBox12.BackColor; }
            if (textBox18.BackColor != Color.Firebrick) { while (textBox18.BackColor != textBox12.BackColor) textBox18.BackColor = textBox12.BackColor; }
            if (textBox19.BackColor != Color.Firebrick) { while (textBox19.BackColor != textBox12.BackColor) textBox19.BackColor = textBox12.BackColor; }
            if (textBox20.BackColor != Color.Firebrick) { while (textBox20.BackColor != textBox12.BackColor) textBox20.BackColor = textBox12.BackColor; }
            if (textBox21.BackColor != Color.Firebrick) { while (textBox21.BackColor != textBox12.BackColor) textBox21.BackColor = textBox12.BackColor; }
            if (textBox22.BackColor != Color.Firebrick) { while (textBox22.BackColor != textBox12.BackColor) textBox22.BackColor = textBox12.BackColor; }
        }

        /// <summary>
        /// output输入保护
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_OutputProtected_Click(object sender, EventArgs e)
        {
            if (bt_OutputProtected.Text == "保 护" || bt_OutputProtected.Text == "Protected")
            {
                bt_OutputProtected.Text = MyDevice.languageType == 0 ? "编 辑" : "Edit";
                textBox12.ReadOnly = true;
                textBox13.ReadOnly = true;
                textBox14.ReadOnly = true;
                textBox15.ReadOnly = true;
                textBox16.ReadOnly = true;
                textBox17.ReadOnly = true;
                textBox18.ReadOnly = true;
                textBox19.ReadOnly = true;
                textBox20.ReadOnly = true;
                textBox21.ReadOnly = true;
                textBox22.ReadOnly = true;

                actXET.RefreshRatio();
                paralist_Checking();

                label34.Text = actXET.R_errSensor;
                label35.Text = actXET.R_resolution;
            }
            else
            {
                bt_OutputProtected.Text = MyDevice.languageType == 0 ? "保 护" : "Protected";
                textBox12.ReadOnly = false;
                textBox13.ReadOnly = false;
                textBox14.ReadOnly = false;
                textBox15.ReadOnly = false;
                textBox16.ReadOnly = false;
                textBox17.ReadOnly = false;
                textBox18.ReadOnly = false;
                textBox19.ReadOnly = false;
                textBox20.ReadOnly = false;
                textBox21.ReadOnly = false;
                textBox22.ReadOnly = false;
            }

            //
            if (textBox2.BackColor != Color.Firebrick) { while (textBox2.BackColor != textBox1.BackColor) textBox2.BackColor = textBox1.BackColor; }
            if (textBox3.BackColor != Color.Firebrick) { while (textBox3.BackColor != textBox1.BackColor) textBox3.BackColor = textBox1.BackColor; }
            if (textBox4.BackColor != Color.Firebrick) { while (textBox4.BackColor != textBox1.BackColor) textBox4.BackColor = textBox1.BackColor; }
            if (textBox5.BackColor != Color.Firebrick) { while (textBox5.BackColor != textBox1.BackColor) textBox5.BackColor = textBox1.BackColor; }
            if (textBox6.BackColor != Color.Firebrick) { while (textBox6.BackColor != textBox1.BackColor) textBox6.BackColor = textBox1.BackColor; }
            if (textBox7.BackColor != Color.Firebrick) { while (textBox7.BackColor != textBox1.BackColor) textBox7.BackColor = textBox1.BackColor; }
            if (textBox8.BackColor != Color.Firebrick) { while (textBox8.BackColor != textBox1.BackColor) textBox8.BackColor = textBox1.BackColor; }
            if (textBox9.BackColor != Color.Firebrick) { while (textBox9.BackColor != textBox1.BackColor) textBox9.BackColor = textBox1.BackColor; }
            if (textBox10.BackColor != Color.Firebrick) { while (textBox10.BackColor != textBox1.BackColor) textBox10.BackColor = textBox1.BackColor; }
            if (textBox11.BackColor != Color.Firebrick) { while (textBox11.BackColor != textBox1.BackColor) textBox11.BackColor = textBox1.BackColor; }
            //
            if (textBox13.BackColor != Color.Firebrick) { while (textBox13.BackColor != textBox12.BackColor) textBox13.BackColor = textBox12.BackColor; }
            if (textBox14.BackColor != Color.Firebrick) { while (textBox14.BackColor != textBox12.BackColor) textBox14.BackColor = textBox12.BackColor; }
            if (textBox15.BackColor != Color.Firebrick) { while (textBox15.BackColor != textBox12.BackColor) textBox15.BackColor = textBox12.BackColor; }
            if (textBox16.BackColor != Color.Firebrick) { while (textBox16.BackColor != textBox12.BackColor) textBox16.BackColor = textBox12.BackColor; }
            if (textBox17.BackColor != Color.Firebrick) { while (textBox17.BackColor != textBox12.BackColor) textBox17.BackColor = textBox12.BackColor; }
            if (textBox18.BackColor != Color.Firebrick) { while (textBox18.BackColor != textBox12.BackColor) textBox18.BackColor = textBox12.BackColor; }
            if (textBox19.BackColor != Color.Firebrick) { while (textBox19.BackColor != textBox12.BackColor) textBox19.BackColor = textBox12.BackColor; }
            if (textBox20.BackColor != Color.Firebrick) { while (textBox20.BackColor != textBox12.BackColor) textBox20.BackColor = textBox12.BackColor; }
            if (textBox21.BackColor != Color.Firebrick) { while (textBox21.BackColor != textBox12.BackColor) textBox21.BackColor = textBox12.BackColor; }
            if (textBox22.BackColor != Color.Firebrick) { while (textBox22.BackColor != textBox12.BackColor) textBox22.BackColor = textBox12.BackColor; }
        }

        #endregion

        #region 采集标定

        /// <summary>
        /// 采集1 & 零点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            button1.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog1 = textBox12.Text;
            Port_send(TASKS.ADCP1);

            //录入进程的变送器传入的数据
            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f1 = actXET.E_ad_point1;//录入进程内码
                        ui_input_f1 = actXET.E_input1;//录入进程灵敏度
                        break;
                    default:
                        break;
                }
            }

            //录入返程的变送器传入的数据
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b1 = actXET.E_ad_point1;//录入返程内码
                        ui_input_b1 = actXET.E_input1;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            button2.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog2 = textBox13.Text;
            Port_send(TASKS.ADCP2);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f2 = actXET.E_ad_point2;//录入进程内码
                        ui_input_f2 = actXET.E_input2;//录入进程灵敏度  
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b2 = actXET.E_ad_point2;//录入返程内码
                        ui_input_b2 = actXET.E_input2;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            button3.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog3 = textBox14.Text;
            Port_send(TASKS.ADCP3);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f3 = actXET.E_ad_point3;//录入进程内码
                        ui_input_f3 = actXET.E_input3;//录入进程灵敏度   
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b3 = actXET.E_ad_point3;//录入返程内码
                        ui_input_b3 = actXET.E_input3;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            button4.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog4 = textBox15.Text;
            Port_send(TASKS.ADCP4);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f4 = actXET.E_ad_point4;//录入进程内码
                        ui_input_f4 = actXET.E_input4;//录入进程灵敏度  
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b4 = actXET.E_ad_point4;//录入返程内码
                        ui_input_b4 = actXET.E_input4;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集5 & 满点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            button5.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog5 = textBox16.Text;
            Port_send(TASKS.ADCP5);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f5 = actXET.E_ad_point5;//录入进程内码
                        ui_input_f5 = actXET.E_input5;//录入进程灵敏度    
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b5 = actXET.E_ad_point5;//录入返程内码
                        ui_input_b5 = actXET.E_input5;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            button6.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog6 = textBox17.Text;
            Port_send(TASKS.ADCP6);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f6 = actXET.E_ad_point6;//录入进程内码
                        ui_input_f6 = actXET.E_input6;//录入进程灵敏度    
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b6 = actXET.E_ad_point6;//录入返程内码
                        ui_input_b6 = actXET.E_input6;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集7
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            button7.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog7 = textBox18.Text;
            Port_send(TASKS.ADCP7);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f7 = actXET.E_ad_point7;//录入进程内码
                        ui_input_f7 = actXET.E_input7;//录入进程灵敏度       
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b7 = actXET.E_ad_point7;//录入返程内码
                        ui_input_b7 = actXET.E_input7;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集8
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            button8.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog8 = textBox19.Text;
            Port_send(TASKS.ADCP8);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f8 = actXET.E_ad_point8;//录入进程内码
                        ui_input_f8 = actXET.E_input8;//录入进程灵敏度     
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b8 = actXET.E_ad_point8;//录入返程内码
                        ui_input_b8 = actXET.E_input8;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集9
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            button9.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog9 = textBox20.Text;
            Port_send(TASKS.ADCP9);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f9 = actXET.E_ad_point9;//录入进程内码
                        ui_input_f9 = actXET.E_input9;//录入进程灵敏度  
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b9 = actXET.E_ad_point9;//录入返程内码
                        ui_input_b9 = actXET.E_input9;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            button10.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog10 = textBox21.Text;
            Port_send(TASKS.ADCP10);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f10 = actXET.E_ad_point10;//录入进程内码
                        ui_input_f10 = actXET.E_input10;//录入进程灵敏度  
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b10 = actXET.E_ad_point10;//录入返程内码
                        ui_input_b10 = actXET.E_input10;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 采集11
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            button11.HoverBackColor = Color.Firebrick;
            Refresh();
            actXET.T_analog11 = textBox22.Text;
            Port_send(TASKS.ADCP11);

            if (radioButtonForward.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_f11 = actXET.E_ad_point11;//录入进程内码
                        ui_input_f11 = actXET.E_input11;//录入进程灵敏度   
                        break;
                    default:
                        break;
                }
            }
            if (radioButtonBack.Checked == true)
            {
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        Thread.Sleep(200);
                        ui_ad_point_b11 = actXET.E_ad_point11;//录入返程内码
                        ui_input_b11 = actXET.E_input11;//录入返程灵敏度
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Write_Click(object sender, EventArgs e)
        {
            if (MyDevice.protocol.IsOpen)
            {
                bt_Write.Text = "BCC";
                bt_Write.HoverBackColor = Color.Firebrick;

                //写入任务状态机设置限制条件：TP10非两点标定且是进程不写入
                if (actXET.S_DeviceType == TYPE.TP10)
                {
                    if (!(((ECVE)actXET.E_curve != ECVE.CTWOPT) && radioButtonForward.Checked))
                    {
                        Port_send(TASKS.BCC);
                    }

                    if (radioButtonBack.Checked == true)
                    {
                        switch ((ECVE)actXET.E_curve)
                        {
                            case ECVE.CFITED:
                            case ECVE.CINTER:
                                actXET.E_ad_point1 = (ui_ad_point_f1 + ui_ad_point_b1) / 2;
                                actXET.E_input1 = (ui_input_f1 + ui_input_b1) / 2;
                                actXET.E_ad_point2 = (ui_ad_point_f2 + ui_ad_point_b2) / 2;
                                actXET.E_input2 = (ui_input_f2 + ui_input_b2) / 2;
                                actXET.E_ad_point3 = (ui_ad_point_f3 + ui_ad_point_b3) / 2;
                                actXET.E_input3 = (ui_input_f3 + ui_input_b3) / 2;
                                actXET.E_ad_point4 = (ui_ad_point_f4 + ui_ad_point_b4) / 2;
                                actXET.E_input4 = (ui_input_f4 + ui_input_b4) / 2;
                                actXET.E_ad_point5 = (ui_ad_point_f5 + ui_ad_point_b5) / 2;
                                actXET.E_input5 = (ui_input_f5 + ui_input_b5) / 2;

                                //返程写入之后进程初始化
                                ui_ad_point_f1 = actXET.E_ad_point1;
                                ui_ad_point_f2 = actXET.E_ad_point2;
                                ui_ad_point_f3 = actXET.E_ad_point3;
                                ui_ad_point_f4 = actXET.E_ad_point4;
                                ui_ad_point_f5 = actXET.E_ad_point5;

                                ui_input_f1 = actXET.E_input1;
                                ui_input_f2 = actXET.E_input2;
                                ui_input_f3 = actXET.E_input3;
                                ui_input_f4 = actXET.E_input4;
                                ui_input_f5 = actXET.E_input5;

                                //写入后更新相应数据
                                actXET.RefreshRatio();
                                paralist_Checking();
                                input_Checking();
                                break;
                            case ECVE.CELTED:
                            case ECVE.CELTER:
                                actXET.E_ad_point1 = (ui_ad_point_f1 + ui_ad_point_b1) / 2;
                                actXET.E_input1 = (ui_input_f1 + ui_input_b1) / 2;
                                actXET.E_ad_point2 = (ui_ad_point_f2 + ui_ad_point_b2) / 2;
                                actXET.E_input2 = (ui_input_f2 + ui_input_b2) / 2;
                                actXET.E_ad_point3 = (ui_ad_point_f3 + ui_ad_point_b3) / 2;
                                actXET.E_input3 = (ui_input_f3 + ui_input_b3) / 2;
                                actXET.E_ad_point4 = (ui_ad_point_f4 + ui_ad_point_b4) / 2;
                                actXET.E_input4 = (ui_input_f4 + ui_input_b4) / 2;
                                actXET.E_ad_point5 = (ui_ad_point_f5 + ui_ad_point_b5) / 2;
                                actXET.E_input5 = (ui_input_f5 + ui_input_b5) / 2;
                                actXET.E_ad_point6 = (ui_ad_point_f6 + ui_ad_point_b6) / 2;
                                actXET.E_input6 = (ui_input_f6 + ui_input_b6) / 2;
                                actXET.E_ad_point7 = (ui_ad_point_f7 + ui_ad_point_b7) / 2;
                                actXET.E_input7 = (ui_input_f7 + ui_input_b7) / 2;
                                actXET.E_ad_point8 = (ui_ad_point_f8 + ui_ad_point_b8) / 2;
                                actXET.E_input8 = (ui_input_f8 + ui_input_b8) / 2;
                                actXET.E_ad_point9 = (ui_ad_point_f9 + ui_ad_point_b9) / 2;
                                actXET.E_input9 = (ui_input_f9 + ui_input_b9) / 2;
                                actXET.E_ad_point10 = (ui_ad_point_f10 + ui_ad_point_b10) / 2;
                                actXET.E_input10 = (ui_input_f10 + ui_input_b10) / 2;
                                actXET.E_ad_point11 = (ui_ad_point_f11 + ui_ad_point_b11) / 2;
                                actXET.E_input11 = (ui_input_f11 + ui_input_b11) / 2;

                                //返程写入之后进程初始化
                                ui_ad_point_f1 = actXET.E_ad_point1;
                                ui_ad_point_f2 = actXET.E_ad_point2;
                                ui_ad_point_f3 = actXET.E_ad_point3;
                                ui_ad_point_f4 = actXET.E_ad_point4;
                                ui_ad_point_f5 = actXET.E_ad_point5;
                                ui_ad_point_f6 = actXET.E_ad_point6;
                                ui_ad_point_f7 = actXET.E_ad_point7;
                                ui_ad_point_f8 = actXET.E_ad_point8;
                                ui_ad_point_f9 = actXET.E_ad_point9;
                                ui_ad_point_f10 = actXET.E_ad_point10;
                                ui_ad_point_f11 = actXET.E_ad_point11;

                                ui_input_f1 = actXET.E_input1;
                                ui_input_f2 = actXET.E_input2;
                                ui_input_f3 = actXET.E_input3;
                                ui_input_f4 = actXET.E_input4;
                                ui_input_f5 = actXET.E_input5;
                                ui_input_f6 = actXET.E_input6;
                                ui_input_f7 = actXET.E_input7;
                                ui_input_f8 = actXET.E_input8;
                                ui_input_f9 = actXET.E_input9;
                                ui_input_f10 = actXET.E_input10;
                                ui_input_f11 = actXET.E_input11;

                                //写入后更新相应数据
                                actXET.RefreshRatio();
                                paralist_Checking();
                                input_Checking();
                                break;
                            default:
                                break;
                        }
                    }

                    if (((ECVE)actXET.E_curve != ECVE.CTWOPT) && radioButtonForward.Checked)
                    {
                        //当进程写入时才激活返程
                        radioButtonBack.Checked = true;
                    }
                }
                else
                {
                    Port_send(TASKS.BCC);
                }
            }
        }

        #endregion

        /// <summary>
        /// 串口通讯委托回调响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                switch (MyDevice.protocol.trTASK)
                {
                    default:
                        //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                        bt_Write.Text = MyDevice.protocol.trTASK.ToString();

                        //继续写
                        MyDevice.mePort_WriteCalTasks();
                        //读实时重量
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            Main.ActiveForm = "SetCalibration";//写入完毕
                            bt_Write.HoverBackColor = Color.Green;
                            bt_Write.Text = MyDevice.languageType == 0 ? "成 功" : "Success";
                            if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                            {
                                MyDevice.mePort_SendCOM(TASKS.QGROSS);
                            }
                        }
                        break;

                    case TASKS.QGROSS:
                        switch (nextTask)
                        {
                            default:
                                if (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重")
                                {
                                    Disp_Weight();
                                    MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                }
                                break;
                            case TASKS.ADCP1:
                                MyDevice.mePort_SendCOM(TASKS.ADCP1);
                                break;
                            case TASKS.ADCP2:
                                MyDevice.mePort_SendCOM(TASKS.ADCP2);
                                break;
                            case TASKS.ADCP3:
                                MyDevice.mePort_SendCOM(TASKS.ADCP3);
                                break;
                            case TASKS.ADCP4:
                                MyDevice.mePort_SendCOM(TASKS.ADCP4);
                                break;
                            case TASKS.ADCP5:
                                MyDevice.mePort_SendCOM(TASKS.ADCP5);
                                break;
                            case TASKS.ADCP6:
                                MyDevice.mePort_SendCOM(TASKS.ADCP6);
                                break;
                            case TASKS.ADCP7:
                                MyDevice.mePort_SendCOM(TASKS.ADCP7);
                                break;
                            case TASKS.ADCP8:
                                MyDevice.mePort_SendCOM(TASKS.ADCP8);
                                break;
                            case TASKS.ADCP9:
                                MyDevice.mePort_SendCOM(TASKS.ADCP9);
                                break;
                            case TASKS.ADCP10:
                                MyDevice.mePort_SendCOM(TASKS.ADCP10);
                                break;
                            case TASKS.ADCP11:
                                MyDevice.mePort_SendCOM(TASKS.ADCP11);
                                break;
                            case TASKS.BCC:
                                Main.ActiveForm = "SetCalibrationUpdate";
                                MyDevice.mePort_ClearState();
                                MyDevice.mePort_WriteCalTasks();
                                break;
                        }
                        nextTask = TASKS.NULL;
                        break;

                    case TASKS.REST:
                        //写入完毕,老版本重启指令无回复
                        if (MyDevice.mSUT.E_test < 0x58)
                        {
                            bt_Write.HoverBackColor = Color.Green;
                            bt_Write.Text = MyDevice.languageType == 0 ? "成 功" : "Success";
                            //读实时重量
                            if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                            {
                                MyDevice.mePort_SendCOM(TASKS.QGROSS);
                            }
                        }
                        break;

                    //采集 -> 串口ReceiveLong -> 更新E_ad_pointX和E_inputX -> 更新TextBox.text=T_input
                    case TASKS.ADCP1:
                        textBox1.Text = actXET.T_input1;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button1.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP2:
                        textBox2.Text = actXET.T_input2;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button2.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP3:
                        textBox3.Text = actXET.T_input3;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button3.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP4:
                        textBox4.Text = actXET.T_input4;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button4.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP5:
                        textBox5.Text = actXET.T_input5;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button5.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP6:
                        textBox6.Text = actXET.T_input6;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button6.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP7:
                        textBox7.Text = actXET.T_input7;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button7.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP8:
                        textBox8.Text = actXET.T_input8;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button8.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP9:
                        textBox9.Text = actXET.T_input9;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button9.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP10:
                        textBox10.Text = actXET.T_input10;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button10.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP11:
                        textBox11.Text = actXET.T_input11;
                        actXET.RefreshRatio();
                        paralist_Checking();
                        button11.HoverBackColor = Color.Green;
                        //读实时重量
                        if (MyDevice.protocol.type == COMP.RS485 && (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重"))
                        {
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                }

                label34.Text = actXET.R_errSensor;
                label35.Text = actXET.R_resolution;
                label34.Refresh();
                label35.Refresh();
            }
        }

        private void Port_send(TASKS tasks)
        {
            if (MyDevice.protocol.trTASK == TASKS.QGROSS)
            {
                nextTask = tasks;
            }
            else
            {
                switch (tasks)
                {
                    default:
                        if (Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重")
                        {
                            Disp_Weight();
                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                        }
                        break;
                    case TASKS.ADCP1:
                        MyDevice.mePort_SendCOM(TASKS.ADCP1);
                        break;
                    case TASKS.ADCP2:
                        MyDevice.mePort_SendCOM(TASKS.ADCP2);
                        break;
                    case TASKS.ADCP3:
                        MyDevice.mePort_SendCOM(TASKS.ADCP3);
                        break;
                    case TASKS.ADCP4:
                        MyDevice.mePort_SendCOM(TASKS.ADCP4);
                        break;
                    case TASKS.ADCP5:
                        MyDevice.mePort_SendCOM(TASKS.ADCP5);
                        break;
                    case TASKS.ADCP6:
                        MyDevice.mePort_SendCOM(TASKS.ADCP6);
                        break;
                    case TASKS.ADCP7:
                        MyDevice.mePort_SendCOM(TASKS.ADCP7);
                        break;
                    case TASKS.ADCP8:
                        MyDevice.mePort_SendCOM(TASKS.ADCP8);
                        break;
                    case TASKS.ADCP9:
                        MyDevice.mePort_SendCOM(TASKS.ADCP9);
                        break;
                    case TASKS.ADCP10:
                        MyDevice.mePort_SendCOM(TASKS.ADCP10);
                        break;
                    case TASKS.ADCP11:
                        MyDevice.mePort_SendCOM(TASKS.ADCP11);
                        break;
                    case TASKS.BCC:
                        Main.ActiveForm = "SetCalibrationUpdate";
                        MyDevice.mePort_ClearState();
                        MyDevice.mePort_WriteCalTasks();
                        break;
                }
            }
        }

        //单程
        private void radioButtonSingle_CheckedChanged(object sender, EventArgs e)
        {
            this.bt_Write.Visible = true;
            string btnStr;
            btnStr = MyDevice.languageType == 0 ? "采集" : "Collect";
            this.button1.Text = btnStr + "1";
            this.button2.Text = btnStr + "2";
            this.button3.Text = btnStr + "3";
            this.button4.Text = btnStr + "4";
            this.button5.Text = btnStr + "5";
            this.button6.Text = btnStr + "6";
            this.button7.Text = btnStr + "7";
            this.button8.Text = btnStr + "8";
            this.button9.Text = btnStr + "9";
            this.button10.Text = btnStr + "10";
            this.button11.Text = btnStr + "11";
            this.bt_Write.Text = MyDevice.languageType == 0 ? "写 入" : "Write";

            //切换之后按钮颜色恢复
            this.button1.HoverBackColor = Color.LightSteelBlue;
            this.button2.HoverBackColor = Color.LightSteelBlue;
            this.button3.HoverBackColor = Color.LightSteelBlue;
            this.button4.HoverBackColor = Color.LightSteelBlue;
            this.button5.HoverBackColor = Color.LightSteelBlue;
            this.button6.HoverBackColor = Color.LightSteelBlue;
            this.button7.HoverBackColor = Color.LightSteelBlue;
            this.button8.HoverBackColor = Color.LightSteelBlue;
            this.button9.HoverBackColor = Color.LightSteelBlue;
            this.button10.HoverBackColor = Color.LightSteelBlue;
            this.button11.HoverBackColor = Color.LightSteelBlue;
            this.bt_Write.HoverBackColor = Color.LightSteelBlue;

            this.radioButtonBack.Enabled = false;

            actXET.ecveType = ECVEType.Single;//标记日志的显示状态

            Refresh();
        }

        //进程
        private void radioButtonForward_CheckedChanged(object sender, EventArgs e)
        {
            this.bt_Write.Visible = true;
            string btnStr;
            btnStr = MyDevice.languageType == 0 ? "进程采" : "Collect";
            this.button1.Text = btnStr + "1";
            this.button2.Text = btnStr + "2";
            this.button3.Text = btnStr + "3";
            this.button4.Text = btnStr + "4";
            this.button5.Text = btnStr + "5";
            this.button6.Text = btnStr + "6";
            this.button7.Text = btnStr + "7";
            this.button8.Text = btnStr + "8";
            this.button9.Text = btnStr + "9";
            this.button10.Text = btnStr + "10";
            this.button11.Text = btnStr + "11";
            this.bt_Write.Text = MyDevice.languageType == 0 ? "返 程" : "Write";

            this.button1.HoverBackColor = Color.LightSteelBlue;
            this.button2.HoverBackColor = Color.LightSteelBlue;
            this.button3.HoverBackColor = Color.LightSteelBlue;
            this.button4.HoverBackColor = Color.LightSteelBlue;
            this.button5.HoverBackColor = Color.LightSteelBlue;
            this.button6.HoverBackColor = Color.LightSteelBlue;
            this.button7.HoverBackColor = Color.LightSteelBlue;
            this.button8.HoverBackColor = Color.LightSteelBlue;
            this.button9.HoverBackColor = Color.LightSteelBlue;
            this.button10.HoverBackColor = Color.LightSteelBlue;
            this.button11.HoverBackColor = Color.LightSteelBlue;
            this.bt_Write.HoverBackColor = Color.LightSteelBlue;

            this.radioButtonBack.Enabled = false;

            Refresh();
        }

        //返程
        private void radioButtonBack_CheckedChanged(object sender, EventArgs e)
        {
            this.bt_Write.Visible = true;
            string btnStr;
            btnStr = MyDevice.languageType == 0 ? "返程采" : "Collect";
            this.button1.Text = btnStr + "1";
            this.button2.Text = btnStr + "2";
            this.button3.Text = btnStr + "3";
            this.button4.Text = btnStr + "4";
            this.button5.Text = btnStr + "5";
            this.button6.Text = btnStr + "6";
            this.button7.Text = btnStr + "7";
            this.button8.Text = btnStr + "8";
            this.button9.Text = btnStr + "9";
            this.button10.Text = btnStr + "10";
            this.button11.Text = btnStr + "11";
            this.bt_Write.Text = MyDevice.languageType == 0 ? "写 入" : "Write";

            this.button1.HoverBackColor = Color.LightSteelBlue;
            this.button2.HoverBackColor = Color.LightSteelBlue;
            this.button3.HoverBackColor = Color.LightSteelBlue;
            this.button4.HoverBackColor = Color.LightSteelBlue;
            this.button5.HoverBackColor = Color.LightSteelBlue;
            this.button6.HoverBackColor = Color.LightSteelBlue;
            this.button7.HoverBackColor = Color.LightSteelBlue;
            this.button8.HoverBackColor = Color.LightSteelBlue;
            this.button9.HoverBackColor = Color.LightSteelBlue;
            this.button10.HoverBackColor = Color.LightSteelBlue;
            this.button11.HoverBackColor = Color.LightSteelBlue;
            this.bt_Write.HoverBackColor = Color.LightSteelBlue;

            this.radioButtonBack.Enabled = true;

            actXET.ecveType = ECVEType.ForBack;//标记日志的显示状态

            Refresh();
        }

        private void MenuSetCalForm_SizeChanged(object sender, EventArgs e)
        {
            autoFormSize.UIComponetForm_Resize(this);
        }
    }
}