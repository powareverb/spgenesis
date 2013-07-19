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
    public sealed class SPGENListInstanceProperties : SPGENElementProperties
    {
        private XmlDocument schemaXml;

        [SPGENPropertyMappingAttribute(OMPropertyResourceName = "TitleResource")]
        public string Title { get { return base.GetPropertyValue<string>("Title"); } set { base.SetPropertyValue("Title", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyResourceName = "DescriptionResource")]
        public string Description { get { return base.GetPropertyValue<string>("Description"); } set { base.SetPropertyValue("Description", value); } }
        [SPGENPropertyMappingAttribute(DisableOMUpdate = true)]
        public int TemplateType { get { return base.GetPropertyValue<int>("TemplateType"); } set { base.SetPropertyValue("TemplateType", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Url", DisableOMUpdate = true)]
        public string WebRelURL { get { return base.GetPropertyValue<string>("WebRelURL"); } set { base.SetPropertyValue("WebRelURL", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "FeatureId", DisableOMUpdate = true)]
        public string TemplateFeatureId { get { return base.GetPropertyValue<string>("TemplateFeatureId"); } set { base.SetPropertyValue("TemplateFeatureId", value); } }
        [SPGENPropertyMappingAttribute]
        public bool OnQuickLaunch { get { return base.GetPropertyValue<bool>("OnQuickLaunch"); } set { base.SetPropertyValue("OnQuickLaunch", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public bool ContentTypesEnabled { get { return base.GetPropertyValue<bool>("ContentTypesEnabled"); } set { base.SetPropertyValue("ContentTypesEnabled", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public SPListDataSource DataSource { get { return base.GetPropertyValue<SPListDataSource>("DataSource"); } set { base.SetPropertyValue("DataSource", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true, DisableOMUpdate = true)]
        public SPGENListInstanceGetMethod GetListDefaultMethod { get { return base.GetPropertyValue<SPGENListInstanceGetMethod>("GetListDefaultMethod"); } set { base.SetPropertyValue("GetListDefaultMethod", value); } }

        private SPGENEventReceiverCollection _eventReceivers;
        public SPGENEventReceiverCollection EventReceivers
        {
            get
            {
                if (_eventReceivers == null)
                    _eventReceivers = new SPGENEventReceiverCollection();

                return _eventReceivers;
            }
            private set
            {
                _eventReceivers = value;
            }
        }

        private SPGENListFieldCollection _fields;
        public SPGENListFieldCollection Fields
        {
            get
            {
                if (_fields == null)
                    LoadFieldCollection();

                return _fields;
            }
            private set
            {
                _fields = value;
            }
        }

        private SPGENListContentTypeCollection _contentTypes;
        public SPGENListContentTypeCollection ContentTypes
        {
            get
            {
                if (_contentTypes == null)
                    LoadContentTypeCollection();

                return _contentTypes;
            }
            private set
            {
                _contentTypes = value;
            }
        }

        private SPGENListViewCollection _views;
        public SPGENListViewCollection Views
        {
            get
            {
                if (_views == null)
                    LoadViewCollection();

                return _views;
            }
            private set
            {
                _views = value;
            }
        }

        public SPGENListInstanceProperties this[int lcid]
        {
            get { return GetLocalizedInstance<SPGENListInstanceProperties, SPGENListInstanceAttribute>(lcid); }
        }

        public SPGENListInstanceProperties this[CultureInfo cultureInfo]
        {
            get { return this[cultureInfo.LCID]; }
        }

        protected override string ElementName
        {
            get { return "ListInstance"; }
        }

        protected override string ElementIdAttribute
        {
            get { return "Url"; }
        }

        internal override object ElementIdValue
        {
            get
            {
                if (this.WebRelURL == null)
                {
                    this.WebRelURL = GetElementIDValueFromAttribute<SPGENListInstanceAttribute>("WebRelURL");
                }

                return this.WebRelURL; 
            }
            set { this.WebRelURL = value.ToString(); }
        }

        public object GetDynamicProperty(Expression<Func<SPList, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPList, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPList>(property, value);
        }

        private void LoadFieldCollection()
        {
            _fields = new SPGENListFieldCollection();

            EnsureSchemaXml();
            if (schemaXml == null)
                return;

            try
            {
                SPGENCommon.PopulateFieldCollection(_fields, schemaXml);
            }
            finally
            {
                _fields.ResetUpdatedStatus();
            }
        }

        private void LoadContentTypeCollection()
        {
            _contentTypes = new SPGENListContentTypeCollection();

            EnsureSchemaXml();
            if (schemaXml == null)
                return;

            SPGENCommon.PopulateContentTypeCollection(_contentTypes, schemaXml);

            _contentTypes.ResetUpdatedStatus();
        }

        private void LoadViewCollection()
        {
            _views = new SPGENListViewCollection();

            EnsureSchemaXml();
            if (schemaXml == null)
                return;

            try
            {
                SPGENCommon.PopulateViewCollection(_views, schemaXml);
            }
            finally
            {
                _views.ResetUpdatedStatus();
            }
        }

        private void EnsureSchemaXml()
        {
            if (schemaXml != null)
                return;

            Guid id;
            if (string.IsNullOrEmpty(this.TemplateFeatureId))
            {
                id = SPGENCommon.GetFeatureIdForBuiltInListType(this.TemplateType);
                if (id == Guid.Empty)
                    throw new SPGENGeneralException("Could not find list schema xml for list template type " + this.TemplateType.ToString() + ". You need to supply the template feature id where this lite template is defined.");
            }
            else
            {
                id = new Guid(this.TemplateFeatureId);
            }

            SPFeatureDefinition featureDef = SPFarm.Local.FeatureDefinitions[id];

            if (featureDef == null)
                return;

            SPElementDefinitionCollection collection = featureDef.GetElementDefinitions(System.Globalization.CultureInfo.CurrentUICulture);

            foreach (SPElementDefinition element in collection)
            {
                if (element.XmlDefinition.LocalName != "ListTemplate")
                    continue;

                try
                {
                    XmlElement el = element.XmlDefinition as XmlElement;
                    if (int.Parse(el.GetAttribute("Type")) == this.TemplateType)
                    {
                        string listName = null;
                        try
                        {
                            XmlElement el2 = element.XmlDefinition as XmlElement;
                            if (int.Parse(el2.GetAttribute("Type")) == this.TemplateType)
                            {
                                listName = el2.GetAttribute("Name");
                            }
                        }
                        catch { }

                        if (string.IsNullOrEmpty(listName))
                            return;

                        string path = element.FeatureDefinition.RootDirectory + "\\" + listName + "\\Schema.xml";
                        if (!System.IO.File.Exists(path))
                            return;

                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(path);

                        schemaXml = xmldoc;
                    }
                }
                catch (Exception ex)
                {
                    throw new SPGENGeneralException("Error resolving the schema file for list type '" + this.TemplateType.ToString() + "'.", ex);
                }
            }
        }
    }

}
