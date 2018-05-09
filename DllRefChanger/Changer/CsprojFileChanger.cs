using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DllRefChanger.Core;
using DllRefChanger.CsprojFileOperator;
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
                throw new FileNotFoundException("File Not Exist.", path);
            }
            SolutionConfig.NewFileAbsolutePath = path;
        }

        public string Message { get; private set; }

        public virtual bool IsUseGitWhenUndo { get; set; } = false;

        private void ScanAllCsprojFileAndDo(Action<string> changeCsprojFileAction)
        {           
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
                            changeCsprojFileAction(file.FullName);
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

        public void DoChange()
        {
            if (IsUseGitWhenUndo)
            {
                CheckCanChange();
            }

            BeforeChange();
            ScanAllCsprojFileAndDo(Change);
            AfterChange();
        }

        public void UndoChange()
        {
            BeforeUndo();
            if (IsUseGitWhenUndo)
            {
                UndoByGitCheckout();          
            }
            else
            {
                UndoByReplaceElement();
            }
            AfterUndo();
        }

        private void UndoByReplaceElement()
        {
            ScanAllCsprojFileAndDo(Undo);
        }

        private void UndoByGitCheckout()
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

        protected abstract void Change(string csprojFile);

        protected abstract void Undo(string csprojFile);

        protected virtual void BeforeChange()
        {
            
        }

        protected virtual void AfterChange()
        {
            
        }

        protected virtual void BeforeUndo()
        {
           
        }

        protected virtual void AfterUndo()
        {
            
        }

        protected XElement FineXElement(XDocument doc, XmlNodeFeature feature)
        {
            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();
            if (itemGroups == null)
            {
                return null;
            }

            List<XElement> refrenceElements = new List<XElement>();
            foreach (XElement itemGroup in itemGroups)
            {
                refrenceElements.AddRange(itemGroup.Elements());
            }

            return refrenceElements.FirstOrDefault(feature.Match);

        }

    }
}
