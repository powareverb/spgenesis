using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public class SPGENEventReceiverCollection : SPGENElementCollectionBase<SPGENEventReceiverProperties, string>
    {
        private List<string> _keepOnlyDeclaredMethodTypes = new List<string>();
        internal List<string> KeepOnlyDeclaredMethodTypes
        {
            get { return _keepOnlyDeclaredMethodTypes; }
            set { _keepOnlyDeclaredMethodTypes = value; }
        }

        protected override bool IsItemEqual(SPGENEventReceiverProperties item1, SPGENEventReceiverProperties item2)
        {
            return item1.IsSameAs(item2);
        }

        protected override string GetIdentifier(SPGENEventReceiverProperties item)
        {
            throw new NotImplementedException();
        }

        public void AddType(string assembly, string className, bool exclusiveAdd)
        {
            AddType(assembly, className, null, exclusiveAdd);
        }

        public void AddType(string assembly, string className, SPEventReceiverType[] includeTypeFilter, bool exclusiveAdd)
        {
            var list = SPGENCommon.GetEventReceiversFromType(assembly, className, null, includeTypeFilter);

            AddFromListOfEventReceivers(list, exclusiveAdd);
        }

        public void AddType(string assembly, string className, int sequenceNumber, bool exclusiveAdd)
        {
            AddType(assembly, className, sequenceNumber, null, exclusiveAdd);
        }

        public void AddType(string assembly, string className, int sequenceNumber, SPEventReceiverType[] includeTypeFilter, bool exclusiveAdd)
        {
            var list = SPGENCommon.GetEventReceiversFromType(assembly, className, sequenceNumber, includeTypeFilter);

            AddFromListOfEventReceivers(list, exclusiveAdd);
        }

        public void AddType(Type eventReceiver, bool exclusiveAdd)
        {
            AddType(eventReceiver, null, exclusiveAdd);
        }

        public void AddType(Type eventReceiver, SPEventReceiverType[] includeTypeFilter, bool exclusiveAdd)
        {
            var list = SPGENCommon.GetEventReceiversFromType(eventReceiver, null, includeTypeFilter);

            AddFromListOfEventReceivers(list, exclusiveAdd);
        }

        public void AddType(Type eventReceiver, int sequenceNumber, SPEventReceiverType[] includeTypeFilter, bool exclusiveAdd)
        {
            var list = SPGENCommon.GetEventReceiversFromType(eventReceiver, sequenceNumber, includeTypeFilter);

            AddFromListOfEventReceivers(list, exclusiveAdd);
        }

        public void AddType<TEventReceiver>(SPEventReceiverType[] includeTypeFilter, bool exclusiveAdd) where TEventReceiver : SPEventReceiverBase
        {
            this.AddType(typeof(TEventReceiver), includeTypeFilter, exclusiveAdd);
        }

        public void AddType<TEventReceiver>(SPEventReceiverType[] includeTypeFilter, int sequenceNumber, bool exclusiveAdd) where TEventReceiver : SPEventReceiverBase
        {
            this.AddType(typeof(TEventReceiver), sequenceNumber, includeTypeFilter, exclusiveAdd);
        }

        private void AddFromListOfEventReceivers(IList<SPGENEventReceiverProperties> eventReceivers, bool exclusiveAdd)
        {
            foreach (var rec in eventReceivers)
            {
                int i = this.IndexOf(rec);
                if (i == -1)
                {
                    this.Add(rec);

                    if (exclusiveAdd)
                    {
                        AddEventReceiverToDeclaredOnlyCollection(rec.Assembly, rec.Class);
                    }
                }
            }
        }

        private void AddEventReceiverToDeclaredOnlyCollection(string assembly, string className)
        {
            string s = assembly + "|" + className;

            if (!_keepOnlyDeclaredMethodTypes.Exists(d => d == s))
            {
                _keepOnlyDeclaredMethodTypes.Add(s);
            }
        }

        internal void AddReceiversFromElementAttributes(Type elementType, SPEventReceiverType[] includeTypeFilter)
        {
            object[] attributes = elementType.GetCustomAttributes(typeof(SPGENEventHandlerRegistrationAttribute), true);
            foreach (SPGENEventHandlerRegistrationAttribute rec in attributes)
            {
                if (rec.UseType != null)
                {
                    if (SPGENCommon.AttributeParameterExists(elementType, typeof(SPGENEventHandlerRegistrationAttribute), "SequenceNumber"))
                    {
                        this.AddType(rec.UseType, rec.SequenceNumber, includeTypeFilter, rec.KeepOnlyDeclaredMethods);
                    }
                    else
                    {
                        this.AddType(rec.UseType, includeTypeFilter, rec.KeepOnlyDeclaredMethods);
                    }

                    if (rec.KeepOnlyDeclaredMethods)
                    {
                        AddEventReceiverToDeclaredOnlyCollection(rec.UseType.Assembly.FullName,  rec.UseType.FullName);
                    }
                }
                else
                {
                    if (SPGENCommon.AttributeParameterExists(elementType, typeof(SPGENEventHandlerRegistrationAttribute), "SequenceNumber"))
                    {
                        this.AddType(rec.ExternalAssemblyName, rec.ExternalClass, rec.SequenceNumber, includeTypeFilter, rec.KeepOnlyDeclaredMethods);
                    }
                    else
                    {
                        this.AddType(rec.ExternalAssemblyName, rec.ExternalClass, includeTypeFilter, rec.KeepOnlyDeclaredMethods);
                    }

                    if (rec.KeepOnlyDeclaredMethods)
                    {
                        AddEventReceiverToDeclaredOnlyCollection(rec.ExternalAssemblyName, rec.ExternalClass);
                    }
                }

            }
        }

        public void Provision(SPEventReceiverDefinitionCollection collection)
        {
            var typedCollection = collection.OfType<SPEventReceiverDefinition>();
            var updatedItems = this.GetAllAddedAndUpdatedItems();

            foreach (var evtRec in updatedItems)
            {
                string uniqueName = evtRec.Class + "_" + evtRec.Type.ToString() + "_" + evtRec.Synchronization.ToString();

                var evr = typedCollection.FirstOrDefault<SPEventReceiverDefinition>(d => evtRec.IsSameAs(d));
                if (evr != null && this.CanUpdate)
                {
                    SPGENListInstanceStorage.Instance.UpdateEventReceiver(evr, uniqueName, evtRec.Assembly, evtRec.Class, evtRec.Type, evtRec.Synchronization, evtRec.SequenceNumber);
                }
                else
                {
                    SPGENListInstanceStorage.Instance.RegisterEventReceiver(collection, uniqueName, evtRec.Assembly, evtRec.Class, evtRec.Type, evtRec.Synchronization, evtRec.SequenceNumber);
                }
            }

            if (!this.CanUpdate)
                return;

            if (this.IsExclusiveAdd)
            {
                var eventReceiversToRemove = new List<Guid>();

                foreach (var def in typedCollection)
                {
                    bool keep = updatedItems.Exists(r => r.IsSameAs(def));

                    if (keep)
                        continue;

                    eventReceiversToRemove.Add(def.Id);
                }

                foreach (Guid id in eventReceiversToRemove)
                {
                    SPGENListInstanceStorage.Instance.UnRegisterEventReceiver(collection, id);
                }
            }
            else
            {
                var removedItems = this.GetAllRemovedItems();

                foreach (var def in removedItems)
                {
                    var r = typedCollection.FirstOrDefault<SPEventReceiverDefinition>(d => def.IsSameAs(d));
                    if (r != null)
                    {
                        SPGENListInstanceStorage.Instance.UnRegisterEventReceiver(collection, r.Id);
                    }
                }
            }

            RemoveUndeclaredMethods(collection);
        }

        private void RemoveUndeclaredMethods(SPEventReceiverDefinitionCollection collection)
        {
            var list = new List<SPEventReceiverDefinition>();
            var updatedItems = this.GetAllAddedAndUpdatedItems();

            foreach (SPEventReceiverDefinition def in collection)
            {
                if (_keepOnlyDeclaredMethodTypes.Exists(a =>
                    {
                        string[] arr = a.Split('|');

                        if (SPGENCommon.CompareAssemblyNames(def.Assembly, arr[0]) && arr[1] == def.Class)
                            return true;
                        else
                            return false;
                    }))
                {
                    if (updatedItems.FindIndex(p => p.IsSameAs(def)) == -1)
                    {
                        list.Add(def);
                    }
                }
            }

            foreach(var def in list)
            {
                def.Delete();
            }
        }

    }
}
