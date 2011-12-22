using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using System.Xml;

namespace NServiceBus.Management.Errors.Monitor.MessageHandlers
{
    class ProcessErrorMessageHandler : IHandleMessages<ProcessErrorMessage>
    {
        public IBus Bus { get; private set; }
        public IPersistErrorMessages ErrorPersister { get; set; }
        //public INotifyOnError ErrorNotifier { get; set; }

        public void Handle(ProcessErrorMessage message)
        {
            // Get the xml content of the message that failed
            var doc = new XmlDocument();
            var messageBodyXml = message.XmlBody;

            // Get the header list as a key value dictionary...
            Dictionary<string, string> headerDictionary = message.HeaderList;

            ErrorMessageReceived errorEvent = Bus.CreateInstance<ErrorMessageReceived>(m =>
            {
                m.FailedMessageId = message.FailedMessageId;
                m.HeaderList = headerDictionary;
                m.ProcessingFailedAddress = message.ProcessingFailedAddress;
                m.TimeSent = message.TimeSent;
                m.WindowsIdentity = message.WindowsIdentity;
                m.XmlBody = messageBodyXml;
            });

            // Save the error in the persistent store
            ErrorPersister.SaveErrorMessage(errorEvent);

            // Notify about the error
            //ErrorNotifier.NotifyOnError(errorEvent);

            // Publish event
            Bus.Publish(errorEvent);

        }
    }
}
