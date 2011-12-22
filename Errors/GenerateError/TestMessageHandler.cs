using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace GenerateError
{
    class TestMessageHandler : IHandleMessages<SendTestMessage>
    {
        public void Handle(SendTestMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
