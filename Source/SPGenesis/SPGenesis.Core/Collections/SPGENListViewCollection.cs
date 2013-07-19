using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Collections.Specialized;

namespace SPGenesis.Core
{
    public sealed class SPGENListViewCollection : SPGENElementCollectionBase<SPGENViewProperties, string>
    {
        protected override bool IsIdentifierEqual(string id1, string id2)
        {
            return id1.Equals(id2, StringComparison.InvariantCultureIgnoreCase);
        }
        protected override bool IsItemEqual(SPGENViewProperties item1, SPGENViewProperties item2)
        {
            return item1.UrlFileName.Equals(item2.UrlFileName, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override string GetIdentifier(SPGENViewProperties item)
        {
            return item.UrlFileName;
        }

        public int IndexOf(string urlFileName)
        {
            return this.IndexOf(new SPGENViewProperties() { UrlFileName = urlFileName });
        }

        public SPGENViewProperties Add<TListViewElement>() where TListViewElement : SPGENViewBase, new()
        {
            return Add(typeof(TListViewElement));
        }

        public SPGENViewProperties Add(Type type)
        {
            var typeInstance = SPGENElementManager.GetInstance(type) as SPGENViewBase; 
            var view = typeInstance.InstanceDefinition;

            this.Add(view, true);
            this.AddElementInstance(view.UrlFileName.ToLower(), typeInstance);

            return view;
        }

        public bool Contains(string urlFileName)
        {
            return this.Contains(new SPGENViewProperties() { UrlFileName = urlFileName });
        }

        public bool Remove(string urlFileName)
        {
            return this.Remove(new SPGENViewProperties() { UrlFileName = urlFileName });
        }

        public void Provision(SPViewCollection viewCollection, bool preserveViewFieldsCollection, IList<string> fieldsAddedToDefaultView)
        {
            var itemsToProvisionList = this.GetAllAddedAndUpdatedItems();

            foreach (var view in itemsToProvisionList)
            {
                if (this.CanUpdate && this.ElementInstanceExists(view.UrlFileName.ToLower()))
                {
                    var instance = GetElementInstance<SPGENViewBase>(view.UrlFileName.ToLower());

                    instance.Provision(viewCollection.List.Views, preserveViewFieldsCollection);
                }
                else
                {
                    bool updatedOnly;
                    SPGENCommon.CreateOrUpdateView(viewCollection, view, preserveViewFieldsCollection, fieldsAddedToDefaultView, out updatedOnly);
                }
            }

            if (!this.CanUpdate)
                return;

            var typedViewCollection = viewCollection.OfType<SPView>().ToList<SPView>();

            if (this.IsExclusiveAdd)
            {
                foreach (SPView v in typedViewCollection)
                {
                    if (!itemsToProvisionList.Exists(vv => vv.UrlFileName.Equals(v.Url.Substring(v.Url.LastIndexOf("/") + 1), StringComparison.InvariantCultureIgnoreCase)))
                        viewCollection.Delete(v.ID);
                }
            }
            else
            {
                //Remove views
                var removedItems = this.GetAllRemovedItems();

                foreach (var item in removedItems)
                {
                    SPView viewToRemove = typedViewCollection.FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + item.UrlFileName, StringComparison.InvariantCultureIgnoreCase));
                    if (viewToRemove != null && viewToRemove.BaseViewID != "0")
                    {
                        viewCollection.Delete(viewToRemove.ID);
                    }
                }
            }
        }
    }
}
