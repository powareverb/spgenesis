using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENProvisioner
    {
        private List<SPGENProvisionerInstance> _elements = new List<SPGENProvisionerInstance>();

        public IList<SPGENProvisionerInstance> Elements
        {
            get { return _elements; }
        }

        public void AddField<TElement>()
        {
            AddField<TElement>(null);
        }
        public void AddField<TElement>(Action<SPField> provisionAction)
        {
            AddField<TElement>(provisionAction, null, SPGENProvisioningOnErrorBehavior.StopAndThrowException);
        }
        public void AddField<TElement>(Action<SPField> provisionAction, Action<SPGENProvisionerInstance, Exception> onErrorAction, SPGENProvisioningOnErrorBehavior onErrorBehavior)
        {
            CheckIfElementAlreadyExists(typeof(TElement));

            var e = new SPGENProvisionerInstance(typeof(TElement), provisionAction);

            _elements.Add(e);
        }

        public void AddContentType<TElement>()
        {
            AddContentType<TElement>(null);
        }
        public void AddContentType<TElement>(Action<SPGENContentTypeProvisioningArguments> provisionAction)
        {
            AddContentType<TElement>(provisionAction, null, SPGENProvisioningOnErrorBehavior.StopAndThrowException);
        }
        public void AddContentType<TElement>(Action<SPGENContentTypeProvisioningArguments> provisionAction, Action<SPGENProvisionerInstance, Exception> onErrorAction, SPGENProvisioningOnErrorBehavior onErrorBehavior)
        {
            CheckIfElementAlreadyExists(typeof(TElement));

            var e = new SPGENProvisionerInstance(typeof(TElement), provisionAction, onErrorAction, onErrorBehavior);

            _elements.Add(e);
        }

        public void AddListInstance<TElement>()
        {
            AddListInstance<TElement>(null);
        }
        public void AddListInstance<TElement>(Action<SPGENListProvisioningArguments> provisionAction)
        {
            AddListInstance<TElement>(provisionAction, null, SPGENProvisioningOnErrorBehavior.StopAndThrowException);
        }
        public void AddListInstance<TElement>(Action<SPGENListProvisioningArguments> provisionAction, Action<SPGENProvisionerInstance, Exception> onErrorAction, SPGENProvisioningOnErrorBehavior onErrorBehavior)
        {
            CheckIfElementAlreadyExists(typeof(TElement));

            var e = new SPGENProvisionerInstance(typeof(TElement), provisionAction);
            e.OnErrorBehavior = onErrorBehavior;

            _elements.Add(e);
        }

        private void CheckIfElementAlreadyExists(Type element)
        {
            bool exists = _elements.Exists(t => t.Type == element);

            if (exists)
            {
                throw new ArgumentException("Element '" + element.FullName + "' already exists in the collection.");
            }
        }

        public int GetElementPosition(Type element)
        {
            return _elements.FindIndex(t => t.Type == element);
        }

        public void Remove<TElement>()
        {
            int idx = _elements.FindIndex(t => t.Type == typeof(TElement));

            if (idx != -1)
            {
                _elements.RemoveAt(idx);
            }
        }

        public void StartProvision(SPSite site)
        {
            using (SPWeb web = site.RootWeb)
            {
                this.StartProvision(web);
            }
        }
        public void StartProvision(SPWeb web)
        {
            for (int i = 0; i < _elements.Count; i++ )
            {
                var element = _elements[i];

                try
                {
                    element.Provision(web);
                }
                catch (Exception ex)
                {
                    if (element.OnErrorAction != null)
                        element.OnErrorAction.Invoke(element, ex);

                    if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.Stop)
                    {
                        return;
                    }
                    else if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.StopAndThrowException)
                    {
                        throw;
                    }
                    else if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.Rollback)
                    {
                        Rollback(web, i);
                    }
                }
            }
        }

        public void StartUnprovision(SPSite site)
        {
            using (SPWeb web = site.RootWeb)
            {
                this.StartUnprovision(web);
            }
        }
        public void StartUnprovision(SPWeb web)
        {
            for(int i = _elements.Count -1; i >= 0; i--)
            {
                var element = _elements[i];

                try
                {
                    element.Unprovision(web);
                }
                catch (Exception ex)
                {
                    if (element.OnErrorAction != null)
                        element.OnErrorAction.Invoke(element, ex);

                    if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.Stop)
                    {
                        return;
                    }
                    else if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.StopAndThrowException)
                    {
                        throw;
                    }
                    else if (element.OnErrorBehavior == SPGENProvisioningOnErrorBehavior.Rollback)
                    {
                        throw new SPGENGeneralException("Can not rollback when unprovisioning.", ex);
                    }
                }
            }
        }


        public static SPField[] ProvisionFields(SPWeb web, bool updateIfExists, bool pushChangesToList, params Type[] fieldDefinitions)
        {
            var listOfFields = new List<SPField>();
            foreach (Type t in fieldDefinitions)
            {
                if (!t.IsSubclassOf(typeof(SPGENFieldBase)))
                    throw new SPGENGeneralException(string.Format("The type '{0}' does not inherit from {1}.", t.FullName, typeof(SPGENFieldBase).Name));

                var instance = SPGENElementManager.GetInstance(t) as SPGENFieldBase;
                listOfFields.Add(instance.Provision(web.Fields, updateIfExists, pushChangesToList));
            }

            return listOfFields.ToArray();
        }

        public static SPField[] ProvisionFields(SPList list, bool updateIfExists, bool pushChangesToList, params Type[] fieldDefinitions)
        {
            var listOfFields = new List<SPField>();
            foreach (Type t in fieldDefinitions)
            {
                if (!t.IsSubclassOf(typeof(SPGENFieldBase)))
                    throw new SPGENGeneralException(string.Format("The type '{0}' does not inherit from {1}.", t.FullName, typeof(SPGENFieldBase).Name));

                var instance = SPGENElementManager.GetInstance(t) as SPGENFieldBase;
                listOfFields.Add(instance.Provision(list.Fields, updateIfExists, pushChangesToList));
            }

            return listOfFields.ToArray();
        }

        public static SPContentType[] ProvisionContentTypes(SPWeb web, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate, params Type[] contentTypeDefinitions)
        {
            var listOfContentTypes = new List<SPContentType>();
            foreach (Type t in contentTypeDefinitions)
            {
                if (!t.IsSubclassOf(typeof(SPGENContentTypeBase)))
                    throw new SPGENGeneralException(string.Format("The type '{0}' does not inherit from {1}.", t.FullName, typeof(SPGENContentTypeBase).Name));

                var instance = SPGENElementManager.GetInstance(t) as SPGENContentTypeBase;
                listOfContentTypes.Add(instance.Provision(web.ContentTypes, null, updateIfExists, updateChildren, stopOnSealedOrReadOnlyUpdate));
            }
            return listOfContentTypes.ToArray();
        }

        public static SPContentType[] ProvisionContentTypes(SPList list, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate, params Type[] contentTypeDefinitions)
        {
            var listOfContentTypes = new List<SPContentType>();
            foreach (Type t in contentTypeDefinitions)
            {
                if (!t.IsSubclassOf(typeof(SPGENContentTypeBase)))
                    throw new SPGENGeneralException(string.Format("The type '{0}' does not inherit from {1}.", t.FullName, typeof(SPGENContentTypeBase).Name));

                var instance = SPGENElementManager.GetInstance(t) as SPGENContentTypeBase;
                listOfContentTypes.Add(instance.Provision(list.ContentTypes, null, updateIfExists, updateChildren, stopOnSealedOrReadOnlyUpdate));
            }
            return listOfContentTypes.ToArray();
        }

        public static SPList[] ProvisionListInstances(SPWeb web, params Type[] listInstanceDefinitions)
        {
            var listOfListInstances = new List<SPList>();
            foreach (Type t in listInstanceDefinitions)
            {
                if (!t.IsSubclassOf(typeof(SPGENListInstanceBase)))
                    throw new SPGENGeneralException(string.Format("The type '{0}' does not inherit from {1}.", t.FullName, typeof(SPGENListInstanceBase).Name));

                var instance = SPGENElementManager.GetInstance(t) as SPGENListInstanceBase;
                listOfListInstances.Add(instance.ProvisionOnWeb(web));
            }
            return listOfListInstances.ToArray();
        }


        private void Rollback(SPWeb Web, int rollbackElementPosition)
        {
            try
            {
                for (int i = rollbackElementPosition; i >= 0; i--)
                {
                    var element = _elements[i];

                    element.Unprovision(Web);
                }
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException("An error occured during rollback.", ex);
            }
        }
    }
}
