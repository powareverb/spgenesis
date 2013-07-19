using System;
using Microsoft.SharePoint;

namespace SPGenesis.Entities
{
    [Serializable]
    public class SPGENEntityGeneralException : Exception
    {
        public SPGENEntityGeneralException() : base() { }

        public SPGENEntityGeneralException(string message) : base(message) { }

        public SPGENEntityGeneralException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public class SPGENEntityMapInitializationException : SPGENEntityGeneralException
    {
        public SPGENEntityMapInitializationException() : base() { }

        public SPGENEntityMapInitializationException(string message) : base(message) { }

        public SPGENEntityMapInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public class SPGENEntityMapNotFoundException : SPGENEntityGeneralException
    {
        public SPGENEntityMapNotFoundException(Type entityType)
            : base("No suitable mapper for the entity '" + entityType.FullName + "' could be found. Please make sure the mapper assembly is installed in the GAC and loaded in the current AppDomain. Otherwise try access it by specifying a suitable mapper (e.g. SPGENEntityManager<TEntity, TMapper>.Instance).") 
        {
        }
    }

    [Serializable]
    public class SPGENEntityMaxFileSizeExceededException : SPGENEntityGeneralException
    {
        public SPGENEntityMaxFileSizeExceededException(long limit)
            : base(string.Format("The file size exceeds the max size of {0} bytes. You can change this limit by setting the MaxFileSizeByteArrays property in the entity map or in the operation parameters.", limit))
        {
        }
    }
}
