using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter
{
    class ConfigureRavenPersistence : IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance
                .RunTimeoutManager();
                //.UseRavenTimeoutPersister()
                //.RavenSagaPersister();
                            
        }
    }
}
