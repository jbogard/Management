using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NServiceBus;
using NServiceBus.Management.Errors.Alerter.Config;

namespace NServiceBus.Management.Errors.Alerter.Host
{
    class ConfigureSagaPersistence : IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance
                .RunTimeoutManager()
                .RavenPersistence()
                .RavenSagaPersister();

            // Configure the Saga Properties
            //NServiceBus.Configure.Instance.Configurer
            //    .ConfigureProperty<ThresholdLimitReachedAlertSaga>(s => s.MaxTimesToAlert, maxTimesToAlert)
            //    .ConfigureProperty<ThresholdLimitReachedAlertSaga>(s => s.CriticalErrorLimits, criticalErrorLimits);
              
        }
    }
}
