using System;
using System.Collections.Generic;
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

        HintReferenceChanger _hintReferenceChanger;
        FileReferenceChanger _fileReferenceChanger;
        public MainWindow()
        {
            InitializeComponent();

            //string solution = @"E:\CVTEWORK\easinote - drop\Code\EasiNote.sln";
            //string target = @"E:\Gitlab\Cvte.Storage\Code\Cvte.Storage\bin\Debug\Cvte.Storage.dll";
            //_hintReferenceChanger = new HintReferenceChanger(solution, target);

            //string source = @"E:\Gitlab\Cvte.Storage\Code\Cvte.Storage\bin\Debug\Cvte.Storage.dll";
            //target = @"C:\Users\JunjieLiu\Desktop\新建文件夹\Cvte.Storage.dll";
            //_fileReferenceChanger = new FileReferenceChanger(solution, source, target);
            
        }
    }
}
