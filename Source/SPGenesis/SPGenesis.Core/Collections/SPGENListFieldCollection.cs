using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public sealed class SPGENListFieldCollection : SPGENElementCollectionBase<SPGENFieldProperties, Guid>
    {
        public bool AutoPushChangesToListContentTypes { get; set; }

        protected override bool IsItemEqual(SPGENFieldProperties item1, SPGENFieldProperties item2)
        {
            return item1.ID == item2.ID;
        }
        protected override Guid GetIdentifier(SPGENFieldProperties item)
        {
            return item.ID;
        }

        public int IndexOf(string fieldId)
        {
            return this.IndexOf(new Guid(fieldId));
        }

        public int IndexOf(Guid fieldId)
        {
            return this.IndexOf(new SPGENFieldProperties<SPField>() { ID = fieldId });
        }

        public SPGENFieldProperties Add(Guid fieldId)
        {
            var item = this.Add(fieldId, false, false);
            this.Update(fieldId);

            return item;
        }

        public SPGENFieldProperties Add(Guid fieldId, bool addToDefaultView, bool addToAllContentTypes)
        {
            var field = new SPGENFieldProperties<SPField>() { ID = fieldId };

            field.AddToAllContentTypes = addToAllContentTypes;
            field.AddToDefaultView = addToDefaultView;
            
            this.Add(field, true);

            return this[field];
        }

        public SPGENFieldProperties Add<TField>()
            where TField : SPGENFieldBase, new()
        {
            return Add<TField>(false, false);
        }

        public SPGENFieldProperties Add<TField>(bool addToDefaultView, bool addToAllContentTypes)
            where TField : SPGENFieldBase, new()
        {
            return Add(typeof(TField), addToDefaultView, addToAllContentTypes);
        }

        public SPGENFieldProperties Add(Type type)
        {
            return Add(type, false, false);
        }

        public SPGENFieldProperties Add(Type type, bool addToDefaultView, bool addToAllContentTypes)
        {
            var typeInstance = SPGENElementManager.GetInstance(type) as SPGENFieldBase;
            var field = typeInstance.InstanceDefinition;

            field.AddToAllContentTypes = addToAllContentTypes;
            field.AddToDefaultView = addToDefaultView;

            this.Add(field);
            this.AddElementInstance(field.ID, typeInstance);

            return this[field];
        }


        public bool Contains(string fieldId)
        {
            return this.Contains(new Guid(fieldId));
        }

        public bool Contains(Guid fieldId)
        {
            return this.Contains(new SPGENFieldProperties<SPField>() { ID = fieldId });
        }

        public bool Remove(string fieldId)
        {
            return this.Remove(new Guid(fieldId));
        }

        public bool Remove(Guid fieldId)
        {
            return this.Remove(new SPGENFieldProperties<SPField>() { ID = fieldId });
        }

        public void Provision(SPFieldCollection fieldCollection)
        {
            var itemsToProvisionList = this.GetAllAddedAndUpdatedItems();

            foreach (var field in itemsToProvisionList)
            {
                try
                {
                    if (this.CanUpdate && this.ElementInstanceExists(field.ID))
                    {
                        var instance = GetElementInstance<SPGENFieldBase>(field.ID);

                        instance.Provision(fieldCollection.List.Fields, true, false);
                    }
                    else
                    {
                        bool updatedOnly;

                        SPGENCommon.CreateOrUpdateField(fieldCollection, field, null, null, true, false, out updatedOnly);
                    }
                }
                catch (Exception ex)
                {
                    string url = (fieldCollection.List != null) ? fieldCollection.List.RootFolder.ServerRelativeUrl : fieldCollection.Web.Url;
                    throw new SPGENGeneralException("The field '" + field.InternalName + " with id ' '" + field.ID + "' failed to provision on " + url + ". " + ex.Message, ex);
                }
            }

            if (!this.CanUpdate)
                return;

            if (this.IsExclusiveAdd)
            {
                var removeFields = new List<Guid>();

                foreach (SPField f in fieldCollection)
                {
                    int i = this.IndexOf(f.Id);

                    if (i == -1 &&
                        !f.FromBaseType &&
                        !itemsToProvisionList.Exists(ff => ff.ID == f.Id))
                    {
                        if (fieldCollection.List != null && IsFromContentType(f.Id, fieldCollection.List))
                            continue;

                        removeFields.Add(f.Id);
                    }
                }

                foreach (var g in removeFields)
                {
                    try
                    {
                        fieldCollection[g].Delete();
                    }
                    catch { }
                }
            }
            else
            {
                var removedItems = this.GetAllRemovedItems();

                foreach (var f in removedItems)
                {
                    if (fieldCollection.Contains(f.ID))
                        fieldCollection[f.ID].Delete();
                }
            }

            if (this.AutoPushChangesToListContentTypes)
            {
                try
                {
                    PushChangesToListContentTypes(fieldCollection, this.GetAllUpdatedItems(), false);
                }
                catch (Exception ex)
                {
                    string url = (fieldCollection.List != null) ? fieldCollection.List.RootFolder.Url : fieldCollection.Web.Url;
                    throw new SPGENGeneralException("Failed to auto push changes to list content types on " + url + ". " + ex.Message, ex);
                }
            }
        }

        public void PushChangesToListContentTypes(SPFieldCollection fieldCollection, List<SPGENFieldProperties> provisionedFields, bool throwOnSealdOrReadOnly)
        {
            if (fieldCollection.List == null)
                throw new SPGENGeneralException("Push changes to list content types failed. The field collection does not belong to a list.");

            var ctCollection = fieldCollection.List.ContentTypes.OfType<SPContentType>().ToList<SPContentType>();

            foreach (var field in provisionedFields)
            {
                if (!fieldCollection.Contains(field.ID))
                    continue;

                for (int n = 0; n < ctCollection.Count; n++)
                {
                    SPFieldLink fl = ctCollection[n].FieldLinks[field.ID];
                    if (fl == null)
                        continue;

                    if (field.IsPropertyValueUpdated("AggregationFunction"))
                        fl.AggregationFunction = field.AggregationFunction;

                    if (field.IsPropertyValueUpdated("DisplayName") && field.DisplayName != null)
                        fl.DisplayName = field.DisplayName;

                    if (field.IsPropertyValueUpdated("Hidden"))
                        fl.Hidden = field.Hidden;

                    if (field.IsPropertyValueUpdated("ReadOnlyField"))
                        fl.ReadOnly = field.ReadOnlyField;

                    if (field.IsPropertyValueUpdated("Required"))
                        fl.Required = field.Required;

                    if (field.IsPropertyValueUpdated("ShowInDisplayForm"))
                        fl.ShowInDisplayForm = (bool)field.ShowInDisplayForm;

                    ctCollection[n].Update(false, throwOnSealdOrReadOnly);
                }

            }

        }

        private bool IsFromContentType(Guid fieldId, SPList list)
        {
            foreach (SPContentType ct in list.ContentTypes)
            {
                var webCt = list.ParentWeb.AvailableContentTypes[ct.Parent.Id];
                if (webCt.FieldLinks[fieldId] != null)
                    return true;
            }

            return false;
        }

    }
}
