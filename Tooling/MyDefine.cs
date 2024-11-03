using DMM6500;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

//Tong Ziyun 20230428

namespace Tooling
{
    //定义委托
    public delegate void freshHandler();

    public enum RXSTP : Byte //通讯接收状态机,找出帧头帧尾
    {
        //02 xx xx 03

        NUL, //null
        STX, //0x02 或 '=' 或 ID
        ACK, //0x29 或 '0x0D' 或 XL
        ETX, //0x03 或 '0x0A' 或 XH
    }

    public static class Constants
    {
        //Self-Uart
        public const Byte VOLTAGE = 0x51; //电压模式
        public const Byte CURRENT = 0x52; //电流模式
        public const Byte ZERO = 0x53; //零点采
        public const Byte FULL = 0x54; //满点采
        public const Byte OPEN = 0x55; //24V开
        public const Byte CLOSE = 0x56; //24V关
    }

    public static class MyDefine
    {
        //User Event 定义事件
        public static event freshHandler myUpdate;

        //设备连接的接口
        public static JIG myJIG;

        #region API函数声明-必须放在类中

        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <param name="iFrequency">声音频率（从37Hz到32767Hz）。在windows95中忽略</param>  
        /// <param name="iDuration">声音的持续时间，以毫秒为单位。</param>  
        [DllImport("Kernel32.dll")]//引入命名空间 using System.Runtime.InteropServices;  
        public static extern bool Beep(int frequency, int duration);

        #endregion

        /// <summary>
        /// DMM6500 连接字符串
        /// </summary>
        public static string DmmUsbTring = "USB::0x05E6::0x6500::04455690::INSTR";

        /// <summary>
        /// DMM6500操作对象
        /// </summary>
        public static Dmm6500 Dm6500;

        /// <summary>
        /// 读取配置文件参数设置
        /// </summary>
        /// <returns></returns>
        public static bool InitConfig()
        {
            try
            {
                ////读取配置文件
                string paths = Path.Combine(Environment.CurrentDirectory, "config\\Main.ini");
                StringBuilder stringBuilder = new StringBuilder(1024);
                GetPrivateProfileString("DMM", "Dmm6500", "USB::0x05E6::0x6500::04455690::INSTR", stringBuilder, 1024, paths);
                DmmUsbTring = stringBuilder.ToString();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 执行委托
        /// </summary>
        public static void callDelegate()
        {
            //委托
            if (myUpdate != null)
            {
                myUpdate();
            }
        }
    }
}
