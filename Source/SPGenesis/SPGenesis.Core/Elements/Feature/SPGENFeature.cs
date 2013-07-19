using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System.Diagnostics;

namespace SPGenesis.Core
{
    /// <summary>
    /// Base class for field elements.
    /// </summary>
    /// <typeparam name="TFeature">The derived type.</typeparam>
    public class SPGENFeature<TFeature> : SPGENFeatureBase 
        where TFeature : SPGENFeature<TFeature>
    {
        private static TFeature _instance = SPGENElementManager.GetInstance<TFeature>();

        private Guid _id;
        private string _name;
        private bool _isUsingTypeName;

        /// <summary>
        /// A set of methods available for this feature element.
        /// </summary>
        public static TFeature Instance = _instance;


        /// <summary>
        /// Returns the feature ID.
        /// </summary>
        public static Guid ID
        {
            get
            {
                return _instance.FeatureId;
            }
        }

        /// <summary>
        /// Returns the feature name.
        /// </summary>
        public static string Name
        {
            get
            {
                return _instance.FeatureName;
            }
        }

        /// <summary>
        /// Returns the feature definition.
        /// </summary>
        public static SPFeatureDefinition Definition
        {
            get
            {
                return _instance.GetDefinition(SPFarm.Local);
            }
        }


        #region Feature methods

        /// <summary>
        /// Returns the feature ID.
        /// </summary>
        public Guid FeatureId
        {
            get
            {
                EnsureRequiredFields();

                return _id;
            }
        }

        /// <summary>
        /// Returns the feature name.
        /// </summary>
        public string FeatureName
        {
            get
            {
                EnsureRequiredFields();

                return _name;
            }
        }


        /// <summary>
        /// Returns the feature definition from the specified farm instance.
        /// </summary>
        /// <param name="farm">The farm instance.</param>
        /// <returns></returns>
        public SPFeatureDefinition GetDefinition(SPFarm farm)
        {
            var definition = farm.FeatureDefinitions[this.FeatureId];
            if (definition == null)
                throw new SPGENGeneralException("Feature '" + this.FeatureId.ToString() + "' not found in the specified farm.");

            return definition;
        }


        /// <summary>
        /// Returns all activations from the specified root URL for this feature.
        /// </summary>
        /// <param name="rootUrl">Root URL to start from.</param>
        /// <param name="isWebAppUrl">Set to true to indicate that the URl is a web application url and not to a site collection.</param>
        /// <returns>A collection of SPFeature objects.</returns>
        public IEnumerable<SPFeature> FindAllActivations(string rootUrl, bool isWebAppUrl)
        {
            if (isWebAppUrl)
            {
                SPWebApplication webapp = SPWebApplication.Lookup(new Uri(rootUrl));

                return webapp.QueryFeatures(Definition.Id);
            }
            else
            {
                using (SPSite site = new SPSite(rootUrl))
                {
                    if (site.Url.Equals(rootUrl, StringComparison.InvariantCulture))
                    {
                        return site.QueryFeatures(Definition.Id);
                    }

                    SPFeatureQueryResultCollection result = site.QueryFeatures(Definition.Id);

                    return result.Where<SPFeature>(feature => feature.Parent is SPWeb && (feature.Parent as SPWeb).Url.StartsWith(rootUrl, StringComparison.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Installs the feature on the local farm.
        /// </summary>
        /// <param name="force">Force installation.<</param>
        public void Install(bool force)
        {
            Install(SPFarm.Local, force);
        }

        /// <summary>
        /// Installs the feature on the farm instance.
        /// </summary>
        /// <param name="farm">The farm isntance.</param>
        /// <param name="force">Force installation.</param>
        public void Install(SPFarm farm, bool force)
        {
            bool installed = false;
            if (!string.IsNullOrEmpty(this.FeatureName))
            {
                try
                {
                    SPFarm.Local.FeatureDefinitions.Add(this.FeatureName + "\\feature.xml", Guid.Empty, force);
                    installed = true;
                }
                catch
                {
                }
            }


            if (!installed)
            {
                if (this.FeatureId == Guid.Empty)
                    throw new SPGENGeneralException("The feature definition has no ID specified.");

                string featuresPath = SPUtility.GetGenericSetupPath("TEMPLATE\\FEATURES");
                string featureName = null;

                foreach (string path in Directory.GetDirectories(featuresPath))
                {
                    if (!File.Exists(path + "\\Feature.xml"))
                        continue;

                    try
                    {
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(path + "\\Feature.xml");

                        Guid featureId = new Guid(SPGENCommon.GetElementDefinitionAttribute(xmldoc.DocumentElement.Attributes, "ID"));

                        if (featureId == this.FeatureId)
                        {
                            featureName = Path.GetFileName(path);
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        SPGENCommon.WriteToULS("Could not load feature.xml path: " + path + ".", ex, 0, TraceSeverity.Medium, EventSeverity.Error);
                        System.Diagnostics.Debug.WriteLine("Could not load feature.xml path: " + path + ". Exception: " + ex.Message, typeof(SPGENFeature<>).Name);
                    }
                }

                if (featureName != null)
                {
                    SPFarm.Local.FeatureDefinitions.Add(featureName + "\\feature.xml", Guid.Empty, force);
                }
                else
                {
                    throw new SPGENGeneralException("The feature with ID " + this.FeatureId.ToString() + " could not be found.");
                }
            }
        }

        /// <summary>
        /// Uninstall the feature.
        /// </summary>
        /// <param name="force">Force uninstallation</param>
        public void Uninstall(bool force)
        {
            SPFarm.Local.FeatureDefinitions.Remove(this.FeatureId, force);
        }

        /// <summary>
        /// Check if the feature is installed on the local farm.
        /// </summary>
        /// <returns>True if the feature is installed on the farm.</returns>
        public bool IsInstalled()
        {
            return IsInstalled(SPFarm.Local);
        }

        /// <summary>
        /// Check if the feature is installed on the spcified farm.
        /// </summary>
        /// <param name="farm">The farm instance.</param>
        /// <returns>True if the feature is installed on the farm.</returns>
        public bool IsInstalled(SPFarm farm)
        {
            var d = farm.FeatureDefinitions[this.FeatureId];

            return (d != null);
        }

        /// <summary>
        /// Check if the feature is activated at the specified URL.
        /// </summary>
        /// <param name="url">The URL to check at.</param>
        /// <returns>True if the feature is activated.</returns>
        public bool IsActivated(string url)
        {
            return IsActivatedInternal(url);
        }
        
        /// <summary>
        /// Check if the feature is activated at the specified web application instance.
        /// </summary>
        /// <param name="webApplication">The web application instance.</param>
        /// <returns>True if the feature is activated.</returns>
        public bool IsActivated(SPWebApplication webApplication)
        {
            return IsActivatedInternal(webApplication);
        }
        
        /// <summary>
        /// Check if the feature is activated at the specified site instance.
        /// </summary>
        /// <param name="site">The site instance.</param>
        /// <returns>True if the feature is activated.</returns>
        public bool IsActivated(SPSite site)
        {
            return IsActivatedInternal(site);
        }
        
        /// <summary>
        /// Check if the feature is activated at the specified web instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <returns>True if the feature is activated.</returns>
        public bool IsActivated(SPWeb web)
        {
            return IsActivatedInternal(web);
        }

        /// <summary>
        /// Activates the feature on the specified URL.
        /// </summary>
        /// <param name="url">Could be either a web, site or web application URL.</param>
        /// <param name="action"></param>
        public void Activate(string url, ActivationAction action)
        {
            ActivateFeatureInternal(url, action);
        }

        /// <summary>
        /// Activates the feature on the specified web application instance.
        /// </summary>
        /// <param name="webApplication">The web application instance.</param>
        /// <param name="action"></param>
        public void Activate(SPWebApplication webApplication, ActivationAction action)
        {
            ActivateFeatureInternal(webApplication, action);
        }
        
        /// <summary>
        /// Activates the feature on the specified site instance.
        /// </summary>
        /// <param name="site">The site instance.</param>
        /// <param name="action"></param>
        public void Activate(SPSite site, ActivationAction action)
        {
            ActivateFeatureInternal(site, action);
        }
        
        /// <summary>
        /// Activates the feature on the specified site instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <param name="action"></param>
        public void Activate(SPWeb web, ActivationAction action)
        {
            ActivateFeatureInternal(web, action);
        }
        
        /// <summary>
        /// Activates the feature on the specified farm instance.
        /// </summary>
        /// <param name="farm">The farm instance</param>
        /// <param name="action"></param>
        public void Activate(SPFarm farm, ActivationAction action)
        {
            ActivateFeatureInternal(farm, action);
        }

        /// <summary>
        /// Deactivates the feature on the specified URL.
        /// </summary>
        /// <param name="url">Could be either a web, site or web application URL.</param>
        /// <param name="action"></param>
        public void Deactivate(string url, DeactivationAction action)
        {
            DeactivateFeatureInternal(url, action);
        }

        /// <summary>
        /// Deactivates the feature on the specified web application instance.
        /// </summary>
        /// <param name="webApplication">The web application instance.</param>
        /// <param name="action"></param>
        public void Deactivate(SPWebApplication webApplication, DeactivationAction action)
        {
            DeactivateFeatureInternal(webApplication, action);
        }

        /// <summary>
        /// Deactivates the feature on the specified site instance.
        /// </summary>
        /// <param name="site">The site instance.</param>
        /// <param name="action"></param>
        public void Deactivate(SPSite site, DeactivationAction action)
        {
            DeactivateFeatureInternal(site, action);
        }

        /// <summary>
        /// Deactivates the feature on the specified site instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <param name="action"></param>
        public void Deactivate(SPWeb web, DeactivationAction action)
        {
            DeactivateFeatureInternal(web, action);
        }

        /// <summary>
        /// Deactivates the feature on the specified farm instance.
        /// </summary>
        /// <param name="farm">The farm instance</param>
        /// <param name="action"></param>
        public void Deactivate(SPFarm farm, DeactivationAction action)
        {
            DeactivateFeatureInternal(farm, action);
        }

        /// <summary>
        /// Reactivates the feature on the specified URL.
        /// </summary>
        /// <param name="url">Could be either a web, site or web application URL.</param>
        /// <param name="force">Force reactivation.</param>
        public void ReActivate(string url, bool force)
        {
            Deactivate(url, force ? DeactivationAction.Force | DeactivationAction.IgnoreIfAlreadyDeactivated : DeactivationAction.NoForce | DeactivationAction.IgnoreIfAlreadyDeactivated);

            Activate(url, force ? ActivationAction.Force | ActivationAction.IgnoreIfAlreadyActivated : ActivationAction.NoForce | ActivationAction.IgnoreIfAlreadyActivated);
        }

        /// <summary>
        /// Reactivates the feature on the specified web application instance.
        /// </summary>
        /// <param name="webApplication">The web application instance.</param>
        /// <param name="force">Force reactivation.</param>
        public void ReActivate(SPWebApplication webApplication, bool force)
        {
            Deactivate(webApplication, force ? DeactivationAction.Force | DeactivationAction.IgnoreIfAlreadyDeactivated : DeactivationAction.NoForce | DeactivationAction.IgnoreIfAlreadyDeactivated);

            Activate(webApplication, force ? ActivationAction.Force | ActivationAction.IgnoreIfAlreadyActivated : ActivationAction.NoForce | ActivationAction.IgnoreIfAlreadyActivated);
        }

        /// <summary>
        /// Reactivates the feature on the specified site instance.
        /// </summary>
        /// <param name="site">The site instance.</param>
        /// <param name="force">Force reactivation.</param>
        public void ReActivate(SPSite site, bool force)
        {
            Deactivate(site, force ? DeactivationAction.Force | DeactivationAction.IgnoreIfAlreadyDeactivated : DeactivationAction.NoForce | DeactivationAction.IgnoreIfAlreadyDeactivated);

            Activate(site, force ? ActivationAction.Force | ActivationAction.IgnoreIfAlreadyActivated : ActivationAction.NoForce | ActivationAction.IgnoreIfAlreadyActivated);
        }

        /// <summary>
        /// Reactivates the feature on the specified site instance.
        /// </summary>
        /// <param name="web">The web instance.</param>
        /// <param name="force">Force reactivation.</param>
        public void ReActivate(SPWeb web, bool force)
        {
            Deactivate(web, force ? DeactivationAction.Force | DeactivationAction.IgnoreIfAlreadyDeactivated : DeactivationAction.NoForce | DeactivationAction.IgnoreIfAlreadyDeactivated);

            Activate(web, force ? ActivationAction.Force | ActivationAction.IgnoreIfAlreadyActivated : ActivationAction.NoForce | ActivationAction.IgnoreIfAlreadyActivated);
        }

        /// <summary>
        /// Reactivates the feature on the specified farm instance.
        /// </summary>
        /// <param name="farm">The farm instance</param>
        /// <param name="force">Force reactivation.</param>
        public void ReActivate(SPFarm farm, bool force)
        {
            Deactivate(farm, force ? DeactivationAction.Force | DeactivationAction.IgnoreIfAlreadyDeactivated : DeactivationAction.NoForce | DeactivationAction.IgnoreIfAlreadyDeactivated);

            Activate(farm, force ? ActivationAction.Force | ActivationAction.IgnoreIfAlreadyActivated : ActivationAction.NoForce | ActivationAction.IgnoreIfAlreadyActivated);
        }

        
        #endregion


        #region Private members

        private bool IsActivatedInternal(object parent)
        {
            SPFeatureScope scope = Definition.Scope;

            if (scope == SPFeatureScope.Farm)
            {
                SPFarm farm = parent as SPFarm;

                SPWebService service = farm.Services.GetValue<SPWebService>();

                return (service.Features.FirstOrDefault<SPFeature>(f => f.DefinitionId == this.FeatureId) != null);
            }
            else if (scope == SPFeatureScope.WebApplication)
            {
                SPWebApplication webapp = (parent is SPWebApplication) ? parent as SPWebApplication : SPWebApplication.Lookup(new Uri(parent.ToString()));

                return (webapp.Features.FirstOrDefault<SPFeature>(f => f.DefinitionId == this.FeatureId) != null);
            }
            else if (scope == SPFeatureScope.Site || scope == SPFeatureScope.Web)
            {
                SPSite site = null;
                SPWeb web = null;
                bool bShouldDispose = false;

                try
                {
                    if (parent is SPSite)
                    {
                        site = parent as SPSite;
                    }
                    else if (parent is SPWeb)
                    {
                        web = parent as SPWeb;
                        site = web.Site;
                    }
                    else
                    {
                        bShouldDispose = true;
                        site = new SPSite(parent.ToString());
                        web = site.OpenWeb();
                    }

                    if (scope == SPFeatureScope.Site)
                    {
                        return (site.Features.FirstOrDefault<SPFeature>(f => f.DefinitionId == this.FeatureId) != null);
                    }
                    else
                    {
                        return (web.Features.FirstOrDefault<SPFeature>(f => f.DefinitionId == this.FeatureId) != null);
                    }

                }
                finally
                {
                    if (bShouldDispose)
                    {
                        if (web != null)
                            web.Close();
                        if (site != null)
                            site.Close();
                    }
                }

            }
            else
            {
                throw new OperationCanceledException("Can not determin activation stauts. The feature has an invalid scope.");
            }
        }

        private void ActivateFeatureInternal(object parent, ActivationAction action)
        {
            if ((action & ActivationAction.IgnoreIfAlreadyActivated) == ActivationAction.IgnoreIfAlreadyActivated)
            {
                if (IsActivatedInternal(parent))
                {
                    return;
                }
            }

            FeatureActivateOrDeactivate(0, parent, (action & ActivationAction.Force) == ActivationAction.Force);
        }
        
        private void DeactivateFeatureInternal(object parent, DeactivationAction action)
        {
            if ((action & DeactivationAction.IgnoreIfAlreadyDeactivated) == DeactivationAction.IgnoreIfAlreadyDeactivated)
            {
                if (!IsActivatedInternal(parent))
                {
                    return;
                }
            }

            FeatureActivateOrDeactivate(1, parent, (action & DeactivationAction.Force) == DeactivationAction.Force);
        }

        /// <summary>
        /// Activates or deactivates a feature.
        /// </summary>
        /// <param name="operation">0 = Activate, 1 Deactivate</param>
        /// <param name="parent">Could be either of these values: string = URL, SPWebApplication, SPSite, SPWeb</param>
        /// <param name="force"></param>
        /// <param name="ignoreIfNotActivated"></param>
        private void FeatureActivateOrDeactivate(int operation, object parent, bool force)
        {
            SPFeatureScope scope = Definition.Scope;

            if (scope == SPFeatureScope.ScopeInvalid)
            {
                throw new OperationCanceledException("Can not activate a feature with invalid scope value.");
            }

            if (scope == SPFeatureScope.Farm)
            {
                SPWebService service = SPFarm.Local.Services.GetValue<SPWebService>();

                if (operation == 0)
                {
                    service.Features.Add(this.FeatureId, force);
                }
                else
                {
                    if (service.Features[this.FeatureId] != null)
                        service.Features.Remove(this.FeatureId);
                }
            }
            else if (scope == SPFeatureScope.WebApplication)
            {
                SPWebApplication webapp = (parent is SPWebApplication) ? parent as SPWebApplication : SPWebApplication.Lookup(new Uri(parent.ToString()));

                if (operation == 0)
                {
                    webapp.Features.Add(this.FeatureId, force);
                }
                else
                {
                    if (webapp.Features[this.FeatureId] != null)
                        webapp.Features.Remove(this.FeatureId, force);
                }
            }
            else if (scope == SPFeatureScope.Site || scope == SPFeatureScope.Web)
            {
                SPSite site = null;
                SPWeb web = null;
                bool bShouldDispose = false;

                try
                {
                    if (parent is SPSite)
                    {
                        site = parent as SPSite;
                    }
                    else if (parent is SPWeb)
                    {
                        web = parent as SPWeb;
                        site = web.Site;
                    }
                    else
                    {
                        bShouldDispose = true;
                        site = new SPSite(parent.ToString());
                        web = site.OpenWeb();
                    }

                    if (scope == SPFeatureScope.Site)
                    {
                        if (operation == 0)
                        {
                            site.Features.Add(this.FeatureId, force);
                        }
                        else
                        {
                            if (site.Features[this.FeatureId] != null)
                                site.Features.Remove(this.FeatureId, force);
                        }
                    }
                    else
                    {
                        if (operation == 0)
                        {
                            web.Features.Add(this.FeatureId, force);
                        }
                        else
                        {
                            if (web.Features[this.FeatureId] != null)
                                web.Features.Remove(this.FeatureId, force);
                        }
                    }

                }
                finally
                {
                    if (bShouldDispose)
                    {
                        if (web != null)
                            web.Close();
                        if (site != null)
                            site.Close();
                    }
                }

            }
        }

        private bool IsUsingTypeName
        {
            get
            {
                EnsureRequiredFields();

                return _isUsingTypeName;
            }
        }

        private bool _requiredFieldsInitialized;
        private object _requiredFieldsLock = new object();

        private void EnsureRequiredFields()
        {
            if (_requiredFieldsInitialized)
                return;

            lock (_requiredFieldsLock)
            {
                if (_requiredFieldsInitialized)
                    return;

                var a = SPGENCommon.GetAttributeFromType<SPGENFeatureAttribute>(typeof(TFeature));
                if (a == null)
                    throw new SPGENGeneralException(string.Format(@"SPGENFeatureAttribute is missing for feature definition '{0}'.", this.GetType().Name));

                _id = new Guid(a.ID);

                try
                {
                    _name = SPFarm.Local.FeatureDefinitions[_id].DisplayName;
                }
                catch
                {
                    if (!string.IsNullOrEmpty(a.Name))
                    {
                        _name = a.Name;
                    }
                    else
                    {
                        _name = typeof(TFeature).Name;
                        _isUsingTypeName = true;
                    }
                }

                _requiredFieldsInitialized = true;
            }
        }

        #endregion
    }



    [Flags]
    public enum ActivationAction
    {
        NoForce = 1,
        Force = 2,
        IgnoreIfAlreadyActivated = 4
    }

    [Flags]
    public enum DeactivationAction
    {
        NoForce = 1,
        Force = 2,
        IgnoreIfAlreadyDeactivated = 4
    }

}
