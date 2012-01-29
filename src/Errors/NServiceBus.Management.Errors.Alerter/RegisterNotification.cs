using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Notify;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter
{
    class RegisterNotification : IWantCustomInitialization
    {
        #region IWantCustomInitialization Members

        public void Init()
        {
            NotifyByEmail notifyOnError = NotifyByEmail.Instance;
            notifyOnError.RecipientList = ConfigurationManager.AppSettings["RecipientList"];
            notifyOnError.CCList = ConfigurationManager.AppSettings["CCList"];
            notifyOnError.BccList = ConfigurationManager.AppSettings["BccList"];
            
            NServiceBus.Configure.With().Configurer.RegisterSingleton<INotifyOnError>(notifyOnError);
            
            //NServiceBus.Configure.With().Configurer.RegisterSingleton<INotifyOnError>(NotifyInConsole.Instance);
        }

        #endregion
    }
}
