using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public abstract class SPGENElementAttributeBase : Attribute
    {
        public bool ExcludeProvisioning { get; set; }
        public int ProvisionSequence { get; set; }
        public int UnprovisionSequence { get; set; }
    }
}
