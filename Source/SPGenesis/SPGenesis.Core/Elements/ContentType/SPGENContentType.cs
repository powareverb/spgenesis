using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    /// <summary>
    /// Base class for field elements.
    /// </summary>
    /// <typeparam name="TContentType">The derived type.</typeparam>
    public class SPGENContentType<TContentType> : SPGENContentTypeBase
        where TContentType : SPGENContentType<TContentType>, new()
    {
        private static TContentType _instance = SPGENElementManager.GetInstance<TContentType>();

        /// <summary>
        /// A set of methods available for this content type element.
        /// </summary>
        public static TContentType Instance = _instance;

        /// <summary>
        /// Returns the static definition of this conent type element.
        /// </summary>
        public static SPGENContentTypeProperties Definition
        {
            get { return _instance.GetDefinition(); }
        }

        /// <summary>
        /// Returns the ID for this content type definition.
        /// </summary>
        public static SPContentTypeId ID
        {
            get { return Definition.ID; }
        }

        /// <summary>
        /// Returns the name for this content type definition.
        /// </summary>
        public static string Name
        {
            get { return Definition.Name; }
        }


        #region Virtual methods

        /// <summary>
        /// This method is called when the element gets initialized. All changes to the properties for this element must be made here.
        /// </summary>
        /// <param name="properties">The element properties.</param>
        protected virtual void InitializeDefinition(SPGENContentTypeProperties properties) { }

        /// <summary>
        /// This method is called when the element gets provisioned.
        /// </summary>
        /// <param name="contentTypeProperties">The element properties that this element will be provisioned with.</param>
        /// <param name="contentTypeCollection">The content type collection that the element is getting provisioned to.</param>
        /// <param name="isParentList">If the content type collection is a list.</param>
        /// <returns>Must return true to continue the provisioning. If false is returned, it will cancel the operation.</returns>
        protected virtual bool OnProvisionStarted(SPGENContentTypeProperties contentTypeProperties, SPContentTypeCollection contentTypeCollection, bool isParentList) { return true; }

        /// <summary>
        /// This method is called before the provisioning of this element is finalized.
        /// </summary>
        /// <param name="contentType">The content type object that has been provisioned.</param>
        /// <param name="isParentList">If the content type collection is a list.</param>
        /// <returns>Must return true to finalize the provisioning. If false is returned, it will cancel the operation.</returns>
        protected virtual bool OnProvisionBeforeFinalization(SPContentType contentType, bool isParentList) { return true; }

        /// <summary>
        /// This method is called when the element is provisioned.
        /// </summary>
        /// <param name="contentType">The content type object that has been provisioned.</param>
        /// <param name="isParentList">If the content type collection is a list.</param>
        protected virtual void OnProvisionFinalized(SPContentType contentType, bool isParentList) { }

        /// <summary>
        /// This method is called when the element gets unprovisioned.
        /// </summary>
        /// <param name="contentType">The content type object that will be unprovisioned.</param>
        /// <param name="isParentList">If the content type collection is a list.</param>
        /// <returns>Must return true to finalize the provisioning. If false is returned, it will cancel the operation.</returns>
        protected virtual bool OnUnprovisionStarted(SPContentType contentType, bool isParentList) { return true; }

        /// <summary>
        /// This method is called when the element is unprovisioned.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection that the element was provisioned from.</param>
        /// <param name="isParentList">If the content type collection is a list.</param>
        protected virtual void OnUnprovisionFinalized(SPContentTypeCollection contentTypeCollection, bool isParentList) { }

        private readonly object _definitionLock = new object();
        private SPGENContentTypeProperties _definition;
        /// <summary>
        /// This method is called when the definition is fetched. You should not override this method if the default behavior is adequate. Override it only if you want to change the behavior for how elements are initialized with data from SP definitions.
        /// </summary>
        /// <returns>The content type properties for this element.</returns>
        protected virtual SPGENContentTypeProperties GetDefinition()
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

        public override SPGENContentTypeProperties InstanceDefinition
        {
            get { return GetDefinition(); }
        }

        internal override SPGENContentTypeProperties StaticDefinition
        {
            get { return Definition; }
        }

        internal override Action<SPGENContentTypeProvisioningArguments> OnProvisionerAction { get; set; }

        internal override SPContentType Provision(SPContentTypeCollection contentTypeCollection, SPList list, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate)
        {
            SPContentType ct;
            bool bNewCT = false;
            bool isCollectionFromList = (list != null);

            var properties = GetDefinition();

            if (!FireOnProvisionStarted(properties, contentTypeCollection, isCollectionFromList))
                return null;

            ct = SPGENContentTypeStorage.Instance.GetContentType(contentTypeCollection, properties.ID, isCollectionFromList);

            if (ct == null)
            {
                if (!isCollectionFromList)
                {
                    bNewCT = true;

                    ct = SPGENContentTypeStorage.Instance.CreateNewContentType(contentTypeCollection, properties.ID, properties.Name);
                }
                else
                {
                    SPContentType webContentType = list.ParentWeb.AvailableContentTypes[properties.ID];
                    if (webContentType == null)
                        throw new SPGENGeneralException("The contentype '" + properties.ID.ToString() + "' could not be found at url " + list.ParentWeb.Url);

                    ct = SPGENContentTypeStorage.Instance.AddContentTypeToCollection(contentTypeCollection, webContentType);
                }
            }
            else
            {
                if (!updateIfExists)
                    return ct;
            }


            SPGENCommon.CopyAttributeToContentType(properties, ct);

            if (this.OnProvisionerAction != null)
            {
                this.OnProvisionerAction.Invoke(
                    new SPGENContentTypeProvisioningArguments()
                    {
                        ContentType = ct,
                        FieldLinks = properties.FieldLinks,
                        FieldLinksToRemove = properties.FieldLinksToRemove,
                        EventReceivers = properties.EventReceivers
                    });
            }



            //Provision field links
            IList<SPGENFieldBase> listOfFieldElements;
            properties.FieldLinks.Provision(ref ct, false, out listOfFieldElements);

            foreach (var g in properties.FieldLinksToRemove)
            {
                if (ct.FieldLinks[g] != null)
                {
                    ct.FieldLinks.Delete(g);
                }
            }


            if (ct.EventReceivers != null)
            {
                properties.EventReceivers.Provision(ct.EventReceivers);
            }

            if (!FireOnProvisionBeforeFinalization(ct, isCollectionFromList))
                return ct;


            if (bNewCT)
            {
                SPGENContentTypeStorage.Instance.AddContentTypeToCollection(contentTypeCollection, ct);
            }
            else
            {
                //Update the content type
                SPGENContentTypeStorage.Instance.UpdateContentType(ct, updateChildren, stopOnSealedOrReadOnlyUpdate);
            }


            //Run OnProvisionFinalized
            FireOnProvisionFinalized(ct, isCollectionFromList);


            //Call onprovision method on field elements previously added in field link collection.
            if (ct.ParentList != null)
            {
                var removedItems = properties.FieldLinks.GetAllRemovedItems();

                foreach (var fieldInstance in listOfFieldElements)
                {
                    var fieldDefinition = fieldInstance.InstanceDefinition;

                    if (removedItems.FirstOrDefault(fl => fl.ID == fieldDefinition.ID) != null)
                        continue;


                    SPField f = list.Fields[fieldDefinition.ID];
                    if (fieldDefinition.IsPropertyValueUpdated("RelationshipDeleteBehavior"))
                    {
                        if (f is SPFieldLookup)
                        {
                            (f as SPFieldLookup).RelationshipDeleteBehavior = fieldDefinition.RelationshipDeleteBehavior;

                            SPGENFieldStorage.Instance.UpdateField(f, false);
                        }
                    }

                    fieldInstance.FireOnProvisionFinalized(list.Fields[fieldDefinition.ID], list.Fields, false, false);
                }
            }


            return ct;
        }

        internal override void Unprovision(SPContentTypeCollection contentTypeCollection, bool isCollectionFromList, bool deleteAllUsages, bool ignoreError)
        {
            SPContentType ct = SPGENContentTypeStorage.Instance.GetContentType(contentTypeCollection, this.InstanceDefinition.ID, isCollectionFromList);

            if (ct == null)
                return;


            if (!FireOnUnprovisionStarted(ct, isCollectionFromList))
                return;

            if (deleteAllUsages)
            {
                var coll = SPContentTypeUsage.GetUsages(ct);

                foreach (var usage in coll)
                {
                    try
                    {
                        using (SPWeb web = ct.ParentWeb.Site.OpenWeb(usage.Url))
                        {
                            SPContentType contentTypeToDelete;
                            if (usage.IsUrlToList)
                            {
                                SPList list = web.GetList(web.Site.MakeFullUrl(usage.Url));
                                contentTypeToDelete = GetContentType(list);
                            }
                            else
                            {
                                contentTypeToDelete = GetContentType(web);
                            }

                            SPGENContentTypeStorage.Instance.DeleteContentType(contentTypeToDelete);
                        }
                    }
                    catch
                    {
                        if (!ignoreError)
                            throw;
                    }
                }

            }

            try
            {
                SPGENContentTypeStorage.Instance.DeleteContentType(ct);
            }
            catch
            {
                if (!ignoreError)
                    throw;
            }

            FireOnUnprovisionFinalized(contentTypeCollection, isCollectionFromList);
        }

        #endregion


        #region Content type methods

        /// <summary>
        /// Gets the content type from the specified URL.
        /// </summary>
        /// <param name="url">The URL to the content type. The URL can either be an URL to a list or web.</param>
        /// <returns>An URL instance object. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public SPGENContentTypeUrlInstance GetContentType(string url)
        {
            SPGENContentTypeUrlInstance instance = SPGENContentTypeStorage.Instance.CreateUrlInstance(url);

            try
            {
                if (instance.List != null)
                {
                    instance.ContenType = GetContentType(instance.List);
                }
                else
                {
                    instance.ContenType = GetContentType(instance.Web);
                }

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Gets the content type from the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to get the content type from.</param>
        /// <returns>The content type object.</returns>
        public SPContentType GetContentType(SPWeb web)
        {
            SPContentType contentType = GetContentTypeInternal(web.ContentTypes, false, false);
            if (contentType != null)
                return contentType;

            return GetContentTypeInternal(web.AvailableContentTypes, false, true);
        }

        /// <summary>
        /// Gets the content type from the specified web instance.
        /// </summary>
        /// <param name="list">The list instance to get the content type from.</param>
        /// <returns>The content type object.</returns>
        public SPContentType GetContentType(SPList list)
        {
            return GetContentTypeInternal(list.ContentTypes, true, true);
        }

        /// <summary>
        /// Try get the content type from the specified URL. If the method fails to find the content type, it will return null.
        /// </summary>
        /// <param name="url">The URL to the content type. The URL can either be an URL to a list or web.</param>
        /// <returns>An URL instance object or null if the content type was not found. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public SPGENContentTypeUrlInstance TryGetInstance(string url)
        {
            if (!Exists(url))
                return null;

            return GetContentType(url);
        }

        /// <summary>
        /// Try get the content type from the specified web instance. If the method fails to find the content type, it will return null.
        /// </summary>
        /// <param name="web">The web instance to get the content type from.</param>
        /// <returns>The content type object.</returns>
        public SPContentType TryGetContentType(SPWeb web)
        {
            if (!Exists(web))
                return null;

            return GetContentType(web);
        }

        /// <summary>
        /// Try get the content type from the specified list instance. If the method fails to find the content type, it will return null.
        /// </summary>
        /// <param name="list">The list instance to get the content type from.</param>
        /// <returns>The content type object.</returns>
        public SPContentType TryGetContentType(SPList list)
        {
            if (!Exists(list))
                return null;

            return GetContentType(list);
        }

        /// <summary>
        /// Updates the content type on the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to update the content type on.</param>
        /// <param name="updateChildren">Updates children content types.</param>
        /// <param name="stopOnSealdOrReadOnlyUpdate">Stops if the child content type is sealed or read only.</param>
        /// <param name="updateInstanceAction">Function to set the content type properties when updating.</param>
        public void UpdateContentType(SPWeb web, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate, Action<SPContentType> updateInstanceAction)
        {
            SPGENContentTypeStorage.Instance.EnsureCollectionIsUpdateble(web, this.InstanceDefinition.ID);
            var contentType = SPGENContentTypeStorage.Instance.GetContentType(web.ContentTypes, this.InstanceDefinition.ID, false);

            updateInstanceAction.Invoke(contentType);
            SPGENContentTypeStorage.Instance.UpdateContentType(contentType, updateChildren, stopOnSealdOrReadOnlyUpdate);
        }

        /// <summary>
        /// Updates the content type on the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to update the content type on.</param>
        /// <param name="updateChildren">Updates children content types.</param>
        /// <param name="stopOnSealdOrReadOnlyUpdate">Stops if the child content type is sealed or read only.</param>
        /// <param name="updateInstanceAction">Function to set the content type properties when updating.</param>
        public void UpdateContentType(SPList list, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate, Action<SPContentType> updateInstanceAction)
        {
            var contentType = SPGENContentTypeStorage.Instance.GetContentType(list.ContentTypes, this.InstanceDefinition.ID, true);

            updateInstanceAction.Invoke(contentType);
            SPGENContentTypeStorage.Instance.UpdateContentType(contentType, updateChildren, stopOnSealdOrReadOnlyUpdate);
        }

        /// <summary>
        /// Updates the content type on the specified URL.
        /// </summary>
        /// <param name="url">The URL to the content type. The URL can either be an URL to a list or web.</param>
        /// <param name="updateChildren">Updates children content types.</param>
        /// <param name="stopOnSealdOrReadOnlyUpdate">Stops if the child content type is sealed or read only.</param>
        /// <param name="updateInstanceAction">Function to set the content type properties when updating.</param>
        public void UpdateContentType(string url, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate, Action<SPContentType> updateInstanceAction)
        {
            using (var instance = SPGENContentTypeStorage.Instance.CreateUrlInstance(url))
            {
                if (instance.List != null)
                {
                    UpdateContentType(instance.List, updateChildren, stopOnSealdOrReadOnlyUpdate, updateInstanceAction);
                }
                else
                {
                    UpdateContentType(instance.Web, updateChildren, stopOnSealdOrReadOnlyUpdate, updateInstanceAction);
                }
            }
        }

        /// <summary>
        /// Checks if the content type exists at the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <returns>Returns true if the content type exists.</returns>
        public bool Exists(string url)
        {
            using (var instance = SPGENContentTypeStorage.Instance.CreateUrlInstance(url))
            {
                return instance.ContenType != null;
            }
        }

        /// <summary>
        /// Checks if the content type exists at the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to check on.</param>
        /// <returns>Returns true if the content type exists.</returns>
        public bool Exists(SPWeb web)
        {
            return GetContentTypeInternal(web.AvailableContentTypes, false, false) != null;
        }

        /// <summary>
        /// Checks if the content type exists at the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to check on.</param>
        /// <returns>Returns true if the content type exists.</returns>
        public bool Exists(SPList list)
        {
            return GetContentTypeInternal(list.ContentTypes, true, false) != null;
        }

        /// <summary>
        /// Adds or updates the content type on the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="disposeWhenReady">True to dispose the site and web objects created by this method when the provisioning is finished.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPGENContentTypeUrlInstance Provision(string url, bool disposeWhenReady)
        {
            return Provision(url, true, true, true, disposeWhenReady);
        }

        /// <summary>
        /// Adds or updates the content type on the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="updateIfExists">True if you want to update the content type if it already exists.</param>
        /// <param name="updateChildren">True if you want to update all child content types.</param>
        /// <param name="stopOnSealdOrReadOnlyUpdate">True if you want to stop the process if any of the child content types are seald or read only.</param>
        /// <param name="disposeWhenReady">True to dispose the site and web objects created by this method when the provisioning is finished.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPGENContentTypeUrlInstance Provision(string url, bool updateIfExists, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate, bool disposeWhenReady)
        {
            SPGENContentTypeUrlInstance instance = null;

            try
            {
                instance = new SPGENContentTypeUrlInstance();
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();

                try
                {
                    instance.List = instance.Web.GetList(url);
                }
                catch { }

                if (instance.List != null)
                {
                    instance.ContenType = Provision(instance.List, updateIfExists, updateChildren, stopOnSealdOrReadOnlyUpdate);
                }
                else
                {
                    instance.ContenType = Provision(instance.Web, updateIfExists, updateChildren, stopOnSealdOrReadOnlyUpdate);
                }

                if (disposeWhenReady)
                {
                    instance.Dispose();
                }

                return instance;
            }
            catch
            {
                if (instance != null)
                    instance.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Adds or updates the content type on the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to provision on.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPContentType Provision(SPWeb web)
        {
            return Provision(web, true, true, true);
        }

        /// <summary>
        /// Adds or updates the content type on the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to provision on.</param>
        /// <param name="updateIfExists">True if you want to update the content type if it already exists.</param>
        /// <param name="updateChildren">True if you want to update all child content types.</param>
        /// <param name="stopOnSealedOrReadOnlyUpdate">True if you want to stop the process if any of the child content types are seald or read only.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPContentType Provision(SPWeb web, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate)
        {
            return Provision(web.ContentTypes, null, updateIfExists, updateChildren, stopOnSealedOrReadOnlyUpdate);
        }

        /// <summary>
        /// Adds or updates the content type on the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to provision on.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPContentType Provision(SPList list)
        {
            return Provision(list, true, true, true);
        }

        /// <summary>
        /// Adds or updates the content type on the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to provision on.</param>
        /// <param name="updateIfExists">True if you want to update the content type if it already exists.</param>
        /// <param name="updateChildren">True if you want to update all child content types.</param>
        /// <param name="stopOnSealedOrReadOnlyUpdate">True if you want to stop the process if any of the child content types are seald or read only.</param>
        /// <returns>The provisioned content type object.</returns>
        public SPContentType Provision(SPList list, bool updateIfExists, bool updateChildren, bool stopOnSealedOrReadOnlyUpdate)
        {
            return Provision(list.ContentTypes, list, updateIfExists, updateChildren, stopOnSealedOrReadOnlyUpdate);
        }

        /// <summary>
        /// Removes the content type from the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        public void Unprovision(string url)
        {
            Unprovision(url, false, false);
        }

        /// <summary>
        /// Removes the content type from the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="deleteAllUsages">Deletes all usages of this content type on this site.</param>
        /// <param name="ignoreError">Ignores any errors that may occur.</param>
        public void Unprovision(string url, bool deleteAllUsages, bool ignoreError)
        {
            using (var instance = SPGENContentTypeStorage.Instance.CreateUrlInstance(url))
            {
                if (instance.List != null)
                {
                    Unprovision(instance.List, deleteAllUsages, ignoreError);
                }
                else
                {
                    Unprovision(instance.Web, deleteAllUsages, ignoreError);
                }
            }
        }

        /// <summary>
        /// Removes the content type from the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to provision on.</param>
        public void Unprovision(SPWeb web)
        {
            Unprovision(web, false, false);
        }

        /// <summary>
        /// Removes the content type from the specified web instance.
        /// </summary>
        /// <param name="web">The web instance to provision on.</param>
        /// <param name="deleteAllUsages">Deletes all usages of this content type on this site.</param>
        /// <param name="ignoreError">Ignores any errors that may occur.</param>
        public void Unprovision(SPWeb web, bool deleteAllUsages, bool ignoreError)
        {
            Unprovision(web.ContentTypes, false, deleteAllUsages, ignoreError);
        }

        /// <summary>
        /// Removes the content type from the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to provision on.</param>
        public void Unprovision(SPList list)
        {
            Unprovision(list, false, false);
        }

        /// <summary>
        /// Removes the content type from the specified list instance.
        /// </summary>
        /// <param name="list">The list instance to provision on.</param>
        /// <param name="deleteAllUsages">Deletes all usages of this content type on this site.</param>
        /// <param name="ignoreError">Ignores any errors that may occur.</param>
        public void Unprovision(SPList list, bool deleteAllUsages, bool ignoreError)
        {
            Unprovision(list.ContentTypes, true, deleteAllUsages, ignoreError);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified URL.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        public void RegisterEventReceiver<TEventReceiver>(string url, bool updateChildren) where TEventReceiver : SPEventReceiverBase
        {
            RegisterEventReceiver<TEventReceiver>(url, updateChildren, false);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified URL.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        public void RegisterEventReceiver<TEventReceiver>(string url, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            using (var instance = GetContentType(url))
            {
                if (instance.List != null)
                {
                    RegisterEventReceivers<TEventReceiver>(instance.List, updateChildren, keepOnlyDeclaredMethods);
                }
                else
                {
                    RegisterEventReceiver<TEventReceiver>(instance.Web, updateChildren, keepOnlyDeclaredMethods);
                }
            }
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified URL.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        public void RegisterEventReceiver<TEventReceiver>(string url, int sequenceNumber, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            using (var instance = GetContentType(url))
            {
                if (instance.List != null)
                {
                    RegisterEventReceivers<TEventReceiver>(instance.List, sequenceNumber, updateChildren, keepOnlyDeclaredMethods);
                }
                else
                {
                    RegisterEventReceiver<TEventReceiver>(instance.Web, sequenceNumber, updateChildren, keepOnlyDeclaredMethods);
                }
            }
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified web instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="web">The web instance to update the content type on.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceiver<TEventReceiver>(SPWeb web, bool updateChildren) where TEventReceiver : SPEventReceiverBase
        {
            return RegisterEventReceiver<TEventReceiver>(web, updateChildren, false);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified web instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="web">The web instance to update the content type on.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceiver<TEventReceiver>(SPWeb web, int sequenceNumber, bool updateChildren) where TEventReceiver : SPEventReceiverBase
        {
            return RegisterEventReceiver<TEventReceiver>(web, sequenceNumber, updateChildren, false);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified web instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="web">The web instance to update the content type on.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceiver<TEventReceiver>(SPWeb web, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            var contentType = GetContentType(web);

            return RegisterEventReceiverInternal(typeof(TEventReceiver), null, contentType, this.InstanceDefinition.ID, updateChildren, keepOnlyDeclaredMethods);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified web instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="web">The web instance to update the content type on.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceiver<TEventReceiver>(SPWeb web, int sequenceNumber, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            var contentType = GetContentType(web);

            return RegisterEventReceiverInternal(typeof(TEventReceiver), sequenceNumber, contentType, this.InstanceDefinition.ID, updateChildren, keepOnlyDeclaredMethods);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified list instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="list">The list instance to update the content type on.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceivers<TEventReceiver>(SPList list, bool updateChildren) where TEventReceiver : SPEventReceiverBase
        {
            return RegisterEventReceivers<TEventReceiver>(list, updateChildren, false);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified list instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="list">The list instance to update the content type on.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceivers<TEventReceiver>(SPList list, int sequenceNumber, bool updateChildren) where TEventReceiver : SPEventReceiverBase
        {
            return RegisterEventReceivers<TEventReceiver>(list, updateChildren, false);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified list instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="list">The list instance to update the content type on.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceivers<TEventReceiver>(SPList list, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            var contentType = GetContentType(list);

            return RegisterEventReceiverInternal(typeof(TEventReceiver), null, contentType, this.InstanceDefinition.ID, updateChildren, keepOnlyDeclaredMethods);
        }

        /// <summary>
        /// Register an event receiver for the content type on the specified list instance.
        /// </summary>
        /// <typeparam name="TEventReceiver">The event receiver type.</typeparam>
        /// <param name="list">The list instance to update the content type on.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="updateChildren">Updates child content types.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns>A content type object instance.</returns>
        public SPContentType RegisterEventReceivers<TEventReceiver>(SPList list, int sequenceNumber, bool updateChildren, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            var contentType = GetContentType(list);

            return RegisterEventReceiverInternal(typeof(TEventReceiver), sequenceNumber, contentType, this.InstanceDefinition.ID, updateChildren, keepOnlyDeclaredMethods);
        }

        /// <summary>
        /// Returs all usages for this content type at the specified URL.
        /// </summary>
        /// <param name="url">The URL can either be an URL to a list or web.</param>
        /// <returns>List of all usages.</returns>
        public IList<SPContentTypeUsage> GetUsage(string url)
        {
            using (var ct = GetContentType(url))
            {
                var usages = SPContentTypeUsage.GetUsages(ct.ContenType);

                return usages;
            }
        }

        /// <summary>
        /// Adds a field link on the specified content type instance.
        /// </summary>
        /// <typeparam name="TField">A SPGENField type to add as a field link.</typeparam>
        /// <param name="contentType">Th content type instance to add the field link on.</param>
        /// <param name="ignoreIfExists">True if it should ignore if the link already exists.</param>
        /// <returns>A SPFieldLink object instance.</returns>
        public SPFieldLink AddSPFieldLink<TField>(SPContentType contentType, bool ignoreIfExists)
            where TField : SPGENFieldBase, new()
        {
            return contentType.AddSPGENFieldLink<TField>(ignoreIfExists);
        }


        #endregion


        #region Private members

        private SPGENContentTypeProperties GetNewDefinitionInstance()
        {
            var properties = SPGENElementProperties.CreateInstance<SPGENContentTypeProperties, SPGENContentTypeAttribute>(this.GetType());
            properties.EventReceivers.AddReceiversFromElementAttributes(this.GetType(), C_EVENT_TYPES);

            InitializeDefinition(properties);

            return properties;
        }

        protected void EnsureRequiredProperties(SPGENContentTypeProperties properties)
        {
            if (properties.ID == SPContentTypeId.Empty)
            {
                throw new SPGENGeneralException("The parameter ID is not specified for content type element " + this.GetType().FullName);
            }

            if (string.IsNullOrEmpty(properties.Name))
            {
                throw new SPGENGeneralException("The parameter Name is not specified for content type element " + this.GetType().FullName);
            }

        }

        private SPContentType GetContentTypeInternal(SPContentTypeCollection contentTypeCollection, bool parentIsList, bool throwExceptionIfNotExists)
        {
            var ct = SPGENContentTypeStorage.Instance.GetContentType(contentTypeCollection, this.InstanceDefinition.ID, parentIsList);
            if (ct == null && throwExceptionIfNotExists)
            {
                throw new SPGENGeneralException("The content type '" + this.GetType().FullName + "' doesn't exist in the specfied content type collection.");
            }
            else
            {
                return ct;
            }
        }

        private bool FireOnProvisionStarted(SPGENContentTypeProperties contentTypeProperties, SPContentTypeCollection contentTypeCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnProvisionStarted(contentTypeProperties, contentTypeCollection, isParentList);
            }
            else
            {
                return true;
            }
        }

        private bool FireOnProvisionBeforeFinalization(SPContentType contentType, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnProvisionBeforeFinalization(contentType, isParentList);
            }
            else
            {
                return true;
            }
        }

        private void FireOnProvisionFinalized(SPContentType contentType, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                OnProvisionFinalized(contentType, isParentList);
            }
        }

        private bool FireOnUnprovisionStarted(SPContentType contentType, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnUnprovisionStarted(contentType, isParentList);
            }
            else
            {
                return true;
            }
        }

        private void FireOnUnprovisionFinalized(SPContentTypeCollection contentTypeCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                OnUnprovisionFinalized(contentTypeCollection, isParentList);
            }
        }

        private bool ShouldCallProvisionEvents(bool isParentList)
        {
            if (isParentList && this.InstanceDefinition.ProvisionEventCallBehavior != SPGENProvisionEventCallBehavior.OnWeb)
            {
                return true;
            }
            else if (!isParentList && this.InstanceDefinition.ProvisionEventCallBehavior != SPGENProvisionEventCallBehavior.OnList)
            {
                return true;
            }

            return false;
        }


        private SPContentType RegisterEventReceiverInternal(Type receiver, int? sequenceNumber, SPContentType contentType, SPContentTypeId contentTypeId, bool updateChildren, bool keepOnlyDeclaredMethods)
        {
            var col = new SPGENEventReceiverCollection();

            if (sequenceNumber.HasValue)
            {
                col.AddType(receiver, sequenceNumber.Value, null, keepOnlyDeclaredMethods);
            }
            else
            {
                col.AddType(receiver, null, keepOnlyDeclaredMethods);
            }

            col.Provision(contentType.EventReceivers);

            SPGENContentTypeStorage.Instance.UpdateContentType(contentType, updateChildren, true);

            return contentType;
        }

        private readonly SPEventReceiverType[] C_EVENT_TYPES =
            new SPEventReceiverType[] { 
                    SPEventReceiverType.ContextEvent,
                    SPEventReceiverType.EmailReceived,
                    SPEventReceiverType.ItemAdded,
                    SPEventReceiverType.ItemAdding,
                    SPEventReceiverType.ItemAttachmentAdded,
                    SPEventReceiverType.ItemAttachmentAdding,
                    SPEventReceiverType.ItemAttachmentDeleted,
                    SPEventReceiverType.ItemAttachmentDeleting,
                    SPEventReceiverType.ItemCheckedIn,
                    SPEventReceiverType.ItemCheckedOut,
                    SPEventReceiverType.ItemCheckingIn,
                    SPEventReceiverType.ItemCheckingOut,
                    SPEventReceiverType.ItemDeleted,
                    SPEventReceiverType.ItemDeleting,
                    SPEventReceiverType.ItemFileConverted,
                    SPEventReceiverType.ItemFileMoved,
                    SPEventReceiverType.ItemFileMoving,
                    SPEventReceiverType.ItemUncheckedOut,
                    SPEventReceiverType.ItemUncheckingOut,
                    SPEventReceiverType.ItemUpdated,
                    SPEventReceiverType.ItemUpdating
                };

        #endregion
    
    }

}
