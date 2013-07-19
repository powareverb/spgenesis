using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities
{
    /// <summary>
    /// This manager contains all standard MS SharePoint Foundation based operations for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class SPGENEntityManager<TEntity> : SPGENEntityManagerFoundationBase<TEntity>
        where TEntity : class
    {
        public static SPGENEntityManager<TEntity> Instance = new SPGENEntityManager<TEntity>();

        protected override Type ResolveEntityMapperType()
        {
            Type mapperType = SPGENEntityMapResolver.FindMapper(typeof(TEntity));
            if (mapperType == null)
                throw new SPGENEntityMapNotFoundException(typeof(TEntity));

            return mapperType;
        }
    }

    /// <summary>
    /// This manager contains all standard MS SharePoint Foundation based operations for entities using the specified mapper.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TMapper">The mapper type.</typeparam>
    public class SPGENEntityManager<TEntity, TMapper> : SPGENEntityManagerFoundationBase<TEntity>
        where TEntity : class
    {
        public static SPGENEntityManager<TEntity, TMapper> Instance = new SPGENEntityManager<TEntity, TMapper>();

        protected override Type ResolveEntityMapperType()
        {
            return typeof(TMapper);
        }
    }
}
