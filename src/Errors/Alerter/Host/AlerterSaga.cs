using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Messages;
using NServiceBus.Management.Errors.Alerter.Messages;
using System.Configuration;

namespace NServiceBus.Management.Errors.Alerter
{
    class AlerterSaga : Saga<AlerterSagaData>,
        IAmStartedByMessages<ErrorMessageReceived>,
        IAmStartedByMessages<ErrorMessageDeleted>,
        IAmStartedByMessages<ErrorMessageReprocessed>
    {

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<ErrorMessageReceived>(s => s.MessageId, m => m.OriginalMessageId);
            ConfigureMapping<ErrorMessageDeleted>(s => s.MessageId, m => m.MessageId);
            ConfigureMapping<ErrorMessageReprocessed>(s => s.MessageId, m => m.MessageId);
        }

        public void Handle(ErrorMessageReceived message)
        {
            if (Data.WasErrorCleared && message.ErrorReceivedTime < Data.TimeOfClearing)
            {
                MarkAsComplete();
            }
            else
            {
                Data.MessageId = message.OriginalMessageId;
                Data.MessageDetails = message;
                RequestUtcTimeout(TimeSpan.FromSeconds(60), "state");
            }
        }

        public void Handle(ErrorMessageDeleted message)
        {
            if (Data.WasErrorReceived)
            {
                if (message.ErrorDeletedTime >= Data.MessageDetails.ErrorReceivedTime)
                {
                    MarkAsComplete();
                    return;
                }
            }
           
            Data.MessageId = message.MessageId;
            Data.WasErrorCleared = true;
            if (message.ErrorDeletedTime > Data.TimeOfClearing)
            {
                Data.TimeOfClearing = message.ErrorDeletedTime;
            }
        }

        public void Handle(ErrorMessageReprocessed message)
        {
            if (Data.WasErrorReceived)
            {
                if (message.ErrorReprocessedTime >= Data.MessageDetails.ErrorReceivedTime)
                {
                    MarkAsComplete();
                    return;
                }
            }
            
            Data.MessageId = message.MessageId;
            Data.WasErrorCleared = true;
            
            if (message.ErrorReprocessedTime > Data.TimeOfClearing)
            {
                Data.TimeOfClearing = message.ErrorReprocessedTime;
            }
        }

        public override void Timeout(object state)
        {
            base.Timeout(state);
            Guid alerterGuid;
            Guid.TryParse(ConfigurationManager.AppSettings["AlerterInstanceId"], out alerterGuid);
            // This message has been sitting out in the error queue for x minutes. Send an alert.
            Bus.SendLocal<ProcessErrorMessageReceived>(m => { m.AlerterInstanceId = alerterGuid; m.MessageDetails = Data.MessageDetails; });
        }
    }
}
