using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Core
{
    internal interface ISPGENPropertyConverter
    {
        object ConvertFrom(object parent, object value);
        object ConvertTo(object parent, object value);
    }
}
