using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    internal static class SPGENElementDefinitionCache
    {
        private static object _lock = new object();
        private static SPGENObjectCache _cache = new SPGENObjectCache();
        private static bool _defaultItemsLoaded;

        public struct CachedElementDefinition
        {
            public Guid FeatureId;
            public XmlNode ElementXml;
        }

        public static void ResetCache()
        {
            lock (_lock)
            {
                _cache.Clear(cache =>
                    {
                        _defaultItemsLoaded = false;

                        EnsureDefaultItems();
                    });
            }
        }

        private static void EnsureDefaultItems()
        {
            if (_defaultItemsLoaded)
                return;

            lock (_lock)
            {
                if (_defaultItemsLoaded)
                    return;

                PreloadDefaultItems();
                _defaultItemsLoaded = true;
            }
        }

        public static CachedElementDefinition? GetElementInfo(string elementType, string idAttributeName, object id)
        {
            EnsureDefaultItems();

            string key = CreateKey(elementType, id.ToString());

            var result = _cache.GetItem<CachedElementDefinition?>(key, () =>
            {
                var coll = SPFarm.Local.FeatureDefinitions;
                CachedElementDefinition? ret = null;

                foreach (var definition in coll)
                {
                    try
                    {
                        var elementInfo = GetElementInfoFromElementDefinition(definition, elementType, idAttributeName, id);
                        if (elementInfo.HasValue)
                        {
                            ret = elementInfo;
                        }
                    }
                    catch (Exception ex)
                    {
                        SPGENCommon.WriteToULS("Error when processing element definitions for feature " + definition.Id.ToString() + ".", ex, 0, TraceSeverity.Medium, EventSeverity.Error);
                        System.Diagnostics.Debug.WriteLine("Error when processing element definitions for feature " + definition.Id.ToString() + ". Message: " + ex.Message, typeof(SPGENElementDefinitionCache).Name);
                    }
                }

                if (ret.HasValue)
                    return ret;

                return null;
                //throw new SPGENGeneralException("No element of type " + elementType + " with id '" + id.ToString() + "' found in this farm configuration.");
            });

            return result;
        }

        private static CachedElementDefinition? GetElementInfoFromElementDefinition(SPFeatureDefinition featureDefinition, string elementType, string idAttributeName, object id)
        {
            var elementCollection = featureDefinition.GetElementDefinitions(CultureInfo.InvariantCulture);
            CachedElementDefinition? ret = null;

            foreach (SPElementDefinition e in elementCollection)
            {
                if (e.ElementType == "ContentType" ||
                    e.ElementType == "Field" ||
                    e.ElementType == "ListInstance" ||
                    e.ElementType == "ListTemplate")
                {
                    string elementId;
                    if (e.ElementType == "ContentType" || e.ElementType == "Field")
                    {
                        elementId = SPGENCommon.GetElementDefinitionAttribute(e.XmlDefinition.Attributes, "ID");
                    }
                    else if (e.ElementType == "ListInstance")
                    {
                        elementId = SPGENCommon.GetElementDefinitionAttribute(e.XmlDefinition.Attributes, "Url");
                    }
                    else
                    {
                        elementId = SPGENCommon.GetElementDefinitionAttribute(e.XmlDefinition.Attributes, "Type");
                    }
                    
                    string key = CreateKey(e.ElementType, elementId);

                    if (_cache.Exists(key))
                        continue;

                    var elementInfo = new CachedElementDefinition();
                    elementInfo.ElementXml = e.XmlDefinition.Clone();
                    elementInfo.FeatureId = featureDefinition.Id;

                    bool equals = false;

                    if (e.ElementType == elementType)
                    {
                        if (id is Guid)
                        {
                            if ((Guid)id == new Guid(elementId))
                                equals = true;
                        }
                        else if (id is SPContentTypeId)
                        {
                            if ((SPContentTypeId)id == new SPContentTypeId(elementId))
                                equals = true;
                        }
                        else
                        {
                            if ((string)id == elementId)
                                equals = true;
                        }
                    }

                    if (!equals)
                    {
                        if (e.ElementType == "ListTemplate")
                        {
                            key += "_" + e.FeatureDefinition.Id.ToString("N").ToUpper();
                        }

                        _cache.Insert(key, elementInfo, true);
                    }
                    else
                    {
                        ret = elementInfo;
                    }
                }
            }

            return ret;
        }

        private static string CreateKey(string elementType, string id)
        {
            string key = elementType + "_";

            if (elementType == "Field")
            {
                key += new Guid(id).ToString("N").ToUpper();
            }
            else
            {
                key += id.ToUpper();
            }

            return key;
        }


        #region DefaultItems

        private static void PreloadDefaultItems()
        {
            var feature = SPFarm.Local.FeatureDefinitions["fields"];
            PreloadDefinitionIntoCache(feature, "ID");

            feature = SPFarm.Local.FeatureDefinitions["ctypes"];
            PreloadDefinitionIntoCache(feature, "ID");

        }

        private static void PreloadDefinitionIntoCache(SPFeatureDefinition featureDefinition, string idAttributeName)
        {
            var coll = featureDefinition.GetElementDefinitions(CultureInfo.InvariantCulture);

            foreach (SPElementDefinition element in coll)
            {
                string elementId = (element.XmlDefinition as XmlElement).GetAttribute(idAttributeName);
                
                string key = CreateKey(element.ElementType, elementId);
                if (_cache.Exists(key))
                    continue;

                var elementInfo = new CachedElementDefinition();
                elementInfo.ElementXml = element.XmlDefinition.Clone();
                elementInfo.FeatureId = featureDefinition.Id;

                if (element.ElementType == "ListTemplate")
                {
                    key += "_" + element.FeatureDefinition.Id.ToString("N").ToUpper();
                }

                _cache.Insert(key, elementInfo, false);
            }
        }

        #endregion
    }
}
