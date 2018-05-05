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

        protected override void AfterChange()
        {
            base.AfterChange();
            InsertProject();
        }


        private void InsertProject(XDocument doc)
        {
            // 这里是新建的实例，对其增删改无效，只能用于查询。
            // ReSharper disable once PossibleNullReferenceException
            List<XElement> itemGroups = doc.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();

            // 找到或创建包含 ProjectReference 节点的 ItemGroup。
            var projectReferenceItemGroup = itemGroups.FirstOrDefault(e => e.Elements().Any(ele => ele.Name.LocalName == "ProjectReference"));
            if (projectReferenceItemGroup == null)
            {
                projectReferenceItemGroup = new XElement(XName.Get("ItemGroup", doc.Root.Name.NamespaceName));
                doc.Root.Add(projectReferenceItemGroup);
            }

            /*	
             <ProjectReference Include="..\..\..\Dependencies\Cvte.Paint\Cvte.Paint.Chart\Cvte.Paint.Chart.csproj">	
                 <Project>{3e1f8f4b-5f57-4f8f-9224-9ea9b987b880}</Project>	
                 <Name>Cvte.Paint.Chart</Name>	
             </ProjectReference>	
             */

            string ns = projectReferenceItemGroup.Name.NamespaceName;
            // 添加新的 ProjectReference 节点 	
            XElement projectReferenceItem = new XElement(XName.Get("ProjectReference", ns));
            var includeValue = SourceCsprojFile; // 使用绝对路径（相对路径也可以）
            projectReferenceItem.SetAttributeValue(XName.Get("Include"), includeValue);

            // 这两个内容的添加没有必要
            ////projectReferenceItem.AddFirst(new XElement(XName.Get("Project")) { Value = SourceCsprojGuid });	
            ////projectReferenceItem.AddFirst(new XElement(XName.Get("Name")) { Value = SourceCsprojName });	

            //projectReferenceItem



            projectReferenceItemGroup.Add(projectReferenceItem);
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
