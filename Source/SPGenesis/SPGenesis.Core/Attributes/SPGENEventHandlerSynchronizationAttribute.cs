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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SPGENEventHandlerSynchronizationAttribute : Attribute
    {
        public SPEventReceiverSynchronization Synchronization { get; set; }

        [Obsolete("This constructor is not longer supported.")]
        public SPGENEventHandlerSynchronizationAttribute()
        {
        }

        public SPGENEventHandlerSynchronizationAttribute(SPEventReceiverSynchronization synchronization)
        {
            this.Synchronization = synchronization;
        }
    }
}
