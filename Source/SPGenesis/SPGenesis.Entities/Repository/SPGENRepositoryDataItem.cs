using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Data;
using System.IO;

namespace SPGenesis.Entities.Repository
{
    public sealed class SPGENRepositoryDataItem
    {
        public IDictionary<string, object> FieldValues { get; set; }
        public object CustomProperty { get; set; }
        public SPListItem ListItem { get; set; }
        public DataRow DataRow { get; set; }
        public IList<SPGENRepositoryDataItemFile> Attachments { get; set; }
        public SPGENRepositoryDataItemFile File { get; set; }

        internal string[] FieldNames;
        internal bool HasFiles;

        [Obsolete("Not longer in use.", true)]
        public SPGENRepositoryDataItem(IList<string> fieldNames)
        {
        }

        public SPGENRepositoryDataItem(string[] fieldNames)
        {
            this.FieldNames = fieldNames;
            this.FieldValues = new Dictionary<string, object>();

            foreach (string fieldName in fieldNames)
            {
                this.FieldValues.Add(fieldName, null);
            }
        }

        internal void ReInitializeFields(string[] newFieldNames)
        {
            var list = this.FieldNames.ToList();

            foreach (var f in this.FieldNames)
            {
                if (!newFieldNames.Contains(f))
                {
                    list.Remove(f);
                    this.FieldValues.Remove(f);
                }
            }

            foreach (var f in newFieldNames)
            {
                if (!list.Contains(f))
                {
                    list.Add(f);
                    this.FieldValues.Add(f, null);
                }
            }

            this.FieldNames = list.ToArray();
        }

        public int ListItemId { get { return (ListItem != null) ? ListItem.ID : -1; } }

        public SPList List
        {
            get
            {
                if (this.ListItem == null)
                    return null;

                return this.ListItem.ParentList;
            }
        }

        internal string[] GetAllAttachmentFileNames()
        {
            return this.Attachments.Select(a => a.FileName).ToArray();
        }

        internal SPGENRepositoryDataItemFile GetSpecificAttachment(string fileName)
        {
            return this.Attachments.Where(a => a.FileName == fileName).FirstOrDefault();
        }
    }
}
