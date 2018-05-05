using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DllRefChanger.CsprojFileOperator
{
    public class XmlNodeFeature
    {
        public string Name { get; set; }

        public Dictionary<string,string> SimilarAttributeFeature { get; set; }
        
        public Dictionary<string, string> EqualAttributeFeature { get; set; }

        public Dictionary<string,string> SimilarChildElementFeature { get; set; }

        public Dictionary<string,string> EqualChildElementFeature { get; set; }

        public XmlNodeFeature(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            SimilarAttributeFeature = new Dictionary<string, string>();
            EqualAttributeFeature = new Dictionary<string, string>();
            SimilarChildElementFeature = new Dictionary<string, string>();
            EqualChildElementFeature = new Dictionary<string, string>();
        }

        public bool Match(XElement xElement)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException(nameof(xElement));
            }

            if (xElement.Name.LocalName != Name)
            {
                return false;
            }

            if (SimilarAttributeFeature.Count != 0 || EqualAttributeFeature.Count != 0)
            {
                if (!xElement.HasAttributes)
                {
                    return false;
                }
            }

            if (SimilarChildElementFeature.Count != 0 || EqualChildElementFeature.Count != 0)
            {
                if (!xElement.HasElements)
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> feature in SimilarAttributeFeature)
            {
                XAttribute attribute = xElement.Attributes().FirstOrDefault(att=>att.Name==feature.Key);
                if (attribute == null)
                {
                    return false;
                }
                if (!attribute.Value.Contains(feature.Value))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> feature in EqualAttributeFeature)
            {
                XAttribute attribute = xElement.Attributes().FirstOrDefault(att => att.Name == feature.Key);
                if (attribute == null)
                {
                    return false;
                }
                if (attribute.Value != feature.Value)
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> feature in SimilarChildElementFeature)
            {
                XElement element = xElement.Elements().FirstOrDefault(ele=>ele.Name==feature.Key);
                if (element == null)
                {
                    return false;
                }
                if (!element.Value.Contains(feature.Value))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> feature in EqualChildElementFeature)
            {
                XElement element = xElement.Elements().FirstOrDefault(ele => ele.Name == feature.Key);
                if (element == null)
                {
                    return false;
                }
                if (element.Value != feature.Value)
                {
                    return false;
                }
            }

            return true;

        }

    }
}
