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
    class AlerterSummarySaga : Saga<AlerterSummarySagaData>, 
        IAmStartedByMessages<ProcessErrorMessageReceived>,
        IAmStartedByMessages<ProcessErrorMessageDeleted>,
        IAmStartedByMessages<ProcessErrorMessageReprocessed>
    {
        private int TimeToWaitBeforeAlerting = int.Parse(ConfigurationManager.AppSettings["TimeToWaitBeforeAlerting"]);
        private int CriticalErrorLimit = int.Parse(ConfigurationManager.AppSettings["CriticalErrorLimit"]);
        
        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<ProcessErrorMessageReceived>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
            ConfigureMapping<ProcessErrorMessageDeleted>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
            ConfigureMapping<ProcessErrorMessageReprocessed>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
        }

        public void Handle(ProcessErrorMessageReceived message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageReceived arrived. MessageId: {0}", message.MessageDetails.OriginalMessageId));
            Data.AlerterInstanceId = message.AlerterInstanceId;
            if (!Data.IsTimeoutAlreadyRequested)
            {
                Data.WriteInfo(string.Format("Timeout is now requested for this saga: {0}", Data.Id));
                Data.IsTimeoutAlreadyRequested = true;
                // This is the first message received, so request a timeout. We are going to keep
                // adding all of the error messages received in the timespan and send out one alert.
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "summary");
            }
            else
            {
                Data.WriteInfo(string.Format("Timeout has already been requested for this saga: {0}", Data.Id));
            }

            // Add to the list
            ErrorAlertInfo alertInfo = new ErrorAlertInfo() { ErrorMessage = message.MessageDetails, NumberOfTimesAlerted = 0 };
            Data.AddAlertForMessage(alertInfo);
        }

        public void Handle(ProcessErrorMessageDeleted message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageDeleted arrived. MessageId: {0}", message.MessageDetails.MessageId));
            Data.AlerterInstanceId = message.AlerterInstanceId;

            if (!Data.IsErrorInAlertList(message.MessageDetails.MessageId) && !Data.IsErrorInToBeClearedList(message.MessageDetails.MessageId))
            {
                Data.WriteInfo(string.Format("Going to handle this message later: MessageId: {0}", message.MessageDetails.MessageId));
                Bus.HandleCurrentMessageLater();
            }
            else
            {
                Data.ClearAlertForMessage(message.MessageDetails.MessageId, message.MessageDetails.ErrorDeletedTime);
            }
        }

        public void Handle(ProcessErrorMessageReprocessed message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageReprocessed arrived. MessageId: {0}", message.MessageDetails.MessageId));
            Data.AlerterInstanceId = message.AlerterInstanceId;
            if (!Data.IsErrorInAlertList(message.MessageDetails.MessageId) && !Data.IsErrorInToBeClearedList(message.MessageDetails.MessageId))
            {
                Data.WriteInfo(string.Format("Going to handle this message later: MessageId: {0}", message.MessageDetails.MessageId));
                Bus.HandleCurrentMessageLater();
            }
            else
            {
                Data.ClearAlertForMessage(message.MessageDetails.MessageId, message.MessageDetails.ErrorReprocessedTime);
            }
        }

        public override void Timeout(object state)
        {
            Data.WriteInfo(string.Format("Timeout has been RECEIVED for this saga: {0}", Data.Id));

            base.Timeout(state);

            // is the error queue flooded?
            if (Data.ErrorListToAlert.Count >= this.CriticalErrorLimit)
            {
                Bus.SendLocal<AlertTooManyErrorsInQueue>(m =>
                {
                    m.Count = Data.ErrorListToAlert.Count;
                    m.FirstErrorMessage = Data.ErrorListToAlert.First().ErrorMessage;
                });
                Data.IncrementAlertCount();
                
                // Request another timeout
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "state");
                return;
            }

            if (Data.ErrorListToAlert.Count > 0)
            {
                var errorList = (from msg in Data.ErrorListToAlert select msg.ErrorMessage).ToList();
                Bus.SendLocal<SendErrorAlert>(m =>
                {
                    m.ErrorList = errorList;
                });
                Data.IncrementAlertCount();
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "state");
            }
            else
            {
                Data.IsTimeoutAlreadyRequested = false;
                Data.WriteInfo("Timeout is now being cleared");
            }
        }
    }
}
