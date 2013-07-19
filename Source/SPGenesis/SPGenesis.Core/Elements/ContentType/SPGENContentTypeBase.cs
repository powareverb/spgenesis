using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public abstract class SPGENContentTypeBase
    {
        /// <summary>
        /// The definition properties for this element.
        /// </summary>
        public abstract SPGENContentTypeProperties InstanceDefinition { get; }
        internal abstract SPGENContentTypeProperties StaticDefinition { get; }
        internal abstract Action<SPGENContentTypeProvisioningArguments> OnProvisionerAction { get; set; }
        internal abstract SPContentType Provision(SPContentTypeCollection contentTypeCollection, SPList list, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate);
        internal abstract void Unprovision(SPContentTypeCollection contentTypeCollection, bool isCollectionFromList, bool deleteAllUsages, bool ignoreError);
    }
}
