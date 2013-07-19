using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENFieldUrlInstance : IDisposable
    {
        public SPField Field { get; set; }
        public SPSite Site { get; set; }
        public SPWeb Web { get; set; }
        public SPList List { get; set; }
        public SPListItem Item { get; set; }

        public void Dispose()
        {
            if (this.Web != null)
                this.Web.Close();
            if (this.Site != null)
                this.Site.Close();
        }
    }
}
