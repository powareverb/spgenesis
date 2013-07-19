using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities.Adapters
{    
    public class SPGENEntityAdapterGeneric<TEntity, TPropertyValue, TListItemValue> : SPGENEntityAdapterDefault<TEntity, TPropertyValue>
        where TEntity : class
    {
        private Func<SPGENEntityAdapterConvArgs<TEntity, TListItemValue>, TPropertyValue> _convertToPropertyValueFunction;
        private Func<SPGENEntityAdapterConvArgs<TEntity, TPropertyValue>, TListItemValue> _convertToListItemValueFunction;

        private Func<SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> _evalComparisonFunction;
        private Func<MethodCallExpression, SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> _evalMethodCallFunction;

        public object CustomProperty { get; set; }

        public bool UseRawListItemValue { get; set; }

        public void RegisterToPropertyValueConverter(Func<SPGENEntityAdapterConvArgs<TEntity, TListItemValue>, TPropertyValue> convertToPropertyValueFunction)
        {
            _convertToPropertyValueFunction = convertToPropertyValueFunction;
        }

        public void RegisterToItemValueConverter(Func<SPGENEntityAdapterConvArgs<TEntity, TPropertyValue>, TListItemValue> convertToListItemValueFunction)
        {
            _convertToListItemValueFunction = convertToListItemValueFunction;
        }

        public override TPropertyValue ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (_convertToPropertyValueFunction == null)
                return (TPropertyValue)arguments.Value;

            if (this.UseRawListItemValue)
            {
                var clonedArguments = arguments.Clone<TListItemValue>((TListItemValue)arguments.Value);

                return _convertToPropertyValueFunction(clonedArguments);
            }
            else
            {
                TListItemValue value = SPGENCommon.ConvertListItemValue<TListItemValue>(arguments.Web, arguments.Value, arguments.Field is SPFieldCalculated);

                var clonedArguments = arguments.Clone<TListItemValue>(value);

                return _convertToPropertyValueFunction(clonedArguments);
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments)
        {
            if (_convertToListItemValueFunction == null)
                return arguments.Value;

            return _convertToListItemValueFunction(arguments);
        }

        [Obsolete("Use RegisterEvalComparisonMethod instead.", true)]
        public void RegisterEvalBinaryExpressionMethod(Func<SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> method)
        {
        }

        public void RegisterEvalComparisonMethod(Func<SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> method)
        {
            _evalComparisonFunction = method;
        }

        public void RegisterEvalMethodCall(Func<MethodCallExpression, SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> method)
        {
            _evalMethodCallFunction = method;
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            if (_evalMethodCallFunction != null)
            {
                return _evalComparisonFunction(args);
            }
            else
            {
                return base.EvalComparison(args);
            }
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            if (_evalMethodCallFunction != null)
            {
                return _evalMethodCallFunction(mce, args);
            }
            else
            {
                return base.EvalMethodCall(mce, args);
            }
        }
    }
}
