using Base.UI.MyControl;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//Junzhe 20230616
//Lumi 20231226

//UI界面主要任务

//界面:根据连接设备数量增加自定义控件MultiDevice，以展示多设备信息列表

//定时:当前窗口的定时任务处理,比如发送净毛重指令,监控状态
//通讯:串口事件解码后的委托处理

//存在MyDevice.devSum和mDevices判断调整不一致的问题
//前者为STATE.WORKING和STATE.CONNECTED时加入
//后者为STATE.WORKING时加入
//暂使用myDevSum解决

namespace Base.UI
{
    public partial class MultipleDevice : Form
    {
        private XET actXET;            //需要操作的设备
        private int comTicker;         //发送指令计时器
        private int showTicker = 0;    //控制显示速度
        private volatile int addrIndex;//已连接设备的地址指针
        private List<Byte> mutiAddres = new List<Byte>();             //存储已连接设备的地址
        private List<MutiDevice> mutiDevices = new List<MutiDevice>();//多设备列表
        private int myDevSum = 0;      //设备数量

        private TASKS nextTask;                                       //按键指令,TASKS.ZERO,TASKS.TARE,TASKS.BOR

        private DateTime stopTime;          //结束时间
        private Boolean isWrite;            //写入数据
        private int count;                  //记录数据列表数据个数
        private int enRecord;               //自动保存记录使能
        private System.Timers.Timer enTimer;//定时使能自动记录数据
        private string enPath = MyDevice.D_datPath + @"\enAutoRecord.txt";  //设置使能文件路径
        private int timeInterval;           //记录时间间隔
        private DateTime lastRecordTime;    //上一次记录数据的时间

        private Dictionary<int, List<double>> dataDic = new Dictionary<int, List<double>>();      //数据集合，用于绘图
        private Dictionary<int, Color> deviceColors = new Dictionary<int, Color>();
        private Random random = new Random();
        private bool showCurve = false;   //是否显示曲线

        //构造函数
        public MultipleDevice()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultipleDevice_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);
            actXET = MyDevice.actDev;

            //按键可见性
            switch (MyDevice.protocol.type)
            {
                default:
                    buttonX1.Visible = true;
                    break;

                case COMP.CANopen:
                    buttonX1.Visible = false;
                    break;
            }

            if ((MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            {
                buttonX1.Enabled = false;
            }
            else
            {
                buttonX1.Enabled = true;
            }

            this.splitContainer1.SplitterDistance = this.splitContainer1.Width;
            //将已连接设备的地址存入列表
            XET[] mDevices;
            switch (MyDevice.protocol.type)
            {
                default:
                case COMP.RS485:
                    mDevices = MyDevice.mBUS;
                    break;

                case COMP.CANopen:
                    mDevices = MyDevice.mCAN;
                    break;

                case COMP.ModbusTCP:
                    mDevices = MyDevice.mMTCP;
                    if (mDevices[0].sTATE == STATE.WORKING)
                    {
                        //modbus tcp 下标0也可存设备
                        mutiAddres.Add(0);
                    }
                    break;
            }
            for (int i = 1; i < mDevices.Length; i++)
            {
                if (mDevices[i].sTATE == STATE.WORKING)
                {
                    Byte mutiAddr = new Byte();

                    mutiAddr = (byte)i;
                    mutiAddres.Add(mutiAddr);
                }
            }

            myDevSum = MyDevice.devSum > mutiAddres.Count ? mutiAddres.Count : MyDevice.devSum;

            //清空多设备列表
            mutiDevices.Clear();
            tableLayoutPanel1.Controls.Clear();

            //根据连接设备数增加MutiDevice控件
            if (MyDevice.protocol.type != COMP.ModbusTCP)
            {
                for (int n = 1, j = 0; n <= myDevSum; n++)
                {
                    MyDevice.protocol.addr = mutiAddres[n - 1];
                    MutiDevice mutiDevice = new MutiDevice();
                    mutiDevice.SetZero += new EventHandler(SetZero);
                    mutiDevice.SetTare += new EventHandler(SetTare);
                    mutiDevices.Add(mutiDevice);
                    tableLayoutPanel1.Controls.Add(mutiDevice, (n - 1) % 4, j);
                    mutiDevice.Dock = DockStyle.Fill;

                    mutiDevice.Address = mutiAddres[n - 1].ToString();

                    //4个为一行
                    if (n % 4 == 0)
                    {
                        j++;
                    }
                }
                MyDevice.protocol.addr = mutiAddres[0];
            }
            else
            {
                //modbus TCP的下标不是e_addr
                for (int n = 1, j = 0; n <= MyDevice.devSum; n++)
                {
                    MutiDevice mutiDevice = new MutiDevice();
                    mutiDevice.SetZero += new EventHandler(SetZero);
                    mutiDevice.SetTare += new EventHandler(SetTare);
                    mutiDevices.Add(mutiDevice);
                    tableLayoutPanel1.Controls.Add(mutiDevice, (n - 1) % 4, j);
                    mutiDevice.Dock = DockStyle.Fill;

                    mutiDevice.Address = MyDevice.mMTCP[mutiAddres[n - 1]].E_addr.ToString();
                    mutiDevice.IPAddress = MyDevice.mMTCP[mutiAddres[n - 1]].R_ipAddr.ToString();

                    //4个为一行
                    if (n % 4 == 0)
                    {
                        j++;
                    }
                }
                MyDevice.protocol.addr = byte.Parse(mutiDevices[0].Address);
                MyDevice.protocol.ipAddr = mutiDevices[0].IPAddress;
            }

            actXET = MyDevice.actDev;

            //根据不同波特率有不同发送间隔
            switch (MyDevice.protocol.type)
            {
                default:
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

                case COMP.CANopen:
                case COMP.ModbusTCP:
                    timer1.Interval = 100;
                    break;
            }

            //启动数据读取
            start_dataMonitor();
            timer1.Enabled = true;

            //初始化数据列表显示
            update_ListView();

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
                stopTime = DateTime.Now.AddDays(1);
                enTimer = new System.Timers.Timer();
                enTimer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                enTimer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                enTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

                label1_dataTip.Visible = true;

                enTimeInterval.Visible = true;
            }
            else
            {
                label1_dataTip.Visible = false;

                enTimeInterval.Visible = false;
            }
            enTimeInterval_Click(null, null);
            timeInterval = int.Parse(textBoxEx_timeInterval.Text);
            lastRecordTime = DateTime.Now;  //确保第一条数据被记录
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultipleDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            MyDevice.mePort_StopDacout();

            timer1.Enabled = false;

            //单设备使能自动保存
            if (enAutoRecord.Checked == true)
            {
                stopTime = DateTime.Now;
            }
        }

        //归零
        private void SetZero(object sender, EventArgs e)
        {
            nextTask = TASKS.ZERO;
        }

        //扣重
        private void SetTare(object sender, EventArgs e)
        {
            nextTask = TASKS.TARE;
        }

        //广播指令,全部归零
        private void buttonX1_Click(object sender, EventArgs e)
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
                nextTask = TASKS.AZERO;
            }
        }

        //更新显示参数
        private void update_OutText()
        {
            actXET = MyDevice.actDev;

            if (count % 10000 == 0)
            {
                dbListView1.Items.Clear();
            }

            //更新litView
            ListViewItem item = new ListViewItem();
            item.SubItems[0].Text = (count++).ToString();
            if (actXET.S_DeviceType == TYPE.TCAN)
            {
                item.SubItems.Add(actXET.E_nodeID.ToString());
            }
            else
            {
                item.SubItems.Add(actXET.E_addr.ToString());
            }
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss:fff"));
            string status = "";    //稳定状态

            //数值 & 超载
            if (actXET.R_overload)
            {
                mutiDevices[addrIndex].Data = "---OL---";
                mutiDevices[addrIndex].DataColor = Color.Red;

                //表格添加数据
                if (actXET.R_grossnet == "毛重")
                {
                    item.SubItems.Add("OG");
                }
                else
                {
                    item.SubItems.Add("ON");
                }
                item.SubItems.Add("---OL---");
            }
            else if (mutiDevices[addrIndex].Outype == "毛重" || mutiDevices[addrIndex].Outype == "净重" || mutiDevices[addrIndex].Outype == "预紧力")
            {
                if (double.TryParse(actXET.R_weight, out double res))
                {
                    update_dataDic(addrIndex, res);
                }
                else
                {
                    return;
                }
                mutiDevices[addrIndex].Data = actXET.R_weight;
                mutiDevices[addrIndex].DataColor = Color.Black;

                //稳定状态写入
                if (actXET.R_stable) status = "S";
                else status = "U";

                if (actXET.R_grossnet == "毛重") status += "G";
                else status += "N";

                //表格添加数据
                item.SubItems.Add(status);
                item.SubItems.Add(mutiDevices[addrIndex].Data.PadLeft(9, ' '));
            }
            else if (mutiDevices[addrIndex].Outype == "模拟量" || mutiDevices[addrIndex].Outype == "重量")
            {
                if (double.TryParse(actXET.R_output, out double res))
                {
                    update_dataDic(addrIndex, res);
                }
                else
                {
                    return;
                }
                mutiDevices[addrIndex].Data = actXET.R_output;
                mutiDevices[addrIndex].DataColor = Color.Black;

                //表格添加数据
                item.SubItems.Add(status);
                item.SubItems.Add(mutiDevices[addrIndex].Data.PadLeft(9, ' '));
            }

            //稳定状态
            if (actXET.R_stable)
            {
                mutiDevices[addrIndex].LampColor = new Color[] { Color.Green };
            }
            else
            {
                mutiDevices[addrIndex].LampColor = new Color[] { Color.Black };
            }

            //更新数据加总
            Dictionary<string, double> WeightSums = new Dictionary<string, double>();
            foreach (MutiDevice dev in mutiDevices)
            {
                if (double.TryParse(dev.Data, out double weight))
                {
                    if (!WeightSums.ContainsKey(dev.Unit))
                    {
                        WeightSums[dev.Unit] = 0;
                    }
                    WeightSums[dev.Unit] += weight;
                }
            }
            if (WeightSums.Count > 0)
            {
                StringBuilder sumText = new StringBuilder("数据加总: ");
                for (int i = 0; i < WeightSums.Count; i++)
                {
                    var unitSum = WeightSums.ElementAt(i);
                    sumText.Append(unitSum.Value.ToString() + unitSum.Key);
                    if (i < WeightSums.Count - 1)
                    {
                        sumText.Append(", ");
                    }
                }
                label2_dataSum.Text = sumText.ToString();
            }
            else
            {
                label2_dataSum.Text = "";
            }

            //表格添加数据单位
            item.SubItems.Add(mutiDevices[addrIndex].Unit);

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
            if (enTimeInterval.Checked)
            {
                TimeSpan waitTime = DateTime.Now - lastRecordTime;
                if (waitTime.TotalMilliseconds > (timeInterval * 1000))
                {
                    dbListView1.Items.Add(item);
                    dbListView1.Items[dbListView1.Items.Count - 1].EnsureVisible();
                    lastRecordTime = DateTime.Now;
                    isWrite = true;//更新导入文件状态
                }
            }
            else
            {
                dbListView1.Items.Add(item);
                dbListView1.Items[dbListView1.Items.Count - 1].EnsureVisible();
                isWrite = true;//更新导入文件状态
            }

            //画曲线
            if (showCurve)
            {
                update_Chart();
            }
        }

        //更新TableLayoutPanel中的控件布局
        private void ReArrangeControlsInTableLayoutPanel(int columns)
        {
            // 设置列数
            this.tableLayoutPanel1.ColumnCount = columns;

            // 获取TableLayoutPanel中的所有控件
            var controls = this.tableLayoutPanel1.Controls.Cast<Control>().ToList();

            // 清除TableLayoutPanel中的所有控件
            this.tableLayoutPanel1.Controls.Clear();

            // 将控件重新添加到TableLayoutPanel中
            for (int i = 0; i < controls.Count; i++)
            {
                // 计算行索引和列索引
                int rowIndex = i / columns;
                int columnIndex = i % columns;

                // 如果需要，添加新的行
                if (rowIndex >= this.tableLayoutPanel1.RowCount)
                {
                    this.tableLayoutPanel1.RowCount++;
                    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }

                // 将控件添加到适当的单元格中
                this.tableLayoutPanel1.Controls.Add(controls[i], columnIndex, rowIndex);
            }
        }

        #region 数据曲线

        //开启关闭数据曲线
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dataDic.Clear();
            deviceColors.Clear();
            if (checkBox1_dataCurve.Checked)
            {
                this.splitContainer1.SplitterDistance = this.splitContainer1.Width / 2;

                ReArrangeControlsInTableLayoutPanel(2);

                // 开启数据曲线
                showCurve = true;
                init_Chart();
            }
            else
            {
                //关闭数据曲线
                this.splitContainer1.SplitterDistance = this.splitContainer1.Width;

                showCurve = false;
                ReArrangeControlsInTableLayoutPanel(4);
            }
        }

        //初始化曲线图
        private void init_Chart()
        {
            // 隐藏X轴刻度
            this.chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            this.chart1.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
            // 隐藏X轴的主要网格线
            this.chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            // 显示Y轴的主要网格线
            this.chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            // 设置坐标轴的颜色为黑色
            this.chart1.ChartAreas[0].AxisX.LineColor = Color.Black;
            this.chart1.ChartAreas[0].AxisY.LineColor = Color.Black;
            // 设置Y轴主要网格线的颜色为灰色
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Gray;
            this.chart1.ChartAreas[0].AxisX.Minimum = 0;  // 设置X轴的最小值
        }

        //更新曲线
        private void update_Chart()
        {
            // 清除Chart的所有数据系列
            this.chart1.Series.Clear();

            // 遍历dataDic字典
            foreach (var kvp in dataDic)
            {
                // 创建一个新的数据系列
                var series = new Series
                {
                    Name = mutiDevices[kvp.Key].Address,
                    Color = deviceColors[kvp.Key],
                    BorderWidth = 2,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Spline,  //曲线图
                };

                // 添加数据点到数据系列
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    series.Points.AddXY(i, kvp.Value[i]);
                }

                // 添加数据系列到Chart
                this.chart1.Series.Add(series);
            }
        }

        //更新数据集合
        private void update_dataDic(int addrIndex, double data)
        {
            if (dataDic.ContainsKey(addrIndex))
            {
                // 如果Dictionary中包含这个键，向对应的List<double>中添加值
                dataDic[addrIndex].Add(data);
            }
            else
            {
                // 如果Dictionary中不包含这个键，添加一个新的键值对
                dataDic.Add(addrIndex, new List<double> { data });

                // 生成一个随机颜色
                Color color;
                bool isColorTooClose;

                do
                {
                    isColorTooClose = false;
                    color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));

                    foreach (var existingColor in deviceColors.Values)
                    {
                        if (ColorDifference(color, existingColor) < 30)
                        {
                            isColorTooClose = true;
                            break;
                        }
                    }
                }
                while (isColorTooClose);

                deviceColors.Add(addrIndex, color);
            }
        }

        // 计算两种颜色之间的差异
        private int ColorDifference(Color c1, Color c2)
        {
            return Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
        }

        #endregion

        #region 数据记录

        /// <summary>
        /// 初始化listView表格
        /// </summary>
        private void update_ListView()
        {
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
        }

        /// <summary>
        /// 数据列表——保存实时数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_Data(object sender, System.Timers.ElapsedEventArgs e)
        {
            int index;//实施记录数据下标

            //判断data目录是否存在
            if (!Directory.Exists(MyDevice.D_dataPath))
            {
                //不存在
                Directory.CreateDirectory(MyDevice.D_dataPath);
            }

            //文件路径
            string mePath;
            if (MyDevice.languageType == 0)
            {
                mePath = MyDevice.D_dataPath + @"\实时数据记录表_多设备_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
            }
            else
            {
                mePath = MyDevice.D_dataPath + @"\data_devices_" + DateTime.Now.ToString("yyMMddHHmmss") + ".csv";
            }

            FileInfo fileInfo = new FileInfo(mePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            //保存文件
            FileStream fs = new FileStream(mePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

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
            int col1 = 0; //实际导出到excel的序号
            while (DateTime.Compare(DateTime.Now, stopTime) < 0)
            {
                if (isWrite)
                {
                    col1++;
                    index = dbListView1.Items.Count - 1;
                    isWrite = false;
                    this.BeginInvoke(new System.Action(delegate
                    {
                        if (index > -1)
                        {
                            data = col1 + "," + dbListView1.Items[index].SubItems[1].Text + "," +
                                   dbListView1.Items[index].SubItems[2].Text + "," + dbListView1.Items[index].SubItems[3].Text + "," +
                                   dbListView1.Items[index].SubItems[4].Text + "," + dbListView1.Items[index].SubItems[5].Text;


                            if (actXET.S_DeviceType == TYPE.TCAN || actXET.S_DeviceType == TYPE.iNet)
                            {
                                data += "," + dbListView1.Items[index].SubItems[6].Text;
                            }
                            sw.WriteLine(data);
                        }
                    }));
                }
            }
            sw.Close();
            fs.Close();
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
                System.IO.File.SetAttributes(enPath, FileAttributes.Normal);
            }

            System.IO.File.WriteAllText(enPath, enRecord.ToString());
            System.IO.File.SetAttributes(enPath, FileAttributes.ReadOnly);//设置文件权限只读

            if (enAutoRecord.Checked == false)
            {
                stopTime = DateTime.Now;

                label1_dataTip.Visible = false;
                enTimeInterval.Visible = false;
            }
            else
            {
                dbListView1.Items.Clear();
                count = 1;

                stopTime = System.DateTime.Now.AddDays(1);
                enTimer = new System.Timers.Timer();
                enTimer.Elapsed += new System.Timers.ElapsedEventHandler(save_Data);//到达时间的时候执行事件；
                enTimer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                enTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

                label1_dataTip.Visible = true;
                enTimeInterval.Visible = true;
            }
            enTimeInterval_Click(null, null);
        }

        /// <summary>
        /// 数据列表——设置间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enTimeInterval_Click(object sender, EventArgs e)
        {
            if (enTimeInterval.Checked && enTimeInterval.Visible)
            {
                label_timeInterval.Visible = true;
                textBoxEx_timeInterval.Visible = true;
            }
            else
            {
                label_timeInterval.Visible = false;
                textBoxEx_timeInterval.Visible = false;
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

        #endregion

        //串口通讯响应
        public void update_FromUart()
        {
            //使用BeginInvoke方法完成委托的封送，类似于使用PostMessage方法给界面线程发送消息，为异步方法
            //在完成委托的封送后，BeginInvoke方法立即返回，不会等待委托的方法执行完毕，调用者线程将不会阻塞
            MethodInvoker myInvoker = new MethodInvoker(() =>
            {
                if (Main.ActiveForm.Contains("MultipleDevice"))
                {
                    switch (nextTask)
                    {
                        case TASKS.ZERO:
                        case TASKS.TARE:
                        case TASKS.AZERO:
                            TASKS task = nextTask;
                            nextTask = TASKS.NULL;
                            MyDevice.mePort_SendCOM(task);

                            comTicker = 0;
                            break;

                        default:
                            if (MyDevice.protocol.trTASK == TASKS.AZERO || MyDevice.protocol.trTASK == TASKS.TARE)
                            {
                                //启动数据读取
                                start_dataMonitor();
                            }
                            else
                            {
                                comTicker = 0;

                                //更新显示参数
                                switch (MyDevice.protocol.type)
                                {
                                    default:
                                    case COMP.RS485:
                                        update_OutText();

                                        //轮询发送问答指令
                                        addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                        MyDevice.protocol.addr = mutiAddres[addrIndex];

                                        switch (mutiDevices[addrIndex].Outype)
                                        {
                                            default:
                                                break;

                                            case "模拟量":
                                                MyDevice.mePort_SendCOM(TASKS.QDAC);
                                                break;

                                            case "毛重":
                                                MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                                break;

                                            case "净重":
                                            case "预紧力":
                                                MyDevice.mePort_SendCOM(TASKS.QNET);
                                                break;
                                        }
                                        break;

                                    case COMP.CANopen:
                                        showTicker++;
                                        if (showTicker > 1)     //CANopen发送速度过快会导致界面卡顿
                                        {
                                            update_OutText();

                                            showTicker = 0;
                                            //轮询发送问答指令
                                            addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            MyDevice.protocol.addr = mutiAddres[addrIndex];

                                            switch (mutiDevices[addrIndex].Outype)
                                            {
                                                default:
                                                    break;

                                                case "重量":
                                                    MyDevice.mePort_SendCOM(TASKS.DAC);
                                                    break;
                                            }
                                        }
                                        break;

                                    case COMP.ModbusTCP:
                                        update_OutText();

                                        //轮询发送问答指令
                                        addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                        if (!TCPServer.FindClientStateByIP(mutiDevices[addrIndex].IPAddress))
                                        {
                                            //不对已掉线的设备发指令
                                            addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                        }
                                        MyDevice.protocol.addr = byte.Parse(mutiDevices[addrIndex].Address);
                                        MyDevice.protocol.ipAddr = mutiDevices[addrIndex].IPAddress;

                                        switch (mutiDevices[addrIndex].Outype)
                                        {
                                            default:
                                                break;

                                            case "毛重":
                                                MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                                break;

                                            case "净重":
                                            case "预紧力":
                                                MyDevice.mePort_SendCOM(TASKS.QNET);
                                                break;
                                        }
                                        break;
                                }
                            }
                            break;

                    }
                }
            });
            this.BeginInvoke(myInvoker);
        }

        //超时监控
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MyDevice.protocol.Is_serial_listening) return;

            if (Main.ActiveForm.Contains("MultipleDevice"))
            {
                switch (nextTask)
                {
                    case TASKS.ZERO:
                    case TASKS.TARE:
                    case TASKS.AZERO:
                        if ((++comTicker) > 5)
                        {
                            comTicker = 0;
                            nextTask = TASKS.NULL;
                        }
                        break;
                    default:
                        switch (MyDevice.protocol.type)
                        {
                            default:
                            case COMP.RS485:
                                switch (mutiDevices[addrIndex].Outype)
                                {
                                    default:
                                        break;

                                    case "模拟量":
                                        //超时询问毛重
                                        if ((++comTicker) > 3)
                                        {
                                            addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                            MyDevice.protocol.addr = mutiAddres[addrIndex];

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.QDAC);
                                        }
                                        break;

                                    case "毛重":
                                        //超时询问毛重
                                        if ((++comTicker) > 3)
                                        {
                                            addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                            MyDevice.protocol.addr = mutiAddres[addrIndex];

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                        }
                                        break;

                                    case "净重":
                                    case "预紧力":
                                        //超时询问净重
                                        if ((++comTicker) > 3)
                                        {
                                            addrIndex = ++addrIndex >= myDevSum ? 0 : addrIndex;
                                            MyDevice.protocol.addr = mutiAddres[addrIndex];

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.QNET);
                                        }
                                        break;
                                }
                                break;

                            case COMP.CANopen:
                                switch (mutiDevices[addrIndex].Outype)
                                {
                                    default:
                                        break;

                                    case "重量":
                                        //超时询问重量
                                        if ((++comTicker) > 1)
                                        {
                                            addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            MyDevice.protocol.addr = mutiAddres[addrIndex];

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.DAC);
                                        }
                                        break;
                                }
                                break;

                            case COMP.ModbusTCP:
                                switch (mutiDevices[addrIndex].Outype)
                                {
                                    default:
                                        break;

                                    case "毛重":
                                        //超时询问毛重
                                        if ((++comTicker) > 3)
                                        {
                                            addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            if (!TCPServer.FindClientStateByIP(mutiDevices[addrIndex].IPAddress))
                                            {
                                                //不对已掉线的设备发指令
                                                addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            }
                                            MyDevice.protocol.addr = byte.Parse(mutiDevices[addrIndex].Address);
                                            MyDevice.protocol.ipAddr = mutiDevices[addrIndex].IPAddress;

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.QGROSS);
                                        }
                                        break;

                                    case "净重":
                                    case "预紧力":
                                        //超时询问净重
                                        if ((++comTicker) > 3)
                                        {
                                            addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            if (!TCPServer.FindClientStateByIP(mutiDevices[addrIndex].IPAddress))
                                            {
                                                //不对已掉线的设备发指令
                                                addrIndex = ++addrIndex >= MyDevice.devSum ? 0 : addrIndex;
                                            }
                                            MyDevice.protocol.addr = byte.Parse(mutiDevices[addrIndex].Address);
                                            MyDevice.protocol.ipAddr = mutiDevices[addrIndex].IPAddress;

                                            comTicker = 0;
                                            MyDevice.mePort_SendCOM(TASKS.QNET);
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
        }

        //启动数据读取
        private void start_dataMonitor()
        {
            comTicker = 0;
            MyDevice.protocol.trTASK = TASKS.QGROSS;
            MyDevice.mePort_ClearState();//清除数据确保R_eeplink不会02或03误设false
        }
    }
}