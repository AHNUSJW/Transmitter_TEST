using Library;
using RecXF;
using LicenseActivation;
using Base.UI;
using Base.UI.MenuFile;
using Base.UI.MenuHelp;
using Base.UI.MenuSet;
using Base.UI.MenuSetCommunication;
using Base.UI.MenuTool;
using Base.UI.MenuToolConfig;
using Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

//Alvin 20230414
//Junzhe 20230616
//Lumi 20240606

//UI界面主要任务

//界面:固定内容调整根据:账户权限,设备型号,设备模拟量输出类型,通讯接口类型
//界面:动态内容调整根据:状态,用户,定时,通讯

//状态:管理窗口
//用户:按钮,文本输入,鼠标点击等操作事件处理
//定时:当前窗口的定时任务处理,比如发送净毛重指令,监控状态
//通讯:串口事件解码后的委托处理

//MeasureDevice 单设备
//MultipleDevice 多设备
//MT2X420Device 特殊设备 T2X420

//SCT数据表更新时同时检查显示界面,要求是方便用户使用的带小数点带单位数据
//MeasureDevice.cs的update_ListTextChart里,中英文两处
//MenuSetCalForm.cs的paralist_Checking里,超级普通账户两处

//SCT数据表更新时同时检查保存到硬盘的log,要求是保留原始数据
//MyDevice.cs的SaveToLog里,超级普通账户两处

namespace Base
{
    public partial class Main : Form
    {
        private XET actXET;//需要操作的设备

        private AutoFormSize autoFormSize = new AutoFormSize();//依据分辨率调整窗口

        private static String activeForm = "";//激活的窗口

        private Boolean isActivated = true;//证书激活状态

        private String myFormText = "MaciX";//窗口标题

        public delegate void showMeasure();//单元设备显示窗口委托

        public static Boolean isMeasure = false;//窗口显示控制位

        public static event showMeasure myMeasure;//单元设备显示窗口事件

        public static String selectMeasure = "";//主界面选择毛重净重模拟量

        public static Boolean isConnected = false;//连接完成才能继续操作

        public static MainUser LicenseForm = new MainUser();

        //获取"激活的窗口"字段
        public static new String ActiveForm
        {
            set
            {
                activeForm = value;
            }
            get
            {
                return activeForm;
            }
        }

        //获取毛重净重选择
        public static String SelectMeasure
        {
            set
            {
                selectMeasure = value;
            }
            get
            {
                return selectMeasure;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Main()
        {
            InitializeComponent();

            //初始化激活
            LicenseForm.RegisterSoftwareName = "MaciX";
            // 获取当前日期
            DateTime currentDate = DateTime.Now;
            DateTime targetDate = new DateTime(2025, 5, 11);
            DateTime endDate = new DateTime(2025, 5, 31);

            if (currentDate < targetDate)
            {
                LicenseForm.TrialDaysTotal = (endDate - currentDate).Days;
            }
            else
            {
                LicenseForm.TrialDaysTotal = 20;
            }

            if (LicenseForm.TrialDaysTotal > 254)
            {
                LicenseForm.TrialDaysTotal = 20;
            }
        }

        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Load(object sender, EventArgs e)
        {
            activeForm = "Main";

            //检查激活
            CheckLicense();

            //图标加载
            string logoIconPath = Path.Combine(Application.StartupPath, "pic", "logo.ico");
            if (File.Exists(logoIconPath))
            {
                this.Icon = new Icon(logoIconPath);
            }
            else
            {
                this.Icon = Properties.Resources.BCS16A;
            }

            //窗口标题加载
            Load_FormText();

            actXET = MyDevice.actDev;

            TSMenuItemVisible();
            TSMenuItemEnable();

            autoFormSize.UIComponetForm(this);
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关闭串口
            if (MyDevice.protocol.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocol.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocol.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                MyDevice.protocol.Protocol_PortClose();
            }

            //关闭串口
            if (MyDevice.protocos.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocos.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocos.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                MyDevice.protocos.Protocol_PortClose();
            }
        }

        /// <summary>
        /// 界面是否可见
        /// </summary>
        private void TSMenuItemVisible()
        {
            //关于菜单隐藏: 超级用户才能见到工厂校准和AI跟踪器设置, 普通用户隐藏菜单
            //关于菜单禁用: 根据产品型号和输出类型, 灰色禁用相关功能, 但是不隐藏菜单
            //bohr账户 所有的菜单 可见 可用

            //隐藏菜单
            if ((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            {
                SetCheatingToolStripMenuItem.Visible = true;//只有iBus有Cheating
                SetFactoryToolStripMenuItem.Visible = true;
                SetFacManualToolStripMenuItem.Visible = true;
                SetFacAutoToolStripMenuItem.Visible = true;
            }
            else if ((MyDevice.D_username == "fac") && (MyDevice.D_password == "woli") || (MyDevice.D_username == "fac2") && (MyDevice.D_password == "123456"))
            {
                switch (actXET.S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH:
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.T420:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                    case TYPE.TDES:
                    case TYPE.TDSS:
                    case TYPE.T4X600H:
                        SetCheatingToolStripMenuItem.Visible = false;
                        SetFactoryToolStripMenuItem.Visible = true;//模拟量有工厂校准
                        SetFacManualToolStripMenuItem.Visible = false;
                        SetFacAutoToolStripMenuItem.Visible = false;
                        break;

                    case TYPE.TD485:
                    case TYPE.TCAN:
                        SetCheatingToolStripMenuItem.Visible = false;
                        SetFactoryToolStripMenuItem.Visible = false;
                        SetFacManualToolStripMenuItem.Visible = false;
                        SetFacAutoToolStripMenuItem.Visible = false;
                        break;

                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        SetCheatingToolStripMenuItem.Visible = true;//只有iBus有Cheating
                        SetFactoryToolStripMenuItem.Visible = false;
                        SetFacManualToolStripMenuItem.Visible = false;
                        SetFacAutoToolStripMenuItem.Visible = false;
                        break;
                }
            }
            else if ((MyDevice.D_username == "admin") && (MyDevice.D_password == "123456"))
            {
                switch (actXET.S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH:
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.T420:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                    case TYPE.TDES:
                    case TYPE.TDSS:
                    case TYPE.T4X600H:
                        SetCheatingToolStripMenuItem.Visible = false;
                        SetFactoryToolStripMenuItem.Visible = true;//模拟量有工厂校准
                        SetFacManualToolStripMenuItem.Visible = false;
                        SetFacAutoToolStripMenuItem.Visible = false;
                        break;

                    case TYPE.TD485:
                    case TYPE.TCAN:
                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        SetCheatingToolStripMenuItem.Visible = false;//只有iBus有Cheating
                        SetFactoryToolStripMenuItem.Visible = false;
                        SetFacManualToolStripMenuItem.Visible = false;
                        SetFacAutoToolStripMenuItem.Visible = false;
                        break;
                }
            }
            else
            {
                SetCheatingToolStripMenuItem.Visible = false;
                SetFactoryToolStripMenuItem.Visible = false;
                SetFacManualToolStripMenuItem.Visible = false;
                SetFacAutoToolStripMenuItem.Visible = false;
            }
        }

        /// <summary>
        /// 界面是否启用
        /// </summary>
        private void TSMenuItemEnable()
        {
            //关于菜单隐藏: 超级用户才能见到工厂校准和AI跟踪器设置, 普通用户隐藏菜单
            //关于菜单禁用: 根据产品型号和输出类型, 灰色禁用相关功能, 但是不隐藏菜单
            //bohr账户 所有的菜单 可见 可用

            //禁用菜单
            //软件激活
            foreach (ToolStripMenuItem menuItem in menuStrip1.Items)
            {
                // 检查菜单项的名称
                if (menuItem.Text == "设置" || menuItem.Text == "工具")
                {
                    // 遍历子项并设置Enabled属性为false
                    foreach (ToolStripMenuItem subItem in menuItem.DropDownItems)
                    {
                        subItem.Enabled = isActivated;
                    }
                }
            }

            if (MyDevice.devSum > 0)
            {
                //基础菜单
                SetParameterToolStripMenuItem.Enabled = true;//参数
                SetCalibrationToolStripMenuItem.Enabled = true;//标定
                SetCorrectionToolStripMenuItem.Enabled = true;//修正

                //特殊菜单根据产品型号变
                switch (actXET.S_DeviceType)
                {
                    default:
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.T420:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                    case TYPE.T4X600H:
                        SetCheatingToolStripMenuItem.Enabled = false;
                        SetRS485ToolStripMenuItem.Enabled = false;
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = false;
                        SetFactoryToolStripMenuItem.Enabled = true;//模拟量才使能工厂校准
                        SetFacAutoToolStripMenuItem.Enabled = true;//模拟量才使能手动校准
                        SetFacManualToolStripMenuItem.Enabled = true;//模拟量才使能自动校准
                        break;

                    case TYPE.BE30AH:
                    case TYPE.TDES:
                    case TYPE.TDSS:
                        SetCheatingToolStripMenuItem.Enabled = false;
                        SetRS485ToolStripMenuItem.Enabled = true;//有RS485
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = false;
                        SetFactoryToolStripMenuItem.Enabled = true;//模拟量才使能工厂校准
                        SetFacAutoToolStripMenuItem.Enabled = true;//模拟量才使能手动校准
                        SetFacManualToolStripMenuItem.Enabled = true;//模拟量才使能自动校准
                        break;

                    case TYPE.TD485:
                        SetCheatingToolStripMenuItem.Enabled = false;
                        SetRS485ToolStripMenuItem.Enabled = true;//有RS485
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = false;
                        SetFactoryToolStripMenuItem.Enabled = false;
                        SetFacAutoToolStripMenuItem.Enabled = false;
                        SetFacManualToolStripMenuItem.Enabled = false;
                        break;

                    case TYPE.TCAN:
                        SetCheatingToolStripMenuItem.Enabled = false;
                        SetRS485ToolStripMenuItem.Enabled = false;
                        SetCANopenToolStripMenuItem.Enabled = true;//有CANopen
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = false;
                        SetFactoryToolStripMenuItem.Enabled = false;
                        SetFacAutoToolStripMenuItem.Enabled = false;
                        SetFacManualToolStripMenuItem.Enabled = false;
                        break;

                    case TYPE.iBus:
                        SetCheatingToolStripMenuItem.Enabled = true;//iBus才使能AI跟踪器
                        SetRS485ToolStripMenuItem.Enabled = true;//有RS485
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = true;//iBus使能滤波范围
                        SetFactoryToolStripMenuItem.Enabled = false;
                        SetFacAutoToolStripMenuItem.Enabled = false;
                        SetFacManualToolStripMenuItem.Enabled = false;
                        break;

                    case TYPE.iNet:
                        SetCheatingToolStripMenuItem.Enabled = true;//iBus才使能AI跟踪器
                        SetRS485ToolStripMenuItem.Enabled = true;//有RS485
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = true;//有ModbusTCP
                        SetWirelessToolStripMenuItem.Enabled = false;
                        SetCalFiltToolStripMenuItem.Enabled = true;//iNet使能滤波范围
                        SetFactoryToolStripMenuItem.Enabled = false;
                        SetFacAutoToolStripMenuItem.Enabled = false;
                        SetFacManualToolStripMenuItem.Enabled = false;
                        break;

                    case TYPE.iStar:
                        SetCheatingToolStripMenuItem.Enabled = true;//iBus才使能AI跟踪器
                        SetRS485ToolStripMenuItem.Enabled = true;//有RS485
                        SetCANopenToolStripMenuItem.Enabled = false;
                        SetModbusTCPToolStripMenuItem.Enabled = false;
                        SetWirelessToolStripMenuItem.Enabled = true;//有wireless
                        SetCalFiltToolStripMenuItem.Enabled = true;//iStar使能滤波范围
                        SetFactoryToolStripMenuItem.Enabled = false;
                        SetFacAutoToolStripMenuItem.Enabled = false;
                        SetFacManualToolStripMenuItem.Enabled = false;
                        break;
                }

                //配置
                ToolParImportToolStripMenuItem.Enabled = true;
                ToolParExportToolStripMenuItem.Enabled = true;
                switch (actXET.S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH:
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                    case TYPE.BS600H:
                    case TYPE.T420:
                    case TYPE.TNP10:
                    case TYPE.TP10:
                    case TYPE.T4X600H:
                    case TYPE.TD485:
                    case TYPE.TCAN:
                    case TYPE.iBus:
                    case TYPE.iNet:
                    case TYPE.iStar:
                        ToolEepromToolStripMenuItem.Enabled = false;
                        break;

                    case TYPE.TDES:
                    case TYPE.TDSS:
                        ToolEepromToolStripMenuItem.Enabled = true;
                        break;
                }

                //调试工具跟着接口变
                switch (MyDevice.protocol.type)
                {
                    default:
                    case COMP.SelfUART:
                        ToolRS485ToolStripMenuItem.Enabled = false;
                        ToolCANopenToolStripMenuItem.Enabled = false;
                        ToolReceiverToolStripMenuItem.Enabled = false;
                        break;

                    case COMP.RS485:
                        ToolRS485ToolStripMenuItem.Enabled = true;
                        ToolCANopenToolStripMenuItem.Enabled = false;
                        ToolReceiverToolStripMenuItem.Enabled = true;
                        break;

                    case COMP.CANopen:
                        ToolRS485ToolStripMenuItem.Enabled = false;
                        ToolCANopenToolStripMenuItem.Enabled = true;
                        ToolReceiverToolStripMenuItem.Enabled = false;
                        break;

                    case COMP.ModbusTCP:
                        ToolRS485ToolStripMenuItem.Enabled = false;
                        ToolCANopenToolStripMenuItem.Enabled = false;
                        ToolReceiverToolStripMenuItem.Enabled = false;
                        break;
                }
                switch (actXET.S_DeviceType)
                {
                    default:
                        break;

                    case TYPE.T8X420H:
                        ToolRS485ToolStripMenuItem.Enabled = false;
                        break;
                }

                //数据
                ToolLogToolStripMenuItem.Enabled = true;
                ToolDataToolStripMenuItem.Enabled = true;
            }
            else if ((MyDevice.D_username == "bohr") && (MyDevice.D_password == "bmc"))
            {
                //设置
                SetParameterToolStripMenuItem.Enabled = true;
                SetCheatingToolStripMenuItem.Enabled = true;
                SetRS485ToolStripMenuItem.Enabled = true;
                SetCANopenToolStripMenuItem.Enabled = true;
                SetModbusTCPToolStripMenuItem.Enabled = true;
                SetWirelessToolStripMenuItem.Enabled = true;

                //标定
                SetCalibrationToolStripMenuItem.Enabled = true;
                SetCorrectionToolStripMenuItem.Enabled = true;
                SetCalFiltToolStripMenuItem.Enabled = true;

                //工厂
                SetFactoryToolStripMenuItem.Enabled = true;
                SetFacAutoToolStripMenuItem.Enabled = true;
                SetFacManualToolStripMenuItem.Enabled = true;

                //配置
                ToolParImportToolStripMenuItem.Enabled = true;
                ToolParExportToolStripMenuItem.Enabled = true;

                //工具
                ToolEepromToolStripMenuItem.Enabled = true;
                ToolRS485ToolStripMenuItem.Enabled = true;
                ToolCANopenToolStripMenuItem.Enabled = true;
                ToolReceiverToolStripMenuItem.Enabled = true;

                //数据
                ToolLogToolStripMenuItem.Enabled = true;
                ToolDataToolStripMenuItem.Enabled = true;
            }
            else
            {
                //设置
                SetParameterToolStripMenuItem.Enabled = false;
                SetCheatingToolStripMenuItem.Enabled = false;
                SetRS485ToolStripMenuItem.Enabled = false;
                SetCANopenToolStripMenuItem.Enabled = false;
                SetModbusTCPToolStripMenuItem.Enabled = false;
                SetWirelessToolStripMenuItem.Enabled = false;

                //标定
                SetCalibrationToolStripMenuItem.Enabled = false;
                SetCorrectionToolStripMenuItem.Enabled = false;
                SetCalFiltToolStripMenuItem.Enabled = false;

                //工厂
                SetFactoryToolStripMenuItem.Enabled = false;
                SetFacAutoToolStripMenuItem.Enabled = false;
                SetFacManualToolStripMenuItem.Enabled = false;

                //配置
                ToolParImportToolStripMenuItem.Enabled = false;
                ToolParExportToolStripMenuItem.Enabled = false;

                //工具
                ToolEepromToolStripMenuItem.Enabled = false;
                ToolRS485ToolStripMenuItem.Enabled = true;
                ToolCANopenToolStripMenuItem.Enabled = true;
                ToolReceiverToolStripMenuItem.Enabled = true;

                //数据
                ToolLogToolStripMenuItem.Enabled = true;
                ToolDataToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// 应用资源
        /// </summary>
        /// <param name>的第一个参数为要设置的控件</param>
        /// <param name>第二个参数为在资源文件中的ID，默认为控件的名称</param>
        private void ApplyResource()
        {
            SuspendLayout();// SuspendLayout()是临时挂起控件的布局逻辑（msdn）
            ComponentResourceManager res = new ComponentResourceManager(typeof(Main));
            foreach (Control ctl in Controls)
            {
                if (ctl == menuStrip1)
                {
                    foreach (ToolStripMenuItem ctl2 in menuStrip1.Items)
                    {
                        res.ApplyResources(ctl2, ctl2.Name);
                        foreach (ToolStripMenuItem ctl3 in ctl2.DropDownItems)
                        {
                            res.ApplyResources(ctl3, ctl3.Name);
                            if (ctl3.DropDownItems.Count > 0)
                            {
                                foreach (ToolStripMenuItem ctl4 in ctl3.DropDownItems)
                                {
                                    res.ApplyResources(ctl4, ctl4.Name);
                                }
                            }
                        }
                    }
                }
                else
                {
                    res.ApplyResources(ctl, ctl.Name);
                }
            }
            //res.ApplyResources(this.ChineseToolStripMenuItem, "ChineseToolStripMenuItem");
            //res.ApplyResources(this.EnglishToolStripMenuItem, "EnglishToolStripMenuItem");
            this.ResumeLayout(false);
            this.PerformLayout();
            Load_FormText();
            res.ApplyResources(this, "$this");
            SuspendLayout();
        }

        /// <summary>
        /// 读取窗口标题Text
        /// </summary>
        private void Load_FormText()
        {
            try
            {
                //读取文件
                string logoIconPath = Path.Combine(Application.StartupPath, "pic", "FormText.ini");
                if (File.Exists(logoIconPath))
                {
                    String[] meLines = File.ReadAllLines(logoIconPath);
                    if (meLines.Length > 0)
                    {
                        myFormText = meLines[0].Split('=')[1];
                    }
                    else
                    {
                        myFormText = "MaciX";
                    }
                }
                else
                {
                    myFormText = "MaciX";
                }
            }
            catch
            {
                myFormText = "MaciX";
            }

            this.Text = myFormText + " V10.10.22";
        }

        /// <summary>
        /// 检查激活
        /// </summary>
        private void CheckLicense()
        {
            if (LicenseForm.AreRegistryKeysExist())
            {
                if (LicenseForm.IsLicenseExpired())
                {
                    //证书已过期
                    MessageBox.Show("证书已过期，请重新激活软件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isActivated = false;
                }
                else if (LicenseForm.IsDateTampered())
                {
                    //读取到的电脑时间早于上次使用日期
                    MessageBox.Show("日期有误，请重新激活软件", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isActivated = false;
                }
                else
                {
                    //将当前日期写入注册表
                    LicenseForm.WriteCurrentDateToRegistry();
                    //已激活
                    isActivated = true;
                }
            }
            else
            {
                //未激活 检查试用期
                if (DateTime.Now > new DateTime(2025, 5, 31))
                {
                    //25年5月时该软件需要激活才能用
                    //只限制试用期为8个月只能保证电脑在首次启动该软件后8个月内能使用
                    //也就是说如果有电脑25年5月才首次使用该软件，试用期也会是8个月
                    //所以这里加了日期限制
                    MessageBox.Show("该软件只支持到2025年5月31日，请联系供应商", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isActivated = false;
                    return;
                }

                if (LicenseForm.CheckAndUpdateTrialStatus())
                {
                    isActivated = true;
                }
                else
                {
                    MessageBox.Show("试用期已过，请激活软件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isActivated = false;
                }
            }
        }

        #region 文件菜单

        //用户登录
        private void FileAccount_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuAccountForm myAccountForm = new MenuAccountForm();

            //
            myAccountForm.MyUser = MyDevice.D_username;
            myAccountForm.MyPSW = MyDevice.D_password;
            myAccountForm.MyDatPath = MyDevice.D_datPath;
            myAccountForm.MyCfgPath = MyDevice.D_cfgPath;
            myAccountForm.MyLogPath = MyDevice.D_logPath;

            //
            myAccountForm.Text = MyDevice.languageType == 0 ? "切换用户！" : "User switch";
            myAccountForm.StartPosition = FormStartPosition.CenterScreen;
            myAccountForm.ShowDialog();

            //超级账户调整界面
            TSMenuItemVisible();
            TSMenuItemEnable();
        }

        //退出系统
        private void FileExit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //关闭串口
            if (MyDevice.protocol.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocol.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocol.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                MyDevice.protocol.Protocol_PortClose();
            }

            //关闭串口
            if (MyDevice.protocos.IsOpen)
            {
                //断开串口时，置位is_serial_closing标记更新
                MyDevice.protocos.Is_serial_closing = true;

                //处理当前在消息队列中的所有 Windows 消息
                //防止界面停止响应
                //https://blog.csdn.net/sinat_23338865/article/details/52596818
                while (MyDevice.protocos.Is_serial_listening)
                {
                    Application.DoEvents();
                }

                //关闭串口
                MyDevice.protocos.Protocol_PortClose();
            }

            //退出所有窗口
            System.Environment.Exit(0);
        }

        #endregion

        #region 设置菜单

        //通讯连接
        private void SetConnect_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetConnect";
            MenuConnectForm myUartComForm = new MenuConnectForm();
            myUartComForm.StartPosition = FormStartPosition.CenterParent;
            myUartComForm.ShowDialog();

            //
            if (isConnected)
            {
                actXET = MyDevice.actDev;
                TSMenuItemVisible();
                TSMenuItemEnable();
            }

            //
            activeForm = "";
            this.BringToFront();

            //重新激活显示窗口前清空一下
            while (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
            }

            DeviceFormActive();
        }

        //标定参数
        private void SetParCal_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParCal";
            MenuSetParaForm calibrationParameter = new MenuSetParaForm();
            calibrationParameter.StartPosition = FormStartPosition.CenterParent;
            calibrationParameter.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //滤波参数
        private void SetParFilt_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParFilt";
            MenuSetFilterForm filterRange = new MenuSetFilterForm();
            filterRange.StartPosition = FormStartPosition.CenterParent;
            filterRange.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //AI跟踪器
        private void SetParCheating_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParCheating";
            MenuSetCheatingForm myCheatSettingForm = new MenuSetCheatingForm();
            myCheatSettingForm.StartPosition = FormStartPosition.CenterParent;
            myCheatSettingForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //RS485参数
        private void SetParRS485_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParRS485";
            MenuParaRS485Form rs485Parameters = new MenuParaRS485Form();
            rs485Parameters.StartPosition = FormStartPosition.CenterParent;
            rs485Parameters.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //CANopen参数
        private void SetParCANopen_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParCANopen";
            MenuParaCANopenForm canopenParameters = new MenuParaCANopenForm();
            canopenParameters.StartPosition = FormStartPosition.CenterParent;
            canopenParameters.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //MODBUS TCP参数
        private void SetParModbusTCP_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParModbusTCP";
            MenuParaModbusTCPForm modbusTCPParameters = new MenuParaModbusTCPForm();
            modbusTCPParameters.StartPosition = FormStartPosition.CenterParent;
            modbusTCPParameters.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //wireless参数
        private void SetParWirelessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetParWireless";
            MenuParaWirelessForm wirelessParameters = new MenuParaWirelessForm();
            wirelessParameters.StartPosition = FormStartPosition.CenterParent;
            wirelessParameters.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //标定传感器
        private void SetCalibration_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetCalibration";
            MenuSetCalForm myMenuSetCalForm = new MenuSetCalForm();
            myMenuSetCalForm.StartPosition = FormStartPosition.CenterParent;
            myMenuSetCalForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //修正传感器
        private void SetCalCorrect_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetCalCorrect";
            MenuSetCorrForm myMenuSetCorrForm = new MenuSetCorrForm();
            myMenuSetCorrForm.StartPosition = FormStartPosition.CenterParent;
            myMenuSetCorrForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //工厂校准
        private void SetFactory_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetFactory";
            MenuFacUserForm myMenuFacUserForm = new MenuFacUserForm();
            myMenuFacUserForm.StartPosition = FormStartPosition.CenterParent;
            myMenuFacUserForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //手动校准
        private void SetFacManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetFacManual";
            MenuFacManualForm myMenuFacManualForm = new MenuFacManualForm();
            myMenuFacManualForm.StartPosition = FormStartPosition.CenterParent;
            myMenuFacManualForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //自动校准
        private void SetFacAutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "SetFacAuto";
            MenuFacAutoForm myMenuFacAutoForm = new MenuFacAutoForm();
            myMenuFacAutoForm.StartPosition = FormStartPosition.CenterParent;
            myMenuFacAutoForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        #endregion

        #region 工具菜单

        //导入配置
        private void ToolCfgImport_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolCfgImport";
            MenuToolParImport myMenuToolParImport = new MenuToolParImport();
            if (MyDevice.languageType == 0)
            {
                myMenuToolParImport.Text = "导入配置";
            }
            else
            {
                myMenuToolParImport.Text = "Import Configuration";
            }
            myMenuToolParImport.StartPosition = FormStartPosition.CenterParent;
            myMenuToolParImport.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //导出配置
        private void ToolCfgExport_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolCfgExport";
            MenuToolParExport myMenuToolParExport = new MenuToolParExport();
            if (MyDevice.languageType == 0)
            {
                myMenuToolParExport.Text = "保存配置";
            }
            else
            {
                myMenuToolParExport.Text = "Save Configuration";
            }
            myMenuToolParExport.StartPosition = FormStartPosition.CenterParent;
            myMenuToolParExport.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //Eeprom配置
        private void ToolEepromCfgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolEepromCfg";
            MenuToolEepromCfg myMenuToolEepromCfg = new MenuToolEepromCfg();
            if (MyDevice.languageType == 0)
            {
                myMenuToolEepromCfg.Text = "TEDS模块参数";
            }
            else
            {
                myMenuToolEepromCfg.Text = "TEDS Module Parameters";
            }
            myMenuToolEepromCfg.StartPosition = FormStartPosition.CenterParent;
            myMenuToolEepromCfg.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //RS485调试工具
        private void ToolComRS485_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolComRS485";
            MenuToolRS485Form menuToolRS485Form = new MenuToolRS485Form();
            menuToolRS485Form.StartPosition = FormStartPosition.CenterParent;
            menuToolRS485Form.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //CANopen调试工具
        private void ToolComCANopen_ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //接收器配置
        private void ToolReceiver_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolReceiver";
            MainFormRecConfig menuToolReceiverForm = new MainFormRecConfig();
            menuToolReceiverForm.StartPosition = FormStartPosition.CenterParent;
            menuToolReceiverForm.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //数据日志
        private void ToolDatLog_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolLogReview";
            MenuToolLogReview menuToolLogReview = new MenuToolLogReview();
            menuToolLogReview.StartPosition = FormStartPosition.CenterParent;
            menuToolLogReview.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        //数据分析
        private void ToolDatAny_ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //产品选型
        private void ToolProduct_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "ToolProduct";
            MenuModelSelect myModelSelect = new MenuModelSelect();
            myModelSelect.WindowState = FormWindowState.Maximized;
            myModelSelect.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();
        }

        #endregion

        #region 帮助菜单

        //使用说明
        private void HelpGuidline_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = Application.StartupPath + "\\pic\\MaciX软件操作说明书.pdf";
            myProcess.StartInfo.Verb = "Open";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();
        }

        //选择CH
        private void HelpLangCH_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //zh-CN 为中文，更多的关于 Culture 的字符串请查 MSDN
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh");
            //语言设置为中文
            MyDevice.languageType = 0;
            //对当前窗体应用更改后的资源
            ApplyResource();
            //保存选择的语言
            MyDevice.SaveLanguage(0);
            //提示
            MessageBox.Show("请重新启动软件");
            //自动关闭软件
            FileExit_ToolStripMenuItem_Click(sender, e);
        }

        //选择EN
        private void HelpLangEN_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //en 为英文，更多的关于 Culture 的字符串请查 MSDN
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            //语言设置为英文
            MyDevice.languageType = 1;
            //对当前窗体应用更改后的资源
            ApplyResource();
            //保存选择的语言
            MyDevice.SaveLanguage(1);
            //提示
            MessageBox.Show("Please restart the software.");
            //自动关闭软件
            FileExit_ToolStripMenuItem_Click(sender, e);
        }

        //关闭开机自启
        private void CloseAutoStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //通过修改注册表关闭开机自启动
            AutoStart.AutoStartByRK("MaciXAutoStart", false);

            //保存关闭开机自启
            MyDevice.SaveAutoStart(false);

            MessageBox.Show("已关闭开机自动启动");
        }

        //开启开机自启
        private void EnableAutoStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //通过修改注册表实现开机自启动
            AutoStart.AutoStartByRK("MaciXAutoStart", true);

            //保存开启开机自启
            MyDevice.SaveAutoStart(true);

            MessageBox.Show("已开启开机自动启动");
        }

        //关于
        private void HelpAbout_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            MyDevice.mePort_StopDacout();

            //
            activeForm = "HelpAbout";
            MenuAboutBox myAboutBox = new MenuAboutBox();
            myAboutBox.ShowDialog();

            //
            activeForm = "";
            this.BringToFront();
            DeviceFormActive();

            CheckLicense();
            //账户调整界面
            TSMenuItemVisible();
            TSMenuItemEnable();
        }

        #endregion

        //激活设备数据显示窗口
        private void DeviceFormActive()
        {
            //多设备显示窗口
            if ((MyDevice.devSum > 1) && (!isMeasure))
            {
                //MenuParaRS485Form修改站点流程
                //确定后先拷贝迁移数据
                //然后系执行BCC->WRX5写入
                //写入过程中的串口委托都会刷新本窗口update_ListTextChart
                //但是actXET指向的addr还是旧的地址
                //在WRX5写入完成后才会修改站号
                //但是已经没有串口委托了
                //因此主界面MenuParaRS485Form关闭时需要触发一下委托来更新本窗口update_ListTextChart
                //这样才能让actXET指向的新的addr
                MyDevice.callDelegate();

                //委托之后才能更新activeForm为MultDevice
                activeForm = "MultipleDevice";

                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MultipleDevice")
                    {
                        form.BringToFront();
                        return;
                    }
                }

                MultipleDevice myMultipleDevice = new MultipleDevice();
                myMultipleDevice.MdiParent = this;
                myMultipleDevice.Show();
                myMultipleDevice.WindowState = FormWindowState.Maximized;

                myMeasure += new showMeasure(ShowDetail);
            }
            //单元设备显示窗口
            else if (MyDevice.devSum > 0 && isConnected)
            {
                //MenuParaRS485Form修改站点流程
                //确定后先拷贝迁移数据
                //然后系执行BCC->WRX5写入
                //写入过程中的串口委托都会刷新本窗口update_ListTextChart
                //但是actXET指向的addr还是旧的地址
                //在WRX5写入完成后才会修改站号
                //但是已经没有串口委托了
                //因此主界面MenuParaRS485Form关闭时需要触发一下委托来更新本窗口update_ListTextChart
                //这样才能让actXET指向的新的addr
                MyDevice.callDelegate();

                //委托之后才能更新activeForm为MeasureDevice
                activeForm = "MeasureDevice";

                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MeasureDevice")
                    {
                        form.BringToFront();
                        return;
                    }
                }

                MeasureDevice myMeasureDevice = new MeasureDevice();
                myMeasureDevice.MdiParent = this;
                myMeasureDevice.Show();
                myMeasureDevice.WindowState = FormWindowState.Maximized;
            }
        }

        //显示单元设备显示窗口事件
        private void ShowDetail()
        {
            if (isMeasure)
            {
                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MultipleDevice")
                    {
                        form.Close();
                    }
                }

                MyDevice.callDelegate();

                //委托之后才能更新activeForm为MeasureDevice
                activeForm = "MeasureDevice";

                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MeasureDevice")
                    {
                        form.BringToFront();
                        return;
                    }
                }

                MeasureDevice myMeasureDevice = new MeasureDevice();
                myMeasureDevice.MdiParent = this;
                myMeasureDevice.Show();
                myMeasureDevice.WindowState = FormWindowState.Maximized;
            }
            else
            {
                MyDevice.callDelegate();

                //委托之后才能更新activeForm为MultDevice
                activeForm = "MultipleDevice";

                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MultipleDevice")
                    {
                        form.BringToFront();
                        return;
                    }
                }

                MultipleDevice myMultipleDevice = new MultipleDevice();
                myMultipleDevice.MdiParent = this;
                myMultipleDevice.Show();
                myMultipleDevice.WindowState = FormWindowState.Maximized;
            }
        }

        //进入单元设备显示窗口委托
        public static void callDelegate()
        {
            //委托
            if (myMeasure != null)
            {
                myMeasure();
            }
        }

        //多设备功能禁用
        private void 设置ToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            if (ActiveForm.Contains("MultipleDevice"))
            {
                SetComToolStripMenuItem.Enabled = false;//通讯设置
                SetCalToolStripMenuItem.Enabled = false;//参数标定
                SetFactoryToolStripMenuItem.Enabled = false;//工厂校准
                配置工具ToolStripMenuItem.Enabled = false;//配置工具
                通讯工具ToolStripMenuItem.Enabled = false;//通讯工具
            }
            else
            {
                SetComToolStripMenuItem.Enabled = SetComToolStripMenuItem.Enabled == false ? false : true;//通讯设置
                SetCalToolStripMenuItem.Enabled = SetCalToolStripMenuItem.Enabled == false ? false : true; ;//参数标定
                SetFactoryToolStripMenuItem.Enabled = SetFactoryToolStripMenuItem.Enabled == false ? false : true; ;//工厂校准
                配置工具ToolStripMenuItem.Enabled = 配置工具ToolStripMenuItem.Enabled == false ? false : true; ;//配置工具
                通讯工具ToolStripMenuItem.Enabled = 通讯工具ToolStripMenuItem.Enabled == false ? false : true; ;//通讯工具
            }
        }
    }
}
