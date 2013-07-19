using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Web;
using Microsoft.SharePoint.Utilities;

namespace SPGenesis.Core
{
    internal static class SPGENResourceHelper
    {
        public static bool HasResourceSyntax(string resourceString)
        {
            return (resourceString.IndexOf("$Resources:") != -1);
        }

        public static string GetString(string resourceString)
        {
            return GetString(resourceString, CultureInfo.CurrentUICulture);
        }

        public static string GetString(string resourceString, int lcid)
        {
            return GetString(resourceString, new CultureInfo(lcid));
        }

        public static string GetString(string resourceString, CultureInfo cultureInfo)
        {
            if (!HasResourceSyntax(resourceString))
                throw new ArgumentException("Parameter resourceString has no valid resource syntax.");

            string file = "core";
            string key;
            int pos = resourceString.IndexOf(",");
            if (pos != -1)
            {
                file = resourceString.Substring(11, pos - 11).Trim();
                key = resourceString.Substring(pos + 1).Trim().TrimEnd(';');
            }
            else
            {
                key = resourceString.Substring(11).Trim().TrimEnd(';');
            }

            string ret = GetString(file, key, cultureInfo);

            return ret;
        }

        public static string GetString(string file, string key)
        {
            return GetString(file, key, System.Globalization.CultureInfo.CurrentUICulture, null);
        }

        public static string GetString(string file, string key, params string[] parameters)
        {
            return GetString(file, key, System.Globalization.CultureInfo.CurrentUICulture, parameters);
        }

        public static string GetString(string file, string key, int lcid, params string[] parameters)
        {
            return GetString(file, key, new CultureInfo(lcid), parameters);
        }

        public static string GetString(string file, string key, CultureInfo cultureInfo, params string[] parameters)
        {
            string retValue;

            if (string.IsNullOrEmpty(file))
            {
                file = "core";
            }

            retValue = SPUtility.GetLocalizedString("$Resources:" + key, file, (uint)cultureInfo.LCID);

            if (parameters != null && parameters.Length > 0)
            {
                return string.Format(retValue, parameters);
            }
            else
            {
                return retValue;
            }
        }
    }
}
