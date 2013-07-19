using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENListProvisioningArguments
    {
        public SPList List { get; set; }
        public SPGENListFieldCollection Fields { get; set; }
        public SPGENListContentTypeCollection ContentTypes { get; set; }
        public SPGENListViewCollection Views { get; set; }
        public SPGENEventReceiverCollection EventReceivers { get; set; }
    }
}
