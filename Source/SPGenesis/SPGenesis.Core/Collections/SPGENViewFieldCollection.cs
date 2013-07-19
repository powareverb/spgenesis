using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public sealed class SPGENViewFieldCollection : SPGENElementCollectionBase<string, string>
    {
        protected override string GetIdentifier(string item)
        {
            return item;
        }

        public void Add(Guid fieldId)
        {
            this.Add(fieldId.ToString("B"), true);
        }

        public void Add<TField>() where TField : SPGENFieldBase, new()
        {
            Add(typeof(TField));
        }

        public void Add(Type type)
        {
            var typeInstance = SPGENElementManager.GetInstance(type) as SPGENFieldBase;
            var field = typeInstance.StaticDefinition;

            this.Add(field.InternalName, true);
        }

        public void Remove(Guid fieldId)
        {
            this.Remove(fieldId.ToString("B"));
        }

        public void Provision(SPViewFieldCollection viewFieldCollection, IList<string> fieldsAddedToDefaultView)
        {
            var itemsToProvisionList = this.GetAllAddedAndUpdatedItems();

            if (this.IsExclusiveAdd)
            {
                viewFieldCollection.DeleteAll();
            }

            foreach (var field in itemsToProvisionList)
            {
                string internalNameToAdd = field;

                if (field.StartsWith("{"))
                {
                    internalNameToAdd = viewFieldCollection.View.ParentList.Fields[new Guid(field)].InternalName;
                }
                else if (field.IndexOf(":") != -1)
                {
                    string[] arr = internalNameToAdd.Split(':');
                    string bdcFieldName = arr[0].Trim();
                    string entityFieldName = arr[1].Trim();

                    SPField fieldInstance = SPGENCommon.GetSecondaryBdcField(viewFieldCollection.View.ParentList, bdcFieldName, entityFieldName);

                    internalNameToAdd = fieldInstance.InternalName;
                }

                if (!viewFieldCollection.Exists(internalNameToAdd))
                    viewFieldCollection.Add(internalNameToAdd);
            }

            if (this.ProvisioningMode == SPGENProvisioningMode.AppendOrUpdate || this.ProvisioningMode == SPGENProvisioningMode.AddUpdateRemove)
            {
                var removedItems = this.GetAllRemovedItems();

                foreach (var f in removedItems)
                {
                    string internalNameToRemove = f;
                    if (internalNameToRemove.StartsWith("{"))
                        internalNameToRemove = viewFieldCollection.View.ParentList.Fields[new Guid(internalNameToRemove)].InternalName;

                    if (fieldsAddedToDefaultView != null && viewFieldCollection.View.DefaultView && fieldsAddedToDefaultView.Contains<string>(internalNameToRemove))
                        continue;

                    if (viewFieldCollection.Exists(internalNameToRemove))
                        viewFieldCollection.Delete(internalNameToRemove);
                }
            }
        }

    }
}
