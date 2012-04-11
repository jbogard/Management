using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus.Management.Errors.Alerter.DomainEvents;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter.AlertProviders.ConsoleAlertProvider
{
    public class NotifyInConsole : IHandleDomainEvents<MaxThresholdLimitReached>
    {
        public void Handle(MaxThresholdLimitReached args)
        {
            // Check to see if this provider is configured
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;
            AlertRule rule = ruleCollection[args.RuleId];
            AlertProviderCollection alertProviders = rule.AlertProviders;

            foreach (Provider provider in alertProviders)
            {
                if (provider.Type == "Console")
                {
                    Console.WriteLine("ALERT -- TOO MANY MESSAGES IN Q (Total:{0})", args.TotalErrorsInErrorQueue);
                }
            }
        }
    }
}
