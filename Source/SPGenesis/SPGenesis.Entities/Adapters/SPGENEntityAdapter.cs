using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities.Adapters
{
    public abstract class SPGENEntityAdapter<TEntity, TPropertyValue> : Linq.Adapters.SPGENEntityLinqAdapter, ISPGENEntityAdapter<TEntity>
        where TEntity : class
    {
        private bool _propertyValueIsValueType;
        private bool _isNullable;

        public SPGENEntityAdapter()
        {
            _propertyValueIsValueType = typeof(TPropertyValue).IsValueType;

            if (typeof(TPropertyValue).IsGenericType)
            {
                if (typeof(TPropertyValue).GetGenericTypeDefinition() == typeof(Nullable<>))
                    _isNullable = true;
            }
        }

        public bool AutoNullCheck { get; set; }

        internal TPropertyValue InvokeConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (this.AutoNullCheck && arguments.Value == null)
            {
                return default(TPropertyValue);
            }
            else
            {
                return ConvertToPropertyValue(arguments);
            }
        }

        internal object InvokeConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments)
        {
            if (!_propertyValueIsValueType || _isNullable)
            {
                if (this.AutoNullCheck && arguments.Value == null)
                {
                    return null;
                }
            }

            return ConvertToListItemValue(arguments);
        }

        public abstract TPropertyValue ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments);
        public abstract object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments);


        object ISPGENEntityAdapter<TEntity>.InvokeConvertToPropertyValueGeneric(object arguments)
        {
            return ConvertToPropertyValue((SPGENEntityAdapterConvArgs<TEntity, object>)arguments);
        }

        object ISPGENEntityAdapter<TEntity>.InvokeConvertToListItemValueGeneric(object arguments)
        {
            return ConvertToListItemValue((SPGENEntityAdapterConvArgs<TEntity, TPropertyValue>)arguments);
        }
    }

    interface ISPGENEntityAdapter<TEntity>
        where TEntity : class
    {
        object InvokeConvertToPropertyValueGeneric(object arguments);
        object InvokeConvertToListItemValueGeneric(object arguments);
    }
}
