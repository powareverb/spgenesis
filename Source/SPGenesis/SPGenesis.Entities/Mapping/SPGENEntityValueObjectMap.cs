using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Linq;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities
{
    public abstract class SPGENEntityValueObjectMap<TValueObject, TFieldValue, TFieldValueCollection> : SPGENEntityValueObjectMap<TValueObject, TFieldValue>
        where TValueObject : class
        where TFieldValueCollection : class
    {
        protected virtual void ConvertToCollectionOfValueObjects<TEntity>(ref IList<TValueObject> valueObjects, TFieldValueCollection fieldValueCollection, SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
        }

        protected virtual void ConvertFromCollectionOfValueObjects<TEntity>(ref TFieldValueCollection fieldValueCollection, IList<TValueObject> valueObjects, SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
        }

        protected virtual TFieldValueCollection CreateFieldCollectionInstance<TEntity>(SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
            return (TFieldValueCollection)SPGENCommon.ConstructListItemValue(context.Web, typeof(TFieldValueCollection));
        }

        internal override IEnumerable<TValueObject> ToValueObjects<TEntity>(object fieldValueCollection, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            IList<TValueObject> ret = new List<TValueObject>();

            if (!(fieldValueCollection is TFieldValueCollection))
            {
                fieldValueCollection = SPGENCommon.ConvertListItemValue(context.Web, fieldValueCollection, typeof(TFieldValueCollection));
            }

            ConvertToCollectionOfValueObjects(ref ret, (TFieldValueCollection)fieldValueCollection, context);

            return ret;
        }

        internal override object ToFieldValue<TEntity>(IEnumerable<TValueObject> valueObjects, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            TFieldValueCollection collection = CreateFieldCollectionInstance(context);
            IList<TValueObject> ret = (valueObjects == null) ? new List<TValueObject>() : new List<TValueObject>(valueObjects);

            ConvertFromCollectionOfValueObjects(ref collection, ret, context);

            return collection;
        }
    }

    public abstract class SPGENEntityValueObjectMap<TValueObject, TFieldValue> : SPGENEntityValueObjectMapBase<TValueObject>
        where TValueObject : class
    {
        private static SPGENEntityMapInitializationState _mapperInitState;
        private static readonly object _mapperInitLock = new object();
        private static HashSet<PropertyInfo> _queryableProperties;
        private static PropertyInfo _identifierProperty;
        private static PropertyInfo _valueProperty;

        private Type _fieldValueType;
        private bool _isCalculatedField;
        private SPGENEntityLinqAdapter _linqAdapter = new SPGENEntityLinqAdapter();

        public SPGENEntityValueObjectMap()
        {
            if (_mapperInitState == SPGENEntityMapInitializationState.Ready)
                return;

            lock (_mapperInitLock)
            {
                if (_mapperInitState == SPGENEntityMapInitializationState.Ready)
                    return;

                if (_mapperInitState == SPGENEntityMapInitializationState.Initializing)
                    throw new SPGENEntityMapInitializationException("The mapper for the value object '" + typeof(TValueObject).FullName + "' can not be accessed while it is being initialized.");

                try
                {
                    _mapperInitState = SPGENEntityMapInitializationState.Initializing;

                    _queryableProperties = GetAllQueryableProperties();
                }
                catch (Exception ex)
                {
                    _mapperInitState = SPGENEntityMapInitializationState.NotInitialized;

                    throw new SPGENEntityMapInitializationException("Failed to initialize the value object map '" + typeof(TValueObject).FullName + "'. " + ex.Message, ex);
                }

                _mapperInitState = SPGENEntityMapInitializationState.Ready;
            }
        }


        protected override TValueObject CreateValueObject<TEntity>(SPGENEntityOperationContext<TEntity> context)
        {
            return Activator.CreateInstance<TValueObject>();
        }

        protected virtual object CreateFieldValueInstance<TEntity>(SPGENEntityOperationContext<TEntity> context, Type requestedType) where TEntity : class
        {
            return SPGENCommon.ConstructListItemValue(context.Web, requestedType);
        }

        protected virtual void ConvertToValueObject<TEntity>(ref TValueObject valueObject, TFieldValue fieldValue, SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
        }

        protected virtual void ConvertToFieldValue<TEntity>(TValueObject valueObject, ref TFieldValue fieldValue, SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
        }

        protected virtual Expression<Func<TValueObject, object>> GetIdentifierProperty()
        {
            return null;
        }

        protected virtual Expression<Func<TValueObject, object>> GetValueProperty()
        {
            return null;
        }

        protected virtual void RegisterQueryableProperties(IList<Expression<Func<TValueObject, object>>> list)
        {
        }

        protected PropertyInfo GetValuePropertyInfo()
        {
            return _valueProperty;
        }

        internal override HashSet<PropertyInfo> GetQueryableProperties()
        {
            return _queryableProperties;
        }

        internal override PropertyInfo IdentifierProperty
        {
            get
            {
                return _identifierProperty;
            }
        }

        internal override PropertyInfo ValueProperty
        {
            get
            {
                return _valueProperty;
            }
        }

        internal override TValueObject ToValueObject<TEntity>(object fieldValue, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            var valueObject = CreateValueObject(context);

            if (!(fieldValue is TFieldValue))
            {
                fieldValue = SPGENCommon.ConvertListItemValue(context.Web, fieldValue, typeof(TFieldValue));
            }

            ConvertToValueObject<TEntity>(ref valueObject, (TFieldValue)fieldValue, context);

            return valueObject;
        }

        internal override IEnumerable<TValueObject> ToValueObjects<TEntity>(object fieldValueCollection, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            IList<TValueObject> ret = new List<TValueObject>();

            fieldValueCollection = SPGENCommon.ConvertListItemValue(context.Web, fieldValueCollection, _fieldValueType);

            if (fieldValueCollection is SPFieldMultiChoiceValue)
            {
                if (_valueProperty == null)
                    throw new SPGENEntityGeneralException("Failed to find the value property. Make sure that the GetValueProperty method is implemented.");

                var c = fieldValueCollection as SPFieldMultiChoiceValue;

                for (int i = 0; i < c.Count; i++)
                {
                    var vo = CreateValueObject(context);
                    ConvertToValueObject(ref vo, (TFieldValue)(object)c[i], context);
                    ret.Add(vo);
                }
            }
            else if (fieldValueCollection is SPFieldMultiColumnValue)
            {
                if (_valueProperty == null)
                    throw new SPGENEntityGeneralException("Failed to find the value property. Make sure that the GetValueProperty method is implemented.");

                var c = fieldValueCollection as SPFieldMultiColumnValue;

                for (int i = 0; i < c.Count; i++)
                {
                    var vo = CreateValueObject(context);
                    ConvertToValueObject(ref vo, (TFieldValue)(object)c[i], context);
                    ret.Add(vo);
                }
            }
            else if (fieldValueCollection is IEnumerable)
            {
                IEnumerable<TFieldValue> e;
                try
                {
                    e = (fieldValueCollection as IEnumerable).Cast<TFieldValue>().ToArray();
                }
                catch (InvalidCastException ex)
                {
                    throw new SPGENEntityGeneralException(string.Format("Could not cast the field value collection to a collection of '{0}'.", typeof(TFieldValue).FullName), ex);
                }

                foreach (TFieldValue v in e)
                {
                    var vo = CreateValueObject(context);
                    ConvertToValueObject(ref vo, v, context);
                    ret.Add(vo);
                }
            }
            else
            {
                throw new NotSupportedException("The collection type is not supported.");
            }

            return ret;
        }

        internal override object ToFieldValue<TEntity>(TValueObject valueObject, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            TFieldValue fieldValue = (TFieldValue)CreateFieldValueInstance(context, typeof(TFieldValue));

            ConvertToFieldValue<TEntity>(valueObject, ref fieldValue, context);

            return fieldValue;
        }

        internal override object ToFieldValue<TEntity>(IEnumerable<TValueObject> valueObjects, string fieldName, SPGENEntityOperationContext<TEntity> context)
        {
            EnsureFieldValueType(context);

            object fieldValueCollection = CreateFieldValueInstance(context, _fieldValueType);

            if (fieldValueCollection is SPFieldMultiChoiceValue)
            {
                if (_valueProperty == null)
                    throw new SPGENEntityGeneralException("Failed to find the value property. Make sure that the GetValueProperty method is implemented.");

                var c = fieldValueCollection as SPFieldMultiChoiceValue;

                foreach (TValueObject vo in valueObjects)
                {
                    var v = _valueProperty.GetValue(vo, null);

                    c.Add(v as string);
                }
            }
            else if (fieldValueCollection is SPFieldMultiColumnValue)
            {
                if (_valueProperty == null)
                    throw new SPGENEntityGeneralException("Failed to find the value property. Make sure that the GetValueProperty method is implemented.");

                var c = fieldValueCollection as SPFieldMultiColumnValue;

                foreach (TValueObject vo in valueObjects)
                {
                    var v = _valueProperty.GetValue(vo, null);

                    c.Add(v as string);
                }
            }
            else if (fieldValueCollection is IList)
            {
                var c = fieldValueCollection as IList;

                foreach (TValueObject vo in valueObjects)
                {
                    var fv = (TFieldValue)CreateFieldValueInstance(context, typeof(TFieldValue));

                    ConvertToFieldValue(vo, ref fv, context);

                    c.Add(fv);
                }
            }
            else
            {
                throw new NotSupportedException("The collection type is not supported.");
            }

            return fieldValueCollection;
        }


        private HashSet<PropertyInfo> GetAllQueryableProperties()
        {
            var list = new List<Expression<Func<TValueObject, object>>>();
            RegisterQueryableProperties(list);

            var result = new HashSet<PropertyInfo>();
            foreach (var expr in list)
            {
                PropertyInfo pInfo = SPGENCommon.ResolveMemberFromExpression<Func<TValueObject, object>>(expr) as PropertyInfo;
                if (pInfo == null)
                    throw new NotSupportedException();

                AddQueryableProperty(pInfo, result);
            }

            _identifierProperty = ReadGetIdentifierProperty();
            if (_identifierProperty != null)
                AddQueryableProperty(_identifierProperty, result);

            _valueProperty = ReadGetValueProperty();
            if (_valueProperty != null)
                AddQueryableProperty(_valueProperty, result);

            return result;
        }

        private PropertyInfo ReadGetIdentifierProperty()
        {
            var expression = GetIdentifierProperty();
            if (expression == null)
                return null;

            MemberInfo member = SPGENCommon.ResolveMemberFromExpression<Func<TValueObject, object>>(expression);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported for mapping the identifier property for mapper " + this.GetType().Name + ".");

            return member as PropertyInfo;
        }

        private PropertyInfo ReadGetValueProperty()
        {
            var expression = GetValueProperty();
            if (expression == null)
                return null;

            MemberInfo member = SPGENCommon.ResolveMemberFromExpression<Func<TValueObject, object>>(expression);
            if (!(member is PropertyInfo))
                throw new ArgumentException("Only properties are supported for mapping the identifier property for mapper " + this.GetType().Name + ".");

            return member as PropertyInfo;
        }

        private void AddQueryableProperty(PropertyInfo property, HashSet<PropertyInfo> h)
        {
            if (!h.Contains(property))
                h.Add(property);
        }

        protected internal void EnsureFieldValueType<TEntity>(SPGENEntityOperationContext<TEntity> context) where TEntity : class
        {
            if (_fieldValueType != null)
                return;

            SPField field = context.GetCurrentField();
            if (field != null)
            {
                _isCalculatedField = (field is SPFieldCalculated);
                _fieldValueType = SPGENCommon.GetFieldValueType(field);
            }
            else
            {
                _isCalculatedField = false;
                _fieldValueType = typeof(TFieldValue);
            }
        }


        [Obsolete("Use the EvalComparison method.", true)]
        public virtual SPGENEntityEvalLinqExprResult EvalBinaryExpression(SPGENEntityEvalLinqExprArgs args) { return EvalComparison(args); }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            ProcessIfLookupField(args);

            return _linqAdapter.EvalComparison(args);
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            ProcessIfLookupField(args);

            return _linqAdapter.EvalMethodCall(mce, args);
        }

        private static void ProcessIfLookupField(SPGENEntityEvalLinqExprArgs args)
        {
            if (typeof(TFieldValue) == typeof(SPFieldLookupValue) ||
                typeof(TFieldValue).IsSubclassOf(typeof(SPFieldLookupValue)) ||
                typeof(TFieldValue) == typeof(SPFieldLookupValueCollection) ||
                typeof(TFieldValue).IsSubclassOf(typeof(SPFieldLookupValueCollection)) ||
                typeof(TFieldValue) == typeof(SPFieldUserValueCollection) ||
                typeof(TFieldValue).IsSubclassOf(typeof(SPFieldUserValueCollection)))
            {
                args.IsLookupIdProperty = (args.SourceProperty != _valueProperty);
            }
            else
            {
                args.IsLookupIdProperty = false;
            }
        }

        public override Type GetFieldValueType()
        {
            return typeof(TFieldValue);
        }
    }
}
