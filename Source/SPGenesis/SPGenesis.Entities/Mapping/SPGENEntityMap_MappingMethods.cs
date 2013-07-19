using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Adapters;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities
{
    partial class SPGENEntityMap<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Maps list item ID as identifier for the entity.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        protected void RegisterIdentifierProperty(Expression<Func<TEntity, int>> property)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            if (pInfo.PropertyType != typeof(int))
                throw new ArgumentException("Specified identifier property must be an Int32 when using built-in item ID.");

            _identifierIsItemId = true;
            _identifierSkipIndexCheck = true;
            _identifierSkipEnforceUniqueValueCheck = true;

            this.IdentifierFieldName = "ID";
            this.IdentifierPropertyAccessor = SPGENEntityPropertyAccessor<TEntity>.CreateAccessor<object>(pInfo, true);
            this.IdentifierPropertyAccessor.MappedFieldName = "ID";
        }

        /// <summary>
        /// Maps a list field as identifier for the entity.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldName">The internal name for the field</param>
        protected void RegisterIdentifierProperty<TIdentifier>(Expression<Func<TEntity, TIdentifier>> property, string fieldName)
        {
            RegisterIdentifierProperty<TIdentifier>(property, fieldName, false, false);
        }

        /// <summary>
        /// Maps a list field as identifier for the entity.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldName">The internal name for the field</param>
        /// <param name="skipIndexCheck"></param>
        /// <param name="skipEnforceUniqueValueCheck"></param>
        protected void RegisterIdentifierProperty<TIdentifier>(Expression<Func<TEntity, TIdentifier>> property, string fieldName, bool skipIndexCheck, bool skipEnforceUniqueValueCheck)
        {
            var pInfo = GetPropertyInfoFromMember(property);

            _identifierIsItemId = false;
            _identifierSkipIndexCheck = skipIndexCheck;
            _identifierSkipEnforceUniqueValueCheck = skipEnforceUniqueValueCheck;

            this.IdentifierFieldName = fieldName;
            this.IdentifierPropertyAccessor = AddToPropertyAccessorMap<TIdentifier>(pInfo, fieldName);
        }

        /// <summary>
        /// Register a specific field as depentent for this entity map. The field will be included in all entity operations.
        /// </summary>
        /// <param name="fieldName">The internal name of the field.</param>
        protected void RegisterDependentField(string fieldName)
        {
            RegisterDependentField(fieldName, false);
        }

        /// <summary>
        /// Register a specific field as dependent for this entity map. The field will be included in all entity operations.
        /// </summary>
        /// <param name="fieldName">The internal name of the field.</param>
        /// <param name="supportsUpdate"></param>
        protected void RegisterDependentField(string fieldName, bool supportsUpdate)
        {
            if (!base.DepententFields.ContainsKey(fieldName))
                base.DepententFields.Add(fieldName, supportsUpdate);
        }

        /// <summary>
        /// Register the property as not updatable. This property will not cause its mapped field to be updated in update operations.
        /// </summary>
        /// <param name="property"></param>
        protected void RegisterAsNotUpdatableProperty(Expression<Func<TEntity, object>> property)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var accessor = FindPropertyAccessor(pInfo);
            if (accessor == null)
                throw new ArgumentException("The specified property is not registered yet.");

            accessor.SupportsUpdate = false;
        }

        /// <summary>
        /// Register the field as not updatable in update operations.
        /// </summary>
        /// <param name="fieldName">The internal name of the field.</param>
        protected void RegisterAsNotUpdatableField(string fieldName)
        {
            if (base.NotUpdatableFields.Contains(fieldName))
                return;

            base.NotUpdatableFields.Add(fieldName);
        }

        /// <summary>
        /// Maps attachment filenames to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fileInclusionMode">Specifies when to include files. The recommended setting is explicitly per operation because you can control when and where to include file contents.</param>
        protected void MapAttachments(Expression<Func<TEntity, IList<string>>> property, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            PropertyInfo pInfo = GetPropertyInfoFromMember(property);

            MapAttachmentsInternal(pInfo, SPGENEntityFileMappingMode.MapFileNameOnly, fileInclusionMode);
        }

        /// <summary>
        /// Maps attachments to an entity property as a dictionary with the file name as key and the value as a lambda function containing the file content as a byte array.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fileInclusionMode">Specifies when to include files. The recommended setting is explicitly per operation because you can control when and where to include file contents.</param>
        protected void MapAttachments(Expression<Func<TEntity, IDictionary<string, Func<byte[]>>>> property, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            PropertyInfo pInfo = GetPropertyInfoFromMember(property);

            MapAttachmentsInternal(pInfo, SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy, fileInclusionMode);
        }

        /// <summary>
        /// Maps read only attachments to an entity property as a dictionary with the file name as key and a byte array containing the file content as value when invoked.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fileInclusionMode">Specifies when to include files. The recommended setting is explicitly per operation because you can control when and where to include file contents.</param>
        protected void MapAttachmentsReadOnly(Expression<Func<TEntity, IDictionary<string, byte[]>>> property, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            PropertyInfo pInfo = GetPropertyInfoFromMember(property);

            MapAttachmentsInternal(pInfo, SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray, fileInclusionMode);
        }

        /// <summary>
        /// Maps attachments to an entity property as a dictionary with the file name as key and the value as a lambda function containing the file content as a stream.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fileInclusionMode">Specifies when to include files. The recommended setting is explicitly per operation because you can control when and where to include file contents.</param>
        protected void MapAttachments(Expression<Func<TEntity, IDictionary<string, Func<Stream>>>> property, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            PropertyInfo pInfo = GetPropertyInfoFromMember(property);

            MapAttachmentsInternal(pInfo, SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy, fileInclusionMode);
        }

        private void MapAttachmentsInternal(PropertyInfo pInfo, SPGENEntityFileMappingMode mappingMode, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            _fileMappingType = mappingMode;
            _fileInclusionMode = fileInclusionMode;

            RegisterDependentField("Attachments", false);

            var adapter = new Func<SPGENEntityAdapterAttachments<TEntity>>(() => new SPGENEntityAdapterAttachments<TEntity>(mappingMode));

            _attachmentsPropertyAccessor = SPGENEntityPropertyAccessor<TEntity>.CreateAccessor<object, SPGENEntityAdapterAttachments<TEntity>>(pInfo, adapter, false);
            _attachmentsPropertyAccessor.SupportsUpdate = (mappingMode != SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray);
        }

        /// <summary>
        /// Maps file name and content in libraries to an entity property as a dictionary with the file name as key and the value as a lambda function containing the file content as a byte array.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="fileNameProperty">Entity property to map the file name to. Syntax: entity => entity.FileName</param>
        /// <param name="contentProperty">Entity property to map the file content to. Syntax: entity => entity.Content</param>
        /// <param name="fileInclusionMode"></param>
        protected void MapFile(Expression<Func<TEntity, string>> fileNameProperty, Expression<Func<TEntity, Func<byte[]>>> contentProperty, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            MapFileInternal(fileNameProperty, GetPropertyInfoFromMember(contentProperty), SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy, fileInclusionMode);
        }

        /// <summary>
        /// Maps file name and read only content in libraries to an entity property as a dictionary with the file name as key and the value containing the file content as a byte array.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="fileNameProperty">Entity property to map the file name to. Syntax: entity => entity.FileName</param>
        /// <param name="contentProperty">Entity property to map the file content to. Syntax: entity => entity.Content</param>
        /// <param name="fileInclusionMode"></param>
        protected void MapFileReadOnly(Expression<Func<TEntity, string>> fileNameProperty, Expression<Func<TEntity, byte[]>> contentProperty, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            MapFileInternal(fileNameProperty, GetPropertyInfoFromMember(contentProperty), SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray, fileInclusionMode);
        }

        /// <summary>
        /// Maps file name and content in libraries to an entity property as a dictionary with the file name as key and the value as a lambda function containing the file content as a stream.
        /// Use this mapping with CAUTION because it could consume large amount of system resources.
        /// </summary>
        /// <param name="fileNameProperty">Entity property to map the file name to. Syntax: entity => entity.FileName</param>
        /// <param name="contentProperty">Entity property to map the file content to. Syntax: entity => entity.Content</param>
        /// <param name="fileInclusionMode"></param>
        protected void MapFile(Expression<Func<TEntity, string>> fileNameProperty, Expression<Func<TEntity, Func<Stream>>> contentProperty, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            MapFileInternal(fileNameProperty, GetPropertyInfoFromMember(contentProperty), SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy, fileInclusionMode);
        }

        private void MapFileInternal(Expression<Func<TEntity, string>> fileNameProperty, PropertyInfo pInfoContent, SPGENEntityFileMappingMode mappingMode, SPGENEntityFileInclusionMode fileInclusionMode)
        {
            _fileMappingType = mappingMode;
            _fileInclusionMode = fileInclusionMode;

            MapField(fileNameProperty, "FileLeafRef");

            var adapter = new Func<SPGENEntityAdapterFile<TEntity>>(() => new SPGENEntityAdapterFile<TEntity>(mappingMode));

            _filePropertyAccessor = SPGENEntityPropertyAccessor<TEntity>.CreateAccessor<object, SPGENEntityAdapterFile<TEntity>>(pInfoContent, adapter, false);
            _filePropertyAccessor.SupportsUpdate = (mappingMode != SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray);
        }

        /// <summary>
        /// Maps a BCS-field to an entity property.
        /// </summary>
        /// <typeparam name="TBCSIdProperty">The property type for the BCS id.</typeparam>
        /// <typeparam name="TValueProperty">The property type for the BCS value.</typeparam>
        /// <param name="bcsIdProperty">The property to map the ID part of the BCS-field. Syntax: entity => entity.BcsID</param>
        /// <param name="valueProperty">The property to map the value part of the BCS-fields. Syntax: entity => entity.BcsValue</param>
        /// <param name="fieldInternalName">The internal name of the BCS-field.</param>
        protected void MapBcsField<TBCSIdProperty, TValueProperty>(Expression<Func<TEntity, TBCSIdProperty>> bcsIdProperty, Expression<Func<TEntity, TValueProperty>> valueProperty, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(bcsIdProperty);

            if (SPGENCommon.HasInterface(pInfo.PropertyType, typeof(IEnumerable)))
                throw new ArgumentException("The property type implements IEnumerable and is not supported.");

            var adapter = new Func<SPGENEntityAdapterBcs<TEntity, TBCSIdProperty>>(() => new SPGENEntityAdapterBcs<TEntity, TBCSIdProperty>() { AutoNullCheck = true });

            AddToPropertyAccessorMap<TBCSIdProperty, SPGENEntityAdapterBcs<TEntity, TBCSIdProperty>>(pInfo, fieldInternalName + "_ID", adapter);

            MapField<TValueProperty>(valueProperty, fieldInternalName);
        }


        /// <summary>
        /// Maps a field value object to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapFieldToValueObject<TValueObject>(Expression<Func<TEntity, TValueObject>> property, string fieldInternalName) where TValueObject : class
        {
            MapFieldToValueObject<TValueObject, SPGENEntityValueObjectMapBase<TValueObject>>(property, fieldInternalName);
        }

        /// <summary>
        /// Maps a field value object to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="useRawFieldValue">Uses the raw field value instead of converting it.</param>
        protected void MapFieldToValueObject<TValueObject>(Expression<Func<TEntity, TValueObject>> property, string fieldInternalName, bool useRawFieldValue) where TValueObject : class
        {
            MapFieldToValueObject<TValueObject, SPGENEntityValueObjectMapBase<TValueObject>>(property, fieldInternalName, useRawFieldValue);
        }

        /// <summary>
        /// Maps a collection of field value objects to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapFieldToValueObject<TValueObject>(Expression<Func<TEntity, IEnumerable<TValueObject>>> property, string fieldInternalName) where TValueObject : class
        {
            MapFieldToValueObject<TValueObject, SPGENEntityValueObjectMapBase<TValueObject>>(property, fieldInternalName);
        }

        /// <summary>
        /// Maps a collection of field value objects to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="useRawFieldValue">Uses the raw field value instead of converting it.</param>
        protected void MapFieldToValueObject<TValueObject>(Expression<Func<TEntity, IEnumerable<TValueObject>>> property, string fieldInternalName, bool useRawFieldValue) where TValueObject : class
        {
            MapFieldToValueObject<TValueObject, SPGENEntityValueObjectMapBase<TValueObject>>(property, fieldInternalName, useRawFieldValue);
        }


        /// <summary>
        /// Maps a field value object to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <typeparam name="TValueObjectMapper">The value object mapper type to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapFieldToValueObject<TValueObject, TValueObjectMapper>(Expression<Func<TEntity, TValueObject>> property, string fieldInternalName)
            where TValueObject : class
            where TValueObjectMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            MapFieldToValueObject<TValueObject, TValueObjectMapper>(property, fieldInternalName, false);
        }

        /// <summary>
        /// Maps a field value object to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <typeparam name="TValueObjectMapper">The value object mapper type to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="useRawFieldValue">Uses the raw field value instead of converting it.</param>
        protected void MapFieldToValueObject<TValueObject, TValueObjectMapper>(Expression<Func<TEntity, TValueObject>> property, string fieldInternalName, bool useRawFieldValue)
            where TValueObject : class
            where TValueObjectMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);

            var adapter = new Func<SPGENEntityAdapterGeneric<TEntity, TValueObject, object>>(() =>
            {
                    var a = new SPGENEntityAdapterGeneric<TEntity, TValueObject, object>();

                    a.UseRawListItemValue = useRawFieldValue;

                    a.RegisterToPropertyValueConverter(args =>
                    {
                        if (args.Value == null)
                            return default(TValueObject);

                        var instance = GetValueObjectMapInstance<TValueObject>(args.TargetProperty);
                        
                        return instance.ToValueObject<TEntity>(args.Value, args.FieldName, args.OperationContext);
                    });

                    a.RegisterToItemValueConverter(args =>
                    {
                        if (args.Value == null)
                            return null;

                        var instance = GetValueObjectMapInstance<TValueObject>(args.TargetProperty);
                        
                        return instance.ToFieldValue<TEntity>(args.Value, args.FieldName, args.OperationContext);
                    });

                    a.RegisterEvalComparisonMethod(args => GetValueObjectMapInstance<TValueObject>(args.OwnerEntityProperty).EvalComparison(args));
                    a.RegisterEvalMethodCall((mce, args) => GetValueObjectMapInstance<TValueObject>(args.OwnerEntityProperty).EvalMethodCall(mce, args));

                    return a;
                });

            var accessor = AddToPropertyAccessorMap<TValueObject, SPGENEntityAdapterGeneric<TEntity, TValueObject, object>>(pInfo, fieldInternalName, adapter);

            AddValueObjectMapProperty<TValueObject, TValueObjectMapper>(pInfo);
        }

        /// <summary>
        /// Maps a collection of field value objects to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <typeparam name="TValueObjectMapper">The value object mapper type to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapFieldToValueObject<TValueObject, TValueObjectMapper>(Expression<Func<TEntity, IEnumerable<TValueObject>>> property, string fieldInternalName)
            where TValueObject : class
            where TValueObjectMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            MapFieldToValueObject<TValueObject, TValueObjectMapper>(property, fieldInternalName, false);
        }

        /// <summary>
        /// Maps a collection of field value objects to an entity property. Value objects are objects mapped with special object value mappers derived from SPGENEntityValueObjectMap.
        /// </summary>
        /// <typeparam name="TValueObject">The value object to use.</typeparam>
        /// <typeparam name="TValueObjectMapper">The value object mapper type to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="useRawFieldValue">Uses the raw field value instead of converting it.</param>
        protected void MapFieldToValueObject<TValueObject, TValueObjectMapper>(Expression<Func<TEntity, IEnumerable<TValueObject>>> property, string fieldInternalName, bool useRawFieldValue)
            where TValueObject : class
            where TValueObjectMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterGeneric<TEntity, IEnumerable<TValueObject>, object>>(() =>
                {
                    var a = new SPGENEntityAdapterGeneric<TEntity, IEnumerable<TValueObject>, object>();

                    a.UseRawListItemValue = useRawFieldValue;

                    a.RegisterToPropertyValueConverter(args =>
                    {
                        if (args.Value == null)
                            return new List<TValueObject>();

                        var instance = GetValueObjectMapInstance<TValueObject>(args.TargetProperty);
                        return instance.ToValueObjects<TEntity>(args.Value, args.FieldName, args.OperationContext);
                    });

                    a.RegisterToItemValueConverter(args =>
                    {
                        if (args.Value == null)
                            return null;

                        var instance = GetValueObjectMapInstance<TValueObject>(args.TargetProperty);
                        return instance.ToFieldValue<TEntity>(args.Value, args.FieldName, args.OperationContext);
                    });

                    a.RegisterEvalComparisonMethod(args => GetValueObjectMapInstance<TValueObject>(args.OwnerEntityProperty).EvalComparison(args));
                    a.RegisterEvalMethodCall((mce, args) => GetValueObjectMapInstance<TValueObject>(args.OwnerEntityProperty).EvalMethodCall(mce, args));

                    return a;
                });

            var accessor = AddToPropertyAccessorMap<IEnumerable<TValueObject>, SPGENEntityAdapterGeneric<TEntity, IEnumerable<TValueObject>, object>>(pInfo, fieldInternalName, adapter);

            AddValueObjectMapProperty<TValueObject, TValueObjectMapper>(pInfo);
        }
                

        /// <summary>
        /// Maps a choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, TEnum>> property, string fieldInternalName, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnum<TEntity, TEnum>>(() => new SPGENEntityAdapterEnum<TEntity, TEnum>(default(TEnum), default(TEnum), mappingOptions));

            AddToPropertyAccessorMap<TEnum, SPGENEntityAdapterEnum<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, Nullable<TEnum>>> property, string fieldInternalName, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnumNullable<TEntity, TEnum>>(() => new SPGENEntityAdapterEnumNullable<TEntity, TEnum>(null, null, mappingOptions));

            AddToPropertyAccessorMap<Nullable<TEnum>, SPGENEntityAdapterEnumNullable<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a collection of choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, IEnumerable<TEnum>>> property, string fieldInternalName, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnumList<TEntity, TEnum>>(() => new SPGENEntityAdapterEnumList<TEntity, TEnum>(null, mappingOptions));

            AddToPropertyAccessorMap<IEnumerable<TEnum>, SPGENEntityAdapterEnumList<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="emptyValue">The enum value to use when the field value is null.</param>
        /// <param name="invalidValue">The enum value to use when no matching field value can be found.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, TEnum>> property, string fieldInternalName, TEnum emptyValue, TEnum invalidValue, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnum<TEntity, TEnum>>(() => new SPGENEntityAdapterEnum<TEntity, TEnum>(emptyValue, invalidValue, mappingOptions));

            AddToPropertyAccessorMap<TEnum, SPGENEntityAdapterEnum<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="emptyValue">The enum value to use when the field value is null.</param>
        /// <param name="invalidValue">The enum value to use when no matching field value can be found.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, Nullable<TEnum>>> property, string fieldInternalName, TEnum emptyValue, TEnum invalidValue, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnumNullable<TEntity, TEnum>>(() => new SPGENEntityAdapterEnumNullable<TEntity, TEnum>(emptyValue, invalidValue, mappingOptions));

            AddToPropertyAccessorMap<Nullable<TEnum>, SPGENEntityAdapterEnumNullable<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a collection of choice or lookup field to an entity property.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to use.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="invalidValue">The enum value to use when no matching field value can be found.</param>
        /// <param name="mappingOptions"></param>
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, IEnumerable<TEnum>>> property, string fieldInternalName, TEnum invalidValue, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterEnumList<TEntity, TEnum>>(() => new SPGENEntityAdapterEnumList<TEntity, TEnum>(invalidValue, mappingOptions));

            AddToPropertyAccessorMap<IEnumerable<TEnum>, SPGENEntityAdapterEnumList<TEntity, TEnum>>(pInfo, fieldInternalName, adapter);
        }


        /// <summary>
        /// Maps a lookup field to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, int>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterLookupID<TEntity>>(() => new SPGENEntityAdapterLookupID<TEntity>());
            AddToPropertyAccessorMap<int, SPGENEntityAdapterLookupID<TEntity>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a lookup field id to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, int?>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterLookupIDNullable<TEntity>>(() => new SPGENEntityAdapterLookupIDNullable<TEntity>());
            AddToPropertyAccessorMap<Nullable<int>, SPGENEntityAdapterLookupIDNullable<TEntity>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a lookup field id collection to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, IList<int>>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterLookupIDMulti<TEntity>>(() => new SPGENEntityAdapterLookupIDMulti<TEntity>());
            AddToPropertyAccessorMap<IList<int>, SPGENEntityAdapterLookupIDMulti<TEntity>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a lookup field value to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, string>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterLookupValue<TEntity>>(() => new SPGENEntityAdapterLookupValue<TEntity>());
            AddToPropertyAccessorMap<string, SPGENEntityAdapterLookupValue<TEntity>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a lookup field value collection to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, IList<string>>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterLookupValueMulti<TEntity>>(() => new SPGENEntityAdapterLookupValueMulti<TEntity>());
            AddToPropertyAccessorMap<IList<string>, SPGENEntityAdapterLookupValueMulti<TEntity>>(pInfo, fieldInternalName, adapter);
        }

        /// <summary>
        /// Maps a lookup field value to multiple entity properties.
        /// </summary>
        /// <param name="idProperty">The property that will hold the lookup id. Syntax: entity => entity.LookupId</param>
        /// <param name="valueProperty">The property that will hold the lookup value. Syntax: entity => entity.LookupValue</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapLookupField(Expression<Func<TEntity, int>> idProperty, Expression<Func<TEntity, string>> valueProperty, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            MapFieldValueProperty<int, SPFieldLookupValue>(idProperty, f => f.LookupId, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier);
            MapFieldValueProperty<string, SPFieldLookupValue>(valueProperty, f => f.LookupValue, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsValue);
        }


        /// <summary>
        /// Maps a URL field to an entity property
        /// </summary>
        /// <param name="descriptionProperty">The entity property to map the description value to. Syntax: entity => entity.MyProperty</param>
        /// <param name="urlProperty">The entity property to map the URL value to. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUrlField(Expression<Func<TEntity, string>> descriptionProperty, Expression<Func<TEntity, string>> urlProperty, string fieldInternalName)
        {
            MapFieldValueProperty<string, SPFieldUrlValue>(descriptionProperty, u => u.Description, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsUpdatableValue);
            MapFieldValueProperty<string, SPFieldUrlValue>(urlProperty, u => u.Url, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsUpdatableValue);
        }
        

        /// <summary>
        /// Maps a user field id to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, int>> property, string fieldInternalName)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterUserID<TEntity>>(() => new SPGENEntityAdapterUserID<TEntity>());
            var accessor = AddToPropertyAccessorMap<int, SPGENEntityAdapterUserID<TEntity>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a user field id to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, int?>> property, string fieldInternalName)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterUserIDNullable<TEntity>>(() => new SPGENEntityAdapterUserIDNullable<TEntity>());
            var accessor = AddToPropertyAccessorMap<int?, SPGENEntityAdapterUserIDNullable<TEntity>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a user field id collection to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, IList<int>>> property, string fieldInternalName)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterUserIDMulti<TEntity>>(() => new SPGENEntityAdapterUserIDMulti<TEntity>());
            var accessor = AddToPropertyAccessorMap<IList<int>, SPGENEntityAdapterUserIDMulti<TEntity>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a user field value to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, string>> property, string fieldInternalName)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterUserDisplayName<TEntity>>(() => new SPGENEntityAdapterUserDisplayName<TEntity>());
            var accessor = AddToPropertyAccessorMap<string, SPGENEntityAdapterUserDisplayName<TEntity>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = false;
        }

        /// <summary>
        /// Maps a user field value collection to an entity property.
        /// </summary>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, IList<string>>> property, string fieldInternalName)
        {
            var pInfo = GetPropertyInfoFromMember(property);
            var adapter = new Func<SPGENEntityAdapterUserDisplayNameMulti<TEntity>>(() => new SPGENEntityAdapterUserDisplayNameMulti<TEntity>());
            var accessor = AddToPropertyAccessorMap<IList<string>, SPGENEntityAdapterUserDisplayNameMulti<TEntity>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a user field value to multiple entity properties.
        /// </summary>
        /// <param name="idProperty">The property that will hold the user id. Syntax: entity => entity.UserId</param>
        /// <param name="displayNameProperty">The property that will hold the user diplay name. Syntax: entity => entity.UserDisplayName</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapUserField(Expression<Func<TEntity, int>> idProperty, Expression<Func<TEntity, string>> displayNameProperty, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            MapFieldValueProperty<int, SPFieldUserValue>(idProperty, f => f.LookupId, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier);
            MapFieldValueProperty<string, SPFieldUserValue>(displayNameProperty, f => f.LookupValue, fieldInternalName, SPGENEntityMultiplePropertyMapOptions.IsValue);
        }


        /// <summary>
        /// Maps a specific property on a field value to an entity property.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <typeparam name="TFieldValue">The type of the field value.</typeparam>
        /// <param name="entityProperty">Entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldValueProperty">The field property to map.</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="options"></param>
        protected void MapFieldValueProperty<TPropertyValue, TFieldValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, Expression<Func<TFieldValue, TPropertyValue>> fieldValueProperty, string fieldInternalName, SPGENEntityMultiplePropertyMapOptions options)
             where TFieldValue : class
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var entityPropInfo = GetPropertyInfoFromMember(entityProperty);
            var fieldValueFunc = fieldValueProperty.Compile();

            var fieldValuePropInfo = SPGENCommon.ResolveMemberFromExpression<Func<TFieldValue, TPropertyValue>>(fieldValueProperty) as PropertyInfo;
            var fieldValuePropertyAccessor = SPGENEntityPropertyAccessor<TFieldValue>.CreateAccessor<TPropertyValue>(fieldValuePropInfo, false);

            var adapter = new Func<SPGENEntityAdapterFieldValueProperties<TEntity, TFieldValue, TPropertyValue>>(
                () => new SPGENEntityAdapterFieldValueProperties<TEntity, TFieldValue, TPropertyValue>(
                    fieldValuePropertyAccessor, fieldValueFunc, options));

            var accessor = AddToPropertyAccessorMap<TPropertyValue, SPGENEntityAdapterFieldValueProperties<TEntity, TFieldValue, TPropertyValue>>(entityPropInfo, fieldInternalName, adapter);

            if (options != SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier &&
                options != SPGENEntityMultiplePropertyMapOptions.IsUpdatableValue)
            {
                accessor.SupportsUpdate = false;
            }
        }


        /// <summary>
        /// Maps a field to an entity property.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        protected void MapField<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);
            
            var accessor = AddToPropertyAccessorMap<TPropertyValue>(pInfo, fieldInternalName);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a field to an entity property using an adapter.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <typeparam name="TAdapter">The adapter type.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="adapter">A lambda function returning a new instance of an adapter class each time it is invoked. Example: () => new MyAdapter(..)</param>
        protected void MapField<TPropertyValue, TAdapter>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, Func<TAdapter> adapter)
            where TAdapter : SPGENEntityAdapter<TEntity, TPropertyValue>
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);

            var accessor = AddToPropertyAccessorMap<TPropertyValue, TAdapter>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = true;
        }

        /// <summary>
        /// Maps a field to an entity property.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="supportsFieldUpdates">If the property supports updates.</param>
        protected void MapField<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, bool supportsFieldUpdates)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);

            var accessor = AddToPropertyAccessorMap<TPropertyValue>(pInfo, fieldInternalName);
            accessor.SupportsUpdate = supportsFieldUpdates;
        }

        /// <summary>
        /// Maps a field to an entity property using an adapter.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <typeparam name="TAdapter">The adapter type.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="supportsFieldUpdates">If the property mapping supports field updates.</param>
        /// <param name="adapter">A lambda function returning a new instance of an adapter class each time it is invoked. Example: () => new MyAdapter(..)</param>
        protected void MapField<TPropertyValue, TAdapter>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, bool supportsFieldUpdates, Func<TAdapter> adapter)
            where TAdapter : SPGENEntityAdapter<TEntity, TPropertyValue>
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);
            
            var pInfo = GetPropertyInfoFromMember(property);

            var accessor = AddToPropertyAccessorMap<TPropertyValue, TAdapter>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = supportsFieldUpdates;
        }

        /// <summary>
        /// Maps a field to an entity property using an adapter.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="property">The entity property to map. Syntax: entity => entity.MyProperty</param>
        /// <param name="fieldInternalName">The internal name of the field to map.</param>
        /// <param name="toPropertyConverter">Lambda function to invoke when converting from field to entity property value.</param>
        /// <param name="toListItemValueConverter">Lambda function to invoke when converting from entity property value to field value.</param>
        protected void MapField<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, Func<SPGENEntityAdapterConvArgs<TEntity, object>, TPropertyValue> toPropertyConverter, Func<SPGENEntityAdapterConvArgs<TEntity, TPropertyValue>, object> toListItemValueConverter)
        {
            EnsureFieldIsNotIdentifier(fieldInternalName);

            var pInfo = GetPropertyInfoFromMember(property);

            var adapter = new Func<SPGENEntityAdapterGeneric<TEntity, TPropertyValue, object>>(() =>
                {
                    var a = new SPGENEntityAdapterGeneric<TEntity, TPropertyValue, object>();

                    if (toPropertyConverter != null)
                    {
                        a.RegisterToPropertyValueConverter(toPropertyConverter);
                    }

                    if (toListItemValueConverter != null)
                    {
                        a.RegisterToItemValueConverter(toListItemValueConverter);
                    }
                    return a;
                });

            var accessor = AddToPropertyAccessorMap<TPropertyValue, SPGENEntityAdapterGeneric<TEntity, TPropertyValue, object>>(pInfo, fieldInternalName, adapter);
            accessor.SupportsUpdate = (toListItemValueConverter != null);
        }


        protected static PropertyInfo GetPropertyInfoFromMember<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property)
        {
            MemberInfo member = SPGENCommon.ResolveMemberFromExpression<Func<TEntity, TPropertyValue>>(property);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported for mapping.");

            return member as PropertyInfo;
        }


        #region Obsolete

        [Obsolete("Not longer supported in this version.", true)]
        protected void MapAttachments(Expression<Func<TEntity, object>> property)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer supported in this version.", true)]
        protected void MapFile(Expression<Func<TEntity, object>> property)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not supported any longer. Try using the overloaded version of this method that takes two type parameters, specifying both the BCS-id and the value type.", true)]
        protected void MapBcsField<TBCSIdProperty>(Expression<Func<TEntity, TBCSIdProperty>> bcsIdProperty, Expression<Func<TEntity, object>> valueProperty, string fieldInternalName)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use.", true)]
        protected void MapFieldToValueObject<TValueObject>(Expression<Func<TEntity, object>> property, string fieldInternalName) where TValueObject : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not in longer in use. Use one of the other available overloaded versions fot this method.", true)]
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, object>> property, string fieldInternalName, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not in longer in use. Use one of the other available overloaded versions fot this method.", true)]
        protected void MapFieldToEnum<TEnum>(Expression<Func<TEntity, object>> property, string fieldInternalName, TEnum emptyValue, TEnum invalidValue, SPGENEntityEnumMappingOptions mappingOptions)
            where TEnum : struct
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Try using different overloaded version of this method.", true)]
        protected void MapLookupField(Expression<Func<TEntity, object>> property, string fieldInternalName)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Try using different overloaded versions of this method.", true)]
        protected void MapUserField(Expression<Func<TEntity, object>> property, string fieldInternalName, UserFieldMappingOptions options)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Convert the adapter in-parameter to a lambda function. Example: () => adapter", true)]
        protected void MapField<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, SPGENEntityAdapter<TEntity, TPropertyValue> adapter)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Convert the adapter in-parameter to a lambda function. Example: () => adapter", true)]
        protected void MapField<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, string fieldInternalName, bool supportsFieldUpdates, SPGENEntityAdapter<TEntity, TPropertyValue> adapter)
        {
            throw new NotSupportedException();
        }


        [Obsolete("Use SPGENEntityUserFieldMapOptions instead.", true)]
        public enum UserFieldMappingOptions
        {
            /// <summary>
            /// Returns the user ID.
            /// </summary>
            UserID,
            /// <summary>
            /// Returns user display name. The property is not updatable when using this option.
            /// </summary>
            /// <remarks>The property is not updatable when using this option.</remarks>
            DisplayName
        }

        [Obsolete("Use SPGENEntityMultiplePropertyMapOptions instead.", true)]
        public enum MultiplePropertyOptions
        {
            None,
            IsIdentifier,
            IsValue,
            IsUpdatableIdentifier,
            IsUpdatableValue
        }

        #endregion
    }

    /// <summary>
    /// Used when specifying multiple field property mappings.
    /// </summary>
    public enum SPGENEntityMultiplePropertyMapOptions
    {
        /// <summary>
        /// Same as IsValue except it can not be used as criteria in queries.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that this mapping is the identifier.
        /// </summary>
        IsIdentifier,
        /// <summary>
        /// Indicates that this mapping is the value.
        /// </summary>
        IsValue,
        /// <summary>
        /// Indicates that this mapping is the identifier and is updatable.
        /// </summary>
        IsUpdatableIdentifier,
        /// <summary>
        /// Indicates that this mapping is the value and is updatable.
        /// </summary>
        IsUpdatableValue
    }

    /// <summary>
    /// Used when specifying user field mappings.
    /// </summary>
    [Obsolete()]
    public enum SPGENEntityUserFieldMapOptions
    {
        /// <summary>
        /// Returns the user ID.
        /// </summary>
        UserID,
        /// <summary>
        /// Returns user display name. The property is not updatable when using this option.
        /// </summary>
        /// <remarks>The property is not updatable when using this option.</remarks>
        DisplayName
    }

    /// <summary>
    /// Selects which method to use when comparing choice values against enum values.
    /// </summary>
    public enum SPGENEntityEnumMappingOptions
    {
        /// <summary>
        /// Uses the choice text value.
        /// </summary>
        ChoiceText,
        /// <summary>
        /// Uses the choice mapping value.
        /// </summary>
        ChoiceMappings,
        /// <summary>
        /// Uses the value of a lookup field.
        /// </summary>
        LookupValue,
        /// <summary>
        /// Uses the identifier value of a lookup field.
        /// </summary>
        LookupId
    }

    internal enum SPGENEntityFileMappingMode
    {
        None,
        MapFileNameOnly,
        MapFileNameAndContentAsByteArray,
        MapFileNameAndContentAsByteArrayLazy,
        MapFileNameAndContentAsStreamLazy,
        CustomMapping
    }

    /// <summary>
    /// Specifies when to include files in operations.
    /// </summary>
    public enum SPGENEntityFileInclusionMode
    {
        /// <summary>
        /// Does not include file and attachments as default in operations. You must explicitly specify that you want to include them per operation basis.
        /// </summary>
        ExplicitlyPerOperation,
        /// <summary>
        /// Includes files on all operations. Use this option with CAUTION because it can consume large amount of system resources.
        /// </summary>
        OnAllOperations
    }
}
