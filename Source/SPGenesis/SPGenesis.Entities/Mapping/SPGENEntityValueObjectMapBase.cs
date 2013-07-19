using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SPGenesis.Entities.Linq;
using System.Linq.Expressions;

namespace SPGenesis.Entities
{
    public abstract class SPGENEntityValueObjectMapBase<TValueObject> : SPGENEntityValueObjectMapBase
        where TValueObject : class
    {
        protected abstract TValueObject CreateValueObject<TEntity>(SPGENEntityOperationContext<TEntity> context) where TEntity : class;
        internal abstract TValueObject ToValueObject<TEntity>(object fieldValue, string fieldName, SPGENEntityOperationContext<TEntity> context) where TEntity : class;
        internal abstract IEnumerable<TValueObject> ToValueObjects<TEntity>(object fieldValue, string fieldName, SPGENEntityOperationContext<TEntity> context) where TEntity : class;
        internal abstract object ToFieldValue<TEntity>(TValueObject valueObject, string fieldName, SPGENEntityOperationContext<TEntity> context) where TEntity : class;
        internal abstract object ToFieldValue<TEntity>(IEnumerable<TValueObject> valueObjects, string fieldName, SPGENEntityOperationContext<TEntity> context) where TEntity : class;
    }

    public abstract class SPGENEntityValueObjectMapBase : ISPGENEntityValueObjectMapBase
    {
        internal abstract HashSet<PropertyInfo> GetQueryableProperties();
        internal abstract PropertyInfo IdentifierProperty { get; }
        internal abstract PropertyInfo ValueProperty { get; }
        public abstract Type GetFieldValueType();
        public abstract SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args);
        public abstract SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args);
    }

    interface ISPGENEntityValueObjectMapBase
    {
    }
}
