using System;
using System.Runtime.InteropServices;
using System.Threading;

//Lumi 202301123

namespace Model
{
    //1.ZLGCAN系列接口卡信息的数据类型。
    public struct VCI_BOARD_INFO
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;
    }

    //2.定义CAN信息帧的数据类型。
    unsafe public struct VCI_CAN_OBJ  //使用不安全代码
    {
        public uint ID;
        public uint TimeStamp;        //时间标识
        public byte TimeFlag;         //是否使用时间标识
        public byte SendType;         //发送标志。保留，未用
        public byte RemoteFlag;       //是否是远程帧, 0=数据帧, 1=远程帧
        public byte ExternFlag;       //是否是扩展帧, 0=标准帧, 1=扩展帧
        public byte DataLen;          //数据长度
        public fixed byte Data[8];    //数据
        public fixed byte Reserved[3];//保留位
    }

    //3.定义初始化CAN的数据类型
    public struct VCI_INIT_CONFIG
    {
        public UInt32 AccCode;
        public UInt32 AccMask;
        public UInt32 Reserved;
        public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
        public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
        public byte Timing1;
        public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
    }

    //4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
    public struct VCI_BOARD_INFO1
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        public byte Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_Usb_Serial;
    }

    class CANPort
    {
        /*------------兼容ZLG的函数描述---------------------------------*/


        [DllImport("controlcan.dll")] //VCI_OpenDevice需与VCI_CloseDevice成对出现
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);

        [DllImport("controlcan.dll")] //VCI_CloseDevice需与VCI_OpenDevice成对出现
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);

        [DllImport("controlcan.dll")] //初始化CAN通道
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")] //获取在CAN适配器某个通道缓冲区中已经接收到的但未被VCI_Recive函数读取的帧的数量,VCI_Receive会返回数量,因此可不用VCI_GetReceiveNum
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")] //Len一定要小于等于pReceive数组大小,WaitTime保留参数
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);

        [DllImport("controlcan.dll")] //等同拔插一次USB, 需要再次VCI_OpenDevice
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);

        [DllImport("controlcan.dll")] //查询CanIndex有多少
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);

        /*------------CAN设备-------------------------------------*/

        public UInt32 CanType = 4;              //CAN盒子型号,3=USBCAN, 4=USBCAN2
        public UInt32 CanIndex = 0;             //CAN盒子索引号0,1,2,3...
        public UInt32 CanChannel = 0;           //CAN盒子的通道索引号0,1
        public Boolean IsOpen = false;          //false=关闭, true=打开

        /*------------CAN设备参数---------------------------------*/

        public Byte CanFrames = 2;              //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
        public Int32 BaudRate = 250;            //CAN 波特率, 单位kbps
        public UInt32 AccCode = 0x00000000;     //CAN ID 匹配值, AccCode和AccMask配合使用
        public UInt32 AccMask = 0xFFFFFFFF;     //CAN ID 过滤值, AccCode和AccMask配合使用

        /*------------CAN设备数据---------------------------------*/

        //环形缓冲区
        //参考：https://blog.csdn.net/jiejiemcu/article/details/80563422
        public struct RingBuff_t
        {
            public UInt32 Head;                  //队列头的index，从头部出队
            public UInt32 Tail;                  //队列尾的index，从尾部入队
            public UInt32 Length;                //队列长度
            public VCI_CAN_OBJ[] Ring_Buff;      //数据帧
        }

        //该环形缓冲区此处使用时，需要加互斥锁
        private static readonly object lock_send = new object();               //确保同一时间只有一个线程被执行
        private static readonly object lock_receive = new object();            //确保同一时间只有一个线程被执行

        //大部分CAN盒子3000-8800帧每秒
        //https://blog.csdn.net/u010443710/article/details/107476570
        //网友测试帧间隔时间11us,大约0.13ms一帧数据,共12188帧耗时1.67s

        //因此50ms读取一次数据,读取数组缓存最小=8800/(1000/50)=440
        //设接收数据缓存2000
        //设接收长度1800
        RingBuff_t rxBuff;                       //接收数据缓存
        private UInt32 rxBuffLen = 2000;         //接收数据缓存2000
        VCI_CAN_OBJ rFrame = new VCI_CAN_OBJ();  //接收帧
        private Thread _thread;                  //接收线程
        private void RxBuff_Init()
        {
            rxBuff.Head = 0;
            rxBuff.Tail = 0;
            rxBuff.Length = 0;
            rxBuff.Ring_Buff = new VCI_CAN_OBJ[rxBuffLen];
        }            //初始化接收缓冲区

        //硬件设备最大提供10帧缓存
        //注意保证CAN总线占用不应该超过总线容量的60-70%
        //VCI_Transmit函数调用间隔至少应设置在5ms以上
        RingBuff_t txBuff;                       //发送数据缓存
        private UInt32 txBuffLen = 10;           //发送数据缓存10
        VCI_CAN_OBJ tFrame = new VCI_CAN_OBJ();  //发送帧
        private void TxBuff_Init()
        {
            txBuff.Head = 0;
            txBuff.Tail = 0;
            txBuff.Length = 0;
            txBuff.Ring_Buff = new VCI_CAN_OBJ[txBuffLen];
        }            //初始化发送缓冲区

        public UInt32 FramesToRead { get { return rxBuff.Length; } }         //接收数据缓存中数据的帧数
        public UInt32 FramesToWrite { get { return txBuff.Length; } }        //发送数据缓存中数据的帧数

        public event SerialFrameReceivedEventHandler FrameReceived;          //定义事件
        private static readonly object lock_callDelegate = new object();     //事件委托的锁

        private UInt32 frameIDHeartBeat = 0;                                 //当前接收到的帧ID
        public UInt32 FrameIDHeartBeat { get { return frameIDHeartBeat; } }  //当前接收到的帧ID

        /*------------CAN设备方法---------------------------------*/

        public void Open()
        {
            if (IsOpen)
            {
                VCI_ResetCAN(CanType, CanIndex, CanChannel);

                System.Threading.Thread.Sleep(100);

                VCI_StartCAN(CanType, CanIndex, CanChannel);
            }
            else
            {
                if (VCI_OpenDevice(CanType, CanIndex, 0) != 0)
                {
                    VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();

                    config.AccCode = AccCode;
                    config.AccMask = AccMask;
                    config.Filter = CanFrames;
                    config.Mode = 0;

                    switch (BaudRate)
                    {
                        case 10:
                            config.Timing0 = 0x31;
                            config.Timing1 = 0x1C;
                            break;
                        case 20:
                            config.Timing0 = 0x18;
                            config.Timing1 = 0x1C;
                            break;
                        case 40:
                            config.Timing0 = 0x87;
                            config.Timing1 = 0xFF;
                            break;
                        case 50:
                            config.Timing0 = 0x09;
                            config.Timing1 = 0x1C;
                            break;
                        case 80:
                            config.Timing0 = 0x83;
                            config.Timing1 = 0xFF;
                            break;
                        case 100:
                            config.Timing0 = 0x04;
                            config.Timing1 = 0x1C;
                            break;
                        case 125:
                            config.Timing0 = 0x03;
                            config.Timing1 = 0x1C;
                            break;
                        case 200:
                            config.Timing0 = 0x81;
                            config.Timing1 = 0xFA;
                            break;
                        case 250:
                            config.Timing0 = 0x01;
                            config.Timing1 = 0x1C;
                            break;
                        case 400:
                            config.Timing0 = 0x80;
                            config.Timing1 = 0xFA;
                            break;
                        case 500:
                            config.Timing0 = 0x00;
                            config.Timing1 = 0x1C;
                            break;
                        case 666:
                            config.Timing0 = 0x80;
                            config.Timing1 = 0xB6;
                            break;
                        case 800:
                            config.Timing0 = 0x00;
                            config.Timing1 = 0x16;
                            break;
                        case 1000:
                            config.Timing0 = 0x00;
                            config.Timing1 = 0x14;
                            break;
                        default://和250相等
                            config.Timing0 = 0x01;
                            config.Timing1 = 0x1C;
                            break;
                    }

                    VCI_InitCAN(CanType, CanIndex, CanChannel, ref config);

                    VCI_StartCAN(CanType, CanIndex, CanChannel);

                    IsOpen = true;

                    RxBuff_Init();    // 初始化接收缓冲区
                    TxBuff_Init();    // 初始化发送缓冲区

                    _thread = new Thread(ReceiveThread);
                    _thread.Name = nameof(ReceiveThread);
                    _thread.Start();  //启动接收子线程
                }
                else
                {
                    IsOpen = false;
                }
            }
        }

        public void Close()
        {
            IsOpen = false;

            _thread.Join();
            _thread = null;

            VCI_CloseDevice(CanType, CanIndex);
        }

        public UInt32 GetPortNumber()
        {
            VCI_BOARD_INFO1 info = new VCI_BOARD_INFO1();

            return VCI_FindUsbDevice(ref info);
        }

        //获取在CAN适配器某个通道缓冲区中已经接收到的但未被VCI_Recive函数读取的帧的数量
        public UInt32 GetReceiveNum()
        {
            return VCI_GetReceiveNum(CanType, CanIndex, CanChannel);
        }

        unsafe public void DiscardInBuffer()
        {
            VCI_ClearBuffer(CanType, CanIndex, CanChannel);

            rxBuff.Head = 0;
            rxBuff.Tail = 0;
            rxBuff.Length = 0;

            for (int i = 0; i < rxBuff.Ring_Buff.Length; i++)
            {
                rxBuff.Ring_Buff[i].ID = 0;
                rxBuff.Ring_Buff[i].TimeStamp = 0;
                rxBuff.Ring_Buff[i].TimeFlag = 0;
                rxBuff.Ring_Buff[i].SendType = 0;
                rxBuff.Ring_Buff[i].RemoteFlag = 0;
                rxBuff.Ring_Buff[i].ExternFlag = 0;
                rxBuff.Ring_Buff[i].DataLen = 0;

                rxBuff.Ring_Buff[i].Data[0] = 0;
                rxBuff.Ring_Buff[i].Data[1] = 0;
                rxBuff.Ring_Buff[i].Data[2] = 0;
                rxBuff.Ring_Buff[i].Data[3] = 0;
                rxBuff.Ring_Buff[i].Data[4] = 0;
                rxBuff.Ring_Buff[i].Data[5] = 0;
                rxBuff.Ring_Buff[i].Data[6] = 0;
                rxBuff.Ring_Buff[i].Data[7] = 0;
            }
        }

        unsafe public void DiscardOutBuffer()
        {
            VCI_ClearBuffer(CanType, CanIndex, CanChannel);

            txBuff.Head = 0;
            txBuff.Tail = 0;
            txBuff.Length = 0;

            for (int i = 0; i < txBuff.Ring_Buff.Length; i++)
            {
                txBuff.Ring_Buff[i].ID = 0;
                txBuff.Ring_Buff[i].TimeStamp = 0;
                txBuff.Ring_Buff[i].TimeFlag = 0;
                txBuff.Ring_Buff[i].SendType = 0;
                txBuff.Ring_Buff[i].RemoteFlag = 0;
                txBuff.Ring_Buff[i].ExternFlag = 0;
                txBuff.Ring_Buff[i].DataLen = 0;

                txBuff.Ring_Buff[i].Data[0] = 0;
                txBuff.Ring_Buff[i].Data[1] = 0;
                txBuff.Ring_Buff[i].Data[2] = 0;
                txBuff.Ring_Buff[i].Data[3] = 0;
                txBuff.Ring_Buff[i].Data[4] = 0;
                txBuff.Ring_Buff[i].Data[5] = 0;
                txBuff.Ring_Buff[i].Data[6] = 0;
                txBuff.Ring_Buff[i].Data[7] = 0;
            }
        }

        //从接收缓冲区中读取一帧
        public VCI_CAN_OBJ ReadFrame()
        {
            if (Read_rxBuff())    //读出成功，返回接收缓冲区的队首
            {
                return rFrame;
            }
            else                  //读出失败，返回一个空的VCI_CAN_OBJ
            {
                return new VCI_CAN_OBJ();
            }
        }

        //将指定数量的字节写入端口。count:指定的数量，范围1-10，默认1
        public unsafe void Write(VCI_CAN_OBJ[] frameData, int count = 1)
        {
            lock (lock_send)
            {
                bool isWriten = true;           //是否已成功存入发送缓冲区
                for (int i = 0; i < count; i++)
                {
                    if (!Write_txBuff(frameData[i]))
                    {
                        isWriten = false;
                        break;
                    }
                }

                if (isWriten)
                {
                    for (int i = 0; i < txBuff.Length; i++)
                    {
                        VCI_CAN_OBJ vCI_CAN_OBJ = new VCI_CAN_OBJ();
                        vCI_CAN_OBJ = txBuff.Ring_Buff[txBuff.Head];    //取发送缓冲区队列队首帧，但暂不将其从发送缓冲区中移出

                        var result = VCI_Transmit(CanType, CanIndex, CanChannel, ref vCI_CAN_OBJ, 1);   //发送队首帧

                        if (result > 0)          //发送成功
                        {
                            if (!Read_txBuff())  //将发送的帧从发送缓冲区中移出
                            {
                                isWriten = false;//发送失败
                                break;
                            }
                        }
                        else                     //USB-CAN设备不存在或USB掉线
                        {
                            isWriten = false;    //发送失败
                            IsOpen = false;      //此时需要停止发送，否则会导致软件卡死
                        }

                        Thread.Sleep(5);         //VCI_Transmit函数调用间隔至少应设置在5ms以上
                    }
                }
            }
        }

        //接收线程
        private unsafe void ReceiveThread()
        {
            while (IsOpen)
            {
                Thread.Sleep(1);   //VCI_Receive函数调用间隔至少应设置在0.15ms以上

                VCI_CAN_OBJ[] frameDatas = new VCI_CAN_OBJ[2000];

                var result = VCI_Receive(CanType, CanIndex, CanChannel, ref frameDatas[0], 1000, 100);   //接收函数
                if (result > 0 && result < 2000)
                {
                    for (int i = 0; i < result; i++)
                    {
                        if (Write_rxBuff(frameDatas[i]))    //将接收到的数据存入接收缓冲区
                        {
                            getReciveFrameID(frameDatas[i]);
                        }
                        else
                        {
                            //存入到缓冲区失败
                        }
                    }
                    callDelegate();
                }
            }
        }

        //往接收缓冲区中写入一帧数据
        private bool Write_rxBuff(VCI_CAN_OBJ frame)
        {
            lock (lock_receive)                  //使用锁来保护共享资源的访问
            {
                if (rxBuff.Length >= rxBuffLen)  //判断缓冲区是否已满
                {
                    return false;                //缓冲区已满，写入失败
                }
                rxBuff.Ring_Buff[rxBuff.Tail] = frame;
                rxBuff.Tail = Convert.ToUInt32((rxBuff.Tail + 1) % rxBuffLen);     //防止越界非法访问
                rxBuff.Length++;
                return true;                     //写入成功
            }
        }

        //从接收缓冲区读取一帧数据
        private bool Read_rxBuff()
        {
            lock (lock_receive)                  //使用锁来保护共享资源的访问
            {
                if (rxBuff.Length == 0)          //判断非空
                {
                    return false;
                }
                rFrame = rxBuff.Ring_Buff[rxBuff.Head];                         //先进先出fifo，从缓冲区头出
                rxBuff.Head = Convert.ToUInt32((rxBuff.Head + 1) % rxBuffLen);  //防止越界非法访问
                rxBuff.Length--;
                return true;
            }
        }

        //往发送缓冲区中写入一帧数据
        private bool Write_txBuff(VCI_CAN_OBJ frame)
        {
            lock (lock_send)                     //使用锁来保护共享资源的访问
            {
                if (txBuff.Length >= txBuffLen)  //判断缓冲区是否已满
                {
                    return false;                //缓冲区已满，写入失败
                }
                txBuff.Ring_Buff[txBuff.Tail] = frame;
                txBuff.Tail = Convert.ToUInt32((txBuff.Tail + 1) % txBuffLen);     //防止越界非法访问
                txBuff.Length++;
                return true;                     //写入成功
            }
        }

        //从发送缓冲区读取一帧数据
        private bool Read_txBuff()
        {
            lock (lock_send)                     //使用锁来保护共享资源的访问
            {
                if (txBuff.Length == 0)          //判断非空
                {
                    return false;
                }
                tFrame = txBuff.Ring_Buff[txBuff.Head];                         //先进先出fifo，从缓冲区头出
                txBuff.Head = Convert.ToUInt32((txBuff.Head + 1) % txBuffLen);  //防止越界非法访问
                txBuff.Length--;
                return true;
            }
        }

        //获取当前接收到的帧的帧ID
        private void getReciveFrameID(VCI_CAN_OBJ vCI_CAN_OBJ)
        {
            switch (vCI_CAN_OBJ.DataLen)
            {
                case 1:
                    uint ID = vCI_CAN_OBJ.ID;
                    if (ID > 0x700 && ID < 0x780)
                    {
                        frameIDHeartBeat = vCI_CAN_OBJ.ID;
                    }
                    break;
                default:
                    break;
            }
        }

        //委托
        public void callDelegate()
        {
            if (FrameReceived == null) return;

            lock (lock_callDelegate)
            {
                FrameReceived();
            }
        }
    }

    public delegate void SerialFrameReceivedEventHandler();   //定义委托
}