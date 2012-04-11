
namespace NServiceBus.Management.Errors.Monitor
{
    using System;
    using System.Messaging;
    using Utils;
    using System.Transactions;
    using NServiceBus.Unicast.Transport;


    public class DeleteMessageFromQueueHelper 
    {
        private const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";
        private const string NoMessageFoundErrorFormat = "INFO: No message found with ID '{0}'. Going to check headers of all messages for one with that original ID.";
        private MessageQueue queue;
        private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);
        public bool ClusteredQueue { get; set; }
        /// <summary>
        /// Constant taken from V2.6: 
        /// https://github.com/NServiceBus/NServiceBus/blob/v2.5/src/impl/unicast/NServiceBus.Unicast.Msmq/MsmqTransport.cs
        /// </summary>
        private const string FAILEDQUEUE = "FailedQ";

        public virtual Address InputQueue
        {
            set
            {
                var path = MsmqUtilities.GetFullPath(value);
                var q = new MessageQueue(path);

                if ((!ClusteredQueue) && (!q.Transactional))
                    throw new ArgumentException(string.Format(NonTransactionalQueueErrorMessageFormat, q.Path));

                queue = q;

                var mpf = new MessagePropertyFilter();
                mpf.SetAll();

                queue.MessageReadPropertyFilter = mpf;
            }
        }     

        /// <summary>
        /// Delete message from the storage queue.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public void DeleteMessageFromSourceQueue(string messageId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required)) 
            {
                try
                {
                    var message = queue.ReceiveById(messageId, TimeoutDuration, MessageQueueTransactionType.Automatic);
                    Console.WriteLine("Success.");
                    scope.Complete();
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        Console.WriteLine(NoMessageFoundErrorFormat, messageId);

                        foreach (var m in queue.GetAllMessages())
                        {
                            var tm = MsmqUtilities.Convert(m);

                            if (tm.Headers.ContainsKey(TransportHeaderKeys.OriginalId))
                            {
                                if (messageId != tm.Headers[TransportHeaderKeys.OriginalId])
                                    continue;

                                Console.WriteLine("Found message - going to delete");

                                using (var tx = new TransactionScope(TransactionScopeOption.Required))
                                {
                                    queue.ReceiveByLookupId(MessageLookupAction.Current, m.LookupId,
                                                            MessageQueueTransactionType.Automatic);
                                    tx.Complete();
                                }

                                Console.WriteLine("Success.");
                                scope.Complete();
                            }
                        }
                    }
                }
            }
        }
    }
}
