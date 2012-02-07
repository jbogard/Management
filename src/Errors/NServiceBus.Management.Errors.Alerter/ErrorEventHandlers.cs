using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Messages;
using System.Configuration;
using NServiceBus;

namespace NServiceBus.Management.Errors.Alerter
{
    class ErrorEventHandlers : IHandleMessages<ErrorMessageReceived>, 
        IHandleMessages<ErrorMessageDeleted>,
        IHandleMessages<ErrorMessageReprocessed>
    {
        public IBus Bus { get; set; }
        private Guid alerterGuid;

        public ErrorEventHandlers()
        {
            Guid.TryParse(ConfigurationManager.AppSettings["AlerterInstanceId"], out alerterGuid);
        }

        public void Handle(ErrorMessageReceived message)
        {
            Bus.SendLocal<ProcessErrorMessageReceived>(m => { m.AlerterInstanceId = this.alerterGuid; m.MessageDetails = message; });
        }

        public void Handle(ErrorMessageDeleted message)
        {
            Bus.SendLocal<ProcessErrorMessageDeleted>(m => { m.AlerterInstanceId = this.alerterGuid; m.MessageDetails = message; });
        }

        public void Handle(ErrorMessageReprocessed message)
        {
            Bus.SendLocal<ProcessErrorMessageReprocessed>(m => { m.AlerterInstanceId = this.alerterGuid; m.MessageDetails = message; });
        }
    }
}
