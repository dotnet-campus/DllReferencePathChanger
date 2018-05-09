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
            MessageInfo = "";
        }

        private string _solutionPath;
        private string _newFilePath;
        private bool _isReplaceHintRef;
        private bool _isReplaceFile;
        private bool _isReplaceCsproj = true;
        private string _messageInfo;
        private bool _canUndo = true;
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

        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                _canUndo = value;
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
                    if (_referenceChanger != null)
                    {
                        _referenceChanger.IsUseGitWhenUndo = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenSlnFileCommand => new RelayCommand(OpenSlnFile, () => !IsLoadFromVsix);
        public ICommand OpenDllFileCommand => new RelayCommand(OpenDllFile);

        public ICommand ReplaceDllCommand =>new RelayCommand(ReplaceDll,CanReplaceDll);
        public ICommand UndoReplaceCommand => new RelayCommand(UndoReplaceDll, () => CanUndo);

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
                _referenceChanger.IsUseGitWhenUndo = AdvancedMode;
                _referenceChanger.DoChange();
            }
            catch (Exception ex)
            {
                MessageInfo = ex.Message;
                MessageBox.Show(MessageInfo, "替换发生错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CanUndo = true;
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
            CanUndo = false;
            MessageInfo = "撤销成功";
        }

        public ICommand HelpCommand=>new RelayCommand(() =>
        {
            System.Diagnostics.Process.Start("https://github.com/JasonGrass/DllReferencePathChanger");
        });
    }
}
