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

        public static bool StartupFromVsix { get; private set; } = false;

        public static void ShowSettingWindow(string solutionPath)
        {
            StartupFromVsix = true;
            SolutionFullName = solutionPath;           
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
