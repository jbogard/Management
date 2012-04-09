using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.DomainEvents;
using NServiceBus.Management.Errrors.Alerter.DomainEvents;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Alerter.MessageHandlers
{
    class SendSummaryAlertHandler : IHandleMessages<SendSummaryAlert>
    {
        public IBus Bus { get; set; }
        public IQueryErrorPersistence QueryProvider { get; set; }
        public void Handle(SendSummaryAlert message)
        {
            Console.WriteLine("Received SendSummaryAlert message ... Checking to see if we have any errors at this time");
            if (QueryProvider.ErrorMessages.Count > 0)
            {
                Dispatcher.Raise<TimeElapsedForSummaryAlert>(new TimeElapsedForSummaryAlert
                {
                    RuleId = message.RuleId,
                    ErrorMessages = QueryProvider.ErrorMessages.ToArray<IErrorMessageDetails>()
                });
            }
            else
                Console.WriteLine("No Error Messages in the Queue");

            // Request the next Defer.
            Bus.Defer(DateTime.Now.AddMinutes(message.DurationToWait), new SendSummaryAlert
            {
                RuleId = message.RuleId,
                DurationToWait = message.DurationToWait
            });
        }
    }
}
