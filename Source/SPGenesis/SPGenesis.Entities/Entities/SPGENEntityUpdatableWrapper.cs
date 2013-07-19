using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities
{
    public sealed class SPGENEntityUpdatableWrapper<TEntity>
        where TEntity : class
    {
        private SPGENEntityManagerFoundationBase<TEntity> _managerInstance;
        internal SPGENRepositoryDataItem DataItem;

        internal SPGENEntityUpdatableWrapper(TEntity entity, SPGENRepositoryDataItem dataItem, SPGENEntityManagerFoundationBase<TEntity> managerInstance)
        {
            this.DataItem = dataItem;
            this.Instance = entity;
            
            _managerInstance = managerInstance;
        }

        public TEntity Instance { get; private set; }

        public void Update()
        {
            _managerInstance.UpdateListItem(this.Instance, this.DataItem.ListItem);
        }

        public void Update(SPGENEntityOperationParameters parameters)
        {
            _managerInstance.UpdateListItem(this.Instance, this.DataItem.ListItem, parameters: parameters);
        }

        public void UpdateWithFiles()
        {
            _managerInstance.UpdateListItemWithFiles(this.Instance, this.DataItem.ListItem);
        }

        public void UpdateWithFiles(SPFileSaveBinaryParameters saveParameters)
        {
            _managerInstance.UpdateListItemWithFiles(this.Instance, this.DataItem.ListItem, saveParameters);
        }

        public void Delete()
        {
            SPGENRepositoryManager.Instance.DeleteListItem(this.DataItem);
        }
    }

    [Obsolete("Not longer in use. Use SPGENEntityUpdatableWrapper instead.", true)]
    public sealed class SPGENUpdatableEntity<TEntity>
        where TEntity : class
    {
    }

    [Obsolete("Not longer in use. Use SPGENEntityUpdatableWrapper instead.", true)]
    public sealed class SPGENUpdatableEntityWrapper<TEntity>
        where TEntity : class
    {
    }
}
