using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Entities
{
    public class SPGENEntityPagedCollectionInfo
    {
        public SPGENEntityPagedCollectionInfo(uint pageSize, int lastItemIdOfLastPage)
        {
            this.PageSize = pageSize;
            this.LastItemIdOfLastPage = lastItemIdOfLastPage;
        }

        internal SPGENEntityPagedCollectionInfo(uint pageSize, int lastItemIdOfLastPage, Action<int> lastItemIdOfCurrentPageAction)
        {
            this.PageSize = pageSize;
            this.LastItemIdOfLastPage = lastItemIdOfLastPage;

            _lastItemIdOfCurrentPageAction = lastItemIdOfCurrentPageAction;
        }

        public uint PageSize { get; private set; }
        public int LastItemIdOfLastPage { get; private set; }

        private int _lastItemIdOfCurrentPage;
        public int LastItemIdOfCurrentPage 
        {
            get
            {
                return _lastItemIdOfCurrentPage;
            }
            internal set
            {
                _lastItemIdOfCurrentPage = value;

                if (_lastItemIdOfCurrentPageAction != null)
                    _lastItemIdOfCurrentPageAction(value);
            }
        }

        private Action<int> _lastItemIdOfCurrentPageAction;
    }
}
