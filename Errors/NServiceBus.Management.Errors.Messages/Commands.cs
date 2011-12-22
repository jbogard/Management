using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public class ReprocessErrorMessage : IMessage
    {
        public string MessageId { get; set; }
    }

    public class ReprocessAllErrors : IMessage
    {
    }

    public class DeleteErrorMessage : IMessage
    {
        public string MessageId { get; set; }
    }

    public interface ProcessErrorMessage : IErrorMessageDetails, IMessage
    {
    }
}
