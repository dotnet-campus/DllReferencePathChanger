using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DllRefChanger.CsprojFileOperator;
using DllRefChanger.Utils;

namespace DllRefChanger.Changer
{
    /// <summary>
    /// 更改csproj文件中，Reference.HintPath 的值，以达到修改引用的目的
    /// 适用于用Nuget包等添加的dll引用
    /// </summary>
    public class HintReferenceChanger : CsprojFileChanger
    {

        public HintReferenceChanger(
            string solutionPath,
            string solutionName,
            string dllName,
            string sourceDllVersion,
            string newFilePath
            ) : base(solutionPath,solutionName,dllName,sourceDllVersion,newFilePath)
        {

        }

        public HintReferenceChanger(
            string solutionPath,
            string newFilePath):base(solutionPath,newFilePath)
        {

        }

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

            var selectElement = FineXElement(doc, feature1);
            if (selectElement != null)
            {
                XmlElementFactory.ChangeReferenceNode(ref selectElement, SolutionConfig.NewFileAbsolutePath);
                doc.Save(csprojFile);
                return;
            }

            selectElement = FineXElement(doc, feature2);
            if (selectElement != null)
            {
                var element = XmlElementFactory.CreateReferenceNode(SolutionConfig.DllName.Trim(), SolutionConfig.NewFileAbsolutePath, doc.Root.Name.NamespaceName);
                selectElement.Parent.Add(element);
                selectElement.Remove();
                doc.Save(csprojFile);
                return;
            }

        }

        protected override void Undo(string csprojFile)
        {
            throw new NotImplementedException();
        }
    }
}
