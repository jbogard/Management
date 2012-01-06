using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public interface IErrorMessageDetails 
    {
        string FailedMessageId { get; set; }
        string ProcessingFailedAddress { get; set; }
        string XmlBody { get; set; }
        DateTime TimeSent { get; set; }
        string WindowsIdentity { get; set; }
        Dictionary<string, string> AdditionalInformation { get; set; }
        string ExceptionInformation { get; set; }
    }
}
