﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SPGenesis.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SPGENFeatureAssociationAttribute : Attribute
    {
        public Type Feature { get; set; }
        public string ID { get; set; }

        public SPGENFeatureAssociationAttribute()
        {
        }

        public SPGENFeatureAssociationAttribute(string featureId)
        {
            this.ID = featureId;
        }

        public SPGENFeatureAssociationAttribute(Type featureClass)
        {
            this.Feature = featureClass;
        }
    }
}
