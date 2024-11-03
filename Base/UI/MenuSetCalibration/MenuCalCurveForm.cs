using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//Junzhe 20230630

namespace Base.UI
{
    public partial class MenuCalCurveForm : Form
    {
        private XET actXET;//需要操作的设备
        private DrawPicture drawPicture;//绘图

        private double upperX;//x轴上限
        private double lowerX;//x轴下限
        private double upperY;//y轴上限
        private double lowerY;//y轴下限

        List<double> x = new List<double>();//标定点的x坐标
        List<double> y = new List<double>();//标定点的y坐标
        List<double> point = new List<double>();//直线拟合的坐标

        //
        public MenuCalCurveForm()
        {
            InitializeComponent();
        }

        //获取x轴上下限
        private void pictureBoxScope_setX()
        {
            double inputz;
            double inputf;

            //
            if (actXET.S_ElevenType)
            {
                inputz = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1);
                inputf = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11);
            }
            else
            {
                inputz = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1);
                inputf = MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5);
            }

            //
            if (inputz > inputf)
            {
                double temp = inputf;
                inputf = inputz;
                inputz = temp;
            }

            //
            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                case OUT.UTP05:
                case OUT.UTP10:
                case OUT.UMASK:
                    //
                    if (inputz < -2.0f)
                    {
                        lowerX = -3.0f;
                    }
                    else if (inputz < -1.0f)
                    {
                        lowerX = -2.0f;
                    }
                    else if (inputz < 0.0f)
                    {
                        lowerX = -1.0f;
                    }
                    else
                    {
                        lowerX = 0.0f;
                    }
                    //
                    if (inputf > 2.0f)
                    {
                        upperX = 3.0f;
                    }
                    else if (inputf > 1.0f)
                    {
                        upperX = 2.0f;
                    }
                    else
                    {
                        upperX = 1.0f;
                    }
                    break;

                case OUT.UTN05:
                case OUT.UTN10:
                    //求正数
                    if (inputz < 0)
                    {
                        inputz = -inputz;
                    }
                    if (inputf < 0)
                    {
                        inputf = -inputf;
                    }
                    //求大值
                    if (inputz > inputf)
                    {
                        upperX = inputz;
                    }
                    else
                    {
                        upperX = inputf;
                    }
                    //求大区间
                    if (upperX > 2.0f)
                    {
                        upperX = 3.0f;
                    }
                    else if (upperX > 1.0f)
                    {
                        upperX = 2.0f;
                    }
                    else
                    {
                        upperX = 1.0f;
                    }
                    //获取正负区间
                    lowerX = -upperX;
                    break;
            }
        }

        //获取y轴上下限
        private void pictureBoxScope_setY()
        {
            //
            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                    upperY = 20;
                    lowerY = 0;
                    break;

                case OUT.UTP05:
                    upperY = 5;
                    lowerY = 0;
                    break;

                case OUT.UTP10:
                    upperY = 10;
                    lowerY = 0;
                    break;

                case OUT.UTN05:
                    upperY = 5;
                    lowerY = -5;
                    break;

                case OUT.UTN10:
                    upperY = 10;
                    lowerY = -10;
                    break;

                case OUT.UMASK:
                    upperY = (int)(actXET.E_wt_full / Math.Pow(10, actXET.E_wt_decimal));
                    lowerY = 0;
                    break;
            }
        }
        
        //构建坐标,画点、网格、直线
        private void pictureBoxScope_draw()
        {
            drawPicture = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);

            //y轴单位
            drawPicture.AxisUnit = actXET.S_unit;

            //x轴上下限
            drawPicture.LimitUpperX = lowerX;
            drawPicture.LimitLowerX = upperX;

            //y轴上下限
            drawPicture.LimitUpperLeftY = upperY;
            drawPicture.LimitLowerLeftY = lowerY;

            //横轴,竖轴数量(网格)
            drawPicture.VerticalAxisNum = 21;
            drawPicture.HorizontalAxisNum = 11;

            //画x轴,y轴,网格
            pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();

            //获取标定点的坐标
            switch (actXET.E_curve)
            {
                case (Byte)ECVE.CTWOPT:
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5));
                    break;

                case (Byte)ECVE.CFITED:
                case (Byte)ECVE.CINTER:
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5));
                    break;

                case (Byte)ECVE.CELTED:
                case (Byte)ECVE.CELTER:
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input1));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input2));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog2));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input3));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog3));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input4));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog4));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input5));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input6));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog6));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input7));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog7));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input8));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog8));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input9));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog9));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input10));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog10));
                    x.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_input11));
                    y.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11));
                    break;

                default:
                    break;
            }

            //画点,画线
            switch (actXET.E_curve)
            {
                case (Byte)ECVE.CINTER:
                case (Byte)ECVE.CELTER:
                    pictureBox1.Image = drawPicture.GetForegroundImageTypeTwoInt(x, y);
                    break;

                case (Byte)ECVE.CTWOPT:
                case (Byte)ECVE.CFITED:
                    point.Add(actXET.E_ad_zero / actXET.S_MVDV);
                    point.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1));
                    point.Add(actXET.E_ad_full / actXET.S_MVDV);
                    point.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog5));
                    pictureBox1.Image = drawPicture.GetForegroundImageTypeTwoFit(x, y, point);
                    break;

                case (Byte)ECVE.CELTED:
                    point.Add(actXET.E_ad_zero / actXET.S_MVDV);
                    point.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog1));
                    point.Add(actXET.E_ad_full / actXET.S_MVDV);
                    point.Add(MyDevice.myUIT.ConvertInt32ToFloat(actXET.E_analog11));
                    pictureBox1.Image = drawPicture.GetForegroundImageTypeTwoFit(x, y, point);
                    break;

                default:
                    break;
            }
        }

        //加载界面
        private void MenuCalCurveForm_Load(object sender, EventArgs e)
        {
            actXET = MyDevice.actDev;
            pictureBoxScope_setX();
            pictureBoxScope_setY();
            pictureBoxScope_draw();
        }

        //拖动窗口自动最大化
        private void MenuCalCurveForm_ResizeEnd(object sender, EventArgs e)
        {
            //
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();
        }

        //窗口大小改变后初始化图像
        private void MenuCalCurveForm_SizeChanged(object sender, EventArgs e)
        {
            if (drawPicture == null)
            {
                return;
            }

            //初始化大小
            drawPicture.Height = pictureBox1.Height;
            drawPicture.Width = pictureBox1.Width;

            //画网格
            pictureBox1.BackgroundImage = drawPicture.GetBackgroundImage();

            //画点,画线
            if (actXET.E_curve == (Byte)ECVE.CINTER || actXET.E_curve == (Byte)ECVE.CELTER)
            {
                pictureBox1.Image = drawPicture.GetForegroundImageTypeTwoInt(x, y);
            }
            else
            {
                pictureBox1.Image = drawPicture.GetForegroundImageTypeTwoFit(x, y, point);
            }
        }
    }
}
