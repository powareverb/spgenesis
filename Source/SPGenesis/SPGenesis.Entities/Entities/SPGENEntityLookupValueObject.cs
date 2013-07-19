using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Reflection;

namespace SPGenesis.Entities
{
    public class SPGENEntityLookupValueObject
    {
        public virtual int Id { get; set; }
        public virtual string Value { get; set; }

        public SPGENEntityLookupValueObject()
        {
        }

        public SPGENEntityLookupValueObject(int id, string value)
        {
            this.Id = id;
            this.Value = value;
        }
    }

    public class SPGENEntityLookupValueMap : SPGENEntityLookupValueMap<SPGENEntityLookupValueObject>
    {
        protected override void GetValues(SPGENEntityLookupValueObject lookup, out int lookupId, out string lookupValue)
        {
            lookupId = lookup.Id;
            lookupValue = lookup.Value;
        }

        protected override void SetValues(SPGENEntityLookupValueObject lookup, int lookupId, string lookupValue)
        {
            lookup.Id = lookupId;
            lookup.Value = lookupValue;
        }

        protected override System.Linq.Expressions.Expression<Func<SPGENEntityLookupValueObject, object>> IdentifierProperty
        {
            get { return e => e.Id; }
        }

        protected override System.Linq.Expressions.Expression<Func<SPGENEntityLookupValueObject, object>> ValueProperty
        {
            get { return e => e.Value; }
        }
    }

    public abstract class SPGENEntityLookupValueMap<TValueObject> : SPGENEntityLookupValueMap<TValueObject, SPFieldLookupValue>
        where TValueObject : class
    {
    }

    public abstract class SPGENEntityLookupValueMap<TValueObject, TFieldValue> : SPGENEntityValueObjectMap<TValueObject, TFieldValue>
        where TValueObject : class
        where TFieldValue : SPFieldLookupValue
    {
        private static PropertyInfo _idProperty;
        private static PropertyInfo _valueProperty;
        private static readonly object _idPropertyLock = new object();
        private static readonly object _valuePropertyLock = new object();

        protected abstract void GetValues(TValueObject lookup, out int lookupId, out string lookupValue);
        protected abstract void SetValues(TValueObject lookup, int lookupId, string lookupValue);

        protected abstract System.Linq.Expressions.Expression<Func<TValueObject, object>> IdentifierProperty { get; }
        protected abstract System.Linq.Expressions.Expression<Func<TValueObject, object>> ValueProperty { get; }


        protected sealed override System.Linq.Expressions.Expression<Func<TValueObject, object>> GetIdentifierProperty()
        {
            return this.IdentifierProperty;
        }

        protected sealed override System.Linq.Expressions.Expression<Func<TValueObject, object>> GetValueProperty()
        {
            return this.ValueProperty;
        }

        protected override void ConvertToValueObject<TEntity>(ref TValueObject valueObject, TFieldValue fieldValue, SPGENEntityOperationContext<TEntity> context)
        {
            SetValues(valueObject, fieldValue.LookupId, fieldValue.LookupValue);
        }

        protected override void ConvertToFieldValue<TEntity>(TValueObject valueObject, ref TFieldValue fieldValue, SPGENEntityOperationContext<TEntity> context)
        {
            int id;
            string value;

            GetValues(valueObject, out id, out value);

            fieldValue.LookupId = id;
        }
    }
}
