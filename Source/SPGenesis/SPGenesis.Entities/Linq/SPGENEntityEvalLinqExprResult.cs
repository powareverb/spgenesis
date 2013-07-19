using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Linq.Expressions;

namespace SPGenesis.Entities.Linq
{
    public sealed class SPGENEntityEvalLinqExprResult
    {
        private XmlNode _comparisonNode;
        public XmlNode ComparisonNode
        {
            get { return _comparisonNode; }
        }

        public XmlElement FieldRefNode
        {
            get
            {
                if (this.ComparisonNode == null)
                    return null;

                var n = this.ComparisonNode.SelectSingleNode("FieldRef");

                return n as XmlElement;
            }
        }

        public XmlElement ValueNode
        {
            get
            {
                if (this.ComparisonNode == null)
                    return null;

                var n = this.ComparisonNode.SelectSingleNode("Value");

                return n as XmlElement;
            }
        }

        public SPGENEntityEvalLinqExprResult(SPGENEntityEvalLinqExprArgs args)
            : this(args, args.Operand)
        {
        }

        public SPGENEntityEvalLinqExprResult(SPGENEntityEvalLinqExprArgs args, string operand)
        {
            XmlDocument xmldoc = args.CamlQuery.OwnerDocument;

            var op = xmldoc.CreateElement(operand);
            var fieldRef = xmldoc.CreateElement("FieldRef");
            var valueElement = xmldoc.CreateElement("Value");

            fieldRef.SetAttribute("Name", args.Field.InternalName);
            valueElement.SetAttribute("Type", string.Empty);

            op.AppendChild(fieldRef);
            op.AppendChild(valueElement);

            _comparisonNode = op;
        }

        public void MakeLookupId()
        {
            if (this.ValueNode != null)
            {
                this.ValueNode.SetAttribute("Type", "Lookup");
                this.FieldRefNode.SetAttribute("LookupId", "TRUE");
            }
        }

        public void SetValue(object value)
        {
            SetValue(null, value);
        }

        public void SetValue(string type, object value)
        {
            if (this.ValueNode != null)
            {
                if (!string.IsNullOrEmpty(type))
                    this.ValueNode.SetAttribute("Type", type);
    
                this.ValueNode.InnerText = (value == null) ? null : value.ToString();
            }
            else
            {
                throw new ArgumentException("There is no value node created yet.");
            }
        }
    }
}
