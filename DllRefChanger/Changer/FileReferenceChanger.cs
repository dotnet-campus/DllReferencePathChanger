using System;
using System.IO;

namespace DllRefChanger.Changer
{
    /// <summary>
    /// 直接更改Debug目录中的dll文件，
    /// 针对于最简单的文件复制类的引用
    /// </summary>
    public class FileReferenceChanger : IReferenceChanger
    {

        public FileReferenceChanger(string solutionPath, string targetDllPath)
        {
            SolutionPath = solutionPath;
            TargetDllPath = targetDllPath;

            if (!File.Exists(solutionPath) || !File.Exists(targetDllPath))
            {
                throw new FileNotFoundException();
            }
        }

        public string SolutionPath { get; set; }

        /// <summary>
        /// 新的DLL文件引用
        /// </summary>
        public string TargetDllPath { get; }

        private string _tempFilePath;
        private bool _hasBackuped = false;

        public string Message { get; private set; }

        public void Change()
        {
            Replace(Path.GetFileName(TargetDllPath), TargetDllPath);
        }

        public void UndoChange()
        {
            if (!CanUndoChange())
            {
                throw new InvalidOperationException($"不存在备份的DLL文件 {_tempFilePath}");
            }
            Replace(Path.GetFileName(TargetDllPath), _tempFilePath);
        }

        public bool CanUndoChange()
        {
            return !string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath);
        }

        private void BackupSourceDll(string sourceDllFullName)
        {
            // 先讲将要被替换的文件保存到临时目录
            string tempPath = Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            _tempFilePath = Path.Combine(tempPath, Path.GetFileName(sourceDllFullName));
            File.Copy(sourceDllFullName, _tempFilePath, true);
        }

        private void Replace(string fileName, string targetFile)
        {

            DirectoryInfo rootDir = Directory.GetParent(SolutionPath);

            Scan(rootDir);

            void Scan(DirectoryInfo dir)
            {
                // 只替换 Debug 目录下的dll文件
                if (dir.Name.ToLower() == "debug")
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        if (file.Name == fileName)
                        {
                            if (!_hasBackuped)
                            {
                                BackupSourceDll(file.FullName);
                                _hasBackuped = true;
                            }
                            File.Copy(targetFile, file.FullName, true);
                        }

                    }
                }

                foreach (DirectoryInfo directory in dir.GetDirectories())
                {
                    Scan(directory);
                }
            }
        }

    }
}
