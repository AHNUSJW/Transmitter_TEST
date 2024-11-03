using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

//未经过审批不得改动

//Alvin 20230320
//Alvin 20230704

//自定义协议用mySelfUART
//双通道T2X420的自定义协议用mySelfUART和mySecondUART
//八通道T8X420需要用myRS485通过地址交换数据

namespace Model
{
    public class SecondUARTProtocol : IProtocol
    {
        #region 定义变量

        //
        //串口在收发字节时关闭会死锁
        //https://blog.csdn.net/guomei1345/article/details/80736721
        //https://blog.csdn.net/sinat_23338865/article/details/52596818
        //解决多线程死锁问题,要用volatile关键字
        private volatile bool is_serial_listening = false;  //串口正在监听标记
        private volatile bool is_serial_closing = false;    //串口正在关闭标记

        //
        private SerialPort mePort = new SerialPort();   //自定义接口使用的串口
        private volatile TASKS trTSK = TASKS.NULL;      //接口读写任务状态机
        private RXSTP rxSTP = RXSTP.NUL;                //通讯接收状态机,找出帧头帧尾
        private Int32 txCnt = 0;                        //发送计数
        private Int32 rxCnt = 0;                        //接收计数
        private Int32 rxDat = 0;                        //接收数据数值
        private String rxStr = null;                    //接收数据字符串
        private Boolean isEQ = false;                   //接收检查

        //
        private Byte[] meTXD = new Byte[200];           //发送缓冲区
        private Byte[] meRXD = new Byte[200];           //接收缓冲区
        private Int32 rxRead = 0;                       //接收缓冲区读指针
        private Int32 rxWrite = 0;                      //接收缓冲区写指针

        #endregion

        #region 定义属性

        public Byte addr
        {
            set
            {
            }
            get
            {
                return MyDevice.mSXT.E_addr;
            }
        }
        public COMP type
        {
            get
            {
                return COMP.SelfUART;
            }
        }
        public bool Is_serial_listening
        {
            set
            {
                is_serial_listening = value;
            }
            get
            {
                return is_serial_listening;
            }
        }
        public bool Is_serial_closing
        {
            set
            {
                is_serial_closing = value;
            }
            get
            {
                return is_serial_closing;
            }
        }
        public String portName
        {
            get
            {
                return mePort.PortName;
            }
        }
        public Int32 baudRate
        {
            get
            {
                return mePort.BaudRate;
            }
        }
        public StopBits stopBits
        {
            get
            {
                return mePort.StopBits;
            }
        }
        public Parity parity
        {
            get
            {
                return mePort.Parity;
            }
        }
        public UInt32 channel
        {
            get
            {
                return 0;
            }
        }
        public string ipAddr
        {
            set
            {
            }
            get
            {
                return "";
            }
        }
        public Boolean IsOpen
        {
            get
            {
                return mePort.IsOpen;
            }
        }
        public TASKS trTASK
        {
            set
            {
                trTSK = value;
            }
            get
            {
                return trTSK;
            }
        }
        public Int32 txCount
        {
            get
            {
                return txCnt;
            }
        }
        public Int32 rxCount
        {
            get
            {
                return rxCnt;
            }
        }
        public Int32 rxData
        {
            get
            {
                return rxDat;
            }
        }
        public String rxString
        {
            get
            {
                return rxStr;
            }
        }
        public Boolean isEqual
        {
            get
            {
                return isEQ;
            }
        }

        #endregion

        //构造函数
        public SecondUARTProtocol()
        {
            //
            mePort.PortName = "COM1";
            mePort.BaudRate = Convert.ToInt32(BAUT.B19200); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.None; //校验位固定
            mePort.ReceivedBytesThreshold = 1; //接收即通知

            //
            trTSK = TASKS.NULL;
            rxSTP = RXSTP.NUL;
            txCnt = 0;
            rxCnt = 0;
            rxDat = 0;
            rxStr = null;
            isEQ = false;

            //
            Array.Clear(meTXD, 0, meTXD.Length);
            Array.Clear(meRXD, 0, meRXD.Length);
            rxRead = 0;
            rxWrite = 0;
        }

        //不是net
        public void Protocol_PortOpen(string ip, Int32 port = 5678)
        {

        }

        //不是CAN
        public void Protocol_PortOpen(UInt32 index, String name, Int32 baud)
        {

        }

        //打开串口
        public void Protocol_PortOpen(String name, Int32 baud, StopBits stb, Parity pay)
        {
            //修改参数必须先关闭串口
            if ((mePort.PortName != name) || (mePort.BaudRate != baud))
            {
                if (mePort.IsOpen)
                {
                    try
                    {
                        //初始化串口监听标记
                        is_serial_listening = false;
                        is_serial_closing = true;

                        //取消异步任务
                        //https://www.cnblogs.com/wucy/p/15128365.html
                        CancellationTokenSource cts = new CancellationTokenSource();
                        cts.Cancel();
                        mePort.DiscardInBuffer();
                        mePort.DiscardOutBuffer();
                        mePort.Close();
                    }
                    catch
                    {
                    }
                }
            }

            //尝试打开串口
            try
            {
                //初始化串口监听标记
                is_serial_listening = false;
                is_serial_closing = false;

                //
                mePort.PortName = name;
                mePort.BaudRate = baud;
                mePort.DataBits = 8; //数据位固定
                mePort.StopBits = StopBits.One; //停止位固定
                mePort.Parity = Parity.None; //校验位固定
                mePort.ReceivedBytesThreshold = 1; //接收即通知
                mePort.Open();
                Task task = new Task(mePort_DataReceived); //接收处理
                task.Start();

                //
                trTSK = TASKS.NULL;
                rxSTP = RXSTP.NUL;
                txCnt = 0;
                rxCnt = 0;
                rxDat = 0;
                rxStr = "";
                isEQ = false;
                rxRead = 0;
                rxWrite = 0;
            }
            catch
            {

            }
        }

        //关闭串口
        public bool Protocol_PortClose()
        {
            if (mePort.IsOpen)
            {
                try
                {
                    //取消异步任务
                    //https://www.cnblogs.com/wucy/p/15128365.html
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.Cancel();

                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();

                    //串口在收发字节时关闭会死锁,
                    //https://blog.csdn.net/guomei1345/article/details/80736721
                    //https://blog.csdn.net/sinat_23338865/article/details/52596818
                    mePort.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        //清除串口任务
        public void Protocol_ClearState()
        {
            trTASK = TASKS.NULL;
            rxSTP = RXSTP.NUL;

            rxRead = 0;
            rxWrite = 0;

            if (mePort.IsOpen)
            {
                mePort.DiscardInBuffer();
                mePort.DiscardOutBuffer();
            }

            for (int i = 0; i < 200; i++)
            {
                meTXD[i] = 0;
                meRXD[i] = 0;
            }
        }

        //刷新IsEQ
        public void Protocol_ChangeEQ()
        {
            isEQ = false;
        }

        //发送读命令
        public void Protocol_SendCOM(TASKS meTask)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            switch (meTask)
            {
                //Borcode
                case TASKS.BOR:
                case TASKS.BCC:
                    //原则上trTSK发送指令和rxSTP接收校验独立管理
                    //设备连续发送dacout的接收字节不确定
                    //在mePort_ReceiveLong中rxSTP状态不确定
                    //读出流程从BOR开始,rxSTP为NUL才会往下走
                    //写入流程都从BCC开始,rxSTP为NUL才会往下走
                    //因此在BOR和BCC清除一下rxSTP
                    rxSTP = RXSTP.NUL;
                    (new Byte[] { Constants.STAR, Constants.CODE, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //读SCT
                case TASKS.RDX0:
                    (new Byte[] { Constants.STAR, Constants.RDX0, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX1:
                    (new Byte[] { Constants.STAR, Constants.RDX1, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX2:
                    (new Byte[] { Constants.STAR, Constants.RDX2, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX3:
                    (new Byte[] { Constants.STAR, Constants.RDX3, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX4:
                    (new Byte[] { Constants.STAR, Constants.RDX4, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX5:
                    (new Byte[] { Constants.STAR, Constants.RDX5, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX6:
                    (new Byte[] { Constants.STAR, Constants.RDX6, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX7:
                    (new Byte[] { Constants.STAR, Constants.RDX7, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX8:
                    (new Byte[] { Constants.STAR, Constants.RDX8, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //采集
                case TASKS.ADCP1:
                case TASKS.ADCP2:
                case TASKS.ADCP3:
                case TASKS.ADCP4:
                case TASKS.ADCP5:
                case TASKS.ADCP6:
                case TASKS.ADCP7:
                case TASKS.ADCP8:
                case TASKS.ADCP9:
                case TASKS.ADCP10:
                case TASKS.ADCP11:
                    (new Byte[] { Constants.STAR, Constants.PONT, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //校准
                case TASKS.GODMZ:
                    (new Byte[] { Constants.STAR, Constants.GODMZ, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GOUPZ:
                    (new Byte[] { Constants.STAR, Constants.GOUPZ, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GODMM:
                    (new Byte[] { Constants.STAR, Constants.GODMM, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GOUPM:
                    (new Byte[] { Constants.STAR, Constants.GOUPM, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GODMF:
                    (new Byte[] { Constants.STAR, Constants.GODMF, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GOUPF:
                    (new Byte[] { Constants.STAR, Constants.GOUPF, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.DONE:
                    (new Byte[] { Constants.STAR, Constants.DONE, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //归零,标定,重启
                case TASKS.TARE:
                case TASKS.ZERO:
                    (new Byte[] { Constants.STAR, Constants.CALZ, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.SPAN:
                    (new Byte[] { Constants.STAR, Constants.CALF, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.REST:
                    (new Byte[] { Constants.STAR, Constants.REST, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //adcout,dacout
                case TASKS.ADC:
                    (new Byte[] { Constants.STAR, Constants.AUTA, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.DAC:
                    (new Byte[] { Constants.STAR, Constants.DACO, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                //滤波参数
                case TASKS.SFLT:
                    (new Byte[] { Constants.STAR, Constants.SFLT, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RFLT:
                    (new Byte[] { Constants.STAR, Constants.RFLT, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDFT:
                    (new Byte[] { Constants.STAR, Constants.RDFT, Constants.sLen, Constants.STOP }).CopyTo(meTXD, 0);
                    break;

                default:
                    return;
            }

            //
            mePort.Write(meTXD, 0, 4);
            txCnt += 4;
            trTSK = meTask;
            isEQ = false;
        }

        //不是RS485
        public void Protocol_SendAddr(Byte addr)
        {
        }

        //不是RS485
        public void Protocol_HwSetAddr(Byte addr)
        {
        }

        //不是RS485
        public void Protocol_SwSetAddr(Byte addr, UInt32 weight)
        {
        }

        //发送写入SCT0命令
        private void Protocol_SendSCT0()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX0;
            meTXD[2] = MyDevice.mSXT.E_test;
            meTXD[3] = MyDevice.mSXT.E_outype;
            meTXD[4] = MyDevice.mSXT.E_curve;
            meTXD[5] = MyDevice.mSXT.E_adspeed;
            meTXD[6] = MyDevice.mSXT.E_autozero;
            meTXD[7] = MyDevice.mSXT.E_trackzero;
            MyDevice.myUIT.I = MyDevice.mSXT.E_checkhigh;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.myUIT.B2;
            meTXD[11] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_checklow;
            meTXD[12] = MyDevice.myUIT.B0;
            meTXD[13] = MyDevice.myUIT.B1;
            meTXD[14] = MyDevice.myUIT.B2;
            meTXD[15] = MyDevice.myUIT.B3;
            MyDevice.myUIT.UI = MyDevice.mSXT.E_mfg_date;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_mfg_srno;
            meTXD[20] = MyDevice.myUIT.B0;
            meTXD[21] = MyDevice.myUIT.B1;
            meTXD[22] = MyDevice.myUIT.B2;
            meTXD[23] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_tmp_min;
            meTXD[24] = MyDevice.myUIT.B0;
            meTXD[25] = MyDevice.myUIT.B1;
            meTXD[26] = MyDevice.myUIT.B2;
            meTXD[27] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_tmp_max;
            meTXD[28] = MyDevice.myUIT.B0;
            meTXD[29] = MyDevice.myUIT.B1;
            meTXD[30] = MyDevice.myUIT.B2;
            meTXD[31] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_tmp_cal;
            meTXD[32] = MyDevice.myUIT.B0;
            meTXD[33] = MyDevice.myUIT.B1;
            meTXD[34] = MyDevice.myUIT.B2;
            meTXD[35] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_bohrcode;
            meTXD[36] = MyDevice.myUIT.B0;
            meTXD[37] = MyDevice.myUIT.B1;
            meTXD[38] = MyDevice.myUIT.B2;
            meTXD[39] = MyDevice.myUIT.B3;
            meTXD[40] = MyDevice.mSXT.E_enspan;
            meTXD[41] = MyDevice.mSXT.E_protype;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX0;
        }

        //发送写入SCT1命令
        private void Protocol_SendSCT1()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX1;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX1;
        }

        //发送写入SCT2命令
        private void Protocol_SendSCT2()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX2;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX2;
        }

        //发送写入SCT3命令
        private void Protocol_SendSCT3()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_zero;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_full;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_vtio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_wtio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_atio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_btio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ctio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_dtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX3;
        }

        //发送写入SCT4命令
        private void Protocol_SendSCT4()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX4;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero_4ma;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full_20ma;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero_05V;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full_05V;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero_10V;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full_10V;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero_N5;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full_P5;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_zero_N10;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_full_P10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX4;
        }

        //发送写入SCT5命令
        private void Protocol_SendSCT5()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX5;
            MyDevice.myUIT.I = MyDevice.mSXT.E_corr;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            meTXD[6] = MyDevice.mSXT.E_mark;
            meTXD[7] = MyDevice.mSXT.E_sign;
            meTXD[8] = MyDevice.mSXT.E_addr;
            meTXD[9] = MyDevice.mSXT.E_baud;
            meTXD[10] = MyDevice.mSXT.E_stopbit;
            meTXD[11] = MyDevice.mSXT.E_parity;
            MyDevice.mSXT.E_wt_zero = 0;//强制写0
            meTXD[12] = 0;
            meTXD[13] = 0;
            meTXD[14] = 0;
            meTXD[15] = 0;
            MyDevice.myUIT.I = MyDevice.mSXT.E_wt_full;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            meTXD[20] = MyDevice.mSXT.E_wt_decimal;
            meTXD[21] = MyDevice.mSXT.E_wt_unit;
            meTXD[22] = MyDevice.mSXT.E_wt_ascii;
            meTXD[23] = MyDevice.mSXT.E_wt_sptime;
            meTXD[24] = MyDevice.mSXT.E_wt_spfilt;
            meTXD[25] = MyDevice.mSXT.E_wt_division;
            meTXD[26] = MyDevice.mSXT.E_wt_antivib;
            MyDevice.myUIT.I = MyDevice.mSXT.E_heartBeat;
            meTXD[27] = MyDevice.myUIT.B0;
            meTXD[28] = MyDevice.myUIT.B1;
            meTXD[29] = MyDevice.mSXT.E_typeTPDO0;
            MyDevice.myUIT.I = MyDevice.mSXT.E_evenTPDO0;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.mSXT.E_nodeID;
            meTXD[33] = MyDevice.mSXT.E_nodeBaud;
            meTXD[34] = MyDevice.mSXT.E_dynazero;
            meTXD[35] = MyDevice.mSXT.E_cheatype;
            meTXD[36] = MyDevice.mSXT.E_thmax;
            meTXD[37] = MyDevice.mSXT.E_thmin;
            meTXD[38] = MyDevice.mSXT.E_stablerange;
            meTXD[39] = MyDevice.mSXT.E_stabletime;
            meTXD[40] = MyDevice.mSXT.E_tkzerotime;
            meTXD[41] = MyDevice.mSXT.E_tkdynatime;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX5;
        }

        //发送写入SCT6命令
        private void Protocol_SendSCT6()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX6;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX6;
        }

        //发送写入SCT7命令
        private void Protocol_SendSCT7()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX7;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX7;
        }

        //发送写入SCT8命令
        private void Protocol_SendSCT8()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX8;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ad_point11;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_da_point11;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_input11;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_analog11;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_etio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_ftio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_gtio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_htio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_itio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_jtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;
            mePort.Write(meTXD, 0, 44);
            txCnt += 44;
            trTSK = TASKS.WRX8;
        }

        //发送写入SCT9命令
        private void Protocol_SendSCT9()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.BytesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX9;
            meTXD[2] = MyDevice.mSXT.E_enGFC;
            meTXD[3] = MyDevice.mSXT.E_enSRDO;
            MyDevice.myUIT.I = MyDevice.mSXT.E_SCT_time;
            meTXD[4] = MyDevice.myUIT.B0;
            meTXD[5] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mSXT.E_COB_ID1;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mSXT.E_COB_ID2;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.mSXT.E_enOL;
            meTXD[11] = MyDevice.mSXT.E_overload;
            meTXD[12] = MyDevice.mSXT.E_alarmMode;
            MyDevice.myUIT.I = MyDevice.mSXT.E_wetTarget;
            meTXD[13] = MyDevice.myUIT.B0;
            meTXD[14] = MyDevice.myUIT.B1;
            meTXD[15] = MyDevice.myUIT.B2;
            meTXD[16] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_wetLow;
            meTXD[17] = MyDevice.myUIT.B0;
            meTXD[18] = MyDevice.myUIT.B1;
            meTXD[19] = MyDevice.myUIT.B2;
            meTXD[20] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_wetHigh;
            meTXD[21] = MyDevice.myUIT.B0;
            meTXD[22] = MyDevice.myUIT.B1;
            meTXD[23] = MyDevice.myUIT.B2;
            meTXD[24] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_filter;
            meTXD[25] = MyDevice.myUIT.B0;
            meTXD[26] = MyDevice.myUIT.B1;
            meTXD[27] = MyDevice.myUIT.B2;
            meTXD[28] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mSXT.E_netServicePort;
            meTXD[29] = MyDevice.myUIT.B0;
            meTXD[30] = MyDevice.myUIT.B1;
            meTXD[31] = MyDevice.mSXT.E_netServiceIP[0];
            meTXD[32] = MyDevice.mSXT.E_netServiceIP[1];
            meTXD[33] = MyDevice.mSXT.E_netServiceIP[2];
            meTXD[34] = MyDevice.mSXT.E_netServiceIP[3];
            meTXD[35] = MyDevice.mSXT.E_netClientIP[0];
            meTXD[36] = MyDevice.mSXT.E_netClientIP[1];
            meTXD[37] = MyDevice.mSXT.E_netClientIP[2];
            meTXD[38] = MyDevice.mSXT.E_netClientIP[3];
            meTXD[39] = MyDevice.mSXT.E_netGatIP[0];
            meTXD[40] = MyDevice.mSXT.E_netGatIP[1];
            meTXD[41] = MyDevice.mSXT.E_netGatIP[2];
            meTXD[42] = MyDevice.mSXT.E_netGatIP[3];
            meTXD[43] = MyDevice.mSXT.E_netMaskIP[0];
            meTXD[44] = MyDevice.mSXT.E_netMaskIP[1];
            meTXD[45] = MyDevice.mSXT.E_netMaskIP[2];
            meTXD[46] = MyDevice.mSXT.E_netMaskIP[3];
            meTXD[47] = MyDevice.mSXT.E_useDHCP;
            meTXD[48] = MyDevice.mSXT.E_useScan;
            meTXD[49] = MyDevice.mSXT.E_addrRF[0];
            meTXD[50] = MyDevice.mSXT.E_addrRF[1];
            meTXD[51] = MyDevice.mSXT.E_spedRF;
            meTXD[52] = MyDevice.mSXT.E_chanRF;
            meTXD[53] = MyDevice.mSXT.E_optionRF;
            MyDevice.myUIT.I = MyDevice.mSXT.E_lockTPDO0;
            meTXD[54] = MyDevice.myUIT.B0;
            meTXD[55] = MyDevice.myUIT.B1;
            meTXD[56] = MyDevice.mSXT.E_entrTPDO0;
            meTXD[57] = MyDevice.mSXT.E_typeTPDO1;
            MyDevice.myUIT.I = MyDevice.mSXT.E_lockTPDO1;
            meTXD[58] = MyDevice.myUIT.B0;
            meTXD[59] = MyDevice.myUIT.B1;
            meTXD[60] = MyDevice.mSXT.E_entrTPDO1;
            MyDevice.myUIT.I = MyDevice.mSXT.E_evenTPDO1;
            meTXD[61] = MyDevice.myUIT.B0;
            meTXD[62] = MyDevice.myUIT.B1;
            MyDevice.myUIT.F = MyDevice.mSXT.E_scaling;
            meTXD[63] = MyDevice.myUIT.B0;
            meTXD[64] = MyDevice.myUIT.B1;
            meTXD[65] = MyDevice.myUIT.B2;
            meTXD[66] = MyDevice.myUIT.B3;
            for (int i = 67; i <= 103; i++)
            {
                meTXD[i] = 0xFF;
            }
            //

            //
            meTXD[106] = Constants.nLen;
            meTXD[107] = Constants.STOP;

            mePort.Write(meTXD, 0, 108);
            txCnt += 108;
            trTSK = TASKS.WRX9;
        }

        //串口读取SCT0
        private void Protocol_GetSCT0()
        {
            MyDevice.mSXT.E_test = meRXD[rxRead++];
            MyDevice.mSXT.E_outype = meRXD[rxRead++];
            MyDevice.mSXT.E_curve = meRXD[rxRead++];
            MyDevice.mSXT.E_adspeed = meRXD[rxRead++];
            MyDevice.mSXT.E_autozero = meRXD[rxRead++];
            MyDevice.mSXT.E_trackzero = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_checkhigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_checklow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_mfg_date = MyDevice.myUIT.UI;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_mfg_srno = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_tmp_min = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_tmp_max = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_tmp_cal = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_bohrcode != MyDevice.myUIT.I)
            {
                //序列号
                MyDevice.mySN = MyDevice.mySN + 1;
                //取得code
                MyDevice.mSXT.E_bohrcode = MyDevice.myUIT.I;
            }
            MyDevice.mSXT.E_enspan = meRXD[rxRead++];
            MyDevice.mSXT.E_protype = meRXD[rxRead++];
            //
            if (MyDevice.mSXT.E_test < 0x58)
            {
                if ((MyDevice.mSXT.E_protype == 0xFF) || (MyDevice.mSXT.E_protype == 0))
                {
                    switch (MyDevice.mSXT.E_outype)
                    {
                        case 0xE6:
                            MyDevice.mSXT.E_outype = (byte)OUT.UMASK;
                            MyDevice.mSXT.E_protype = (byte)TYPE.TD485;
                            break;
                        case 0xE7:
                            MyDevice.mSXT.E_outype = (byte)OUT.UMASK;
                            MyDevice.mSXT.E_protype = (byte)TYPE.TCAN;
                            break;
                        case 0xF6:
                            MyDevice.mSXT.E_outype = (byte)OUT.UMASK;
                            MyDevice.mSXT.E_protype = (byte)TYPE.iBus;
                            break;
                    }
                }
            }
            isEQ = true;
        }

        //串口读取SCT1
        private void Protocol_GetSCT1()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT2
        private void Protocol_GetSCT2()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT3
        private void Protocol_GetSCT3()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_vtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_atio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_btio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ctio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_dtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT4
        private void Protocol_GetSCT4()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero_4ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full_20ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero_N5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full_P5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_zero_N10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_full_P10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT5
        private void Protocol_GetSCT5()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_corr = MyDevice.myUIT.I;
            MyDevice.mSXT.E_mark = meRXD[rxRead++];
            MyDevice.mSXT.E_sign = meRXD[rxRead++];
            MyDevice.mSXT.E_addr = meRXD[rxRead++];
            MyDevice.mSXT.E_baud = meRXD[rxRead++];
            MyDevice.mSXT.E_stopbit = meRXD[rxRead++];
            MyDevice.mSXT.E_parity = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_full = MyDevice.myUIT.I;
            MyDevice.mSXT.E_wt_decimal = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_unit = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_ascii = meRXD[rxRead++];
            //iBus
            MyDevice.mSXT.E_wt_sptime = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_spfilt = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_division = meRXD[rxRead++];
            MyDevice.mSXT.E_wt_antivib = meRXD[rxRead++];
            //CANopen
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_heartBeat = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_typeTPDO0 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_evenTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_nodeID = meRXD[rxRead++];
            MyDevice.mSXT.E_nodeBaud = meRXD[rxRead++];
            //iBus
            MyDevice.mSXT.E_dynazero = meRXD[rxRead++];
            MyDevice.mSXT.E_cheatype = meRXD[rxRead++];
            MyDevice.mSXT.E_thmax = meRXD[rxRead++];
            MyDevice.mSXT.E_thmin = meRXD[rxRead++];
            MyDevice.mSXT.E_stablerange = meRXD[rxRead++];
            MyDevice.mSXT.E_stabletime = meRXD[rxRead++];
            MyDevice.mSXT.E_tkzerotime = meRXD[rxRead++];
            MyDevice.mSXT.E_tkdynatime = meRXD[rxRead++];
            isEQ = true;
        }

        //串口读取SCT6
        private void Protocol_GetSCT6()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT7
        private void Protocol_GetSCT7()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT8
        private void Protocol_GetSCT8()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ad_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_da_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_input11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_analog11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_etio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_ftio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_gtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_htio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_itio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_jtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT9
        private void Protocol_GetSCT9()
        {
            MyDevice.mSXT.E_enGFC = meRXD[rxRead++];
            MyDevice.mSXT.E_enSRDO = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_SCT_time = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_COB_ID1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_COB_ID2 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_enOL = meRXD[rxRead++];
            MyDevice.mSXT.E_overload = meRXD[rxRead++];
            MyDevice.mSXT.E_alarmMode = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wetTarget = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wetLow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_wetHigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_filter = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.mSXT.E_netServicePort = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_netServiceIP[0] = meRXD[rxRead++];
            MyDevice.mSXT.E_netServiceIP[1] = meRXD[rxRead++];
            MyDevice.mSXT.E_netServiceIP[2] = meRXD[rxRead++];
            MyDevice.mSXT.E_netServiceIP[3] = meRXD[rxRead++];
            MyDevice.mSXT.E_netClientIP[0] = meRXD[rxRead++];
            MyDevice.mSXT.E_netClientIP[1] = meRXD[rxRead++];
            MyDevice.mSXT.E_netClientIP[2] = meRXD[rxRead++];
            MyDevice.mSXT.E_netClientIP[3] = meRXD[rxRead++];
            MyDevice.mSXT.E_netGatIP[0] = meRXD[rxRead++];
            MyDevice.mSXT.E_netGatIP[1] = meRXD[rxRead++];
            MyDevice.mSXT.E_netGatIP[2] = meRXD[rxRead++];
            MyDevice.mSXT.E_netGatIP[3] = meRXD[rxRead++];
            MyDevice.mSXT.E_netMaskIP[0] = meRXD[rxRead++];
            MyDevice.mSXT.E_netMaskIP[1] = meRXD[rxRead++];
            MyDevice.mSXT.E_netMaskIP[2] = meRXD[rxRead++];
            MyDevice.mSXT.E_netMaskIP[3] = meRXD[rxRead++];
            MyDevice.mSXT.E_useDHCP = meRXD[rxRead++];
            MyDevice.mSXT.E_useScan = meRXD[rxRead++];
            MyDevice.mSXT.E_addrRF[0] = meRXD[rxRead++];
            MyDevice.mSXT.E_addrRF[1] = meRXD[rxRead++];
            MyDevice.mSXT.E_spedRF = meRXD[rxRead++];
            MyDevice.mSXT.E_chanRF = meRXD[rxRead++];
            MyDevice.mSXT.E_optionRF = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_lockTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_entrTPDO0 = meRXD[rxRead++];
            MyDevice.mSXT.E_typeTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_lockTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mSXT.E_entrTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mSXT.E_evenTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mSXT.E_scaling = MyDevice.myUIT.F;
            isEQ = true;
        }

        //串口写入后读出的校验SCT0
        private void Protocol_CheckSCT0()
        {
            isEQ = true;
            rxStr = "";
            if (MyDevice.mSXT.E_test != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_test"; } //1
            if (MyDevice.mSXT.E_outype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_outype"; } //2
            if (MyDevice.mSXT.E_curve != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_curve"; } //3
            if (MyDevice.mSXT.E_adspeed != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_adspeed"; } //4
            if (MyDevice.mSXT.E_autozero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_autozero"; } //5
            if (MyDevice.mSXT.E_trackzero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_trackzero"; } //6
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_checkhigh != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_checkhigh"; } //78910
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_checklow != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_checklow"; } //11~14
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_mfg_date != MyDevice.myUIT.UI) { isEQ = false; rxStr = "error E_mfg_date"; } //15~18
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_mfg_srno != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_mfg_srno"; } //19~22
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mSXT.E_tmp_min != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_min"; } //23~26
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mSXT.E_tmp_max != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_max"; } //27~30
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mSXT.E_tmp_cal != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_cal"; } //31~34
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_bohrcode != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_bohrcode"; } //35~38
            if (MyDevice.mSXT.E_enspan != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enspan"; } //39
            if (MyDevice.mSXT.E_protype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_protype"; } //40
        }

        //串口写入后读出的校验SCT1
        private void Protocol_CheckSCT1()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point5"; }
        }

        //串口写入后读出的校验SCT2
        private void Protocol_CheckSCT2()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog5"; }
        }

        //串口写入后读出的校验SCT3
        private void Protocol_CheckSCT3()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_full"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_vtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_vtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_atio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_atio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_btio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_btio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ctio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ctio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_dtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_dtio"; }
        }

        //串口写入后读出的校验SCT4
        private void Protocol_CheckSCT4()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero_4ma != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_4ma"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full_20ma != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_20ma"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero_05V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_05V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full_05V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_05V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero_10V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_10V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full_10V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_10V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero_N5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_N5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full_P5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_P5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_zero_N10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_N10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_full_P10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_P10"; }
        }

        //串口写入后读出的校验SCT5
        private void Protocol_CheckSCT5()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_corr != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_corr"; }
            if (MyDevice.mSXT.E_mark != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_mark"; }
            if (MyDevice.mSXT.E_sign != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_sign"; }
            if (MyDevice.mSXT.E_addr != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addr"; }
            if (MyDevice.mSXT.E_baud != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_baud"; }
            if (MyDevice.mSXT.E_stopbit != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stopbit"; }
            if (MyDevice.mSXT.E_parity != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_parity"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wt_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wt_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wt_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wt_full"; }
            if (MyDevice.mSXT.E_wt_decimal != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_decimal"; }
            if (MyDevice.mSXT.E_wt_unit != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_unit"; }
            if (MyDevice.mSXT.E_wt_ascii != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_ascii"; }
            //iBus
            if (MyDevice.mSXT.E_wt_sptime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_sptime"; }
            if (MyDevice.mSXT.E_wt_spfilt != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_spfilt"; }
            if (MyDevice.mSXT.E_wt_division != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_division"; }
            if (MyDevice.mSXT.E_wt_antivib != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_antivib"; }
            //CANopen
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_heartBeat != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_heartBeat"; }
            if (MyDevice.mSXT.E_typeTPDO0 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_typeTPDO0"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_evenTPDO0 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_evenTPDO0"; }
            if (MyDevice.mSXT.E_nodeID != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_nodeID"; }
            if (MyDevice.mSXT.E_nodeBaud != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_nodeBaud"; }
            //iBus
            if (MyDevice.mSXT.E_dynazero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_dynazero"; }
            if (MyDevice.mSXT.E_cheatype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_cheatype"; }
            if (MyDevice.mSXT.E_thmax != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_thmax"; }
            if (MyDevice.mSXT.E_thmin != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_thmin"; }
            if (MyDevice.mSXT.E_stablerange != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stablerange"; }
            if (MyDevice.mSXT.E_stabletime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stabletime"; }
            if (MyDevice.mSXT.E_tkzerotime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_tkzerotime"; }
            if (MyDevice.mSXT.E_tkdynatime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_tkdynatime"; }
        }

        //串口写入后读出的校验SCT6
        private void Protocol_CheckSCT6()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point10"; }
        }

        //串口写入后读出的校验SCT7
        private void Protocol_CheckSCT7()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog10"; }
        }

        //串口写入后读出的校验SCT8
        private void Protocol_CheckSCT8()
        {
            isEQ = true;
            rxStr = "";
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ad_point11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_da_point11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_input11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_analog11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_etio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_etio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_ftio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ftio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_gtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_gtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_htio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_htio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_itio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_itio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_jtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_jtio," + MyDevice.mSXT.E_jtio.ToString("X2") + " ? " + MyDevice.myUIT.I.ToString("X2"); }
        }

        //串口写入后读出的校验SCT9
        private void Protocol_CheckSCT9()
        {
            isEQ = true;
            rxStr = "";
            if (MyDevice.mSXT.E_enGFC != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enGFC"; }
            if (MyDevice.mSXT.E_enSRDO != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enSRDO"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_SCT_time != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_SCT_time"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_COB_ID1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_COB_ID1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_COB_ID2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_COB_ID2"; }
            if (MyDevice.mSXT.E_enOL != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enOL"; }
            if (MyDevice.mSXT.E_overload != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_overload"; }
            if (MyDevice.mSXT.E_alarmMode != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_alarmMode"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wetTarget != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetTarget"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wetLow != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetLow"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_wetHigh != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetHigh"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_filter != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_filtRange"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_netServicePort != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_netServicePort"; }
            if (MyDevice.mSXT.E_netServiceIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[0]"; }
            if (MyDevice.mSXT.E_netServiceIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[1]"; }
            if (MyDevice.mSXT.E_netServiceIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[2]"; }
            if (MyDevice.mSXT.E_netServiceIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[3]"; }
            if (MyDevice.mSXT.E_netClientIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[0]"; }
            if (MyDevice.mSXT.E_netClientIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[1]"; }
            if (MyDevice.mSXT.E_netClientIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[2]"; }
            if (MyDevice.mSXT.E_netClientIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[3]"; }
            if (MyDevice.mSXT.E_netGatIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[0]"; }
            if (MyDevice.mSXT.E_netGatIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[1]"; }
            if (MyDevice.mSXT.E_netGatIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[2]"; }
            if (MyDevice.mSXT.E_netGatIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[3]"; }
            if (MyDevice.mSXT.E_netMaskIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[0]"; }
            if (MyDevice.mSXT.E_netMaskIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[1]"; }
            if (MyDevice.mSXT.E_netMaskIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[2]"; }
            if (MyDevice.mSXT.E_netMaskIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[3]"; }
            if (MyDevice.mSXT.E_useDHCP != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_useDHCP"; }
            if (MyDevice.mSXT.E_useScan != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_useScan"; }
            if (MyDevice.mSXT.E_addrRF[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addrRF[0]"; }
            if (MyDevice.mSXT.E_addrRF[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addrRF[1]"; }
            if (MyDevice.mSXT.E_spedRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_spedRF"; }
            if (MyDevice.mSXT.E_chanRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_chanRF"; }
            if (MyDevice.mSXT.E_optionRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_optionRF"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_lockTPDO0 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_lockTPDO0"; }
            if (MyDevice.mSXT.E_entrTPDO0 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_entrTPDO0 "; }
            if (MyDevice.mSXT.E_typeTPDO1 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_typeTPDO1 "; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_lockTPDO1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_lockTPDO1"; }
            if (MyDevice.mSXT.E_entrTPDO1 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_entrTPDO1 "; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mSXT.E_evenTPDO1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_evenTPDO1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mSXT.E_scaling != MyDevice.myUIT.F) { isEQ = false; rxStr = "error E_scaling"; }
        }

        //串口写入Eeprom参数
        public void Protocol_SendEeprom()
        {

        }

        //网络管理
        public void Protocol_SendNMT(Byte NMT_CS)
        {

        }

        //获取心跳的COB-ID
        public uint Protocol_GetHeartBeatID()
        {
            return 0;
        }

        //设置心跳时间间隔
        public void Protocol_SendHeartBeat(UInt16 period)
        {

        }

        //串口接收BohrCode
        private void Protocol_mePort_ReceiveBohrCode()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (Byte)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL://0x02
                        //检查是否有外部EEPROM
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.TDES) || (MyDevice.mSXT.S_DeviceType == TYPE.TDSS))
                        {
                            switch (meChr)
                            {
                                case 0x02:
                                    MyDevice.mSXT.R_eeplink = false;
                                    break;
                                case 0x42:
                                    meChr &= 0x0F;
                                    MyDevice.mSXT.R_eeplink = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            MyDevice.mSXT.R_eeplink = false;
                        }
                        //
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        break;
                    case RXSTP.STX://0x08
                        if ((meChr == 0x08) && (rxWrite == 0x08))
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else if ((meChr == 0x09) && (rxWrite == 0x08))
                        {
                            //iBus不支持老版本软件（V10.6.7及之前)
                            //新版02 BB 0C 7C C4 2C 23 09 18 09 03 
                            rxSTP = RXSTP.ACK;
                        }
                        else if ((meChr == 0x0A) && (rxWrite == 0x08))
                        {
                            //iBus不支持老版本软件（V10.6.7及之前)
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            //接收数据
                            meRXD[rxWrite] = meChr;
                            //写指针记录接收的长度
                            if ((++rxWrite) >= meRXD.Length)
                            {
                                rxWrite = 0;
                            }
                        }
                        break;
                    case RXSTP.ACK://0x03
                        if (meChr == Constants.STOP)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX://取数据
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //解码
                if (meRXD[0] == Constants.CODE)
                {
                    //颠倒
                    MyDevice.myUIT.B0 = meRXD[7];
                    MyDevice.myUIT.B1 = meRXD[6];
                    MyDevice.myUIT.B2 = meRXD[5];
                    MyDevice.myUIT.B3 = meRXD[4];
                    MyDevice.mSXT.E_bohrcode = MyDevice.myUIT.I;
                    //
                    MyDevice.mSXT.R_bohrcode_long = meRXD[7] +
                                            ((Int64)meRXD[6] << 8) +
                                            ((Int64)meRXD[5] << 16) +
                                            ((Int64)meRXD[4] << 24) +
                                            ((Int64)meRXD[3] << 32) +
                                            ((Int64)meRXD[2] << 40) +
                                            ((Int64)meRXD[1] << 48);

                    //
                    MyDevice.mSXT.sTATE = STATE.CONNECTED;

                    //
                    isEQ = true;

                    //委托
                    MyDevice.callSecondDelegate();
                }

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口校验BohrCode
        private void Protocol_mePort_ReceiveBohrCodeCheck()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (Byte)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL://0x02
                        //检查是否有外部EEPROM
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.TDES) || (MyDevice.mSXT.S_DeviceType == TYPE.TDSS))
                        {
                            switch (meChr)
                            {
                                case 0x02:
                                    MyDevice.mSXT.R_eeplink = false;
                                    break;
                                case 0x42:
                                    meChr &= 0x0F;
                                    MyDevice.mSXT.R_eeplink = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            MyDevice.mSXT.R_eeplink = false;
                        }
                        //
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        break;
                    case RXSTP.STX://0x08
                        if ((meChr == 0x08) && (rxWrite == 0x08))
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else if ((meChr == 0x09) && (rxWrite == 0x08))
                        {
                            //iBus不支持老版本软件（V10.6.7及之前)
                            //新版02 BB 0C 7C C4 2C 23 09 18 09 03 
                            rxSTP = RXSTP.ACK;
                        }
                        else if ((meChr == 0x0A) && (rxWrite == 0x08))
                        {
                            //iBus不支持老版本软件（V10.6.7及之前)
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            //接收数据
                            meRXD[rxWrite] = meChr;
                            //写指针记录接收的长度
                            if ((++rxWrite) >= meRXD.Length)
                            {
                                rxWrite = 0;
                            }
                        }
                        break;
                    case RXSTP.ACK://0x03
                        if (meChr == Constants.STOP)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX://取数据
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //解码
                if (meRXD[0] == Constants.CODE)
                {
                    //颠倒
                    MyDevice.myUIT.B0 = meRXD[7];
                    MyDevice.myUIT.B1 = meRXD[6];
                    MyDevice.myUIT.B2 = meRXD[5];
                    MyDevice.myUIT.B3 = meRXD[4];

                    //非模拟量设备第一次更新Bohrcode
                    if ((MyDevice.mSXT.E_bohrcode == -1) && (MyDevice.mSXT.S_OutType == OUT.UMASK))
                    {
                        MyDevice.mSXT.E_bohrcode = MyDevice.myUIT.I;
                    }

                    //校验E_bohrcode
                    if (MyDevice.mSXT.E_bohrcode == MyDevice.myUIT.I)
                    {
                        isEQ = true;
                        rxStr = "";
                    }
                    else
                    {
                        isEQ = true;//不再设置BCC校验失败,用户可以继续使用产品,但是要提示返厂模拟量校准
                        rxStr = "error E_bohrcode," + MyDevice.mSXT.E_bohrcode.ToString("X2") + " ? " + MyDevice.myUIT.I.ToString("X2");
                    }

                    //委托
                    MyDevice.callSecondDelegate();
                }

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收读取Sector
        //SCT0-SCT8
        private void Protocol_mePort_ReceiveSector()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (Byte)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL:
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        break;
                    case RXSTP.STX:
                        if ((meChr == Constants.tLen) && (rxWrite == Constants.tLen))
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            //接收数据
                            meRXD[rxWrite] = meChr;
                            if ((++rxWrite) >= meRXD.Length)
                            {
                                rxWrite = 0;
                            }
                        }
                        break;
                    case RXSTP.ACK:
                        if (meChr == Constants.STOP)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX:
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //
                rxRead = 1;

                //解码
                switch (trTSK)
                {
                    //读出数据覆盖当前值
                    case TASKS.RDX0:
                    case TASKS.RDX1:
                    case TASKS.RDX2:
                    case TASKS.RDX3:
                    case TASKS.RDX4:
                    case TASKS.RDX5:
                    case TASKS.RDX6:
                    case TASKS.RDX7:
                    case TASKS.RDX8:
                    case TASKS.RDX9:
                        //RS485高位在前低位在后
                        //SelfUART低位在前高位在后
                        switch (meRXD[0])
                        {
                            case Constants.WRX0: Protocol_GetSCT0(); break; //RDX0
                            case Constants.WRX1: Protocol_GetSCT1(); break; //RDX1
                            case Constants.WRX2: Protocol_GetSCT2(); break; //RDX2
                            case Constants.WRX3: Protocol_GetSCT3(); break; //RDX3
                            case Constants.WRX4: Protocol_GetSCT4(); break; //RDX4
                            case Constants.WRX5: Protocol_GetSCT5(); break; //RDX5
                            case Constants.WRX6: Protocol_GetSCT6(); break; //RDX6
                            case Constants.WRX7: Protocol_GetSCT7(); break; //RDX7
                            case Constants.WRX8: Protocol_GetSCT8(); break; //RDX8
                            case Constants.WRX9: Protocol_GetSCT9(); break; //RDX9
                            default: return;
                        }
                        break;

                    //写入后读出校验
                    case TASKS.WRX0:
                    case TASKS.WRX1:
                    case TASKS.WRX2:
                    case TASKS.WRX3:
                    case TASKS.WRX4:
                    case TASKS.WRX5:
                    case TASKS.WRX6:
                    case TASKS.WRX7:
                    case TASKS.WRX8:
                    case TASKS.WRX9:
                        //RS485高位在前低位在后
                        //SelfUART低位在前高位在后
                        switch (meRXD[0])
                        {
                            case Constants.WRX0: Protocol_CheckSCT0(); break;
                            case Constants.WRX1: Protocol_CheckSCT1(); break;
                            case Constants.WRX2: Protocol_CheckSCT2(); break;
                            case Constants.WRX3: Protocol_CheckSCT3(); break;
                            case Constants.WRX4: Protocol_CheckSCT4(); break;
                            case Constants.WRX5: Protocol_CheckSCT5(); break;
                            case Constants.WRX6: Protocol_CheckSCT6(); break;
                            case Constants.WRX7: Protocol_CheckSCT7(); break;
                            case Constants.WRX8: Protocol_CheckSCT8(); break;
                            case Constants.WRX9: Protocol_CheckSCT9(); break;
                            default: return;
                        }
                        break;

                    default:
                        return;
                }

                //委托
                MyDevice.callSecondDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收采样的滤波内码
        //=±123\r\n
        private void Protocol_mePort_ReceiveAdpoint()
        {
            //
            char meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (char)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (meChr)
                {
                    case '=':
                        rxSTP = RXSTP.STX;
                        rxStr = null;//default
                        break;

                    case ' ':
                        break;

                    case '+':
                        rxStr += meChr.ToString();
                        break;

                    case '-':
                        rxStr += meChr.ToString();
                        break;

                    case (char)0x0D:
                        if (rxSTP == RXSTP.STX)
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;

                    case (char)0x0A:
                        if (rxSTP == RXSTP.ACK)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;

                    default:
                        if ((meChr >= '0') && (meChr <= '9'))
                        {
                            if (rxSTP == RXSTP.STX)
                            {
                                //接收数据
                                rxStr += meChr.ToString();
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                        }
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //取值
                rxDat = Convert.ToInt32(rxStr);

                //mV/V
                double mvdv = (double)rxDat / MyDevice.mSXT.S_MVDV;

                //
                switch (trTSK)
                {
                    case TASKS.ADCP1:
                        MyDevice.mSXT.E_input1 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point1 = rxDat;
                        MyDevice.mSXT.E_ad_zero = rxDat;
                        break;
                    case TASKS.ADCP2:
                        MyDevice.mSXT.E_input2 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point2 = rxDat;
                        break;
                    case TASKS.ADCP3:
                        MyDevice.mSXT.E_input3 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point3 = rxDat;
                        break;
                    case TASKS.ADCP4:
                        MyDevice.mSXT.E_input4 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point4 = rxDat;
                        break;
                    case TASKS.ADCP5:
                        MyDevice.mSXT.E_input5 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point5 = rxDat;
                        if (!MyDevice.mSXT.S_ElevenType) MyDevice.mSXT.E_ad_full = rxDat;
                        break;
                    case TASKS.ADCP6:
                        MyDevice.mSXT.E_input6 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point6 = rxDat;
                        break;
                    case TASKS.ADCP7:
                        MyDevice.mSXT.E_input7 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point7 = rxDat;
                        break;
                    case TASKS.ADCP8:
                        MyDevice.mSXT.E_input8 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point8 = rxDat;
                        break;
                    case TASKS.ADCP9:
                        MyDevice.mSXT.E_input9 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point9 = rxDat;
                        break;
                    case TASKS.ADCP10:
                        MyDevice.mSXT.E_input10 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point10 = rxDat;
                        break;
                    case TASKS.ADCP11:
                        MyDevice.mSXT.E_input11 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mSXT.E_ad_point11 = rxDat;
                        if (MyDevice.mSXT.S_ElevenType) MyDevice.mSXT.E_ad_full = rxDat;
                        break;
                }

                //委托
                MyDevice.callSecondDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收读取Long
        //=±123\r\n
        //=F123\r\n
        private void Protocol_mePort_ReceiveLong()
        {
            //
            char meChr;
            bool isFLT = false;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (char)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (meChr)
                {
                    case '=':
                        rxSTP = RXSTP.STX;
                        isFLT = false;
                        rxStr = null;//default
                        break;

                    case ' ':
                        break;

                    case '+':
                        isFLT = false;
                        rxStr += meChr.ToString();
                        break;

                    case '-':
                        isFLT = false;
                        rxStr += meChr.ToString();
                        break;

                    case 'F':
                        isFLT = true;
                        break;

                    case (char)0x0D:
                        if (rxSTP == RXSTP.STX)
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;

                    case (char)0x0A:
                        if (rxSTP == RXSTP.ACK)
                        {
                            rxSTP = RXSTP.ETX;

                            //设备连续发送数据的,应该在while里面解码结尾就触发委托
                            switch (trTSK)
                            {
                                case TASKS.TARE:
                                case TASKS.ZERO:
                                case TASKS.SPAN:
                                case TASKS.ADC:
                                case TASKS.SFLT:
                                case TASKS.RFLT:
                                    if (isFLT)
                                    {
                                        rxDat = (int)TASKS.SFLT;
                                        MyDevice.mSXT.E_filter = Convert.ToInt32(rxStr);
                                    }
                                    else
                                    {
                                        rxDat = 0;
                                        MyDevice.mSXT.R_adcout = Convert.ToInt32(rxStr);
                                    }
                                    break;

                                case TASKS.RDFT:
                                    if (isFLT)
                                    {
                                        rxDat = (int)TASKS.SFLT;
                                        MyDevice.mSXT.E_filter = Convert.ToInt32(rxStr);
                                        isEQ = true;//iBUS读SCT1-8后读MEM.filt_range 采样滤波范围
                                    }
                                    else
                                    {
                                        rxDat = 0;
                                        MyDevice.mSXT.R_adcout = Convert.ToInt32(rxStr);
                                    }
                                    break;

                                case TASKS.GODMZ:
                                case TASKS.GOUPZ:
                                case TASKS.GODMM:
                                case TASKS.GOUPM:
                                case TASKS.GODMF:
                                case TASKS.GOUPF:
                                    //直接使用rxStr
                                    if (isFLT)
                                    {
                                        rxStr = "";
                                    }
                                    break;
                            }

                            //委托
                            MyDevice.callSecondDelegate();

                            //协议
                            rxSTP = RXSTP.NUL;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;

                    default:
                        if ((meChr >= '0') && (meChr <= '9'))
                        {
                            if (rxSTP == RXSTP.STX)
                            {
                                //接收数据
                                rxStr += meChr.ToString();
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                        }
                        break;
                }
            }
        }

        //串口接收读取dacout
        //02 80 80 80
        //02 80 80 80 80
        private void Protocol_mePort_ReceiveDacout()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (Byte)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //检查是否有外部EEPROM
                if ((MyDevice.mSXT.S_DeviceType == TYPE.TDES) || (MyDevice.mSXT.S_DeviceType == TYPE.TDSS))
                {
                    switch (meChr)
                    {
                        case 0x02:
                        case 0x03:
                            MyDevice.mSXT.R_eeplink = false;
                            break;

                        case 0x42:
                        case 0x43:
                            meChr &= 0x0F;
                            MyDevice.mSXT.R_eeplink = true;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    MyDevice.mSXT.R_eeplink = false;
                }

                //帧解析
                switch (meChr)
                {
                    //正数
                    case Constants.STAR:
                        //
                        switch (rxWrite)
                        {
                            //02 80 80 80 02
                            case 3:
                                if ((meRXD[0] >= 0x80) && (meRXD[1] >= 0x80) && (meRXD[2] >= 0x80))
                                {
                                    MyDevice.myUIT.B0 = meRXD[2];  //低位
                                    MyDevice.myUIT.B1 = meRXD[1];  //中位
                                    MyDevice.myUIT.B2 = meRXD[0];  //高位
                                    MyDevice.myUIT.B3 = 0;

                                    //
                                    if ((meRXD[0] & 0x10) == 0)
                                    {
                                        MyDevice.myUIT.B0 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x20) == 0)
                                    {
                                        MyDevice.myUIT.B1 &= (byte)0x7F;
                                    }

                                    //
                                    MyDevice.myUIT.B2 &= (byte)0x0F;

                                    //
                                    rxDat = MyDevice.myUIT.I;

                                    //
                                    MyDevice.mSXT.RefreshDacout(rxDat);

                                    //委托
                                    MyDevice.callSecondDelegate();
                                }
                                break;

                            //02 80 80 80 80 02
                            case 4:
                                if ((meRXD[0] >= 0x80) && (meRXD[1] >= 0x80) && (meRXD[2] >= 0x80) && (meRXD[3] >= 0x80))
                                {
                                    MyDevice.myUIT.B0 = meRXD[3];  //低位
                                    MyDevice.myUIT.B1 = meRXD[2];  //中位
                                    MyDevice.myUIT.B2 = meRXD[1];  //中位
                                    MyDevice.myUIT.B3 = meRXD[0];  //高位

                                    //
                                    if ((meRXD[0] & 0x10) == 0)
                                    {
                                        MyDevice.myUIT.B0 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x20) == 0)
                                    {
                                        MyDevice.myUIT.B1 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x40) == 0)
                                    {
                                        MyDevice.myUIT.B2 &= (byte)0x7F;
                                    }

                                    //
                                    MyDevice.myUIT.B3 &= (byte)0x0F;

                                    //
                                    rxDat = MyDevice.myUIT.I;

                                    //
                                    MyDevice.mSXT.RefreshDacout(rxDat);

                                    //委托
                                    MyDevice.callSecondDelegate();
                                }
                                break;
                        }
                        //
                        meRXD[0] = 0;
                        meRXD[1] = 0;
                        meRXD[2] = 0;
                        meRXD[3] = 0;
                        rxWrite = 0;
                        break;

                    //负数
                    case Constants.STOP:
                        //
                        switch (rxWrite)
                        {
                            //03 80 80 80 03
                            case 3:
                                if ((meRXD[0] >= 0x80) && (meRXD[1] >= 0x80) && (meRXD[2] >= 0x80))
                                {
                                    MyDevice.myUIT.B0 = meRXD[2];  //低位
                                    MyDevice.myUIT.B1 = meRXD[1];  //中位
                                    MyDevice.myUIT.B2 = meRXD[0];  //高位
                                    MyDevice.myUIT.B3 = 0;

                                    //
                                    if ((meRXD[0] & 0x10) == 0)
                                    {
                                        MyDevice.myUIT.B0 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x20) == 0)
                                    {
                                        MyDevice.myUIT.B1 &= (byte)0x7F;
                                    }

                                    //
                                    MyDevice.myUIT.B2 &= (byte)0x0F;

                                    //
                                    rxDat = -MyDevice.myUIT.I;

                                    //
                                    MyDevice.mSXT.RefreshDacout(rxDat);

                                    //委托
                                    MyDevice.callSecondDelegate();
                                }
                                break;

                            //03 80 80 80 80 03
                            case 4:
                                if ((meRXD[0] >= 0x80) && (meRXD[1] >= 0x80) && (meRXD[2] >= 0x80) && (meRXD[3] >= 0x80))
                                {
                                    MyDevice.myUIT.B0 = meRXD[3];  //低位
                                    MyDevice.myUIT.B1 = meRXD[2];  //中位
                                    MyDevice.myUIT.B2 = meRXD[1];  //中位
                                    MyDevice.myUIT.B3 = meRXD[0];  //高位

                                    //
                                    if ((meRXD[0] & 0x10) == 0)
                                    {
                                        MyDevice.myUIT.B0 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x20) == 0)
                                    {
                                        MyDevice.myUIT.B1 &= (byte)0x7F;
                                    }

                                    //
                                    if ((meRXD[0] & 0x40) == 0)
                                    {
                                        MyDevice.myUIT.B2 &= (byte)0x7F;
                                    }

                                    //
                                    MyDevice.myUIT.B3 &= (byte)0x0F;

                                    //
                                    rxDat = -MyDevice.myUIT.I;

                                    //
                                    MyDevice.mSXT.RefreshDacout(rxDat);

                                    //委托
                                    MyDevice.callSecondDelegate();
                                }
                                break;
                        }
                        //
                        meRXD[0] = 0;
                        meRXD[1] = 0;
                        meRXD[2] = 0;
                        meRXD[3] = 0;
                        rxWrite = 0;
                        break;

                    default:
                        meRXD[rxWrite++] = meChr;
                        if (rxWrite >= 200) rxWrite = 0;
                        break;
                }
            }
        }

        //不是RS485
        private void Protocol_mePort_ReceiveAscii()
        {
            mePort.DiscardInBuffer();
        }

        //串口接收Reset
        private void Protocol_mePort_ReceiveReset()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.BytesToRead > 0)
            {
                //取字节
                meChr = (Byte)mePort.ReadByte();

                //字节统计
                rxCnt++;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL://0x02
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        break;
                    case RXSTP.STX://0x01
                        if ((meChr == 0x01) && (rxWrite == 0x01))
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            //接收数据
                            meRXD[rxWrite] = meChr;
                            if ((++rxWrite) >= meRXD.Length)
                            {
                                rxWrite = 0;
                            }
                        }
                        break;
                    case RXSTP.ACK://0x03
                        if (meChr == Constants.STOP)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX://取数据
                        if (meChr == Constants.STAR)
                        {
                            rxSTP = RXSTP.STX;
                            //default
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //新版本设备解码重启指令
                //老版本设备重启后上电归零时间长
                //老版本的自定义协议不回复重启指令
                //老版本的RS485协议回复0x00重启指令
                if (meRXD[0] == Constants.REST)
                {
                    isEQ = true;
                }
                else
                {
                    isEQ = false;
                }

                //委托
                MyDevice.callSecondDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //不是RS485
        private void Protocol_mePort_ReceiveRS485Scan()
        {
            mePort.DiscardInBuffer();
        }

        //不是RS485
        private void Protocol_mePort_ReceiveRS485String()
        {
            mePort.DiscardInBuffer();
        }

        //接收触发函数,实际会由串口线程创建
        private void mePort_DataReceived()
        {
            while (true)
            {
                if (is_serial_closing)
                {
                    is_serial_listening = false;//准备关闭串口时，reset串口侦听标记
                    return;
                }
                try
                {
                    //串口有数据时，接受数据并处理
                    if (mePort.BytesToRead > 0)
                    {
                        is_serial_listening = true;

                        //接收处理
                        switch (trTSK)
                        {
                            case TASKS.BOR:
                                Protocol_mePort_ReceiveBohrCode();
                                break;

                            case TASKS.BCC:
                                Protocol_mePort_ReceiveBohrCodeCheck();
                                break;

                            case TASKS.RDX0:
                            case TASKS.RDX1:
                            case TASKS.RDX2:
                            case TASKS.RDX3:
                            case TASKS.RDX4:
                            case TASKS.RDX5:
                            case TASKS.RDX6:
                            case TASKS.RDX7:
                            case TASKS.RDX8:
                            case TASKS.WRX0:
                            case TASKS.WRX1:
                            case TASKS.WRX2:
                            case TASKS.WRX3:
                            case TASKS.WRX4:
                            case TASKS.WRX5:
                            case TASKS.WRX6:
                            case TASKS.WRX7:
                            case TASKS.WRX8:
                                Protocol_mePort_ReceiveSector();
                                break;

                            case TASKS.ADCP1:
                            case TASKS.ADCP2:
                            case TASKS.ADCP3:
                            case TASKS.ADCP4:
                            case TASKS.ADCP5:
                            case TASKS.ADCP6:
                            case TASKS.ADCP7:
                            case TASKS.ADCP8:
                            case TASKS.ADCP9:
                            case TASKS.ADCP10:
                            case TASKS.ADCP11:
                                Protocol_mePort_ReceiveAdpoint();
                                break;

                            case TASKS.GODMZ:
                            case TASKS.GOUPZ:
                            case TASKS.GODMM:
                            case TASKS.GOUPM:
                            case TASKS.GODMF:
                            case TASKS.GOUPF:
                            case TASKS.SFLT:
                            case TASKS.RFLT:
                            case TASKS.RDFT:
                            case TASKS.TARE:
                            case TASKS.ZERO:
                            case TASKS.SPAN:
                            case TASKS.ADC:
                                Protocol_mePort_ReceiveLong();
                                break;

                            case TASKS.DAC:
                                Protocol_mePort_ReceiveDacout();
                                break;

                            case TASKS.QNET:
                            case TASKS.QGROSS:
                                Protocol_mePort_ReceiveAscii();
                                break;

                            case TASKS.REST:
                                Protocol_mePort_ReceiveReset();
                                break;

                            case TASKS.SCAN:
                                Protocol_mePort_ReceiveRS485Scan();
                                break;

                            case TASKS.RSBUF:
                                Protocol_mePort_ReceiveRS485String();
                                break;

                            default:
                                mePort.DiscardInBuffer();
                                break;
                        }
                    }
                }
                finally
                {
                    is_serial_listening = false;//串口调用完毕后，reset串口侦听标记
                }
            }
        }

        //串口读取任务状态机 BOR -> RDX0 -> RDX1 -> RDX2 -> RDX3 -> RDX4 -> RDX5 -> RDX6 -> RDX7 -> RDX8 -> RDX9
        public void Protocol_mePort_ReadTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧发送
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.DONE);//115200下的指令无法识别,用于连接时要切换19200后喂一个清除错误缓存
                    Thread.Sleep(10);
                    Protocol_SendCOM(TASKS.DONE);//Transducer\Firmware\BD161015_V21只有DONE指令才能停止连续发送dacout
                    Thread.Sleep(10);
                    Protocol_SendCOM(TASKS.BOR);//开始先读Bohrcode
                    break;

                case TASKS.BOR:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX0);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.DONE);
                        Thread.Sleep(10);
                        Protocol_SendCOM(TASKS.BOR);
                    }
                    break;

                case TASKS.RDX0:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX1);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX0);
                    }
                    break;

                case TASKS.RDX1:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX2);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX1);
                    }
                    break;

                case TASKS.RDX2:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX3);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX2);
                    }
                    break;

                case TASKS.RDX3:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX4);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX3);
                    }
                    break;

                case TASKS.RDX4:
                    if (isEQ)
                    {
                        switch (MyDevice.mSXT.S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                MyDevice.mSXT.sTATE = STATE.WORKING;
                                MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                                trTSK = TASKS.NULL;
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mSXT.E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendCOM(TASKS.RDX5);
                                }
                                else
                                {
                                    MyDevice.mSXT.sTATE = STATE.WORKING;
                                    MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                                    trTSK = TASKS.NULL;
                                }
                                break;

                            case TYPE.TD485:
                            case TYPE.TCAN:
                            case TYPE.iBus:
                            case TYPE.iNet:
                            case TYPE.iStar:
                                Protocol_SendCOM(TASKS.RDX5);
                                break;
                        }
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX4);
                    }
                    break;

                case TASKS.RDX5:
                    if (isEQ)
                    {
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.iBus) || (MyDevice.mSXT.S_DeviceType == TYPE.TD485) || (MyDevice.mSXT.S_DeviceType == TYPE.TP10) || (MyDevice.mSXT.S_DeviceType == TYPE.iNet) || (MyDevice.mSXT.S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendCOM(TASKS.RDX6);
                        }
                        else if (MyDevice.mSXT.S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mSXT.E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendCOM(TASKS.RDX6);
                            }
                            else
                            {
                                MyDevice.mSXT.sTATE = STATE.WORKING;
                                MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mSXT);
                            }
                        }
                        else
                        {
                            MyDevice.mSXT.sTATE = STATE.WORKING;
                            MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                            trTSK = TASKS.NULL;
                        }
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX5);
                    }
                    break;

                case TASKS.RDX6:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX7);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX6);
                    }
                    break;

                case TASKS.RDX7:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX8);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX7);
                    }
                    break;

                case TASKS.RDX8:
                    if (isEQ)
                    {
                        if (MyDevice.mSXT.S_DeviceType == TYPE.TCAN || MyDevice.mSXT.S_DeviceType == TYPE.iNet || MyDevice.mSXT.S_DeviceType == TYPE.iStar)//有sct9
                        {
                            Protocol_SendCOM(TASKS.RDX9);
                        }
                        else if (MyDevice.mSXT.S_DeviceType == TYPE.iBus)//iBUS读SCT1-8后读MEM.filt_range 采样滤波范围
                        {
                            Protocol_SendCOM(TASKS.RDFT);
                        }
                        else
                        {
                            MyDevice.mSXT.sTATE = STATE.WORKING;
                            MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                            trTSK = TASKS.NULL;
                        }
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX8);
                    }
                    break;

                case TASKS.RDX9:
                    if (isEQ)
                    {
                        MyDevice.mSXT.sTATE = STATE.WORKING;
                        MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX9);
                    }
                    break;

                case TASKS.RDFT:
                    if (isEQ)
                    {
                        MyDevice.mSXT.sTATE = STATE.WORKING;
                        MyDevice.mSXT.R_checklink = MyDevice.mSXT.R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDFT);
                    }
                    break;
            }
        }

        //串口写入任务状态机 WRX0 -> RST
        //工厂校准用
        public void Protocol_mePort_WriteTypTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendSCT0();
                    break;

                case TASKS.WRX0:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.REST);
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mSXT);
                        }
                    }
                    else
                    {
                        Protocol_SendSCT0();
                    }
                    break;

                case TASKS.REST:
                    if (isEQ)
                    {
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX5 -> (WRX9)-> (rst)
        //参数AI跟踪器修改用
        public void Protocol_mePort_WriteParTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BCC);//开始先校验Bohrcode
                    break;

                case TASKS.BCC:
                    if (isEQ)
                    {
                        Protocol_SendSCT5();
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.BCC);
                    }
                    break;

                case TASKS.WRX5:
                    if (isEQ)
                    {
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.TCAN && MyDevice.mSXT.E_test > 0x58) || (MyDevice.mSXT.S_DeviceType == TYPE.iNet))  //新版本TCAN才有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mSXT);
                        }
                    }
                    else
                    {
                        Protocol_SendSCT5();
                    }
                    break;

                case TASKS.WRX9:
                    if (isEQ)
                    {
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendSCT9();
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX5 -> RST
        //RS485界面串口参数修改用
        public void Protocol_mePort_WriteBusTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BCC);//开始先校验Bohrcode
                    break;

                case TASKS.BCC:
                    if (isEQ)
                    {
                        Protocol_SendSCT5();
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.BCC);
                    }
                    break;

                case TASKS.WRX5:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.REST);
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mSXT);
                        }
                    }
                    else
                    {
                        Protocol_SendSCT5();
                    }
                    break;

                case TASKS.REST:
                    if (isEQ)
                    {
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst)
        //工厂校准用
        public void Protocol_mePort_WriteFacTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BCC);//开始先校验Bohrcode
                    break;

                case TASKS.BCC:
                    if (isEQ)
                    {
                        Protocol_SendSCT1();
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.BCC);
                    }
                    break;

                case TASKS.WRX1:
                    if (isEQ)
                    {
                        Protocol_SendSCT2();
                    }
                    else
                    {
                        Protocol_SendSCT1();
                    }
                    break;

                case TASKS.WRX2:
                    if (isEQ)
                    {
                        Protocol_SendSCT3();
                    }
                    else
                    {
                        Protocol_SendSCT2();
                    }
                    break;

                case TASKS.WRX3:
                    if (isEQ)
                    {
                        Protocol_SendSCT4();
                    }
                    else
                    {
                        Protocol_SendSCT3();
                    }
                    break;

                case TASKS.WRX4:
                    if (isEQ)
                    {
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendSCT4();
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX6 -> WRX7 -> WRX8 -> (rst)
        //标定传感器和修正传感器用
        public void Protocol_mePort_WriteCalTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BCC);//开始先校验Bohrcode
                    break;

                case TASKS.BCC:
                    if (isEQ)
                    {
                        Protocol_SendSCT1();
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.BCC);
                    }
                    break;

                case TASKS.WRX1:
                    if (isEQ)
                    {
                        Protocol_SendSCT2();
                    }
                    else
                    {
                        Protocol_SendSCT1();
                    }
                    break;

                case TASKS.WRX2:
                    if (isEQ)
                    {
                        Protocol_SendSCT3();
                    }
                    else
                    {
                        Protocol_SendSCT2();
                    }
                    break;

                case TASKS.WRX3:
                    if (isEQ)
                    {
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.iBus) || (MyDevice.mSXT.S_DeviceType == TYPE.TD485) || (MyDevice.mSXT.S_DeviceType == TYPE.TP10) || (MyDevice.mSXT.S_DeviceType == TYPE.iNet) || (MyDevice.mSXT.S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendSCT6();
                        }
                        else
                        {
                            if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mSXT);
                        }
                    }
                    else
                    {
                        Protocol_SendSCT3();
                    }
                    break;

                case TASKS.WRX6:
                    if (isEQ)
                    {
                        Protocol_SendSCT7();
                    }
                    else
                    {
                        Protocol_SendSCT6();
                    }
                    break;

                case TASKS.WRX7:
                    if (isEQ)
                    {
                        Protocol_SendSCT8();
                    }
                    else
                    {
                        Protocol_SendSCT7();
                    }
                    break;

                case TASKS.WRX8:
                    if (isEQ)
                    {
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendSCT8();
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX0 -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> WRX5 -> WRX6 -> WRX7 -> WRX8 -> WRX9 -> RST
        //标定参数修改用
        public void Protocol_mePort_WriteAllTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callSecondDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BCC);//开始先校验Bohrcode
                    break;

                case TASKS.BCC:
                    if (isEQ)
                    {
                        Protocol_SendSCT0();
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.BCC);
                    }
                    break;

                case TASKS.WRX0:
                    if (isEQ)
                    {
                        Protocol_SendSCT1();
                    }
                    else
                    {
                        Protocol_SendSCT0();
                    }
                    break;

                case TASKS.WRX1:
                    if (isEQ)
                    {
                        Protocol_SendSCT2();
                    }
                    else
                    {
                        Protocol_SendSCT1();
                    }
                    break;

                case TASKS.WRX2:
                    if (isEQ)
                    {
                        Protocol_SendSCT3();
                    }
                    else
                    {
                        Protocol_SendSCT2();
                    }
                    break;

                case TASKS.WRX3:
                    if (isEQ)
                    {
                        Protocol_SendSCT4();
                    }
                    else
                    {
                        Protocol_SendSCT3();
                    }
                    break;

                case TASKS.WRX4:
                    if (isEQ)
                    {
                        switch (MyDevice.mSXT.S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mSXT);
                                }
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mSXT.E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendSCT5();
                                }
                                else
                                {
                                    Protocol_SendCOM(TASKS.REST);
                                    if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                    {
                                        trTSK = TASKS.NULL;
                                        MyDevice.SaveToLog(MyDevice.mSXT);
                                    }
                                }
                                break;

                            case TYPE.TD485:
                            case TYPE.TCAN:
                            case TYPE.iBus:
                            case TYPE.iNet:
                            case TYPE.iStar:
                                Protocol_SendSCT5();
                                break;
                        }
                    }
                    else
                    {
                        Protocol_SendSCT4();
                    }
                    break;

                case TASKS.WRX5:
                    if (isEQ)
                    {
                        if ((MyDevice.mSXT.S_DeviceType == TYPE.iBus) || (MyDevice.mSXT.S_DeviceType == TYPE.TD485) || (MyDevice.mSXT.S_DeviceType == TYPE.TP10) || (MyDevice.mSXT.S_DeviceType == TYPE.iNet) || (MyDevice.mSXT.S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendSCT6();
                        }
                        else if (MyDevice.mSXT.S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mSXT.E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendSCT6();
                            }
                            else
                            {
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mSXT);
                                }
                            }
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mSXT);
                            }
                        }
                    }
                    else
                    {
                        Protocol_SendSCT5();
                    }
                    break;

                case TASKS.WRX6:
                    if (isEQ)
                    {
                        Protocol_SendSCT7();
                    }
                    else
                    {
                        Protocol_SendSCT6();
                    }
                    break;

                case TASKS.WRX7:
                    if (isEQ)
                    {
                        Protocol_SendSCT8();
                    }
                    else
                    {
                        Protocol_SendSCT7();
                    }
                    break;

                case TASKS.WRX8:
                    if (isEQ)
                    {
                        if (MyDevice.mSXT.S_DeviceType == TYPE.TCAN || MyDevice.mSXT.S_DeviceType == TYPE.iNet)//有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mSXT);
                            }
                        }
                    }
                    else
                    {
                        Protocol_SendSCT8();
                    }
                    break;

                case TASKS.WRX9:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.REST);
                        if (MyDevice.mSXT.E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mSXT);
                        }
                    }
                    else
                    {
                        Protocol_SendSCT9();
                    }
                    break;

                case TASKS.REST:
                    if (isEQ)
                    {
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mSXT);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }
    }
}
