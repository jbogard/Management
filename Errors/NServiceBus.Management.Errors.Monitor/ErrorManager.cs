
namespace NServiceBus.Management.Errors.Monitor
{
    using System;
    using System.Messaging;
    using System.Transactions;
    using Utils;
    using NServiceBus.Unicast.Transport.Msmq;

    public class ErrorManager
    {
        private const string NonTransactionalQueueErrorMessageFormat = "Queue '{0}' must be transactional.";
        private const string NoMessageFoundErrorFormat = "INFO: No message found with ID '{0}'. Going to check headers of all messages for one with that original ID.";
        private MessageQueue queue;
        private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(5);

        public virtual string InputQueue
        {
            set
            {
                string path = MsmqUtilities.GetFullPath(value);
                var q = new MessageQueue(path);

                if (!q.Transactional)
                    throw new ArgumentException("Queue must be transactional (" + q.Path + ").");

                queue = q;

                var mpf = new MessagePropertyFilter();
                mpf.SetAll();

                queue.MessageReadPropertyFilter = mpf;
            }
        }

        public void ReturnAll()
        {
            foreach (var m in queue.GetAllMessages())
                ReturnMessageToSourceQueue(m.Id);
        }

        /// <summary>
        /// May throw a timeout exception if a message with the given id cannot be found.
        /// </summary>
        /// <param name="messageId"></param>
        public void ReturnMessageToSourceQueue(string messageId)
        {
            try
            {
                ReturnMessage(messageId);
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    Console.WriteLine("Could not return message to source queue.\nReason: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                Console.WriteLine("Message ID not found in time. Going to look in message labels for original ID.");

                foreach (var m in queue.GetAllMessages())
                {
                    var id = MsmqTransport.GetRealMessageId(m);
                    if (id == messageId)
                    {
                        try
                        {
                            ReturnMessage(m.Id);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Could not return message to source queue.\nReason: " + e.Message);
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not return message to source queue.\nReason: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public void RemoveMessage(string messageId)
        {
            try
            {
                var m = queue.ReceiveById(messageId, TimeSpan.FromSeconds(5), MessageQueueTransactionType.Automatic);
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    Console.WriteLine("Could not return message to source queue.\nReason: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                Console.WriteLine("Message ID not found in time. Going to look in message labels for original ID.");

                foreach (var m in queue.GetAllMessages())
                {
                    var id = MsmqTransport.GetRealMessageId(m);
                    if (id == messageId)
                    {
                        try
                        {
                            RemoveMessage(m.Id);
                            Console.WriteLine("Found id and removed message from queue.");
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Could not return message to source queue.\nReason: " + e.Message);
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }
            }
        }

        private void ReturnMessage(string messageId)
        {
            //using (var scope = new TransactionScope())
            {
                var m = queue.ReceiveById(messageId, TimeSpan.FromSeconds(5), MessageQueueTransactionType.Automatic);

                var failedQueue = MsmqTransport.GetFailedQueue(m);

                m.Label = MsmqTransport.GetLabelWithoutFailedQueue(m);

                using (var q = new MessageQueue(failedQueue))
                {
                    Console.WriteLine("Returning message with id " + messageId + " to queue " + failedQueue);
                    q.Send(m, MessageQueueTransactionType.Automatic);
                }

                //scope.Complete();
            }
        }

        
    }
}
