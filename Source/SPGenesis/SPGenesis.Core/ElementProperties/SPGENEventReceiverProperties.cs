using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Linq.Expressions;

namespace SPGenesis.Core
{
    public sealed class SPGENEventReceiverProperties : SPGENElementProperties
    {
        [SPGENPropertyMappingAttribute]
        public Type UseType { get { return base.GetPropertyValue<Type>("UseType"); } internal set { base.SetPropertyValue("UseType", value); this.Assembly = value.Assembly.FullName; this.Class = value.FullName; } }
        [SPGENPropertyMappingAttribute]
        public string Name { get { return base.GetPropertyValue<string>("Name"); } internal set { base.SetPropertyValue("Name", value); } }
        [SPGENPropertyMappingAttribute]
        public string Assembly { get { return base.GetPropertyValue<string>("Assembly"); } internal set { base.SetPropertyValue("Assembly", value); } }
        [SPGENPropertyMappingAttribute]
        public string Class { get { return base.GetPropertyValue<string>("Class"); } internal set { base.SetPropertyValue("Class", value); } }
        [SPGENPropertyMappingAttribute]
        public int? SequenceNumber { get { return base.GetPropertyValue<int?>("SequenceNumber"); } internal set { base.SetPropertyValue("SequenceNumber", value); } }
        [SPGENPropertyMappingAttribute]
        public SPEventReceiverType Type { get { return base.GetPropertyValue<SPEventReceiverType>("Type"); } internal set { base.SetPropertyValue("Type", value); } }
        [SPGENPropertyMappingAttribute]
        public SPEventReceiverSynchronization Synchronization { get { return base.GetPropertyValue<SPEventReceiverSynchronization>("Synchronization"); } internal set { base.SetPropertyValue("Synchronization", value); } }

        public bool IsSameAs(SPGENEventReceiverProperties compareWith)
        {
            SPEventReceiverSynchronization thisSyncType = this.Synchronization;
            SPEventReceiverSynchronization compareWithSyncType = compareWith.Synchronization;

            if (thisSyncType == SPEventReceiverSynchronization.Default)
            {
                string evtType = this.Type.ToString();
                if (evtType.EndsWith("ing"))
                {
                    thisSyncType = SPEventReceiverSynchronization.Synchronous;
                }
                else
                {
                    thisSyncType = SPEventReceiverSynchronization.Asynchronous;
                }
            }

            if (compareWithSyncType == SPEventReceiverSynchronization.Default)
            {
                string evtType = this.Type.ToString();
                if (evtType.EndsWith("ing"))
                {
                    compareWithSyncType = SPEventReceiverSynchronization.Synchronous;
                }
                else
                {
                    compareWithSyncType = SPEventReceiverSynchronization.Asynchronous;
                }
            }

            if (SPGENCommon.CompareAssemblyNames(this.Assembly, compareWith.Assembly) &&
                this.Class == compareWith.Class &&
                this.Type == compareWith.Type &&
                thisSyncType == compareWithSyncType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsSameAs(SPEventReceiverDefinition definition)
        {
            return this.IsSameAs(new SPGENEventReceiverProperties() { Assembly = definition.Assembly, Class = definition.Class, Type = definition.Type, Synchronization = definition.Synchronization });
        }

        public object GetDynamicProperty(Expression<Func<SPEventReceiverDefinition, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPEventReceiverDefinition, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPEventReceiverDefinition>(property, value);
        }

        protected override string ElementIdAttribute
        {
            get { throw new NotImplementedException(); }
        }
        internal override object ElementIdValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        protected override string ElementName
        {
            get { throw new NotImplementedException(); }
        }
    }
}
