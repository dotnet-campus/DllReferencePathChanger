using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DllRefChanger;
using MahApps.Metro.Controls;

namespace DllRefChangerSettingView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {      
        public MainWindow()
        {
            InitializeComponent();   
            this.Closing+= OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            MainViewModel mainViewModel = this.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                if (!mainViewModel.HasUndo)
                {
                    if (MessageBoxResult.Cancel ==
                        MessageBox.Show(
                            "尚未对更改做出撤销操作，是否退出?", 
                            "确定退出", 
                            MessageBoxButton.OKCancel, 
                            MessageBoxImage.Warning))
                    {
                        cancelEventArgs.Cancel = true;
                    }
                }
            }
        }
    }
}
