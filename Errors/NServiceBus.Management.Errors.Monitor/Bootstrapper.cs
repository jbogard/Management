using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Unicast.Transport;
using NServiceBus.Management.Errors.Messages;
using System.Xml;
using NServiceBus.Unicast.Transport.Msmq;
using System.Configuration;


namespace NServiceBus.Management.Errors.Monitor
{
    class Bootstrapper : IWantToRunAtStartup
    {
        #region IWantToRunAtStartup Members
        public MsmqTransport ErrorMonitorTransport { get; private set; }
        public MsmqTransport ErrorMessageTransport = new MsmqTransport();
        public IBus Bus { get; private set; }
        public void Run()
        {
            // Create an in memory transport which will listen for messages on the error queue.
            // When a message is received in the error queue, the TransportMessageReceived event handler 
            // will handle the message. The contents of the message from the error queue will be inspected
            // and a ProcessErrorMessage will be sent using the Bus.

            ErrorMessageTransport.InputQueue = ConfigurationManager.AppSettings["ErrorQueueToMonitor"];
            ErrorMessageTransport.IsTransactional = ErrorMonitorTransport.IsTransactional;
            ErrorMessageTransport.MaxRetries = ErrorMonitorTransport.MaxRetries;
            ErrorMessageTransport.NumberOfWorkerThreads = ErrorMonitorTransport.NumberOfWorkerThreads;
            ErrorMessageTransport.SkipDeserialization = true;
            ErrorMessageTransport.ForwardReceivedMessagesTo = string.Format("{0}.Storage",ErrorMessageTransport.InputQueue);
            ErrorMessageTransport.TransportMessageReceived += new EventHandler<TransportMessageReceivedEventArgs>(ErrorMessageTransport_TransportMessageReceived);

            // Start the transport and start listening for messages in the error queue.
            ErrorMessageTransport.Start();
            
        }

        void ErrorMessageTransport_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            var message = e.Message;

            if (e.Message.MessageIntent == MessageIntentEnum.Subscribe || e.Message.MessageIntent == MessageIntentEnum.Unsubscribe)
            {
                // Ignore this message. Could be CompletionMessages.
                return;
            }

            // Get the xml content of the message that failed
            var doc = new XmlDocument();
            doc.Load(message.BodyStream);
            var messageBodyXml = doc.InnerXml;

            // Get the header list as a key value dictionary...
            Dictionary<string, string> headerDictionary = message.Headers.ToDictionary(k => k.Key, v => v.Value);

            // Send a command to the processing endpoint.
            Bus.Send<ProcessErrorMessage>(m =>
            {
                m.FailedMessageId = message.Id;
                m.ProcessingFailedAddress = message.ProcessingFailedAddress;
                m.TimeSent = message.TimeSent;
                m.WindowsIdentity = message.WindowsIdentityName;
                m.HeaderList = headerDictionary;
                m.XmlBody = messageBodyXml;
            });
        }

        public void Stop()
        {
            ErrorMessageTransport.TransportMessageReceived -= new EventHandler<TransportMessageReceivedEventArgs>(ErrorMessageTransport_TransportMessageReceived);
        }

        #endregion

    }
}
