using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace NServiceBus.Management.Errors.TestHarness
{
    public class SendTestMessage : IMessage
    {
        public string Name { get; set; }
    }
}
