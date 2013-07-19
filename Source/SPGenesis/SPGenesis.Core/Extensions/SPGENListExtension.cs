using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Linq;

namespace SPGenesis.Core
{
    public static class SPGENListExtensions
    {
        public static void RegisterEventReceiver<TReceiver>(this SPList list, bool keepOnlyDeclaredMethods)
        {
            var col = new SPGENEventReceiverCollection();
            col.AddType(typeof(TReceiver), null, keepOnlyDeclaredMethods);
            col.Provision(list.EventReceivers);

            SPGENListInstanceStorage.Instance.UpdateList(list);
        }

        public static EntityList<TEntity> GetEntityList<TEntity>(this SPList list, DataContext dataContext)
        {
            return dataContext.GetList<TEntity>(list.Title);
        }
    }
}
