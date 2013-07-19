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
using System.IO;

namespace SPGenesis.Entities
{
    public abstract class SPGENEntityManagerBase<TEntity, TMapperBase>
        where TEntity : class
        where TMapperBase : SPGENEntityMapBase<TEntity>
    {

        /// <summary>
        /// Fires when the mapper type needs to be resolved.
        /// </summary>
        /// <returns></returns>
        protected abstract Type ResolveEntityMapperType();

        /// <summary>
        /// Gets an instance of the entity mapper.
        /// </summary>
        /// <returns></returns>
        public virtual TMapperBase GetMapperInstance()
        {
            try
            {
                var t = ResolveEntityMapperType();
                var result = Activator.CreateInstance(t) as TMapperBase;

                if (result == null)
                    throw new SPGENEntityGeneralException("The mapper type is incompatible with this manager instance.");

                return result;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is SPGENEntityMapInitializationException)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }
    }
}
