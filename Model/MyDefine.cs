using System;
using System.ComponentModel;

//未经过审批不得改动

//Alvin 20230414
//Junzhe 20230616
//Lumi 20231227

//枚举和常量和设备程序保持一致

//参考<变送器默认参数20230301.xlsx>
//规定了各设备和版本的要求

//UNIT移动到了Library/UnitHelper下

namespace Model
{
    public enum TYPE : Byte
    {
        //数字输出的型号(0x01~0x6F)
        //RS485 MODBUS RTU
        //CANopen
        //DeviceNet
        //SAEJ1939
        //CC-Link
        //EtherNet MODBUS TCP
        //EtherNet/IP
        //EtherCAT
        //PROFIBUS
        //PROFINET
        //Powerlink
        TD485 = 0x01,
        TCAN = 0x02,
        iBus = 0x03,
        iNet = 0x04,
        iStar = 0x05,

        //模拟量输出的型号(0x20~0xF8)
        //0x20
        //0x28
        //0x30
        //0x38
        //0x40
        //0x48
        //0x50
        //0x58
        //0x60
        //0x68
        BE30AH = 0x70,
        //0x78
        BS420H = 0x80,  //BR420H
        T8X420H = 0x88, //T8X420H
        BS600H = 0x90,
        //0x98
        T420 = 0xA0,    //T420,T420T,T2X420
        //0xA8
        TNP10 = 0xB0,
        //0xB8
        TDES = 0xC0,    //TDES-420-bmt,TDES-420-nm,TDES-10-bmt,TDES-10-nm
        //0xC8
        TDSS = 0xD0,    //TDSS-420-bmt,TDSS-420-nm,TDSS-NP10-bmt,TDSS-NP10-nm
        //0xD8
        TP10 = 0xE0,    //TDES-10-ns等于TP10
        //0xE8
        T4X600H = 0xF0, //
        //0xF8
    }

    public enum OUT : Byte
    {
        [Description("Output 4-20mA")] UT420 = 0x01,
        [Description("Output 0-5V")] UTP05 = 0x02,
        [Description("Output 0-10V")] UTP10 = 0x03,
        [Description("Output ±5V")] UTN05 = 0x04,
        [Description("Output ±10V")] UTN10 = 0x05,
        [Description("Output 数字量")] UMASK = 0x0F,
    }

    public enum ECVE : Byte
    {
        [Description("直线算法两点法")] CTWOPT = 2,
        [Description("直线算法五点拟合")] CFITED = 3,
        [Description("直线算法五点插值")] CINTER = 4,
        [Description("直线算法十一点拟合")] CELTED = 5,
        [Description("直线算法十一点插值")] CELTER = 6,
    }

    public enum ECVEType : Byte
    {
        [Description("单程")] Single,
        [Description("进返程")] ForBack,
    }

    public enum EPGA : Byte
    {
        [Description("输入信号范围±400mV/V")] ADPGA1 = 0x00,
        [Description("输入信号范围±200mV/V")] ADPGA2 = 0x04,
        [Description("输入信号范围±7.0mV/V")] ADPGA64 = 0x08,
        [Description("输入信号范围±3.5mV/V")] ADPGA128 = 0x0C,
    }

    public enum ESPD : Byte
    {
        [Description("ADC输出速率10Hz")] CSF10 = 0x00,
        [Description("ADC输出速率40Hz")] CSF40 = 0x10,
        [Description("ADC输出速率600Hz")] CSF640 = 0x20,
        [Description("ADC输出速率1200Hz")] CSF1280 = 0x30,
    }

    public enum EATZ : Byte
    {
        [Description("开机不归零 0%Rang")] ATZ0 = 0,
        [Description("开机自动归零 2%Rang")] ATZ2 = 2,
        [Description("开机自动归零 4%Rang")] ATZ4 = 4,
        [Description("开机自动归零 10%Rang")] ATZ10 = 10,
        [Description("开机自动归零 20%Rang")] ATZ20 = 20,
        [Description("开机自动归零 50%Rang")] ATZ50 = 50,
        [Description("other")] ATZ00 = 200,
    };

    public enum EATK : Byte
    {
        [Description("0 fdn")] TKZ0 = 0,
        [Description("2 fdn")] TKZ2 = 2,
        [Description("4 fdn")] TKZ4 = 4,
        [Description("8 fdn")] TKZ8 = 8,
        [Description("16 fdn")] TKZ16 = 16,
        [Description("32 fdn")] TKZ32 = 32,

        [Description("0 e")] TKZ00 = 0,
        [Description("0.5 e")] TKZ5 = 1,
        [Description("1 e")] TKZ10 = 2,
        [Description("2 e")] TKZ20 = 4,
        [Description("3 e")] TKZ30 = 6,
        [Description("4 e")] TKZ40 = 8,
        [Description("5 e")] TKZ50 = 10,
        [Description("6 e")] TKZ60 = 12,
        [Description("7 e")] TKZ70 = 14,
        [Description("8 e")] TKZ80 = 16,
        [Description("9 e")] TKZ90 = 18,
        [Description("10 e")] TKZ100 = 20,
        [Description("20 e")] TKZ200 = 40,
        [Description("30 e")] TKZ300 = 60,
        [Description("40 e")] TKZ400 = 80,
        [Description("50 e")] TKZ500 = 100,
    }

    public enum ELV : Byte
    {
        [Description("滤波等级 LV0")] LV0 = 0,
        [Description("滤波等级 LV1")] LV1 = 1,
        [Description("滤波等级 LV2")] LV2 = 2,
        [Description("滤波等级 LV3")] LV3 = 3,
        [Description("滤波等级 LV4")] LV4 = 4,
        [Description("滤波等级 LV5")] LV5 = 5,
        [Description("滤波等级 LV6")] LV6 = 6,
        [Description("滤波等级 LV7")] LV7 = 7,
        [Description("滤波等级 LV8")] LV8 = 8,
        [Description("滤波等级 LV9")] LV9 = 9,
        [Description("滤波等级 LV10")] LV10 = 10,
    }

    public enum TIM : Byte
    {
        [Description("时间 0.1s")] TIM0_1 = 1,
        [Description("时间 0.2s")] TIM0_2 = 2,
        [Description("时间 0.3s")] TIM0_3 = 3,
        [Description("时间 0.4s")] TIM0_4 = 4,
        [Description("时间 0.5s")] TIM0_5 = 5,
        [Description("时间 0.6s")] TIM0_6 = 6,
        [Description("时间 0.7s")] TIM0_7 = 7,
        [Description("时间 0.8s")] TIM0_8 = 8,
        [Description("时间 0.9s")] TIM0_9 = 9,
        [Description("时间 1.0s")] TIM1_0 = 10,
        [Description("时间 1.1s")] TIM1_1 = 11,
        [Description("时间 1.2s")] TIM1_2 = 12,
        [Description("时间 1.3s")] TIM1_3 = 13,
        [Description("时间 1.4s")] TIM1_4 = 14,
        [Description("时间 1.5s")] TIM1_5 = 15,
        [Description("时间 1.6s")] TIM1_6 = 16,
        [Description("时间 1.7s")] TIM1_7 = 17,
        [Description("时间 1.8s")] TIM1_8 = 18,
        [Description("时间 1.9s")] TIM1_9 = 19,
        [Description("时间 2.0s")] TIM2_0 = 20,
        [Description("时间 2.1s")] TIM2_1 = 21,
        [Description("时间 2.2s")] TIM2_2 = 22,
        [Description("时间 2.3s")] TIM2_3 = 23,
        [Description("时间 2.4s")] TIM2_4 = 24,
        [Description("时间 2.5s")] TIM2_5 = 25,
        [Description("时间 2.6s")] TIM2_6 = 26,
        [Description("时间 2.7s")] TIM2_7 = 27,
        [Description("时间 2.8s")] TIM2_8 = 28,
        [Description("时间 2.9s")] TIM2_9 = 29,
        [Description("时间 3.0s")] TIM3_0 = 30,
    }

    public enum CHEAT : Byte
    {
        [Description("无AI跟踪")] CHEAT_null = 0,    //无作弊
        [Description("AI跟踪H级")] CHEAT_hundred = 1,    //整百追踪作弊
        [Description("AI跟踪S级")] CHEAT_thousand = 2,    //整千追踪作弊
        [Description("AI跟踪T级")] CHEAT_tenth = 3,    //整万追踪作弊
    }

    public enum COMP
    {
        [Description("Self-UART")] SelfUART,
        [Description("RS485 MODBUS RTU")] RS485,
        [Description("EtherNet MODBUS TCP")] ModbusTCP,
        [Description("CANopen")] CANopen,
        [Description("EtherCAT")] EtherCAT,
        [Description("EtherNet/IP")] EtherNetIP,
        [Description("PROFINET")] PROFINET,
        [Description("PROFIBUS")] PROFIBUS,
    }

    public enum BAUT : Int32
    {
        [Description("波特率1200")] B1200 = 1200,
        [Description("波特率2400")] B2400 = 2400,
        [Description("波特率4800")] B4800 = 4800,
        [Description("波特率9600")] B9600 = 9600,
        [Description("波特率14400")] B14400 = 14400,
        [Description("波特率19200")] B19200 = 19200,
        [Description("波特率38400")] B38400 = 38400,
        [Description("波特率57600")] B57600 = 57600,
        [Description("波特率115200")] B115200 = 115200,
    }

    //空中速率
    public enum AIRR : Byte
    {
        //0x05、0x06、0x07都对应168k的空中速率
        [Description("空中速率2.5k")] A2500 = 0x00,
        [Description("空中速率5k")] A5000 = 0x01,
        [Description("空中速率12k")] A12000 = 0x02,
        [Description("空中速率28k")] A28000 = 0x03,
        [Description("空中速率64k")] A64000 = 0x04,
        [Description("空中速率168k")] A168000_1 = 0x05,
        [Description("空中速率168k")] A168000_2 = 0x06,
        [Description("空中速率168k")] A168000_3 = 0x07,
    }

    public enum RXSTP : Byte //通讯接收状态机,找出帧头帧尾
    {
        //02 xx 01 03
        //02 xx .. xx 08 03
        //02 xx .. xx 29 03

        //02 80 80 80
        //02 80 80 80 80
        //03 80 80 80
        //03 80 80 80 80

        //=±123\r\n
        //=F123\r\n

        //ID 03 LEN ...... XL XH
        //ID 06 LEN ...... XL XH
        //ID 10 LEN ...... XL XH

        NUL, //null
        STX, //0x02 或 '=' 或 ID
        ACK, //0x29 或 '0x0D' 或 XL
        ETX, //0x03 或 '0x0A' 或 XH
    }

    public enum TASKS : Byte //任务状态机,根据任务选择指令,根据接口指令装帧发送,接收字节后根据状态解析数据,然后委托里根据状态执行下一个任务
    {
        NULL,       //null,必须放第一个

        //BohrCode
        BOR,        //读
        BCC,        //校验

        //读SCT
        RDX0,       //
        RDX1,       //
        RDX2,       //
        RDX3,       //
        RDX4,       //
        RDX5,       //
        RDX6,       //
        RDX7,       //
        RDX8,       //
        RDX9,       //

        //写SCT
        WRX0,       //
        WRX1,       //
        WRX2,       //
        WRX3,       //
        WRX4,       //
        WRX5,       //
        WRX6,       //
        WRX7,       //
        WRX8,       //
        WRX9,       //

        //采集
        ADCP1,      //ad_point1
        ADCP2,      //ad_point2
        ADCP3,      //ad_point3
        ADCP4,      //ad_point4
        ADCP5,      //ad_point5
        ADCP6,      //ad_point6
        ADCP7,      //ad_point7
        ADCP8,      //ad_point8
        ADCP9,      //ad_point9
        ADCP10,     //ad_point10
        ADCP11,     //ad_point11

        //
        GODMZ,      //矫正4mA减命令
        GOUPZ,      //矫正4mA加命令
        GODMM,      //矫正12mA减命令
        GOUPM,      //矫正12mA加命令
        GODMF,      //矫正20mA减命令
        GOUPF,      //矫正20mA加命令
        DONE,       //结束校准

        //
        TARE,       //扣重
        ZERO,       //归零
        SPAN,       //标定
        REST,       //重启

        //
        ADC,        //连续adcout =±123\r\n
        DAC,        //连续dacout 02 80 80 80 80
        QDAC,       //问答dacset =±123\r\n
        QNET,       //问答净重 =SN+123456.7kg\r\n
        QGROSS,     //问答毛重 =SG-  3456.7kg\r\n
        QEEPROM,    //问EEPROM版本

        //
        SFLT,       //设置滤波范围 =±123\r\n
        RFLT,       //重置滤波范围 =F123\r\n
        RDFT,       //读采样滤波范围 =F123\r\n

        //
        SCAN,       //RS485搜索站点
        RSBUF,      //RS485调试指令直接转成字符串显示

        //
        AZERO,      //广播指令,全部归零
        HWADDR,     //广播指令,有重量设置地址
        SWADDR,     //广播指令,检索重量设置地址

        //
        WEPRM,      //写eeprom参数
        REPRM,      //读eeprom参数

        //
        WNMT,       //写网络管理状态
        RNMT,       //读网络管理状态

        //
        WHEART,     //设定生产者心跳时间
    }

    public static class Constants
    {
        //Self-Uart

        public const Byte AUTA  = 0x61; //连续发送adcout
        public const Byte AUTD  = 0x62; //连续发送dacset
        public const Byte GODMZ = 0x63; //矫正4mA减命令
        public const Byte GOUPZ = 0x64; //矫正4mA加命令
        public const Byte GODMF = 0x65; //矫正20mA减命令
        public const Byte GOUPF = 0x66; //矫正20mA加命令
        public const Byte DONE  = 0x67; //结束矫正
        public const Byte DACO  = 0x68; //连续发送dacout
        public const Byte GODMM = 0x69; //矫正12mA减命令
        public const Byte GOUPM = 0x6A; //矫正12mA加命令
        public const Byte SFLT  = 0x71; //设置滤波范围
        public const Byte RFLT  = 0x72; //重置滤波范围
        public const Byte RDFT  = 0x73; //读滤波范围
        public const Byte CALS  = 0x90; //快速归零
        public const Byte CALZ  = 0x91; //标定零点
        public const Byte CALF  = 0x92; //标定满点
        public const Byte PONT  = 0x93; //取AD值

        public const Byte RDX0  = 0x94; //读SCT0
        public const Byte RDX1  = 0x95; //读SCT1
        public const Byte RDX2  = 0x96; //读SCT2
        public const Byte RDX3  = 0x97; //读SCT3
        public const Byte RDX4  = 0x98; //读SCT4
        public const Byte RDX5  = 0xA0; //读SCT5
        public const Byte RDX6  = 0xA5; //读SCT6
        public const Byte RDX7  = 0xA6; //读SCT7
        public const Byte RDX8  = 0xA7; //读SCT8
        public const Byte RDX9  = 0xA8; //读SCT9

        public const Byte WRX0  = 0x99; //写SCT0
        public const Byte WRX1  = 0x9A; //写SCT1
        public const Byte WRX2  = 0x9B; //写SCT2
        public const Byte WRX3  = 0x9C; //写SCT3
        public const Byte WRX4  = 0x9D; //写SCT4
        public const Byte WRX5  = 0xA1; //写SCT5
        public const Byte WRX6  = 0xA2; //写SCT6
        public const Byte WRX7  = 0xA3; //写SCT7
        public const Byte WRX8  = 0xA4; //写SCT8
        public const Byte WRX9  = 0xA9; //写SCT9

        public const Byte WEPRM = 0xA8; //写eeprom参数
        public const Byte REPRM = 0xA9; //读eeprom参数

        public const Byte REST  = 0xAA; //重启Reset
        public const Byte CODE  = 0xBB; //BohrCode

        public const Byte STAR  = 0x02; //起始符
        public const Byte STOP  = 0x03; //结束符

        public const Byte sLen  = 0x01; //长度
        public const Byte tLen  = 0x29; //长度
        public const Byte eLen  = 0x77; //长度
        public const Byte nLen  = 0x69; //SCT9长度

        //RS485 MODBUS

        public const UInt16 REG_RW_addr     = 0x0000;   // 0x0000  1byte //设置地址
        public const UInt16 REG_RW_baud     = 0x0100;   // 0x0001  1byte //设置波特率
        public const UInt16 REG_RW_stop     = 0x0200;   // 0x0002  1byte //设置停止位
        public const UInt16 REG_RW_parity   = 0x0300;   // 0x0003  1byte //设置校验位

        public const UInt16 REG_RW_output   = 0x1000;   // 0x0010  4byte //设置数字量满点值
        public const UInt16 REG_RW_decimal  = 0x1200;   // 0x0012  1byte //设置数字量小数点
        public const UInt16 REG_RW_unit     = 0x1300;   // 0x0013  1byte //设置数字量单位
        public const UInt16 REG_RW_ascii    = 0x1400;   // 0x0014  1byte //设置连续发送格式

        public const UInt16 REG_R_adcout    = 0x2000;   // 0x0020  4byte //读内码
        public const UInt16 REG_R_adasmple  = 0x2200;   // 0x0022  4byte //读滤波内码
        public const UInt16 REG_R_voltage   = 0x2400;   // 0x0024  4byte //读自带4位小数的电流电压
        public const UInt16 REG_R_output    = 0x2600;   // 0x0026  4byte //读数字量

        public const UInt16 REG_W_tare      = 0x3000;   // 0x0030  1byte //扣重
        public const UInt16 REG_W_zero      = 0x3100;   // 0x0031  1byte //归零
        public const UInt16 REG_W_span      = 0x3200;   // 0x0032  1byte //标定
        public const UInt16 REG_W_reset     = 0x3300;   // 0x0033  1byte //重启
        public const UInt16 REG_W_spset     = 0x3500;   // 0x0032  1byte //设置采样滤波范围
        public const UInt16 REG_W_spreset   = 0x3800;   // 0x0033  1byte //重置采样滤波范围

        public const UInt16 COM_R_version   = 0x90A0;   // 0xA090  1byte //version
        public const UInt16 COM_R_bohrcode  = 0x91A0;   // 0xA091  7byte //bohrcode,01 03 A0 91 00 07 XL XH

        public const UInt16 COM_R_SCT0      = 0x94A0;   // 0xA094  Xbyte //读SCT0
        public const UInt16 COM_R_SCT1      = 0x95A0;   // 0xA095  Xbyte //读SCT1
        public const UInt16 COM_R_SCT2      = 0x96A0;   // 0xA096  Xbyte //读SCT2
        public const UInt16 COM_R_SCT3      = 0x97A0;   // 0xA097  Xbyte //读SCT3
        public const UInt16 COM_R_SCT4      = 0x98A0;   // 0xA098  Xbyte //读SCT4
        public const UInt16 COM_R_SCT5      = 0xA0A0;   // 0xA0A0  Xbyte //读SCT5
        public const UInt16 COM_R_SCT6      = 0xA5A0;   // 0xA0A5  Xbyte //读SCT6
        public const UInt16 COM_R_SCT7      = 0xA6A0;   // 0xA0A6  Xbyte //读SCT7
        public const UInt16 COM_R_SCT8      = 0xA7A0;   // 0xA0A7  Xbyte //读SCT8
        public const UInt16 COM_R_SCT9      = 0xAAA0;   // 0xA0AA  Xbyte //读SCT9

        public const UInt16 COM_W_SCT0      = 0x99A0;   // 0xA099  Xbyte //写SCT0
        public const UInt16 COM_W_SCT1      = 0x9AA0;   // 0xA09A  Xbyte //写SCT1
        public const UInt16 COM_W_SCT2      = 0x9BA0;   // 0xA09B  Xbyte //写SCT2
        public const UInt16 COM_W_SCT3      = 0x9CA0;   // 0xA09C  Xbyte //写SCT3
        public const UInt16 COM_W_SCT4      = 0x9DA0;   // 0xA09D  Xbyte //写SCT4
        public const UInt16 COM_W_SCT5      = 0xA1A0;   // 0xA0A1  Xbyte //写SCT5
        public const UInt16 COM_W_SCT6      = 0xA2A0;   // 0xA0A2  Xbyte //写SCT6
        public const UInt16 COM_W_SCT7      = 0xA3A0;   // 0xA0A3  Xbyte //写SCT7
        public const UInt16 COM_W_SCT8      = 0xA4A0;   // 0xA0A4  Xbyte //写SCT8
        public const UInt16 COM_W_SCT9      = 0xABA0;   // 0xA0AB  Xbyte //写SCT9

        public const UInt16 COM_W_EPRM      = 0xA8A0;   // 0xA0A3  Xbyte //写eeprom参数
        public const UInt16 COM_R_EPRM      = 0xA9A0;   // 0xA0A4  Xbyte //读eeprom参数

        public const UInt16 COM_R_adcout    = 0x9AA1;   // 0xA19A  Xbyte //adcout,连续回复=±123\r\n
        public const UInt16 COM_R_dacset    = 0x9BA1;   // 0xA19B  Xbyte //dacset,连续回复=±123\r\n
        public const UInt16 COM_R_dacout    = 0x9CA1;   // 0xA19C  Xbyte //dacout,连续回复02 80 80 80

        public const UInt16 COM_R_weight    = 0x9DA1;   // 0xA19D  Xbyte //dacset,问答回复=±123\r\n
        public const UInt16 COM_R_net       = 0x9EA1;   // 0xA19E  Xbyte //AsciiNet,问答回复=SN+123456.7kg\r\n
        public const UInt16 COM_R_gross     = 0x9FA1;   // 0xA19F  Xbyte //AsciiGross,问答回复=SG+     6.7kg\r\n

        public const UInt16 COM_W_spset     = 0x71A2;   // 0xA271  Xbyte //设置采样滤波范围,回复=±123\r\n或=F123\r\n
        public const UInt16 COM_W_spreset   = 0x72A2;   // 0xA272  Xbyte //重置采样滤波范围,回复=±123\r\n或=F123\r\n
        public const UInt16 COM_R_spset     = 0x73A2;   // 0xA273  Xbyte //读采样滤波范围,回复=F123\r\n

        public const Byte RS485_READ        = 0X03;     //读
        public const Byte RS485_WRITE       = 0X06;     //写
        public const Byte RS485_Sequence    = 0X10;     //写

        //CANopen
        //参考R:\ProBohr\BD-Project\TCAN\Firmware\canopen_F4\canopen_config.h
        //功能码     
        public const UInt16 FunID_NMT       = 0x0 << 7; //0x000 网络管理
        public const UInt16 FunID_SYNC      = 0x1 << 7; //0x080 同步对象
        public const UInt16 FunID_EMCY      = 0x1 << 7; //0x080 + nodeID 应急对象
        public const UInt16 FunID_SRDO      = 0x2 << 7; //0x100 + nodeID * 2 - 1,COB_ID1;0x100 + nodeID * 2,COB_ID2
        public const UInt16 TPDO1           = 0x3 << 7; //0x180 + nodeID 过程数据对象
        public const UInt16 FunID_RSDO      = 0xB << 7; //0x580 + nodeID 服务数据对象 答
        public const UInt16 FunID_TSDO      = 0xC << 7; //0x600 + nodeID 服务数据对象 问
        public const UInt16 FunID_NMTERR    = 0xE << 7; //0x700 + nodeID 网络管理-错误控制报文 心跳回复NMTERR

        //网络管理命令
        public const Byte NMT_CS_Run        = 0x01;     //CS指令 启动命令      
        public const Byte NMT_CS_Stop       = 0x02;     //CS指令 停止命令      
        public const Byte NMT_CS_PRERUN     = 0x80;     //CS指令 进入预操作命令
        public const Byte NMT_CS_APP_RST    = 0x81;     //CS指令 复位节点应用层    
        public const Byte NMT_CS_COM_RST    = 0x82;     //CS指令 复位节点通讯

        //SDO命令符(Command specifier)
        public const Byte SDO_WR_4_Bytes    = 0x23;     //写4字节
        public const Byte SDO_WR_3_Bytes    = 0x27;     //写3字节
        public const Byte SDO_WR_2_Bytes    = 0x2B;     //写2字节
        public const Byte SDO_WR_1_Bytes    = 0x2F;     //写1字节  
        public const Byte SDO_WR_REPLY      = 0x60;     //写成功应答
        public const Byte SDO_RD_4_Bytes    = 0x43;     //读取响应4字节
        public const Byte SDO_RD_3_Bytes    = 0x47;     //读取响应3字节
        public const Byte SDO_RD_2_Bytes    = 0x4B;     //读取响应2字节
        public const Byte SDO_RD_1_Bytes    = 0x4F;     //读取响应1字节
        public const Byte SDO_RD_Request    = 0x40;     //读取请求

        //BT(Block transfer) 块传输
        public const Byte SDO_BLK_StaReq    = 0xC6;     //CS写指令 块下载启动请求,块上传启动应答
        public const Byte SDO_BLK_StaAck    = 0xA4;     //CS写指令 块下载启动应答,块上传启动请求
        public const Byte SDO_BLK_StaUp     = 0xA3;     //CS写指令 块上传开始
        public const Byte SDO_BLK_TraAck    = 0xA2;     //CS写指令 块下载或块上传确认
        public const Byte SDO_BLK_EndAck    = 0xA1;     //结束
        public const Byte SDO_BLK_END_SCT   = 0xD5;     //结束，有校验和数据，SCT0-8
        public const Byte SDO_BLK_END_SCT9  = 0xD1;     //结束，有校验和数据，SCT9
        public const Byte SDO_BLK_BYTE_SCT  = 0x2C;     //SCT0-8的字节数
        public const Byte SDO_BLK_BYTE_SCT9 = 0x6C;     //SCT9的字节数
        public const Byte SDO_BLK_SIZE_SCT  = 0x07;     //SCT0-8的段数
        public const Byte SDO_BLK_SIZE_SCT9 = 0x10;     //SCT9的段数

        //数据字典
        public const UInt16 Index_HEARTBEAT      = 0x1017;   //生产者心跳时间
        public const UInt16 Index_TCAN_RAD_Adc   = 0x2203;   //读ADC值
        public const UInt16 Index_REST           = 0x2204;   //复位
        public const UInt16 Index_TCAN_GET_Code  = 0x2205;   //读bohrcode，子索引0-1
        public const UInt16 Index_SDO_BLOCK      = 0x2300;   //块传输
        public const UInt16 Index_MEAS_WEIGHT    = 0x6000;   //测量压力值
        public const UInt16 Index_MEAS_AN1       = 0x6100;   //AN1电桥的AD值(24bit)
        public const UInt16 Index_MEAS_AN1_ZERO  = 0x6200;   //归零
        public const Byte Sub_Index_00      = 0x00;     //子索引0
        public const Byte Sub_Index_01      = 0x01;     //子索引1
        public const Byte Sub_Index_02      = 0x02;     //子索引2
        public const Byte Sub_Index_03      = 0x03;     //子索引3
        public const Byte Sub_Index_04      = 0x04;     //子索引4
        public const Byte Sub_Index_05      = 0x05;     //子索引5
        public const Byte Sub_Index_06      = 0x06;     //子索引6
        public const Byte Sub_Index_07      = 0x07;     //子索引7
        public const Byte Sub_Index_08      = 0x08;     //子索引8
        public const Byte Sub_Index_09      = 0x09;     //子索引9 

        //NMTERR报文
        public const Byte NMTERR_start      = 0x00;     //NMTERR报文,1个启动心跳消息
        public const Byte NMTERR_stop       = 0x04;     //NMTERR报文,停止状态
        public const Byte NMTERR_run        = 0x05;     //NMTERR报文,操作状态
        public const Byte NMTERR_pre_run    = 0x7F;     //NMTERR报文,预操作状态
    }

    public enum STATE : Byte
    {
        [Description("未找到")] INVALID,   //无效,未初始化设备,不需要在界面展示设备信息和数据
        [Description("已连接")] CONNECTED, //已连接,SCAN和BOR成功的设备,可以读SCT参数和其它操作
        [Description("工作中")] WORKING,   //正常工作,RDX完成的设备,可以刷新界面设备信息和数据
        [Description("已掉线")] OFFLINE,   //掉线,刷新界面数据时没有通讯反应,不更新界面,也不调整界面
    }
}
