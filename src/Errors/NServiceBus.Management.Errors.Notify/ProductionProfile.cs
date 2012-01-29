using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errors.Notify
{
    class ProductionProfile : IHandleProfile<NServiceBus.Production>
    {
        #region IHandleProfile Members

        public void ProfileActivated()
        {
            NServiceBus.Configure.With().Configurer.RegisterSingleton<INotifyOnError>(NotifyByEmail.Instance);
        }

        #endregion
    }
}
