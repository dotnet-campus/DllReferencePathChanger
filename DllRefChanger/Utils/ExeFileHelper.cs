using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger.Utils
{
    class ExeFileHelper
    {
        public static string GetGitExePath()
        {       
            var gitpath = @"Git\bin\git.exe";
            return FindFileInProgramFiles(gitpath, "git.exe");
        }

        public static string GetDotNetExePath()
        {
            var dotnetExePath = @"dotnet\dotnet.exe";
            return FindFileInProgramFiles(dotnetExePath, "dotnet.exe");
        }


        private static string FindFileInProgramFiles(string path, string defaultPath)
        {

            Func<string, string, string> exist = (files, filesx86) =>
            {
                var gitFile = Path.Combine(files, path);
                if (File.Exists(gitFile))
                {
                    return gitFile;
                }
                gitFile = Path.Combine(filesx86, path);
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

            return defaultPath;
        }

    }
}
