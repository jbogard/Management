using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Messages;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter
{
    class MessageHandlers : IHandleMessages<ErrorMessageReceived>, 
        IHandleMessages<ErrorMessageDeleted>,
        IHandleMessages<ErrorMessageReprocessed>
    {
        public IBus Bus { get; set; }
        private Guid alerterGuid;

        public MessageHandlers()
        {
            Guid.TryParse(ConfigurationManager.AppSettings["AlerterSagaId"], out alerterGuid);
        }

        public void Handle(ErrorMessageReceived message)
        {
            Bus.Send<ProcessErrorMessageReceived>(m => { m.AlerterSagaId = this.alerterGuid; m.MessageDetails = message; });
        }

        public void Handle(ErrorMessageDeleted message)
        {
            Bus.Send<ProcessErrorMessageDeleted>(m => { m.AlerterSagaId = this.alerterGuid; m.MessageDetails = message; });
        }

        public void Handle(ErrorMessageReprocessed message)
        {
            Bus.Send<ProcessErrorMessageReprocessed>(m => { m.AlerterSagaId = this.alerterGuid; m.MessageDetails = message; });
        }
    }
}
