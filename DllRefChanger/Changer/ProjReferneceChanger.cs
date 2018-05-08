using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DllRefChanger.CsprojFileOperator;
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

        protected override void Change(string csprojFile)
        {
            XDocument doc = XDocument.Load(csprojFile);

            XmlNodeFeature feature1 = new XmlNodeFeature("Reference")
            {
                SimilarAttributeFeature = new Dictionary<string, string>()
                {
                    { "Include", SolutionConfig.DllName.Trim() }
                }
            };

            XmlNodeFeature feature2 = new XmlNodeFeature("PackageReference")
            {
                SimilarAttributeFeature = new Dictionary<string, string>()
                {
                    { "Include", SolutionConfig.DllName.Trim() }
                }
            };

            //var selectElement = FindReferenceItem(doc);
            var selectElement = FineXElement(doc, feature1) ?? FineXElement(doc, feature2);

            if (selectElement == null)
            {
                return;
            }

            // 添加新节点。
            selectElement.Parent.Add(XmlElementFactory.CreateProjectReferenceNode(SourceCsprojFile, doc.Root.Name.NamespaceName));

            // 删除此节点。
            selectElement.Remove();

            doc.Save(csprojFile);
        }

        protected override void Undo(string csprojFile)
        {
            throw new NotImplementedException();
        }

        protected override void BeforeChange()
        {
            if(!File.Exists(DotNetExe))
            {
               throw new NotSupportedException("Please install dotnet core sdk first. \n url: https://www.microsoft.com/net/download/windows");
            }
        }

        protected override void AfterChange()
        {
            base.AfterChange();
            InsertProject();
        }

        /// <summary>
        /// 使用 dotnet.exe 工具添加解决方案对替换工程的引用
        /// </summary>
        private void InsertProject()
        {
            if (!CmdHelper.ExecuteTool(DotNetExe, $"sln {SolutionConfig.AbsolutePath} add {SourceCsprojFile}",
                out string result, out string errorMsg))
            {
                throw new NotSupportedException("fail when add reference to sln use dotnet.exe " + errorMsg);
            }
        }

        /// <summary>
        /// 使用 dotnet.exe 工具移除解决方案对替换工程的引用
        /// </summary>
        private void RemoveProject()
        {
            if (!CmdHelper.ExecuteTool(DotNetExe, $"sln {SolutionConfig.AbsolutePath} remove {SourceCsprojFile}",
                out string result, out string errorMsg))
            {
                throw new NotSupportedException("fail when remove reference to sln use dotnet.exe " + errorMsg);
            }
        }


        private string DotNetExe => ExeFileHelper.GetDotNetExePath();

    }
}
