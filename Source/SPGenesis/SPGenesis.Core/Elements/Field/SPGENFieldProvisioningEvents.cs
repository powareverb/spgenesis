using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENFieldProvisioningEvents
    {
        public delegate bool OnProvisionStartedDelegate(SPGENFieldProperties fieldProperties, SPFieldCollection fieldCollection, bool isParentList);
        public delegate bool OnProvisionFieldSchemaXmlDelegate(XmlElement schemaXml, SPGENFieldProperties fieldProperties, SPFieldCollection fieldCollection, bool isParentList);
        public delegate bool OnProvisionBeforeUpdateDelegate(SPField field, SPFieldCollection fieldCollection, bool isParentList);
        public delegate void OnProvisionFinalizedDelegate(SPField field, SPFieldCollection fieldCollection, bool isParentList, bool updatedOnly);
        public delegate bool OnUnprovisionStartedDelegate(SPField field, SPFieldCollection fieldCollection, bool isParentList);
        public delegate void OnUnprovisionFinalizedDelegate(SPFieldCollection fieldCollection, bool isParentList);

        public OnProvisionStartedDelegate OnProvisionStarted;
        public OnProvisionFieldSchemaXmlDelegate OnProvisionFieldSchemaXml;
        public OnProvisionBeforeUpdateDelegate OnProvisionBeforeUpdate;
        public OnProvisionFinalizedDelegate OnProvisionFinalized;
        public OnUnprovisionStartedDelegate OnUnprovisionStarted;
        public OnUnprovisionFinalizedDelegate OnUnprovisionFinalized;
    }
}
