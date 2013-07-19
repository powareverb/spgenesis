using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SPGenesis.Entities
{
    internal struct SPGENEntityPropertyAccessorArguments
    {
        public object GetConverterInstance;
        public object SetConverterInstance;
        public object AdapterInstance;
        public string FieldName;
    }
}
