using System;

//未经过审批不得改动

//Alvin 20230414
//Junzhe 20231124
//Lumi 20240105

/******************************************************************************/

//参考<变送器重定义SCT20230313.c>

//规定以下内容:

//产品型号和输出类型
//模拟量计算
//数字量计算
//初始值原则
//操作的读写解释

/******************************************************************************/

//变量命名,定义顺序,作用注释, 都要求全部统一

//SCT管理设备SCT0 - SCT8参数以及SCT衍生换算参数

//input和string的传值,需要同步更新ad_point和ad_zero/ad_full
//analog和string的传值,需要同步更新da_point和da_zero/da_full
//wt_zero/wt_full和string的传值

//根据设备类型S_DeviceType和S_OutType使用其中的参数

//设备操作必须先读出所有SCT数据,做到电脑数据和设备同步

//修改参数必须先BCC校验再写入,做到写入保护

/******************************************************************************/

namespace Model
{
    //EEPROM数据块使用
    public partial class SCT
    {
        //SCT0
        private Byte e_test;            // 0x02 固件版本
        private Byte e_outype;          // 0x03 输出类型420mA,0-5V,0-10V,±5V,±10V
        private Byte e_curve;           // 0x04 线性拟合方式
        private Byte e_adspeed;         // 0x05 灵敏度档位和AD速率10Hz,40Hz,640Hz,1280Hz
        private Byte e_autozero;        // 0x06 上电归零范围
        private Byte e_trackzero;       // 0x07 零点跟踪范围
        private Int32 e_checkhigh;      // 0x08 检重上限
        private Int32 e_checklow;       // 0x0C 检重下限
        private UInt32 e_mfg_date;      // 0x10 制造首次测试时间,1612310830=2016年12月31号08点30分//42年12月31日 23:59 之后又不能用了
        private Int32 e_mfg_srno;       // 0x14 制造分配测试序列号,Test Log file name = TYPE-mfg_date-mfg_srno = BE420L-1612310830-srno
        private Int32 e_tmp_min;        // 0x18 工作最低温度记录
        private Int32 e_tmp_max;        // 0x1C 工作最高温度记录
        private Int32 e_tmp_cal;        // 0x20 等效25度AD值
        private Int32 e_bohrcode;       // 0x24 校验PCBA的UID
        private Byte e_enspan;          // 0x28 禁止SPAN
        private Byte e_protype;         // 0x29 产品类型,outype升级成protype+outype

        #region set SCT0 and get SCT0

        public Byte E_test
        {
            set
            {
                e_test = value;
            }
            get
            {
                return e_test;
            }
        }
        public Byte E_outype
        {
            set
            {
                e_outype = value;
            }
            get
            {
                return e_outype;
            }
        }
        public Byte E_curve
        {
            set
            {
                e_curve = value;
            }
            get
            {
                return e_curve;
            }
        }
        public Byte E_adspeed
        {
            set
            {
                e_adspeed = value;
            }
            get
            {
                return e_adspeed;
            }
        }
        public Byte E_autozero
        {
            set
            {
                e_autozero = value;
            }
            get
            {
                return e_autozero;
            }
        }
        public Byte E_trackzero
        {
            set
            {
                e_trackzero = value;
            }
            get
            {
                return e_trackzero;
            }
        }
        public Int32 E_checkhigh
        {
            set
            {
                e_checkhigh = value;
            }
            get
            {
                return e_checkhigh;
            }
        }
        public Int32 E_checklow
        {
            set
            {
                e_checklow = value;
            }
            get
            {
                return e_checklow;
            }
        }
        public UInt32 E_mfg_date
        {
            set
            {
                e_mfg_date = value;
            }
            get
            {
                return e_mfg_date;
            }
        }
        public Int32 E_mfg_srno
        {
            set
            {
                e_mfg_srno = value;
            }
            get
            {
                return e_mfg_srno;
            }
        }
        public Int32 E_tmp_min
        {
            set
            {
                e_tmp_min = value;
            }
            get
            {
                return e_tmp_min;
            }
        }
        public Int32 E_tmp_max
        {
            set
            {
                e_tmp_max = value;
            }
            get
            {
                return e_tmp_max;
            }
        }
        public Int32 E_tmp_cal
        {
            set
            {
                e_tmp_cal = value;
            }
            get
            {
                return e_tmp_cal;
            }
        }
        public Int32 E_bohrcode
        {
            set
            {
                e_bohrcode = value;
            }
            get
            {
                return e_bohrcode;
            }
        }
        public Byte E_enspan
        {
            set
            {
                e_enspan = value;
            }
            get
            {
                return e_enspan;
            }
        }
        public Byte E_protype
        {
            set
            {
                e_protype = value;
            }
            get
            {
                return e_protype;
            }
        }

        #endregion

        //SCT1
        private Int32 e_ad_point1;      // 0x02 输入adc内码
        private Int32 e_ad_point2;      // 0x06 输入adc内码
        private Int32 e_ad_point3;      // 0x0A 输入adc内码
        private Int32 e_ad_point4;      // 0x0E 输入adc内码
        private Int32 e_ad_point5;      // 0x12 输入adc内码
        private Int32 e_da_point1;      // 0x16 输出dac值或数字量
        private Int32 e_da_point2;      // 0x1A 输出dac值或数字量
        private Int32 e_da_point3;      // 0x1E 输出dac值或数字量
        private Int32 e_da_point4;      // 0x22 输出dac值或数字量
        private Int32 e_da_point5;      // 0x26 输出dac值或数字量

        #region set SCT1 and get SCT1

        public Int32 E_ad_point1
        {
            set
            {
                e_ad_point1 = value;
            }
            get
            {
                return e_ad_point1;
            }
        }
        public Int32 E_ad_point2
        {
            set
            {
                e_ad_point2 = value;
            }
            get
            {
                return e_ad_point2;
            }
        }
        public Int32 E_ad_point3
        {
            set
            {
                e_ad_point3 = value;
            }
            get
            {
                return e_ad_point3;
            }
        }
        public Int32 E_ad_point4
        {
            set
            {
                e_ad_point4 = value;
            }
            get
            {
                return e_ad_point4;
            }
        }
        public Int32 E_ad_point5
        {
            set
            {
                e_ad_point5 = value;
            }
            get
            {
                return e_ad_point5;
            }
        }
        public Int32 E_da_point1
        {
            set
            {
                e_da_point1 = value;
            }
            get
            {
                return e_da_point1;
            }
        }
        public Int32 E_da_point2
        {
            set
            {
                e_da_point2 = value;
            }
            get
            {
                return e_da_point2;
            }
        }
        public Int32 E_da_point3
        {
            set
            {
                e_da_point3 = value;
            }
            get
            {
                return e_da_point3;
            }
        }
        public Int32 E_da_point4
        {
            set
            {
                e_da_point4 = value;
            }
            get
            {
                return e_da_point4;
            }
        }
        public Int32 E_da_point5
        {
            set
            {
                e_da_point5 = value;
            }
            get
            {
                return e_da_point5;
            }
        }

        #endregion

        //SCT2
        private Int32 e_input1;         // 0x02	输入灵敏度
        private Int32 e_input2;         // 0x06 输入灵敏度
        private Int32 e_input3;         // 0x0A 输入灵敏度
        private Int32 e_input4;         // 0x0E 输入灵敏度
        private Int32 e_input5;         // 0x12 输入灵敏度
        private Int32 e_analog1;        // 0x16 输出的模拟量数字量
        private Int32 e_analog2;        // 0x1A 输出的模拟量数字量
        private Int32 e_analog3;        // 0x1E 输出的模拟量数字量
        private Int32 e_analog4;        // 0x22 输出的模拟量数字量
        private Int32 e_analog5;        // 0x26 输出的模拟量数字量

        #region set SCT2 and get SCT2

        public Int32 E_input1
        {
            set
            {
                e_input1 = value;
            }
            get
            {
                return e_input1;
            }
        }
        public Int32 E_input2
        {
            set
            {
                e_input2 = value;
            }
            get
            {
                return e_input2;
            }
        }
        public Int32 E_input3
        {
            set
            {
                e_input3 = value;
            }
            get
            {
                return e_input3;
            }
        }
        public Int32 E_input4
        {
            set
            {
                e_input4 = value;
            }
            get
            {
                return e_input4;
            }
        }
        public Int32 E_input5
        {
            set
            {
                e_input5 = value;
            }
            get
            {
                return e_input5;
            }
        }
        public Int32 E_analog1
        {
            set
            {
                e_analog1 = value;
            }
            get
            {
                return e_analog1;
            }
        }
        public Int32 E_analog2
        {
            set
            {
                e_analog2 = value;
            }
            get
            {
                return e_analog2;
            }
        }
        public Int32 E_analog3
        {
            set
            {
                e_analog3 = value;
            }
            get
            {
                return e_analog3;
            }
        }
        public Int32 E_analog4
        {
            set
            {
                e_analog4 = value;
            }
            get
            {
                return e_analog4;
            }
        }
        public Int32 E_analog5
        {
            set
            {
                e_analog5 = value;
            }
            get
            {
                return e_analog5;
            }
        }

        #endregion

        //SCT3
        private Int32 e_ad_zero;        // 0x02 零点内码
        private Int32 e_ad_full;        // 0x06 满点内码
        private Int32 e_da_zero;        // 0x0A 零点dac或数字量
        private Int32 e_da_full;        // 0x0E 满点dac或数字量
        private Int32 e_vtio;           // 0x12 两点标定斜率
        private Int32 e_wtio;           // 0x16 多点标定拟合斜率
        private Int32 e_atio;           // 0x1A 5/11点标定分段斜率
        private Int32 e_btio;           // 0x1E 5/11点标定分段斜率
        private Int32 e_ctio;           // 0x22 5/11点标定分段斜率
        private Int32 e_dtio;           // 0x26 5/11点标定分段斜率

        #region set SCT3 and get SCT3

        public Int32 E_ad_zero
        {
            set
            {
                e_ad_zero = value;
            }
            get
            {
                return e_ad_zero;
            }
        }
        public Int32 E_ad_full
        {
            set
            {
                e_ad_full = value;
            }
            get
            {
                return e_ad_full;
            }
        }
        public Int32 E_da_zero
        {
            set
            {
                e_da_zero = value;
            }
            get
            {
                return e_da_zero;
            }
        }
        public Int32 E_da_full
        {
            set
            {
                e_da_full = value;
            }
            get
            {
                return e_da_full;
            }
        }
        public Int32 E_vtio
        {
            set
            {
                e_vtio = value;
            }
            get
            {
                return e_vtio;
            }
        }
        public Int32 E_wtio
        {
            set
            {
                e_wtio = value;
            }
            get
            {
                return e_wtio;
            }
        }
        public Int32 E_atio
        {
            set
            {
                e_atio = value;
            }
            get
            {
                return e_atio;
            }
        }
        public Int32 E_btio
        {
            set
            {
                e_btio = value;
            }
            get
            {
                return e_btio;
            }
        }
        public Int32 E_ctio
        {
            set
            {
                e_ctio = value;
            }
            get
            {
                return e_ctio;
            }
        }
        public Int32 E_dtio
        {
            set
            {
                e_dtio = value;
            }
            get
            {
                return e_dtio;
            }
        }

        #endregion

        //SCT4
        private Int32 e_da_zero_4ma;    // 0x02 4-20mA zero
        private Int32 e_da_full_20ma;   // 0x06 4-20mA full
        private Int32 e_da_zero_05V;    // 0x0A 0-5V zero
        private Int32 e_da_full_05V;    // 0x0A 0-5V zero
        private Int32 e_da_zero_10V;    // 0x12 0-10V zero
        private Int32 e_da_full_10V;    // 0x16 0-10V full
        private Int32 e_da_zero_N5;     // 0x1A ±5V zero
        private Int32 e_da_full_P5;     // 0x1E ±5V full
        private Int32 e_da_zero_N10;    // 0x22 ±10V zero
        private Int32 e_da_full_P10;    // 0x26 ±10V full

        #region set SCT4 and get SCT4

        public Int32 E_da_zero_4ma
        {
            set
            {
                e_da_zero_4ma = value;
            }
            get
            {
                return e_da_zero_4ma;
            }
        }
        public Int32 E_da_full_20ma
        {
            set
            {
                e_da_full_20ma = value;
            }
            get
            {
                return e_da_full_20ma;
            }
        }
        public Int32 E_da_zero_05V
        {
            set
            {
                e_da_zero_05V = value;
            }
            get
            {
                return e_da_zero_05V;
            }
        }
        public Int32 E_da_full_05V
        {
            set
            {
                e_da_full_05V = value;
            }
            get
            {
                return e_da_full_05V;
            }
        }
        public Int32 E_da_zero_10V
        {
            set
            {
                e_da_zero_10V = value;
            }
            get
            {
                return e_da_zero_10V;
            }
        }
        public Int32 E_da_full_10V
        {
            set
            {
                e_da_full_10V = value;
            }
            get
            {
                return e_da_full_10V;
            }
        }
        public Int32 E_da_zero_N5
        {
            set
            {
                e_da_zero_N5 = value;
            }
            get
            {
                return e_da_zero_N5;
            }
        }
        public Int32 E_da_full_P5
        {
            set
            {
                e_da_full_P5 = value;
            }
            get
            {
                return e_da_full_P5;
            }
        }
        public Int32 E_da_zero_N10
        {
            set
            {
                e_da_zero_N10 = value;
            }
            get
            {
                return e_da_zero_N10;
            }
        }
        public Int32 E_da_full_P10
        {
            set
            {
                e_da_full_P10 = value;
            }
            get
            {
                return e_da_full_P10;
            }
        }

        #endregion

        //SCT5
        private Int32 e_corr;           // 0x02 对CS1237的校准系数
        private Byte e_mark;            // 0x06 标记是否CS1237校准
        private Byte e_sign;            // 0x07 设备通讯校验类型
        private Byte e_addr;            // 0x08 设备通讯地址
        private Byte e_baud;            // 0x09 通讯波特率
        private Byte e_stopbit;         // 0x0A 通讯停止位
        private Byte e_parity;          // 0x0B 通讯校验位
        private Int32 e_wt_zero;        // 0x0C 数字量量程
        private Int32 e_wt_full;        // 0x10 数字量量程
        private Byte e_wt_decimal;      // 0x14 数字量小数点
        private Byte e_wt_unit;         // 0x15 数字量单位
        private Byte e_wt_ascii;        // 0x16 数字量连续发送格式
        private Byte e_wt_sptime;       // 0x17 稳定次数
        private Byte e_wt_spfilt;       // 0x18 滤波深度
        private Byte e_wt_division;     // 0x19 数字量分度值
        private Byte e_wt_antivib;      // 0x1A 抗振动滤波等级
        private UInt16 e_heartBeat;     // 0x1B CANopen生产者者心跳时间
        private Byte e_typeTPDO0;       // 0x1D CANopen的TPO传输类型
        private UInt16 e_evenTPDO0;     // 0x1E CANopen的TPDO触发时间间隔
        private Byte e_nodeID;          // 0x20 CANopen节点ID
        private Byte e_nodeBaud;        // 0x21 CANopen节点波特率
        private Byte e_dynazero;        // 0x22 动态跟踪范围
        private Byte e_cheatype;        // 0x23 作弊类型
        private Byte e_thmax;           // 0x24 外阈值1~9,思考1~20
        private Byte e_thmin;           // 0x25 内阈值0~8
        private Byte e_stablerange;     // 0x26 读稳定数字量范围
        private Byte e_stabletime;      // 0x27 读稳定数字量时间
        private Byte e_tkzerotime;      // 0x28 零点跟踪时间
        private Byte e_tkdynatime;      // 0x29 动态跟踪时间

        #region set SCT5 and get SCT5

        public Int32 E_corr
        {
            set
            {
                e_corr = value;
            }
            get
            {
                return e_corr;
            }
        }
        public Byte E_mark
        {
            set
            {
                e_mark = value;
            }
            get
            {
                return e_mark;
            }
        }
        public Byte E_sign
        {
            set
            {
                e_sign = value;
            }
            get
            {
                return e_sign;
            }
        }
        public Byte E_addr
        {
            set
            {
                e_addr = value;
            }
            get
            {
                return e_addr;
            }
        }
        public Byte E_baud
        {
            set
            {
                e_baud = value;
            }
            get
            {
                return e_baud;
            }
        }
        public Byte E_stopbit
        {
            set
            {
                e_stopbit = value;
            }
            get
            {
                return e_stopbit;
            }
        }
        public Byte E_parity
        {
            set
            {
                e_parity = value;
            }
            get
            {
                return e_parity;
            }
        }
        public Int32 E_wt_zero
        {
            set
            {
                e_wt_zero = value;
            }
            get
            {
                return e_wt_zero;
            }
        }
        public Int32 E_wt_full
        {
            set
            {
                e_wt_full = value;
            }
            get
            {
                return e_wt_full;
            }
        }
        public Byte E_wt_decimal
        {
            set
            {
                e_wt_decimal = value;
            }
            get
            {
                return e_wt_decimal;
            }
        }
        public Byte E_wt_unit
        {
            set
            {
                e_wt_unit = value;
            }
            get
            {
                return e_wt_unit;
            }
        }
        public Byte E_wt_ascii
        {
            set
            {
                e_wt_ascii = value;
            }
            get
            {
                return e_wt_ascii;
            }
        }
        public Byte E_wt_sptime
        {
            set
            {
                e_wt_sptime = value;
            }
            get
            {
                return e_wt_sptime;
            }
        }
        public Byte E_wt_spfilt
        {
            set
            {
                e_wt_spfilt = value;
            }
            get
            {
                return e_wt_spfilt;
            }
        }
        public Byte E_wt_division
        {
            set
            {
                e_wt_division = value;
            }
            get
            {
                return e_wt_division;
            }
        }
        public Byte E_wt_antivib
        {
            set
            {
                e_wt_antivib = value;
            }
            get
            {
                return e_wt_antivib;
            }
        }
        public UInt16 E_heartBeat
        {
            set
            {
                e_heartBeat = value;
            }
            get
            {
                return e_heartBeat;
            }
        }
        public Byte E_typeTPDO0
        {
            set
            {
                e_typeTPDO0 = value;
            }
            get
            {
                return e_typeTPDO0;
            }
        }
        public UInt16 E_evenTPDO0
        {
            set
            {
                e_evenTPDO0 = value;
            }
            get
            {
                return e_evenTPDO0;
            }
        }
        public Byte E_nodeID
        {
            set
            {
                e_nodeID = value;
            }
            get
            {
                return e_nodeID;
            }
        }
        public Byte E_nodeBaud
        {
            set
            {
                e_nodeBaud = value;
            }
            get
            {
                return e_nodeBaud;
            }
        }
        public Byte E_dynazero
        {
            set
            {
                e_dynazero = value;
            }
            get
            {
                return e_dynazero;
            }
        }
        public Byte E_cheatype
        {
            set
            {
                e_cheatype = value;
            }
            get
            {
                return e_cheatype;
            }
        }
        public Byte E_thmax
        {
            set
            {
                e_thmax = value;
            }
            get
            {
                return e_thmax;
            }
        }
        public Byte E_thmin
        {
            set
            {
                e_thmin = value;
            }
            get
            {
                return e_thmin;
            }
        }
        public Byte E_stablerange
        {
            set
            {
                e_stablerange = value;
            }
            get
            {
                return e_stablerange;
            }
        }
        public Byte E_stabletime
        {
            set
            {
                e_stabletime = value;
            }
            get
            {
                return e_stabletime;
            }
        }
        public Byte E_tkzerotime
        {
            set
            {
                e_tkzerotime = value;
            }
            get
            {
                return e_tkzerotime;
            }
        }
        public Byte E_tkdynatime
        {
            set
            {
                e_tkdynatime = value;
            }
            get
            {
                return e_tkdynatime;
            }
        }

        #endregion

        //SCT6
        private Int32 e_ad_point6;      // 0x02 输入adc内码
        private Int32 e_ad_point7;      // 0x06 输入adc内码
        private Int32 e_ad_point8;      // 0x0A 输入adc内码
        private Int32 e_ad_point9;      // 0x0E 输入adc内码
        private Int32 e_ad_point10;     // 0x12 输入adc内码
        private Int32 e_da_point6;      // 0x16 输出dac值或数字量
        private Int32 e_da_point7;      // 0x1A 输出dac值或数字量
        private Int32 e_da_point8;      // 0x1E 输出dac值或数字量
        private Int32 e_da_point9;      // 0x22 输出dac值或数字量
        private Int32 e_da_point10;     // 0x26 输出dac值或数字量

        #region set SCT6 and get SCT6
        public Int32 E_ad_point6
        {
            set
            {
                e_ad_point6 = value;
            }
            get
            {
                return e_ad_point6;
            }
        }
        public Int32 E_ad_point7
        {
            set
            {
                e_ad_point7 = value;
            }
            get
            {
                return e_ad_point7;
            }
        }
        public Int32 E_ad_point8
        {
            set
            {
                e_ad_point8 = value;
            }
            get
            {
                return e_ad_point8;
            }
        }
        public Int32 E_ad_point9
        {
            set
            {
                e_ad_point9 = value;
            }
            get
            {
                return e_ad_point9;
            }
        }
        public Int32 E_ad_point10
        {
            set
            {
                e_ad_point10 = value;
            }
            get
            {
                return e_ad_point10;
            }
        }
        public Int32 E_da_point6
        {
            set
            {
                e_da_point6 = value;
            }
            get
            {
                return e_da_point6;
            }
        }
        public Int32 E_da_point7
        {
            set
            {
                e_da_point7 = value;
            }
            get
            {
                return e_da_point7;
            }
        }
        public Int32 E_da_point8
        {
            set
            {
                e_da_point8 = value;
            }
            get
            {
                return e_da_point8;
            }
        }
        public Int32 E_da_point9
        {
            set
            {
                e_da_point9 = value;
            }
            get
            {
                return e_da_point9;
            }
        }
        public Int32 E_da_point10
        {
            set
            {
                e_da_point10 = value;
            }
            get
            {
                return e_da_point10;
            }
        }

        #endregion

        //SCT7
        private Int32 e_input6;         // 0x02 输入灵敏度
        private Int32 e_input7;         // 0x06 输入灵敏度
        private Int32 e_input8;         // 0x0A 输入灵敏度
        private Int32 e_input9;         // 0x0E 输入灵敏度
        private Int32 e_input10;        // 0x12 输入灵敏度
        private Int32 e_analog6;        // 0x16 输出的模拟量数字量
        private Int32 e_analog7;        // 0x1A 输出的模拟量数字量
        private Int32 e_analog8;        // 0x1E 输出的模拟量数字量
        private Int32 e_analog9;        // 0x22 输出的模拟量数字量
        private Int32 e_analog10;       // 0x26 输出的模拟量数字量

        #region set SCT7 and get SCT7

        public Int32 E_input6
        {
            set
            {
                e_input6 = value;
            }
            get
            {
                return e_input6;
            }
        }
        public Int32 E_input7
        {
            set
            {
                e_input7 = value;
            }
            get
            {
                return e_input7;
            }
        }
        public Int32 E_input8
        {
            set
            {
                e_input8 = value;
            }
            get
            {
                return e_input8;
            }
        }
        public Int32 E_input9
        {
            set
            {
                e_input9 = value;
            }
            get
            {
                return e_input9;
            }
        }
        public Int32 E_input10
        {
            set
            {
                e_input10 = value;
            }
            get
            {
                return e_input10;
            }
        }
        public Int32 E_analog6
        {
            set
            {
                e_analog6 = value;
            }
            get
            {
                return e_analog6;
            }
        }
        public Int32 E_analog7
        {
            set
            {
                e_analog7 = value;
            }
            get
            {
                return e_analog7;
            }
        }
        public Int32 E_analog8
        {
            set
            {
                e_analog8 = value;
            }
            get
            {
                return e_analog8;
            }
        }
        public Int32 E_analog9
        {
            set
            {
                e_analog9 = value;
            }
            get
            {
                return e_analog9;
            }
        }
        public Int32 E_analog10
        {
            set
            {
                e_analog10 = value;
            }
            get
            {
                return e_analog10;
            }
        }

        #endregion

        //SCT8
        private Int32 e_ad_point11;     // 0x02 输入adc内码
        private Int32 e_da_point11;     // 0x06 输出dac值或数字量
        private Int32 e_input11;        // 0x0A 输入灵敏度
        private Int32 e_analog11;       // 0x0E 输出的模拟量数字量
        private Int32 e_etio;           // 0x12 11点标定分段斜率
        private Int32 e_ftio;           // 0x16 11点标定分段斜率
        private Int32 e_gtio;           // 0x1A 11点标定分段斜率
        private Int32 e_htio;           // 0x1E 11点标定分段斜率
        private Int32 e_itio;           // 0x22 11点标定分段斜率
        private Int32 e_jtio;           // 0x26 11点标定分段斜率

        #region set SCT8 and get SCT8

        public Int32 E_ad_point11
        {
            set
            {
                e_ad_point11 = value;
            }
            get
            {
                return e_ad_point11;
            }
        }
        public Int32 E_da_point11
        {
            set
            {
                e_da_point11 = value;
            }
            get
            {
                return e_da_point11;
            }
        }
        public Int32 E_input11
        {
            set
            {
                e_input11 = value;
            }
            get
            {
                return e_input11;
            }
        }
        public Int32 E_analog11
        {
            set
            {
                e_analog11 = value;
            }
            get
            {
                return e_analog11;
            }
        }
        public Int32 E_etio
        {
            set
            {
                e_etio = value;
            }
            get
            {
                return e_etio;
            }
        }
        public Int32 E_ftio
        {
            set
            {
                e_ftio = value;
            }
            get
            {
                return e_ftio;
            }
        }
        public Int32 E_gtio
        {
            set
            {
                e_gtio = value;
            }
            get
            {
                return e_gtio;
            }
        }
        public Int32 E_htio
        {
            set
            {
                e_htio = value;
            }
            get
            {
                return e_htio;
            }
        }
        public Int32 E_itio
        {
            set
            {
                e_itio = value;
            }
            get
            {
                return e_itio;
            }
        }
        public Int32 E_jtio
        {
            set
            {
                e_jtio = value;
            }
            get
            {
                return e_jtio;
            }
        }

        #endregion

        //SCT9
        private Byte e_enGFC;           // 0x02 GFC有效
        private Byte e_enSRDO;          // 0x03 信息方向
        private UInt16 e_SCT_time;      // 0x04 安全保障周期时间
        private UInt16 e_COB_ID1;       // 0x06 COB_ID1,正常数据COBID
        private UInt16 e_COB_ID2;       // 0x08 COB_ID2,取反数据COBID
        private Byte e_enOL;            // 0x0A 超载报警，TRUE超载有效
        private Byte e_overload;        // 0x0B 超载值，超载范围100-200%,超载报警百分比
        private Byte e_alarmMode;       // 0x0C 报警选择，读报警模式
        private Int32 e_wetTarget;      // 0x0D 目标报警,目标值数值
        private Int32 e_wetLow;         // 0x11 区间低报警值
        private Int32 e_wetHigh;        // 0x15 区间高报警值
        private Int32 e_filter;         // 0x19 设置滤波范围
        private UInt16 e_netServicePort;// 0x1D 目标主机端口号
        private Byte[] e_netServiceIP = new Byte[4]; // 0x1F 目标主机IP地址
        private Byte[] e_netClientIP = new Byte[4];  // 0x23 本机IP地址
        private Byte[] e_netGatIP = new Byte[4];     // 0x27 本机网关地址
        private Byte[] e_netMaskIP = new Byte[4];    // 0x2B 本机子网掩码
        private Byte e_useDHCP;         // 0x2F 使用DHCP方式 否则固定本机IP地址
        private Byte e_useScan;         // 0x30 使用Scan方式 否则固定本机IP地址
        private Byte[] e_addrRF = new Byte[2];        // 0x31 无线通讯地址,[0];ADDH [1]:ADDL
        private Byte e_spedRF;          // 0x33 bit 6、7:串口检验位  3、4、5:波特率  0、1、2:空中速率
        private Byte e_chanRF;          // 0x34 包长设定、通信信道
        private Byte e_optionRF;        // 0x35 无线通讯选项 参考:R:\ProBohr\BD-Project\RecXF\Component\RF模块\E70-433TxxSx_UserManual_CN_v1.0.pdf
        private UInt16 e_lockTPDO0;     // 0x36 TPDO0锁定时间（单位100us）
        private Byte e_entrTPDO0;       // 0x38 TPDO0兼容性条目
        private Byte e_typeTPDO1;       // 0x39 TPDO1传输类型
        private UInt16 e_lockTPDO1;     // 0x3A TPDO1锁定时间（单位100us）
        private Byte e_entrTPDO1;       // 0x3C TPDO1兼容性条目
        private UInt16 e_evenTPDO1;     // 0x3D TPDO1触发时间间隔(单位ms)
        private float e_scaling;        // 0x3F 缩放比例

        #region set SCT9 and get SCT9

        public Byte E_enGFC
        {
            set
            {
                e_enGFC = value;
            }
            get
            {
                return e_enGFC;
            }
        }
        public Byte E_enSRDO
        {
            set
            {
                e_enSRDO = value;
            }
            get
            {
                return e_enSRDO;
            }
        }
        public UInt16 E_SCT_time
        {
            set
            {
                e_SCT_time = value;
            }
            get
            {
                return e_SCT_time;
            }
        }
        public UInt16 E_COB_ID1
        {
            set
            {
                e_COB_ID1 = value;
            }
            get
            {
                return e_COB_ID1;
            }
        }
        public UInt16 E_COB_ID2
        {
            set
            {
                e_COB_ID2 = value;
            }
            get
            {
                return e_COB_ID2;
            }
        }
        public Byte E_enOL
        {
            set
            {
                e_enOL = value;
            }
            get
            {
                return e_enOL;
            }
        }
        public Byte E_overload
        {
            set
            {
                e_overload = value;
            }
            get
            {
                return e_overload;
            }
        }
        public Byte E_alarmMode
        {
            set
            {
                e_alarmMode = value;
            }
            get
            {
                return e_alarmMode;
            }
        }
        public Int32 E_wetTarget
        {
            set
            {
                e_wetTarget = value;
            }
            get
            {
                return e_wetTarget;
            }
        }
        public Int32 E_wetLow
        {
            set
            {
                e_wetLow = value;
            }
            get
            {
                return e_wetLow;
            }
        }
        public Int32 E_wetHigh
        {
            set
            {
                e_wetHigh = value;
            }
            get
            {
                return e_wetHigh;
            }
        }
        public Int32 E_filter
        {
            set
            {
                e_filter = value;
            }
            get
            {
                return e_filter;
            }
        }
        public UInt16 E_netServicePort
        {
            set
            {
                e_netServicePort = value;
            }
            get
            {
                return e_netServicePort;
            }
        }
        public Byte[] E_netServiceIP
        {
            set
            {
                e_netServiceIP = value;
            }
            get
            {
                return e_netServiceIP;
            }
        }
        public Byte[] E_netClientIP
        {
            set
            {
                e_netClientIP = value;
            }
            get
            {
                return e_netClientIP;
            }
        }
        public Byte[] E_netGatIP
        {
            set
            {
                e_netGatIP = value;
            }
            get
            {
                return e_netGatIP;
            }
        }
        public Byte[] E_netMaskIP
        {
            set
            {
                e_netMaskIP = value;
            }
            get
            {
                return e_netMaskIP;
            }
        }
        public Byte E_useDHCP
        {
            set
            {
                e_useDHCP = value;
            }
            get
            {
                return e_useDHCP;
            }
        }
        public Byte E_useScan
        {
            set
            {
                e_useScan = value;
            }
            get
            {
                return e_useScan;
            }
        }

        public Byte[] E_addrRF
        {
            set
            {
                e_addrRF = value;
            }
            get
            {
                return e_addrRF;
            }
        }

        public Byte E_spedRF
        {
            set
            {
                e_spedRF = value;
            }
            get
            {
                return e_spedRF;
            }
        }

        public Byte E_chanRF
        {
            set
            {
                e_chanRF = value;
            }
            get
            {
                return e_chanRF;
            }
        }

        public Byte E_optionRF
        {
            set
            {
                e_optionRF = value;
            }
            get
            {
                return e_optionRF;
            }
        }

        public UInt16 E_lockTPDO0
        {
            set
            {
                e_lockTPDO0 = value;
            }
            get
            {
                return e_lockTPDO0;
            }
        }

        public Byte E_entrTPDO0
        {
            set
            {
                e_entrTPDO0 = value;
            }
            get
            {
                return e_entrTPDO0;
            }
        }

        public Byte E_typeTPDO1
        {
            set
            {
                e_typeTPDO1 = value;
            }
            get
            {
                return e_typeTPDO1;
            }
        }

        public UInt16 E_lockTPDO1
        {
            set
            {
                e_lockTPDO1 = value;
            }
            get
            {
                return e_lockTPDO1;
            }
        }

        public Byte E_entrTPDO1
        {
            set
            {
                e_entrTPDO1 = value;
            }
            get
            {
                return e_entrTPDO1;
            }
        }

        public UInt16 E_evenTPDO1
        {
            set
            {
                e_evenTPDO1 = value;
            }
            get
            {
                return e_evenTPDO1;
            }
        }

        public float E_scaling
        {
            set
            {
                e_scaling = value;
            }
            get
            {
                return e_scaling;
            }
        }

        #endregion

        //EEPROM_PARA
        private Byte ep_version;
        private Byte ep_curve;
        private Byte ep_adspeed;
        private Int32 ep_ad_zero;
        private Int32 ep_ad_point1;
        private Int32 ep_ad_point2;
        private Int32 ep_ad_point3;
        private Int32 ep_ad_point4;
        private Int32 ep_ad_point5;
        private Int32 ep_input1;
        private Int32 ep_input2;
        private Int32 ep_input3;
        private Int32 ep_input4;
        private Int32 ep_input5;
        private Int32 ep_analog1;
        private Int32 ep_analog2;
        private Int32 ep_analog3;
        private Int32 ep_analog4;
        private Int32 ep_analog5;
        private Int32 ep_wt_zero;
        private Int32 ep_wt_full;
        private Byte ep_wt_decimal;
        private Byte ep_wt_unit;
        private Byte ep_outype;
        private Byte[] ep_text;

        #region set EEPROM_PARA and get EEPROM_PARA

        public Byte Ep_version
        {
            set
            {
                ep_version = value;
            }
            get
            {
                return ep_version;
            }
        }
        public Byte Ep_curve
        {
            set
            {
                ep_curve = value;
            }
            get
            {
                return ep_curve;
            }
        }
        public Byte Ep_adspeed
        {
            set
            {
                ep_adspeed = value;
            }
            get
            {
                return ep_adspeed;
            }
        }
        public Int32 Ep_ad_zero
        {
            set
            {
                ep_ad_zero = value;
            }
            get
            {
                return ep_ad_zero;
            }
        }
        public Int32 Ep_ad_point1
        {
            set
            {
                ep_ad_point1 = value;
            }
            get
            {
                return ep_ad_point1;
            }
        }
        public Int32 Ep_ad_point2
        {
            set
            {
                ep_ad_point2 = value;
            }
            get
            {
                return ep_ad_point2;
            }
        }
        public Int32 Ep_ad_point3
        {
            set
            {
                ep_ad_point3 = value;
            }
            get
            {
                return ep_ad_point3;
            }
        }
        public Int32 Ep_ad_point4
        {
            set
            {
                ep_ad_point4 = value;
            }
            get
            {
                return ep_ad_point4;
            }
        }
        public Int32 Ep_ad_point5
        {
            set
            {
                ep_ad_point5 = value;
            }
            get
            {
                return ep_ad_point5;
            }
        }
        public Int32 Ep_input1
        {
            set
            {
                ep_input1 = value;
            }
            get
            {
                return ep_input1;
            }
        }
        public Int32 Ep_input2
        {
            set
            {
                ep_input2 = value;
            }
            get
            {
                return ep_input2;
            }
        }
        public Int32 Ep_input3
        {
            set
            {
                ep_input3 = value;
            }
            get
            {
                return ep_input3;
            }
        }
        public Int32 Ep_input4
        {
            set
            {
                ep_input4 = value;
            }
            get
            {
                return ep_input4;
            }
        }
        public Int32 Ep_input5
        {
            set
            {
                ep_input5 = value;
            }
            get
            {
                return ep_input5;
            }
        }
        public Int32 Ep_analog1
        {
            set
            {
                ep_analog1 = value;
            }
            get
            {
                return ep_analog1;
            }
        }
        public Int32 Ep_analog2
        {
            set
            {
                ep_analog2 = value;
            }
            get
            {
                return ep_analog2;
            }
        }
        public Int32 Ep_analog3
        {
            set
            {
                ep_analog3 = value;
            }
            get
            {
                return ep_analog3;
            }
        }
        public Int32 Ep_analog4
        {
            set
            {
                ep_analog4 = value;
            }
            get
            {
                return ep_analog4;
            }
        }
        public Int32 Ep_analog5
        {
            set
            {
                ep_analog5 = value;
            }
            get
            {
                return ep_analog5;
            }
        }
        public Int32 Ep_wt_zero
        {
            set
            {
                ep_wt_zero = value;
            }
            get
            {
                return ep_wt_zero;
            }
        }
        public Int32 Ep_wt_full
        {
            set
            {
                ep_wt_full = value;
            }
            get
            {
                return ep_wt_full;
            }
        }
        public Byte Ep_wt_decimal
        {
            set
            {
                ep_wt_decimal = value;
            }
            get
            {
                return ep_wt_decimal;
            }
        }
        public Byte Ep_wt_unit
        {
            set
            {
                ep_wt_unit = value;
            }
            get
            {
                return ep_wt_unit;
            }
        }
        public Byte Ep_outype
        {
            set
            {
                ep_outype = value;
            }
            get
            {
                return ep_outype;
            }
        }
        public Byte[] Ep_text
        {
            set
            {
                ep_text = value;
            }
            get
            {
                return ep_text;
            }
        }

        #endregion

        #region 调试灵敏度输入值,和TextBox相互传递值
        public String T_input1
        {
            set
            {
                e_input1 = GetAngFromText(value);
                e_ad_point1 = GetAdcFromText(value);
                e_ad_zero = e_ad_point1;
            }
            get
            {
                return GetTextFromAng(e_input1);
            }
        }
        public String T_input2
        {
            set
            {
                e_input2 = GetAngFromText(value);
                e_ad_point2 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input2);
            }
        }
        public String T_input3
        {
            set
            {
                e_input3 = GetAngFromText(value);
                e_ad_point3 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input3);
            }
        }
        public String T_input4
        {
            set
            {
                e_input4 = GetAngFromText(value);
                e_ad_point4 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input4);
            }
        }
        public String T_input5
        {
            set
            {
                e_input5 = GetAngFromText(value);
                e_ad_point5 = GetAdcFromText(value);
                if (!S_ElevenType) e_ad_full = e_ad_point5;
            }
            get
            {
                return GetTextFromAng(e_input5);
            }
        }
        public String T_input6
        {
            set
            {
                e_input6 = GetAngFromText(value);
                e_ad_point6 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input6);
            }
        }
        public String T_input7
        {
            set
            {
                e_input7 = GetAngFromText(value);
                e_ad_point7 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input7);
            }
        }
        public String T_input8
        {
            set
            {
                e_input8 = GetAngFromText(value);
                e_ad_point8 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input8);
            }
        }
        public String T_input9
        {
            set
            {
                e_input9 = GetAngFromText(value);
                e_ad_point9 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input9);
            }
        }
        public String T_input10
        {
            set
            {
                e_input10 = GetAngFromText(value);
                e_ad_point10 = GetAdcFromText(value);
            }
            get
            {
                return GetTextFromAng(e_input10);
            }
        }
        public String T_input11
        {
            set
            {
                e_input11 = GetAngFromText(value);
                e_ad_point11 = GetAdcFromText(value);
                if (S_ElevenType) e_ad_full = e_ad_point11;
            }
            get
            {
                return GetTextFromAng(e_input11);
            }
        }

        #endregion

        #region 调试模拟量或数字量输出值,和TextBox相互传递值
        public String T_analog1
        {
            set
            {
                e_analog1 = GetAngFromText(value);
                e_da_point1 = GetDatFromText(value);
                e_da_zero = e_da_point1;
            }
            get
            {
                return GetTextFromAng(e_analog1);
            }
        }
        public String T_analog2
        {
            set
            {
                e_analog2 = GetAngFromText(value);
                e_da_point2 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog2);
            }
        }
        public String T_analog3
        {
            set
            {
                e_analog3 = GetAngFromText(value);
                e_da_point3 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog3);
            }
        }
        public String T_analog4
        {
            set
            {
                e_analog4 = GetAngFromText(value);
                e_da_point4 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog4);
            }
        }
        public String T_analog5
        {
            set
            {
                e_analog5 = GetAngFromText(value);
                e_da_point5 = GetDatFromText(value);
                if (!S_ElevenType) e_da_full = e_da_point5;
            }
            get
            {
                return GetTextFromAng(e_analog5);
            }
        }
        public String T_analog6
        {
            set
            {
                e_analog6 = GetAngFromText(value);
                e_da_point6 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog6);
            }
        }
        public String T_analog7
        {
            set
            {
                e_analog7 = GetAngFromText(value);
                e_da_point7 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog7);
            }
        }
        public String T_analog8
        {
            set
            {
                e_analog8 = GetAngFromText(value);
                e_da_point8 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog8);
            }
        }
        public String T_analog9
        {
            set
            {
                e_analog9 = GetAngFromText(value);
                e_da_point9 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog9);
            }
        }
        public String T_analog10
        {
            set
            {
                e_analog10 = GetAngFromText(value);
                e_da_point10 = GetDatFromText(value);
            }
            get
            {
                return GetTextFromAng(e_analog10);
            }
        }
        public String T_analog11
        {
            set
            {
                e_analog11 = GetAngFromText(value);
                e_da_point11 = GetDatFromText(value);
                if (S_ElevenType) e_da_full = e_da_point11;
            }
            get
            {
                return GetTextFromAng(e_analog11);
            }
        }

        #endregion

        #region 调整量程,和TextBox相互传递值
        public String T_wt_zero
        {
            set
            {
                e_wt_zero = GetWetFromText(value);
            }
            get
            {
                return GetTextFromWet(e_wt_zero);
            }
        }
        public String T_wt_full
        {
            set
            {
                e_wt_full = GetWetFromText(value);
            }
            get
            {
                return GetTextFromWet(e_wt_full);
            }
        }

        #endregion

        #region 调整TPDO,和TextBox相互传递值

        public String T_timeTPDO
        {
            get
            {
                if (e_typeTPDO0 != 0xFE)
                {
                    return e_typeTPDO0.ToString();//同步模式
                }
                else
                {
                    return e_evenTPDO0.ToString();//异步模式
                }
            }
        }

        #endregion

        #region 调整报警值,和TextBox相互传递值
        public String T_wetTarget
        {
            set
            {
                e_wetTarget = GetWetFromText(value);
            }
            get
            {
                return GetTextFromWet(e_wetTarget);
            }
        }
        public String T_wetLow
        {
            set
            {
                e_wetLow = GetWetFromText(value);
            }
            get
            {
                return GetTextFromWet(e_wetLow);
            }
        }
        public String T_wetHigh
        {
            set
            {
                e_wetHigh = GetWetFromText(value);
            }
            get
            {
                return GetTextFromWet(e_wetHigh);
            }
        }

        #endregion
    }
}
