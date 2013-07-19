using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Core
{
    public enum SPGENProvisioningMode
    {
        AddUpdateRemove,
        AddUpdateRemoveExclusive,
        AddOnly,

        [Obsolete("Use 'AppendUpdateRemove' instead.")]
        AppendOrUpdate,
        [Obsolete("Use 'AddUpdateRemoveExclusive' instead.")]
        ExclusiveAdd
    }
}
