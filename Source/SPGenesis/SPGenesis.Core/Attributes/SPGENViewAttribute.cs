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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SPGENViewAttribute : SPGENElementAttributeBase
    {
        public string BaseViewID { get; set; }
        public string Title { get; set; }
        public string UrlFileName { get; set; }
        public bool DefaultView { get; set; }
        public bool Paged { get; set; }
        public bool Hidden { get; set; }
        public string Query { get; set; }
        public uint RowLimit { get; set; }
    }
}
