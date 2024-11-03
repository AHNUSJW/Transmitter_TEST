using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Forms;

//Lumi 20230608
//Lumi 20230706

namespace Base.UI.MenuTool
{
    public partial class MenuToolLogReview : Form
    {
        //listBoxLog中显示的日志记录
        List<String> listLogReport = new List<string>();

        public MenuToolLogReview()
        {
            InitializeComponent();

            Rectangle ScreenArea = Screen.GetWorkingArea(this);
            if (ScreenArea.Height < this.Height)
            {
                this.Height = ScreenArea.Height;
            }
        }

        //初始化
        private void MenuToolLogReview_Load(object sender, EventArgs e)
        {
            //查询类型、日期初始化
            comboBoxType.SelectedIndex = 0;
            dateTimePickerStart.Text = DateTime.Now.ToString();
            dateTimePickerEnd.Text = DateTime.Now.ToString();

            //获取日志文件清单
            GetLogFile(sender, e);
        }

        //查询日志文件
        private void buttonSelect_Click(object sender, EventArgs e)
        {
            SelectLogFile(sender, e);
        }

        //查询按键颜色
        private void buttonSelect_MouseMove(object sender, MouseEventArgs e)
        {
            buttonSelect.BackColor = Color.DodgerBlue;
        }

        //查询按键颜色
        private void buttonSelect_MouseLeave(object sender, EventArgs e)
        {
            buttonSelect.BackColor = Color.LightSteelBlue;
        }

        //打印日志
        private void buttonPrint_Click(object sender, EventArgs e)
        {
            PrintLogData(sender, e);
        }

        //打印按键颜色
        private void buttonPrint_MouseMove(object sender, MouseEventArgs e)
        {
            buttonPrint.BackColor = Color.DodgerBlue;
        }

        //打印按键颜色
        private void buttonPrint_MouseLeave(object sender, EventArgs e)
        {
            buttonPrint.BackColor = Color.LightSteelBlue;
        }

        //选择日志文件
        private void listBoxFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            //获取日志操作时间
            GetLogTime(sender, e);
        }

        //选择操作时间
        private void listBoxTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            //获得具体日志内容
            GetLogReport(sender, e);
        }

        //打印日志实际调用的方法
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font f = new Font("Arial", 10);
            int currentY = 40;
            int currentX = 40;

            for (int i = 0; i < listLogReport.Count; i++)
            {
                if (currentY > e.PageBounds.Height - 40)
                {
                    currentX = Convert.ToInt32(e.PageBounds.Width * 0.5);
                    currentY = 40;
                }
                //逐行打印
                e.Graphics.DrawString(listLogReport[i], f, Brushes.Black, currentX, currentY);
                currentY += 15;
            }
            e.HasMorePages = false;
        }

        //查询日志文件
        private void SelectLogFile(object sender, EventArgs e)
        {
            listLogReport.Clear();
            listBoxFile.Items.Clear();
            listBoxTime.Items.Clear();
            listBoxLog.Items.Clear();

            //判断日志目录是否存在
            if (!Directory.Exists(MyDevice.D_logPath))
            {
                //不存在
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("没有找到日志文件");
                }
                else
                {
                    MessageBox.Show("No log file found");
                }
                Directory.CreateDirectory(MyDevice.D_logPath);
            }
            else if (Directory.Exists(MyDevice.D_logPath))
            {
                //存在
                DirectoryInfo meDirectory = new DirectoryInfo(MyDevice.D_logPath);

                //依据型号查询
                String typeString = null;
                switch (comboBoxType.SelectedIndex)
                {
                    //查询全部
                    case 0:
                        typeString = "*.txt";
                        break;
                    case 1:
                        typeString = "BE30AH*.txt";
                        break;
                    case 2:
                        typeString = "BS420H*.txt";
                        break;
                    case 3:
                        typeString = "BS600H*.txt";
                        break;
                    case 4:
                        typeString = "T420*.txt";
                        break;
                    case 5:
                        typeString = "TNP10*.txt";
                        break;
                    case 6:
                        typeString = "TP10*.txt";
                        break;
                    case 7:
                        typeString = "TDES*.txt";
                        break;
                    case 8:
                        typeString = "TDSS*.txt";
                        break;
                    case 9:
                        typeString = "TD485*.txt";
                        break;
                    case 10:
                        typeString = "TCAN*.txt";
                        break;
                    case 11:
                        typeString = "iBus*.txt";
                        break;
                    default:
                        typeString = null;
                        break;
                }

                String meString = null;
                DateTime lastWriteTime = DateTime.Now;
                DateTime startTime = Convert.ToDateTime(dateTimePickerStart.Text);
                DateTime endTime = Convert.ToDateTime(dateTimePickerEnd.Text).AddDays(1);

                if (DateTime.Compare(startTime, endTime) > 0)
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("开始时间不能晚于结束时间");
                    }
                    else
                    {
                        MessageBox.Show("The start time cannot be later than the end time");
                    }
                }
                else
                {
                    //先判断类型是否符合所选
                    foreach (FileInfo meFiles in meDirectory.GetFiles(typeString))
                    {
                        //再判断时间是否符合所选
                        meString = meFiles.Name;
                        lastWriteTime = meFiles.LastWriteTime;
                        meString = meString.Replace(".txt", "");
                        if ((DateTime.Compare(lastWriteTime, startTime) > 0 && DateTime.Compare(lastWriteTime, endTime) < 0) ||
                           (DateTime.Compare(lastWriteTime, startTime) == 0 || DateTime.Compare(lastWriteTime, endTime) == 0))
                        {
                            listBoxFile.Items.Add(meString);
                        }
                    }
                }
            }
        }

        //获取日志文件清单
        private void GetLogFile(object sender, EventArgs e)
        {
            //判断日志目录是否存在
            if (!Directory.Exists(MyDevice.D_logPath))
            {
                //不存在
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("未找到日志文件");
                }
                else
                {
                    MessageBox.Show("No log file found");
                }
                Directory.CreateDirectory(MyDevice.D_logPath);
            }
            else if (Directory.Exists(MyDevice.D_logPath))
            {
                //存在
                DirectoryInfo meDirectory = new DirectoryInfo(MyDevice.D_logPath);

                String meString = null;
                foreach (FileInfo meFiles in meDirectory.GetFiles("*.txt"))
                {
                    meString = meFiles.Name;
                    meString = meString.Replace(".txt", "");
                    listBoxFile.Items.Add(meString);
                }

            }
        }

        //获取日志记录和记录时间
        private void GetLogTime(object sender, EventArgs e)
        {
            listLogReport.Clear();
            listBoxLog.Items.Clear();
            listBoxTime.Items.Clear();

            if (listBoxFile.SelectedItem == null)
            {
                return;
            }

            String filePath = MyDevice.D_logPath + "\\" + listBoxFile.SelectedItem.ToString() + ".txt";
            String[] meLines = null;
            if (!File.Exists(filePath))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("选择的日志文件不存在");
                }
                else
                {
                    MessageBox.Show("The selected log file does not exist");
                }
            }
            else if (File.Exists(filePath))
            {
                meLines = File.ReadAllLines(filePath);
            }

            if (meLines != null)
            {
                foreach (String line in meLines)
                {
                    //通过判断读取的line是否可以转换为时间获取单条记录的操作时间
                    DateTime tmpDate;
                    if (DateTime.TryParse(line, out tmpDate))
                    {
                        listBoxTime.Items.Add(line);
                    }
                }
            }
            else
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("时间未读取成功");
                }
                else
                {
                    MessageBox.Show("The time was not read successfully");
                }
            }
        }

        //获取符合操作时间选择的日志记录
        private void GetLogReport(object sender, EventArgs e)
        {
            listLogReport.Clear();
            listBoxLog.Items.Clear();

            List<String> listLogTmp = new List<string>();
            String filePath = MyDevice.D_logPath + "\\" + listBoxFile.SelectedItem.ToString() + ".txt";
            String[] meLines = null;
            if (!File.Exists(filePath))
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("选择的日志文件不存在");
                }
                else
                {
                    MessageBox.Show("The selected log file does not exist");
                }
            }
            else if (File.Exists(filePath))
            {
                meLines = File.ReadAllLines(filePath);
            }

            //获取符合所选操作时间的日志记录
            bool isFind = false;
            if (meLines != null && listBoxTime.SelectedItem != null)
            {
                int lineNumNow = -1;
                int lineNumStart = -1;
                int lineNumEnd = -1;

                //获取符合所选操作时间的日志记录的起始行号和结束行号
                foreach (String line in meLines)
                {
                    lineNumNow++;
                    //找起始行号
                    if (line.Equals(listBoxTime.SelectedItem.ToString()))
                    {
                        lineNumStart = lineNumNow;
                        isFind = true;
                    }

                    //找结束行号
                    if (line.Equals("[END]") && isFind)
                    {
                        lineNumEnd = lineNumNow;
                        lineNumNow = -1;
                        break;
                    }
                }

                //获取符合所选操作时间的日志记录并转换格式
                if (isFind)
                {
                    //获取符合所选操作时间的日志记录
                    foreach (String line in meLines)
                    {
                        lineNumNow++;
                        if ((lineNumNow > lineNumStart && lineNumNow < lineNumEnd) ||
                             lineNumNow == lineNumStart ||
                             lineNumNow == lineNumEnd)
                        {
                            listLogTmp.Add(line);
                        }
                        else if (lineNumNow > lineNumEnd)
                        {
                            break;
                        }
                    }

                    //将日志记录转换为符合格式的报告
                    FormatLogReport(listLogTmp);

                    //将转换好的报告加入到listbox
                    foreach (String line in listLogReport)
                    {
                        listBoxLog.Items.Add(line);
                    }
                }
            }
        }

        //将日志记录转换为要求的格式
        private void FormatLogReport(List<String> items)
        {
            listLogReport.Clear();

            bool isBohr = false;   //标记日志类型
            string type = null;    //型号
            string sct;            //记录当前item属于SCT几
            string itemsTmp;       //值为items[i].ToString()
            Dictionary<String, int> itemDic = new Dictionary<string, int>()
            {
                { "SCT0", 0 },
                { "SCT1", 0 },
                { "SCT2", 0 },
                { "SCT3", 0 },
                { "SCT4", 0 },
                { "SCT5", 0 },
                { "SCT6", 0 },
                { "SCT7", 0 },
                { "SCT8", 0 },
                { "SCT9", 0 }
            }; //记录该item为第几个SCT中的第几项

            //判断日志格式和产品类型
            for (int i = 0; i < items.Count; i++)
            {
                itemsTmp = items[i].ToString();

                //判断日志格式
                if (itemsTmp.Contains("myXET.E_test"))
                {
                    isBohr = true;
                    break;
                }

                //判断日志类型
                if (itemsTmp.ToString().Contains("[type]"))
                {
                    type = LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1];
                }
            }

            //转换格式
            listLogReport.Add("------------------------------------------");
            listLogReport.Add("型号                 = " + type);
            for (int i = 1; i < items.Count; i++)
            {
                itemsTmp = items[i].ToString();
                if (itemsTmp != "")
                {
                    switch (LineToValue(itemsTmp)[1])
                    {
                        case "datecode":
                            listLogReport.Add("操作时间             = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ":" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]);
                            listLogReport.Add("------------------------------------------");
                            break;
                        default:
                            break;
                    }
                }

                //日志用户为bohr、BCS16时
                if (isBohr && itemsTmp.Contains("SCT"))
                {
                    sct = LineToValue(itemsTmp)[1];
                    if (sct == "SCT0")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {

                            case "myXET.E_test":
                                listLogReport.Add("[SCT0] FWx           = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_outype":
                                listLogReport.Add("[SCT0] 配置          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_curve":
                                listLogReport.Add("[SCT0] 线性拟合方式  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_adspeed":
                                listLogReport.Add("[SCT0] 采样参数      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_autozero":
                                listLogReport.Add("[SCT0] 上电归零      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_trackzero":
                                listLogReport.Add("[SCT0] 零点跟踪      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_checkhigh":
                                listLogReport.Add("[SCT0] 检重上限      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_checklow":
                                listLogReport.Add("[SCT0] 检重下限      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_mfg_date":
                                listLogReport.Add("[SCT0] 校准时间      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_mfg_srno":
                                listLogReport.Add("[SCT0] 序列号        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_tmp_min":
                                listLogReport.Add("[SCT0] 低温度记录    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_tmp_max":
                                listLogReport.Add("[SCT0] 高温度记录    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_tmp_cal":
                                listLogReport.Add("[SCT0] AD值          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_bohrcode":
                                listLogReport.Add("[SCT0] UID           = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_enspan":
                                listLogReport.Add("[SCT0] 按键SPAN锁    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_protype":
                                listLogReport.Add("[SCT0] 产品类型      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT0] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT1"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT1")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_ad_point1":
                                listLogReport.Add("[SCT1] 内码1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point2":
                                listLogReport.Add("[SCT1] 内码2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point3":
                                listLogReport.Add("[SCT1] 内码3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point4":
                                listLogReport.Add("[SCT1] 内码4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point5":
                                listLogReport.Add("[SCT1] 内码5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point1":
                                listLogReport.Add("[SCT1] 数值1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point2":
                                listLogReport.Add("[SCT1] 数值2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point3":
                                listLogReport.Add("[SCT1] 数值3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point4":
                                listLogReport.Add("[SCT1] 数值4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point5":
                                listLogReport.Add("[SCT1] 数值5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT1] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT2"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT2")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_input1":
                                listLogReport.Add("[SCT2] 输入1         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input2":
                                listLogReport.Add("[SCT2] 输入2         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input3":
                                listLogReport.Add("[SCT2] 输入3         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input4":
                                listLogReport.Add("[SCT2] 输入4         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input5":
                                listLogReport.Add("[SCT2] 输入5         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog1":
                                listLogReport.Add("[SCT2] 输出1         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog2":
                                listLogReport.Add("[SCT2] 输出2         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog3":
                                listLogReport.Add("[SCT2] 输出3         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog4":
                                listLogReport.Add("[SCT2] 输出4         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog5":
                                listLogReport.Add("[SCT2] 输出5         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            default:
                                listLogReport.Add("[SCT2] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT3"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT3")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_ad_zero":
                                listLogReport.Add("[SCT3] 零点内码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_full":
                                listLogReport.Add("[SCT3] 满点内码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_zero":
                                listLogReport.Add("[SCT3] 零点数值      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full":
                                listLogReport.Add("[SCT3] 满点数值      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_vtio":
                                listLogReport.Add("[SCT3] e_vtio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_wtio":
                                listLogReport.Add("[SCT3] e_wtio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_atio":
                                listLogReport.Add("[SCT3] e_atio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_btio":
                                listLogReport.Add("[SCT3] e_btio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_ctio":
                                listLogReport.Add("[SCT3] e_ctio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_dtio":
                                listLogReport.Add("[SCT3] e_dtio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            default:
                                listLogReport.Add("[SCT3] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT4"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT4")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_da_zero_4ma":
                                listLogReport.Add("[SCT4] 零点4-20mA    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full_20ma":
                                listLogReport.Add("[SCT4] 满点4-20mA    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_zero_05V":
                                listLogReport.Add("[SCT4] 零点0-5V      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full_05V":
                                listLogReport.Add("[SCT4] 满点0-5V      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_zero_10V":
                                listLogReport.Add("[SCT4] 零点0-10V     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full_10V":
                                listLogReport.Add("[SCT4] 满点0-10V     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_zero_N5":
                                listLogReport.Add("[SCT4] 零点±5V      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full_P5":
                                listLogReport.Add("[SCT4] 满点±5V      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_zero_N10":
                                listLogReport.Add("[SCT4] 零点±10V     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_full_P10":
                                listLogReport.Add("[SCT4] 满点±10V     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT4] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT5"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT5")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_corr":
                                listLogReport.Add("[SCT5] 校准系数      = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_mark":
                                listLogReport.Add("[SCT5] 校准标记      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_sign":
                                listLogReport.Add("[SCT5] 校验方式      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_addr":
                                listLogReport.Add("[SCT5] 站点地址      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_baud":
                                listLogReport.Add("[SCT5] 通讯波特率    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_stopbit":
                                listLogReport.Add("[SCT5] 通讯停止位    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_parity":
                                listLogReport.Add("[SCT5] 通讯校验位    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_zero":
                                listLogReport.Add("[SCT5] 数字量零点    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_full":
                                listLogReport.Add("[SCT5] 数字量满点    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_decimal":
                                listLogReport.Add("[SCT5] 数字量小数点  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_unit":
                                listLogReport.Add("[SCT5] 数字量单位    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_ascii":
                                listLogReport.Add("[SCT5] 连续发送格式  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_sptime":
                                listLogReport.Add("[SCT5] 稳定次数      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_spfilt":
                                listLogReport.Add("[SCT5] 滤波深度      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_division":
                                listLogReport.Add("[SCT5] 数字量分度值  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wt_antivib":
                                listLogReport.Add("[SCT5] 滤波等级      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_heartBeat":
                                listLogReport.Add("[SCT5] 心跳时间      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_typeTPDO0":
                                listLogReport.Add("[SCT5] TPDO0类型     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_evenTPDO0":
                                listLogReport.Add("[SCT5] TPDO0时间     = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_nodeID":
                                listLogReport.Add("[SCT5] 节点ID        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_nodeBaud":
                                listLogReport.Add("[SCT5] 节点波特率    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_thmax":
                                listLogReport.Add("[SCT5] AI            = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_thmin":
                                listLogReport.Add("[SCT5] AI            = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_stablerange":
                                listLogReport.Add("[SCT5] 稳定范围      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_stabletime":
                                listLogReport.Add("[SCT5] 稳定时间      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_tkzerotime":
                                listLogReport.Add("[SCT5] 零点跟踪      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_tkdynatime":
                                listLogReport.Add("[SCT5] 动态跟踪      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT5] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if ((items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT6")) ||
                           (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]")))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT6")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_ad_point6":
                                listLogReport.Add("[SCT6] 内码6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point7":
                                listLogReport.Add("[SCT6] 内码7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point8":
                                listLogReport.Add("[SCT6] 内码8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point9":
                                listLogReport.Add("[SCT6] 内码9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_ad_point10":
                                listLogReport.Add("[SCT6] 内码10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point6":
                                listLogReport.Add("[SCT6] 数值6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point7":
                                listLogReport.Add("[SCT6] 数值7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point8":
                                listLogReport.Add("[SCT6] 数值8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point9":
                                listLogReport.Add("[SCT6] 数值9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point10":
                                listLogReport.Add("[SCT6] 数值10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT6] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if ((items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT7")) ||
                           (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]")))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT7")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_input6":
                                listLogReport.Add("[SCT7] 输入6         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input7":
                                listLogReport.Add("[SCT7] 输入7         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input8":
                                listLogReport.Add("[SCT7] 输入8         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input9":
                                listLogReport.Add("[SCT7] 输入9         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_input10":
                                listLogReport.Add("[SCT7] 输入10        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog6":
                                listLogReport.Add("[SCT7] 输出6         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog7":
                                listLogReport.Add("[SCT7] 输出7         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog8":
                                listLogReport.Add("[SCT7] 输出8         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog9":
                                listLogReport.Add("[SCT7] 输出9         = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog10":
                                listLogReport.Add("[SCT7] 输出10        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            default:
                                listLogReport.Add("[SCT7] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if ((items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT8")) ||
                           (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]")))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT8")
                    {
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_ad_point11":
                                listLogReport.Add("[SCT8] 内码11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_da_point11":
                                listLogReport.Add("[SCT8] 数值11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_input11":
                                listLogReport.Add("[SCT8] 输入11        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_analog11":
                                listLogReport.Add("[SCT8] 输出11        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_etio":
                                listLogReport.Add("[SCT8] e_etio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_ftio":
                                listLogReport.Add("[SCT8] e_ftio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_gtio":
                                listLogReport.Add("[SCT8] e_gtio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_htio":
                                listLogReport.Add("[SCT8] e_htio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_itio":
                                listLogReport.Add("[SCT8] e_itio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            case "myXET.E_jtio":
                                listLogReport.Add("[SCT8] e_jtio        = " + LineToValue(itemsTmp)[3] + " <" + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 2] + ">"); break;
                            default:
                                listLogReport.Add("[SCT3] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }
                        if ((items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT9")) ||
                           (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]")))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT9")
                    {
                        //依据顺序匹配无参数名的
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "myXET.E_enGFC":
                                listLogReport.Add("[SCT9] GFC有效       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_enSRDO":
                                listLogReport.Add("[SCT9] 信息方向      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_SCT_time":
                                listLogReport.Add("[SCT9] SCT时间       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_COB_ID1":
                                listLogReport.Add("[SCT9] COB_ID1       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_COB_ID2":
                                listLogReport.Add("[SCT9] COB_ID2       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_enOL":
                                listLogReport.Add("[SCT9] 超载1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_overload":
                                listLogReport.Add("[SCT9] 超载2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.alarmMode":
                                listLogReport.Add("[SCT9] 报警1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wetTarget":
                                listLogReport.Add("[SCT9] 报警2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wetLow":
                                listLogReport.Add("[SCT9] 报警值1       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_wetHigh":
                                listLogReport.Add("[SCT9] 报警值2       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_filter":
                                listLogReport.Add("[SCT9] 滤波范围      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_netServicePort":
                                listLogReport.Add("[SCT9] 端口号        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_netServiceIP":
                                listLogReport.Add("[SCT9] IP地址1       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_netClientIP":
                                listLogReport.Add("[SCT9] IP地址2       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_netGatIP":
                                listLogReport.Add("[SCT9] 网关地址      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_netMaskIP":
                                listLogReport.Add("[SCT9] 子网掩码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_useDHCP":
                                listLogReport.Add("[SCT9] DHCP          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_useScan":
                                listLogReport.Add("[SCT9] Scan          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_addrRF":
                                listLogReport.Add("[SCT9] 网关地址      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_spedRF":
                                listLogReport.Add("[SCT9] 子网掩码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_chanRF":
                                listLogReport.Add("[SCT9] DHCP          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "myXET.E_optionRF":
                                listLogReport.Add("[SCT9] Scan          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT9] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                }
                //日志用户为其他时
                else if (!isBohr && itemsTmp != "")
                {
                    sct = LineToValue(itemsTmp)[1];

                    if (sct == "SCT0")
                    {
                        //依次读取日志条目，根据顺序和条目名匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT0"]++;

                        //先匹配有参数名的
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "test":
                                listLogReport.Add("[SCT0] FWx           = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "outype":
                                listLogReport.Add("[SCT0] 配置          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "curve":
                                listLogReport.Add("[SCT0] 线性拟合方式  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "adspeed":
                                listLogReport.Add("[SCT0] 采样参数      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "protype":
                                listLogReport.Add("[SCT0] 产品类型      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                break;
                        }

                        //再依据顺序匹配无参数名的
                        switch (itemDic["SCT0"])
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                break;
                            case 5:
                                listLogReport.Add("[SCT0] 上电归零      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT0] 零点跟踪      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT0] 上限          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT0] 下限          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT0] 校准时间      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT0] 序列号        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 11:
                                listLogReport.Add("[SCT0] 最低温记录    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 12:
                                listLogReport.Add("[SCT0] 最高温记录    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 13:
                                listLogReport.Add("[SCT0] AD值          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 14:
                                listLogReport.Add("[SCT0] UID           = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 15:
                                listLogReport.Add("[SCT0] 按键SPAN锁    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 16:
                                break;
                            default:
                                listLogReport.Add("[SCT0] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT1"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT1")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT1"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT1"])
                        {
                            case 1:
                                listLogReport.Add("[SCT1] 内码1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT1] 内码2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT1] 内码3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT1] 内码4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT1] 内码5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT1] 数值1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT1] 数值2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT1] 数值3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT1] 数值4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT1] 数值5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT1] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT2"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT2")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT2"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT2"])
                        {
                            case 1:
                                listLogReport.Add("[SCT2] 输入1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT2] 输入2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT2] 输入3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT2] 输入4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT2] 输入5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT2] 输出1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT2] 输出2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT2] 输出3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT2] 输出4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT2] 输出5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT2] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT3"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT3")
                    {
                        //依次读取日志条目，根据顺序和条目名匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT3"]++;

                        //先匹配有参数名的
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "adz":
                                listLogReport.Add("[SCT3] 零点内码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "adf":
                                listLogReport.Add("[SCT3] 满点内码      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "daz":
                                listLogReport.Add("[SCT3] 零点数值      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "daf":
                                listLogReport.Add("[SCT3] 满点数值      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                break;
                        }

                        //再依据顺序匹配无参数名的
                        switch (itemDic["SCT3"])
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                break;
                            case 5:
                                listLogReport.Add("[SCT3] e_vtio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT3] e_wtio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT3] e_atio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT3] e_btio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT3] e_ctio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT3] e_dtio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT3] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT4"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT4")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT4"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT4"])
                        {
                            case 1:
                                listLogReport.Add("[SCT4] cal值1        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT4] cal值2        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT4] cal值3        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT4] cal值4        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT4] cal值5        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT4] cal值6        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT4] cal值7        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT4] cal值8        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT4] cal值9        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT4] cal值10       = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT4] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT5"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT5")
                    {
                        //依次读取日志条目，根据顺序和条目名匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT5"]++;

                        //先匹配有参数名的
                        switch (LineToValue(itemsTmp)[2])
                        {
                            case "addr":
                                listLogReport.Add("[SCT5] 站点地址      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "wtz":
                                listLogReport.Add("[SCT5] 数字量零点    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "wtf":
                                listLogReport.Add("[SCT5] 数字量满点    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "decimal":
                                listLogReport.Add("[SCT5] 数字量小数点  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "unit":
                                listLogReport.Add("[SCT5] 数字量单位    = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "division":
                                listLogReport.Add("[SCT5] 数字量分度值  = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "beat":
                                listLogReport.Add("[SCT5] 心跳时间      = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case "nodeID":
                                listLogReport.Add("[SCT5] 节点ID        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                break;
                        }

                        //再依据顺序匹配无参数名的
                        switch (itemDic["SCT5"])
                        {
                            case 1:
                                listLogReport.Add("[SCT5] 校准1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT5] 校准2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT5] 校验1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                break;
                            case 5:
                                listLogReport.Add("[SCT5] 通讯1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT5] 通讯2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT5] 通讯3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                                break;
                            case 12:
                                listLogReport.Add("[SCT5] 参数1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 13:
                                listLogReport.Add("[SCT5] 参数2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 14:
                                listLogReport.Add("[SCT5] 参数3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 15:
                                break;
                            case 16:
                                listLogReport.Add("[SCT5] 参数4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 17:
                                break;
                            case 18:
                                listLogReport.Add("[SCT5] 参数5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 19:
                                listLogReport.Add("[SCT5] 参数6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 20:
                                break;
                            case 21:
                                listLogReport.Add("[SCT5] 参数7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 22:
                                listLogReport.Add("[SCT5] 参数8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 23:
                                listLogReport.Add("[SCT5] 参数9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 24:
                                listLogReport.Add("[SCT5] 参数10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 25:
                                listLogReport.Add("[SCT5] 参数11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 26:
                                listLogReport.Add("[SCT5] 参数12        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 27:
                                listLogReport.Add("[SCT5] 参数13        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 28:
                                listLogReport.Add("[SCT5] 参数14        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 29:
                                listLogReport.Add("[SCT5] 参数15        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT5] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT6"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT6")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT6"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT6"])
                        {
                            case 1:
                                listLogReport.Add("[SCT6] 内码6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT6] 内码7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT6] 内码8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT6] 内码9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT6] 内码10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT6] 数值6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT6] 数值7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT6] 数值8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT6] 数值9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT6] 数值10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT6] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT7"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT7")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT7"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT7"])
                        {
                            case 1:
                                listLogReport.Add("[SCT7] 输入6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT7] 输入7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT7] 输入8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT7] 输入9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT7] 输入10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT7] 输出6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT7] 输出7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT7] 输出8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT7] 输出9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT7] 输出10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT7] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT8"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT8")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT8"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT8"])
                        {
                            case 1:
                                listLogReport.Add("[SCT8] 内码11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT8] 数值11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT8] 输入11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT8] 输出11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT8] e_etio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT8] e_ftio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT8] e_gtio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT8] e_htio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT8] e_itio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT8] e_jtio        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT8] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if ((items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]")) || (items[i + 2].ToString() != "" && items[i + 2].ToString().Contains("SCT9")))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                    else if (sct == "SCT9")
                    {
                        //依次读取日志条目，根据顺序匹配对应的名称，并保证显示后参数顺序和日志文件一致
                        itemDic["SCT9"]++;

                        //依据顺序匹配无参数名的
                        switch (itemDic["SCT9"])
                        {
                            case 1:
                                listLogReport.Add("[SCT9] 参数1         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 2:
                                listLogReport.Add("[SCT9] 参数2         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 3:
                                listLogReport.Add("[SCT9] 参数3         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 4:
                                listLogReport.Add("[SCT9] 参数4         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 5:
                                listLogReport.Add("[SCT9] 参数5         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 6:
                                listLogReport.Add("[SCT9] 参数6         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 7:
                                listLogReport.Add("[SCT9] 参数7         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 8:
                                listLogReport.Add("[SCT9] 参数8         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 9:
                                listLogReport.Add("[SCT9] 参数9         = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 10:
                                listLogReport.Add("[SCT9] 参数10        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            case 11:
                                listLogReport.Add("[SCT9] 参数11        = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                            default:
                                listLogReport.Add("[SCT9] 参数          = " + LineToValue(itemsTmp)[LineToValue(itemsTmp).Length - 1]); break;
                        }

                        if (items[i + 3].ToString() != "" && items[i + 3].ToString().Contains("[END]"))
                        {
                            listLogReport.Add("------------------------------------------");
                        }
                    }
                }
            }
        }

        //打印日志
        private void PrintLogData(object sender, EventArgs e)
        {
            if (!Directory.Exists(MyDevice.D_logPath))
            {
                //日志路径不存在
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("没有日志文件！");
                }
                else
                {
                    MessageBox.Show("No log file found.");
                }
                Directory.CreateDirectory(MyDevice.D_cfgPath);
            }
            else if (Directory.Exists(MyDevice.D_logPath) && listLogReport.Any())
            {
                PrintDocument printDocument1 = new PrintDocument();
                //注册PrintPage事件，打印每一页时会触发该事件
                printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);

                //初始化打印对话框对象
                PrintDialog pd = new PrintDialog();
                //将printDocument1对象赋值给打印对话框的Document属性
                pd.Document = printDocument1;

                //打开打印对话框
                DialogResult result = pd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    //开始打印
                    printDocument1.Print();
                }
            }
            else if (Directory.Exists(MyDevice.D_logPath) && listLogReport.Any() != true)
            {
                if (MyDevice.languageType == 0)
                {
                    MessageBox.Show("请选择日志和操作时间后再打印");
                }
                else
                {
                    MessageBox.Show("Please select log and operation time before printing");
                }
            }
        }

        //截取字符串
        public static string[] LineToValue(String line)
        {
            line = line.Replace(" ", "");
            return line.Split(new char[] { '=', '[', ']', ':', ';', '<', '>' });
        }
    }
}
