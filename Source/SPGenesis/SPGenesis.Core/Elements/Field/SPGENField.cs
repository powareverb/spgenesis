using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;
using System.Collections;

namespace SPGenesis.Core
{
    /// <summary>
    /// Base class for field elements.
    /// </summary>
    /// <typeparam name="TField">The derived type.</typeparam>
    /// <typeparam name="TSPFieldType">The SPField type to be used or returned in methods.</typeparam>
    /// <typeparam name="TListItemValue">The corresponding list item value for this field. For example, SPFieldText should be using string as a list item value.</typeparam>
    public class SPGENField<TField, TSPFieldType, TListItemValue> : SPGENFieldBase
        where TField : SPGENField<TField, TSPFieldType, TListItemValue>, new()
        where TSPFieldType : SPField
    {
        private static TField _instance = SPGENElementManager.GetInstance<TField>();

        /// <summary>
        /// A set of methods available for this field element.
        /// </summary>
        public static TField Instance = _instance;

        /// <summary>
        /// The static definition of this field element.
        /// </summary>
        public static SPGENFieldProperties<TSPFieldType> Definition
        {
            get { return _instance.GetDefinition(); }
        }

        /// <summary>
        /// Returns the ID for this field element.
        /// </summary>
        public static Guid ID
        {
            get { return Definition.ID; }
        }

        /// <summary>
        /// Returns the internal name for this field element.
        /// </summary>
        public static string InternalName
        {
            get { return Definition.InternalName; }
        }


        #region Virtual methods


        /// <summary>
        /// This method is called when the element gets initialized. All changes to the properties for this element must be made here.
        /// </summary>
        /// <param name="properties">The element properties.</param>
        protected virtual void InitializeDefinition(SPGENFieldProperties<TSPFieldType> properties) { }

        /// <summary>
        /// This method is called when provisioning of this field starts (before the field is created or updated). Return true to continue or false to cancel the provisioning.
        /// </summary>
        /// <param name="fieldProperties">The field properties to use when provisioning the field.</param>
        /// <param name="fieldCollection">The parent field collection.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        /// <returns>Return true to continue or false to cancel the provisioning.</returns>
        protected virtual bool OnProvisionStarted(SPGENFieldProperties<TSPFieldType> fieldProperties, SPFieldCollection fieldCollection, bool isParentList) { return true; }

        /// <summary>
        /// This method is called when the schema xml for this field is created. Return true to continue or false to cancel the provisioning.
        /// </summary>
        /// <param name="schemaXml">The schema xml.</param>
        /// <param name="fieldProperties">The field properties to use when provisioning the field.</param>
        /// <param name="fieldCollection">The parent field collection.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        /// <returns>Return true to continue or false to cancel the provisioning.</returns>
        protected virtual bool OnProvisionFieldSchemaXml(XmlElement schemaXml, SPGENFieldProperties<TSPFieldType> fieldProperties, SPFieldCollection fieldCollection, bool isParentList) { return true; }

        /// <summary>
        /// This method is called when provisioning of this field is finalized (the field was created or updated).
        /// </summary>
        /// <param name="field">The provisioned field instance.</param>
        /// <param name="fieldCollection">The parent collection of the provisioned field.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        /// <param name="updatedOnly">Indicates whether the field was created in the parent field collection or updated only.</param>
        protected virtual void OnProvisionFinalized(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList, bool updatedOnly) { }

        /// <summary>
        /// This method is called when unprovisioning of this field starts (before field is deleted from its parent collection). Return true to continue or false to cancel the unprovisioning.
        /// </summary>
        /// <param name="field">The field instance that is going to be unprovisioned.</param>
        /// <param name="fieldCollection">The parent field collection.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        /// <returns>Return true to continue or false to cancel the unprovisioning.</returns>
        protected virtual bool OnUnprovisionStarted(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList) { return true; }

        /// <summary>
        /// This method is called when unprovisioning of this field is finalized (the field is deleted from its parent collection). 
        /// </summary>
        /// <param name="fieldCollection">The parent field collection of the unprovisioned field.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        protected virtual void OnUnprovisionFinalized(SPFieldCollection fieldCollection, bool isParentList) { }

        /// <summary>
        /// This method is called when this field is about to be created or updated. Return true to continue or false to cancel the provisioning.
        /// </summary>
        /// <param name="field">The field instance that is going to be provisioned.</param>
        /// <param name="fieldCollection">The parent field collection.</param>
        /// <param name="isParentList">If the parent of the field collection is a list or a web.</param>
        /// <returns>Return true to continue or false to cancel the provisioning.</returns>
        protected virtual bool OnProvisionBeforeUpdate(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList) { return true; }

        private readonly object _definitionLock = new object();
        private SPGENFieldProperties<TSPFieldType> _definition;
        /// <summary>
        /// This method is called when the definition is fetched. You should not override this method if the default behavior is adequate. Override it only if you want to change the behavior for how elements are initialized with data from SP definitions.
        /// </summary>
        /// <returns>The content type properties for this element.</returns>
        protected virtual SPGENFieldProperties<TSPFieldType> GetDefinition()
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

        public sealed override SPGENFieldProperties InstanceDefinition
        {
            get { return GetDefinition(); }
        }

        internal override SPGENFieldProperties StaticDefinition
        {
            get { return (SPGENFieldProperties)Definition; }
        }

        internal override Action<SPField> OnProvisionerAction { get; set; }

        internal override bool FireOnProvisionStarted(SPGENFieldProperties fieldProperties, SPFieldCollection fieldCollection, bool isParentList)
        {
            return FireOnProvisionStarted((SPGENFieldProperties<TSPFieldType>)fieldProperties, fieldCollection, isParentList);
        }

        internal override void FireOnProvisionFinalized(SPField field, SPFieldCollection fieldCollection, bool isParentList, bool updatedOnly)
        {
            FireOnProvisionFinalized((TSPFieldType)field, fieldCollection, isParentList, updatedOnly);
        }

        

        internal override SPField Provision(SPFieldCollection fieldCollection, bool updateIfExists, bool pushChangesToList)
        {
            bool bUpdatedOnly;
            bool isList = (fieldCollection.List != null);

            var properties = GetDefinition();

            if (properties.ID == Guid.Empty)
                throw new SPGENGeneralException(string.Format(@"Field '{0}' has an invalid ID.", this.GetType().FullName));

            if (properties.Type == SPFieldType.Invalid && string.IsNullOrEmpty(properties.CustomType))
                throw new SPGENGeneralException(string.Format(@"Field '{0}' has an invalid field type.", this.GetType().FullName));

            if (!FireOnProvisionStarted(properties, fieldCollection, isList))
                return null;

            SPField field = SPGENCommon.CreateOrUpdateField(
                fieldCollection,
                properties,
                xml =>
                {
                    return FireOnProvisionFieldSchemaXml(xml, properties, fieldCollection, isList);
                },
                provisionedField =>
                {
                    if (this.OnProvisionerAction != null)
                        this.OnProvisionerAction.Invoke(provisionedField);

                    return FireOnProvisionBeforeUpdate((TSPFieldType)provisionedField, fieldCollection, isList);
                },
                updateIfExists,
                pushChangesToList,
                out bUpdatedOnly);

            FireOnProvisionFinalized(field as TSPFieldType, fieldCollection, isList, bUpdatedOnly);

            return field;
        }

        internal override void Unprovision(SPFieldCollection fieldCollection)
        {
            if (!fieldCollection.Contains(this.InstanceDefinition.ID))
                return;

            if (!FireOnUnprovisionStarted(fieldCollection[this.InstanceDefinition.ID] as TSPFieldType, fieldCollection, (fieldCollection.List != null)))
                return;

            SPGENFieldStorage.Instance.DeleteField(fieldCollection, this.InstanceDefinition.ID);

            FireOnUnprovisionFinalized(fieldCollection, (fieldCollection.List != null));
        }

        #endregion


        #region Field methods members

        /// <summary>
        /// Gets the field from the specified URL. You must dispose the returned object when it is not used anymore to save system resources.
        /// </summary>
        /// <param name="url">The URL to the field instance. This could either be a list or a web URL.</param>
        /// <returns>Returns an URL instance object. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public SPGENFieldUrlInstance GetField(string url)
        {
            SPGENFieldUrlInstance instance = SPGENFieldStorage.Instance.CreateUrlInstance(url);

            try
            {
                if (instance.List != null)
                {
                    instance.Field = GetField(instance.List);
                }
                else
                {
                    instance.Field = GetField(instance.Web);
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
        /// Gets the field from the specified web.
        /// </summary>
        /// <param name="web">The web instance to get the field from.</param>
        /// <returns>Returns an object of a type specified as the generic type parameter (TSPFieldType).</returns>
        public TSPFieldType GetField(SPWeb web)
        {
            SPField field = SPGENFieldStorage.Instance.GetField(web.Fields, this.InstanceDefinition.ID);
            if (field != null)
                return (TSPFieldType)field;

            field = SPGENFieldStorage.Instance.GetField(web.AvailableFields, this.InstanceDefinition.ID);
            if (field != null)
                return (TSPFieldType)field;

            throw new SPGENGeneralException("The field '" + this.GetType().FullName + "' doesn't exist in the field collection on this web.");
        }

        /// <summary>
        /// Gets the field from the specified list instance.
        /// </summary>
        /// <param name="web">The list instance to get the field from.</param>
        /// <returns>Returns an object of a type specified as the generic type parameter (TSPFieldType).</returns>
        public TSPFieldType GetField(SPList list)
        {
            if (Exists(list))
            {
                return (TSPFieldType)SPGENFieldStorage.Instance.GetField(list.Fields, this.InstanceDefinition.ID);
            }
            else
            {
                throw new SPGENGeneralException("The field '" + this.GetType().FullName + "' doesn't exist in the field collection on this list.");
            }
        }

        /// <summary>
        /// Tries to get the field from the specified URL. You must dispose the returned object when it is not used anymore to save system resources.
        /// </summary>
        /// <param name="url">The URL to the field instance. This could either be a list or a web URL.</param>
        /// <returns>Returns an URL instance object or null if it was not found. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public SPGENFieldUrlInstance TryGetField(string url)
        {
            if (!Exists(url))
                return null;

            return GetField(url);
        }

        /// <summary>
        /// Tries to get the field from the specified web instance. You must dispose the returned object when it is not used anymore to save system resources.
        /// </summary>
        /// <param name="web">The web instance to get the field from.</param>
        /// <returns>Returns an object of a type specified as the generic type parameter (TSPFieldType) or null if it was not found. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public TSPFieldType TryGetField(SPWeb web)
        {
            if (!Exists(web))
                return null;

            return GetField(web);
        }

        /// <summary>
        /// Tries to get the field from the specified list instance. You must dispose the returned object when it is not used anymore to save system resources.
        /// </summary>
        /// <param name="web">The list instance to get the field from.</param>
        /// <returns>Returns an object of a type specified as the generic type parameter (TSPFieldType) or null if it was not found. Make sure do dispose the object correctly to avoid leaking SPSite and SPWeb object instances.</returns>
        public TSPFieldType TryGetField(SPList list)
        {
            if (!Exists(list))
                return null;

            return GetField(list);
        }


        /// <summary>
        /// Checks if the field exists at the specified web URL.
        /// </summary>
        /// <param name="url">The URL to the site or list.</param>
        /// <returns></returns>
        public bool Exists(string url)
        {
            using (var instance = SPGENFieldStorage.Instance.CreateUrlInstance(url))
            {
                if (instance.List != null)
                {
                    return Exists(instance.List);
                }
                else
                {
                    return Exists(instance.Web);
                }
            }
        }

        /// <summary>
        /// Checks if the field exists in the specified web.
        /// </summary>
        /// <param name="web">The web object to check on.</param>
        /// <returns></returns>
        public bool Exists(SPWeb web)
        {
            return SPGENFieldStorage.Instance.ContainsField(web.AvailableFields, this.InstanceDefinition.ID);
        }

        /// <summary>
        /// Checks if the field exists in the specified list.
        /// </summary>
        /// <param name="list">The list object to check on.</param>
        /// <returns></returns>
        public bool Exists(SPList list)
        {
            return SPGENFieldStorage.Instance.ContainsField(list.Fields, this.InstanceDefinition.ID);
        }


        /// <summary>
        /// Updates a specific instance of this field on the specified web.
        /// </summary>
        /// <param name="web">The web object to update the field on.</param>
        /// <param name="updateInstanceAction">A lamba expression to use when updating the field.</param>
        public void UpdateField(SPWeb web, Action<TSPFieldType> updateInstanceAction)
        {
            if (!SPGENFieldStorage.Instance.ContainsField(web.Fields, this.InstanceDefinition.ID))
            {
                if (SPGENFieldStorage.Instance.ContainsField(web.AvailableFields, this.InstanceDefinition.ID))
                {
                    throw new SPGENGeneralException("The field '" + this.GetType().FullName + "' doesn't exist in the updateble field collection on this web but is a member of an ancestor web.");
                }
                else
                {
                    throw new SPGENGeneralException("The field '" + this.GetType().FullName + "' doesn't exist in the updateble field collection on this web.");
                }
            }

            var field = (TSPFieldType)SPGENFieldStorage.Instance.GetField(web.Fields, this.InstanceDefinition.ID);

            updateInstanceAction.Invoke(field);

            SPGENFieldStorage.Instance.UpdateField(field, true);
        }

        /// <summary>
        /// Updates a specific instance of this field on the specified list.
        /// </summary>
        /// <param name="list">The list object to update the field on.</param>
        /// <param name="updateInstanceAction">A lamba expression to use when updating the field.</param>
        public void UpdateField(SPList list, Action<TSPFieldType> updateInstanceAction)
        {
            var field = (TSPFieldType)SPGENFieldStorage.Instance.GetField(list.Fields, this.InstanceDefinition.ID);

            updateInstanceAction.Invoke(field);

            SPGENFieldStorage.Instance.UpdateField(field, false);
        }

        /// <summary>
        /// Updates a specific instance of this field at the specified web URL.
        /// </summary>
        /// <param name="url">The URL to the site or list.</param>
        /// <param name="updateInstanceAction">A lamba expression to use when updating the field.</param>
        public void UpdateField(string url, Action<TSPFieldType> updateInstanceAction)
        {
            using (var instance = GetField(url))
            {
                if (instance.List != null)
                {
                    UpdateField(instance.List, updateInstanceAction);
                }
                else
                {
                    UpdateField(instance.Web, updateInstanceAction);
                }
            }
        }


        public SPGENFieldUrlInstance Provision(string url, bool disposeWhenReady)
        {
            return Provision(url, true, true, disposeWhenReady);
        }

        /// <summary>
        /// Starts provision of this field at the specified web URL (field will be created or updated). You should dispose the fields parent web and site object when ready using it to save system resources.
        /// </summary>
        /// <param name="url">The URL to provision the field at.</param>
        /// <param name="updateIfExists">True if you want to update the field and if it already exists at the specified URL.</param>
        /// <param name="pushChangesToList">True to update all lists using this field. Applies only for site columns.</param>
        /// <param name="disposeWhenReady">True to dispose the site and web objects created by this method when the provisioning is finished.</param>
        /// <returns>The provisioned field instance.</returns>
        public SPGENFieldUrlInstance Provision(string url, bool updateIfExists, bool pushChangesToList, bool disposeWhenReady)
        {
            SPGENFieldUrlInstance instance = null;

            try
            {
                instance = new SPGENFieldUrlInstance();
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();

                if (instance.List != null)
                {
                    instance.Field = Provision(instance.List, updateIfExists, pushChangesToList);
                }
                else
                {
                    instance.Field = Provision(instance.Web, updateIfExists, pushChangesToList);
                }

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

        /// <summary>
        /// Starts provision of this field on the specified web (field will be created or updated).
        /// </summary>
        /// <param name="web">The parent web object.</param>
        public TSPFieldType Provision(SPWeb web)
        {
            return Provision(web, true, true);
        }

        /// <summary>
        /// Starts provision of this field on the specified web (field will be created or updated).
        /// </summary>
        /// <param name="web">The parent web object.</param>
        /// <param name="updateIfExists">True if you want to update the field and if it already exists in the specified web.</param>
        /// <param name="pushChangesToList">True to update all lists using this field. Applies only for site columns.</param>
        /// <returns>The provisioned field instance.</returns>
        public TSPFieldType Provision(SPWeb web, bool updateIfExists, bool pushChangesToList)
        {
            return (TSPFieldType)Provision(web.Fields, updateIfExists, pushChangesToList);
        }

        /// <summary>
        /// Starts provision of this field on the specified list (field will be created or updated).
        /// </summary>
        /// <param name="list">The parent list object.</param>
        public TSPFieldType Provision(SPList list)
        {
            return Provision(list, true, true);
        }

        /// <summary>
        /// Starts provision of this field on the specified list (field will be created or updated).
        /// </summary>
        /// <param name="list">The parent list object.</param>
        /// <param name="updateIfExists">True if you want to update the field and if it already exists in the specified list.</param>
        /// <param name="pushChangesToList">This parameter is ignored for list fields.</param>
        /// <returns>The provisioned field instance.</returns>
        public TSPFieldType Provision(SPList list, bool updateIfExists, bool pushChangesToList)
        {
            return (TSPFieldType)Provision(list.Fields, updateIfExists, pushChangesToList);
        }

        /// <summary>
        /// Starts unprovision of this field at the specified web URL (field will be deleted).
        /// </summary>
        /// <param name="url">The URL to unprovision the field at.</param>
        public void Unprovision(string url)
        {
            using (var instance = GetField(url))
            {
                if (instance.List != null)
                {
                    Unprovision(instance.List);
                }
                else
                {
                    Unprovision(instance.Web);
                }
            }
        }

        /// <summary>
        /// Starts unprovision of this field on the specified web (field will be deleted).
        /// </summary>
        /// <param name="web">The parent web object.</param>
        public void Unprovision(SPWeb web)
        {
            this.Unprovision(web.Fields);
        }

        /// <summary>
        /// Starts unprovision of this field on the specified list (field will be deleted).
        /// </summary>
        /// <param name="list">The parent list object.</param>
        public void Unprovision(SPList list)
        {
            this.Unprovision(list.Fields);
        }


        /// <summary>
        /// Gets the name of a secondary field bound to a business data field. Applies only for fields of bussiness data type.
        /// </summary>
        /// <param name="list">Parent list</param>
        /// <param name="entityFieldName">Field name specified in the entity declaration.</param>
        /// <returns>The internal name of the field.</returns>
        public string GetSecondaryBdcFieldName(SPList list, string entityFieldName)
        {
            return GetSecondaryBdcField(list, entityFieldName).InternalName;
        }

        /// <summary>
        /// Gets a secondary field bound to a business data field. Applies only for fields of bussiness data type.
        /// </summary>
        /// <param name="list">Parent list</param>
        /// <param name="entityFieldName">Field name specified in the entity declaration.</param>
        /// <returns>The field as a SPField type.</returns>
        public SPField GetSecondaryBdcField(SPList list, string entityFieldName)
        {
            SPField field = SPGENCommon.GetSecondaryBdcField(list, this.InstanceDefinition.InternalName, entityFieldName);

            return field;
        }


        public string GetChoiceMappingValue(SPList parentList, string choiceString, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(choiceString))
                return choiceString;

            SPField field = SPGENFieldStorage.Instance.GetField(parentList.Fields, this.InstanceDefinition.ID);

            if (!(field is SPFieldChoice))
                throw new SPGENGeneralException("The field must be a singel choice field.");


            string ret = SPGENChoiceMappingsCache.GetMappingValue(field, choiceString, ignoreCase);

            return ret;
        }

        /// <summary>
        /// Gets the mapped value for a choice value in a mapped choice field. Applies only for fields of choice type.
        /// </summary>
        /// <param name="listItem">The list item to fetch this fields value from.</param>
        /// <param name="ignoreCase">Ignores case when matching the mapped value.</param>
        /// <returns></returns>
        public string GetChoiceMappingValue(SPListItem listItem, bool ignoreCase)
        {
            return GetChoiceMappingValue(listItem.ParentList, GetItemValue(listItem) as string, ignoreCase);
        }


        /// <summary>
        /// Gets the mapped value as an int32 for a choice value in a mapped choice field. Applies only for fields of choice type.
        /// </summary>
        /// <param name="parentList">The SPList instance of the field.</param>
        /// <param name="choiceString">The choice string (as seen in the UI).</param>
        /// <param name="ignoreCase">Ignores case when matching the mapped value.</param>
        /// <returns></returns>
        public Int32? GetChoiceMappingValueAsInt32(SPList parentList, string choiceString, bool ignoreCase)
        {
            string val = GetChoiceMappingValue(parentList, choiceString, ignoreCase);

            if (val == null)
                return null;

            return Int32.Parse(val);
        }

        /// <summary>
        /// Gets the mapped value as an int32 for a choice value in a mapped choice field. Applies only for fields of choice type.
        /// </summary>
        /// <param name="listItem">The list item to fetch this fields value from.</param>
        /// <param name="ignoreCase">Ignores case when matching the mapped value.</param>
        /// <returns></returns>
        public Int32? GetChoiceMappingValueAsInt32(SPListItem listItem, bool ignoreCase)
        {
            return GetChoiceMappingValueAsInt32(listItem.ParentList, GetItemValue(listItem) as string, ignoreCase);
        }


        public string GetChoiceMappingTextString(SPList parentList, string choiceMappingValue)
        {
            if (string.IsNullOrEmpty(choiceMappingValue))
                return choiceMappingValue;

            string ret = SPGENChoiceMappingsCache.GetTextValue(parentList.Fields[this.InstanceDefinition.ID], choiceMappingValue);

            return ret;
        }

        public IDictionary<string, string> GetChoiceMappings(SPList parentList)
        {
            var result = SPGENChoiceMappingsCache.GetMappings(parentList.Fields[this.InstanceDefinition.ID]);

            return result;
        }

        /// <summary>
        /// Gets the value for this field from a SPItemEventProperties object in an event receiver.
        /// </summary>
        /// <param name="properties">The SPItemEventProperties object from the event receiver.</param>
        /// <param name="collectionType">Type of event properties (before or after).</param>
        /// <returns>The typed value.</returns>
        public TListItemValue GetValueFromEventProperties(SPItemEventProperties properties, SPGENItemEventPropertiesType collectionType)
        {
            return GetValueFromEventProperties<TListItemValue>(properties, collectionType);
        }

        /// <summary>
        /// Gets the value for this field from a SPItemEventProperties object in an event receiver.
        /// </summary>
        /// <typeparam name="TResult">The result type to cast the returned value to.</typeparam>
        /// <param name="properties">The SPItemEventProperties object from the event receiver.</param>
        /// <param name="collectionType">Type of event properties (before or after).</param>
        /// <returns>The typed valued.</returns>
        public TResult GetValueFromEventProperties<TResult>(SPItemEventProperties properties, SPGENItemEventPropertiesType collectionType)
        {
            return GetValueFromEventProperties<TResult>(properties, collectionType, false);
        }

        /// <summary>
        /// Gets the value for this field from a SPItemEventProperties object in an event receiver.
        /// </summary>
        /// <typeparam name="TResult">The result type to cast the returned value to.</typeparam>
        /// <param name="properties">The SPItemEventProperties object from the event receiver.</param>
        /// <param name="collectionType">Type of event properties (before or after).</param>
        /// <returns>The typed valued.</returns>
        public TResult GetValueFromEventProperties<TResult>(SPItemEventProperties properties, SPGENItemEventPropertiesType collectionType, bool useListItemValueIfNotFound)
        {
            string internalName = SPGENFieldStorage.Instance.GetField(properties.List.Fields, this.InstanceDefinition.ID).InternalName;

            if (useListItemValueIfNotFound)
            {
                var c = (collectionType == SPGENItemEventPropertiesType.AfterProperties) ?
                    properties.AfterProperties.OfType<DictionaryEntry>() :
                    properties.BeforeProperties.OfType<DictionaryEntry>();

                foreach (var de in c)
                {
                    if ((de.Key as string) == internalName)
                    {
                        return SPGENCommon.ConvertListItemValue<TResult>(properties.Web, de.Value, IsCalculatedFieldType());
                    }
                }

                return SPGENCommon.ConvertListItemValue<TResult>(properties.Web, properties.ListItem[internalName], IsCalculatedFieldType());
            }
            else
            {
                object value = (collectionType == SPGENItemEventPropertiesType.BeforeProperties) ? properties.BeforeProperties[internalName] : properties.AfterProperties[internalName];
                
                return SPGENCommon.ConvertListItemValue<TResult>(properties.Web, value, IsCalculatedFieldType());
            }
        }

        /// <summary>
        /// Gets the value for this field from a SPListItem object.
        /// </summary>
        /// <param name="item">The SPListItem object.</param>
        /// <returns>The typed value.</returns>
        public virtual TListItemValue GetItemValue(SPListItem item)
        {
            return GetItemValue<TListItemValue>(item);
        }

        /// <summary>
        /// Gets the value for this field from a SPListItem object.
        /// </summary>
        /// <typeparam name="TResult">The result type to cast the returned value to.</typeparam>
        /// <param name="item">The list item.</param>
        /// <returns>The typed value.</returns>
        public virtual TResult GetItemValue<TResult>(SPListItem item)
        {
            return SPGENCommon.ConvertListItemValue<TResult>(item.ParentList.ParentWeb, item[this.InstanceDefinition.ID], IsCalculatedFieldType());
        }

        public virtual void SetItemValue(SPListItem listItem, object value)
        {
            listItem[this.StaticDefinition.ID] = value;
        }

        public virtual void SetItemValue(SPListItem listItem, TListItemValue value)
        {
            listItem[this.StaticDefinition.ID] = value;
        }

        public void UpdateItemValue(SPListItem item, Func<TListItemValue, TListItemValue> updateFunction, bool doUpdate, bool systemUpdate, bool overwriteVersion)
        {
            var itemValue = GetItemValue<TListItemValue>(item);

            if (!typeof(TListItemValue).IsValueType)
            {
                if ((object)itemValue == null)
                {
                    itemValue = (TListItemValue)SPGENCommon.ConstructListItemValue(item.ParentList.ParentWeb, item.Fields[this.InstanceDefinition.ID]);
                }
            }

            var value = updateFunction.Invoke(itemValue);

            item[this.InstanceDefinition.ID] = value;

            if (doUpdate)
            {
                if (systemUpdate)
                {
                    item.SystemUpdate();
                }
                else if (overwriteVersion)
                {
                    item.UpdateOverwriteVersion();
                }
                else
                {
                    item.Update();
                }
            }
        }

        public void UpdateFieldValueInEventProperties(SPItemEventProperties properties, SPGENItemEventPropertiesType collectionType, Func<TListItemValue, TListItemValue> updateFunction)
        {
            string internalName = SPGENFieldStorage.Instance.GetField(properties.List.Fields, this.InstanceDefinition.ID).InternalName;
            SPItemEventDataCollection collection = (collectionType == SPGENItemEventPropertiesType.BeforeProperties) ? properties.BeforeProperties : properties.AfterProperties;

            var parameterValue = default(TListItemValue);
            TListItemValue propertyValue;
            object value = null;

            if (!typeof(TListItemValue).IsValueType)
            {
                parameterValue = (TListItemValue)SPGENCommon.ConstructListItemValue(properties.Web, properties.List.Fields[this.InstanceDefinition.ID]);
                propertyValue = updateFunction.Invoke(parameterValue);

                if (propertyValue != null)
                {
                    value = propertyValue.ToString();
                }
            }
            else
            {
                propertyValue = updateFunction.Invoke(parameterValue);
            }

            if (collection[internalName] != null)
            {
                collection[internalName] = propertyValue;
            }
            else
            {
                collection.ChangedProperties.Add(internalName, propertyValue);
            }
        }

        #endregion


        #region Private methods

        private static bool IsCalculatedFieldType()
        {
            if (typeof(TSPFieldType) == typeof(SPFieldCalculated))
                return true;

            if (typeof(TSPFieldType).IsSubclassOf(typeof(SPFieldCalculated)))
                return true;

            return false;
        }

        private SPGENFieldProperties<TSPFieldType> GetNewDefinitionInstance()
        {
            var properties = SPGENElementProperties.CreateInstance<SPGENFieldProperties<TSPFieldType>, SPGENFieldAttribute>(this.GetType());
            InitializeDefinition(properties);

            return properties;
        }

        private bool FireOnProvisionStarted(SPGENFieldProperties<TSPFieldType> fieldProperties, SPFieldCollection fieldCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnProvisionStarted(fieldProperties, fieldCollection, isParentList);
            }
            else
            {
                return true;
            }
        }

        private bool FireOnProvisionFieldSchemaXml(XmlElement schemaXml, SPGENFieldProperties<TSPFieldType> fieldProperties, SPFieldCollection fieldCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnProvisionFieldSchemaXml(schemaXml, fieldProperties, fieldCollection, isParentList);
            }
            else
            {
                return true;
            }
        }

        private bool FireOnProvisionBeforeUpdate(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnProvisionBeforeUpdate(field, fieldCollection, isParentList);
            }
            else
            {
                return true;
            }
        }

        private void FireOnProvisionFinalized(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList, bool updatedOnly)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                OnProvisionFinalized(field, fieldCollection, isParentList, updatedOnly);
            }
        }

        private bool FireOnUnprovisionStarted(TSPFieldType field, SPFieldCollection fieldCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                return OnUnprovisionStarted(field, fieldCollection, isParentList);
            }
            else
            {
                return true;
            }
        }

        private void FireOnUnprovisionFinalized(SPFieldCollection fieldCollection, bool isParentList)
        {
            if (ShouldCallProvisionEvents(isParentList))
            {
                OnUnprovisionFinalized(fieldCollection, isParentList);
            }
        }

        private bool ShouldCallProvisionEvents(bool isParentList)
        {
            var behavior = this.InstanceDefinition.ProvisionEventCallBehavior;
            Type instanceType = _instance.GetType();

            if (!this.InstanceDefinition.IsPropertyValueUpdated("ProvisionEventCallBehavior"))
            {
                if (instanceType.IsNested)
                {
                    Type t = instanceType.DeclaringType;
                    if (t.IsSubclassOf(typeof(SPGENListInstanceBase)))
                    {
                        behavior = SPGENProvisionEventCallBehavior.OnList;
                    }
                }
            }

            if (isParentList && behavior != SPGENProvisionEventCallBehavior.OnWeb)
            {
                return true;
            }
            else if (!isParentList && behavior != SPGENProvisionEventCallBehavior.OnList)
            {
                return true;
            }

            return false;
        }

        protected void EnsureRequiredProperties(SPGENFieldProperties properties)
        {
            if (properties.ID == Guid.Empty)
            {
                throw new SPGENGeneralException("The parameter ID is not specified for field element " + this.GetType().FullName);
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class for field elements.
    /// </summary>
    /// <typeparam name="TField">The derived type.</typeparam>
    public class SPGENField<TField> : SPGENField<TField, SPField, object>
       where TField : SPGENField<TField, SPField, object>, new()
    {
    }

    /// <summary>
    /// Base class for field elements.
    /// </summary>
    /// <typeparam name="TField">The derived type.</typeparam>
    /// <typeparam name="TListItemValue">The corresponding list item value for this field. For example, SPFieldText should be using string as a list item value.</typeparam>
    public class SPGENField<TField, TListItemValue> : SPGENField<TField, SPField, TListItemValue>
        where TField : SPGENField<TField, SPField, TListItemValue>, new()
    {
    }

}
