using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;


namespace NServiceBus.Management.Errors.Alerter.Providers.SmtpAlertProvider
{
    class SendEmailHandler : IHandleMessages<SendEmail>
    {
        public void Handle(SendEmail messageToSend)
        {
            MailMessage message = new MailMessage();

            // Need to set the email recipient list, if we are using the smtp notifier.
            foreach (string recipient in messageToSend.Recipients)
            {
                message.To.Add(recipient);
            }

            if (messageToSend.CC != null) // can be null
            {
                foreach (string recipient in messageToSend.CC)
                {
                    message.CC.Add(recipient);
                }
            }

            if (messageToSend.Bcc != null) // can be null
            {
                foreach (string recipient in messageToSend.Bcc)
                {
                    message.Bcc.Add(recipient);
                }
            }

            message.Subject = messageToSend.Subject;
            message.Body = messageToSend.Body;
            message.IsBodyHtml = messageToSend.IsBodyHtml;

            // Send the message
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Send(message);
        }
    }
}
