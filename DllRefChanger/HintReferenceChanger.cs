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
    public class HintReferenceChanger : CsprojFileChanger
    {

        public HintReferenceChanger(
            string solutionPath,
            string solutionName,
            string dllName,
            string sourceDllVersion,
            string targetDllPath
            ) : base(solutionPath,solutionName,dllName,sourceDllVersion,targetDllPath)
        {

        }

        public HintReferenceChanger(
            string solutionPath,
            string targetDllPath):base(solutionPath,targetDllPath)
        {

        }

        protected override void ChangeToTarget(string csprojFile)
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

            hintPath.Value = PathHelper.GetRelativePath(csprojFile, SolutionConfig.TargetDllAbsolutePath);

            doc.Save(csprojFile);

        }

    }
}
