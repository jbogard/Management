using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus.Unicast;

namespace NServiceBus.Management.Errors.Alerter.Rules.ThresholdLimitReached
{
    class BootstrapThresholdLimitReachedSaga : IWantToRunWhenTheBusStarts
    {
        public IBus Bus { get; set; }
        public void Run()
        {
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;
            int maxTimesToAlert = ruleCollection.MaxTimesToAlert;

            foreach (AlertRule rule in ruleCollection)
            {
                if (rule.Tag.Equals("CriticalLimit"))
                {
                    Bus.SendLocal<EvaluateThresholdLimitExceededAlertRule>(m =>
                    {
                        m.CriticalErrorLimit = int.Parse(rule.Value);
                        m.RuleId = rule.Name;
                        m.MaxTimesToAlert = maxTimesToAlert;
                    });
                }
            }
        }
    }
}
