using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace SPGenesis.Core
{
    /// <summary>
    /// Supports the framework and should not be used outside.
    /// </summary>
    public sealed class SPGENObjectCache
    {
        private object _cacheLock = new object();
        private Hashtable _items = new Hashtable();
        private List<object> _itemKeys = new List<object>();

        private int _capacity = 0;
        private int _itemIndex = 0;

        public SPGENObjectCache()
        {
        }

        public SPGENObjectCache(int capacity)
        {
            _capacity = capacity;
        }

        [Obsolete("Not longer in use.", true)]
        public SPGENObjectCache(string uniqueKey)
        {
        }

        [Obsolete("Not longer in use.", true)]
        public SPGENObjectCache(string uniqueKey, int capacity)
        {
        }


        public bool Exists(object key)
        {
            return _items.ContainsKey(key);
        }

        public TResult GetItem<TResult>(object key, Func<object> fetchItemToCacheFunction)
        {
            return (TResult)GetItem(key, fetchItemToCacheFunction);
        }

        public object GetItem(object key, Func<object> fetchItemToCacheFunction)
        {
            ItemContainer container = _items[key] as ItemContainer;

            if (container == null)
            {
                container = AddItemToCache(key, fetchItemToCacheFunction);
            }

            return container.Item;
        }

        public void Insert(object key, object item, bool ignoreIfExists)
        {
            lock (_cacheLock)
            {
                if (_items.ContainsKey(key))
                {
                    if (!ignoreIfExists)
                    {
                        _items[key] = item;
                    }
                }
                else
                {
                    AddItemToCache(key, () => item);
                }
            }
        }

        public void Clear()
        {
            Clear(null);
        }

        public void Clear(Action<SPGENObjectCache> methodToRunAfterClear)
        {
            lock (_cacheLock)
            {
                _items = new Hashtable();
                _itemKeys = new List<object>();
                _itemIndex = 0;

                if (methodToRunAfterClear != null)
                    methodToRunAfterClear(this);
            }
        }

        private ItemContainer AddItemToCache(object uniqueKey, Func<object> fetchItemToCacheFunction)
        {
            lock (_cacheLock)
            {
                if (_items.ContainsKey(uniqueKey))
                    return _items[uniqueKey] as ItemContainer;

                var container = new ItemContainer(fetchItemToCacheFunction());

                if (_capacity > 0)
                {
                    if (_itemIndex == _capacity)
                    {
                        object firstKey = _itemKeys[0];
                        _items.Remove(firstKey);
                        _itemKeys.RemoveAt(0);
                    }
                    else
                    {
                        _itemIndex++;
                    }
                }

                _items.Add(uniqueKey, container);
                _itemKeys.Add(uniqueKey);

                return container;

            }
        }

        class ItemContainer
        {
            private object _item;

            public ItemContainer(object item)
            {
                _item = item;
            }

            public object Item
            {
                get
                {
                    return _item;
                }
            }
        }
    }
}
