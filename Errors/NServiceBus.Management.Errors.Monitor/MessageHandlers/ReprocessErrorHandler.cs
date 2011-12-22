using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using System.Messaging;
using System.Transactions;
using NServiceBus.Unicast.Transport.Msmq;
using System.Configuration;
using NServiceBus.Utils;

namespace NServiceBus.Management.Errors.Monitor.MessageHandlers
{
    class ReprocessErrorHandler : IHandleMessages<ReprocessErrorMessage>
    {
        private ErrorManager errorManager = new ErrorManager();
        public IPersistErrorMessages ErrorPersister { get; private set; }
        public IBus Bus { get; private set; }
        
        public void Handle(ReprocessErrorMessage messageToReprocess)
        {
            errorManager.InputQueue = string.Format("{0}.Storage", ConfigurationManager.AppSettings["ErrorQueueToMonitor"]);
            errorManager.ReturnMessageToSourceQueue(messageToReprocess.MessageId);

            // Get rid of message in the storage queue
            errorManager.RemoveMessage(messageToReprocess.MessageId);

            // Remove message from the persistent store.
            ErrorPersister.DeleteErrorMessage(messageToReprocess.MessageId);

            // Publish event
            Bus.Publish<ErrorMessageReprocessed>(m => { m.MessageId = messageToReprocess.MessageId; });
            
        }
    }
}
