using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.SharePoint;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities
{
    public sealed class SPGENEntityCollection<TEntity> : IEnumerable<TEntity>, IEnumerator<TEntity> 
        where TEntity : class
    {
        private IEnumerator _dataItemCollectionEnumerator;
        private SPGENRepositoryDataItemCollection _dataItemCollection;
        private SPGENRepositoryDataItem _currentDataItem;
        private SPGENEntityOperationContext<TEntity> _context;

        internal SPGENEntityCollection(SPGENRepositoryDataItemCollection dataItemCollection, SPGENEntityOperationContext<TEntity> context)
        {
            _dataItemCollection = dataItemCollection;
            _context = context;
            _dataItemCollectionEnumerator = dataItemCollection.GetEnumerator();
        }

        public TEntity this[int index]
        {
            get
            {
                _currentDataItem = _dataItemCollection[index];
                _context.DataItem = _currentDataItem;
                _context.CreateAndPopulateEntity();

                return _context.Entity;
            }
        }

        public TEntity Current
        {
            get
            {
                _currentDataItem = _dataItemCollection.Current;
                _context.DataItem = _currentDataItem;
                _context.CreateAndPopulateEntity();

                return _context.Entity;
            }
        }

        public SPListItemCollection ListItemCollection { get { return _dataItemCollection.ListItemCollection; } }

        internal SPGENRepositoryDataItemCollection DataItemCollection
        {
            get { return _dataItemCollection; }
        }

        internal SPGENRepositoryDataItem GetCurrentDataItem()
        {
            return _currentDataItem;
        }

        public void Dispose()
        {
            _dataItemCollectionEnumerator = null;
            _currentDataItem = null;
            _context = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return _dataItemCollectionEnumerator.MoveNext();
        }

        public void Reset()
        {
            _dataItemCollectionEnumerator.Reset();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
