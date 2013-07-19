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
    public class SPGENListInstanceAttribute : SPGENElementAttributeBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int TemplateType { get; set; }
        public string WebRelURL { get; set; }
        public string TemplateFeatureId { get; set; }
        public bool OnQuickLaunch { get; set; }
        public bool ContentTypesEnabled { get; set; }
        public SPGENListInstanceGetMethod GetListDefaultMethod { get; set; }
    }
}
