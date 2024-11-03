using Base.UI;
using Base.UI.MenuFile;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Model;
using Library;

//Lumi 20240409

namespace RTU
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //判断是否开机自启
            bool isAutoStart = false;
            try
            {
                string mePath = MyDevice.D_datPath + @"\RTUConfig.ini";
                //读取文件或创建文件
                if (File.Exists(MyDevice.D_datPath + "\\" + "RTUConfig.ini"))
                {
                    String[] meLines = File.ReadAllLines(MyDevice.D_datPath + "\\" + "RTUConfig.ini");
                    foreach (String line in meLines)
                    {
                        if (line.StartsWith("AutoStart="))
                        {
                            switch (Convert.ToInt32(line.Split('=')[1]))
                            {
                                default:
                                case 0:
                                    isAutoStart = false;
                                    break;
                                case 1:
                                    isAutoStart = true;
                                    break;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    isAutoStart = false;
                }
            }
            catch
            {
                isAutoStart = false;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                InitializationRegister();
                AutoStart.AutoStartByRK("RTUAutoStart", isAutoStart); //开机自启
                if (isAutoStart)
                {
                    //开机自启时，不需要登录
                    Application.Run(new RTUDevice()); //主窗体
                }
                else
                {
                    MenuAccountForm frm = new MenuAccountForm(); //登录
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new RTUDevice()); //主窗体
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
        }

        private static void InitializationRegister()
        {
            //验证MAC地址
            Int64 net_Mac = 0;
            Int64 net_Var = 0;
            //验证regedit
            Int64 reg_Mac = 0;
            Int64 reg_Var = 0;
            //验证C盘文件
            Int64 sys_Mac = 0;
            Int64 sys_Var = 0;
            Int32 sys_num = 0;
            //验证本地文件
            Int64 use_Mac = 0;
            Int64 use_Var = 0;
            Int32 use_num = 0;

            //验证MAC地址
            string macAddress = "";
            Process myProcess = null;
            StreamReader reader = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo("cmd.exe");

                start.FileName = "ipconfig";
                start.Arguments = "/all";
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.RedirectStandardInput = true;
                start.UseShellExecute = false;
                myProcess = Process.Start(start);
                reader = myProcess.StandardOutput;
                string line = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    if (line.ToLower().IndexOf("physical address") > 0 || line.ToLower().IndexOf("物理地址") > 0)
                    {
                        int index = line.IndexOf(":");
                        index += 2;
                        macAddress = line.Substring(index);
                        macAddress = macAddress.Replace('-', ':');
                        break;
                    }
                    line = reader.ReadLine();
                }
            }
            catch
            {

            }
            finally
            {
                if (myProcess != null)
                {
                    reader.ReadToEnd();
                    myProcess.WaitForExit();
                    myProcess.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }

            if (macAddress.Length == 17)
            {
                macAddress = macAddress.Replace(":", "");
                net_Mac = Convert.ToInt64(macAddress, 16);
                net_Var = net_Mac;
                while ((net_Var % 2) == 0)
                {
                    net_Var = net_Var / 2;
                }
                while ((net_Var % 3) == 0)
                {
                    net_Var = net_Var / 3;
                }
                while ((net_Var % 5) == 0)
                {
                    net_Var = net_Var / 5;
                }
                while ((net_Var % 7) == 0)
                {
                    net_Var = net_Var / 7;
                }
            }

            //验证regedit
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("software");
            string[] names = myKey.GetSubKeyNames();
            foreach (string keyName in names)
            {
                if (keyName == "WinES")
                {
                    myKey = Registry.LocalMachine.OpenSubKey("software\\WinES");
                    reg_Mac = Convert.ToInt64(myKey.GetValue("input").ToString());
                    reg_Var = Convert.ToInt64(myKey.GetValue("ouput").ToString());
                }
            }
            myKey.Close();

            //验证C盘文件
            if (!File.Exists("C:\\Windows\\user.dat"))
            {
                if (File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
                {
                    try
                    {
                        File.Copy((Application.StartupPath + @"\dat" + @"\user.num"), ("C:\\Windows\\user.dat"), true);
                    }
                    catch
                    {

                    }
                }
            }
            if (File.Exists("C:\\Windows\\user.dat"))
            {
                //读取用户信息
                FileStream meFS = new FileStream("C:\\Windows\\user.dat", FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    sys_Mac = meRead.ReadInt64();
                    sys_Var = meRead.ReadInt64();
                    sys_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }

            //验证本地文件
            if (!File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
            {
                if (File.Exists("C:\\Windows\\user.dat"))
                {
                    if (!Directory.Exists(Application.StartupPath + @"\dat"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + @"\dat");
                    }
                    File.Copy(("C:\\Windows\\user.dat"), (Application.StartupPath + @"\dat" + @"\user.num"), true);
                }
            }
            if (File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
            {
                //读取用户信息
                FileStream meFS = new FileStream((Application.StartupPath + @"\dat" + @"\user.num"), FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    use_Mac = meRead.ReadInt64();
                    use_Var = meRead.ReadInt64();
                    use_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }

            //注册分析
            if ((net_Mac == reg_Mac) && (net_Var == reg_Var) && (sys_Mac == use_Mac) && (sys_Var == use_Var) && (net_Mac == use_Mac) && (net_Var == use_Var))
            {
                MyDevice.myPC = 1;
                MyDevice.myMac = sys_Mac;
                MyDevice.myVar = sys_Var;
            }
            else
            {
            }
        }
    }
}
