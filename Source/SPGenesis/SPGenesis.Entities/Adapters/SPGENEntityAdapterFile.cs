using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterFile<TEntity> : SPGENEntityAdapter<TEntity, object>
        where TEntity : class
    {
        private SPGENEntityFileMappingMode _mode;

        internal SPGENEntityAdapterFile(SPGENEntityFileMappingMode mode)
        {
            _mode = mode;
            this.AutoNullCheck = false;
        }

        public override object ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            var file = arguments.DataItem.File;
            if (file == null)
                return null;

            if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
            {
                return file.FileByteArrayFunc.Invoke();
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                return file.FileByteArrayFunc;
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                return file.FileStreamFunc;
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameOnly)
            {
                return file.FileName;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            string fileName = arguments.DataItem.FieldValues["FileLeafRef"] as string;
            if (string.IsNullOrEmpty(fileName))
                return null;

            if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                throw new NotSupportedException();
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                return new SPGENRepositoryDataItemFile(fileName, arguments.Value as Func<byte[]>);
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                return new SPGENRepositoryDataItemFile(fileName, arguments.Value as Func<Stream>);
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameOnly)
            {
                return new SPGENRepositoryDataItemFile(fileName);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override Linq.SPGENEntityEvalLinqExprResult EvalComparison(Linq.SPGENEntityEvalLinqExprArgs args)
        {
            throw new NotSupportedException();
        }

        public override Linq.SPGENEntityEvalLinqExprResult EvalMethodCall(System.Linq.Expressions.MethodCallExpression mce, Linq.SPGENEntityEvalLinqExprArgs args)
        {
            throw new NotSupportedException();
        }

    }
}
