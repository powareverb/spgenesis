using System;
using System.Linq;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.BusinessData.Infrastructure;
using Microsoft.SharePoint.BusinessData.MetadataModel;
using Microsoft.SharePoint.BusinessData.SharedService;

namespace SPGenesis.Core
{
    public sealed class SPGENBdcFieldSynchronizer<TField>
        where TField : SPGENFieldBase, new()
    {
        private SPGENFieldProperties _fieldProperties;
        private IView _defaultSpecificFinderView;

        private IEntity _entity;
        private ILobSystemInstance _systemInstance;

        public SPGENBdcFieldSynchronizer(SPSite site)
        {
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(string.Empty);
            SPServiceContext serviceContext = SPServiceContext.GetContext(site);
            DatabaseBackedMetadataCatalog databaseBackedMetadataCatalog = service.GetDatabaseBackedMetadataCatalog(serviceContext);

            var instance = (TField)SPGENElementManager.GetInstance(typeof(TField));
            if (instance == null)
                throw new ArgumentException("Generic type argument is not valid.", "TField");

            _fieldProperties = ((SPGENFieldBase)instance).StaticDefinition;

            _entity = databaseBackedMetadataCatalog.GetEntity(_fieldProperties.BdcEntityNamespace, _fieldProperties.BdcEntityName);
            _systemInstance = _entity.GetLobSystem().GetLobSystemInstances()[_fieldProperties.BdcSystemInstance];

            _defaultSpecificFinderView = _entity.GetDefaultSpecificFinderView();

        }

        /// <summary>
        /// Updates all list items in a list.
        /// </summary>
        /// <param name="list">SPList object</param>
        /// <param name="systemUpdate">Uses SystemUpdate instead of Update method when updating list items.</param>
        /// <param name="overwriteVersion">Overwrites the item version.</param>
        public void UpdateList(SPList list, bool systemUpdate, bool overwriteVersion)
        {
            UpdateList(list, null, systemUpdate, overwriteVersion);
        }
        /// <summary>
        /// Updates all list items in a list.
        /// </summary>
        /// <param name="list">SPList object</param>
        /// <param name="onUpdatingFunction">Fires before an item is updated. Return true to cancel the whole operation, otherwise return false to continue.</param>
        /// <param name="systemUpdate">Uses SystemUpdate instead of Update method when updating list items.</param>
        /// <param name="overwriteVersion">Overwrites the item version.</param>
        public void UpdateList(SPList list, Func<SPListItem, bool> onUpdatingFunction ,bool systemUpdate, bool overwriteVersion)
        {
            var coll = list.Items;

            foreach (SPListItem item in coll)
            {
                if (onUpdatingFunction != null)
                {
                    bool cancel = onUpdatingFunction.Invoke(item);
                    if (cancel)
                        return;
                }

                UpdateItem(item, true, systemUpdate, overwriteVersion);
            }
        }

        public void UpdateItem(SPListItem item, bool doUpdate, bool systemUpdate, bool overwriteVersion)
        {
            SPField relatedField = item.Fields.GetFieldByInternalName(_fieldProperties.RelatedField);

            object[] identifiers = EntityInstanceIdEncoder.DecodeEntityInstanceId(item[relatedField.Id] as string);
            
            var entityInst = _entity.FindSpecific(new Identity(identifiers), _systemInstance);

            SPBusinessDataField field = item.ParentList.Fields[_fieldProperties.ID] as SPBusinessDataField;

            string[] secBcsFieldNames = field.GetSecondaryFieldsNames();
            string[] secWssFieldnames = SPGENCommon.GetSecondaryWssFieldNames(field).ToArray<string>();
            int i = 0;

            foreach (string s in secBcsFieldNames)
            {
                var viewField = _defaultSpecificFinderView.Fields.FirstOrDefault<IField>(f => f.Name == s);
                object value = entityInst.GetFormatted(viewField);
                item[item.Fields.GetFieldByInternalName(secWssFieldnames[i]).Id] = value;
                i++;
            }

            if (doUpdate)
            {
                if (systemUpdate)
                {
                    item.SystemUpdate();
                }
                else
                {
                    if (overwriteVersion)
                    {
                        item.UpdateOverwriteVersion();
                    }
                    else
                    {
                        item.Update();
                    }
                }
            }
        }
    }
}
