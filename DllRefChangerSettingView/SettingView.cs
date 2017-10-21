using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChangerSettingView
{
    public class SettingView
    {
        public static string SolutionFullName { get; private set; }

        public static void ShowSettingWindow(string solutionPath)
        {
            SolutionFullName = solutionPath;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
