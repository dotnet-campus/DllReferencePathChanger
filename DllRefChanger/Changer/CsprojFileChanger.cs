using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DllRefChanger.Utils;

namespace DllRefChanger.Changer
{
    public abstract class CsprojFileChanger : IReferenceChanger
    {
        protected CsprojFileChanger(
            string solutionPath,
            string solutionName,
            string dllName,
            string sourceDllVersion,
            string newFilePath
            )
        {
            SolutionConfig = new SolutionConfig()
            {
                AbsolutePath = solutionPath,
                Name = solutionName,
                DllName = dllName,
                SourceDllVersion = sourceDllVersion,
                NewFileAbsolutePath = newFilePath
            };
            CheckFile();
        }

        protected CsprojFileChanger(
            string solutionPath,
            string newFilePath)
        {
            SolutionConfig = new SolutionConfig()
            {
                AbsolutePath = solutionPath,
                Name = Path.GetFileNameWithoutExtension(solutionPath),
                DllName = Path.GetFileNameWithoutExtension(newFilePath),
                SourceDllVersion = string.Empty,
                NewFileAbsolutePath = newFilePath
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
            SolutionConfig.NewFileAbsolutePath = path;
        }

        public string Message { get; private set; }
        public virtual bool UseDefaultCheckCanChange { get; set; } = true;

        public void Change()
        {
            if (UseDefaultCheckCanChange)
            {
                CheckCanChange();
            }

            BeforeChange();

            GitExecuter gitExecuter = new GitExecuter(ExeFileHelper.GetGitExePath(), SolutionConfig.AbsolutePath);

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
            GitExecuter gitExecuter = new GitExecuter(ExeFileHelper.GetGitExePath(), SolutionConfig.AbsolutePath);

            bool success = gitExecuter.Execute("checkout *.csproj", out string result, out string err);

            if (!success)
            {
                throw new InvalidOperationException($"哎呀，恢复 csproj 文件时出错了：{err}");
            }

            success = gitExecuter.Execute("checkout *.sln", out string result2, out string err2);
            if (!success)
            {
                throw new InvalidOperationException($"哎呀，恢复 sln 文件时出错了：{err2}");
            }

        }

        private void CheckFile()
        {
            if (!File.Exists(SolutionConfig.AbsolutePath))
            {
                throw new FileNotFoundException("文件不存在", SolutionConfig.AbsolutePath);
            }
            if (!File.Exists(SolutionConfig.NewFileAbsolutePath))
            {
                throw new FileNotFoundException("文件不存在", SolutionConfig.NewFileAbsolutePath);
            }
        }

        private void CheckCanChange()
        {
            GitExecuter gitExecuter = new GitExecuter(ExeFileHelper.GetGitExePath(), SolutionConfig.AbsolutePath);

            bool success = gitExecuter.Execute("status", out string result, out string err);

            if (!success)
            {
                throw new InvalidOperationException(err);
            }

            if (result.Contains(".csproj"))
            {
                throw new InvalidOperationException("有尚未提交的对 csproj 文件的修改，请先撤销或者提交这些修改");
            }

            if (result.Contains(".sln"))
            {
                throw new InvalidOperationException("有尚未提交的对 sln 文件的修改，请先撤销或者提交这些修改");
            }
        }

        protected abstract void ChangeToTarget(string csprojFile);

        protected virtual void BeforeChange()
        {
            // 如果在替换之前需要进行某些操作，在子类中重新该方法以实现该操作
        }

        /// <summary>
        /// 找到 csprojFile 文件中包含待替换dll信息的 Reference 节点；没有找到则返回 null .
        /// </summary>
        /// <returns></returns>
        protected XElement FindReferenceItem(XDocument doc)
        {
            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();

            // 找到包含 Reference 节点的 ItemGroup 
            itemGroups = itemGroups?.Where(e => e.Elements().Any(ele => ele.Name.LocalName == "Reference")).ToList();

            if (itemGroups == null)
            {
                return null;
            }

            XElement selectElement = null;
            foreach (XElement itemGroup in itemGroups)
            {
                selectElement = itemGroup?.Elements().FirstOrDefault(e =>
                {

                    /*value : 
                     "System.Composition.AttributedModel, Version=1.0.31.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL"
                     */

                    string value = e.Attribute("Include")?.Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        return false;
                    }

                    string dllName = value.Split(',').FirstOrDefault();
                    return dllName?.Trim() == SolutionConfig.DllName.Trim();

                });
                if (selectElement != null)
                {
                    break;
                }
            }

            if (selectElement == null)
            {
                return null;
            }


            if (!string.IsNullOrEmpty(SolutionConfig.SourceDllVersion))
            {
                // 指定了版本号，如果版本不匹配，返回
                if (!selectElement.Attribute("Include")?.Value.Contains(SolutionConfig.SourceDllVersion) ?? true)
                {
                    return null;
                }
            }

            return selectElement;
        }


    }
}
