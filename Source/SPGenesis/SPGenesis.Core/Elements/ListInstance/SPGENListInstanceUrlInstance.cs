using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Linq;

namespace SPGenesis.Core
{
    public sealed class SPGENListInstanceUrlInstance : IDisposable
    {
        public SPSite Site { get; set; }
        public SPWeb Web { get; set; }
        public SPList List { get; set; }

        internal SPGENListInstanceUrlInstance()
        {
        }

        public void Dispose()
        {
            if (this.Web != null)
                this.Web.Close();
            if (this.Site != null)
                this.Site.Close();
        }
    }
}
