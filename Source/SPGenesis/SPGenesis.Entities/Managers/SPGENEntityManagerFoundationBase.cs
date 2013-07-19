using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Repository;
using System.Reflection;
using System.Xml;

namespace SPGenesis.Entities
{
    /// <summary>
    /// This manager base contains all standard MS SharePoint Foundation based operations for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract partial class SPGENEntityManagerFoundationBase<TEntity> : SPGENEntityManagerBase<TEntity, SPGENEntityMap<TEntity>>
        where TEntity : class
    {
        private ISPGENRepositoryManager _repositoryManager = SPGENRepositoryManager.Instance;

        /// <summary>
        /// Makes a LINQ query updateable. The query must consist of entities of type TEntity.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>An updatable collection.</returns>
        public SPGENEntityUpdatableWrapperCollection<TEntity> MakeQueryUpdatable(IQueryable<TEntity> query)
        {
            return new SPGENEntityUpdatableWrapperCollection<TEntity>(query, this);
        }

        /// <summary>
        /// Makes a read only collection updatable.
        /// </summary>
        /// <param name="collection">The read only collection make updatable.</param>
        /// <returns>An updatable collection.</returns>
        public SPGENEntityUpdatableWrapperCollection<TEntity> MakeCollectionUpdatable(SPGENEntityCollection<TEntity> collection)
        {
            return new SPGENEntityUpdatableWrapperCollection<TEntity>(collection, this);
        }

        /// <summary>
        /// Makes a read only collection updatable. The collection source must be a SPGENEntityCollection[TEntity].
        /// </summary>
        /// <param name="collection">The read only collection make updatable.</param>
        /// <returns>An updatable collection.</returns>
        public SPGENEntityUpdatableWrapperCollection<TEntity> MakeCollectionUpdatable(IEnumerable<TEntity> collection)
        {
            return new SPGENEntityUpdatableWrapperCollection<TEntity>(collection, this);
        }


        #region ConvertEntityPropertyToFieldValue

        /// <summary>
        /// Converts an entity property value to a field value.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="entityProperty">The entity property to convert to. Syntax: entity => entity.MyProperty</param>
        /// <param name="value">The value to convert</param>
        /// <param name="list">SPList instance to use field information from.</param>
        /// <returns>The converted value.</returns>
        public object ConvertEntityPropertyToFieldValue<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, TPropertyValue value, SPList list)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            return ConvertEntityPropertyToFieldValueInternal(entityProperty, value, context);
        }

        /// <summary>
        /// Converts an entity property value to a field value.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="entityProperty">The entity property to convert to. Syntax: entity => entity.MyProperty</param>
        /// <param name="value">The value to convert</param>
        /// <param name="web">SPWeb instance to use field information from.</param>
        /// <returns>The converted value.</returns>
        public object ConvertEntityPropertyToFieldValue<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, TPropertyValue value, SPWeb web)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextSiteDataQueryBasedOperations<TEntity>(this, web, null, null);
            return ConvertEntityPropertyToFieldValueInternal(entityProperty, value, context);
        }

        private object ConvertEntityPropertyToFieldValueInternal<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, TPropertyValue value, SPGENEntityOperationContext<TEntity> context)
        {
            var accessor = context.EntityMap.FindPropertyAccessor(GetPropertyInfoFromMember(entityProperty));
            if (accessor == null)
                throw new ArgumentException("The property is not mapped.");

            var adapter = accessor.GetAdapterInstance() as Adapters.ISPGENEntityAdapter<TEntity>;

            context.FieldName = accessor.MappedFieldName;

            var args = accessor.CreateGetPropertyConvArgs() as Adapters.ISPGENEntityAdapterConvArgs<TEntity>;
            args.FieldName = accessor.MappedFieldName;
            args.OperationContext = context;
            args.OperationParameters = context.Parameters;
            args.TargetProperty = accessor.Property;
            args.SetValue(value);

            return adapter.InvokeConvertToListItemValueGeneric(args);
        }

        #endregion


        #region ConvertEntityPropertiesToFieldValues

        /// <summary>
        /// Convert from entity property values to field values.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">SPlist instance to fetch field information from.</param>
        /// <returns>A dictionary with the converted values.</returns>
        public IDictionary<string, object> ConvertEntityPropertiesToFieldValues(TEntity entity, SPList list)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            return ConvertEntityPropertiesToFieldValuesInternal(entity, context);
        }

        /// <summary>
        /// Convert from entity property values to field values.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="web">SPWeb instance to use field information from.</param>
        /// <returns>A dictionary with the converted values.</returns>
        public IDictionary<string, object> ConvertEntityPropertiesToFieldValues(TEntity entity, SPWeb web)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextSiteDataQueryBasedOperations<TEntity>(this, web, null, null);
            return ConvertEntityPropertiesToFieldValuesInternal(entity, context);
        }

        private IDictionary<string, object> ConvertEntityPropertiesToFieldValuesInternal(TEntity entity, SPGENEntityOperationContext<TEntity> context)
        {
            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());
            context.PopulateRepositoryDataItem();

            return context.DataItem.FieldValues;
        }

        #endregion


        #region ConvertFieldValueToEntityProperty

        /// <summary>
        /// Convert from field values to a new entity.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="entityProperty">The entity property to convert to. Syntax: entity => entity.MyProperty</param>
        /// <param name="list">The list instance to use field information from.</param>
        /// <param name="fieldValue">The field value to convert.</param>
        /// <returns></returns>
        public TPropertyValue ConvertFieldValueToEntityProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, SPList list, object fieldValue)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this, list, null);
            return ConvertFieldValueToEntityPropertyInternal<TPropertyValue>(entityProperty, fieldValue, context);
        }

        /// <summary>
        /// Convert from field values to a new entity.
        /// </summary>
        /// <typeparam name="TPropertyValue">The entity property type.</typeparam>
        /// <param name="entityProperty">The entity property to convert to. Syntax: entity => entity.MyProperty</param>
        /// <param name="web">The web instance to use field information from.</param>
        /// <param name="fieldValue">The field value to convert.</param>
        /// <returns></returns>
        public TPropertyValue ConvertFieldValueToEntityProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, SPWeb web, object fieldValue)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextSiteDataQueryBasedOperations<TEntity>(this, web, null, null);
            return ConvertFieldValueToEntityPropertyInternal<TPropertyValue>(entityProperty, fieldValue, context);
        }

        private TPropertyValue ConvertFieldValueToEntityPropertyInternal<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> entityProperty, object fieldValue, SPGENEntityOperationContext<TEntity> context)
        {
            var accessor = context.EntityMap.FindPropertyAccessor(GetPropertyInfoFromMember(entityProperty));
            if (accessor == null)
                throw new ArgumentException("The property is not mapped.");

            context.Entity = context.EntityMap.CreateEntityInstance(context);
            context.FieldName = accessor.MappedFieldName;
            context.EntityMap.InvokeSetPropertyAccessor(accessor, context, fieldValue);

            return entityProperty.Compile().Invoke(context.Entity);
        }

        #endregion


        #region ConvertFieldValuesToEntityProperties

        /// <summary>
        /// Convert from field values to a new entity.
        /// </summary>
        /// <param name="fieldValues">The field values.</param>
        /// <param name="list">The list instance to use field information from.</param>
        /// <returns></returns>
        public TEntity ConvertFieldValuesToEntityProperties(IDictionary<string, object> fieldValues, SPList list)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            return ConvertFieldValuesToEntityPropertiesInternal(fieldValues, context);
        }

        /// <summary>
        /// Convert from field values to a new entity.
        /// </summary>
        /// <param name="fieldValues">The field values.</param>
        /// <param name="web">The web instance to use field information from.</param>
        /// <returns></returns>
        public TEntity ConvertFieldValuesToEntityProperties(IDictionary<string, object> fieldValues, SPWeb web)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextSiteDataQueryBasedOperations<TEntity>(this, web, null, null);
            return ConvertFieldValuesToEntityPropertiesInternal(fieldValues, context);
        }

        private TEntity ConvertFieldValuesToEntityPropertiesInternal(IDictionary<string, object> fieldValues, SPGENEntityOperationContext<TEntity> context)
        {
            context.Entity = context.EntityMap.CreateEntityInstance(context);
            context.DataItem = new SPGENRepositoryDataItem(fieldValues.Keys.ToArray());
            context.DataItem.FieldValues = fieldValues;
            context.PopulateEntity();

            return context.Entity;
        }

        #endregion


        #region GetQueryableList

        /// <summary>
        /// Gets a queryable list implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableList(SPList list)
        {
            return GetQueryableList(list, false, null);
        }

        /// <summary>
        /// Gets a queryable list implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="parameters">Operation parameters</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableList(SPList list, SPGENEntityOperationParameters parameters)
        {
            return GetQueryableList(list, false, parameters);
        }

        /// <summary>
        /// Gets an queryable list implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="makeEntitiesUpdatable">Makes the fetched entities updatable.</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableList(SPList list, bool makeEntitiesUpdatable)
        {
            return GetQueryableList(list, makeEntitiesUpdatable, null);
        }

        /// <summary>
        /// Gets an queryable list implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="makeEntitiesUpdatable">Makes the fetched entities updatable.</param>
        /// <param name="parameters">Operation parameters</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableList(SPList list, bool makeEntitiesUpdatable, SPGENEntityOperationParameters parameters)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            if (!parameters.UpdatableEntities.HasValue)
                parameters.UpdatableEntities = makeEntitiesUpdatable;

            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return new SPGENLinqQueryableList<TEntity>(context);
        }

        #endregion


        #region ConvertToEntity

        /// <summary>
        /// Converts a list item to an entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <returns></returns>
        public TEntity ConvertToEntity(SPListItem listItem)
        {
            return ConvertToEntity(listItem, null);
        }

        /// <summary>
        /// Converts a list item to an entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity ConvertToEntity(SPListItem listItem, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, parameters);
            return ConvertToEntityInternal(listItem, context, false);
        }

        private TEntity ConvertToEntityInternal(SPListItem listItem, SPGENEntityOperationContext<TEntity> context, bool isWrite)
        {
            context.DataItem = new SPGENRepositoryDataItem((isWrite) ? context.GetRequiredFieldsForWrite() : context.GetRequiredFieldsForRead());
            _repositoryManager.ConvertToDataItem(listItem, context.DataItem, CreateFileOperationParameters(context));

            context.CreateAndPopulateEntity();

            return context.Entity;
        }

        #endregion


        #region ConvertToEntityAsUpdatable

        /// <summary>
        /// Converts a SPListItem to an updatable entity. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <returns></returns>
        public TEntity ConvertToEntityAsUpdatable(SPListItem listItem)
        {
            return ConvertToEntity(listItem, null);
        }

        /// <summary>
        /// Converts a SPListItem to an updatable entity. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity ConvertToEntityAsUpdatable(SPListItem listItem, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, parameters);
            EnsureEntityStateEnabled(context);

            return ConvertToEntityInternal(listItem, context, false);
        }

        #endregion


        #region ConvertToEntities

        /// <summary>
        /// Converts an entire list to entities.
        /// </summary>
        /// <param name="list">The list instance to convert.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntities(SPList list)
        {
            return ConvertToEntities(list, parameters: null);
        }

        /// <summary>
        /// Converts an entire list to entities.
        /// </summary>
        /// <param name="list">The list instance to convert.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntities(SPList list, SPGENEntityOperationParameters parameters)
        {
            return ExecuteListItemsFetchOperation(list, null, null, null, parameters);
        }

        /// <summary>
        /// Converts a list item collection to entities.
        /// </summary>
        /// <param name="itemCollection">The list item collection</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntities(SPListItemCollection itemCollection)
        {
            return ConvertToEntities(itemCollection, null);
        }

        /// <summary>
        /// Converts a list item collection to entities.
        /// </summary>
        /// <param name="itemCollection">The list item collection</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntities(SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            return ExecuteListItemsFetchOperation(null, itemCollection, null, null, parameters);
        }

        #endregion


        #region ConvertToEntitiesAsUpdatable

        /// <summary>
        /// Converts all items in a SPList to updatable entities. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The SPList instance.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesAsUpdatable(SPList list)
        {
            return ConvertToEntitiesAsUpdatable(list, null);
        }

        /// <summary>
        /// Converts all items in a SPList to updatable entities. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The SPList instance.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesAsUpdatable(SPList list, SPGENEntityOperationParameters parameters)
        {
            EnsureEntityStateEnabled(ref parameters);

            return ExecuteListItemsFetchOperation(list, null, null, null, parameters);
        }

        /// <summary>
        /// Converts all items in a SPListItemCollection to updatable entities. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="itemCollection">The SPListItemCollection instance.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesAsUpdatable(SPListItemCollection itemCollection)
        {
            return ConvertToEntitiesAsUpdatable(itemCollection, null);
        }

        /// <summary>
        /// Converts all items in a SPListItemCollection to updatable entities. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="itemCollection">The SPListItemCollection instance.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesAsUpdatable(SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            EnsureEntityStateEnabled(ref parameters);

            return ExecuteListItemsFetchOperation(null, itemCollection, null, null, parameters);
        }

        #endregion


        #region TryGetEntity

        /// <summary>
        /// Tries to get an entity from a list. Returns null if it doesn't exist.
        /// </summary>
        /// <param name="list">The parent list to fetch the entity from.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns>Entity instance or null if it doesn't exist.</returns>
        public TEntity TryGetEntity(SPList list, int listItemId)
        {
            return TryGetEntity(list, listItemId, null);
        }

        /// <summary>
        /// Tries to get an entity from a list. Returns null if it doesn't exist.
        /// </summary>
        /// <param name="list">The parent list to fetch the entity from.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="parameters"></param>
        /// <returns>Entity instance or null if it doesn't exist.</returns>
        public TEntity TryGetEntity(SPList list, int listItemId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return TryGetEntityInternal(list, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), parameters, context);
        }

        /// <summary>
        /// Tries to get an entity from a list based on custom ID. Returns null if it doesn't exist.
        /// </summary>
        /// <typeparam name="TEntityId">The type of custom ID.</typeparam>
        /// <param name="list">The parent list to fetch the entity from.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns>Entity instance or null if it doesn't exist.</returns>
        public TEntity TryGetEntity<TEntityId>(SPList list, TEntityId customId)
        {
            return TryGetEntity<TEntityId>(list, customId, null);
        }

        /// <summary>
        /// Tries to get an entity from a list based on custom ID. Returns null if it doesn't exist.
        /// </summary>
        /// <typeparam name="TEntityId">The type of custom ID.</typeparam>
        /// <param name="list">The parent list to fetch the entity from.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="parameters"></param>
        /// <returns>Entity instance or null if it doesn't exist.</returns>
        public TEntity TryGetEntity<TEntityId>(SPList list, TEntityId customId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return TryGetEntityInternal(list, IdentifierInfo.CreateCustomId(customId, context.EntityMap), parameters, context);
        }

        private TEntity TryGetEntityInternal(SPList list, IdentifierInfo info, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context)
        {
            LoadDataItemIntoContext(list, null, info, context, false);

            if (context.DataItem == null)
                return null;

            context.CreateAndPopulateEntity();

            return context.Entity;
        }

        #endregion


        #region GetEntity

        /// <summary>
        /// Gets an entity from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPList list, int listItemId)
        {
            return GetEntity(list, listItemId, null);
        }

        /// <summary>
        /// Gets an entity from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntity<TEntityId>(list, customId, null);
        }

        /// <summary>
        /// Gets an entity from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPList list, int listItemId, out SPListItem listItem)
        {
            return GetEntity(list, listItemId, null, out listItem);
        }

        /// <summary>
        /// Gets an entity from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPList list, TEntityId customId, out SPListItem listItem)
        {
            return GetEntity<TEntityId>(list, customId, null, out listItem);
        }

        /// <summary>
        /// Gets an entity from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity GetEntity(SPList list, int listItemId, SPGENEntityOperationParameters parameters)
        {
            SPListItem listItem;
            return GetEntity(list, listItemId, parameters, out listItem);
        }

        /// <summary>
        /// Gets an entity from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPList list, TEntityId customId, SPGENEntityOperationParameters parameters)
        {
            SPListItem listItem;
            return GetEntity<TEntityId>(list, customId, parameters, out listItem);
        }

        /// <summary>
        /// Gets an entity from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="parameters"></param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPList list, int listItemId, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityInternal(list, null, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), parameters, context, out listItem);
        }

        /// <summary>
        /// Gets an entity from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="parameters"></param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPList list, TEntityId customId, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityInternal(list, null, IdentifierInfo.CreateCustomId(customId, context.EntityMap), parameters, context, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection.
        /// </summary>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPListItemCollection listItemCollection, int listItemId)
        {
            SPListItem listItem;
            return GetEntity(listItemCollection, listItemCollection, null, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection with a custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPListItemCollection listItemCollection, TEntityId customId)
        {
            SPListItem listItem;
            return GetEntity<TEntityId>(listItemCollection, customId, null, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection.
        /// </summary>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPListItemCollection listItemCollection, int listItemId, out SPListItem listItem)
        {
            return GetEntity(listItemCollection, listItemId, null, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection with a custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPListItemCollection listItemCollection, TEntityId customId, out SPListItem listItem)
        {
            return GetEntity<TEntityId>(listItemCollection, customId, null, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection.
        /// </summary>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="parameters"></param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity(SPListItemCollection listItemCollection, int listItemId, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForItemCollectionBasedOperations<TEntity>(this, listItemCollection, parameters);

            return GetEntityInternal(null, listItemCollection, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), parameters, context, out listItem);
        }

        /// <summary>
        /// Get entity from a list item collection with a custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="listItemCollection">The list item collection.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="parameters"></param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntity<TEntityId>(SPListItemCollection listItemCollection, TEntityId customId, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForItemCollectionBasedOperations<TEntity>(this, listItemCollection, parameters);

            return GetEntityInternal(null, listItemCollection, IdentifierInfo.CreateCustomId(customId, context.EntityMap), parameters, context, out listItem);
        }

        private TEntity GetEntityInternal(SPList list, SPListItemCollection listItemCollection, IdentifierInfo info, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context, out SPListItem listItem)
        {
            LoadDataItemIntoContext(list, listItemCollection, info, context, true);
            context.CreateAndPopulateEntity();

            listItem = context.DataItem.ListItem;

            return context.Entity;
        }

        #endregion


        #region GetEntityAsUpdatable

        /// <summary>
        /// Gets an updatable entity by list item ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns></returns>
        public TEntity GetEntityAsUpdatable(SPList list, int listItemId)
        {
            return GetEntityAsUpdatable(list, listItemId, null);
        }

        /// <summary>
        /// Gets an updatable entity by custom ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">The type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID.</param>
        /// <returns></returns>
        public TEntity GetEntityAsUpdatable<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntityAsUpdatable<TEntityId>(list, customId, null);
        }

        /// <summary>
        /// Gets an updatable entity by list item ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity GetEntityAsUpdatable(SPList list, int listItemId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityAsUpdatableInternal(list, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), parameters, context);
        }

        /// <summary>
        /// Gets an updatable entity by custom ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">The type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity GetEntityAsUpdatable<TEntityId>(SPList list, TEntityId customId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityAsUpdatableInternal(list, IdentifierInfo.CreateCustomId(customId, context.EntityMap), parameters, context);
        }

        private TEntity GetEntityAsUpdatableInternal(SPList list, IdentifierInfo info, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureEntityStateEnabled(context);

            LoadDataItemIntoContext(list, null, info, context, true);
            context.CreateAndPopulateEntity();

            return context.Entity;
        }

        #endregion


        #region GetEntityAsUpdatableWrapper

        /// <summary>
        /// Gets an updatable entity wrapper by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listItemId"></param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityAsUpdatableWrapper(SPList list, int listItemId)
        {
            return GetEntityAsUpdatableWrapper(list, listItemId, null);
        }

        /// <summary>
        /// Gets an updatable entity wrapper by custom ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId"></typeparam>
        /// <param name="list"></param>
        /// <param name="customId"></param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityAsUpdatableWrapper<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntityAsUpdatableWrapper<TEntityId>(list, customId, null);
        }

        /// <summary>
        /// Gets an updatable entity wrapper by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listItemId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityAsUpdatableWrapper(SPList list, int listItemId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityAsUpdatableWrapperInternal(list, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), parameters, context);
        }

        /// <summary>
        /// Gets an updatable entity wrapper by custom ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId"></typeparam>
        /// <param name="list"></param>
        /// <param name="customId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityAsUpdatableWrapper<TEntityId>(SPList list, TEntityId customId, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return GetEntityAsUpdatableWrapperInternal(list, IdentifierInfo.CreateCustomId(customId, context.EntityMap), parameters, context);
        }

        private SPGENEntityUpdatableWrapper<TEntity> GetEntityAsUpdatableWrapperInternal(SPList list, IdentifierInfo info, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureEntityStateEnabled(context);

            LoadDataItemIntoContext(list, null, info, context, true);
            context.CreateAndPopulateEntity();

            return new SPGENEntityUpdatableWrapper<TEntity>(context.Entity, context.DataItem, this);
        }

        #endregion


        #region GetEntityFromEventProperties

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType)
        {
            return GetEntityFromEventProperties(eventProperties, collectionType, null, false);
        }

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, bool useListItemValueIfNotFound)
        {
            return GetEntityFromEventProperties(eventProperties, collectionType, null, useListItemValueIfNotFound);
        }

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <param name="valuesFromEventProperties">Outputs the values found in the event properties.</param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, bool useListItemValueIfNotFound, out IDictionary<string, object> valuesFromEventProperties)
        {
            return GetEntityFromEventProperties(eventProperties, collectionType, null, useListItemValueIfNotFound, out valuesFromEventProperties);
        }

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENEntityOperationParameters parameters)
        {
            return GetEntityFromEventProperties(eventProperties, collectionType, parameters, false);
        }

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="parameters"></param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENEntityOperationParameters parameters, bool useListItemValueIfNotFound)
        {
            IDictionary<string, object> values;
            return GetEntityFromEventProperties(eventProperties, collectionType, parameters, useListItemValueIfNotFound, out values);
        }

        /// <summary>
        /// Gets an entity based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="parameters"></param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <param name="valuesFromEventProperties">Outputs the values found in the event properties.</param>
        /// <returns></returns>
        public TEntity GetEntityFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENEntityOperationParameters parameters, bool useListItemValueIfNotFound, out IDictionary<string, object> valuesFromEventProperties)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEventBasedOperations<TEntity>(this, eventProperties, collectionType, parameters);
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForRead());

            _repositoryManager.ConvertToDataItem(eventProperties, collectionType, context.DataItem, useListItemValueIfNotFound, out valuesFromEventProperties);

            context.CreateAndPopulateEntity();

            return context.Entity;
        }

        #endregion


        #region GetEntities

        /// <summary>
        /// Gets an enumerable collection of entities based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="query">The query object to use.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, SPQuery query)
        {
            return GetEntities(list, query.Query, parameters: null);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="query">The query object to use.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, SPQuery query, SPGENEntityOperationParameters parameters)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            parameters.SPQueryTemplate = query;

            return ExecuteListItemsFetchOperation(list, null, query.Query, null, parameters);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a CAML-query string against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query string to use.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, string CAMLquery)
        {
            return GetEntities(list, CAMLquery, parameters: null);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a CAML-query string against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query string to use.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, string CAMLquery, SPGENEntityOperationParameters parameters)
        {
            return ExecuteListItemsFetchOperation(list, null, CAMLquery, null, parameters);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a filter expression which will be translated into a CAML-query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntities(list, filterExpression, parameters: null);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a filter expression which will be translated into a CAML-query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPGENEntityOperationParameters parameters)
        {
            return ExecuteListItemsFetchOperation(list, null, null, filterExpression, parameters);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a site data query against a web instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <param name="query">The site data query instance.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPWeb web, SPSiteDataQuery query)
        {
            return GetEntities(web, query, null);
        }

        /// <summary>
        /// Gets an enumerable collection of entities based on a site data query against a web instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <param name="query">The site data query instance.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntities(SPWeb web, SPSiteDataQuery query, SPGENEntityOperationParameters parameters)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            parameters.IncludeFiles = false;

            var context = SPGENEntityOperationContextFactory.CreateContextSiteDataQueryBasedOperations<TEntity>(this, web, query, parameters);
            context.SiteDataQuery = query;

            var itemCollection = ExecuteSiteDataQueryEnsureRequiredFields(web, context, query);

            return new SPGENEntityCollection<TEntity>(itemCollection, context);
        }

        #endregion


        #region GetEntitiesAsUpdatable

        /// <summary>
        /// Gets an enumerable collection of updatable entities based on a query. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list, string CAMLquery)
        {
            return GetEntitiesAsUpdatable(list, CAMLquery, null);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities based on a query. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list, string CAMLquery, SPGENEntityOperationParameters parameters)
        {
            EnsureEntityStateEnabled(ref parameters);
            return GetEntities(list, CAMLquery, parameters);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities from a list. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntitiesAsUpdatable(list, filterExpression, null);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities from a list. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPGENEntityOperationParameters parameters)
        {
            EnsureEntityStateEnabled(ref parameters);
            return GetEntities(list, filterExpression, parameters);
        }

        #endregion


        #region GetEntitiesAsUpdatableWrapper

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers based on a query. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list, string CAMLquery)
        {
            return GetEntitiesAsUpdatableWrapper(list, CAMLquery, null);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers based on a query. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list, string CAMLquery, SPGENEntityOperationParameters parameters)
        {
            return new SPGENEntityUpdatableWrapperCollection<TEntity>(GetEntitiesAsUpdatable(list, CAMLquery, parameters), this);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntitiesAsUpdatableWrapper(list, filterExpression, null);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPGENEntityOperationParameters parameters)
        {
            return new SPGENEntityUpdatableWrapperCollection<TEntity>(GetEntitiesAsUpdatable(list, filterExpression, parameters), this);
        }

        #endregion


        #region GetEntitiesWithPaging

        [Obsolete("Use the 'UsePaging' method in operation parameters instead.", false)]
        public int GetEntitiesWithPaging(SPList list, int pageSize, int lastPageItemId, out IList<TEntity> result)
        {
            return GetEntitiesWithPaging(list, pageSize, lastPageItemId, out result, null);
        }

        [Obsolete("Use the 'UsePaging' method in operation parameters instead.", false)]
        public int GetEntitiesWithPaging(SPList list, int pageSize, int lastPageItemId, out IList<TEntity> result, SPGENEntityOperationParameters parameters)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            parameters.PagingInfo = new SPGENEntityPagedCollectionInfo(Convert.ToUInt32(pageSize), lastPageItemId);

            result = ConvertToEntities(list, parameters).ToList();

            return parameters.PagingInfo.LastItemIdOfCurrentPage;
        }

        [Obsolete("Use the 'UsePaging' method in operation parameters instead.", false)]
        public int GetEntitiesWithPaging(SPList list, string CAML, int pageSize, int lastPageItemId, out IList<TEntity> result)
        {
            int id = 0;
            var p = SPGENEntityOperationParameters.UsePaging(Convert.ToUInt32(pageSize), lastPageItemId, lid => id = lid);

            result = GetEntities(list, CAML, p).ToList();
            return id;
        }

        [Obsolete("Use the 'UsePaging' method in operation parameters instead.", false)]
        public int GetEntitiesWithPaging(SPList list, Expression<Func<TEntity, bool>> filterExpression, int pageSize, int lastPageItemId, out IList<TEntity> result)
        {
            return GetEntitiesWithPaging(list, filterExpression, pageSize, lastPageItemId, out result, null);
        }

        [Obsolete("Use the 'UsePaging' method in operation parameters instead.", false)]
        public int GetEntitiesWithPaging(SPList list, Expression<Func<TEntity, bool>> filterExpression, int pageSize, int lastPageItemId, out IList<TEntity> result, SPGENEntityOperationParameters parameters)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            parameters.PagingInfo = new SPGENEntityPagedCollectionInfo(Convert.ToUInt32(pageSize), lastPageItemId);

            result = GetEntities(list, filterExpression, parameters).ToList();

            return parameters.PagingInfo.LastItemIdOfCurrentPage;
        }

        #endregion


        #region ModifyListItem

        public TEntity ModifyListItem(SPList list, int listItemId, Action<TEntity> modifyItemAction)
        {
            return ModifyListItem(list, listItemId, modifyItemAction, null);
        }

        public TEntity ModifyListItem<TEntityId>(SPList list, TEntityId customId, Action<TEntity> modifyItemAction)
        {
            return ModifyListItem<TEntityId>(list, customId, modifyItemAction, null);
        }

        public TEntity ModifyListItem(SPList list, int listItemId, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters)
        {
            SPListItem listItem;
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return ModifyListItemInternal(list, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), modifyItemAction, parameters, context, out listItem);
        }

        public TEntity ModifyListItem<TEntityId>(SPList list, TEntityId customId, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters)
        {
            SPListItem listItem;
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return ModifyListItemInternal(list, IdentifierInfo.CreateCustomId(customId, context.EntityMap), modifyItemAction, parameters, context, out listItem);
        }

        public TEntity ModifyListItem(SPList list, int listItemId, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return ModifyListItemInternal(list, IdentifierInfo.CreateIntId(listItemId, context.EntityMap), modifyItemAction, parameters, context, out listItem);
        }

        public TEntity ModifyListItem<TEntityId>(SPList list, TEntityId customId, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);

            return ModifyListItemInternal(list, IdentifierInfo.CreateCustomId(customId, context.EntityMap), modifyItemAction, parameters, context, out listItem);
        }

        private TEntity ModifyListItemInternal(SPList list, IdentifierInfo info, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context, out SPListItem listItem)
        {
            var entity = GetEntityInternal(list, null, info, parameters, context, out listItem);

            modifyItemAction(entity);

            UpdateListItem(entity, listItem, parameters);

            return entity;
        }

        public TEntity ModifyListItem(SPListItem listItem, Action<TEntity> modifyItemAction, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, parameters);
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForRead());

            _repositoryManager.ConvertToDataItem(listItem, context.DataItem, CreateFileOperationParameters(context));
            context.CreateAndPopulateEntity();

            modifyItemAction(context.Entity);

            context.DataItem.ReInitializeFields(context.GetRequiredFieldsForWrite());
            UpdateListItemInternal(context.Entity, listItem, parameters, context);

            return context.Entity;
        }

        #endregion


        #region UpdateListItem

        /// <summary>
        /// Updates a list item with the specified entity. The entity should be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        public void UpdateListItem(TEntity updatableEntity)
        {
            UpdateListItem(updatableEntity, parameters: null);
        }

        /// <summary>
        /// Updates a list item with the specified entity. The entity should be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        /// <param name="parameters"></param>
        public void UpdateListItem(TEntity updatableEntity, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, parameters);

            PopulateRepositoryItem(context, true);

            if (context.CancelOperation)
                return;

            EnsureItemUpdated(context);
        }

        /// <summary>
        /// Updates a list item in the specified list with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        public void UpdateListItem(TEntity entity, SPList list)
        {
            UpdateListItem(entity, list, parameters: null);
        }

        /// <summary>
        /// Updates a list item in the specified list with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parameters"></param>
        public void UpdateListItem(TEntity entity, SPList list, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);
            context.Entity = entity;

            if (!context.EntityMap.HasIdentifierProperty)
                throw new ArgumentNullException("No identifier property is set for this entity mapper.");

            LoadDataItemIntoContext(list, null, null, null, context, true);

            PopulateRepositoryItem(context, true);

            if (context.CancelOperation)
                return;

            EnsureItemUpdated(context);
        }

        /// <summary>
        /// Updates a list item in the specified list item collection with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="itemCollection">The list item collection.</param>
        public void UpdateListItem(TEntity entity, SPListItemCollection itemCollection)
        {
            UpdateListItem(entity, itemCollection, parameters: null);
        }

        /// <summary>
        /// Updates a list item in the specified list item collection with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="parameters"></param>
        public void UpdateListItem(TEntity entity, SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForItemCollectionBasedOperations<TEntity>(this, itemCollection, parameters);
            context.Entity = entity;

            if (!context.EntityMap.HasIdentifierProperty)
                throw new ArgumentNullException("No identifier property is set for this entity mapper.");
            
            LoadDataItemIntoContext(null, itemCollection, null, null, context, true);

            PopulateRepositoryItem(context, true);

            if (context.CancelOperation)
                return;

            EnsureItemUpdated(context);
        }

        /// <summary>
        /// Updates the specified list item with the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        public void UpdateListItem(TEntity entity, SPListItem listItem)
        {
            UpdateListItem(entity, listItem, parameters: null);
        }

        /// <summary>
        /// Updates the specified list item with the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        /// <param name="parameters"></param>
        public void UpdateListItem(TEntity entity, SPListItem listItem, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, parameters);

            UpdateListItemInternal(entity, listItem, parameters, context);
        }

        private void UpdateListItemInternal(TEntity entity, SPListItem listItem, SPGENEntityOperationParameters parameters, SPGENEntityOperationContext<TEntity> context)
        {
            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());

            _repositoryManager.ConvertToDataItem(listItem, context.DataItem, CreateFileOperationParameters(context));

            PopulateRepositoryItem(context, false);

            if (context.CancelOperation)
                return;

            EnsureItemUpdated(context);
        }

        #endregion


        #region UpdateEventProperties

        /// <summary>
        /// Updates the specified item event properties with the entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Before or after properties.</param>
        public void UpdateEventProperties(TEntity entity, SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType)
        {
            UpdateEventProperties(entity, eventProperties, collectionType, null);
        }

        /// <summary>
        /// Updates the specified item event properties with the entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Before or after properties.</param>
        /// <param name="parameters"></param>
        public void UpdateEventProperties(TEntity entity, SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEventBasedOperations<TEntity>(this, eventProperties, collectionType, parameters);
            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());

            PopulateRepositoryItem(context, false);

            if (context.CancelOperation)
                return;

            _repositoryManager.UpdateEventProperties(context.DataItem, eventProperties, collectionType);
        }

        #endregion


        #region BatchUpdateListItems

        /// <summary>
        /// Updates multiple list items with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        public void BatchUpdateListItems(IEnumerable<TEntity> updatableEntities)
        {
            BatchUpdateListItemsInternal(updatableEntities, null, null, null, null, null);
        }

        /// <summary>
        /// Updates multiple list items with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> updatableEntities, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(updatableEntities, null, null, null, null, parameters);
        }

        /// <summary>
        /// Updates multiple list items with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> updatableEntities, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(updatableEntities, null, null, beforeItemUpdateFunction, null, parameters);
        }

        /// <summary>
        /// Updates multiple list items with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> updatableEntities, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(updatableEntities, null, null, beforeItemUpdateFunction, afterItemUpdateFunction, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPList list)
        {
            BatchUpdateListItems(entities, list, parameters: null);
        }

        /// <summary>
        /// Updates multiple list items in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPList list, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(entities, list: list, itemCollection: null, beforeItemUpdateFunction: null, afterItemUpdateFunction: null, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPList list, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(entities, list: list, itemCollection: null, beforeItemUpdateFunction: beforeItemUpdateFunction, afterItemUpdateFunction: null, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPList list, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(entities, list: list, itemCollection: null, beforeItemUpdateFunction: beforeItemUpdateFunction, afterItemUpdateFunction: afterItemUpdateFunction, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection)
        {
            BatchUpdateListItems(entities, itemCollection, null, parameters: null);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItems(entities, itemCollection, null, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(entities, list: null, itemCollection: itemCollection, beforeItemUpdateFunction: beforeItemUpdateFunction, afterItemUpdateFunction: null, parameters: parameters);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        /// <param name="parameters"></param>
        public void BatchUpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            BatchUpdateListItemsInternal(entities, list: null, itemCollection: itemCollection, beforeItemUpdateFunction: beforeItemUpdateFunction, afterItemUpdateFunction: afterItemUpdateFunction, parameters: parameters);
        }

        private void BatchUpdateListItemsInternal(IEnumerable<TEntity> entities, SPList list, SPListItemCollection itemCollection, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            SPGENEntityOperationContext<TEntity> context;
            if (itemCollection != null)
            {
                context = SPGENEntityOperationContextFactory.CreateContextForItemCollectionBasedOperations<TEntity>(this, itemCollection, parameters);
            }
            else
            {
                context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this, list, parameters);
            }

            foreach (var entity in entities)
            {
                context.CancelItemUpdate = false;
                context.Entity = entity;

                if (list == null && itemCollection == null)
                {
                    SPGENEntityRepositoryState state = context.EntityMap.GetRepositoryStateFromEntity(entity);
                    if (state == null)
                        throw new SPGENEntityGeneralException("The entity instance has no state associated with it.");

                    context.DataItem = state.DataItem;
                }
                else
                {
                    LoadDataItemIntoContext(list, itemCollection, null, null, context, true);
                }

                PopulateRepositoryItem(context, true);

                if (beforeItemUpdateFunction != null)
                {
                    beforeItemUpdateFunction(context);

                    if (context.CancelOperation)
                        return;
                }

                if (!context.CancelItemUpdate)
                {
                    EnsureItemUpdated(context);
                }

                if (afterItemUpdateFunction != null)
                {
                    afterItemUpdateFunction(context);

                    if (context.CancelOperation)
                        return;
                }
            }
        }

        #endregion


        #region CreateNewListItem

        /// <summary>
        /// Creates a new list item in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItem(TEntity entity, SPList list)
        {
            return CreateNewListItem(entity, list, parameters: null);
        }

        /// <summary>
        /// Creates a new list item in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPListItem CreateNewListItem(TEntity entity, SPList list, SPGENEntityOperationParameters parameters)
        {
            return CreateNewListItem(entity, list, null, parameters);
        }

        /// <summary>
        /// Creates a new list item in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItem(TEntity entity, SPList list, string parentFolderRelUrl)
        {
            return CreateNewListItem(entity, list, parentFolderRelUrl, null);
        }

        /// <summary>
        /// Creates a new list item in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPListItem CreateNewListItem(TEntity entity, SPList list, string parentFolderRelUrl, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);
            CreateNewListItemInternal(entity, context, parentFolderRelUrl, null, null);

            return context.ListItem;
        }

        private void CreateNewListItemInternal(TEntity entity, SPGENEntityOperationContext<TEntity> context, string folderUrl, string folderName, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction)
        {
            SPList list = context.List;
            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());

            bool isFolder = !string.IsNullOrEmpty(folderName);
            var fileParams = CreateFileOperationParameters(context);

            if (isFolder == false && list.BaseType == SPBaseType.DocumentLibrary)
            {
                if (context.EntityMap.FileMappingMode == SPGENEntityFileMappingMode.None)
                    throw new SPGENEntityGeneralException("File contents must be mapped in order to execute this operation.");

                if (!context.ShouldIncludeFiles)
                    throw new SPGENEntityGeneralException("List items can not be added to a document library. Use file methods instead.");

                string fileName = context.DataItem.FieldValues["FileLeafRef"] as string;
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("Invalid file name property.");

                _repositoryManager.CreateNewFile(list, context.DataItem, folderUrl, fileName, fileParams);

                PopulateRepositoryItem(context, false);

                if (context.CancelOperation)
                    return;

                if (beforeItemUpdateFunction != null)
                {
                    beforeItemUpdateFunction(context);

                    if (context.CancelOperation || context.CancelItemUpdate)
                        return;
                }

                _repositoryManager.UpdateListItem(context.DataItem, (context.Parameters != null ? context.Parameters.UpdateMethod : SPGENEntityUpdateMethod.Normal), fileParams);
            }
            else
            {
                if (isFolder)
                {
                    _repositoryManager.CreateNewFolder(list, context.DataItem, folderUrl, folderName, CreateFileOperationParameters(context));
                }
                else
                {
                    _repositoryManager.CreateNewListItem(list, context.DataItem, folderUrl, CreateFileOperationParameters(context));
                }

                PopulateRepositoryItem(context, false);

                if (context.CancelOperation)
                    return;

                if (beforeItemUpdateFunction != null)
                {
                    beforeItemUpdateFunction(context);

                    if (context.CancelOperation || context.CancelItemUpdate)
                        return;
                }

                _repositoryManager.UpdateListItem(context.DataItem, (context.Parameters != null ? context.Parameters.UpdateMethod : SPGENEntityUpdateMethod.Normal), CreateFileOperationParameters(context));

                EnsureAttachmentFiles(context, true);
            }

            EnsureItemId(context);

            context.DataItem.ReInitializeFields(context.GetRequiredFieldsForRead());
            PopulateEntityInternal(context);
        }

        #endregion


        #region CreateNewFolder

        /// <summary>
        /// Creates a new folder on the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="folderName">The folder name.</param>
        /// <returns></returns>
        public SPListItem CreateNewFolder(TEntity entity, SPList list, string folderName)
        {
            return CreateNewFolder(entity, list, folderName, null, null);
        }

        /// <summary>
        /// Creates a new folder on the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="folderName">The folder name.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPListItem CreateNewFolder(TEntity entity, SPList list, string folderName, SPGENEntityOperationParameters parameters)
        {
            return CreateNewFolder(entity, list, folderName, null, parameters);
        }

        /// <summary>
        /// Creates a new folder on the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="folderName">The folder name.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <returns></returns>
        public SPListItem CreateNewFolder(TEntity entity, SPList list, string folderName, string parentFolderRelUrl)
        {
            return CreateNewFolder(entity, list, folderName, parentFolderRelUrl, null);
        }

        /// <summary>
        /// Creates a new folder on the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="folderName">The folder name.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SPListItem CreateNewFolder(TEntity entity, SPList list, string folderName, string parentFolderRelUrl, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);
            CreateNewListItemInternal(entity, context, parentFolderRelUrl, folderName, null);

            return context.ListItem;
        }

        #endregion


        #region BatchCreateNewListItems

        /// <summary>
        /// Creates multiple list items on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list)
        {
            BatchCreateNewListItems(entities, list, null, null, null, null);
        }

        /// <summary>
        /// Creates multiple list items on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parameters"></param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list, SPGENEntityOperationParameters parameters)
        {
            BatchCreateNewListItems(entities, list, null, null, null, parameters);
        }

        /// <summary>
        /// Creates multiple list items on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl)
        {
            BatchCreateNewListItems(entities, list, parentFolderRelUrl, null, null, null);
        }

        /// <summary>
        /// Creates multiple list items on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="parameters"></param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl, SPGENEntityOperationParameters parameters)
        {
            BatchCreateNewListItems(entities, list, parentFolderRelUrl, null, null, parameters);
        }

        /// <summary>
        /// Creates multiple list items on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="beforeItemCreatedFunction">Function to execute before each item is created.</param>
        /// <param name="afterItemCreatedFunction">Function to execute after each item is created.</param>
        /// <param name="parameters"></param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl, Action<SPGENEntityOperationContext<TEntity>> beforeItemCreatedFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemCreatedFunction, SPGENEntityOperationParameters parameters)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, parameters);
            var fop = CreateFileOperationParameters(context);

            foreach (var entity in entities)
            {
                CreateNewListItemInternal(entity, context, parentFolderRelUrl, null, beforeItemCreatedFunction);

                if (context.CancelOperation)
                    return;

                if (context.CancelItemUpdate)
                {
                    context.CancelItemUpdate = false;
                    continue;
                }

                if (afterItemCreatedFunction != null)
                {
                    afterItemCreatedFunction(context);

                    if (context.CancelOperation)
                        return;
                }
            }
        }

        #endregion


        #region DeleteListItem

        /// <summary>
        /// Deletes the list item using the state stored inside the updatable entity.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        public void DeleteListItem(TEntity updatableEntity)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, null);
            _repositoryManager.DeleteListItem(context.List, context.ListItem.ID);
        }

        /// <summary>
        /// Deletes the list item using the specified entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        public void DeleteListItem(TEntity entity, SPList list)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            context.Entity = entity;

            var info = context.EntityMap.GetIdentifierValue(context);
            if (info is SPGENEntityBuiltInItemIdIdentifierInfo)
            {
                int id = (info as SPGENEntityBuiltInItemIdIdentifierInfo).ItemId;
                _repositoryManager.DeleteListItem(list, id);
            }
            else
            {
                LoadDataItemIntoContext(list, null, null, (info as SPGENEntityCustomItemIdentifierInfo).CustomId, context, true);
                _repositoryManager.DeleteListItem(list, context.DataItem.ListItemId);
            }

            context.EntityMap.ResetIdentifierValue(context);
        }

        #endregion


        #region BatchDeleteListItems

        /// <summary>
        /// Deletes multiple list items on the specified list using the specified filter expression.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The filter expression.</param>
        public void BatchDeleteListItems(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            var ql = GetQueryableList(list);
            ql.Where<TEntity>(filterExpression);

            foreach (var entity in ql)
            {
                DeleteListItem(entity, list);
            }
        }

        /// <summary>
        /// Deletes multiple list items on the specified list using the specified entity collection.
        /// </summary>
        /// <param name="entities">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        public void BatchDeleteListItems(IEnumerable<TEntity> entities, SPList list)
        {
            foreach (var entity in entities)
            {
                DeleteListItem(entity, list);
            }
        }

        /// <summary>
        /// Deletes multiple list items using the state on each updatable entity.
        /// </summary>
        /// <param name="updatableEntities">The updatable entity instance.</param>
        public void BatchDeleteListItems(IEnumerable<TEntity> updatableEntities)
        {
            foreach (var entity in updatableEntities)
            {
                DeleteListItem(entity);
            }
        }

        #endregion


        #region Obsolete

        [Obsolete("This method is not longer supported. Use ConvertEntitiesAsUpdatableWrapper instead.", true)]
        public IEnumerable<SPGENUpdatableEntity<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertEntitiesAsUpdatableWrapper instead.", true)]
        public IEnumerable<SPGENUpdatableEntity<TEntity>> GetEntitiesAsUpdatableWrapper(SPList list, SPGENEntityOperationParameters<TEntity> parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertEntitiesAsUpdatable instead.", true)]
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertEntitiesAsUpdatable instead.", true)]
        public IEnumerable<TEntity> GetEntitiesAsUpdatable(SPList list, SPGENEntityOperationParameters<TEntity> parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntity instead.", true)]
        public void PopulateEntity(TEntity entity, SPList list, int listItemId)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntity instead.", true)]
        public void PopulateEntity(TEntity entity, SPList list, int listItemId, SPGENEntityOperationParameters<TEntity> parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntity instead.", true)]
        public void PopulateEntity(TEntity entity, SPListItem listItem)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntity instead.", true)]
        public void PopulateEntity(TEntity entity, SPListItem listItem, SPGENEntityOperationParameters<TEntity> parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported.", true)]
        public void SaveAttachments(TEntity entity, SPListItem listItem, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported.", true)]
        public void SaveFile(TEntity entity, SPListItem listItem, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntities(itemCollection).ToList() instead.", true)]
        public IList<TEntity> ConvertToEntitiesAsList(SPListItemCollection itemCollection)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntities(itemCollection, parameters).ToList() instead.", true)]
        public IList<TEntity> ConvertToEntitiesAsList(SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntities instead.", true)]
        public IEnumerable<TEntity> GetEntities(SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use ConvertToEntities instead.", true)]
        public IEnumerable<TEntity> GetEntities(SPList list, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use GetEntity instead.", true)]
        public TEntity ConvertToEntity(SPListItemCollection itemCollection, int listItemId)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use GetEntity instead.", true)]
        public TEntity ConvertToEntity(SPListItemCollection itemCollection, int listItemId, out SPListItem listItem)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use GetEntity instead.", true)]
        public TEntity ConvertToEntity(SPListItemCollection itemCollection, int listItemId, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("This method is not longer supported. Use GetEntity instead.", true)]
        public TEntity ConvertToEntity(SPListItemCollection itemCollection, int listItemId, SPGENEntityOperationParameters parameters, out SPListItem listItem)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchDeleteListItems method insted.", true)]
        public void DeleteListItems(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchDeleteListItems method insted.", true)]
        public void DeleteListItems(IEnumerable<TEntity> entities, SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchCreateNewListItems method insted.", true)]
        public void CreateNewListItems(IEnumerable<TEntity> entities, SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchCreateNewListItems method insted.", true)]
        public void CreateNewListItems(IEnumerable<TEntity> entities, SPList list, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchCreateNewListItems method insted.", true)]
        public void CreateNewListItems(IEnumerable<TEntity> entities, SPList list, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchCreateNewListItems method insted.", true)]
        public void CreateNewListItems(IEnumerable<TEntity> entities, SPList list, string folderUrl, SPFileSystemObjectType? underlyingObjectType, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchCreateNewListItems method insted.", true)]
        public void CreateNewListItems(IEnumerable<TEntity> entities, SPList list, string folderUrl, SPFileSystemObjectType? underlyingObjectType, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPList list, string CAML)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPList list)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPList list, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPList list, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use BatchUpdateListItems method insted.", true)]
        public void UpdateListItems(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, SPGENEntityOperationParameters parameters)
        {
            throw new NotSupportedException();
        }

        #endregion


        #region Private and internal members

        private static PropertyInfo GetPropertyInfoFromMember<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property)
        {
            MemberInfo member = SPGENCommon.ResolveMemberFromExpression<Func<TEntity, TPropertyValue>>(property);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported for mapping.");

            return member as PropertyInfo;
        }

        private static void PopulateEntityInternal(SPGENEntityOperationContext<TEntity> context)
        {
            context.PopulateEntity();
        }

        private static void PopulateRepositoryItem(SPGENEntityOperationContext<TEntity> context, bool switchFromReadtoWrite)
        {
            if (switchFromReadtoWrite)
            {
                context.DataItem.ReInitializeFields(context.GetRequiredFieldsForWrite());
            }

            context.PopulateRepositoryDataItem();
        }

        private void LoadDataItemIntoContext(SPList list, SPListItemCollection itemCollection, IdentifierInfo idInfo, SPGENEntityOperationContext<TEntity> context, bool throwExceptionIfNotExists)
        {
            if (idInfo.IsCustomId)
            {
                LoadDataItemIntoContext(list, itemCollection, null, idInfo.CustomId, context, throwExceptionIfNotExists);
            }
            else
            {
                LoadDataItemIntoContext(list, itemCollection, idInfo.ItemId, null, context, throwExceptionIfNotExists);
            }
        }

        private void LoadDataItemIntoContext(SPList list, SPListItemCollection itemCollection, int? itemId, object customId, SPGENEntityOperationContext<TEntity> context, bool throwExceptionIfNotExists)
        {
            if (!context.EntityMap.HasIdentifierProperty)
                throw new SPGENEntityGeneralException("No identifier property is set for this entity mapper.");


            SPGENRepositoryDataItem dataItem = null;

            if (!itemId.HasValue && customId == null)
            {
                var info = context.EntityMap.GetIdentifierValue(context);
                if (info is SPGENEntityBuiltInItemIdIdentifierInfo)
                {
                    itemId = (info as SPGENEntityBuiltInItemIdIdentifierInfo).ItemId;
                }
                else
                {
                    customId = (info as SPGENEntityCustomItemIdentifierInfo).CustomId;
                }
            }

            if (itemCollection != null)
            {
                if (itemId.HasValue)
                {
                    dataItem = _repositoryManager.GetDataItem(itemCollection, itemId.Value, context.GetRequiredFieldsForRead(), CreateFileOperationParameters(context));
                }
                else
                {
                    foreach (SPListItem item in itemCollection)
                    {
                        if (item[context.EntityMap.IdentifierFieldName] == customId)
                        {
                            dataItem = _repositoryManager.GetDataItem(itemCollection, item.ID, context.GetRequiredFieldsForRead(), CreateFileOperationParameters(context));
                            break;
                        }
                    }
                }
            }
            else
            {
                if (itemId.HasValue)
                {
                    if (throwExceptionIfNotExists)
                    {
                        dataItem = _repositoryManager.GetDataItem(list, itemId.Value, context.GetRequiredFieldsForRead(), context.ShouldIncludeAllFields, CreateFileOperationParameters(context));
                    }
                    else
                    {
                        string caml = string.Format("<Where><Eq><FieldRef Name=\"ID\"/><Value Type=\"Counter\">{0}</Value></Eq></Where>", itemId.Value.ToString());
                        SPQuery query = new SPQuery()
                        {
                            Query = caml,
                            RowLimit = 1
                        };

                        var result = ExecuteListQueryEnsureRequiredFields(context, query).ToArray();
                        if (result.Length > 0)
                        {
                            dataItem = result[0];
                        }
                    }
                }
                else
                {
                    if (customId == null)
                        throw new ArgumentNullException("The identifier value can not be null.");

                    string caml = string.Format("<Where><Eq><FieldRef Name=\"{0}\"/><Value Type=\"{1}\">{2}</Value></Eq></Where>",
                        context.EntityMap.IdentifierFieldName,
                        context.EntityMap.GetCustomIdentifierFieldType(context),
                        customId.ToString());

                    SPQuery query = new SPQuery()
                    {
                        Query = caml,
                        RowLimit = 2
                    };

                    var result = ExecuteListQueryEnsureRequiredFields(context, query).ToArray();

                    if (result.Length > 1)
                    {
                        throw new SPGENEntityGeneralException("There were more than one item matching this identity in the target collection.");
                    }
                    else if (result.Length == 1)
                    {
                        dataItem = result[0];
                    }
                }
            }

            if (dataItem == null && throwExceptionIfNotExists)
                throw new SPGENEntityGeneralException("There were no items matching the identifier value in the target collection.");

            context.DataItem = dataItem;
        }

        private static SPGENEntityFileOperationArguments CreateFileOperationParameters(SPGENEntityOperationContext<TEntity> context)
        {
            var p = new SPGENEntityFileOperationArguments();

            if (context.ShouldIncludeFiles)
            {
                p.FileMappingMode = context.EntityMap.FileMappingMode;
            }
            else
            {
                p.FileMappingMode = SPGENEntityFileMappingMode.None;
            }

            if (context.Parameters != null)
            {
                p.OpenFileOptions = context.Parameters.OpenFileParameters;
                p.SaveFileParameters = context.Parameters.SaveFileParameters;
                p.SaveNewFileParameters = context.Parameters.SaveNewFileParameters;

                if (context.Parameters.MaxFileSizeByteArrays.HasValue)
                {
                    p.MaxFileSizeByteArrays = context.Parameters.MaxFileSizeByteArrays.Value;
                }
                else
                {
                    p.MaxFileSizeByteArrays = context.EntityMap.MaxFileSizeByteArrays;
                }
            }
            else
            {
                p.MaxFileSizeByteArrays = context.EntityMap.MaxFileSizeByteArrays;
            }

            return p;
        }

        private void EnsureItemUpdated(SPGENEntityOperationContext<TEntity> context)
        {
            SPGENEntityUpdateMethod updateMethod = SPGENEntityUpdateMethod.Normal;
            var fileOperationParameters = CreateFileOperationParameters(context);

            if (context.ShouldIncludeFiles && context.List.BaseType == SPBaseType.DocumentLibrary)
            {
                if (context.EntityMap.FileMappingMode != SPGENEntityFileMappingMode.CustomMapping &&
                    context.EntityMap.FileMappingMode != SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
                {
                    if (context.Parameters == null)
                        updateMethod = SPGENEntityUpdateMethod.SystemUpdateOverwriteVersion;
                    
                    _repositoryManager.UpdateListItem(context.DataItem, updateMethod, fileOperationParameters);
                    
                    _repositoryManager.SaveFile(context.DataItem, CreateFileOperationParameters(context));
                }
                else
                {
                    if (context.Parameters != null)
                        updateMethod = context.Parameters.UpdateMethod;
    
                    _repositoryManager.UpdateListItem(context.DataItem, updateMethod, fileOperationParameters);
                }
            }
            else
            {
                EnsureAttachmentFiles(context, false);

                if (context.Parameters != null)
                    updateMethod = context.Parameters.UpdateMethod;

                _repositoryManager.UpdateListItem(context.DataItem, updateMethod, fileOperationParameters);
            }

            PopulateEntityInternal(context);
        }

        private static void EnsureEntityStateEnabled(SPGENEntityOperationContext<TEntity> context)
        {
            if (context.Parameters != null)
            {
                context.Parameters.UpdatableEntities = true;
            }
            else
            {
                context.Parameters = SPGENEntityOperationParameters.UseUpdatableEntities;
            }
        }

        private static void EnsureEntityStateEnabled(ref SPGENEntityOperationParameters parameters)
        {
            if (parameters != null)
            {
                parameters.UpdatableEntities = true;
            }
            else
            {
                parameters = SPGENEntityOperationParameters.UseUpdatableEntities;
            }
        }

        private void EnsureAttachmentFiles(SPGENEntityOperationContext<TEntity> context, bool forceUpdateOfAllAttachments)
        {
            if (context.DataItem.Attachments == null)
                return;

            if (!context.ShouldIncludeFiles || 
                context.EntityMap.FileMappingMode == SPGENEntityFileMappingMode.CustomMapping ||
                context.EntityMap.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
                return;

            var p = CreateFileOperationParameters(context);
            p.ForceFileSave = forceUpdateOfAllAttachments;

            _repositoryManager.SaveAttachments(context.DataItem, p);
        }

        private static void EnsureItemId(SPGENEntityOperationContext<TEntity> context)
        {
            if (context.EntityMap.HasIdentifierProperty)
            {
                context.EntityMap.SetIdentifierValue(context);
            }
        }

        private SPGENEntityCollection<TEntity> ExecuteListItemsFetchOperation(SPList list, SPListItemCollection listItemCollection, string CAMLquery, Expression<Func<TEntity, bool>> filterExpression, SPGENEntityOperationParameters parameters)
        {
            if (listItemCollection != null)
            {
                var context = SPGENEntityOperationContextFactory.CreateContextForItemCollectionBasedOperations<TEntity>(this, listItemCollection, parameters);

                return ExecuteListItemsFetchOperation(context, CAMLquery, filterExpression);
            }
            else
            {
                var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this, list, parameters);

                return ExecuteListItemsFetchOperation(context, CAMLquery, filterExpression);
            }
        }

        internal SPGENEntityCollection<TEntity> ExecuteListItemsFetchOperation(SPGENEntityOperationContext<TEntity> context, string CAMLquery, Expression<Func<TEntity, bool>> filterExpression)
        {
            SPGENEntityCollection<TEntity> ret;
            bool usePaging = false;

            var parameters = context.Parameters;
            if (parameters != null && parameters.PagingInfo != null)
            {
                if (parameters.SPQueryTemplate == null)
                    parameters.SPQueryTemplate = new SPQuery();

                parameters.SPQueryTemplate.RowLimit = parameters.PagingInfo.PageSize;

                if (parameters.PagingInfo.LastItemIdOfLastPage > 0)
                {
                    parameters.SPQueryTemplate.ListItemCollectionPosition = new SPListItemCollectionPosition("Paged=TRUE&p_ID=" + parameters.PagingInfo.LastItemIdOfLastPage.ToString());
                }

                if (CAMLquery == null && filterExpression == null)
                    CAMLquery = "<Where></Where>";

                usePaging = true;
            }

            if (context.ListItemCollection != null)
            {
                var itemCollection = _repositoryManager.GetDataItems(context.ListItemCollection, context.GetRequiredFieldsForRead(), CreateFileOperationParameters(context));

                ret = new SPGENEntityCollection<TEntity>(itemCollection, context);
            }
            else if (context.List != null && CAMLquery == null && filterExpression == null)
            {
                string[] reqFieldNames = context.GetRequiredFieldsForRead();

                var itemCollection = _repositoryManager.GetDataItems(context.List.GetItems(reqFieldNames), reqFieldNames, CreateFileOperationParameters(context));

                ret = new SPGENEntityCollection<TEntity>(itemCollection, context);
            }
            else if (CAMLquery != null)
            {
                SPQuery query = (parameters == null) ? new SPQuery() : parameters.SPQueryTemplate;
                query.Query = CAMLquery;

                context.ListQuery = query;

                var coll = ExecuteListQueryEnsureRequiredFields(context, query);

                ret = new SPGENEntityCollection<TEntity>(coll, context);
            }
            else if (filterExpression != null)
            {
                var ql = new SPGENLinqQueryableList<TEntity>(context);
                var result = ql.Where<TEntity>(filterExpression);

                ret = result.GetEnumerator() as SPGENEntityCollection<TEntity>;
            }
            else
            {
                throw new NotSupportedException();
            }

            if (usePaging)
            {
                if (ret.ListItemCollection.ListItemCollectionPosition != null)
                {
                    string s = ret.ListItemCollection.ListItemCollectionPosition.PagingInfo;
                    var nvc = System.Web.HttpUtility.ParseQueryString(s);

                    parameters.PagingInfo.LastItemIdOfCurrentPage = int.Parse(nvc["p_ID"]);
                }
                else
                {
                    parameters.PagingInfo.LastItemIdOfCurrentPage = new Int32();
                }
            }

            return ret;
        }

        private SPGENRepositoryDataItemCollection ExecuteListQueryEnsureRequiredFields(SPGENEntityOperationContext<TEntity> context, SPQuery query)
        {
            string[] reqFieldNames = CalculateRequiredFieldsFromViewFields(query.ViewFields, context.GetRequiredFieldsForRead());

            if (string.IsNullOrEmpty(query.ViewFields) && !context.ShouldIncludeAllFields)
            {
                query.ViewFields = GetViewFieldsCollection(reqFieldNames, false);
                query.ViewFieldsOnly = !context.UseEntityState;
            }

            return _repositoryManager.FindDataItems(context.List, query, reqFieldNames, CreateFileOperationParameters(context));
        }

        private SPGENRepositoryDataItemCollection ExecuteSiteDataQueryEnsureRequiredFields(SPWeb web, SPGENEntityOperationContext<TEntity> context, SPSiteDataQuery query)
        {
            string[] reqFieldNames = CalculateRequiredFieldsFromViewFields(query.ViewFields, context.GetRequiredFieldsForRead());

            if (string.IsNullOrEmpty(query.ViewFields) && !context.ShouldIncludeAllFields)
            {
                query.ViewFields = GetViewFieldsCollection(reqFieldNames, true);
            }

            return _repositoryManager.FindDataItems(web, query, reqFieldNames);
        }

        private string[] CalculateRequiredFieldsFromViewFields(string viewFields, string[] reqFields)
        {
            if (string.IsNullOrEmpty(viewFields))
                return reqFields;

            List<string> ret = new List<string>();
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml("<FieldRefs>" + viewFields + "</FieldRefs>");

            foreach (XmlElement el in xmldoc.DocumentElement.ChildNodes)
            {
                string fieldName = el.GetAttribute("Name");

                ret.Add(fieldName);
            }

            return ret.ToArray();
        }

        private static string GetViewFieldsCollection(string[] fieldNames, bool nullable)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string f in fieldNames)
            {
                if (nullable)
                {
                    sb.Append(string.Format(@"<FieldRef Name=""{0}"" Nullable=""TRUE""/>", f));
                }
                else
                {
                    sb.Append(string.Format(@"<FieldRef Name=""{0}""/>", f));
                }
            }

            return sb.ToString();
        }

        private class IdentifierInfo
        {
            public int ItemId;
            public object CustomId;

            private IdentifierInfo()
            {
            }

            public static IdentifierInfo CreateFromContext(SPGENEntityOperationContext<TEntity> context)
            {
                var ret = new IdentifierInfo();
                var info = context.EntityMap.GetIdentifierValue(context);
                if (info is SPGENEntityCustomItemIdentifierInfo)
                {
                    ret.CustomId = (info as SPGENEntityCustomItemIdentifierInfo).CustomId;
                }
                else
                {
                    ret.ItemId = (info as SPGENEntityBuiltInItemIdIdentifierInfo).ItemId;
                }
                return ret;
            }

            public static IdentifierInfo CreateIntId(int id, SPGENEntityMap<TEntity> map)
            {
                var ret = new IdentifierInfo();
                if (map.HasCustomId)
                {
                    ret.CustomId = id;
                }
                else
                {
                    ret.ItemId = id;
                }
                return ret;
            }

            public static IdentifierInfo CreateCustomId(object customId, SPGENEntityMap<TEntity> map)
            {
                var ret = new IdentifierInfo();
                if (map.HasCustomId)
                {
                    ret.CustomId = customId;
                }
                else
                {
                    if (customId is int)
                        ret.ItemId = (int)customId;
                    else
                        throw new ArgumentException("The identifier value must be an Int32.");
                }
                return ret;
            }

            public bool IsCustomId { get { return this.CustomId != null; } }
        }

        #endregion
    }
}
