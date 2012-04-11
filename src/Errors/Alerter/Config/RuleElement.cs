using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.Config
{
    public class AlertRule : ConfigurationElement
    {
        [ConfigurationProperty("Name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["Name"]; }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("Tag", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Tag
        {
            get { return (string)base["Tag"]; }
            set { base["Tag"] = value; }
        }

        [ConfigurationProperty("Value", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Value
        {
            get { return (string)base["Value"]; }
            set { base["Value"] = value; }
        }

        [ConfigurationProperty("AlertProviders")]
        public AlertProviderCollection AlertProviders
        {
            get { return (AlertProviderCollection)base["AlertProviders"]; }
            set { base["AlertProviders"] = value; }
        }
    }
}
