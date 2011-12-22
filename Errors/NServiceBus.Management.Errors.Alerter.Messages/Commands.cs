using NServiceBus;
using System;
using NServiceBus.Management.Errors.Messages;
using System.Collections.Generic;

namespace NServiceBus.Management.Errors.Alerter.Messages
{
    public class ProcessErrorMessageReceived : IMessage
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageReceived MessageDetails { get; set; }  
    }

    public class ProcessErrorMessageReprocessed : IMessage
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageReprocessed MessageDetails { get; set; }
    }

    public class ProcessErrorMessageDeleted : IMessage
    {
        public Guid AlerterSagaId { get; set; }
        public ErrorMessageDeleted MessageDetails { get; set; }
    }

    public class SendErrorAlert : IMessage
    {
        public List<ErrorMessageReceived> ErrorList { get; set; }
    }

    public class AlertTooManyErrorsInQueue : IMessage
    {
        public int Count { get; set; }
        public ErrorMessageReceived FirstErrorMessage { get; set; }
    }
}
