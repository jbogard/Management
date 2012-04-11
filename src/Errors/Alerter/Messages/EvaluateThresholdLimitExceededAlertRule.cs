using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Alerter.Messages
{
    public class EvaluateThresholdLimitExceededAlertRule : ICommand
    {
        public string RuleId { get; set; }
        public int MaxTimesToAlert { get; set; }
        public int CriticalErrorLimit { get; set; }
    }
}
