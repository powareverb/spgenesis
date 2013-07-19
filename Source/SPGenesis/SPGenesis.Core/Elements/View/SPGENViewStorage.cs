using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;

namespace SPGenesis.Core
{
    public class SPGENViewStorage : ISPGENViewStorage
    {
        public static ISPGENViewStorage Instance = new SPGENViewStorage();

        protected SPGENViewStorage()
        {
        }

        public virtual SPGENViewUrlInstance GetUrlInstance(string url)
        {
            var instance = new SPGENViewUrlInstance();

            try
            {
                instance.Site = new SPSite(url);
                instance.Web = instance.Site.OpenWeb();
                instance.List = SPGENListInstanceStorage.Instance.GetListByUrl(instance.Web, url);

                return instance;
            }
            catch
            {
                instance.Dispose();

                throw;
            }
        }

        public virtual void UpdateView(SPView view)
        {
            view.Update();
        }

        public virtual void DeleteView(SPList list, Guid viewId)
        {
            list.Views.Delete(viewId);

            list.Update();
        }
    }
}
