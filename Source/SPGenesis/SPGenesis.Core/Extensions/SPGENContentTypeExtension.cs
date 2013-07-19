using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public static class SPGENContentTypeExtension
    {
        public static void RegisterEventReceiver<TReceiver>(this SPContentType contentType, bool keepOnlyDeclaredMethods, bool updateChildren, bool stopOnSealdOrReadOnlyUpdate)
        {
            var col = new SPGENEventReceiverCollection();
            col.AddType(typeof(TReceiver), null, keepOnlyDeclaredMethods);
            col.Provision(contentType.EventReceivers);

            SPGENContentTypeStorage.Instance.UpdateContentType(contentType, updateChildren, stopOnSealdOrReadOnlyUpdate);
        }

        public static SPFieldLink AddSPGENFieldLink<TField>(this SPContentType contentType, bool ignoreIfExists)
            where TField : SPGENFieldBase, new ()
        {
            SPField field = null;

            var fieldDefinition = ((SPGENFieldBase)SPGENElementManager.GetInstance<TField>()).StaticDefinition;
            Guid id = fieldDefinition.ID;

            if (contentType.FieldLinks[id] != null)
            {
                if (ignoreIfExists)
                {
                    return contentType.FieldLinks[id];
                }
                else
                {
                    throw new SPGENGeneralException("Field link already exists in contentype.");
                }
            }

            if (contentType.ParentList != null)
            {
                if (contentType.ParentList.Fields.Contains(id))
                {
                    field = contentType.ParentList.Fields[id];
                }
            }

            if (field == null)
            {
                field = contentType.ParentWeb.AvailableFields[id];
            }

            contentType.FieldLinks.Add(new SPFieldLink(field));

            return contentType.FieldLinks[id];
        }
    }
}
