using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors
{
    public interface INotifyOnError
    {
        void AlertOnError(IErrorMessageDetails[] errorMessages);
        void AlertTooManyMessagesInErrorQueue(int count, IErrorMessageDetails lastErrorMessage);
    }
}
