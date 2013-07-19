using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.BusinessData.Infrastructure;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterPassThrough<TEntity> : SPGENEntityAdapter<TEntity, object>
            where TEntity : class
    {
        public override object ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            return arguments.Value;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            return arguments.Value;
        }
    }
}
