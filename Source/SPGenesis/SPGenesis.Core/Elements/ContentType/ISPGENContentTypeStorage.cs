using System;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public interface ISPGENContentTypeStorage
    {
        SPContentType AddContentTypeToCollection(SPContentTypeCollection contentTypeCollection, SPContentType contentType);
        SPContentType CreateNewContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, string contentTypeName);
        void DeleteContentType(SPContentType contentType);
        void EnsureCollectionIsUpdateble(SPWeb web, SPContentTypeId contentTypeId);
        SPContentType GetContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, bool isCollectionFromList);
        SPGENContentTypeUrlInstance CreateUrlInstance(string url);
        void UpdateContentType(SPContentType contentType, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate);
    }
}
