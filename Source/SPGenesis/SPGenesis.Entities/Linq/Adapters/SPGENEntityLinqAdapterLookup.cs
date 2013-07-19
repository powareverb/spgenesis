using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using SPGenesis.Core;
using System.Collections;

namespace SPGenesis.Entities.Linq.Adapters
{
    [Obsolete("Not longer in use.", false)]
    public class SPGENEntityLinqAdapterLookup : SPGENEntityLinqAdapter
    {
        private bool _isLookupId;

        public SPGENEntityLinqAdapterLookup(bool isLookupId)
        {
            _isLookupId = isLookupId;
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            if (args.Value == null && !_isLookupId)
            {
                var result = new SPGENEntityEvalLinqExprResult(args);
                result.ValueNode.SetAttribute("Type", "Text");

                return result;
            }
            else
            {
                var result = base.EvalComparison(args);
                if (_isLookupId)
                    result.MakeLookupId();

                return result;
            }            
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            if (mce.Method.Name != "Contains")
                throw new NotSupportedException("The method '" + mce.Method.Name + "' is not supported in this context.");

            if (args.Operand == "Not")
                throw new NotSupportedException("The operand 'Not' is not supported in this context.");

            SPGENEntityEvalLinqExprResult result;
            if (SPGENCommon.HasInterface(SPGENCommon.GetFieldValueType(args.Field), typeof(IEnumerable)))
            {
                if (mce.Arguments.Count == 1)
                {
                    args.Value = LambdaExpression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke();
                }
                else if (mce.Arguments.Count == 2)
                {
                    args.Value = LambdaExpression.Lambda(mce.Arguments[1]).Compile().DynamicInvoke();
                }
                else
                {
                    throw new NotSupportedException("The method '" + mce.Method.Name + "' contained arguments that is not supported.");
                }

                if (args.Value == null)
                    throw new NotSupportedException("The Contains method does not support null as in-parameter.");

                result = new SPGENEntityEvalLinqExprResult(args, "Includes");
                result.ValueNode.SetAttribute("Type", "Lookup");
                result.ValueNode.InnerText = args.Value.ToString();
            }
            else
            {
                result = base.EvalMethodCall(mce, args);
            }

            if (_isLookupId)
                result.MakeLookupId();

            return result;
        }
    }
}
