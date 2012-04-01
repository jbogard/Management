using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Alerter
{

    public class AlerterSagaData : ISagaEntity
    {
        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }

        public string MessageId { get; set; }
        public bool WasErrorReceived { get; set; } 
        public ErrorMessageReceived MessageDetails { get; set; }
        public bool WasErrorCleared { get; set; }
        public DateTime TimeOfClearing { get; set; }
    }

}
