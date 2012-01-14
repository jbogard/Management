using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Unicast.Transport;
using NServiceBus.Management.Errors.Messages;
using System.Xml;
using NServiceBus.Unicast.Transport.Transactional;
using System.Configuration;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Queuing.Msmq;
using System.Net;
using System.IO;
using NServiceBus.ObjectBuilder;
using NServiceBus.Unicast;
using NServiceBus.Utils;
using System.Messaging;


namespace NServiceBus.Management.Errors.Monitor
{
    class Bootstrapper : IWantToRunAtStartup
    {
        #region IWantToRunAtStartup Members
        public TransactionalTransport ErrorMonitorTransport { get; set; }
        public ITransport ErrorMessageTransport;
        public IBus Bus { get; set; }
        public IBuilder Builder { get; set; }
        public ISendMessages MessageForwarder { get; set; }
        private Address errorStorageQueueAddress;

        public void Run()
        {
            // Create an in memory transport which will listen for messages on the error queue.
            // When a message is received in the error queue, the TransportMessageReceived event handler 
            // will handle the message. The contents of the message from the error queue will be inspected
            // and a ProcessErrorMessage will be sent using the Bus.
            string errorQueue = ConfigurationManager.AppSettings["ErrorQueueToMonitor"];
            string machineName = Environment.MachineName;

            // Make sure that the queue being monitored, exists!
            if (!MessageQueue.Exists(MsmqUtilities.GetFullPathWithoutPrefix(errorQueue)))
            {
                // The error queue being monitored must be local to this endpoint
                throw new Exception(string.Format("The error queue {0} being monitored must be local to this endpoint and must exist. Make sure a transactional queue by the specified name exists. The error queue to be monitored is specified in the app.config", errorQueue));
            }

            errorStorageQueueAddress = new Address(string.Format("{0}.Storage", errorQueue), machineName);

            ErrorMessageTransport = new TransactionalTransport()
            {
                IsTransactional = ErrorMonitorTransport.IsTransactional,
                MaxRetries = ErrorMonitorTransport.MaxRetries,
                IsolationLevel = ErrorMonitorTransport.IsolationLevel,
                MessageReceiver = new MsmqMessageReceiver(),
                NumberOfWorkerThreads = ErrorMonitorTransport.NumberOfWorkerThreads,
                TransactionTimeout = ErrorMonitorTransport.TransactionTimeout,
                FailureManager = ErrorMonitorTransport.FailureManager
            };          

            ErrorMessageTransport.Start(new Address(errorQueue, machineName));
            ErrorMessageTransport.TransportMessageReceived +=new EventHandler<TransportMessageReceivedEventArgs>(ErrorMessageTransport_TransportMessageReceived);
        }

        void ErrorMessageTransport_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            var message = e.Message;

            if (e.Message.MessageIntent == MessageIntentEnum.Subscribe || e.Message.MessageIntent == MessageIntentEnum.Unsubscribe)
            {
                // Ignore this message. Could be CompletionMessages.
                return;
            }

            // Forward this message to the storage queue.
            MessageForwarder.Send(message, errorStorageQueueAddress);


            // Get the xml content of the message that failed
            var doc = new XmlDocument();
            doc.Load(new MemoryStream(message.Body));
            var messageBodyXml = doc.InnerXml;

            // Get the header list as a key value dictionary...
            Dictionary<string, string> headerDictionary = message.Headers.ToDictionary(k => k.Key, v => v.Value);

            var processingFailedAddress = headerDictionary["NServiceBus.FailedQ"];
            var windowsIdentity = headerDictionary["WinIdName"];
            var originalId = headerDictionary["NServiceBus.OriginalId"];

            // Promoting the Processing Failed address, Windows Identity and the OriginalId from the dictionary 
            // to the main interface to provide more clarity about the error.
            headerDictionary.Remove("NServiceBus.FailedQ");
            headerDictionary.Remove("WinIdName");
            headerDictionary.Remove("NServiceBus.OriginalId");

            var exceptionInfo = string.Format("{0} - {1} {2}", headerDictionary["NServiceBus.ExceptionInfo.ExceptionType"],
                headerDictionary["NServiceBus.ExceptionInfo.Message"],
                headerDictionary["NServiceBus.ExceptionInfo.StackTrace"]);
            
            // Send a command to the processing endpoint.
            Bus.Send<ProcessErrorMessage>(m =>
            {
                m.FailedMessageId = message.Id;
                m.OriginalMessageId = originalId;
                m.ProcessingFailedAddress = processingFailedAddress;
                m.TimeSent = DateTime.ParseExact(headerDictionary["NServiceBus.TimeSent"], "yyyy-MM-dd HH:mm:ss:ffffff Z", System.Globalization.CultureInfo.InvariantCulture);
                m.WindowsIdentity = windowsIdentity; 
                m.AdditionalInformation = headerDictionary;
                m.XmlBody = messageBodyXml;
                m.ExceptionInformation = exceptionInfo;
            });

        }

        public void Stop()
        {
            ErrorMessageTransport.TransportMessageReceived -= new EventHandler<TransportMessageReceivedEventArgs>(ErrorMessageTransport_TransportMessageReceived);
        }

        #endregion

    }
}
