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
        public IBus Bus { get; set; }
        public IPersistErrorMessages ErrorPersister { get; set; }
        //public INotifyOnError ErrorNotifier { get; set; }

        public void Handle(ProcessErrorMessage message)
        {
           
            ErrorMessageReceived errorEvent = Bus.CreateInstance<ErrorMessageReceived>(m =>
            {
                m.FailedMessageId = message.FailedMessageId;
                m.OriginalMessageId = message.OriginalMessageId;
                m.AdditionalInformation = message.AdditionalInformation;
                m.ProcessingFailedAddress = message.ProcessingFailedAddress;
                m.TimeSent = message.TimeSent;
                m.WindowsIdentity = message.WindowsIdentity;
                m.XmlBody = message.XmlBody;
                m.ExceptionInformation = message.ExceptionInformation;
            });

            // Save the error in the persistent store
            ErrorPersister.SaveErrorMessage(errorEvent);

            // Publish event
            Bus.Publish(errorEvent);

        }
    }
}
