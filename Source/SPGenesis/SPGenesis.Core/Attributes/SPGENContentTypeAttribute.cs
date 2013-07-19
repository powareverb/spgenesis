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
    public class SPGENContentTypeAttribute : SPGENElementAttributeBase
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public bool Hidden { get; set; }
        public bool Sealed { get; set; }
        public bool ReadOnly { get; set; }
        public Type InheritsType { get; set; }
        public SPGENProvisionEventCallBehavior ProvisionEventCallBehavior { get; set; }

        [Obsolete("This attribute is not longer available since it is only used in features and XML-definitions.", false)]
        public bool Inherits { get; set; }
    }
}
