using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.SharePoint;
using System.Reflection;

namespace SPGenesis.Entities.Linq
{
    public sealed class SPGENEntityEvalLinqExprArgs
    {
        [Obsolete("", true)]
        public SPGENEntityEvalLinqExprArgs(string operand, object value, Expression expr, XmlNode CamlQuery, PropertyInfo sourceProperty, SPField field)
            : this(operand, value, expr, CamlQuery, sourceProperty, sourceProperty, field)
        {
        }

        public SPGENEntityEvalLinqExprArgs(string operand, object value, Expression expr, XmlNode CamlQuery, PropertyInfo sourceProperty, PropertyInfo ownerProperty, SPField field)
        {
            this.Operand = operand;
            this.Value = value;
            this.Expression = expr;
            this.CamlQuery = CamlQuery;
            this.SourceProperty = sourceProperty;
            this.Field = field;
            this.OwnerEntityProperty = ownerProperty;
        }

        public string Operand;
        public object Value;
        public Expression Expression;
        public XmlNode CamlQuery;
        public PropertyInfo SourceProperty;
        public SPField Field;
        public PropertyInfo OwnerEntityProperty;

        internal bool IsLookupIdProperty;
    }
}
