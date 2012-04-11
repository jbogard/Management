using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using log4net;

namespace ThresholdLimitReached
{
    public class SagaData : ISagaEntity
    {
        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }

        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Unique]
        public string RuleId { get; set; }
           
        public Dictionary<string, DateTime> ErrorsToBeCleared;
        public Dictionary<string, DateTime> ErrorsToAlert;

        public int TimesAlerted;
        
        public int MaxTimesToAlert { get; set; }
        public int CriticalErrorLimit { get; set; }

        public SagaData()
        {
            ErrorsToAlert = new Dictionary<string, DateTime>();
            ErrorsToBeCleared = new Dictionary<string, DateTime>();
        }

        /// <summary>
        ///  ErrorsToAlert         ErrorsToBeCleared      Action
        ///    F                         F               Add to the ErrorsToAlert dictionary
        ///    F                         T               Only add to the ErrorsToAlert, if the timestamp of this error is more current, and also remove from cleared if that's teh case
        ///    T                         F               Update the timestamp in the ErrorsToAlert, if the new error is more current.
        ///    T                         T               This should generate a warning!, Can happen when you are running it multi threaded.
        /// </summary>
        /// <param name="originalMessageId"></param>
        /// <param name="timeReceived"></param>
        public void AddAlertForMessage(string originalMessageId, DateTime timeReceived)
        {
            // If its not in either list, just add to list
            if (!ErrorsToAlert.ContainsKey(originalMessageId) && !ErrorsToBeCleared.ContainsKey(originalMessageId))
            {
                WriteInfo(string.Format("AddAlertForMessage: Error not in dictionary. Adding {0} to alert dictionary", originalMessageId));
                ErrorsToAlert.Add(originalMessageId, timeReceived);
                return;
            }

            // Not in the to be alerted list yet, but in the cleared list
            if (!ErrorsToAlert.ContainsKey(originalMessageId) && ErrorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > ErrorsToBeCleared[originalMessageId])
                {
                    // Remove from the cleared list, add to the to be alerted list
                    ErrorsToBeCleared.Remove(originalMessageId);
                    ErrorsToAlert.Add(originalMessageId, timeReceived);
                    WriteInfo(string.Format("AddAlertForMessage: Removing {0} to from cleared dictionary and adding it to the Alert dictionary", originalMessageId));
                    return;
                }
            }

            // if the error is in the alert list and not in the cleared list
            if (ErrorsToAlert.ContainsKey(originalMessageId) && !ErrorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > ErrorsToAlert[originalMessageId])
                {
                    ErrorsToAlert[originalMessageId] = timeReceived;
                    WriteInfo(string.Format("AddAlertForMessage: Updating {0} in alert dictionary", originalMessageId));

                    return;
                }
            }

            // The only time you might run into this situation is when you run this saga as multi threaded. This saga is not
            // designed to be run multithreaded. As all of the messages go into the same saga store, it will create a lot of 
            // contention.
            WriteWarn(string.Format("AddAlertForMessage: *** {0} IN BOTH DICTIONARIES ***, Are you running this saga as multi threaded? This saga is not designed to be run multithreaded. As all of the messages go into the same saga store, it will create a lot of contention.", originalMessageId));
        }

        /// <summary>
        ///  ErrorsToAlert         ErrorsToBeCleared      Action
        ///    F                         F               HandleCurrentMessageLater --> This function wont be invoked. The only way this function will be called is if one of the dictionary's has the original message Id
        ///    F                         T               Update the timestamp if the error just received is more current.
        ///    T                         F               if the timestamp is more recent than the one in hte ErrorsToAlert, remove the entry from the ErrorsToAlert.
        ///    T                         T               This should generate a warning!, Can happen when you are running it multi threaded.
        /// </summary>
        /// <param name="originalMessageId"></param>
        /// <param name="timeReceived"></param>
        public void ClearAlertForMessage(string originalMessageId, DateTime timeReceived)
        {

            // Not in the to be alerted list, but in the cleared list
            if (!ErrorsToAlert.ContainsKey(originalMessageId) && ErrorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > ErrorsToBeCleared[originalMessageId])
                {
                    // Update the time received, as this one is newer than what we already have.
                    ErrorsToBeCleared[originalMessageId] = timeReceived;
                    WriteInfo(string.Format("ClearAlertForMessage: Updating {0} in cleared dictionary", originalMessageId));

                    return;
                }
            }

            // if the error is in the alert list and not in the cleared list
            if (ErrorsToAlert.ContainsKey(originalMessageId) && !ErrorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > ErrorsToAlert[originalMessageId])
                {
                    // We can safely remove the error in the alert list, as we have reprocessed or deleted event whose time stamp is greater than the error originally received.
                    WriteInfo(string.Format("ClearAlertForMessage: Removing {0} from alert dictionary", originalMessageId));
                    ErrorsToAlert.Remove(originalMessageId);
                    return;
                }
            }

            // The only time you might run into this situation is when you run this saga as multi threaded. This saga is not
            // designed to be run multithreaded. As all of the messages go into the same saga store, it will create a lot of 
            // contention.
            WriteWarn(string.Format("ClearAlertForMessage: *** {0} IN BOTH DICTIONARIES ***, Are you running this saga as multi threaded? This saga is not designed to be run multithreaded. As all of the messages go into the same saga store, it will create a lot of contention.", originalMessageId));
        }

        public void WriteInfo(string message)
        {
            string logMsg = string.Format("SagaId: {0}, ThreadName: {1}, DateTime: {2}, Message: {3}", this.Id, System.Threading.Thread.CurrentThread.Name, DateTime.Now, message);
            Console.WriteLine(logMsg);
            Logger.Info(logMsg);
        }

        public void WriteWarn(string message)
        {
            string logMsg = string.Format("SagaId: {0}, ThreadName: {1}, DateTime: {2}, Message: {3}", this.Id, System.Threading.Thread.CurrentThread.Name, DateTime.Now, message);
            Console.WriteLine(logMsg);
            Logger.Warn(logMsg);
        }

        public bool ShouldThisMessageBeProcessedLater(string messageId)
        {
            return (!(ErrorsToAlert.ContainsKey(messageId) | ErrorsToBeCleared.ContainsKey(messageId)));
        }
    }
}
