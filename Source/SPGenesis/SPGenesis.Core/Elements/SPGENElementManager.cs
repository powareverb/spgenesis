using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SPGenesis.Core
{
    internal static class SPGENElementManager
    {
        internal static TElement GetInstance<TElement>()
        {
            return (TElement)GetInstance(typeof(TElement));
        }

        internal static object GetInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
