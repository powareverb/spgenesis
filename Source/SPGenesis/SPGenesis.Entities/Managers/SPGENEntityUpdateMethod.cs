using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Entities
{
    public enum SPGENEntityUpdateMethod
    {
        Normal,
        SkipUpdate,
        SystemUpdate,
        SystemUpdateOverwriteVersion,
        OverwriteVersion,
    }
}
