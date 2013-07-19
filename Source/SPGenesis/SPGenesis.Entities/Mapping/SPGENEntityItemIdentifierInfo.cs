using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Entities
{
    internal abstract class SPGENEntityItemIdentifierInfoBase
    {
    }

    internal class SPGENEntityCustomItemIdentifierInfo : SPGENEntityItemIdentifierInfoBase
    {
        public object CustomId;
        public string FieldName;
        public string FieldType;
    }

    internal class SPGENEntityBuiltInItemIdIdentifierInfo : SPGENEntityItemIdentifierInfoBase
    {
        public int ItemId;
    }
}
