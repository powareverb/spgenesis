using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;

namespace SPGenesis.Core
{
    internal static class SPGENTypeValidator
    {
        private static Hashtable _validatedAssemblyNames = new Hashtable();
        private static object _lock = new object();

        public static void ValidateTypesInAssembly(Assembly assembly, Type[] typesToValidate)
        {
            if (CheckCachedAssemblyValidation(assembly))
                return;

            lock (_lock)
            {
                if (CheckCachedAssemblyValidation(assembly))
                    return;


                Type[] types = assembly.GetTypes();

                foreach (Type t in types)
                {
                    bool skip = true;

                    foreach(Type ttv in typesToValidate)
                    {
                        if (t.IsSubclassOf(ttv))
                        {
                            skip = false;
                            break;
                        }
                    }


                    if (skip)
                        continue;

                    
                    Type[] arr = t.BaseType.GetGenericArguments();
                    if (arr.Length == 0 || arr[0] != t)
                    {
                        _validatedAssemblyNames.Add(assembly.FullName, new Type[] { t, arr[0] });

                        throw new SPGENAssemblyValidationExcpetion("Assembly validation error. The type '" + t.FullName + "' has invalid generic arguments. The first generic parameter must be the same as the declaring type. Change '" + arr[0].Name + "' to '" + t.Name + "'.");
                    }
                }

                _validatedAssemblyNames.Add(assembly.FullName, null);

            }
        }

        private static bool CheckCachedAssemblyValidation(Assembly assembly)
        {
            if (_validatedAssemblyNames.ContainsKey(assembly.FullName))
            {
                Type[] t = (Type[])_validatedAssemblyNames[assembly.FullName];
                if (t != null)
                {
                    throw new SPGENAssemblyValidationExcpetion("Assembly validation error. The type '" + t[0].FullName + "' has invalid generic arguments. The first generic parameter must be the same as the declaring type. Change '" + t[1].Name + "' to '" + t[0].Name + "'.");
                }

                return true;
            }

            return false;
        }
    }
}
