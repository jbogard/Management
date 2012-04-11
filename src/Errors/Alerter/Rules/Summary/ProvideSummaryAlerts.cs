using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Config;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus.Unicast;

namespace NServiceBus.Management.Errors.Alerter.Rules.Summary
{
    class ProvideSummaryAlerts : IWantToRunWhenTheBusStarts
    {
        public IBus Bus { get; set; }

        public void Run()
        {
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;    

            // Get the time to wait before providing summary alert.
            foreach (AlertRule rule in ruleCollection)
            {
                if (rule.Tag.Equals("SummaryAlert"))
                {
                    Bus.Defer(DateTime.Now.AddMinutes(int.Parse(rule.Value)), new SendSummaryAlert
                    {
                        RuleId = rule.Name,
                        DurationToWait = int.Parse(rule.Value)
                    });
                }
            }
        }
    }
}
