using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors
{
    public interface IPersistErrorMessages
    {
        void SaveErrorMessage(IErrorMessageDetails details);
        void DeleteErrorMessage(string messageId);
    }
}
