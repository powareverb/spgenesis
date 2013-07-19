using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPGenesis.Entities.Repository;
using System.Collections;

namespace SPGenesis.Entities
{
    public sealed class SPGENEntityUpdatableWrapperCollection<TEntity> : IEnumerable<SPGENEntityUpdatableWrapper<TEntity>>, IEnumerator<SPGENEntityUpdatableWrapper<TEntity>>
        where TEntity : class
    {
        private SPGENEntityCollection<TEntity> _collection;
        private IQueryable<TEntity> _query;
        private bool _enumeratorIsInvoked;
        private SPGENEntityManagerFoundationBase<TEntity> _managerInstance;

        internal SPGENEntityUpdatableWrapperCollection(SPGENEntityCollection<TEntity> collection, SPGENEntityManagerFoundationBase<TEntity> managerInstance)
        {
            if (collection == null)
                throw new ArgumentException("Parameter can not be null.", "collection");

            _collection = collection;
            _managerInstance = managerInstance;
        }

        public SPGENEntityUpdatableWrapperCollection(IQueryable<TEntity> query)
        {
            if (query == null)
                throw new ArgumentException("Parameter can not be null.", "query");

            _query = query;
        }

        public SPGENEntityUpdatableWrapperCollection(IQueryable<TEntity> query, SPGENEntityManagerFoundationBase<TEntity> managerInstance)
        {
            if (query == null)
                throw new ArgumentException("Parameter can not be null.", "query");

            _query = query;
            _managerInstance = managerInstance;
        }

        internal SPGENEntityUpdatableWrapperCollection(IEnumerable<TEntity> collection, SPGENEntityManagerFoundationBase<TEntity> managerInstance)
        {
            _collection = collection as SPGENEntityCollection<TEntity>;
            if (_collection == null)
                throw new ArgumentException("The enumerable collection source must be a SPGENEntityCollection type.", "collection");

            _managerInstance = managerInstance;
        }

        public SPGENEntityUpdatableWrapper<TEntity> this[int index]
        {
            get
            {
                EnsureEnumeratorIsInvokedOnce();

                var entity = _collection[index];
                var item = _collection.GetCurrentDataItem();

                var result = new SPGENEntityUpdatableWrapper<TEntity>(entity, item, _managerInstance);
                result.DataItem = item;

                return result;
            }
        }

        public SPGENEntityUpdatableWrapper<TEntity> Current
        {
            get
            {
                EnsureEnumeratorIsInvokedOnce();

                var entity = _collection.Current;
                var item = _collection.GetCurrentDataItem();

                return new SPGENEntityUpdatableWrapper<TEntity>(entity, item, _managerInstance);
            }
        }


        [Obsolete("Not longer supported.", true)]
        public Linq.SPGENLinqQueryableList<TEntity> Query
        {
            get
            {
                EnsureEnumeratorIsInvokedOnce();

                var ql = _query as Linq.SPGENLinqQueryableList<TEntity>;
                if (ql == null)
                    throw new ArgumentException("The query instance must be of SPGENEntityCollection<TEntity> type.", "query");

                return (ql as Linq.SPGENLinqQueryableList<TEntity>);
            }
        }

        public void Dispose()
        {
            _collection.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            {
                EnsureEnumeratorIsInvokedOnce();
                return this.Current;
            }
        }

        public bool MoveNext()
        {
            EnsureEnumeratorIsInvokedOnce();

            return _collection.MoveNext();
        }

        public void Reset()
        {
            EnsureEnumeratorIsInvokedOnce();

            _collection.Reset();
        }

        public IEnumerator<SPGENEntityUpdatableWrapper<TEntity>> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        private void EnsureEnumeratorIsInvokedOnce()
        {
            if (_enumeratorIsInvoked)
                return;

            if (_query != null)
            {
                _collection = _query.GetEnumerator() as SPGENEntityCollection<TEntity>;
            }
            else
            {
                _collection.GetEnumerator();
            }

            _enumeratorIsInvoked = true;
        }
    }

    [Obsolete("Not longer in use. Use SPGENEntityUpdatableWrapperCollection instead.", true)]
    public sealed class SPGENUpdatableEntityCollection<TEntity>
        where TEntity : class
    {
    }

    [Obsolete("Not longer in use. Use SPGENEntityUpdatableWrapperCollection instead.", true)]
    public sealed class SPGENUpdatableEntityWrapperCollection<TEntity>
        where TEntity : class
    {
    }
}
