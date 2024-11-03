using Microsoft.Win32;
using System;
using System.Windows.Forms;

//Lumi 20240401

namespace Library
{
    public static class AutoStart
    {
        /// <summary>
        /// 使用修改注册表的方式实现开机自启动
        /// </summary>
        /// <param name="SoftWare">要设置软件名称，有唯一性要求，最好起特别一些</param>
        /// <param name="isAuto">是否开机自启动</param>
        public static void AutoStartByRK(string SoftWare, bool isAuto = true)
        {
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser;

            try
            {
                using (RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (isAuto)
                    {
                        // 检测是否之前有设置自启动了，如果设置了，就看值是否一样
                        string old_path = (string)rk2.GetValue(SoftWare);

                        if (old_path == null || !path.Equals(old_path))
                        {
                            rk2.SetValue(SoftWare, path);
                        }
                    }
                    else
                    {
                        // 取消开机自启动
                        rk2.DeleteValue(SoftWare, false);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("权限不足，无法设置开机自启动: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置开机自启动时发生错误: " + ex.Message);
            }
        }
    }
}
