using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public interface IErrorMessageDetails 
    {
        string FailedMessageId { get; set; }
        string OriginalMessageId { get; set; }
        string ProcessingFailedAddress { get; set; }
        string Body { get; set; }
        DateTime ErrorReceivedTime { get; set; }
        string Identity { get; set; }
        Dictionary<string, string> Headers { get; set; }
        string ExceptionInformation { get; set; }
    }
}
