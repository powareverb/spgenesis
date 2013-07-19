using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    public static class SPGENFarmExtension
    {
        /// <summary>
        /// Iterates through all web applications in the farm.
        /// </summary>
        /// <param name="farm"></param>
        /// <param name="methodToCall">Method to call on each web application visit. Method should return true for continuing the iteration or false to end and return.</param>
        public static void ForEachWebApplication(this SPFarm farm, Func<SPWebApplication, bool> methodToCall)
        {
            IList<SPWebApplication> webAppColl = SPWebService.AdministrationService.WebApplications.ToList<SPWebApplication>();

            foreach (SPWebApplication webapp in webAppColl)
            {
                bool bContinue = methodToCall.Invoke(webapp);

                if (!bContinue)
                    return;
            }

            webAppColl = SPWebService.ContentService.WebApplications.ToList<SPWebApplication>();
            foreach (SPWebApplication webapp in webAppColl)
            {
                bool bContinue = methodToCall.Invoke(webapp);

                if (!bContinue)
                    return;
            }
        }
    }
}
