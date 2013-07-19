using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Linq.Expressions;
using SPGenesis.Entities.Repository;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities
{
    public partial class SPGENEntityManagerFoundationBase<TEntity>
        where TEntity : class
    {
        #region ConvertToEntityWithFiles

        /// <summary>
        /// Converts a list item to an entity with file contents.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <returns></returns>
        public TEntity ConvertToEntityWithFiles(SPListItem listItem)
        {
            return ConvertToEntityWithFiles(listItem, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts a list item to an entity with file contents.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TEntity ConvertToEntityWithFiles(SPListItem listItem, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntity(listItem, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region ConvertToEntityWithFilesAsUpdatable

        /// <summary>
        /// Converts a SPListItem to an updatable entity with file contents. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <returns></returns>
        public TEntity ConvertToEntityWithFilesAsUpdatable(SPListItem listItem)
        {
            return ConvertToEntityWithFilesAsUpdatable(listItem, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts a SPListItem to an updatable entity with file contents. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="listItem">The list item to convert.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public TEntity ConvertToEntityWithFilesAsUpdatable(SPListItem listItem, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntityAsUpdatable(listItem, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region ConvertToEntitiesWithFiles

        /// <summary>
        /// Converts an entire list to entities with file contents.
        /// </summary>
        /// <param name="list">The list instance to convert.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFiles(SPList list)
        {
            return ConvertToEntitiesWithFiles(list, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts an entire list to entities with file contents.
        /// </summary>
        /// <param name="list">The list instance to convert.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFiles(SPList list, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntities(list, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Converts a list item collection to entities with file contents.
        /// </summary>
        /// <param name="itemCollection">The list item collection</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFiles(SPListItemCollection itemCollection)
        {
            return ConvertToEntitiesWithFiles(itemCollection, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts a list item collection to entities with file contents.
        /// </summary>
        /// <param name="itemCollection">The list item collection</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFiles(SPListItemCollection itemCollection, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntities(itemCollection, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region ConvertToEntitiesWithFilesAsUpdatable

        /// <summary>
        /// Converts all items in a SPList to updatable entities with file contents. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The SPList instance.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFilesAsUpdatable(SPList list)
        {
            return ConvertToEntitiesWithFilesAsUpdatable(list, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts all items in a SPList to updatable entities with file contents. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The SPList instance.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFilesAsUpdatable(SPList list, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntitiesAsUpdatable(list, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Converts all items in a SPListItemCollection to updatable entities with file contents. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="itemCollection">The SPListItemCollection instance.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFilesAsUpdatable(SPListItemCollection itemCollection)
        {
            return ConvertToEntitiesWithFilesAsUpdatable(itemCollection, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Converts all items in a SPListItemCollection to updatable entities with file contents. The entities can then be updated without the overhead of refetching it from the list store if they support storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="itemCollection">The SPListItemCollection instance.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ConvertToEntitiesWithFilesAsUpdatable(SPListItemCollection itemCollection, SPOpenBinaryOptions options)
        {
            return this.ConvertToEntitiesAsUpdatable(itemCollection, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region GetEntityWithFiles

        /// <summary>
        /// Gets an entity with file contents from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles(SPList list, int listItemId)
        {
            return GetEntityWithFiles(list, listItemId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntityWithFiles<TEntityId>(list, customId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="listItem">The list item fetched dureing this operation.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles(SPList list, int listItemId, out SPListItem listItem)
        {
            return GetEntityWithFiles(list, listItemId, SPOpenBinaryOptions.None, out listItem);
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles<TEntityId>(SPList list, TEntityId customId, out SPListItem listItem)
        {
            return GetEntityWithFiles<TEntityId>(list, customId, SPOpenBinaryOptions.None, out listItem);
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles(SPList list, int listItemId, SPOpenBinaryOptions options)
        {
            return this.GetEntity(list, listItemId, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles<TEntityId>(SPList list, TEntityId customId, SPOpenBinaryOptions options)
        {
            return this.GetEntity<TEntityId>(list, customId, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified item ID.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles(SPList list, int listItemId, SPOpenBinaryOptions options, out SPListItem listItem)
        {
            return this.GetEntity(list, listItemId, CreateParametersForIncludeFiles(options), out listItem);
        }

        /// <summary>
        /// Gets an entity with file contents from a list with the specified custom ID.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <param name="listItem">The list item fetched during this operation.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFiles<TEntityId>(SPList list, TEntityId customId, SPOpenBinaryOptions options, out SPListItem listItem)
        {
            return this.GetEntity<TEntityId>(list, customId, CreateParametersForIncludeFiles(options), out listItem);
        }

        #endregion


        #region GetEntityWithFilesAsUpdatable

        /// <summary>
        /// Gets an updatable entity with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesAsUpdatable(SPList list, int listItemId)
        {
            return GetEntityWithFilesAsUpdatable(list, listItemId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an updatable entity with file contents by custom ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesAsUpdatable<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntityWithFilesAsUpdatable<TEntityId>(list, customId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an updatable entity with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="listItemId">The list item ID.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesAsUpdatable(SPList list, int listItemId, SPOpenBinaryOptions options)
        {
            return this.GetEntityAsUpdatable(list, listItemId, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an updatable entity with file contents by custom ID. The entity can then be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesAsUpdatable<TEntityId>(SPList list, TEntityId customId, SPOpenBinaryOptions options)
        {
            return this.GetEntityAsUpdatable<TEntityId>(list, customId, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region GetEntityWithFilesAsUpdatableWrapper

        /// <summary>
        /// Gets an updatable entity wrapper with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listItemId"></param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityWithFilesAsUpdatableWrapper(SPList list, int listItemId)
        {
            return GetEntityWithFilesAsUpdatableWrapper(list, listItemId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an updatable entity wrapper with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityWithFilesAsUpdatableWrapper<TEntityId>(SPList list, TEntityId customId)
        {
            return GetEntityWithFilesAsUpdatableWrapper<TEntityId>(list, customId, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an updatable entity wrapper with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listItemId"></param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityWithFilesAsUpdatableWrapper(SPList list, int listItemId, SPOpenBinaryOptions options)
        {
            return this.GetEntityAsUpdatableWrapper(list, listItemId, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an updatable entity wrapper with file contents by list item ID. The entity can then be updated without the overhead of refetching it from the list store. Don't use this method if you only need read-only entity.
        /// </summary>
        /// <typeparam name="TEntityId">Type of custom ID.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="customId">The custom ID to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public SPGENEntityUpdatableWrapper<TEntity> GetEntityWithFilesAsUpdatableWrapper<TEntityId>(SPList list, TEntityId customId, SPOpenBinaryOptions options)
        {
            return this.GetEntityAsUpdatableWrapper<TEntityId>(list, customId, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region GetEntityWithFilesFromEventProperties

        /// <summary>
        /// Gets an entity with file contents based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType)
        {
            return GetEntityWithFilesFromEventProperties(eventProperties, collectionType, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an entity with file contents based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, bool useListItemValueIfNotFound)
        {
            return GetEntityWithFilesFromEventProperties(eventProperties, collectionType, SPOpenBinaryOptions.None, useListItemValueIfNotFound);
        }

        /// <summary>
        /// Gets an entity with file contents based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPOpenBinaryOptions options)
        {
            return this.GetEntityFromEventProperties(eventProperties, collectionType, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an entity with file contents based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPOpenBinaryOptions options, bool useListItemValueIfNotFound)
        {
            return this.GetEntityFromEventProperties(eventProperties, collectionType, CreateParametersForIncludeFiles(options), useListItemValueIfNotFound);
        }

        /// <summary>
        /// Gets an entity with file contents based on list item event properties.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="collectionType">Specifies the type of property collection (before/after).</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <param name="useListItemValueIfNotFound">Uses list item values if they are not found in the event properties collection.</param>
        /// <param name="valuesFromEventProperties">Outputs the values found in the event properties.</param>
        /// <returns></returns>
        public TEntity GetEntityWithFilesFromEventProperties(SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPOpenBinaryOptions options, bool useListItemValueIfNotFound, out IDictionary<string, object> valuesFromEventProperties)
        {
            return this.GetEntityFromEventProperties(eventProperties, collectionType, CreateParametersForIncludeFiles(options), useListItemValueIfNotFound, out valuesFromEventProperties);
        }

        #endregion


        #region GetEntitiesWithFiles

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="query">The query object to use.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, SPQuery query)
        {
            return GetEntitiesWithFiles(list, query, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="query">The query object to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, SPQuery query, SPOpenBinaryOptions options)
        {
            return this.GetEntities(list, query, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLQuery">The CAML query to use.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, string CAMLQuery)
        {
            return GetEntitiesWithFiles(list, CAMLQuery, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLQuery">The CAML query to use.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, string CAMLQuery, SPOpenBinaryOptions options)
        {
            return this.GetEntities(list, CAMLQuery, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntitiesWithFiles(list, filterExpression, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of entities with file contents based on a query against a list.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFiles(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPOpenBinaryOptions options)
        {
            return this.GetEntities(list, filterExpression, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region GetEntitiesWithFilesAsUpdatable

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a query. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable(SPList list, string CAMLQuery)
        {
            return GetEntitiesWithFilesAsUpdatable(list, CAMLQuery, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a query. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable(SPList list, string CAMLQuery, SPOpenBinaryOptions options)
        {
            return this.GetEntitiesAsUpdatable(list, CAMLQuery, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a filter expression. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntitiesWithFilesAsUpdatable(list, filterExpression, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a filter expression. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPOpenBinaryOptions options)
        {
            return this.GetEntitiesAsUpdatable(list, filterExpression, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region GetEntitiesWithFilesAsUpdatableWrapper

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers with file contents based on a query. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesWithFilesAsUpdatableWrapper(SPList list, string CAMLQuery)
        {
            return GetEntitiesWithFilesAsUpdatableWrapper(list, CAMLQuery, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entity wrappers based on a query. The entities can then be updated without the overhead of refetching them from the list store. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="CAMLquery">The CAML-query as string.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesWithFilesAsUpdatableWrapper(SPList list, string CAMLQuery, SPOpenBinaryOptions options)
        {
            return this.GetEntitiesAsUpdatableWrapper(list, CAMLQuery, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a filter expression. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesWithFilesAsUpdatableWrapper(SPList list, Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetEntitiesWithFilesAsUpdatableWrapper(list, filterExpression, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets an enumerable collection of updatable entities with file contents based on a filter expression. Each entity can be updated without the overhead of refetching it from the list store if it supports storing of state. Don't use this method if you only need read-only entities.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <param name="filterExpression">The expression to use. Syntax: MyEntity.Title == "foo"</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public IEnumerable<SPGENEntityUpdatableWrapper<TEntity>> GetEntitiesWithFilesAsUpdatableWrapper(SPList list, Expression<Func<TEntity, bool>> filterExpression, SPOpenBinaryOptions options)
        {
            return this.GetEntitiesAsUpdatableWrapper(list, filterExpression, CreateParametersForIncludeFiles(options));
        }

        #endregion


        #region UpdateListItemWithFiles

        /// <summary>
        /// Updates a list item and file contents with the specified entity. The entity should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        public void UpdateListItemWithFiles(TEntity updatableEntity)
        {
            UpdateListItemWithFiles(updatableEntity, saveParameters: null);
        }

        /// <summary>
        /// Updates a list item and file contents with the specified entity. The entity should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void UpdateListItemWithFiles(TEntity updatableEntity, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.UpdateListItem(updatableEntity, p);
        }

        /// <summary>
        /// Updates a list item and file contents in the specified list with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPList list)
        {
            UpdateListItemWithFiles(entity, list, saveParameters: null);
        }

        /// <summary>
        /// Updates a list item and file contents in the specified list with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPList list, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.UpdateListItem(entity, list, p);
        }

        /// <summary>
        /// Updates a list item and file contents in the specified list item collection with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="itemCollection">The list item collection.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPListItemCollection itemCollection)
        {
            UpdateListItemWithFiles(entity, itemCollection, saveParameters: null);
        }

        /// <summary>
        /// Updates a list item and file contents in the specified list item collection with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPListItemCollection itemCollection, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.UpdateListItem(entity, itemCollection, p);
        }

        /// <summary>
        /// Updates a list item and file contents with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPListItem listItem)
        {
            UpdateListItemWithFiles(entity, listItem, saveParameters: null);
        }

        /// <summary>
        /// Updates a list item and file contents with the specified entity using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void UpdateListItemWithFiles(TEntity entity, SPListItem listItem, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.UpdateListItem(entity, listItem, p);
        }

        #endregion


        #region BatchUpdateListItemWithFiles

        /// <summary>
        /// Updates multiple list items and file contents with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> updatableEntities)
        {
            BatchUpdateListItemWithFiles(updatableEntities, saveParameters: null, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items and file contents with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> updatableEntities, SPFileSaveBinaryParameters saveParameters)
        {
            BatchUpdateListItemWithFiles(updatableEntities, saveParameters: saveParameters, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items and file contents with the specified collection of updatable entities. The entities should be fetched as updatable prior to execute this method.
        /// </summary>
        /// <param name="updatableEntities">The collection of updatable entities.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> updatableEntities, SPFileSaveBinaryParameters saveParameters, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.BatchUpdateListItems(updatableEntities, beforeItemUpdateFunction, afterItemUpdateFunction, p);
        }

        /// <summary>
        /// Updates multiple list items and file contents in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPList list)
        {
            BatchUpdateListItemWithFiles(entities, list, saveParameters: null, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items and file contents in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPList list, SPFileSaveBinaryParameters saveParameters)
        {
            BatchUpdateListItemWithFiles(entities, list, saveParameters: saveParameters, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items and file contents in the specified list with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPList list, SPFileSaveBinaryParameters saveParameters, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.BatchUpdateListItems(entities, list, beforeItemUpdateFunction, afterItemUpdateFunction, p);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPListItemCollection itemCollection)
        {
            BatchUpdateListItemWithFiles(entities, itemCollection, saveParameters: null, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, SPFileSaveBinaryParameters saveParameters)
        {
            BatchUpdateListItemWithFiles(entities, itemCollection, saveParameters: saveParameters, beforeItemUpdateFunction: null, afterItemUpdateFunction: null);
        }

        /// <summary>
        /// Updates multiple list items in the specified list item collection with the specified collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="itemCollection">The list item collection.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        public void BatchUpdateListItemWithFiles(IEnumerable<TEntity> entities, SPListItemCollection itemCollection, SPFileSaveBinaryParameters saveParameters, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            this.BatchUpdateListItems(entities, itemCollection, beforeItemUpdateFunction, afterItemUpdateFunction, p);
        }

        #endregion


        #region CreateNewListItemWithFiles

        /// <summary>
        /// Creates a new list item with file contents in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItemWithFiles(TEntity entity, SPList list)
        {
            return CreateNewListItemWithFiles(entity, list, createFileParameters: null);
        }

        /// <summary>
        /// Creates a new list item with file contents in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="createFileParameters">Use parameters when files are created.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItemWithFiles(TEntity entity, SPList list, SPFileCollectionAddParameters createFileParameters)
        {
            var p = CreateParametersForIncludeFiles(null, null, createFileParameters);
            return this.CreateNewListItem(entity, list, p);
        }

        /// <summary>
        /// Creates a new list item with file contents in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItemWithFiles(TEntity entity, SPList list, string parentFolderRelUrl)
        {
            return CreateNewListItemWithFiles(entity, list, parentFolderRelUrl, createFileParameters: null);
        }

        /// <summary>
        /// Creates a new list item with file contents in the specified list using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="createFileParameters">Use parameters when files are created.</param>
        /// <returns></returns>
        public SPListItem CreateNewListItemWithFiles(TEntity entity, SPList list, string parentFolderRelUrl, SPFileCollectionAddParameters createFileParameters)
        {
            var p = CreateParametersForIncludeFiles(null, null, createFileParameters);
            return this.CreateNewListItem(entity, list, parentFolderRelUrl, p);
        }

        #endregion


        #region BatchCreateNewListItemsWithFiles

        /// <summary>
        /// Creates multiple list items with file contents on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        public void BatchCreateNewListItemsWithFiles(IEnumerable<TEntity> entities, SPList list)
        {
            BatchCreateNewListItemsWithFiles(entities, list, createFileParameters: null);
        }

        /// <summary>
        /// Creates multiple list items with file contents on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="createFileParameters">Use parameters when files are created.</param>
        public void BatchCreateNewListItemsWithFiles(IEnumerable<TEntity> entities, SPList list, SPFileCollectionAddParameters createFileParameters)
        {
            var p = CreateParametersForIncludeFiles(null, null, createFileParameters);
            this.BatchCreateNewListItems(entities, list, p);
        }

        /// <summary>
        /// Creates multiple list items with file contents on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        public void BatchCreateNewListItemsWithFiles(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl)
        {
            this.BatchCreateNewListItemsWithFiles(entities, list, parentFolderRelUrl, null, null, null);
        }

        /// <summary>
        /// Creates multiple list items with file contents on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="createFileParameters">Use parameters when files are created.</param>
        public void BatchCreateNewListItems(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl, SPFileCollectionAddParameters createFileParameters)
        {
            BatchCreateNewListItemsWithFiles(entities, list, parentFolderRelUrl, null, null, createFileParameters);
        }

        /// <summary>
        /// Creates multiple list items with file contents on the specified list using a collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="parentFolderRelUrl">The parent folder url relative to the list root folder.</param>
        /// <param name="beforeItemUpdateFunction">Function to execute before each item is updated.</param>
        /// <param name="afterItemUpdateFunction">Function to execute after each item is updated.</param>
        /// <param name="createFileParameters">Use parameters when files are created.</param>
        public void BatchCreateNewListItemsWithFiles(IEnumerable<TEntity> entities, SPList list, string parentFolderRelUrl, Action<SPGENEntityOperationContext<TEntity>> beforeItemUpdateFunction, Action<SPGENEntityOperationContext<TEntity>> afterItemUpdateFunction, SPFileCollectionAddParameters createFileParameters)
        {
            var p = CreateParametersForIncludeFiles(null, null, createFileParameters);
            this.BatchCreateNewListItems(entities, list, parentFolderRelUrl, beforeItemUpdateFunction, afterItemUpdateFunction, p);
        }

        #endregion


        #region Save files

        /// <summary>
        /// Saves attachments and item data using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        public void SaveAttachments(TEntity entity, SPListItem listItem)
        {
            SaveAttachments(entity, listItem, saveParameters: null);
        }

        /// <summary>
        /// Saves attachments and item data using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void SaveAttachments(TEntity entity, SPListItem listItem, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, p);

            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());
            context.DataItem.ListItem = listItem;

            PopulateRepositoryItem(context, false);

            if (context.CancelOperation)
                return;

            EnsureAttachmentFiles(context, false);

            PopulateEntityInternal(context);
        }

        /// <summary>
        /// Saves file and item data using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        public void SaveFile(TEntity entity, SPListItem listItem)
        {
            SaveFile(entity, listItem, saveParameters: null);
        }

        /// <summary>
        /// Saves file and item data using the specified entity.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="listItem">The list item instance.</param>
        /// <param name="saveParameters">Use parameters when files are saved.</param>
        public void SaveFile(TEntity entity, SPListItem listItem, SPFileSaveBinaryParameters saveParameters)
        {
            var p = CreateParametersForIncludeFiles(null, saveParameters, null);
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,listItem.ParentList, p);

            if (context.EntityMap.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
                throw new SPGENEntityGeneralException("Mapping of files are in read only when using byte arrays.");

            context.Entity = entity;
            context.DataItem = new SPGENRepositoryDataItem(context.GetRequiredFieldsForWrite());

            PopulateRepositoryItem(context, false);

            if (context.CancelOperation)
                return;

            _repositoryManager.SaveFile(context.DataItem, CreateFileOperationParameters(context));

            PopulateEntityInternal(context);
        }

        #endregion


        #region CheckInCheckOutFile

        /// <summary>
        /// Checks out the file using the specified updatable entity. The entity must be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        public void CheckOutFile(TEntity updatableEntity)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, null);
            context.ListItem.File.CheckOut();
        }

        /// <summary>
        /// Checks out the file using the specified updatable entity. The entity must be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        /// <param name="checkOutType"></param>
        /// <param name="lastModifiedDate"></param>
        /// <param name="ignoreIfAlreadyCheckedOut">Ignores check out if the file is already checked out.</param>
        public void CheckOutFile(TEntity updatableEntity, SPFile.SPCheckOutType checkOutType, string lastModifiedDate, bool ignoreIfAlreadyCheckedOut)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, null);
            if (ignoreIfAlreadyCheckedOut)
            {
                if (context.ListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                    return;
            }

            context.ListItem.File.CheckOut(checkOutType, lastModifiedDate);
        }

        /// <summary>
        /// Checks out the file in the specified list using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        public void CheckOutFile(TEntity entity, SPList list)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            LoadDataItemIntoContext(list, null, IdentifierInfo.CreateFromContext(context), context, true);

            context.ListItem.File.CheckOut();
        }

        /// <summary>
        /// Checks out the file in the specified list using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="checkOutType"></param>
        /// <param name="lastModifiedDate"></param>
        /// <param name="ignoreIfAlreadyCheckedOut">Ignores check out if the file is already checked out.</param>
        public void CheckOutFile(TEntity entity, SPList list, SPFile.SPCheckOutType checkOutType, string lastModifiedDate, bool ignoreIfAlreadyCheckedOut)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            LoadDataItemIntoContext(list, null, IdentifierInfo.CreateFromContext(context), context, true);
            
            if (ignoreIfAlreadyCheckedOut)
            {
                if (context.ListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                    return;
            }

            context.ListItem.File.CheckOut(checkOutType, lastModifiedDate);
        }

        /// <summary>
        /// Checks in the file using the specified updatable entity. The entity must be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        /// <param name="checkInComment"></param>
        public void CheckInFile(TEntity updatableEntity, string checkInComment)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, null);
            context.ListItem.File.CheckIn(checkInComment);
        }

        /// <summary>
        /// Checks in the file using the specified updatable entity. The entity must be fetched as updatable before executing this method.
        /// </summary>
        /// <param name="updatableEntity">The updatable entity instance.</param>
        /// <param name="checkInComment"></param>
        /// <param name="checkInType"></param>
        /// <param name="ignoreIfAlreadyCheckedIn">Ignores check in if the file is already checked in.</param>
        public void CheckInFile(TEntity updatableEntity, string checkInComment, SPCheckinType checkInType, bool ignoreIfAlreadyCheckedIn)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForEntityWithState(this, updatableEntity, null);
            if (ignoreIfAlreadyCheckedIn)
            {
                if (context.ListItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                    return;
            }

            context.ListItem.File.CheckIn(checkInComment, checkInType);
        }

        /// <summary>
        /// Checks in the file in the specified list using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="checkInComment"></param>
        public void CheckInFile(TEntity entity, SPList list, string checkInComment)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            LoadDataItemIntoContext(list, null, IdentifierInfo.CreateFromContext(context), context, true);

            context.ListItem.File.CheckIn(checkInComment);
        }

        /// <summary>
        /// Checks in the file in the specified list using the entity identifier.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="list">The list instance.</param>
        /// <param name="checkInComment"></param>
        /// <param name="checkInType"></param>
        /// <param name="ignoreIfAlreadyCheckedIn">Ignores check in if the file is already checked in.</param>
        public void CheckInFile(TEntity entity, SPList list, string checkInComment, SPCheckinType checkInType, bool ignoreIfAlreadyCheckedIn)
        {
            var context = SPGENEntityOperationContextFactory.CreateContextForListBasedOperations<TEntity>(this,list, null);
            LoadDataItemIntoContext(list, null, IdentifierInfo.CreateFromContext(context), context, true);

            if (ignoreIfAlreadyCheckedIn)
            {
                if (context.ListItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                    return;
            }

            context.ListItem.File.CheckIn(checkInComment, checkInType);
        }

        #endregion


        #region GetQueryableListWithFiles

        /// <summary>
        /// Gets a queryable list with file contents implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles(SPList list)
        {
            return GetQueryableListWithFiles(list, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets a queryable list with file contents implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns>A queryable list.</returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles(SPList list, SPOpenBinaryOptions options)
        {
            return this.GetQueryableList(list, false, CreateParametersForIncludeFiles(options));
        }

        /// <summary>
        /// Gets a queryable list with file contents implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="makeEntitiesUpdatable">Makes the fetched entities updatable.</param>
        /// <returns></returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles(SPList list, bool makeEntitiesUpdatable)
        {
            return GetQueryableListWithFiles(list, makeEntitiesUpdatable, SPOpenBinaryOptions.None);
        }

        /// <summary>
        /// Gets a queryable list with file contents implementing IQueryable for LINQ queries.
        /// </summary>
        /// <param name="list">The SPList instance to base the queryable list on.</param>
        /// <param name="makeEntitiesUpdatable">Makes the fetched entities updatable.</param>
        /// <param name="options">Use options when files are opened.</param>
        /// <returns></returns>
        public SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles(SPList list, bool makeEntitiesUpdatable, SPOpenBinaryOptions options)
        {
            return this.GetQueryableList(list, makeEntitiesUpdatable, CreateParametersForIncludeFiles(options));
        }

        #endregion


        private SPGENEntityOperationParameters CreateParametersForIncludeFiles(SPOpenBinaryOptions options)
        {
            return new SPGENEntityOperationParameters()
            {
                IncludeFiles = true,
                OpenFileParameters = options
            };
        }

        private SPGENEntityOperationParameters CreateParametersForIncludeFiles(SPGENEntityOperationParameters parameters, SPFileSaveBinaryParameters saveFileParams, SPFileCollectionAddParameters addFileParams)
        {
            if (parameters == null)
                parameters = new SPGENEntityOperationParameters();

            parameters.IncludeFiles = true;

            if (saveFileParams != null)
                parameters.SaveFileParameters = saveFileParams;

            if (addFileParams != null)
                parameters.SaveNewFileParameters = addFileParams;

            return parameters;
        }
    }
}
