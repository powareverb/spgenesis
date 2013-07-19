using System;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public interface ISPGENFieldStorage
    {
        bool ContainsField(SPFieldCollection fieldCollection, Guid fieldId);
        string CreateField(SPFieldCollection fieldCollection, XmlNode schemaXml, bool addToDefaultView, SPAddFieldOptions options);
        string AddField(SPFieldCollection fieldCollection, SPField field);
        void DeleteField(SPFieldCollection fieldCollection, Guid fieldId);
        SPField GetField(SPFieldCollection fieldCollection, Guid fieldId);
        SPGENFieldUrlInstance CreateUrlInstance(string url);
        void UpdateField(SPField field, bool pushChangesToList);
    }
}
