using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.Config
{
    public class Provider : ConfigurationElement
    {
        [ConfigurationProperty("Type", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)base["Type"]; }
            set { base["Type"] = value; }
        }

        [ConfigurationProperty("RecipientList", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string RecipientList
        {
            get { return (string)base["RecipientList"]; }
            set { base["RecipientList"] = value; }
        }
    }
}
