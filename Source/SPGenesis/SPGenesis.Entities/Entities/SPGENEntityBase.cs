using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPGenesis.Entities.Repository;
using Microsoft.SharePoint;
using SPGenesis.Core;

namespace SPGenesis.Entities
{
    public abstract class SPGENEntityBase
    {
        private static SPGENObjectCache _proxyCache;
        private Type _instanceType;

        protected internal SPGENEntityRepositoryState RepositoryState { get; set; }

        public virtual void CreateListItem(SPList list)
        {
            var proxy = GetProxyInstance();
            if (proxy.FileInclusionMode == SPGENEntityFileInclusionMode.OnAllOperations)
            {
                proxy.CreateListItemWithFiles(this, list);
            }
            else
            {
                proxy.CreateListItem(this, list);
            }
        }

        public virtual void CreateListItemWithFiles(SPList list)
        {
            GetProxyInstance().CreateListItemWithFiles(this, list);
        }

        public virtual void UpdateListItem()
        {
            EnsureStateIsValid();

            if (this.RepositoryState.DataItem.HasFiles)
            {
                GetProxyInstance().UpdateListItemWithFiles(this);
            }
            else
            {
                GetProxyInstance().UpdateListItem(this);
            }
        }

        public virtual void DeleteListItem()
        {
            EnsureStateIsValid();

            GetProxyInstance().DeleteListItem(this);
        }

        protected virtual int ObjectCacheLimit
        {
            get { return 1000; }
        }

        [Obsolete("Not longer in use. Use ObjectCacheLimit instead.", true)]
        public static void SetObjectCacheLimit(int size)
        {
            throw new NotSupportedException();
        }

        private void EnsureStateIsValid()
        {
            if (this.RepositoryState == null)
                throw new SPGENEntityGeneralException("Invalid entity state. The entity instance might be in read only mode or not created from a list item.");
        }

        protected virtual object GetManagerInstance()
        {
            return null;
        }

        private IProxy GetProxyInstance()
        {
            if (_instanceType == null)
                _instanceType = this.GetType();

            if (_proxyCache == null)
            {
                lock (_proxyCache)
                {
                    if (_proxyCache == null)
                    {
                        _proxyCache = new SPGENObjectCache(this.ObjectCacheLimit);
                    }
                }
            }

            var result = _proxyCache.GetItem<IProxy>(_instanceType, () =>
                {
                    var proxyInstanceType = typeof(Proxy<>).GetGenericTypeDefinition().MakeGenericType(_instanceType);

                    return Activator.CreateInstance(proxyInstanceType, GetManagerInstance());
                });

            return result;
        }

        class Proxy<TEntity> : IProxy
            where TEntity : class
        {
            private SPGENEntityManagerFoundationBase<TEntity> _managerInstance;

            public Proxy(SPGENEntityManagerFoundationBase<TEntity> managerInstance)
            {
                if (managerInstance == null)
                    managerInstance = SPGENEntityManager<TEntity>.Instance;

                _managerInstance = managerInstance;
            }

            public void CreateListItem(object entity, SPList list)
            {
                _managerInstance.CreateNewListItem((TEntity)entity, list);
            }

            public void UpdateListItem(object entity)
            {
                _managerInstance.UpdateListItem((TEntity)entity);
            }

            public void DeleteListItem(object entity)
            {
                _managerInstance.DeleteListItem((TEntity)entity);
            }

            public void CreateListItemWithFiles(object entity, SPList list)
            {
                _managerInstance.CreateNewListItemWithFiles((TEntity)entity, list);
            }

            public void UpdateListItemWithFiles(object entity)
            {
                _managerInstance.UpdateListItemWithFiles((TEntity)entity);
            }

            public SPGENEntityFileInclusionMode FileInclusionMode { get { return _managerInstance.GetMapperInstance().FileInclusionMode; } }
        }

        interface IProxy
        {
            SPGENEntityFileInclusionMode FileInclusionMode { get; }
            void CreateListItem(object entity, SPList list);
            void CreateListItemWithFiles(object entity, SPList list);
            void UpdateListItem(object entity);
            void UpdateListItemWithFiles(object entity);
            void DeleteListItem(object entity);
        }
    }
}
