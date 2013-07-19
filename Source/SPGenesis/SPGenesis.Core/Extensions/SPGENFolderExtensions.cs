using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public static class SPGENFolderExtension
    {
        /// <summary>
        /// Iterates through all sub folders in this folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="includeThisFolder">Include this folder in the iteration</param>
        /// <param name="recursive">Recursively loop through all sub folders</param>
        /// <param name="methodToCall">Method to call on each folder visit. Method should return true for continuing the iteration or false to end and return.</param>
        public static void ForEachFolder(this SPFolder folder, bool includeThisFolder, bool recursive, Func<SPFolder, bool> methodToCall)
        {
            if (includeThisFolder)
            {
                bool bContinue = methodToCall.Invoke(folder);

                if (!bContinue)
                    return;
            }

            ProcessAllSubFolders(folder, recursive, methodToCall);

        }

        private static bool ProcessAllSubFolders(SPFolder Folder, bool recursive, Func<SPFolder, bool> methodToCall)
        {
            IList<SPFolder> subFolders = Folder.SubFolders.Cast<SPFolder>().ToList<SPFolder>();

            foreach (SPFolder subFolder in subFolders)
            {
                //Loop through all sub webs recursively
                bool bContinue;

                bContinue = methodToCall.Invoke(subFolder);

                if (!bContinue)
                    return false;


                if (recursive && subFolder.Exists)
                {
                    bContinue = ProcessAllSubFolders(subFolder, recursive, methodToCall);

                    if (!bContinue)
                        return false;
                }
            }

            return true;
        }
    }
}
