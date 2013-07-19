using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.BusinessData.Infrastructure;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterGuidSingle<TEntity> : SPGENEntityAdapter<TEntity, Guid>
            where TEntity : class
    {
        public override Guid ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(Guid);

            var result = new Guid(arguments.Value as string);

            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, Guid> arguments)
        {
            return arguments.Value;
        }
    }

    public class SPGENEntityAdapterGuidNullableSingle<TEntity> : SPGENEntityAdapter<TEntity, Guid?>
            where TEntity : class
    {
        public override Guid? ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return default(Guid?);

            var result = new Guid(arguments.Value as string);

            return result;
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, Guid?> arguments)
        {
            return arguments.Value;
        }
    }
}
