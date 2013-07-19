using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;

namespace SPGenesis.Entities
{
    internal static class SPGENEntityOperationContextFactory
    {
        public static SPGENEntityOperationContext<TEntity> CreateContextForEntityWithState<TEntity>(SPGENEntityManagerFoundationBase<TEntity> managerInstance, TEntity entity, SPGENEntityOperationParameters parameters)
            where TEntity : class
        {
            var context = new SPGENEntityOperationContext<TEntity>(managerInstance);

            var state = context.EntityMap.GetRepositoryStateFromEntity(entity);
            if (state == null)
                throw new SPGENEntityGeneralException("The entity instance has no state associated with it.");

            context.Parameters = parameters;
            context.Entity = entity;
            context.DataItem = state.DataItem;

            return context;
        }

        public static SPGENEntityOperationContext<TEntity> CreateContextForListBasedOperations<TEntity>(SPGENEntityManagerFoundationBase<TEntity> managerInstance, SPList list, SPGENEntityOperationParameters parameters)
            where TEntity : class
        {
            var context = new SPGENEntityOperationContext<TEntity>(managerInstance);
            context.List = list;
            context.Parameters = parameters;

            CheckIdentifierConstraints(list, context.EntityMap);

            return context;
        }

        public static SPGENEntityOperationContext<TEntity> CreateContextForItemCollectionBasedOperations<TEntity>(SPGENEntityManagerFoundationBase<TEntity> managerInstance, SPListItemCollection listItemCollection, SPGENEntityOperationParameters parameters)
            where TEntity : class
        {
            var context = new SPGENEntityOperationContext<TEntity>(managerInstance);

            context.Parameters = parameters;
            context.List = listItemCollection.List;
            context.ListItemCollection = listItemCollection;

            CheckIdentifierConstraints(listItemCollection.List, context.EntityMap);

            return context;
        }

        public static SPGENEntityOperationContext<TEntity> CreateContextForEventBasedOperations<TEntity>(SPGENEntityManagerFoundationBase<TEntity> managerInstance, SPItemEventProperties eventProperties, SPGENItemEventPropertiesType collectionType, SPGENEntityOperationParameters parameters)
            where TEntity : class
        {
            var context = new SPGENEntityOperationContext<TEntity>(managerInstance);
            context.Parameters = parameters;
            context.List = eventProperties.List;
            context.EventProperties = eventProperties;
            context.EventPropertiesCollectionType = collectionType;

            CheckIdentifierConstraints(eventProperties.List, context.EntityMap);

            return context;
        }

        public static SPGENEntityOperationContext<TEntity> CreateContextSiteDataQueryBasedOperations<TEntity>(SPGENEntityManagerFoundationBase<TEntity> managerInstance, SPWeb web, SPSiteDataQuery query, SPGENEntityOperationParameters parameters)
            where TEntity : class
        {
            var context = new SPGENEntityOperationContext<TEntity>(managerInstance);

            context.Web = web;
            context.SiteDataQuery = query;
            context.Parameters = parameters;

            return context;
        }

        private static void CheckIdentifierConstraints<TEntity>(SPList list, SPGENEntityMap<TEntity> map)
            where TEntity : class
        {
            if (!map.HasIdentifierProperty)
                return;

            if (!map.IdentifierSkipIndexCheck)
            {
                SPField field = list.Fields.GetFieldByInternalName(map.IdentifierFieldName);
                if (!field.Indexed)
                    throw new SPGENEntityGeneralException(string.Format("The identifier for the entity type '{0}' is not indexed.", typeof(TEntity).FullName));
            }

            if (!map.IdentifierSkipEnforceUniqueValueCheck)
            {
                SPField field = list.Fields.GetFieldByInternalName(map.IdentifierFieldName);
                if (!field.EnforceUniqueValues)
                    throw new SPGENEntityGeneralException(string.Format("The identifier for the entity type '{0}' does not enforce unique values.", typeof(TEntity).FullName));
            }
        }

    }
}
