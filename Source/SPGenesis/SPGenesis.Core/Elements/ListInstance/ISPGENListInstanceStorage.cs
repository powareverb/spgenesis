using System;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public interface ISPGENListInstanceStorage
    {
        [Obsolete("Not supported.", true)]
        SPList GetListByTitle(SPWeb web, string title);
        SPList GetListByTitle(SPWeb web, string title, bool throwUnauthorizedAccessException);
        SPList GetListByUrl(SPWeb web, string listUrl);
        SPGENListInstanceUrlInstance CreateUrlInstance(string url);
        void UpdateList(SPList list);
        void UpdateListItem(SPListItem item);
        SPEventReceiverDefinition RegisterEventReceiver(SPEventReceiverDefinitionCollection collection, string eventName, string assembly, string className, SPEventReceiverType typeName, SPEventReceiverSynchronization sync, int? sequenceNumber);
        void UpdateEventReceiver(SPEventReceiverDefinition eventReceiver, string eventReceiverName, string assembly, string className, SPEventReceiverType type, SPEventReceiverSynchronization sync, int? sequenceNumber);
        void UnRegisterEventReceiver(SPEventReceiverDefinitionCollection collection, Guid definitionId);
    }
}
