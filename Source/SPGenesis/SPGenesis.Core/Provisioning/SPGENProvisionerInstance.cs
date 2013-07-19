using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPGenesis.Core
{
    public sealed class SPGENProvisionerInstance
    {
        private object _instance;
        private object _provisionAction;

        public Type Type { get; private set; }
        public SPGENProvisioningOnErrorBehavior OnErrorBehavior { get; set; }
        public Action<SPGENProvisionerInstance, Exception> OnErrorAction { get; set; }

        internal SPGENProvisionerInstance(Type element)
            : this(element, null, null, SPGENProvisioningOnErrorBehavior.Continue)
        {
        }
        internal SPGENProvisionerInstance(Type element, object provisionAction)
            : this(element, provisionAction, null, SPGENProvisioningOnErrorBehavior.Continue)
        {
        }
        internal SPGENProvisionerInstance(Type element, object provisionAction, Action<SPGENProvisionerInstance, Exception> onErrorAction ,SPGENProvisioningOnErrorBehavior onErrorBehavior)
        {
            _instance = SPGENElementManager.GetInstance(element);
            _provisionAction = provisionAction;

            this.Type = element;
            this.OnErrorAction = onErrorAction;
            this.OnErrorBehavior = onErrorBehavior;
        }

        internal void Provision(SPWeb web)
        {
            ProvisionOrUnprovision(web, 0);
        }

        internal void Unprovision(SPWeb web)
        {
            ProvisionOrUnprovision(web, 1);
        }

        private void ProvisionOrUnprovision(SPWeb web, int mode)
        {
            if (_instance is SPGENFieldBase)
            {
                var e = _instance as SPGENFieldBase;
                e.OnProvisionerAction = _provisionAction as Action<SPField>;

                if (mode == 0)
                {
                    e.Provision(web.Fields, true, true);
                }
                else
                {
                    e.Unprovision(web.Fields);
                }
            }
            else if (_instance is SPGENContentTypeBase)
            {
                var e = _instance as SPGENContentTypeBase;
                e.OnProvisionerAction = _provisionAction as Action<SPGENContentTypeProvisioningArguments>;

                if (mode == 0)
                {
                    e.Provision(web.ContentTypes, null, true, true, true);
                }
                else
                {
                    e.Unprovision(web.ContentTypes, false, false, true);
                }
            }
            else if (_instance is SPGENListInstanceBase)
            {
                var e = _instance as SPGENListInstanceBase;
                e.OnProvisionerAction = _provisionAction as Action<SPGENListProvisioningArguments>;

                if (mode == 0)
                {
                    e.ProvisionOnWeb(web);
                }
                else
                {
                    e.Unprovision(web.Lists);
                }
            }
        }

    }

    public enum SPGENProvisioningOnErrorBehavior
    {
        StopAndThrowException,
        Stop,
        Continue,
        Rollback
    }
}
