using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Entities
{
    public static class SPGENEntityListExtensions
    {
        public static IEnumerable<TEntity> ConvertToEntities<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntities(list);
        }

        public static IEnumerable<TEntity> ConvertToEntitiesWithFiles<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntitiesWithFiles(list);
        }

        public static IEnumerable<TEntity> ConvertToEntitiesAsUpdatable<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntitiesAsUpdatable(list);
        }

        public static IEnumerable<TEntity> ConvertToEntitiesWithFilesAsUpdatable<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntitiesWithFilesAsUpdatable(list);
        }


        public static IEnumerable<TEntity> GetEntities<TEntity>(this SPList list, string CAMLQuery) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntities(list, CAMLQuery);
        }

        public static IEnumerable<TEntity> GetEntities<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntities(list, filterExpression);
        }

        public static IEnumerable<TEntity> GetEntitiesWithFiles<TEntity>(this SPList list, string CAMLQuery) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesWithFiles(list, CAMLQuery);
        }

        public static IEnumerable<TEntity> GetEntitiesWithFiles<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesWithFiles(list, filterExpression);
        }

        public static IEnumerable<TEntity> GetEntitiesAsUpdatable<TEntity>(this SPList list, string CAMLQuery) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesAsUpdatable(list, CAMLQuery);
        }

        public static IEnumerable<TEntity> GetEntitiesAsUpdatable<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesAsUpdatable(list, filterExpression);
        }

        public static IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable<TEntity>(this SPList list, string CAMLQuery) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesWithFilesAsUpdatable(list, CAMLQuery);
        }

        public static IEnumerable<TEntity> GetEntitiesWithFilesAsUpdatable<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntitiesWithFilesAsUpdatable(list, filterExpression);
        }


        public static TEntity GetEntity<TEntity>(this SPList list, int listItemId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntity(list, listItemId);
        }

        public static TEntity GetEntity<TEntity, TIdentifier>(this SPList list, TIdentifier customId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntity<TIdentifier>(list, customId);
        }


        public static TEntity GetEntityWithFiles<TEntity>(this SPList list, int listItemId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityWithFiles(list, listItemId);
        }

        public static TEntity GetEntityWithFiles<TEntity, TIdentifier>(this SPList list, TIdentifier customId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityWithFiles<TIdentifier>(list, customId);
        }


        public static TEntity GetEntityAsUpdatable<TEntity>(this SPList list, int listItemId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityAsUpdatable(list, listItemId);
        }

        public static TEntity GetEntityAsUpdatable<TEntity, TIdentifier>(this SPList list, TIdentifier customId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityAsUpdatable<TIdentifier>(list, customId);
        }


        public static TEntity GetEntityWithFilesAsUpdatable<TEntity>(this SPList list, int listItemId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityWithFilesAsUpdatable(list, listItemId);
        }

        public static TEntity GetEntityWithFilesAsUpdatable<TEntity, TIdentifier>(this SPList list, TIdentifier customId) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetEntityWithFilesAsUpdatable<TIdentifier>(list, customId);
        }


        public static void UpdateListItem<TEntity>(this SPList list, TEntity entity) where TEntity : class
        {
            SPGENEntityManager<TEntity>.Instance.UpdateListItem(entity, list);
        }

        public static void UpdateListItemWithFiles<TEntity>(this SPList list, TEntity entity) where TEntity : class
        {
            SPGENEntityManager<TEntity>.Instance.UpdateListItemWithFiles(entity, list);
        }


        public static SPListItem CreateNewListItem<TEntity>(this SPList list, TEntity entity) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.CreateNewListItem(entity, list);
        }

        public static SPListItem CreateNewListItemWithFiles<TEntity>(this SPList list, TEntity entity) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.CreateNewListItemWithFiles(entity, list);
        }


        public static Linq.SPGENLinqQueryableList<TEntity> GetQueryableList<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetQueryableList(list);
        }

        public static Linq.SPGENLinqQueryableList<TEntity> GetQueryableList<TEntity>(this SPList list, bool makeEntitiesUpdatable) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetQueryableList(list, makeEntitiesUpdatable);
        }

        public static Linq.SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles<TEntity>(this SPList list) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetQueryableListWithFiles(list);
        }

        public static Linq.SPGENLinqQueryableList<TEntity> GetQueryableListWithFiles<TEntity>(this SPList list, bool makeEntitiesUpdatable) where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.GetQueryableListWithFiles(list, makeEntitiesUpdatable);
        }



        [Obsolete("Not longer in use.", true)]
        public static Linq.SPGENLinqQueryableList<TEntity> GetQueryableList<TEntity>(this SPList list, bool enableUpdates, SPGENEntityOperationParameters<TEntity> parameters) where TEntity : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Use ConvertToEntities<TEntity>(this SPList list) instead.", true)]
        public static IEnumerable<TEntity> GetEntities<TEntity>(this SPList list) where TEntity : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Use GetEntities<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression).ToList() instead.", true)]
        public static IList<TEntity> GetEntitiesAsList<TEntity>(this SPList list, System.Linq.Expressions.Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Use GetEntities<TEntity>(this SPList list, string CAML).ToList() instead.", true)]
        public static IList<TEntity> GetEntitiesAsList<TEntity>(this SPList list, string CAML) where TEntity : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Use GetEntities<TEntity>(this SPList list, SPQuery query).ToList() instead.", true)]
        public static IEnumerable<TEntity> GetEntitiesAsList<TEntity>(this SPList list, SPQuery query) where TEntity : class
        {
            throw new NotSupportedException();
        }

        [Obsolete("Not longer in use. Use GetEntities<TEntity>(this SPList list).ToList() instead.", true)]
        public static IList<TEntity> GetEntitiesAsList<TEntity>(this SPList list) where TEntity : class
        {
            throw new NotSupportedException();
        }
    }
}
