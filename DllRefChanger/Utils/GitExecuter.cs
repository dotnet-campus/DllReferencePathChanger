using System;
using System.IO;

namespace DllRefChanger.Utils
{
    public class GitExecuter
    {
        public GitExecuter(string gitPath, string projectPath)
        {
            if (File.Exists(projectPath))
            {
                // 将文件转换为目录
                projectPath = Directory.GetParent(projectPath).FullName;
            }
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException(projectPath);
            }

            GitPath = gitPath;

            DirectoryInfo dirInfo = new DirectoryInfo(projectPath);
            Scan(dirInfo);

            void Scan(DirectoryInfo dir)
            {
                while (true)
                {
                    foreach (DirectoryInfo directory in dir.GetDirectories())
                    {
                        if (directory.Name == ".git")
                        {
                            GitDir = directory.FullName;
                            WorkTree = directory.Parent.FullName;
                            return;
                        }
                    }
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        if (file.Name == ".git")
                        {
                            GitDir = file.FullName;
                            WorkTree = file.DirectoryName;
                            return;
                        }
                    }
                    if (dir.Parent == null)
                    {
                        throw new InvalidOperationException($"无法在{projectPath}目录及其父目录中找到git仓库");
                    }
                    dir = dir.Parent;
                }
            }
        }

        public string GitPath { get; }

        public string GitDir { get; private set; }

        public string WorkTree { get; private set; }

        public bool Execute(string args, out string result, out string errorMsg)
        {
            string argsPrefix = $" --git-dir=\"{GitDir}\" --work-tree=\"{WorkTree}\" ";
            args = argsPrefix + args;
            return CmdHelper.ExecuteTool(GitPath, args, out result, out errorMsg);
        }


    }
}
