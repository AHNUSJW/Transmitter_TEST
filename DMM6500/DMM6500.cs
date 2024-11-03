using TekVISANet;

//Tong Ziyun 20230428

namespace DMM6500
{
    public class Dmm6500
    {
        private VISA _TVA = new VISA(); //驱动程序软件架构对象

        public bool _IsOpen = false; //标志是否连接DMM6500

        public bool GetDeviceStatus()
        {
            return _TVA.Status.Equals(TekVISADefs.Status.SUCCESS) ? true : false;
        }

        /// <summary>
        /// 连接DMM6500
        /// </summary>
        /// <param name="usbStringAddress">连接字符串</param>
        /// <returns></returns>
        public bool OpenDmm(string connectString)
        {
            if (_TVA.Open(connectString))
            {
                _IsOpen = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭DMM6500
        /// </summary>
        /// <returns></returns>
        public bool CloseDmm()
        {
            try
            {
                _IsOpen = false;
                return true;
            }
            finally
            {
                //关闭
                _TVA.Close();
                //销毁
                _TVA.Dispose();
            }
        }

        /// <summary>
        /// 查找所有VISA资源设备
        /// </summary>
        /// <returns></returns>
        public System.Collections.ArrayList FindVisA()
        {
            System.Collections.ArrayList instList;

            _TVA.FindResources("?*", out instList);
            return instList;
        }

        /// <summary>
        /// 读取查询Value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Query(out double value)
        {
            if (_TVA.Query(":MEASure?", out value))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 查询使用的指令集
        /// </summary>
        /// <returns></returns>
        public string QueryAssemble()
        {
            string value;
            if (_TVA.Query("*Lang?", out value))
            {
                return value;
            }
            return value;
        }

        #region 切换测量类型

        //SENSe:DIGitize:FUNCtion\s"VOLT" 切换到 Digi V
        /// <summary>
        /// 切换测量切换到 Digi V
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeDIGI_V()
        {
            if (_TVA.Write(":SENSe:DIGitize:FUNCtion \"VOLT\""))
            {
                return true;
            }
            return false;
        }

        //SENSe:DIGitize:FUNCtion\s"CURR" 切换到 Digi I
        /// <summary>
        /// 切换测量 Digi I
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeDIGI_I()
        {
            if (_TVA.Write(":SENSe:DIGitize:FUNCtion \"CURR\""))
            {
                return true;
            }
            return false;
        }

        //:SENSe:FUNCtion "VOLTage"    :SENS:FUNC\s"VOLT"  切换 DCV
        /// <summary>
        /// 切换测量类型DCV
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeDCV()
        {
            if (_TVA.Write(":SENS:FUNC \"VOLT\""))
            {
                return true;
            }
            return false;
        }

        //:SENSe:FUNCtion "current"	:SENS:FUNC\s"curr"切换DCI
        /// <summary>
        /// 切换测量切换DCI
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeDCI()
        {
            if (_TVA.Write(":SENS:FUNC \"CURR\""))
            {
                return true;
            }
            return false;
        }

        //:SENS:FUNC\s"VOLT:AC"		切换 ACV
        /// <summary>
        /// 切换测量切换 ACV
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeACV()
        {
            if (_TVA.Write(":SENS:FUNC \"VOLT:AC\""))
            {
                return true;
            }
            return false;
        }

        //:SENS:FUNC\s"Curr:AC"		切换ACI
        /// <summary>
        /// 切换测量切换ACI
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeACI()
        {
            if (_TVA.Write(":SENS:FUNC \"Curr:AC\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"TEMP"                TEMP
        /// <summary>
        /// 切换测量类型TEMP
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeTEMP()
        {
            if (_TVA.Write(":SENS:FUNC \"TEMP\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"CONT"  			COnt
        /// <summary>
        /// 切换测量类型CONT
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeCONT()
        {
            if (_TVA.Write(":SENS:FUNC \"CONT\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"diode"			diode
        /// <summary>
        /// 切换测量类型diode
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeDIODE()
        {
            if (_TVA.Write(":SENS:FUNC \"diode\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"period"			period
        /// <summary>
        /// 切换测量类型period
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypePERIOD()
        {
            if (_TVA.Write(":SENS:FUNC \"period\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"cap"			cap
        /// <summary>
        /// 切换测量类型cap
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeCAP()
        {
            if (_TVA.Write(":SENS:FUNC \"cap\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"Freq"			freq
        /// <summary>
        /// 切换测量类型freq
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeFREQ()
        {
            if (_TVA.Write(":SENS:FUNC \"freq\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"VOLT:RAT"    切换Ratio
        /// <summary>
        /// 切换测量类型Ratio
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeRATIO()
        {
            if (_TVA.Write(":SENS:FUNC \"VOLT:RAT\""))
            {
                return true;
            }
            return false;
        }

        //:SENSe:FUNCtion\s"FRESistance"  4W Ω
        /// <summary>
        /// 切换测量类型4W Ω
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeFRESI()
        {
            if (_TVA.Write(":SENS:FUNC \"FRESistance\""))
            {
                return true;
            }
            return false;
        }

        //FUNC\s"RESistance"				2W Ω
        /// <summary>
        /// 切换测量类型2W Ω
        /// </summary>
        /// <returns></returns>
        public bool SwitchScanTypeRESI()
        {
            if (_TVA.Write(":SENS:FUNC \"RESistance\""))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
