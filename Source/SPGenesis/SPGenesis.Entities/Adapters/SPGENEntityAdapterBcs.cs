using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.BusinessData.Infrastructure;
using SPGenesis.Core;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterBcs<TEntity, TPropertyValue> : SPGENEntityAdapter<TEntity, TPropertyValue>
            where TEntity : class
    {
        public override TPropertyValue ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            try
            {
                object[] arr = EntityInstanceIdEncoder.DecodeEntityInstanceId((string)arguments.Value);

                return (TPropertyValue)arr[0];
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not read bcs ID field '" + arguments.FieldName + "'.", ex);
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments)
        {
            try
            {
                return EntityInstanceIdEncoder.EncodeEntityInstanceId(new object[] { arguments.Value });
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not read bcs ID field '" + arguments.FieldName + "'.", ex);
            }
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            args.Value = EntityInstanceIdEncoder.EncodeEntityInstanceId(new object[] { args.Value });

            return base.EvalComparison(args);
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(System.Linq.Expressions.MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            args.Value = EntityInstanceIdEncoder.EncodeEntityInstanceId(new object[] { args.Value });
            
            return base.EvalMethodCall(mce, args);
        }

    }
}
