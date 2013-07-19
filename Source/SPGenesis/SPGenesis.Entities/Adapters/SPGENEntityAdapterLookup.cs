using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Linq.Expressions;
using System.Xml;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterLookupID<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, int>
            where TEntity : class
    {
        public SPGENEntityAdapterLookupID() : base(true) { }

        public override int ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(int);

            var result = (arguments.Value is SPFieldLookupValue) ? (SPFieldLookupValue)arguments.Value : new SPFieldLookupValue((string)arguments.Value);

            return result.LookupId;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, int> arguments)
        {
            if (arguments.Value == 0)
                return null;

            return new SPFieldLookupValue((int)arguments.Value, "");
        }
    }

    public class SPGENEntityAdapterLookupIDNullable<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, int?>
            where TEntity : class
    {
        public SPGENEntityAdapterLookupIDNullable() : base(true) { }

        public override int? ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(int?);

            var result = (arguments.Value is SPFieldLookupValue) ? (SPFieldLookupValue)arguments.Value : new SPFieldLookupValue((string)arguments.Value);

            return result.LookupId;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, int?> arguments)
        {
            if (!arguments.Value.HasValue)
                return null;

            return new SPFieldLookupValue(arguments.Value.Value, "");
        }
    }

    public class SPGENEntityAdapterLookupIDMulti<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, IList<int>>
            where TEntity : class
    {
        public SPGENEntityAdapterLookupIDMulti() : base(true) { }

        public override IList<int> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return new List<int>();

            var coll = (SPFieldLookupValueCollection)arguments.Value;
            var result = new List<int>();

            foreach (var lookup in coll)
            {
                result.Add(lookup.LookupId);
            }

            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, IList<int>> arguments)
        {
            if (arguments.Value == null)
                return null;

            var result = new SPFieldLookupValueCollection();

            foreach (var id in arguments.Value)
            {
                result.Add(new SPFieldLookupValue(id, ""));
            }

            return result;
        }
    }

    public class SPGENEntityAdapterLookupValue<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, string>
            where TEntity : class
    {
        public SPGENEntityAdapterLookupValue() : base(false) { }

        public override string ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(string);

            var result = (arguments.Value is SPFieldLookupValue) ? (SPFieldLookupValue)arguments.Value : new SPFieldLookupValue((string)arguments.Value);

            return result.LookupValue;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, string> arguments)
        {
            throw new NotSupportedException("Update is not supported when using lookup values. Use lookup id instead.");
        }
    }

    public class SPGENEntityAdapterLookupValueMulti<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, IList<string>>
            where TEntity : class
    {
        public SPGENEntityAdapterLookupValueMulti() : base(false) { }

        public override IList<string> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return new List<string>();

            var coll = (SPFieldLookupValueCollection)arguments.Value;
            var result = new List<string>();

            foreach (var lookup in coll)
            {
                result.Add(lookup.LookupValue);
            }

            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, IList<string>> arguments)
        {
            throw new NotSupportedException("Update is not supported when using lookup values. Use lookup id instead.");
        }
    }

    public abstract class SPGENEntityAdapterLookupLinq<TEntity, TPropertyValue> : SPGENEntityAdapter<TEntity, TPropertyValue>
            where TEntity : class
    {
        private bool _isLookupId;

        public SPGENEntityAdapterLookupLinq(bool isLookupId)
        {
            _isLookupId = isLookupId;
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            args.IsLookupIdProperty = _isLookupId;

            return base.EvalComparison(args);
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            args.IsLookupIdProperty = _isLookupId;

            return base.EvalMethodCall(mce, args);
        }
    }
}
