using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public static class SPGENWebExtensions
    {

        /// <summary>
        /// Iterates through all web sites in this web.
        /// </summary>
        /// <param name="web"></param>
        /// <param name="includeThisWeb">Include this web site in the iteration</param>
        /// <param name="recursive">Recursively loop through all sub webs</param>
        /// <param name="methodToCall">Method to call on each web site visit. Method should return true for continuing the iteration or false to end and return.</param>
        public static void ForEachWeb(this SPWeb web, bool includeThisWeb, bool recursive, Func<SPWeb, bool> methodToCall)
        {
            if (includeThisWeb)
            {
                bool bContinue = methodToCall.Invoke(web);

                if (!bContinue)
                    return;
            }

            ProcessAllSubWebs(web, recursive, methodToCall);

        }

        private static bool ProcessAllSubWebs(SPWeb web, bool recursive, Func<SPWeb, bool> methodToCall)
        {
            IList<SPWebInfo> webs = web.Webs.WebsInfo;

            foreach (SPWebInfo info in webs)
            {
                using (SPWeb currentWeb = web.Site.OpenWeb(info.Id))
                {
                    //Loop through all sub webs recursively
                    bool bContinue;

                    bContinue = methodToCall.Invoke(currentWeb);

                    if (!bContinue)
                        return false;


                    if (recursive && currentWeb.Exists)
                    {
                        bContinue = ProcessAllSubWebs(currentWeb, recursive, methodToCall);

                        if (!bContinue)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
