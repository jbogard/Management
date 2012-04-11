using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.PrismEvents
{
    public class ErrorMessageReceivedPrismEvent : CompositePresentationEvent<ErrorMessageReceived>
    {
    }
}
