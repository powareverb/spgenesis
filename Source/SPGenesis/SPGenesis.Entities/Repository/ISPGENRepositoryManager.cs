using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.IO;

namespace SPGenesis.Entities.Repository
{
    public interface ISPGENRepositoryManager
    {
        void ConvertToDataItem(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENRepositoryDataItem dataItem, bool useListItemValueIfNotFound, out IDictionary<string, object> valuesFromEventProperties);
        void ConvertToDataItem(SPListItem listItem, SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams);
        void ConvertToDataItem(DataRow row, SPGENRepositoryDataItem dataItem);
        void CreateNewFolder(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, string folderName, SPGENEntityFileOperationArguments fileOperationParams);
        void CreateNewListItem(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, SPGENEntityFileOperationArguments fileOperationParams);
        void CreateNewFile(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, string fileName, SPGENEntityFileOperationArguments fileOperationParams);
        void DeleteListItem(SPList list, int itemId);
        void DeleteListItem(SPGENRepositoryDataItem dataItem);
        SPGENRepositoryDataItemCollection FindDataItems(SPList list, SPQuery query, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams);
        SPGENRepositoryDataItemCollection FindDataItems(SPWeb web, SPSiteDataQuery query, string[] fieldNames);
        SPGENRepositoryDataItem GetDataItem(SPList list, int itemId, string[] fieldNames, bool includeAllItemFields, SPGENEntityFileOperationArguments fileOperationParams);
        SPGENRepositoryDataItem GetDataItem(SPListItemCollection itemCollection, int itemId, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams);
        SPGENRepositoryDataItemCollection GetDataItems(SPListItemCollection listItemCollection, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams);
        void SaveAttachments(SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams);
        void SaveFile(SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams);
        void UpdateListItem(SPGENRepositoryDataItem dataItem, SPGENEntityUpdateMethod updateMethod, SPGENEntityFileOperationArguments fileOperationParams);
        void UpdateEventProperties(SPGENRepositoryDataItem item, SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType);

        [Obsolete("This method is obsolete and not supported any more.", true)]
        void SaveAttachments(SPGENRepositoryDataItem item, SPGenesis.Entities.SPGENEntityAttachmentsUpdateMethod updateMethod);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        void SaveFile(SPGENRepositoryDataItem item, SPFileSaveBinaryParameters parameters);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        void ConvertToDataItem(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENRepositoryDataItem dataItem);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        void ConvertToDataItem(SPListItem listItem, SPGENRepositoryDataItem dataItem, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItem CreateNewDataItem(SPList list, string[] fieldNames, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItem CreateNewDataItem(SPList list, string folderUrl, SPFileSystemObjectType fsoType, string[] fieldNames, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItemCollection FindDataItems(SPList list, SPQuery query, string[] fieldNames, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItem GetDataItem(SPList list, int itemId, string[] fieldNames, bool includeAllItemFields, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItem GetDataItem(SPListItemCollection itemCollection, int itemId, string[] fieldNames, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        SPGENRepositoryDataItemCollection GetListItems(SPListItemCollection listItemCollection, string[] fieldNames, bool includeFiles);
        [Obsolete("This method is obsolete and not supported any more.", true)]
        void UpdateDataItem(SPGENRepositoryDataItem dataItem, SPGenesis.Entities.SPGENEntityUpdateMethod updateMethod, string[] fieldNames, bool includeFiles);
    }
}
