using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

//Tong Ziyun 20230410
//Tong Ziyun 20230506
//Lumi 20240314

namespace Base.UI.MenuTool
{
    public partial class MenuToolParImport : Form
    {
        private XET actXET;//需要操作的设备
        private List<byte> addrList = new List<byte>();//需要操作的设备地址列表
        private int index = 0;//正在操作的设备列表下标

        public MenuToolParImport()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 导入设备参数界面加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolParImport_Load(object sender, EventArgs e)
        {
            actXET = MyDevice.actDev;

            //加载接收触发
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //所有用户加载
            if (Directory.Exists(MyDevice.D_cfgPath))
            {
                //存在
                DirectoryInfo meDirectory = new DirectoryInfo(MyDevice.D_cfgPath);
                String meString = null;
                foreach (FileInfo meFiles in meDirectory.GetFiles("*.txt"))
                {
                    meString = meFiles.Name;
                    meString = meString.Replace(".txt", "");
                    listBox1.Items.Add(meString);
                }
            }
            else
            {
                //不存在则创建文件夹
                Directory.CreateDirectory(MyDevice.D_cfgPath);
            }

            //初始化设备地址值
            switch (MyDevice.protocol.type)
            {
                default:
                    if (MyDevice.devSum > 0)//单设备
                    {
                        checkedListBox1.Items.Add("  SelfUART");
                        checkedListBox1.Enabled = false;
                    }
                    break;
                case COMP.RS485:
                    for (int i = 0; i < 255; i++)
                    {
                        if (MyDevice.mBUS[i].sTATE == STATE.WORKING)
                        {
                            checkedListBox1.Items.Add("  RS485_[ADDR]" + i);
                        }
                    }
                    if (MyDevice.devSum == 1)//单设备
                    {
                        checkedListBox1.Enabled = false;
                    }
                    break;
                case COMP.CANopen:
                    for (int i = 0; i < 127; i++)
                    {
                        if (MyDevice.mCAN[i].sTATE == STATE.WORKING)
                        {
                            checkedListBox1.Items.Add("  CANopen_[ADDR]" + i);
                        }
                    }
                    break;
                case COMP.ModbusTCP:
                    for (int i = 0; i < 255; i++)
                    {
                        if (MyDevice.mMTCP[i].sTATE == STATE.WORKING)
                        {
                            checkedListBox1.Items.Add("  ModbusTCP_" + MyDevice.mMTCP[i].E_addr + ":" + MyDevice.mMTCP[i].R_ipAddr);
                        }
                    }
                    break;
            }

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolParImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消接收触发
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        /// <summary>
        /// 选择不同文件导入文件数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem?.ToString() == null) return;

            String meString = MyDevice.D_cfgPath + "\\" + listBox1.SelectedItem.ToString() + ".txt";
            if (File.Exists(meString))
            {
                String[] meLines = MyDevice.LoadFromCfg(listBox1.SelectedItem.ToString());
                if (meLines != null)
                {
                    listBox2.Items.Clear();
                    foreach (String line in meLines)
                    {
                        listBox2.Items.Add(line);
                    }
                }
                else
                {
                    if (MyDevice.languageType == 0)
                    {
                        MessageBox.Show("未导入成功");
                    }
                    else
                    {
                        MessageBox.Show("Failed import");
                    }
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// 导入数据文件更新设备数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Byte addr; //  地址值

            //存储listBox2中的数据
            List<String> list = new List<string>();
            foreach (string line in listBox2.Items)
            {
                list.Add(line);
            }

            //初始化label1，提示信息
            label1.Text = "Tips:";

            //清空需要操作的设备地址列表
            addrList.Clear();

            //初始化操作设备地址列表下标
            index = 0;

            if (listBox2.Items.Count > 0)//导入参数详情有数据时
            {
                if (checkedListBox1.CheckedItems.Count > 0)
                {
                    button2.HoverBackColor = Color.Red;

                    //遍历所有勾选的设备，导入参数
                    foreach (string line in checkedListBox1.CheckedItems)
                    {
                        switch (LineToValue(line.Trim())[0])
                        {
                            default:
                            case "SelfUART": addr = 1; break;
                            case "RS485": addr = Convert.ToByte(LineToValue(line.Trim())[3]); break;
                            case "CANopen": addr = Convert.ToByte(LineToValue(line.Trim())[3]); break;
                        }

                        MyDevice.protocol.addr = (byte)addr;

                        actXET = MyDevice.actDev;

                        //参数可以写入设备时，记录下该设备
                        if (WriteToSCT(actXET, list) && MyDevice.protocol.IsOpen)
                        {
                            addrList.Add(addr);
                        }
                        else
                        {
                            label1.Text += line.Trim() + ",";//记录未写成功的设备并显示
                        }
                    }

                    //记录未写成功的设备并显示
                    if (label1.Text != "Tips:")
                    {
                        if (MyDevice.languageType == 0)
                        {
                            label1.Text += "未写入成功";
                        }
                        else
                        {
                            label1.Text += "Failed to write";
                        }
                    }

                    //遍历可以写参数的设备，将参数写入到设备
                    if (addrList.Count > 0)
                    {
                        //BE30AH\BS420H\BS600H\TNP10\T420型号没有SCT5，不存在actXET.E_addr，默认值为0
                        MyDevice.protocol.addr = addrList[0];
                        MyDevice.mePort_ClearState();
                        MyDevice.mePort_WriteAllTasks();
                    }
                }
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
                //继续写
                MyDevice.mePort_WriteAllTasks();

                //按钮文字表示当前写入状态,没有收到回复或者回复校验失败
                button2.Text = MyDevice.protocol.trTASK.ToString();

                //单设备写完后，判断是否继续写其他设备
                if (MyDevice.protocol.trTASK == TASKS.NULL)
                {
                    if (++index < addrList.Count)
                    {
                        //多设备导入参数时，设备写入
                        MyDevice.protocol.addr = addrList[index];
                        MyDevice.mePort_ClearState();
                        MyDevice.mePort_WriteAllTasks();
                    }
                    else
                    {
                        button2.HoverBackColor = Color.Green;
                        button2.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                        if (label1.Text == "Tips:")
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label1.Text += "写入成功";
                            }
                            else
                            {
                                label1.Text += "All written successfully";
                            }
                        }
                    }
                }
                else if ((MyDevice.protocol.trTASK == TASKS.REST) && (actXET.E_test < 0x58)) //老版本重启指令无回复
                {
                    //多设备导入参数时，设备写入
                    if (++index < addrList.Count)
                    {
                        MyDevice.protocol.addr = addrList[index];
                        MyDevice.mePort_ClearState();
                        MyDevice.mePort_WriteAllTasks();
                    }
                    else
                    {
                        button2.HoverBackColor = Color.Green;
                        button2.Text = MyDevice.languageType == 0 ? "成功" : "Success";
                        if (label1.Text == "Tips:")
                        {
                            if (MyDevice.languageType == 0)
                            {
                                label1.Text += "全部写入成功";
                            }
                            else
                            {
                                label1.Text += "All written successfully";
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据文档配置参数修改设备参数
        /// </summary>
        /// <param name="myXET"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool WriteToSCT(XET myXET, List<string> items)
        {
            //判断是否是同类型的
            if (myXET.S_DeviceType.ToString() != LineToValue(items[0].ToString())[1])
            {
                return false;
            }

            //获取写入的配置数据
            OUT w_OutType = (OUT)Enum.Parse(typeof(OUT), LineToValue(items[1].ToString())[1]);

            //判断配置是否可以被写入
            switch (myXET.S_DeviceType)
            {
                case TYPE.TDES:
                    if (myXET.S_OutType == OUT.UT420)
                    {
                        //设备为TDES-420时，导入的设备参数模拟量输出只能为4-20mA
                        if (w_OutType != OUT.UT420)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //设备为TDES-10或TP10时，导入的设备参数模拟量输出不能为4-20mA
                        if (w_OutType == OUT.UT420)
                        {
                            return false;
                        }
                        else
                        {
                            myXET.S_OutType = w_OutType;
                        }
                    }
                    break;

                case TYPE.TDSS:
                    if (myXET.S_OutType == OUT.UT420)
                    {
                        //设备为TDSS-420时，导入的设备参数模拟量输出只能为4-20mA
                        if (w_OutType != OUT.UT420)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //设备为TDES-NP10时，导入的设备参数模拟量输出不能为4-20mA
                        if (w_OutType == OUT.UT420)
                        {
                            return false;
                        }
                        else
                        {
                            myXET.S_OutType = w_OutType;
                        }
                    }
                    break;

                default:
                    myXET.S_OutType = w_OutType;
                    break;
            }

            //导入配置
            for (int i = 2; i < items.Count; i++)
            {
                if (items[i].ToString() != "")
                {
                    switch (LineToValue(items[i].ToString())[0])
                    {
                        case "采样参数": myXET.E_adspeed = byte.Parse(LineToValue(items[i].ToString())[1].Substring(2), System.Globalization.NumberStyles.HexNumber); break;
                        case "上电归零": myXET.E_autozero = byte.Parse(LineToValue(items[i].ToString())[1].Substring(2), System.Globalization.NumberStyles.HexNumber); break;
                        case "零点跟踪": myXET.E_trackzero = byte.Parse(LineToValue(items[i].ToString())[1].Substring(2), System.Globalization.NumberStyles.HexNumber); break;
                        case "按键SPAN锁": myXET.E_enspan = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;

                        case "零点输入": myXET.T_input1 = LineToValue(items[i].ToString())[1]; break;
                        case "满点输入": myXET.T_input5 = LineToValue(items[i].ToString())[1]; break;
                        case "输入1": myXET.T_input1 = LineToValue(items[i].ToString())[1]; break;
                        case "输入2": myXET.T_input2 = LineToValue(items[i].ToString())[1]; break;
                        case "输入3": myXET.T_input3 = LineToValue(items[i].ToString())[1]; break;
                        case "输入4": myXET.T_input4 = LineToValue(items[i].ToString())[1]; break;
                        case "输入5": myXET.T_input5 = LineToValue(items[i].ToString())[1]; break;
                        case "输入6": myXET.T_input6 = LineToValue(items[i].ToString())[1]; break;
                        case "输入7": myXET.T_input7 = LineToValue(items[i].ToString())[1]; break;
                        case "输入8": myXET.T_input8 = LineToValue(items[i].ToString())[1]; break;
                        case "输入9": myXET.T_input9 = LineToValue(items[i].ToString())[1]; break;
                        case "输入10": myXET.T_input10 = LineToValue(items[i].ToString())[1]; break;
                        case "输入11": myXET.T_input11 = LineToValue(items[i].ToString())[1]; break;


                        case "零点输出": myXET.T_analog1 = LineToValue(items[i].ToString())[1]; break;
                        case "满点输出": myXET.T_analog5 = LineToValue(items[i].ToString())[1]; break;
                        case "输出1": myXET.T_analog1 = LineToValue(items[i].ToString())[1]; break;
                        case "输出2": myXET.T_analog2 = LineToValue(items[i].ToString())[1]; break;
                        case "输出3": myXET.T_analog3 = LineToValue(items[i].ToString())[1]; break;
                        case "输出4": myXET.T_analog4 = LineToValue(items[i].ToString())[1]; break;
                        case "输出5": myXET.T_analog5 = LineToValue(items[i].ToString())[1]; break;
                        case "输出6": myXET.T_analog6 = LineToValue(items[i].ToString())[1]; break;
                        case "输出7": myXET.T_analog7 = LineToValue(items[i].ToString())[1]; break;
                        case "输出8": myXET.T_analog8 = LineToValue(items[i].ToString())[1]; break;
                        case "输出9": myXET.T_analog9 = LineToValue(items[i].ToString())[1]; break;
                        case "输出10": myXET.T_analog10 = LineToValue(items[i].ToString())[1]; break;
                        case "输出11": myXET.T_analog11 = LineToValue(items[i].ToString())[1]; break;

                        case "校验方式 ": myXET.E_sign = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "站点地址 ": myXET.E_addr = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "通讯波特率": myXET.E_baud = Convert.ToByte(LineToValue(items[i].ToString())[2]); break;
                        case "通讯停止位": myXET.E_stopbit = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "通讯校验位": myXET.E_parity = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "数字量零点": myXET.T_wt_zero = LineToValue(items[i].ToString())[1]; break;
                        case "数字量满点": myXET.T_wt_full = LineToValue(items[i].ToString())[1]; break;
                        case "数字量小数点": myXET.E_wt_decimal = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "数字量分度值": myXET.E_wt_division = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "数字量单位": myXET.E_wt_unit = Convert.ToByte(LineToValue(items[i].ToString())[2]); break;
                        case "连续发送格式": myXET.E_wt_ascii = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "稳定次数 ": myXET.E_wt_sptime = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "滤波深度 ": myXET.E_wt_spfilt = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "抗振动等级 ": myXET.E_wt_antivib = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "蠕变跟踪 ": myXET.E_dynazero = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "稳定范围": myXET.E_stablerange = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "稳定时间": myXET.E_stabletime = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "零点跟踪时间": myXET.E_tkzerotime = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "动态跟踪时间": myXET.E_tkdynatime = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;

                        case "AI类型": myXET.E_cheatype = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "AI外阈值": myXET.E_thmax = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "AI内阈值": myXET.E_thmin = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;

                        case "心跳时间 ": myXET.E_heartBeat = Convert.ToUInt16(LineToValue(items[i].ToString())[1]); break;
                        case "TPDO0类型": myXET.E_typeTPDO0 = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "TPDO0时间": myXET.E_evenTPDO0 = Convert.ToUInt16(LineToValue(items[i].ToString())[1]); break;
                        case "节点ID": myXET.E_nodeID = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "节点波特率": myXET.E_nodeBaud = Convert.ToByte(LineToValue(items[i].ToString())[2]); break;

                        case "滤波范围 ": myXET.E_filter = Convert.ToInt32(LineToValue(items[i].ToString())[1]); break;

                        case "主机端口号": myXET.E_netServicePort = Convert.ToUInt16(LineToValue(items[i].ToString())[1]); break;
                        case "主机IP地址":
                            byte[] ip = myXET.GetIpAddressFromString(LineToValue(items[i].ToString())[1]);
                            myXET.E_netServiceIP[0] = ip[0];
                            myXET.E_netServiceIP[1] = ip[1];
                            myXET.E_netServiceIP[2] = ip[2];
                            myXET.E_netServiceIP[3] = ip[3];
                            break;
                        case "设备IP地址":
                            byte[] ip2 = myXET.GetIpAddressFromString(LineToValue(items[i].ToString())[1]);
                            myXET.E_netClientIP[0] = ip2[0];
                            myXET.E_netClientIP[1] = ip2[1];
                            myXET.E_netClientIP[2] = ip2[2];
                            myXET.E_netClientIP[3] = ip2[3];
                            break;
                        case "设备网关地址":
                            byte[] ip3 = myXET.GetIpAddressFromString(LineToValue(items[i].ToString())[1]);
                            myXET.E_netGatIP[0] = ip3[0];
                            myXET.E_netGatIP[1] = ip3[1];
                            myXET.E_netGatIP[2] = ip3[2];
                            myXET.E_netGatIP[3] = ip3[3];
                            break;
                        case "设备子网掩码":
                            byte[] ip4 = myXET.GetIpAddressFromString(LineToValue(items[i].ToString())[1]);
                            myXET.E_netMaskIP[0] = ip4[0];
                            myXET.E_netMaskIP[1] = ip4[1];
                            myXET.E_netMaskIP[2] = ip4[2];
                            myXET.E_netMaskIP[3] = ip4[3];
                            break;
                        case "DHCP": myXET.E_useDHCP = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "Scan": myXET.E_useScan = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "RF地址":
                            string[] addr = LineToValue(items[i].ToString())[1].Split(',');
                            myXET.E_addrRF[0] = Byte.Parse(addr[0]);
                            if (addr.Length > 1)
                            {
                                myXET.E_addrRF[1] = Byte.Parse(addr[1]);
                            }
                            break;
                        case "接收器1": myXET.E_spedRF = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "接收器2": myXET.E_chanRF = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                        case "接收器3": myXET.E_optionRF = Convert.ToByte(LineToValue(items[i].ToString())[1]); break;
                    }
                }

            }

            return true;
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string[] LineToValue(String line)
        {
            line = line.Replace(" ", "");
            return line.Split(new char[] { '=', '_', '(', ')', '[', ']' });
        }
    }
}
