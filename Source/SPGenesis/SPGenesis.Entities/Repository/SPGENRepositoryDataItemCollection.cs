using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.SharePoint;
using System.Data;

namespace SPGenesis.Entities.Repository
{
    public sealed class SPGENRepositoryDataItemCollection : IEnumerable<SPGENRepositoryDataItem>, IEnumerator<SPGENRepositoryDataItem>
    {
        private IEnumerator _itemCollectionEnumerator;
        private SPListItemCollection _itemCollection;
        private DataTable _dataTable;
        private int _dataRowIndex;
        private IList<string> _fieldNames;
        private SPGENRepositoryDataItem _currentDataItem;
        private SPGENEntityFileOperationArguments _fileOperationsParams;
        private bool _sourceIsSiteDataQuery;

        internal SPGENRepositoryDataItemCollection(SPListItemCollection itemCollection, string[] fieldNames, SPGENEntityFileOperationArguments fileOperationParams)
        {
            _itemCollection = itemCollection;
            _fieldNames = fieldNames;
            _fileOperationsParams = fileOperationParams;
            _currentDataItem = new SPGENRepositoryDataItem(fieldNames);
            _sourceIsSiteDataQuery = false;

            EnsureUnderlyingEnumeratorIsInvokedOnce();
        }

        internal SPGENRepositoryDataItemCollection(DataTable dataTable, string[] fieldNames)
        {
            _dataTable = dataTable;
            _dataRowIndex = -1;
            _fieldNames = fieldNames;
            _fileOperationsParams = new SPGENEntityFileOperationArguments() { FileMappingMode = SPGENEntityFileMappingMode.None };
            _currentDataItem = new SPGENRepositoryDataItem(fieldNames);
            _sourceIsSiteDataQuery = true;
        }

        private void EnsureUnderlyingEnumeratorIsInvokedOnce()
        {
            if (_sourceIsSiteDataQuery || _itemCollectionEnumerator != null)
                return;

            try
            {
                _itemCollectionEnumerator = _itemCollection.GetEnumerator();
            }
            catch (SPException ex)
            {
                throw new SPGENEntityGeneralException("The item collection could not be initialized. Please check that the query is well formed. " + ex.Message, ex);
            }
        }

        public SPListItemCollection ListItemCollection
        {
            get { return _itemCollection; }
        }

        public DataTable DataTable
        {
            get { return _dataTable; }
        }

        public SPGENRepositoryDataItem this[int index]
        {
            get
            {
                EnsureUnderlyingEnumeratorIsInvokedOnce();

                if (!_sourceIsSiteDataQuery)
                {
                    SPListItem listItem = _itemCollection[index];
                    SPGENRepositoryManager.Instance.ConvertToDataItem(listItem, _currentDataItem, _fileOperationsParams);

                    return _currentDataItem;
                }
                else
                {
                    SPGENRepositoryManager.Instance.ConvertToDataItem(_dataTable.Rows[index], _currentDataItem);

                    return _currentDataItem;
                }
            }
        }

        public SPGENRepositoryDataItem Current
        {
            get
            {
                EnsureUnderlyingEnumeratorIsInvokedOnce();

                if (!_sourceIsSiteDataQuery)
                {
                    SPListItem listItem = _itemCollectionEnumerator.Current as SPListItem;
                    SPGENRepositoryManager.Instance.ConvertToDataItem(listItem, _currentDataItem, _fileOperationsParams);

                    return _currentDataItem;
                }
                else
                {
                    SPGENRepositoryManager.Instance.ConvertToDataItem(_dataTable.Rows[_dataRowIndex], _currentDataItem);

                    return _currentDataItem;
                }
            }
        }

        public void Dispose()
        {
            _itemCollectionEnumerator = null;
            _currentDataItem = null;

            if (_dataTable != null)
            {
                _dataTable.Dispose();
                _dataTable = null;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            EnsureUnderlyingEnumeratorIsInvokedOnce();

            if (!_sourceIsSiteDataQuery)
            {
                return _itemCollectionEnumerator.MoveNext();
            }
            else
            {
                _dataRowIndex++;
                if (_dataRowIndex >= _dataTable.Rows.Count)
                    return false;

                return true;
            }
        }

        public void Reset()
        {
            EnsureUnderlyingEnumeratorIsInvokedOnce();

            if (!_sourceIsSiteDataQuery)
            {
                _itemCollectionEnumerator.Reset();
            }
            else
            {
                _dataRowIndex = -1;
            }
        }

        public IEnumerator<SPGENRepositoryDataItem> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
