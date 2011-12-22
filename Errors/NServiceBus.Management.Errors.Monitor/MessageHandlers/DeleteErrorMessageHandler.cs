using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using System.Configuration;

namespace NServiceBus.Management.Errors.Monitor.MessageHandlers
{
    class DeleteErrorMessageHandler : IHandleMessages<DeleteErrorMessage>
    {
        public IBus Bus { get; private set; }
        public IPersistErrorMessages ErrorPersister { get; private set; }
        
        private ErrorManager errorManager = new ErrorManager();

        public void Handle(DeleteErrorMessage message)
        {
            // Get rid of message in the storage queue
            errorManager.InputQueue = string.Format("{0}.Storage", ConfigurationManager.AppSettings["ErrorQueueToMonitor"]);
            errorManager.RemoveMessage(message.MessageId);

            // Get rid of message in the persistent store.
            ErrorPersister.DeleteErrorMessage(message.MessageId);

            // Publish event
            Bus.Publish<ErrorMessageDeleted>(m => { m.MessageId = message.MessageId; });
            
        }
    }
}
