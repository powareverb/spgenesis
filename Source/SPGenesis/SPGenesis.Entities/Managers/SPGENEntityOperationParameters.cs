using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;

namespace SPGenesis.Entities
{
    public class SPGENEntityOperationParameters
    {
        private HashSet<PropertyInfo> _excludedPropertiesOnWrite = new HashSet<PropertyInfo>();
        private HashSet<PropertyInfo> _excludedPropertiesOnRead = new HashSet<PropertyInfo>();

        private HashSet<string> _excludedFieldsOnWrite = new HashSet<string>();
        private HashSet<string> _excludedFieldsOnRead = new HashSet<string>();

        public object CustomProperty { get; set; }
        public SPGENEntityUpdateMethod UpdateMethod { get; set; }
        public bool IncludeAllFields { get; set; }
        public bool? IncludeFiles { get; set; }
        public bool? UpdatableEntities { get; set; }
        public SPFileCollectionAddParameters SaveNewFileParameters { get; set; }
        public SPFileSaveBinaryParameters SaveFileParameters { get; set; }
        public SPOpenBinaryOptions OpenFileParameters { get; set; }
        public SPQuery SPQueryTemplate { get; set; }
        public long? MaxFileSizeByteArrays { get; set; }
        public SPGENEntityPagedCollectionInfo PagingInfo { get; set; }

        [Obsolete("Not longer supported. Use IncludeFiles property instead.", true)]
        public bool? IncludeFilesAndAttachments { get; set; }

        [Obsolete("Not longer supported.", true)]
        public SPGENEntityAttachmentsUpdateMethod AttachmentsUpdateMethod { get; set; }

        [Obsolete("Not longer supported. Use SPQueryTemplate instead.", true)]
        public SPQuery SPQueryObject { get; set; }

        [Obsolete("Not longer supported. Use UpdatableEntities instead.", true)]
        public bool EnableUpdate { get; set; }

        public SPGENEntityOperationParameters()
        {
            this.SPQueryTemplate = new SPQuery();
        }

        public void ExcludePropertyOnReadAndWrite<TEntity>(params Expression<Func<TEntity, object>>[] properties)
        {
            foreach (var p in properties)
            {
                ExcludePropertyOnRead(p);
                ExcludePropertyOnWrite(p);
            }
        }

        public void ExcludePropertyOnWrite<TEntity>(params Expression<Func<TEntity, object>>[] properties)
        {
            foreach (var p in properties)
            {
                var pInfo = GetPropertyInfoFromMember(p);

                if (!_excludedPropertiesOnWrite.Contains(pInfo))
                    _excludedPropertiesOnWrite.Add(pInfo);
            }
        }

        public void ExcludePropertyOnRead<TEntity>(params Expression<Func<TEntity, object>>[] properties)
        {
            foreach (var p in properties)
            {
                var pInfo = GetPropertyInfoFromMember(p);

                if (!_excludedPropertiesOnRead.Contains(pInfo))
                    _excludedPropertiesOnRead.Add(pInfo);
            }
        }

        public void ExcludeFieldOnReadAndWrite(params string[] fieldNames)
        {
            foreach (var f in fieldNames)
            {
                ExcludeFieldOnRead(f);
                ExcludeFieldOnWrite(f);
            }
        }

        public void ExcludeFieldOnRead(params string[] fieldNames)
        {
            foreach (var f in fieldNames)
            {
                if (!_excludedFieldsOnRead.Contains(f))
                    _excludedFieldsOnRead.Add(f);
            }
        }

        public void ExcludeFieldOnWrite(params string[] fieldNames)
        {
            foreach (var f in fieldNames)
            {
                if (!_excludedFieldsOnWrite.Contains(f))
                    _excludedFieldsOnWrite.Add(f);
            }
        }

        
        internal bool IsPropertyExcludedForWrite(PropertyInfo property)
        {
            if (property == null)
                return false;

            return _excludedPropertiesOnWrite.Contains(property);
        }

        internal bool IsPropertyExcludedForRead(PropertyInfo property)
        {
            if (property == null)
                return false;

            return _excludedPropertiesOnRead.Contains(property);
        }

        internal bool IsFieldExcludedForRead(string fieldName)
        {
            return _excludedFieldsOnRead.Contains(fieldName);
        }

        internal bool IsFieldExcludedForWrite(string fieldName)
        {
            return _excludedFieldsOnWrite.Contains(fieldName);
        }

        private PropertyInfo GetPropertyInfoFromMember<TPropertyValue, TEntity>(Expression<Func<TEntity, TPropertyValue>> property)
        {
            MemberInfo member = SPGENCommon.ResolveMemberFromExpression<Func<TEntity, TPropertyValue>>(property);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported for mapping.");

            return member as PropertyInfo;
        }

        /// <summary>
        /// Include files and attachments in operations.
        /// </summary>
        public static SPGENEntityOperationParameters UseIncludeFile { get { return new SPGENEntityOperationParameters() { IncludeFiles = true }; } }

        /// <summary>
        /// Includes all item fields in operations.
        /// </summary>
        public static SPGENEntityOperationParameters UseIncludeAllFields { get { return new SPGENEntityOperationParameters() { IncludeAllFields = true }; } }

        /// <summary>
        /// Enables updatable entities in operations. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        public static SPGENEntityOperationParameters UseUpdatableEntities { get { return new SPGENEntityOperationParameters() { UpdatableEntities = true }; } }

        /// <summary>
        /// Applies specific update method on item update.
        /// </summary>
        /// <param name="updateMethod"></param>
        /// <returns></returns>
        public static SPGENEntityOperationParameters UseItemUpdateParameter(SPGENEntityUpdateMethod updateMethod)
        {
            return new SPGENEntityOperationParameters() { UpdateMethod = updateMethod };
        }

        /// <summary>
        /// Applies paging on operations that returns collections.
        /// </summary>
        /// <param name="pageSize">Amount of entities per page.</param>
        /// <param name="lastItemIdOfLastPage">The last item ID of the last page. Use 0 if it is the first page.</param>
        /// <param name="lastItemIdSetAction">Method to invoke to set the last item ID of the fetched page so it can be used in the call for the next page. When the last page has been reached, the ID will be 0.</param>
        /// <returns></returns>
        public static SPGENEntityOperationParameters UsePaging(uint pageSize, int lastItemIdOfLastPage, Action<int> lastItemIdSetAction)
        {
            return new SPGENEntityOperationParameters()
            {
                PagingInfo = new SPGENEntityPagedCollectionInfo(pageSize, lastItemIdOfLastPage, lastItemIdSetAction) 
            };
        }

        /// <summary>
        /// Applies paging on operations that returns collections.
        /// </summary>
        /// <param name="pageSize">Amount of entities per page.</param>
        /// <param name="lastItemIdOfLastPage">The last item ID of the last page. Use 0 if it is the first page.</param>
        /// <param name="lastItemIdSetAction">Method to invoke to set the last item ID of the fetched page so it can be used in the call for the next page. When the last page has been reached, the ID will be 0.</param>
        /// <param name="useUpdatableEntities">Makes entities updatable.</param>
        /// <returns></returns>
        public static SPGENEntityOperationParameters UsePaging(uint pageSize, int lastItemIdOfLastPage, Action<int> lastItemIdSetAction, bool useUpdatableEntities)
        {
            return new SPGENEntityOperationParameters()
            {
                UpdatableEntities = true,
                PagingInfo = new SPGENEntityPagedCollectionInfo(pageSize, lastItemIdOfLastPage, lastItemIdSetAction)
            };
        }

        /// <summary>
        /// Applies a custom SPQuery object to the operation.
        /// </summary>
        /// <param name="queryTemplate"></param>
        /// <returns></returns>
        public static SPGENEntityOperationParameters UseSPQueryTemplate(SPQuery queryTemplate)
        {
            return new SPGENEntityOperationParameters() { SPQueryTemplate = queryTemplate };
        }

        public static SPGENEntityOperationParameters UseRowLimit(uint rowLimit)
        {
            return new SPGENEntityOperationParameters() { SPQueryTemplate = new SPQuery() { RowLimit = rowLimit } };
        }

        public static SPGENEntityOperationParameters UseOptionsOnSaveFile(SPFileSaveBinaryParameters parameters)
        {
            return new SPGENEntityOperationParameters() { IncludeFiles = true, SaveFileParameters = parameters };
        }

        public static SPGENEntityOperationParameters UseOptionsOnSaveNewFile(SPFileCollectionAddParameters parameters)
        {
            return new SPGENEntityOperationParameters() { IncludeFiles = true, SaveNewFileParameters = parameters };
        }

        public static SPGENEntityOperationParameters UseOptionsOnOpenFile(SPOpenBinaryOptions options)
        {
            return new SPGENEntityOperationParameters() { IncludeFiles = true, OpenFileParameters = options };
        }

    }

    //For backwards compatibility.
    [Obsolete("This class has been replaced by the non-generic version of this class.", false)]
    public sealed class SPGENEntityOperationParameters<TEntity> : SPGENEntityOperationParameters
        where TEntity : class
    {
        public void ExcludePropertyOnReadAndWrite(Expression<Func<TEntity, object>> property)
        {
            base.ExcludePropertyOnReadAndWrite<TEntity>(property);
        }

        public void ExcludePropertyOnWrite(Expression<Func<TEntity, object>> property)
        {
            base.ExcludePropertyOnWrite<TEntity>(property);
        }

        public void ExcludePropertyOnRead(Expression<Func<TEntity, object>> property)
        {
            base.ExcludePropertyOnRead<TEntity>(property);
        }
    }
}
