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
    public sealed class SPGENFieldProperties<TSPFieldType> : SPGENFieldProperties
        where TSPFieldType : SPField
    {
        public SPGENFieldProperties() { }
        internal SPGENFieldProperties(XmlNode elementDefinitionXml) : base(elementDefinitionXml) { }

        public object GetDynamicProperty(Expression<Func<TSPFieldType, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<TSPFieldType, object>> property, object value)
        {
            AddDynamicPropertyInternal<TSPFieldType>(property, value);
        }
    }

    public class SPGENFieldProperties : SPGENElementProperties
    {
        public SPGENFieldProperties() { }
        internal SPGENFieldProperties(XmlNode elementDefinitionXml) : base(elementDefinitionXml) { }

        [SPGENPropertyMappingAttribute(XmlAttributeName = "Mult")]
        public bool AllowMultipleValues { get { return base.GetPropertyValue<bool>("AllowMultipleValues"); } set { base.SetPropertyValue("AllowMultipleValues", value); } }
        [SPGENPropertyMappingAttribute]
        public string AggregationFunction { get { return base.GetPropertyValue<string>("AggregationFunction"); } set { base.SetPropertyValue("AggregationFunction", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "BdcField")]
        public string BdcField { get { return base.GetPropertyValue<string>("BdcField"); } set { base.SetPropertyValue("BdcField", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "EntityName", OMPropertyName = "EntityName")]
        public string BdcEntityName { get { return base.GetPropertyValue<string>("BdcEntityName"); } set { base.SetPropertyValue("BdcEntityName", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "EntityNamespace", OMPropertyName = "EntityNamespace")]
        public string BdcEntityNamespace { get { return base.GetPropertyValue<string>("BdcEntityNamespace"); } set { base.SetPropertyValue("BdcEntityNamespace", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "SystemInstance", OMPropertyName = "SystemInstanceName")]
        public string BdcSystemInstance { get { return base.GetPropertyValue<string>("BdcSystemInstance"); } set { base.SetPropertyValue("BdcSystemInstance", value); } }
        [SPGENPropertyMappingAttribute]
        public SPCalendarType CalendarType { get { return base.GetPropertyValue<SPCalendarType>("CalendarType"); } set { base.SetPropertyValue("CalendarType", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Format")]
        public SPDateTimeFieldFormatType DateFormat { get { return base.GetPropertyValue<SPDateTimeFieldFormatType>("DateFormat"); } set { base.SetPropertyValue("DateFormat", value); } }
        [SPGENPropertyMappingAttribute]
        public string DefaultFormula { get { return base.GetPropertyValue<string>("DefaultFormula"); } set { base.SetPropertyValue("DefaultFormula", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public string DefaultValue { get { return base.GetPropertyValue<string>("DefaultValue"); } set { base.SetPropertyValue("DefaultValue", value); } }
        [SPGENPropertyMappingAttribute]
        public string Description { get { return base.GetPropertyValue<string>("Description"); } set { base.SetPropertyValue("Description", value); } }
        [SPGENPropertyMappingAttribute]
        public int DifferencingLimit { get { return base.GetPropertyValue<int>("DifferencingLimit"); } set { base.SetPropertyValue("DifferencingLimit", value); } }
        [SPGENPropertyMappingAttribute]
        public string Direction { get { return base.GetPropertyValue<string>("Direction"); } set { base.SetPropertyValue("Direction", value); } }
        [SPGENPropertyMappingAttribute]
        public SPNumberFormatTypes DisplayFormat { get { return base.GetPropertyValue<SPNumberFormatTypes>("DisplayFormat"); } set { base.SetPropertyValue("DisplayFormat", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyName="Title")]
        public string DisplayName { get { return base.GetPropertyValue<string>("DisplayName"); } set { base.SetPropertyValue("DisplayName", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyName = "DisplayFormat", XmlAttributeName = "Format")]
        public SPUrlFieldFormatType DisplayUrlFormat { get { return base.GetPropertyValue<SPUrlFieldFormatType>("DisplayUrlFormat"); } set { base.SetPropertyValue("DisplayUrlFormat", value); } }
        [SPGENPropertyMappingAttribute]
        public string DisplaySize { get { return base.GetPropertyValue<string>("DisplaySize"); } set { base.SetPropertyValue("DisplaySize", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyName = "DisplayFormat", XmlAttributeName = "Format")]
        public SPChoiceFormatType EditFormat { get { return base.GetPropertyValue<SPChoiceFormatType>("EditFormat"); } set { base.SetPropertyValue("EditFormat", value); } }
        [SPGENPropertyMappingAttribute]
        public bool EnforceUniqueValues { get { return base.GetPropertyValue<bool>("EnforceUniqueValues"); } set { base.SetPropertyValue("EnforceUniqueValues", value); } }
        [SPGENPropertyMappingAttribute]
        public bool FillInChoice { get { return base.GetPropertyValue<bool>("FillInChoice"); } set { base.SetPropertyValue("FillInChoice", value); } }
        [SPGENPropertyMappingAttribute(DisableOMUpdate = true)]
        public bool Filterable { get { return base.GetPropertyValue<bool>("Filterable"); } set { base.SetPropertyValue("Filterable", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute=true)]
        public string Formula { get { return base.GetPropertyValue<string>("Formula"); } set { base.SetPropertyValue("Formula", value); } }
        [SPGENPropertyMappingAttribute]
        public string Group { get { return base.GetPropertyValue<string>("Group"); } set { base.SetPropertyValue("Group", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Hidden { get { return base.GetPropertyValue<bool>("Hidden"); } set { base.SetPropertyValue("Hidden", value); } }
        [SPGENPropertyMappingAttribute(OMPropertyName = "Id", FromAttributeConverter = typeof(ConvertID), ToXmlConverter = typeof(ConvertID))]
        public Guid ID { get { return base.GetPropertyValue<Guid>("ID"); } set { base.SetPropertyValue("ID", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Indexed { get { return base.GetPropertyValue<bool>("Indexed"); } set { base.SetPropertyValue("Indexed", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Name")]
        public string InternalName { get { return base.GetPropertyValue<string>("InternalName"); } set { base.SetPropertyValue("InternalName", value); } }
        [SPGENPropertyMappingAttribute]
        public string JumpToField { get { return base.GetPropertyValue<string>("JumpToField"); } set { base.SetPropertyValue("JumpToField", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Max")]
        public double MaximumValue { get { return base.GetPropertyValue<double>("MaximumValue"); } set { base.SetPropertyValue("MaximumValue", value); } }
        [SPGENPropertyMappingAttribute]
        public int MaxLength { get { return base.GetPropertyValue<int>("MaxLength"); } set { base.SetPropertyValue("MaxLength", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Min")]
        public double MinimumValue { get { return base.GetPropertyValue<double>("MinimumValue"); } set { base.SetPropertyValue("MinimumValue", value); } }
        [SPGENPropertyMappingAttribute]
        public bool NoCrawl { get { return base.GetPropertyValue<bool>("NoCrawl"); } set { base.SetPropertyValue("NoCrawl", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "NumLines")]
        public int NumberOfLines { get { return base.GetPropertyValue<int>("NumberOfLines"); } set { base.SetPropertyValue("NumberOfLines", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "ResultType")]
        public SPFieldType OutputType { get { return base.GetPropertyValue<SPFieldType>("OutputType"); } set { base.SetPropertyValue("OutputType", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Presence { get { return base.GetPropertyValue<bool>("Presence"); } set { base.SetPropertyValue("Presence", value); } }
        [SPGENPropertyMappingAttribute]
        public SPPreviewValueSize PreviewValueSize { get { return base.GetPropertyValue<SPPreviewValueSize>("PreviewValueSize"); } set { base.SetPropertyValue("PreviewValueSize", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "ReadOnly")]
        public bool ReadOnlyField { get { return base.GetPropertyValue<bool>("ReadOnlyField"); } set { base.SetPropertyValue("ReadOnlyField", value); } }
        [SPGENPropertyMappingAttribute]
        public string RelatedField { get { return base.GetPropertyValue<string>("RelatedField"); } set { base.SetPropertyValue("RelatedField", value); } }
        [SPGENPropertyMappingAttribute(DisableOMUpdate = true)]
        public string RelatedFieldWssStaticName { get { return base.GetPropertyValue<string>("RelatedFieldWssStaticName"); } set { base.SetPropertyValue("RelatedFieldWssStaticName", value); } }
        [SPGENPropertyMappingAttribute]
        public SPRelationshipDeleteBehavior RelationshipDeleteBehavior { get { return base.GetPropertyValue<SPRelationshipDeleteBehavior>("RelationshipDeleteBehavior"); } set { base.SetPropertyValue("RelationshipDeleteBehavior", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Required { get { return base.GetPropertyValue<bool>("Required"); } set { base.SetPropertyValue("Required", value); } }
        [SPGENPropertyMappingAttribute]
        public bool RestrictedMode { get { return base.GetPropertyValue<bool>("RestrictedMode"); } set { base.SetPropertyValue("RestrictedMode", value); } }
        [SPGENPropertyMappingAttribute]
        public bool RichText { get { return base.GetPropertyValue<bool>("RichText"); } set { base.SetPropertyValue("RichText", value); } }
        [SPGENPropertyMappingAttribute]
        public SPRichTextMode RichTextMode { get { return base.GetPropertyValue<SPRichTextMode>("RichTextMode"); } set { base.SetPropertyValue("RichTextMode", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Sealed { get { return base.GetPropertyValue<bool>("Sealed"); } set { base.SetPropertyValue("Sealed", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "UserSelectionGroup")]
        public int SelectionGroup { get { return base.GetPropertyValue<int>("SelectionGroup"); } set { base.SetPropertyValue("SelectionGroup", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "UserSelectionMode")]
        public SPFieldUserSelectionMode SelectionMode { get { return base.GetPropertyValue<SPFieldUserSelectionMode>("SelectionMode"); } set { base.SetPropertyValue("SelectionMode", value); } }
        [SPGENPropertyMappingAttribute]
        public bool ShowAsPercentage { get { return base.GetPropertyValue<bool>("ShowAsPercentage"); } set { base.SetPropertyValue("ShowAsPercentage", value); } }
        [SPGENPropertyMappingAttribute]
        public bool? ShowInDisplayForm { get { return base.GetPropertyValue<bool?>("ShowInDisplayForm"); } set { base.SetPropertyValue("ShowInDisplayForm", value); } }
        [SPGENPropertyMappingAttribute]
        public bool? ShowInEditForm { get { return base.GetPropertyValue<bool?>("ShowInEditForm"); } set { base.SetPropertyValue("ShowInEditForm", value); } }
        [SPGENPropertyMappingAttribute]
        public bool? ShowInListSettings { get { return base.GetPropertyValue<bool?>("ShowInListSettings"); } set { base.SetPropertyValue("ShowInListSettings", value); } }
        [SPGENPropertyMappingAttribute]
        public bool? ShowInNewForm { get { return base.GetPropertyValue<bool?>("ShowInNewForm"); } set { base.SetPropertyValue("ShowInNewForm", value); } }
        [SPGENPropertyMappingAttribute]
        public bool ShowInVersionHistory { get { return base.GetPropertyValue<bool>("ShowInVersionHistory"); } set { base.SetPropertyValue("ShowInVersionHistory", value); } }
        [SPGENPropertyMappingAttribute]
        public bool? ShowInViewForms { get { return base.GetPropertyValue<bool?>("ShowInViewForms"); } set { base.SetPropertyValue("ShowInViewForms", value); } }
        [SPGENPropertyMappingAttribute(DisableOMUpdate = true)]
        public bool Sortable { get { return base.GetPropertyValue<bool>("Sortable"); } set { base.SetPropertyValue("Sortable", value); } }
        [SPGENPropertyMappingAttribute]
        public string StaticName { get { return base.GetPropertyValue<string>("StaticName"); } set { base.SetPropertyValue("StaticName", value); } }
        [SPGENPropertyMappingAttribute]
        public string ValidationFormula { get { return base.GetPropertyValue<string>("ValidationFormula"); } set { base.SetPropertyValue("ValidationFormula", value); } }
        [SPGENPropertyMappingAttribute]
        public string ValidationMessage { get { return base.GetPropertyValue<string>("ValidationMessage"); } set { base.SetPropertyValue("ValidationMessage", value); } }
        [SPGENPropertyMappingAttribute]
        public bool UnlimitedLengthInDocumentLibrary { get { return base.GetPropertyValue<bool>("UnlimitedLengthInDocumentLibrary"); } set { base.SetPropertyValue("UnlimitedLengthInDocumentLibrary", value); } }

        private Dictionary<string, string> _choiceMappings;
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public Dictionary<string, string> ChoiceMappings
        {
            get
            {
                if (_choiceMappings == null)
                    _choiceMappings = new Dictionary<string, string>();

                return _choiceMappings;
            }
            set
            {
                _choiceMappings = value;
            }
        }

        private List<string> _choices;
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public List<string> Choices
        {
            get
            {
                if (_choices == null)
                    _choices = new List<string>();

                return _choices;
            }
            set
            {
                _choices = value;
            }
        }

        private List<string> _secondaryFieldBdcNames;
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public List<string> SecondaryFieldBdcNames
        {
            get
            {
                if (_secondaryFieldBdcNames == null)
                    _secondaryFieldBdcNames = new List<string>();

                return _secondaryFieldBdcNames;
            }
            set
            {
                _secondaryFieldBdcNames = value;
            }
        }

        private List<string> _fieldRefs;
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true)]
        public List<string> FieldRefs
        {
            get
            {
                if (_fieldRefs == null)
                    _fieldRefs = new List<string>();

                return _fieldRefs;
            }
            set
            {
                _fieldRefs = value;
            }
        }

        [SPGENPropertyMappingAttribute(XmlAttributeName = "Type", DisableOMUpdate = true, ToXmlConverter = typeof(ConvertFieldType), FromAttributeConverter = typeof(ConvertFieldType))]
        public SPFieldType Type { get { return base.GetPropertyValue<SPFieldType>("Type"); } set { base.SetPropertyValue("Type", value); } }
        [SPGENPropertyMappingAttribute(NoXmlAttribute = true, DisableOMUpdate = true)]
        public string CustomType { get { return base.GetPropertyValue<string>("CustomType"); } set { base.SetPropertyValue("CustomType", value); this.Type = SPFieldType.Invalid; } }

        [SPGENPropertyMappingAttribute(XmlAttributeName = "List", DisableOMUpdate = true, ToXmlConverter = typeof(ConvertLookupList))]
        public object LookupList { get { return base.GetPropertyValue<object>("LookupList"); } set { base.SetPropertyValue("LookupList", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "ShowField", DisableOMUpdate = true, ToXmlConverter = typeof(ConvertLookupField))]
        public object LookupField { get { return base.GetPropertyValue<object>("LookupField"); } set { base.SetPropertyValue("LookupField", value); } }
        public bool IsLookupField { get; set; }

        [SPGENPropertyMappingAttribute(XmlAttributeName = "FieldRef", DisableOMUpdate = true, ToXmlConverter = typeof(ConvertLookupFieldRef))]
        public object LookupFieldRef { get { return base.GetPropertyValue<object>("LookupFieldRef"); } set { base.SetPropertyValue("LookupFieldRef", value); } }

        public bool AddToDefaultView { get; set; }
        public bool AddToAllContentTypes { get; set; }

        [SPGENPropertyMappingAttribute(NoXmlAttribute = true, DisableOMUpdate = true)]
        public SPGENProvisionEventCallBehavior ProvisionEventCallBehavior { get { return base.GetPropertyValue<SPGENProvisionEventCallBehavior>("ProvisionEventCallBehavior"); } set { base.SetPropertyValue("ProvisionEventCallBehavior", value); } }

        public object GetDynamicProperty(Expression<Func<SPField, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPField, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPField>(property, value);
        }

        public void CreateBdcField(string entityName, string entityNamespace, string systemInstance, string bdcField, string[] secondaryFields)
        {
            this.BdcEntityName = entityName;
            this.BdcEntityNamespace = entityNamespace;
            this.BdcSystemInstance = systemInstance;
            this.BdcField = bdcField;

            this.RelatedField = entityName + "_ID";
            this.RelatedFieldWssStaticName = this.RelatedField;

            _secondaryFieldBdcNames = new List<string>(secondaryFields);
        }

        protected override void SetInitValues()
        {
            try
            {
                this.DisablePropertyTracking = true;

                this.Filterable = true;
                this.ShowInDisplayForm = true;
                this.ShowInEditForm = true;
                this.ShowInListSettings = true;
                this.ShowInNewForm = true;
                this.ShowInVersionHistory = true;
                this.ShowInViewForms = true;
                this.Sortable = true;
            }
            finally
            {
                this.DisablePropertyTracking = false;
            }
        }
        protected override string ElementName
        {
            get { return "Field"; }
        }
        protected override string ElementIdAttribute
        {
            get { return "ID"; }
        }
        internal override object ElementIdValue
        {
            get
            {
                if (this.ID == Guid.Empty)
                {
                    this.ID = new Guid(GetElementIDValueFromAttribute<SPGENFieldAttribute>("ID"));
                }

                return this.ID; 
            }
            set { this.ID = (Guid)value; }
        }

        protected override void OnAfterInitialization()
        {
            if (string.IsNullOrEmpty(this.InternalName))
            {
                this.InternalName = this.ElementType.Name;
            }

            if (string.IsNullOrEmpty(this.StaticName))
            {
                this.StaticName = this.InternalName;
            }

            if (string.IsNullOrEmpty(this.DisplayName))
            {
                this.DisplayName = this.InternalName;
            }
        }

        protected override void OnParseXmlDefinition(XmlNode xmlDefinition)
        {
            IEnumerable<XmlElement> childNodes = xmlDefinition.ChildNodes.OfType<XmlElement>();
            
            XmlElement xmlChoices = childNodes.FirstOrDefault<XmlElement>(n => n.LocalName.Equals("CHOICES", StringComparison.InvariantCultureIgnoreCase));
            if (xmlChoices != null)
            {
                _choices = new List<string>();

                foreach (XmlNode c in xmlChoices.ChildNodes)
                {
                    if (c.NodeType != XmlNodeType.Element || c.LocalName.ToUpper() != "CHOICE")
                        continue;

                    _choices.Add(c.InnerText);
                }
            }

            XmlElement xmlChoiceMappings = childNodes.FirstOrDefault<XmlElement>(n => n.LocalName.Equals("MAPPINGS", StringComparison.InvariantCultureIgnoreCase));
            if (xmlChoiceMappings != null)
            {
                _choiceMappings = new Dictionary<string, string>();

                foreach (XmlNode c in xmlChoiceMappings.ChildNodes)
                {
                    if (c.NodeType != XmlNodeType.Element || c.LocalName.ToUpper() != "MAPPING")
                        continue;

                    _choiceMappings.Add((c as XmlElement).GetAttribute("Value"), c.InnerText);
                }
            }

            XmlElement xmlFieldRefs = childNodes.FirstOrDefault<XmlElement>(n => n.LocalName.Equals("FieldRefs", StringComparison.InvariantCultureIgnoreCase));
            if (xmlFieldRefs != null)
            {
                _fieldRefs = new List<string>();

                foreach (XmlNode f in xmlFieldRefs)
                {
                    if (f.NodeType != XmlNodeType.Element || f.LocalName.ToUpper() != "FIELDREF")
                        continue;

                    _fieldRefs.Add(f.Attributes["Name"].Value);
                }
            }

            XmlElement xmlFormula = childNodes.FirstOrDefault<XmlElement>(n => n.LocalName.Equals("Forumula", StringComparison.InvariantCultureIgnoreCase));
            if (xmlFormula != null)
            {
                try
                {
                    this.DisablePropertyTracking = true;
                    this.Formula = xmlFormula.InnerText;
                }
                finally
                {
                    this.DisablePropertyTracking = false;
                }
            }
        }

        protected override void OnComposeXmlDefinition(XmlNode xmlDefinition)
        {
            bool useMappings = false;
            XmlNode xmlChoiceMappings = null;
            if (_choiceMappings != null && _choiceMappings.Count > 0)
            {
                xmlChoiceMappings = xmlDefinition.ChildNodes.Cast<XmlNode>().FirstOrDefault<XmlNode>(n => n.LocalName.Equals("MAPPINGS", StringComparison.InvariantCultureIgnoreCase));

                if (xmlChoiceMappings == null)
                {
                    xmlChoiceMappings = xmlDefinition.OwnerDocument.CreateElement("MAPPINGS");
                    xmlDefinition.AppendChild(xmlChoiceMappings);
                }

                foreach (KeyValuePair<string, string> kvp in _choiceMappings)
                {
                    XmlElement el = xmlDefinition.OwnerDocument.CreateElement("MAPPING");
                    el.SetAttribute("Value", kvp.Key);
                    el.InnerText = kvp.Value;

                    xmlChoiceMappings.AppendChild(el);
                }

                useMappings = true;
            }
            
            if ((_choices != null && _choices.Count > 0) || useMappings)
            {
                XmlNode xmlChoices = xmlDefinition.ChildNodes.Cast<XmlNode>().FirstOrDefault<XmlNode>(n => n.LocalName.Equals("CHOICES", StringComparison.InvariantCultureIgnoreCase));

                if (xmlChoices == null)
                {
                    xmlChoices = xmlDefinition.OwnerDocument.CreateElement("CHOICES");

                    if (useMappings)
                    {
                        xmlDefinition.InsertBefore(xmlChoices, xmlChoiceMappings);
                    }
                    else
                    {
                        xmlDefinition.AppendChild(xmlChoices);
                    }
                }

                if (!useMappings)
                {
                    foreach (string choice in _choices)
                    {
                        XmlElement el = xmlDefinition.OwnerDocument.CreateElement("CHOICE");
                        el.InnerText = choice;

                        xmlChoices.AppendChild(el);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in _choiceMappings)
                    {
                        XmlElement el = xmlDefinition.OwnerDocument.CreateElement("CHOICE");
                        el.InnerText = kvp.Value;

                        xmlChoices.AppendChild(el);
                    }
                }
            }



            if (this.LookupFieldRef != null)
            {
                if (!this.IsPropertyValueUpdated("ReadOnlyField"))
                {
                    this.ReadOnlyField = true;
                }
            }

            if (base.IsPropertyValueUpdated("Formula"))
            {
                XmlNode formulaXml = xmlDefinition.ChildNodes.Cast<XmlNode>().FirstOrDefault<XmlNode>(n => n.LocalName.Equals("Formula", StringComparison.InvariantCultureIgnoreCase));
                if (formulaXml == null)
                {
                    formulaXml = xmlDefinition.OwnerDocument.CreateElement("Formula");
                    xmlDefinition.AppendChild(formulaXml);
                }
                formulaXml.InnerText = this.Formula;
            }

            if (this.FieldRefs != null)
            {
                XmlNode fieldRefsXml = xmlDefinition.ChildNodes.Cast<XmlNode>().FirstOrDefault<XmlNode>(n => n.LocalName.Equals("FieldRefs", StringComparison.InvariantCultureIgnoreCase));
                if (fieldRefsXml == null && this.FieldRefs.Count > 0)
                {
                    fieldRefsXml = xmlDefinition.OwnerDocument.CreateElement("FieldRefs");
                    xmlDefinition.AppendChild(fieldRefsXml);
                }

                foreach (var fieldRef in this.FieldRefs)
                {
                    XmlElement fieldRefXml = xmlDefinition.OwnerDocument.CreateElement("FieldRef");
                    fieldRefXml.SetAttribute("Name", fieldRef);

                    fieldRefsXml.AppendChild(fieldRefXml);
                }
            }
        }

        public SPGENFieldProperties this[int lcid]
        {
            get { return GetLocalizedInstance<SPGENFieldProperties, SPGENFieldAttribute>(lcid); }
        }
        public SPGENFieldProperties this[CultureInfo cultureInfo]
        {
            get { return this[cultureInfo.LCID]; }
        }

        public void EnsureChoiceValue(string value)
        {
            if (!this.Choices.Contains(value))
                this.Choices.Add(value);
        }

        public void EnsureChoiceValue(string value, int position)
        {            
            if (position > this.Choices.Count)
                position = this.Choices.Count;

            int currentIndex = this.Choices.IndexOf(value);
            if (currentIndex == -1)
            {
                this.Choices.Insert(position, value);
            }
            else
            {
                this.Choices.Insert(position, value);
                this.Choices.RemoveAt(currentIndex);
            }
        }

        public void EnsureChoiceValue(string mappingValue, string text)
        {
            if (!this.ChoiceMappings.ContainsKey(mappingValue))
                this.ChoiceMappings.Add(mappingValue, text);
        }

        public XmlNode ComposeXmlDefinitionLookup(bool forceBoolUpperCase, SPFieldCollection fieldCollection)
        {
            SPWeb lookupListWeb = (fieldCollection.List != null) ? fieldCollection.List.ParentWeb : fieldCollection.Web;
            SPList lookupList = GetLookupFieldSource(this.LookupList, lookupListWeb, fieldCollection.List);
            SPField lookupField = GetLookupField(this.LookupField, lookupList.Fields);

            XmlElement element = this.CreateXmlDefinition(forceBoolUpperCase) as XmlElement;

            element.SetAttribute("List", lookupList.ID.ToString());
            element.SetAttribute("ShowField", lookupField.InternalName);

            return element;
        }

        private SPList GetLookupFieldSource(object definitionPropertyValue, SPWeb parentWeb, SPList parentList)
        {
            if (definitionPropertyValue is Type)
            {
                if (!(definitionPropertyValue as Type).IsSubclassOf(typeof(SPGENListInstanceBase)))
                {
                    throw new ArgumentException("The type must support the interface ISPGENListInstance.");
                }

                var list = SPGENElementManager.GetInstance(definitionPropertyValue as Type) as SPGENListInstanceBase;

                return list.GetList(parentWeb);
            }
            else if (definitionPropertyValue is string)
            {
                if (string.Equals((definitionPropertyValue as string), "Self", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (parentList == null)
                        throw new SPGENGeneralException("The lookup list can not be set to 'Self' if the field is a site scoped field.");

                    return parentList;
                }

                Guid g = Guid.Empty;
                try
                {
                    g = new Guid(definitionPropertyValue as string);
                }
                catch { }

                if (g == Guid.Empty)
                {
                    return parentWeb.GetList((parentWeb.ServerRelativeUrl != "/" ? parentWeb.ServerRelativeUrl : "") + "/" + definitionPropertyValue.ToString());
                }
                else
                {
                    return parentWeb.Lists[g];
                }
            }

            throw new SPGENGeneralException("Wrong lookup list type for field definition '" + this.ID.ToString() + "'.");
        }

        private SPField GetLookupField(object definitionPropertyValue, SPFieldCollection parentCollection)
        {
            if (definitionPropertyValue is Type)
            {
                var field = SPGENElementManager.GetInstance(definitionPropertyValue as Type) as SPGENFieldBase;
                if (field != null)
                {
                    return parentCollection[field.StaticDefinition.ID];
                }
            }
            else if (definitionPropertyValue is string)
            {
                string name = definitionPropertyValue.ToString();
                if (parentCollection.ContainsField(name))
                {
                    return parentCollection.GetFieldByInternalName(name);
                }
                else
                {
                    return parentCollection[name];
                }
            }
            else if (definitionPropertyValue is Guid)
            {
                return parentCollection[(Guid)definitionPropertyValue];
            }

            throw new SPGENGeneralException("Wrong lookup field type for field definition '" + this.ID.ToString() + "'.");
        }


        #region Converters

        public class ConvertID : ISPGENPropertyConverter
        {
            public object ConvertFrom(object Parent, object Value)
            {
                return new Guid(Value.ToString());
            }

            public object ConvertTo(object Parent, object Value)
            {
                return Value.ToString();
            }
        }

        public class ConvertFieldType : ISPGENPropertyConverter
        {
            public object ConvertFrom(object Parent, object Value)
            {
                SPFieldType t = SPFieldType.Invalid;
                var def = Parent as SPGENFieldProperties;

                try
                {
                    t = (SPFieldType)Enum.Parse(typeof(SPFieldType), Value.ToString());
                }
                catch (ArgumentException) { }

                if (t == SPFieldType.Invalid)
                {
                    def.CustomType = Value.ToString();
                }

                return t;
            }

            public object ConvertTo(object Parent, object Value)
            {
                var def = Parent as SPGENFieldProperties;
                if (def.Type == SPFieldType.Invalid)
                {
                    return def.CustomType;
                }
                else
                {
                    return def.Type.ToString();
                }
            }
        }

        public class ConvertLookupList : ISPGENPropertyConverter
        {
            public object ConvertFrom(object parent, object value)
            {
                return value;
            }

            public object ConvertTo(object parent, object value)
            {
                SPGENFieldAttribute a = parent as SPGENFieldAttribute;

                if (value is Type)
                {
                    if (!(value as Type).IsSubclassOf(typeof(SPGENListInstanceBase)))
                    {
                        throw new ArgumentException("The type must inherit from SPGENListInstanceBase.");
                    }

                    var list = SPGENElementManager.GetInstance(value as Type) as SPGENListInstanceBase;

                    return list.StaticDefinition.WebRelURL;
                }
                else if (value is string)
                {
                    return value as string;
                }
                else if (value == null)
                {
                    return null;
                }

                throw new SPGENGeneralException("Wrong lookup list type for field definition '" + a.ID.ToString() + "'.");
            }
        }

        public class ConvertLookupField : ISPGENPropertyConverter
        {
            public object ConvertFrom(object parent, object value)
            {
                return value;
            }

            public object ConvertTo(object parent, object value)
            {
                SPGENFieldAttribute a = parent as SPGENFieldAttribute;

                if (value is Type)
                {
                    if (!(value as Type).IsSubclassOf(typeof(SPGENFieldBase)))
                    {
                        throw new ArgumentException("The type must inherit from SPGENListInstanceBase.");
                    }

                    var field = SPGENElementManager.GetInstance(value as Type) as SPGENFieldBase;

                    return field.StaticDefinition.InternalName;
                }
                else if (value is string)
                {
                    return value as string;
                }
                else if (value == null)
                {
                    return null;
                }

                throw new SPGENGeneralException("Wrong lookup list type for field definition '" + a.ID.ToString() + "'.");
            }
        }

        public class ConvertLookupFieldRef : ISPGENPropertyConverter
        {
            public object ConvertFrom(object parent, object value)
            {
                return value;
            }

            public object ConvertTo(object parent, object value)
            {
                SPGENFieldAttribute a = parent as SPGENFieldAttribute;

                if (value is Type)
                {
                    if (!(value as Type).IsSubclassOf(typeof(SPGENFieldBase)))
                    {
                        throw new ArgumentException("The type must support the interface ISPGENField.");
                    }

                    var field = SPGENElementManager.GetInstance(value as Type) as SPGENFieldBase;

                    return field.StaticDefinition.ID.ToString();
                }
                else if (value is string)
                {
                    return value as string;
                }
                else if (value == null)
                {
                    return null;
                }

                throw new SPGENGeneralException("Wrong lookup list type for field definition '" + a.ID.ToString() + "'.");
            }
        }

        #endregion
    }
}
