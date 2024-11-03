using Model;
using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

//Tong Ziyun 20230410
//Tong Ziyun 20230506
//Lumi 20231222

namespace Base.UI.MenuTool
{
    public partial class MenuToolParExport : Form
    {
        private XET actXET;//需要操作的设备
        private CheckedListBox[] checkedListBoxes = new CheckedListBox[4];//CheckListBox控件数组
        private CheckBox[] checkBoxes = new CheckBox[4];//全选控件数组
        private Boolean change = true;//若是checkedListBox控件的改变导致全选控件的变化，则全选控件事件不执行

        public MenuToolParExport()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 导出设备参数界面加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolParExport_Load(object sender, EventArgs e)
        {
            actXET = MyDevice.actDev;

            //初始化checkedListBoxs数组
            checkedListBoxes[0] = checkedListBox1;
            checkedListBoxes[1] = checkedListBox2;
            checkedListBoxes[2] = checkedListBox3;
            checkedListBoxes[3] = checkedListBox4;

            //初始化CheckBox数组
            checkBoxes[0] = checkBox1;
            checkBoxes[1] = checkBox2;
            checkBoxes[2] = checkBox3;
            checkBoxes[3] = checkBox4;

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
            if (MyDevice.devSum > 1)//多设备
            {
                for (int i = 0; i < 255; i++)
                {
                    switch (MyDevice.protocol.type)
                    {
                        default:
                            break;

                        case COMP.RS485:
                            if (MyDevice.mBUS[i].sTATE == STATE.WORKING)
                            {
                                comboBox1.Items.Add("  RS485_" + i);
                            }
                            break;

                        case COMP.CANopen:
                            if (i > 127) break;
                            if (MyDevice.mCAN[i].sTATE == STATE.WORKING)
                            {
                                comboBox1.Items.Add("  CAN_" + i);
                            }
                            break;

                        case COMP.ModbusTCP:
                            if (MyDevice.mMTCP[i].sTATE == STATE.WORKING)
                            {
                                comboBox1.Items.Add("  ModbusTCP_" + MyDevice.mMTCP[i].E_addr + ":" + MyDevice.mMTCP[i].R_ipAddr);
                            }
                            break;
                    }
                }
                comboBox1.SelectedIndex = 0;
            }
            else if (MyDevice.devSum > 0)//单设备
            {
                if (MyDevice.protocol.type == COMP.CANopen)
                {
                    comboBox1.Items.Add("  " + actXET.E_nodeID);
                }
                else if (MyDevice.protocol.type == COMP.ModbusTCP)
                {
                    comboBox1.Items.Add("  " + actXET.E_addr + ":" + actXET.R_ipAddr);
                }
                else
                {
                    comboBox1.Items.Add("  " + actXET.E_addr);
                }
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = false;
            }
        }

        /// <summary>
        /// 选择不同的设备地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count > 1)
            {
                foreach (string line in checkedListBox1.CheckedItems)
                {
                    switch (LineToValue(line.Trim())[0])
                    {
                        case "RS485":
                            //导入设备参数
                            loadListBox(ReadFromSCT(MyDevice.mBUS[Convert.ToInt32(LineToValue(line.Trim())[1])]));
                            break;
                        case "CAN":
                            //导入设备参数
                            loadListBox(ReadFromSCT(MyDevice.mCAN[Convert.ToInt32(LineToValue(line.Trim())[1])]));
                            break;
                    }
                }
            }
            else if (comboBox1.Items.Count > 0)//单设备
            {
                //导入设备参数
                loadListBox(ReadFromSCT(actXET));
            }
        }

        /// <summary>
        /// 删除按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                string messageStr;
                if (MyDevice.languageType == 0)
                {
                    messageStr = "是否删除选中配置";
                }
                else
                {
                    messageStr = "Whether to remove the selected configuration";
                }
                if (MessageBox.Show(messageStr, "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    String meString = MyDevice.D_cfgPath + "\\" + listBox1.SelectedItem.ToString() + ".txt";
                    if (File.Exists(meString))
                    {
                        File.Delete(meString);
                    }
                    listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                }
            }
        }

        /// <summary>
        /// 保存按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //保存设备名称： 型号.输出.输入的名称_时间（年月日）
            String name = actXET.S_DeviceType + "." + actXET.S_OutType + "." + textBox1.Text + "_" + System.DateTime.Now.ToString("yyyyMMdd");

            //保存设备的路径
            String meString = MyDevice.D_cfgPath + "\\" + name + ".txt";

            //存储listBox2中的数据
            List<String> list = new List<string>();
            foreach (string line in listBox2.Items)
            {
                list.Add(line);
            }

            //覆盖
            if (File.Exists(meString))
            {
                string messageStr;
                if (MyDevice.languageType == 0)
                {
                    messageStr = "是否覆盖已有配置";
                }
                else
                {
                    messageStr = "Whether to override existing configurations";
                }
                if (MessageBox.Show(messageStr, "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    MyDevice.SaveToCfg(actXET, name, list);
                }
            }
            //新增
            else
            {
                MyDevice.SaveToCfg(actXET, name, list);
                //
                listBox1.Items.Add(name);
            }
        }

        /// <summary>
        /// 初始化列表
        /// </summary>
        /// <param name="messageList">设备数据信息</param>
        private void loadListBox(List<List<string>> messageList)
        {
            //初始化checekedListBox列表
            for (int i = 0; i < messageList.Count; i++)
            {
                for (int j = 0; j < messageList[i].Count; j++)
                {
                    checkedListBoxes[i].Items.Add(messageList[i][j]);
                    checkedListBoxes[i].SetItemChecked(j, true);
                    listBox2.Items.Add(messageList[i][j]);
                }
                listBox2.Items.Add("");
            }

            //判断是否显示checkedListBox4
            //BE30AH\BS420H\BS600H\TNP10\T420没有SCT5
            if (messageList[3].Count == 0)
            {
                checkedListBoxes[3].Visible = false;
                checkBoxes[3].Visible = false;
            }

            //checkedListBox绑定事件
            for (int i = 0; i < checkedListBoxes.Length; i++)
            {
                checkedListBoxes[i].SelectedIndexChanged += MenuToolParExport_SelectedIndexChanged;
                checkBoxes[i].CheckedChanged += MenuToolParExport_CheckedChanged;
            }
        }

        /// <summary>
        /// 全选控件check属性更改时事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolParExport_CheckedChanged(object sender, EventArgs e)
        {
            //若点击checkdedListBox更换了checkedBox选中状态，则不执行代码
            if (!change)
            {
                return;
            }

            //获取控件状态
            bool status = ((CheckBox)sender).Checked;

            //获得全选控件在全选控件数组的下标
            int index = Convert.ToInt32(((CheckBox)sender).Name.Last().ToString()) - 1;

            //改变对应list中item的状态
            for (int i = 0; i < checkedListBoxes[index].Items.Count; i++)
            {
                checkedListBoxes[index].SetItemChecked(i, status);
            }

            //跟新列表
            updateListBox();
        }

        /// <summary>
        /// checkedListBox选择变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolParExport_SelectedIndexChanged(object sender, EventArgs e)
        {
            //获取操作的checkedListBox数组的下标
            int index = Convert.ToInt32(((CheckedListBox)sender).Name.Last().ToString()) - 1;

            //将操作的checkedListBox对应的全选勾选框置为false,但不执行属性更改事件
            change = false;
            checkBoxes[index].Checked = false;
            //保证下一次全选按钮变化事件的执行
            change = true;

            //更新列表
            updateListBox();
        }

        /// <summary>
        /// 更新列表
        /// </summary>
        private void updateListBox()
        {
            //型号和配置必选
            checkedListBoxes[0].SetItemChecked(0, true);
            checkedListBoxes[0].SetItemChecked(1, true);

            //更新list表格
            listBox2.Items.Clear();
            for (int i = 0; i < checkedListBoxes.Length; i++)
            {
                //如果checkedListBox列表未选择，则返回，防止打印多余的空行
                if (checkedListBoxes[i].CheckedItems.Count == 0)
                {
                    continue;
                }
                for (int j = 0; j < checkedListBoxes[i].CheckedItems.Count; j++)
                {
                    listBox2.Items.Add(checkedListBoxes[i].CheckedItems[j].ToString());
                }
                listBox2.Items.Add("");
            }
        }

        /// <summary>
        /// 读出选中设备数据
        /// </summary>
        /// <param name="myXET"></param>
        /// <returns>返回数据列表</returns>
        public static List<List<string>> ReadFromSCT(XET myXET)
        {
            List<List<string>> message = new List<List<string>>();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();

            list1.Add("型号         = " + myXET.S_DeviceType);
            list1.Add("配置         = " + myXET.S_OutType);
            list1.Add("采样参数     = 0x" + myXET.E_adspeed.ToString("X2"));
            list1.Add("上电归零     = 0x" + myXET.E_autozero.ToString("X2"));
            list1.Add("零点跟踪     = 0x" + myXET.E_trackzero.ToString("X2"));
            list1.Add("按键SPAN锁   = " + myXET.E_enspan);

            switch ((ECVE)myXET.E_curve)
            {
                case ECVE.CTWOPT:
                    list2.Add("零点输入     = " + myXET.T_input1);
                    list2.Add("满点输入     = " + myXET.T_input5);

                    list3.Add("零点输出     = " + myXET.T_analog1);
                    list3.Add("满点输出     = " + myXET.T_analog5);
                    break;
                case ECVE.CFITED:
                case ECVE.CINTER:
                    list2.Add("输入1        = " + myXET.T_input1);
                    list2.Add("输入2        = " + myXET.T_input2);
                    list2.Add("输入3        = " + myXET.T_input3);
                    list2.Add("输入4        = " + myXET.T_input4);
                    list2.Add("输入5        = " + myXET.T_input5);

                    list3.Add("输出1        = " + myXET.T_analog1);
                    list3.Add("输出2        = " + myXET.T_analog2);
                    list3.Add("输出3        = " + myXET.T_analog3);
                    list3.Add("输出4        = " + myXET.T_analog4);
                    list3.Add("输出5        = " + myXET.T_analog5);
                    break;
                case ECVE.CELTED:
                case ECVE.CELTER:
                    list2.Add("输入1        = " + myXET.T_input1);
                    list2.Add("输入2        = " + myXET.T_input2);
                    list2.Add("输入3        = " + myXET.T_input3);
                    list2.Add("输入4        = " + myXET.T_input4);
                    list2.Add("输入5        = " + myXET.T_input5);
                    list2.Add("输入6        = " + myXET.T_input6);
                    list2.Add("输入7        = " + myXET.T_input7);
                    list2.Add("输入8        = " + myXET.T_input8);
                    list2.Add("输入9        = " + myXET.T_input9);
                    list2.Add("输入10       = " + myXET.T_input10);
                    list2.Add("输入11       = " + myXET.T_input11);

                    list3.Add("输出1        = " + myXET.T_analog1);
                    list3.Add("输出2        = " + myXET.T_analog2);
                    list3.Add("输出3        = " + myXET.T_analog3);
                    list3.Add("输出4        = " + myXET.T_analog4);
                    list3.Add("输出5        = " + myXET.T_analog5);
                    list3.Add("输出6        = " + myXET.T_analog6);
                    list3.Add("输出7        = " + myXET.T_analog7);
                    list3.Add("输出8        = " + myXET.T_analog8);
                    list3.Add("输出9        = " + myXET.T_analog9);
                    list3.Add("输出10       = " + myXET.T_analog10);
                    list3.Add("输出11       = " + myXET.T_analog11);
                    break;
            }

            //特殊菜单根据产品型号变
            switch (myXET.S_DeviceType)
            {
                default:
                    break;

                case TYPE.TDES:
                case TYPE.TDSS:
                case TYPE.TD485:
                    //list4.Add("校验方式     = " + myXET.E_sign.ToString());//未开放
                    list4.Add("站点地址     = " + myXET.E_addr.ToString());
                    switch (myXET.E_baud)
                    {
                        case 0: list4.Add("通讯波特率   = 1200" + "(" + myXET.E_baud + ")"); break;
                        case 1: list4.Add("通讯波特率   = 2400" + "(" + myXET.E_baud + ")"); break;
                        case 2: list4.Add("通讯波特率   = 4800" + "(" + myXET.E_baud + ")"); break;
                        case 3: list4.Add("通讯波特率   = 9600" + "(" + myXET.E_baud + ")"); break;
                        case 4: list4.Add("通讯波特率   = 14400" + "(" + myXET.E_baud + ")"); break;
                        case 5: list4.Add("通讯波特率   = 19200" + "(" + myXET.E_baud + ")"); break;
                        case 6: list4.Add("通讯波特率   = 38400" + "(" + myXET.E_baud + ")"); break;
                        case 7: list4.Add("通讯波特率   = 57600" + "(" + myXET.E_baud + ")"); break;
                        case 8: list4.Add("通讯波特率   = 115200" + "(" + myXET.E_baud + ")"); break;
                        case 9: list4.Add("通讯波特率   = 230400" + "(" + myXET.E_baud + ")"); break;
                        case 10: list4.Add("通讯波特率   = 256000" + "(" + myXET.E_baud + ")"); break;
                    }
                    list4.Add("通讯停止位   = " + myXET.E_stopbit.ToString());
                    list4.Add("通讯校验位   = " + myXET.E_parity.ToString());

                    list1.Add("数字量零点   = " + myXET.T_wt_zero);
                    list1.Add("数字量满点   = " + myXET.T_wt_full);
                    list1.Add("数字量小数点 = " + myXET.E_wt_decimal.ToString());
                    list1.Add("数字量单位   = " + ((UNIT)myXET.E_wt_unit).ToString() + "(" + myXET.E_wt_unit + ")");
                    list1.Add("连续发送格式 = " + myXET.E_wt_ascii.ToString());
                    break;

                case TYPE.TCAN:
                    list1.Add("数字量零点   = " + myXET.T_wt_zero);
                    list1.Add("数字量满点   = " + myXET.T_wt_full);
                    list1.Add("数字量小数点 = " + myXET.E_wt_decimal.ToString());
                    list1.Add("数字量分度值 = " + myXET.E_wt_division);
                    list1.Add("数字量单位   = " + ((UNIT)myXET.E_wt_unit).ToString() + "(" + myXET.E_wt_unit + ")");

                    list4.Add("心跳时间     = " + myXET.E_heartBeat.ToString());
                    list4.Add("TPDO0类型    = " + myXET.E_typeTPDO0.ToString());
                    list4.Add("TPDO0时间    = " + myXET.E_evenTPDO0.ToString());
                    list4.Add("节点ID       = " + myXET.E_nodeID.ToString());
                    switch (myXET.E_nodeBaud)
                    {
                        case 0: list4.Add("节点波特率   = 10 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 1: list4.Add("节点波特率   = 20 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 2: list4.Add("节点波特率   = 50 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 3: list4.Add("节点波特率   = 125 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 4: list4.Add("节点波特率   = 250 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 5: list4.Add("节点波特率   = 500 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 6: list4.Add("节点波特率   = 800 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                        case 7: list4.Add("节点波特率   = 1000 kbps" + "(" + myXET.E_nodeBaud + ")"); break;
                    }
                    break;

                case TYPE.iBus:
                    list4.Add("校验方式     = " + myXET.E_sign.ToString());
                    list4.Add("站点地址     = " + myXET.E_addr.ToString());
                    switch (myXET.E_baud)
                    {
                        case 0: list4.Add("通讯波特率   = 1200" + "(" + myXET.E_baud + ")"); break;
                        case 1: list4.Add("通讯波特率   = 2400" + "(" + myXET.E_baud + ")"); break;
                        case 2: list4.Add("通讯波特率   = 4800" + "(" + myXET.E_baud + ")"); break;
                        case 3: list4.Add("通讯波特率   = 9600" + "(" + myXET.E_baud + ")"); break;
                        case 4: list4.Add("通讯波特率   = 14400" + "(" + myXET.E_baud + ")"); break;
                        case 5: list4.Add("通讯波特率   = 19200" + "(" + myXET.E_baud + ")"); break;
                        case 6: list4.Add("通讯波特率   = 38400" + "(" + myXET.E_baud + ")"); break;
                        case 7: list4.Add("通讯波特率   = 57600" + "(" + myXET.E_baud + ")"); break;
                        case 8: list4.Add("通讯波特率   = 115200" + "(" + myXET.E_baud + ")"); break;
                        case 9: list4.Add("通讯波特率   = 230400" + "(" + myXET.E_baud + ")"); break;
                        case 10: list4.Add("通讯波特率   = 256000" + "(" + myXET.E_baud + ")"); break;
                    }
                    list4.Add("通讯停止位   = " + myXET.E_stopbit.ToString());
                    list4.Add("通讯校验位   = " + myXET.E_parity.ToString());

                    list1.Add("数字量零点   = " + myXET.T_wt_zero);
                    list1.Add("数字量满点   = " + myXET.T_wt_full);
                    list1.Add("数字量小数点 = " + myXET.E_wt_decimal.ToString());
                    list1.Add("数字量分度值 = " + myXET.E_wt_division);
                    list1.Add("数字量单位   = " + ((UNIT)myXET.E_wt_unit).ToString() + "(" + myXET.E_wt_unit + ")");
                    list1.Add("连续发送格式 = " + myXET.E_wt_ascii.ToString());
                    list1.Add("稳定次数     = " + myXET.E_wt_sptime.ToString());
                    list1.Add("滤波深度     = " + myXET.E_wt_spfilt.ToString());
                    list1.Add("抗振动等级   = " + myXET.E_wt_antivib.ToString());
                    list1.Add("蠕变跟踪     = " + myXET.E_dynazero.ToString());
                    list1.Add("稳定范围     = " + myXET.E_stablerange.ToString());
                    list1.Add("稳定时间     = " + myXET.E_stabletime.ToString());
                    list1.Add("零点跟踪时间 = " + myXET.E_tkzerotime.ToString());
                    list1.Add("动态跟踪时间 = " + myXET.E_tkdynatime.ToString());
                    list1.Add("AI类型       = " + myXET.E_cheatype.ToString());
                    list1.Add("AI外阈值     = " + myXET.E_thmax.ToString());
                    list1.Add("AI内阈值     = " + myXET.E_thmin.ToString());
                    list1.Add("滤波范围     = " + myXET.E_filter.ToString());
                    break;

                case TYPE.iNet:
                    list4.Add("校验方式     = " + myXET.E_sign.ToString());
                    list4.Add("站点地址     = " + myXET.E_addr.ToString());
                    switch (myXET.E_baud)
                    {
                        case 0: list4.Add("通讯波特率   = 1200" + "(" + myXET.E_baud + ")"); break;
                        case 1: list4.Add("通讯波特率   = 2400" + "(" + myXET.E_baud + ")"); break;
                        case 2: list4.Add("通讯波特率   = 4800" + "(" + myXET.E_baud + ")"); break;
                        case 3: list4.Add("通讯波特率   = 9600" + "(" + myXET.E_baud + ")"); break;
                        case 4: list4.Add("通讯波特率   = 14400" + "(" + myXET.E_baud + ")"); break;
                        case 5: list4.Add("通讯波特率   = 19200" + "(" + myXET.E_baud + ")"); break;
                        case 6: list4.Add("通讯波特率   = 38400" + "(" + myXET.E_baud + ")"); break;
                        case 7: list4.Add("通讯波特率   = 57600" + "(" + myXET.E_baud + ")"); break;
                        case 8: list4.Add("通讯波特率   = 115200" + "(" + myXET.E_baud + ")"); break;
                        case 9: list4.Add("通讯波特率   = 230400" + "(" + myXET.E_baud + ")"); break;
                        case 10: list4.Add("通讯波特率   = 256000" + "(" + myXET.E_baud + ")"); break;
                    }
                    list4.Add("通讯停止位   = " + myXET.E_stopbit.ToString());
                    list4.Add("通讯校验位   = " + myXET.E_parity.ToString());

                    list1.Add("数字量零点   = " + myXET.T_wt_zero);
                    list1.Add("数字量满点   = " + myXET.T_wt_full);
                    list1.Add("数字量小数点 = " + myXET.E_wt_decimal.ToString());
                    list1.Add("数字量分度值 = " + myXET.E_wt_division);
                    list1.Add("数字量单位   = " + ((UNIT)myXET.E_wt_unit).ToString() + "(" + myXET.E_wt_unit + ")");
                    list1.Add("连续发送格式 = " + myXET.E_wt_ascii.ToString());
                    list1.Add("稳定次数     = " + myXET.E_wt_sptime.ToString());
                    list1.Add("滤波深度     = " + myXET.E_wt_spfilt.ToString());
                    list1.Add("抗振动等级   = " + myXET.E_wt_antivib.ToString());
                    list1.Add("蠕变跟踪     = " + myXET.E_dynazero.ToString());
                    list1.Add("稳定时间     = " + myXET.E_stabletime.ToString());
                    list1.Add("零点跟踪时间 = " + myXET.E_tkzerotime.ToString());
                    list1.Add("动态跟踪时间 = " + myXET.E_tkdynatime.ToString());
                    list1.Add("AI类型       = " + myXET.E_cheatype.ToString());
                    list1.Add("AI外阈值     = " + myXET.E_thmax.ToString());
                    list1.Add("AI内阈值     = " + myXET.E_thmin.ToString());
                    list1.Add("滤波范围     = " + myXET.E_filter.ToString());
                    list1.Add("主机端口号   = " + myXET.E_netServicePort.ToString());
                    list1.Add("主机IP地址   = " + myXET.GetIpAddressFromArray(myXET.E_netServiceIP));
                    list1.Add("设备IP地址   = " + myXET.GetIpAddressFromArray(myXET.E_netClientIP));
                    list1.Add("设备网关地址 = " + myXET.GetIpAddressFromArray(myXET.E_netGatIP));
                    list1.Add("设备子网掩码 = " + myXET.GetIpAddressFromArray(myXET.E_netMaskIP));
                    list1.Add("DHCP         = " + myXET.E_useDHCP.ToString());
                    list1.Add("Scan         = " + myXET.E_useScan.ToString());
                    break;

                case TYPE.iStar:
                    list4.Add("校验方式     = " + myXET.E_sign.ToString());
                    list4.Add("站点地址     = " + myXET.E_addr.ToString());
                    switch (myXET.E_baud)
                    {
                        case 0: list4.Add("通讯波特率   = 1200" + "(" + myXET.E_baud + ")"); break;
                        case 1: list4.Add("通讯波特率   = 2400" + "(" + myXET.E_baud + ")"); break;
                        case 2: list4.Add("通讯波特率   = 4800" + "(" + myXET.E_baud + ")"); break;
                        case 3: list4.Add("通讯波特率   = 9600" + "(" + myXET.E_baud + ")"); break;
                        case 4: list4.Add("通讯波特率   = 14400" + "(" + myXET.E_baud + ")"); break;
                        case 5: list4.Add("通讯波特率   = 19200" + "(" + myXET.E_baud + ")"); break;
                        case 6: list4.Add("通讯波特率   = 38400" + "(" + myXET.E_baud + ")"); break;
                        case 7: list4.Add("通讯波特率   = 57600" + "(" + myXET.E_baud + ")"); break;
                        case 8: list4.Add("通讯波特率   = 115200" + "(" + myXET.E_baud + ")"); break;
                        case 9: list4.Add("通讯波特率   = 230400" + "(" + myXET.E_baud + ")"); break;
                        case 10: list4.Add("通讯波特率   = 256000" + "(" + myXET.E_baud + ")"); break;
                    }
                    list4.Add("通讯停止位   = " + myXET.E_stopbit.ToString());
                    list4.Add("通讯校验位   = " + myXET.E_parity.ToString());

                    list1.Add("数字量零点   = " + myXET.T_wt_zero);
                    list1.Add("数字量满点   = " + myXET.T_wt_full);
                    list1.Add("数字量小数点 = " + myXET.E_wt_decimal.ToString());
                    list1.Add("数字量分度值 = " + myXET.E_wt_division);
                    list1.Add("数字量单位   = " + ((UNIT)myXET.E_wt_unit).ToString() + "(" + myXET.E_wt_unit + ")");
                    list1.Add("连续发送格式 = " + myXET.E_wt_ascii.ToString());
                    list1.Add("稳定次数     = " + myXET.E_wt_sptime.ToString());
                    list1.Add("滤波深度     = " + myXET.E_wt_spfilt.ToString());
                    list1.Add("抗振动等级   = " + myXET.E_wt_antivib.ToString());
                    list1.Add("蠕变跟踪     = " + myXET.E_dynazero.ToString());
                    list1.Add("稳定时间     = " + myXET.E_stabletime.ToString());
                    list1.Add("零点跟踪时间 = " + myXET.E_tkzerotime.ToString());
                    list1.Add("动态跟踪时间 = " + myXET.E_tkdynatime.ToString());
                    list1.Add("AI类型       = " + myXET.E_cheatype.ToString());
                    list1.Add("AI外阈值     = " + myXET.E_thmax.ToString());
                    list1.Add("AI内阈值     = " + myXET.E_thmin.ToString());
                    list1.Add("滤波范围     = " + myXET.E_filter.ToString());
                    list1.Add("RF地址       = " + myXET.E_addrRF[0].ToString() + "," + myXET.E_addrRF[1].ToString());
                    list1.Add("接收器1      = " + myXET.E_spedRF.ToString());
                    list1.Add("接收器2      = " + myXET.E_chanRF.ToString());
                    list1.Add("接收器3      = " + myXET.E_optionRF.ToString());

                    break;
            }

            message.Add(list1);
            message.Add(list2);
            message.Add(list3);
            message.Add(list4);
            return message;
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string[] LineToValue(String line)
        {
            line = line.Replace(" ", "");
            return line.Split(new char[] { '=', '_', '(', ')' });
        }
    }
}
