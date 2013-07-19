using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENListInstance<TListInstance> : SPGENListInstanceBase
        where TListInstance : SPGENListInstance<TListInstance>, new()
    {
        private static TListInstance _instance = SPGENElementManager.GetInstance<TListInstance>();

        /// <summary>
        /// A set of methods available for this list instance element.
        /// </summary>
        public static TListInstance Instance = _instance;

        /// <summary>
        /// Returns the static definition of this list instance element.
        /// </summary>
        public static SPGENListInstanceProperties Definition
        {
            get { return _instance.GetDefinition(); }
        }

        /// <summary>
        /// Returns the relative web URL for this list instance element.
        /// </summary>
        public static string WebRelURL
        {
            get { return Definition.WebRelURL; }
        }

        /// <summary>
        /// Returns the list title from the definition.
        /// </summary>
        public static string Title
        {
            get { return Definition.Title; }
        }

        
        #region Virtual methods

        /// <summary>
        /// This method is called when the element gets initialized. All changes to the properties for this element must be made here.
        /// </summary>
        /// <param name="properties">The element properties.</param>
        protected virtual void InitializeDefinition(SPGENListInstanceProperties properties) { }

        /// <summary>
        /// Fires when provisioning of this list instance starts (before the list instance is created or updated). Return true to continue or false to cancel the provisioning.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <returns>Return true to continue or false to cancel the provisioning.</returns>
        protected virtual bool OnProvisionStarted(SPGENListInstanceProperties listInstanceProperties, SPWeb web) { return true; }

        /// <summary>
        /// Fires before the whole provisioning sequence is finished. Return true to continue or false to cancel the provisioning.
        /// </summary>
        /// <param name="list">The list instance.</param>
        /// <returns>Return true to continue or false to cancel the provisioning.</returns>
        protected virtual bool OnProvisionBeforeFinalization(SPList list) { return true; }

        /// <summary>
        /// Fires when the whole provisioning sequence is finished. 
        /// </summary>
        /// <param name="list">The list instance.</param>
        protected virtual void OnProvisionFinalized(SPList list) { }

        /// <summary>
        /// Fires when the unprovisioning sequence is finished (list is deleted).
        /// </summary>
        /// <param name="parentWeb"></param>
        protected virtual void OnUnprovisioned(SPWeb parentWeb) { }


        private readonly object _definitionLock = new object();
        private SPGENListInstanceProperties _definition;
        /// <summary>
        /// This method is called when the definition is fetched. You should not override this method if the default behavior is adequate. Override it only if you want to change the behavior for how elements are initialized with data from SP definitions.
        /// </summary>
        /// <returns>The list instance properties for this element.</returns>
        protected virtual SPGENListInstanceProperties GetDefinition()
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

        public sealed override SPGENListInstanceProperties InstanceDefinition
        {
            get { return GetDefinition(); }
        }

        internal override SPGENListInstanceProperties StaticDefinition
        {
            get { return Definition; }
        }

        internal override Action<SPGENListProvisioningArguments> OnProvisionerAction { get; set; }

        #endregion


        #region List instance methods

        /// <summary>
        /// Gets a list instance from the specified web URL. You should dispose the returned object to save system resources.
        /// </summary>
        /// <param name="webUrl">The URL to the site.</param>
        /// <returns>A proxy object containing a references to the specific list instance. The returned object should be disposed when it isn't needed any more.</returns>
        public SPGENListInstanceUrlInstance GetList(string webUrl)
        {
            return GetList(webUrl, this.InstanceDefinition.GetListDefaultMethod);
        }

        public SPGENListInstanceUrlInstance GetList(string webUrl, SPGENListInstanceGetMethod method)
        {
            SPGENListInstanceUrlInstance instance = null;

            try
            {
                instance = SPGENListInstanceStorage.Instance.CreateUrlInstance(webUrl);
                instance.List = GetInstanceInternal(instance.Web, method, true, true);

                return instance;
            }
            catch
            {
                if (instance != null)
                    instance.Dispose();

                throw;
            }
        }

        [Obsolete("This method is not longer available. Use the overload that accepts the parameter 'SPGENListInstanceGetMethod' instead.", true)]
        public SPGENListInstanceUrlInstance GetList(string webUrl, bool useGetFromUrlMethod) { throw new NotSupportedException(); }

        /// <summary>
        /// Gets a specific instance of this list instance in the specified web.
        /// </summary>
        /// <param name="web">The parent web object to fetch the list instance from.</param>
        /// <returns>Returns an SPList object instance.</returns>
        public override SPList GetList(SPWeb web)
        {
            return GetInstanceInternal(web, this.InstanceDefinition.GetListDefaultMethod, true, true);
        }

        public SPList GetList(SPWeb web, SPGENListInstanceGetMethod method)
        {
            return GetInstanceInternal(web, method, true, true);
        }

        [Obsolete("This method is not longer available. Use the overload that accepts the parameter 'SPGENListInstanceGetMethod' instead.", true)]
        public SPList GetList(SPWeb web, bool useGetFromUrlMethod) { throw new NotSupportedException(); }

        public SPGENListInstanceUrlInstance TryGetList(string webUrl)
        {
            return TryGetList(webUrl, this.InstanceDefinition.GetListDefaultMethod);
        }

        public SPGENListInstanceUrlInstance TryGetList(string webUrl, SPGENListInstanceGetMethod method)
        {
            return TryGetList(webUrl, this.InstanceDefinition.GetListDefaultMethod, false);
        }

        public SPGENListInstanceUrlInstance TryGetList(string webUrl, SPGENListInstanceGetMethod method, bool catchUnauthorizedException)
        {
            SPGENListInstanceUrlInstance instance = null;

            try
            {
                instance = SPGENListInstanceStorage.Instance.CreateUrlInstance(webUrl);
                instance.List = GetInstanceInternal(instance.Web, method, false, !catchUnauthorizedException);

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        [Obsolete("This method is not longer available. Use the overload that accepts the parameter 'SPGENListInstanceGetMethod' instead.", true)]
        public SPGENListInstanceUrlInstance TryGetList(string webUrl, bool useGetFromUrlMethod) { throw new NotSupportedException(); }

        public SPList TryGetList(SPWeb web)
        {
            return GetInstanceInternal(web, this.InstanceDefinition.GetListDefaultMethod, false, true);
        }

        public SPList TryGetList(SPWeb web, SPGENListInstanceGetMethod method)
        {
            return GetInstanceInternal(web, method, false, true);
        }

        public SPList TryGetList(SPWeb web, SPGENListInstanceGetMethod method, bool catchUnauthorizedException)
        {
            return GetInstanceInternal(web, method, false, !catchUnauthorizedException);
        }

        [Obsolete("This method is not longer available. Use the overload that accepts the parameter 'SPGENListInstanceGetMethod' instead.", true)]
        public SPList TryGetList(SPWeb web, bool useGetFromUrlMethod) { throw new NotSupportedException(); }


        /// <summary>
        /// Checks if the list instance exists at the specified web URL.
        /// </summary>
        /// <param name="webUrl">The URL to the site.</param>
        /// <returns></returns>
        public bool Exists(string webUrl)
        {
            using (SPSite site = new SPSite(webUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    return Exists(web);
                }
            }
        }

        /// <summary>
        /// Checks if the list instance exists in the specified web.
        /// </summary>
        /// <param name="web">The parent web object to check on.</param>
        /// <returns></returns>
        public bool Exists(SPWeb web)
        {
            return GetInstanceInternal(web, this.InstanceDefinition.GetListDefaultMethod, false, true) != null;
        }


        /// <summary>
        /// Updates a specific instance of this list instance on the specified web.
        /// </summary>
        /// <param name="web">The parent web object to update the list instance on.</param>
        /// <param name="updateInstanceAction">A lamba expression to use when updating the list instance.</param>
        public void UpdateList(SPWeb web, Action<SPList> updateInstanceAction)
        {
            SPList list = GetList(web);

            updateInstanceAction.Invoke(list);

            SPGENListInstanceStorage.Instance.UpdateList(list);
        }

        /// <summary>
        /// Updates a specific instance of this list instance at the specified web URL.
        /// </summary>
        /// <param name="webUrl">The URL to the site.</param>
        /// <param name="updateInstanceAction">A lamba expression to use when updating the list instance.</param>
        public void UpdateList(string webUrl, Action<SPList> updateInstanceAction)
        {
            using (var instance = GetList(webUrl))
            {
                UpdateList(instance.Web, updateInstanceAction);
            }
        }


        /// <summary>
        /// Adds a list item to the a specific list instance.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="addListItemFunction">A custom action providing the added SPListItem object. It will automatically update the item after the function has finished.</param>
        /// <returns></returns>
        public SPListItem AddListItem(SPWeb web, Action<SPListItem> addListItemFunction)
        {
            return AddListItem(web, addListItemFunction, true);
        }

        /// <summary>
        /// Adds a list item to the a specific list instance.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="addListItemFunction">A custom action providing the added SPListItem object.</param>
        /// <param name="updateItemWhenReady">Updates the list item when after the custom update action has finished.</param>
        /// <returns></returns>
        public SPListItem AddListItem(SPWeb web, Action<SPListItem> addListItemFunction, bool updateItemWhenReady)
        {
            SPList list = GetList(web);
            SPListItem item = list.Items.Add();

            addListItemFunction.Invoke(item);

            if (updateItemWhenReady)
                SPGENListInstanceStorage.Instance.UpdateListItem(item);

            return item;
        }


        public SPListItem UpdateListItem(SPListItem item, Action<SPListItem> updateListItemFunction, bool updateItemWhenReady)
        {
            return UpdateListItem(item.ParentList.ParentWeb, item.ID, updateListItemFunction, updateItemWhenReady);
        }

        public SPListItem UpdateListItem(SPWeb web, int itemId, Action<SPListItem> updateListItemFunction, bool updateItemWhenReady)
        {
            SPList list = GetList(web);
            SPListItem item = list.GetItemById(itemId);

            updateListItemFunction.Invoke(item);

            if (updateItemWhenReady)
                SPGENListInstanceStorage.Instance.UpdateListItem(item);

            return item;
        }


        /// <summary>
        /// Iterates through all list items in a specific list instance.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="forEachListItemFunction">A lambda expression to use on each list item. Return true to continue and false to end the iteration.</param>
        public void ForEachListItem(SPWeb web, Func<SPListItem, bool> forEachListItemFunction)
        {
            SPList list = GetList(web);

            SPListItemCollection coll = list.Items;
            int c = coll.Count;

            for (int i = 0; i < c; i++)
            {
                var item = coll[i];
                bool bContinue = forEachListItemFunction.Invoke(item);

                if (!bContinue)
                    return;
            }
        }

        /// <summary>
        /// Iterates through all list fields in a specific list instance.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="forEachFieldFunction">A lambda expression to use on each list field. Return true to continue and false to end the iteration.</param>
        public void ForEachField(SPWeb web, Func<SPField, bool> forEachFieldFunction)
        {
            SPList list = GetList(web);

            SPFieldCollection coll = list.Fields;
            int c = coll.Count;

            for (int i = 0; i < c; i++)
            {
                var field = coll[i];
                bool bContinue = forEachFieldFunction.Invoke(field);

                if (!bContinue)
                    return;
            }
        }

        /// <summary>
        /// Iterates through all files in a specific list instance.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="forEachFieldFunction">A lambda expression to use on each file in the list. Return true to continue and false to end the iteration.</param>
        public void ForEachFile(SPWeb web, Func<SPFile, bool> forEachFieldFunction)
        {
            SPList list = GetList(web);

            list.RootFolder.ForEachFolder(true, true, f =>
            {
                SPFileCollection coll = f.Files;
                int c = coll.Count;

                for (int i = 0; i < c; i++)
                {
                    var file = coll[i];
                    bool bContinue = forEachFieldFunction.Invoke(file);

                    if (!bContinue)
                        return false;
                }

                return true;
            });
        }


        /// <summary>
        /// Starts provision of this list instance at the specified web URL (list instance will be created or updated). You should dispose the fields parent web and site object when ready using it to save system resources.
        /// </summary>
        /// <param name="webUrl">The URL to provision the list instance at.</param>
        /// <param name="disposeWhenReady">True to dispose the site and web objects created by this method when the provisioning is finished.</param>
        /// <returns>The provisioned list instance.</returns>
        public SPGENListInstanceUrlInstance Provision(string webUrl, bool disposeWhenReady)
        {
            SPGENListInstanceUrlInstance instance = new SPGENListInstanceUrlInstance();
            SPWeb web = null;

            try
            {
                instance.Site = new SPSite(webUrl);
                web = instance.Site.OpenWeb();

                instance.List = Provision(web);

                if (disposeWhenReady)
                {
                    web.Dispose();
                    instance.Dispose();
                }

                return instance;
            }
            catch
            {
                web.Dispose();
                instance.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Starts provision of this list instance on the specified web (list instance will be created or updated).
        /// </summary>
        /// <param name="web">The parent web object</param>
        /// <returns>The provisioned list instance.</returns>
        public SPList Provision(SPWeb web)
        {
            return ProvisionOnWeb(web);
        }


        /// <summary>
        /// Starts unprovision of this list instance in the specified at the spcified web URL (list instance will be deleted).
        /// </summary>
        /// <param name="webUrl">The URL to unprovision the list instance at.</param>
        public void Unprovision(string webUrl)
        {
            using (var instance = SPGENListInstanceStorage.Instance.CreateUrlInstance(webUrl))
            {
                Unprovision(instance.Web);
            }
        }

        /// <summary>
        /// Starts unprovision of this list instance in the specified on the spcified web (list instance will be deleted).
        /// </summary>
        /// <param name="web">The parent web object.</param>
        public void Unprovision(SPWeb web)
        {
            Unprovision(web.Lists);
        }


        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="web">The parent web object.</param>
        /// <returns>The updated list instance.</returns>
        public SPList RegisterEventReceiver<TEventReceiver>(SPWeb web) where TEventReceiver : SPEventReceiverBase
        {
            return RegisterEventReceiver<TEventReceiver>(web, false);
        }

        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="web">The web instance where get the list instance from.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns></returns>
        public SPList RegisterEventReceiver<TEventReceiver>(SPWeb web, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            SPList list = GetList(web);

            RegisterEventReceiver<TEventReceiver>(list, keepOnlyDeclaredMethods);

            return list;
        }

        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="web">The web instance where get the list instance from.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        /// <returns></returns>
        public SPList RegisterEventReceiver<TEventReceiver>(SPWeb web, int sequenceNumber, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            SPList list = GetList(web);

            RegisterEventReceiver<TEventReceiver>(list, sequenceNumber, keepOnlyDeclaredMethods);

            return list;
        }

        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="list">The list to update.</param>
        public void RegisterEventReceiver<TEventReceiver>(SPList list) where TEventReceiver : SPEventReceiverBase
        {
            RegisterEventReceiver<TEventReceiver>(list, false);
        }

        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="list">The list to update.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        public void RegisterEventReceiver<TEventReceiver>(SPList list, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            RegisterEventReceiversInternal(typeof(TEventReceiver), list, null, keepOnlyDeclaredMethods);
        }

        /// <summary>
        /// Registers event receivers for this list instance in the specified web. This method will reflect the TEventReceiver type and automatically add all the event methods declared as public inside the specified type.
        /// </summary>
        /// <typeparam name="TEventReceiver">The type to reflect.</typeparam>
        /// <param name="list">The list to update.</param>
        /// <param name="sequenceNumber">Event receiver sequence number.</param>
        /// <param name="keepOnlyDeclaredMethods">Keeps only declared ethods if set to true. Removes all undeclared methods found on the content type that belonged to this type.</param>
        public void RegisterEventReceiver<TEventReceiver>(SPList list, int sequenceNumber, bool keepOnlyDeclaredMethods) where TEventReceiver : SPEventReceiverBase
        {
            RegisterEventReceiversInternal(typeof(TEventReceiver), list, sequenceNumber, keepOnlyDeclaredMethods);
        }


        /// <summary>
        /// The localized web relative URL to this list instance.
        /// </summary>
        public string GetLocalizedWebRelativeUrl(SPWeb web)
        {
            string url;
            var properties = GetDefinition();

            if (SPGENResourceHelper.HasResourceSyntax(properties.WebRelURL))
            {
                url = SPGENResourceHelper.GetString(properties[web.UICulture].WebRelURL);
                //If no string was found it will still be a resource string.
                if (SPGENResourceHelper.HasResourceSyntax(url))
                    throw new SPGENGeneralException("Could not find resource string '" + properties.WebRelURL + "'.");
            }
            else
            {
                url = properties.WebRelURL;
            }

            return url;
        }

        /// <summary>
        /// Gets the full URL to this list instance in a web site.
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <returns></returns>
        public string GetFullURL(SPWeb web)
        {
            return web.Url + "/" + GetLocalizedWebRelativeUrl(web);
        }

        #endregion


        #region Private members

        private SPList GetInstanceInternal(SPWeb web, SPGENListInstanceGetMethod method, bool throwExceptionIfNotExists, bool throwUnauthorizedAccessException)
        {
            SPList list = null;
            string title = string.Empty;

            if (method == SPGENListInstanceGetMethod.ByTitle)
            {
                title = this.InstanceDefinition[web.UICulture].Title;
                if (string.IsNullOrEmpty(title))
                    method = SPGENListInstanceGetMethod.ByUrl;
            }

            bool catchAccessDeniedExceptionState = web.Site.CatchAccessDeniedException;

            if (method == SPGENListInstanceGetMethod.ByUrl)
            {
                string url = GetLocalizedWebRelativeUrl(web);

                if (!string.IsNullOrEmpty(url))
                {
                    url = (web.ServerRelativeUrl != "/" ? web.ServerRelativeUrl : "") + "/" + this.InstanceDefinition.WebRelURL;
                    try
                    {
                        if (!throwUnauthorizedAccessException)
                            web.Site.CatchAccessDeniedException = false;

                        list = SPGENListInstanceStorage.Instance.GetListByUrl(web, url);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (throwUnauthorizedAccessException)
                            throw;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        if (throwExceptionIfNotExists)
                            throw;

                    }
                    finally
                    {
                        if (!throwUnauthorizedAccessException)
                            web.Site.CatchAccessDeniedException = catchAccessDeniedExceptionState;
                    }
                    
                }
                else
                {
                    throw new SPGENGeneralException("List title value is empty for list instance element " + this.GetType().FullName + ".");
                }
            }
            else
            {
                if (SPGENResourceHelper.HasResourceSyntax(title))
                    throw new SPGENGeneralException("The list instance of element " + this.GetType().FullName + " has resource syntax for the list title but the specified resource string could not be resolved.");


                try
                {
                    if (!throwUnauthorizedAccessException)
                        web.Site.CatchAccessDeniedException = false;

                    list = SPGENListInstanceStorage.Instance.GetListByTitle(web, title, throwUnauthorizedAccessException);
                }
                catch (UnauthorizedAccessException)
                {
                    if (throwUnauthorizedAccessException)
                        throw;
                }
                finally
                {
                    if (!throwUnauthorizedAccessException)
                        web.Site.CatchAccessDeniedException = catchAccessDeniedExceptionState;
                }

                if (list == null && throwExceptionIfNotExists)
                    throw new SPGENListDoesNotExistException("The list instance of element " + this.GetType().FullName + " does not exist on the web " + web.Url + ".");
            }

            return list;
        }

        private SPGENListInstanceProperties GetNewDefinitionInstance()
        {
            var properties = SPGENElementProperties.CreateInstance<SPGENListInstanceProperties, SPGENListInstanceAttribute>(this.GetType());
            properties.EventReceivers.AddReceiversFromElementAttributes(this.GetType(), C_EVENT_TYPES);

            InitializeDefinition(properties);

            return properties;
        }

        internal override SPList ProvisionOnWeb(SPWeb web)
        {
            SPList list = null;
            bool create = false;

            var listInstanceProperties = GetDefinition();

            if (!OnProvisionStarted(listInstanceProperties, web))
                return null;

            list = TryGetList(web, SPGENListInstanceGetMethod.ByUrl);
            if (list == null)
                create = true;


            Guid listId = Guid.Empty;

            if (create)
            {
                Guid tmpl_feature_id = Guid.Empty;

                if (string.IsNullOrEmpty(listInstanceProperties.TemplateFeatureId))
                {
                    foreach (SPListTemplate template in web.ListTemplates)
                    {
                        if ((int)template.Type == listInstanceProperties.TemplateType)
                        {
                            tmpl_feature_id = template.FeatureId;
                            break;
                        }
                    }
                }
                else
                {
                    tmpl_feature_id = new Guid(listInstanceProperties.TemplateFeatureId);
                }

                if (tmpl_feature_id == Guid.Empty)
                {
                    throw new SPGENGeneralException("No template feature id was specified for list instance '" + this.GetType().FullName + "'.");
                }

                string url;
                if (SPGENResourceHelper.HasResourceSyntax(listInstanceProperties.WebRelURL))
                {
                    url = this.GetLocalizedWebRelativeUrl(web);
                }
                else
                {
                    url = listInstanceProperties.WebRelURL;
                }

                if (listInstanceProperties.TemplateType == (int)SPListTemplateType.ExternalList)
                {
                    listId = web.Lists.Add(listInstanceProperties.Title, listInstanceProperties.Description, url, listInstanceProperties.DataSource);
                }
                else
                {
                    listId = web.Lists.Add(listInstanceProperties.Title, listInstanceProperties.Description, url, tmpl_feature_id.ToString(), listInstanceProperties.TemplateType, null);
                }

                list = web.Lists.GetList(listId, true);
            }


            listInstanceProperties.SetPropertiesOnOMObject(list, true);

            listInstanceProperties.UpdateAllDynamicProperties(list);

            if (this.OnProvisionerAction != null)
            {
                this.OnProvisionerAction.Invoke(
                    new SPGENListProvisioningArguments()
                    {
                        List = list,
                        ContentTypes = listInstanceProperties.ContentTypes,
                        EventReceivers = listInstanceProperties.EventReceivers,
                        Fields = listInstanceProperties.Fields,
                        Views = listInstanceProperties.Views
                    });
            }


            //Add or update fields to list
            SPFieldCollection fieldCollection = list.Fields;

            try
            {
                listInstanceProperties.Fields.Provision(fieldCollection);
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException("Failed provision list fields on list instance '" + _instance.GetType().Name + "'. " + ex.Message, ex);
            }


            //Add or update content types to list
            try
            {
                listInstanceProperties.ContentTypes.Provision(list);
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException("Failed provision list content types on list instance '" + _instance.GetType().Name + "'. " + ex.Message, ex);
            }


            //Add or update views
            SPViewCollection viewCollection = list.Views;
            var fieldsAddedToDefaultView = (from f in listInstanceProperties.Fields where f.AddToDefaultView == true select f.InternalName).ToList<string>();

            try
            {
                listInstanceProperties.Views.Provision(viewCollection, true, fieldsAddedToDefaultView);
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException("Failed provision list views on list instance '" + _instance.GetType().Name + "'. " + ex.Message, ex);
            } 


            //Add or update event receivers
            try
            {
                listInstanceProperties.EventReceivers.Provision(list.EventReceivers);
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException("Failed provision list event receivers on list instance '" + _instance.GetType().Name + "'. " + ex.Message, ex);
            }


            if (!OnProvisionBeforeFinalization(list))
                return list;


            list.Update();


            OnProvisionFinalized(list);

            return list;

        }

        internal override void Unprovision(SPListCollection listCollection)
        {
            SPWeb web = listCollection.Web;

            SPList list = TryGetList(web);
            if (list == null)
                return;

            web.Lists.Delete(list.ID);

            OnUnprovisioned(web);
        }

        private void RegisterEventReceiversInternal(Type receiver, SPList list, int? sequenceNumber, bool keepOnlyDeclaredMethods)
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

            col.Provision(list.EventReceivers);

            SPGENListInstanceStorage.Instance.UpdateList(list);
        }

        protected void EnsureRequiredProperties(SPGENListInstanceProperties properties)
        {
            if (string.IsNullOrEmpty(properties.WebRelURL))
            {
                throw new SPGENGeneralException("The parameter WebRelURL is not specified for list instance element " + this.GetType().FullName);
            }

            if (string.IsNullOrEmpty(properties.Title))
            {
                throw new SPGENGeneralException("The parameter Title is not specified for list instance element " + this.GetType().FullName);
            }

        }

        private static readonly SPEventReceiverType[] C_EVENT_TYPES =
                new SPEventReceiverType[] { 
                        SPEventReceiverType.ContextEvent,
                        SPEventReceiverType.EmailReceived,
                        SPEventReceiverType.FieldAdded,
                        SPEventReceiverType.FieldAdding,
                        SPEventReceiverType.FieldDeleted,
                        SPEventReceiverType.FieldDeleting,
                        SPEventReceiverType.FieldUpdated,
                        SPEventReceiverType.FieldUpdating,
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

    public enum SPGENListInstanceGetMethod
    {
        ByUrl,
        ByTitle
    }
}
