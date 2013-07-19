using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SPGenesis.Core
{
    public abstract class SPGENElementCollectionBase<TItem, TIdentifier> : IList<TItem> where TItem : class
    {
        protected class ItemContainer
        {
            public object ElementInstance { get; set; }
            public TItem Item { get; set; }
            public bool IsUpdated { get; set; }
            public bool IsRemoved { get; set; }
            public bool IsFromDefinition { get; set; }
        }

        private List<ItemContainer> _listOfItems = new List<ItemContainer>();
        private Dictionary<TIdentifier, object> _dictionaryOfElementInstances = new Dictionary<TIdentifier, object>();

        protected void AddElementInstance(TIdentifier id, object instance)
        {
            if (!_dictionaryOfElementInstances.ContainsKey(id))
            {
                _dictionaryOfElementInstances.Add(id, instance);
            }
            else
            {
                _dictionaryOfElementInstances[id] = instance;
            }
        }

        [Obsolete()]
        protected void RemoveElementInstance(TIdentifier id)
        {
            if (_dictionaryOfElementInstances.ContainsKey(id))
            {
                _dictionaryOfElementInstances.Remove(id);
            }
        }

        protected TResult GetElementInstance<TResult>(TIdentifier id)
        {
            if (_dictionaryOfElementInstances.ContainsKey(id))
                return (TResult)_dictionaryOfElementInstances[id];

            return default(TResult);
        }

        protected bool ElementInstanceExists(TIdentifier id)
        {
            return _dictionaryOfElementInstances.ContainsKey(id);
        }

        public SPGENProvisioningMode ProvisioningMode
        {
            get;
            set;
        }

        protected IList<ItemContainer> AddedItems { get { return _listOfItems; } }

        public SPGENElementCollectionBase()
        {
            this.ProvisioningMode = SPGENProvisioningMode.AddUpdateRemove;
        }

        protected bool CanUpdate { get { return (this.ProvisioningMode != SPGENProvisioningMode.AddOnly); } }
        protected bool IsExclusiveAdd { get { return (this.ProvisioningMode == SPGENProvisioningMode.ExclusiveAdd || this.ProvisioningMode == SPGENProvisioningMode.AddUpdateRemoveExclusive); } }

        internal void ResetUpdatedStatus()
        {
            foreach(var item in _listOfItems)
                item.IsUpdated = false;
        }
        internal void ResetRemovedStatus()
        {
            foreach (var item in _listOfItems)
                item.IsRemoved = false;
        }

        protected virtual bool IsItemEqual(TItem item1, TItem item2) { return item1.Equals(item2); }

        protected virtual bool IsIdentifierEqual(TIdentifier id1, TIdentifier id2) { return id1.Equals(id2); }

        protected abstract TIdentifier GetIdentifier(TItem item);

        public List<TItem> GetAllAddedAndUpdatedItems()
        {
            var list = new List<TItem>();

            foreach (var i in _listOfItems)
            {
                if (i.IsRemoved == false)
                {
                    if (i.IsFromDefinition && !i.IsUpdated)
                        continue;

                    list.Add(i.Item);
                }
            }

            return list;
        }

        public List<TItem> GetAllUpdatedItems()
        {
            var list = new List<TItem>();

            foreach (var i in _listOfItems)
            {
                if (i.IsRemoved == false && i.IsUpdated)
                {
                    list.Add(i.Item);
                }
            }

            return list;
        }

        public List<TItem> GetAllRemovedItems()
        {
            var q = from i in _listOfItems
                    where i.IsRemoved == true
                    select i.Item;

            return q.ToList<TItem>();
        }

        
        public virtual void Update(TItem item)
        {
            int idx = IndexOf(item);
            if (idx == -1)
            {
                throw new SPGENGeneralException("Item does not exist in this collection.");
            }
            else
            {
                var container = _listOfItems[idx];
                container.IsUpdated = true;
                container.IsRemoved = false;
            }
        }
        public void Update(TIdentifier identifier)
        {
            Update(this[identifier]);
        }

        public int IndexOf(TItem item)
        {
            for (int i = 0; i < _listOfItems.Count; i++)
            {
                if (IsItemEqual(item, _listOfItems[i].Item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, TItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TItem this[int index]
        {
            get
            {
                return _listOfItems[index].Item;
            }
            set
            {
                if (IndexOf(_listOfItems[index].Item) == -1)
                {
                    var container = _listOfItems[index];
                    container.Item = value;
                    container.IsUpdated = true;
                    container.IsRemoved = false;
                }
                else
                {
                    throw new ArgumentException("An item with the same identifier already exists in the collection.");
                }
            }
        }

        public TItem this[TItem item]
        {
            get
            {
                int idx = IndexOf(item);
                if (idx == -1)
                {
                    return default(TItem);
                }
                else
                {
                    return _listOfItems[idx].Item;
                }
            }
        }

        public TItem this[TIdentifier id]
        {
            get
            {
                var container = _listOfItems.FirstOrDefault<ItemContainer>(i => IsIdentifierEqual(GetIdentifier(i.Item), id));
                if (container == null)
                    return null;

                return container.Item;
            }
        }

        public void Add(TItem item)
        {
            Add(item, false);
        }

        internal void Add(TItem item, bool registerAsUpdated)
        {
            Add(item, registerAsUpdated, true, false);
        }

        internal void Add(TItem item, bool registerAsUpdated, bool updateIfAlreadyExists, bool isFromDefinition)
        {
            int idx = IndexOf(item);
            if (idx == -1)
            {
                var container = new ItemContainer();
                
                container.Item = item;
                container.IsUpdated = registerAsUpdated;
                container.IsFromDefinition = isFromDefinition;

                _listOfItems.Add(container);
            }
            else
            {
                if (updateIfAlreadyExists)
                {
                    var container = _listOfItems[idx];

                    container.Item = item;
                    container.IsUpdated = registerAsUpdated;
                    container.IsRemoved = false;
                    container.IsFromDefinition = isFromDefinition;
                }
                else
                {
                    throw new SPGENGeneralException("Item already exists in the collection.");
                }
            }
        }

        private ItemContainer GetContainer(TItem item)
        {
            return _listOfItems.FirstOrDefault<ItemContainer>(c => IsItemEqual(c.Item, item));
        }

        public void Clear()
        {
            _listOfItems.Clear();
            _dictionaryOfElementInstances.Clear();
        }

        public void Clear(bool unprovision, bool removeItemsFromXmlDefinitions)
        {
            foreach (var item in _listOfItems)
            {
                if (item.IsFromDefinition && !removeItemsFromXmlDefinitions)
                    continue;

                item.IsRemoved = true;
            }
        }

        public bool Contains(TItem item)
        {
            var result = _listOfItems.FirstOrDefault<ItemContainer>(i => IsItemEqual(item, i.Item));

            return result != null;
        }

        public virtual void CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _listOfItems.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        internal void RemoveDirect(TItem item)
        {
            int idx = IndexOf(item);
            if (idx != -1)
            {
                _listOfItems.RemoveAt(idx);

                var id = GetIdentifier(item);
                if (_dictionaryOfElementInstances.ContainsKey(id))
                    _dictionaryOfElementInstances.Remove(id);
            }
        }

        public bool Remove(TItem item)
        {
            int idx = IndexOf(item);
            if (idx == -1)
            {
                Add(item, false);

                _listOfItems[IndexOf(item)].IsRemoved = true;
            }
            else
            {
                _listOfItems[IndexOf(item)].IsRemoved = true;
            }

            return true;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            var q = from n in _listOfItems
                    select n.Item;

            return q.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _listOfItems.GetEnumerator();
        }
    }
}
