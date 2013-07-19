using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Entities
{
    [Obsolete("Not longer in use.", true)]
    public enum SPGENEntityAttachmentsUpdateMethod
    {
        NoUpdate,
        AppendOrUpdate,
        ExclusiveAdd
    }
}
