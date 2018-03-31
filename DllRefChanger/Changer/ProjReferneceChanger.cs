using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DllRefChanger.Utils;

namespace DllRefChanger.Changer
{
    public class ProjReferneceChanger : CsprojFileChanger
    {
        public ProjReferneceChanger(string solutionPath, string solutionName, string dllName, string sourceDllVersion, string newFilePath) 
            : base(solutionPath, solutionName, dllName, sourceDllVersion, newFilePath)
        {
        }

        public ProjReferneceChanger(string solutionPath, string newFilePath) 
            : base(solutionPath, newFilePath)
        {

        }

        /// <summary>
        /// 替换源的 csproj 文件路径
        /// </summary>
        public string SourceCsprojFile => SolutionConfig.NewFileAbsolutePath;

        protected override void ChangeToTarget(string csprojFile)
        {
            XDocument doc = XDocument.Load(csprojFile);
            var selectElement = FindReferenceItem(doc);
            if (selectElement == null)
            {
                return;
            }

            // 这里是新建的实例，对其增删改无效哦。
            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();

            if (itemGroups == null)
            {
                return;
            }

            // 删除此节点
            selectElement.Remove();

            doc.Save(csprojFile);

            InsertProject(csprojFile);
        }


        /// <summary>
        /// 使用 dotnet.exe 工具添加工程和解决方案的引用
        /// </summary>
        /// <param name="csprojFile"></param>
        private void InsertProject(string csprojFile)
        {
            if (!CmdHelper.ExecuteTool(DotNetExe, $" add {csprojFile} reference {SourceCsprojFile}", 
                out string result, out string errorMsg))
            {
                throw new NotSupportedException("fail when add reference to csproj use dotnet.exe " + errorMsg);
            }

            if (!CmdHelper.ExecuteTool(DotNetExe, $"sln {SolutionConfig.AbsolutePath} add {SourceCsprojFile}",
                out string result2, out string errorMsg2))
            {
                throw new NotSupportedException("fail when add reference to sln use dotnet.exe " + errorMsg);
            }
        }

        private string DotNetExe => GetDotNetExeFileDefault();

        private string GetDotNetExeFileDefault()
        {
            var dotnetExePath = @"dotnet\dotnet.exe";

            Func<string, string, string> exist = (files, filesx86) =>
            {
                var gitFile = Path.Combine(files, dotnetExePath);
                if (File.Exists(gitFile))
                {
                    return gitFile;
                }
                gitFile = Path.Combine(filesx86, dotnetExePath);
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

            return "dotnet.exe";
        }
    }
}
