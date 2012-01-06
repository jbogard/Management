using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using NServiceBus.Faults;

namespace GenerateError
{
    class ConfigureForwarder : IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance.MessageForwardingInCaseOfFault();
        }
    }
}
