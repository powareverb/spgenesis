using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public abstract class SPGENViewBase
    {
        public abstract SPGENViewProperties InstanceDefinition { get; }
        internal abstract SPGENViewProperties StaticDefinition { get; }
        internal abstract SPView Provision(SPViewCollection viewCollection, bool preserveViewFieldsCollection);
    }
}
