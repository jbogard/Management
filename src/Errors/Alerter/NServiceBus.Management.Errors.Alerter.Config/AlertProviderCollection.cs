using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.Config
{
    [ConfigurationCollection(typeof(Provider))]
    public class AlertProviderCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "Provider";
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Provider();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Provider)(element)).Type;
        }

        public Provider this[object index]
        {
            get { return (Provider)BaseGet(index); }
        }
    }
}
