using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENFieldLinkCollection : SPGENElementCollectionBase<SPGENFieldLinkProperties, Guid>
    {
        public bool RestoreInheritedLinks { get; set; }

        protected override bool IsItemEqual(SPGENFieldLinkProperties item1, SPGENFieldLinkProperties item2)
        {
            return item1.ID == item2.ID;
        }

        protected override Guid GetIdentifier(SPGENFieldLinkProperties item)
        {
            return item.ID;
        }

        public int IndexOf(string fieldId)
        {
            return this.IndexOf(new Guid(fieldId));
        }

        public int IndexOf(Guid fieldId)
        {
            return this.IndexOf(new SPGENFieldLinkProperties() { ID = fieldId });
        }

        public SPGENFieldLinkProperties Add<TField>()
            where TField : SPGENFieldBase, new()
        {
            return Add(typeof(TField));
        }

        public SPGENFieldLinkProperties Add(Type type)
        {
            var typeInstance = SPGENElementManager.GetInstance(type) as SPGENFieldBase;
            var field = typeInstance.StaticDefinition;

            var link = new SPGENFieldLinkProperties();

            link.ID = field.ID;
            link.InternalName = field.InternalName;
            link.ParentFieldElement = type;

            this.Add(link, true);
            this.AddElementInstance(link.ID, typeInstance);

            return link;
        }

        public SPGENFieldLinkProperties Add(Guid fieldId)
        {
            var link = new SPGENFieldLinkProperties(fieldId);

            this.Add(link, true);

            return link;
        }

        public bool Contains(string fieldId)
        {
            return this.Contains(new Guid(fieldId));
        }

        public bool Contains(Guid fieldId)
        {
            return this.Contains(new SPGENFieldLinkProperties(fieldId));
        }

        public bool Remove(string fieldId)
        {
            return this.Remove(new Guid(fieldId));
        }

        public bool Remove(Guid fieldId)
        {
            return this.Remove(new SPGENFieldLinkProperties(fieldId));
        }

        public void Provision(ref SPContentType contentType, bool update, out IList<SPGENFieldBase> fieldElementsInCollection)
        {
            fieldElementsInCollection = new List<SPGENFieldBase>();
            var itemsToProvision = this.GetAllAddedAndUpdatedItems();

            foreach (var item in itemsToProvision)
            {
                if (contentType.ParentList != null && item.ParentFieldElement != null)
                {
                    if (this.ElementInstanceExists(item.ID))
                    {
                        var fieldInstance = GetElementInstance<SPGENFieldBase>(item.ID);
                        var fieldProperties = fieldInstance.InstanceDefinition;
                        
                        if (fieldProperties.ProvisionEventCallBehavior != SPGENProvisionEventCallBehavior.OnWeb)
                        {
                            fieldInstance.FireOnProvisionStarted(fieldProperties, contentType.ParentList.Fields, true);
                        }

                        fieldElementsInCollection.Add(fieldInstance);
                    }
                }

                SPFieldLink fieldLink = contentType.FieldLinks[item.ID];
                if (fieldLink == null)
                {
                    SPField field = GetSPField(contentType, item.ID);
                    fieldLink = new SPFieldLink(field);

                    contentType.FieldLinks.Add(fieldLink);

                    fieldLink.AggregationFunction = item.IsPropertyValueUpdated("AggregationFunction") ? field.AggregationFunction : item.AggregationFunction;
                    fieldLink.Customization = item.Customization;

                    if (item.IsPropertyValueUpdated("DisplayName") && item.DisplayName != null)
                    {
                        fieldLink.DisplayName = item.DisplayName;
                    }
                    else
                    {
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.LoadXml(field.SchemaXmlWithResourceTokens);

                        string fieldTitle = xmldoc.DocumentElement.GetAttribute("DisplayName");
                        if (string.IsNullOrEmpty(fieldTitle))
                        {
                            fieldLink.DisplayName = item.IsPropertyValueUpdated("DisplayName") ? item.DisplayName : field.Title;
                        }
                        else
                        {
                            fieldLink.DisplayName = fieldTitle;
                        }
                    }

                    fieldLink.Hidden = item.IsPropertyValueUpdated("Hidden") ? item.Hidden : field.Hidden;
                    fieldLink.PIAttribute = item.IsPropertyValueUpdated("PIAttribute") ? item.PIAttribute : field.PIAttribute;
                    fieldLink.PITarget = item.IsPropertyValueUpdated("PITarget") ? item.PITarget : field.PITarget;
                    fieldLink.PrimaryPIAttribute = item.IsPropertyValueUpdated("PrimaryPIAttribute") ? item.PrimaryPIAttribute : field.PrimaryPIAttribute;
                    fieldLink.PrimaryPITarget = item.IsPropertyValueUpdated("PrimaryPITarget") ? item.PrimaryPITarget : field.PrimaryPITarget;
                    fieldLink.ReadOnly = item.IsPropertyValueUpdated("ReadOnly") ? item.ReadOnly : field.ReadOnlyField;
                    fieldLink.Required = item.IsPropertyValueUpdated("Required") ? item.Required : field.Required;
                    fieldLink.ShowInDisplayForm = item.IsPropertyValueUpdated("Hidden") || !field.ShowInDisplayForm.HasValue ? item.ShowInDisplayForm : field.ShowInDisplayForm.Value;
                    fieldLink.XPath = item.IsPropertyValueUpdated("XPath") ? item.XPath : field.XPath;
                }
                else if (this.CanUpdate)
                {
                    if (item.IsPropertyValueUpdated("AggregationFunction"))
                        fieldLink.AggregationFunction = item.AggregationFunction;

                    if (item.IsPropertyValueUpdated("Customization"))
                        fieldLink.Customization = item.Customization;

                    if (item.IsPropertyValueUpdated("DisplayName"))
                        if (item.DisplayName != null)
                            fieldLink.DisplayName = item.DisplayName;

                    if (item.IsPropertyValueUpdated("Hidden"))
                        fieldLink.Hidden = item.Hidden;

                    if (item.IsPropertyValueUpdated("PIAttribute"))
                        fieldLink.PIAttribute = item.PIAttribute;

                    if (item.IsPropertyValueUpdated("PITarget"))
                        fieldLink.PITarget = item.PITarget;

                    if (item.IsPropertyValueUpdated("PrimaryPIAttribute"))
                        fieldLink.PrimaryPIAttribute = item.PrimaryPIAttribute;

                    if (item.IsPropertyValueUpdated("PrimaryPITarget"))
                        fieldLink.PrimaryPITarget = item.PrimaryPITarget;

                    if (item.IsPropertyValueUpdated("ReadOnly"))
                        fieldLink.ReadOnly = item.ReadOnly;

                    if (item.IsPropertyValueUpdated("Required"))
                        fieldLink.Required = item.Required;

                    if (item.IsPropertyValueUpdated("ShowInDisplayForm"))
                        fieldLink.ShowInDisplayForm = item.ShowInDisplayForm;

                    if (item.IsPropertyValueUpdated("XPath"))
                        fieldLink.XPath = item.XPath;
                }
            }


            if (this.CanUpdate)
            {
                if (this.IsExclusiveAdd)
                {
                    var removeFields = new List<Guid>();

                    foreach (SPFieldLink link in contentType.FieldLinks)
                    {
                        if (!this.Contains(link.Id) && !itemsToProvision.Exists(f => f.ID == link.Id))
                        {
                            if (contentType.Parent != null)
                            {
                                if (contentType.Parent.FieldLinks[link.Id] == null)
                                {
                                    removeFields.Add(link.Id);
                                }
                            }
                        }
                    }

                    foreach (var g in removeFields)
                    {
                        contentType.FieldLinks.Delete(g);
                    }
                }
                else
                {
                    if (this.RestoreInheritedLinks)
                    {
                        SPContentType currentContentType = contentType.Parent;
                        while (currentContentType.Id.ToString() != "0x")
                        {
                            foreach (SPFieldLink link in currentContentType.FieldLinks)
                            {
                                if (contentType.FieldLinks[link.Id] != null)
                                    continue;

                                SPField field = GetSPField(currentContentType, link.Id);

                                SPFieldLink newLink = new SPFieldLink(field);

                                contentType.FieldLinks.Add(newLink);

                            }

                            currentContentType = currentContentType.Parent;
                        }
                    }

                    //Remove field links
                    var removedItems = this.GetAllRemovedItems();

                    foreach (var item in removedItems)
                    {
                        if (contentType.FieldLinks[item.ID] != null)
                            contentType.FieldLinks.Delete(item.ID);
                    }

                }
            }


            if (update)
            {
                bool updateChildren = (contentType.ParentList == null);

                contentType.Update(updateChildren);
            }
        }

        private SPField GetSPField(SPContentType contentType, Guid fieldId)
        {
            SPField field;
            if (contentType.ParentWeb != null && contentType.ParentWeb.AvailableFields.Contains(fieldId))
            {
                field = contentType.ParentWeb.AvailableFields[fieldId];
            }
            else if (contentType.ParentList != null && contentType.ParentList.Fields.Contains(fieldId))
            {
                field = contentType.ParentList.Fields[fieldId];
            }
            else
            {
                throw new SPGENGeneralException("Could not add field link '" + fieldId.ToString() + "' to content type '" + contentType.Id.ToString() + "'.");
            }

            return field;
        }
    }
}
