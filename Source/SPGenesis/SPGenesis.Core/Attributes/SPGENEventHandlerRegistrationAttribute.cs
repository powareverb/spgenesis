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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SPGENEventHandlerRegistrationAttribute : Attribute
    {
        public Type UseType { get; set; }
        public string ExternalAssemblyName { get; set; }
        public string ExternalClass { get; set; }
        public bool KeepOnlyDeclaredMethods { get; set; }
        public SPEventReceiverType[] EventReceiverTypes { get; set; }
        public int SequenceNumber { get; set; }

        public SPGENEventHandlerRegistrationAttribute()
        {
        }

        public SPGENEventHandlerRegistrationAttribute(Type useType)
        {
            this.UseType = useType;
        }

        public SPGENEventHandlerRegistrationAttribute(string externalAssemblyName, string externalClass)
        {
            this.ExternalAssemblyName = externalAssemblyName;
            this.ExternalClass = externalClass;
        }
    }
}
