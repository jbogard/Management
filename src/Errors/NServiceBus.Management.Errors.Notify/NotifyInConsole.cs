using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Notify
{
    public class NotifyInConsole : INotifyOnError
    {
        private readonly static NotifyInConsole instance = new NotifyInConsole();
        NotifyInConsole() { }

        #region INotifyOnError Members


        public void NotifyOnError(IErrorMessageDetails errorMessage)
        {
            Console.WriteLine("Message failed in Queue: {0}", errorMessage.ProcessingFailedAddress);
            Console.WriteLine("Message Id: {0}", errorMessage.FailedMessageId);
            Console.WriteLine("Message received at: {0}", errorMessage.ErrorReceivedTime);
            Console.WriteLine("Xml contents of message: {0}", errorMessage.Body);
        }

        #endregion

        public static NotifyInConsole Instance
        {
            get { return instance; }
        }


        public void AlertOnError(IErrorMessageDetails[] errorMessages)
        {
            Console.WriteLine("ALERT -- (Total:{0})", errorMessages.Length);
        }

        public void AlertTooManyMessagesInErrorQueue(int count, IErrorMessageDetails lastErrorMessage)
        {
            Console.WriteLine("ALERT -- TOO MANY MESSAGES IN Q (Total:{0})");
        }

    }
}
