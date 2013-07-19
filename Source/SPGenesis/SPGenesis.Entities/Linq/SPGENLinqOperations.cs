using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace SPGenesis.Entities.Linq
{
    public static class SPGENLinqOperations
    {
        public static TResult FieldRef<TResult>(string fieldName)
        {
            return default(TResult);
        }

        public static bool ContainsValues<TEntity>(Expression<Func<TEntity, object>> entityProperty, params object[] values)
        {
            return false;
        }

        [Obsolete("Not longer in use. Use ContainsValues instead.", true)]
        public static bool HasValues<T>(T entityProperty, params object[] values)
        {
            return false;
        }
    }
}
