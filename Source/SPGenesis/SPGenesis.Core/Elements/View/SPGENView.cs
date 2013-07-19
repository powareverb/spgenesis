using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENView<TView> : SPGENViewBase
        where TView : SPGENView<TView>, new()
    {
        private static TView _instance = SPGENElementManager.GetInstance<TView>();

        public static TView Instance = _instance;

        public static SPGENViewProperties Definition
        {
            get { return _instance.GetDefinition(); }
        }

        public static string UrlFileName
        {
            get { return Definition.UrlFileName; }
        }


        #region Virtual methods

        protected virtual void InitializeDefinition(SPGENViewProperties properties) { }
        protected virtual bool OnProvisionStarted(SPGENViewProperties properties, SPViewCollection viewCollection) { return true; }
        protected virtual void OnProvisionFinalized(SPView view, SPViewCollection viewCollection, bool updatedOnly) { }
        protected virtual bool OnUnprovisionStarted(SPView view, SPViewCollection viewCollection) { return true; }
        protected virtual void OnUnprovisionFinalized(SPViewCollection viewCollection) { }
        
        private readonly object _definitionLock = new object();
        private SPGENViewProperties _definition;
        protected virtual SPGENViewProperties GetDefinition()
        {
            if (_definition != null)
                return _definition;

            lock (_definitionLock)
            {
                if (_definition != null)
                    return _definition;

                var d = GetNewDefinitionInstance();

                EnsureRequiredProperties(d);

                _definition = d;
            }

            return _definition;
        }

        #endregion


        #region Overrided members

        public override SPGENViewProperties InstanceDefinition
        {
            get { return GetDefinition(); }
        }

        internal override SPGENViewProperties StaticDefinition
        {
            get { return Definition; }
        }

        internal override SPView Provision(SPViewCollection viewCollection, bool preserveViewFieldsCollection)
        {
            bool bUpdatedOnly;
            var viewProperties = GetDefinition();

            if (string.IsNullOrEmpty(viewProperties.UrlFileName))
                throw new SPGENGeneralException(string.Format(@"List view '{0}' is missing an UrlFileName value.", this.GetType().FullName));

            if (!OnProvisionStarted(viewProperties, viewCollection))
                return null;

            try
            {
                SPView view = SPGENCommon.CreateOrUpdateView(viewCollection, viewProperties, preserveViewFieldsCollection, null, out bUpdatedOnly);

                OnProvisionFinalized(view, viewCollection, bUpdatedOnly);

                return view;
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException(string.Format(@"Error provisioning list view '{0}'. {1}", this.GetType().FullName, ex.ToString()));
            }
        }

        #endregion


        #region View methods

        public SPGENViewUrlInstance GetView(string listUrl, bool throwExceptionIfNotExists)
        {
            SPGENViewUrlInstance instance = null;

            try
            {
                instance = SPGENViewStorage.Instance.GetUrlInstance(listUrl);
                instance.View = GetView(instance.List, throwExceptionIfNotExists);

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        public SPView GetView(SPList list, bool throwExceptionIfNotExists)
        {
            var coll = list.Views.OfType<SPView>();

            if (throwExceptionIfNotExists)
            {
                return coll.FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + this.InstanceDefinition.UrlFileName, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                try
                {
                    return coll.FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + this.InstanceDefinition.UrlFileName, StringComparison.InvariantCultureIgnoreCase));
                }
                catch
                {
                    return null;
                }
            }
        }


        public void UpdateView(string listUrl, Action<SPView> updateInstanceAction)
        {
            using (var instance = GetView(listUrl, true))
            {
                UpdateView(instance.List, updateInstanceAction);
            }
        }

        public void UpdateView(SPList list, Action<SPView> updateInstanceAction)
        {
            SPView view = GetView(list, true);

            updateInstanceAction.Invoke(view);

            SPGENViewStorage.Instance.UpdateView(view);
        }


        /// <summary>
        /// Checks if the view exists in the specified list URL.
        /// </summary>
        /// <param name="webUrl">The URL to the list.</param>
        /// <returns></returns>
        public bool Exists(string listUrl)
        {
            using (var instance = GetView(listUrl, false))
            {
                return instance.View != null;
            }
        }

        /// <summary>
        /// Checks if the view exists in the specified list.
        /// </summary>
        /// <param name="web">The list object to check on.</param>
        /// <returns></returns>
        public bool Exists(SPList list)
        {
            SPView view = list.Views.OfType<SPView>().FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + this.InstanceDefinition.UrlFileName, StringComparison.InvariantCultureIgnoreCase));

            return view != null;
        }


        public SPGENViewUrlInstance Provision(string listUrl, bool preserveViewFieldsCollection, bool disposeWhenReady)
        {
            SPGENViewUrlInstance instance = null;

            try
            {
                instance = new SPGENViewUrlInstance();
                instance.Site = new SPSite(listUrl);
                instance.Web = instance.Site.OpenWeb();
                instance.List = SPGENListInstanceStorage.Instance.GetListByUrl(instance.Web, listUrl);

                if (instance.List == null)
                    throw new SPGENGeneralException("The list at url '" + listUrl + "' could not be found.");

                instance.View = Provision(instance.List, preserveViewFieldsCollection);

                if (disposeWhenReady)
                    instance.Dispose();

                return instance;
            }
            catch
            {
                if (instance != null)
                    instance.Dispose();

                throw;
            }
        }

        public SPView Provision(SPList list, bool preserveViewFieldsCollection)
        {
            return this.Provision(list.Views, preserveViewFieldsCollection);
        }

        public string GetFullUrl(SPList list)
        {
            SPWeb web = list.ParentWeb;

            string url = web.Url + "/" + list.RootFolder.Url + "/" + this.InstanceDefinition.UrlFileName;

            return url;
        }

        public void Unprovision(SPList list)
        {
            this.Unprovision(list.Views);
        }

        public void Unprovision(SPViewCollection viewCollection)
        {
            SPList list = viewCollection.List;
            SPView view = this.GetView(list, true);

            if (!OnUnprovisionStarted(view, viewCollection))
                return;

            SPGENViewStorage.Instance.DeleteView(list, view.ID);

            OnUnprovisionFinalized(viewCollection);
        }

        #endregion


        #region Private members

        private SPGENViewProperties GetNewDefinitionInstance()
        {
            var properties = SPGENElementProperties.CreateInstance<SPGENViewProperties, SPGENViewAttribute>(_instance.GetType());
            InitializeDefinition(properties);

            return properties;
        }

        protected void EnsureRequiredProperties(SPGENViewProperties properties)
        {
            if (string.IsNullOrEmpty(properties.UrlFileName))
            {
                throw new SPGENGeneralException("The parameter UrlFileName is not specified for view element " + this.GetType().FullName);
            }
        }

        #endregion
    }

}
