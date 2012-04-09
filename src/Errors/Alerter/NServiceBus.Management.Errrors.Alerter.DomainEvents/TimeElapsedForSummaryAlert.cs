using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errrors.Alerter.DomainEvents
{
    public class TimeElapsedForSummaryAlert : IDomainEvent
    {
        public string RuleId { get; set; }
        public IErrorMessageDetails[] ErrorMessages { get; set; }
    }
}
