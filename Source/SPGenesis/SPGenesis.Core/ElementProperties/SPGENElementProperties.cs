using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Linq.Expressions;

namespace SPGenesis.Core
{
    public abstract class SPGENElementProperties
    {
        private Dictionary<int, object> _localizedDefinitions = new Dictionary<int, object>();
        private Dictionary<string, object> _updatedProperties = new Dictionary<string, object>();
        private Dictionary<string, object> _allProperties = new Dictionary<string, object>();
        private Dictionary<string, object> _dynamicProperties = new Dictionary<string, object>();

        protected abstract string ElementName { get; }
        protected abstract string ElementIdAttribute { get; }
        internal abstract object ElementIdValue { get; set; }
        internal Type ElementType { get; set; }
        internal Type ElementAttributeType { get; set; }
        internal object SourceAttribute { get; set; }
        internal Guid ParentFeatureId { get; private set; }
        internal bool DisablePropertyTracking { get; set; }
        public bool IsFromSPDefinition { get; internal set; }

        public bool ExcludeProvisioning { get; set; }
        public int ProvisionSequence { get; set; }
        public int UnprovisionSequence { get; set; }

        public SPGENElementProperties()
        {
        }

        internal SPGENElementProperties(XmlNode elementDefinitionXml)
        {
            this.ElementDefinitionXml = elementDefinitionXml;
            this.IsFromSPDefinition = true;

            ExtractPropertiesFromDefinition(elementDefinitionXml);
        }

        public XmlAttributeCollection AllSchemaAttributes
        {
            get 
            {
                if (this.ElementDefinitionXml != null)
                {
                    return this.ElementDefinitionXml.Attributes;
                }
                else
                {
                    throw new SPGENGeneralException("There is no element xml loaded.");
                }
            }
        }

        protected internal IDictionary<string, object> DynamicProperties
        {
            get { return _dynamicProperties; }
        }

        internal IDictionary<string, object> UpdatedProperties { get { return _updatedProperties; } }

        internal bool IsPropertyValueUpdated(string propertyName)
        {
            return _updatedProperties.ContainsKey(propertyName);
        }

        private XmlNode _elementDefinitionXml;
        internal XmlNode ElementDefinitionXml
        {
            get
            {
                if (_elementDefinitionXml != null)
                    return _elementDefinitionXml;

                throw new SPGENGeneralException("No element xml is set for this element properties instance.");
            }
            set
            {
                _elementDefinitionXml = value;
            }
        }

        private void Initialize(CultureInfo cultureInfo)
        {
            this.ParentFeatureId = SPGENCommon.GetAssociatedFeatureIdForElement(this.ElementType, true);

            SetInitValues();

            if (this.ParentFeatureId != Guid.Empty)
            {
                bool success = ReadFromFeatureDefinition(cultureInfo);

                if (!success)
                {
                    throw new SPGENGeneralException("The element " + this.ElementType.FullName + " with ID value " + this.ElementIdValue.ToString() + " was not found in feature " + this.ParentFeatureId.ToString());
                }

            }
            else
            {
                CopyPropertiesFromTypeAttribute(cultureInfo);
            }

            OnAfterInitialization();
        }

        protected object GetDynamicProperty<T>(Expression<Func<T, object>> property)
        {
            var member = SPGENCommon.ResolveMemberFromExpression<Func<T, object>>(property);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported.", "property");

            var pInfo = member as PropertyInfo;

            if (_dynamicProperties.ContainsKey(pInfo.Name))
                return _dynamicProperties[pInfo.Name];

            throw new ArgumentException("The property could not be found.");
        }

        protected void AddDynamicPropertyInternal<T>(Expression<Func<T, object>> property, object value)
        {
            var member = SPGENCommon.ResolveMemberFromExpression<Func<T, object>>(property);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported.", "property");

            var pInfo = member as PropertyInfo;

            if (value == null)
            {
                if (pInfo.PropertyType.IsValueType)
                {
                    throw new ArgumentException("Can not accept null value for the property '" + pInfo.Name + "'.", "Value");
                }
            }
            else
            {
                Type vt = value.GetType();
                if (pInfo.PropertyType != vt && !pInfo.PropertyType.IsInstanceOfType(vt))
                {
                    throw new ArgumentException("The value specified is incompatible with the value type for the property '" + pInfo.Name + "'.", "Value");
                }
            }

            if (_dynamicProperties.ContainsKey(member.Name))
            {
                _dynamicProperties[member.Name] = value;
            }
            else
            {
                _dynamicProperties.Add(member.Name, value);
            }

            //Set updated property value state
            SetPropertyValue(member.Name, value);
        }

        internal void UpdateAllDynamicProperties(object dest)
        {
            foreach (var kvp in this.DynamicProperties)
            {
                try
                {
                    PropertyInfo pInfo = dest.GetType().GetProperty(kvp.Key);

                    if (kvp.Value is string && SPGENResourceHelper.HasResourceSyntax(kvp.Value as string))
                    {
                        pInfo.SetValue(dest, SPGENResourceHelper.GetString(kvp.Value as string), null);
                    }
                    else
                    {
                        pInfo.SetValue(dest, kvp.Value, null);
                    }
                }
                catch (Exception ex)
                {
                    throw new SPGENGeneralException("Property '" + kvp.Key + "' can not be updated on destination object. " + ex.Message);
                }
            }
        }

        public XmlNode CreateXmlDefinition(bool forceBoolUpperCase)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml("<" + this.ElementName + "/>");

            PropertyInfo[] pInfoArray = this.GetType().GetProperties();

            foreach (PropertyInfo pInfo in pInfoArray)
            {
                SPGENPropertyMappingAttribute a = (SPGENPropertyMappingAttribute)Attribute.GetCustomAttribute(pInfo, typeof(SPGENPropertyMappingAttribute));

                if (a == null || a.NoXmlAttribute)
                    continue;

                if (!_updatedProperties.ContainsKey(pInfo.Name))
                    continue;

                string xmlAttributeName = (a.XmlAttributeName == null) ? pInfo.Name : a.XmlAttributeName;

                XmlAttribute attribute = xmldoc.CreateAttribute(xmlAttributeName);
                string value = GetPropertyValue(this, pInfo, a.ToXmlConverter, forceBoolUpperCase);

                if (value == null)
                    continue;

                attribute.Value = value;

                xmldoc.DocumentElement.Attributes.Append(attribute);
            }


            this.OnComposeXmlDefinition(xmldoc.DocumentElement);

            return xmldoc.DocumentElement;
        }

        protected void SetPropertyValue(string PropertyName, object value)
        {
            if (_allProperties.ContainsKey(PropertyName))
            {
                _allProperties[PropertyName] = value;
            }
            else
            {
                _allProperties.Add(PropertyName, value);
            }

            if (!this.DisablePropertyTracking)
            {
                if (_updatedProperties.ContainsKey(PropertyName))
                {
                    _updatedProperties[PropertyName] = value;
                }
                else
                {
                    _updatedProperties.Add(PropertyName, value);
                }
            }
        }

        protected TResult GetPropertyValue<TResult>(string propertyName)
        {
            if (_allProperties.ContainsKey(propertyName))
            {
                return (TResult)_allProperties[propertyName];
            }
            else
            {
                return default(TResult);
            }
        }

        protected TDefinition GetLocalizedInstance<TDefinition, TAttribute>(int lcid)
            where TDefinition : SPGENElementProperties
            where TAttribute : SPGENElementAttributeBase
        {
            if (_localizedDefinitions.ContainsKey(lcid))
                return (TDefinition)_localizedDefinitions[lcid];

            lock (_localizedDefinitions)
            {
                if (_localizedDefinitions.ContainsKey(lcid))
                    return (TDefinition)_localizedDefinitions[lcid];

                object def = SPGENElementProperties.CreateInstance<TDefinition, TAttribute>(this.ElementType, new CultureInfo(lcid));

                _localizedDefinitions.Add(lcid, def);
            }

            return (TDefinition)_localizedDefinitions[lcid];
        }

        internal static TDefinition CreateInstance<TDefinition, TAttribute>(Type elementType)
            where TDefinition : SPGENElementProperties
            where TAttribute : SPGENElementAttributeBase
        {
            return CreateInstance<TDefinition, TAttribute>(elementType, CultureInfo.InvariantCulture);
        }

        internal static TDefinition CreateInstance<TDefinition, TAttribute>(Type elementType, CultureInfo cultureInfo)
            where TDefinition : SPGENElementProperties
            where TAttribute : SPGENElementAttributeBase
        {
            var def = Activator.CreateInstance(typeof(TDefinition)) as SPGENElementProperties;

            def.ElementType = elementType;
            def.ElementAttributeType = typeof(TAttribute);

            def.SourceAttribute = SPGENCommon.GetAttributeFromType<TAttribute>(elementType);

            def.Initialize(cultureInfo);

            return (TDefinition)def;
        }

        protected virtual void SetInitValues()
        {
        }

        private bool ReadFromFeatureDefinition(CultureInfo cultureInfo)
        {

            SPFeatureDefinitionCollection coll = SPFarm.Local.FeatureDefinitions;
            SPFeatureDefinition featureDef = coll[ParentFeatureId];

            if (featureDef == null)
            {
                throw new SPGENGeneralException("Feature " + ParentFeatureId.ToString() + " was not found in this farm.");
            }


            SPElementDefinitionCollection elements = featureDef.GetElementDefinitions(cultureInfo);
            int c = 0;

            foreach (SPElementDefinition element in elements)
            {
                c++;

                if (element.XmlDefinition.LocalName != this.ElementName)
                    continue;


                if (this.ElementIdValue is Guid)
                {
                    if ((Guid)this.ElementIdValue != new Guid(SPGENCommon.GetElementDefinitionAttribute(element.XmlDefinition.Attributes, this.ElementIdAttribute)))
                        continue;
                }
                else if (this.ElementIdValue is SPContentTypeId)
                {
                    if ((SPContentTypeId)this.ElementIdValue != new SPContentTypeId(SPGENCommon.GetElementDefinitionAttribute(element.XmlDefinition.Attributes, this.ElementIdAttribute)))
                        continue;
                }
                else
                {
                    if (this.ElementIdValue.ToString() != SPGENCommon.GetElementDefinitionAttribute(element.XmlDefinition.Attributes, this.ElementIdAttribute))
                        continue;
                }


                this.ElementDefinitionXml = element.XmlDefinition;
                this.IsFromSPDefinition = true;

                //Get localized element definition
                if (cultureInfo != CultureInfo.InvariantCulture)
                {
                    int c2 = 0;
                    SPElementDefinitionCollection newCollection = featureDef.GetElementDefinitions(cultureInfo);
                    foreach (SPElementDefinition newElementDef in newCollection)
                    {
                        c2++;
                        if (c2 == c)
                        {
                            ExtractPropertiesFromDefinition(newElementDef.XmlDefinition);
                            
                            return true;
                        }
                    }
                }
                else
                {
                    ExtractPropertiesFromDefinition(element.XmlDefinition);

                    return true;
                }

                return false;
            }


            //Element definition not found.
            return false;
        }

        internal void ClearUpdatedAttributesStatus()
        {
            _updatedProperties = new Dictionary<string, object>();
        }

        private void ExtractPropertiesFromDefinition(string xmlNode)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlNode);

            ExtractPropertiesFromDefinition(xmldoc.DocumentElement);
        }

        private void ExtractPropertiesFromDefinition(XmlNode xmlNode)
        {
            foreach (PropertyInfo pInfo in this.GetType().GetProperties())
            {
                var a = Attribute.GetCustomAttribute(pInfo, typeof(SPGENPropertyMappingAttribute)) as SPGENPropertyMappingAttribute;

                if (a == null || a.NoXmlAttribute)
                    continue;

                SetPropertyValue(this, pInfo, xmlNode, a);
            }


            ClearUpdatedAttributesStatus();

            this.OnParseXmlDefinition(xmlNode);
        }

        internal void SetPropertiesOnOMObject(object OMObject, bool ignoreResourcePropertyValues)
        {
            PropertyInfo[] pInfoArray = this.GetType().GetProperties();
            Type OMObjectType = OMObject.GetType();

            foreach (var pInfo in pInfoArray)
            {
                var a = Attribute.GetCustomAttribute(pInfo, typeof(SPGENPropertyMappingAttribute)) as SPGENPropertyMappingAttribute;

                if (a == null || a.DisableOMUpdate || !IsPropertyValueUpdated(pInfo.Name))
                    continue;

                string name = (a.OMPropertyName != null) ? a.OMPropertyName : pInfo.Name;

                PropertyInfo omPropertyInfo = OMObjectType.GetProperty(name);
                if (!omPropertyInfo.CanWrite)
                    continue;

                object value = pInfo.GetValue(this, null);

                if (value is string)
                {
                    if (SPGENResourceHelper.HasResourceSyntax(value as string) && ignoreResourcePropertyValues)
                    {
                        continue;
                    }
                }

                if (a.FromAttributeConverter != null)
                {
                    var conv = Activator.CreateInstance(a.FromAttributeConverter) as ISPGENPropertyConverter;

                    value = conv.ConvertFrom(this, value);
                }

                omPropertyInfo.SetValue(OMObject, value, null);
            }
        }

        internal void CopyPropertiesFromTypeAttribute(CultureInfo cultureInfo)
        {
            PropertyInfo[] pInfoArray = this.GetType().GetProperties();
            var srcAttr = (this.SourceAttribute as SPGENElementAttributeBase);

            var elementAttribute = CustomAttributeData.GetCustomAttributes(this.ElementType).FirstOrDefault<CustomAttributeData>(
                    c => c.Constructor.DeclaringType == this.ElementAttributeType
                );

            if (elementAttribute == null)
                return;

            foreach (PropertyInfo pInfo in pInfoArray)
            {
                var a = Attribute.GetCustomAttribute(pInfo, typeof(SPGENPropertyMappingAttribute)) as SPGENPropertyMappingAttribute;

                if (a == null)
                    continue;

                string name = (a.ElementAttributeName != null) ? a.ElementAttributeName : pInfo.Name;

                var parameterValue = elementAttribute.NamedArguments.FirstOrDefault<CustomAttributeNamedArgument>(cana => cana.MemberInfo.Name == name);

                object value = parameterValue.TypedValue.Value;
                if (value != null)
                {
                    if (value is string && cultureInfo != CultureInfo.InvariantCulture)
                    {
                        if (value != null && SPGENResourceHelper.HasResourceSyntax(value as string))
                        {
                            value = SPGENResourceHelper.GetString((value as string), cultureInfo);
                        }
                    }

                    if (a.FromAttributeConverter != null)
                    {
                        var conv = Activator.CreateInstance(a.FromAttributeConverter) as ISPGENPropertyConverter;

                        pInfo.SetValue(this, conv.ConvertFrom(this, value), null);
                    }
                    else
                    {
                        pInfo.SetValue(this, value, null);
                    }
                }
            }

            this.ExcludeProvisioning = srcAttr.ExcludeProvisioning;
            this.ProvisionSequence = srcAttr.ProvisionSequence;
            this.UnprovisionSequence = srcAttr.UnprovisionSequence;
        }

        private string GetPropertyValue(object sourceObject, PropertyInfo propertyInfo, Type converterType, bool forceBoolUpperCase)
        {
            string value;
            if (converterType == null)
            {
                object o = propertyInfo.GetValue(sourceObject, null);
                if (o != null)
                {
                    var pt = propertyInfo.PropertyType;
                    if (pt.Name == "Nullable`1")
                    {
                        pt = pt.GetGenericArguments()[0];
                    }

                    if (pt == typeof(bool))
                    {
                        if (forceBoolUpperCase)
                        {
                            value = o.ToString().ToUpper();
                        }
                        else
                        {
                            value = o.ToString();
                        }
                    }
                    else
                    {
                        value = o.ToString();
                    }
                }
                else
                {
                    value = string.Empty;
                }
            }
            else
            {
                ISPGENPropertyConverter converter = Activator.CreateInstance(converterType) as ISPGENPropertyConverter;

                value = converter.ConvertTo(sourceObject, propertyInfo.GetValue(sourceObject, null)) as string;
            }

            return value;
        }

        private void SetPropertyValue(object targetObject, PropertyInfo propertyInfo, XmlNode xmlNode, SPGENPropertyMappingAttribute attribute)
        {
            string name = (attribute.XmlAttributeName == null) ? propertyInfo.Name : attribute.XmlAttributeName;
            string value = SPGENCommon.GetElementDefinitionAttribute(xmlNode.Attributes, name);

            if (string.IsNullOrEmpty(value))
                return;

            if (attribute.ToXmlConverter == null)
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(targetObject, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(Int32))
                {
                    try
                    {
                        propertyInfo.SetValue(targetObject, int.Parse(value), null);
                    }
                    catch { }
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    try
                    {
                        propertyInfo.SetValue(targetObject, double.Parse(value), null);
                    }
                    catch { }
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    try
                    {
                        propertyInfo.SetValue(targetObject, bool.Parse(value), null);
                    }
                    catch { }
                }
            }
            else
            {
                ISPGENPropertyConverter converter = Activator.CreateInstance(attribute.ToXmlConverter) as ISPGENPropertyConverter;

                propertyInfo.SetValue(targetObject, converter.ConvertFrom(targetObject, value), null);
            }

        }

        protected virtual void OnAfterInitialization()
        {
        }
        protected virtual void OnParseXmlDefinition(XmlNode xmlDefinition)
        {
        }
        protected virtual void OnComposeXmlDefinition(XmlNode xmlDefinition)
        {
        }

        protected string GetElementIDValueFromAttribute<TAttribute>(string parameterName)
        {
            if (!SPGENCommon.AttributeParameterExists(this.ElementType, typeof(TAttribute), parameterName))
            {
                throw new SPGENGeneralException("ID attribute is not set for the element " + this.ElementType.FullName + " is not set.");
            }

            string value = SPGENCommon.GetNamedAttributeParameter(CustomAttributeData.GetCustomAttributes(this.ElementType), typeof(TAttribute), parameterName) as string;

            return value;
        }
    }

}
