using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DllRefChanger;
using Microsoft.Win32;

namespace DllRefChangerSettingView
{
    class MainViewModel : ObservableObject
    {

        public MainViewModel()
        {
            if (SettingView.StartupFromVsix)
            {
                SolutionPath = SettingView.SolutionFullName;
                if (!string.IsNullOrWhiteSpace(SettingView.DllFileName))
                {
                    DllPath = SettingView.DllFileName;
                }
            }
            MessageInfo =
                "HintPath引用替换 是替换csproj中的引用路径，一般用于替换Nuget引用\n" +
                "文件替换 是简单地对Debug目录下的DLL文件进行替换";
        }

        private string _solutionPath;
        private string _dllPath;
        private bool _isReplaceHintRef = true;
        private bool _isReplaceFile;
        private string _messageInfo;
        private bool _hasUndo = true;

        public string SolutionPath
        {
            get => _solutionPath;
            set
            {
                _solutionPath = value;
                OnPropertyChanged();
            }
        }

        public string DllPath
        {
            get => _dllPath;
            set
            {
                _dllPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoadFromVsix => SettingView.StartupFromVsix;

        public bool IsLoadNormal => !SettingView.StartupFromVsix;

        public bool IsReplaceHintRef
        {
            get => _isReplaceHintRef;
            set
            {
                _isReplaceHintRef = value;
                OnPropertyChanged();
            }
        }

        public bool IsReplaceFile
        {
            get => _isReplaceFile;
            set
            {
                _isReplaceFile = value;
                OnPropertyChanged();
            }
        }

        public string MessageInfo
        {
            get => _messageInfo;
            set
            {
                _messageInfo = value;
                OnPropertyChanged();
            }
        }

        public bool HasUndo
        {
            get => _hasUndo;
            set
            {
                _hasUndo = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSlnFileCommand => new RelayCommand(OpenSlnFile, () => !IsLoadFromVsix);
        public ICommand OpenDllFileCommand => new RelayCommand(OpenDllFile);

        public ICommand ReplaceDllCommand =>new RelayCommand(ReplaceDll,CanReplaceDll);
        public ICommand UndoReplaceCommand => new RelayCommand(UndoReplaceDll,()=>!HasUndo);

        IReferenceChanger _referenceChanger;

        private void OpenSlnFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "解决方案文件(*.sln)|*.sln" ,
                DefaultExt = ".sln",
                Multiselect = false,
            };
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                SolutionPath = ofd.FileName;
            }
        }

        private void OpenDllFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "DLL文件(*.dll)|*.dll",
                DefaultExt = ".dll",
                Multiselect = false,
            };
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                DllPath = ofd.FileName;
            }
        }

        private void ReplaceDll()
        {
            try
            {
                if (IsReplaceHintRef)
                {
                    _referenceChanger = new HintReferenceChanger(SolutionPath, DllPath);
                }
                else if (IsReplaceFile)
                {
                    _referenceChanger = new FileReferenceChanger(SolutionPath, DllPath);
                }
                _referenceChanger.Change();
            }
            catch (Exception ex)
            {
                MessageInfo = ex.Message;
                MessageBox.Show(MessageInfo, "替换发生错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            HasUndo = false;
            MessageInfo = "替换成功";
        }

        private bool CanReplaceDll()
        {
            return File.Exists(SolutionPath) && File.Exists(DllPath);
        }

        private void UndoReplaceDll()
        {
            try
            {
                _referenceChanger.UndoChange();
            }
            catch (Exception ex)
            {
                MessageInfo = ex.Message;
                MessageBox.Show(MessageInfo, "撤销替换发生错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            HasUndo = true;
            MessageInfo = "撤销成功";
        }

        public ICommand HelpCommand=>new RelayCommand(() =>
        {
            System.Diagnostics.Process.Start("https://github.com/JasonGrass/DllReferencePathChanger");
        });
    }
}
