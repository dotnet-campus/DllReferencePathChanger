using System;
using System.Linq;
using System.Xml.Linq;
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

        protected override void ChangeToTarget(string csprojFile)
        {
            XDocument doc = XDocument.Load(csprojFile);

            var selectElement = FindReferenceItem(doc);
            if (selectElement == null)
            {
                return;
            }

            XElement specificVersion = selectElement.Elements().FirstOrDefault(e => e.Name.LocalName == "SpecificVersion");
            XElement hintPath = selectElement.Elements().FirstOrDefault(e => e.Name.LocalName == "HintPath");

            if (hintPath == null)
            {
                throw new ArgumentNullException(nameof(hintPath), "没有引用值/No HintPath value");
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

            hintPath.Value = PathHelper.GetRelativePath(csprojFile, SolutionConfig.NewFileAbsolutePath);

            doc.Save(csprojFile);

        }

    }
}
