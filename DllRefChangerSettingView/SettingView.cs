using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChangerSettingView
{
    public static class SettingView
    {
        public static string SolutionFullName { get; private set; }

        public static string DllFileName { get; private set; }

        public static bool StartupFromVsix { get; private set; } = false;

        public static void ShowSettingWindow(string solutionPath, string dllFile = null)
        {
            StartupFromVsix = true;
            SolutionFullName = solutionPath;
            DllFileName = dllFile;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
