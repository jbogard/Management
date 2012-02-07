using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Messages;
using log4net;

namespace NServiceBus.Management.Errors.Alerter
{
    public class AlerterSummarySagaData : ISagaEntity
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }

        [Unique]
        public Guid AlerterInstanceId { get; set; }

        public bool IsTimeoutAlreadyRequested { get; set; }

        public Dictionary<string, DateTime> errorsToBeCleared;
        public Dictionary<string, ErrorAlertInfo> errorsToAlert;
        
        public AlerterSummarySagaData()
        {
            errorsToAlert = new Dictionary<string, ErrorAlertInfo>();
            errorsToBeCleared = new Dictionary<string, DateTime>();
            
        }

        public List<ErrorAlertInfo> ErrorListToAlert
        {
            get { return errorsToAlert.Values.ToList<ErrorAlertInfo>(); }
        }

        public Dictionary<string, DateTime> ErrorsToBeCleared
        {
            get { return errorsToBeCleared; }
            set { errorsToBeCleared = value; }
        }

        public void ClearAlertForMessage(string originalMessageId, DateTime timeReceived)
        {
            // If its not in either list, just add to list, the message will be sent back to the queue.
            
            // Not in the to be alerted list, but in the cleared list
            if (!errorsToAlert.ContainsKey(originalMessageId) && errorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > errorsToBeCleared[originalMessageId])
                {
                    // Update the list
                    errorsToBeCleared[originalMessageId] = timeReceived;
                    WriteInfo(string.Format("ClearAlertForMessage: Updating {0} in cleared dictionary", originalMessageId)); 
                
                    return;
                }
            }

            // if the error is in the alert list and not in the cleared list
            if (errorsToAlert.ContainsKey(originalMessageId) && !errorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (timeReceived > errorsToAlert[originalMessageId].ErrorMessage.ErrorReceivedTime)
                {
                    WriteInfo(string.Format("ClearAlertForMessage: Removing {0} from alert dictionary", originalMessageId)); 
                    errorsToAlert.Remove(originalMessageId);
                    return;
                }
            }

            WriteInfo(string.Format("ClearAlertForMessage: *** {0} IN BOTH DICTIONARIES ***", originalMessageId)); 
            

            // This is in both lists! Is this even possible????!!
            if (timeReceived > errorsToBeCleared[originalMessageId])
            {
                errorsToBeCleared[originalMessageId] = timeReceived;
            }
            if (timeReceived > errorsToAlert[originalMessageId].ErrorMessage.ErrorReceivedTime)
            {
                errorsToAlert.Remove(originalMessageId);
            }
        }

        public void AddAlertForMessage(ErrorAlertInfo errorAlertInfo)
        {
            var originalMessageId = errorAlertInfo.ErrorMessage.OriginalMessageId;
            // If its not in either list, just add to list
            if (!errorsToAlert.ContainsKey(originalMessageId) && !errorsToBeCleared.ContainsKey(originalMessageId))
            {
                WriteInfo(string.Format("AddAlertForMessage: Error not in dictionary. Adding {0} to alert dictionary", originalMessageId)); 
                errorsToAlert.Add(originalMessageId, errorAlertInfo);
                return;
            }

            // Not in the to be alerted list, but in the cleared list
            if (!errorsToAlert.ContainsKey(originalMessageId) && errorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (errorAlertInfo.ErrorMessage.ErrorReceivedTime > errorsToBeCleared[originalMessageId])
                {
                    // Remove from the cleared list, add to the to be alerted list
                    errorsToBeCleared.Remove(originalMessageId);
                    errorsToAlert.Add(originalMessageId, errorAlertInfo);
                    WriteInfo(string.Format("AddAlertForMessage: Removing {0} to from cleared dictionary and adding it to the Alert dictionary", originalMessageId)); 
                
                    return;
                }
            }

            // if the error is in the alert list and not in the cleared list
            if (errorsToAlert.ContainsKey(originalMessageId) && !errorsToBeCleared.ContainsKey(originalMessageId))
            {
                if (errorAlertInfo.ErrorMessage.ErrorReceivedTime > errorsToAlert[originalMessageId].ErrorMessage.ErrorReceivedTime)
                {
                    errorsToAlert[originalMessageId] = errorAlertInfo;
                    WriteInfo(string.Format("AddAlertForMessage: Updating {0} in alert dictionary", originalMessageId)); 
                
                    return;
                }
            }

            WriteInfo(string.Format("AddAlertForMessage: *** {0} IN BOTH DICTIONARIES ***", originalMessageId)); 
                
            // This is in both lists! Is this even possible????!!
            if (errorAlertInfo.ErrorMessage.ErrorReceivedTime > errorsToBeCleared[originalMessageId])
            {
                errorsToBeCleared.Remove(originalMessageId);
            }
            if (errorAlertInfo.ErrorMessage.ErrorReceivedTime > errorsToAlert[originalMessageId].ErrorMessage.ErrorReceivedTime)
            {
                errorsToAlert[originalMessageId] = errorAlertInfo;
            }
        }

        public bool IsErrorInAlertList(string id)
        {
            return errorsToAlert.ContainsKey(id);
        }

        public bool IsErrorInToBeClearedList(string id)
        {
            return errorsToBeCleared.ContainsKey(id);
        }

        // Not exactly sure, if this will be useful or not. Does it make sense to hold on to how many times a particular error
        // was alerted before taking action?
        public void IncrementAlertCount()
        {
            foreach (ErrorAlertInfo info in errorsToAlert.Values)
            {
                info.NumberOfTimesAlerted++;
            }
        }

        public void WriteInfo(string message)
        {
            string logMsg = string.Format("SagaId: {0}, ThreadName: {1}, DateTime: {2}, Message: {3}", this.Id, System.Threading.Thread.CurrentThread.Name, DateTime.Now, message);
            Console.WriteLine(logMsg);
            Logger.Info(logMsg);
        }
    }
}
