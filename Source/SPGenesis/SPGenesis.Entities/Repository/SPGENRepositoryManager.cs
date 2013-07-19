using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Data;

namespace SPGenesis.Entities.Repository
{
    public class SPGENRepositoryManager : ISPGENRepositoryManager
    {
        public static ISPGENRepositoryManager Instance = new SPGENRepositoryManager();

        protected SPGENRepositoryManager()
        {
        }

        public virtual void ConvertToDataItem(SPListItem listItem, SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams)
        {
            dataItem.ListItem = listItem;

            foreach (string fieldName in dataItem.FieldNames)
            {
                dataItem.FieldValues[fieldName] = listItem[fieldName];
            }

            EnsureFiles(listItem, dataItem, fileOperationParams);
        }

        private void EnsureFiles(SPListItem listItem, SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams)
        {
            if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.None)
            {
                dataItem.HasFiles = false;
                return;
            }

            dataItem.HasFiles = true;

            if (listItem.ParentList.BaseType == SPBaseType.DocumentLibrary)
            {
                SPFile file = listItem.File;

                if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray ||
                    fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
                {
                    CheckByteArrayFileSize(dataItem, file.Length, fileOperationParams.MaxFileSizeByteArrays);
                }

                dataItem.File = new SPGENRepositoryDataItemFile(file, fileOperationParams);
            }
            else
            {
                dataItem.Attachments = new List<SPGENRepositoryDataItemFile>(); 

                foreach (string fileName in listItem.Attachments)
                {
                    if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameOnly)
                    {
                        dataItem.Attachments.Add(new SPGENRepositoryDataItemFile(fileName));
                    }
                    else
                    {
                        SPFile file = listItem.Web.GetFile(listItem.Attachments.UrlPrefix + fileName);
                        if (!file.Exists)
                            throw new SPGENEntityGeneralException("The file can not be found.");

                        if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray ||
                            fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
                        {
                            CheckByteArrayFileSize(dataItem, file.Length, fileOperationParams.MaxFileSizeByteArrays);
                        }

                        dataItem.Attachments.Add(new SPGENRepositoryDataItemFile(file, fileOperationParams));
                    }
                }
            }
        }

        public virtual void ConvertToDataItem(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENRepositoryDataItem dataItem, bool useListItemValueIfNotFound, out IDictionary<string, object> valuesFromEventProperties)
        {
            valuesFromEventProperties = new Dictionary<string, object>();
            SPItemEventDataCollection collection = collectionType == SPGENItemEventPropertiesType.AfterProperties ? eventProperties.AfterProperties : eventProperties.BeforeProperties;
            dataItem.ListItem = eventProperties.ListItem;

            string[] fieldNamesInProperties = null;
            if (useListItemValueIfNotFound)
            {
                fieldNamesInProperties = collection.OfType<DictionaryEntry>().Select(de => de.Key as string).ToArray();
            }

            foreach (string fieldName in dataItem.FieldNames)
            {
                object value = null;

                if (useListItemValueIfNotFound)
                {
                    if (fieldNamesInProperties.Contains(fieldName, StringComparer.Ordinal))
                    {
                        value = collection[fieldName];
                        valuesFromEventProperties.Add(fieldName, value);
                    }
                    else
                    {
                        value = eventProperties.ListItem[fieldName];
                    }
                }
                else
                {
                    value = collection[fieldName];
                    valuesFromEventProperties.Add(fieldName, value);
                }

                dataItem.FieldValues[fieldName] = value;
            }
        }

        public virtual void ConvertToDataItem(DataRow row, SPGENRepositoryDataItem dataItem)
        {
            dataItem.DataRow = row;

            foreach (string fieldName in dataItem.FieldNames)
            {
                dataItem.FieldValues[fieldName] = row[fieldName];
            }
        }

        public virtual SPGENRepositoryDataItem GetDataItem(SPListItemCollection itemCollection, int itemId, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams)
        {
            var dataItem = new SPGENRepositoryDataItem(fieldNames);

            ConvertToDataItem(itemCollection.GetItemById(itemId), dataItem, fileOperationParams);

            return dataItem;
        }

        public virtual SPGENRepositoryDataItem GetDataItem(SPList list, int itemId, string[] fieldNames, bool includeAllItemFields, SPGENEntityFileOperationArguments fileOperationParams)
        {
            SPListItem listItem = (includeAllItemFields) ? list.GetItemById(itemId) : list.GetItemByIdSelectedFields(itemId, fieldNames);

            var dataItem = new SPGENRepositoryDataItem(fieldNames);
            ConvertToDataItem(listItem, dataItem, fileOperationParams);

            return dataItem;
        }

        public virtual SPGENRepositoryDataItemCollection GetDataItems(SPListItemCollection listItemCollection, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams)
        {
            return new SPGENRepositoryDataItemCollection(listItemCollection, fieldNames, fileOperationParams);
        }

        public virtual SPGENRepositoryDataItemCollection FindDataItems(SPList list, SPQuery query, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams)
        {
            var itemCollection = list.GetItems(query);
            var result = new SPGENRepositoryDataItemCollection(itemCollection, fieldNames, fileOperationParams);

            return result;
        }

        public virtual SPGENRepositoryDataItemCollection FindDataItems(SPWeb web, SPSiteDataQuery query, string[] fieldNames)
        {
            DataTable dataTable = null;
            try
            {
                dataTable = web.GetSiteData(query);

                return new SPGENRepositoryDataItemCollection(dataTable, fieldNames);
            }
            catch
            {
                if (dataTable != null)
                    dataTable.Dispose();

                throw;
            }
        }

        public virtual void UpdateEventProperties(SPGENRepositoryDataItem item, SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType)
        {
            SPItemEventDataCollection collection = collectionType == SPGENItemEventPropertiesType.AfterProperties ? eventProperties.AfterProperties : eventProperties.BeforeProperties;

            foreach (string fieldName in item.FieldNames)
            {
                if (collection[fieldName] == null)
                {
                    collection.ChangedProperties.Add(fieldName, item.FieldValues[fieldName]);
                }
                else
                {
                    collection[fieldName] = item.FieldValues[fieldName];
                }
            }
        }

        public virtual void UpdateListItem(SPGENRepositoryDataItem dataItem, SPGENEntityUpdateMethod updateMethod, SPGENEntityFileOperationArguments fileOperationParams)
        {
            SPListItem listItem = dataItem.ListItem;

            foreach (string fieldName in dataItem.FieldNames)
            {
                try
                {
                    listItem[fieldName] = dataItem.FieldValues[fieldName];
                }
                catch (Exception ex)
                {
                    throw new SPGENEntityGeneralException(string.Format("Failed to set list item value for field '{0}'.", fieldName), ex);
                }
            }

            if (updateMethod != SPGENEntityUpdateMethod.SkipUpdate)
            {
                if (updateMethod != SPGENEntityUpdateMethod.SkipUpdate)
                {
                    if (updateMethod == SPGENEntityUpdateMethod.Normal)
                    {
                        listItem.Update();
                    }
                    else if (updateMethod == SPGENEntityUpdateMethod.SystemUpdate)
                    {
                        listItem.SystemUpdate();
                    }
                    else if (updateMethod == SPGENEntityUpdateMethod.SystemUpdateOverwriteVersion)
                    {
                        listItem.SystemUpdate(false);
                    }
                    else
                    {
                        listItem.UpdateOverwriteVersion();
                    }
                }
            }
        }

        public virtual void SaveAttachments(SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams)
        {
            if (dataItem.Attachments == null)
                return;

            SPListItem listItem = dataItem.ListItem;
            SPAttachmentCollection attColl = listItem.Attachments;
            string[] listItemAttachments = attColl.Cast<string>().ToArray();
            string[] dataItemAttachments = dataItem.GetAllAttachmentFileNames();
            bool refreshListItemOnExit = false;
            bool shouldUpdateListItem = true;

            foreach(string fileName in listItemAttachments)
            {
                if (!dataItemAttachments.Contains(fileName, StringComparer.InvariantCultureIgnoreCase))
                {
                    attColl.Delete(fileName);
                }
            }

            foreach (string fileName in dataItemAttachments)
            {
                if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
                {
                    byte[] b = dataItem.GetSpecificAttachment(fileName).GetByteArray(fileOperationParams.ForceFileSave);
                    if (b == null)
                        continue;

                    CheckByteArrayFileSize(dataItem, b.Length, fileOperationParams.MaxFileSizeByteArrays);

                    if (listItemAttachments.Contains(fileName, StringComparer.InvariantCultureIgnoreCase))
                        attColl.Delete(fileName);

                    attColl.Add(fileName, b);
                }
                else if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
                {
                    using (var s = dataItem.GetSpecificAttachment(fileName).GetStream(fileOperationParams.ForceFileSave))
                    {
                        if (s == null)
                            continue;

                        //SPItemAttachmentCollection doesn't support adding new files with streams (only byte arrays).
                        //Workaround: If the attachment is new, add a dummy file containg 1 byte first and save the stream through the SPFile object after.

                        if (shouldUpdateListItem)
                        {
                            listItem.SystemUpdate(false);
                            shouldUpdateListItem = false;
                            refreshListItemOnExit = true;
                        }

                        try
                        {
                            attColl.AddNow(fileName, new byte[1]);
                        }
                        catch
                        {
                            try
                            {
                                attColl.DeleteNow(fileName);
                            }
                            catch { }

                            throw;
                        }

                        SPFile f = listItem.Web.GetFile(attColl.UrlPrefix + fileName);
                        if (!f.Exists)
                            throw new SPGENEntityGeneralException(string.Format("The file {0} does not exist.", f.ServerRelativeUrl));

                        f.SaveBinary(s);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            if (refreshListItemOnExit)
            {
                dataItem.ListItem = listItem.ParentList.GetItemById(listItem.ID);
            }
        }

        public virtual void SaveFile(SPGENRepositoryDataItem dataItem, SPGENEntityFileOperationArguments fileOperationParams)
        {
            if (dataItem.File == null)
                return;

            SPFile file = dataItem.ListItem.File;

            if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                byte[] b = dataItem.File.GetByteArray(fileOperationParams.ForceFileSave);

                CheckByteArrayFileSize(dataItem, b.Length, fileOperationParams.MaxFileSizeByteArrays);

                if (fileOperationParams.SaveFileParameters != null)
                {
                    file.SaveBinary(b, fileOperationParams.SaveFileParameters.CheckRequiredFields);
                }
                else
                {
                    file.SaveBinary(b);
                }
            }
            else if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                using (var s = dataItem.File.GetStream(fileOperationParams.ForceFileSave))
                {
                    file.SaveBinary(s, parameters: fileOperationParams.SaveFileParameters);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public virtual void CreateNewFolder(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, string folderName, SPGENEntityFileOperationArguments fileOperationParams)
        {
            SPFolder parentFolder = string.IsNullOrEmpty(parentFolderRelUrl) ? list.RootFolder : list.ParentWeb.GetFolder(GetSiteRelativeFolderUrl(list, parentFolderRelUrl));
            if (!parentFolder.Exists)
                throw new FileNotFoundException(string.Format("The folder {0} does not exist.", parentFolder.ServerRelativeUrl));

            dataItem.ListItem = list.Folders.Add(parentFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, folderName);
        }

        public virtual void CreateNewListItem(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, SPGENEntityFileOperationArguments fileOperationParams)
        {
            SPListItem listItem;

            if (string.IsNullOrEmpty(parentFolderRelUrl))
            {
                listItem = list.AddItem();
            }
            else
            {
                listItem = list.AddItem(GetSiteRelativeFolderUrl(list, parentFolderRelUrl), SPFileSystemObjectType.Invalid);
            }

            dataItem.ListItem = listItem;
        }

        public virtual void CreateNewFile(SPList list, SPGENRepositoryDataItem dataItem, string parentFolderRelUrl, string fileName, SPGENEntityFileOperationArguments fileOperationParams)
        {
            SPFolder parentFolder;
            SPFile file;

            if (dataItem.File == null)
                throw new SPGENEntityGeneralException("There is no file content to save.");

            if (!string.IsNullOrEmpty(parentFolderRelUrl))
            {
                parentFolder = list.ParentWeb.GetFolder(GetSiteRelativeFolderUrl(list, parentFolderRelUrl));
                if (!parentFolder.Exists)
                    throw new FileNotFoundException(string.Format("The folder {0} does not exist.", parentFolder.ServerRelativeUrl));
            }
            else
            {
                parentFolder = list.RootFolder;
            }

            if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                byte[] b = dataItem.File.GetByteArray(true);

                CheckByteArrayFileSize(dataItem, b.Length, fileOperationParams.MaxFileSizeByteArrays);

                if (fileOperationParams.SaveFileParameters != null)
                {
                    file = parentFolder.Files.Add(fileName, b, fileOperationParams.SaveNewFileParameters);
                }
                else
                {
                    file = parentFolder.Files.Add(fileName, b);
                }
            }
            else if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                using (var s = dataItem.File.GetStream(true))
                {
                    if (fileOperationParams.SaveFileParameters != null)
                    {
                        file = parentFolder.Files.Add(fileName, s, fileOperationParams.SaveNewFileParameters);
                    }
                    else
                    {
                        file = parentFolder.Files.Add(fileName, s);
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            dataItem.ListItem = file.Item;
        }

        public virtual void DeleteListItem(SPList list, int itemId)
        {
            var item = list.GetItemById(itemId);

            item.Delete();
        }

        public virtual void DeleteListItem(SPGENRepositoryDataItem dataItem)
        {
            dataItem.ListItem.Delete();
        }

        private string GetSiteRelativeFolderUrl(SPList list, string url)
        {
            return list.RootFolder.ServerRelativeUrl + "/" + url;
        }

        private void CheckByteArrayFileSize(SPGENRepositoryDataItem dataItem, long actualSize, long limit)
        {
            if (actualSize > limit)
                throw new SPGENEntityMaxFileSizeExceededException(limit);
        }

        #region Obsolete

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual void ConvertToDataItem(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENRepositoryDataItem dataItem)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItemCollection GetListItems(SPListItemCollection listItemCollection, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItem GetDataItem(SPListItemCollection itemCollection, int itemId, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItem GetDataItem(SPList list, int itemId, string[] fieldNames, bool includeAllItemFields, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual void ConvertToDataItem(SPListItem listItem, SPGENRepositoryDataItem dataItem, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItem CreateNewDataItem(SPList list, string folderUrl, SPFileSystemObjectType fsoType, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItem CreateNewDataItem(SPList list, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual void SaveFile(SPGENRepositoryDataItem item, SPFileSaveBinaryParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual void SaveAttachments(SPGENRepositoryDataItem item, SPGENEntityAttachmentsUpdateMethod updateMethod)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual SPGENRepositoryDataItemCollection FindDataItems(SPList list, SPQuery query, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is obsolete and not supported any more.", true)]
        public virtual void UpdateDataItem(SPGENRepositoryDataItem dataItem, SPGENEntityUpdateMethod updateMethod, string[] fieldNames, bool includeFiles)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
