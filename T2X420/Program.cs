using Base.UI;
using System;
using System.Windows.Forms;

namespace T2X420
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MT2X420Device());
        }
    }
}
