using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus.Management.Errors.Alerter.DomainEvents;
using NServiceBus.Management.Errrors.Alerter.DomainEvents;

namespace NServiceBus.Management.Errors.Alerter.MessageHandlers
{
    class SendCriticalErrorLimitReachedAlertHandler : IHandleMessages<SendCriticalErrorLimitReachedAlert>
    {
        public IBus Bus { get; set; }
        public IQueryErrorPersistence QueryProvider { get; set; }
        public void Handle(SendCriticalErrorLimitReachedAlert message)
        {
            if (QueryProvider.ErrorMessages.Count > 0)
            {
                Dispatcher.Raise (new MaxThresholdLimitReached
                {
                    RuleId = message.RuleId,
                    TotalErrorsInErrorQueue = message.Count,
                    FirstErrorMessage = QueryProvider.ErrorMessages[0]
                });
            }
        }
    }
}
