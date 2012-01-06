using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public class ErrorMessageReceived : IErrorMessageDetails, IEvent
    {
        public string FailedMessageId { get; set; }
        public string ProcessingFailedAddress { get; set; }
        public string XmlBody { get; set; }
        public DateTime TimeSent { get; set; }
        public string WindowsIdentity { get; set; }
        public Dictionary<string, string> AdditionalInformation { get; set; }
        public string ExceptionInformation { get; set; }
    }

    public class ErrorMessageReprocessed : IEvent
    {
        public string MessageId { get; set; }
    }

    public class ErrorMessageDeleted : IEvent
    {
        public string MessageId { get; set; }
    }
}
