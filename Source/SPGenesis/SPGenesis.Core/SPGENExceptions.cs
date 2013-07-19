using System;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public class SPGENGeneralException : Exception
    {
        public SPGENGeneralException() : base() { }

        public SPGENGeneralException(string message) : base(message) { }

        public SPGENGeneralException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SPGENFieldInternalNameMissmatchException : SPGENGeneralException
    {
        public SPGENFieldInternalNameMissmatchException() : base() { }

        public SPGENFieldInternalNameMissmatchException(string message) : base(message) { }
    }

    public class SPGENViewUrlMissmatchException : SPGENGeneralException
    {
        public SPGENViewUrlMissmatchException() : base() { }

        public SPGENViewUrlMissmatchException(string message) : base(message) { }
    }

    public class SPGENListDoesNotExistException : SPGENGeneralException
    {
        public SPGENListDoesNotExistException() : base() { }

        public SPGENListDoesNotExistException(string message) : base(message) { }
    }

    public class SPGENAssemblyValidationExcpetion : Exception
    {
        public SPGENAssemblyValidationExcpetion(string message) : base(message) { }
    }

}
