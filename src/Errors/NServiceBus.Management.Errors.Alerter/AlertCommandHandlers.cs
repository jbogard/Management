using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Messages;

namespace NServiceBus.Management.Errors.Alerter
{
    class AlertCommandHandlers : IHandleMessages<SendErrorAlert>, 
        IHandleMessages<AlertTooManyErrorsInQueue>
    {
        public INotifyOnError ErrorNotifier { get; set; }
       
        public void Handle(SendErrorAlert message)
        {
            ErrorNotifier.AlertOnError(message.ErrorList.ToArray<IErrorMessageDetails>());
        }

        public void Handle(AlertTooManyErrorsInQueue message)
        {
            ErrorNotifier.AlertTooManyMessagesInErrorQueue(message.Count, message.FirstErrorMessage);
        }
    }
}
