using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml.Linq;
using System.Collections;

namespace SPGenesis.Core
{

    /// <summary>
    /// This class supports the framework and was not designed to be used outside.
    /// </summary>
    public static class SPGENChoiceMappingsCache
    {
        private static SPGENObjectCache _cache;
        private static int _mappingsCacheLimit = 1000;

        static SPGENChoiceMappingsCache()
        {
            _cache = new SPGENObjectCache(_mappingsCacheLimit);
        }

        private static string GenerateKey(SPField field)
        {
            if (field.ParentList == null)
                throw new SPGENGeneralException("The field must belong to a list when executing this command.");

            return field.ParentList.ID.ToString("N") + "_" + field.Id.ToString("N");
        }

        public static int CachedItemsLimit
        {
            get
            {
                return _mappingsCacheLimit;
            }
            set
            {
                _mappingsCacheLimit = value;

                _cache = new SPGENObjectCache(_mappingsCacheLimit);
            }
        }

        public static string GetMappingValue(SPField field, string textValue, bool ignoreCase)
        {
            var mappings = GetMappings(field);

            foreach (var kvp in mappings)
            {
                if (ignoreCase)
                {
                    if (kvp.Value.Equals(textValue, StringComparison.InvariantCultureIgnoreCase))
                        return kvp.Key;
                }
                else
                {
                    if (kvp.Value == textValue)
                        return kvp.Key;
                }
            }

            return null;
        }

        public static string GetTextValue(SPField field, string mappingValue)
        {
            var mappings = GetMappings(field);

            if (mappings.ContainsKey(mappingValue))
            {
                return mappings[mappingValue];
            }
            else
            {
                return null;
            }
        }

        public static bool HasMappings(SPField field)
        {
            var mappings = GetMappings(field);

            if (mappings.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static IDictionary<string, string> GetMappings(SPField field)
        {
            string key = GenerateKey(field);

            return _cache.GetItem<IDictionary<string, string>>(key, () => GetChoiceMapping(field));
        }

        private static IDictionary<string, string> GetChoiceMapping(SPField field)
        {
            var map = new Dictionary<string, string>();

            XDocument xdoc = XDocument.Parse(field.SchemaXml);
            var nodes = xdoc.Descendants(XName.Get("MAPPING"));

            foreach (var node in nodes)
            {
                string key = node.Attribute("Value").Value;
                if (!map.ContainsKey(key))
                {
                    map.Add(key, node.Value);
                }
            }

            return map;
        }

    }
}
