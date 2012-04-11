using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Alerter.Messages
{
    public class EvaluateSummaryAlertRule : ICommand
    {
        public string RuleId { get; set; }
        public int TimeToWaitBeforeAlerting { get; set; }
    }
}
