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
        IHandleMessages<ProcessErrorMessageDeleted>,
        IHandleMessages<ProcessErrorMessageReprocessed>
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
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), null);
            }

            // Add to the list
            ErrorAlertInfo alertInfo = new ErrorAlertInfo() { MessageId = message.MessageDetails.FailedMessageId, ErrorMessage = message.MessageDetails, NumberOfTimesAlerted = 0 };
            Data.ErrorListToAlert.Add(alertInfo);
        }

        public void Handle(ProcessErrorMessageDeleted message)
        {
            ClearAlertForMessage(message.MessageDetails.MessageId);
        }

        public void Handle(ProcessErrorMessageReprocessed message)
        {
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
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), null);
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
            }

            // Request another timeout if we have errors
            if (Data.ErrorListToAlert.Count > 0)
            {
                RequestUtcTimeout(TimeSpan.FromSeconds(TimeToWaitBeforeAlerting), null);
            }
            else
            {
                Data.IsTimeoutAlreadyRequested = false;
                // So when we get a new error, we start requesting timeouts again.
            }
        }

        private void ClearAlertForMessage(string id)
        {
            var messageToRemove = (from msg in Data.ErrorListToAlert
                    where msg.MessageId.Equals(id)
                    select msg).FirstOrDefault();
            // Multiple clients could send the Delete command. If it so happens that the GUI hasn't been updated.
            // The command should succeed. 
            if (messageToRemove != null)
            {
                // Remove from the alert list
                Data.ErrorListToAlert.Remove(messageToRemove);
            }
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
