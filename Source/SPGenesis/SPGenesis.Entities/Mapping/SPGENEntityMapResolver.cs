using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SPGenesis.Core;

namespace SPGenesis.Entities
{
    internal static class SPGENEntityMapResolver
    {
        private static readonly object _lock = new object();

        private static HashSet<string> _scannedAssemblies = new HashSet<string>();
        private static Hashtable _entityMappings = new Hashtable();

        [Obsolete("This method is not supported any more.", true)]
        public static object FindMapper<TEntity>()
        {
            throw new NotSupportedException();
        }

        public static Type FindMapper(Type entityType)
        {
            Type ret = GetEntityMap(entityType);
            if (ret != null)
                return ret;

            lock (_lock)
            {
                ret = GetEntityMap(entityType);
                if (ret != null)
                    return ret;

                Assembly entityAssembly = entityType.Assembly;
                AssemblyName entityAssemblyName = entityAssembly.GetName();

                //Scan and load mappers from the assembly containing the requested entity first.
                LoadEntityMappersInAssembly(entityAssembly);
                ret = GetEntityMap(entityType);
                if (ret != null)
                    return ret;

                //If not found, go through all assemblies in the App domain referencing the assembly of the requested entity.
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.GlobalAssemblyCache)
                        continue;

                    string name = assembly.GetName().Name;

                    if (ShouldIgnoreAssemblyName(name))
                        continue;

                    foreach (var a in assembly.GetReferencedAssemblies())
                    {
                        if (ShouldIgnoreAssemblyName(a.Name))
                            continue;

                        if (a.Name == entityAssemblyName.Name)
                        {
                            LoadEntityMappersInAssembly(assembly);

                            ret = GetEntityMap(entityType);
                            if (ret != null)
                                return ret;
                        }
                    }
                }
            }

            return GetEntityMap(entityType);
        }

        private static bool ShouldIgnoreAssemblyName(string name)
        {
            if (name == "mscorlib" ||
                name == "System" ||
                name.StartsWith("Microsoft.") ||
                name.StartsWith("System.") ||
                name == "SPGenesis.Core" ||
                _scannedAssemblies.Contains(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void LoadEntityMappersInAssembly(Assembly assembly)
        {
            if (assembly == null)
                return;

            string name = assembly.GetName().Name;
            if (_scannedAssemblies.Contains(name))
                return;

            Type[] arrayOfTypes = assembly.GetTypes();

            foreach (Type type in arrayOfTypes)
            {
                if (SPGENCommon.HasInterface(type, typeof(ISPGENEntityMapBase)) ||
                    SPGENCommon.HasInterface(type, typeof(ISPGENEntityValueObjectMapBase)))
                {
                    if (type.BaseType == null || !type.BaseType.IsGenericType)
                        continue;

                    Type entityType = type.BaseType.GetGenericArguments()[0];
                    if (_entityMappings.ContainsKey(entityType))
                        throw new SPGENEntityGeneralException("Duplicate entity mappings detected. The entity mapper '" + type.FullName + "' is already mapping the same entity as '" + (_entityMappings[entityType] as Type).FullName + "'.");

                    _entityMappings.Add(entityType, type);
                }
            }

            _scannedAssemblies.Add(name);
        }

        private static Type GetEntityMap(Type entityType)
        {
            if (_entityMappings.ContainsKey(entityType))
                return (Type)_entityMappings[entityType];

            return null;
        }
    }
}
