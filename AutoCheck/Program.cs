using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace iWay.AutoCheck
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            if (Settings.RESTRICT_INSTANCE_COUNT)
            {
                string processName = Process.GetCurrentProcess().ProcessName;
                int processCount = Process.GetProcessesByName(processName).Length;
                if (processCount > 1)
                {
                    return;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Window());
        }
    }
}
