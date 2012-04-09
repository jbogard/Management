using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.Config
{
    [ConfigurationCollection(typeof(AlertRule))]
    public class AlertRuleCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "AlertRule";
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        [ConfigurationProperty("MaxTimesToAlert", DefaultValue = "5", IsKey = false, IsRequired = false)]
        public int MaxTimesToAlert
        {
            get { return (int)base["MaxTimesToAlert"]; }
            set { base["MaxTimesToAlert"] = value; }
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
            return new AlertRule();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AlertRule)(element)).Name;
        }

        public AlertRule this[object index]
        {
            get { return (AlertRule)BaseGet(index); }
        }
    }
}
