using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DllRefChanger.Model
{
    public class RefrenceItem
    {
        public string CsprojFile { get; set; }

        public XElement SourceElement { get; set; }

        public XElement NewElement { get; set; }

        public RefrenceItem(string csprojFile, XElement sourceElement, XElement newElement)
        {
            CsprojFile = csprojFile;
            SourceElement = sourceElement;
            NewElement = newElement;
        }
    }
}
