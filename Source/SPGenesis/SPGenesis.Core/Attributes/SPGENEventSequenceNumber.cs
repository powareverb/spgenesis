using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPGenesis.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SPGENEventSequenceNumber : Attribute
    {
        public int SequenceNumber { get; set; }

        public SPGENEventSequenceNumber(int sequenceNumber)
        {
            this.SequenceNumber = sequenceNumber;
        }
    }
}
