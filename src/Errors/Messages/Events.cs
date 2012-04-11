using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public class ErrorMessageReceived : IErrorMessageDetails, IEvent
    {
        public string FailedMessageId { get; set; }
        public string OriginalMessageId { get; set; }
        public string ProcessingFailedAddress { get; set; }
        public string Body { get; set; }
        public DateTime ErrorReceivedTime { get; set; }
        public string Identity { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string ExceptionInformation { get; set; }
    }

    public class ErrorMessageReprocessed : IEvent
    {
        public string MessageId { get; set; }
        public DateTime ErrorReprocessedTime { get; set; }
    }

    public class ErrorMessageDeleted : IEvent
    {
        public string MessageId { get; set; }
        public DateTime ErrorDeletedTime { get; set; }
    }
}
