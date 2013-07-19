using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace SPGenesis.Core
{
    public sealed class SPGENViewProperties : SPGENElementProperties
    {
        [SPGENPropertyMappingAttribute]
        public string BaseViewID { get { return base.GetPropertyValue<string>("BaseViewID"); } internal set { base.SetPropertyValue("BaseViewID", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "DisplayName")]
        public string Title { get { return base.GetPropertyValue<string>("Title"); } set { base.SetPropertyValue("Title", value); } }
        [SPGENPropertyMappingAttribute(XmlAttributeName = "Url")]
        public string UrlFileName { get { return base.GetPropertyValue<string>("UrlFileName"); } set { base.SetPropertyValue("UrlFileName", value); } }
        [SPGENPropertyMappingAttribute]
        public bool DefaultView { get { return base.GetPropertyValue<bool>("DefaultView"); } set { base.SetPropertyValue("DefaultView", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Paged { get { return base.GetPropertyValue<bool>("Paged"); } set { base.SetPropertyValue("Paged", value); } }
        [SPGENPropertyMappingAttribute]
        public bool Hidden { get { return base.GetPropertyValue<bool>("Hidden"); } set { base.SetPropertyValue("Hidden", value); } }
        [SPGENPropertyMappingAttribute]
        public string Query { get { return base.GetPropertyValue<string>("Query"); } set { base.SetPropertyValue("Query", value); } }
        [SPGENPropertyMappingAttribute]
        public uint RowLimit { get { return base.GetPropertyValue<uint>("RowLimit"); } set { base.SetPropertyValue("RowLimit", value); } }

        public SPGENViewFieldCollection ViewFields { get; private set; }

        public string CloneViewUrlFileName { get; private set; }

        public SPGENViewProperties()
        {
            this.ViewFields = new SPGENViewFieldCollection();
        }

        public SPGENViewProperties(string cloneViewUrlFileName)
        {
            this.ViewFields = new SPGENViewFieldCollection();
            this.CloneViewUrlFileName = cloneViewUrlFileName;
        }
        
        internal SPGENViewProperties(XmlNode elementDefinitionXml) : base(elementDefinitionXml) 
        {
            this.ViewFields = new SPGENViewFieldCollection();
        }

        public string GetFullUrl(SPList list)
        {
            SPWeb web = list.ParentWeb;

            string url = web.Url + "/" + list.RootFolder.Url + "/" + this.UrlFileName;

            return url;
        }

        public object GetDynamicProperty(Expression<Func<SPView, object>> property)
        {
            return GetDynamicProperty(property);
        }

        public void AddDynamicProperty(Expression<Func<SPView, object>> property, object value)
        {
            AddDynamicPropertyInternal<SPView>(property, value);
        }

        protected override string ElementIdAttribute
        {
            get { throw new NotImplementedException(); }
        }

        internal override object ElementIdValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override string ElementName
        {
            get { throw new NotImplementedException(); }
        }


    }
}
