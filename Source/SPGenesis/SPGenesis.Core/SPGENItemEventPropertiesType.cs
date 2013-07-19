using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Linq.Expressions;

namespace SPGenesis.Core
{
    public enum SPGENItemEventPropertiesType
    {
        /// <summary>
        /// Use the before properties from the event properties.
        /// </summary>
        BeforeProperties,
        /// <summary>
        /// Use the after properties from the event properties.
        /// </summary>
        AfterProperties
    }
}
