using System;
using System.IO.Ports;

//未经过审批不得改动

//Alvin 20230314
//Junzhe 20231124

//接口参数
//接口方法

//参考<变送器通讯规范20230314.c>

//规定了通讯接口类型

//规定了通讯的指令
//规定了通讯的回复格式
//规定了问答和连续发送
//规定了广播地址
//规定了操作对通讯的要求(零满点采样)

namespace Model
{
    public interface IProtocol
    {
        #region

        COMP type { get; }      	//接口类型
        Byte addr { set; get; }     //设备站点

        Boolean Is_serial_listening { set; get; }   //串口正在监听标记
        Boolean Is_serial_closing { set; get; }     //串口正在关闭标记

        String portName { get; }    //接口用的串口COMx
        Int32 baudRate { get; }     //接口用的串口波特率
        StopBits stopBits { get; }  //接口用的串口停止位
        Parity parity { get; }      //接口用的串口校验位
        UInt32 channel { get; }     //接口用的频道
        String ipAddr { get; set; }     //目标客户端ip地址
        Boolean IsOpen { get; }     //接口串口是否打开
        TASKS trTASK { set; get; }  //接口读写状态机
        Int32 txCount { get; }      //接口发送字节计数
        Int32 rxCount { get; }      //接口接收字节计数
        Int32 rxData { get; }       //收到的数值
        String rxString { get; }    //收到的字符串
        Boolean isEqual { get; }    //接收的SCT校验结果

        #endregion
        //打开端口
        void Protocol_PortOpen(string ip, Int32 port = 5678);

        //打开CAN口
        void Protocol_PortOpen(UInt32 index, String name, Int32 baud);

        //打开串口
        void Protocol_PortOpen(String name, Int32 baud, StopBits stb, Parity pay);

        //关闭串口
        bool Protocol_PortClose();

        //清除串口任务
        void Protocol_ClearState();

        //刷新IsEQ
        void Protocol_ChangeEQ();

        //发送读命令
        void Protocol_SendCOM(TASKS meTask);

        //扫描地址
        void Protocol_SendAddr(Byte addr);

        //广播指令,有重量设置地址
        void Protocol_HwSetAddr(Byte addr);

        //广播指令,检索重量设置地址
        void Protocol_SwSetAddr(Byte addr, UInt32 weight);

        //串口写入Eeprom参数
        void Protocol_SendEeprom();

        //发网络管理指令
        void Protocol_SendNMT(Byte NMT_CS);

        //获取心跳的Node ID
        uint Protocol_GetHeartBeatID();

        //设置心跳间隔
        void Protocol_SendHeartBeat(UInt16 period);

        //串口读取任务状态机 BOR -> RDX0 -> RDX1 -> RDX2 -> RDX3 -> RDX4 -> RDX5 -> RDX6 -> RDX7 -> RDX8
        void Protocol_mePort_ReadTasks();

        //串口写入任务状态机 WRX0 -> RST
        void Protocol_mePort_WriteTypTasks();

        //串口写入任务状态机 BCC -> WRX5 -> (rst)
        void Protocol_mePort_WriteParTasks();

        //串口写入任务状态机 BCC -> WRX5 -> RST
        void Protocol_mePort_WriteBusTasks();

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> (rst)
        void Protocol_mePort_WriteFacTasks();

        //串口写入任务状态机 BCC -> WRX1 -> WRX2 -> WRX3 -> WRX6 -> WRX7 -> WRX8 -> (rst)
        void Protocol_mePort_WriteCalTasks();

        //串口写入任务状态机 BCC -> WRX0 -> WRX1 -> WRX2 -> WRX3 -> WRX4 -> WRX5 -> WRX6 -> WRX7 -> WRX8 -> RST
        void Protocol_mePort_WriteAllTasks();
    }
}

