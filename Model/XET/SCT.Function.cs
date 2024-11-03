using Library;
using System;
using System.Collections.Generic;

//未经过审批不得改动

//Alvin 20230414

//SCT变量用于管理设备SCT参数以及SCT衍生换算参数
//SCT方法用于数值换算

//以下需要数值换算方法
//input和string的传值,需要同步更新ad_point和ad_zero/ad_full
//analog和string的传值,需要同步更新da_point和da_zero/da_full
//wt_zero/wt_full和string的传值

//从SCT变量中解码设备型号
//从SCT变量中解码设备模拟量输出类型
//从SCT变量中解码设备11点标定
//根据SCT变量调整灵敏度常数

namespace Model
{
    public partial class SCT
    {
        public SCT()
        {
            //SCT0
            e_test = 0x55;
            e_outype = (Byte)TYPE.TDSS + (Byte)OUT.UT420;
            e_curve = (Byte)ECVE.CTWOPT;
            e_adspeed = (Byte)ESPD.CSF40 + (Byte)EPGA.ADPGA128;
            e_autozero = (Byte)EATZ.ATZ10;
            e_trackzero = (Byte)EATK.TKZ2;
            e_checkhigh = 0;
            e_checklow = 1000000;
            e_mfg_date = Convert.ToUInt32(System.DateTime.Now.ToString("yyMMddHHmm"));
            e_mfg_srno = 0;
            e_tmp_min = 8000000;
            e_tmp_max = 0;
            e_tmp_cal = 0;
            e_bohrcode = 0;
            e_enspan = 1;
            e_protype = (Byte)TYPE.TD485;

            //SCT1
            e_ad_point1 = 0;
            e_ad_point2 = 30000;
            e_ad_point3 = 60000;
            e_ad_point4 = 90000;
            e_ad_point5 = 120000;
            e_da_point1 = 0;
            e_da_point2 = 250000;
            e_da_point3 = 500000;
            e_da_point4 = 750000;
            e_da_point5 = 1000000;

            //SCT2
            e_input1 = MyDevice.myUIT.ConvertFloatToInt32(0.0f);
            e_input2 = MyDevice.myUIT.ConvertFloatToInt32(0.5f);
            e_input3 = MyDevice.myUIT.ConvertFloatToInt32(1.0f);
            e_input4 = MyDevice.myUIT.ConvertFloatToInt32(1.5f);
            e_input5 = MyDevice.myUIT.ConvertFloatToInt32(2.0f);
            e_analog1 = MyDevice.myUIT.ConvertFloatToInt32(4.0f);
            e_analog2 = MyDevice.myUIT.ConvertFloatToInt32(8.0f);
            e_analog3 = MyDevice.myUIT.ConvertFloatToInt32(12.0f);
            e_analog4 = MyDevice.myUIT.ConvertFloatToInt32(16.0f);
            e_analog5 = MyDevice.myUIT.ConvertFloatToInt32(20.0f);

            //SCT3
            e_ad_zero = 0;
            e_ad_full = 120000;
            e_da_zero = 0;
            e_da_full = 1000000;
            e_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_wtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_atio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_btio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_ctio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_dtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);

            //SCT4
            e_da_zero_4ma = 200000;
            e_da_full_20ma = 1000000;
            e_da_zero_05V = 0;
            e_da_full_05V = 1000000;
            e_da_zero_10V = 0;
            e_da_full_10V = 1000000;
            e_da_zero_N5 = 48000;
            e_da_full_P5 = 1000000;
            e_da_zero_N10 = 48000;
            e_da_full_P10 = 1000000;

            //SCT5
            e_corr = MyDevice.myUIT.ConvertFloatToInt32(1.0f);
            e_mark = 0xFF;
            e_sign = 0x03; //MODBUS
            e_addr = 0x01;
            e_baud = 0x03; //9600
            e_stopbit = 0x01;
            e_parity = 0x00;
            e_wt_zero = 0;
            e_wt_full = 1000;
            e_wt_decimal = 0;
            e_wt_unit = 1;
            e_wt_ascii = 0;
            e_wt_sptime = 0;
            e_wt_spfilt = 0;
            e_wt_division = 1;
            e_wt_antivib = 0;
            e_heartBeat = 0x64;
            e_typeTPDO0 = 0xFE;
            e_evenTPDO0 = 0x64;
            e_nodeID = 0x01;
            e_nodeBaud = 0x04;
            e_dynazero = (Byte)EATK.TKZ2;
            e_cheatype = 0;
            e_thmax = 7;
            e_thmin = 3;
            e_stablerange = 0;
            e_stabletime = 10;
            e_tkzerotime = 10;
            e_tkdynatime = 3;

            //SCT6
            e_ad_point6 = 10000;
            e_ad_point7 = 20000;
            e_ad_point8 = 30000;
            e_ad_point9 = 40000;
            e_ad_point10 = 50000;
            e_da_point6 = 1250000;
            e_da_point7 = 1500000;
            e_da_point8 = 1750000;
            e_da_point9 = 2000000;
            e_da_point10 = 2250000;

            //SCT7
            e_input6 = MyDevice.myUIT.ConvertFloatToInt32(0.5f);
            e_input7 = MyDevice.myUIT.ConvertFloatToInt32(1.0f);
            e_input8 = MyDevice.myUIT.ConvertFloatToInt32(1.5f);
            e_input9 = MyDevice.myUIT.ConvertFloatToInt32(2.0f);
            e_input10 = MyDevice.myUIT.ConvertFloatToInt32(2.5f);
            e_analog6 = MyDevice.myUIT.ConvertFloatToInt32(8.0f);
            e_analog7 = MyDevice.myUIT.ConvertFloatToInt32(10.0f);
            e_analog8 = MyDevice.myUIT.ConvertFloatToInt32(12.0f);
            e_analog9 = MyDevice.myUIT.ConvertFloatToInt32(16.0f);
            e_analog10 = MyDevice.myUIT.ConvertFloatToInt32(18.0f);

            //SCT8
            e_ad_point11 = 60000;
            e_da_point11 = 70000;
            e_input11 = MyDevice.myUIT.ConvertFloatToInt32(3.0f);
            e_analog11 = MyDevice.myUIT.ConvertFloatToInt32(20.0f);
            e_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_wtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_atio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_btio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_ctio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            e_dtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);

            //SCT9
            e_enGFC = 0;
            e_enSRDO = 0;
            e_SCT_time = 0x19;
            e_COB_ID1 = 0x0109;
            e_COB_ID2 = 0x010A;
            e_enOL = 1;
            e_overload = 0x78;
            e_alarmMode = 0;
            e_wetTarget = 0x03E8;
            e_wetLow = 0x01F4;
            e_wetHigh = 0x03E8;
            e_filter = 10;
            e_netServicePort = 5678;
            e_netServiceIP = new Byte[4] { 192, 168, 1, 1 };
            e_netClientIP = new Byte[4] { 192, 168, 1, 200 };
            e_netGatIP = new Byte[4] { 192, 168, 1, 1 };
            e_netMaskIP = new Byte[4] { 255, 255, 255, 255 };
            e_useDHCP = 0x01;
            e_useScan = 0x01;
            e_addrRF = new byte[2] { 0, 0 };
            e_spedRF = 0x18;
            e_chanRF = 0x04;
            e_optionRF = 0x1E;
            e_lockTPDO0 = 0;
            e_entrTPDO0 = 0;
            e_typeTPDO1 = 1;
            e_lockTPDO1 = 0;
            e_entrTPDO1 = 0;
            e_evenTPDO1 = 0;
            e_scaling = 1.0f;
        }

        #region

        //将不带小数点的量程,转成带小数点的String给TextBox
        private String GetTextFromWet(Int32 dat)
        {
            //界面数值 = 变量值 / 10^小数点
            return (dat / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);
        }

        //将TextBox的带小数点String,转成不带小数点的量程
        private Int32 GetWetFromText(String txt)
        {
            //变量值 = 界面数值 * 10^小数点
            return (int)(Convert.ToDouble(txt) * Math.Pow(10, E_wt_decimal));
        }

        //将字节存格式存储的浮点数input或analog,转成String给TextBox
        private String GetTextFromAng(Int32 dat)
        {
            UIT uit = new UIT();

            //提取字节存储格式的浮点数
            uit.I = dat;

            //返回浮点数ToString
            return uit.F.ToString();
        }

        //将TextBox的输入值,转成字节存格式存储的浮点数input或analog
        private Int32 GetAngFromText(String txt)
        {
            UIT uit = new UIT();

            //转成浮点数
            uit.F = (float)Convert.ToDouble(txt);

            //返回字节存储格式的浮点数
            return uit.I;
        }

        //将TextBox的输出值,换算成da_point
        private Int32 GetDatFromText(String txt)
        {
            double outputz;
            double outputf;
            Int32 da_zero;
            Int32 da_full;

            //模拟量变送器,da_point根据analog和校准值计算
            //数字量变送器,da_point根据analog和wt_decilam计算

            switch (S_OutType)
            {
                default:
                case OUT.UT420:
                    outputz = 4.0d;
                    outputf = 20.0d;
                    da_zero = e_da_zero_4ma;
                    da_full = e_da_full_20ma;
                    return (int)((Convert.ToDouble(txt) - outputz) * (da_full - da_zero) / (outputf - outputz) + da_zero);

                case OUT.UTP05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    da_zero = e_da_zero_05V;
                    da_full = e_da_full_05V;
                    return (int)((Convert.ToDouble(txt) - outputz) * (da_full - da_zero) / (outputf - outputz) + da_zero);

                case OUT.UTP10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    da_zero = e_da_zero_10V;
                    da_full = e_da_full_10V;
                    return (int)((Convert.ToDouble(txt) - outputz) * (da_full - da_zero) / (outputf - outputz) + da_zero);

                case OUT.UTN05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    da_zero = (e_da_zero_N5 + e_da_full_P5) / 2;
                    da_full = e_da_full_P5;
                    return (int)((Convert.ToDouble(txt) - outputz) * (da_full - da_zero) / (outputf - outputz) + da_zero);

                case OUT.UTN10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    da_zero = (e_da_zero_N10 + e_da_full_P10) / 2;
                    da_full = e_da_full_P10;
                    return (int)((Convert.ToDouble(txt) - outputz) * (da_full - da_zero) / (outputf - outputz) + da_zero);

                case OUT.UMASK:
                    //变量值 = 界面数值 * 10^小数点
                    return (int)(Convert.ToDouble(txt) * Math.Pow(10, E_wt_decimal));
            }
        }

        //将TextBox的灵敏度值,换算成ad_point
        private Int32 GetAdcFromText(String txt)
        {
            return (int)(Convert.ToDouble(txt) * S_MVDV);
        }

        //将字节存格式存储的浮点数input或analog,转成浮点数再取绝对值
        public float GetAbsfFromInput(Int32 dat)
        {
            return Math.Abs(MyDevice.myUIT.ConvertInt32ToFloat(dat));
        }

        //将存储ip地址的byte数组转换为ip地址string
        public String GetIpAddressFromArray(Byte[] ip)
        {
            return ip[0].ToString() + "." + ip[1].ToString() + "." + ip[2].ToString() + "." + ip[3].ToString();
        }

        //将存储ip地址的string转换为ip地址byte数组
        public Byte[] GetIpAddressFromString(String ip)
        {
            String[] ipArray;
            Byte[] ipArrayByte = { 0, 0, 0, 0 };
            ipArray = ip.Split('.');
            if (ipArray.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (byte.TryParse(ipArray[i], out Byte b))
                    {
                        ipArrayByte[i] = b;
                    }
                    else
                    {
                        //格式有误
                        return new Byte[4] { 0, 0, 0, 0 };
                    }
                }
                return ipArrayByte;
            }
            else
            {
                //格式有误
                return new Byte[4] { 0, 0, 0, 0 };
            }
        }

        //获取数字量单位
        public String GetUnitUMASK()
        {
            return UnitHelper.GetUnitDescription((UNIT)e_wt_unit);
        }

        #endregion

        #region

        //获取产品型号
        public TYPE S_DeviceType
        {
            get
            {
                if (e_outype != (Byte)OUT.UMASK)
                {
                    return (TYPE)(e_outype & 0xF8);
                }
                else
                {
                    return (TYPE)e_protype;
                }
            }
        }

        //模拟输出类型
        public OUT S_OutType
        {
            set
            {
                switch (value)
                {
                    case OUT.UT420:
                    case OUT.UTP05:
                    case OUT.UTP10:
                    case OUT.UTN05:
                    case OUT.UTN10:
                        e_outype = (byte)((e_outype & 0xF8) + (byte)value);
                        break;

                    case OUT.UMASK:
                        e_outype = (byte)OUT.UMASK;
                        break;

                    default:
                        break;
                }
            }
            get
            {
                if (e_outype != (Byte)OUT.UMASK)
                {
                    return (OUT)(e_outype & 0x07);
                }
                else
                {
                    return (OUT)e_outype;
                }
            }
        }

        //获取单位
        public String S_unit
        {
            get
            {
                //mA,V,kg
                switch (S_OutType)
                {
                    case OUT.UT420:
                        return "mA";

                    case OUT.UTP05:
                    case OUT.UTP10:
                    case OUT.UTN05:
                    case OUT.UTN10:
                        return "V";

                    case OUT.UMASK:
                        return UnitHelper.GetUnitAdjustedDescription((UNIT)E_wt_unit);

                    default:
                        return "";
                }
            }
        }

        //获取小数点
        public Byte S_decimal
        {
            get
            {
                if (S_OutType == OUT.UMASK)
                {
                    return e_wt_decimal;
                }
                else
                {
                    return 3;
                }
            }
        }

        //获取11点标定类型
        public bool S_ElevenType
        {
            get
            {
                switch ((ECVE)e_curve)
                {
                    default:
                    case ECVE.CTWOPT: return false;
                    case ECVE.CFITED: return false;
                    case ECVE.CINTER: return false;
                    case ECVE.CELTED: return true;
                    case ECVE.CELTER: return true;
                }
            }
        }

        //获取半点标定类型
        public bool S_HalfCal
        {
            get
            {
                switch (S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH: return false;
                    case TYPE.BS420H: return false;
                    case TYPE.T8X420H: return false;
                    case TYPE.BS600H: return false;
                    case TYPE.T4X600H: return false;
                    case TYPE.TNP10: return false;
                    case TYPE.TP10: return false;
                    case TYPE.TDSS: return false;
                    case TYPE.TD485: return false;
                    case TYPE.TCAN: return false;
                    case TYPE.iBus: return false;
                    case TYPE.iNet: return false;
                    case TYPE.iStar: return false;
                    case TYPE.TDES:
                        if (E_test >= 0x59 && E_outype == 0xC1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case TYPE.T420:
                        if (E_test >= 0x59 && E_outype == 0xA1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                }
            }
        }

        //获取灵敏度系数
        public double S_MVDV
        {
            get
            {
                double myAD;

                //不同采样速率灵敏度系数不同
                switch (e_adspeed & 0xF0)
                {
                    case (Byte)ESPD.CSF10: myAD = 134218.0; break; //4  2^19=524288, 524288/3.90625 = 134218
                    case (Byte)ESPD.CSF40: myAD = 67109.0; break;  //5  2^18=262144, 262144/3.90625 = 67109
                    case (Byte)ESPD.CSF640: myAD = 33554.0; break; //6  2^17=131072, 131072/3.90625 = 33554
                    case (Byte)ESPD.CSF1280: myAD = 8389.0; break; //8  2^15=32768,  32768/3.90625  = 8389
                    default: myAD = 67109.0; break;
                }

                //不同放大增益灵敏度系数不同
                switch (e_adspeed & 0x0F)
                {
                    case (Byte)EPGA.ADPGA1: myAD /= 128.0; break;
                    case (Byte)EPGA.ADPGA2: myAD /= 64.0; break;
                    case (Byte)EPGA.ADPGA64: myAD /= 2.0; break;
                    default: break;
                }

                //iBus再放大2*5倍或2*2倍
                if (S_DeviceType == TYPE.iBus || S_DeviceType == TYPE.iNet || S_DeviceType == TYPE.iStar)
                {
                    switch (e_adspeed & 0xF0)
                    {
                        case (Byte)ESPD.CSF10:
                        case (Byte)ESPD.CSF640:
                            myAD *= 10;
                            break;

                        default:
                            myAD *= 4;
                            break;
                    }
                }

                return myAD;
            }
        }

        //最低温度
        public String S_tmp_min
        {
            get
            {
                if (e_tmp_cal == 0)
                {
                    return e_tmp_min.ToString();
                }
                else
                {
                    return ((float)e_tmp_min / (float)e_tmp_cal * (273.15f + 25.0f) - 273.15f).ToString("f1") + " ℃";
                }
            }
        }

        //最高温度
        public String S_tmp_max
        {
            get
            {
                if (e_tmp_cal == 0)
                {
                    return e_tmp_max.ToString();
                }
                else
                {
                    return ((float)e_tmp_max / (float)e_tmp_cal * (273.15f + 25.0f) - 273.15f).ToString("f1") + " ℃";
                }
            }
        }

        //RF串口校验位，从e_spedRF获取
        public Byte S_parityRF
        {
            get
            {
                return (Byte)((e_spedRF & 0b11000000) >> 6);
            }
        }

        //RF串口波特率，从e_spedRF获取
        public Int16 S_baudrateRF
        {
            get
            {
                return (Byte)((e_spedRF & 0b00111000) >> 3);
            }
        }

        //RF空中速率，从e_spedRF获取
        public Int16 S_airrateRF
        {
            get
            {
                return (Byte)(e_spedRF & 0b00000111);
            }
        }

        //RF通信信道，从e_chanRF获取
        public Int16 S_channelRF
        {
            get
            {
                return (Byte)(e_chanRF & 0b00011111);
            }
        }

        #endregion

        #region

        //校准限制误差
        public double S_limitNum
        {
            get
            {
                double magnification;
                switch (S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH:
                        magnification = Math.Pow(2, 12);
                        break;
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                        magnification = Math.Pow(2, 16);
                        break;
                    case TYPE.BS600H:
                    case TYPE.T4X600H:
                        magnification = Math.Pow(2, 16);
                        break;
                    case TYPE.T420:
                        magnification = Math.Pow(2, 12);
                        break;
                    case TYPE.TNP10:
                        magnification = Math.Pow(2, 16);
                        break;
                    case TYPE.TDES:
                        magnification = Math.Pow(2, 14);
                        break;
                    case TYPE.TDSS:
                        magnification = Math.Pow(2, 16);
                        break;
                    case TYPE.TP10:
                        magnification = Math.Pow(2, 14);
                        break;
                }

                switch (S_OutType)
                {
                    default:
                    case OUT.UT420:
                        return 21.5 / magnification;
                    case OUT.UTP05:
                    case OUT.UTN05:
                        return 5.0 / magnification;
                    case OUT.UTP10:
                    case OUT.UTN10:
                        return 10.0 / magnification;
                }
            }
        }

        //校准方案outType
        public List<OUT> S_listPlan
        {
            get
            {
                switch (S_DeviceType)
                {
                    default:
                    case TYPE.BE30AH:
                        return new List<OUT>() { OUT.UTP05, OUT.UTP10 };
                    case TYPE.BS420H:
                    case TYPE.T8X420H:
                        return new List<OUT>() { OUT.UTP05, OUT.UTP10, OUT.UT420 };
                    case TYPE.BS600H:
                    case TYPE.T4X600H:
                        return new List<OUT>() { OUT.UTN05, OUT.UTN10, OUT.UTP05, OUT.UTP10 };
                    case TYPE.T420:
                        return new List<OUT>() { OUT.UT420 };
                    case TYPE.TNP10:
                        return new List<OUT>() { OUT.UTN05, OUT.UTN10, OUT.UTP05, OUT.UTP10 };
                    case TYPE.TDES:
                        return new List<OUT>() { OUT.UTP05, OUT.UTP10 };
                    case TYPE.TDSS:
                        return new List<OUT>() { OUT.UTN05, OUT.UTN10, OUT.UTP05, OUT.UTP10 };
                    case TYPE.TP10:
                        return new List<OUT>() { OUT.UTP05, OUT.UTP10 };
                }
            }
        }

        #endregion
    }
}
