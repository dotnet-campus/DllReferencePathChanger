using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DllRefChanger;
using DllRefChanger.Changer;
using DllRefChanger.Core;
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
                    NewFilePath = SettingView.DllFileName;
                }
            }
            MessageInfo =
                "csproj引用替换:\n将引用dll的方式转换为工程引用的方式，调试CBB源码将非常方便\n" +
                "HintPath引用替换:\n替换csproj中的引用路径，一般用于替换Nuget引用\n" +
                "文件替换:\n简单地对Debug目录下的DLL文件进行替换\n\n" +
                "特别注意：\n" +
                "csproj 和 HintPath 引用替换是对 csproj 文件进行修改，" +
                "撤销操作是使用 git checkout 命令撤销所有对 csproj 和 sln 文件的修改，如果有文件添加和删除等影响 csproj 文件的操作，" +
                "请谨慎使用自动撤销。\n" +
                "再啰嗦一句：csproj 引用替换功能需要安装 .net core sdk";
        }

        private string _solutionPath;
        private string _newFilePath;
        private bool _isReplaceHintRef;
        private bool _isReplaceFile;
        private bool _isReplaceCsproj = true;
        private string _messageInfo;
        private bool _hasUndo = true;
        private bool _advancedMode;

        public string SolutionPath
        {
            get => _solutionPath;
            set
            {
                _solutionPath = value;
                OnPropertyChanged();
            }
        }

        public string NewFilePath
        {
            get => _newFilePath;
            set
            {
                _newFilePath = value;
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
                if (Path.GetExtension(NewFilePath) != ".dll")
                {
                    NewFilePath = string.Empty;
                }
                OnPropertyChanged();
            }
        }

        public bool IsReplaceFile
        {
            get => _isReplaceFile;
            set
            {
                _isReplaceFile = value;
                if (Path.GetExtension(NewFilePath) != ".dll")
                {
                    NewFilePath = string.Empty;
                }
                OnPropertyChanged();
            }
        }    

        public bool IsReplaceCsproj
        {
            get => _isReplaceCsproj;
            set
            {
                if (_isReplaceCsproj != value)
                {
                    _isReplaceCsproj = value;
                    if (Path.GetExtension(NewFilePath) != ".csproj")
                    {
                        NewFilePath = string.Empty;
                    }
                    OnPropertyChanged();
                }
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
    
        public bool AdvancedMode
        {
            get => _advancedMode;
            set
            {
                if (_advancedMode != value)
                {
                    _advancedMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenSlnFileCommand => new RelayCommand(OpenSlnFile, () => !IsLoadFromVsix);
        public ICommand OpenDllFileCommand => new RelayCommand(OpenDllFile);

        public ICommand ReplaceDllCommand =>new RelayCommand(ReplaceDll,CanReplaceDll);
        public ICommand UndoReplaceCommand => new RelayCommand(UndoReplaceDll, () => !HasUndo && !AdvancedMode);

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
            string filter = "Dll File(*.dll)|*.dll|Exe File(*.exe)|*.exe|All(*.*)|*.*";
            if (IsReplaceCsproj)
            {
                filter = "csproj File(*.csproj)|*.csproj|All(*.*)|*.*";
            }
            else if (IsReplaceHintRef)
            {
                filter = "Dll File(*.dll)|*.dll|All(*.*)|*.*";
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = filter,
                DefaultExt = ".dll",
                Multiselect = false,
            };
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                NewFilePath = ofd.FileName;
            }
        }

        private void ReplaceDll()
        {
            try
            {
                if (IsReplaceHintRef)
                {
                    _referenceChanger = new HintReferenceChanger(SolutionPath, NewFilePath);
                }
                else if (IsReplaceFile)
                {
                    //_referenceChanger = new FileReferenceChanger(SolutionPath, NewFilePath);
                }
                else if (IsReplaceCsproj)
                {
                    _referenceChanger = new ProjReferneceChanger(SolutionPath, NewFilePath);              
                }
                _referenceChanger.UseDefaultCheckCanChange = !AdvancedMode;
                _referenceChanger.DoChange();
            }
            catch (Exception ex)
            {
                MessageInfo = ex.Message;
                MessageBox.Show(MessageInfo, "替换发生错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            HasUndo = false;
            MessageInfo = "替换完成\n" + _referenceChanger.Message;
        }

        private bool CanReplaceDll()
        {
            return File.Exists(SolutionPath) && File.Exists(NewFilePath);
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
