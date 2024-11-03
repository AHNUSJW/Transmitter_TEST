using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

//Lumi 20230630
//Lumi 20230704
//Lumi 20230707

namespace Base.UI.MenuSet
{
    public partial class MenuCalScopeForm : Form
    {
        private XET actXET;//需要操作的设备
        private DrawPicture drawPicture; //绘图
        private TASKS nextTask; //按键指令
        private System.Timers.Timer curveTimer;
        private int comTicker;  //发送指令计时器
        private int showTicker; //用于调整曲线更新速率
        private int basePointX; //原点横坐标
        Bitmap tmpCurve; //当前曲线

        //参数
        #region parameter

        //范围
        private Int32 grid;

        //坐标基准用
        private Int32 dax_zero;
        private Int32 dax_full;
        private Int32 aix_zero;
        private Int32 aix_full;

        //显示输出值用
        private float out_zero;
        private float out_full;
        private float out_curr;

        //通讯接收数据
        private Int32 dac_zero;
        private Int32 dac_full;
        private Int32 dac_curr;

        //显示波形用
        private Int32 pic_zero;
        private Int32 pic_full;
        private Int32 pic_curr;

        //
        private Boolean isPause;
        private Int32 maxSize;

        //振动参数
        private int sp_maxwt = 0;
        private int sp_minwt = 0xFFFFF;
        private List<int> receive = new List<int>();//接收到的数据缓存
        private List<int> dispaly = new List<int>();//接收到的显示缓存

        //当前显示和暂停显示
        private List<int> rxbuf = new List<int>();//接收到的数据缓存
        private string str_Span;
        private string str_Zero;
        private string str_Out;
        private string str_Max;
        private string str_Min;
        private string str_deci = "f3";

        #endregion

        //构造函数
        public MenuCalScopeForm()
        {
            InitializeComponent();
        }

        //加载界面
        private void MenuCalScopeForm_Load(object sender, EventArgs e)
        {
            //设置激活窗口
            Main.ActiveForm = "SetCalScope";

            MyDevice.myUpdate += new freshHandler(update_FromUart);

            actXET = MyDevice.actDev;

            //初始化坐标轴和网格
            update_Picture();

            //初始化Timer
            curveTimer = new System.Timers.Timer();
            curveTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            curveTimer.Interval = 100;
            curveTimer.AutoReset = true;
            curveTimer.Enabled = true;

            //启动数据读取
            start_dataMonitor();

            //初始化暂停键
            isPause = false;
            PauseToolStripMenuItem.Text = MyDevice.languageType == 0 ? "暂 停" : "Pause";
            PauseToolStripMenuItem.BackColor = Color.Firebrick;
        }

        //关闭界面
        private void MenuCalScopeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            curveTimer.Enabled = false;

            MyDevice.mePort_StopDacout();

            Main.ActiveForm = "SetCalibration";
        }

        //窗口最大化
        private void maximizeForm_move(object sender, EventArgs e)
        {
            //
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();

            //
            update_Picture();
        }

        //开始，暂停键
        private void PauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isPause)
            {
                isPause = false;
                PauseToolStripMenuItem.Text = MyDevice.languageType == 0 ? "暂 停" : "Pause";
                PauseToolStripMenuItem.BackColor = Color.Firebrick;
                labelLeftYNum.Visible = false;
            }
            else
            {
                isPause = true;
                PauseToolStripMenuItem.Text = MyDevice.languageType == 0 ? "开 始" : "Start";
                PauseToolStripMenuItem.BackColor = Color.Green;
                tmpCurve = drawPicture.GetForegroundImageTypeOne(actXET.R_datum);
            }
        }

        //鼠标点击曲线后获取当前坐标的值
        void pictureBoxScope_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isPause) return;

            pictureBoxScope.Image = tmpCurve;

            //点击处在x轴左侧
            if (e.X <= basePointX) return;

            int mouseX;
            int lineX;
            int labelX;
            int labelY;

            if (e.X < (rxbuf.Count + basePointX) && e.X > basePointX)
            {
                mouseX = e.X;
                lineX = e.X;
            }
            else
            {
                mouseX = rxbuf.Count + basePointX - 1;
                lineX = rxbuf.Count + basePointX + 3;
            }
            labelX = mouseX + 5;
            labelY = getPicValue(rxbuf[mouseX - basePointX]) + 5;

            //显示点击处的Y值
            labelLeftYNum.Visible = true;
            labelLeftYNum.Text = getOutValue(rxbuf[mouseX - basePointX]).ToString();
            labelLeftYNum.Location = new Point(labelX, labelY);

            //画竖线
            Bitmap img = new Bitmap(pictureBoxScope.Width, pictureBoxScope.Height);
            Graphics g = Graphics.FromImage(img);
            g.DrawLine(Pens.LightSteelBlue, new Point(lineX, 0), new Point(lineX, pictureBoxScope.Height));

            //合并图像
            pictureBoxScope.Image = UniteImage(pictureBoxScope.Width, pictureBoxScope.Height, img, tmpCurve);
        }

        //背景绘图
        private void update_Picture()
        {
            string YUnit;//y轴单位
            int upper;//y轴上限
            int lower;//y轴下限

            this.maxSize = pictureBoxScope.Width;

            switch (actXET.S_OutType)
            {
                case OUT.UT420:
                    //
                    grid = pictureBoxScope.Height / 21;
                    grid = grid << 1;
                    //
                    dax_zero = actXET.E_da_zero_4ma;
                    dax_full = actXET.E_da_full_20ma;
                    //
                    aix_zero = pictureBoxScope.Height - (grid * 2);
                    aix_full = aix_zero - (grid * 8);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11);
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5);
                        out_curr = out_zero;
                    }
                    break;

                case OUT.UTP05:
                    //
                    grid = pictureBoxScope.Height / 11;
                    //
                    dax_zero = actXET.E_da_zero_05V;
                    dax_full = actXET.E_da_full_05V;
                    //
                    aix_zero = pictureBoxScope.Height - (grid / 2);
                    aix_full = aix_zero - (grid * 10);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11);
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5);
                        out_curr = out_zero;
                    }
                    break;

                case OUT.UTP10:
                    //
                    grid = pictureBoxScope.Height / 11;
                    //
                    dax_zero = actXET.E_da_zero_10V;
                    dax_full = actXET.E_da_full_10V;
                    //
                    aix_zero = pictureBoxScope.Height - (grid / 2);
                    aix_full = aix_zero - (grid * 10);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11);
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5);
                        out_curr = out_zero;
                    }
                    break;

                case OUT.UTN05:
                    //
                    grid = pictureBoxScope.Height / 11;
                    //
                    dax_zero = (actXET.E_da_zero_N5 + actXET.E_da_full_P5) / 2;
                    dax_full = actXET.E_da_full_P5;
                    //
                    aix_zero = pictureBoxScope.Height / 2;
                    aix_full = aix_zero - (grid * 5);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11);
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5);
                        out_curr = out_zero;
                    }
                    break;

                case OUT.UTN10:
                    //
                    grid = pictureBoxScope.Height / 21;
                    grid = grid << 1;
                    //
                    dax_zero = (actXET.E_da_zero_N10 + actXET.E_da_full_P10) / 2;
                    dax_full = actXET.E_da_full_P10;
                    //
                    aix_zero = pictureBoxScope.Height / 2;
                    aix_full = aix_zero - (grid * 5);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11);
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1);
                        out_full = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5);
                        out_curr = out_zero;
                    }
                    break;

                case OUT.UMASK:
                    //
                    grid = pictureBoxScope.Height / 11;
                    //
                    dax_zero = actXET.E_wt_zero;
                    dax_full = actXET.E_wt_full;
                    //
                    aix_zero = pictureBoxScope.Height - (grid / 2);
                    aix_full = aix_zero - (grid * 10);
                    //
                    if (actXET.S_ElevenType)
                    {
                        out_zero = actXET.E_da_point1 / 100;
                        out_full = actXET.E_da_point11 / 100;
                        out_curr = out_zero;
                    }
                    else
                    {
                        out_zero = actXET.E_da_point1 / 1000;
                        out_full = actXET.E_da_point5 / 1000;
                        out_curr = out_zero;
                    }
                    break;
            }

            dac_zero = actXET.E_da_zero;
            dac_full = actXET.E_da_full;
            dac_curr = dac_zero;

            pic_zero = getPicValue(actXET.E_da_zero);
            pic_full = getPicValue(actXET.E_da_full);
            pic_curr = pic_zero;

            drawPicture = new DrawPicture(pictureBoxScope.Height, pictureBoxScope.Width, BackgroundImageType.OnlyXYAxisAndGrid);

            basePointX = drawPicture.TextInfo;

            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                    upper = 20;
                    lower = 0;
                    YUnit = "mA";
                    break;

                case OUT.UTP05:
                    upper = 5;
                    lower = 0;
                    YUnit = "V";
                    break;

                case OUT.UTP10:
                    upper = 10;
                    lower = 0;
                    YUnit = "V";
                    break;

                case OUT.UTN05:
                    upper = 5;
                    lower = -5;
                    YUnit = "V";
                    break;

                case OUT.UTN10:
                    upper = 10;
                    lower = -10;
                    YUnit = "V";
                    break;

                case OUT.UMASK:
                    upper = (int)(actXET.E_wt_full / Math.Pow(10, actXET.E_wt_decimal));
                    lower = 0;
                    YUnit = actXET.S_unit;
                    break;
            }

            drawPicture.LimitUpperLeftY = upper;
            drawPicture.LimitLowerLeftY = lower;
            drawPicture.HorizontalAxisNum = 11;
            drawPicture.UnitLeftY = YUnit;
            pictureBoxScope.BackgroundImage = drawPicture.GetBackgroundImage();
        }

        //求参和曲线绘图
        private void update_Curve()
        {
            if (isPause) return;
            if (pictureBoxScope.Height == 0) return;

            showTicker = 0;

            //取数
            int iw = MyDevice.protocol.rxData;

            //计算
            pic_curr = getPicValue(iw);
            out_curr = getOutValue(iw);

            //重量入栈
            receive.Add(iw);
            dispaly.Add(pic_curr);

            //极限
            if (dispaly.Count > this.maxSize)
            {
                receive.RemoveAt(0);
                dispaly.RemoveAt(0);
            }
            if (receive.Count < 1) return;
            if (dispaly.Count < 1) return;

            rxbuf.Clear();

            //描点参数
            int count = dispaly.Count - 1;
            int start = count - pictureBoxScope.Width + grid / 2;

            if (start < 2)
            {
                start = 2;
            }

            for (int i = start, posx = 0; i < count; i++)
            {
                rxbuf.Add(receive[i]);
                //
                if (receive[i] > sp_maxwt)
                {
                    sp_maxwt = receive[i];
                }
                if (receive[i] < sp_minwt)
                {
                    sp_minwt = receive[i];
                }
                //
                posx++;
            }

            //
            str_Span = "Span = " + out_full.ToString();
            str_Zero = "Zero = " + out_zero.ToString();

            //
            switch (actXET.S_OutType)
            {
                case OUT.UMASK:
                    str_Out = "Out = " + actXET.R_weight;
                    break;

                default:
                    str_Out = "Out = " + out_curr.ToString(str_deci);
                    break;
            }

            //
            str_Max = "Max = " + getOutValue(sp_maxwt).ToString();
            str_Min = "Min = " + getOutValue(sp_minwt).ToString();

            //显示参数
            label1.Text = str_Span;
            label2.Text = str_Zero;
            label3.Text = str_Out;
            label4.Text = str_Max;
            label5.Text = str_Min;

            pictureBoxScope.Image = drawPicture.GetForegroundImageTypeOne(actXET.R_datum);
        }

        //处理窗口消息
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)
            {
                switch ((int)m.WParam)
                {
                    //禁止双击标题栏关闭窗体
                    //case 0xF063:
                    //case 0xF093:
                    //    m.WParam = IntPtr.Zero;
                    //    break;
                    //禁止拖拽标题栏还原窗体
                    //case 0xF012:
                    //case 0xF010:
                    //    m.WParam = IntPtr.Zero;
                    //    break;
                    //禁止双击标题栏
                    case 0xf122:
                        m.WParam = IntPtr.Zero;
                        break;
                        //禁止关闭按钮
                        //case 0xF060:
                        //    m.WParam = IntPtr.Zero;
                        //    break;
                        //禁止最大化按钮
                        //case 0xf020:
                        //    m.WParam = IntPtr.Zero;
                        //    break;
                        //禁止最小化按钮
                        //case 0xf030:
                        //    m.WParam = IntPtr.Zero;
                        //    break;
                        //禁止还原按钮
                        //case 0xf120:
                        //    m.WParam = IntPtr.Zero;
                        //    break;
                }
            }
            base.WndProc(ref m);
        }

        //串口通信响应
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
                if (showTicker > 1)
                {
                    comTicker = 0;

                    update_Curve();
                }
            }
        }

        //timer的回调方法
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //控制显示速度
            showTicker++;

            switch (nextTask)
            {
                //监控任务
                case TASKS.DAC:
                    if ((++comTicker) > 5)
                    {
                        comTicker = 0;
                        MyDevice.mePort_SendCOM(TASKS.DAC);
                    }
                    break;
                default:
                    break;
            }
        }

        //启动数据读取
        private void start_dataMonitor()
        {
            comTicker = 0;
            nextTask = TASKS.DAC;
            MyDevice.mePort_ClearState();//清除数据确保R_eeplink不会02或03误设false
        }

        //
        private float getOutValue(Int32 dac)
        {
            float nfen = (float)(dac - dac_zero);
            float mfen = (float)(dac_full - dac_zero);
            return (((Int64)(out_full - out_zero) * nfen / mfen) + out_zero);
        }

        //
        private Int32 getPicValue(Int32 dac)
        {
            return (Int32)(((Int64)(dac - dax_zero) * (Int64)(aix_full - aix_zero) / (dax_full - dax_zero)) + aix_zero);
        }

        //合并图像
        public Image UniteImage(int width, int height, Image img1, Image img2)
        {
            Image img = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(img);

            g.Clear(Color.Transparent);

            g.DrawImage(img1, 0, 0, img1.Width, img1.Height);
            g.DrawImage(img2, 0, 0, img2.Width, img2.Height);

            return img;
        }
    }
}
