using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPGenesis.Entities.Repository;
using Microsoft.SharePoint;

namespace SPGenesis.Entities
{
    public sealed class SPGENEntityRepositoryState
    {
        internal SPGENEntityRepositoryState(SPGENRepositoryDataItem dataItem)
        {
            this.DataItem = dataItem;
        }

        public SPGENRepositoryDataItem DataItem { get; private set; }
        public SPWeb Web { get { return this.DataItem.List.ParentWeb; } }
        public SPList List { get { return this.DataItem.List; } }
        public SPListItem ListItem { get { return this.DataItem.ListItem; } }
    }
}
