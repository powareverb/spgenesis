using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Xml;
using Microsoft.SharePoint.Utilities;
using System.Linq.Expressions;
using System.Reflection;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterDefault<TEntity, TPropertyValue> : SPGENEntityAdapter<TEntity, TPropertyValue>
        where TEntity : class
    {
        private bool _isDateTime;

        public SPGENEntityAdapterDefault()
        {
            if (typeof(TPropertyValue) == typeof(DateTime))
                _isDateTime = true;
        }

        public override TPropertyValue ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            return SPGENCommon.ConvertListItemValue<TPropertyValue>(arguments.Web, arguments.Value, arguments.Field is SPFieldCalculated);
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TPropertyValue> arguments)
        {
            if (_isDateTime)
            {
                object v = (object)arguments.Value;
                if ((DateTime)v == default(DateTime))
                    return null;

               return v;
            }
            else
            {
                return arguments.Value;
            }
        }
    }
}
