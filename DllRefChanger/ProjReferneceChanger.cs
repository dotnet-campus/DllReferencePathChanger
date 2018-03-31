using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DllRefChanger
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

        /// <summary>
        /// 替换源工程的 GUID
        /// </summary>
        public string SourceCsprojGuid { get; set; }

        /// <summary>
        /// 替换源工程的名称
        /// </summary>
        public string SourceCsprojName { get; set; }

        protected override void ChangeToTarget(string csprojFile)
        {
            XDocument doc = XDocument.Load(csprojFile);
            var selectElement = FindReferenceItem(doc);
            if (selectElement == null)
            {
                return;
            }

            // 删除此节点
            selectElement.Remove();

            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "ItemGroup").ToList();

            if (itemGroups == null)
            {
                return;
            }

            // 找到包含 ProjectReference 节点的 ItemGroup 
            var projectReferenceItemGroup = itemGroups.FirstOrDefault(e => e.Elements().Any(ele => ele.Name.LocalName == "ProjectReference"));
            if (projectReferenceItemGroup == null)
            {
                projectReferenceItemGroup = new XElement("ItemGroup");
                itemGroups.Add(projectReferenceItemGroup);
            }

            /*
            <ProjectReference Include="..\..\..\Dependencies\Cvte.Paint\Cvte.Paint.Chart\Cvte.Paint.Chart.csproj">
                <Project>{3e1f8f4b-5f57-4f8f-9224-9ea9b987b880}</Project>
                <Name>Cvte.Paint.Chart</Name>
            </ProjectReference>
             */

            // 添加新的 ProjectReference 节点 
            XElement projectReferenceItem = new XElement("ProjectReference");
            var includeValue = PathHelper.GetRelativePath(csprojFile, SourceCsprojFile);

            projectReferenceItem.SetAttributeValue(XName.Get("Include"), includeValue);
            projectReferenceItem.AddFirst(new XElement("Project") { Value = SourceCsprojGuid });
            projectReferenceItem.AddFirst(new XElement("Name") { Value = SourceCsprojName });

            doc.Save(csprojFile);
        }

        protected override void BeforeChange()
        {
            if (!File.Exists(SourceCsprojFile))
            {
                throw new FileNotFoundException(SourceCsprojFile);
            }

            AnalyzeSourceCsprojFile(SourceCsprojFile);
            base.BeforeChange();
        }

        private void AnalyzeSourceCsprojFile(string csprojFile)
        {
            XDocument doc = XDocument.Load(csprojFile);
            List<XElement> itemGroups = doc.Root?.Elements().Where(e => e.Name.LocalName == "PropertyGroup").ToList();

            if (itemGroups == null)
            {
                throw new ArgumentException("Broken csproj file, can not fine ‘PropertyGroup’ item");
            }

            foreach (XElement propertyGroup in itemGroups)
            {
                var guid = propertyGroup.Element("ProjectGuid")?.Value;
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }
                var assemblyName = propertyGroup.Element("AssemblyName")?.Value;

                SourceCsprojGuid = guid;
                SourceCsprojName = assemblyName;

                break;
            }

            if (string.IsNullOrWhiteSpace(SourceCsprojGuid) || string.IsNullOrWhiteSpace(SourceCsprojName))
            {
                throw new InvalidOperationException($"Can not get ProjectGuid or AssemblyName of {csprojFile}");
            }

        }

    }
}
