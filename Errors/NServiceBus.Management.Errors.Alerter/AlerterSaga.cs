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
        IAmStartedByMessages<ProcessErrorMessageReceived>,
        IAmStartedByMessages<ProcessErrorMessageDeleted>,
        IAmStartedByMessages<ProcessErrorMessageReprocessed>
    {
        private int TimeToWaitBeforeAlerting = int.Parse(ConfigurationManager.AppSettings["TimeToWaitBeforeAlerting"]);
        private int CriticalErrorLimit = int.Parse(ConfigurationManager.AppSettings["CriticalErrorLimit"]);

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<ProcessErrorMessageReceived>(s => s.AlerterSagaId, m => m.AlerterSagaId);
            ConfigureMapping<ProcessErrorMessageDeleted>(s => s.AlerterSagaId, m => m.AlerterSagaId);
            ConfigureMapping<ProcessErrorMessageReprocessed>(s => s.AlerterSagaId, m => m.AlerterSagaId);
        }

        public void Handle(ProcessErrorMessageReceived message)
        {   
            Data.AlerterSagaId = message.AlerterSagaId;
            if (!Data.IsTimeoutAlreadyRequested) 
            {
                Data.IsTimeoutAlreadyRequested = true;
                // This is the first message received, so request a timeout. We are going to keep
                // adding all of the error messages received in the timespan and send out one alert.
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "state");
            }

            // Add to the list
            ErrorAlertInfo alertInfo = new ErrorAlertInfo() { MessageId = message.MessageDetails.FailedMessageId, ErrorMessage = message.MessageDetails, NumberOfTimesAlerted = 0 };
            Data.ErrorListToAlert.Add(alertInfo);
        }

        public void Handle(ProcessErrorMessageDeleted message)
        {
            Data.AlerterSagaId = message.AlerterSagaId;
            ClearAlertForMessage(message.MessageDetails.MessageId);
        }

        public void Handle(ProcessErrorMessageReprocessed message)
        {
            Data.AlerterSagaId = message.AlerterSagaId;
            ClearAlertForMessage(message.MessageDetails.MessageId);
        }

        public override void Timeout(object state)
        {
            base.Timeout(state);

            // is the error queue flooded?
            if (Data.ErrorListToAlert.Count >= this.CriticalErrorLimit)
            {
                Bus.Send<AlertTooManyErrorsInQueue>(m =>
                {
                    m.Count = Data.ErrorListToAlert.Count;
                    m.FirstErrorMessage = Data.ErrorListToAlert.First().ErrorMessage;
                });
                IncrementAlertCount(Data.ErrorListToAlert);
                
                // Request another timeout
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "state");
                return;
            }

            if (Data.ErrorListToAlert.Count > 0)
            {
                var errorList = (from msg in Data.ErrorListToAlert select msg.ErrorMessage).ToList();
                Bus.Send<SendErrorAlert>(m =>
                {
                    m.ErrorList = errorList;
                });
                IncrementAlertCount(Data.ErrorListToAlert);
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), "state");
            }
            else
            {
                Data.IsTimeoutAlreadyRequested = false;
            }
        }

        private void ClearAlertForMessage(string id)
        {
            // Messages (New Error Message Received and or ErrorMessageDeleted or reprocessed), can 
            // be processed out of sequence. If the error message deleted arrives first, this code
            // will exception and the message will be sent back to the queue, giving the message received
            // to cause it to get added in the list first. After which the code will succeed.

            var messageToRemove = (from msg in Data.ErrorListToAlert
                    where msg.MessageId.Equals(id) ||
                    msg.ErrorMessage.OriginalMessageId.Equals(id)
                    select msg).First();
            
            Data.ErrorListToAlert.Remove(messageToRemove);
            
        }

        private void IncrementAlertCount(List<ErrorAlertInfo> list)
        {
            foreach (ErrorAlertInfo info in list)
            {
                info.NumberOfTimesAlerted++;
            }
        }
    }
}
