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
    public sealed class SPGENFieldLinkProperties : SPGENElementProperties
    {
        [SPGENPropertyMappingAttribute]
        public string AggregationFunction { get { return base.GetPropertyValue<string>("AggregationFunction"); } set { base.SetPropertyValue("AggregationFunction", value); } }
        [SPGENPropertyMappingAttribute]
        public string Customization { get { return base.GetPropertyValue<string>("Customization"); } set { base.SetPropertyValue("Customization", value); } }
        [SPGENPropertyMappingAttribute]
        public string DisplayName { get { return base.GetPropertyValue<string>("DisplayName"); } set { base.SetPropertyValue("DisplayName", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Hidden { get { return base.GetPropertyValue<bool>("Hidden"); } set { base.SetPropertyValue("Hidden", value); } }
        [SPGENPropertyMappingAttribute(ToXmlConverter = typeof(ConvertFieldId))]
        public Guid ID { get { return base.GetPropertyValue<Guid>("ID"); } set { base.SetPropertyValue("ID", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Name")]
        public string InternalName { get { return base.GetPropertyValue<string>("InternalName"); } set { base.SetPropertyValue("InternalName", value); } }
        [SPGENPropertyMappingAttribute]
        public string PIAttribute { get { return base.GetPropertyValue<string>("PIAttribute"); } set { base.SetPropertyValue("PIAttribute", value); } }
        [SPGENPropertyMappingAttribute]
        public string PITarget { get { return base.GetPropertyValue<string>("PITarget"); } set { base.SetPropertyValue("PITarget", value); } }
        [SPGENPropertyMappingAttribute]
        public string PrimaryPIAttribute { get { return base.GetPropertyValue<string>("PrimaryPIAttribute"); } set { base.SetPropertyValue("PrimaryPIAttribute", value); } }
        [SPGENPropertyMappingAttribute]
        public string PrimaryPITarget { get { return base.GetPropertyValue<string>("PrimaryPITarget"); } set { base.SetPropertyValue("PrimaryPITarget", value); } }
        [SPGENPropertyMappingAttribute]
        public bool ReadOnly { get { return base.GetPropertyValue<bool>("ReadOnly"); } set { base.SetPropertyValue("ReadOnly", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Required { get { return base.GetPropertyValue<bool>("Required"); } set { base.SetPropertyValue("Required", value); } }
        [SPGENPropertyMappingAttribute]
        public bool ShowInDisplayForm { get { return base.GetPropertyValue<bool>("ShowInDisplayForm"); } set { base.SetPropertyValue("ShowInDisplayForm", value); } }
        [SPGENPropertyMappingAttribute]
        public string XPath { get { return base.GetPropertyValue<string>("XPath"); } set { base.SetPropertyValue("XPath", value); } }

        internal Type ParentFieldElement { get; set; }

        public SPGENFieldLinkProperties()
        {
            SetInitValues();
        }
        public SPGENFieldLinkProperties(Guid fieldId)
        {
            this.ID = fieldId;

            SetInitValues();
        }
        public SPGENFieldLinkProperties(string fieldId)
        {
            this.ID = new Guid(fieldId);

            SetInitValues();
        }

        internal SPGENFieldLinkProperties(XmlNode elementDefinitionXml) : base(elementDefinitionXml) 
        {
            SetInitValues();
        }

        internal SPGENFieldLinkProperties Clone()
        {
            SPGENFieldLinkProperties link;
            if (this.IsFromSPDefinition)
            {
                link = new SPGENFieldLinkProperties(this.ElementDefinitionXml);
            }
            else
            {
                link = new SPGENFieldLinkProperties();
            }

            link.DisablePropertyTracking = true;

            link.AggregationFunction = this.AggregationFunction;
            link.Customization = this.Customization;
            link.DisplayName = this.DisplayName;
            link.Hidden = this.Hidden;
            link.ID = this.ID;
            link.InternalName = this.InternalName;
            link.ParentFieldElement = this.ParentFieldElement;
            link.PIAttribute = this.PIAttribute;
            link.PITarget = this.PITarget;
            link.PrimaryPIAttribute = this.PrimaryPIAttribute;
            link.PrimaryPITarget = this.PrimaryPITarget;
            link.ReadOnly = this.ReadOnly;
            link.Required = this.Required;
            link.ShowInDisplayForm = this.ShowInDisplayForm;
            link.XPath = this.XPath;

            foreach (var kvp in this.DynamicProperties)
                link.DynamicProperties.Add(kvp);

            link.DisablePropertyTracking = false;
            return link;
        }

        public SPFieldLink CreateSPFieldLink(SPField field)
        {
            return
                new SPFieldLink(field)
                {
                    AggregationFunction = this.AggregationFunction,
                    Customization = this.Customization,
                    DisplayName = this.DisplayName,
                    Hidden = this.Hidden,
                    PIAttribute = this.PIAttribute,
                    PITarget = this.PITarget,
                    ReadOnly = this.ReadOnly,
                    Required = this.Required,
                    ShowInDisplayForm = this.ShowInDisplayForm,
                    XPath = this.XPath
                };
        }

        protected override void SetInitValues()
        {
            try
            {
                this.DisablePropertyTracking = true;

                this.ShowInDisplayForm = true;
            }
            finally
            {
                this.DisablePropertyTracking = false;
            }
        }

        public object GetDynamicProperty(Expression<Func<SPFieldLink, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPFieldLink, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPFieldLink>(property, value);
        }

        protected override string ElementName
        {
            get { return "FieldRef"; }
        }
        protected override string ElementIdAttribute
        {
            get { return "ID"; }
        }
        internal override object ElementIdValue
        {
            get { return this.ID; }
            set { this.ID = (Guid)value; }
        }

        public class ConvertFieldId : ISPGENPropertyConverter
        {
            public object ConvertFrom(object Parent, object Value)
            {
                return new Guid(Value.ToString());
            }

            public object ConvertTo(object Parent, object Value)
            {
                return Value as string;
            }
        }
    }
}
