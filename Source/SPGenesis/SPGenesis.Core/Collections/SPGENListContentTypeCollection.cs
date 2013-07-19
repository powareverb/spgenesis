using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public sealed class SPGENListContentTypeCollection : SPGENElementCollectionBase<SPGENContentTypeProperties, SPContentTypeId>
    {
        protected override bool IsItemEqual(SPGENContentTypeProperties item1, SPGENContentTypeProperties item2)
        {
            return item1.ID == item2.ID;
        }

        protected override SPContentTypeId GetIdentifier(SPGENContentTypeProperties item)
        {
            return item.ID;
        }

        public int IndexOf(string contentTypeId)
        {
            return this.IndexOf(new SPContentTypeId(contentTypeId));
        }

        public int IndexOf(SPContentTypeId contentTypeId)
        {
            return this.IndexOf(new SPGENContentTypeProperties() { ID = contentTypeId });
        }

        public SPGENContentTypeProperties Add(SPContentTypeId id)
        {
            var ct = new SPGENContentTypeProperties() { ID = id };

            this.Add(ct, true);

            return ct;
        }

        public SPGENContentTypeProperties Add<TContentType>()
            where TContentType : SPGENContentTypeBase, new()
        {
            return Add(typeof(TContentType));
        }

        public SPGENContentTypeProperties Add(Type type)
        {
            var typeInstance = SPGENElementManager.GetInstance(type) as SPGENContentTypeBase;
            var contentType = typeInstance.InstanceDefinition;

            contentType.FieldLinks.ResetUpdatedStatus();
            contentType.FieldLinks.ResetRemovedStatus();

            contentType.ClearUpdatedAttributesStatus();

            this.Add(contentType, true);
            this.AddElementInstance(contentType.ID, typeInstance);

            return contentType;
        }

        public bool Contains(string contentTypeId)
        {
            return this.Contains(new SPContentTypeId(contentTypeId));
        }

        public bool Contains(SPContentTypeId contentTypeId)
        {
            return this.Contains(new SPGENContentTypeProperties() { ID = contentTypeId });
        }

        public bool Remove(string contentTypeId)
        {
            return this.Remove(new SPContentTypeId(contentTypeId));
        }

        public bool Remove(SPContentTypeId contentTypeId)
        {
            return this.Remove(new SPGENContentTypeProperties() { ID = contentTypeId });
        }

        public void Provision(SPList list)
        {
            var contentTypeCollection = list.ContentTypes;
            var typedCollection = contentTypeCollection.OfType<SPContentType>();
            var itemsToProvisionList = this.GetAllAddedAndUpdatedItems();

            foreach (var ctToProvision in itemsToProvisionList)
            {
                try
                {
                    SPContentTypeId ctId = ctToProvision.ID;
                    IList<SPGENFieldBase> listOfFieldElements;

                    if (this.CanUpdate && this.ElementInstanceExists(ctToProvision.ID))
                    {
                        var instance = GetElementInstance<SPGENContentTypeBase>(ctToProvision.ID);

                        instance.Provision(list.ContentTypes, list, true, false, true);
                    }
                    else
                    {
                        var currentCT = typedCollection.FirstOrDefault<SPContentType>(c => c.Id == ctId || c.Parent.Id == ctId);

                        if (currentCT == null)
                        {
                            currentCT = contentTypeCollection.Add(list.ParentWeb.AvailableContentTypes[ctId]);
                            SPGENCommon.CopyAttributeToContentType(ctToProvision, currentCT);

                            ctToProvision.FieldLinks.Provision(ref currentCT, true, out listOfFieldElements);
                        }
                        else if (this.CanUpdate)
                        {
                            SPGENCommon.CopyAttributeToContentType(ctToProvision, currentCT);

                            ctToProvision.FieldLinks.Provision(ref currentCT, true, out listOfFieldElements);
                        }
                        else
                        {
                            continue;
                        }

                        foreach (var g in ctToProvision.FieldLinksToRemove)
                        {
                            if (currentCT.FieldLinks[g] != null)
                            {
                                currentCT.FieldLinks.Delete(g);
                            }
                        }

                        currentCT.Update();
                    }

                    //Refresh content type collection
                    contentTypeCollection = list.ContentTypes;
                }
                catch (Exception ex)
                {
                    string url = list.RootFolder.ServerRelativeUrl;
                    throw new SPGENGeneralException("The content type '" + ctToProvision.ID + "' failed to provision on " + url + ". " + ex.Message, ex);
                }
            }

            if (!this.CanUpdate)
                return;

            if (this.IsExclusiveAdd)
            {
                var removeContentTypes = new List<SPContentTypeId>();

                foreach (SPContentType c in contentTypeCollection)
                {
                    bool exists = itemsToProvisionList.Exists(cc => cc.ID == c.Id || cc.ID == c.Parent.Id);
                    if (exists)
                        continue;

                    removeContentTypes.Add(c.Id);
                }

                foreach (var id in removeContentTypes)
                {
                    contentTypeCollection[id].Delete();
                }
            }
            else
            {
                var removedItems = this.GetAllRemovedItems();

                foreach (var item in removedItems)
                {
                    if (contentTypeCollection[item.ID] != null)
                        contentTypeCollection.Delete(item.ID);
                }
            }
        }
    }
}
