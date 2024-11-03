using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

//未经过审批不得改动

//Alvin 20230414

//管理设备

//对象: 账户密码
//对象: 注册权限
//对象: 委托事件  myUpdate
//对象: UIT变换   myUIT
//对象: 通讯接口  protocol
//对象: 设备实例  mSUT,mBUS,mCAN等

//方法: 本地硬盘存储
//方法: 通讯接口调用和读写任务
//方法: 站点变更后设备数据迁移

namespace Model
{
    //定义委托
    public delegate void freshHandler();

    //多设备
    public static class MyDevice
    {
        //User function
        public static Int32 languageType;//语言,0=中文,1=英文
        public static String D_username;//账户名
        public static String D_password;//账户密码
        public static String D_datPath;//保存账户
        public static String D_cfgPath;//保存设备的配置参数
        public static String D_logPath;//保存日志和Excel数据
        public static String D_picPath;//保存图片说明书等资源
        public static String D_dataPath;//保存cpk测试表和主界面数据

        //User PC Copyright
        public static Byte myPC = 0;//注册电脑
        public static Int64 myMac = 0;//电脑的MAC,
        public static Int64 myVar = 0;//电脑的MAC,复检
        public static Int32 mySN = 0;//生产操作序列号

        //User Event 定义事件
        public static event freshHandler myUpdate;
        public static event freshHandler mySecondeUpdate;

        //浮点数转换
        public static UIT myUIT = new UIT();

        //设备连接的接口类型
        //自定义协议用mySelfUART
        //双通道T2X420的自定义协议用mySelfUART和mySecondUART
        //八通道T8X420需要用myRS485通过地址交换数据
        public static IProtocol mySelfUART = new SelfUARTProtocol();
        public static IProtocol mySecondUART = new SecondUARTProtocol();
        public static IProtocol myRS485 = new RS485Protocol();
        public static IProtocol myCANopen = new CANopenProtocol();
        public static IProtocol myModbusTCP = new ModbusTCPProtocol();

        //设备连接的接口
        public static IProtocol protocol; //主接口
        public static IProtocol protocos; //副接口 T2X420

        //设备数据
        public static XET mSUT = new XET();        //SelfUART
        public static XET mSXT = new XET();        //SecondUART 副接口T2X420
        public static XET[] mBUS = new XET[256];   //RS485
        public static XET[] mCAN = new XET[128];   //CANopen
        public static XET[] mMTCP = new XET[256];  //Modbus TCP
        public static XET[] mECAT = new XET[256];  //EtherCAT
        public static XET[] mEIP = new XET[256];   //EtherNetIP
        public static XET[] mPNET = new XET[256];  //PROFINET
        public static XET[] mPBUS = new XET[256];  //PROFIBUS

        /// <summary>
        /// protocol指向的设备数量
        /// </summary>
        public static Int32 devSum
        {
            get
            {
                int num = 0;

                switch (protocol.type)
                {
                    default:
                    case COMP.SelfUART:
                        switch (mSUT.sTATE)
                        {
                            case STATE.INVALID:
                            case STATE.OFFLINE:
                                return 0;

                            case STATE.CONNECTED:
                            case STATE.WORKING:
                                return 1;
                        }
                        return 0;

                    case COMP.RS485:
                        for (int i = 0; i < 256; i++)
                        {
                            switch (mBUS[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    num++;
                                    break;
                            }
                        }
                        return num;

                    case COMP.CANopen:
                        for (int i = 0; i < 128; i++)
                        {
                            switch (mCAN[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    num++;
                                    break;
                            }
                        }
                        return num;

                    case COMP.ModbusTCP:
                        for (int i = 0; i < 256; i++)
                        {
                            switch (mMTCP[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    num++;
                                    break;
                            }
                        }
                        return num;
                }
            }
        }

        /// <summary>
        /// protocol.addr指向的设备
        /// </summary>
        public static XET actDev
        {
            get
            {
                switch (protocol.type)
                {
                    default:
                    case COMP.SelfUART:
                        return mSUT;

                    case COMP.RS485:
                        return mBUS[MyDevice.protocol.addr];

                    case COMP.CANopen:
                        return mCAN[MyDevice.protocol.addr];

                    case COMP.ModbusTCP:
                        //modbusTCP同一个E_addr可能有多个设备
                        //故不能只使用MyDevice.protocol.addr做下标来匹配
                        //需要先确认ip地址
                        //再匹配MyDevice.protocol.addr
                        int xetIndex = FindClientIndexByEndpointAndAddr(MyDevice.protocol.ipAddr, MyDevice.protocol.addr);
                        if (xetIndex > 0 && xetIndex < 256)
                        {
                            return mMTCP[(byte)xetIndex];
                        }
                        else
                        {
                            return mMTCP[0];
                        }
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        static MyDevice()
        {
            protocol = mySelfUART;
            protocos = mySecondUART;

            //
            languageType = 0;
            D_username = "user";
            D_password = "";
            D_datPath = AppDomain.CurrentDomain.BaseDirectory + @"\dat";
            D_cfgPath = AppDomain.CurrentDomain.BaseDirectory + @"\cfg";
            D_logPath = AppDomain.CurrentDomain.BaseDirectory + @"\log";
            D_picPath = AppDomain.CurrentDomain.BaseDirectory + @"\pic";
            D_dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\data";

            //
            myPC = 0;
            myMac = 0;
            myVar = 0;
            mySN = 0;

            //SelfUART
            mSUT = new XET
            {
                sTATE = STATE.INVALID,
                R_bohrcode_long = 0,
                R_adcout = 0,
                R_dacset = 0,
                R_stable = false,
                R_overload = false,
                R_grossnet = "",
                R_weight = "",
                R_output = ""
            };

            //SecondUART
            mSXT = new XET
            {
                sTATE = STATE.INVALID,
                R_bohrcode_long = 0,
                R_adcout = 0,
                R_dacset = 0,
                R_stable = false,
                R_overload = false,
                R_grossnet = "",
                R_weight = "",
                R_output = ""
            };

            //RS485
            for (int i = 0; i < 256; i++)
            {
                mBUS[i] = new XET();
                mBUS[i].sTATE = STATE.INVALID;
                mBUS[i].R_bohrcode_long = 0;
                mBUS[i].R_adcout = 0;
                mBUS[i].R_dacset = 0;
                mBUS[i].R_stable = false;
                mBUS[i].R_overload = false;
                mBUS[i].R_grossnet = "";
                mBUS[i].R_weight = "";
                mBUS[i].R_output = "";
            }

            //CANopen
            for (int i = 0; i < 128; i++)
            {
                mCAN[i] = new XET();
                mCAN[i].sTATE = STATE.INVALID;
                mCAN[i].R_bohrcode_long = 0;
                mCAN[i].R_adcout = 0;
                mCAN[i].R_dacset = 0;
                mCAN[i].R_stable = false;
                mCAN[i].R_overload = false;
                mCAN[i].R_grossnet = "";
                mCAN[i].R_weight = "";
                mCAN[i].R_output = "";
            }

            //Modbus TCP
            for (int i = 0; i < 256; i++)
            {
                mMTCP[i] = new XET();
                mMTCP[i].sTATE = STATE.INVALID;
                mMTCP[i].R_bohrcode_long = 0;
                mMTCP[i].R_adcout = 0;
                mMTCP[i].R_dacset = 0;
                mMTCP[i].R_stable = false;
                mMTCP[i].R_overload = false;
                mMTCP[i].R_grossnet = "";
                mMTCP[i].R_weight = "";
                mMTCP[i].R_output = "";
            }
        }

        /// <summary>
        /// 执行委托
        /// </summary>
        public static void callDelegate()
        {
            //委托
            if (myUpdate != null)
            {
                myUpdate();
            }
        }

        /// <summary>
        /// 执行委托
        /// </summary>
        public static void callSecondDelegate()
        {
            //委托
            if (mySecondeUpdate != null)
            {
                mySecondeUpdate();
            }
        }

        /// <summary>
        /// 加载配置文档
        /// </summary>
        /// <param name="meName"></param>
        /// <returns></returns>
        public static String[] LoadFromCfg(String meName)
        {
            //空
            if (MyDevice.D_cfgPath == null)
            {
                return null;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.D_cfgPath))
            {
                Directory.CreateDirectory(MyDevice.D_cfgPath);
            }

            //配置文件
            String filePath = MyDevice.D_cfgPath + "\\" + meName + ".txt";
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 保存帐号
        /// </summary>
        /// <returns></returns>
        public static bool SaveToDat()
        {
            //空
            if (D_datPath == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(D_datPath))
            {
                Directory.CreateDirectory(D_datPath);
            }

            //写入
            try
            {
                String filePath = D_datPath + @"\user." + D_username + ".dat";
                FileStream meFS = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                BinaryWriter meWrite = new BinaryWriter(meFS);
                //
                meWrite.Write(D_username);
                meWrite.Write(D_password);
                meWrite.Write(D_datPath);
                meWrite.Write(D_cfgPath);
                meWrite.Write(D_logPath);
                meWrite.Write(D_dataPath);
                //
                meWrite.Close();
                meFS.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="myXET"></param>
        /// <param name="meName"></param>
        /// <returns></returns>
        public static bool SaveToCfg(XET myXET, String meName, List<string> items)
        {
            //空
            if (MyDevice.D_cfgPath == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.D_cfgPath))
            {
                Directory.CreateDirectory(MyDevice.D_cfgPath);
            }

            //写入
            try
            {
                String filePath = MyDevice.D_cfgPath + "\\" + meName + ".txt";
                FileStream meFS = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);
                //
                for (int i = 0; i < items.Count; i++)
                {
                    meWrite.WriteLine(items[i]);
                }
                //
                meWrite.Close();
                meFS.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存标定
        /// </summary>
        public static bool SaveToLog(XET myXET)
        {
            //空
            if (D_logPath == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(D_logPath))
            {
                Directory.CreateDirectory(D_logPath);
            }

            //写入
            try
            {
                String filePath;
                if ((D_username == "bohr") && (D_password == "bmc"))
                {
                    filePath = D_logPath + @"\" + myXET.S_DeviceType.ToString() + "." + myXET.R_bohrcode_long.ToString("X14") + ".txt";
                }
                else
                {
                    filePath = D_logPath + @"\" + myXET.S_DeviceType.ToString() + "." + myXET.R_bohrcode_long.ToString("X14") + "." + myXET.S_OutType.ToString() + "-" + myXET.E_mfg_srno.ToString("0000") + ".txt";
                }
                FileStream meFS = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);
                //
                meWrite.WriteLine(System.DateTime.Now.ToString());
                meWrite.WriteLine(";===============================================================");
                meWrite.WriteLine("[user]");
                if ((D_username == "bohr") && (D_password == "bmc"))
                {
                    meWrite.WriteLine("[username]      user name : administrator");
                }
                else
                {
                    meWrite.WriteLine("[username]      user name : " + D_username);
                }
                meWrite.WriteLine("[testlog]       test log export : " + D_logPath);
                meWrite.WriteLine("");
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("[ini]");
                meWrite.WriteLine("[type]          Transducer type : " + myXET.S_DeviceType.ToString());
                meWrite.WriteLine("[output]        Transducer : " + myXET.S_OutType.ToString());
                meWrite.WriteLine("[datecode]      PCBA calibration date : " + System.DateTime.Now.ToString("yyyy-MM-dd, HH:mm"));
                meWrite.WriteLine("[seriesno]      PCBA series number : " + myXET.E_mfg_srno.ToString("0000"));
                meWrite.WriteLine("[bohrcode]      PCBA universal ID : " + myXET.R_bohrcode_long.ToString("X14"));
                meWrite.WriteLine("");
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("[set]");
                switch (myXET.E_curve)
                {
                    case (Byte)ECVE.CTWOPT:
                        meWrite.WriteLine("[calibration]   Transducer calibration point : two point line");
                        break;

                    case (Byte)ECVE.CFITED:
                        //TP10型号标记进返程状态
                        if (myXET.S_DeviceType == TYPE.TP10)
                        {
                            if (myXET.ecveType == ECVEType.Single)
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : five point fitting Single");
                            }
                            else
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : five point fitting F&B");
                            }
                        }
                        else
                        {
                            meWrite.WriteLine("[calibration]   Transducer calibration point : five point fitting");
                        }
                        break;

                    case (Byte)ECVE.CINTER:
                        if (myXET.S_DeviceType == TYPE.TP10)
                        {
                            if (myXET.ecveType == ECVEType.Single)
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : five point interpolating Single");
                            }
                            else
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : five point interpolating F&B");
                            }
                        }
                        else
                        {
                            meWrite.WriteLine("[calibration]   Transducer calibration point : five point interpolating");
                        }
                        break;

                    case (Byte)ECVE.CELTED:
                        if (myXET.S_DeviceType == TYPE.TP10)
                        {
                            if (myXET.ecveType == ECVEType.Single)
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point fitting Single");
                            }
                            else
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point fitting F&B");
                            }
                        }
                        else
                        {
                            meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point fitting");
                        }
                        break;

                    case (Byte)ECVE.CELTER:
                        if (myXET.S_DeviceType == TYPE.TP10)
                        {
                            if (myXET.ecveType == ECVEType.Single)
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point interpolating Single");
                            }
                            else
                            {
                                meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point interpolating F&B");
                            }
                        }
                        else
                        {
                            meWrite.WriteLine("[calibration]   Transducer calibration point : eleven point interpolating");
                        }
                        break;

                    default:
                        meWrite.WriteLine("[calibration]   Transducer calibration point :");
                        break;
                }
                switch (myXET.E_adspeed & 0x0F)
                {
                    case (Byte)EPGA.ADPGA1: meWrite.WriteLine("[adspeed]       input range : ±400mV/V"); break;
                    case (Byte)EPGA.ADPGA2: meWrite.WriteLine("[adspeed]       input range : ±200mV/V"); break;
                    case (Byte)EPGA.ADPGA64: meWrite.WriteLine("[adspeed]       input range : ±7.0mV/V"); break;
                    case (Byte)EPGA.ADPGA128: meWrite.WriteLine("[adspeed]       input range : ±3.5mV/V"); break;
                    default: meWrite.WriteLine("[adspeed]       input range :"); break;
                }
                switch (myXET.E_adspeed & 0xF0)
                {
                    case (Byte)ESPD.CSF10: meWrite.WriteLine("[adspeed]       Transducer speed : 10Hz"); break;
                    case (Byte)ESPD.CSF40: meWrite.WriteLine("[adspeed]       Transducer speed : 40Hz"); break;
                    case (Byte)ESPD.CSF640: meWrite.WriteLine("[adspeed]       Transducer speed : 600Hz"); break;
                    case (Byte)ESPD.CSF1280: meWrite.WriteLine("[adspeed]       Transducer speed : 1200Hz"); break;
                    default: meWrite.WriteLine("[adspeed]       Transducer speed :"); break;
                }
                meWrite.WriteLine("");
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("[Table]");
                meWrite.WriteLine("");
                if ((D_username == "bohr") && (D_password == "bmc"))
                {
                    meWrite.WriteLine("[SCT0]          myXET.E_test         = 0x" + myXET.E_test.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_outype       = 0x" + myXET.E_outype.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_curve        = 0x" + myXET.E_curve.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_adspeed      = 0x" + myXET.E_adspeed.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_autozero     = 0x" + myXET.E_autozero.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_trackzero    = 0x" + myXET.E_trackzero.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          myXET.E_checkhigh    = " + myXET.E_checkhigh.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_checklow     = " + myXET.E_checklow.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_mfg_date     = " + myXET.E_mfg_date.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_mfg_srno     = " + myXET.E_mfg_srno.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_tmp_min      = " + myXET.E_tmp_min.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_tmp_max      = " + myXET.E_tmp_max.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_tmp_cal      = " + myXET.E_tmp_cal.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_bohrcode     = " + myXET.R_bohrcode_long.ToString("X14"));
                    meWrite.WriteLine("[SCT0]          myXET.E_enspan       = " + myXET.E_enspan.ToString());
                    meWrite.WriteLine("[SCT0]          myXET.E_protype      = " + myXET.E_protype.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT1]          myXET.E_ad_point1    = " + myXET.E_ad_point1.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_ad_point2    = " + myXET.E_ad_point2.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_ad_point3    = " + myXET.E_ad_point3.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_ad_point4    = " + myXET.E_ad_point4.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_ad_point5    = " + myXET.E_ad_point5.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_da_point1    = " + myXET.E_da_point1.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_da_point2    = " + myXET.E_da_point2.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_da_point3    = " + myXET.E_da_point3.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_da_point4    = " + myXET.E_da_point4.ToString());
                    meWrite.WriteLine("[SCT1]          myXET.E_da_point5    = " + myXET.E_da_point5.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT2]          myXET.E_input1       = " + myXET.E_input1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input1).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_input2       = " + myXET.E_input2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input2).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_input3       = " + myXET.E_input3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input3).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_input4       = " + myXET.E_input4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input4).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_input5       = " + myXET.E_input5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input5).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_analog1      = " + myXET.E_analog1.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog1).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_analog2      = " + myXET.E_analog2.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog2).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_analog3      = " + myXET.E_analog3.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog3).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_analog4      = " + myXET.E_analog4.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog4).ToString() + ">");
                    meWrite.WriteLine("[SCT2]          myXET.E_analog5      = " + myXET.E_analog5.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog5).ToString() + ">");
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT3]          myXET.E_ad_zero      = " + myXET.E_ad_zero.ToString());
                    meWrite.WriteLine("[SCT3]          myXET.E_ad_full      = " + myXET.E_ad_full.ToString());
                    meWrite.WriteLine("[SCT3]          myXET.E_da_zero      = " + myXET.E_da_zero.ToString());
                    meWrite.WriteLine("[SCT3]          myXET.E_da_full      = " + myXET.E_da_full.ToString());
                    meWrite.WriteLine("[SCT3]          myXET.E_vtio         = " + myXET.E_vtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_vtio).ToString() + ">");
                    meWrite.WriteLine("[SCT3]          myXET.E_wtio         = " + myXET.E_wtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_wtio).ToString() + ">");
                    meWrite.WriteLine("[SCT3]          myXET.E_atio         = " + myXET.E_atio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_atio).ToString() + ">");
                    meWrite.WriteLine("[SCT3]          myXET.E_btio         = " + myXET.E_btio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_btio).ToString() + ">");
                    meWrite.WriteLine("[SCT3]          myXET.E_ctio         = " + myXET.E_ctio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ctio).ToString() + ">");
                    meWrite.WriteLine("[SCT3]          myXET.E_dtio         = " + myXET.E_dtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_dtio).ToString() + ">");
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT4]          myXET.E_da_zero_4ma  = " + myXET.E_da_zero_4ma.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_full_20ma = " + myXET.E_da_full_20ma.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_zero_05V  = " + myXET.E_da_zero_05V.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_full_05V  = " + myXET.E_da_full_05V.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_zero_10V  = " + myXET.E_da_zero_10V.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_full_10V  = " + myXET.E_da_full_10V.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_zero_N5   = " + myXET.E_da_zero_N5.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_full_P5   = " + myXET.E_da_full_P5.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_zero_N10  = " + myXET.E_da_zero_N10.ToString());
                    meWrite.WriteLine("[SCT4]          myXET.E_da_full_P10  = " + myXET.E_da_full_P10.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT5]          myXET.E_corr         = " + myXET.E_corr.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_corr).ToString() + ">");
                    meWrite.WriteLine("[SCT5]          myXET.E_mark         = 0x" + myXET.E_mark.ToString("X2"));
                    switch (myXET.S_DeviceType)
                    {
                        case TYPE.TDES:
                        case TYPE.TDSS:
                            meWrite.WriteLine("[SCT5]          myXET.E_sign         = 0x" + myXET.E_sign.ToString("X2"));
                            meWrite.WriteLine("[SCT5]          myXET.E_addr         = 0x" + myXET.E_addr.ToString("X2"));
                            switch (myXET.E_baud)
                            {
                                case 0: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 1200"); break;
                                case 1: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 2400"); break;
                                case 2: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 4800"); break;
                                case 3: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 9600"); break;
                                case 4: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 14400"); break;
                                case 5: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 19200"); break;
                                case 6: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 38400"); break;
                                case 7: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 57600"); break;
                                case 8: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 115200"); break;
                                case 9: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 230400"); break;
                                case 10: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 256000"); break;
                            }
                            meWrite.WriteLine("[SCT5]          myXET.E_stopbit      = " + myXET.E_stopbit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_parity       = " + myXET.E_parity.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            break;
                        case TYPE.TD485:
                            meWrite.WriteLine("[SCT5]          myXET.E_sign         = 0x" + myXET.E_sign.ToString("X2"));
                            meWrite.WriteLine("[SCT5]          myXET.E_addr         = 0x" + myXET.E_addr.ToString("X2"));
                            switch (myXET.E_baud)
                            {
                                case 0: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 1200"); break;
                                case 1: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 2400"); break;
                                case 2: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 4800"); break;
                                case 3: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 9600"); break;
                                case 4: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 14400"); break;
                                case 5: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 19200"); break;
                                case 6: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 38400"); break;
                                case 7: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 57600"); break;
                                case 8: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 115200"); break;
                                case 9: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 230400"); break;
                                case 10: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 256000"); break;
                            }
                            meWrite.WriteLine("[SCT5]          myXET.E_stopbit      = " + myXET.E_stopbit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_parity       = " + myXET.E_parity.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_sptime    = " + myXET.E_wt_sptime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_spfilt    = " + myXET.E_wt_spfilt.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_division  = " + myXET.E_wt_division.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_antivib   = " + myXET.E_wt_antivib.ToString());
                            break;
                        case TYPE.TCAN:
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_heartBeat    = " + myXET.E_heartBeat.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_typeTPDO0    = " + myXET.E_typeTPDO0.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_evenTPDO0    = " + myXET.E_evenTPDO0.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_nodeID       = " + myXET.E_nodeID.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_nodeBaud     = " + myXET.E_nodeBaud.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point6    = " + myXET.E_ad_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point7    = " + myXET.E_ad_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point8    = " + myXET.E_ad_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point9    = " + myXET.E_ad_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point10   = " + myXET.E_ad_point10.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point6    = " + myXET.E_da_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point7    = " + myXET.E_da_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point8    = " + myXET.E_da_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point9    = " + myXET.E_da_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point10   = " + myXET.E_da_point10.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT7]          myXET.E_input6       = " + myXET.E_input6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input7       = " + myXET.E_input7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input8       = " + myXET.E_input8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input9       = " + myXET.E_input9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input10      = " + myXET.E_input10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input10).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog6      = " + myXET.E_analog6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog7      = " + myXET.E_analog7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog8      = " + myXET.E_analog8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog9      = " + myXET.E_analog9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog10     = " + myXET.E_analog10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog10).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT8]          myXET.E_ad_point11   = " + myXET.E_ad_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_da_point11   = " + myXET.E_da_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_input11      = " + myXET.E_input11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_analog11     = " + myXET.E_analog11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_etio         = " + myXET.E_etio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_etio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_ftio         = " + myXET.E_ftio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ftio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_gtio         = " + myXET.E_gtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_gtio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_htio         = " + myXET.E_htio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_htio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_itio         = " + myXET.E_itio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_itio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_jtio         = " + myXET.E_jtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_jtio).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT9]          myXET.E_enGFC        = " + myXET.E_enGFC.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_enSRDO       = " + myXET.E_enSRDO.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_SCT_time     = " + myXET.E_SCT_time.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_COB_ID1      = " + myXET.E_COB_ID1.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_COB_ID2      = " + myXET.E_COB_ID2.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_enOL         = " + myXET.E_enOL.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_overload     = " + myXET.E_overload.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_alarmMode    = " + myXET.E_alarmMode.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_wetTarget    = " + myXET.E_wetTarget.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_wetLow       = " + myXET.E_wetLow.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_wetHigh      = " + myXET.E_wetHigh.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_lockTPDO0    = " + myXET.E_lockTPDO0.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_entrTPDO0    = " + myXET.E_entrTPDO0.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_typeTPDO1    = " + myXET.E_typeTPDO1.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_lockTPDO1    = " + myXET.E_lockTPDO1.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_entrTPDO1    = " + myXET.E_entrTPDO1.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_evenTPDO1    = " + myXET.E_evenTPDO1.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_scaling      = " + myXET.E_scaling.ToString("0.0"));
                            break;
                        case TYPE.iBus:
                            meWrite.WriteLine("[SCT5]          myXET.E_sign         = 0x" + myXET.E_sign.ToString("X2"));
                            meWrite.WriteLine("[SCT5]          myXET.E_addr         = 0x" + myXET.E_addr.ToString("X2"));
                            switch (myXET.E_baud)
                            {
                                case 0: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 1200"); break;
                                case 1: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 2400"); break;
                                case 2: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 4800"); break;
                                case 3: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 9600"); break;
                                case 4: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 14400"); break;
                                case 5: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 19200"); break;
                                case 6: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 38400"); break;
                                case 7: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 57600"); break;
                                case 8: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 115200"); break;
                                case 9: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 230400"); break;
                                case 10: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 256000"); break;
                            }
                            meWrite.WriteLine("[SCT5]          myXET.E_stopbit      = " + myXET.E_stopbit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_parity       = " + myXET.E_parity.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_sptime    = " + myXET.E_wt_sptime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_spfilt    = " + myXET.E_wt_spfilt.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_division  = " + myXET.E_wt_division.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_antivib   = " + myXET.E_wt_antivib.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_dynazero     = " + myXET.E_dynazero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_cheatype     = " + myXET.E_cheatype.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmax        = " + myXET.E_thmax.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_thmin.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_stablerange.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_stabletime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkzerotime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkdynatime.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point6    = " + myXET.E_ad_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point7    = " + myXET.E_ad_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point8    = " + myXET.E_ad_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point9    = " + myXET.E_ad_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point10   = " + myXET.E_ad_point10.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point6    = " + myXET.E_da_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point7    = " + myXET.E_da_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point8    = " + myXET.E_da_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point9    = " + myXET.E_da_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point10   = " + myXET.E_da_point10.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT7]          myXET.E_input6       = " + myXET.E_input6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input7       = " + myXET.E_input7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input8       = " + myXET.E_input8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input9       = " + myXET.E_input9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input10      = " + myXET.E_input10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input10).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog6      = " + myXET.E_analog6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog7      = " + myXET.E_analog7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog8      = " + myXET.E_analog8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog9      = " + myXET.E_analog9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog10     = " + myXET.E_analog10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog10).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT8]          myXET.E_ad_point11   = " + myXET.E_ad_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_da_point11   = " + myXET.E_da_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_input11      = " + myXET.E_input11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_analog11     = " + myXET.E_analog11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_etio         = " + myXET.E_etio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_etio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_ftio         = " + myXET.E_ftio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ftio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_gtio         = " + myXET.E_gtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_gtio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_htio         = " + myXET.E_htio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_htio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_itio         = " + myXET.E_itio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_itio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_jtio         = " + myXET.E_jtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_jtio).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT9]          myXET.E_filter       = " + myXET.E_filter.ToString());
                            break;
                        case TYPE.iNet:
                            meWrite.WriteLine("[SCT5]          myXET.E_sign         = 0x" + myXET.E_sign.ToString("X2"));
                            meWrite.WriteLine("[SCT5]          myXET.E_addr         = 0x" + myXET.E_addr.ToString("X2"));
                            switch (myXET.E_baud)
                            {
                                case 0: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 1200"); break;
                                case 1: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 2400"); break;
                                case 2: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 4800"); break;
                                case 3: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 9600"); break;
                                case 4: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 14400"); break;
                                case 5: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 19200"); break;
                                case 6: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 38400"); break;
                                case 7: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 57600"); break;
                                case 8: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 115200"); break;
                                case 9: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 230400"); break;
                                case 10: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 256000"); break;
                            }
                            meWrite.WriteLine("[SCT5]          myXET.E_stopbit      = " + myXET.E_stopbit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_parity       = " + myXET.E_parity.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_sptime    = " + myXET.E_wt_sptime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_spfilt    = " + myXET.E_wt_spfilt.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_division  = " + myXET.E_wt_division.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_antivib   = " + myXET.E_wt_antivib.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_dynazero     = " + myXET.E_dynazero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_cheatype     = " + myXET.E_cheatype.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmax        = " + myXET.E_thmax.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_thmin.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_stabletime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkzerotime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkdynatime.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point6    = " + myXET.E_ad_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point7    = " + myXET.E_ad_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point8    = " + myXET.E_ad_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point9    = " + myXET.E_ad_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point10   = " + myXET.E_ad_point10.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point6    = " + myXET.E_da_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point7    = " + myXET.E_da_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point8    = " + myXET.E_da_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point9    = " + myXET.E_da_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point10   = " + myXET.E_da_point10.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT7]          myXET.E_input6       = " + myXET.E_input6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input7       = " + myXET.E_input7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input8       = " + myXET.E_input8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input9       = " + myXET.E_input9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input10      = " + myXET.E_input10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input10).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog6      = " + myXET.E_analog6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog7      = " + myXET.E_analog7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog8      = " + myXET.E_analog8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog9      = " + myXET.E_analog9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog10     = " + myXET.E_analog10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog10).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT8]          myXET.E_ad_point11   = " + myXET.E_ad_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_da_point11   = " + myXET.E_da_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_input11      = " + myXET.E_input11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_analog11     = " + myXET.E_analog11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_etio         = " + myXET.E_etio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_etio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_ftio         = " + myXET.E_ftio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ftio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_gtio         = " + myXET.E_gtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_gtio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_htio         = " + myXET.E_htio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_htio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_itio         = " + myXET.E_itio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_itio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_jtio         = " + myXET.E_jtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_jtio).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT9]          myXET.E_filter       = " + myXET.E_filter.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_netServicePort = " + myXET.E_netServicePort.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_netServiceIP = " + myXET.GetIpAddressFromArray(myXET.E_netServiceIP));
                            meWrite.WriteLine("[SCT9]          myXET.E_netClientIP  = " + myXET.GetIpAddressFromArray(myXET.E_netClientIP));
                            meWrite.WriteLine("[SCT9]          myXET.E_netGatIP     = " + myXET.GetIpAddressFromArray(myXET.E_netGatIP));
                            meWrite.WriteLine("[SCT9]          myXET.E_netMaskIP    = " + myXET.GetIpAddressFromArray(myXET.E_netMaskIP));
                            meWrite.WriteLine("[SCT9]          myXET.E_useDHCP      = " + myXET.E_useDHCP.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_useScan      = " + myXET.E_useScan.ToString());
                            break;
                        case TYPE.iStar:
                            meWrite.WriteLine("[SCT5]          myXET.E_sign         = 0x" + myXET.E_sign.ToString("X2"));
                            meWrite.WriteLine("[SCT5]          myXET.E_addr         = 0x" + myXET.E_addr.ToString("X2"));
                            switch (myXET.E_baud)
                            {
                                case 0: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 1200"); break;
                                case 1: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 2400"); break;
                                case 2: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 4800"); break;
                                case 3: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 9600"); break;
                                case 4: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 14400"); break;
                                case 5: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 19200"); break;
                                case 6: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 38400"); break;
                                case 7: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 57600"); break;
                                case 8: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 115200"); break;
                                case 9: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 230400"); break;
                                case 10: meWrite.WriteLine("[SCT5]          myXET.E_baud         = 256000"); break;
                            }
                            meWrite.WriteLine("[SCT5]          myXET.E_stopbit      = " + myXET.E_stopbit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_parity       = " + myXET.E_parity.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_zero      = " + myXET.E_wt_zero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_full      = " + myXET.E_wt_full.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_decimal   = " + myXET.E_wt_decimal.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_unit      = " + myXET.E_wt_unit.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_ascii     = " + myXET.E_wt_ascii.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_sptime    = " + myXET.E_wt_sptime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_spfilt    = " + myXET.E_wt_spfilt.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_division  = " + myXET.E_wt_division.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_wt_antivib   = " + myXET.E_wt_antivib.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_dynazero     = " + myXET.E_dynazero.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_cheatype     = " + myXET.E_cheatype.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmax        = " + myXET.E_thmax.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_thmin.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_stabletime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkzerotime.ToString());
                            meWrite.WriteLine("[SCT5]          myXET.E_thmin        = " + myXET.E_tkdynatime.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point6    = " + myXET.E_ad_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point7    = " + myXET.E_ad_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point8    = " + myXET.E_ad_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point9    = " + myXET.E_ad_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_ad_point10   = " + myXET.E_ad_point10.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point6    = " + myXET.E_da_point6.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point7    = " + myXET.E_da_point7.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point8    = " + myXET.E_da_point8.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point9    = " + myXET.E_da_point9.ToString());
                            meWrite.WriteLine("[SCT6]          myXET.E_da_point10   = " + myXET.E_da_point10.ToString());
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT7]          myXET.E_input6       = " + myXET.E_input6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input7       = " + myXET.E_input7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input8       = " + myXET.E_input8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input9       = " + myXET.E_input9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_input10      = " + myXET.E_input10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input10).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog6      = " + myXET.E_analog6.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog6).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog7      = " + myXET.E_analog7.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog7).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog8      = " + myXET.E_analog8.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog8).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog9      = " + myXET.E_analog9.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog9).ToString() + ">");
                            meWrite.WriteLine("[SCT7]          myXET.E_analog10     = " + myXET.E_analog10.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog10).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT8]          myXET.E_ad_point11   = " + myXET.E_ad_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_da_point11   = " + myXET.E_da_point11.ToString());
                            meWrite.WriteLine("[SCT8]          myXET.E_input11      = " + myXET.E_input11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_analog11     = " + myXET.E_analog11.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog11).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_etio         = " + myXET.E_etio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_etio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_ftio         = " + myXET.E_ftio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ftio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_gtio         = " + myXET.E_gtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_gtio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_htio         = " + myXET.E_htio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_htio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_itio         = " + myXET.E_itio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_itio).ToString() + ">");
                            meWrite.WriteLine("[SCT8]          myXET.E_jtio         = " + myXET.E_jtio.ToString() + " <" + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_jtio).ToString() + ">");
                            meWrite.WriteLine("");
                            meWrite.WriteLine("[SCT9]          myXET.E_filter       = " + myXET.E_filter.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_addrRF       = " + myXET.E_addrRF[0].ToString() + "," + myXET.E_addrRF[1].ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_spedRF       = " + myXET.E_spedRF.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_chanRF       = " + myXET.E_chanRF.ToString());
                            meWrite.WriteLine("[SCT9]          myXET.E_optionRF     = " + myXET.E_optionRF.ToString());
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    meWrite.WriteLine("[SCT0] test     = 0x" + myXET.E_test.ToString("X2"));
                    meWrite.WriteLine("[SCT0] outype   = 0x" + myXET.E_outype.ToString("X2"));
                    meWrite.WriteLine("[SCT0] curve    = 0x" + myXET.E_curve.ToString("X2"));
                    meWrite.WriteLine("[SCT0] adspeed  = 0x" + myXET.E_adspeed.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          = 0x" + myXET.E_autozero.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          = 0x" + myXET.E_trackzero.ToString("X2"));
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_checkhigh.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_checklow.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_mfg_date.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_mfg_srno.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_tmp_min.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_tmp_max.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_tmp_cal.ToString());
                    meWrite.WriteLine("[SCT0]          = " + myXET.R_bohrcode_long.ToString("X14"));
                    meWrite.WriteLine("[SCT0]          = " + myXET.E_enspan.ToString());
                    meWrite.WriteLine("[SCT0] protype  = " + myXET.E_protype.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_ad_point1.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_ad_point2.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_ad_point3.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_ad_point4.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_ad_point5.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_da_point1.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_da_point2.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_da_point3.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_da_point4.ToString());
                    meWrite.WriteLine("[SCT1]          = " + myXET.E_da_point5.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input1).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input2).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input3).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input4).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input5).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog1).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog2).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog3).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog4).ToString());
                    meWrite.WriteLine("[SCT2]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog5).ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT3] adz      = " + myXET.E_ad_zero.ToString());
                    meWrite.WriteLine("[SCT3] adf      = " + myXET.E_ad_full.ToString());
                    meWrite.WriteLine("[SCT3] daz      = " + myXET.E_da_zero.ToString());
                    meWrite.WriteLine("[SCT3] daf      = " + myXET.E_da_full.ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_vtio).ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_wtio).ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_atio).ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_btio).ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ctio).ToString());
                    meWrite.WriteLine("[SCT3]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_dtio).ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_zero_4ma.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_full_20ma.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_zero_05V.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_full_05V.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_zero_10V.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_full_10V.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_zero_N5.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_full_P5.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_zero_N10.ToString());
                    meWrite.WriteLine("[SCT4] cal      = " + myXET.E_da_full_P10.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT5]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_corr).ToString());
                    meWrite.WriteLine("[SCT5]          = 0x" + myXET.E_mark.ToString("X2"));
                    meWrite.WriteLine("[SCT5]          = 0x" + myXET.E_sign.ToString("X2"));
                    meWrite.WriteLine("[SCT5] addr     = 0x" + myXET.E_addr.ToString("X2"));
                    switch (myXET.E_baud)
                    {
                        case 0: meWrite.WriteLine("[SCT5]          = 1200"); break;
                        case 1: meWrite.WriteLine("[SCT5]          = 2400"); break;
                        case 2: meWrite.WriteLine("[SCT5]          = 4800"); break;
                        case 3: meWrite.WriteLine("[SCT5]          = 9600"); break;
                        case 4: meWrite.WriteLine("[SCT5]          = 14400"); break;
                        case 5: meWrite.WriteLine("[SCT5]          = 19200"); break;
                        case 6: meWrite.WriteLine("[SCT5]          = 38400"); break;
                        case 7: meWrite.WriteLine("[SCT5]          = 57600"); break;
                        case 8: meWrite.WriteLine("[SCT5]          = 115200"); break;
                        case 9: meWrite.WriteLine("[SCT5]          = 230400"); break;
                        case 10: meWrite.WriteLine("[SCT5]          = 256000"); break;
                    }
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_stopbit.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_parity.ToString());
                    meWrite.WriteLine("[SCT5] wtz      = " + myXET.E_wt_zero.ToString());
                    meWrite.WriteLine("[SCT5] wtf      = " + myXET.E_wt_full.ToString());
                    meWrite.WriteLine("[SCT5] decimal  = " + myXET.E_wt_decimal.ToString());
                    meWrite.WriteLine("[SCT5] unit     = " + myXET.E_wt_unit.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_wt_ascii.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_wt_sptime.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_wt_spfilt.ToString());
                    meWrite.WriteLine("[SCT5] division = " + myXET.E_wt_division.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_wt_antivib.ToString());
                    meWrite.WriteLine("[SCT5] beat     = " + myXET.E_heartBeat.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_typeTPDO0.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_evenTPDO0.ToString());
                    meWrite.WriteLine("[SCT5] nodeID   = " + myXET.E_nodeID.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_nodeBaud.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_dynazero.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_cheatype.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_thmax.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_thmin.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_stablerange.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_stabletime.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_tkzerotime.ToString());
                    meWrite.WriteLine("[SCT5]          = " + myXET.E_tkdynatime.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_ad_point6.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_ad_point7.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_ad_point8.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_ad_point9.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_ad_point10.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_da_point6.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_da_point7.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_da_point8.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_da_point9.ToString());
                    meWrite.WriteLine("[SCT6]          = " + myXET.E_da_point10.ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input6).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input7).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input8).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input9).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input10).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog6).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog7).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog8).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog9).ToString());
                    meWrite.WriteLine("[SCT7]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog10).ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT8]          = " + myXET.E_ad_point11.ToString());
                    meWrite.WriteLine("[SCT8]          = " + myXET.E_da_point11.ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_input11).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_analog11).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_etio).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_ftio).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_gtio).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_htio).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_itio).ToString());
                    meWrite.WriteLine("[SCT8]          = " + MyDevice.myUIT.ConvertInt32ToFloat(myXET.E_jtio).ToString());
                    meWrite.WriteLine("");
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_enGFC.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_enSRDO.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_SCT_time.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_COB_ID1.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_COB_ID2.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_enOL.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_overload.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_alarmMode.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_wetTarget.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_wetLow.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_wetHigh.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_filter.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_netServicePort.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.GetIpAddressFromArray(myXET.E_netServiceIP));
                    meWrite.WriteLine("[SCT9]          = " + myXET.GetIpAddressFromArray(myXET.E_netClientIP));
                    meWrite.WriteLine("[SCT9]          = " + myXET.GetIpAddressFromArray(myXET.E_netGatIP));
                    meWrite.WriteLine("[SCT9]          = " + myXET.GetIpAddressFromArray(myXET.E_netMaskIP));
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_useDHCP.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_useScan.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_addrRF[0].ToString() + "," + myXET.E_addrRF[1].ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_spedRF.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_chanRF.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_optionRF.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_lockTPDO0.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_entrTPDO0.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_typeTPDO1.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_lockTPDO1.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_entrTPDO1.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_evenTPDO1.ToString());
                    meWrite.WriteLine("[SCT9]          = " + myXET.E_scaling.ToString("0.0"));
                }
                meWrite.WriteLine("");
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("[END]");
                meWrite.WriteLine("");
                meWrite.WriteLine("");
                //
                meWrite.Close();
                meFS.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存语言
        /// </summary>
        /// <param name="language"></param>
        public static void SaveLanguage(int language)
        {
            //空
            if (MyDevice.D_datPath == null)
            {
                return;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.D_datPath))
            {
                Directory.CreateDirectory(MyDevice.D_datPath);
            }

            //写入
            try
            {
                string mePath = MyDevice.D_datPath + @"\Language.txt";//设置文件路径
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                File.WriteAllText(mePath, language.ToString());
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 保存开机启动
        /// </summary>
        /// <param name="language"></param>
        public static void SaveAutoStart(bool isEnable)
        {
            //空
            if (MyDevice.D_datPath == null)
            {
                return;
            }
            //创建新路径
            else if (!Directory.Exists(MyDevice.D_datPath))
            {
                Directory.CreateDirectory(MyDevice.D_datPath);
            }

            //写入
            try
            {
                string mePath = MyDevice.D_datPath + @"\AutoStart.txt";//设置文件路径
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                if (isEnable)
                {
                    File.WriteAllText(mePath, "1");
                }
                else
                {
                    File.WriteAllText(mePath, "0");
                }
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 取枚举的描述文字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumByDescription<T>(string description) where T : Enum
        {
            System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
                if (objs.Length > 0 && (objs[0] as DescriptionAttribute).Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
        }

        //检查是否切换协议
        public static void mePort_ChangeProtocol(COMP comp)
        {
            if (MyDevice.protocol.type != comp)
            {
                //注意
                //如果重新new SelfUARTProtocol()或new RS485Protocol()
                //会因为protocol释放空间导致打开的mePort丢失句柄
                //所以new情况下必须mePort_Close();
                MyDevice.protocol.Protocol_PortClose();

                //
                switch (comp)
                {
                    case COMP.SelfUART:
                        MyDevice.protocol = mySelfUART;
                        break;

                    case COMP.RS485:
                        MyDevice.protocol = myRS485;
                        break;

                    case COMP.CANopen:
                        MyDevice.protocol = myCANopen;
                        break;

                    case COMP.ModbusTCP:
                        MyDevice.protocol = myModbusTCP;
                        break;
                }
            }
        }

        //打开CAN口
        public static void mePort_Open(UInt32 index, String name, Int32 baud)
        {
            protocol.Protocol_PortOpen(index, name, baud);
        }

        //打开串口
        public static void mePort_Open(String name, Int32 baud, StopBits stb, Parity pay)
        {
            protocol.Protocol_PortOpen(name, baud, stb, pay);
        }

        //关闭串口
        public static bool mePort_Close()
        {
            return protocol.Protocol_PortClose();
        }

        //清除串口任务
        public static void mePort_ClearState()
        {
            protocol.Protocol_ClearState();
        }

        //停止设备连续发送
        public static void mePort_StopDacout()
        {
            //发指令停止连续发送dacout
            if (MyDevice.protocol.trTASK == TASKS.DAC || MyDevice.protocol.trTASK == TASKS.ADC)
            {
                switch (MyDevice.protocol.type)
                {
                    default:
                    case COMP.SelfUART:
                        //自定义用0x67指令停连续发送
                        MyDevice.mePort_SendCOM(TASKS.DONE);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.DONE);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        break;

                    case COMP.RS485:
                        //RS485用BCC指令停连续发送
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        break;

                    case COMP.CANopen:
                        //CANopen的TASK.DAC和ADC是问答形式
                        break;

                    case COMP.ModbusTCP:
                        //EtherNet用BCC指令停连续发送
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        break;
                }
            }

            mePort_ClearState();
        }

        //停止设备连续发送
        public static void mePort_StopDacout(IProtocol ptl)
        {
            //发指令停止连续发送dacout
            if (ptl.trTASK == TASKS.DAC || ptl.trTASK == TASKS.ADC)
            {
                switch (ptl.type)
                {
                    default:
                    case COMP.SelfUART:
                        //自定义用0x67指令停连续发送
                        ptl.Protocol_SendCOM(TASKS.DONE);
                        Thread.Sleep(50);
                        ptl.Protocol_SendCOM(TASKS.DONE);
                        Thread.Sleep(50);
                        break;

                    case COMP.RS485:
                        //RS485用BCC指令停连续发送
                        ptl.Protocol_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        ptl.Protocol_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        break;

                    case COMP.CANopen:
                        //CANopen的TASK.DAC和ADC是问答形式
                        break;

                    case COMP.ModbusTCP:
                        //EtherNet用BCC指令停连续发送
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        MyDevice.mePort_SendCOM(TASKS.BCC);
                        Thread.Sleep(50);
                        break;
                }
            }

            ptl.Protocol_ClearState();
        }

        //发送读命令,参数提供任务类型
        public static void mePort_SendCOM(TASKS meTask)
        {
            protocol.Protocol_SendCOM(meTask);
        }

        /// <summary>
        /// 串口读取任务状态机 BOR -> RDX0 -> RDX1 -> RDX2 -> RDX3 -> RDX4 -> RDX5 -> RDX6 -> RDX7 -> RDX8
        /// </summary>
        public static void mePort_ReadTasks()
        {
            protocol.Protocol_mePort_ReadTasks();
        }

        //串口写入任务状态机 WRX0 -> RST
        //工厂校准-MenuFacUserForm
        //更新Bohrcode和Outype并重启
        public static void mePort_WriteTypTasks()
        {
            protocol.Protocol_mePort_WriteTypTasks();
        }

        //串口写入任务状态机 BCC -> WRX5 -> (rst)
        //参数设置-AI跟踪器-MenuParaCheatingForm
        //只更新SCT5
        public static void mePort_WriteParTasks()
        {
            protocol.Protocol_mePort_WriteParTasks();
        }

        //串口写入任务状态机 BCC -> WRX5 -> RST
        //参数设置-RS485参数-MenuParaRS485Form
        //如果修改了串口且RS485连接方式的需要重启
        public static void mePort_WriteBusTasks()
        {
            protocol.Protocol_mePort_WriteBusTasks();
        }

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst)
        //工厂校准-MenuFacUserForm
        //校准DAC更新后同步更新SCT123的da_point
        public static void mePort_WriteFacTasks()
        {
            protocol.Protocol_mePort_WriteFacTasks();
        }

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX6 -> WRX7 -> WRX8 -> (rst)
        //标定设置-标定传感器-MenuSetCalForm
        //标定设置-修正传感器-MenuSetCorrForm
        //主要更新input和ad_point和da_point和analog参数
        public static void mePort_WriteCalTasks()
        {
            protocol.Protocol_mePort_WriteCalTasks();
        }

        //串口写入任务状态机 BCC -> WRX0 -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> WRX5 -> WRX6 -> WRX7 -> WRX8 -> RST
        //参数设置-标定参数-MenuSetParaForm
        //修改SCT0和SCT5参数后,也需要换算更新其它所有SCT参数
        public static void mePort_WriteAllTasks()
        {
            protocol.Protocol_mePort_WriteAllTasks();
        }

        //设备数据拷贝,站点变更后设备数据迁移
        public static void mBUS_DeepCopy(Byte target, Byte org)
        {
            DeepCopy(mBUS, mBUS, target, org);
        }

        //设备数据拷贝,站点变更后设备数据迁移
        public static void mCAN_DeepCopy(Byte target, Byte org)
        {
            DeepCopy(mCAN, mCAN, target, org);
        }

        //设备数据拷贝,站点变更后设备数据迁移
        public static void mMTCP_DeepCopy(Byte target, Byte org)
        {
            DeepCopy(mMTCP, mMTCP, target, org);
        }

        // 设备数据拷贝，站点变更后设备数据迁移
        public static void DeepCopy<T>(T[] targetArray, T[] orgArray, Byte target, Byte org) where T : XET
        {
            if (target == org) return;
            //非SCT的数据至少要迁移R_bohrcode_long
            targetArray[target].R_bohrcode_long = orgArray[org].R_bohrcode_long;
            targetArray[target].R_errSensor = orgArray[org].R_errSensor;
            targetArray[target].R_resolution = orgArray[org].R_resolution;

            targetArray[target].E_test = orgArray[org].E_test;
            targetArray[target].E_outype = orgArray[org].E_outype;
            targetArray[target].E_curve = orgArray[org].E_curve;
            targetArray[target].E_adspeed = orgArray[org].E_adspeed;
            targetArray[target].E_autozero = orgArray[org].E_autozero;
            targetArray[target].E_trackzero = orgArray[org].E_trackzero;
            targetArray[target].E_checkhigh = orgArray[org].E_checkhigh;
            targetArray[target].E_checklow = orgArray[org].E_checklow;
            targetArray[target].E_mfg_date = orgArray[org].E_mfg_date;
            targetArray[target].E_mfg_srno = orgArray[org].E_mfg_srno;
            targetArray[target].E_tmp_min = orgArray[org].E_tmp_min;
            targetArray[target].E_tmp_max = orgArray[org].E_tmp_max;
            targetArray[target].E_tmp_cal = orgArray[org].E_tmp_cal;
            targetArray[target].E_bohrcode = orgArray[org].E_bohrcode;
            targetArray[target].E_enspan = orgArray[org].E_enspan;
            targetArray[target].E_protype = orgArray[org].E_protype;

            targetArray[target].E_ad_point1 = orgArray[org].E_ad_point1;
            targetArray[target].E_ad_point2 = orgArray[org].E_ad_point2;
            targetArray[target].E_ad_point3 = orgArray[org].E_ad_point3;
            targetArray[target].E_ad_point4 = orgArray[org].E_ad_point4;
            targetArray[target].E_ad_point5 = orgArray[org].E_ad_point5;
            targetArray[target].E_da_point1 = orgArray[org].E_da_point1;
            targetArray[target].E_da_point2 = orgArray[org].E_da_point2;
            targetArray[target].E_da_point3 = orgArray[org].E_da_point3;
            targetArray[target].E_da_point4 = orgArray[org].E_da_point4;
            targetArray[target].E_da_point5 = orgArray[org].E_da_point5;

            targetArray[target].E_input1 = orgArray[org].E_input1;
            targetArray[target].E_input2 = orgArray[org].E_input2;
            targetArray[target].E_input3 = orgArray[org].E_input3;
            targetArray[target].E_input4 = orgArray[org].E_input4;
            targetArray[target].E_input5 = orgArray[org].E_input5;
            targetArray[target].E_analog1 = orgArray[org].E_analog1;
            targetArray[target].E_analog2 = orgArray[org].E_analog2;
            targetArray[target].E_analog3 = orgArray[org].E_analog3;
            targetArray[target].E_analog4 = orgArray[org].E_analog4;
            targetArray[target].E_analog5 = orgArray[org].E_analog5;

            targetArray[target].E_ad_zero = orgArray[org].E_ad_zero;
            targetArray[target].E_ad_full = orgArray[org].E_ad_full;
            targetArray[target].E_da_zero = orgArray[org].E_da_zero;
            targetArray[target].E_da_full = orgArray[org].E_da_full;
            targetArray[target].E_vtio = orgArray[org].E_vtio;
            targetArray[target].E_wtio = orgArray[org].E_wtio;
            targetArray[target].E_atio = orgArray[org].E_atio;
            targetArray[target].E_btio = orgArray[org].E_btio;
            targetArray[target].E_ctio = orgArray[org].E_ctio;
            targetArray[target].E_dtio = orgArray[org].E_dtio;

            targetArray[target].E_da_zero_4ma = orgArray[org].E_da_zero_4ma;
            targetArray[target].E_da_full_20ma = orgArray[org].E_da_full_20ma;
            targetArray[target].E_da_zero_05V = orgArray[org].E_da_zero_05V;
            targetArray[target].E_da_full_05V = orgArray[org].E_da_full_05V;
            targetArray[target].E_da_zero_10V = orgArray[org].E_da_zero_10V;
            targetArray[target].E_da_full_10V = orgArray[org].E_da_full_10V;
            targetArray[target].E_da_zero_N5 = orgArray[org].E_da_zero_N5;
            targetArray[target].E_da_full_P5 = orgArray[org].E_da_full_P5;
            targetArray[target].E_da_zero_N10 = orgArray[org].E_da_zero_N10;
            targetArray[target].E_da_full_P10 = orgArray[org].E_da_full_P10;

            targetArray[target].E_corr = orgArray[org].E_corr;
            targetArray[target].E_mark = orgArray[org].E_mark;
            targetArray[target].E_sign = orgArray[org].E_sign;
            targetArray[target].E_addr = orgArray[org].E_addr;
            targetArray[target].E_baud = orgArray[org].E_baud;
            targetArray[target].E_stopbit = orgArray[org].E_stopbit;
            targetArray[target].E_parity = orgArray[org].E_parity;
            targetArray[target].E_wt_zero = orgArray[org].E_wt_zero;
            targetArray[target].E_wt_full = orgArray[org].E_wt_full;
            targetArray[target].E_wt_decimal = orgArray[org].E_wt_decimal;
            targetArray[target].E_wt_unit = orgArray[org].E_wt_unit;
            targetArray[target].E_wt_ascii = orgArray[org].E_wt_ascii;
            targetArray[target].E_wt_sptime = orgArray[org].E_wt_sptime;
            targetArray[target].E_wt_spfilt = orgArray[org].E_wt_spfilt;
            targetArray[target].E_wt_division = orgArray[org].E_wt_division;
            targetArray[target].E_wt_antivib = orgArray[org].E_wt_antivib;
            targetArray[target].E_heartBeat = orgArray[org].E_heartBeat;
            targetArray[target].E_typeTPDO0 = orgArray[org].E_typeTPDO0;
            targetArray[target].E_evenTPDO0 = orgArray[org].E_evenTPDO0;
            targetArray[target].E_nodeID = orgArray[org].E_nodeID;
            targetArray[target].E_nodeBaud = orgArray[org].E_nodeBaud;
            targetArray[target].E_dynazero = orgArray[org].E_dynazero;
            targetArray[target].E_cheatype = orgArray[org].E_cheatype;
            targetArray[target].E_thmax = orgArray[org].E_thmax;
            targetArray[target].E_thmin = orgArray[org].E_thmin;
            targetArray[target].E_stablerange = orgArray[org].E_stablerange;
            targetArray[target].E_stabletime = orgArray[org].E_stabletime;
            targetArray[target].E_tkzerotime = orgArray[org].E_tkzerotime;
            targetArray[target].E_tkdynatime = orgArray[org].E_tkdynatime;

            targetArray[target].E_ad_point6 = orgArray[org].E_ad_point6;
            targetArray[target].E_ad_point7 = orgArray[org].E_ad_point7;
            targetArray[target].E_ad_point8 = orgArray[org].E_ad_point8;
            targetArray[target].E_ad_point9 = orgArray[org].E_ad_point9;
            targetArray[target].E_ad_point10 = orgArray[org].E_ad_point10;
            targetArray[target].E_da_point6 = orgArray[org].E_da_point6;
            targetArray[target].E_da_point7 = orgArray[org].E_da_point7;
            targetArray[target].E_da_point8 = orgArray[org].E_da_point8;
            targetArray[target].E_da_point9 = orgArray[org].E_da_point9;
            targetArray[target].E_da_point10 = orgArray[org].E_da_point10;

            targetArray[target].E_input6 = orgArray[org].E_input6;
            targetArray[target].E_input7 = orgArray[org].E_input7;
            targetArray[target].E_input8 = orgArray[org].E_input8;
            targetArray[target].E_input9 = orgArray[org].E_input9;
            targetArray[target].E_input10 = orgArray[org].E_input10;
            targetArray[target].E_analog6 = orgArray[org].E_analog6;
            targetArray[target].E_analog7 = orgArray[org].E_analog7;
            targetArray[target].E_analog8 = orgArray[org].E_analog8;
            targetArray[target].E_analog9 = orgArray[org].E_analog9;
            targetArray[target].E_analog10 = orgArray[org].E_analog10;

            targetArray[target].E_ad_point11 = orgArray[org].E_ad_point11;
            targetArray[target].E_da_point11 = orgArray[org].E_da_point11;
            targetArray[target].E_input11 = orgArray[org].E_input11;
            targetArray[target].E_analog11 = orgArray[org].E_analog11;
            targetArray[target].E_etio = orgArray[org].E_etio;
            targetArray[target].E_ftio = orgArray[org].E_ftio;
            targetArray[target].E_gtio = orgArray[org].E_gtio;
            targetArray[target].E_htio = orgArray[org].E_htio;
            targetArray[target].E_itio = orgArray[org].E_itio;
            targetArray[target].E_jtio = orgArray[org].E_jtio;

            targetArray[target].E_enGFC = orgArray[org].E_enGFC;
            targetArray[target].E_enSRDO = orgArray[org].E_enSRDO;
            targetArray[target].E_SCT_time = orgArray[org].E_SCT_time;
            targetArray[target].E_COB_ID1 = orgArray[org].E_COB_ID1;
            targetArray[target].E_COB_ID2 = orgArray[org].E_COB_ID2;
            targetArray[target].E_enOL = orgArray[org].E_enOL;
            targetArray[target].E_overload = orgArray[org].E_overload;
            targetArray[target].E_alarmMode = orgArray[org].E_alarmMode;
            targetArray[target].E_wetTarget = orgArray[org].E_wetTarget;
            targetArray[target].E_wetLow = orgArray[org].E_wetLow;
            targetArray[target].E_wetHigh = orgArray[org].E_wetHigh;
            targetArray[target].E_filter = orgArray[org].E_filter;
            targetArray[target].E_netServicePort = orgArray[org].E_netServicePort;
            targetArray[target].E_netServiceIP = orgArray[org].E_netServiceIP;
            targetArray[target].E_netClientIP = orgArray[org].E_netClientIP;
            targetArray[target].E_netGatIP = orgArray[org].E_netGatIP;
            targetArray[target].E_netMaskIP = orgArray[org].E_netMaskIP;
            targetArray[target].E_useDHCP = orgArray[org].E_useDHCP;
            targetArray[target].E_useScan = orgArray[org].E_useScan;
            targetArray[target].E_addrRF = orgArray[org].E_addrRF;
            targetArray[target].E_spedRF = orgArray[org].E_spedRF;
            targetArray[target].E_chanRF = orgArray[org].E_chanRF;
            targetArray[target].E_optionRF = orgArray[org].E_optionRF;
            targetArray[target].E_lockTPDO0 = orgArray[org].E_lockTPDO0;
            targetArray[target].E_entrTPDO0 = orgArray[org].E_entrTPDO0;
            targetArray[target].E_typeTPDO1 = orgArray[org].E_typeTPDO1;
            targetArray[target].E_lockTPDO1 = orgArray[org].E_lockTPDO1;
            targetArray[target].E_entrTPDO1 = orgArray[org].E_entrTPDO1;
            targetArray[target].E_evenTPDO1 = orgArray[org].E_evenTPDO1;
            targetArray[target].E_scaling = orgArray[org].E_scaling;
        }

        /// <summary>
        /// 依据IP地址和e_addr获取mMTCP的index
        /// </summary>
        /// <returns></returns>
        public static int FindClientIndexByEndpointAndAddr(string ipAddr, byte addr)
        {
            int foundIndex = Array.FindIndex(MyDevice.mMTCP, x => x.R_ipAddr == ipAddr && x.E_addr == addr);
            return foundIndex;
        }

        /// <summary>
        /// 将ip地址加入到mMTCP
        /// 仅连接界面读bohrcode及扫描读地址时用
        /// 其他地方使用可能会导致mMTCP中存入从未在连接界面连接的设备
        /// </summary>
        public static int AddEndpointToFirstNonEmptymMTCP(string ipAddr, byte addr)
        {
            int foundIndex = FindClientIndexByEndpointAndAddr(ipAddr, addr);  //查找是否已存在对应ip地址和e_addr的设备
            if (foundIndex == -1)                                             //不存在
            {
                int newIndex = Array.FindIndex(MyDevice.mMTCP, x => string.IsNullOrEmpty(x.R_ipAddr));  //将第一个R_endPoint不为空的元素的下标作为新下标
                if (newIndex != -1)
                {
                    MyDevice.mMTCP[newIndex].R_ipAddr = ipAddr;               //更新nMTCP
                    MyDevice.mMTCP[newIndex].E_addr = addr;                   //更新nMTCP 
                }
                else
                {
                    //记录的ip和addr配对数已经超过mMTCP的长度
                    //mMTCP中没有未使用过的元素了
                }
                return newIndex;                                             //返回新加入的元素的下标
            }
            else
            {
                return foundIndex;                                            //不加入，返回原有下标
            }
        }
    }
}

