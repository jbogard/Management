using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Alerter.Messages;

namespace NServiceBus.Management.Errors.Alerter
{
    public class ThresholdLimitReachedAlertSaga : Saga<ThresholdLimitReachedSagaData>,
        IAmStartedByMessages<ProcessErrorMessageReceived>,
        IAmStartedByMessages<ProcessErrorMessageReprocessed>,
        IAmStartedByMessages<ProcessErrorMessageDeleted>

    {
        public int MaxTimesToAlert { get; set; }
        public Dictionary<string, int> CriticalErrorLimits { get; set; }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<ProcessErrorMessageReceived>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
            ConfigureMapping<ProcessErrorMessageDeleted>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
            ConfigureMapping<ProcessErrorMessageReprocessed>(s => s.AlerterInstanceId, m => m.AlerterInstanceId);
        }

        public void Handle(ProcessErrorMessageReceived message)
        {
            Console.WriteLine("Saga is started");
            Data.AlerterInstanceId = message.AlerterInstanceId;

            // Add to the list
            Data.AddAlertForMessage(message.MessageDetails.OriginalMessageId, message.MessageDetails.ErrorReceivedTime);
            EvaluateCriticalAlert();

            Console.WriteLine("Saga is started");
        }

        public void Handle(ProcessErrorMessageReprocessed message)
        {
            Data.WriteInfo(string.Format("ProcessErrorMessageReprocessed arrived. MessageId: {0}", message.MessageDetails.MessageId));
            Data.AlerterInstanceId = message.AlerterInstanceId;
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
            Data.AlerterInstanceId = message.AlerterInstanceId;

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
            foreach (string ruleId in CriticalErrorLimits.Keys)
            {
                if (!Data.TimesAlerted.ContainsKey(ruleId))
                {
                    Data.TimesAlerted.Add(ruleId, 0);
                }

                if (Data.ErrorsToAlert.Count >= CriticalErrorLimits[ruleId])
                {
                    Console.WriteLine("Current error count has exceeded critical count");
                    
                    // Make sure you respect the max times to alert property to avoid spamming!
                    if (Data.TimesAlerted[ruleId] < MaxTimesToAlert)
                    {
                        Bus.SendLocal<SendCriticalErrorLimitReachedAlert>(m =>
                        {
                            m.RuleId = ruleId;
                            m.Count = Data.ErrorsToAlert.Count;
                        });

                        // Increment the alert count
                        Data.TimesAlerted[ruleId]++;
                    }
                }
                else
                {
                    Console.WriteLine("Current error count has NOT exceeded critical count, Clearing the notifications sent to 0.");
                    // Make sure to reset the count, so we will alert next time the threshold drops and increases.
                    Data.TimesAlerted[ruleId] = 0;
                }
            }
        }
    }
}
