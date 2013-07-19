using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Repository;
using System.Reflection;
using System.Data;

namespace SPGenesis.Entities
{
    public sealed class SPGENEntityOperationContext<TEntity> where TEntity : class
    {
        private Dictionary<string, SPField> _fields = new Dictionary<string, SPField>();

        [Obsolete("Not longer in use. Use FieldName instead.", true)]
        public Guid FieldId { get; internal set; }

        public string FieldName { get; internal set; }
        public SPGENEntityOperationParameters Parameters { get; internal set; }
        public SPSiteDataQuery SiteDataQuery { get; internal set; }
        public SPQuery ListQuery { get; internal set; }
        public bool CancelOperation { get; set; }
        public bool CancelItemUpdate { get; set; }
        public SPGENEntityManagerFoundationBase<TEntity> ManagerInstance { get; private set; }

        internal SPGENRepositoryDataItem DataItem { get; set; }

        internal SPGENEntityOperationContext(SPGENEntityManagerFoundationBase<TEntity> managerInstance)
        {
            this.ManagerInstance = managerInstance;
        }

        internal void CreateAndPopulateEntity()
        {
            EnsureDataItemLoaded();

            this.Entity = this.EntityMap.CreateEntityInstance(this);

            this.EntityMap.PopulateEntity(this);
            this.EntityMap.PopulateEntityWithFiles(this);
        }

        internal void PopulateEntity()
        {
            EnsureDataItemLoaded();

            this.EntityMap.PopulateEntity(this);
            this.EntityMap.PopulateEntityWithFiles(this);
        }

        internal void PopulateRepositoryDataItem()
        {
            EnsureDataItemLoaded();

            this.EntityMap.PopulateRepositoryDataItem(this);
            this.EntityMap.PopulateRepositoryDataItemWithFiles(this);
        }

        private void EnsureDataItemLoaded()
        {
            if (this.DataItem == null)
                throw new SPGENEntityGeneralException("No data item was loaded.");
        }

        public DataRow DataRow
        {
            get { return this.DataItem.DataRow; }
        }

        private TEntity _entity;
        public TEntity Entity 
        {
            get
            {
                return _entity;
            }
            internal set
            {
                _entity = value;
            }
        }

        public SPListItem ListItem
        {
            get 
            {
                EnsureDataItemLoaded();

                return this.DataItem.ListItem; 
            }
        }

        private SPList _list;
        public SPList List
        {
            get { return _list; }
            internal set { _list = value; }
        }

        internal SPField GetCurrentField()
        {
            if (_fields.ContainsKey(this.FieldName))
                return _fields[this.FieldName];

            SPFieldCollection coll;
            if (this.List != null)
            {
                coll = this.List.Fields;
            }
            else
            {
                coll = this.Web.AvailableFields;
            }

            if (coll.ContainsField(this.FieldName))
            {
                _fields.Add(this.FieldName, coll.GetFieldByInternalName(this.FieldName));

                return _fields[this.FieldName];
            }
            else
            {
                _fields.Add(this.FieldName, null);

                return null;
            }
        }


        private SPListItemCollection _listItemCollection;
        public SPListItemCollection ListItemCollection
        {
            get
            {
                return _listItemCollection;
            }
            internal set
            {
                _listItemCollection = value;
            }
        }

        public SPItemEventProperties EventProperties { get; internal set; }
        public SPGENItemEventPropertiesType? EventPropertiesCollectionType { get; internal set; }

        private SPWeb _web;
        public SPWeb Web
        {
            get
            {
                if (this.List != null)
                    return this.List.ParentWeb;

                return _web;
            }
            internal set
            {
                _web = value;
            }
        }

        private SPGENEntityMap<TEntity> _entityMap;
        internal SPGENEntityMap<TEntity> EntityMap 
        { 
            get 
            { 
                if (_entityMap == null)
                    _entityMap = this.ManagerInstance.GetMapperInstance();

                return _entityMap;
            } 
        }

        internal bool ShouldIncludeFiles
        {
            get
            {
                if (this.EntityMap.FileMappingMode == SPGENEntityFileMappingMode.None)
                    return false;

                if (this.Parameters != null)
                {
                    if (this.Parameters.IncludeFiles.HasValue)
                        return this.Parameters.IncludeFiles.Value;
                }

                if (this.EntityMap.FileInclusionMode == SPGENEntityFileInclusionMode.OnAllOperations)
                    return true;
                else
                    return false;
            }
        }

        internal bool ShouldIncludeAllFields
        {
            get { return (this.Parameters != null && this.Parameters.IncludeAllFields); }
        }

        internal bool UseEntityState
        {
            get
            {
                if (this.Parameters != null && this.Parameters.UpdatableEntities.HasValue)
                    return this.Parameters.UpdatableEntities.Value;

                return false;
            }
        }

        private string[] _requiredFieldsForRead;
        internal string[] GetRequiredFieldsForRead()
        {
            if (_requiredFieldsForRead != null)
                return _requiredFieldsForRead;

            if (this.Parameters == null)
            {
                _requiredFieldsForRead = this.EntityMap.GetRequiredFieldsForRead();
                return _requiredFieldsForRead;
            }

            var ret = new List<string>();

            foreach (var f in this.EntityMap.GetRequiredFieldsForRead())
            {
                if (!this.Parameters.IsFieldExcludedForRead(f))
                    ret.Add(f);
            }

            _requiredFieldsForRead = ret.ToArray();

            return _requiredFieldsForRead;
        }

        private string[] _requiredFieldsForWrite;
        internal string[] GetRequiredFieldsForWrite()
        {
            if (_requiredFieldsForWrite != null)
                return _requiredFieldsForWrite;

            if (this.Parameters == null)
            {
                _requiredFieldsForWrite = this.EntityMap.GetRequiredFieldsForWrite();
                return _requiredFieldsForWrite;
            }

            var ret = new List<string>();

            foreach (var f in this.EntityMap.GetRequiredFieldsForWrite())
            {
                if (!this.Parameters.IsFieldExcludedForWrite(f))
                    ret.Add(f);
            }

            _requiredFieldsForWrite = ret.ToArray();

            return _requiredFieldsForWrite;
        }
    }


    [Obsolete("Not longer used. Use SPGENEntityOperationContext<TEntity> instead.", true)]
    public sealed class SPGENEntityUpdateOperationContext<TEntity>
        where TEntity : class
    {
    }
}
