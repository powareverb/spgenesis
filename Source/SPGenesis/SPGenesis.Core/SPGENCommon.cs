using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Globalization;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.SharePoint.Utilities;

namespace SPGenesis.Core
{
    public static class SPGENCommon
    {
        internal static void WriteToULS(string message, Exception ex, uint id, TraceSeverity traceSeverity, EventSeverity eventSeverity)
        {
            string Category = Assembly.GetCallingAssembly().GetName().Name;
            SPDiagnosticsService.Local.WriteTrace(id, new SPDiagnosticsCategory(Category, traceSeverity, eventSeverity), traceSeverity, message + ", " + ex.Message, ex.StackTrace);
        }

        public static object ConstructListItemValue(SPWeb web, SPField field)
        {
            return ConstructListItemValue(web, GetFieldValueType(field));
        }

        public static object ConstructListItemValue(SPWeb web, Type fieldValueType)
        {
            if (fieldValueType == typeof(SPFieldUserValue) ||
                fieldValueType.IsSubclassOf(typeof(SPFieldUserValue)))
            {
                return new SPFieldUserValue(web);
            }
            else
            {
                return Activator.CreateInstance(fieldValueType);
            }
        }

        public static TResult ConvertListItemValue<TResult>(SPWeb web, object value)
        {
            return (TResult)ConvertListItemValue(web, value, typeof(TResult), false);
        }

        public static TResult ConvertListItemValue<TResult>(SPWeb web, object value, bool isCalculated)
        {
            return (TResult)ConvertListItemValue(web, value, typeof(TResult), isCalculated);
        }

        public static object ConvertListItemValue(SPWeb web, object value, Type resultValueType)
        {
            return ConvertListItemValue(web, value, resultValueType, false);
        }

        public static object ConvertListItemValue(SPWeb web, object value, Type resultValueType, bool isCalculated)
        {
            bool isNullableType = false;

            if (resultValueType.IsGenericType)
            {
                if (resultValueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    isNullableType = true;
            }

            if (value == null)
            {
                if (resultValueType.IsValueType && !isNullableType)
                {
                    //Set default value for the result type.
                    value = Activator.CreateInstance(resultValueType);
                }

                return value;
            }

            Type actualValueType = value.GetType();

            if (!isCalculated && actualValueType == resultValueType)
                return value;

            if (isNullableType)
                resultValueType = resultValueType.GetGenericArguments()[0];
            
            if (!isCalculated && !resultValueType.IsValueType)
            {
                if (resultValueType == typeof(SPFieldUserValue) && !(value is SPFieldUserValue))
                {
                    return new SPFieldUserValue(web, value.ToString());
                }
                else if (resultValueType == typeof(SPFieldUserValueCollection) && !(value is SPFieldUserValueCollection))
                {
                    return new SPFieldUserValueCollection(web, value.ToString());
                }
                else if (resultValueType == typeof(SPFieldLookupValue) && !(value is SPFieldLookupValue))
                {
                    return new SPFieldLookupValue(value.ToString());
                }
                else if (resultValueType == typeof(SPFieldLookupValueCollection) && !(value is SPFieldLookupValueCollection))
                {
                    return new SPFieldLookupValueCollection(value.ToString());
                }
                else if (resultValueType == typeof(SPFieldMultiChoiceValue) && !(value is SPFieldMultiChoiceValue))
                {
                    return new SPFieldMultiChoiceValue(value.ToString());
                }
                else if (resultValueType == typeof(SPFieldRatingScaleValue) && !(value is SPFieldRatingScaleValue))
                {
                    return new SPFieldRatingScaleValue(value.ToString());
                }
                else if (resultValueType == typeof(SPFieldUrlValue) && !(value is SPFieldUrlValue))
                {
                    return new SPFieldUrlValue(value.ToString());
                }

                return value;
            }
            else
            {
                if (isCalculated)
                {
                    SPFieldMultiColumnValue mcv;
                    if (value is SPFieldMultiColumnValue)
                        mcv = value as SPFieldMultiColumnValue;
                    else
                        mcv = new SPFieldMultiColumnValue(value.ToString());

                    if (mcv.Count < 2)
                        return (resultValueType.IsValueType && !isNullableType) ? Activator.CreateInstance(resultValueType) : null;

                    string t = mcv[0];                    
                    string valueAsString = mcv[1];

                    if (t == "float")
                    {
                        actualValueType = typeof(float);
                        value = float.Parse(valueAsString, System.Globalization.CultureInfo.CurrentUICulture);
                    }
                    else if (t == "datetime")
                    {
                        actualValueType = typeof(DateTime);
                        value = SPUtility.CreateDateTimeFromISO8601DateTimeString(valueAsString);
                    }
                    else if (t == "boolean")
                    {
                        actualValueType = typeof(bool);
                        value = (valueAsString == "1") ? true : false;
                    }
                    else
                    {
                        actualValueType = typeof(string);
                        value = valueAsString;
                    }
                }
                else if (actualValueType == typeof(string))
                {
                    string valueAsString = value.ToString();
                    if (valueAsString == string.Empty)
                        return (resultValueType.IsValueType && !isNullableType) ? Activator.CreateInstance(resultValueType) : null;

                    if (resultValueType == typeof(DateTime))
                    {
                        return (isNullableType) ? new Nullable<DateTime>(SPUtility.CreateDateTimeFromISO8601DateTimeString(valueAsString)) : SPUtility.CreateDateTimeFromISO8601DateTimeString(valueAsString);
                    }
                    else if (resultValueType == typeof(Guid))
                    {
                        return (isNullableType) ? new Nullable<Guid>(new Guid(valueAsString)) : new Guid(valueAsString);
                    }
                    else if (resultValueType == typeof(Boolean))
                    {
                        return (isNullableType) ? new Nullable<bool>(valueAsString == "1") : (valueAsString == "1");
                    }
                }


                if (actualValueType != resultValueType)
                {
                    return Convert.ChangeType(value, resultValueType, System.Globalization.CultureInfo.CurrentUICulture);
                }

                return value;
            }
        }

        public static Type GetFieldValueType(SPField field)
        {
            if (field.FieldValueType == null)
                return typeof(object);

            return field.FieldValueType;
        }

        public static MemberInfo ResolveMemberFromExpression<TDelegate>(Expression<TDelegate> property)
        {
            LambdaExpression lambda = property as LambdaExpression;

            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is invalid.");
            }

            MemberInfo member = memberExpression.Member;

            return member;
        }

        internal static List<SPGENEventReceiverProperties> GetEventReceiversFromType(string assemblyFullName, string className, int? sequenceNumber, SPEventReceiverType[] include)
        {
            Assembly asm;
            try
            {
                asm = Assembly.ReflectionOnlyLoad(assemblyFullName);
            }
            catch (Exception ex)
            {
                throw new SPGENGeneralException(string.Format("Could not locate assembly '{0}' in the GAC.", assemblyFullName), ex);
            }

            Type eventHandlerType = asm.GetType(className);

            if (eventHandlerType == null)
                throw new NullReferenceException("The event handler type " + className + " could not be found in assembly " + assemblyFullName + ".");

            return GetEventReceiversFromType(eventHandlerType, sequenceNumber, include);
        }

        internal static List<SPGENEventReceiverProperties> GetEventReceiversFromType(Type eventHandlerType, int? sequenceNumber, SPEventReceiverType[] include)
        {
            var listOfEventReceivers = new List<SPGENEventReceiverProperties>();
            MethodInfo[] methods = eventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                if (!method.IsVirtual || method.IsPrivate)
                    continue;

                SPEventReceiverType event_rec_type = SPEventReceiverType.InvalidReceiver;

                try
                {
                    event_rec_type = (SPEventReceiverType)Enum.Parse(typeof(SPEventReceiverType), method.Name);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (include != null && Array.IndexOf(include, event_rec_type) == -1)
                    continue;

                var attributeData = CustomAttributeData.GetCustomAttributes(method);
                SPEventReceiverSynchronization sync = SPEventReceiverSynchronization.Default;

                if (attributeData != null && attributeData.Count > 0)
                {
                    var sync_param_value = GetNamedAttributeParameter(attributeData, typeof(SPGENEventHandlerSynchronizationAttribute), "Synchronization");
                    if (sync_param_value != null)
                        sync = (SPEventReceiverSynchronization)sync_param_value;
                }

                var properties = new SPGENEventReceiverProperties()
                {
                    Assembly = eventHandlerType.Assembly.FullName,
                    Class = eventHandlerType.FullName,
                    Type = event_rec_type,
                    Synchronization = sync,
                    SequenceNumber = sequenceNumber
                };

                listOfEventReceivers.Add(properties);
            }

            return listOfEventReceivers;
        }

        private static void AddEventReceiversInternal(Type eventHandlerType, SPEventReceiverDefinitionCollection definitionCollection, SPEventReceiverType[] include, bool keepOnlyDeclared)
        {
            MethodInfo[] methods;
            List<string> listOfEventTypes = new List<string>();
            List<Guid> lisfOfRegistrationsToRemove = new List<Guid>();

            listOfEventTypes = new List<string>();
            methods = eventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                if (!method.IsVirtual || method.IsPrivate)
                    continue;

                SPEventReceiverType event_rec_type = SPEventReceiverType.InvalidReceiver;

                try
                {
                    event_rec_type = (SPEventReceiverType)Enum.Parse(typeof(SPEventReceiverType), method.Name);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (include != null && Array.IndexOf(include, event_rec_type) == -1)
                    continue;

                var attributeData = CustomAttributeData.GetCustomAttributes(method);
                SPEventReceiverSynchronization sync = SPEventReceiverSynchronization.Default;
                if (attributeData != null && attributeData.Count > 0)
                {
                    var sync_param_value = GetNamedAttributeParameter(attributeData, typeof(SPGENEventHandlerSynchronizationAttribute), "Synchronization");
                    if (sync_param_value != null)
                    {
                        sync = (SPEventReceiverSynchronization)sync_param_value;
                    }
                }

                

                SPEventReceiverDefinition existentDef = EventReceiverExists(definitionCollection, eventHandlerType.Assembly.FullName, eventHandlerType.FullName, event_rec_type, sync);
                if (existentDef == null)
                {
                    SPEventReceiverDefinition d = definitionCollection.Add();
                    d.Name = eventHandlerType.FullName + "_" + event_rec_type.ToString() + "_" + sync.ToString();
                    d.Assembly = eventHandlerType.Assembly.FullName;
                    d.Class = eventHandlerType.FullName;
                    d.Type = event_rec_type;
                    d.Synchronization = sync;

                    listOfEventTypes.Add(d.Type.ToString() + "_" + d.Synchronization.ToString());
                    
                    d.Update();
                }
                else
                {
                    listOfEventTypes.Add(existentDef.Type.ToString() + "_" + existentDef.Synchronization.ToString());
                }
            }

            if (!keepOnlyDeclared)
                return;

            //Remove registrations not declared for this class.
            lisfOfRegistrationsToRemove = new List<Guid>();
            foreach (SPEventReceiverDefinition def in definitionCollection)
            {
                bool keep = false;

                if (CompareAssemblyNames(def.Assembly, eventHandlerType.Assembly.FullName) && def.Class == eventHandlerType.FullName)
                {
                    if (listOfEventTypes.Exists(evt =>
                        {
                            string[] arr = evt.Split('_');
                            string evtType = arr[0];
                            string syncType = arr[1];
                            if (syncType == "Default")
                            {
                                if (evtType.EndsWith("ing"))
                                {
                                    syncType = "Synchronous";
                                }
                                else
                                {
                                    syncType = "Asynchronous";
                                }
                            }

                            if (evtType == def.Type.ToString() && syncType == def.Synchronization.ToString())
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }))
                    {
                        keep = true;
                    }
                }

                if (!keep)
                {
                    lisfOfRegistrationsToRemove.Add(def.Id);
                }
            }

            foreach (Guid id in lisfOfRegistrationsToRemove)
            {
                definitionCollection[id].Delete();
            }
        }

        internal static bool CompareAssemblyNames(string name1, string name2)
        {
            if (name1.Replace(" ", "") == name2.Replace(" ", ""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static SPEventReceiverDefinition EventReceiverExists(SPEventReceiverDefinitionCollection coll, string assemblyFullName, string className, SPEventReceiverType Type, SPEventReceiverSynchronization Sync)
        {
            foreach (SPEventReceiverDefinition def in coll)
            {
                var sourceEvtProp = new SPGENEventReceiverProperties()
                {
                    Assembly = def.Assembly,
                    Class = def.Class,
                    Type = def.Type,
                    Synchronization = def.Synchronization
                };

                var compareWithEvtProp = new SPGENEventReceiverProperties()
                {
                    Assembly = assemblyFullName,
                    Class = className,
                    Type = Type,
                    Synchronization = Sync
                };

                if (sourceEvtProp.Equals(compareWithEvtProp))
                    return def;
            }

            return null;
        }

        internal static SPField GetSecondaryBdcField(SPList list, string bdcFieldName, string entityFieldName)
        {
            SPBusinessDataField sourceField = list.Fields.GetFieldByInternalName(bdcFieldName) as SPBusinessDataField;

            if (sourceField == null)
                throw new SPGENGeneralException("The field is not a business data field.");

            var arr = sourceField.GetSecondaryFieldsNames();

            var idx = Array.IndexOf<string>(arr, entityFieldName);
            if (idx == -1)
                throw new SPGENGeneralException("Could not find field '" + entityFieldName + "' in the secondary field names collection.");

            var lisfOfFieldNames = GetSecondaryWssFieldNames(sourceField);

            var fieldToReturn = list.Fields.GetFieldByInternalName(lisfOfFieldNames[idx]);

            return fieldToReturn;
        }
        internal static List<string> GetSecondaryWssFieldNames(SPField bdcField)
        {
            XmlDocument fieldXml = new XmlDocument();
            fieldXml.LoadXml(bdcField.SchemaXmlWithResourceTokens);

            string wssNames = fieldXml.DocumentElement.GetAttribute("SecondaryFieldWssNames");
            if (wssNames == "0")
                return null;

            string wssNamesUnescaped = System.Web.HttpUtility.UrlDecode(wssNames);

            int posOfFirstFieldName = int.Parse(wssNames.Substring(wssNames.LastIndexOf("%20") + 3));

            var listOfLengths = new List<int>();
            var listfOfFieldNames = new List<string>();

            int pos = 0;

            foreach (string s in wssNamesUnescaped.Split(' '))
            {
                if (pos < posOfFirstFieldName)
                {
                    listOfLengths.Add(int.Parse(s));
                    pos += s.Length + 1;
                }
                else
                {
                    listfOfFieldNames.Add(s);

                    if (listfOfFieldNames.Count == listOfLengths.Count)
                        break;
                }
            }

            return listfOfFieldNames;
        }

        public static bool HasInterface(object objectInstance, Type interfaceType)
        {
            return HasInterface(objectInstance.GetType(), interfaceType);
        }

        public static bool HasInterface(Type type, Type interfaceType)
        {
            if (interfaceType == type)
                return true;

            if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition)
            {
                return type.GetInterface(interfaceType.GetGenericTypeDefinition().Name) != null;
            }
            else
            {
                return type.GetInterface(interfaceType.Name) != null;
            }
        }

        internal static object GetNamedAttributeParameter(IList<CustomAttributeData> attributes, Type attributeType, string parameterName)
        {
            object ret = null;
            foreach (CustomAttributeData cad in attributes)
            {
                if (cad.Constructor.DeclaringType == attributeType)
                {
                    foreach (CustomAttributeNamedArgument cana in cad.NamedArguments)
                    {
                        if (parameterName == cana.MemberInfo.Name)
                        {
                            ret = cana.TypedValue.Value;
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        internal static bool AttributeParameterExists<TElement, TAttribute>(string parameterName)
        {
            object o = GetNamedAttributeParameter(CustomAttributeData.GetCustomAttributes(typeof(TElement)), typeof(TAttribute), parameterName);

            return o != null;
        }
        internal static bool AttributeParameterExists(Type elementType, Type attribute, string parameterName)
        {
            object o = GetNamedAttributeParameter(CustomAttributeData.GetCustomAttributes(elementType), attribute, parameterName);

            return o != null;
        }

        internal static TAttribute GetAttributeFromType<TType, TAttribute>() where TAttribute : class
        {
            TAttribute result = Attribute.GetCustomAttribute(typeof(TType), typeof(TAttribute)) as TAttribute;

            return result;
        }
        internal static TAttribute GetAttributeFromType<TAttribute>(Type element) where TAttribute : class
        {
            TAttribute result = Attribute.GetCustomAttribute(element, typeof(TAttribute)) as TAttribute;

            return result;
        }

        internal static object GetAttributeFromType(Type element, Type attributeType)
        {
            object result = Attribute.GetCustomAttribute(element, attributeType);

            return result;
        }

        internal static string GetElementDefinitionAttribute(XmlAttributeCollection attributes, string attributeName)
        {
            if (attributes.GetNamedItem(attributeName) != null)
                return attributes[attributeName].Value;

            foreach (XmlAttribute a in attributes)
            {
                if (a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return a.Value;
                }
            }

            return string.Empty;
        }

        internal static Guid GetAssociatedFeatureIdForElement(Type element, bool ignoreIfNotExists)
        {
            Guid id = Guid.Empty;
            var assoc = GetAttributeFromType<SPGENFeatureAssociationAttribute>(element);

            if (assoc == null)
            {
                if (element.IsNested)
                {
                    var feature = GetAttributeFromType<SPGENFeatureAttribute>(element.DeclaringType);

                    if (feature != null)
                    {
                        id = new Guid(feature.ID);
                    }
                }
            }
            else
            {
                id = new Guid(assoc.ID);
            }

            if (id == Guid.Empty && !ignoreIfNotExists)
            {
                throw new SPGENGeneralException("The element '" + element.FullName + "' is not associated with a feature.");
            }

            return id;
        }

        internal static int GetProvisionSequenceNumber(SPGENElementProperties elementAttribute)
        {
            int ret = 0;

            if (elementAttribute != null)
            {
                ret = elementAttribute.ProvisionSequence;
            }

            return ret;
        }

        internal static int GetUnprovisionSequenceNumber(SPGENElementProperties elementAttribute)
        {
            int ret = 0;

            if (elementAttribute != null)
            {
                ret = elementAttribute.UnprovisionSequence;

                if (ret == 0)
                {
                    ret -= elementAttribute.ProvisionSequence;
                }
            }

            return ret;
        }

        internal static Guid GetFeatureIdForBuiltInListType(int listType)
        {
            Guid ret;
            switch (listType)
            {
                case 100:
                    ret = new Guid("00BFEA71-DE22-43B2-A848-C05709900100");
                    break;
                case 101:
                    ret = new Guid("00BFEA71-E717-4E80-AA17-D0C71B360101");
                    break;
                case 102:
                    ret = new Guid("00BFEA71-EB8A-40B1-80C7-506BE7590102");
                    break;
                case 103:
                    ret = new Guid("00BFEA71-2062-426C-90BF-714C59600103");
                    break;
                case 104:
                    ret = new Guid("00BFEA71-D1CE-42de-9C63-A44004CE0104");
                    break;
                case 105:
                    ret = new Guid("00BFEA71-7E6D-4186-9BA8-C047AC750105");
                    break;
                case 106:
                    ret = new Guid("00BFEA71-EC85-4903-972D-EBE475780106");
                    break;
                case 107:
                    ret = new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107");
                    break;
                case 108:
                    ret = new Guid("00BFEA71-6A49-43FA-B535-D15C05500108");
                    break;
                case 109:
                    ret = new Guid("00BFEA71-52D4-45B3-B544-B1C71B620109");
                    break;
                case 110:
                    ret = new Guid("00BFEA71-F381-423D-B9D1-DA7A54C50110");
                    break;
                case 115:
                    ret = new Guid("00BFEA71-1E1D-4562-B56A-F05371BB0115");
                    break;
                case 118:
                    ret = new Guid("00BFEA71-2D77-4A75-9FCA-76516689E21A");
                    break;
                case 119:
                    ret = new Guid("00BFEA71-C796-4402-9F2F-0EB9A6E71B18");
                    break;
                case 120:
                    ret = new Guid("00BFEA71-3A1D-41D3-A0EE-651D11570120");
                    break;
                default:
                    
                    var element = SPGENElementDefinitionCache.GetElementInfo("ListTemplate", "Type", listType.ToString());
                    if (!element.HasValue)
                    {
                        ret = Guid.Empty;
                    }
                    else
                    {
                        ret = element.Value.FeatureId;
                    }

                    break;
            }

            return ret;
        }

        internal static IDictionary<Guid, XmlElement> GetAllOwnAndInheritedFieldLinks(string contentTypeId, bool inherits)
        {
            Dictionary<Guid, string> fieldToContentTypeIdMap = new Dictionary<Guid, string>();
            Dictionary<Guid, XmlElement> fields = new Dictionary<Guid, XmlElement>();

            SPContentTypeId ctid = new SPContentTypeId(contentTypeId);
            bool hasParents = true;

            while (hasParents)
            {
                string ctid_string = ctid.ToString();
                var element = SPGENElementDefinitionCache.GetElementInfo("ContentType", "ID", ctid);

                if (!element.HasValue)
                {
                    if (ctid == SPBuiltInContentTypeId.System)
                        break;

                    ctid = ctid.Parent;
                    continue;
                }

                XmlNamespaceManager manager = new XmlNamespaceManager(element.Value.ElementXml.OwnerDocument.NameTable);
                manager.AddNamespace("mssp", "http://schemas.microsoft.com/sharepoint/");

                XmlNodeList nl = element.Value.ElementXml.SelectNodes("mssp:FieldRefs/mssp:FieldRef", manager);
                foreach (XmlNode nd in nl)
                {
                    XmlElement xe = nd as XmlElement;
                    if (xe == null)
                        continue;

                    Guid fieldId = new Guid(GetElementDefinitionAttribute(xe.Attributes, "ID"));
                    if (fieldToContentTypeIdMap.ContainsKey(fieldId))
                    {
                        if (ctid_string.Length > fieldToContentTypeIdMap[fieldId].Length)
                        {
                            fieldToContentTypeIdMap[fieldId] = ctid_string;
                            fields[fieldId] = xe;
                        }
                    }
                    else
                    {
                        fieldToContentTypeIdMap.Add(fieldId, ctid_string);
                        fields.Add(fieldId, xe);
                    }
                }


                nl = element.Value.ElementXml.SelectNodes("mssp:FieldRefs/mssp:RemoveFieldRef", manager);
                foreach (XmlNode nd in nl)
                {
                    XmlElement xe = nd as XmlElement;
                    if (xe == null)
                        continue;

                    Guid fieldId = new Guid(GetElementDefinitionAttribute(xe.Attributes, "ID"));
                    if (fieldToContentTypeIdMap.ContainsKey(fieldId))
                    {
                        if (ctid_string.Length > fieldToContentTypeIdMap[fieldId].Length)
                        {
                            fieldToContentTypeIdMap[fieldId] = ctid_string;
                            if (fields.ContainsKey(fieldId))
                                fields.Remove(fieldId);
                        }
                    }
                    else
                    {
                        fieldToContentTypeIdMap.Add(fieldId, ctid_string);
                        if (fields.ContainsKey(fieldId))
                            fields.Remove(fieldId);
                    }
                }

                if (ctid == SPBuiltInContentTypeId.System)
                    break;

                ctid = ctid.Parent;
            }

            return fields;
        }

        internal static void PopulateFieldCollection(SPGENListFieldCollection fieldCollection, XmlDocument schemaXml)
        {
            XmlNodeList nl = schemaXml.SelectNodes("/List/MetaData/Fields/*");

            foreach (XmlNode nd in nl)
            {
                XmlElement xe = nd as XmlElement;
                if (xe == null)
                    continue;

                if (xe.LocalName == "Field")
                {

                    var field = new SPGENFieldProperties(xe);

                    if (fieldCollection.Contains(field.ID))
                        continue;

                    fieldCollection.Add(field, false, false, true);
                }
            }

            var ctColl = new SPGENListContentTypeCollection();
            
            PopulateContentTypeCollection(ctColl, schemaXml);

            foreach (var ct in ctColl)
            {
                foreach (var f in ct.FieldLinks)
                {
                    if (ct.FieldLinksToRemove.FirstOrDefault(rfid => rfid == f.ID) != Guid.Empty)
                        continue;

                    if (fieldCollection.FirstOrDefault(ff => ff.ID == f.ID) != null)
                        continue;

                    var element = SPGENElementDefinitionCache.GetElementInfo("Field", "ID", f.ID);

                    if (!element.HasValue)
                        continue;

                    var newFieldProp = new SPGENFieldProperties(element.Value.ElementXml);

                    fieldCollection.Add(newFieldProp, false, false, true);
                }
            }
        }

        internal static void PopulateContentTypeCollection(SPGENListContentTypeCollection contentTypeCollection, XmlDocument schemaXml)
        {
            XmlNodeList nl = schemaXml.SelectNodes("/List/MetaData/ContentTypes/*");
            
            foreach (XmlNode nd in nl)
            {
                XmlElement xe = nd as XmlElement;
                if (xe == null)
                    continue;

                if (xe.LocalName == "ContentType")
                {
                    var ct = new SPGENContentTypeProperties(xe);

                    if (contentTypeCollection.Contains(ct.ID))
                        continue;

                    contentTypeCollection.Add(ct, false, false, true);
                }
                else if (xe.LocalName == "ContentTypeRef")
                {
                    SPContentTypeId ctid = new SPContentTypeId(GetElementDefinitionAttribute(xe.Attributes, "ID"));
                    var element = SPGENElementDefinitionCache.GetElementInfo("ContentType", "ID", ctid);
                    if (!element.HasValue)
                        continue;

                    var ct = new SPGENContentTypeProperties(element.Value.ElementXml);

                    if (contentTypeCollection.Contains(ct.ID))
                        continue;

                    contentTypeCollection.Add(ct, false, false, true);
                }
            }
        }

        internal static void PopulateViewCollection(SPGENListViewCollection viewCollection, XmlDocument schemaXml)
        {
            XmlNodeList nl = schemaXml.SelectNodes("/List/MetaData/Views/*");

            foreach (XmlNode nd in nl)
            {
                XmlElement xe = nd as XmlElement;
                if (xe == null)
                    continue;

                if (xe.LocalName == "View")
                {

                    var view = new SPGENViewProperties(xe);

                    if (string.IsNullOrEmpty(view.UrlFileName))
                        view.UrlFileName = "BaseViewID_" + xe.GetAttribute("BaseViewID");

                    if (viewCollection.Contains(view.UrlFileName))
                        continue;

                    XmlNode ndQuery = xe.SelectSingleNode("Query");
                    if (ndQuery != null)
                    {
                        view.Query = ndQuery.InnerXml;
                    }

                    XmlNodeList ndViewFields = xe.SelectNodes("ViewFields/FieldRef");
                    if (ndViewFields.Count > 0)
                    {
                        foreach (XmlElement xeField in ndViewFields)
                        {
                            view.ViewFields.Add(xeField.GetAttribute("Name"), false, false, true);
                        }
                    }

                    viewCollection.Add(view, false, false, true);
                }
            }
        }

        internal static void CopyAttributeToContentType(SPGENContentTypeProperties attribute, SPContentType contentType)
        {
            if (attribute.Name != null && attribute.IsPropertyValueUpdated("Name"))
                contentType.Name = attribute.Name;

            if (attribute.Description != null && attribute.IsPropertyValueUpdated("Description"))
                contentType.Description = attribute.Description;

            if (attribute.Group != null && attribute.IsPropertyValueUpdated("Group"))
                contentType.Group = attribute.Group;
            
            if (attribute.IsPropertyValueUpdated("Hidden"))
                contentType.Hidden = attribute.Hidden;

            if (attribute.IsPropertyValueUpdated("Sealed"))
                contentType.Sealed = attribute.Sealed;

            if (attribute.IsPropertyValueUpdated("ReadOnly"))
                contentType.ReadOnly = attribute.ReadOnly;

            attribute.UpdateAllDynamicProperties(contentType);
        }

        internal static SPField CreateOrUpdateField(SPFieldCollection fieldCollection, SPGENFieldProperties fieldProperties, Func<XmlElement, bool> onProvisionFieldSchemaXml, Func<SPField, bool> onProvisionBeforeUpdate, bool updateIfExists, bool updateChildren, out bool updatedOnly)
        {
            SPField field;
            bool isCollectionList = fieldCollection.List != null;
            updatedOnly = false;


            if (!SPGENFieldStorage.Instance.ContainsField(fieldCollection, fieldProperties.ID))
            {
                if (fieldProperties.Type == SPFieldType.Calculated && fieldProperties.OutputType == SPFieldType.Invalid)
                {
                    fieldProperties.OutputType = SPFieldType.Text;
                }
            }

            XmlNode schemaXmlnode;
            if (fieldProperties.Type == SPFieldType.Lookup || fieldProperties.IsLookupField)
            {
                schemaXmlnode = fieldProperties.ComposeXmlDefinitionLookup(true, fieldCollection);
            }
            else
            {
                schemaXmlnode = fieldProperties.CreateXmlDefinition(true);
            }

            //Do not update the RelationshipDeleteBehavior if the field is not provisioned to a list.
            if (schemaXmlnode.Attributes.GetNamedItem("RelationshipDeleteBehavior") != null && !isCollectionList)
                schemaXmlnode.Attributes.Remove(schemaXmlnode.Attributes["RelationshipDeleteBehavior"]);

            if (!SPGENFieldStorage.Instance.ContainsField(fieldCollection, fieldProperties.ID))
            {
                SPAddFieldOptions options = (fieldProperties.AddToAllContentTypes) ? SPAddFieldOptions.AddFieldInternalNameHint | SPAddFieldOptions.AddToAllContentTypes : SPAddFieldOptions.AddFieldInternalNameHint;

                if (onProvisionFieldSchemaXml != null)
                {
                    if (!onProvisionFieldSchemaXml(schemaXmlnode as XmlElement))
                    {
                        return null;
                    }
                }

                bool useWebField = false;
                if (isCollectionList)
                {
                    if (SPGENFieldStorage.Instance.ContainsField(fieldCollection.Web.AvailableFields, fieldProperties.ID))
                    {
                        useWebField = true;
                    }
                }

                string name;
                if (useWebField)
                {
                    SPField webField = SPGENFieldStorage.Instance.GetField(fieldCollection.Web.AvailableFields, fieldProperties.ID);
                    name = SPGENFieldStorage.Instance.AddField(fieldCollection, webField);
                }
                else
                {
                    name = SPGENFieldStorage.Instance.CreateField(
                        fieldCollection,
                        schemaXmlnode,
                        fieldProperties.AddToDefaultView,
                        options);
                }


                if (!useWebField && name != fieldProperties.InternalName)
                    throw new SPGENFieldInternalNameMissmatchException("Field " + fieldProperties.ID.ToString() + " could not be created with the specified internal name. The suggested internal name was " + name + ".");

                field = SPGENFieldStorage.Instance.GetField(fieldCollection, fieldProperties.ID);

                /* workaround for business data field */
                if (field is SPBusinessDataField)
                {
                    var businessField = field as SPBusinessDataField;

                    businessField.SetSecondaryFieldsNames(fieldProperties.SecondaryFieldBdcNames.ToArray<string>());
                }
                /* end workaround */

            }
            else
            {

                field = SPGENFieldStorage.Instance.GetField(fieldCollection, fieldProperties.ID);

                if (!updateIfExists)
                    return field;

                XmlDocument oldSchema = new XmlDocument();
                oldSchema.LoadXml(field.SchemaXmlWithResourceTokens);

                //prohibit update of the internal name
                if (schemaXmlnode.Attributes.GetNamedItem("Name") != null)
                    schemaXmlnode.Attributes.GetNamedItem("Name").Value = field.InternalName;


                XmlNode mergedNode = SPGENCommon.MergeElementSchemaXml(oldSchema.DocumentElement, schemaXmlnode);

                if (!string.IsNullOrEmpty(schemaXmlnode.InnerXml))
                {
                    mergedNode.InnerXml = schemaXmlnode.InnerXml;
                }

                /* workaround for business data field */
                if (field is SPBusinessDataField)
                {
                    var businessField = field as SPBusinessDataField;

                    businessField.SetSecondaryFieldsNames(fieldProperties.SecondaryFieldBdcNames.ToArray<string>());
                }
                /* end workaround */


                if (onProvisionFieldSchemaXml != null)
                {
                    if (!onProvisionFieldSchemaXml(schemaXmlnode as XmlElement))
                    {
                        return null;
                    }
                }


                field.SchemaXml = mergedNode.OuterXml;


                //Set allow multiple values if the field is a lookup (even for user fields because they inherit from lookup).
                if (field is SPFieldLookup && fieldProperties.IsPropertyValueUpdated("AllowMultipleValues"))
                {
                    (field as SPFieldLookup).AllowMultipleValues = fieldProperties.AllowMultipleValues;
                }


                if (field is SPFieldChoice || field is SPFieldMultiChoice)
                {
                    StringCollection coll = (field is SPFieldChoice) ? (field as SPFieldChoice).Choices : (field as SPFieldMultiChoice).Choices;
                    SPWeb web = fieldCollection.Web == null ? fieldCollection.List.ParentWeb : fieldCollection.Web;

                    coll.Clear();

                    if (fieldProperties.ChoiceMappings.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in fieldProperties.ChoiceMappings)
                        {
                            string choice = kvp.Value;
                            if (SPGENResourceHelper.HasResourceSyntax(kvp.Value))
                            {
                                choice = SPGENResourceHelper.GetString(kvp.Value, web.UICulture);
                            }

                            if (field is SPFieldChoice)
                            {
                                (field as SPFieldChoice).Choices.Add(choice);
                            }
                            else
                            {
                                (field as SPFieldMultiChoice).Choices.Add(choice);
                            }
                        }

                    }
                    else
                    {
                        foreach (string c in fieldProperties.Choices)
                        {
                            string choice = c;
                            if (SPGENResourceHelper.HasResourceSyntax(c))
                            {
                                choice = SPGENResourceHelper.GetString(c, web.UICulture);
                            }

                            if (field is SPFieldChoice)
                            {
                                (field as SPFieldChoice).Choices.Add(choice);
                            }
                            else
                            {
                                (field as SPFieldMultiChoice).Choices.Add(choice);
                            }
                        }
                    }
                }


                //Add field to default view
                if (fieldCollection.List != null && fieldProperties.AddToDefaultView)
                {
                    if (fieldCollection.List.DefaultView != null)
                    {
                        SPView defaultView = fieldCollection.List.GetView(fieldCollection.List.DefaultView.ID);
                        if (!defaultView.ViewFields.Exists(field.InternalName))
                        {
                            defaultView.ViewFields.Add(field.InternalName);
                            defaultView.Update();
                        }
                    }
                }

                updatedOnly = true;
            }

            if (fieldProperties.IsPropertyValueUpdated("DefaultValue"))
                field.DefaultValue = fieldProperties.DefaultValue;

            if (field is SPFieldUser && !fieldProperties.IsPropertyValueUpdated("LookupList"))
            {
                if (fieldProperties.LookupList == null)
                    (field as SPFieldUser).LookupList = "UserInfo";
            }

            fieldProperties.UpdateAllDynamicProperties(field);


            if (onProvisionBeforeUpdate != null)
            {
                if (!onProvisionBeforeUpdate(field))
                    return field;
            }

            SPGENFieldStorage.Instance.UpdateField(field, updateChildren);

            return field;
                
        }

        internal static SPView CreateOrUpdateView(SPViewCollection viewCollection, SPGENViewProperties viewProperties, bool preserveViewFieldsCollection, IList<string> fieldsAddedToDefaultView, out bool updatedOnly)
        {
            bool needsUpdate = true;
            var typedViewCollection = viewCollection.OfType<SPView>().ToList<SPView>();


            SPView currentView = typedViewCollection.FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + viewProperties.UrlFileName, StringComparison.InvariantCultureIgnoreCase));

            if (currentView == null)
            {
                string viewName = System.IO.Path.GetFileNameWithoutExtension(viewProperties.UrlFileName);

                if (viewProperties.CloneViewUrlFileName == null)
                {
                    currentView = viewCollection.Add(viewName, new StringCollection(), viewProperties.Query, viewProperties.RowLimit, viewProperties.Paged, viewProperties.DefaultView);

                    needsUpdate = false;
                }
                else
                {
                    SPView viewToClone = typedViewCollection.FirstOrDefault<SPView>(v => v.Url.EndsWith("/" + viewProperties.CloneViewUrlFileName, StringComparison.InvariantCultureIgnoreCase));

                    currentView = viewToClone.Clone(viewName, viewToClone.RowLimit, viewToClone.Paged, viewProperties.DefaultView);
                }


                if (!currentView.Url.EndsWith("/" + viewProperties.UrlFileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SPGENViewUrlMissmatchException("View " + viewProperties.UrlFileName + " could not be created with the specified url. The suggested url was " + currentView.Url + ".");
                }

                if (SPGENResourceHelper.HasResourceSyntax(viewProperties.Title))
                {
                    currentView.Title = SPGENResourceHelper.GetString(viewProperties.Title);
                }
                else
                {
                    currentView.Title = viewProperties.Title;
                }

                updatedOnly = false;
            }
            else
            {
                updatedOnly = true;
            }

            //Provision view fields
            viewProperties.ViewFields.Provision(currentView.ViewFields, fieldsAddedToDefaultView);

            if (needsUpdate)
            {
                if (viewProperties.IsPropertyValueUpdated("Title"))
                {
                    if (SPGENResourceHelper.HasResourceSyntax(viewProperties.Title))
                    {
                        currentView.Title = SPGENResourceHelper.GetString(viewProperties.Title);
                    }
                    else
                    {
                        currentView.Title = viewProperties.Title;
                    }
                }

                if (viewProperties.IsPropertyValueUpdated("DefaultView"))
                    currentView.DefaultView = viewProperties.DefaultView;

                if (viewProperties.IsPropertyValueUpdated("Paged"))
                    currentView.Paged = viewProperties.Paged;

                if (viewProperties.IsPropertyValueUpdated("RowLimit"))
                    currentView.RowLimit = viewProperties.RowLimit;

                if (viewProperties.IsPropertyValueUpdated("Query"))
                    currentView.Query = viewProperties.Query;

                if (viewProperties.IsPropertyValueUpdated("Hidden"))
                    currentView.Hidden = viewProperties.Hidden;

            }

            viewProperties.UpdateAllDynamicProperties(currentView);

            SPGENViewStorage.Instance.UpdateView(currentView);

            return currentView;

        }

        internal static XmlNode MergeElementSchemaXml(XmlNode sourceSchema, XmlNode mergeWithSchema)
        {
            XmlNode ret = sourceSchema.Clone();

            foreach (XmlAttribute attribute in mergeWithSchema.Attributes)
            {
                if (ret.Attributes.GetNamedItem(attribute.Name) == null)
                {
                    if (attribute.Value != string.Empty)
                    {
                        XmlAttribute a = ret.OwnerDocument.CreateAttribute(attribute.Name);
                        a.Value = attribute.Value;

                        ret.Attributes.Append(a);
                    }
                }
                else
                {
                    ret.Attributes[attribute.Name].Value = attribute.Value;
                }
            }

            return ret;
        }


        internal enum ActionEnum
        {
            Activate,
            Deactivate,
            Install,
            Uninstall
        }

        internal static readonly Type[] TypesToValidate = new Type[] { 
            typeof(SPGENFieldBase), 
            typeof(SPGENContentTypeBase), 
            typeof(SPGENListInstanceBase), 
            typeof(SPGENViewBase), 
            typeof(SPGENFeatureBase)};
    }
}
