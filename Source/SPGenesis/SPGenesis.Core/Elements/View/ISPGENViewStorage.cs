using System;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public interface ISPGENViewStorage
    {
        void DeleteView(SPList list, Guid viewId);
        SPGENViewUrlInstance GetUrlInstance(string url);
        void UpdateView(SPView view);
    }
}
