using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace SPGenesis.Entities.Linq.Adapters
{
    [Obsolete("Not longer in use.", false)]
    public class SPGENEntityLinqAdapterGeneric : SPGENEntityLinqAdapter
    {
        private Func<SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> _evalComparisonFunction;
        private Func<MethodCallExpression, SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> _evalMethodCallFunction;

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            if (_evalComparisonFunction != null)
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

        public void RegisterEvalComparison(Func<SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> method)
        {
            _evalComparisonFunction = method;
        }

        public void RegisterEvalMethodCall(Func<MethodCallExpression, SPGENEntityEvalLinqExprArgs, SPGENEntityEvalLinqExprResult> method)
        {
            _evalMethodCallFunction = method;
        }
    }
}
