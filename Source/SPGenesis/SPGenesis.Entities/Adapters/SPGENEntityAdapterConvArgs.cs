using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Repository;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterConvArgs<TEntity, TValue> : ISPGENEntityAdapterConvArgs<TEntity>
        where TEntity : class
    {
        [Obsolete("Not longer in use. Use FieldName instead.", true)]
        public Guid? FieldId { get; internal set; }

        public string FieldName { get; set; }
        public TEntity Entity { get; set; }
        public TValue Value { get; set; }
        public PropertyInfo TargetProperty { get; set; }
        public SPGENEntityOperationParameters OperationParameters { get; set; }
        public SPGENEntityOperationContext<TEntity> OperationContext { get; set; }
        public SPGENRepositoryDataItem DataItem { get { return this.OperationContext.DataItem; } }

        public bool ShouldIncludeFileContent { get { return this.OperationContext.ShouldIncludeFiles; } }
        
        public SPField Field
        {
            get { return this.OperationContext.GetCurrentField(); }
        }

        public SPWeb Web
        {
            get { return this.OperationContext.Web; }
        }

        public SPList List
        {
            get { return this.OperationContext.List; }
        }

        public void SetValue(object value)
        {
            this.Value = (TValue)value;
        }

        public SPListItem ListItem
        {
            get
            {
                return (this.DataItem != null) ? this.DataItem.ListItem : null;
            }
        }

        [Obsolete()]
        public SPGENEntityAdapterConvArgs<TEntity, TTargetValue> Clone<TTargetValue>(bool skipConvertValue)
        {
            var target = new SPGENEntityAdapterConvArgs<TEntity, TTargetValue>();

            target.Entity = this.Entity;
            target.FieldName = this.FieldName;
            target.OperationContext = this.OperationContext;
            target.TargetProperty = this.TargetProperty;
            target.OperationParameters = this.OperationParameters;

            if (!skipConvertValue)
            {
                target.Value = (TTargetValue)Convert.ChangeType(this.Value, typeof(TTargetValue));
            }

            return target;
        }

        public SPGENEntityAdapterConvArgs<TEntity, TTargetValue> Clone<TTargetValue>(TTargetValue newValue)
        {
            var target = new SPGENEntityAdapterConvArgs<TEntity, TTargetValue>();

            target.Entity = this.Entity;
            target.FieldName = this.FieldName;
            target.OperationContext = this.OperationContext;
            target.TargetProperty = this.TargetProperty;
            target.OperationParameters = this.OperationParameters;
            target.SetValue(newValue);

            return target;
        }

        public object GetValue()
        {
            return this.Value;
        }
    }

    public interface ISPGENEntityAdapterConvArgs<TEntity>
        where TEntity : class
    {
        TEntity Entity { get; set; }
        string FieldName { get; set; }
        SPWeb Web { get; }
        SPList List { get; }
        SPListItem ListItem { get; }
        SPGENEntityOperationContext<TEntity> OperationContext { get; set; }
        SPGENRepositoryDataItem DataItem { get; }
        PropertyInfo TargetProperty { get; set; }
        SPGENEntityOperationParameters OperationParameters { get; set; }
        void SetValue(object value);
        object GetValue();
    }
}
