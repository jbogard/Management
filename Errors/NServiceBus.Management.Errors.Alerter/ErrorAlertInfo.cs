using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Alerter
{
    public class ErrorAlertInfo
    {
        public string MessageId { get; set; }
        public ErrorMessageReceived ErrorMessage { get; set; }
        public int NumberOfTimesAlerted { get; set; }
    }
}
