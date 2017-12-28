using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DllRefChanger
{
    /// <summary>
    /// 更改csproj文件中，Reference.HintPath 的值，以达到修改引用的目的
    /// 适用于用Nuget包等添加的dll引用
    /// </summary>
    public class HintReferenceChanger : IReferenceChanger
    {

        public HintReferenceChanger(
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

        public HintReferenceChanger(
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
                        ChangeToTarget(file.FullName);
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

        private void ChangeToTarget(string csprojFile)
        {

            XDocument doc = XDocument.Load(csprojFile);
            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();
            XElement itemGroup = itemGroups?.FirstOrDefault(e => e.Elements().Any(ele => ele.Name.LocalName == "Reference"));
            XElement selectElement = itemGroup?.Elements().FirstOrDefault(e => e.Attribute("Include")?.Value.Contains(SolutionConfig.DllName) ?? false);

            if (selectElement == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SolutionConfig.SourceDllVersion))
            {
                // 指定了版本号，如果版本不匹配，返回
                if (!selectElement.Attribute("Include")?.Value.Contains(SolutionConfig.SourceDllVersion) ?? true)
                {
                    return;
                }
            }

            XElement specificVersion = selectElement.Elements().FirstOrDefault(e => e.Name.LocalName == "SpecificVersion");
            XElement hintPath = selectElement.Elements().FirstOrDefault(e => e.Name.LocalName == "HintPath");

            if (hintPath == null)
            {
                throw new ArgumentNullException(nameof(hintPath));
            }

            if (specificVersion == null)
            {
                XName xn = XName.Get("SpecificVersion", hintPath.Name.NamespaceName);
                XElement sc = new XElement(xn)
                {
                    Value = "False",
                };
                selectElement.AddFirst(sc);
            }
            else
            {
                specificVersion.Value = "False";
            }

            hintPath.Value = PathHelper.GetRelativePath(csprojFile, SolutionConfig.TargetDllAbsolutePath);

            doc.Save(csprojFile);

        }

    }
}
