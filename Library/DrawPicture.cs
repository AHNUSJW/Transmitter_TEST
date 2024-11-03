using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

//Ziyun
//Lumi 20230630
//Junzhe 20230630
//Lumi 20230704
//Ricardo 20230710

namespace Library
{
    /// <summary>
    /// 轴模式
    /// </summary>
    public enum BackgroundImageType
    {
        [Description("仅有x轴")] OnlyXAxis,
        [Description("仅有xy轴")] OnlyXYAxis,
        [Description("有一个x轴两个y轴")] OneXTwoYAxis,
        [Description("仅有x轴和网格")] OnlyXAxisAndGrid,
        [Description("仅有xy轴和网格")] OnlyXYAxisAndGrid,
        [Description("有一个x轴两个y轴和网格")] OneXTwoYAxisAndGrid,
    }

    /// <summary>
    ///
    /// </summary>
    public class DrawPicture
    {
        private int height;//画图的高度
        private int width;//画图的宽度
        private int textInfo = 60;//数字显示宽度

        private BackgroundImageType imageType;//轴模式
        private int horizontalAxisNum;//横轴数量
        private int verticalAxisNum;//竖轴数量
        private int offsetX = 0;//X轴偏移量
        private int offsetY = 0;//Y轴偏移量
        private double limitUpperX;//x轴上限
        private double limitLowerX;//x轴下限
        private double limitUpperLeftY;//y轴左轴上限
        private double limitLowerLeftY;//y轴左轴下限
        private double limitUpperRightY;//y轴右轴上限
        private double limitLowerRightY;//y轴右轴下限

        private string axisUnit;//坐标轴单位
        private int lineNumbers;//画线数量
        private List<List<double>> data = new List<List<double>>();//存储数据
        private List<List<Point>> points = new List<List<Point>>();//存储数据点位坐标

        private string unitLeftY = "";//左侧数据单位

        //画线颜色
        private Color[] drawLines = new Color[20] {
            Color.AntiqueWhite,
            Color.Aqua,
            Color.Aquamarine,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.BurlyWood,
            Color.CadetBlue,
            Color.Chartreuse,
            Color.CornflowerBlue,
            Color.Crimson,
            Color.DarkBlue,
            Color.DarkBlue,
            Color.DarkCyan,
            Color.DarkSlateBlue,
            Color.DarkSlateGray,
            Color.Gold,
            Color.LightCoral,
            Color.SaddleBrown,
            Color.SlateGray,
        };
        //定义背景画笔
        private Pen mypen = new Pen(Color.Black, 1.0f);
        //定义背景格子画笔
        private Pen myGridPen = new Pen(Color.SlateGray, 1.0f);
        //定义文字体和文字大小
        private Font fontText = new Font("Arial", 12);
        //定义文字颜色
        private Brush brushAxis = Brushes.Silver;

        #region set and get

        public int Height
        {
            get => height;
            set
            {
                height = value;
                if (height < 200)
                {
                    height = 200;
                }
            }
        }
        public int Width
        {
            get => width;
            set
            {
                width = value;
                if (width < 200)
                {
                    width = 200;
                }
            }
        }
        public int TextInfo { get => textInfo; set => textInfo = value; }
        public BackgroundImageType ImageType { get => imageType; set => imageType = value; }
        public int HorizontalAxisNum { get => horizontalAxisNum; set => horizontalAxisNum = value; }
        public int VerticalAxisNum { get => verticalAxisNum; set => verticalAxisNum = value; }
        public int OffsetX { get => offsetX; set => offsetX = value; }
        public int OffsetY { get => offsetY; set => offsetY = value; }
        public double LimitUpperX { get => limitUpperX; set => limitUpperX = value; }
        public double LimitLowerX { get => limitLowerX; set => limitLowerX = value; }
        public double LimitUpperLeftY { get => limitUpperLeftY; set => limitUpperLeftY = value; }
        public double LimitLowerLeftY { get => limitLowerLeftY; set => limitLowerLeftY = value; }
        public double LimitUpperRightY { get => limitUpperRightY; set => limitUpperRightY = value; }
        public double LimitLowerRightY { get => limitLowerRightY; set => limitLowerRightY = value; }
        public string UnitLeftY { get => unitLeftY; set => unitLeftY = value; }
        public string AxisUnit { get => axisUnit; set => axisUnit = value; }

        public int LineNumbers
        {
            get => lineNumbers;
            set
            {
                lineNumbers = value;
                Data.Clear();
                for (int i = 0; i < lineNumbers; i++)
                {
                    Data.Add(new List<double>());
                }
            }
        }
        public List<List<double>> Data { get => data; set => data = value; }

        #endregion

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        public DrawPicture()
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        public DrawPicture(int height, int width)
            : this(height, width, BackgroundImageType.OnlyXAxis, 1)
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        /// <param name="imageType">轴类型选择</param>
        public DrawPicture(int height, int width, BackgroundImageType imageType)
            : this(height, width, imageType, 1)
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        /// <param name="imageType">轴类型选择</param>
        /// <param name="lineNumbers">画曲线数量</param>
        public DrawPicture(int height, int width, BackgroundImageType imageType, int lineNumbers)
        {
            Height = height;
            Width = width;
            ImageType = imageType;
            LineNumbers = lineNumbers;
        }

        /// <summary>
        /// 底层画图仅有x轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeOne(Graphics g)
        {
            //画横线
            if (HorizontalAxisNum > 1)
            {
                double intervalTemp = HorizontalAxisNum - 1;//间隔数量
                double intervalHeight = (Height - 2 * TextInfo) / intervalTemp;//间隔高度
                double intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;//间隔差值

                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - i * intervalHeight)), new Point(Width, (int)(Height - TextInfo - i * intervalHeight)));

                    //标y轴坐标值
                    if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                    {
                        g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 5)));
                    }
                }
            }
            else if (HorizontalAxisNum == 1)
            {
                g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo), new Point(Width, Height - TextInfo));

                //标y轴坐标值
                if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                {
                    g.DrawString(LimitLowerLeftY.ToString("F2"), fontText, brushAxis, new Point(0, Height - TextInfo));
                }
            }
        }

        /// <summary>
        /// 底层画图仅有xy轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeTwo(Graphics g)
        {
            //画横线
            if (HorizontalAxisNum > 1)
            {
                double intervalTemp = HorizontalAxisNum - 1;//间隔数量
                double intervalHeight = (Height - 2 * TextInfo) / intervalTemp;//间隔高度
                double intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;//间隔差值

                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - i * intervalHeight)), new Point(Width, (int)(Height - TextInfo - i * intervalHeight)));

                    //标y轴坐标值
                    if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                    {
                        if (i == HorizontalAxisNum - 1)
                        {
                            g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' ') + AxisUnit.ToString(), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 10)));
                        }
                        else
                        {
                            g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 10)));
                        }
                    }
                }
            }
            else if (HorizontalAxisNum == 1)
            {
                g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo), new Point(Width, Height - TextInfo));

                //标y轴坐标值
                if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                {
                    g.DrawString(LimitLowerLeftY.ToString("F2").PadLeft(6, ' ') + AxisUnit.ToString(), fontText, brushAxis, new Point(0, Height - TextInfo));
                }
            }

            //画竖线
            if (VerticalAxisNum > 1)
            {
                int intervalWidth = (Width - 2 * TextInfo) / (VerticalAxisNum - 1);//间隔高度
                double intervalNumber = (limitUpperX - limitLowerX) / (VerticalAxisNum - 1);//间隔差值

                for (int i = 0; i < VerticalAxisNum; i++)
                {
                    g.DrawLine(mypen, new Point(Width - TextInfo - i * intervalWidth, TextInfo), new Point(Width - TextInfo - i * intervalWidth, Height - TextInfo));

                    //标x轴坐标值
                    if (limitUpperX != -1 || limitLowerX != -1)
                    {
                        if (i == 0)
                        {
                            g.DrawString((limitLowerX + i * intervalNumber).ToString("F2").PadLeft(6, ' ') + "mV/V", fontText, brushAxis, new Point(Width - TextInfo - i * intervalWidth - 25, Height - TextInfo));
                        }
                        else
                        {
                            g.DrawString((limitLowerX + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(Width - TextInfo - i * intervalWidth - 25, Height - TextInfo));
                        }
                    }
                }
            }
            else if (VerticalAxisNum == 1)
            {
                g.DrawLine(mypen, new Point(Width - TextInfo, TextInfo), new Point(Width - TextInfo, Height - TextInfo));

                //标x轴坐标值
                if (limitUpperX != -1 || limitLowerX != -1)
                {
                    g.DrawString(limitLowerX.ToString("F2").PadLeft(6, ' ') + "mV/V", fontText, brushAxis, new Point(Width - TextInfo, Height - TextInfo));
                }
            }
        }

        /// <summary>
        /// 底层画图有两个y轴一个x轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeThree(Graphics g)
        {
            //画x轴
            g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo), new Point(Width - 2 * TextInfo, Height - TextInfo));
            //画y轴
            g.DrawLine(mypen, new Point(TextInfo, TextInfo), new Point(TextInfo, Height - TextInfo));
            g.DrawLine(mypen, new Point(Width - TextInfo, TextInfo), new Point(Width - TextInfo, Height - TextInfo));

            //标y轴坐标值
            if (limitUpperLeftY != -1 || limitLowerLeftY != -1)
            {
                double intervalTemp = HorizontalAxisNum - 1;//间隔数量
                double intervalHeight = (Height - 2 * TextInfo - OffsetY) / intervalTemp;//间隔高度
                double intervalNumberLeft = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;//间隔差值
                double intervalNumberRight = (LimitUpperRightY - LimitUpperRightY) / intervalTemp;//间隔差值

                //画y轴坐标值和标识
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - i * intervalHeight)), new Point(Width - TextInfo, (int)(Height - TextInfo - i * intervalHeight)));//画背景网格
                    g.DrawString((LimitLowerLeftY + i * intervalNumberLeft).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 5)));//标y轴坐标值
                    g.DrawString((LimitLowerRightY + i * intervalNumberRight).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(Width - TextInfo, (int)(Height - TextInfo - i * intervalHeight - 5)));//标y轴坐标值
                }
            }
        }

        /// <summary>
        /// 底层画图仅有x轴和网格线
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeFour(Graphics g)
        {
            double intervalHeight = 0;//间隔高度
            double intervalNumber = 0;//间隔差值
            double intervalGrid = 0;//网格间距
            double intervalTemp = HorizontalAxisNum - 1;//间隔数量
            int HorizontalAxisNum2 = HorizontalAxisNum;//用于画网格

            //计算格子间距，避免网格过密
            if (HorizontalAxisNum > 1 && HorizontalAxisNum < 7)
            {
                intervalHeight = (Height - 2 * TextInfo) / intervalTemp;
                intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                intervalGrid = intervalHeight / 2;
            }
            else if (HorizontalAxisNum == 1)
            {
                intervalHeight = (Height - 2 * TextInfo) / 5d;
                intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / 5d;
                intervalGrid = intervalHeight / 2;
                HorizontalAxisNum2 = 6;
            }
            else if (HorizontalAxisNum >= 7)
            {
                intervalHeight = (Height - 2 * TextInfo) / intervalTemp;
                intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                intervalGrid = intervalHeight;
            }

            double tempX = TextInfo;//网格中竖线的x值
            double tempY = Height - TextInfo + intervalGrid;//网格中横线的y值

            //画网格
            //横线
            if (HorizontalAxisNum > 1)
            {
                while (tempY > TextInfo + intervalHeight)
                {
                    tempY -= intervalHeight;
                    g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width, (int)tempY));
                }

            }
            else if (HorizontalAxisNum == 1)
            {
                while (tempY > TextInfo + intervalGrid)
                {
                    tempY -= intervalGrid;
                    g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width, (int)tempY));
                }
            }

            g.DrawLine(myGridPen, new Point(TextInfo, (int)(Height - TextInfo - HorizontalAxisNum2 * intervalHeight + intervalHeight)), new Point(Width, (int)(Height - TextInfo - HorizontalAxisNum2 * intervalHeight + intervalHeight)));

            //竖线
            while (tempX < Width - intervalGrid)
            {
                tempX += intervalGrid;
                g.DrawLine(myGridPen, new Point((int)tempX, Height - TextInfo), new Point((int)tempX, (int)(Height - TextInfo - HorizontalAxisNum2 * intervalHeight + intervalHeight)));
            }
            g.DrawLine(myGridPen, new Point(TextInfo, Height - TextInfo), new Point(TextInfo, (int)(Height - TextInfo - HorizontalAxisNum2 * intervalHeight + intervalHeight)));
            g.DrawLine(myGridPen, new Point(Width - 1, Height - TextInfo), new Point(Width - 1, (int)(Height - TextInfo - HorizontalAxisNum2 * intervalHeight + intervalHeight)));

            //画x轴
            //画横线
            if (HorizontalAxisNum > 1)
            {
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - i * intervalHeight)), new Point(Width, (int)(Height - TextInfo - i * intervalHeight)));

                    //标y轴坐标值
                    if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                    {
                        if (i == HorizontalAxisNum - 1)
                        {
                            g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' ') + AxisUnit.ToString(), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 10)));
                        }
                        else
                        {
                            g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 10)));
                        }
                    }
                }
            }
            else if (HorizontalAxisNum == 1)
            {
                g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo), new Point(Width, Height - TextInfo));

                //标y轴坐标值
                if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                {
                    g.DrawString(LimitLowerLeftY.ToString("F2").PadLeft(6, ' ') + AxisUnit.ToString(), fontText, brushAxis, new Point(0, Height - TextInfo));
                }
            }
        }

        /// <summary>
        /// 底层画图有xy轴和网格线
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeFive(Graphics g)
        {
            //标y轴坐标值
            if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
            {
                double intervalHeight = 0;//间隔高度
                double intervalNumber = 0;//间隔差值
                double intervalGrid = 0;//网格间距
                double intervalTemp = HorizontalAxisNum - 1;//间隔数量
                int stringX;//坐标值x轴位置
                bool isString = false;//是否与数值同一y值

                //计算格子间距，避免网格过密
                if (HorizontalAxisNum > 1 && HorizontalAxisNum < 7)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / intervalTemp;
                    intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                    intervalGrid = intervalHeight / 2;
                }
                else if (HorizontalAxisNum == 1)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / 5d;
                    intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / 5d;
                    intervalGrid = intervalHeight / 2;
                }
                else if (HorizontalAxisNum >= 7)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / intervalTemp;
                    intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                    intervalGrid = intervalHeight;
                }

                double tempX = TextInfo;//网格中竖线的x值
                double tempY = Height - TextInfo - OffsetY;//网格中横线的y值

                //画网格
                //横线
                while (tempY >= TextInfo + intervalGrid)
                {
                    if (HorizontalAxisNum > 1 && HorizontalAxisNum < 7)
                    {
                        if (isString)
                        {
                            tempY = tempY + intervalGrid - intervalHeight;
                            isString = false;
                        }
                        else
                        {
                            tempY -= intervalGrid;
                            isString = true;
                        }
                        g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width - TextInfo, (int)tempY));
                    }
                    else
                    {
                        tempY -= intervalGrid;
                        g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width - TextInfo, (int)tempY));
                    }
                }
                //竖线
                while (tempX < Width - TextInfo - intervalGrid)
                {
                    tempX += intervalGrid;
                    g.DrawLine(myGridPen, new Point((int)tempX, (int)tempY), new Point((int)tempX, Height - TextInfo));
                }
                g.DrawLine(myGridPen, new Point(Width - TextInfo, (int)tempY), new Point(Width - TextInfo, Height - TextInfo));

                //画x轴
                if (limitLowerLeftY < 0)
                {
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - OffsetY - (HorizontalAxisNum - 1) / 2 * intervalHeight)), new Point(Width - TextInfo, (int)(Height - TextInfo - OffsetY - (HorizontalAxisNum - 1) / 2 * intervalHeight)));
                    g.DrawLine(myGridPen, new Point(TextInfo, Height - TextInfo - OffsetY), new Point(Width - TextInfo, Height - TextInfo - OffsetY));
                }
                else
                {
                    g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo - OffsetY), new Point(Width - TextInfo, Height - TextInfo - OffsetY));
                }
                //画y轴
                g.DrawLine(mypen, new Point(TextInfo + OffsetX, (int)tempY), new Point(TextInfo + OffsetX, Height - TextInfo));

                //计算坐标值x轴的位置
                if (Width - 2 * TextInfo < 2 * OffsetX)
                {
                    stringX = TextInfo + OffsetX + 4;
                }
                else
                {
                    stringX = OffsetY;
                }

                //画y轴坐标值和标识
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new Point(TextInfo + OffsetX, (int)(Height - TextInfo - OffsetY - i * intervalHeight)), new Point(TextInfo + OffsetX, (int)(Height - TextInfo - OffsetY - i * intervalHeight)));
                    g.DrawString(((LimitLowerLeftY + i * intervalNumber).ToString("F1") + UnitLeftY).PadLeft(8, ' '), fontText, brushAxis, new Point(stringX - 5, (int)(Height - TextInfo - i * intervalHeight - 8)));//y轴有单位
                }
            }
        }

        /// <summary>
        /// 底层画图有两个y轴一个x轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeSix(Graphics g)
        {
            //标y轴坐标值
            if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
            {
                double intervalHeight = 0;//间隔高度
                double intervalNumberLeft = 0;//间隔差值
                double intervalNumberRight = 0;//间隔差值
                double intervalGrid = 0;//网格间距
                double intervalTemp = HorizontalAxisNum - 1;//间隔数量
                bool isString = false;//是否与数值同一y值

                //计算格子间距，避免网格过密
                if (HorizontalAxisNum > 1 && HorizontalAxisNum < 7)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / intervalTemp;
                    intervalNumberLeft = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                    intervalNumberRight = (LimitUpperRightY - LimitUpperRightY) / intervalTemp;
                    intervalGrid = intervalHeight / 2;
                }
                else if (HorizontalAxisNum == 1)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / 5d;
                    intervalNumberLeft = (LimitUpperLeftY - LimitLowerLeftY) / 5d;
                    intervalNumberRight = (LimitUpperRightY - LimitUpperRightY) / 5d;
                    intervalGrid = intervalHeight / 2;
                }
                else if (HorizontalAxisNum >= 7)
                {
                    intervalHeight = (Height - 2 * TextInfo - OffsetY) / intervalTemp;
                    intervalNumberLeft = (LimitUpperLeftY - LimitLowerLeftY) / intervalTemp;
                    intervalNumberRight = (LimitUpperRightY - LimitUpperRightY) / intervalTemp;
                    intervalGrid = intervalHeight;
                }

                double tempX = TextInfo;//网格中竖线的x值
                double tempY = Height - TextInfo - OffsetY;//网格中横线的y值

                //画网格
                //横线
                myGridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;//画虚线
                while (tempY >= TextInfo + intervalGrid)
                {
                    if (HorizontalAxisNum > 1 && HorizontalAxisNum < 7)
                    {
                        if (isString)
                        {
                            tempY = tempY + intervalGrid - intervalHeight;
                            isString = false;
                        }
                        else
                        {
                            tempY -= intervalGrid;
                            isString = true;
                        }
                        g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width - TextInfo, (int)tempY));
                    }
                    else
                    {
                        tempY -= intervalGrid;
                        g.DrawLine(myGridPen, new Point(TextInfo, (int)tempY), new Point(Width - TextInfo, (int)tempY));
                    }
                }
                //竖线
                while (tempX < Width - 2 * TextInfo - intervalGrid)
                {
                    tempX += intervalGrid;
                    g.DrawLine(myGridPen, new Point((int)tempX, (int)tempY), new Point((int)tempX, Height - TextInfo));
                }
                if (tempX + intervalGrid < Width - TextInfo)
                {
                    g.DrawLine(myGridPen, new Point((int)(tempX + intervalGrid), (int)tempY), new Point((int)(tempX + intervalGrid), Height - TextInfo));
                }

                //画x轴
                g.DrawLine(mypen, new Point(TextInfo, Height - TextInfo), new Point(Width - 2 * TextInfo, Height - TextInfo));
                //画y轴
                g.DrawLine(mypen, new Point(TextInfo, (int)tempY), new Point(TextInfo, Height - TextInfo));
                g.DrawLine(mypen, new Point(Width - TextInfo, (int)tempY), new Point(Width - TextInfo, Height - TextInfo));

                //画y轴坐标值和标识
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;//画虚线
                    g.DrawLine(mypen, new Point(TextInfo, (int)(Height - TextInfo - i * intervalHeight)), new Point(Width - TextInfo, (int)(Height - TextInfo - i * intervalHeight)));//画背景网格
                    g.DrawString((LimitLowerLeftY + i * intervalNumberLeft).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(0, (int)(Height - TextInfo - i * intervalHeight - 5)));//标y轴坐标值
                    g.DrawString((LimitLowerRightY + i * intervalNumberRight).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new Point(Width - TextInfo, (int)(Height - TextInfo - i * intervalHeight - 5)));//标y轴坐标值
                }
            }
        }

        /// <summary>
        /// 画底层
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBackgroundImage()
        {
            //层图
            Bitmap BGimg = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(BGimg);

            //填充白色
            g.FillRectangle(Brushes.White, 0, 0, Width, Height);

            switch (ImageType)
            {
                case BackgroundImageType.OnlyXAxis:
                    BackgroundImageTypeOne(g);
                    break;
                case BackgroundImageType.OnlyXYAxis:
                    BackgroundImageTypeTwo(g);
                    break;
                case BackgroundImageType.OneXTwoYAxis:
                    BackgroundImageTypeThree(g);
                    break;
                case BackgroundImageType.OnlyXAxisAndGrid:
                    BackgroundImageTypeFour(g);
                    break;
                case BackgroundImageType.OnlyXYAxisAndGrid:
                    BackgroundImageTypeFive(g);
                    break;
                case BackgroundImageType.OneXTwoYAxisAndGrid:
                    BackgroundImageTypeSix(g);
                    break;
            }

            g.Dispose();
            GC.Collect();
            return BGimg;
        }

        /// <summary>
        /// 轴模式一计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointTypeOne()
        {
            //间隔高度
            double interval = (Height - 2 * TextInfo) / (LimitUpperLeftY - LimitLowerLeftY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<Point> point1 = new List<Point>();

                //计算起始坐标数；超过图形范围不显示
                int index = Data[j].Count - Width + TextInfo;
                index = index < 0 ? 0 : index;

                for (int i = index; i < Data[j].Count; i++)
                {
                    point1.Add(new Point(TextInfo + i - index, (int)(Height - TextInfo - interval * (Data[j][i] - LimitLowerLeftY) + 0.5d)));
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 轴模式二计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointTypeTwo()
        {
            //间隔高度
            double interval = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<Point> point1 = new List<Point>();

                //计算起始坐标数；超过图形范围不显示
                int index = Data[j].Count - Width + TextInfo;
                index = index < 0 ? 0 : index;

                for (int i = index; i < Data[j].Count; i++)
                {
                    point1.Add(new Point(TextInfo + OffsetX + i - index, (int)(Height - TextInfo - interval * (Data[j][i] - LimitLowerLeftY) + 0.5d)));
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 轴模式三计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointTypeThree()
        {
            //间隔高度
            double intervalLeft = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);
            double intervalRight = (Height - 2 * TextInfo - OffsetY) / (LimitUpperRightY - LimitLowerRightY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<Point> point1 = new List<Point>();

                //计算起始坐标数；超过图形范围不显示
                int index = Data[j].Count - Width + TextInfo;
                index = index < 0 ? 0 : index;

                for (int i = index; i < Data[j].Count; i++)
                {
                    point1.Add(new Point(TextInfo + OffsetX + i - index, (int)(Height - TextInfo - intervalLeft * (Data[j][i] - LimitLowerLeftY) + 0.5d)));
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 计算坐标点
        /// </summary>
        private void DataToPoint(params double[] datas)
        {
            //添加数据
            for (int i = 0; i < Data.Count; i++)
            {
                for (int j = 0; j < datas.Length; j++)
                {
                    Data[i].Add(datas[j]);
                }
            }

            //清空点位
            points.Clear();

            //计算坐标点
            switch (ImageType)
            {
                case BackgroundImageType.OnlyXAxis:
                    DataToPointTypeOne();
                    break;
                case BackgroundImageType.OnlyXYAxis:
                    DataToPointTypeTwo();
                    break;
                case BackgroundImageType.OneXTwoYAxis:
                    DataToPointTypeThree();
                    break;
                case BackgroundImageType.OnlyXAxisAndGrid:
                    DataToPointTypeOne();
                    break;
                case BackgroundImageType.OnlyXYAxisAndGrid:
                    DataToPointTypeTwo();
                    break;
                case BackgroundImageType.OneXTwoYAxisAndGrid:
                    DataToPointTypeThree();
                    break;
            }
        }

        /// <summary>
        /// 画上层
        /// </summary>
        /// <returns></returns>
        public Bitmap GetForegroundImageTypeOne(params double[] datas)
        {
            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //计算点位
            DataToPoint(datas);

            //画图
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Count < 2) continue;
                g.DrawCurve(new Pen(Color.Blue, 2.0f), points[i].ToArray(), 0);
            }

            g.Dispose();

            return img;
        }

        /// <summary>
        /// 五点差值、十一点差值
        /// </summary>
        /// <returns></returns>
        public Bitmap GetForegroundImageTypeTwoInt(List<double> x, List<double> y)
        {
            int xAxis;//圆点x坐标
            int yAxis;//圆点y坐标
            double k1;//直线斜率
            double k2;//直线斜率
            double b1;//y = k1 * x + b1
            double b2;//y = k2 * x + b2

            //圆点列表
            List<Point> dot = new List<Point>();

            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //画点
            for (int i = 0; i < x.Count; i++)
            {
                //计算圆点坐标
                xAxis = (int)((x[i] - LimitUpperX) * (width - 2 * textInfo) / (LimitLowerX - LimitUpperX)) + textInfo;
                yAxis = height - (int)((y[i] - LimitLowerLeftY) * (height - 2 * textInfo) / (LimitUpperLeftY - LimitLowerLeftY) + textInfo);

                //绘制圆点
                dot.Add(new Point(xAxis, yAxis));
                g.DrawEllipse(new Pen(Color.Green, 7.0f), dot[i].X - 2.5f, dot[i].Y - 2.5f, 5, 5);

                //绘制两点间直线
                if (i > 0)
                {
                    g.DrawLine(new Pen(Color.Orange, 2.54f), dot[i - 1], dot[i]);
                }
            }

            //将线段延伸为直线
            k1 = (dot[1].Y - dot[0].Y) * 1.0 / (dot[1].X - dot[0].X);
            b1 = dot[0].Y - (k1 * dot[0].X);

            if (dot[0].Y < Height)
            {
                dot[0] = new Point((int)((Height - b1) / k1), Height);
            }
            g.DrawLine(new Pen(Color.Orange, 2.54f), dot[0], dot[1]);

            //将线段延伸为直线
            k2 = (dot[x.Count - 1].Y - dot[x.Count - 2].Y) * 1.0 / (dot[x.Count - 1].X - dot[x.Count - 2].X);
            b2 = dot[x.Count - 2].Y - (k2 * dot[x.Count - 2].X);

            if (dot[x.Count - 1].Y > 0)
            {
                dot[x.Count - 1] = new Point((int)(-b2 / k2), 0);
            }
            g.DrawLine(new Pen(Color.Orange, 2.54f), dot[x.Count - 2], dot[x.Count - 1]);

            g.Dispose();

            return img;
        }

        /// <summary>
        /// 两点、五点拟合、十一点拟合
        /// </summary>
        /// <returns></returns>
        public Bitmap GetForegroundImageTypeTwoFit(List<double> x, List<double> y, List<double> point)
        {
            int xAxis;//圆点x坐标
            int yAxis;//圆点y坐标
            double k;//拟合直线斜率
            double b;//y = k * x + b
            Point pointA = new Point(0, 0);//拟合直线点起点
            Point pointB = new Point(0, 0);//拟合直线点终点
            List<Point> dot = new List<Point>();//圆点列表

            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //画点
            for (int i = 0; i < x.Count; i++)
            {
                //计算圆点坐标
                xAxis = (int)((x[i] - LimitUpperX) * (width - 2 * textInfo) / (LimitLowerX - LimitUpperX)) + textInfo;
                yAxis = height - (int)((y[i] - LimitLowerLeftY) * (height - 2 * textInfo) / (LimitUpperLeftY - LimitLowerLeftY) + textInfo);

                //绘制圆点
                dot.Add(new Point(xAxis, yAxis));
                g.DrawEllipse(new Pen(Color.Green, 7.0f), dot[i].X - 2.5f, dot[i].Y - 2.5f, 5, 5);
            }

            //计算拟合直线的两端点
            pointA.X = (int)((point[0] - LimitUpperX) * (width - 2 * textInfo) / (LimitLowerX - LimitUpperX)) + textInfo;
            pointA.Y = height - (int)((point[1] - LimitLowerLeftY) * (height - 2 * textInfo) / (LimitUpperLeftY - LimitLowerLeftY) + textInfo);
            pointB.X = (int)((point[2] - LimitUpperX) * (width - 2 * textInfo) / (LimitLowerX - LimitUpperX)) + textInfo;
            pointB.Y = height - (int)((point[3] - LimitLowerLeftY) * (height - 2 * textInfo) / (LimitUpperLeftY - LimitLowerLeftY) + textInfo);

            //将线段延伸为直线
            k = (pointB.Y - pointA.Y) * 1.0 / (pointB.X - pointA.X);
            b = pointA.Y - (k * pointA.X);

            if (pointA.Y < Height)
            {
                pointA.Y = Height;
                pointA.X = (int)((pointA.Y - b) / k);
            }
            if (pointB.Y > 0)
            {
                pointB.Y = 0;
                pointB.X = (int)(-b / k);
            }

            //绘制拟合直线
            g.DrawLine(new Pen(Color.Orange, 2.54f), pointA, pointB);

            g.Dispose();

            return img;
        }
    }
}
