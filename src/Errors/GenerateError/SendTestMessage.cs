using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace GenerateError
{
    public class SendTestMessage : IMessage
    {
        public string Name { get; set; }
    }
}
