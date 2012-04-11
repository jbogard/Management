using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;
using NServiceBus.Management.Errors.Alerter.Messages;

namespace NServiceBus.Management.Errors.Alerter.Rules.ThresholdLimitReached
{
    class ErrorEventHandlers : IHandleMessages<ErrorMessageReceived>,
        IHandleMessages<ErrorMessageReprocessed>,
        IHandleMessages<ErrorMessageDeleted>
    {
        public IBus Bus { get; set; }
        public void Handle(ErrorMessageReceived message)
        {
            var ruleIds = GetRuleIds();
            foreach (string ruleId in ruleIds)
            {
                Bus.SendLocal<ProcessErrorMessageReceived>(m =>
                {
                    m.RuleId = ruleId;
                    m.MessageDetails = message;
                });
            }
        }

        public void Handle(ErrorMessageReprocessed message)
        {
            var ruleIds = GetRuleIds();
            foreach (string ruleId in ruleIds)
            {
                Bus.SendLocal<ProcessErrorMessageReprocessed>(m =>
                {
                    m.RuleId = ruleId;
                    m.MessageDetails = message;
                });
            }
        }

        public void Handle(ErrorMessageDeleted message)
        {
            var ruleIds = GetRuleIds();
            foreach (string ruleId in ruleIds)
            {
                Bus.SendLocal<ProcessErrorMessageDeleted>(m =>
                {
                    m.RuleId = ruleId;
                    m.MessageDetails = message;
                });
            }
        }

        private List<string> GetRuleIds()
        {
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;

            List<string> ruleIdsToProcess = new List<string>();
            
            foreach (AlertRule rule in ruleCollection)
            {
                if (rule.Tag.Equals("CriticalLimit"))
                {
                    ruleIdsToProcess.Add(rule.Name);
                }
            }
            return ruleIdsToProcess;
        }
    }
}
