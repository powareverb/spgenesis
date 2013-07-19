using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    internal class SPGENPropertyMappingAttribute : Attribute
    {
        public string XmlAttributeName { get; set; }
        public bool NoXmlAttribute { get; set; }
        public Type ToXmlConverter { get; set; }
        public Type FromAttributeConverter { get; set; }
        public string ElementAttributeName { get; set; }
        public string OMPropertyName { get; set; }
        public bool DisableOMUpdate { get; set; }
        public string OMPropertyResourceName { get; set; }
    }
}
