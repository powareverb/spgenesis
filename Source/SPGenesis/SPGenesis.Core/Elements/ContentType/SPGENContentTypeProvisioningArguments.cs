using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENContentTypeProvisioningArguments
    {
        /// <summary>
        /// The content type isntance that is provisioned or unprovisioned.
        /// </summary>
        public SPContentType ContentType { get; set; }

        /// <summary>
        /// The field link collection.
        /// </summary>
        public SPGENFieldLinkCollection FieldLinks { get; set; }

        /// <summary>
        /// Field links to remove from the content type instance.
        /// </summary>
        public IList<Guid> FieldLinksToRemove { get; set; }

        /// <summary>
        /// Event receiver collection.
        /// </summary>
        public SPGENEventReceiverCollection EventReceivers { get; set; }
    }
}
