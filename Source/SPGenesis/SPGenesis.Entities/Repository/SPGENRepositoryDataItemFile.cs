using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SharePoint;

namespace SPGenesis.Entities.Repository
{
    public class SPGENRepositoryDataItemFile
    {
        [ThreadStatic]
        private static bool _isInternalCall;
        [ThreadStatic]
        private static bool _skipInternalCallCheck;

        public SPGENRepositoryDataItemFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName", "Parameter can not be null.");

            this.FileName = fileName;
        }

        internal SPGENRepositoryDataItemFile(string fileName, Func<Stream> stream)
        {
            this.FileName = fileName;
            this.FileStreamFunc = stream;
        }

        internal SPGENRepositoryDataItemFile(string fileName, Func<byte[]> byteArray)
        {
            this.FileName = fileName;
            this.FileByteArrayFunc = byteArray;
        }

        internal SPGENRepositoryDataItemFile(SPFile file, SPGENEntityFileOperationArguments fileOperationParams)
        {
            this.FileName = file.Name;

            if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                this.FileStreamFunc = new Func<Stream>(() =>
                    {
                        if (_isInternalCall && !_skipInternalCallCheck)
                            return null;

                        return file.OpenBinaryStream(fileOperationParams.OpenFileOptions);
                    });
            }
            else if (fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy ||
                       fileOperationParams.FileMappingMode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
            {
                this.FileByteArrayFunc = new Func<byte[]>(() =>
                {
                    if (_isInternalCall && !_skipInternalCallCheck)
                        return null;

                    return file.OpenBinary(fileOperationParams.OpenFileOptions);
                });
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public string FileName { get; private set;  }

        internal Func<Stream> FileStreamFunc { get; private set; }

        internal Func<byte[]> FileByteArrayFunc { get; private set; }

        internal Stream GetStream(bool forceGet)
        {
            try
            {
                _isInternalCall = true;
                _skipInternalCallCheck = forceGet;
                
                return this.FileStreamFunc.Invoke();
            }
            finally
            {
                _isInternalCall = false;
            }
        }

        internal byte[] GetByteArray(bool forceGet)
        {
            try
            {
                _isInternalCall = true;
                _skipInternalCallCheck = forceGet;

                var ret = this.FileByteArrayFunc.Invoke();

                return ret;
            }
            finally
            {
                _isInternalCall = false;
            }
        }


        public static implicit operator byte[](SPGENRepositoryDataItemFile file)
        {
            return file.GetByteArray(true);
        }

        public static implicit operator Stream(SPGENRepositoryDataItemFile file)
        {
            return file.GetStream(true);
        }
    }
}
