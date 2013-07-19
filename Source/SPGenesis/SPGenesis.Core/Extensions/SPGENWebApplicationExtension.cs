using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public static class SPGENWebApplicationExtension
    {
        /// <summary>
        /// Iterates through all site collections in this web application.
        /// </summary>
        /// <param name="webApplication"></param>
        /// <param name="methodToCall">Method to call on each site collection visit. Method should return true for continuing the iteration or false to end and return.</param>
        public static void ForEachSite(this SPWebApplication webApplication, Func<SPSite, bool> methodToCall)
        {
            ForEachSite(webApplication, SPUrlZone.Default, methodToCall);
        }

        /// <summary>
        /// Iterates through all site collections in this web application.
        /// </summary>
        /// <param name="webApplication"></param>
        /// <param name="urlZone"></param>
        /// <param name="methodToCall">Method to call on each site collection visit. Method should return true for continuing the iteration or false to end and return.</param>
        public static void ForEachSite(this SPWebApplication webApplication, SPUrlZone urlZone, Func<SPSite, bool> methodToCall)
        {
            string webAppUrl = webApplication.GetResponseUri(urlZone).ToString();
            string[] siteUrls = webApplication.Sites.Names;

            foreach (string siteUrl in siteUrls)
            {
                string url = webAppUrl;
                if (webAppUrl.EndsWith("/"))
                {
                    url = webAppUrl.Substring(0, webAppUrl.Length - 1);
                }

                if (siteUrl.StartsWith("/"))
                {
                    url += siteUrl;
                }
                else
                {
                    url += "/" + siteUrl;
                }

                using (SPSite site = new SPSite(url))
                {
                    bool bContinue = methodToCall.Invoke(site);

                    if (!bContinue)
                        return;
                }
            }
        }
    }
}
