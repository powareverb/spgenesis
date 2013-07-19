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
using System.Linq.Expressions;

namespace SPGenesis.Core
{
    public class SPGENContentTypeProperties : SPGENElementProperties
    {
        public SPGENContentTypeProperties() { }
        internal SPGENContentTypeProperties(XmlNode elementDefinitionXml) : base(elementDefinitionXml) { }

        [SPGENPropertyMappingAttribute(ToXmlConverter=typeof(ConvertID), FromAttributeConverter=typeof(ConvertID))]
        public SPContentTypeId ID { get { return base.GetPropertyValue<SPContentTypeId>("ID"); } set { base.SetPropertyValue("ID", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyResourceName = "NameResource")]
        public string Name { get { return base.GetPropertyValue<string>("Name"); } set { base.SetPropertyValue("Name", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyResourceName = "DescriptionResource")]
        public string Description { get { return base.GetPropertyValue<string>("Description"); } set { base.SetPropertyValue("Description", value); } }
        [SPGENPropertyMappingAttribute]
        public string Group { get { return base.GetPropertyValue<string>("Group"); } set { base.SetPropertyValue("Group", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Hidden { get { return base.GetPropertyValue<bool>("Hidden"); } set { base.SetPropertyValue("Hidden", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Sealed { get { return base.GetPropertyValue<bool>("Sealed"); } set { base.SetPropertyValue("Sealed", value); } }
        [SPGENPropertyMappingAttribute]
        public bool ReadOnly { get { return base.GetPropertyValue<bool>("ReadOnly"); } set { base.SetPropertyValue("ReadOnly", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Inherits { get { return base.GetPropertyValue<bool>("Inherits"); } internal set { base.SetPropertyValue("Inherits", value); } }

        private SPGENEventReceiverCollection _eventReceivers;
        public SPGENEventReceiverCollection EventReceivers
        {
            get
            {
                if (_eventReceivers == null)
                {
                    _eventReceivers = new SPGENEventReceiverCollection();
                }

                return _eventReceivers;
            }
            private set
            {
                _eventReceivers = value;
            }
        }

        private SPGENFieldLinkCollection _fieldLinks;
        public SPGENFieldLinkCollection FieldLinks
        {
            get
            {
                if (_fieldLinks == null)
                    LoadFieldLinkCollection();

                return _fieldLinks;
            }
            private set
            {
                _fieldLinks = value;
            }
        }

        private List<Guid> _fieldLinksToRemove;
        public List<Guid> FieldLinksToRemove
        {
            get
            {
                if (_fieldLinksToRemove == null)
                    LoadFieldLinkCollection();

                return _fieldLinksToRemove;
            }
            private set
            {
                _fieldLinksToRemove = value;
            }
        }

        [SPGENPropertyMappingAttribute(NoXmlAttribute = true, DisableOMUpdate = true)]
        public SPGENProvisionEventCallBehavior ProvisionEventCallBehavior { get { return base.GetPropertyValue<SPGENProvisionEventCallBehavior>("ProvisionEventCallBehavior"); } set { base.SetPropertyValue("ProvisionEventCallBehavior", value); } }

        [SPGENPropertyMappingAttribute(NoXmlAttribute = true, DisableOMUpdate = true)]
        public Type InheritsType { get { return base.GetPropertyValue<Type>("InheritsType"); } set { base.SetPropertyValue("InheritsType", value); } }

        private void LoadFieldLinkCollection()
        {
            _fieldLinks = new SPGENFieldLinkCollection();
            _fieldLinksToRemove = new List<Guid>();

            try
            {
                LoadFieldLinksFromDefinitions();

                if (this.InheritsType == null)
                    return;

                var ct = SPGENElementManager.GetInstance(this.InheritsType) as SPGENContentTypeBase;
                if (ct == null)
                    throw new SPGENGeneralException("The inherited type does not inherit from SPGENContentTypeBase.");

                var fieldLinksToRemove = new List<SPGENFieldLinkProperties>();

                foreach (var link in ct.StaticDefinition.FieldLinks)
                {
                    if (ct.StaticDefinition.FieldLinksToRemove.Contains(link.ID))
                    {
                        fieldLinksToRemove.Add(link);
                        continue;
                    }

                    if (_fieldLinks.Contains(link.ID) || _fieldLinksToRemove.Contains(link.ID))
                        continue;

                    var clonedLink = link.Clone();

                    _fieldLinks.Add(clonedLink);
                }

                foreach (var fl in fieldLinksToRemove)
                {
                    _fieldLinks.RemoveDirect(fl);
                }

            }
            finally
            {
                _fieldLinks.ResetUpdatedStatus();
            }
        }

        private void LoadFieldLinksFromDefinitions()
        {
            var linkCollection = SPGENCommon.GetAllOwnAndInheritedFieldLinks(this.ID.ToString(), this.Inherits);

            foreach (KeyValuePair<Guid, XmlElement> kvp in linkCollection)
            {
                var link = new SPGENFieldLinkProperties(kvp.Value);

                _fieldLinks.Add(link, false, false, true);
            }

            if (!this.IsFromSPDefinition)
                return;

            XDocument doc = XDocument.Parse(this.ElementDefinitionXml.OuterXml);

            foreach (XElement element in doc.Descendants())
            {
                if (element.Name.LocalName != "RemoveFieldRef")
                    continue;

                var q = from a in element.Attributes()
                        where a.Name.LocalName.Equals("ID", StringComparison.InvariantCultureIgnoreCase)
                        select a;

                if (q.Count<XAttribute>() == 0)
                    continue;

                Guid id = new Guid(q.First<XAttribute>().Value);

                if (_fieldLinksToRemove.FirstOrDefault<Guid>(l => l == id) != null)
                    continue;

                _fieldLinksToRemove.Add(id);
            }
        }

        public SPGENContentTypeProperties this[int lcid]
        {
            get { return GetLocalizedInstance<SPGENContentTypeProperties, SPGENContentTypeAttribute>(lcid); }
        }
        public SPGENContentTypeProperties this[CultureInfo cultureInfo]
        {
            get { return this[cultureInfo.LCID]; }
        }

        protected override void OnAfterInitialization()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = this.ElementType.Name;
            }
        }

        protected override string ElementName
        {
            get { return "ContentType"; }
        }
        protected override string ElementIdAttribute
        {
            get { return "ID"; }
        }
        internal override object ElementIdValue
        {
            get
            {
                if (this.ID == SPContentTypeId.Empty)
                {
                    this.ID = new SPContentTypeId(GetElementIDValueFromAttribute<SPGENContentTypeAttribute>("ID"));
                }

                return this.ID; 
            }
            set { this.ID = new SPContentTypeId(value.ToString()); }
        }

        public object GetDynamicProperty(Expression<Func<SPContentType, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPContentType, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPContentType>(property, value);
        }

        public class ConvertID : ISPGENPropertyConverter
        {
            public object ConvertFrom(object Parent, object Value)
            {
                return new SPContentTypeId(Value.ToString());
            }

            public object ConvertTo(object Parent, object Value)
            {
                return Value.ToString();
            }
        }
    }

}
