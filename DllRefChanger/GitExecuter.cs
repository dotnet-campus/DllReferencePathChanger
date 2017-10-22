using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger
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

        public static string GetGitExePath()
        {
            var gitpath = @"Git\bin\git.exe";

            Func<string, string, string> exist = (files, filesx86) =>
              {
                  var gitFile = Path.Combine(files, gitpath);
                  if (File.Exists(gitFile))
                  {
                      return gitFile;
                  }
                  gitFile = Path.Combine(filesx86, gitpath);
                  if (File.Exists(gitFile))
                  {
                      return gitFile;
                  }
                  return string.Empty;
              };

            var programFiles = @"C:\Program Files";
            var programFilesx86 = @"C:\Program Files (x86)";
            var file = exist(programFiles, programFilesx86);
            if (!string.IsNullOrEmpty(file))
            {
                return file;
            }

            programFiles = programFiles.Remove(0, 1).Insert(0, "D");
            programFilesx86 = programFilesx86.Remove(0, 1).Insert(0, "D");
            file = exist(programFiles, programFilesx86);
            if (!string.IsNullOrEmpty(file))
            {
                return file;
            }

            programFiles = programFiles.Remove(0, 1).Insert(0, "E");
            programFilesx86 = programFilesx86.Remove(0, 1).Insert(0, "E");
            file = exist(programFiles, programFilesx86);
            if (!string.IsNullOrEmpty(file))
            {
                return file;
            }

            //return @"D:\Program Files\Git\cmd\git.exe";
            // 这里不用写完整的安装路径，前提是系统的环境变量中可以找到
            return "git.exe";
        }

    }
}
