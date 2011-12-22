using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public interface IErrorMessageDetails : IMessage
    {
        string FailedMessageId { get; set; }
        string ProcessingFailedAddress { get; set; }
        string XmlBody { get; set; }
        DateTime TimeSent { get; set; }
        string WindowsIdentity { get; set; }
        Dictionary<string, string> HeaderList { get; set; }
    }
}
