using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;

//未经过审批不得改动

//Alvin 20230414

//XET变量用于管理设备的状态和非SCT参数
//XET方法用于各种计算设备数据

//斜率线性计算
//校准值和模拟量计算
//输出模拟量数字量和DAC计算
//dacout解码模拟量和数字量
//ascii解码数字量

namespace Model
{
    public partial class XET : SCT
    {
        //
        public XET()
        {
            //
            sTATE = STATE.INVALID;

            //
            R_bohrcode_long = 0;
            R_adcout = 0;
            R_dacset = 0;
            R_stable = false;
            R_overload = false;
            R_eeplink = false;
            R_grossnet = "";
            R_weight = "";
            R_output = "";
            R_nmterr = 0;

            //
            R_checklink = false;
            R_eepversion = "";
            R_errSensor = "";
            R_resolution = "";
            R_ipAddr = "";
        }

        //重置SCT0-SCT8参数,不改变硬件信息(outype,bohrcode)
        public void ResetDeviceSct()
        {
            //SCT0
            E_curve = (Byte)ECVE.CTWOPT;
            switch (S_DeviceType)
            {
                default:
                    E_adspeed = (Byte)ESPD.CSF40 + (Byte)EPGA.ADPGA128;
                    break;

                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    E_adspeed = (Byte)ESPD.CSF10 + (Byte)EPGA.ADPGA128;
                    break;

                case TYPE.TP10:
                case TYPE.TDES:
                    E_adspeed = (Byte)ESPD.CSF1280 + (Byte)EPGA.ADPGA128;
                    break;
            }
            E_autozero = (Byte)EATZ.ATZ0;
            E_trackzero = (Byte)EATK.TKZ0;
            E_checkhigh = 0;
            E_checklow = 1000000;
            E_mfg_date = Convert.ToUInt32(System.DateTime.Now.ToString("yyMMddHHmm"));
            E_mfg_srno = 0;
            E_tmp_min = 8000000;
            E_tmp_max = 0;
            E_tmp_cal = 0;
            E_enspan = 1;

            //SCT5
            E_corr = MyDevice.myUIT.ConvertFloatToInt32(1.0f);
            E_mark = 0xFF;
            E_sign = 0x03; //MODBUS
            E_addr = 0x01;
            E_baud = 0x03; //9600
            E_stopbit = 0x01;
            E_parity = 0x00;
            E_wt_zero = 0;
            E_wt_full = 1000;
            E_wt_decimal = 0;
            E_wt_unit = 1;
            E_wt_ascii = 0;
            E_wt_sptime = 0;
            E_wt_spfilt = 0;
            E_wt_division = 1;
            E_wt_antivib = 0;
            E_heartBeat = 0x64;
            E_typeTPDO0 = 0xFE;
            E_evenTPDO0 = 0x64;
            E_nodeID = 0x01;
            E_nodeBaud = 0x04;
            E_dynazero = (Byte)EATK.TKZ0;
            E_cheatype = 0;
            E_thmax = 7;
            E_thmin = 3;
            E_stablerange = 0;
            E_stabletime = 10;
            E_tkzerotime = 10;
            E_tkdynatime = 3;

            //input
            T_input1 = "0.0";
            T_input2 = "0.5";
            T_input3 = "1.0";
            T_input4 = "1.5";
            T_input5 = "2.0";
            T_input6 = "1.0";
            T_input7 = "1.0";
            T_input8 = "1.0";
            T_input9 = "1.0";
            T_input10 = "1.0";
            T_input11 = "2.0";

            //output
            switch (S_OutType)
            {
                default:
                case OUT.UT420:
                    T_analog1 = "4";
                    T_analog2 = "8.0";
                    T_analog3 = "12.0";
                    T_analog4 = "16.0";
                    T_analog5 = "20.0";
                    T_analog6 = "0.0";
                    T_analog7 = "0.0";
                    T_analog8 = "0.0";
                    T_analog9 = "0.0";
                    T_analog10 = "0.0";
                    T_analog11 = "0.0";
                    break;

                case OUT.UTP05:
                case OUT.UTN05:
                    T_analog1 = "0";
                    T_analog2 = "1.25";
                    T_analog3 = "2.5";
                    T_analog4 = "3.75";
                    T_analog5 = "5.0";
                    T_analog6 = "0.0";
                    T_analog7 = "0.0";
                    T_analog8 = "0.0";
                    T_analog9 = "0.0";
                    T_analog10 = "0.0";
                    T_analog11 = "0.0";
                    break;

                case OUT.UTP10:
                case OUT.UTN10:
                    T_analog1 = "0";
                    T_analog2 = "2.5";
                    T_analog3 = "5.0";
                    T_analog4 = "7.5";
                    T_analog5 = "10.0";
                    T_analog6 = "0.0";
                    T_analog7 = "0.0";
                    T_analog8 = "0.0";
                    T_analog9 = "0.0";
                    T_analog10 = "0.0";
                    T_analog11 = "0.0";
                    break;

                case OUT.UMASK:
                    T_analog1 = "0";
                    T_analog2 = "250.0";
                    T_analog3 = "500.0";
                    T_analog4 = "750.0";
                    T_analog5 = "1000.0";
                    T_analog6 = "0.0";
                    T_analog7 = "0.0";
                    T_analog8 = "0.0";
                    T_analog9 = "0.0";
                    T_analog10 = "0.0";
                    T_analog11 = "0.0";
                    break;
            }

            //Vtio
            RefreshVtio();
        }

        //重新计算Vtio值和AD值(两点)
        private void RefreshVtio()
        {
            double slope = ((double)(E_da_point5 - E_da_point1)) / ((double)(E_ad_point5 - E_ad_point1));
            //
            E_vtio = MyDevice.myUIT.ConvertFloatToInt32((float)slope);
            E_wtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_atio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_btio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ctio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_dtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_etio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ftio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_gtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_htio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_itio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_jtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            //
            E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point1)) / slope) + E_ad_point1;
            E_ad_full = (Int32)(((double)(E_da_full - E_da_point1)) / slope) + E_ad_point1;
        }

        //重新计算Wtio值和AD值（五点拟合）
        private void RefreshWtio()
        {
            Int64 summationX;//累加
            Int64 summationY;//累加
            Int64 numerator;//分子
            Int64 denominator;//分母

            //累加
            summationX = (Int64)(E_ad_point1 + E_ad_point2 + E_ad_point3 + E_ad_point4 + E_ad_point5);
            summationY = (Int64)(E_da_point1 + E_da_point2 + E_da_point3 + E_da_point4 + E_da_point5);
            //分子
            numerator = (((Int64)E_ad_point1 * (Int64)E_da_point1) +
                         ((Int64)E_ad_point2 * (Int64)E_da_point2) +
                         ((Int64)E_ad_point3 * (Int64)E_da_point3) +
                         ((Int64)E_ad_point4 * (Int64)E_da_point4) +
                         ((Int64)E_ad_point5 * (Int64)E_da_point5)) * 5 - (summationX * summationY);
            //分母
            denominator = (((Int64)E_ad_point1 * (Int64)E_ad_point1) +
                           ((Int64)E_ad_point2 * (Int64)E_ad_point2) +
                           ((Int64)E_ad_point3 * (Int64)E_ad_point3) +
                           ((Int64)E_ad_point4 * (Int64)E_ad_point4) +
                           ((Int64)E_ad_point5 * (Int64)E_ad_point5)) * 5 - (summationX * summationX);
            //
            double slope = (double)numerator / (double)denominator;
            //
            E_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_wtio = MyDevice.myUIT.ConvertFloatToInt32((float)slope);
            E_atio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_btio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ctio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_dtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_etio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ftio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_gtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_htio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_itio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_jtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            //
            numerator = (Int64)E_da_zero * 5 - summationY;
            double zp = (double)numerator / slope + (double)summationX;
            E_ad_zero = (Int32)(zp / 5.0f);
            E_ad_full = E_ad_zero + (Int32)((double)(E_da_full - E_da_zero) / slope);
        }

        //重新计算atio&btio&ctio&dtio值和AD值（五点插值）
        private void RefreshXtio()
        {
            double slpoA = ((double)(E_da_point2 - E_da_point1)) / ((double)(E_ad_point2 - E_ad_point1));
            double slpoB = ((double)(E_da_point3 - E_da_point2)) / ((double)(E_ad_point3 - E_ad_point2));
            double slpoC = ((double)(E_da_point4 - E_da_point3)) / ((double)(E_ad_point4 - E_ad_point3));
            double slpoD = ((double)(E_da_point5 - E_da_point4)) / ((double)(E_ad_point5 - E_ad_point4));
            //
            E_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_wtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_atio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoA);
            E_btio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoB);
            E_ctio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoC);
            E_dtio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoD);
            E_etio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ftio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_gtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_htio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_itio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_jtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            //
            if (E_da_zero < E_da_point2)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point1)) / slpoA) + E_ad_point1;
            }
            else if (E_da_zero < E_da_point3)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point2)) / slpoB) + E_ad_point2;
            }
            else if (E_da_zero < E_da_point4)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point3)) / slpoC) + E_ad_point3;
            }
            else
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point4)) / slpoD) + E_ad_point4;
            }
            //
            if (E_da_full < E_da_point2)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point1)) / slpoA) + E_ad_point1;
            }
            else if (E_da_full < E_da_point3)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point2)) / slpoB) + E_ad_point2;
            }
            else if (E_da_full < E_da_point4)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point3)) / slpoC) + E_ad_point3;
            }
            else
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point4)) / slpoD) + E_ad_point4;
            }
        }

        //重新计算Wtio值和AD值（十一点拟合）
        private void RefreshFtio()
        {
            Int64 summationX;//累加
            Int64 summationY;//累加
            Int64 numerator;//分子
            Int64 denominator;//分母

            //累加
            summationX = (Int64)(E_ad_point1 + E_ad_point2 + E_ad_point3 + E_ad_point4 + E_ad_point5 + E_ad_point6 + E_ad_point7 + E_ad_point8 + E_ad_point9 + E_ad_point10 + E_ad_point11);
            summationY = (Int64)(E_da_point1 + E_da_point2 + E_da_point3 + E_da_point4 + E_da_point5 + E_da_point6 + E_da_point7 + E_da_point8 + E_da_point9 + E_da_point10 + E_da_point11);
            //分子
            numerator = (((Int64)E_ad_point1 * (Int64)E_da_point1) +
                         ((Int64)E_ad_point2 * (Int64)E_da_point2) +
                         ((Int64)E_ad_point3 * (Int64)E_da_point3) +
                         ((Int64)E_ad_point4 * (Int64)E_da_point4) +
                         ((Int64)E_ad_point5 * (Int64)E_da_point5) +
                         ((Int64)E_ad_point6 * (Int64)E_da_point6) +
                         ((Int64)E_ad_point7 * (Int64)E_da_point7) +
                         ((Int64)E_ad_point8 * (Int64)E_da_point8) +
                         ((Int64)E_ad_point9 * (Int64)E_da_point9) +
                         ((Int64)E_ad_point10 * (Int64)E_da_point10) +
                         ((Int64)E_ad_point11 * (Int64)E_da_point11)) * 11 - (summationX * summationY);
            //分母
            denominator = (((Int64)E_ad_point1 * (Int64)E_ad_point1) +
                           ((Int64)E_ad_point2 * (Int64)E_ad_point2) +
                           ((Int64)E_ad_point3 * (Int64)E_ad_point3) +
                           ((Int64)E_ad_point4 * (Int64)E_ad_point4) +
                           ((Int64)E_ad_point5 * (Int64)E_ad_point5) +
                           ((Int64)E_ad_point6 * (Int64)E_ad_point6) +
                           ((Int64)E_ad_point7 * (Int64)E_ad_point7) +
                           ((Int64)E_ad_point8 * (Int64)E_ad_point8) +
                           ((Int64)E_ad_point9 * (Int64)E_ad_point9) +
                           ((Int64)E_ad_point10 * (Int64)E_ad_point10) +
                           ((Int64)E_ad_point11 * (Int64)E_ad_point11)) * 11 - (summationX * summationX);
            //
            double slope = (double)numerator / (double)denominator;
            //
            E_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_wtio = MyDevice.myUIT.ConvertFloatToInt32((float)slope);
            E_atio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_btio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ctio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_dtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_etio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_ftio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_gtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_htio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_itio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_jtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            //
            numerator = (Int64)E_da_zero * 11 - summationY;
            double zp = (double)numerator / slope + (double)summationX;
            E_ad_zero = (Int32)(zp / 11.0f);
            E_ad_full = E_ad_zero + (Int32)((double)(E_da_full - E_da_zero) / slope);
        }

        //重新计算gtio&htio&itio&jtio值和AD值（十一点插值）
        private void RefreshKtio()
        {
            double slpoA = ((double)(E_da_point2 - E_da_point1)) / ((double)(E_ad_point2 - E_ad_point1));
            double slpoB = ((double)(E_da_point3 - E_da_point2)) / ((double)(E_ad_point3 - E_ad_point2));
            double slpoC = ((double)(E_da_point4 - E_da_point3)) / ((double)(E_ad_point4 - E_ad_point3));
            double slpoD = ((double)(E_da_point5 - E_da_point4)) / ((double)(E_ad_point5 - E_ad_point4));
            double slpoE = ((double)(E_da_point6 - E_da_point5)) / ((double)(E_ad_point6 - E_ad_point5));
            double slpoF = ((double)(E_da_point7 - E_da_point6)) / ((double)(E_ad_point7 - E_ad_point6));
            double slpoG = ((double)(E_da_point8 - E_da_point7)) / ((double)(E_ad_point8 - E_ad_point7));
            double slpoH = ((double)(E_da_point9 - E_da_point8)) / ((double)(E_ad_point9 - E_ad_point8));
            double slpoI = ((double)(E_da_point10 - E_da_point9)) / ((double)(E_ad_point10 - E_ad_point9));
            double slpoJ = ((double)(E_da_point11 - E_da_point10)) / ((double)(E_ad_point11 - E_ad_point10));
            //
            E_vtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_wtio = MyDevice.myUIT.ConvertFloatToInt32(12.5f);
            E_atio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoA);
            E_btio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoB);
            E_ctio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoC);
            E_dtio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoD);
            E_etio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoE);
            E_ftio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoF);
            E_gtio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoG);
            E_htio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoH);
            E_itio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoI);
            E_jtio = MyDevice.myUIT.ConvertFloatToInt32((float)slpoJ);
            //
            if (E_da_zero < E_da_point2)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point1)) / slpoA) + E_ad_point1;
            }
            else if (E_da_zero < E_da_point3)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point2)) / slpoB) + E_ad_point2;
            }
            else if (E_da_zero < E_da_point4)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point3)) / slpoC) + E_ad_point3;
            }
            else if (E_da_zero < E_da_point5)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point4)) / slpoD) + E_ad_point4;
            }
            else if (E_da_zero < E_da_point6)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point5)) / slpoD) + E_ad_point5;
            }
            else if (E_da_zero < E_da_point7)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point6)) / slpoD) + E_ad_point6;
            }
            else if (E_da_zero < E_da_point8)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point7)) / slpoD) + E_ad_point7;
            }
            else if (E_da_zero < E_da_point9)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point8)) / slpoD) + E_ad_point8;
            }
            else if (E_da_zero < E_da_point10)
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point9)) / slpoD) + E_ad_point9;
            }
            else
            {
                E_ad_zero = (Int32)(((double)(E_da_zero - E_da_point10)) / slpoD) + E_ad_point10;
            }
            //
            if (E_da_full < E_da_point2)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point1)) / slpoA) + E_ad_point1;
            }
            else if (E_da_full < E_da_point3)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point2)) / slpoB) + E_ad_point2;
            }
            else if (E_da_full < E_da_point4)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point3)) / slpoC) + E_ad_point3;
            }
            else if (E_da_full < E_da_point5)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point4)) / slpoD) + E_ad_point4;
            }
            else if (E_da_full < E_da_point6)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point5)) / slpoD) + E_ad_point5;
            }
            else if (E_da_full < E_da_point7)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point6)) / slpoD) + E_ad_point6;
            }
            else if (E_da_full < E_da_point8)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point7)) / slpoD) + E_ad_point7;
            }
            else if (E_da_full < E_da_point9)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point8)) / slpoD) + E_ad_point8;
            }
            else if (E_da_full < E_da_point10)
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point9)) / slpoD) + E_ad_point9;
            }
            else
            {
                E_ad_full = (Int32)(((double)(E_da_full - E_da_point10)) / slpoD) + E_ad_point10;
            }
        }

        //参数更新后重新计算ratio和AD值,然后可以智能检查故障
        public void RefreshRatio()
        {
            //
            switch ((ECVE)E_curve)
            {
                default:
                case ECVE.CTWOPT: RefreshVtio(); break;
                case ECVE.CFITED: RefreshWtio(); break;
                case ECVE.CINTER: RefreshXtio(); break;
                case ECVE.CELTED: RefreshFtio(); break;
                case ECVE.CELTER: RefreshKtio(); break;
            }

            //传感器信号可能故障
            RefreshCalInfo();
        }

        //智能检查故障
        public void RefreshCalInfo()
        {
            int reso;

            R_errSensor = "";
            R_resolution = "";

            //ad_span
            if (!S_ElevenType)
            {
                int diff = E_ad_point5 - E_ad_point1;
                if (diff > int.MaxValue) return;
                reso = Math.Abs(E_ad_point5 - E_ad_point1);
            }
            else
            {
                int diff = E_ad_point11 - E_ad_point1;
                if (diff > int.MaxValue) return;
                reso = Math.Abs(E_ad_point11 - E_ad_point1);
            }

            //有效分辨率
            reso = reso / 500;
            reso = reso * 500;
            switch (S_DeviceType)
            {
                default:
                case TYPE.BE30AH:
                case TYPE.T420:
                case TYPE.TP10:
                    if (reso > 4000)
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString() + ", 模拟输出分辨率1/4000";
                    }
                    else
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString();
                    }
                    break;

                case TYPE.BS420H:
                case TYPE.T8X420H:
                case TYPE.BS600H:
                case TYPE.T4X600H:
                case TYPE.TNP10:
                    if (reso > 60000)
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString() + ", 模拟输出分辨率1/60000";
                    }
                    else
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString();
                    }
                    break;

                case TYPE.TDES:
                    if (reso > 4000)
                    {
                        R_resolution = "数字分辨率1/" + reso.ToString() + ", 模拟输出分辨率1/4000";
                    }
                    else
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString();
                    }
                    break;

                case TYPE.TDSS:
                    if (reso > 60000)
                    {
                        R_resolution = "数字分辨率1/" + reso.ToString() + ", 模拟输出分辨率1/60000";
                    }
                    else
                    {
                        R_resolution = "采样分辨率1/" + reso.ToString();
                    }
                    break;

                case TYPE.TD485:
                case TYPE.TCAN:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    R_resolution = "数字分辨率1/" + reso.ToString();
                    break;
            }

            //标定斜率不够
            if (reso < 500)
            {
                R_errSensor += "标定故障! ";
            }

            //传感器信号可能故障
            switch ((ECVE)E_curve)
            {
                default:
                case ECVE.CTWOPT:
                    if ((E_ad_point1 == 0) && (E_ad_point5 == 0))
                    {
                        R_errSensor += "未标定,或传感器信号可能故障! ";
                    }
                    break;

                case ECVE.CINTER:
                case ECVE.CFITED:
                    if ((E_ad_point1 == 0) && (E_ad_point2 == 0) && (E_ad_point3 == 0) && (E_ad_point4 == 0) && (E_ad_point5 == 0))
                    {
                        R_errSensor += "未标定,或传感器信号可能故障! ";
                    }
                    break;

                case ECVE.CELTED:
                case ECVE.CELTER:
                    if ((E_ad_point1 == 0) && (E_ad_point2 == 0) && (E_ad_point3 == 0) && (E_ad_point4 == 0) && (E_ad_point5 == 0) &&
                        (E_ad_point6 == 0) && (E_ad_point7 == 0) && (E_ad_point8 == 0) && (E_ad_point9 == 0) && (E_ad_point10 == 0) && (E_ad_point11 == 0))
                    {
                        R_errSensor += "未标定,或传感器信号可能故障! ";
                    }
                    break;
            }

            //传感器信号故障,S+和S-极大值或极小值
            float mvdvmax;
            switch ((EPGA)(E_adspeed & 0x0F))
            {
                case EPGA.ADPGA1: mvdvmax = 499.2f; break;
                case EPGA.ADPGA2: mvdvmax = 249.6f; break;
                case EPGA.ADPGA64: mvdvmax = 7.8f; break;
                case EPGA.ADPGA128: mvdvmax = 3.9f; break;
                default: mvdvmax = 3.9f; break;
            }
            if (R_errSensor.Contains("可能故障") == false)
            {
                switch ((ECVE)E_curve)
                {
                    default:
                    case ECVE.CTWOPT:
                        if ((GetAbsfFromInput(E_input1) > mvdvmax) || (GetAbsfFromInput(E_input5) > mvdvmax))
                        {
                            R_errSensor += "传感器信号故障! ";
                        }
                        break;

                    case ECVE.CINTER:
                    case ECVE.CFITED:
                        if ((GetAbsfFromInput(E_input1) > mvdvmax) || (GetAbsfFromInput(E_input2) > mvdvmax) || (GetAbsfFromInput(E_input3) > mvdvmax) || (GetAbsfFromInput(E_input4) > mvdvmax) || (GetAbsfFromInput(E_input5) > mvdvmax))
                        {
                            R_errSensor += "传感器信号故障! ";
                        }
                        break;

                    case ECVE.CELTED:
                    case ECVE.CELTER:
                        if ((GetAbsfFromInput(E_input1) > mvdvmax) || (GetAbsfFromInput(E_input2) > mvdvmax) || (GetAbsfFromInput(E_input3) > mvdvmax) || (GetAbsfFromInput(E_input4) > mvdvmax) || (GetAbsfFromInput(E_input5) > mvdvmax) ||
                            (GetAbsfFromInput(E_input6) > mvdvmax) || (GetAbsfFromInput(E_input7) > mvdvmax) || (GetAbsfFromInput(E_input8) > mvdvmax) || (GetAbsfFromInput(E_input9) > mvdvmax) || (GetAbsfFromInput(E_input10) > mvdvmax) || (GetAbsfFromInput(E_input11) > mvdvmax))
                        {
                            R_errSensor += "传感器信号故障! ";
                        }
                        break;
                }
            }

            //正反斜率多点标定曲线错误
            switch ((ECVE)E_curve)
            {
                default:
                case ECVE.CTWOPT:
                    break;

                case ECVE.CINTER:
                case ECVE.CFITED:
                    if (E_atio >= 0)
                    {
                        if ((E_btio < 0) || (E_ctio < 0) || (E_dtio < 0))
                        {
                            R_errSensor += "多点标定加载线性错误! ";
                        }
                    }
                    else
                    {
                        if ((E_btio > 0) || (E_ctio > 0) || (E_dtio > 0))
                        {
                            R_errSensor += "多点标定加载线性错误! ";
                        }
                    }
                    break;

                case ECVE.CELTED:
                case ECVE.CELTER:
                    if (E_atio >= 0)
                    {
                        if ((E_btio < 0) || (E_ctio < 0) || (E_dtio < 0) || (E_etio < 0) || (E_ftio < 0) || (E_gtio < 0) || (E_htio < 0) || (E_itio < 0) || (E_jtio < 0))
                        {
                            R_errSensor += "多点标定加载线性错误! ";
                        }
                    }
                    else
                    {
                        if ((E_btio > 0) || (E_ctio > 0) || (E_dtio > 0) || (E_etio > 0) || (E_ftio > 0) || (E_gtio > 0) || (E_htio > 0) || (E_itio > 0) || (E_jtio > 0))
                        {
                            R_errSensor += "多点标定加载线性错误! ";
                        }
                    }
                    break;
            }
        }

        //Outype修改后重新计算analog和da_point,比如将10.4(4-20mA)转成2(0-5V)
        public void RefreshOutypeChange(Byte target, Byte org)
        {
            //原来的零满点值
            double orgz;
            double orgf;

            //需要转成的零满点值
            double targetz;
            double targetf;

            //原来的模拟量标定点
            double ang1;
            double ang2;
            double ang3;
            double ang4;
            double ang5;

            if (target == org)
            {
                return;
            }

            switch ((OUT)(org & 0x7))
            {
                default:
                case OUT.UT420:
                    orgz = 4.0d;
                    orgf = 20.0d;
                    break;

                case OUT.UTP05:
                    orgz = 0.0d;
                    orgf = 5.0d;
                    break;

                case OUT.UTP10:
                    orgz = 0.0d;
                    orgf = 10.0d;
                    break;

                case OUT.UTN05:
                    orgz = 0.0d;
                    orgf = 5.0d;
                    break;

                case OUT.UTN10:
                    orgz = 0.0d;
                    orgf = 10.0d;
                    break;

                case OUT.UMASK:
                    return;
            }

            switch ((OUT)(target & 0x7))
            {
                default:
                case OUT.UT420:
                    targetz = 4.0d;
                    targetf = 20.0d;
                    break;

                case OUT.UTP05:
                    targetz = 0.0d;
                    targetf = 5.0d;
                    break;

                case OUT.UTP10:
                    targetz = 0.0d;
                    targetf = 10.0d;
                    break;

                case OUT.UTN05:
                    targetz = 0.0d;
                    targetf = 5.0d;
                    break;

                case OUT.UTN10:
                    targetz = 0.0d;
                    targetf = 10.0d;
                    break;

                case OUT.UMASK:
                    return;
            }

            //原来的模拟量输出值
            ang1 = MyDevice.myUIT.ConvertInt32ToFloat(E_analog1);
            ang2 = MyDevice.myUIT.ConvertInt32ToFloat(E_analog2);
            ang3 = MyDevice.myUIT.ConvertInt32ToFloat(E_analog3);
            ang4 = MyDevice.myUIT.ConvertInt32ToFloat(E_analog4);
            ang5 = MyDevice.myUIT.ConvertInt32ToFloat(E_analog5);

            //新的模拟量输出值
            ang1 = (ang1 - orgz) * (targetf - targetz) / (orgf - orgz) + targetz;
            ang2 = (ang2 - orgz) * (targetf - targetz) / (orgf - orgz) + targetz;
            ang3 = (ang3 - orgz) * (targetf - targetz) / (orgf - orgz) + targetz;
            ang4 = (ang4 - orgz) * (targetf - targetz) / (orgf - orgz) + targetz;
            ang5 = (ang5 - orgz) * (targetf - targetz) / (orgf - orgz) + targetz;

            //更新da_point和da_zero和da_full
            T_analog1 = ang1.ToString();
            T_analog2 = ang2.ToString();
            T_analog3 = ang3.ToString();
            T_analog4 = ang4.ToString();
            T_analog5 = ang5.ToString();
        }

        //adspeed修改后重新计算input和ad_point
        public void RefreshAdspeedChange()
        {
            //
            double inp1;
            double inp2;
            double inp3;
            double inp4;
            double inp5;
            double inp6;
            double inp7;
            double inp8;
            double inp9;
            double inp10;
            double inp11;

            //取出原来的灵敏度值
            inp1 = MyDevice.myUIT.ConvertInt32ToFloat(E_input1);
            inp2 = MyDevice.myUIT.ConvertInt32ToFloat(E_input2);
            inp3 = MyDevice.myUIT.ConvertInt32ToFloat(E_input3);
            inp4 = MyDevice.myUIT.ConvertInt32ToFloat(E_input4);
            inp5 = MyDevice.myUIT.ConvertInt32ToFloat(E_input5);
            inp6 = MyDevice.myUIT.ConvertInt32ToFloat(E_input6);
            inp7 = MyDevice.myUIT.ConvertInt32ToFloat(E_input7);
            inp8 = MyDevice.myUIT.ConvertInt32ToFloat(E_input8);
            inp9 = MyDevice.myUIT.ConvertInt32ToFloat(E_input9);
            inp10 = MyDevice.myUIT.ConvertInt32ToFloat(E_input10);
            inp11 = MyDevice.myUIT.ConvertInt32ToFloat(E_input11);

            //更新ad_point和ad_zero和ad_full
            T_input1 = inp1.ToString();
            T_input2 = inp2.ToString();
            T_input3 = inp3.ToString();
            T_input4 = inp4.ToString();
            T_input5 = inp5.ToString();
            T_input6 = inp6.ToString();
            T_input7 = inp7.ToString();
            T_input8 = inp8.ToString();
            T_input9 = inp9.ToString();
            T_input10 = inp10.ToString();
            T_input11 = inp11.ToString();
        }

        //接收ReceiveDacout后计算
        public void RefreshDacout(Int32 dat)
        {
            Int32 result;
            double outputz;
            double outputm;
            double outputf;
            Int32 da_zero;
            Int32 da_mid;
            Int32 da_full;
            Int32 division;

            //模拟量变送器,根据模拟输出类型和校准值计算,将dacout换算成模拟量
            //模拟量变送器,根据模拟输出类型和校准值计算和数字量量程,将dacout换算成数字量
            //数字量变送器,数字量就是dacout和wt_decilam

            R_overload = false;
            R_grossnet = "";

            //0x58之前的SCT5没有分度值
            if ((E_wt_division <= 0) || (E_wt_division > 100))
            {
                division = 1;
            }
            else
            {
                division = E_wt_division;
            }

            //有些设备没有小数点参数
            if (E_wt_decimal > 10)
            {
                E_wt_decimal = 0;
            }

            switch (S_OutType)
            {
                default:
                case OUT.UT420:
                    outputz = 4.0d;
                    outputm = 12.0d;
                    outputf = 20.0d;
                    da_mid = E_da_zero_05V;
                    da_zero = E_da_zero_4ma;
                    da_full = E_da_full_20ma;

                    if (S_HalfCal)
                    {
                        if (dat < da_mid)
                        {
                            R_datum = ((dat - da_zero) * (outputm - outputz) / (da_mid - da_zero) + outputz);//计算模拟量浮点数,mA值
                        }
                        else
                        {
                            R_datum = ((dat - da_mid) * (outputf - outputm) / (da_full - da_mid) + outputm);//计算模拟量浮点数,mA值
                        }
                    }
                    else
                    {
                        R_datum = ((dat - da_zero) * (outputf - outputz) / (da_full - da_zero) + outputz);//计算模拟量浮点数,mA值
                    }

                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    result = (dat - da_zero) * (E_wt_full - E_wt_zero) / (da_full - da_zero) + E_wt_zero;//计算数字量
                    result = result / division;//格式化分度值
                    R_weight = (result * division / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;

                case OUT.UTP05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    da_zero = E_da_zero_05V;
                    da_full = E_da_full_05V;
                    R_datum = ((dat - da_zero) * (outputf - outputz) / (da_full - da_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    result = (dat - da_zero) * (E_wt_full - E_wt_zero) / (da_full - da_zero) + E_wt_zero;//计算数字量
                    result = result / division;//格式化分度值
                    R_weight = (result * division / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;

                case OUT.UTP10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    da_zero = E_da_zero_10V;
                    da_full = E_da_full_10V;
                    R_datum = ((dat - da_zero) * (outputf - outputz) / (da_full - da_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    result = (dat - da_zero) * (E_wt_full - E_wt_zero) / (da_full - da_zero) + E_wt_zero;//计算数字量
                    result = result / division;//格式化分度值
                    R_weight = (result * division / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;

                case OUT.UTN05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    da_zero = (E_da_zero_N5 + E_da_full_P5) / 2;
                    da_full = E_da_full_P5;
                    R_datum = ((dat - da_zero) * (outputf - outputz) / (da_full - da_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    result = (dat - da_zero) * (E_wt_full - E_wt_zero) / (da_full - da_zero) + E_wt_zero;//计算数字量
                    result = result / division;//格式化分度值
                    R_weight = (result * division / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;

                case OUT.UTN10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    da_zero = (E_da_zero_N10 + E_da_full_P10) / 2;
                    da_full = E_da_full_P10;
                    R_datum = ((dat - da_zero) * (outputf - outputz) / (da_full - da_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    result = (dat - da_zero) * (E_wt_full - E_wt_zero) / (da_full - da_zero) + E_wt_zero;//计算数字量
                    result = result / division;//格式化分度值
                    R_weight = (result * division / Math.Pow(10, E_wt_decimal)).ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;

                case OUT.UMASK:
                    R_datum = (dat / division) / Math.Pow(10, E_wt_decimal) * division;//数字量的浮点数
                    R_output = R_datum.ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    R_weight = R_datum.ToString("f" + E_wt_decimal);//数字量加小数点转字符串
                    break;
            }
        }

        //接收ReceiveAscii后计算
        public void RefreshAscii(String str)
        {
            double outputz;
            double outputf;
            double datum;

            //模拟量变送器,根据模拟输出类型和wt量程,将数字量换算成模拟量
            //模拟量变送器,数字量就是收到的字符串
            //数字量变送器,数字量就是收到的字符串

            if(!double.TryParse(str,out _))
            {
                Console.WriteLine("error in RefreshAscii()");
                return;
            }

            switch (S_OutType)
            {
                default:
                case OUT.UT420:
                    outputz = 4.0d;
                    outputf = 20.0d;
                    R_datum = ((Convert.ToDouble(str.Trim()) - E_wt_zero) * (outputf - outputz) / (E_wt_full - E_wt_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    R_weight = Convert.ToDouble(str.Trim()).ToString("f" + E_wt_decimal).PadLeft(9, ' ');//计算数字量加小数点转字符串
                    break;

                case OUT.UTP05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    R_datum = ((Convert.ToDouble(str.Trim()) - E_wt_zero) * (outputf - outputz) / (E_wt_full - E_wt_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    R_weight = Convert.ToDouble(str.Trim()).ToString("f" + E_wt_decimal).PadLeft(9, ' ');//计算数字量加小数点转字符串
                    break;

                case OUT.UTP10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    R_datum = ((Convert.ToDouble(str.Trim()) - E_wt_zero) * (outputf - outputz) / (E_wt_full - E_wt_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    R_weight = Convert.ToDouble(str.Trim()).ToString("f" + E_wt_decimal).PadLeft(9, ' ');//计算数字量加小数点转字符串
                    break;

                case OUT.UTN05:
                    outputz = 0.0d;
                    outputf = 5.0d;
                    R_datum = ((Convert.ToDouble(str.Trim()) - E_wt_zero) * (outputf - outputz) / (E_wt_full - E_wt_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    R_weight = Convert.ToDouble(str.Trim()).ToString("f" + E_wt_decimal).PadLeft(9, ' ');//计算数字量加小数点转字符串
                    break;

                case OUT.UTN10:
                    outputz = 0.0d;
                    outputf = 10.0d;
                    R_datum = ((Convert.ToDouble(str.Trim()) - E_wt_zero) * (outputf - outputz) / (E_wt_full - E_wt_zero) + outputz);//计算模拟量浮点数
                    R_output = R_datum.ToString("f3");//模拟量加小数点转字符串
                    R_weight = Convert.ToDouble(str.Trim()).ToString("f" + E_wt_decimal).PadLeft(9, ' ');//计算数字量加小数点转字符串
                    break;

                case OUT.UMASK:
                    try
                    {
                        datum = Convert.ToDouble(str.Trim());//数字量的浮点数
                        R_datum = datum;
                    }
                    catch
                    {
                        //半双工RS485发送QNET和QGROSS指令打乱了数据接收,导致str无ToDouble异常
                        //根据baudRate调节QNET和QGROSS周期,降低异常概率
                        //再抛弃异常数据
                    }
                    R_output = R_datum.ToString("f" + E_wt_decimal).PadLeft(9, ' ');//数字量加小数点转字符串
                    R_weight = R_output;//数字量加小数点转字符串
                    break;
            }
        }

        //依据接口重新计算mV/V
        public double RefreshmVDV()
        {
            return R_adcout / S_MVDV;
        }

        //重新计算灵敏度
        public string RefreshSens()
        {
            //灵敏度 sens = delta_V * delta_FullZero / delta_X
            string sens;
            double delta_V = 0;          //actXET.E_input5
            double delta_X = 0;          //actXET.E_analog
            double delta_FullZero = 0;   //量程
            int inputDeci;               //小数位

            //量程
            //TD485, TCAN, iBus, iNet是按数字量的量程来计算的
            //其它带模拟量的型号, 要按模拟量来计算
            switch (S_DeviceType)
            {
                case TYPE.TD485:
                case TYPE.TCAN:
                case TYPE.iBus:
                case TYPE.iNet:
                case TYPE.iStar:
                    delta_FullZero = (E_wt_full / Math.Pow(10, E_wt_decimal)) - (E_wt_zero / Math.Pow(10, E_wt_decimal));
                    break;
                default:
                    switch (S_OutType)
                    {
                        default:
                            break;
                        case OUT.UT420:
                            delta_FullZero = 16;
                            break;
                        case OUT.UTP05:
                        case OUT.UTN05:
                            delta_FullZero = 5;
                            break;
                        case OUT.UTP10:
                        case OUT.UTN10:
                            delta_FullZero = 10;
                            break;
                        case OUT.UMASK:
                            delta_FullZero = (E_wt_full / Math.Pow(10, E_wt_decimal)) - (E_wt_zero / Math.Pow(10, E_wt_decimal));
                            break;
                    }
                    break;
            }
            //输入
            switch ((ECVE)E_curve)
            {
                default:
                    break;
                case ECVE.CTWOPT:
                case ECVE.CFITED:
                case ECVE.CINTER:
                    delta_V = MyDevice.myUIT.ConvertInt32ToFloat(E_input5) - MyDevice.myUIT.ConvertInt32ToFloat(E_input1);
                    delta_X = MyDevice.myUIT.ConvertInt32ToFloat(E_analog5) - MyDevice.myUIT.ConvertInt32ToFloat(E_analog1);
                    break;
                case ECVE.CELTED:
                case ECVE.CELTER:
                    delta_V = MyDevice.myUIT.ConvertInt32ToFloat(E_input11) - MyDevice.myUIT.ConvertInt32ToFloat(E_input1);
                    delta_X = MyDevice.myUIT.ConvertInt32ToFloat(E_analog11) - MyDevice.myUIT.ConvertInt32ToFloat(E_analog1);
                    break;
            }

            if (MyDevice.myUIT.ConvertInt32ToFloat(E_input1).ToString().IndexOf('.') > 0)
            {
                //获取E_input1的小数点位数
                inputDeci = MyDevice.myUIT.ConvertInt32ToFloat(E_input1).ToString().Length - MyDevice.myUIT.ConvertInt32ToFloat(E_input1).ToString().IndexOf('.') - 1;
            }
            else
            {
                inputDeci = 0;
            }
            //用输入的灵敏度,换算成量程的灵敏度
            if (delta_X != 0)
            {
                double sensData = Math.Abs(delta_V * delta_FullZero / delta_X);
                if (((sensData.ToString().Length - sensData.ToString().IndexOf(".") - 1) > 0) && inputDeci == 0)
                {
                    //E_input1小数位为0，但计算出的量程的灵敏度小数位不为0时
                    sens = sensData.ToString("0.0000000") + " mV/V";
                }
                else
                {
                    //使计算出的sens小数点位数,和E_input1或E的小数点位数保持一致
                    sens = sensData.ToString("f" + inputDeci) + " mV/V";
                }
            }
            else
            {
                sens = "----";
            }
            return sens;
        }
    }
}

