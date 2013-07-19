using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;
using System.Linq.Expressions;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterUserID<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, int>
            where TEntity : class
    {
        public SPGENEntityAdapterUserID() : base(true) { }

        public override int ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(int);

            var result = (arguments.Value is SPFieldLookupValue) ? (SPFieldLookupValue)arguments.Value : new SPFieldLookupValue((string)arguments.Value);

            return result.LookupId;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, int> arguments)
        {
            if (arguments.Value < 1)
                return null;

            return new SPFieldUserValue(arguments.Web, arguments.Value, null);
        }
    }

    public class SPGENEntityAdapterUserIDNullable<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, int?>
            where TEntity : class
    {
        public SPGENEntityAdapterUserIDNullable() : base(true) { }

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

            return new SPFieldUserValue(arguments.Web, (int)arguments.Value, null);
        }
    }

    public class SPGENEntityAdapterUserIDMulti<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, IList<int>>
            where TEntity : class
    {
        public SPGENEntityAdapterUserIDMulti() : base(true) { }

        public override IList<int> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return new List<int>();

            var coll = (SPFieldUserValueCollection)arguments.Value;
            IList<int> result = new List<int>(from user in coll select user.LookupId);

            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, IList<int>> arguments)
        {
            if (arguments.Value == null)
                return null;

            var result = new SPFieldUserValueCollection(arguments.Web, "");

            foreach (var userId in arguments.Value)
            {
                result.Add(new SPFieldUserValue(arguments.Web, userId, null));
            }

            return result;
        }
    }

    public class SPGENEntityAdapterUserDisplayName<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, string>
            where TEntity : class
    {
        public SPGENEntityAdapterUserDisplayName() : base(false) { }

        public override string ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(string);

            var result = (arguments.Value is SPFieldLookupValue) ? (SPFieldLookupValue)arguments.Value : new SPFieldLookupValue((string)arguments.Value);

            return result.LookupValue;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, string> arguments)
        {
            throw new NotSupportedException("Updating is not supported when using user display name. Only user ID is supported.");
        }
    }

    public class SPGENEntityAdapterUserDisplayNameMulti<TEntity> : SPGENEntityAdapterLookupLinq<TEntity, IList<string>>
            where TEntity : class
    {
        public SPGENEntityAdapterUserDisplayNameMulti() : base(false) { }

        public override IList<string> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return null;

            var coll = (SPFieldUserValueCollection)arguments.Value;
            IList<string> result = new List<string>(from user in coll select user.LookupValue);
            
            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, IList<string>> arguments)
        {
            throw new NotSupportedException("Updating is not supported when using user display name. Only user ID is supported.");
        }
    }
}
