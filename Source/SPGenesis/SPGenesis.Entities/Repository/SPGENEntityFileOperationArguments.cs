using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Entities.Repository
{
    public sealed class SPGENEntityFileOperationArguments
    {
        internal bool ForceFileSave { get; set; }
        internal SPGENEntityFileMappingMode FileMappingMode { get; set; }

        public SPOpenBinaryOptions OpenFileOptions { get; set;}
        public SPFileSaveBinaryParameters SaveFileParameters { get; set; }
        public SPFileCollectionAddParameters SaveNewFileParameters { get; set; }
        public long MaxFileSizeByteArrays { get; set; }
        public long MaxFileSizeTotalByteArraysBatch { get; set; }

        internal SPGENEntityFileOperationArguments()
        {
        }
    }
}
