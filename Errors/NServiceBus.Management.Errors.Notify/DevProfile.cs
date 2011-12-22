using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Notify
{
    class DevProfile : IHandleProfile<NServiceBus.Lite>
    {
        #region IHandleProfile Members

        public void ProfileActivated()
        {
            NServiceBus.Configure.With().Configurer.RegisterSingleton<INotifyOnError>(NotifyInConsole.Instance);
        }

        #endregion
    }
}
