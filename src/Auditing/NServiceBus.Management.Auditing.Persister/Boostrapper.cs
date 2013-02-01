using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NServiceBus.Unicast.Transport;
using NServiceBus.Unicast.Transport.Transactional;
using NServiceBus.Unicast.Queuing.Msmq;
using System.Messaging;
using NServiceBus.Utils;
using System.Configuration;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Raven.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace NServiceBus.Management.Auditing.Persister
{
    class Boostrapper : IWantToRunAtStartup
    {
        public IBus Bus { get; set; }
        public ITransport AuditMessageQueueTransport;
        public TransactionalTransport CurrentEndpointTransport { get; set; }
        public IPersistAuditMessages Persister { get; set; }

        public void Run()
        {
            // Create an in-memory TransactionalTransport and point it to the AuditQ
            // Serialize data into the database.

            string auditQueue = ConfigurationManager.AppSettings["AuditQueue"];
            string machineName = Environment.MachineName;

            // Make sure that the queue being monitored, exists!
            if (!MessageQueue.Exists(MsmqUtilities.GetFullPathWithoutPrefix(auditQueue)))
            {
                // The error queue being monitored must be local to this endpoint
                throw new Exception(string.Format("The audit queue {0} being monitored must be local to this endpoint and must exist. Make sure a transactional queue by the specified name exists. The audit queue to be monitored is specified in the app.config", auditQueue));
            }

            // Create an in-memory transport with the same configuration as that of the current endpoint.
            AuditMessageQueueTransport = new TransactionalTransport()
            {
                IsTransactional = CurrentEndpointTransport.IsTransactional,
                MaxRetries = CurrentEndpointTransport.MaxRetries,
                IsolationLevel = CurrentEndpointTransport.IsolationLevel,
                MessageReceiver = new MsmqMessageReceiver(),
                NumberOfWorkerThreads = CurrentEndpointTransport.NumberOfWorkerThreads,
                TransactionTimeout = CurrentEndpointTransport.TransactionTimeout,
                FailureManager = CurrentEndpointTransport.FailureManager
            };

            AuditMessageQueueTransport.Start(new Address(auditQueue, machineName));
            AuditMessageQueueTransport.TransportMessageReceived += new EventHandler<TransportMessageReceivedEventArgs>(AuditMessageQueueTransport_TransportMessageReceived);
        }

        public void Stop()
        {
            AuditMessageQueueTransport.TransportMessageReceived -= new EventHandler<TransportMessageReceivedEventArgs>(AuditMessageQueueTransport_TransportMessageReceived);
        }

        void AuditMessageQueueTransport_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            var message = e.Message;

            // Get the xml content of the message in the audit Q that's being stored.
            //var doc = new XmlDocument();
            //doc.Load(new MemoryStream(message.Body));
            //var messageBodyXml = doc.InnerXml;


            var messageBodyXml = System.Text.Encoding.UTF8.GetString(message.Body);
            RavenJObject jsonBody;
            
            if (messageBodyXml.StartsWith("["))
            {
                jsonBody = (RavenJObject) RavenJArray.Load(new JsonTextReader(new StreamReader(new MemoryStream(message.Body)))).Values().First();
            }
            else
            {
                var doc = new XmlDocument();
                doc.Load(new MemoryStream(message.Body));
                var firstMessageNode = doc.DocumentElement.ChildNodes.Cast<XmlNode>().First();
                var convertedJson = JsonConvert.SerializeXmlNode(firstMessageNode, Formatting.Indented, true);
                jsonBody = RavenJObject.Parse(convertedJson);
            }

            // Get the header list as a key value dictionary...
            Dictionary<string, string> headerDictionary = message.Headers.ToDictionary(k => k.Key, v => v.Value);

            var enclosedMessageType = headerDictionary["NServiceBus.EnclosedMessageTypes"];
            string messageType = enclosedMessageType;
            if (!String.IsNullOrWhiteSpace(enclosedMessageType))
            {
                messageType = enclosedMessageType.Split(new char[] { ',' }, StringSplitOptions.None)[0];
            }

            AuditMessage messageToStore = new AuditMessage
            {
                MessageId = message.Id,
                OriginalMessageId = message.GetOriginalId(),
                OriginalBody = messageBodyXml,
                Body = jsonBody,
                Headers = headerDictionary,
                ReplyToAddress = message.ReplyToAddress.ToString(),
                MessageType = messageType,
                ReceivedTime = DateTime.ParseExact(headerDictionary["NServiceBus.TimeSent"], "yyyy-MM-dd HH:mm:ss:ffffff Z", System.Globalization.CultureInfo.InvariantCulture)
            };

            // Save the message
            Console.WriteLine("Saving {0}", messageType);
            Persister.Persist(messageToStore);

        }

    }
}
