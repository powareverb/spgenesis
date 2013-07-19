using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Entities.Extensions
{
    public static class SPGENEntityListItemExtensions
    {
        public static TEntity ConvertToEntity<TEntity>(this SPListItem listItem)
            where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntity(listItem);
        }

        public static TEntity ConvertToEntityAsUpdatable<TEntity>(this SPListItem listItem)
            where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntityAsUpdatable(listItem);
        }

        public static TEntity ConvertToEntityWithFiles<TEntity>(this SPListItem listItem)
            where TEntity : class
        {
            return SPGENEntityManager<TEntity>.Instance.ConvertToEntityWithFiles(listItem);
        }
    }
}
