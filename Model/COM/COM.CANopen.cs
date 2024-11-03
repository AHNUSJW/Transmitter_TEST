using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

//未经过审批不得改动

//Alvin 20230319
//Lumi 20231222

namespace Model
{
    public class CANopenProtocol : IProtocol
    {
        #region 定义变量

        //
        private Byte sAddress = 5;  //访问mCAN[128]的指针

        //
        private volatile bool is_serial_listening = false;  //串口正在监听标记
        private volatile bool is_serial_closing = false;    //串口正在关闭标记

        //
        private CANPort mePort = new CANPort();         //CANopen接口使用的串口
        private TASKS trTSK = TASKS.NULL;               //接口读写任务状态机
        private RXSTP rxSTP = RXSTP.NUL;                //通讯接收状态机,找出帧头帧尾
        private Int32 txCnt = 0;                        //发送计数
        private Int32 rxCnt = 0;                        //接收计数
        private Int32 rxDat = 0;                        //接收数据数值
        private String rxStr = null;                    //接收数据字符串
        private Boolean isEQ = false;                   //接收检查

        //
        private Byte[] meTXD = new Byte[200];           //发送缓冲区
        private Byte[] meRXD = new Byte[1000];          //接收缓冲区
        private Int32 rxRead = 0;                       //接收缓冲区读指针
        private Int32 rxWrite = 0;                      //接收缓冲区写指针

        private static readonly object _lock_rtcmd = new object();                            //确保同一时间只有一个发送线程被执行
        private SortedDictionary<byte, byte[]> meSCT = new SortedDictionary<byte, byte[]>();  //记录SCT,确保块传输接收SCT时数据顺序不变乱
        private Byte[] meTXD_SCT = new Byte[200];                                             //缓存写SCT，用于求校验和
        private SortedDictionary<byte, byte[]> meBOR = new SortedDictionary<byte, byte[]>();  //记录bohrcode,确保块传输接收bohrcode时数据顺序不变乱

        #endregion

        #region 定义属性
        public Byte addr
        {
            set
            {
                sAddress = value;
            }
            get
            {
                return sAddress;
            }
        }
        public COMP type
        {
            get
            {
                return COMP.CANopen;
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
                if (mePort.CanChannel != 1)
                {
                    return "CAN1";
                }
                else
                {
                    return "CAN2";
                }
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
                return StopBits.None;
            }
        }
        public Parity parity
        {
            get
            {
                return Parity.None;
            }
        }
        public UInt32 channel
        {
            get
            {
                return mePort.CanChannel;
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
        public CANopenProtocol()
        {
            mePort.CanType = 4;//固定USBCAN2
            mePort.CanIndex = 0;//固定0
            mePort.CanChannel = 0;
            mePort.CanFrames = 2;//固定标准帧
            mePort.BaudRate = 250;
            mePort.AccCode = 0x00000000;//固定
            mePort.AccMask = 0xFFFFFFFF;//固定
            mePort.IsOpen = false;

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

        //不是Uart
        public void Protocol_PortOpen(String name, Int32 baud, StopBits stb, Parity pay)
        {

        }

        //打开CAN
        public void Protocol_PortOpen(UInt32 index, String name, Int32 baud)
        {
            //修改参数必须先关闭串口
            if ((("CH" + (mePort.CanChannel + 1)) != name) || (mePort.BaudRate != baud))
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

            //尝试打开
            try
            {
                //初始化串口监听标记
                is_serial_listening = false;
                is_serial_closing = false;

                //
                mePort.CanIndex = index;
                switch (name)
                {
                    default:
                    case "CH1":
                        mePort.CanChannel = 0;
                        break;
                    case "CH2":
                        mePort.CanChannel = 1;
                        break;
                }
                mePort.BaudRate = baud;
                mePort.Open();
                mePort.FrameReceived += new SerialFrameReceivedEventHandler(Protocol_mePort_DataReceived); //接收处理

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

        //关闭CAN
        public bool Protocol_PortClose()
        {
            if (mePort.IsOpen)
            {
                try
                {
                    mePort.FrameReceived -= new SerialFrameReceivedEventHandler(Protocol_mePort_DataReceived); //接收处理
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
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

        #region 校验接收帧
        //校验读bohrcode
        private bool ValidateData_BOR(byte[] data, byte subIndex)
        {
            if (subIndex == Constants.Sub_Index_00)
            {
                if (data[0] != Constants.SDO_RD_4_Bytes) return false;  // 固定值校验失败
            }
            if (subIndex == Constants.Sub_Index_01)
            {
                if (data[0] != Constants.SDO_RD_3_Bytes) return false;  // 固定值校验失败
            }
            if (data[1] != (Constants.Index_TCAN_GET_Code & 0xFF) || data[2] != (Constants.Index_TCAN_GET_Code >> 8))
            {
                return false;  // 索引校验失败
            }
            switch (subIndex)
            {
                case Constants.Sub_Index_00:
                case Constants.Sub_Index_01:
                    if (data[3] != subIndex)
                    {
                        return false;  //校验子索引
                    }
                    return true;       //校验通过
                default:
                    return false;
            }
        }

        //校验写SCT开始
        private bool ValidateData_StartWRX(byte[] data)
        {
            if (data[0] != Constants.SDO_BLK_StaAck || data[1] != (Constants.Index_SDO_BLOCK & 0xFF) || data[2] != (Constants.Index_SDO_BLOCK >> 8) || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00)
            {
                return false;    // 固定值校验失败
            }
            if (data[3] >= Constants.Sub_Index_00 && data[3] <= Constants.Sub_Index_08)
            {
                if (data[4] != Constants.SDO_BLK_SIZE_SCT)
                {
                    return false; // 字节数校验失败
                }
            }
            else if (data[3] == Constants.Sub_Index_09)
            {
                if (data[4] != Constants.SDO_BLK_SIZE_SCT9)
                {
                    return false; // 字节数校验失败
                }
            }
            else
            {
                return false;     // 子索引号校验失败
            }
            return true;          // 数据校验通过
        }

        //校验写SCT结束
        private bool ValidateData_EndWRX(byte[] data)
        {
            if (data[0] != Constants.SDO_BLK_EndAck || data[1] != 0x00 || data[2] != 0x00 || data[3] != 0x00 || data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;    // 固定值校验失败
            return true;         // 数据校验通过
        }

        //校验写SCT数据段结束
        private bool ValidateData_EndWRX_Segment(byte[] data)
        {
            if (data[0] != Constants.SDO_BLK_TraAck || data[3] != 0x00 || data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;    // 固定值校验失败
            if (!((data[1] != 0x07 && data[2] != 0x07) || (data[1] != 0x10 & data[2] != 0x10))) return false;    // 段号段数校验失败
            return true;         // 数据校验通过
        }

        //校验读SCT开始
        private bool ValidateData_StartRDX(byte[] data)
        {
            if (data[0] != Constants.SDO_BLK_StaReq || data[1] != (Constants.Index_SDO_BLOCK & 0xFF) || data[2] != (Constants.Index_SDO_BLOCK >> 8) || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00)
            {
                return false;    // 固定值校验失败
            }
            if (data[3] >= Constants.Sub_Index_00 && data[3] <= Constants.Sub_Index_08)
            {
                if (data[4] != Constants.SDO_BLK_BYTE_SCT)
                {
                    return false; // 字节数校验失败
                }
            }
            else if (data[3] == Constants.Sub_Index_09)
            {
                if (data[4] != Constants.SDO_BLK_BYTE_SCT9)
                {
                    return false; // 字节数校验失败
                }
            }
            else
            {
                return false;     // 子索引号校验失败
            }
            return true;          // 数据校验通过
        }

        //校验读SCT结束
        private bool ValidateData_EndRDX(byte[] data)
        {
            if (data[0] != Constants.SDO_BLK_END_SCT && data[0] != Constants.SDO_BLK_END_SCT9) return false;    // 固定值校验失败
            if (data[3] != 0x00 || data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;    // 固定值校验失败
            return true;         // 数据校验通过
        }

        //校验读归零
        private bool ValidateData_ZERO(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_WR_REPLY) return false;  //校验首位
            if (data[1] != (Constants.Index_MEAS_AN1_ZERO & 0xFF) || data[2] != (Constants.Index_MEAS_AN1_ZERO >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            if (data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;   //校验零
            return true;
        }

        //校验复位
        private bool ValidateData_REST(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_WR_REPLY) return false;  //校验首位
            if (data[1] != (Constants.Index_REST & 0xFF) || data[2] != (Constants.Index_REST >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            if (data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;   //校验零
            return true;
        }

        //校验心跳
        private bool ValidateData_WHEART(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_WR_REPLY) return false;  //校验首位
            if (data[1] != (Constants.Index_HEARTBEAT & 0xFF) || data[2] != (Constants.Index_HEARTBEAT >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            if (data[4] != 0x00 || data[5] != 0x00 || data[6] != 0x00 || data[7] != 0x00) return false;   //校验零
            return true;
        }

        //校验读ADC
        private bool ValidateData_ADC(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_RD_4_Bytes) return false;  //校验首位
            if (data[1] != (Constants.Index_TCAN_RAD_Adc & 0xFF) || data[2] != (Constants.Index_TCAN_RAD_Adc >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            return true;
        }

        //校验读MEAS_WEIGHT
        private bool ValidateData_WEIGHT(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_RD_4_Bytes) return false;  //校验首位
            if (data[1] != (Constants.Index_MEAS_WEIGHT & 0xFF) || data[2] != (Constants.Index_MEAS_WEIGHT >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            return true;
        }

        //校验读MEAS_AN1
        private bool ValidateData_AN1(byte[] data)
        {
            //帧
            if (data[0] != Constants.SDO_RD_4_Bytes) return false;  //校验首位
            if (data[1] != (Constants.Index_MEAS_AN1 & 0xFF) || data[2] != (Constants.Index_MEAS_AN1 >> 8) || data[3] != Constants.Sub_Index_00) return false;   //校验索引
            return true;
        }

        #endregion

        //转16进制字符串
        private byte[] GetByteArray(string shex)
        {
            string[] ssArray = shex.Split(' ');
            List<byte> bytList = new List<byte>();
            foreach (var s in ssArray)
            {
                //将十六进制的字符串转换成数值
                bytList.Add(Convert.ToByte(s, 16));
            }
            //返回字节数组
            return bytList.ToArray();
        }

        //CRC校验
        private byte[] AP_CRC16_MODBUS(Byte[] pData, int len)
        {
            //校验XL
            Byte[] TABLE_8005_LO = new Byte[] {
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            };

            //校验XH
            Byte[] TABLE_8005_HI = new Byte[] {
                0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04,
                0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8,
                0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
                0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 0x11, 0xD1, 0xD0, 0x10,
                0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
                0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
                0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C,
                0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0,
                0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
                0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
                0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C,
                0xB4, 0x74, 0x75, 0xB5, 0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
                0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54,
                0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98,
                0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80, 0x40,
            };

            ushort CRC_Init_0xFFFF = 0xffff;

            Byte crchi = (Byte)(CRC_Init_0xFFFF >> 8);
            Byte crclo = (Byte)(CRC_Init_0xFFFF);
            Byte index;

            for (int i = 0; i < len; i++)
            {
                index = (Byte)(crclo ^ pData[i]);
                crclo = (Byte)(crchi ^ TABLE_8005_LO[index]);
                crchi = TABLE_8005_HI[index];
            }

            string ss = (crclo << 8 | crchi).ToString("X4");
            ss = ss.Insert(2, " ");
            byte[] bt;
            bt = GetByteArray(ss);
            return bt;
        }

        // 求校验和数据
        public ushort AP_CRC16_XMODEN(Byte[] pData, int len, Boolean XHXL)
        {
            ushort CRC_Init_0x0000 = 0x0000;

            Byte crchi;
            Byte crclo;

            for (int i = 0; i < len; i++)
            {
                CRC_Init_0x0000 ^= (ushort)(pData[i] << 8);

                for (Byte j = 0; j < 8; j++)
                {
                    if (CRC_Init_0x0000 >= 0x8000)
                    {
                        CRC_Init_0x0000 = (ushort)((CRC_Init_0x0000 << 1) ^ 0x1021);
                    }
                    else
                    {
                        CRC_Init_0x0000 <<= 1;
                    }
                }
            }

            if (XHXL)
            {
                return CRC_Init_0x0000;
            }
            else
            {
                crchi = (Byte)(CRC_Init_0x0000 >> 8);
                crclo = (Byte)(CRC_Init_0x0000);
                return (ushort)(crclo << 8 | crchi);
            }
        }

        //发送读命令
        public void Protocol_SendCOM(TASKS meTask)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
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
                    (new Byte[] { Constants.SDO_RD_Request, Constants.Index_TCAN_GET_Code & 0xFF, Constants.Index_TCAN_GET_Code >> 8, Constants.Sub_Index_00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //读SCT
                case TASKS.RDX0:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_00, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX1:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_01, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX2:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_02, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX3:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_03, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX4:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_04, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX5:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_05, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX6:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_06, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX7:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_07, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX8:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_08, Constants.SDO_BLK_SIZE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX9:
                    (new Byte[] { Constants.SDO_BLK_StaAck, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_09, Constants.SDO_BLK_SIZE_SCT9, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //采样
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
                    (new Byte[] { Constants.SDO_RD_Request, Constants.Index_TCAN_RAD_Adc & 0xFF, Constants.Index_TCAN_RAD_Adc >> 8, Constants.Sub_Index_00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //归零,重启
                case TASKS.ZERO:
                    (new Byte[] { Constants.SDO_WR_1_Bytes, Constants.Index_MEAS_AN1_ZERO & 0xFF, Constants.Index_MEAS_AN1_ZERO >> 8, Constants.Sub_Index_00, 0x91, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.REST:
                    (new Byte[] { Constants.SDO_WR_1_Bytes, Constants.Index_REST & 0xFF, Constants.Index_REST >> 8, Constants.Sub_Index_00, 0xAA, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //adcout,dacout
                case TASKS.ADC:
                    (new Byte[] { Constants.SDO_RD_Request, Constants.Index_MEAS_AN1 & 0xFF, Constants.Index_MEAS_AN1 >> 8, Constants.Sub_Index_00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.DAC:
                    (new Byte[] { Constants.SDO_RD_Request, Constants.Index_MEAS_WEIGHT & 0xFF, Constants.Index_MEAS_WEIGHT >> 8, Constants.Sub_Index_00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                default:
                    return;
            }

            trTSK = meTask;
            txCnt += 8;
            isEQ = false;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送读第二段bohrcode
        private void Protocol_SendBohr2()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //装帧
            (new Byte[] { Constants.SDO_RD_Request, Constants.Index_TCAN_GET_Code & 0xFF, Constants.Index_TCAN_GET_Code >> 8, Constants.Sub_Index_01, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);

            //
            txCnt += 8;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送块传输上传开始
        private void Protocol_SendSegmentStart()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //装帧
            (new Byte[] { Constants.SDO_BLK_StaUp, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);

            //
            txCnt += 8;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送块传输SCT上传结束
        private void Protocol_SendSegmentUpdateEnd(Byte segmentID, Byte segmentNum)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //装帧
            (new Byte[] { Constants.SDO_BLK_TraAck, segmentID, segmentNum, 0x00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);

            //
            txCnt += 8;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送块传输SCT下载结束
        private void Protocol_SendSegmentDownloadEnd()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //求校验和数据，装帧
            ushort XLXH = 0x0000;
            switch (trTSK)
            {
                //写SCT
                case TASKS.WRX0:
                case TASKS.WRX1:
                case TASKS.WRX2:
                case TASKS.WRX3:
                case TASKS.WRX4:
                case TASKS.WRX5:
                case TASKS.WRX6:
                case TASKS.WRX7:
                case TASKS.WRX8:
                    XLXH = AP_CRC16_XMODEN(meTXD_SCT, 44, false);
                    (new Byte[] { Constants.SDO_BLK_END_SCT, (Byte)(XLXH >> 8), (Byte)(XLXH & 0xFF), 0x00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX9:
                    XLXH = AP_CRC16_XMODEN(meTXD_SCT, 108, false);
                    (new Byte[] { Constants.SDO_BLK_END_SCT9, (Byte)(XLXH >> 8), (Byte)(XLXH & 0xFF), 0x00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                default:
                    return;
            }

            //
            txCnt += 8;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送块传输成功完成
        private void Protocol_SendBTEnd()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //装帧
            (new Byte[] { Constants.SDO_BLK_EndAck, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);

            //
            txCnt += 8;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //扫描地址
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

        //串口写入Eeprom参数
        public void Protocol_SendEeprom()
        {

        }

        //发送网络管理命令
        public void Protocol_SendNMT(Byte NMT_CS)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            switch (NMT_CS)
            {
                //网络状态
                case Constants.NMT_CS_Run:
                case Constants.NMT_CS_Stop:
                case Constants.NMT_CS_PRERUN:
                    (new Byte[] { NMT_CS, sAddress }).CopyTo(meTXD, 0);
                    break;
                default:
                    return;
            }

            trTSK = TASKS.WNMT;
            txCnt += 2;
            isEQ = false;
            SendByteBase((uint)Constants.FunID_NMT, meTXD, 0, 2);
        }

        //获取心跳的COB-ID
        public uint Protocol_GetHeartBeatID()
        {
            uint frameID = mePort.FrameIDHeartBeat;
            if (frameID > Constants.FunID_NMTERR)
            {
                return frameID - Constants.FunID_NMTERR;
            }
            else
            {
                return 0;
            }
        }

        //设置心跳时间间隔
        public void Protocol_SendHeartBeat(UInt16 period)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            (new Byte[] { Constants.SDO_WR_2_Bytes, Constants.Index_HEARTBEAT & 0xFF, Constants.Index_HEARTBEAT >> 8, Constants.Sub_Index_00, (Byte)(period & 0xFF), (Byte)(period >> 8), 0x00, 0x00 }).CopyTo(meTXD, 0);

            trTSK = TASKS.WHEART;
            txCnt += 8;
            isEQ = false;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送写入SCT命令包头
        private void Protocol_SendSCT(TASKS meTask)
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            switch (meTask)
            {
                //写SCT
                case TASKS.WRX0:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_00, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX1:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_01, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX2:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_02, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX3:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_03, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX4:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_04, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX5:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_05, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX6:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_06, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX7:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_07, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX8:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_08, Constants.SDO_BLK_BYTE_SCT, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRX9:
                    (new Byte[] { Constants.SDO_BLK_StaReq, Constants.Index_SDO_BLOCK & 0xFF, Constants.Index_SDO_BLOCK >> 8, Constants.Sub_Index_09, Constants.SDO_BLK_BYTE_SCT9, 0x00, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                default:
                    return;
            }

            trTSK = meTask;
            txCnt += 8;
            isEQ = true;
            SendByteBase((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 8);
        }

        //发送写入SCT0命令
        private void Protocol_SendSCT0()
        {
            Protocol_SendSCT(TASKS.WRX0);
        }

        //发送写入SCT1命令
        private void Protocol_SendSCT1()
        {
            Protocol_SendSCT(TASKS.WRX1);
        }

        //发送写入SCT2命令
        private void Protocol_SendSCT2()
        {
            Protocol_SendSCT(TASKS.WRX2);
        }

        //发送写入SCT3命令
        private void Protocol_SendSCT3()
        {
            Protocol_SendSCT(TASKS.WRX3);
        }

        //发送写入SCT4命令
        private void Protocol_SendSCT4()
        {
            Protocol_SendSCT(TASKS.WRX4);
        }

        //发送写入SCT5命令
        private void Protocol_SendSCT5()
        {
            Protocol_SendSCT(TASKS.WRX5);
        }

        //发送写入SCT6命令
        private void Protocol_SendSCT6()
        {
            Protocol_SendSCT(TASKS.WRX6);
        }

        //发送写入SCT7命令
        private void Protocol_SendSCT7()
        {
            Protocol_SendSCT(TASKS.WRX7);
        }

        //发送写入SCT8命令
        private void Protocol_SendSCT8()
        {
            Protocol_SendSCT(TASKS.WRX8);
        }

        //发送写入SCT9命令
        private void Protocol_SendSCT9()
        {
            Protocol_SendSCT(TASKS.WRX9);
        }

        #region 写SCT块传输的包体

        //发送写入SCT0命令块
        private void Protocol_SendSCT0_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX0;
            meTXD[2] = MyDevice.mCAN[sAddress].E_test;
            meTXD[3] = MyDevice.mCAN[sAddress].E_outype;
            meTXD[4] = MyDevice.mCAN[sAddress].E_curve;
            meTXD[5] = MyDevice.mCAN[sAddress].E_adspeed;
            meTXD[6] = MyDevice.mCAN[sAddress].E_autozero;
            meTXD[7] = MyDevice.mCAN[sAddress].E_trackzero;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_checkhigh;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.myUIT.B2;
            meTXD[11] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_checklow;
            meTXD[12] = MyDevice.myUIT.B0;
            meTXD[13] = MyDevice.myUIT.B1;
            meTXD[14] = MyDevice.myUIT.B2;
            meTXD[15] = MyDevice.myUIT.B3;
            MyDevice.myUIT.UI = MyDevice.mCAN[sAddress].E_mfg_date;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_mfg_srno;
            meTXD[20] = MyDevice.myUIT.B0;
            meTXD[21] = MyDevice.myUIT.B1;
            meTXD[22] = MyDevice.myUIT.B2;
            meTXD[23] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_tmp_min;
            meTXD[24] = MyDevice.myUIT.B0;
            meTXD[25] = MyDevice.myUIT.B1;
            meTXD[26] = MyDevice.myUIT.B2;
            meTXD[27] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_tmp_max;
            meTXD[28] = MyDevice.myUIT.B0;
            meTXD[29] = MyDevice.myUIT.B1;
            meTXD[30] = MyDevice.myUIT.B2;
            meTXD[31] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_tmp_cal;
            meTXD[32] = MyDevice.myUIT.B0;
            meTXD[33] = MyDevice.myUIT.B1;
            meTXD[34] = MyDevice.myUIT.B2;
            meTXD[35] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_bohrcode;
            meTXD[36] = MyDevice.myUIT.B0;
            meTXD[37] = MyDevice.myUIT.B1;
            meTXD[38] = MyDevice.myUIT.B2;
            meTXD[39] = MyDevice.myUIT.B3;
            meTXD[40] = MyDevice.mCAN[sAddress].E_enspan;
            meTXD[41] = MyDevice.mCAN[sAddress].E_protype;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT1命令块
        private void Protocol_SendSCT1_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX1;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT2命令块
        private void Protocol_SendSCT2_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX2;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT3命令块
        private void Protocol_SendSCT3_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_zero;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_full;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_vtio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_wtio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_atio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_btio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ctio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_dtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT4命令块
        private void Protocol_SendSCT4_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX4;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero_4ma;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full_20ma;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero_05V;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full_05V;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero_10V;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full_10V;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero_N5;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full_P5;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_zero_N10;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_full_P10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT5命令块
        private void Protocol_SendSCT5_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX5;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_corr;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            meTXD[6] = MyDevice.mCAN[sAddress].E_mark;
            meTXD[7] = MyDevice.mCAN[sAddress].E_sign;
            meTXD[8] = MyDevice.mCAN[sAddress].E_addr;
            meTXD[9] = MyDevice.mCAN[sAddress].E_baud;
            meTXD[10] = MyDevice.mCAN[sAddress].E_stopbit;
            meTXD[11] = MyDevice.mCAN[sAddress].E_parity;
            MyDevice.mCAN[sAddress].E_wt_zero = 0;//强制写0
            meTXD[12] = 0;
            meTXD[13] = 0;
            meTXD[14] = 0;
            meTXD[15] = 0;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_wt_full;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            meTXD[20] = MyDevice.mCAN[sAddress].E_wt_decimal;
            meTXD[21] = MyDevice.mCAN[sAddress].E_wt_unit;
            meTXD[22] = MyDevice.mCAN[sAddress].E_wt_ascii;
            meTXD[23] = MyDevice.mCAN[sAddress].E_wt_sptime;
            meTXD[24] = MyDevice.mCAN[sAddress].E_wt_spfilt;
            meTXD[25] = MyDevice.mCAN[sAddress].E_wt_division;
            meTXD[26] = MyDevice.mCAN[sAddress].E_wt_antivib;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_heartBeat;
            meTXD[27] = MyDevice.myUIT.B0;
            meTXD[28] = MyDevice.myUIT.B1;
            meTXD[29] = MyDevice.mCAN[sAddress].E_typeTPDO0;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_evenTPDO0;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.mCAN[sAddress].E_nodeID;
            meTXD[33] = MyDevice.mCAN[sAddress].E_nodeBaud;
            meTXD[34] = MyDevice.mCAN[sAddress].E_dynazero;
            meTXD[35] = MyDevice.mCAN[sAddress].E_cheatype;
            meTXD[36] = MyDevice.mCAN[sAddress].E_thmax;
            meTXD[37] = MyDevice.mCAN[sAddress].E_thmin;
            meTXD[38] = MyDevice.mCAN[sAddress].E_stablerange;
            meTXD[39] = MyDevice.mCAN[sAddress].E_stabletime;
            meTXD[40] = MyDevice.mCAN[sAddress].E_tkzerotime;
            meTXD[41] = MyDevice.mCAN[sAddress].E_tkdynatime;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT6命令块
        private void Protocol_SendSCT6_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX6;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT7命令块
        private void Protocol_SendSCT7_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX7;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT8命令块
        private void Protocol_SendSCT8_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX8;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ad_point11;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_da_point11;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_input11;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_analog11;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_etio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_ftio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_gtio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_htio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_itio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_jtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            txCnt += 44;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 44);
        }

        //发送写入SCT9命令块
        private void Protocol_SendSCT9_Segment()
        {
            //
            if (!mePort.IsOpen) return;
            while (mePort.FramesToWrite > 0) ;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX9;
            meTXD[2] = MyDevice.mCAN[sAddress].E_enGFC;
            meTXD[3] = MyDevice.mCAN[sAddress].E_enSRDO;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_SCT_time;
            meTXD[4] = MyDevice.myUIT.B0;
            meTXD[5] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_COB_ID1;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_COB_ID2;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.mCAN[sAddress].E_enOL;
            meTXD[11] = MyDevice.mCAN[sAddress].E_overload;
            meTXD[12] = MyDevice.mCAN[sAddress].E_alarmMode;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_wetTarget;
            meTXD[13] = MyDevice.myUIT.B0;
            meTXD[14] = MyDevice.myUIT.B1;
            meTXD[15] = MyDevice.myUIT.B2;
            meTXD[16] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_wetLow;
            meTXD[17] = MyDevice.myUIT.B0;
            meTXD[18] = MyDevice.myUIT.B1;
            meTXD[19] = MyDevice.myUIT.B2;
            meTXD[20] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_wetHigh;
            meTXD[21] = MyDevice.myUIT.B0;
            meTXD[22] = MyDevice.myUIT.B1;
            meTXD[23] = MyDevice.myUIT.B2;
            meTXD[24] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_filter;
            meTXD[25] = MyDevice.myUIT.B0;
            meTXD[26] = MyDevice.myUIT.B1;
            meTXD[27] = MyDevice.myUIT.B2;
            meTXD[28] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_netServicePort;
            meTXD[29] = MyDevice.myUIT.B0;
            meTXD[30] = MyDevice.myUIT.B1;
            meTXD[31] = MyDevice.mCAN[sAddress].E_netServiceIP[0];
            meTXD[32] = MyDevice.mCAN[sAddress].E_netServiceIP[1];
            meTXD[33] = MyDevice.mCAN[sAddress].E_netServiceIP[2];
            meTXD[34] = MyDevice.mCAN[sAddress].E_netServiceIP[3];
            meTXD[35] = MyDevice.mCAN[sAddress].E_netClientIP[0];
            meTXD[36] = MyDevice.mCAN[sAddress].E_netClientIP[1];
            meTXD[37] = MyDevice.mCAN[sAddress].E_netClientIP[2];
            meTXD[38] = MyDevice.mCAN[sAddress].E_netClientIP[3];
            meTXD[39] = MyDevice.mCAN[sAddress].E_netGatIP[0];
            meTXD[40] = MyDevice.mCAN[sAddress].E_netGatIP[1];
            meTXD[41] = MyDevice.mCAN[sAddress].E_netGatIP[2];
            meTXD[42] = MyDevice.mCAN[sAddress].E_netGatIP[3];
            meTXD[43] = MyDevice.mCAN[sAddress].E_netMaskIP[0];
            meTXD[44] = MyDevice.mCAN[sAddress].E_netMaskIP[1];
            meTXD[45] = MyDevice.mCAN[sAddress].E_netMaskIP[2];
            meTXD[46] = MyDevice.mCAN[sAddress].E_netMaskIP[3];
            meTXD[47] = MyDevice.mCAN[sAddress].E_useDHCP;
            meTXD[48] = MyDevice.mCAN[sAddress].E_useScan;
            meTXD[49] = MyDevice.mCAN[sAddress].E_addrRF[0];
            meTXD[50] = MyDevice.mCAN[sAddress].E_addrRF[1];
            meTXD[51] = MyDevice.mCAN[sAddress].E_spedRF;
            meTXD[52] = MyDevice.mCAN[sAddress].E_chanRF;
            meTXD[53] = MyDevice.mCAN[sAddress].E_optionRF;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_lockTPDO0;
            meTXD[54] = MyDevice.myUIT.B0;
            meTXD[55] = MyDevice.myUIT.B1;
            meTXD[56] = MyDevice.mCAN[sAddress].E_entrTPDO0;
            meTXD[57] = MyDevice.mCAN[sAddress].E_typeTPDO1;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_lockTPDO1;
            meTXD[58] = MyDevice.myUIT.B0;
            meTXD[59] = MyDevice.myUIT.B1;
            meTXD[60] = MyDevice.mCAN[sAddress].E_entrTPDO1;
            MyDevice.myUIT.I = MyDevice.mCAN[sAddress].E_evenTPDO1;
            meTXD[61] = MyDevice.myUIT.B0;
            meTXD[62] = MyDevice.myUIT.B1;
            MyDevice.myUIT.F = MyDevice.mCAN[sAddress].E_scaling;
            meTXD[63] = MyDevice.myUIT.B0;
            meTXD[64] = MyDevice.myUIT.B1;
            meTXD[65] = MyDevice.myUIT.B2;
            meTXD[66] = MyDevice.myUIT.B3;
            for (int i = 67; i <= 103; i++)
            {
                meTXD[i] = 0xFF;
            }
            //
            AP_CRC16_MODBUS(meTXD, 104).CopyTo(meTXD, 104);
            //
            meTXD[106] = Constants.nLen;
            meTXD[107] = Constants.STOP;

            txCnt += 108;
            Array.Copy(meTXD, 0, meTXD_SCT, 0, meTXD_SCT.Length);
            SendSegments((uint)Constants.FunID_TSDO + sAddress, meTXD, 0, 108);
        }

        #endregion

        //串口读取SCT0
        private void Protocol_GetSCT0()
        {
            MyDevice.mCAN[sAddress].E_test = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_outype = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_curve = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_adspeed = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_autozero = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_trackzero = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_checkhigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_checklow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_mfg_date = MyDevice.myUIT.UI;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_mfg_srno = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_tmp_min = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_tmp_max = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_tmp_cal = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mCAN[sAddress].E_bohrcode != MyDevice.myUIT.I)
            {
                //序列号
                MyDevice.mySN = MyDevice.mySN + 1;
                //取得code
                MyDevice.mCAN[sAddress].E_bohrcode = MyDevice.myUIT.I;
            }
            MyDevice.mCAN[sAddress].E_enspan = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_protype = meRXD[rxRead++];
            //
            if (MyDevice.mCAN[sAddress].E_test < 0x58)
            {
                if ((MyDevice.mCAN[sAddress].E_protype == 0xFF) || (MyDevice.mCAN[sAddress].E_protype == 0))
                {
                    switch (MyDevice.mCAN[sAddress].E_outype)
                    {
                        case 0xE6:
                            MyDevice.mCAN[sAddress].E_outype = (byte)OUT.UMASK;
                            MyDevice.mCAN[sAddress].E_protype = (byte)TYPE.TD485;
                            break;
                        case 0xE7:
                            MyDevice.mCAN[sAddress].E_outype = (byte)OUT.UMASK;
                            MyDevice.mCAN[sAddress].E_protype = (byte)TYPE.TCAN;
                            break;
                        case 0xF6:
                            MyDevice.mCAN[sAddress].E_outype = (byte)OUT.UMASK;
                            MyDevice.mCAN[sAddress].E_protype = (byte)TYPE.iBus;
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
            MyDevice.mCAN[sAddress].E_ad_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT2
        private void Protocol_GetSCT2()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT3
        private void Protocol_GetSCT3()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_vtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_atio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_btio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ctio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_dtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT4
        private void Protocol_GetSCT4()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero_4ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full_20ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero_N5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full_P5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_zero_N10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_full_P10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT5
        private void Protocol_GetSCT5()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_corr = MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_mark = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_sign = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_addr = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_baud = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_stopbit = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_parity = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_full = MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_wt_decimal = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_unit = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_ascii = meRXD[rxRead++];
            //iBus
            MyDevice.mCAN[sAddress].E_wt_sptime = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_spfilt = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_division = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wt_antivib = meRXD[rxRead++];
            //CANopen
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_heartBeat = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_typeTPDO0 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_evenTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_nodeID = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_nodeBaud = meRXD[rxRead++];
            //iBus
            MyDevice.mCAN[sAddress].E_dynazero = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_cheatype = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_thmax = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_thmin = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_stablerange = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_stabletime = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_tkzerotime = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_tkdynatime = meRXD[rxRead++];
            isEQ = true;
        }

        //串口读取SCT6
        private void Protocol_GetSCT6()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT7
        private void Protocol_GetSCT7()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT8
        private void Protocol_GetSCT8()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ad_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_da_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_input11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_analog11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_etio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_ftio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_gtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_htio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_itio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_jtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT9
        private void Protocol_GetSCT9()
        {
            MyDevice.mCAN[sAddress].E_enGFC = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_enSRDO = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_SCT_time = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_COB_ID1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_COB_ID2 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_enOL = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_overload = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_alarmMode = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wetTarget = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wetLow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_wetHigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_filter = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netServicePort = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_netServiceIP[0] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netServiceIP[1] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netServiceIP[2] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netServiceIP[3] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netClientIP[0] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netClientIP[1] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netClientIP[2] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netClientIP[3] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netGatIP[0] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netGatIP[1] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netGatIP[2] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netGatIP[3] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netMaskIP[0] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netMaskIP[1] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netMaskIP[2] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_netMaskIP[3] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_useDHCP = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_useScan = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_addrRF[0] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_addrRF[1] = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_spedRF = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_chanRF = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_optionRF = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_lockTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_entrTPDO0 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_typeTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_lockTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mCAN[sAddress].E_entrTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mCAN[sAddress].E_evenTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mCAN[sAddress].E_scaling = MyDevice.myUIT.F;
            isEQ = true;
        }

        //串口接收BohrCode
        private unsafe void Protocol_mePort_ReceiveBohrCode()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL:
                        Array.Clear(meRXD, 0, meRXD.Length);
                        rxWrite = 0;
                        rxRead = 0;
                        meBOR.Clear();

                        if (ValidateData_BOR(meData, Constants.Sub_Index_00))
                        {
                            Byte[] sctSegment = new Byte[4];
                            Array.Copy(meData, 4, sctSegment, 0, 4);
                            meBOR[meData[3]] = sctSegment;
                            Protocol_SendBohr2();
                            rxSTP = RXSTP.STX;
                        }
                        break;
                    case RXSTP.STX:
                        if (ValidateData_BOR(meData, Constants.Sub_Index_01))
                        {
                            Byte[] sctSegment = new Byte[3];
                            Array.Copy(meData, 4, sctSegment, 0, 3);
                            meBOR[meData[3]] = sctSegment;

                            foreach (var value in meBOR.Values)
                            {
                                if (rxWrite - 1 + value.Length < meRXD.Length)
                                {
                                    Buffer.BlockCopy(value, 0, meRXD, rxWrite, value.Length);
                                    rxWrite += value.Length;
                                }
                                else
                                {
                                    int rxNumTemp = meRXD.Length - rxWrite;
                                    Buffer.BlockCopy(value, 0, meRXD, rxWrite, meRXD.Length - rxWrite);
                                    rxWrite = 0;
                                    Buffer.BlockCopy(value, rxNumTemp, meRXD, rxWrite, value.Length - rxNumTemp);
                                }
                            }
                            rxSTP = RXSTP.ETX;
                        }
                        break;
                    case RXSTP.ETX://取数据
                        if (ValidateData_BOR(meData, Constants.Sub_Index_00))
                        {
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                            meBOR.Clear();

                            Byte[] sctSegment = new Byte[4];
                            Array.Copy(meData, 4, sctSegment, 0, 4);
                            meBOR[meData[3]] = sctSegment;
                            Protocol_SendBohr2();
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    default:
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //解码
                //颠倒
                MyDevice.myUIT.B0 = meRXD[6];
                MyDevice.myUIT.B1 = meRXD[5];
                MyDevice.myUIT.B2 = meRXD[4];
                MyDevice.myUIT.B3 = meRXD[3];
                MyDevice.mCAN[sAddress].E_bohrcode = MyDevice.myUIT.I;
                //
                MyDevice.mCAN[sAddress].R_bohrcode_long = meRXD[6] +
                                        ((Int64)meRXD[5] << 8) +
                                        ((Int64)meRXD[4] << 16) +
                                        ((Int64)meRXD[3] << 24) +
                                        ((Int64)meRXD[2] << 32) +
                                        ((Int64)meRXD[1] << 40) +
                                        ((Int64)meRXD[0] << 48);

                //
                MyDevice.mCAN[sAddress].sTATE = STATE.CONNECTED;

                //
                isEQ = true;

                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口校验BohrCode
        private unsafe void Protocol_mePort_ReceiveBohrCodeCheck()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL:
                        Array.Clear(meRXD, 0, meRXD.Length);
                        rxWrite = 0;
                        rxRead = 0;
                        meBOR.Clear();

                        if (ValidateData_BOR(meData, Constants.Sub_Index_00))
                        {
                            Byte[] sctSegment = new Byte[4];
                            Array.Copy(meData, 4, sctSegment, 0, 4);
                            meBOR[meData[3]] = sctSegment;
                            Protocol_SendBohr2();
                            rxSTP = RXSTP.STX;
                        }
                        break;
                    case RXSTP.STX:
                        if (ValidateData_BOR(meData, Constants.Sub_Index_01))
                        {
                            Byte[] sctSegment = new Byte[3];
                            Array.Copy(meData, 4, sctSegment, 0, 3);
                            meBOR[meData[3]] = sctSegment;

                            foreach (var value in meBOR.Values)
                            {
                                if (rxWrite - 1 + value.Length < meRXD.Length)
                                {
                                    Buffer.BlockCopy(value, 0, meRXD, rxWrite, value.Length);
                                    rxWrite += value.Length;
                                }
                                else
                                {
                                    int rxNumTemp = meRXD.Length - rxWrite;
                                    Buffer.BlockCopy(value, 0, meRXD, rxWrite, meRXD.Length - rxWrite);
                                    rxWrite = 0;
                                    Buffer.BlockCopy(value, rxNumTemp, meRXD, rxWrite, value.Length - rxNumTemp);
                                }
                            }
                            rxSTP = RXSTP.ETX;
                        }
                        break;
                    case RXSTP.ETX://取数据
                        if (ValidateData_BOR(meData, Constants.Sub_Index_00))
                        {
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                            meBOR.Clear();

                            Byte[] sctSegment = new Byte[4];
                            Array.Copy(meData, 4, sctSegment, 0, 4);
                            meBOR[meData[3]] = sctSegment;
                            Protocol_SendBohr2();
                            rxSTP = RXSTP.ACK;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    default:
                        break;
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //颠倒
                MyDevice.myUIT.B0 = meRXD[6];
                MyDevice.myUIT.B1 = meRXD[5];
                MyDevice.myUIT.B2 = meRXD[4];
                MyDevice.myUIT.B3 = meRXD[3];

                //非模拟量设备第一次更新Bohrcode
                if ((MyDevice.mCAN[sAddress].E_bohrcode == -1) && (MyDevice.mCAN[sAddress].S_OutType == OUT.UMASK))
                {
                    MyDevice.mCAN[sAddress].E_bohrcode = MyDevice.myUIT.I;
                }

                //校验E_bohrcode
                if (MyDevice.mCAN[sAddress].E_bohrcode == MyDevice.myUIT.I)
                {
                    isEQ = true;
                    rxStr = "";
                }
                else
                {
                    isEQ = true;//不再设置BCC校验失败,用户可以继续使用产品,但是要提示返厂模拟量校准
                    rxStr = "error E_bohrcode," + MyDevice.mCAN[sAddress].E_bohrcode.ToString("X2") + " ? " + MyDevice.myUIT.I.ToString("X2");
                }

                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收读取Sector(上传和下载时块的结构不一样)
        //SCT0-SCT9
        private unsafe void Protocol_mePort_ReceiveSectorRead()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL:   //C6成功回复支持检验和，索引0x2300_00，数据2C上传字节数
                        Array.Clear(meRXD, 0, meRXD.Length);
                        rxWrite = 0;
                        rxRead = 0;
                        meSCT.Clear();

                        if (ValidateData_StartRDX(meData)) //校验
                        {
                            rxSTP = RXSTP.STX;
                            Protocol_SendSegmentStart();   //发送 A3
                        }
                        break;
                    case RXSTP.STX:
                        Byte[] sctSegment = new Byte[7];
                        //接收数据
                        switch (meData[0])
                        {
                            case 0x01:
                                if (meData[1] == Constants.STAR)
                                {
                                    sctSegment = new Byte[7];
                                    Array.Copy(meData, 1, sctSegment, 0, 7);
                                    meSCT[meData[0]] = sctSegment;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
                                break;
                            case 0x02:
                            case 0x03:
                            case 0x04:
                            case 0x05:
                            case 0x06:
                            case 0x07:
                            case 0x08:
                            case 0x09:
                            case 0x0A:
                            case 0x0B:
                            case 0x0C:
                            case 0x0D:
                            case 0x0E:
                            case 0x0F:
                                Array.Copy(meData, 1, sctSegment, 0, 7);
                                meSCT[meData[0]] = sctSegment;
                                break;
                            case 0x87:
                                if ((meData[1] == Constants.tLen) && (meData[2] == Constants.STOP))
                                {
                                    sctSegment = new Byte[2];
                                    Array.Copy(meData, 1, sctSegment, 0, 2);
                                    meSCT[meData[0]] = sctSegment;

                                    foreach (var value in meSCT.Values)
                                    {
                                        if (rxWrite - 1 + value.Length < meRXD.Length)
                                        {
                                            Buffer.BlockCopy(value, 0, meRXD, rxWrite, value.Length);
                                            rxWrite += value.Length;
                                        }
                                        else
                                        {
                                            int rxNumTemp = meRXD.Length - rxWrite;
                                            Buffer.BlockCopy(value, 0, meRXD, rxWrite, meRXD.Length - rxWrite);
                                            rxWrite = 0;
                                            Buffer.BlockCopy(value, rxNumTemp, meRXD, rxWrite, value.Length - rxNumTemp);
                                        }
                                    }

                                    Protocol_SendSegmentUpdateEnd(0x07, 0x07);  //发送 A2
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }

                                break;
                            case 0x90:
                                if ((meData[2] == Constants.nLen) && (meData[3] == Constants.STOP))
                                {
                                    sctSegment = new Byte[3];
                                    Array.Copy(meData, 1, sctSegment, 0, 3);
                                    meSCT[meData[0]] = sctSegment;

                                    foreach (var value in meSCT.Values)
                                    {
                                        if (rxWrite - 1 + value.Length < meRXD.Length)
                                        {
                                            Buffer.BlockCopy(value, 0, meRXD, rxWrite, value.Length);
                                            rxWrite += value.Length;
                                        }
                                        else
                                        {
                                            int rxNumTemp = meRXD.Length - rxWrite;
                                            Buffer.BlockCopy(value, 0, meRXD, rxWrite, meRXD.Length - rxWrite);
                                            rxWrite = 0;
                                            Buffer.BlockCopy(value, rxNumTemp, meRXD, rxWrite, value.Length - rxNumTemp);
                                        }
                                    }

                                    Protocol_SendSegmentUpdateEnd(0x10, 0x10);  //发送 A2
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }

                                break;

                            default:
                                rxSTP = RXSTP.NUL;
                                break;
                        }
                        break;
                    case RXSTP.ACK:
                        if (ValidateData_EndRDX(meData))    //校验
                        {
                            ushort XLXH = (ushort)((meData[1] << 8) | meData[2]);
                            if (XLXH == AP_CRC16_XMODEN(meRXD, 44, false) || XLXH == AP_CRC16_XMODEN(meRXD, 108, false))
                            {
                                rxSTP = RXSTP.ETX;
                                Protocol_SendBTEnd();       //发送 A1
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX:
                        if (ValidateData_StartRDX(meData))
                        {
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                            meSCT.Clear();

                            rxSTP = RXSTP.STX;
                            Protocol_SendSegmentStart();    //发送 A3
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
                //RDX和WRX的帧头字节数不一样
                rxRead = 2;

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
                        switch (meRXD[1])
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

                    default:
                        return;
                }

                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收写入Sector(上传和下载时块的结构不一样)
        //SCT0-SCT9
        private unsafe void Protocol_mePort_ReceiveSectorWrite()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                //帧解析
                switch (rxSTP)
                {
                    case RXSTP.NUL:   //C6成功回复支持检验和，索引0x2300_00，数据2C上传字节数
                        Array.Clear(meRXD, 0, meRXD.Length);
                        rxWrite = 0;
                        rxRead = 0;
                        meSCT.Clear();

                        if (ValidateData_StartWRX(meData))     //校验 A4
                        {
                            rxSTP = RXSTP.STX;
                            //发SCT数据段
                            switch (meData[3])
                            {
                                case Constants.Sub_Index_00: Protocol_SendSCT0_Segment(); break; //WRX0
                                case Constants.Sub_Index_01: Protocol_SendSCT1_Segment(); break; //WRX1
                                case Constants.Sub_Index_02: Protocol_SendSCT2_Segment(); break; //WRX2
                                case Constants.Sub_Index_03: Protocol_SendSCT3_Segment(); break; //WRX3
                                case Constants.Sub_Index_04: Protocol_SendSCT4_Segment(); break; //WRX4
                                case Constants.Sub_Index_05: Protocol_SendSCT5_Segment(); break; //WRX5
                                case Constants.Sub_Index_06: Protocol_SendSCT6_Segment(); break; //WRX6
                                case Constants.Sub_Index_07: Protocol_SendSCT7_Segment(); break; //WRX7
                                case Constants.Sub_Index_08: Protocol_SendSCT8_Segment(); break; //WRX8
                                case Constants.Sub_Index_09: Protocol_SendSCT9_Segment(); break; //WRX9
                                default: break;
                            }
                        }
                        break;
                    case RXSTP.STX:
                        if (ValidateData_EndWRX_Segment(meData))  //校验 A2
                        {
                            rxSTP = RXSTP.ACK;
                            Protocol_SendSegmentDownloadEnd();
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ACK:
                        if (ValidateData_EndWRX(meData))         //校验 A1
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                    case RXSTP.ETX:
                        if (ValidateData_StartRDX(meData))
                        {
                            Array.Clear(meRXD, 0, meRXD.Length);
                            rxWrite = 0;
                            rxRead = 0;
                            meSCT.Clear();

                            rxSTP = RXSTP.STX;
                            //发SCT数据段
                            switch (meData[3])
                            {
                                case Constants.Sub_Index_00: Protocol_SendSCT0_Segment(); break; //WRX0
                                case Constants.Sub_Index_01: Protocol_SendSCT1_Segment(); break; //WRX1
                                case Constants.Sub_Index_02: Protocol_SendSCT2_Segment(); break; //WRX2
                                case Constants.Sub_Index_03: Protocol_SendSCT3_Segment(); break; //WRX3
                                case Constants.Sub_Index_04: Protocol_SendSCT4_Segment(); break; //WRX4
                                case Constants.Sub_Index_05: Protocol_SendSCT5_Segment(); break; //WRX5
                                case Constants.Sub_Index_06: Protocol_SendSCT6_Segment(); break; //WRX6
                                case Constants.Sub_Index_07: Protocol_SendSCT7_Segment(); break; //WRX7
                                case Constants.Sub_Index_08: Protocol_SendSCT8_Segment(); break; //WRX8
                                case Constants.Sub_Index_09: Protocol_SendSCT9_Segment(); break; //WRX9
                                default: break;
                            }
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
                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收采样的滤波内码
        //01 03 04 00 03 82 89 AB 35
        private unsafe void Protocol_mePort_ReceiveAdpoint()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_ADC(meData))
                {
                    //取值
                    MyDevice.myUIT.B3 = meData[7]; //MSB
                    MyDevice.myUIT.B2 = meData[6];
                    MyDevice.myUIT.B1 = meData[5];
                    MyDevice.myUIT.B0 = meData[4]; //LSB
                    rxDat = MyDevice.myUIT.I;

                    //mV/V
                    double mvdv = (double)rxDat / MyDevice.mCAN[sAddress].S_MVDV;

                    //
                    switch (trTSK)
                    {
                        case TASKS.ADCP1:
                            MyDevice.mCAN[sAddress].E_input1 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point1 = rxDat;
                            MyDevice.mCAN[sAddress].E_ad_zero = rxDat;
                            break;
                        case TASKS.ADCP2:
                            MyDevice.mCAN[sAddress].E_input2 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point2 = rxDat;
                            break;
                        case TASKS.ADCP3:
                            MyDevice.mCAN[sAddress].E_input3 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point3 = rxDat;
                            break;
                        case TASKS.ADCP4:
                            MyDevice.mCAN[sAddress].E_input4 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point4 = rxDat;
                            break;
                        case TASKS.ADCP5:
                            MyDevice.mCAN[sAddress].E_input5 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point5 = rxDat;
                            if (!MyDevice.mCAN[sAddress].S_ElevenType) MyDevice.mCAN[sAddress].E_ad_full = rxDat;
                            break;
                        case TASKS.ADCP6:
                            MyDevice.mCAN[sAddress].E_input6 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point6 = rxDat;
                            break;
                        case TASKS.ADCP7:
                            MyDevice.mCAN[sAddress].E_input7 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point7 = rxDat;
                            break;
                        case TASKS.ADCP8:
                            MyDevice.mCAN[sAddress].E_input8 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point8 = rxDat;
                            break;
                        case TASKS.ADCP9:
                            MyDevice.mCAN[sAddress].E_input9 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point9 = rxDat;
                            break;
                        case TASKS.ADCP10:
                            MyDevice.mCAN[sAddress].E_input10 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point10 = rxDat;
                            break;
                        case TASKS.ADCP11:
                            MyDevice.mCAN[sAddress].E_input11 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                            MyDevice.mCAN[sAddress].E_ad_point11 = rxDat;
                            if (MyDevice.mCAN[sAddress].S_ElevenType) MyDevice.mCAN[sAddress].E_ad_full = rxDat;
                            break;
                    }

                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        //串口接收读取Long
        //=±123\r\n
        //=F123\r\n
        private unsafe void Protocol_mePort_ReceiveLong()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_AN1(meData))
                {
                    MyDevice.mCAN[sAddress].R_isFLT = false;

                    //取值
                    MyDevice.myUIT.B3 = meData[7]; //MSB
                    MyDevice.myUIT.B2 = meData[6];
                    MyDevice.myUIT.B1 = meData[5];
                    MyDevice.myUIT.B0 = meData[4]; //LSB
                    MyDevice.mCAN[sAddress].R_adcout = MyDevice.myUIT.I;

                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        //串口接收读取dacout
        //02 80 80 80
        //02 80 80 80 80
        private unsafe void Protocol_mePort_ReceiveDacout()
        {
            //
            Byte[] meData;
            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_WEIGHT(meData))
                {
                    MyDevice.mCAN[sAddress].R_isFLT = false;

                    //取值
                    MyDevice.myUIT.B3 = meData[7]; //MSB
                    MyDevice.myUIT.B2 = meData[6];
                    MyDevice.myUIT.B1 = meData[5];
                    MyDevice.myUIT.B0 = meData[4]; //LSB
                    rxDat = MyDevice.myUIT.I;

                    //
                    MyDevice.mCAN[sAddress].RefreshDacout(rxDat);

                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        //串口接收读取ZERO
        //=±123\r\n
        //=F123\r\n
        private unsafe void Protocol_mePort_ReceiveZERO()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_ZERO(meData))
                {
                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        /// <summary>
        /// 串口接收Reset
        /// </summary>
        private unsafe void Protocol_mePort_ReceiveReset()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_REST(meData))
                {
                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        //串口接收读取网络管理回复
        //=±123\r\n
        //=F123\r\n
        private unsafe void Protocol_mePort_ReceiveNMTERR()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_NMTERR + sAddress) break;  //判断ID
                if (meFrame.DataLen != 1) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                switch (meData[0])
                {
                    case Constants.NMTERR_pre_run:
                        MyDevice.mCAN[sAddress].R_nmterr = Constants.NMT_CS_PRERUN;
                        break;
                    case Constants.NMTERR_run:
                        MyDevice.mCAN[sAddress].R_nmterr = Constants.NMT_CS_Run;
                        break;
                    case Constants.NMTERR_stop:
                        MyDevice.mCAN[sAddress].R_nmterr = Constants.NMT_CS_Stop;
                        break;
                    case Constants.NMTERR_start:
                    default:
                        MyDevice.mCAN[sAddress].R_nmterr = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// 串口接收心跳修改回复
        /// </summary>
        private unsafe void Protocol_mePort_ReceiveWHEART()
        {
            //
            Byte[] meData;

            //读取帧
            while (mePort.FramesToRead > 0)
            {
                //取帧
                VCI_CAN_OBJ meFrame = mePort.ReadFrame();

                if (meFrame.ID != (uint)Constants.FunID_RSDO + sAddress) break;  //判断ID
                if (meFrame.DataLen != 8) break; //判断长度

                //取字节
                meData = new byte[meFrame.DataLen];
                for (int i = 0; i < meFrame.DataLen; i++)
                {
                    meData[i] = meFrame.Data[i];
                }

                //字节统计
                rxCnt += meFrame.DataLen;

                if (ValidateData_WHEART(meData))
                {
                    //委托
                    MyDevice.callDelegate();
                }
            }
        }

        //接收触发函数,实际会由串口线程创建
        private void Protocol_mePort_DataReceived()
        {
            if (is_serial_closing)
            {
                is_serial_listening = false;//准备关闭串口时，reset串口侦听标记
                return;
            }
            try
            {
                if (mePort.FramesToRead > 0)
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
                        case TASKS.RDX9:
                            Protocol_mePort_ReceiveSectorRead();
                            break;

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
                            Protocol_mePort_ReceiveSectorWrite();
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

                        case TASKS.ZERO:
                            Protocol_mePort_ReceiveZERO();
                            break;

                        case TASKS.ADC:
                            Protocol_mePort_ReceiveLong();
                            break;

                        case TASKS.DAC:
                            Protocol_mePort_ReceiveDacout();
                            break;

                        case TASKS.REST:
                            Protocol_mePort_ReceiveReset();
                            break;

                        case TASKS.WNMT:
                            Protocol_mePort_ReceiveNMTERR();
                            break;

                        case TASKS.WHEART:
                            Protocol_mePort_ReceiveWHEART();
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

        //串口读取任务状态机 BOR -> RDX0 -> RDX1 -> RDX2 -> RDX3 -> RDX4 -> RDX5 -> RDX6 -> RDX7 -> RDX8 -> RDX9
        public void Protocol_mePort_ReadTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧发送
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_SendCOM(TASKS.BOR);//开始先读Bohrcode
                    break;

                case TASKS.BOR:
                    if (isEQ)
                    {
                        Protocol_SendCOM(TASKS.RDX0);
                    }
                    else
                    {
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
                        switch (MyDevice.mCAN[sAddress].S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                                MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mCAN[sAddress].E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendCOM(TASKS.RDX5);
                                }
                                else
                                {
                                    MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                                    MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if ((MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iBus) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TD485) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TP10) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendCOM(TASKS.RDX6);
                        }
                        else if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mCAN[sAddress].E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendCOM(TASKS.RDX6);
                            }
                            else
                            {
                                MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                                MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                            }
                        }
                        else
                        {
                            MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                            MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN || MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet || MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iStar)//有sct9
                        {
                            Protocol_SendCOM(TASKS.RDX9);
                        }
                        else if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iBus)//iBUS读SCT1-8后读MEM.filt_range 采样滤波范围
                        {
                            Protocol_SendCOM(TASKS.RDFT);
                        }
                        else
                        {
                            MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                            MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                        MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX9);
                    }
                    break;

                case TASKS.RDFT:
                    if (isEQ)
                    {
                        MyDevice.mCAN[sAddress].sTATE = STATE.WORKING;
                        MyDevice.mCAN[sAddress].R_checklink = MyDevice.mCAN[sAddress].R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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

        }

        //串口写入任务状态机 BCC -> WRX5 -> (WRX9)-> (rst)
        //参数AI跟踪器修改用
        public void Protocol_mePort_WriteParTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
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
                        if ((MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN && MyDevice.mCAN[sAddress].E_test > 0x58) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet))  //新版本TCAN才有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                    }
                    else
                    {
                        Protocol_SendSCT9();
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX5 -> WRX9 -> RST
        //CANopen界面串口参数修改用
        public void Protocol_mePort_WriteBusTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
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
                        if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN && MyDevice.mCAN[sAddress].E_test > 0x58)  //新版本TCAN才有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                            }
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
                        Protocol_SendCOM(TASKS.REST);
                        if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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

        }

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX6 -> WRX7 -> WRX8 -> (rst)
        //标定传感器和修正传感器用
        public void Protocol_mePort_WriteCalTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
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
                        if ((MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iBus) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TD485) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TP10) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendSCT6();
                        }
                        else
                        {
                            if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                    }
                    else
                    {
                        Protocol_SendSCT8();
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX0 -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> WRX5 -> WRX6 -> WRX7 -> WRX8 -> WRX9 ->RST
        //标定参数修改用
        public void Protocol_mePort_WriteAllTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
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
                        switch (MyDevice.mCAN[sAddress].S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                                }
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mCAN[sAddress].E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendSCT5();
                                }
                                else
                                {
                                    Protocol_SendCOM(TASKS.REST);
                                    if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                    {
                                        trTSK = TASKS.NULL;
                                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if ((MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iBus) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TD485) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TP10) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet) || (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iStar))//有SCT6-SCT9
                        {
                            Protocol_SendSCT6();
                        }
                        else if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mCAN[sAddress].E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendSCT6();
                            }
                            else
                            {
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                                }
                            }
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if (MyDevice.mCAN[sAddress].S_DeviceType == TYPE.TCAN || MyDevice.mCAN[sAddress].S_DeviceType == TYPE.iNet)  //有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        if (MyDevice.mCAN[sAddress].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
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
                        MyDevice.SaveToLog(MyDevice.mCAN[sAddress]);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }

        //将数据封装为一帧发送
        private unsafe bool SendByteBase(uint cmdId, byte[] data, int offset, int count)
        {
            lock (_lock_rtcmd)
            {
                if (count <= 8)
                {
                    var frame = new VCI_CAN_OBJ();
                    frame.ID = cmdId;
                    frame.DataLen = (byte)count;
                    for (int i = 0; i < count; i++)
                    {
                        frame.Data[i] = data[i + offset];
                    }
                    VCI_CAN_OBJ[] frames = { frame };

                    mePort.Write(frames);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //将数据转换为段发送
        private unsafe bool SendSegments(uint cmdId, byte[] data, int offset, int count)
        {
            int splitCount = count / 7 + 1;   // 计算需要拆分后的段数
            byte[] dataToSend = new byte[8];  // 存储本次发送的段

            for (int i = 0; i < splitCount; i++)
            {
                if (i == splitCount - 1)   // 存储序号
                {
                    if (splitCount == 7)
                    {
                        dataToSend[0] = 0x87;
                    }
                    else if (splitCount == 16)
                    {
                        dataToSend[0] = 0x90;
                    }
                }
                else
                {
                    dataToSend[0] = (byte)(i + 1);
                }

                // 拆分并存储元素
                for (int j = 0; j < 7; j++)
                {
                    if (i * 7 + j < count)
                    {
                        dataToSend[j + 1] = data[i * 7 + j + offset];
                    }
                    else
                    {
                        dataToSend[j + 1] = 0;  // 如果原数组元素不足 7 个，则用 0 填充
                    }
                }

                //发送段
                SendByteBase(cmdId, dataToSend, 0, 8);
            }
            return true;
        }
    }
}
