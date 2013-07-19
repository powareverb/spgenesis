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

namespace SPGenesis.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SPGENFieldAttribute : SPGENElementAttributeBase
    {
        public bool AllowMultipleValues { get; set; }
        public string AggregationFunction { get; set; }
        public string BdcField { get; set; }
        public string BdcEntityName { get; set; }
        public string BdcEntityNamespace { get; set; }
        public string BdcSystemInstance { get; set; }
        public SPCalendarType CalendarType { get; set; }
        public SPDateTimeFieldFormatType DateFormat { get; set; }
        public string DefaultFormula { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public int DifferencingLimit { get; set; }
        public string Direction { get; set; }
        public SPNumberFormatTypes DisplayFormat{ get; set; } 
        public string DisplayName { get; set; }
        public SPUrlFieldFormatType DisplayUrlFormat{ get; set; } 
        public string DisplaySize { get; set; }
        public SPChoiceFormatType EditFormat { get; set; }
        public bool EnforceUniqueValues { get; set; }
        public bool FillInChoice { get; set; }
        public bool Filterable { get; set; }
        public string Formula { get; set; }
        public string Group { get; set; }
        public bool Hidden { get; set; }
        public string ID { get; set; }
        public bool Indexed { get; set; }
        public string InternalName { get; set; }
        public string JumpToField { get; set; }
        public double MaximumValue { get; set; }
        public int MaxLength { get; set; }
        public double MinimumValue { get; set; }
        public bool NoCrawl { get; set; }
        public int NumberOfLines { get; set; }
        public SPFieldType OutputType { get; set; }
        public bool Presence { get; set; }
        public SPPreviewValueSize PreviewValueSize{ get; set; } 
        public bool ReadOnlyField { get; set; }
        public string RelatedField { get; set; }
        public string RelatedFieldWssStaticName { get; set; }
        public SPRelationshipDeleteBehavior RelationshipDeleteBehavior { get; set; }
        public bool Required { get; set; }
        public bool RestrictedMode { get; set; }
        public bool RichText { get; set; }
        public SPRichTextMode RichTextMode{ get; set; } 
        public bool Sealed { get; set; }
        public int SelectionGroup { get; set; }
        public SPFieldUserSelectionMode SelectionMode{ get; set; } 
        public bool ShowAsPercentage { get; set; }
        public bool ShowInDisplayForm { get; set; }
        public bool ShowInEditForm { get; set; }
        public bool ShowInListSettings { get; set; }
        public bool ShowInNewForm { get; set; }
        public bool ShowInVersionHistory { get; set; }
        public bool ShowInViewForms { get; set; }
        public bool Sortable { get; set; }
        public string StaticName { get; set; }
        public string ValidationFormula { get; set; }
        public string ValidationMessage { get; set; }
        public bool UnlimitedLengthInDocumentLibrary { get; set; }

        public SPFieldType Type { get; set; }
        public string CustomType { get; set; }
        public object LookupList { get; set; }
        public object LookupField { get; set; }
        public object LookupFieldRef { get; set; }

        public SPGENProvisionEventCallBehavior ProvisionEventCallBehavior { get; set; }
    }
}
