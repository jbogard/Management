using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;


namespace NServiceBus.Management.Errors.Alerter
{
    class ConfigureTransactionIsolation : IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance.MsmqTransport().IsolationLevel(IsolationLevel.Serializable);
        }
    }
}
