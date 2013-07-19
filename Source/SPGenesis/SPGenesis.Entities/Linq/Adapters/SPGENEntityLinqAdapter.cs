using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SPGenesis.Core;

namespace SPGenesis.Entities.Linq.Adapters
{
    public class SPGENEntityLinqAdapter
    {
        public virtual SPGENEntityEvalLinqExprResult EvalComparison(SPGENEntityEvalLinqExprArgs args)
        {
            SPGENEntityEvalLinqExprResult result = null;

            if (args.Value == null)
            {
                if (args.Field is SPFieldLookup && !args.IsLookupIdProperty)
                {
                    result = new SPGENEntityEvalLinqExprResult(args);
                    result.ValueNode.SetAttribute("Type", "Lookup");

                    return result;
                }

                if (args.Operand == "Eq")
                {
                    result = new SPGENEntityEvalLinqExprResult(args, "IsNull");
                }
                else if (args.Operand == "Neq")
                {
                    result = new SPGENEntityEvalLinqExprResult(args, "IsNotNull");
                }
                else
                {
                    result = new SPGENEntityEvalLinqExprResult(args);
                }

                //Remove value node
                result.ComparisonNode.RemoveChild(result.ComparisonNode.LastChild);

                return result;
            }
            else
            {
                result = new SPGENEntityEvalLinqExprResult(args);
            }

            if (args.Value is object[])
            {
                var valuesElement = result.ValueNode.OwnerDocument.CreateElement("Values");
                foreach (object o in (object[])args.Value)
                {
                    var v = result.ValueNode.OwnerDocument.CreateElement("Value");
                    args.Value = o;
                    SetValueNode(args, v);
                    valuesElement.AppendChild(v);
                }

                var parent = result.ValueNode.ParentNode;
                parent.ReplaceChild(valuesElement, result.ValueNode);
            }
            else
            {
                SetValueNode(args, result.ValueNode);
            }

            if (args.Field is SPFieldLookup && args.IsLookupIdProperty)
                result.MakeLookupId();

            return result;
        }

        public virtual SPGENEntityEvalLinqExprResult EvalMethodCall(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            if (args.Field is SPFieldLookup)
            {
                return EvalMethodCallLookup(mce, args);
            }
            else
            {
                return EvalMethodCallDefault(mce, args);
            }
        }

        private SPGENEntityEvalLinqExprResult EvalMethodCallDefault(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            SPGENEntityEvalLinqExprResult result = null;
            MethodInfo method = mce.Method;

            if (method.DeclaringType == typeof(string))
            {
                if (method.Name == "StartsWith")
                {
                    if (args.Operand == "Not")
                        throw new NotSupportedException("The operand 'Not' is not supported in this context.");

                    args.Value = LambdaExpression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke();
                    result = new SPGENEntityEvalLinqExprResult(args, "BeginsWith");
                }
                else if (method.Name == "Contains")
                {
                    if (args.Operand == "Not")
                        throw new NotSupportedException("The operand 'Not' is not supported in this context.");

                    args.Value = LambdaExpression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke();
                    result = new SPGENEntityEvalLinqExprResult(args, "Contains");
                }
            }
            else if (SPGENCommon.HasInterface(SPGENCommon.GetFieldValueType(args.Field), typeof(IEnumerable)))
            {
                if (method.Name == "Contains")
                {
                    if (args.Operand == "Not")
                        throw new NotSupportedException("The operand 'Not' is not supported in this context.");

                    args.Value = LambdaExpression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke();
                    if (args.Value == null)
                        throw new NotSupportedException("The Contains method does not support null as a paramter.");

                    result = new SPGENEntityEvalLinqExprResult(args, "Contains");
                }
            }

            if (result == null)
                throw new NotSupportedException(string.Format("The method '{0}' is not supported for the entity property '{1}'.", method.Name, args.SourceProperty.Name));

            SetValueNode(args, result.ValueNode);

            return result;
        }

        private SPGENEntityEvalLinqExprResult EvalMethodCallLookup(MethodCallExpression mce, SPGENEntityEvalLinqExprArgs args)
        {
            if (mce.Method.Name != "Contains")
                throw new NotSupportedException("The method '" + mce.Method.Name + "' is not supported in this context.");

            if (args.Operand == "Not")
                throw new NotSupportedException("The operand 'Not' is not supported in this context.");

            SPGENEntityEvalLinqExprResult result;
            if (SPGENCommon.HasInterface(SPGENCommon.GetFieldValueType(args.Field), typeof(IEnumerable)))
            {
                if (mce.Arguments.Count == 1)
                {
                    args.Value = LambdaExpression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke();
                }
                else if (mce.Arguments.Count == 2)
                {
                    args.Value = LambdaExpression.Lambda(mce.Arguments[1]).Compile().DynamicInvoke();
                }
                else
                {
                    throw new NotSupportedException("The method '" + mce.Method.Name + "' contained arguments that is not supported.");
                }

                if (args.Value == null)
                    throw new NotSupportedException("The Contains method does not support null as in-parameter.");

                result = new SPGENEntityEvalLinqExprResult(args, "Includes");
                result.ValueNode.SetAttribute("Type", "Lookup");
                result.ValueNode.InnerText = args.Value.ToString();
            }
            else
            {
                result = EvalMethodCallDefault(mce, args);
            }

            if (args.IsLookupIdProperty)
                result.MakeLookupId();

            return result;
        }

        private void SetValueNode(SPGENEntityEvalLinqExprArgs args, XmlElement valueElement)
        {
            if (args.Field is SPFieldBoolean)
            {
                valueElement.SetAttribute("Type", "Boolean");
                valueElement.InnerText = (bool)args.Value == true ? "1" : "0";
            }
            else if (args.Field is SPFieldDateTime)
            {
                valueElement.SetAttribute("Type", "DateTime");

                bool includeTimeValue = true;
                if (args.Expression is MemberExpression)
                {
                    var mex = args.Expression as MemberExpression;
                    if (mex.Member.DeclaringType == typeof(DateTime))
                    {
                        if (mex.Member.Name == "Date")
                            includeTimeValue = false;
                    }
                }

                valueElement.SetAttribute("IncludeTimeValue", includeTimeValue.ToString().ToUpper());
                valueElement.InnerText = SPUtility.CreateISO8601DateTimeFromSystemDateTime((DateTime)args.Value);
            }
            else if (args.Field is SPFieldNumber)
            {
                valueElement.SetAttribute("Type", "Number");
            }
            else if (args.Field is SPFieldLookup)
            {
                valueElement.SetAttribute("Type", "Lookup");
            }
            else if (args.Field is SPFieldGuid)
            {
                valueElement.SetAttribute("Type", "Guid");
            }
            else if (args.Field.TypeAsString == "Counter")
            {
                valueElement.SetAttribute("Type", "Counter");
            }
            else
            {
                valueElement.SetAttribute("Type", "Text");
            }

            if (args.Value != null && string.IsNullOrEmpty(valueElement.InnerText))
                valueElement.InnerText = args.Value.ToString();
        }

    }
}
