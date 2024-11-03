using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Resources;

//Alvin 20230414
//Ziyun 20230625
//Lumi 20230704
//Junzhe 20230706
//Ricardo 20230706
//Lumi 20230706
//Ricardo 20230707
//Lumi 20230707
//Ricardo 20230710
//Ricardo 20231115
//Lumi 20231222

//单设备实时数据页面

//SelfUART	调试OK
//RS485		调试OK
//CANopen	未调试

namespace Base.UI
{
    public partial class MeasureDevice : Form
    {
        private XET actXET;                 //需要操作的设备
        private TASKS nextTask;             //按键指令,TASKS.ZERO,TASKS.TARE,TASKS.BOR
        private int comTicker;              //发送指令计时器
        private int showTicker;             //曲线要快速,但是更新TextBox不能太快

        private DrawPicture drawPicture = new DrawPicture();    //绘图
        private bool isClickZero = false;   //是否按下归零
        private bool isClickTare = false;   //是否按下扣重

        private System.Timers.Timer timer;  //定时记录数据
        private DateTime stopTime;          //结束时间
        private int count;                  //记录数据列表数据个数
        private int dataIndex;              //记录数据列表数据下标
        private int enRecord;               //自动保存记录使能
        private System.Timers.Timer enTimer;//定时使能自动记录数据
        private int timeInterval;           //记录时间间隔
        private DateTime lastRecordTime;    //上一次记录数据的时间

        private ConcurrentQueue<ListViewItem> itemQueue = new ConcurrentQueue<ListViewItem>();         //数据表缓存
        private ConcurrentQueue<ListViewItem> itemQueueToExcel = new ConcurrentQueue<ListViewItem>();  //存储到Excel表数据缓存
        private System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();             //数据表更新定时器

        private AutoFormSize autoFormSize = new AutoFormSize();             //自适应屏幕分辨率
        private string enPath = MyDevice.D_datPath + @"\enAutoRecord.txt";  //设置使能文件路径

        private ResourceManager res = new ResourceManager("Base.Properties.language", typeof(MeasureDevice).Assembly);  //加载语言资源文件

        //构造函数
        public MeasureDevice()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeasureDevice_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            textBox2.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            tbUSL.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);
            tbLSL.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_RationalNumber);

            //初始化listBox和数值和图标
            update_ListText();

            //初始显示参数
            update_OutText();

            //初始显示参数
            update_Picture();

            //初始化数据列表显示
            update_ListView();

            //依据权限设置按键可用性
            if ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            {
                button_zero.Enabled = false;
            }
            else
            {
                button_zero.Enabled = true;
            }

            //初始化combox2记录时间
            comboBox2.SelectedIndex = 0;

            //
            switch (MyDevice.protocol.type)
            {
                default:
                case COMP.SelfUART:
                case COMP.CANopen:
                case COMP.ModbusTCP:
                    break;

                case COMP.RS485:
                    if (MyDevice.protocol.baudRate == 1200)
                    {
                        timer1.Interval = 250;
                    }
                    else if (MyDevice.protocol.baudRate == 2400)
                    {
                        timer1.Interval = 150;
                    }
                    else
                    {
                        timer1.Interval = 100;
                    }
                    break;
            }
            autoFormSize.UIComponetForm(this);
            start_dataMonitor();

            timer1.Enabled = true;
            // 初始化数据记录定时器
            updateTimer.Interval = 250;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Enabled = true;

            //
            if (actXET.E_bohrcode == -1)
            {
                switch (actXET.S_DeviceType)
                {
                    default:
                        break;

                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("模拟量4-20mA/0-5V/0-10V输出需要返厂校准");
                        }
                        else
                        {
                            MessageBox.Show("Analog 4-20mA/0-5V/0-10V output needs to be calibrated back to the factory");
                        }
                        break;

                    case TYPE.T420:
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("模拟量4-20mA输出需要返厂校准");
                        }
                        else
                        {
                            MessageBox.Show("Analog 4-20mA output needs to be calibrated back to the factory");
                        }
                        break;

                    case TYPE.BE30AH:
                    case TYPE.BS600H:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                    case TYPE.T4X600H:
                        if (MyDevice.languageType == 0)
                        {
                            MessageBox.Show("模拟量电压输出需要返厂校准");
                        }
                        else
                        {
                            MessageBox.Show("Analog voltage output needs to be returned to factory for calibration");
                        }
                        break;

                    case TYPE.TDES:
                    case TYPE.TDSS:
                        if (actXET.S_OutType == OUT.UT420)
                        {
                            if (MyDevice.languageType == 0)
                            {
                                MessageBox.Show("模拟量4-20mA输出需要返厂校准");
                            }
                            else
                            {
                                MessageBox.Show("Analog 4-20mA output needs to be calibrated back to the factory");
                            }
                        }
                        else
                        {
                            if (MyDevice.languageType == 0)
                            {
                                MessageBox.Show("模拟量电压输出需要返厂校准");
                            }
                            else
                            {
                                MessageBox.Show("Analog voltage output needs to be returned to factory for calibration");
                            }
                        }
                        break;
                }
            }

            //数据自动记录 初始化
            if (File.Exists(enPath))
            {
                using (StreamReader sr = new StreamReader(enPath, Encoding.GetEncoding("gb2312")))
                {
                    enAutoRecord.Checked = Convert.ToInt16(File.ReadAllText(enPath)) == 1 ? true : false;
                }
            }

            if (enAutoRecord.Checked == true)
            {
                buttonX6.Enabled = false;
                comboBox2.Enabled = false;
                stopTime = DateTime.Now.AddDays(1);
                enTimer = new System.Timers.Timer();
                enTimer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                enTimer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                enTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
            else
            {
                buttonX6.Enabled = true;
                comboBox2.Enabled = true;
            }
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeasureDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            MyDevice.mePort_StopDacout();

            timer1.Enabled = false;
            updateTimer.Enabled = false;

            //单设备使能自动保存
            if (enAutoRecord.Checked == true)
            {
                stopTime = DateTime.Now;
            }

            //若有多设备连接则跳转到多设备界面
            if (MyDevice.devSum > 1)
            {
                Main.isMeasure = false;
                Main.callDelegate();
            }
        }

        /// <summary>
        /// 绘图大小调整
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeasureDevice_SizeChanged(object sender, EventArgs e)
        {
            drawPicture.Height = pictureBox1.Height;
            drawPicture.Width = pictureBox1.Width;
            pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();
        }

        /// <summary>
        /// 界面随窗体大小变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeasureDevice_Resize(object sender, EventArgs e)
        {
            autoFormSize.UIComponetForm_Resize(this);
        }

        /// <summary>
        /// 界面大小变化时，数字显示位置更着变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void signalOutput1_SizeChanged(object sender, EventArgs e)
        {
            int locationX = (panel1.Location.X + panel1.Width) / 2 - signalOutput1.Width - 100;
            signalOutput1.Location = new Point(locationX, signalOutput1.Location.Y);
            signalStable1.Location = new Point(locationX + signalOutput1.Width + 20, signalStable1.Location.Y);
            signalUnit1.Location = new Point(locationX + signalOutput1.Width + 12, signalUnit1.Location.Y);
        }

        /// <summary>
        /// 模拟量或净毛重状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_Picture();

            if (comboBox1.SelectedValue.ToString() == "模拟量" || comboBox1.SelectedValue.ToString() == "重量")
            {
                if (MyDevice.protocol.trTASK == TASKS.ADC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                }

                //更新界面显示
                label1.Text = "";
                button_tare.Visible = true;
                button_zero.Visible = true;
                signalStable1.Visible = false;
                comboBox_unit.Visible = false;

                if (drawPicture != null)
                {
                    drawPicture.Data[0].Clear();
                }

                //单位
                switch (actXET.S_OutType)
                {
                    case OUT.UT420:
                        signalUnit1.Text = "mA";
                        break;

                    case OUT.UTP05:
                    case OUT.UTP10:
                    case OUT.UTN05:
                    case OUT.UTN10:
                        signalUnit1.Text = "V";
                        break;

                    default:
                        signalUnit1.Text = "";
                        break;
                }
            }
            else if (comboBox1.SelectedValue.ToString() == "mV/V")
            {
                if (MyDevice.protocol.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                }

                //更新界面显示
                label1.Text = "";
                signalUnit1.Text = "mV/V";
                button_tare.Visible = false;
                button_zero.Visible = false;
                signalStable1.Visible = false;
                comboBox_unit.Visible = false;
            }
            else if (comboBox1.SelectedValue.ToString() == "内码")
            {
                if (MyDevice.protocol.trTASK == TASKS.DAC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                }

                //更新界面显示
                label1.Text = "";
                signalUnit1.Text = "";
                button_tare.Visible = false;
                button_zero.Visible = false;
                signalStable1.Visible = false;
                comboBox_unit.Visible = false;
            }
            else
            {
                //更新控件显示
                label1.Visible = true;
                button_tare.Visible = true;
                button_zero.Visible = true;
                signalStable1.Visible = true;

                //单位
                signalUnit1.Text = UnitHelper.GetUnitDescription((UNIT)actXET.E_wt_unit);
                string unitCategory = UnitHelper.GetUnitCategory((UNIT)actXET.E_wt_unit);
                var unitItems = UnitHelper.GetUnitDescriptionsByCategory(UNIT.无, unitCategory);
                comboBox_unit.Items.Clear();
                switch (unitCategory)
                {
                    case "重量":
                    case "力":
                    case "扭矩":
                    case "压力":
                    case "温度":
                        foreach (var item in unitItems)
                        {
                            comboBox_unit.Items.Add(item);
                        }
                        comboBox_unit.Visible = true;
                        break;
                    case "无":
                    case "其它":
                    default:
                        comboBox_unit.Visible = false;
                        return;
                }

                for (int i = 0; i < comboBox_unit.Items?.Count; i++)
                {
                    if (comboBox_unit.Items[i].ToString() == signalUnit1.Text)
                    {
                        comboBox_unit.SelectedIndex = i;
                        break;
                    }
                }

                drawPicture.Data[0].Clear();
            }

            Main.SelectMeasure = comboBox1.SelectedValue.ToString();
            label5.Text = signalUnit1.Text;
            label9.Text = signalUnit1.Text;
            label11.Text = signalUnit1.Text;

            switch (actXET.S_DeviceType)
            {
                case TYPE.T4X600H:
                    //该型号没有去皮
                    button_tare.Visible = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 切换单位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_Picture();
        }

        /// <summary>
        /// 归零
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string tip;
            switch (actXET.S_DeviceType)
            {
                case TYPE.T4X600H:
                    tip = "是否预紧力全部归零？";
                    break;
                default:
                    tip = "是否净重、毛重、皮重全部归零？";
                    break;
            }
            var res = MessageBox.Show($"{tip}", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (res == DialogResult.OK)
            {
                isClickZero = true;
                button_zero.HoverBackColor = Color.Firebrick;
                Refresh();
                nextTask = TASKS.ZERO;
            }
        }

        /// <summary>
        /// 扣重
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            isClickTare = true;
            button_tare.HoverBackColor = Color.Firebrick;
            Refresh();
            nextTask = TASKS.TARE;
        }

        /// <summary>
        /// 更新listBox表
        /// </summary>
        public void update_ListText()
        {
            byte deci;
            string unit;

            //MenuParaRS485Form修改站点流程
            //确定后先拷贝迁移数据
            //然后系执行BCC->WRX5写入
            //写入过程中的串口委托都会刷新本窗口update_ListText
            //但是actXET指向的addr还是旧的地址
            //在WRX5写入完成后才会修改站号
            //但是已经没有串口委托了
            //因此主界面MenuParaRS485Form关闭时需要触发一下委托来更新本窗口update_ListText
            //这样才能让actXET指向的新的addr

            actXET = MyDevice.actDev;

            //初始化comboBox和label和button
            //归零后会发读SCT3导致update_ListText()被触发更新combobox1的选择
            //归零前是什么，归零后应该保持什么，不要改变combobox1的选择，故需要判断isClickZero
            if (!isClickZero)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Text", typeof(string));
                dt.Columns.Add("Value", typeof(string));
                switch (actXET.S_DeviceType)
                {
                    default:
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.T420:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                        dt.Rows.Add(res.GetString("模拟量"), "模拟量");
                        dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                        dt.Rows.Add(res.GetString("内码"), "内码");
                        label1.Text = "";
                        label2.Text = "";
                        break;

                    case TYPE.T4X600H:
                        dt.Rows.Add(res.GetString("预紧力"), "预紧力");
                        dt.Rows.Add(res.GetString("模拟量"), "模拟量");
                        dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                        dt.Rows.Add(res.GetString("内码"), "内码");
                        label1.Text = "";
                        label2.Text = "ID=" + actXET.E_addr.ToString();
                        break;

                    case TYPE.BE30AH:
                    case TYPE.TDES:
                    case TYPE.TDSS:
                        switch (MyDevice.protocol.type)
                        {
                            default:
                            case COMP.SelfUART:
                                dt.Rows.Add(res.GetString("模拟量"), "模拟量");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "";
                                label2.Text = "";
                                break;

                            case COMP.RS485:
                                dt.Rows.Add(res.GetString("模拟量"), "模拟量");
                                dt.Rows.Add(res.GetString("毛重"), "毛重");
                                dt.Rows.Add(res.GetString("净重"), "净重");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "";
                                label2.Text = "ID=" + actXET.E_addr.ToString();
                                break;
                        }
                        break;

                    case TYPE.TD485:
                    case TYPE.iBus:
                        switch (MyDevice.protocol.type)
                        {
                            case COMP.SelfUART:
                                dt.Rows.Add(res.GetString("重量"), "重量");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "";
                                label2.Text = "";
                                break;

                            default:
                            case COMP.RS485:
                                dt.Rows.Add(res.GetString("毛重"), "毛重");
                                dt.Rows.Add(res.GetString("净重"), "净重");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "净重";
                                label2.Text = "ID=" + actXET.E_addr.ToString();
                                break;
                        }
                        break;

                    case TYPE.iNet:
                    case TYPE.iStar:
                        switch (MyDevice.protocol.type)
                        {
                            default:
                            case COMP.RS485:
                            case COMP.ModbusTCP:
                                dt.Rows.Add(res.GetString("毛重"), "毛重");
                                dt.Rows.Add(res.GetString("净重"), "净重");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "净重";
                                label2.Text = "ID=" + actXET.E_addr.ToString();
                                break;
                        }
                        break;

                    case TYPE.TCAN:
                        switch (MyDevice.protocol.type)
                        {
                            default:
                            case COMP.SelfUART:
                                dt.Rows.Add(res.GetString("重量"), "重量");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "";
                                label2.Text = "";
                                break;

                            case COMP.CANopen:
                                dt.Rows.Add(res.GetString("重量"), "重量");
                                dt.Rows.Add(res.GetString("mV/V"), "mV/V");
                                dt.Rows.Add(res.GetString("内码"), "内码");
                                label1.Text = "";
                                label2.Text = "ID=" + actXET.E_nodeID.ToString();
                                break;
                        }
                        break;
                }
                comboBox1.DisplayMember = "Text";  //需显示的字段，支持中英文
                comboBox1.ValueMember = "Value";   //对应的值，中文
                comboBox1.DataSource = dt;
                //？
                if (Main.ActiveForm.Contains("SetCalibration") && !(Main.SelectMeasure == "毛重" || Main.SelectMeasure == "净重" || Main.SelectMeasure == "预紧力"))
                {
                    for (int i = 0; i < comboBox1.Items.Count; i++)
                    {
                        if (comboBox1.GetItemText(comboBox1.Items[i]) == Main.SelectMeasure)
                        {
                            comboBox1.SelectedIndex = i;
                            label1.Text = "";
                            break;
                        }
                    }
                }
                else
                {
                    if (actXET.S_DeviceType == TYPE.T4X600H)//T4X600H 优先预紧力
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                    else if (comboBox1.Items.Count == 5)   //有模拟量毛重净重 默认选模拟量
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                    else if (comboBox1.Items.Count == 4)   //有毛重净重 默认选净重
                    {
                        comboBox1.SelectedIndex = 1;
                    }
                    else                                   //选模拟量或重量
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                }
                //可见
                switch (MyDevice.protocol.type)
                {
                    default:
                    case COMP.SelfUART:
                        signalStable1.Visible = false;
                        button_tare.Visible = false;
                        label2.Visible = false;
                        break;

                    case COMP.RS485:
                        signalStable1.Visible = comboBox1.SelectedValue.ToString() == "模拟量" ? false : true;
                        button_tare.Visible = true;
                        label2.Visible = true;
                        break;

                    case COMP.CANopen:
                        signalStable1.Visible = false;
                        button_tare.Visible = false;
                        label2.Visible = true;
                        break;

                    case COMP.ModbusTCP:
                        signalStable1.Visible = comboBox1.SelectedValue.ToString() == "模拟量" ? false : true;
                        button_tare.Visible = true;
                        label2.Visible = true;
                        break;
                }

                switch (actXET.S_DeviceType)
                {
                    case TYPE.T4X600H:
                        //该型号没有去皮
                        button_tare.Visible = false;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                isClickZero = false;
            }

            //列表数据
            deci = actXET.S_decimal;
            unit = " " + actXET.S_unit;
            string unitUmask = actXET.GetUnitUMASK();
            string unitUmaskSolo = UnitHelper.GetUnitAdjustedDescription((UNIT)actXET.E_wt_unit);//使数字量单位列不显示""
            string sens = actXET.RefreshSens();

            if (MyDevice.languageType == 0)
            {
                //中文
                listBox1.Items.Clear();
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("FWx  = 0x" + actXET.E_test.ToString("X2"));
                listBox1.Items.Add("UID  = 0x" + actXET.E_bohrcode.ToString("X2"));
                listBox1.Items.Add("型号 = " + actXET.S_DeviceType.ToString());
                listBox1.Items.Add("配置 = " + actXET.S_OutType.ToString());
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("采样参数   = 0x" + actXET.E_adspeed.ToString("X2"));
                listBox1.Items.Add("上电归零   = 0x" + actXET.E_autozero.ToString("X2"));
                listBox1.Items.Add("零点跟踪   = 0x" + actXET.E_trackzero.ToString("X2"));
                listBox1.Items.Add("校准时间   = " + actXET.E_mfg_date.ToString());
                listBox1.Items.Add("序列号     = " + actXET.E_mfg_srno.ToString());
                listBox1.Items.Add("低温度记录 = " + actXET.S_tmp_min);
                listBox1.Items.Add("高温度记录 = " + actXET.S_tmp_max);
                listBox1.Items.Add("按键SPAN锁 = " + actXET.E_enspan.ToString());
                listBox1.Items.Add("----------------");
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CTWOPT:
                        listBox1.Items.Add("零点输入 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("满点输入 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("零点输出 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("满点输出 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("灵敏度   = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("内码1    = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("内码5    = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("数值1    = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("数值5    = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("零点内码 = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("满点内码 = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("零点数值 = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("满点数值 = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                        listBox1.Items.Add("输入1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("输入2    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("输入3    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("输入4    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("输入5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("输出1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出2    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出3    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出4    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("内码1    = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("内码2    = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("内码3    = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("内码4    = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("内码5    = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("数值1    = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("数值2    = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("数值3    = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("数值4    = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("数值5    = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("零点内码 = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("满点内码 = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("零点数值 = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("满点数值 = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("灵敏度   = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("e_wtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBox1.Items.Add("e_atio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBox1.Items.Add("e_btio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBox1.Items.Add("e_ctio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBox1.Items.Add("e_dtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        listBox1.Items.Add("输入1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("输入2    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("输入3    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("输入4    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("输入5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("输入6    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V");
                        listBox1.Items.Add("输入7    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V");
                        listBox1.Items.Add("输入8    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V");
                        listBox1.Items.Add("输入9    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V");
                        listBox1.Items.Add("输入10   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V");
                        listBox1.Items.Add("输入11   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V");
                        listBox1.Items.Add("输出1    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出2    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出3    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出4    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出5    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出6    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出7    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出8    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出9    = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出10   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                        listBox1.Items.Add("输出11   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("内码1    = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("内码2    = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("内码3    = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("内码4    = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("内码5    = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("内码6    = " + actXET.E_ad_point6.ToString());
                        listBox1.Items.Add("内码7    = " + actXET.E_ad_point7.ToString());
                        listBox1.Items.Add("内码8    = " + actXET.E_ad_point8.ToString());
                        listBox1.Items.Add("内码9    = " + actXET.E_ad_point9.ToString());
                        listBox1.Items.Add("内码10   = " + actXET.E_ad_point10.ToString());
                        listBox1.Items.Add("内码11   = " + actXET.E_ad_point11.ToString());
                        listBox1.Items.Add("数值1    = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("数值2    = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("数值3    = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("数值4    = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("数值5    = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("数值6    = " + actXET.E_da_point6.ToString());
                        listBox1.Items.Add("数值7    = " + actXET.E_da_point7.ToString());
                        listBox1.Items.Add("数值8    = " + actXET.E_da_point8.ToString());
                        listBox1.Items.Add("数值9    = " + actXET.E_da_point9.ToString());
                        listBox1.Items.Add("数值10   = " + actXET.E_da_point10.ToString());
                        listBox1.Items.Add("数值11   = " + actXET.E_da_point11.ToString());
                        listBox1.Items.Add("零点内码 = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("满点内码 = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("零点数值 = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("满点数值 = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("灵敏度   = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("e_wtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBox1.Items.Add("e_atio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBox1.Items.Add("e_btio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBox1.Items.Add("e_ctio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBox1.Items.Add("e_dtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBox1.Items.Add("e_etio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_etio).ToString());
                        listBox1.Items.Add("e_ftio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ftio).ToString());
                        listBox1.Items.Add("e_gtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_gtio).ToString());
                        listBox1.Items.Add("e_htio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_htio).ToString());
                        listBox1.Items.Add("e_itio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_itio).ToString());
                        listBox1.Items.Add("e_jtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_jtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                }
                switch (actXET.S_DeviceType)
                {
                    case TYPE.BE30AH:
                    case TYPE.TDSS:
                    case TYPE.TDES:
                    case TYPE.TD485:
                    case TYPE.T4X600H:
                        listBox1.Items.Add("校验方式     = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("站点地址     = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("通讯波特率   = 1200"); break;
                            case 1: listBox1.Items.Add("通讯波特率   = 2400"); break;
                            case 2: listBox1.Items.Add("通讯波特率   = 4800"); break;
                            case 3: listBox1.Items.Add("通讯波特率   = 9600"); break;
                            case 4: listBox1.Items.Add("通讯波特率   = 14400"); break;
                            case 5: listBox1.Items.Add("通讯波特率   = 19200"); break;
                            case 6: listBox1.Items.Add("通讯波特率   = 38400"); break;
                            case 7: listBox1.Items.Add("通讯波特率   = 57600"); break;
                            case 8: listBox1.Items.Add("通讯波特率   = 115200"); break;
                            case 9: listBox1.Items.Add("通讯波特率   = 230400"); break;
                            case 10: listBox1.Items.Add("通讯波特率   = 256000"); break;
                        }
                        listBox1.Items.Add("通讯停止位   = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("通讯校验位   = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("数字量零点   = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("数字量满点   = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("数字量小数点 = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("数字量单位   = " + unitUmaskSolo);  //数字量单位由actXET.E_wt_unit的值得出
                        listBox1.Items.Add("连续发送格式 = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.TCAN:
                        listBox1.Items.Add("数字量零点   = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("数字量满点   = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("数字量小数点 = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("数字量分度值 = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("数字量单位   = " + unitUmaskSolo);
                        listBox1.Items.Add("心跳时间     = " + actXET.E_heartBeat.ToString());
                        listBox1.Items.Add("TPDO0类型    = " + actXET.E_typeTPDO0.ToString());
                        listBox1.Items.Add("TPDO0时间    = " + actXET.E_evenTPDO0.ToString());
                        listBox1.Items.Add("节点ID       = " + actXET.E_nodeID.ToString());
                        switch (actXET.E_nodeBaud)
                        {
                            case 0: listBox1.Items.Add("节点波特率   = 10 kbps"); break;
                            case 1: listBox1.Items.Add("节点波特率   = 20 kbps"); break;
                            case 2: listBox1.Items.Add("节点波特率   = 50 kbps"); break;
                            case 3: listBox1.Items.Add("节点波特率   = 125 kbps"); break;
                            case 4: listBox1.Items.Add("节点波特率   = 250 kbps"); break;
                            case 5: listBox1.Items.Add("节点波特率   = 500 kbps"); break;
                            case 6: listBox1.Items.Add("节点波特率   = 800 kbps"); break;
                            case 7: listBox1.Items.Add("节点波特率   = 1000 kbps"); break;
                        }
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("CFG状态      = " + actXET.E_enGFC.ToString());
                        listBox1.Items.Add("信息方向     = " + actXET.E_enSRDO.ToString());
                        listBox1.Items.Add("SCT时间      = " + actXET.E_SCT_time.ToString() + "ms");
                        listBox1.Items.Add("COB-ID1      = " + "0x" + actXET.E_COB_ID1.ToString("X2"));
                        listBox1.Items.Add("COB-ID2      = " + "0x" + actXET.E_COB_ID2.ToString("X2"));
                        listBox1.Items.Add("超载报警     = " + actXET.E_enOL.ToString());
                        listBox1.Items.Add("超载值       = " + actXET.E_overload.ToString());
                        listBox1.Items.Add("报警模式     = " + actXET.E_alarmMode.ToString());
                        listBox1.Items.Add("目标报警     = " + actXET.T_wetTarget.ToString());
                        listBox1.Items.Add("区间低报警值 = " + actXET.T_wetLow.ToString());
                        listBox1.Items.Add("区间高报警值 = " + actXET.T_wetHigh.ToString());
                        listBox1.Items.Add("TPDO0锁定    = " + actXET.E_lockTPDO0.ToString());
                        listBox1.Items.Add("TPDO0条目    = " + actXET.E_entrTPDO0.ToString());
                        listBox1.Items.Add("TPDO1类型    = " + actXET.E_typeTPDO1.ToString());
                        listBox1.Items.Add("TPDO1锁定    = " + actXET.E_lockTPDO1.ToString());
                        listBox1.Items.Add("TPDO1条目    = " + actXET.E_entrTPDO1.ToString());
                        listBox1.Items.Add("TPDO1时间    = " + actXET.E_evenTPDO1.ToString());
                        listBox1.Items.Add("缩放比例     = " + actXET.E_scaling.ToString("0.0"));
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iBus:
                        listBox1.Items.Add("校验方式     = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("站点地址     = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("通讯波特率   = 1200"); break;
                            case 1: listBox1.Items.Add("通讯波特率   = 2400"); break;
                            case 2: listBox1.Items.Add("通讯波特率   = 4800"); break;
                            case 3: listBox1.Items.Add("通讯波特率   = 9600"); break;
                            case 4: listBox1.Items.Add("通讯波特率   = 14400"); break;
                            case 5: listBox1.Items.Add("通讯波特率   = 19200"); break;
                            case 6: listBox1.Items.Add("通讯波特率   = 38400"); break;
                            case 7: listBox1.Items.Add("通讯波特率   = 57600"); break;
                            case 8: listBox1.Items.Add("通讯波特率   = 115200"); break;
                            case 9: listBox1.Items.Add("通讯波特率   = 230400"); break;
                            case 10: listBox1.Items.Add("通讯波特率   = 256000"); break;
                        }
                        listBox1.Items.Add("通讯停止位   = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("通讯校验位   = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("数字量零点   = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("数字量满点   = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("数字量小数点 = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("数字量分度值 = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("数字量单位   = " + unitUmaskSolo);
                        listBox1.Items.Add("连续发送格式 = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("稳定次数     = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("滤波深度     = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("抗振动等级   = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("蠕变跟踪     = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("稳定范围     = " + actXET.E_stablerange.ToString());
                        listBox1.Items.Add("稳定时间     = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("零点跟踪时间 = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("动态跟踪时间 = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iNet:
                        listBox1.Items.Add("校验方式     = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("站点地址     = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("通讯波特率   = 1200"); break;
                            case 1: listBox1.Items.Add("通讯波特率   = 2400"); break;
                            case 2: listBox1.Items.Add("通讯波特率   = 4800"); break;
                            case 3: listBox1.Items.Add("通讯波特率   = 9600"); break;
                            case 4: listBox1.Items.Add("通讯波特率   = 14400"); break;
                            case 5: listBox1.Items.Add("通讯波特率   = 19200"); break;
                            case 6: listBox1.Items.Add("通讯波特率   = 38400"); break;
                            case 7: listBox1.Items.Add("通讯波特率   = 57600"); break;
                            case 8: listBox1.Items.Add("通讯波特率   = 115200"); break;
                            case 9: listBox1.Items.Add("通讯波特率   = 230400"); break;
                            case 10: listBox1.Items.Add("通讯波特率   = 256000"); break;
                        }
                        listBox1.Items.Add("通讯停止位   = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("通讯校验位   = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("数字量零点   = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("数字量满点   = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("数字量小数点 = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("数字量分度值 = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("数字量单位   = " + unitUmaskSolo);
                        listBox1.Items.Add("连续发送格式 = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("稳定次数     = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("滤波深度     = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("抗振动等级   = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("蠕变跟踪     = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("稳定时间     = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("零点跟踪时间 = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("动态跟踪时间 = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("设置滤波范围 = " + actXET.E_filter.ToString());
                        listBox1.Items.Add("主机端口号   = " + actXET.E_netServicePort.ToString());
                        listBox1.Items.Add("主机IP地址   = " + actXET.GetIpAddressFromArray(actXET.E_netServiceIP));
                        listBox1.Items.Add("本机IP地址   = " + actXET.GetIpAddressFromArray(actXET.E_netClientIP));
                        listBox1.Items.Add("本机网关地址 = " + actXET.GetIpAddressFromArray(actXET.E_netGatIP));
                        listBox1.Items.Add("本机子网掩码 = " + actXET.GetIpAddressFromArray(actXET.E_netMaskIP));
                        listBox1.Items.Add("DHCP方式     = " + actXET.E_useDHCP.ToString());
                        listBox1.Items.Add("Scan方式     = " + actXET.E_useScan.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iStar:
                        listBox1.Items.Add("校验方式     = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("站点地址     = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("通讯波特率   = 1200"); break;
                            case 1: listBox1.Items.Add("通讯波特率   = 2400"); break;
                            case 2: listBox1.Items.Add("通讯波特率   = 4800"); break;
                            case 3: listBox1.Items.Add("通讯波特率   = 9600"); break;
                            case 4: listBox1.Items.Add("通讯波特率   = 14400"); break;
                            case 5: listBox1.Items.Add("通讯波特率   = 19200"); break;
                            case 6: listBox1.Items.Add("通讯波特率   = 38400"); break;
                            case 7: listBox1.Items.Add("通讯波特率   = 57600"); break;
                            case 8: listBox1.Items.Add("通讯波特率   = 115200"); break;
                            case 9: listBox1.Items.Add("通讯波特率   = 230400"); break;
                            case 10: listBox1.Items.Add("通讯波特率   = 256000"); break;
                        }
                        listBox1.Items.Add("通讯停止位   = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("通讯校验位   = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("数字量零点   = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("数字量满点   = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("数字量小数点 = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("数字量分度值 = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("数字量单位   = " + unitUmaskSolo);
                        listBox1.Items.Add("连续发送格式 = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("稳定次数     = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("滤波深度     = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("抗振动等级   = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("蠕变跟踪     = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("稳定时间     = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("零点跟踪时间 = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("动态跟踪时间 = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("设置滤波范围 = " + actXET.E_filter.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    default:
                        break;
                }
                if (actXET.R_eepversion.Length > 0)
                {
                    listBox1.Items.Add("TEDS数据ver = " + actXET.R_eepversion);
                }
            }
            else
            {
                //英文
                listBox1.Items.Clear();
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("FWx  = 0x" + actXET.E_test.ToString("X2"));
                listBox1.Items.Add("UID  = 0x" + actXET.E_bohrcode.ToString("X2"));
                listBox1.Items.Add("TYPE = " + actXET.S_DeviceType.ToString());
                listBox1.Items.Add("OUT  = " + actXET.S_OutType.ToString());
                listBox1.Items.Add("----------------");
                listBox1.Items.Add("adc setting   = 0x" + actXET.E_adspeed.ToString("X2"));
                listBox1.Items.Add("auto zero     = 0x" + actXET.E_autozero.ToString("X2"));
                listBox1.Items.Add("track zero    = 0x" + actXET.E_trackzero.ToString("X2"));
                listBox1.Items.Add("MFR date      = " + actXET.E_mfg_date.ToString());
                listBox1.Items.Add("MFR srno      = " + actXET.E_mfg_srno.ToString());
                listBox1.Items.Add("tmp min       = " + actXET.S_tmp_min);
                listBox1.Items.Add("tmp max       = " + actXET.S_tmp_max);
                listBox1.Items.Add("KEY SPAN lock = " + actXET.E_enspan.ToString());
                listBox1.Items.Add("----------------");
                switch ((ECVE)actXET.E_curve)
                {
                    case ECVE.CTWOPT:
                        listBox1.Items.Add("input zero  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("input full  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("output zero = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output full = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("sensibility = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("adc 1    = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("adc 5    = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("cal 1    = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("cal 5    = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("adc zero = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("adc full = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("cal zero = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("cal full = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case ECVE.CFITED:
                    case ECVE.CINTER:
                        listBox1.Items.Add("input 1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("input 2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("input 3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("input 4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("input 5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("output 1 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output 2 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output 3 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output 4 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output 5 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("adc 1    = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("adc 2    = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("adc 3    = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("adc 4    = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("adc 5    = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("cal 1    = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("cal 2    = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("cal 3    = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("cal 4    = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("cal 5    = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("adc zero = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("adc full = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("cal zero = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("cal full = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("sensibility = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("e_wtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBox1.Items.Add("e_atio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBox1.Items.Add("e_btio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBox1.Items.Add("e_ctio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBox1.Items.Add("e_dtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        listBox1.Items.Add("input1   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1).ToString() + " mV/V");
                        listBox1.Items.Add("input2   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2).ToString() + " mV/V");
                        listBox1.Items.Add("input3   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3).ToString() + " mV/V");
                        listBox1.Items.Add("input4   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4).ToString() + " mV/V");
                        listBox1.Items.Add("input5   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5).ToString() + " mV/V");
                        listBox1.Items.Add("input6   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6).ToString() + " mV/V");
                        listBox1.Items.Add("input7   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7).ToString() + " mV/V");
                        listBox1.Items.Add("input8   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8).ToString() + " mV/V");
                        listBox1.Items.Add("input9   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9).ToString() + " mV/V");
                        listBox1.Items.Add("input10  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10).ToString() + " mV/V");
                        listBox1.Items.Add("input11  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11).ToString() + " mV/V");
                        listBox1.Items.Add("output1  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output2  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output3  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output4  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output5  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output6  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output7  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output8  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output9  = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output10 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10).ToString("f" + deci) + unit);
                        listBox1.Items.Add("output11 = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11).ToString("f" + deci) + unit);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("adc1     = " + actXET.E_ad_point1.ToString());
                        listBox1.Items.Add("adc2     = " + actXET.E_ad_point2.ToString());
                        listBox1.Items.Add("adc3     = " + actXET.E_ad_point3.ToString());
                        listBox1.Items.Add("adc4     = " + actXET.E_ad_point4.ToString());
                        listBox1.Items.Add("adc5     = " + actXET.E_ad_point5.ToString());
                        listBox1.Items.Add("adc6     = " + actXET.E_ad_point6.ToString());
                        listBox1.Items.Add("adc7     = " + actXET.E_ad_point7.ToString());
                        listBox1.Items.Add("adc8     = " + actXET.E_ad_point8.ToString());
                        listBox1.Items.Add("adc9     = " + actXET.E_ad_point9.ToString());
                        listBox1.Items.Add("adc10    = " + actXET.E_ad_point10.ToString());
                        listBox1.Items.Add("adc11    = " + actXET.E_ad_point11.ToString());
                        listBox1.Items.Add("cal1     = " + actXET.E_da_point1.ToString());
                        listBox1.Items.Add("cal2     = " + actXET.E_da_point2.ToString());
                        listBox1.Items.Add("cal3     = " + actXET.E_da_point3.ToString());
                        listBox1.Items.Add("cal4     = " + actXET.E_da_point4.ToString());
                        listBox1.Items.Add("cal5     = " + actXET.E_da_point5.ToString());
                        listBox1.Items.Add("cal6     = " + actXET.E_da_point6.ToString());
                        listBox1.Items.Add("cal7     = " + actXET.E_da_point7.ToString());
                        listBox1.Items.Add("cal8     = " + actXET.E_da_point8.ToString());
                        listBox1.Items.Add("cal9     = " + actXET.E_da_point9.ToString());
                        listBox1.Items.Add("cal10    = " + actXET.E_da_point10.ToString());
                        listBox1.Items.Add("cal11    = " + actXET.E_da_point11.ToString());
                        listBox1.Items.Add("adc zero = " + actXET.E_ad_zero.ToString());
                        listBox1.Items.Add("adc full = " + actXET.E_ad_full.ToString());
                        listBox1.Items.Add("cal zero = " + actXET.E_da_zero.ToString());
                        listBox1.Items.Add("cal full = " + actXET.E_da_full.ToString());
                        listBox1.Items.Add("sensibility = " + sens);
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("e_vtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_vtio).ToString());
                        listBox1.Items.Add("e_wtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_wtio).ToString());
                        listBox1.Items.Add("e_atio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_atio).ToString());
                        listBox1.Items.Add("e_btio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_btio).ToString());
                        listBox1.Items.Add("e_ctio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ctio).ToString());
                        listBox1.Items.Add("e_dtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_dtio).ToString());
                        listBox1.Items.Add("e_etio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_etio).ToString());
                        listBox1.Items.Add("e_ftio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_ftio).ToString());
                        listBox1.Items.Add("e_gtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_gtio).ToString());
                        listBox1.Items.Add("e_htio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_htio).ToString());
                        listBox1.Items.Add("e_itio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_itio).ToString());
                        listBox1.Items.Add("e_jtio   = " + MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_jtio).ToString());
                        listBox1.Items.Add("----------------");
                        break;
                }
                switch (actXET.S_DeviceType)
                {
                    case TYPE.BE30AH:
                    case TYPE.TDSS:
                    case TYPE.TDES:
                    case TYPE.TD485:
                    case TYPE.T4X600H:
                        listBox1.Items.Add("device crc    = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("device addr   = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("baud          = 1200"); break;
                            case 1: listBox1.Items.Add("baud          = 2400"); break;
                            case 2: listBox1.Items.Add("baud          = 4800"); break;
                            case 3: listBox1.Items.Add("baud          = 9600"); break;
                            case 4: listBox1.Items.Add("baud          = 14400"); break;
                            case 5: listBox1.Items.Add("baud          = 19200"); break;
                            case 6: listBox1.Items.Add("baud          = 38400"); break;
                            case 7: listBox1.Items.Add("baud          = 57600"); break;
                            case 8: listBox1.Items.Add("baud          = 115200"); break;
                            case 9: listBox1.Items.Add("baud          = 230400"); break;
                            case 10: listBox1.Items.Add("baud          = 256000"); break;
                        }
                        listBox1.Items.Add("stop bit      = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("parity        = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("out zero      = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("out capacity  = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("decimal       = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("unit          = " + unitUmaskSolo);
                        listBox1.Items.Add("frame type    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.TCAN:
                        listBox1.Items.Add("out zero      = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("out capacity  = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("decimal       = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("division      = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("unit          = " + unitUmaskSolo);
                        listBox1.Items.Add("filt time LV  = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("filt range LV = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("heart beat    = " + actXET.E_heartBeat.ToString());
                        listBox1.Items.Add("TPDO0 type    = " + actXET.E_typeTPDO0.ToString());
                        listBox1.Items.Add("TPDO0 time    = " + actXET.E_evenTPDO0.ToString());
                        listBox1.Items.Add("node ID       = " + actXET.E_nodeID.ToString());
                        listBox1.Items.Add("node baud     = " + actXET.E_nodeBaud.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("CFG           = " + actXET.E_enGFC.ToString());
                        listBox1.Items.Add("SRDO          = " + actXET.E_enSRDO.ToString());
                        listBox1.Items.Add("SCT time      = " + actXET.E_SCT_time.ToString() + "ms");
                        listBox1.Items.Add("COB-ID1       = " + "0x" + actXET.E_COB_ID1.ToString("X2"));
                        listBox1.Items.Add("COB-ID2       = " + "0x" + actXET.E_COB_ID2.ToString("X2"));
                        listBox1.Items.Add("overload alarm= " + actXET.E_enOL.ToString());
                        listBox1.Items.Add("overload      = " + actXET.E_overload.ToString());
                        listBox1.Items.Add("alarm mode    = " + actXET.E_alarmMode.ToString());
                        listBox1.Items.Add("target        = " + actXET.T_wetTarget.ToString());
                        listBox1.Items.Add("low range     = " + actXET.T_wetLow.ToString());
                        listBox1.Items.Add("high range    = " + actXET.T_wetHigh.ToString());
                        listBox1.Items.Add("TPDO0 lock    = " + actXET.E_lockTPDO0.ToString());
                        listBox1.Items.Add("TPDO0 entry   = " + actXET.E_entrTPDO0.ToString());
                        listBox1.Items.Add("TPDO1 type    = " + actXET.E_typeTPDO1.ToString());
                        listBox1.Items.Add("TPDO1 lock    = " + actXET.E_lockTPDO1.ToString());
                        listBox1.Items.Add("TPDO1 entry   = " + actXET.E_entrTPDO1.ToString());
                        listBox1.Items.Add("TPDO1 time    = " + actXET.E_evenTPDO1.ToString());
                        listBox1.Items.Add("scaling       = " + actXET.E_scaling.ToString("0.0"));
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iBus:
                        listBox1.Items.Add("device crc    = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("device addr   = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("baud          = 1200"); break;
                            case 1: listBox1.Items.Add("baud          = 2400"); break;
                            case 2: listBox1.Items.Add("baud          = 4800"); break;
                            case 3: listBox1.Items.Add("baud          = 9600"); break;
                            case 4: listBox1.Items.Add("baud          = 14400"); break;
                            case 5: listBox1.Items.Add("baud          = 19200"); break;
                            case 6: listBox1.Items.Add("baud          = 38400"); break;
                            case 7: listBox1.Items.Add("baud          = 57600"); break;
                            case 8: listBox1.Items.Add("baud          = 115200"); break;
                            case 9: listBox1.Items.Add("baud          = 230400"); break;
                            case 10: listBox1.Items.Add("baud          = 256000"); break;
                        }
                        listBox1.Items.Add("stop bit      = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("parity        = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("out zero      = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("out capacity  = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("decimal       = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("division      = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("unit          = " + unitUmaskSolo);
                        listBox1.Items.Add("frame type    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("filt time LV  = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("filt range LV = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("antivib LV    = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("dyn track     = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("stable range   = " + actXET.E_stablerange.ToString());
                        listBox1.Items.Add("stable time   = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("tk zero time  = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("dy zero time  = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iNet:
                        listBox1.Items.Add("device crc     = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("device addr    = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("baud         = 1200"); break;
                            case 1: listBox1.Items.Add("baud         = 2400"); break;
                            case 2: listBox1.Items.Add("baud         = 4800"); break;
                            case 3: listBox1.Items.Add("baud         = 9600"); break;
                            case 4: listBox1.Items.Add("baud         = 14400"); break;
                            case 5: listBox1.Items.Add("baud         = 19200"); break;
                            case 6: listBox1.Items.Add("baud         = 38400"); break;
                            case 7: listBox1.Items.Add("baud         = 57600"); break;
                            case 8: listBox1.Items.Add("baud         = 115200"); break;
                            case 9: listBox1.Items.Add("baud         = 230400"); break;
                            case 10: listBox1.Items.Add("baud         = 256000"); break;
                        }
                        listBox1.Items.Add("stop bit     = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("parity       = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("out zero     = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("out capacity = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("decimal      = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("division     = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("unit         = " + unitUmaskSolo);
                        listBox1.Items.Add("frame type   = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("filt time LV = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("filt range LV= " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("antivib LV   = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("dyn track    = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("stable time  = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("tk zero time = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("dy zero time = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI           = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("filter       = " + actXET.E_filter.ToString());
                        listBox1.Items.Add("server port  = " + actXET.E_netServicePort.ToString());
                        listBox1.Items.Add("server IP    = " + actXET.GetIpAddressFromArray(actXET.E_netServiceIP));
                        listBox1.Items.Add("client IP    = " + actXET.GetIpAddressFromArray(actXET.E_netClientIP));
                        listBox1.Items.Add("gat IP       = " + actXET.GetIpAddressFromArray(actXET.E_netGatIP));
                        listBox1.Items.Add("mask IP      = " + actXET.GetIpAddressFromArray(actXET.E_netMaskIP));
                        listBox1.Items.Add("DHCP         = " + actXET.E_useDHCP.ToString());
                        listBox1.Items.Add("Scan         = " + actXET.E_useScan.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    case TYPE.iStar:
                        listBox1.Items.Add("device crc    = " + actXET.E_sign.ToString());
                        listBox1.Items.Add("device addr   = " + actXET.E_addr.ToString());
                        switch (actXET.E_baud)
                        {
                            case 0: listBox1.Items.Add("baud          = 1200"); break;
                            case 1: listBox1.Items.Add("baud          = 2400"); break;
                            case 2: listBox1.Items.Add("baud          = 4800"); break;
                            case 3: listBox1.Items.Add("baud          = 9600"); break;
                            case 4: listBox1.Items.Add("baud          = 14400"); break;
                            case 5: listBox1.Items.Add("baud          = 19200"); break;
                            case 6: listBox1.Items.Add("baud          = 38400"); break;
                            case 7: listBox1.Items.Add("baud          = 57600"); break;
                            case 8: listBox1.Items.Add("baud          = 115200"); break;
                            case 9: listBox1.Items.Add("baud          = 230400"); break;
                            case 10: listBox1.Items.Add("baud          = 256000"); break;
                        }
                        listBox1.Items.Add("stop bit      = " + actXET.E_stopbit.ToString());
                        listBox1.Items.Add("parity        = " + actXET.E_parity.ToString());
                        listBox1.Items.Add("out zero      = " + actXET.T_wt_zero + " " + unitUmask);
                        listBox1.Items.Add("out capacity  = " + actXET.T_wt_full + " " + unitUmask);
                        listBox1.Items.Add("decimal       = " + actXET.E_wt_decimal.ToString());
                        listBox1.Items.Add("division      = " + actXET.E_wt_division.ToString() + "e");
                        listBox1.Items.Add("unit          = " + unitUmaskSolo);
                        listBox1.Items.Add("frame type    = " + actXET.E_wt_ascii.ToString());
                        listBox1.Items.Add("filt time LV  = " + actXET.E_wt_sptime.ToString());
                        listBox1.Items.Add("filt range LV = " + actXET.E_wt_spfilt.ToString());
                        listBox1.Items.Add("antivib LV    = " + actXET.E_wt_antivib.ToString());
                        listBox1.Items.Add("dyn track     = " + actXET.E_dynazero.ToString());
                        listBox1.Items.Add("stable time   = " + actXET.E_stabletime.ToString());
                        listBox1.Items.Add("tk zero time  = " + actXET.E_tkzerotime.ToString());
                        listBox1.Items.Add("dy zero time  = " + actXET.E_tkdynatime.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_cheatype.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_thmax.ToString());
                        listBox1.Items.Add("AI            = " + actXET.E_thmin.ToString());
                        listBox1.Items.Add("----------------");
                        listBox1.Items.Add("filter        = " + actXET.E_filter.ToString());
                        listBox1.Items.Add("----------------");
                        break;
                    default:
                        break;
                }
                if (actXET.R_eepversion.Length > 0)
                {
                    listBox1.Items.Add("TEDS data ver = " + actXET.R_eepversion);
                }
            }
        }

        /// <summary>
        /// 更新显示参数
        /// </summary>
        private void update_OutText()
        {
            //如果是自己窗口,更新页面数据
            //如果是别的窗口,更新界面状态和列表参数
            if (Main.ActiveForm.Contains("MeasureDevice"))
            {
                //数值类型,不由comboBox1决定
                switch (comboBox1.SelectedValue.ToString())
                {
                    case "毛重":
                    case "净重":
                    case "预紧力":
                        label1.Text = comboBox1.Text;
                        break;
                    default:
                        label1.Text = "";
                        break;
                }

                //间隔时间=showTicker*100ms
                if (count % 10000 == 0)
                {
                    dbListView1.Items.Clear();
                }

                //更新litView
                ListViewItem item = new ListViewItem();
                count++;
                item.SubItems[0].Text = (dataIndex++).ToString();
                if (actXET.S_DeviceType == TYPE.TCAN)
                {
                    item.SubItems.Add(actXET.E_nodeID.ToString());
                }
                else
                {
                    item.SubItems.Add(actXET.E_addr.ToString());
                }
                item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss:fff"));

                //数值 & 超载
                if (actXET.R_overload)
                {
                    signalOutput1.Text = "---OL---";
                    signalOutput1.ForeColor = Color.Red;

                    //表格添加数据
                    if (actXET.R_grossnet == "毛重")
                    {
                        item.SubItems.Add("OG");
                    }
                    else
                    {
                        item.SubItems.Add("ON");
                    }
                    item.SubItems.Add(signalOutput1.Text);
                }
                else
                {
                    signalOutput1.ForeColor = Color.Black;
                    string status = "";
                    switch (comboBox1.SelectedValue.ToString())
                    {
                        case "模拟量":
                        case "重量":
                            signalOutput1.Text = actXET.R_output;

                            switch (actXET.S_OutType)
                            {
                                case OUT.UT420:
                                    signalUnit1.Text = "mA";
                                    break;

                                case OUT.UTP05:
                                case OUT.UTP10:
                                case OUT.UTN05:
                                case OUT.UTN10:
                                    signalUnit1.Text = "V";
                                    break;

                                default:
                                    signalUnit1.Text = "";
                                    break;
                            }

                            //画曲线
                            update_PictureBox(actXET.R_datum);
                            if (showTicker > 1)
                            {
                                pictureBox1.Image = drawPicture.GetForegroundImageTypeOne(actXET.R_datum);
                                showTicker = 0;
                            }
                            break;

                        case "mV/V":
                            signalOutput1.Text = actXET.RefreshmVDV().ToString("0.0000000");
                            signalUnit1.Text = "mV/V";
                            break;

                        case "内码":
                            signalOutput1.Text = actXET.R_adcout.ToString();
                            signalUnit1.Text = "";
                            break;

                        default:
                            //稳定状态写入
                            if (actXET.R_stable) status = "S";
                            else status = "U";

                            if (actXET.R_grossnet == "毛重") status += "G";
                            else status += "N";

                            string originalUnit = actXET.GetUnitUMASK();
                            if (comboBox_unit.SelectedItem == null || originalUnit == comboBox_unit.Text)
                            {
                                //不转换
                                signalUnit1.Text = originalUnit;
                                signalOutput1.Text = actXET.R_weight;
                                //画曲线S(毛重净重状态下传入实时数据)
                                update_PictureBox(actXET.R_weight);
                            }
                            else
                            {
                                //转换单位
                                signalUnit1.Text = comboBox_unit.Text;
                                signalOutput1.Text = UnitHelper.ConvertUnit(actXET.R_weight.Trim(), actXET.E_wt_decimal, originalUnit, comboBox_unit.Text);
                                string dataStr = UnitHelper.ConvertUnit(actXET.R_weight.Trim(), actXET.E_wt_decimal, originalUnit, comboBox_unit.Text, false);
                                //画曲线S(毛重净重状态下传入实时数据)
                                update_PictureBox(dataStr);
                            }
                            break;
                    }

                    //表格添加数据
                    item.SubItems.Add(status);
                    item.SubItems.Add(signalOutput1.Text.PadLeft(9, ' '));
                }

                //表格添加数据单位
                item.SubItems.Add(signalUnit1.Text);

                //若为TCAN设备，则添加十六进制转换
                if (actXET.S_DeviceType == TYPE.TCAN)
                {
                    item.SubItems.Add(BaseConvert.ConvertDecToHex(actXET.R_datum));
                }
                //iNet设备，添加ip地址
                else if (actXET.S_DeviceType == TYPE.iNet)
                {
                    item.SubItems.Add(actXET.R_ipAddr);
                }

                //将更新的listVew行添加进listView
                add_ListView(item);

                //稳定状态
                if (actXET.R_stable)
                {
                    signalStable1.LampColor = new Color[] { Color.Green };
                }
                else
                {
                    signalStable1.LampColor = new Color[] { Color.Black };
                }

                //检测外部EEPROM芯片
                if (actXET.R_checklink != actXET.R_eeplink)
                {
                    actXET.R_checklink = actXET.R_eeplink;

                    //提示
                    if (actXET.R_eeplink)
                    {
                        if (MessageBox.Show("检测到TEDS芯片插入,是否需要重新读取传感器参数?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            //MessageBox弹窗会卡接收,先StopDacout再ClearState,确保ReadTasks能顺利进行
                            MyDevice.mePort_StopDacout();
                            nextTask = TASKS.BOR;
                            MyDevice.mePort_ClearState();
                            MyDevice.mePort_ReadTasks();
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("TEDS芯片掉线");
                    }
                }
            }
            else if (Main.ActiveForm.Contains("SetConnect") ||
                     Main.ActiveForm.Contains("SetParRS485") ||
                     Main.ActiveForm.Contains("SetParCANopen") ||
                     Main.ActiveForm.Contains("SetParModbusTCP") ||
                     Main.ActiveForm.Contains("SetParCal") ||
                     Main.ActiveForm.Contains("SetCalibrationUpdate")
                     )
            {
                update_ListText();
            }
        }

        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="data"></param>
        private void update_PictureBox(object weight)
        {
            if (showTicker > 1)
            {
                if (weight is string weightStr && double.TryParse(weightStr, out double data))
                {
                    pictureBox1.Image = drawPicture.GetForegroundImageTypeOne(data);
                    showTicker = 0;
                }
                else if (weight is double weightDouble)
                {
                    pictureBox1.Image = drawPicture.GetForegroundImageTypeOne(weightDouble);
                    showTicker = 0;
                }
            }
        }

        /// <summary>
        /// 初始化绘图
        /// </summary>
        private void update_Picture()
        {
            int upper;
            int lower;
            string comboBox1Str = comboBox1.SelectedValue.ToString();

            drawPicture = new DrawPicture(pictureBox1.Height, pictureBox1.Width);

            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                    upper = 20;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? upper : upper * 100;
                    drawPicture.LimitLowerLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? lower : lower * 100;
                    break;

                case OUT.UTP05:
                    upper = 5;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? upper : upper * 200;
                    drawPicture.LimitLowerLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? lower : lower * 200;
                    break;

                case OUT.UTP10:
                    upper = 10;
                    lower = 0;
                    drawPicture.LimitUpperLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? upper : upper * 100;
                    drawPicture.LimitLowerLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? lower : lower * 100;
                    break;

                case OUT.UTN05:
                    upper = 5;
                    lower = -5;
                    drawPicture.LimitUpperLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? upper : upper * 200;
                    drawPicture.LimitLowerLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? lower : lower * 200;
                    break;

                case OUT.UTN10:
                    upper = 10;
                    lower = -10;
                    drawPicture.LimitUpperLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? upper : upper * 100;
                    drawPicture.LimitLowerLeftY = (comboBox1Str == "模拟量" || comboBox1Str == "重量") ? lower : lower * 100;
                    break;

                case OUT.UMASK:
                    upper = (int)(actXET.E_wt_full / Math.Pow(10, actXET.E_wt_decimal));
                    lower = 0;
                    drawPicture.LimitUpperLeftY = upper;
                    drawPicture.LimitLowerLeftY = lower;
                    break;
            }

            //选择的单位和设定的单位不一样，单位转换
            if (comboBox1Str == "毛重" || comboBox1Str == "净重" || comboBox1Str == "预紧力")
            {
                UNIT originalUnit = (UNIT)actXET.E_wt_unit;
                string UnitCategory = UnitHelper.GetUnitCategory(originalUnit);
                switch (UnitCategory)
                {
                    case "重量":
                    case "力":
                    case "扭矩":
                    case "压力":
                    case "温度":
                        if (comboBox_unit.SelectedItem != null)
                        {
                            string originalUnitStr = actXET.GetUnitUMASK();
                            if (originalUnitStr != comboBox_unit.Text)
                            {
                                if (drawPicture.LimitUpperLeftY > 0)
                                {
                                    drawPicture.LimitUpperLeftY = (int)double.Parse(UnitHelper.ConvertUnit(drawPicture.LimitUpperLeftY.ToString(), 0, originalUnit, comboBox_unit.Text, false));
                                    if (drawPicture.LimitUpperLeftY < 1) drawPicture.LimitUpperLeftY = 1;
                                }
                                if (drawPicture.LimitLowerLeftY > 0)
                                {
                                    drawPicture.LimitLowerLeftY = (int)double.Parse(UnitHelper.ConvertUnit(drawPicture.LimitLowerLeftY.ToString(), 0, originalUnit, comboBox_unit.Text, false));
                                }
                            }
                        }
                        break;
                    case "无":
                    case "其他":
                    default:
                        break;
                }
            }

            drawPicture.HorizontalAxisNum = 11;
            pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();
        }

        /// <summary>
        /// 串口通讯响应
        /// </summary>
        public void update_FromUart()
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
                if (!Main.ActiveForm.Contains("MeasureDevice") && !Main.ActiveForm.Contains("SetCalScope"))
                {
                    update_Picture();
                }

                comTicker = 0;

                update_OutText();

                switch (nextTask)
                {
                    case TASKS.BOR:
                        //继续读取
                        MyDevice.mePort_ReadTasks();
                        //读结束
                        if (MyDevice.protocol.trTASK == TASKS.NULL)
                        {
                            //界面
                            update_ListText();
                            //启动读数据
                            start_dataMonitor();
                        }
                        break;

                    case TASKS.RDX3:
                        //只读SCT3
                        if (MyDevice.protocol.trTASK == TASKS.RDX3)
                        {
                            //界面
                            update_ListText();
                            button_zero.HoverBackColor = Color.Green;
                            Refresh();
                            //读SCT3后启动读数据
                            start_dataMonitor();
                        }
                        break;

                    //执行归零任务
                    case TASKS.ZERO:
                        nextTask = TASKS.NULL;
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        MyDevice.mePort_SendCOM(TASKS.ZERO);
                        break;

                    //执行扣重任务
                    case TASKS.TARE:
                        nextTask = TASKS.NULL;
                        if (MyDevice.protocol.trTASK == TASKS.DAC)
                        {
                            //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                            MyDevice.mePort_StopDacout(MyDevice.protocol);
                        }
                        MyDevice.mePort_SendCOM(TASKS.TARE);
                        break;

                    default:
                        if (MyDevice.protocol.trTASK == TASKS.ZERO)
                        {
                            //归零后读SCT3
                            nextTask = TASKS.RDX3;
                            MyDevice.mePort_SendCOM(TASKS.RDX3);
                        }
                        else if (MyDevice.protocol.trTASK == TASKS.TARE)
                        {
                            //界面
                            button_tare.HoverBackColor = Color.Green;
                            Refresh();
                            if (isClickTare)
                            {
                                isClickTare = false;
                                if (comboBox1.SelectedValue.ToString() == "净重" || comboBox1.SelectedValue.ToString() == "毛重")
                                {
                                    //点击去皮后自动切换到净重
                                    comboBox1.SelectedValue = "净重";
                                }
                                else if (comboBox1.SelectedValue.ToString() == "预紧力")
                                {
                                    //点击去皮后自动切换到预紧力
                                    comboBox1.SelectedValue = "预紧力";
                                }
                            }
                            //扣重后读数据
                            start_dataMonitor();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 超时监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Main.ActiveForm.Contains("MeasureDevice"))
            {
                //控制显示速度
                showTicker++;

                if (MyDevice.protocol.Is_serial_listening) return;

                switch (nextTask)
                {
                    //读SCT任务
                    case TASKS.BOR:
                        //如果超时需要重新读
                        if ((++comTicker) > 5)
                        {
                            comTicker = 0;
                            MyDevice.mePort_ReadTasks();
                        }
                        break;

                    //监控任务
                    case TASKS.DAC:
                    case TASKS.QGROSS:
                    case TASKS.QNET:
                        switch (MyDevice.protocol.type)
                        {
                            default:
                            case COMP.SelfUART:
                                //自定义协议串口
                                //不管数字量模拟量
                                //都用DACO(0x68)指令启动设备连续发送dacout/weight
                                //如果没有收到02 80 80 80则comTicker计时器每300ms发送一次DACO指令
                                //如果有接收到02 80 80 80则comTicker计时器在接收委托中清零
                                if ((++comTicker) > 5)
                                {
                                    comTicker = 0;
                                    switch (comboBox1.SelectedValue.ToString())
                                    {
                                        default:
                                        case "模拟量":
                                        case "重量":
                                            MyDevice.mePort_SendCOM(TASKS.DAC);
                                            break;

                                        case "mV/V":
                                        case "内码":
                                            MyDevice.mePort_SendCOM(TASKS.ADC);
                                            break;
                                    }
                                }
                                break;

                            case COMP.RS485:
                                switch (comboBox1.SelectedValue.ToString())
                                {
                                    default:
                                    case "模拟量":
                                        //用DACO(0x68)指令启动设备连续发送dacout/weight
                                        //如果没有收到02 80 80 80则comTicker计时器每300ms发送一次DACO指令
                                        //如果有接收到02 80 80 80则comTicker计时器在接收委托中清零
                                        if ((++comTicker) > 5)
                                        {
                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.DAC);
                                        }
                                        break;

                                    case "毛重":
                                        //间隔时间询问毛重
                                        nextTask = TASKS.QGROSS;
                                        MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                        break;

                                    case "净重":
                                    case "预紧力":
                                        //间隔时间询问净重
                                        nextTask = TASKS.QNET;
                                        MyDevice.mePort_SendCOM(TASKS.QNET);
                                        break;

                                    case "mV/V":
                                    case "内码":
                                        if ((++comTicker) > 5)
                                        {
                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.ADC);
                                        }
                                        break;
                                }
                                break;

                            case COMP.CANopen:
                                if ((++comTicker) > 1)
                                {
                                    comTicker = 0;
                                    switch (comboBox1.SelectedValue.ToString())
                                    {
                                        default:
                                        case "重量":
                                            //间隔时间询问重量
                                            nextTask = TASKS.DAC;
                                            MyDevice.mePort_SendCOM(TASKS.DAC);
                                            break;

                                        case "mV/V":
                                        case "内码":
                                            MyDevice.mePort_SendCOM(TASKS.ADC);
                                            break;
                                    }
                                }
                                break;

                            case COMP.ModbusTCP:
                                switch (comboBox1.SelectedValue.ToString())
                                {
                                    case "毛重":
                                        //间隔时间询问毛重
                                        nextTask = TASKS.QGROSS;
                                        MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                        break;

                                    default:
                                    case "净重":
                                        //间隔时间询问净重
                                        nextTask = TASKS.QNET;
                                        MyDevice.mePort_SendCOM(TASKS.QNET);
                                        break;

                                    case "mV/V":
                                    case "内码":
                                        if ((++comTicker) > 5)
                                        {
                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.ADC);
                                        }
                                        break;
                                }
                                break;
                        }
                        break;

                    case TASKS.ZERO:
                        if ((++comTicker) > 5)
                        {
                            comTicker = 0;
                            MyDevice.mePort_SendCOM(TASKS.ZERO);
                        }
                        break;

                    case TASKS.TARE:
                        if ((++comTicker) > 5)
                        {
                            comTicker = 0;
                            MyDevice.mePort_SendCOM(TASKS.TARE);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 启动数据读取
        /// </summary>
        private void start_dataMonitor()
        {
            comTicker = 0;
            nextTask = TASKS.DAC;
            MyDevice.mePort_ClearState();//清除数据确保R_eeplink不会02或03误设false
        }

        #region 数据记录

        /// <summary>
        /// 初始化listView表格
        /// </summary>
        private void update_ListView()
        {
            count = 1;
            dataIndex = 1;
            itemQueueToExcel = new ConcurrentQueue<ListViewItem>();
            itemQueue = new ConcurrentQueue<ListViewItem>();
            if (MyDevice.languageType == 0)
            {
                dbListView1.Columns.Add("序号");
                dbListView1.Columns.Add("ID");
                dbListView1.Columns.Add("时间");
                dbListView1.Columns.Add("稳定状态");
                dbListView1.Columns.Add("数据");
                dbListView1.Columns.Add("单位");
                if (actXET.S_DeviceType == TYPE.TCAN)
                {
                    dbListView1.Columns.Add("16进制数");
                }
                else if (actXET.S_DeviceType == TYPE.iNet)
                {
                    dbListView1.Columns.Add("IP地址");
                }
            }
            else
            {
                dbListView1.Columns.Add("Number");
                dbListView1.Columns.Add("ID");
                dbListView1.Columns.Add("Time");
                dbListView1.Columns.Add("Stability ");
                dbListView1.Columns.Add("Data");
                dbListView1.Columns.Add("Unit");
                if (actXET.S_DeviceType == TYPE.TCAN)
                {
                    dbListView1.Columns.Add("HEX");
                }
                else if (actXET.S_DeviceType == TYPE.iNet)
                {
                    dbListView1.Columns.Add("IP");
                }
            }
            for (int i = 0; i < dbListView1.Columns.Count; i++)
            {
                dbListView1.Columns[i].Width = (int)(1.0 / dbListView1.Columns.Count * dbListView1.ClientRectangle.Width);
            }

            enTimeInterval_Click(null, null);
            timeInterval = int.Parse(textBoxEx_timeInterval.Text);
            lastRecordTime = DateTime.Now;  //确保第一条数据被记录
        }

        /// <summary>
        /// 将数据添加到ListView中
        /// </summary>
        /// <param name="item"></param>
        private void add_ListView(ListViewItem item)
        {
            if (enTimeInterval.Checked)
            {
                TimeSpan waitTime = DateTime.Now - lastRecordTime;
                if (waitTime.TotalMilliseconds > (timeInterval * 1000))
                {
                    EnqueueItem(item);
                    lastRecordTime = DateTime.Now;
                }
                else
                {
                    dataIndex--;
                }
            }
            else
            {
                EnqueueItem(item);
            }
        }

        /// <summary>
        /// 暂存要添加的 ListViewItem
        /// </summary>
        /// <param name="item"></param>
        private void EnqueueItem(ListViewItem item)
        {
            itemQueue.Enqueue(item);
        }

        /// <summary>
        /// 定期批量更新 ListView，减少重绘次数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            List<ListViewItem> itemsToAdd = new List<ListViewItem>();

            while (itemQueue.TryDequeue(out ListViewItem item))
            {
                itemsToAdd.Add(item);
            }

            if (itemsToAdd.Count > 0)
            {
                //批量更新
                dbListView1.BeginUpdate();
                try
                {
                    foreach (var item in itemsToAdd)
                    {
                        dbListView1.Items.Add(item);
                        dbListView1.Items[dbListView1.Items.Count - 1].EnsureVisible();
                        itemQueueToExcel.Enqueue(item);
                    }
                }
                finally
                {
                    dbListView1.EndUpdate();
                }
            }
        }

        /// <summary>
        /// 数据列表——开始记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX6_Click(object sender, EventArgs e)
        {
            if (buttonX6.Text == "开始记录" || buttonX6.Text == "Start")
            {
                //清空数据
                count = 1;
                dataIndex = 1;
                itemQueueToExcel = new ConcurrentQueue<ListViewItem>();
                itemQueue = new ConcurrentQueue<ListViewItem>();
                dbListView1.Items.Clear();
                //更新显示
                buttonX6.Text = MyDevice.languageType == 0 ? "停止记录" : "Stop";
                label7.Text = MyDevice.languageType == 0 ? "数据记录中" : "Recording...";
                //记录中不允许更改时间
                comboBox2.Enabled = false;
                //记录结束时间
                stopTime = System.DateTime.Now.AddMinutes(Convert.ToInt32(comboBox2.Text));
                timer = new System.Timers.Timer();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                timer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                timer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
            else
            {
                buttonX6.Text = MyDevice.languageType == 0 ? "开始记录" : "Start";
                stopTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 数据列表——清空数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX7_Click(object sender, EventArgs e)
        {
            count = 1;
            dataIndex = 1;
            itemQueueToExcel = new ConcurrentQueue<ListViewItem>();
            itemQueue = new ConcurrentQueue<ListViewItem>();
            dbListView1.Items.Clear();
        }

        /// <summary>
        /// 数据列表——连接后自动保存记录（使能）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        private void enAutoRecord_Click(object sender, EventArgs e)
        {
            enRecord = enAutoRecord.Checked == true ? 1 : 0;//使能自动保存

            if (!Directory.Exists(MyDevice.D_datPath))
            {
                Directory.CreateDirectory(MyDevice.D_datPath);
            }

            if (File.Exists(enPath))
            {
                File.SetAttributes(enPath, FileAttributes.Normal);
            }

            File.WriteAllText(enPath, enRecord.ToString());
            File.SetAttributes(enPath, FileAttributes.ReadOnly);//设置文件权限只读

            if (enAutoRecord.Checked == false)
            {
                stopTime = DateTime.Now;
                buttonX6.Enabled = true;
                comboBox2.Enabled = true;
            }
            else
            {
                buttonX6.Enabled = false;
                comboBox2.Enabled = false;
                count = 1;
                dataIndex = 1;
                itemQueueToExcel = new ConcurrentQueue<ListViewItem>();
                itemQueue = new ConcurrentQueue<ListViewItem>();
                dbListView1.Items.Clear();

                stopTime = DateTime.Now.AddDays(1);
                enTimer = new System.Timers.Timer();
                enTimer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                enTimer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                enTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
        }

        /// <summary>
        /// 数据列表——启动记录时间间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enTimeInterval_Click(object sender, EventArgs e)
        {
            if (enTimeInterval.Checked)
            {
                label_timeInterval.Visible = true;
                textBoxEx_timeInterval.Visible = true;
                label_s.Visible = true;
            }
            else
            {
                label_timeInterval.Visible = false;
                textBoxEx_timeInterval.Visible = false;
                label_s.Visible = false;
            }
        }

        /// <summary>
        /// 数据列表——修改时间间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxEx_timeInterval_TextChanged(object sender, EventArgs e)
        {
            timeInterval = int.Parse(textBoxEx_timeInterval.Text);
        }

        /// <summary>
        /// 数据列表——保存实时数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_Data(object sender, System.Timers.ElapsedEventArgs e)
        {
            //判断data目录是否存在
            if (!Directory.Exists(MyDevice.D_dataPath))
            {
                //不存在
                Directory.CreateDirectory(MyDevice.D_dataPath);
            }

            //文件路径
            string mePath;
            if (MyDevice.protocol.type == COMP.SelfUART)
            {
                if (MyDevice.languageType == 0)
                {
                    mePath = MyDevice.D_dataPath + @"\实时数据记录表_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
                }
                else
                {
                    mePath = MyDevice.D_dataPath + @"\data_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
                }
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    mePath = MyDevice.D_dataPath + @"\实时数据记录表_站点" + MyDevice.protocol.addr.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
                }
                else
                {
                    mePath = MyDevice.D_dataPath + @"\data_addr" + MyDevice.protocol.addr.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
                }
            }

            FileInfo fileInfo = new FileInfo(mePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            //保存文件
            FileStream fs = new FileStream(mePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

            //写入列名
            string data = "";
            for (int i = 0; i < dbListView1.Columns.Count; i++)
            {
                data += dbListView1.Columns[i].Text;
                if (i < dbListView1.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            //写入数据
            stopTime = DateTime.Now + new TimeSpan(0, 0, 0, 15, 100);
            while (DateTime.Compare(DateTime.Now, stopTime) < 0)
            {
                if (itemQueueToExcel.TryDequeue(out ListViewItem item))
                {
                    this.BeginInvoke(new Action(delegate
                    {
                        data = item.SubItems[0].Text + "," + item.SubItems[1].Text + "," +
                               item.SubItems[2].Text + "," + item.SubItems[3].Text + "," +
                               item.SubItems[4].Text + "," + item.SubItems[5].Text;
                        if (actXET.S_DeviceType == TYPE.TCAN || actXET.S_DeviceType == TYPE.iNet)
                        {
                            data += "," + item.SubItems[6].Text;
                        }
                        sw.WriteLine(data);
                    }));
                }
            }
            sw.Close();
            fs.Close();
            this.BeginInvoke(new Action(delegate
            {
                buttonX6.Text = MyDevice.languageType == 0 ? "开始记录" : "Start";
                label7.Text = MyDevice.languageType == 0 ? "记录完成已自动保存！" : "The data is saved automatically";
                comboBox2.Enabled = true;
            }));
        }

        #endregion

        #region CPK测试

        /// <summary>
        /// CPK测试——计算CPK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            double USL;//规格上限
            double LSL;//规格下限
            double avg;//平均数
            double sum;//各数值与平均数的差值的平方，然后求和
            double V;//方差
            double σ;// 标准差(σ)
            double T;//规格公差
            double U;//规格中心值
            double Ca;//制程准确度
            double Cp;//制程精密度
            double Cpku;//制程单边规格上限
            double Cpkl;//制程单边规格下限
            double Cpk;//制程能力指数
            double max;//最大值
            double min;//最小值

            if (tbUSL.Text == "" || tbLSL.Text == "")
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先输入USL和LSL");
                }
                else
                {
                    MessageBox.Show("Please enter USL and LSL first");
                }
                return;
            }

            if (!double.TryParse(tbUSL.Text, out USL) || !double.TryParse(tbLSL.Text, out LSL))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先输入USL和LSL");
                }
                else
                {
                    MessageBox.Show("Input USL or LSL is not reasonable, please enter rational number");
                }
                return;
            }

            if (dataGridView1.Rows.Count == 1)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先输入USL和LSL");
                }
                else
                {
                    MessageBox.Show("Write data is empty, please write data again");
                }
                return;
            }

            //取出数据
            List<double> data = new List<double>();
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                data.Add(Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value));
            }

            //  计算平均数
            avg = data.Average();

            //  计算各数值与平均数的差值的平方，然后求和
            sum = data.Sum(d => Math.Pow(d - avg, 2));

            //  计算方差
            V = sum / data.Count();

            //  计算标准差
            σ = Math.Sqrt(V);

            //  计算规格公差(T)=规格上限-规格下限
            T = USL - LSL;

            //  计算规格中心值(U)=（规格上限+规格下限）/2
            U = (USL + LSL) / 2;

            //  计算出制程准确度Ca值：Ca=(X-U)/(T/2)  (x为所有取样数据的平均值)
            Ca = (avg - U) / (T / 2);

            //  计算出制程精密度Cp值：Cp =T/6σ
            Cp = T / (6 * σ);

            //  计算出制程单边规格上限Cpku值：Cpku=(USL-avg)/(3σ)
            Cpku = (USL - avg) / (3 * σ);

            //  计算出制程单边规格下限Cpkl值：Cpkl=(avg-LSL)/(3σ)
            Cpkl = (avg - LSL) / (3 * σ);

            //  计算出制程能力指数Cpk值: Cpk=min(Cpkl,Cpku)
            Cpk = Math.Min(Cpku, Cpkl);

            //  计算最大值
            max = data.Max();

            //  计算最小值
            min = data.Min();

            //更新listBox表格
            listBox2.Items.Clear();
            listBox2.Items.Add((MyDevice.languageType == 0 ? "目标值：" : "Target value:") + textBox2.Text);
            listBox2.Items.Add((MyDevice.languageType == 0 ? "上限：" : "Upper limit:") + USL.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "下限：" : "Lower limit:") + LSL.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "平均值：" : "Average value:") + avg.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "方差：" : "Variance:") + V.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "标准差：" : "Standard deviation:") + σ.ToString());
            listBox2.Items.Add("CA：" + Ca.ToString());
            listBox2.Items.Add("CP：" + Cp.ToString());
            listBox2.Items.Add("Cpku：" + Cpku.ToString());
            listBox2.Items.Add("Cpkl：" + Cpkl.ToString());
            listBox2.Items.Add("Cpk：" + Cpk.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "最大值：" : "Maximum:") + max.ToString());
            listBox2.Items.Add((MyDevice.languageType == 0 ? "最小值：" : "Minimum:") + min.ToString());
        }

        /// <summary>
        /// CPK测试——保存文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX2_Click(object sender, EventArgs e)
        {
            string fileName = "";
            string myExcel = "";   //保存CPK数据
            SaveFileDialog pSaveFileDialog = new SaveFileDialog
            {
                Title = MyDevice.languageType == 0 ? "保存为:" : "Save as",
                RestoreDirectory = true,
                Filter = "*.xlsx|*.xlsx",
                DefaultExt = ".xlsx",
            };//同打开文件，也可指定任意类型的文件
            if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                myExcel = pSaveFileDialog.FileName;
            }
            else
            {
                return;
            }

            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("无法创建Excel对象，您的电脑可能未安装Excel");
                }
                else
                {
                    MessageBox.Show("Unable to create Excel object. Excel may not be installed on your computer");
                }
                return;
            }
            Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
            Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];//取得sheet1
                                                                                                                                  //写入标题
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
            }
            for (int r = 0; r < dataGridView1.Rows.Count; r++)
            {
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    worksheet.Cells[r + 2, i + 1] = dataGridView1.Rows[r].Cells[i].Value;
                }
                System.Windows.Forms.Application.DoEvents();
            }

            //写入CPK数据
            if (listBox2.Items.Count > 0)
            {
                List<string> list = new List<string>();
                foreach (string str in listBox2.Items)
                {
                    list.Add(str.Split('：')[0]);
                    list.Add(str.Split('：')[1]);
                }
                for (int i = 0, j = dataGridView1.Rows.Count, col = 0; i < list.Count;)
                {
                    if (col < 2)
                    {
                        worksheet.Cells[j + 2, col + 1] = list[i];
                        col++;
                        i++;
                    }
                    else
                    {
                        col = 0;
                        j++;
                    }
                }
            }

            worksheet.Columns.EntireColumn.AutoFit();//列宽自适应
            if (MyDevice.languageType == 0)
            {
                MessageBox.Show(fileName + "数据导出成功", "提示", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show(fileName + " Data exported successfully", "Information", MessageBoxButtons.OK);
            }
            if (myExcel != "")
            {
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(myExcel);
                }
                catch (Exception ex)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("导出文件时出错,文件可能正被打开！\n" + ex.Message);
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while exporting the file. The file may be being opened\n" + ex.Message);
                    }
                }
                xlApp.Quit();
                GC.Collect();//强行销毁
            }

        }

        /// <summary>
        /// CPK测试——清空数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "1";
            dataGridView1.Rows.Clear();
        }

        /// <summary>
        /// CPK测试——写入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX3_Click(object sender, EventArgs e)
        {
            int index;
            if (textBox2.Text == "")
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请先输入测试值选择");
                }
                else
                {
                    MessageBox.Show("Please enter the test value selection first");
                }
                return;
            }

            if (textBox1.Text == "")
            {
                index = this.dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = index + 1;
                dataGridView1.Rows[index].Cells[1].Value = textBox2.Text;
                dataGridView1.Rows[index].Cells[2].Value = signalOutput1.Text;
                dataGridView1.Rows[index].Cells[3].Value = signalUnit1.Text;
                dataGridView1.Rows[index].Cells[4].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            }
            else
            {
                if (Int32.TryParse(textBox1.Text, out index))
                {
                    if (index > dataGridView1.Rows.Count - 1)
                    {
                        dataGridView1.Rows.Add();
                    }
                    dataGridView1.Rows[index - 1].Cells[0].Value = index;
                    dataGridView1.Rows[index - 1].Cells[1].Value = textBox2.Text;
                    dataGridView1.Rows[index - 1].Cells[2].Value = signalOutput1.Text;
                    dataGridView1.Rows[index - 1].Cells[3].Value = signalUnit1.Text;
                    dataGridView1.Rows[index - 1].Cells[4].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                }
            }
            textBox1.Text = (dataGridView1.Rows.Count).ToString();
        }

        /// <summary>
        /// 记录点监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_Leave(object sender, EventArgs e)
        {
            int num;
            if (Int32.TryParse(textBox1.Text, out num))
            {
                if (num > dataGridView1.RowCount)
                {
                    textBox1.Text = (dataGridView1.RowCount + 1).ToString();
                }
            }
        }

        #endregion
    }
}
