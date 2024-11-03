using System;
using System.IO.Ports;

//Tong Ziyun 20230428

namespace Tooling
{
    public class JIG
    {
        private SerialPort mePort = new SerialPort();           //自定义接口使用的串口
        private RXSTP rxSTP = RXSTP.NUL;                        //通讯接收状态机,找出帧头帧尾
        private Int32 rxCnt = 0;                                //接收计数

        private Byte[] meTXD = new Byte[] { 02, 51, 01, 03 };   //发送缓冲区

        #region 定义属性
        public Boolean IsOpen
        {
            get
            {
                return mePort.IsOpen;
            }
        }
        #endregion

        public JIG()
        {
            mePort.PortName = "COM1";
            mePort.BaudRate = Convert.ToInt32(19200); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.None; //校验位固定
            mePort.ReceivedBytesThreshold = 1; //接收即通知
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="name"></param>
        public void PortOpen(String name)
        {
            //修改参数必须先关闭串口
            if (mePort.PortName != name)
            {
                if (mePort.IsOpen)
                {
                    try
                    {
                        mePort.DataReceived -= new SerialDataReceivedEventHandler(mePort_DataReceived);
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
                //
                mePort.PortName = name;
                mePort.Open();
                mePort.DataReceived += new SerialDataReceivedEventHandler(mePort_DataReceived); //接收处理
            }
            catch
            {

            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public bool PortClose()
        {
            if (mePort.IsOpen)
            {
                try
                {
                    mePort.DataReceived -= new SerialDataReceivedEventHandler(mePort_DataReceived);
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

        /// <summary>
        /// 发送读命令
        /// </summary>
        /// <param name="meChr"></param>
        public void SendCOM(Byte meChr)
        {
            //
            if (!mePort.IsOpen) return;

            meTXD[1] = meChr;
            mePort.Write(meTXD, 0, 4);

        }

        /// <summary>
        /// 串口接收函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte meChr;

            //读取Byte, 02 51 00 00 00 00 00 00 00 00 03
            while (mePort.BytesToRead > 0)
            {
                //
                meChr = (Byte)mePort.ReadByte();

                switch (rxSTP)
                {
                    case RXSTP.NUL://0x02
                        if (meChr == 0x02)
                        {
                            rxCnt = 0;
                            rxSTP = RXSTP.STX;
                        }
                        break;
                    case RXSTP.STX://51
                        if (meChr == meTXD[1])
                        {
                            rxSTP = RXSTP.ACK;
                        }
                        break;
                    case RXSTP.ACK:
                        if (++rxCnt != 9) continue;

                        if (meChr == 0x03)
                        {
                            rxSTP = RXSTP.ETX;
                        }
                        else
                        {
                            rxSTP = RXSTP.NUL;
                        }
                        break;
                }
            }

            if (rxSTP == RXSTP.ETX)
            {
                //委托
                MyDefine.callDelegate();
            }
        }
    }
}
