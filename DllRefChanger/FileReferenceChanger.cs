using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger
{
    /// <summary>
    /// 直接更改Debug目录中的dll文件，
    /// 针对于最简单的文件复制类的引用
    /// </summary>
    public class FileReferenceChanger : IReferenceChanger
    {
        public FileReferenceChanger(string solutionPath, string sourceDllPath, string targetDllPath)
        {
            SolutionPath = solutionPath;
            SourceDllPath = sourceDllPath;
            TargetDllPath = targetDllPath;

            if (!File.Exists(solutionPath) || !File.Exists(sourceDllPath) || !File.Exists(targetDllPath))
            {
                throw new FileNotFoundException();
            }
            if (Path.GetFileName(sourceDllPath) != Path.GetFileName(targetDllPath))
            {
                throw new ArgumentException("源于目标的文件名不一致");
            }

        }

        public string SolutionPath { get; set; }

        /// <summary>
        /// 原来的DLL文件引用
        /// </summary>
        public string SourceDllPath { get; }

        /// <summary>
        /// 新的DLL文件引用
        /// </summary>
        public string TargetDllPath { get; }

        private string _tempFilePath;

        public void Change()
        {
            // 先讲将要被替换的文件保存到临时目录
            string tempPath = Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            _tempFilePath = Path.Combine(tempPath, Path.GetFileName(SourceDllPath));
            File.Copy(SourceDllPath, _tempFilePath, true);

            Replace(Path.GetFileName(SourceDllPath),TargetDllPath);

        }

        public void UndoChange()
        {
            Replace(Path.GetFileName(TargetDllPath), _tempFilePath);
        }

        private void Replace(string fileName, string targetFile)
        {
            
            DirectoryInfo rootDir = Directory.GetParent(SolutionPath);

            Scan(rootDir);

            void Scan(DirectoryInfo dir)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Name == fileName)
                    {
                        File.Copy(targetFile, file.FullName, true);
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
