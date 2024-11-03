using System;

//未经过审批不得改动

//Alvin 20230414

//XET变量用于管理设备的状态和非SCT参数
//XET方法用于各种计算设备数据

//E_开头的是SCT变量
//T_开头的是SCT衍生变量,和Textbox交换数据,自动更新SCT0-8参数
//S_开头的是SCT衍生变量,从SCT中换算出来的设备型号,输出类型,小数点,单位等
//R_开头的是设备实时数据变量,sTATE,接收解码的设备重量,稳定,净毛重,adcout等

namespace Model
{
    public partial class XET : SCT
    {
        //设备信息
        public STATE sTATE = STATE.INVALID;         //设备连接和工作状态
        public ECVEType ecveType = ECVEType.Single; //标定单程进程返程状态

        //设备参数
        public Int64 R_bohrcode_long;       //设备UID,在Protocol_mePort_ReceiveBohrCode更新
        public Int32 R_adcout;              //显示adcout,在Protocol_mePort_ReceiveLong更新
        public Int32 R_dacset;              //显示dacset,在Protocol_mePort_ReceiveLong更新
        public Boolean R_isFLT;             //是否接收到设置Lv0滤波范围的回复,在Protocol_mePort_ReceiveLong更新
        public Boolean R_stable;            //显示稳定符号,在Protocol_mePort_ReceiveAscii更新
        public Boolean R_overload;          //显示超载,在Protocol_mePort_ReceiveAscii更新
        public Boolean R_eeplink;           //是否有外部EEPROM芯片连接,在Protocol_mePort_ReceiveDacout,Protocol_mePort_ReceiveAscii更新
        public String R_grossnet;           //显示"毛重"或"净重",在Protocol_mePort_ReceiveAscii更新
        public double R_datum;              //显示的模拟量或数字量浮点值,在Protocol_mePort_ReceiveAscii更新
        public String R_weight;             //显示的数字量,在Protocol_mePort_ReceiveAscii更新
        public String R_output;             //显示的模拟量,在Protocol_mePort_ReceiveDacout更新
        public Byte R_nmterr;               //NMT_CS，网络管理状态 ，在Protocol_mePort_ReceiveNMTERR更新
        public String R_ipAddr;             //设备的ip地址,在Protocol_mePort_ReceiveBohrCode()、Protocol_mePort_ReceiveModbusTCPScan()更新

        //设备自检
        public Boolean R_checklink;         //触发外部EEPROM芯片连接提示
        public String R_eepversion;         //外部EEPROM数据版本
        public String R_errSensor;          //标定自检,在RefreshRatio更新
        public String R_resolution;         //标定分辨率,在RefreshRatio更新
    }
}