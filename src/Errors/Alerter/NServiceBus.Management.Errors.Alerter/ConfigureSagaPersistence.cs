using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NServiceBus;
using NServiceBus.Management.Errors.Alerter.Config;

namespace NServiceBus.Management.Errors.Alerter
{
    class ConfigureSagaPersistence : IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance
                .RunTimeoutManager()
                .RavenPersistence()
                .RavenSagaPersister();

            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;
            int maxTimesToAlert = ruleCollection.MaxTimesToAlert;

            Dictionary<string, int> criticalErrorLimits = new Dictionary<string,int>();
            Dictionary<string, int> summaryAlerts = new Dictionary<string,int>();
            Dictionary<string, string> messageTypeAlerts = new Dictionary<string,string>();
            Dictionary<string, int> timeInErrorQExceededAlerts = new Dictionary<string,int>();

            // Get the time to wait before providing summary alert.
            foreach(AlertRule rule in ruleCollection)
            {
                if (rule.Tag.Equals("CriticalLimit"))
                {
                    criticalErrorLimits.Add(rule.Name, int.Parse(rule.Value));
                }

                if (rule.Tag.Equals("SummaryAlert"))
                {
                    summaryAlerts.Add(rule.Name, int.Parse(rule.Value));
                }

                if (rule.Tag.Equals("ErrorMessageType"))
                {
                    messageTypeAlerts.Add(rule.Name, rule.Value);
                }

                if (rule.Tag.Equals("TimeInErrorQExceeded"))
                {
                    timeInErrorQExceededAlerts.Add(rule.Name, int.Parse(rule.Value));
                }
            }

            // Configure the Saga Properties
            NServiceBus.Configure.Instance.Configurer
                .ConfigureProperty<ThresholdLimitReachedAlertSaga>(s => s.MaxTimesToAlert, maxTimesToAlert)
                .ConfigureProperty<ThresholdLimitReachedAlertSaga>(s => s.CriticalErrorLimits, criticalErrorLimits);
              
        }
    }
}
