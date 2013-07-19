using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using SPGenesis.Entities.Linq.Adapters;
using SPGenesis.Core;

namespace SPGenesis.Entities.Linq
{
    internal class SPGENLinqExpressionTreeVisitor<TEntity> : SPGENLinqExpressionVisitorBase
        where TEntity : class
    {        
        private SPGENEntityOperationContext<TEntity> _context;
        private XmlNode _whereNode;
        private XmlNode _currentBooleanNode;
        private SPField _lastReferencedField;
        private PropertyInfo _lastReferencedProperty;
        private PropertyInfo _lastReferencedOwnerProperty;
        private Expression _lastUnevaluatedBooleanExpression;
        private bool _isNotOperand;
        private string _lastOperand;
        private string _lastFieldName;
        private bool _isNullableComparison;
        private bool _isNullableHasValueComparison;

        private List<KeyValuePair<PropertyInfo, bool>> _orderByProperties = new List<KeyValuePair<PropertyInfo,bool>>();
        private SPGENEntityLinqAdapter _defaultAdapter = new SPGENEntityLinqAdapter();

        private SPGENLinqExpressionTreeVisitor(Type entityType, SPGENEntityOperationContext<TEntity> context)
        {
            if (context.List == null)
                throw new SPGENEntityGeneralException("There is no list instance in the operation context.");

            _context = context;

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(@"<Query><Where></Where></Query>");

            this.CAMLQuery = xmldoc.DocumentElement;

            _whereNode = this.CAMLQuery.FirstChild;
        }

        public static SPGENLinqExpressionTreeVisitor<TEntity> Execute(Expression expression, Type entityType, SPGENEntityOperationContext<TEntity> context) 
        {
            var visitor = new SPGENLinqExpressionTreeVisitor<TEntity>(entityType, context);

            visitor.Visit(expression);
            visitor.EnsureWhereNode();
            visitor.EnsureOrderByNode();

            return visitor;
        }

        internal XmlNode CAMLQuery { get; private set; }

        private XmlDocument CAMLOwnerDocument { get { return this.CAMLQuery.OwnerDocument; } }

        private XmlNode CAMLWhereNode { get { return this.CAMLQuery.FirstChild; } }

        public string GetCAMLAsString(bool formatted)
        {
            if (this.CAMLQuery == null)
                return string.Empty;

            if (formatted)
            {
                StringBuilder sb = new StringBuilder();
                XmlWriter xw = XmlTextWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true, IndentChars = "  ", Indent = true, CloseOutput = true });

                foreach (XmlNode node in this.CAMLQuery.ChildNodes)
                {
                    node.WriteTo(xw);
                    xw.Flush();
                }

                string result = sb.ToString();
                xw.Close();
                
                return result;
            }
            else
            {
                return this.CAMLQuery.InnerXml;
            }
        }

        private void EnsureWhereNode()
        {
            if (_whereNode.ChildNodes.Count == 0)
                _whereNode.ParentNode.RemoveChild(_whereNode);
        }

        private void EnsureOrderByNode()
        {
            if (_orderByProperties.Count == 0)
                return;

            var orderBy = CreateElement("OrderBy");

            for (int i = _orderByProperties.Count - 1; i >= 0; i--)
            {
                var kvp = _orderByProperties[i];
                var ep = FindEntityPropertyInfo(kvp.Key);
                var spfield = FindSPField(ep.FieldName);

                var fieldRef = CreateElement("FieldRef");
                fieldRef.SetAttribute("Name", spfield.InternalName);
                
                if (kvp.Value)
                    fieldRef.SetAttribute("Ascending", "FALSE");

                orderBy.AppendChild(fieldRef);
            }

            this.CAMLQuery.AppendChild(orderBy);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;

            switch (exp.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    AddBooleanOperandNode("And");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    AddBooleanOperandNode("Or");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.LessThan:
                    SetCurrentOperand("Lt");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.GreaterThan:
                    SetCurrentOperand("Gt");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.GreaterThanOrEqual:
                    SetCurrentOperand("Geq");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.LessThanOrEqual:
                    SetCurrentOperand("Leq");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Equal:
                    SetCurrentOperand("Eq");
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.NotEqual:
                    SetCurrentOperand("Neq");
                    return this.VisitBinary((BinaryExpression)exp);

                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return this.VisitUnary((UnaryExpression)exp);

                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);

                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);

                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);


            case ExpressionType.New:
                return this.VisitNew((NewExpression)exp);
                    /*
            case ExpressionType.TypeIs:
                return this.VisitTypeIs((TypeBinaryExpression)exp);
            case ExpressionType.Conditional:
                return this.VisitConditional((ConditionalExpression)exp);
            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
                return this.VisitNewArray((NewArrayExpression)exp);
            case ExpressionType.Invoke:
                return this.VisitInvocation((InvocationExpression)exp);
            case ExpressionType.MemberInit:
                return this.VisitMemberInit((MemberInitExpression)exp);
            case ExpressionType.ListInit:
                return this.VisitListInit((ListInitExpression)exp);
                */
                default:
                    throw new NotSupportedException(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            Expression ret;
            if (u.NodeType == ExpressionType.Not)
            {
                _isNotOperand = !_isNotOperand;
                ret = base.VisitUnary(u);
                _isNotOperand = !_isNotOperand;
            }
            else
            {
                ret = base.VisitUnary(u);
            }

            if (_lastUnevaluatedBooleanExpression != null)
            {
                var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
                var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

                InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, true, _lastUnevaluatedBooleanExpression, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, _lastReferencedField));
            }

            return ret;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (!(b.Left is BinaryExpression))
            {
                bool isSpecialOp = false;

                if (b.Left is UnaryExpression)
                    return base.VisitBinary(b);

                MemberExpression m = null;
                if (b.Left is MethodCallExpression)
                {
                    var mce = (b.Left as MethodCallExpression);
                    if (mce.Method.DeclaringType != typeof(SPGENLinqOperations))
                    {
                        if (mce.Object != null)
                        {
                            m = (MemberExpression)mce.Object;
                        }
                        else if (mce.Arguments.Count > 0 && mce.Arguments[0] is MemberExpression)
                        {
                            m = (MemberExpression)mce.Arguments[0];
                        }
                    }
                    else
                    {
                        isSpecialOp = true;
                    }
                }
                else if (b.Left is MemberExpression)
                {
                    m = b.Left as MemberExpression;
                }

                if (!isSpecialOp)
                {
                    if (m == null)
                    {
                        throw new SPGENEntityGeneralException("Query expression comparison must have an entity member on the left side.");
                    }
                    else
                    {
                        //Check if the property is valid.
                        var pInfo = GetOwnerProperty(m);
                        FindEntityPropertyInfo(pInfo);
                    }
                }
            }

            Expression ret = base.VisitBinary(b);

            if (_lastUnevaluatedBooleanExpression != null)
            {
                var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
                var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

                InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, true, _lastUnevaluatedBooleanExpression, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, _lastReferencedField));
            }

            return ret;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (_lastReferencedField == null)
                return base.VisitNew(nex);

            object retVal = LambdaExpression.Lambda(nex).Compile().DynamicInvoke();
            var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
            var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

            InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, retVal, nex, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, _lastReferencedField));

            return nex;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (_lastFieldName != null)
            {
                CreateComparisonNodeForFieldNameCriteria(c);
                _lastFieldName = null;
                return c;
            }

            if (_lastReferencedField == null)
                return base.VisitConstant(c);

            object retVal = c.Value;
            var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
            var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

            InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, retVal, c, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, _lastReferencedField));

            return c;
        }

        protected override Expression VisitMemberAccess(MemberExpression mex)
        {
            if (_lastFieldName != null)
            {
                CreateComparisonNodeForFieldNameCriteria(mex);
                
                _lastFieldName = null;

                return mex;
            }


            if (_lastReferencedProperty == null)
            {
                _lastReferencedProperty = mex.Member as PropertyInfo;
                if (_lastReferencedProperty == null)
                    throw new ArgumentException("Only properties are allowed in queries.");

                _lastReferencedOwnerProperty = GetOwnerProperty(mex);

                var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);

                _lastReferencedField = FindSPField(entityPropInfo.FieldName);
                _isNullableComparison = IsMemberExpressionNullableType(mex.Member as PropertyInfo);

                if (_isNullableComparison)
                    _isNullableHasValueComparison = (mex.Member.Name == "HasValue");

                if (_lastReferencedProperty.PropertyType.IsAssignableFrom(typeof(bool)))
                {
                    _lastUnevaluatedBooleanExpression = mex;
                    SetCurrentOperand("Eq");
                }
            }
            else
            {
                var value = LambdaExpression.Lambda(mex).Compile().DynamicInvoke();                
                var entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
                var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

                InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, value, mex, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, _lastReferencedField));
            }

            return mex;
        }

        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            var c = ConstantExpression.Constant(Expression.Lambda(iv).Compile().DynamicInvoke());

            return this.VisitConstant(c);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            SPField field;
            SPGENEntityPropertyAccessorArguments? entityPropInfo = null;
            PropertyInfo pInfo = null;

            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == "Where" || m.Method.Name == "Select")
                {
                    return base.VisitMethodCall(m);
                }
                else
                {
                    if (m.Method.Name == "OrderBy")
                    {
                        ProcessOrderByStatement(m, false);
                        return base.Visit(m.Arguments[0]);
                    }
                    else if (m.Method.Name == "OrderByDescending")
                    {
                        ProcessOrderByStatement(m, true);
                        return Visit(m.Arguments[0]);
                    }
                    else if (m.Method.Name == "ThenBy")
                    {
                        ProcessOrderByStatement(m, false);
                        return Visit(m.Arguments[0]);
                    }
                    else if (m.Method.Name == "ThenByDescending")
                    {
                        ProcessOrderByStatement(m, true);
                        return base.Visit(m.Arguments[0]);
                    }
                }
            }

            if (m.Method.DeclaringType == typeof(SPGENLinqOperations))
            {
                return ProcessSpecialLinqOp(m);
            }

            
            if (_lastFieldName != null)
            {
                CreateComparisonNodeForFieldNameCriteria(m);
            }
            else if (_lastReferencedProperty != null)
            {
                object value = LambdaExpression.Lambda(m).Compile().DynamicInvoke();

                pInfo = _lastReferencedProperty;
                entityPropInfo = FindEntityPropertyInfo(_lastReferencedOwnerProperty);
                field = FindSPField(entityPropInfo.Value.FieldName);

                var adapter = entityPropInfo.Value.AdapterInstance as SPGENEntityLinqAdapter;

                InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, value, null, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, field));
            }
            else
            {
                if (m.NodeType == ExpressionType.Call)
                {
                    if (m.Object != null)
                    {
                        if (m.Object is MethodCallExpression)
                        {
                            ProcessSpecialLinqOp(m.Object as MethodCallExpression);
                        }
                        else
                        {
                            pInfo = (m.Object as MemberExpression).Member as PropertyInfo;
                        }
                    }
                    else if (m.Arguments.Count > 0 && m.Arguments[0] is MemberExpression)
                    {
                        pInfo = (m.Arguments[0] as MemberExpression).Member as PropertyInfo;
                    }
                    else if (m.Object is MemberExpression)
                    {
                        pInfo = (m.Object as MemberExpression).Member as PropertyInfo;
                    }
                }
                else
                {
                    pInfo = (m.Object as MemberExpression).Member as PropertyInfo;
                }

                if (pInfo == null)
                    throw new NotSupportedException("The method '" + m.Method.Name + "' is not supported.");

                entityPropInfo = FindEntityPropertyInfo(pInfo);

                if (entityPropInfo.HasValue)
                {
                    field = FindSPField(entityPropInfo.Value.FieldName);

                    string method = m.Method.Name;
                    SetCurrentOperand(method);

                    var adapter = entityPropInfo.Value.AdapterInstance as SPGENEntityLinqAdapter;

                    InvokeEvalLinqExpression(adapter, new SPGENEntityEvalLinqExprArgs(_lastOperand, null, m, this.CAMLQuery, _lastReferencedProperty, _lastReferencedOwnerProperty, field));
                }
                else
                {
                    throw new NotSupportedException("The property is not allowed in queries.");
                }
            }

            return m;
        }

        private Expression ProcessSpecialLinqOp(MethodCallExpression m)
        {
            string methodName = m.Method.Name;

            if (methodName == "FieldRef")
            {
                _lastFieldName = (string)LambdaExpression.Lambda(m.Arguments[0]).Compile().DynamicInvoke();
                if (string.IsNullOrEmpty(_lastFieldName))
                    throw new ArgumentException("No field name was specified.");

                return m;
            }
            else if (methodName == "ContainsValues")
            {
                LambdaExpression le = LambdaExpression.Lambda(m.Arguments[0]).Compile().DynamicInvoke() as LambdaExpression;
                MemberExpression me = (le.Body as MemberExpression);
                var pInfo = me.Member as PropertyInfo;
                var entityPropInfo = FindEntityPropertyInfo(pInfo);
                var adapter = entityPropInfo.AdapterInstance as SPGENEntityLinqAdapter;

                var parameters = LambdaExpression.Lambda(m.Arguments[1]).Compile().DynamicInvoke();
                if (parameters == null)
                    throw new ArgumentException("No values supplied.", "values");

                SPField field = FindSPField(entityPropInfo.FieldName);
                SetCurrentOperand("In");

                InvokeEvalLinqExpression(null, new SPGENEntityEvalLinqExprArgs(_lastOperand, parameters, m, this.CAMLQuery, null, null, field));

                return m;
            }

            throw new NotSupportedException();
        }

        private void CreateComparisonNodeForFieldNameCriteria(Expression e)
        {
            SPField field = _context.List.Fields.GetFieldByInternalName(_lastFieldName);
            object value = LambdaExpression.Lambda(e).Compile().DynamicInvoke();

            InvokeEvalLinqExpression(null, new SPGENEntityEvalLinqExprArgs(_lastOperand, value, e, this.CAMLQuery, null, null, field));
        }

        private void ProcessOrderByStatement(MethodCallExpression m, bool descending)
        {
            try
            {
                var mex = ((m.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression;

                _orderByProperties.Add(new KeyValuePair<PropertyInfo, bool>(mex.Member as PropertyInfo, descending));
            }
            catch(Exception ex)
            {
                throw new NotSupportedException("The order by statement is not supported.", ex);
            }
        }

        private void InvokeEvalLinqExpression(SPGENEntityLinqAdapter adapter, SPGENEntityEvalLinqExprArgs args)
        {
            SPGENEntityEvalLinqExprResult result;

            if (_isNullableComparison && _isNullableHasValueComparison)
            {
                args.Operand = (bool)args.Value ? "IsNotNull" : "IsNull";
                args.Value = null;

                _isNullableComparison = false;
                _isNullableHasValueComparison = false;
            }            
            
            if (_isNotOperand)
            {
                TransformNotOperations(args);
            }

            var m = args.Expression as MethodCallExpression;
            if (m != null && m.Method.DeclaringType != typeof(SPGENLinqOperations))
            {
                if (adapter != null)
                {
                    result = adapter.EvalMethodCall(m, args);
                }
                else
                {
                    result = _defaultAdapter.EvalMethodCall(m, args);
                }
            }
            else
            {
                if (adapter != null)
                {
                    result = adapter.EvalComparison(args);
                }
                else
                {
                    result = _defaultAdapter.EvalComparison(args);
                }
            }

            if (result == null)
                throw new SPGENEntityGeneralException("The Linq adapter didn't return any result.");

            if (result.ComparisonNode == null)
                throw new SPGENEntityGeneralException("The Linq adapter didn't return any comparison node.");

            AddComparisonOperandNode(result.ComparisonNode);

            _lastReferencedProperty = null;
            _lastReferencedOwnerProperty = null;
            _lastReferencedField = null;
            _lastUnevaluatedBooleanExpression = null;
            _lastFieldName = null;

            SetCurrentOperand(null);
        }

        private static void TransformNotOperations(SPGENEntityEvalLinqExprArgs args)
        {
            if (args.Operand == "Eq")
            {
                args.Operand = "Neq";
            }
            else if (args.Operand == "Neq")
            {
                args.Operand = "Eq";
            }
            else if (args.Operand == "Gt")
            {
                args.Operand = "Leq";
            }
            else if (args.Operand == "Lt")
            {
                args.Operand = "Geq";
            }
            else if (args.Operand == "Geq")
            {
                args.Operand = "Lt";
            }
            else if (args.Operand == "Leq")
            {
                args.Operand = "Gt";
            }
            else if (args.Operand == "IsNull")
            {
                args.Operand = "IsNotNull";
            }
            else if (args.Operand == "IsNotNull")
            {
                args.Operand = "IsNull";
            }
            else if (args.Operand == "Includes")
            {
                args.Operand = "NotIncludes";
            }
            else if (args.Operand == "NotIncludes")
            {
                args.Operand = "Includes";
            }
            else
            {
                throw new NotSupportedException("The not operator is not supported on this expression.");
            }
        }

        private void SetCurrentOperand(string operand)
        {
            _lastOperand = operand;
        }

        private void AddComparisonOperandNode(XmlNode element)
        {
            if (_currentBooleanNode == null)
                _currentBooleanNode = this.CAMLWhereNode;

            //Find the nearest node with < 2 child nodes.
            while (_currentBooleanNode.ChildNodes.Count == 2)
            {
                _currentBooleanNode = _currentBooleanNode.ParentNode;
            }

            _currentBooleanNode.AppendChild(element);
        }

        private void AddBooleanOperandNode(string operand)
        {
            var current = this.CAMLWhereNode;

            if (_isNotOperand)
            {
                operand = (operand == "And") ? "Or" : "And";
            }

            _currentBooleanNode = CreateElement(operand);

            //Find inner most boolean node.
            while (current.ChildNodes.Count > 0)
            {
                if (current.FirstChild == null)
                    break;

                string op = current.FirstChild.LocalName;
                
                if (op != "And" && op != "Or" && op != "Where")
                    break;

                current = current.FirstChild;
            }

            //Check wether we have already to child nodes.
            if (current.ChildNodes.Count == 2)
            {
                //Find the nearest node with < 2 child nodes.
                while (current.ChildNodes.Count == 2)
                {
                    current = current.ParentNode;
                }
            }

            current.InsertBefore(_currentBooleanNode, current.FirstChild);
        }

        private XmlElement CreateElement(string elementName)
        {
            return this.CAMLOwnerDocument.CreateElement(elementName);
        }

        private SPField FindSPField(string fieldName)
        {
            return _context.List.Fields.GetFieldByInternalName(fieldName);
        }

        private bool IsMemberExpressionNullableType(PropertyInfo pInfo)
        {
            if (pInfo.DeclaringType.IsGenericType)
            {
                if (pInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return true;
                }
            }

            return false;
        }

        private PropertyInfo GetOwnerProperty(MemberExpression mex)
        {
            PropertyInfo pInfo = FindOwnerProperty(mex);
            if (pInfo == null)
                throw new ArgumentException("Only properties are allowed in queries.");

            return pInfo;
        }

        private PropertyInfo FindOwnerProperty(MemberExpression mex)
        {
            int c = 0;

            while(c < 5)
            {
                PropertyInfo pInfo = mex.Member as PropertyInfo;

                if (pInfo.DeclaringType == typeof(TEntity) ||
                    typeof(TEntity).IsSubclassOf(pInfo.DeclaringType))
                    return pInfo;

                if (pInfo.DeclaringType == null)
                    return null;

                mex = mex.Expression as MemberExpression;
                if (mex == null)
                    return null;

                c++;
            }

            throw new SPGENEntityGeneralException("Maximum supported property depth for queries is 5 levels.");
        }

        private SPGENEntityPropertyAccessorArguments FindEntityPropertyInfo(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("Invalid property info.");

            var ret = _context.EntityMap.GetPropertyAccessorArguments(property);
            if (!ret.HasValue)
            {
                throw new ArgumentException(string.Format("The entity property '{0}' is not supported for linq queries.", property.Name));
            }

            return ret.Value;
        }
    }
}
