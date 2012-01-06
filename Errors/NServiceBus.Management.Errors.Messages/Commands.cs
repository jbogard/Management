using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public class ReprocessErrorMessage : ICommand
    {
        public string MessageId { get; set; }
    }

    public class ReprocessAllErrors : ICommand
    {
    }

    public class DeleteErrorMessage : ICommand
    {
        public string MessageId { get; set; }
    }

    public interface ProcessErrorMessage : IErrorMessageDetails, ICommand
    {
    }
}
