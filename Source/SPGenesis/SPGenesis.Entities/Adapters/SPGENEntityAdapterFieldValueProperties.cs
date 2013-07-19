using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPGenesis.Core;
using Microsoft.SharePoint;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterFieldValueProperties<TEntity, TFieldValue, TPropertyValue> : SPGENEntityAdapter<TEntity, TPropertyValue>
        where TEntity : class
        where TFieldValue : class
    {
        private SPGENEntityPropertyAccessor<TFieldValue> _fieldValuePropertyAccessor;
        private Func<TFieldValue, TPropertyValue> _fieldValueFunc;
        private SPGENEntityMultiplePropertyMapOptions _options;
        private SPGENEntityLinqAdapter _linqAdapter;
        private TEntity _currentEntity;

        internal SPGENEntityAdapterFieldValueProperties(SPGENEntityPropertyAccessor<TFieldValue> fieldValuePropertyAccessor, Func<TFieldValue, TPropertyValue> fieldValueFunc, SPGENEntityMultiplePropertyMapOptions options)
        {
            _fieldValuePropertyAccessor = fieldValuePropertyAccessor;
            _fieldValueFunc = fieldValueFunc;
            _options = options;
        }

        public override TPropertyValue ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(TPropertyValue);

            TFieldValue value = SPGENCommon.ConvertListItemValue<TFieldValue>(arguments.Web, arguments.Value, arguments.Field is SPFieldCalculated);
            
            return _fieldValueFunc.Invoke(value);
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments)
        {
            if (_options == SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier)
            {
                if (default(TPropertyValue).Equals(arguments.Value))
                {
                    //Set current entity to this entity so we can skip further update of this value instance.
                    _currentEntity = arguments.Entity;

                    return null;
                }
            }


            //Skip setting value if there are more mapped properties to this field value.
            if (_currentEntity == arguments.Entity)
                return null;
            else
                _currentEntity = null;

            object value = arguments.DataItem.FieldValues[arguments.FieldName];
            TFieldValue fieldValue = null;

            if (value == null)
            {
                fieldValue = (TFieldValue)SPGENCommon.ConstructListItemValue(arguments.Web, typeof(TFieldValue));
            }
            else
            {
                if (value.GetType() != typeof(TFieldValue))
                    fieldValue = SPGENCommon.ConvertListItemValue<TFieldValue>(arguments.Web, value, arguments.Field is SPFieldCalculated);
            }

            _fieldValuePropertyAccessor.InvokeSetProperty(fieldValue, arguments.Value);

            return fieldValue;
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            if (_options == SPGENEntityMultiplePropertyMapOptions.None)
                throw new NotSupportedException("The entity property '" + args.SourceProperty.Name + "' does not support linq queries.");

            if (_options == SPGENEntityMultiplePropertyMapOptions.IsIdentifier ||
                _options == SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier)
            {
                args.IsLookupIdProperty = true;
            }
            else
            {
                args.IsLookupIdProperty = false;
            }


            var result = base.EvalComparison(args);

            if (_options == SPGENEntityMultiplePropertyMapOptions.IsUpdatableIdentifier ||
                _options == SPGENEntityMultiplePropertyMapOptions.IsIdentifier)
            {
                if (default(TPropertyValue).Equals(args.Value))
                {
                    if (result.ComparisonNode.Name == "Eq")
                        result = new SPGENEntityEvalLinqExprResult(args, "IsNull");
                    else
                        result = new SPGENEntityEvalLinqExprResult(args, "IsNotNull");

                    result.ComparisonNode.RemoveChild(result.ComparisonNode.LastChild);
                }
            }

            return result;
        }
    }
}
