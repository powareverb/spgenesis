using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Adapters;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Repository;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities
{
    /// <summary>
    /// Base class for entity mappers.
    /// </summary>
    /// <typeparam name="TEntity">The entity to map.</typeparam>
    public abstract partial class SPGENEntityMap<TEntity> : SPGENEntityMapBase<TEntity>
        where TEntity : class
    {
        private static string _identifierFieldType = string.Empty;
        private static bool _identifierIsItemId;
        private static bool _identifierSkipIndexCheck;
        private static bool _identifierSkipEnforceUniqueValueCheck;

        private static SPGENEntityFileMappingMode _fileMappingType;
        private static SPGENEntityFileInclusionMode _fileInclusionMode;
        private static SPGENEntityPropertyAccessor<TEntity> _filePropertyAccessor;
        private static SPGENEntityPropertyAccessor<TEntity> _attachmentsPropertyAccessor;

        [Obsolete("This property is obsolete. Use SPGENEntityManager<TEntity>.Instance instead.", true)]
        public static SPGENEntityManagerFoundationBase<TEntity> Manager
        {
            get { throw new NotSupportedException(); }
        }

        [Obsolete("Not longer in use. Use GetRequiredFieldsForRead or GetRequiredFieldsForWrite instead.", true)]
        public virtual string[] GetRequiredFields()
        {
            throw new NotSupportedException();
        }

        private long _maxFileSizeByteArrays = 20971520; //20 MB
        /// <summary>
        /// Max file size to apply on file operations when using byte arrays.
        /// </summary>
        public long MaxFileSizeByteArrays
        {
            get
            {
                return _maxFileSizeByteArrays;
            }
            set
            {
                _maxFileSizeByteArrays = value;
            }
        }


        #region Internal members

        internal bool IdentifierSkipIndexCheck { get { return _identifierSkipIndexCheck; } }

        internal bool IdentifierSkipEnforceUniqueValueCheck { get { return _identifierSkipEnforceUniqueValueCheck; } }

        internal override void SetIdentifierValue(SPGENEntityOperationContext<TEntity> context)
        {
            if (_identifierIsItemId)
            {
                if (context.DataItem.ListItem == null)
                {
                    SetIdentifierValueIntID(context, 0);
                }
                else
                {
                    SetIdentifierValueIntID(context, context.DataItem.ListItemId);
                }
            }
            else
            {
                context.FieldName = base.GetIdentifierFieldName();

                if (context.DataItem.ListItem == null)
                {
                    Type t = base.GetIdentifierValueType();
                    if (t.IsValueType)
                    {
                        base.SetIdentifierValueCustomId(context, Activator.CreateInstance(t));
                    }
                    else
                    {
                        base.SetIdentifierValueCustomId(context, null);
                    }
                }
                else
                {
                    base.SetIdentifierValueCustomId(context, context.DataItem.FieldValues[context.FieldName]);
                }
            }
        }

        internal SPGENEntityFileInclusionMode FileInclusionMode
        {
            get
            {
                return _fileInclusionMode;
            }
        }

        internal SPGENEntityFileMappingMode FileMappingMode 
        { 
            get 
            {
                return _fileMappingType;
            }
        }

        internal bool HasCustomId { get { return !_identifierIsItemId; } }

        internal string GetCustomIdentifierFieldType(SPGENEntityOperationContext<TEntity> context)
        { 
            if (_identifierFieldType == string.Empty)
            {
                lock (_identifierFieldType)
                {
                    if (_identifierFieldType == string.Empty)
                    {
                        if (context.List != null)
                        {
                            _identifierFieldType = context.List.Fields.GetFieldByInternalName(base.GetIdentifierFieldName()).TypeAsString;
                        }
                        else
                        {
                            _identifierFieldType = context.Web.AvailableFields.GetFieldByInternalName(base.GetIdentifierFieldName()).TypeAsString;
                        }
                    }
                }
            }

            return _identifierFieldType;
        }

        internal SPGENEntityItemIdentifierInfoBase GetIdentifierValue(SPGENEntityOperationContext<TEntity> context)
        {
            if (!this.HasIdentifierProperty)
                throw new ArgumentNullException("No identifier property is registered for this entity mapper.");

            if (_identifierIsItemId)
            {
                return new SPGENEntityBuiltInItemIdIdentifierInfo() { ItemId = base.GetIdentifierValueIntId(context) };
            }
            else
            {
                return new SPGENEntityCustomItemIdentifierInfo() 
                {
                    CustomId = base.GetIdentifierValue<object>(context),
                    FieldName = base.GetIdentifierFieldName(),
                    FieldType = GetCustomIdentifierFieldType(context)
                };
            }
        }

        internal void PopulateEntityWithFiles(SPGENEntityOperationContext<TEntity> context)
        {
            if (!context.ShouldIncludeFiles)
                return;

            if (_attachmentsPropertyAccessor != null)
            {
                if (!base.ShouldExcludeProperty(null, _attachmentsPropertyAccessor, context.Parameters, false))
                {
                    InvokeSetPropertyAccessor(_attachmentsPropertyAccessor, context, context.ListItem.Attachments);
                }
            }

            if (_filePropertyAccessor != null)
            {
                if (!base.ShouldExcludeProperty(null, _filePropertyAccessor, context.Parameters, false))
                {
                    InvokeSetPropertyAccessor(_filePropertyAccessor, context, context.ListItem.File);
                }
            }
        }

        internal void PopulateRepositoryDataItemWithFiles(SPGENEntityOperationContext<TEntity> context)
        {
            if (!context.ShouldIncludeFiles)
                return;

            if (_attachmentsPropertyAccessor != null)
            {
                if (!ShouldExcludeProperty(null, _attachmentsPropertyAccessor, context.Parameters, true))
                {
                    context.DataItem.Attachments = (IList<SPGENRepositoryDataItemFile>)InvokeGetPropertyAccessor(_attachmentsPropertyAccessor, context);
                }
            }

            if (_filePropertyAccessor != null)
            {
                if (!ShouldExcludeProperty(null, _filePropertyAccessor, context.Parameters, true))
                {
                    //Ensure file name first
                    if (context.DataItem.FieldValues["FileLeafRef"] == null)
                    {
                        var pa = base.FindPropertyAccessor(_filePropertyAccessor.Property);
                        context.DataItem.FieldValues["FileLeafRef"] = InvokeGetPropertyAccessor(pa, context);
                    }

                    context.DataItem.Attachments = (IList<SPGENRepositoryDataItemFile>)InvokeGetPropertyAccessor(_filePropertyAccessor, context);
                }
            }
        }

        internal override void AddPropertyAccessorArguments(IDictionary<Guid, SPGENEntityPropertyAccessorArguments> instances)
        {
            if (_attachmentsPropertyAccessor != null)
            {
                instances.Add(_attachmentsPropertyAccessor.Id, base.CreatePropertyAccessorArguments(_attachmentsPropertyAccessor));
            }

            if (_filePropertyAccessor != null)
            {
                instances.Add(_filePropertyAccessor.Id, base.CreatePropertyAccessorArguments(_filePropertyAccessor));
            }
        }

        #endregion


        #region Private members

        private void EnsureFieldIsNotIdentifier(string fieldName)
        {
            if (this.HasCustomId && this.IdentifierFieldName == fieldName)
            {
                throw new ArgumentException("The field name is already mapped as identifier for this entity.");
            }
        }
        
        #endregion

    }

    [Obsolete("Not longer used.", true)]
    public enum SPGENFieldIDCacheModeEnum
    {
        None = 0,
        PerProcess = 1,
        PerListAndProcess = 2
    }
}
