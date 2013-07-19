using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using System.Collections;
using System.Linq.Expressions;
using System.Xml;
using SPGenesis.Entities.Linq;

namespace SPGenesis.Entities.Adapters
{
    public class SPGENEntityAdapterEnumList<TEntity, TEnum> : SPGENEntityAdapterEnumBase<TEntity, IEnumerable<TEnum>>
        where TEntity : class
        where TEnum : struct
    {
        private TEnum? _invalidValue;
        private SPGENEntityEnumMappingOptions _options;
        private SPGENEntityAdapterEnumHelper<TEntity, TEnum> _helper;

        public SPGENEntityAdapterEnumList(TEnum? invalidValue, SPGENEntityEnumMappingOptions options)
            : base(typeof(TEnum), null, options)
        {
            _invalidValue = invalidValue;
            _options = options;
            _helper = new SPGENEntityAdapterEnumHelper<TEntity, TEnum>(options, invalidValue);
        }

        public override IEnumerable<TEnum> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return new List<TEnum>();

            if (_options == SPGENEntityEnumMappingOptions.LookupId ||
                _options == SPGENEntityEnumMappingOptions.LookupValue)
            {
                var lookupValues = new SPFieldLookupValueCollection(arguments.Value as string);
                var result = new List<TEnum>();
                if (lookupValues.Count == 0)
                    return result;

                foreach (SPFieldLookupValue l in lookupValues)
                {
                    if (_options == SPGENEntityEnumMappingOptions.LookupId)
                    {
                        var en = _helper.ParseEnumFromInt(l.LookupId, arguments);
                        if (en.HasValue)
                            result.Add(en.Value);
                        else if (_invalidValue.HasValue)
                            result.Add(_invalidValue.Value);
                    }
                    else
                    {
                        var en = _helper.ParseEnumFromString(l.LookupValue, arguments);
                        if (en.HasValue)
                            result.Add(en.Value);
                        else if (_invalidValue.HasValue)
                            result.Add(_invalidValue.Value);
                    }
                }

                return result;
            }
            else
            {
                var choices = new SPFieldMultiChoiceValue(arguments.Value as string);
                var result = new List<TEnum>();
                if (choices.Count == 0)
                    return result;

                for (int i = 0; i < choices.Count; i++ )
                {
                    var en = _helper.ParseEnumFromString(choices[i], arguments);
                    if (en.HasValue)
                        result.Add(en.Value);
                    else if (_invalidValue.HasValue)
                        result.Add(_invalidValue.Value);
                }

                return result;
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, IEnumerable<TEnum>> arguments)
        {
            if (arguments.Value == null || arguments.Value.Count() == 0)
                return null;

            if (_options == SPGENEntityEnumMappingOptions.LookupId)
            {
                var result = new SPFieldLookupValueCollection();
                foreach (TEnum t in arguments.Value)
                {
                    result.Add(new SPFieldLookupValue(Convert.ToInt32(t), null));
                }

                return result;
            }
            else if (_options == SPGENEntityEnumMappingOptions.ChoiceMappings ||
                _options == SPGENEntityEnumMappingOptions.ChoiceText)
            {
                var result = new SPFieldMultiChoiceValue();
                foreach (TEnum t in arguments.Value)
                {
                    if (_options == SPGENEntityEnumMappingOptions.ChoiceMappings)
                    {
                        result.Add(_helper.GetChoiceMappingValueByEnum(t, arguments));
                    }
                    else
                    {
                        result.Add(t.ToString());
                    }
                }

                return result;
            }
            else
            {
                throw new SPGENEntityGeneralException("Invalid update of lookup field. Only lookup ID is supported for updating.");
            }
        }
    }

    public class SPGENEntityAdapterEnumNullable<TEntity, TEnum> : SPGENEntityAdapterEnumBase<TEntity, Nullable<TEnum>>
        where TEntity : class
        where TEnum : struct
    {
        private TEnum? _invalidValue;
        private TEnum? _emptyValue;
        private SPGENEntityEnumMappingOptions _options;
        private SPGENEntityAdapterEnumHelper<TEntity, TEnum> _helper;

        public SPGENEntityAdapterEnumNullable(TEnum invalidEnumValue, SPGENEntityEnumMappingOptions options)
            : this(null, invalidEnumValue, options)
        {
        }

        public SPGENEntityAdapterEnumNullable(TEnum? emptyEnumValue, TEnum? invalidEnumValue, SPGENEntityEnumMappingOptions options)
            : base(typeof(TEnum), emptyEnumValue, options)
        {
            _invalidValue = invalidEnumValue;
            _emptyValue = emptyEnumValue;
            _options = options;
            _helper = new SPGENEntityAdapterEnumHelper<TEntity, TEnum>(options, invalidEnumValue);
        }

        public override Nullable<TEnum> ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
            {
                if (_emptyValue.HasValue)
                    return _emptyValue;

                return null;
            }

            if (_options == SPGENEntityEnumMappingOptions.LookupId ||
                _options == SPGENEntityEnumMappingOptions.LookupValue)
            {
                var lookupValues = new SPFieldLookupValueCollection(arguments.Value as string);
                if (lookupValues.Count == 0)
                    return null;

                if (_options == SPGENEntityEnumMappingOptions.LookupId)
                {
                    return _helper.ParseEnumFromInt(lookupValues[0].LookupId, arguments);
                }
                else
                {
                    return _helper.ParseEnumFromString(lookupValues[0].LookupValue, arguments);
                }
            }
            else
            {
                var choices = new SPFieldMultiChoiceValue(arguments.Value as string);
                if (choices.Count == 0)
                    return null;

                return _helper.ParseEnumFromString(choices[0], arguments);
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TEnum?> arguments)
        {
            if (!arguments.Value.HasValue)
                return null;

            if (_options == SPGENEntityEnumMappingOptions.LookupId)
            {
                var lookupValue = new SPFieldLookupValue();
                lookupValue.LookupId = Convert.ToInt32(arguments.Value.Value);

                return lookupValue;
            }
            else if (_options == SPGENEntityEnumMappingOptions.ChoiceMappings)
            {
                return _helper.GetChoiceMappingValueByEnum(arguments.Value.Value, arguments);
            }
            else if (_options == SPGENEntityEnumMappingOptions.ChoiceText)
            {
                return arguments.Value.Value.ToString();
            }
            else
            {
                throw new SPGENEntityGeneralException("Invalid update of lookup field. Only lookup ID is supported for updating.");
            }
        }
    }

    public class SPGENEntityAdapterEnum<TEntity, TEnum> : SPGENEntityAdapterEnumBase<TEntity, TEnum>
        where TEntity : class
        where TEnum : struct
    {
        private TEnum _invalidValue;
        private TEnum _emptyValue;
        private SPGENEntityEnumMappingOptions _options;
        private SPGENEntityAdapterEnumHelper<TEntity, TEnum> _helper;

        public SPGENEntityAdapterEnum(TEnum emptyValue, TEnum invalidValue, SPGENEntityEnumMappingOptions options)
            : base(typeof(TEnum), emptyValue, options)
        {
            _emptyValue = emptyValue;
            _invalidValue = invalidValue;
            _options = options;
            _helper = new SPGENEntityAdapterEnumHelper<TEntity, TEnum>(options, invalidValue);
        }


        public override TEnum ConvertToPropertyValue(SPGENEntityAdapterConvArgs<TEntity, object> arguments)
        {
            if (arguments.Value == null)
                return _emptyValue;

            if (_options == SPGENEntityEnumMappingOptions.LookupId ||
                _options == SPGENEntityEnumMappingOptions.LookupValue)
            {
                var lookupValues = new SPFieldLookupValueCollection(arguments.Value as string);
                if (lookupValues.Count == 0)
                    return _emptyValue;

                if (_options == SPGENEntityEnumMappingOptions.LookupId)
                {
                    var en = _helper.ParseEnumFromInt(lookupValues[0].LookupId, arguments);
                    if (en.HasValue)
                        return en.Value;
                    else
                        return _invalidValue;
                }
                else
                {
                    var en = _helper.ParseEnumFromString(lookupValues[0].LookupValue, arguments);
                    if (en.HasValue)
                        return en.Value;
                    else
                        return _invalidValue;
                }
            }
            else
            {
                var choices = new SPFieldMultiChoiceValue(arguments.Value as string);
                if (choices.Count == 0)
                    return _emptyValue;

                var en = _helper.ParseEnumFromString(choices[0], arguments);
                if (en.HasValue)
                    return en.Value;
                else
                    return _invalidValue;
            }
        }

        public override object ConvertToListItemValue(SPGENEntityAdapterConvArgs<TEntity, TEnum> arguments)
        {
            if (Enum.Equals(arguments.Value, _emptyValue) ||
                Enum.Equals(arguments.Value, _invalidValue))
                return null;

            if (_options == SPGENEntityEnumMappingOptions.LookupId)
            {
                var lookupValue = new SPFieldLookupValue();
                lookupValue.LookupId = Convert.ToInt32(arguments.Value);

                return lookupValue;
            }
            else if (_options == SPGENEntityEnumMappingOptions.ChoiceMappings)
            {
                return _helper.GetChoiceMappingValueByEnum(arguments.Value, arguments);
            }
            else if (_options == SPGENEntityEnumMappingOptions.ChoiceText)
            {
                return arguments.Value.ToString();
            }
            else
            {
                throw new SPGENEntityGeneralException("Invalid update of lookup field. Only lookup ID is supported for updating.");
            }
        }
    }

    internal class SPGENEntityAdapterEnumHelper<TEntity, TEnum>
        where TEntity : class
        where TEnum : struct
    {
        private TEnum? _invalidEnumValue;
        private SPGENEntityEnumMappingOptions _mappingOptions;
        private Type _enumType;

        private bool _isInitialized;
        private readonly object _initLock = new object();
        private Dictionary<int, TEnum> _intMappings;
        private Dictionary<string, TEnum> _stringMappings;

        internal SPGENEntityAdapterEnumHelper(SPGENEntityEnumMappingOptions options, TEnum? invalidValue)
        {
            _enumType = typeof(TEnum);
            _mappingOptions = options;
            _invalidEnumValue = invalidValue;
        }

        private void EnsureInit(ISPGENEntityAdapterConvArgs<TEntity> arguments)
        {
            if (_isInitialized)
                return;

            lock (_initLock)
            {
                if (_isInitialized)
                    return;

                _intMappings = new Dictionary<int,TEnum>();
                foreach(int i in Enum.GetValues(_enumType))
                {
                    _intMappings.Add(i, (TEnum)Enum.ToObject(_enumType, i));
                }

                if (_mappingOptions == SPGENEntityEnumMappingOptions.ChoiceMappings)
                {
                    InitChoiceMappingValues(arguments.List.Fields.GetFieldByInternalName(arguments.FieldName));
                }
                else
                {
                    _stringMappings = new Dictionary<string, TEnum>();
                    foreach (string s in Enum.GetNames(_enumType))
                    {
                        _stringMappings.Add(s, (TEnum)Enum.Parse(_enumType, s));
                    }
                }

                _isInitialized = true;
            }
        }

        private void InitChoiceMappingValues(SPField field)
        {
            if (!SPGENChoiceMappingsCache.HasMappings(field))
                throw new SPGENEntityGeneralException("No choice mappings where found for the field '" + field.InternalName + "'.");

            _stringMappings = new Dictionary<string, TEnum>();

            var d = SPGENChoiceMappingsCache.GetMappings(field);
            foreach (var kvp in d)
            {
                if (!_stringMappings.ContainsKey(kvp.Value))
                {
                    int n;
                    if (!int.TryParse(kvp.Key, out n))
                    {
                        throw new SPGENEntityGeneralException("The choice mapping for the field '" + field.InternalName + "' contained invalid values. Only integers are allowed.");
                    }

                    _stringMappings.Add(kvp.Value, (TEnum)Enum.ToObject(_enumType, n));
                }
            }
        }

        internal TEnum? ParseEnumFromString(string value, ISPGENEntityAdapterConvArgs<TEntity> arguments)
        {
            EnsureInit(arguments);

            if (!_stringMappings.ContainsKey(value))
                return _invalidEnumValue;

            return _stringMappings[value];
        }

        internal TEnum? ParseEnumFromInt(int value, ISPGENEntityAdapterConvArgs<TEntity> arguments)
        {
            EnsureInit(arguments);

            if (!_intMappings.ContainsKey(value))
                return _invalidEnumValue;

            return _intMappings[value];
        }

        internal string GetChoiceMappingValueByEnum(TEnum enumValue, ISPGENEntityAdapterConvArgs<TEntity> arguments)
        {
            EnsureInit(arguments);

            foreach (var kvp in _stringMappings)
            {
                if (Enum.Equals(kvp.Value, enumValue))
                    return kvp.Key;
            }

            return null;
        }
    }

    public abstract class SPGENEntityAdapterEnumBase<TEntity, T> : SPGENEntityAdapter<TEntity, T>
        where TEntity : class
    {
        private object _emptyEnumValue;
        private SPGENEntityEnumMappingOptions _mappingOptions;
        private Type _enumType;

        public SPGENEntityAdapterEnumBase(Type enumType, object emptyEnumValue, SPGENEntityEnumMappingOptions mappingOptions)
        {
            _emptyEnumValue = emptyEnumValue;
            _mappingOptions = mappingOptions;
            _enumType = enumType;
        }

        public override SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            var result = new SPGENEntityEvalLinqExprResult(args);
            ProcessResult(args, ref result);

            return result;
        }

        public override SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            if (mce.Method.Name != "Contains" && mce.Method.DeclaringType != typeof(IEnumerable))
                throw new NotSupportedException("The method '" + mce.Method.Name + "' is not supported in this context.");

            var m = args.Expression as MethodCallExpression;
            if (m.Arguments.Count == 1)
            {
                args.Value = LambdaExpression.Lambda(m.Arguments[0]).Compile().DynamicInvoke();
            }
            else if (m.Arguments.Count == 2)
            {
                args.Value = LambdaExpression.Lambda(m.Arguments[1]).Compile().DynamicInvoke();
            }
            else
            {
                throw new NotSupportedException("The method '" + mce.Method.Name + "' contained arguments that is not supported.");
            }

            if (args.Value == null)
                throw new NotSupportedException("The Contains method does not support null as in-parameter.");

            //args.Operand = (args.Operand == "Not") ? "Neq" : "Eq";

            var result = new SPGENEntityEvalLinqExprResult(args, args.Operand);

            ProcessResult(args, ref result);

            return result;
        }

        private void ProcessResult(SPGENEntityEvalLinqExprArgs args, ref SPGENEntityEvalLinqExprResult result)
        {
            object enumValue;
            if (args.Value.GetType() == typeof(int))
            {
                enumValue = Enum.ToObject(_enumType, (int)args.Value);
            }
            else if (args.Value.GetType() == typeof(string))
            {
                enumValue = Enum.Parse(_enumType, args.Value.ToString());
            }
            else if (args.Value.GetType() == _enumType)
            {
                enumValue = args.Value;
            }
            else
            {
                throw new NotSupportedException("The enum type used is not supported.");
            }

            if (_emptyEnumValue != null)
            {
                if (Enum.Equals(_emptyEnumValue, enumValue))
                {
                    args.Value = null;
                    result = base.EvalComparison(args);
                    return;
                }
            }

            if (_mappingOptions == SPGENEntityEnumMappingOptions.ChoiceMappings)
            {
                if (!SPGENChoiceMappingsCache.HasMappings(args.Field))
                    throw new SPGENEntityGeneralException("No choice mappings found for this field.");

                int v = Convert.ToInt32(enumValue);

                result.ValueNode.SetAttribute("Type", (args.Field is SPFieldMultiChoice) ? "MultiChoice" : "Choice");
                result.ValueNode.InnerText = SPGENChoiceMappingsCache.GetTextValue(args.Field, v.ToString());
            }
            else if (_mappingOptions == SPGENEntityEnumMappingOptions.LookupId)
            {
                result.ValueNode.SetAttribute("Type", "Lookup");
                result.ValueNode.InnerText = Convert.ToInt32(enumValue).ToString();
            }
            else
            {
                result.ValueNode.SetAttribute("Type", (args.Field is SPFieldMultiChoice) ? "MultiChoice" : "Choice");
                result.ValueNode.InnerText = enumValue.ToString();
            }
        }
    }

}
