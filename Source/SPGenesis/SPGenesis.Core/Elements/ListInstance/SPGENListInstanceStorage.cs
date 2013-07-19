using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENListInstanceStorage : ISPGENListInstanceStorage
    {
        public static ISPGENListInstanceStorage Instance = new SPGENListInstanceStorage();

        protected SPGENListInstanceStorage()
        {
        }

        [Obsolete("Not supported.", true)]
        public virtual SPList GetListByTitle(SPWeb web, string title)
        {
            throw new NotSupportedException();
        }

        public virtual SPList GetListByTitle(SPWeb web, string title, bool throwUnauthorizedAccessException)
        {
            if (throwUnauthorizedAccessException)
            {
                return web.Lists[title];
            }
            else
            {
                return web.Lists.TryGetList(title);
            }
        }

        public virtual SPList GetListByUrl(SPWeb web, string listUrl)
        {
            return web.GetList(listUrl);
        }

        public virtual SPGENListInstanceUrlInstance CreateUrlInstance(string url)
        {
            var instance = new SPGENListInstanceUrlInstance();

            try
            {
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        public virtual void UpdateList(SPList list)
        {
            list.Update();
        }

        public virtual void UpdateListItem(SPListItem item)
        {
            item.Update();
        }

        public virtual SPEventReceiverDefinition RegisterEventReceiver(SPEventReceiverDefinitionCollection collection, string eventReceiverName, string assembly, string className, SPEventReceiverType type, SPEventReceiverSynchronization sync, int? sequenceNumber)
        {
            var result = collection.Add();

            result.Name = eventReceiverName;
            result.Assembly = assembly;
            result.Class = className;
            result.Type = type;
            result.Synchronization = sync;

            if (sequenceNumber.HasValue)
                result.SequenceNumber = sequenceNumber.Value;

            result.Update();

            return result;
        }

        public virtual void UpdateEventReceiver(SPEventReceiverDefinition eventReceiver, string eventReceiverName, string assembly, string className, SPEventReceiverType type, SPEventReceiverSynchronization sync, int? sequenceNumber)
        {
            eventReceiver.Name = eventReceiverName;
            eventReceiver.Assembly = assembly;
            eventReceiver.Class = className;
            eventReceiver.Synchronization = sync;

            if (sequenceNumber.HasValue)
                eventReceiver.SequenceNumber = sequenceNumber.Value;

            eventReceiver.Update();
        }

        public virtual void UnRegisterEventReceiver(SPEventReceiverDefinitionCollection collection, Guid definitionId)
        {
            collection[definitionId].Delete();
        }
    }
}
