using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core 
{
    public class SPGENFieldStorage : ISPGENFieldStorage
    {
        public static ISPGENFieldStorage Instance = new SPGENFieldStorage();

        protected SPGENFieldStorage()
        {
        }

        public virtual SPField GetField(SPFieldCollection fieldCollection, Guid fieldId)
        {
            if (fieldCollection.Contains(fieldId))
            {
                return fieldCollection[fieldId];
            }
            else
            {
                return null;
            }
        }

        public virtual bool ContainsField(SPFieldCollection fieldCollection, Guid fieldId)
        {
            return fieldCollection.Contains(fieldId);
        }

        public virtual string CreateField(SPFieldCollection fieldCollection, XmlNode schemaXml, bool addToDefaultView, SPAddFieldOptions options)
        {
            return fieldCollection.AddFieldAsXml(schemaXml.OuterXml, addToDefaultView, options);
        }

        public string AddField(SPFieldCollection fieldCollection, SPField field)
        {
            return fieldCollection.Add(field);
        }

        public virtual void UpdateField(SPField field, bool pushChangesToList)
        {
            field.Update(pushChangesToList);
        }

        public virtual void DeleteField(SPFieldCollection fieldCollection, Guid fieldId)
        {
            SPField field = fieldCollection[fieldId];

            if (fieldCollection.List == null)
            {
                var ctColl = fieldCollection.Web.ContentTypes;

                for (int i = 0; i < ctColl.Count; i++)
                {
                    var ct = ctColl[i];
                    if (ct.FieldLinks[field.Id] != null)
                    {
                        ct.FieldLinks.Delete(field.Id);

                        ct.Update(true);
                    }
                }
            }

            field.Delete();
        }

        public virtual SPGENFieldUrlInstance CreateUrlInstance(string url)
        {
            var instance = new SPGENFieldUrlInstance();

            try
            {
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();
                instance.List = instance.Web.Lists.TryGetList(url);

                if (url.Length != instance.Web.Url.Length)
                {
                    if (instance.List == null)
                    {
                        throw new ArgumentException("The url is invalid.");
                    }
                }

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }
    }
}
