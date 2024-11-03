using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

//Lumi 20240119

namespace Model
{
    public class ModbusTCPProtocol : IProtocol
    {
        #region 定义变量

        //
        private Byte sAddress = 1;  //设备E_addr
        private Byte nAddress = 1;  //修改ID之后的新地址

        private string clientIpAddr = "";   //客户端ip
        private Byte clientIndex = 0;       //mMTCP的下标

        //
        private volatile bool is_serial_listening = false;  //串口正在监听标记
        private volatile bool is_serial_closing = false;    //串口正在关闭标记

        //
        public TCPServer mePort = new TCPServer();      //Modbus TCP使用的网口
        private TASKS trTSK = TASKS.NULL;               //接口读写任务状态机
        private RXSTP rxSTP = RXSTP.NUL;                //通讯接收状态机,找出帧头帧尾
        private Int32 txCnt = 0;                        //发送计数
        private Int32 rxCnt = 0;                        //接收计数
        private Int32 rxDat = 0;                        //接收数据数值
        private String rxStr = null;                    //接收数据字符串
        private Boolean isEQ = false;                   //接收检查

        //
        private Byte[] meTXD = new Byte[rxSize];        //发送缓冲区
        private Byte[] meRXD = new Byte[rxSize];        //接收缓冲区
        private Int32 rxRead = 0;                       //接收缓冲区读指针
        private Int32 rxWrite = 0;                      //接收缓冲区写指针
        private const Int16 rxSize = 1024;              //缓冲区大小

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
                return COMP.ModbusTCP;
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
                return mePort.ServerSocket?.LocalEndPoint?.ToString();
            }
        }
        public Int32 baudRate
        {
            get
            {
                return 0;
            }
        }
        public StopBits stopBits
        {
            get
            {
                return 0;
            }
        }
        public Parity parity
        {
            get
            {
                return 0;
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
                clientIpAddr = value;
            }
            get
            {
                return clientIpAddr;
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
        public ModbusTCPProtocol()
        {
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


        //打开socket客户端
        public void Protocol_PortOpen(string ip, Int32 port = 5678)
        {
            if (mePort.IsOpen)
            {
                return;
            }

            //尝试打开串口
            try
            {
                //初始化串口监听标记
                is_serial_listening = false;
                is_serial_closing = false;

                //
                mePort.Listen(ip, port);
                mePort.DataReceived += Protocol_mePort_DataReceived;//接收处理

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

        //不是CAN
        public void Protocol_PortOpen(UInt32 index, String name, Int32 baud)
        {

        }

        //不是Selfuart，RS485
        public void Protocol_PortOpen(String name, Int32 baud, StopBits stb, Parity pay)
        {
        }

        //关闭串口
        public bool Protocol_PortClose()
        {
            if (mePort.IsOpen)
            {
                try
                {
                    mePort.DataReceived -= Protocol_mePort_DataReceived;//接收处理
                    mePort.DiscardBuffer();
                    mePort.Stop();
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
                mePort.DiscardBuffer();
            }

            for (int i = 0; i < rxSize; i++)
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

        //发送读命令
        public void Protocol_SendCOM(TASKS meTask)
        {
            //
            if (!mePort.IsOpen) return;

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
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x91, 0x00, 0x08 }).CopyTo(meTXD, 0);
                    break;

                //读SCT
                case TASKS.RDX0:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x94, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX1:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x95, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX2:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x96, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX3:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x97, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX4:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0x98, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX5:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xA0, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX6:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xA5, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX7:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xA6, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX8:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xA7, 0x00, 0x16 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDX9:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xAA, 0x00, 0x40 }).CopyTo(meTXD, 0);
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
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x22, 0x00, 0x02 }).CopyTo(meTXD, 0);
                    break;

                //校准
                case TASKS.GODMZ:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x40, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GOUPZ:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x41, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GODMF:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x42, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.GOUPF:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x43, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.DONE:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x44, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;

                //扣重,归零,标定,重启
                case TASKS.TARE:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0x00, 0x30, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.ZERO:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0x00, 0x31, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.SPAN:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0x00, 0x32, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.REST:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0x00, 0x33, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //adcout,dacout, dacset和Net和Gross
                case TASKS.ADC:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA1, 0x9A, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.DAC:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA1, 0x9C, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.QDAC:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA1, 0x9D, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.QNET:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA1, 0x9E, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.QGROSS:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA1, 0x9F, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;

                //滤波参数
                case TASKS.SFLT:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0xA2, 0x71, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RFLT:
                    (new Byte[] { sAddress, Constants.RS485_WRITE, 0xA2, 0x72, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.RDFT:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA2, 0x73, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                //读eeprom参数
                case TASKS.REPRM:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0xA0, 0xA9, 0x00, 0x76 }).CopyTo(meTXD, 0);
                    break;

                //扫描地址
                case TASKS.SCAN:
                    (new Byte[] { sAddress, Constants.RS485_READ, 0x00, 0x00, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;

                //广播指令,全部归零
                case TASKS.AZERO:
                    (new Byte[] { 0x00, Constants.RS485_WRITE, 0x00, 0x31, 0x00, 0x00 }).CopyTo(meTXD, 0);
                    break;

                default:
                    return;
            }

            //
            AP_CRC16_MODBUS(meTXD, 6).CopyTo(meTXD, 6);
            SendByteBase(meTXD, 0, 8);
            txCnt += 8;
            trTSK = meTask;
            isEQ = false;
        }

        //扫描地址
        public void Protocol_SendAddr(Byte addr)
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //装帧
            (new Byte[] { addr, Constants.RS485_READ, 0x00, 0x00, 0x00, 0x01 }).CopyTo(meTXD, 0);

            //
            AP_CRC16_MODBUS(meTXD, 6).CopyTo(meTXD, 6);
            SendByteBase(meTXD, 0, 8);
            txCnt += 8;
            trTSK = TASKS.SCAN;
        }

        //广播指令,有重量设置地址 不是RS485
        public void Protocol_HwSetAddr(Byte addr)
        {
        }

        //广播指令,检索重量设置地址 不是RS485
        public void Protocol_SwSetAddr(Byte addr, UInt32 weight)
        {
        }

        //不是CANopen
        public void Protocol_SendNMT(Byte NMT_CS)
        {

        }

        //不是CANopen
        public uint Protocol_GetHeartBeatID()
        {
            return 0;
        }

        //设置心跳时间间隔
        public void Protocol_SendHeartBeat(UInt16 period)
        {

        }

        #region SCT0-9 发送接收和校验

        //发送写入SCT0命令
        private void Protocol_SendSCT0()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX0;
            meTXD[2] = MyDevice.mMTCP[clientIndex].E_test;
            meTXD[3] = MyDevice.mMTCP[clientIndex].E_outype;
            meTXD[4] = MyDevice.mMTCP[clientIndex].E_curve;
            meTXD[5] = MyDevice.mMTCP[clientIndex].E_adspeed;
            meTXD[6] = MyDevice.mMTCP[clientIndex].E_autozero;
            meTXD[7] = MyDevice.mMTCP[clientIndex].E_trackzero;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_checkhigh;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.myUIT.B2;
            meTXD[11] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_checklow;
            meTXD[12] = MyDevice.myUIT.B0;
            meTXD[13] = MyDevice.myUIT.B1;
            meTXD[14] = MyDevice.myUIT.B2;
            meTXD[15] = MyDevice.myUIT.B3;
            MyDevice.myUIT.UI = MyDevice.mMTCP[clientIndex].E_mfg_date;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_mfg_srno;
            meTXD[20] = MyDevice.myUIT.B0;
            meTXD[21] = MyDevice.myUIT.B1;
            meTXD[22] = MyDevice.myUIT.B2;
            meTXD[23] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_tmp_min;
            meTXD[24] = MyDevice.myUIT.B0;
            meTXD[25] = MyDevice.myUIT.B1;
            meTXD[26] = MyDevice.myUIT.B2;
            meTXD[27] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_tmp_max;
            meTXD[28] = MyDevice.myUIT.B0;
            meTXD[29] = MyDevice.myUIT.B1;
            meTXD[30] = MyDevice.myUIT.B2;
            meTXD[31] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_tmp_cal;
            meTXD[32] = MyDevice.myUIT.B0;
            meTXD[33] = MyDevice.myUIT.B1;
            meTXD[34] = MyDevice.myUIT.B2;
            meTXD[35] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_bohrcode;
            meTXD[36] = MyDevice.myUIT.B0;
            meTXD[37] = MyDevice.myUIT.B1;
            meTXD[38] = MyDevice.myUIT.B2;
            meTXD[39] = MyDevice.myUIT.B3;
            meTXD[40] = MyDevice.mMTCP[clientIndex].E_enspan;
            meTXD[41] = MyDevice.mMTCP[clientIndex].E_protype;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT0 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT0 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX0;
        }

        //发送写入SCT1命令
        private void Protocol_SendSCT1()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX1;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT1 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT1 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX1;
        }

        //发送写入SCT2命令
        private void Protocol_SendSCT2()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX2;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input1;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input2;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input3;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input4;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input5;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog1;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog2;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog3;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog4;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog5;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT2 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT2 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX2;
        }

        //发送写入SCT3命令
        private void Protocol_SendSCT3()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_zero;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_full;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_vtio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_wtio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_atio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_btio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ctio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_dtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT3 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT3 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX3;
        }

        //发送写入SCT4命令
        private void Protocol_SendSCT4()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX4;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero_4ma;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full_20ma;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero_05V;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full_05V;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero_10V;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full_10V;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero_N5;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full_P5;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_zero_N10;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_full_P10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT4 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT4 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX4;
        }

        //发送写入SCT5命令
        private void Protocol_SendSCT5()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //写SCT5时记录修改后的新ID
            nAddress = MyDevice.mMTCP[clientIndex].E_addr;

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX5;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_corr;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            meTXD[6] = MyDevice.mMTCP[clientIndex].E_mark;
            meTXD[7] = MyDevice.mMTCP[clientIndex].E_sign;
            meTXD[8] = MyDevice.mMTCP[clientIndex].E_addr;
            meTXD[9] = MyDevice.mMTCP[clientIndex].E_baud;
            meTXD[10] = MyDevice.mMTCP[clientIndex].E_stopbit;
            meTXD[11] = MyDevice.mMTCP[clientIndex].E_parity;
            MyDevice.mMTCP[clientIndex].E_wt_zero = 0;//强制写0
            meTXD[12] = 0;
            meTXD[13] = 0;
            meTXD[14] = 0;
            meTXD[15] = 0;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_wt_full;
            meTXD[16] = MyDevice.myUIT.B0;
            meTXD[17] = MyDevice.myUIT.B1;
            meTXD[18] = MyDevice.myUIT.B2;
            meTXD[19] = MyDevice.myUIT.B3;
            meTXD[20] = MyDevice.mMTCP[clientIndex].E_wt_decimal;
            meTXD[21] = MyDevice.mMTCP[clientIndex].E_wt_unit;
            meTXD[22] = MyDevice.mMTCP[clientIndex].E_wt_ascii;
            meTXD[23] = MyDevice.mMTCP[clientIndex].E_wt_sptime;
            meTXD[24] = MyDevice.mMTCP[clientIndex].E_wt_spfilt;
            meTXD[25] = MyDevice.mMTCP[clientIndex].E_wt_division;
            meTXD[26] = MyDevice.mMTCP[clientIndex].E_wt_antivib;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_heartBeat;
            meTXD[27] = MyDevice.myUIT.B0;
            meTXD[28] = MyDevice.myUIT.B1;
            meTXD[29] = MyDevice.mMTCP[clientIndex].E_typeTPDO0;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_evenTPDO0;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.mMTCP[clientIndex].E_nodeID;
            meTXD[33] = MyDevice.mMTCP[clientIndex].E_nodeBaud;
            meTXD[34] = MyDevice.mMTCP[clientIndex].E_dynazero;
            meTXD[35] = MyDevice.mMTCP[clientIndex].E_cheatype;
            meTXD[36] = MyDevice.mMTCP[clientIndex].E_thmax;
            meTXD[37] = MyDevice.mMTCP[clientIndex].E_thmin;
            meTXD[38] = MyDevice.mMTCP[clientIndex].E_stablerange;
            meTXD[39] = MyDevice.mMTCP[clientIndex].E_stabletime;
            meTXD[40] = MyDevice.mMTCP[clientIndex].E_tkzerotime;
            meTXD[41] = MyDevice.mMTCP[clientIndex].E_tkdynatime;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT5 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT5 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX5;
        }

        //发送写入SCT6命令
        private void Protocol_SendSCT6()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX6;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT6 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT6 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX6;
        }

        //发送写入SCT7命令
        private void Protocol_SendSCT7()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX7;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input6;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input7;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input8;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input9;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input10;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog6;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog7;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog8;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog9;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog10;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT7 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT7 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX7;
        }

        //发送写入SCT8命令
        private void Protocol_SendSCT8()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX8;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ad_point11;
            meTXD[2] = MyDevice.myUIT.B0;
            meTXD[3] = MyDevice.myUIT.B1;
            meTXD[4] = MyDevice.myUIT.B2;
            meTXD[5] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_da_point11;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_input11;
            meTXD[10] = MyDevice.myUIT.B0;
            meTXD[11] = MyDevice.myUIT.B1;
            meTXD[12] = MyDevice.myUIT.B2;
            meTXD[13] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_analog11;
            meTXD[14] = MyDevice.myUIT.B0;
            meTXD[15] = MyDevice.myUIT.B1;
            meTXD[16] = MyDevice.myUIT.B2;
            meTXD[17] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_etio;
            meTXD[18] = MyDevice.myUIT.B0;
            meTXD[19] = MyDevice.myUIT.B1;
            meTXD[20] = MyDevice.myUIT.B2;
            meTXD[21] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_ftio;
            meTXD[22] = MyDevice.myUIT.B0;
            meTXD[23] = MyDevice.myUIT.B1;
            meTXD[24] = MyDevice.myUIT.B2;
            meTXD[25] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_gtio;
            meTXD[26] = MyDevice.myUIT.B0;
            meTXD[27] = MyDevice.myUIT.B1;
            meTXD[28] = MyDevice.myUIT.B2;
            meTXD[29] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_htio;
            meTXD[30] = MyDevice.myUIT.B0;
            meTXD[31] = MyDevice.myUIT.B1;
            meTXD[32] = MyDevice.myUIT.B2;
            meTXD[33] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_itio;
            meTXD[34] = MyDevice.myUIT.B0;
            meTXD[35] = MyDevice.myUIT.B1;
            meTXD[36] = MyDevice.myUIT.B2;
            meTXD[37] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_jtio;
            meTXD[38] = MyDevice.myUIT.B0;
            meTXD[39] = MyDevice.myUIT.B1;
            meTXD[40] = MyDevice.myUIT.B2;
            meTXD[41] = MyDevice.myUIT.B3;

            //
            meTXD[42] = Constants.tLen;
            meTXD[43] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 43; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT8 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT8 >> 8;
            AP_CRC16_MODBUS(meTXD, 48).CopyTo(meTXD, 48);
            SendByteBase(meTXD, 0, 50);
            txCnt += 50;
            trTSK = TASKS.WRX8;
        }

        //发送写入SCT9命令
        private void Protocol_SendSCT9()
        {
            //
            if (!mePort.IsOpen) return;

            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = Constants.STAR;
            meTXD[1] = Constants.WRX9;
            meTXD[2] = MyDevice.mMTCP[clientIndex].E_enGFC;
            meTXD[3] = MyDevice.mMTCP[clientIndex].E_enSRDO;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_SCT_time;
            meTXD[4] = MyDevice.myUIT.B0;
            meTXD[5] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_COB_ID1;
            meTXD[6] = MyDevice.myUIT.B0;
            meTXD[7] = MyDevice.myUIT.B1;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_COB_ID2;
            meTXD[8] = MyDevice.myUIT.B0;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.mMTCP[clientIndex].E_enOL;
            meTXD[11] = MyDevice.mMTCP[clientIndex].E_overload;
            meTXD[12] = MyDevice.mMTCP[clientIndex].E_alarmMode;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_wetTarget;
            meTXD[13] = MyDevice.myUIT.B0;
            meTXD[14] = MyDevice.myUIT.B1;
            meTXD[15] = MyDevice.myUIT.B2;
            meTXD[16] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_wetLow;
            meTXD[17] = MyDevice.myUIT.B0;
            meTXD[18] = MyDevice.myUIT.B1;
            meTXD[19] = MyDevice.myUIT.B2;
            meTXD[20] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_wetHigh;
            meTXD[21] = MyDevice.myUIT.B0;
            meTXD[22] = MyDevice.myUIT.B1;
            meTXD[23] = MyDevice.myUIT.B2;
            meTXD[24] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_filter;
            meTXD[25] = MyDevice.myUIT.B0;
            meTXD[26] = MyDevice.myUIT.B1;
            meTXD[27] = MyDevice.myUIT.B2;
            meTXD[28] = MyDevice.myUIT.B3;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_netServicePort;
            meTXD[29] = MyDevice.myUIT.B0;
            meTXD[30] = MyDevice.myUIT.B1;
            meTXD[31] = MyDevice.mMTCP[clientIndex].E_netServiceIP[0];
            meTXD[32] = MyDevice.mMTCP[clientIndex].E_netServiceIP[1];
            meTXD[33] = MyDevice.mMTCP[clientIndex].E_netServiceIP[2];
            meTXD[34] = MyDevice.mMTCP[clientIndex].E_netServiceIP[3];
            meTXD[35] = MyDevice.mMTCP[clientIndex].E_netClientIP[0];
            meTXD[36] = MyDevice.mMTCP[clientIndex].E_netClientIP[1];
            meTXD[37] = MyDevice.mMTCP[clientIndex].E_netClientIP[2];
            meTXD[38] = MyDevice.mMTCP[clientIndex].E_netClientIP[3];
            meTXD[39] = MyDevice.mMTCP[clientIndex].E_netGatIP[0];
            meTXD[40] = MyDevice.mMTCP[clientIndex].E_netGatIP[1];
            meTXD[41] = MyDevice.mMTCP[clientIndex].E_netGatIP[2];
            meTXD[42] = MyDevice.mMTCP[clientIndex].E_netGatIP[3];
            meTXD[43] = MyDevice.mMTCP[clientIndex].E_netMaskIP[0];
            meTXD[44] = MyDevice.mMTCP[clientIndex].E_netMaskIP[1];
            meTXD[45] = MyDevice.mMTCP[clientIndex].E_netMaskIP[2];
            meTXD[46] = MyDevice.mMTCP[clientIndex].E_netMaskIP[3];
            meTXD[47] = MyDevice.mMTCP[clientIndex].E_useDHCP;
            meTXD[48] = MyDevice.mMTCP[clientIndex].E_useScan;
            meTXD[49] = MyDevice.mMTCP[clientIndex].E_addrRF[0];
            meTXD[50] = MyDevice.mMTCP[clientIndex].E_addrRF[1];
            meTXD[51] = MyDevice.mMTCP[clientIndex].E_spedRF;
            meTXD[52] = MyDevice.mMTCP[clientIndex].E_chanRF;
            meTXD[53] = MyDevice.mMTCP[clientIndex].E_optionRF;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_lockTPDO0;
            meTXD[54] = MyDevice.myUIT.B0;
            meTXD[55] = MyDevice.myUIT.B1;
            meTXD[56] = MyDevice.mMTCP[clientIndex].E_entrTPDO0;
            meTXD[57] = MyDevice.mMTCP[clientIndex].E_typeTPDO1;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_lockTPDO1;
            meTXD[58] = MyDevice.myUIT.B0;
            meTXD[59] = MyDevice.myUIT.B1;
            meTXD[60] = MyDevice.mMTCP[clientIndex].E_entrTPDO1;
            MyDevice.myUIT.I = MyDevice.mMTCP[clientIndex].E_evenTPDO1;
            meTXD[61] = MyDevice.myUIT.B0;
            meTXD[62] = MyDevice.myUIT.B1;
            MyDevice.myUIT.F = MyDevice.mMTCP[clientIndex].E_scaling;
            meTXD[63] = MyDevice.myUIT.B0;
            meTXD[64] = MyDevice.myUIT.B1;
            meTXD[65] = MyDevice.myUIT.B2;
            meTXD[66] = MyDevice.myUIT.B3;
            meTXD[104] = 0x00;
            meTXD[106] = 0x00;

            //
            meTXD[106] = Constants.nLen;
            meTXD[107] = Constants.STOP;

            //以上和SelfUart相同
            for (int i = 107; i >= 0; i--)
            {
                meTXD[i + 4] = meTXD[i];
            }

            //移动后加帧头帧尾
            meTXD[0] = sAddress;
            meTXD[1] = Constants.RS485_WRITE;
            meTXD[2] = Constants.COM_W_SCT9 & 0xFF;
            meTXD[3] = Constants.COM_W_SCT9 >> 8;
            AP_CRC16_MODBUS(meTXD, 112).CopyTo(meTXD, 112);
            SendByteBase(meTXD, 0, 114);
            txCnt += 114;
            trTSK = TASKS.WRX9;
        }

        //串口读取SCT0
        private void Protocol_GetSCT0()
        {
            MyDevice.mMTCP[clientIndex].E_test = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_outype = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_curve = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_adspeed = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_autozero = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_trackzero = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_checkhigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_checklow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_mfg_date = MyDevice.myUIT.UI;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_mfg_srno = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_tmp_min = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_tmp_max = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_tmp_cal = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_bohrcode != MyDevice.myUIT.I)
            {
                //序列号
                MyDevice.mySN = MyDevice.mySN + 1;
                //取得code
                MyDevice.mMTCP[clientIndex].E_bohrcode = MyDevice.myUIT.I;
            }
            MyDevice.mMTCP[clientIndex].E_enspan = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_protype = meRXD[rxRead++];
            isEQ = true;
        }

        //串口读取SCT1
        private void Protocol_GetSCT1()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT2
        private void Protocol_GetSCT2()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog1 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog2 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog3 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog4 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog5 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT3
        private void Protocol_GetSCT3()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_vtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_atio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_btio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ctio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_dtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT4
        private void Protocol_GetSCT4()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero_4ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full_20ma = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full_05V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full_10V = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero_N5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full_P5 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_zero_N10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_full_P10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT5
        private void Protocol_GetSCT5()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_corr = MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_mark = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_sign = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_addr = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_baud = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_stopbit = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_parity = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_zero = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_full = MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_wt_decimal = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_unit = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_ascii = meRXD[rxRead++];
            //iBus
            MyDevice.mMTCP[clientIndex].E_wt_sptime = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_spfilt = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_division = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wt_antivib = meRXD[rxRead++];
            //CANopen
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_heartBeat = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_typeTPDO0 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_evenTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_nodeID = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_nodeBaud = meRXD[rxRead++];
            //iBus
            MyDevice.mMTCP[clientIndex].E_dynazero = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_cheatype = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_thmax = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_thmin = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_stablerange = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_stabletime = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_tkzerotime = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_tkdynatime = meRXD[rxRead++];
            isEQ = true;
        }

        //串口读取SCT6
        private void Protocol_GetSCT6()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT7
        private void Protocol_GetSCT7()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input10 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog6 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog7 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog8 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog9 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog10 = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT8
        private void Protocol_GetSCT8()
        {
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ad_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_da_point11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_input11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_analog11 = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_etio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_ftio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_gtio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_htio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_itio = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_jtio = MyDevice.myUIT.I;
            isEQ = true;
        }

        //串口读取SCT9
        private void Protocol_GetSCT9()
        {
            MyDevice.mMTCP[clientIndex].E_enGFC = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_enSRDO = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_SCT_time = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_COB_ID1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_COB_ID2 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_enOL = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_overload = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_alarmMode = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wetTarget = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wetLow = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_wetHigh = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_filter = MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netServicePort = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_netServiceIP[0] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netServiceIP[1] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netServiceIP[2] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netServiceIP[3] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netClientIP[0] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netClientIP[1] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netClientIP[2] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netClientIP[3] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netGatIP[0] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netGatIP[1] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netGatIP[2] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netGatIP[3] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netMaskIP[0] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netMaskIP[1] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netMaskIP[2] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_netMaskIP[3] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_useDHCP = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_useScan = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_addrRF[0] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_addrRF[1] = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_spedRF = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_chanRF = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_optionRF = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_lockTPDO0 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_entrTPDO0 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_typeTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_lockTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.mMTCP[clientIndex].E_entrTPDO1 = meRXD[rxRead++];
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            MyDevice.mMTCP[clientIndex].E_evenTPDO1 = (UInt16)MyDevice.myUIT.I;
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            MyDevice.mMTCP[clientIndex].E_scaling = MyDevice.myUIT.F;
            isEQ = true;
        }

        //串口写入后读出的校验SCT0
        private void Protocol_CheckSCT0()
        {
            isEQ = true;
            rxStr = "";
            if (MyDevice.mMTCP[clientIndex].E_test != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_test"; } //1
            if (MyDevice.mMTCP[clientIndex].E_outype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_outype"; } //2
            if (MyDevice.mMTCP[clientIndex].E_curve != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_curve"; } //3
            if (MyDevice.mMTCP[clientIndex].E_adspeed != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_adspeed"; } //4
            if (MyDevice.mMTCP[clientIndex].E_autozero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_autozero"; } //5
            if (MyDevice.mMTCP[clientIndex].E_trackzero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_trackzero"; } //6
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_checkhigh != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_checkhigh"; } //78910
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_checklow != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_checklow"; } //11~14
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_mfg_date != MyDevice.myUIT.UI) { isEQ = false; rxStr = "error E_mfg_date"; } //15~18
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_mfg_srno != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_mfg_srno"; } //19~22
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mMTCP[clientIndex].E_tmp_min != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_min"; } //23~26
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mMTCP[clientIndex].E_tmp_max != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_max"; } //27~30
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            //if (MyDevice.mMTCP[clientIndex].E_tmp_cal != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_tmp_cal"; } //31~34
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_bohrcode != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_bohrcode"; } //35~38
            if (MyDevice.mMTCP[clientIndex].E_enspan != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enspan"; } //39
            if (MyDevice.mMTCP[clientIndex].E_protype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_protype"; } //40
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
            if (MyDevice.mMTCP[clientIndex].E_ad_point1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point5"; }
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
            if (MyDevice.mMTCP[clientIndex].E_input1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog2"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog3 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog3"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog4 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog4"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog5"; }
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
            if (MyDevice.mMTCP[clientIndex].E_ad_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_full"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_vtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_vtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_atio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_atio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_btio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_btio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ctio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ctio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_dtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_dtio"; }
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
            if (MyDevice.mMTCP[clientIndex].E_da_zero_4ma != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_4ma"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full_20ma != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_20ma"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_zero_05V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_05V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full_05V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_05V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_zero_10V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_10V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full_10V != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_10V"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_zero_N5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_N5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full_P5 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_P5"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_zero_N10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_zero_N10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_full_P10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_full_P10"; }
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
            if (MyDevice.mMTCP[clientIndex].E_corr != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_corr"; }
            if (MyDevice.mMTCP[clientIndex].E_mark != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_mark"; }
            if (MyDevice.mMTCP[clientIndex].E_sign != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_sign"; }
            if (MyDevice.mMTCP[clientIndex].E_addr != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addr"; }
            if (MyDevice.mMTCP[clientIndex].E_baud != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_baud"; }
            if (MyDevice.mMTCP[clientIndex].E_stopbit != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stopbit"; }
            if (MyDevice.mMTCP[clientIndex].E_parity != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_parity"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wt_zero != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wt_zero"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wt_full != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wt_full"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_decimal != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_decimal"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_unit != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_unit"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_ascii != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_ascii"; }
            //iBus
            if (MyDevice.mMTCP[clientIndex].E_wt_sptime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_sptime"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_spfilt != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_spfilt"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_division != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_division"; }
            if (MyDevice.mMTCP[clientIndex].E_wt_antivib != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_wt_antivib"; }
            //CANopen
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_heartBeat != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_heartBeat"; }
            if (MyDevice.mMTCP[clientIndex].E_typeTPDO0 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_typeTPDO0"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_evenTPDO0 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_evenTPDO0"; }
            if (MyDevice.mMTCP[clientIndex].E_nodeID != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_nodeID"; }
            if (MyDevice.mMTCP[clientIndex].E_nodeBaud != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_nodeBaud"; }
            //iBus
            if (MyDevice.mMTCP[clientIndex].E_dynazero != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_dynazero"; }
            if (MyDevice.mMTCP[clientIndex].E_cheatype != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_cheatype"; }
            if (MyDevice.mMTCP[clientIndex].E_thmax != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_thmax"; }
            if (MyDevice.mMTCP[clientIndex].E_thmin != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_thmin"; }
            if (MyDevice.mMTCP[clientIndex].E_stablerange != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stablerange"; }
            if (MyDevice.mMTCP[clientIndex].E_stabletime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_stabletime"; }
            if (MyDevice.mMTCP[clientIndex].E_tkzerotime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_tkzerotime"; }
            if (MyDevice.mMTCP[clientIndex].E_tkdynatime != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_tkdynatime"; }
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
            if (MyDevice.mMTCP[clientIndex].E_ad_point6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ad_point10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point10"; }
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
            if (MyDevice.mMTCP[clientIndex].E_input6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input10"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog6 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog6"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog7 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog7"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog8 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog8"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog9 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog9"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog10 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog10"; }
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
            if (MyDevice.mMTCP[clientIndex].E_ad_point11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ad_point11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_da_point11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_da_point11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_input11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_input11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_analog11 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_analog11"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_etio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_etio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_ftio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_ftio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_gtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_gtio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_htio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_htio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_itio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_itio"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_jtio != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_jtio"; }
        }

        //串口写入后读出的校验SCT9
        private void Protocol_CheckSCT9()
        {
            isEQ = true;
            rxStr = "";
            if (MyDevice.mMTCP[clientIndex].E_enGFC != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enGFC"; }
            if (MyDevice.mMTCP[clientIndex].E_enSRDO != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enSRDO"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_SCT_time != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_SCT_time"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_COB_ID1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_COB_ID1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_COB_ID2 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_COB_ID2"; }
            if (MyDevice.mMTCP[clientIndex].E_enOL != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_enOL"; }
            if (MyDevice.mMTCP[clientIndex].E_overload != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_overload"; }
            if (MyDevice.mMTCP[clientIndex].E_alarmMode != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_alarmMode"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wetTarget != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetTarget"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wetLow != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetLow"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_wetHigh != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_wetHigh"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_filter != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_filtRange"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_netServicePort != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_netServicePort"; }
            if (MyDevice.mMTCP[clientIndex].E_netServiceIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[0]"; }
            if (MyDevice.mMTCP[clientIndex].E_netServiceIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[1]"; }
            if (MyDevice.mMTCP[clientIndex].E_netServiceIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[2]"; }
            if (MyDevice.mMTCP[clientIndex].E_netServiceIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netServiceIP[3]"; }
            if (MyDevice.mMTCP[clientIndex].E_netClientIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[0]"; }
            if (MyDevice.mMTCP[clientIndex].E_netClientIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[1]"; }
            if (MyDevice.mMTCP[clientIndex].E_netClientIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[2]"; }
            if (MyDevice.mMTCP[clientIndex].E_netClientIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netClientIP[3]"; }
            if (MyDevice.mMTCP[clientIndex].E_netGatIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[0]"; }
            if (MyDevice.mMTCP[clientIndex].E_netGatIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[1]"; }
            if (MyDevice.mMTCP[clientIndex].E_netGatIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[2]"; }
            if (MyDevice.mMTCP[clientIndex].E_netGatIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netGatIP[3]"; }
            if (MyDevice.mMTCP[clientIndex].E_netMaskIP[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[0]"; }
            if (MyDevice.mMTCP[clientIndex].E_netMaskIP[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[1]"; }
            if (MyDevice.mMTCP[clientIndex].E_netMaskIP[2] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[2]"; }
            if (MyDevice.mMTCP[clientIndex].E_netMaskIP[3] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_netMaskIP[3]"; }
            if (MyDevice.mMTCP[clientIndex].E_useDHCP != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_useDHCP"; }
            if (MyDevice.mMTCP[clientIndex].E_useScan != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_useScan"; }
            if (MyDevice.mMTCP[clientIndex].E_addrRF[0] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addrRF[0]"; }
            if (MyDevice.mMTCP[clientIndex].E_addrRF[1] != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_addrRF[1]"; }
            if (MyDevice.mMTCP[clientIndex].E_spedRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_spedRF"; }
            if (MyDevice.mMTCP[clientIndex].E_chanRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_chanRF"; }
            if (MyDevice.mMTCP[clientIndex].E_optionRF != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_optionRF"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_lockTPDO0 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_lockTPDO0"; }
            if (MyDevice.mMTCP[clientIndex].E_entrTPDO0 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_entrTPDO0 "; }
            if (MyDevice.mMTCP[clientIndex].E_typeTPDO1 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_typeTPDO1 "; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_lockTPDO1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_lockTPDO1"; }
            if (MyDevice.mMTCP[clientIndex].E_entrTPDO1 != meRXD[rxRead++]) { isEQ = false; rxStr = "error E_entrTPDO1 "; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = 0;
            MyDevice.myUIT.B3 = 0;
            if (MyDevice.mMTCP[clientIndex].E_evenTPDO1 != MyDevice.myUIT.I) { isEQ = false; rxStr = "error E_evenTPDO1"; }
            MyDevice.myUIT.B0 = meRXD[rxRead++];
            MyDevice.myUIT.B1 = meRXD[rxRead++];
            MyDevice.myUIT.B2 = meRXD[rxRead++];
            MyDevice.myUIT.B3 = meRXD[rxRead++];
            if (MyDevice.mMTCP[clientIndex].E_scaling != MyDevice.myUIT.F) { isEQ = false; rxStr = "error E_evenTPDO1"; }
        }

        #endregion

        //串口写入Eeprom参数
        public void Protocol_SendEeprom()
        {

        }

        //串口接收BohrCode
        private void Protocol_mePort_ReceiveBohrCode()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;
                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                                meRXD[1] = 0;
                                meRXD[2] = 0;
                                meRXD[3] = 0;
                                meRXD[4] = 0;
                                meRXD[5] = 0;
                                meRXD[6] = 0;
                                meRXD[7] = 0;
                                meRXD[8] = 0;
                                meRXD[9] = 0;
                                meRXD[10] = 0;
                            }
                            break;
                        case RXSTP.STX:
                            if ((meRXD[2] == 0x07) && (rxWrite >= 10))
                            {
                                //01 03 07 21 38 0C FF FF 20 1D 88 82
                                if (meChr == AP_CRC16_MODBUS(meRXD, 10)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
                            }
                            else if ((meRXD[2] == 0x08) && (rxWrite >= 11))
                            {
                                //01 03 08 0D 00 71 80 23 26 5D 05 4D AD
                                if (meChr == AP_CRC16_MODBUS(meRXD, 11)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
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
                            if ((meRXD[2] == 0x07) && (meChr == AP_CRC16_MODBUS(meRXD, 10)[1]))
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else if ((meRXD[2] == 0x08) && (meChr == AP_CRC16_MODBUS(meRXD, 11)[1]))
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                                meRXD[1] = 0;
                                meRXD[2] = 0;
                                meRXD[3] = 0;
                                meRXD[4] = 0;
                                meRXD[5] = 0;
                                meRXD[6] = 0;
                                meRXD[7] = 0;
                                meRXD[8] = 0;
                                meRXD[9] = 0;
                                meRXD[10] = 0;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //解码
                if ((meRXD[2] == 0x07) || (meRXD[2] == 0x08))
                {
                    //获取index
                    clientIndex = (byte)MyDevice.AddEndpointToFirstNonEmptymMTCP(clientIpAddr, sAddress);
                    if (clientIndex < 0) return;

                    //颠倒
                    MyDevice.myUIT.B0 = meRXD[9];
                    MyDevice.myUIT.B1 = meRXD[8];
                    MyDevice.myUIT.B2 = meRXD[7];
                    MyDevice.myUIT.B3 = meRXD[6];
                    MyDevice.mMTCP[clientIndex].E_bohrcode = MyDevice.myUIT.I;
                    //
                    MyDevice.mMTCP[clientIndex].R_bohrcode_long = meRXD[9] +
                                     ((Int64)meRXD[8] << 8) +
                                     ((Int64)meRXD[7] << 16) +
                                     ((Int64)meRXD[6] << 24) +
                                     ((Int64)meRXD[5] << 32) +
                                     ((Int64)meRXD[4] << 40) +
                                     ((Int64)meRXD[3] << 48);

                    //检查是否有外部EEPROM
                    if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDES) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDSS))
                    {
                        if ((meRXD[10] < 0x0F) && (meRXD[10] > 0x03))
                        {
                            MyDevice.mMTCP[clientIndex].R_eeplink = true;
                            MyDevice.mMTCP[clientIndex].R_eepversion = "V" + meRXD[10].ToString();
                        }
                        else
                        {
                            MyDevice.mMTCP[clientIndex].R_eeplink = false;
                            MyDevice.mMTCP[clientIndex].R_eepversion = "";
                        }
                    }
                    else
                    {
                        MyDevice.mMTCP[clientIndex].R_eeplink = false;
                        MyDevice.mMTCP[clientIndex].R_eepversion = "";
                    }

                    //
                    MyDevice.mMTCP[clientIndex].sTATE = STATE.CONNECTED;

                    //
                    isEQ = true;

                    //委托
                    MyDevice.callDelegate();
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
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                                meRXD[1] = 0;
                                meRXD[2] = 0;
                                meRXD[3] = 0;
                                meRXD[4] = 0;
                                meRXD[5] = 0;
                                meRXD[6] = 0;
                                meRXD[7] = 0;
                                meRXD[8] = 0;
                                meRXD[9] = 0;
                                meRXD[10] = 0;
                            }
                            break;
                        case RXSTP.STX:
                            if ((meRXD[2] == 0x07) && (rxWrite >= 10))
                            {
                                //01 03 07 21 38 0C FF FF 20 1D 88 82
                                if (meChr == AP_CRC16_MODBUS(meRXD, 10)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
                            }
                            else if ((meRXD[2] == 0x08) && (rxWrite >= 11))
                            {
                                //01 03 08 0D 00 71 80 23 26 5D 05 4D AD
                                if (meChr == AP_CRC16_MODBUS(meRXD, 11)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
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
                            if ((meRXD[2] == 0x07) && (meChr == AP_CRC16_MODBUS(meRXD, 10)[1]))
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else if ((meRXD[2] == 0x08) && (meChr == AP_CRC16_MODBUS(meRXD, 11)[1]))
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                                meRXD[1] = 0;
                                meRXD[2] = 0;
                                meRXD[3] = 0;
                                meRXD[4] = 0;
                                meRXD[5] = 0;
                                meRXD[6] = 0;
                                meRXD[7] = 0;
                                meRXD[8] = 0;
                                meRXD[9] = 0;
                                meRXD[10] = 0;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //解码
                if ((meRXD[2] == 0x07) || (meRXD[2] == 0x08))
                {
                    //写SCT5时记录修改后的新ID
                    nAddress = MyDevice.mMTCP[clientIndex].E_addr;
                    //获取index
                    int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                    if (index < 0) return;
                    clientIndex = (byte)index;

                    //颠倒
                    MyDevice.myUIT.B0 = meRXD[9];
                    MyDevice.myUIT.B1 = meRXD[8];
                    MyDevice.myUIT.B2 = meRXD[7];
                    MyDevice.myUIT.B3 = meRXD[6];

                    //非模拟量设备第一次更新Bohrcode
                    if ((MyDevice.mMTCP[clientIndex].E_bohrcode == -1) && (MyDevice.mMTCP[clientIndex].S_OutType == OUT.UMASK))
                    {
                        MyDevice.mMTCP[clientIndex].E_bohrcode = MyDevice.myUIT.I;
                    }

                    //校验E_bohrcode
                    if (MyDevice.mMTCP[clientIndex].E_bohrcode == MyDevice.myUIT.I)
                    {
                        isEQ = true;
                        rxStr = "";
                    }
                    else
                    {
                        isEQ = true;//不再设置BCC校验失败,用户可以继续使用产品,但是要提示返厂模拟量校准
                        rxStr = "error E_bohrcode," + MyDevice.mMTCP[clientIndex].E_bohrcode.ToString("X2") + " ? " + MyDevice.myUIT.I.ToString("X2");
                    }

                    //检查是否有外部EEPROM
                    if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDES) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDSS))
                    {
                        if ((meRXD[10] < 0x0F) && (meRXD[10] > 0x03))
                        {
                            MyDevice.mMTCP[clientIndex].R_eeplink = true;
                            MyDevice.mMTCP[clientIndex].R_eepversion = "V" + meRXD[10].ToString();
                        }
                        else
                        {
                            MyDevice.mMTCP[clientIndex].R_eeplink = false;
                            MyDevice.mMTCP[clientIndex].R_eepversion = "";
                        }
                    }
                    else
                    {
                        MyDevice.mMTCP[clientIndex].R_eeplink = false;
                    }

                    //委托
                    MyDevice.callDelegate();
                }

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收读取Sector(读写字节长度不一样)
        //SCT0-SCT8
        private void Protocol_mePort_ReceiveSectorRead()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            break;
                        case RXSTP.STX:
                            if (trTASK == TASKS.REPRM)
                            {
                                if (rxWrite >= 121)
                                {
                                    //读01 03 2C 02 99 56 D3 02 3C 0A 02 00 0F 42 40 00 00 00 00 78 85 98 51 00 00 00 00 00 7A 12 00 00 00 00 00 00 00 00 00 FF FF FF FF 01 00 29 03 32 41
                                    //读34 03 2C 02 A1 3F 80 00 00 FF 03 34 03 01 00 00 00 00 00 00 00 03 E8 00 00 00 03 0A 01 00 FF FF FF FF FF FF FF 02 00 04 03 FF FF FF FF 29 03 24 BA
                                    //RS485高位在前低位在后
                                    //SelfUART低位在前高位在后
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 121)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            else if (trTASK == TASKS.RDX9)
                            {
                                if (rxWrite >= 111)
                                {
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 111)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            else
                            {
                                if (rxWrite >= 47)
                                {
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 47)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else if (trTASK != TASKS.REPRM)
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            break;
                        case RXSTP.ACK:
                            if (meChr == AP_CRC16_MODBUS(meRXD, 47)[1] || meChr == AP_CRC16_MODBUS(meRXD, 121)[1] || meChr == AP_CRC16_MODBUS(meRXD, 111)[1])
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if ((rxSTP == RXSTP.ETX) && ((meRXD[2] == 0x2C) || (meRXD[2] == 0x6C)))
            {
                //获取index
                int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                if (index < 0) return;
                clientIndex = (byte)index;

                //RDX和WRX的帧头字节数不一样
                rxRead = 5;

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
                        //RDX和WRX的帧头字节数不一样
                        switch (meRXD[4])
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

        //串口接收写入Sector(读写字节长度不一样)
        //SCT0-SCT8
        private void Protocol_mePort_ReceiveSectorWrite()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if ((meChr == sAddress) || (meChr == nAddress))   //改地址后WRX5的回复可能为新地址或旧地址
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            break;
                        case RXSTP.STX:
                            if (trTASK == TASKS.WEPRM)
                            {
                                if (rxWrite >= 122)
                                {
                                    //写34 06 A0 A1 02 A1 3F 80 00 00 FF 03 01 03 01 00 00 00 00 00 00 00 03 E8 00 01 00 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF 29 03 0F 01
                                    //RS485高位在前低位在后
                                    //SelfUART低位在前高位在后
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 122)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            else if (trTASK == TASKS.WRX9)
                            {
                                if (rxWrite >= 112)
                                {
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 112)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            else
                            {
                                if (rxWrite >= 48)
                                {
                                    if (meChr == AP_CRC16_MODBUS(meRXD, 48)[0])
                                    {
                                        rxSTP = RXSTP.ACK;
                                    }
                                    else
                                    {
                                        rxSTP = RXSTP.NUL;
                                    }
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
                            }
                            break;
                        case RXSTP.ACK:
                            if (meChr == AP_CRC16_MODBUS(meRXD, 48)[1] || meChr == AP_CRC16_MODBUS(meRXD, 122)[1] || meChr == AP_CRC16_MODBUS(meRXD, 112)[1])
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if ((rxSTP == RXSTP.ETX) && (meRXD[2] == 0xA0))
            {
                //获取index
                int index;
                if (sAddress != nAddress)
                {
                    index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, nAddress);
                }
                else
                {
                    index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                }
                if (index < 0) return;
                clientIndex = (byte)index;

                //RDX和WRX的帧头字节数不一样
                rxRead = 6;

                //解码
                switch (trTSK)
                {
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
                        //RDX和WRX的帧头字节数不一样
                        switch (meRXD[5])
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
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收采样的滤波内码
        //01 03 04 00 03 82 89 AB 35
        private void Protocol_mePort_ReceiveAdpoint()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            break;
                        case RXSTP.STX:
                            if (rxWrite >= 7)
                            {
                                //01 03 04 00 03 82 89 AB 35
                                if (meChr == AP_CRC16_MODBUS(meRXD, 7)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
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
                            if (meChr == AP_CRC16_MODBUS(meRXD, 7)[1])
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if ((rxSTP == RXSTP.ETX) && (meRXD[1] == 0x03) && (meRXD[2] == 0x04))
            {
                //获取index
                int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                if (index < 0) return;
                clientIndex = (byte)index;

                //取值
                MyDevice.myUIT.B3 = meRXD[3]; //MSB
                MyDevice.myUIT.B2 = meRXD[4];
                MyDevice.myUIT.B1 = meRXD[5];
                MyDevice.myUIT.B0 = meRXD[6]; //LSB
                rxDat = MyDevice.myUIT.I;

                //mV/V
                double mvdv = (double)rxDat / MyDevice.mMTCP[clientIndex].S_MVDV;

                //
                switch (trTSK)
                {
                    case TASKS.ADCP1:
                        MyDevice.mMTCP[clientIndex].E_input1 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point1 = rxDat;
                        MyDevice.mMTCP[clientIndex].E_ad_zero = rxDat;
                        break;
                    case TASKS.ADCP2:
                        MyDevice.mMTCP[clientIndex].E_input2 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point2 = rxDat;
                        break;
                    case TASKS.ADCP3:
                        MyDevice.mMTCP[clientIndex].E_input3 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point3 = rxDat;
                        break;
                    case TASKS.ADCP4:
                        MyDevice.mMTCP[clientIndex].E_input4 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point4 = rxDat;
                        break;
                    case TASKS.ADCP5:
                        MyDevice.mMTCP[clientIndex].E_input5 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point5 = rxDat;
                        if (!MyDevice.mMTCP[clientIndex].S_ElevenType) MyDevice.mMTCP[clientIndex].E_ad_full = rxDat;
                        break;
                    case TASKS.ADCP6:
                        MyDevice.mMTCP[clientIndex].E_input6 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point6 = rxDat;
                        break;
                    case TASKS.ADCP7:
                        MyDevice.mMTCP[clientIndex].E_input7 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point7 = rxDat;
                        break;
                    case TASKS.ADCP8:
                        MyDevice.mMTCP[clientIndex].E_input8 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point8 = rxDat;
                        break;
                    case TASKS.ADCP9:
                        MyDevice.mMTCP[clientIndex].E_input9 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point9 = rxDat;
                        break;
                    case TASKS.ADCP10:
                        MyDevice.mMTCP[clientIndex].E_input10 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point10 = rxDat;
                        break;
                    case TASKS.ADCP11:
                        MyDevice.mMTCP[clientIndex].E_input11 = MyDevice.myUIT.ConvertFloatToInt32((float)mvdv);
                        MyDevice.mMTCP[clientIndex].E_ad_point11 = rxDat;
                        if (MyDevice.mMTCP[clientIndex].S_ElevenType) MyDevice.mMTCP[clientIndex].E_ad_full = rxDat;
                        break;
                }

                //委托
                MyDevice.callDelegate();

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

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                //获取index
                int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                if (index < 0) return;
                clientIndex = (byte)index;

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = (char)meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (meChr)
                    {
                        case '=':
                            rxSTP = RXSTP.STX;
                            MyDevice.mMTCP[clientIndex].R_isFLT = false;
                            rxStr = null;//default
                            break;

                        case ' ':
                            break;

                        case '+':
                            MyDevice.mMTCP[clientIndex].R_isFLT = false;
                            rxStr += meChr.ToString();
                            break;

                        case '-':
                            MyDevice.mMTCP[clientIndex].R_isFLT = false;
                            rxStr += meChr.ToString();
                            break;

                        case 'F':
                            MyDevice.mMTCP[clientIndex].R_isFLT = true;
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
                                    case TASKS.ADC:
                                    case TASKS.SFLT:
                                    case TASKS.RFLT:
                                        rxDat = 234;
                                        if (MyDevice.mMTCP[clientIndex].R_isFLT)
                                        {
                                            rxDat = (int)TASKS.SFLT;
                                            MyDevice.mMTCP[clientIndex].E_filter = Convert.ToInt32(rxStr);
                                        }
                                        else
                                        {
                                            rxDat = 0;
                                            MyDevice.mMTCP[clientIndex].R_adcout = Convert.ToInt32(rxStr);
                                        }
                                        break;

                                    case TASKS.RDFT:
                                        rxDat = 234;
                                        if (MyDevice.mMTCP[clientIndex].R_isFLT)
                                        {
                                            rxDat = (int)TASKS.SFLT;
                                            MyDevice.mMTCP[clientIndex].E_filter = Convert.ToInt32(rxStr);
                                            isEQ = true;//iBus读SCT1-8后读MEM.filt_range 采样滤波范围
                                        }
                                        else
                                        {
                                            rxDat = 0;
                                            MyDevice.mMTCP[clientIndex].R_adcout = Convert.ToInt32(rxStr);
                                        }
                                        break;

                                    case TASKS.QDAC:
                                        rxDat = 234;
                                        if (MyDevice.mMTCP[clientIndex].R_isFLT)
                                        {
                                            rxDat = (int)TASKS.SFLT;
                                            MyDevice.mMTCP[clientIndex].E_filter = Convert.ToInt32(rxStr);
                                        }
                                        else
                                        {
                                            rxDat = 0;
                                            MyDevice.mMTCP[clientIndex].R_dacset = Convert.ToInt32(rxStr);
                                            MyDevice.mMTCP[clientIndex].RefreshDacout(MyDevice.mMTCP[clientIndex].R_dacset);
                                        }
                                        break;

                                    case TASKS.GODMZ:
                                    case TASKS.GOUPZ:
                                    case TASKS.GODMF:
                                    case TASKS.GOUPF:
                                        //直接使用rxStr
                                        if (MyDevice.mMTCP[clientIndex].R_isFLT)
                                        {
                                            rxStr = "";
                                        }
                                        break;
                                }

                                //委托
                                MyDevice.callDelegate();

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
        }

        //串口接收读取dacout
        //02 80 80 80
        //02 80 80 80 80
        //03 80 80 80
        //03 80 80 80 80
        private void Protocol_mePort_ReceiveDacout()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                //获取index
                int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                if (index < 0) return;
                clientIndex = (byte)index;

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //检查是否有外部EEPROM
                    if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDES) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDSS))
                    {
                        switch (meChr)
                        {
                            case 0x02:
                            case 0x03:
                                MyDevice.mMTCP[clientIndex].R_eeplink = false;
                                break;

                            case 0x42:
                            case 0x43:
                                meChr &= 0x0F;
                                MyDevice.mMTCP[clientIndex].R_eeplink = true;
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        MyDevice.mMTCP[clientIndex].R_eeplink = false;
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
                                        MyDevice.mMTCP[clientIndex].RefreshDacout(rxDat);

                                        //委托
                                        MyDevice.callDelegate();
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
                                        MyDevice.mMTCP[clientIndex].RefreshDacout(rxDat);

                                        //委托
                                        MyDevice.callDelegate();
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
                                        MyDevice.mMTCP[clientIndex].RefreshDacout(rxDat);

                                        //委托
                                        MyDevice.callDelegate();
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
                                        MyDevice.mMTCP[clientIndex].RefreshDacout(rxDat);

                                        //委托
                                        MyDevice.callDelegate();
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
                            break;
                    }
                }
            }
        }

        //问答接收ascii
        //=SN+123456.7kg\r\n
        //=SG-  3456.7kg\r\n
        private void Protocol_mePort_ReceiveAscii()
        {
            //
            char meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                //获取index
                int index = MyDevice.FindClientIndexByEndpointAndAddr(clientIpAddr, sAddress);
                if (index < 0) return;
                clientIndex = (byte)index;

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = (char)meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //检查是否有外部EEPROM
                    if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDES) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TDSS))
                    {
                        switch (meChr)
                        {
                            case '=':
                                MyDevice.mMTCP[clientIndex].R_eeplink = false;
                                break;

                            case '}':
                                meChr = '=';
                                MyDevice.mMTCP[clientIndex].R_eeplink = true;
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        MyDevice.mMTCP[clientIndex].R_eeplink = false;
                    }

                    //帧解析
                    switch (meChr)
                    {
                        case '=':
                            rxSTP = RXSTP.STX;
                            rxStr = "=";//default
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
                            if (rxSTP == RXSTP.STX)
                            {
                                //接收数据
                                rxStr += meChr.ToString();
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                //字符O,S,U
                if (rxStr.Contains("=O"))
                {
                    rxStr = rxStr.Replace("=O", "=");
                    MyDevice.mMTCP[clientIndex].R_overload = true;
                }
                else if (rxStr.Contains("=S"))
                {
                    rxStr = rxStr.Replace("=S", "=");
                    MyDevice.mMTCP[clientIndex].R_stable = true;
                    MyDevice.mMTCP[clientIndex].R_overload = false;
                }
                else if (rxStr.Contains("=U"))
                {
                    rxStr = rxStr.Replace("=U", "=");
                    MyDevice.mMTCP[clientIndex].R_stable = false;
                    MyDevice.mMTCP[clientIndex].R_overload = false;
                }

                //字符G,N
                if (rxStr.Contains("=G"))
                {
                    MyDevice.mMTCP[clientIndex].R_grossnet = "毛重";
                    rxStr = rxStr.Replace("=G", "");
                }
                else if (rxStr.Contains("=N"))
                {
                    MyDevice.mMTCP[clientIndex].R_grossnet = "净重";
                    rxStr = rxStr.Replace("=N", "");
                }
                else
                {
                    MyDevice.mMTCP[clientIndex].R_grossnet = "";
                    rxStr = rxStr.Replace("=", "");
                }

                //删除单位
                rxStr = rxStr.Replace("kg", "");
                rxStr = rxStr.Replace("lb", "");
                rxStr = rxStr.Replace("oz", "");
                rxStr = rxStr.Replace("g", "");
                rxStr = rxStr.Replace("m", "");//mg
                rxStr = rxStr.Replace("t", "");
                rxStr = rxStr.Replace("c", "");//ct
                rxStr = rxStr.Replace("N", "");
                rxStr = rxStr.Replace("k", "");//kN
                rxStr = rxStr.Replace(" ", "");

                //取重量
                MyDevice.mMTCP[clientIndex].RefreshAscii(rxStr);

                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //串口接收Tare,Zero,Span,Reset指令
        private void Protocol_mePort_ReceiveReset()
        {
            //
            Byte meChr;

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //帧解析
                    switch (rxSTP)
                    {
                        case RXSTP.NUL:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            break;
                        case RXSTP.STX:
                            if (rxWrite >= 6)
                            {
                                //01 06 00 33 00 01 B8 05
                                if (meChr == AP_CRC16_MODBUS(meRXD, 6)[0])
                                {
                                    rxSTP = RXSTP.ACK;
                                }
                                else
                                {
                                    rxSTP = RXSTP.NUL;
                                }
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
                            if (meChr == AP_CRC16_MODBUS(meRXD, 6)[1])
                            {
                                rxSTP = RXSTP.ETX;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                        case RXSTP.ETX:
                            if (meChr == sAddress)
                            {
                                rxSTP = RXSTP.STX;
                                //default
                                Array.Clear(meRXD, 0, meRXD.Length);
                                rxWrite = 0;
                                rxRead = 0;
                                //
                                meRXD[rxWrite++] = meChr;
                            }
                            else
                            {
                                rxSTP = RXSTP.NUL;
                            }
                            break;
                    }
                }
            }

            //判断协议
            if (rxSTP == RXSTP.ETX)
            {
                switch (meRXD[3])
                {
                    case 0x30:// TASKS.TARE
                    case 0x31:// TASKS.ZERO
                    case 0x32:// TASKS.SPAN
                              //回复指令正确
                        isEQ = true;
                        break;

                    case 0x33:// TASKS.REST:
                        //新版本设备解码重启指令
                        //老版本设备重启后上电归零时间长
                        //老版本的自定义协议不回复重启指令
                        //老版本的RS485协议回复0x00重启指令
                        if (meRXD[5] == 0x01)
                        {
                            isEQ = true;
                        }
                        else
                        {
                            isEQ = false;
                        }
                        break;

                    default:
                        isEQ = false;
                        break;
                }

                //委托
                MyDevice.callDelegate();

                //协议
                rxSTP = RXSTP.NUL;
            }
        }

        //ModbusTCP扫描站点
        private void Protocol_mePort_ReceiveModbusTCPScan()
        {
            //
            Byte meChr;
            Byte[] meCrc = new Byte[7];

            //读取Byte
            while (mePort.MessagesToRead > 0)
            {
                //取字节
                TCPMessage meMSG = mePort.ReadMessage();

                if (meMSG.MsgIP.ToString() != clientIpAddr) break;  //判断ip地址

                for (int i = 0; i < meMSG.DataLen; i++)
                {
                    meChr = meMSG.Data[i];

                    //字节统计
                    rxCnt++;

                    //取字节
                    rxRead = rxWrite;

                    //取字节
                    meRXD[rxWrite++] = meChr;

                    //数据指针
                    if (rxWrite >= meRXD.Length) rxWrite = 0;

                    //取帧
                    meCrc[6] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[5] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[4] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[3] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[2] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[1] = meRXD[rxRead--];
                    if (rxRead < 0) rxRead = meRXD.Length - 1;
                    meCrc[0] = meRXD[rxRead--];

                    //校验
                    if (meCrc[0] == meCrc[4])//addr校验
                    {
                        if (meCrc[1] == Constants.RS485_READ)//读指令校验
                        {
                            if (meCrc[2] == 0x02)//长度校验
                            {
                                if (meCrc[5] == AP_CRC16_MODBUS(meCrc, 5)[0])//MODMTCP CRC校验
                                {
                                    if (meCrc[6] == AP_CRC16_MODBUS(meCrc, 5)[1])//MODMTCP CRC校验
                                    {
                                        //获取扫描到的地址
                                        sAddress = meCrc[0];

                                        //获取index
                                        clientIndex = (byte)MyDevice.AddEndpointToFirstNonEmptymMTCP(clientIpAddr, sAddress);
                                        if (clientIndex < 0) return;

                                        //状态更新
                                        MyDevice.mMTCP[clientIndex].sTATE = STATE.CONNECTED;

                                        //委托
                                        MyDevice.callDelegate();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //接收触发函数,实际会由串口线程创建
        private void Protocol_mePort_DataReceived(object sender, NetDataReceiveEventArgs e)
        {
            if (is_serial_closing)
            {
                is_serial_listening = false;//准备关闭串口时，reset串口侦听标记
                return;
            }
            try
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
                    case TASKS.REPRM:
                        Protocol_mePort_ReceiveSectorRead();//读和写帧头不一样
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
                    case TASKS.WEPRM:
                        Protocol_mePort_ReceiveSectorWrite();//读和写帧头不一样
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
                    case TASKS.GODMF:
                    case TASKS.GOUPF:
                    case TASKS.SFLT:
                    case TASKS.RFLT:
                    case TASKS.RDFT:
                    case TASKS.QDAC:
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

                    case TASKS.TARE:
                    case TASKS.ZERO:
                    case TASKS.SPAN:
                        Protocol_mePort_ReceiveReset();
                        break;
                    case TASKS.REST:
                        Protocol_mePort_ReceiveReset();
                        break;

                    case TASKS.SCAN:
                        Protocol_mePort_ReceiveModbusTCPScan();
                        break;

                    default:
                        mePort.DiscardBuffer();
                        break;
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
                        switch (MyDevice.mMTCP[clientIndex].S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                                MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mMTCP[clientIndex].E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendCOM(TASKS.RDX5);
                                }
                                else
                                {
                                    MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                                    MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iBus) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TD485) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TP10) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendCOM(TASKS.RDX6);
                        }
                        else if (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mMTCP[clientIndex].E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendCOM(TASKS.RDX6);
                            }
                            else
                            {
                                MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                                MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                            }
                        }
                        else
                        {
                            MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                            MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN || MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet || MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iStar)//有sct9
                        {
                            Protocol_SendCOM(TASKS.RDX9);
                        }
                        else if (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iBus) //iBus读SCT1-8后读MEM.filt_range 采样滤波范围
                        {
                            Protocol_SendCOM(TASKS.RDFT);
                        }
                        else
                        {
                            MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                            MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                        MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.RDX9);
                    }
                    break;

                case TASKS.RDFT:
                    if (isEQ)
                    {
                        MyDevice.mMTCP[clientIndex].sTATE = STATE.WORKING;
                        MyDevice.mMTCP[clientIndex].R_checklink = MyDevice.mMTCP[clientIndex].R_eeplink;
                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
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
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }

        //串口写入任务状态机 BCC -> WRX5  -> (WRX9)-> (rst)
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
                        if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN && MyDevice.mMTCP[clientIndex].E_test > 0x58) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet))  //新版本TCAN才有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        Protocol_SendCOM(TASKS.REST);
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iBus) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TD485) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TP10) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendSCT6();
                        }
                        else
                        {
                            if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                Protocol_SendCOM(TASKS.REST);
                            }

                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            Protocol_SendCOM(TASKS.REST);
                        }

                        trTSK = TASKS.NULL;
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        switch (MyDevice.mMTCP[clientIndex].S_DeviceType)
                        {
                            default:
                            case TYPE.BS420H:
                            case TYPE.T8X420H:
                            case TYPE.BS600H:
                            case TYPE.T420:
                            case TYPE.TNP10:
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                                }
                                break;

                            case TYPE.BE30AH:
                            case TYPE.TP10:
                            case TYPE.TDES:
                            case TYPE.TDSS:
                            case TYPE.T4X600H:
                                if (MyDevice.mMTCP[clientIndex].E_test > 0x55)//TDES/TDSS老版本没有SCT5
                                {
                                    Protocol_SendSCT5();
                                }
                                else
                                {
                                    Protocol_SendCOM(TASKS.REST);
                                    if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                    {
                                        trTSK = TASKS.NULL;
                                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if ((MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iBus) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TD485) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TP10) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet) || (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iStar))//有SCT6-SCT8
                        {
                            Protocol_SendSCT6();
                        }
                        else if (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN)
                        {
                            if (MyDevice.mMTCP[clientIndex].E_test > 0x58)//TCAN老版本没有SCT6-SCT9
                            {
                                Protocol_SendSCT6();
                            }
                            else
                            {
                                Protocol_SendCOM(TASKS.REST);
                                if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                                {
                                    trTSK = TASKS.NULL;
                                    MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                                }
                            }
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.TCAN || MyDevice.mMTCP[clientIndex].S_DeviceType == TYPE.iNet)//有SCT9
                        {
                            Protocol_SendSCT9();
                        }
                        else
                        {
                            Protocol_SendCOM(TASKS.REST);
                            if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                            {
                                trTSK = TASKS.NULL;
                                MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        if (MyDevice.mMTCP[clientIndex].E_test < 0x58)//标定后,老版本的需要发重启指令,设备会重启但是不会有回复
                        {
                            trTSK = TASKS.NULL;
                            MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
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
                        MyDevice.SaveToLog(MyDevice.mMTCP[clientIndex]);
                    }
                    else
                    {
                        Protocol_SendCOM(TASKS.REST);
                    }
                    break;
            }
        }

        /// <summary>
        /// 将数据封装为message发送
        /// </summary>
        /// <param name="cmdId"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool SendByteBase(byte[] data, int offset, int count)
        {
            Socket socket = TCPServer.FindSocketByIP(clientIpAddr);
            if (socket != null && socket.Connected)
            {
                var msg = new TCPMessage(socket);
                msg.DataLen = count;
                for (int i = 0; i < count; i++)
                {
                    msg.Data[i] = data[i + offset];
                }

                mePort.Send(msg);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
