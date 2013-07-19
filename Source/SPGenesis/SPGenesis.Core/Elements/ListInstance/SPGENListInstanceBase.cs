using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public abstract class SPGENListInstanceBase
    {
        /// <summary>
        /// The definition properties for this element.
        /// </summary>
        public abstract SPGENListInstanceProperties InstanceDefinition { get; }
        internal abstract SPGENListInstanceProperties StaticDefinition { get; }
        internal abstract Action<SPGENListProvisioningArguments> OnProvisionerAction { get; set; }
        public abstract SPList GetList(SPWeb web);
        internal abstract SPList ProvisionOnWeb(SPWeb web);
        internal abstract void Unprovision(SPListCollection listCollection);
    }
}
