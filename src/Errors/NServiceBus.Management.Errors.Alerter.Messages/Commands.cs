using NServiceBus;
using System;
using NServiceBus.Management.Errors.Messages;
using System.Collections.Generic;

namespace NServiceBus.Management.Errors.Alerter.Messages
{
    public class ProcessErrorMessageReceived : ICommand
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageReceived MessageDetails { get; set; }  
    }

    public class ProcessErrorMessageReprocessed : ICommand
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageReprocessed MessageDetails { get; set; }
    }

    public class ProcessErrorMessageDeleted : ICommand
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageDeleted MessageDetails { get; set; }
    }

    public class SendErrorAlert : ICommand
    {
        public List<ErrorMessageReceived> ErrorList { get; set; }
    }

    public class AlertTooManyErrorsInQueue : ICommand
    {
        public int Count { get; set; }
        public ErrorMessageReceived FirstErrorMessage { get; set; }
    }
}
