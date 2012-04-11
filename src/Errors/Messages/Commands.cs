using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Messages
{
    public class ReprocessErrorMessage : ICommand
    {
        public string OriginalMessageId { get; set; }
    }

    public class ReprocessAllErrors : ICommand
    {
    }

    public class DeleteErrorMessage : ICommand
    {
        public string OriginalMessageId { get; set; }
    }

    public interface ProcessErrorMessage : IErrorMessageDetails, ICommand
    {
    }
}
