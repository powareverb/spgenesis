using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public abstract class SPGENFieldBase
    {
        public abstract SPGENFieldProperties InstanceDefinition { get; }
        internal abstract SPGENFieldProperties StaticDefinition { get; }
        internal abstract Action<SPField> OnProvisionerAction { get; set; }
        internal abstract SPField Provision(SPFieldCollection fieldCollection, bool updateIfExists, bool pushChangesToList);
        internal abstract void Unprovision(SPFieldCollection fieldCollection);
        internal abstract bool FireOnProvisionStarted(SPGENFieldProperties fieldProperties, SPFieldCollection fieldCollection, bool isParentList);
        internal abstract void FireOnProvisionFinalized(SPField field, SPFieldCollection fieldCollection, bool isParentList, bool updatedOnly);
    }
}
