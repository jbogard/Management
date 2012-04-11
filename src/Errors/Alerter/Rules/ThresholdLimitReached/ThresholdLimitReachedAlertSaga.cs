using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Alerter.Messages;
using NServiceBus;

namespace ThresholdLimitReached
{
    public class ThresholdSaga : Saga<SagaData>,
        IAmStartedByMessages<EvaluateThresholdLimitExceededAlertRule>,
        IHandleMessages<ProcessErrorMessageReceived>,
        IHandleMessages<ProcessErrorMessageReprocessed>,
        IHandleMessages<ProcessErrorMessageDeleted>
    {
        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<EvaluateThresholdLimitExceededAlertRule>(s => s.RuleId, m => m.RuleId);
            ConfigureMapping<ProcessErrorMessageReceived>(s => s.RuleId, m => m.RuleId);
            ConfigureMapping<ProcessErrorMessageDeleted>(s => s.RuleId, m => m.RuleId);
            ConfigureMapping<ProcessErrorMessageReprocessed>(s => s.RuleId, m => m.RuleId);
        }

        public void Handle(EvaluateThresholdLimitExceededAlertRule message)
        {
            Data.RuleId = message.RuleId;
            Data.MaxTimesToAlert = message.MaxTimesToAlert;
            Data.CriticalErrorLimit = message.CriticalErrorLimit;
        }

        public void Handle(ProcessErrorMessageReceived message)
        {
            Console.WriteLine("Saga is started");
            
            // Add to the list
            Data.AddAlertForMessage(message.MessageDetails.OriginalMessageId, message.MessageDetails.ErrorReceivedTime);
            EvaluateCriticalAlert();

            Console.WriteLine("Saga is started");
        }

        public void Handle(ProcessErrorMessageReprocessed message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageReprocessed arrived. MessageId: {0}", message.MessageDetails.MessageId));
            
            if (Data.ShouldThisMessageBeProcessedLater(message.MessageDetails.MessageId))
            {
                Data.WriteInfo(string.Format("Going to handle this message later: MessageId: {0}", message.MessageDetails.MessageId));
                Bus.HandleCurrentMessageLater();
            }
            else
            {
                Data.ClearAlertForMessage(message.MessageDetails.MessageId, message.MessageDetails.ErrorReprocessedTime);
            }
        }

        public void Handle(ProcessErrorMessageDeleted message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageDeleted arrived. MessageId: {0}", message.MessageDetails.MessageId));
            
            if (Data.ShouldThisMessageBeProcessedLater(message.MessageDetails.MessageId))
            {
                Data.WriteInfo(string.Format("Going to handle this message later: MessageId: {0}", message.MessageDetails.MessageId));
                Bus.HandleCurrentMessageLater();
            }
            else
            {
                Data.ClearAlertForMessage(message.MessageDetails.MessageId, message.MessageDetails.ErrorDeletedTime);
            } 
        }

        private void EvaluateCriticalAlert()
        {
            if (Data.ErrorsToAlert.Count  >= Data.CriticalErrorLimit)
            {
                Console.WriteLine("Current error count has exceeded critical count");
                    
                // Make sure you respect the max times to alert property to avoid spamming!
                if (Data.TimesAlerted < Data.MaxTimesToAlert)
                {
                    Bus.SendLocal<SendCriticalErrorLimitReachedAlert>(m =>
                    {
                        m.RuleId = Data.RuleId;
                        m.Count = Data.ErrorsToAlert.Count;
                    });

                    // Increment the alert count
                    Data.TimesAlerted++;
                }
            }
            else
            {
                Console.WriteLine("Current error count has NOT exceeded critical count, Clearing the notifications sent to 0.");
                // Make sure to reset the count, so we will alert next time the threshold drops and increases.
                Data.TimesAlerted = 0;
            }
        }
    }
}
