using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.Config
{
    public class AlertRulesSection : ConfigurationSection
    {
        [ConfigurationProperty("AlertRules")]
        public AlertRuleCollection RuleCollection
        {
            get { return ((AlertRuleCollection)(base["AlertRules"])); }
            set { base["AlertRules"] = value; }
        }
    }
}
