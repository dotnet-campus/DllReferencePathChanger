using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DllRefChanger
{
    public abstract class CsprojFileChanger : IReferenceChanger
    {
        protected CsprojFileChanger(
            string solutionPath,
            string solutionName,
            string dllName,
            string sourceDllVersion,
            string targetDllPath
            )
        {
            SolutionConfig = new SolutionConfig()
            {
                AbsolutePath = solutionPath,
                Name = solutionName,
                DllName = dllName,
                SourceDllVersion = sourceDllVersion,
                TargetDllAbsolutePath = targetDllPath
            };
            CheckFile();
        }

        protected CsprojFileChanger(
            string solutionPath,
            string targetDllPath)
        {
            SolutionConfig = new SolutionConfig()
            {
                AbsolutePath = solutionPath,
                Name = Path.GetFileNameWithoutExtension(solutionPath),
                DllName = Path.GetFileNameWithoutExtension(targetDllPath),
                SourceDllVersion = string.Empty,
                TargetDllAbsolutePath = targetDllPath
            };
            CheckFile();
        }

        public SolutionConfig SolutionConfig { get; }

        public void SetTargetDllPath(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("你逗我！这个文件不存在。", path);
            }
            SolutionConfig.TargetDllAbsolutePath = path;
        }

        public string Message { get; private set; }

        public void Change()
        {
            CheckCanChange();

            GitExecuter gitExecuter = new GitExecuter(GitExecuter.GetGitExePath(), SolutionConfig.AbsolutePath);

            // 有些工程的保存路径不按套路出牌，那就搜索全部git管理目录下的文件夹
            DirectoryInfo rootDir = Directory.GetParent(gitExecuter.GitDir);
            Scan(rootDir);

            void Scan(DirectoryInfo dir)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Extension == ".csproj")
                    {
                        try
                        {
                            ChangeToTarget(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Message += $"#Fail : {file.Name}; {ex.Message}\n";
                        }

                    }
                }

                foreach (DirectoryInfo innerDir in dir.GetDirectories())
                {
                    if (innerDir.Name.StartsWith("."))
                    {
                        // .vs .git ignore
                        continue;
                    }
                    Scan(innerDir);
                }
            }

        }

        public void UndoChange()
        {
            GitExecuter gitExecuter = new GitExecuter(GitExecuter.GetGitExePath(), SolutionConfig.AbsolutePath);

            bool success = gitExecuter.Execute("checkout *.csproj", out string result, out string err);

            if (!success)
            {
                throw new InvalidOperationException($"哎呀，恢复csproj文件时出错了：{err}");
            }

        }

        private void CheckFile()
        {
            if (!File.Exists(SolutionConfig.AbsolutePath))
            {
                throw new FileNotFoundException("文件不存在", SolutionConfig.AbsolutePath);
            }
            if (!File.Exists(SolutionConfig.TargetDllAbsolutePath))
            {
                throw new FileNotFoundException("文件不存在", SolutionConfig.TargetDllAbsolutePath);
            }
        }

        private void CheckCanChange()
        {
            GitExecuter gitExecuter = new GitExecuter(GitExecuter.GetGitExePath(), SolutionConfig.AbsolutePath);

            bool success = gitExecuter.Execute("status", out string result, out string err);

            if (!success)
            {
                throw new InvalidOperationException(err);
            }

            if (result.Contains(".csproj"))
            {
                throw new InvalidOperationException("有尚未提交的对csproj文件的修改，请先撤销或者提交这些修改");
            }
        }

        protected abstract void ChangeToTarget(string csprojFile);


    }
}
