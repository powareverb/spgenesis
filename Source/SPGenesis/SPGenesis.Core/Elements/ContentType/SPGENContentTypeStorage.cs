using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENContentTypeStorage : ISPGENContentTypeStorage
    {
        public static ISPGENContentTypeStorage Instance = new SPGENContentTypeStorage();

        protected SPGENContentTypeStorage()
        {
        }

        public virtual SPContentType GetContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, bool isCollectionFromList)
        {
            if (isCollectionFromList)
            {
                return contentTypeCollection.OfType<SPContentType>().FirstOrDefault<SPContentType>(c => c.Id == contentTypeId || c.Parent.Id == contentTypeId);
            }
            else
            {
                return contentTypeCollection[contentTypeId];
            }
        }

        public virtual void EnsureCollectionIsUpdateble(SPWeb web, SPContentTypeId contentTypeId)
        {
            if (web.ContentTypes[contentTypeId] == null)
            {
                if (web.AvailableContentTypes[contentTypeId] != null)
                {
                    throw new SPGENGeneralException("The content type '" + contentTypeId.ToString() + "' doesn't exist in the updateble content type collection on this web but is a member of an ancestor web.");
                }
                else
                {
                    throw new SPGENGeneralException("The content type '" + contentTypeId.ToString() + "' doesn't exist in the updateble content type collection on this web.");
                }
            }
        }

        public virtual SPGENContentTypeUrlInstance CreateUrlInstance(string url)
        {
            var instance = new SPGENContentTypeUrlInstance();

            try
            {
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();

                try
                {
                    instance.List = instance.Web.GetList(url);
                }
                catch { }

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        public virtual SPContentType CreateNewContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, string contentTypeName)
        {
            var result = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);

            return result;
        }

        public virtual SPContentType AddContentTypeToCollection(SPContentTypeCollection contentTypeCollection, SPContentType contentType)
        {
            var result = contentTypeCollection.Add(contentType);

            return result;
        }

        public virtual void UpdateContentType(SPContentType contentType, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate)
        {
            contentType.Update(updateChildren, stopOnSealdOrReadOnlyUpdate);
        }

        public virtual void DeleteContentType(SPContentType contentType)
        {
            contentType.Delete();
        }

    }
}
