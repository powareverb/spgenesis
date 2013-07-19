using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterAttachments<TEntity> : SPGENEntityAdapter<TEntity, object>
        where TEntity : class
    {
        private SPGENEntityFileMappingMode _mode;

        internal SPGENEntityAdapterAttachments(SPGENEntityFileMappingMode mode)
        {
            _mode = mode;
            this.AutoNullCheck = false;
        }

        public override object ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            var list = arguments.DataItem.Attachments;
            if (list == null)
                return null;

            if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
            {
                return list.ToDictionary<SPGENRepositoryDataItemFile, string, byte[]>(k => k.FileName, k => k.GetByteArray(true));
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                return list.ToDictionary<SPGENRepositoryDataItemFile, string, Func<byte[]>>(k => k.FileName, k => k.FileByteArrayFunc);
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                return list.ToDictionary<SPGENRepositoryDataItemFile, string, Func<Stream>>(k => k.FileName, k => k.FileStreamFunc);
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameOnly)
            {
                return list.Select(f => f.FileName).ToList();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return null;

            var ret = new List<SPGENRepositoryDataItemFile>();

            if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArray)
            {
                throw new NotSupportedException();
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsByteArrayLazy)
            {
                foreach(var kvp in arguments.Value as IDictionary<string, Func<byte[]>>)
                    ret.Add(new SPGENRepositoryDataItemFile(kvp.Key, kvp.Value));
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameAndContentAsStreamLazy)
            {
                foreach(var kvp in arguments.Value as IDictionary<string, Func<Stream>>)
                    ret.Add(new SPGENRepositoryDataItemFile(kvp.Key, kvp.Value));
            }
            else if (_mode == SPGENEntityFileMappingMode.MapFileNameOnly)
            {
                foreach(var s in arguments.Value as IList<string>)
                    ret.Add(new SPGENRepositoryDataItemFile(s));
            }
            else
            {
                throw new NotSupportedException();
            }

            return ret;
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
