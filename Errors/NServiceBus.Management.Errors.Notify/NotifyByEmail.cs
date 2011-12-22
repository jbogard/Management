using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using System.Web;
using NServiceBus.Management.Errors.Messages;
using HtmlAgilityPack;

namespace NServiceBus.Management.Errors.Notify
{
    public class NotifyByEmail : INotifyOnError
    {
        #region INotifyOnError Members

        private readonly static NotifyByEmail instance = new NotifyByEmail();
        NotifyByEmail() { FieldValuePrefix = "$$"; }

        // TO DO
        public string EscalationRecipientList { get; set; }

        private string recipientList;
        public string RecipientList 
        {
            get { return recipientList; }
            set
            {
                recipientList = value;
                recipientArr = recipientList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private string ccList;
        public string CCList 
        {
            get { return ccList; }
            set
            {
                ccList = value;
                if (ccList != null)
                    ccArr = ccList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }     
        }

        private string bccList;
        public string BccList 
        {
            get { return bccList; }
            set
            {
                bccList = value;
                if (bccList != null)
                    bccArr = bccList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }     
        }
        
        public string Subject { get; set; }

        private string[] recipientArr;
        private string[] bccArr;
        private string[] ccArr;

        // The html template of the email that needs to be sent -- do I need this as a configuration??
        public string HtmlTemplateFileName { get; set; }
        
        // do I need this!!?? This is the prefix that marks the beginning and the end of the tag that needs to be replaced by its appropriate value.
        public string FieldValuePrefix { get; set; } 

        public static NotifyByEmail Instance
        {
            get { return instance; }
        }
        #endregion

        private string GetBodyForErrorMsgTemplate(IErrorMessageDetails[] errorMessageList)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory;
            string emailTemplateHtmFile = Path.Combine(dirName, "ErrorMessageEmailTemplate.htm");

            HtmlDocument doc = new HtmlDocument();
            doc.Load(emailTemplateHtmFile);

            // Update the datetime
            HtmlNode dateTime = doc.GetElementbyId("DateTime");
            var innerHtml = dateTime.InnerHtml;
            innerHtml = innerHtml.Replace("$$DateTime$$", DateTime.Now.ToString());
            dateTime.InnerHtml = innerHtml;

            // Get the heading row
            HtmlNode label = doc.GetElementbyId("CountLabel");
            innerHtml = label.InnerHtml;
            innerHtml = innerHtml.Replace("$$ErrorCount$$", errorMessageList.Length.ToString());
            label.InnerHtml = innerHtml;

            // This is the templated row node for each error.
            HtmlNode node = doc.GetElementbyId("ErrorListTableRowValues");
            var table = node.ParentNode;
            // Get a list of public properties exposed by the interface
            PropertyInfo[] propertyInfoArr = typeof(IErrorMessageDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (IErrorMessageDetails var in errorMessageList)
            {
                HtmlNode newNode = node.Clone();
                innerHtml = newNode.InnerHtml;
                // Replace the fields.
                foreach (PropertyInfo pi in propertyInfoArr)
                {
                    string fieldToReplace = string.Format("{0}{1}{2}", FieldValuePrefix, pi.Name, FieldValuePrefix);
                    string value = pi.GetValue(var, null).ToString();
                    innerHtml = innerHtml.Replace(fieldToReplace, HttpUtility.HtmlEncode(value));
                }
                newNode.InnerHtml = innerHtml;
                table.AppendChild(newNode);
            }
            // Remove the templated row.
            table.RemoveChild(node);
            return doc.DocumentNode.WriteContentTo();
        }

        private string GetBodyForTooManyErrorsMsgTemplate(int totalErrors, IErrorMessageDetails firstErrorMessage)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory;
            string emailTemplateHtmFile = Path.Combine(dirName, "TooManyErrorsEmailTemplate.htm");

            HtmlDocument doc = new HtmlDocument();
            doc.Load(emailTemplateHtmFile);

            // Update the datetime
            HtmlNode dateTime = doc.GetElementbyId("DateTime");
            var innerHtml = dateTime.InnerHtml;
            innerHtml = innerHtml.Replace("$$DateTime$$", DateTime.Now.ToString());
            dateTime.InnerHtml = innerHtml;

            // Get the heading row
            HtmlNode label = doc.GetElementbyId("CountLabel");
            innerHtml = label.InnerHtml;
            innerHtml = innerHtml.Replace("$$ErrorCount$$", totalErrors.ToString());
            label.InnerHtml = innerHtml;

            // Get a list of public properties exposed by the interface
            PropertyInfo[] propertyInfoArr = typeof(IErrorMessageDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // This is the templated table that displays all the properties in the IErrorDetails interface.
            HtmlNode node = doc.GetElementbyId("ErrorValuesTable");
            innerHtml = node.InnerHtml;

            // Replace the fields.
            foreach (PropertyInfo pi in propertyInfoArr)
            {
                string fieldToReplace = string.Format("{0}{1}{2}", FieldValuePrefix, pi.Name, FieldValuePrefix);
                string value = pi.GetValue(firstErrorMessage, null).ToString();
                innerHtml = innerHtml.Replace(fieldToReplace, HttpUtility.HtmlEncode(value));
            }
            node.InnerHtml = innerHtml;
            
            return doc.DocumentNode.WriteContentTo();
        }


        

        public void AlertOnError(IErrorMessageDetails[] errorMessages)
        {
            // Need to set the email recipient list, if we are using the smtp notifier.
            if (String.IsNullOrEmpty(RecipientList))
                throw new Exception("Recipient email list cannot be empty. Please add the list of recipients in the app settings separated by a semi-colon character");
            string subject = "Error Messages Received in the Queue";
            string body = this.GetBodyForErrorMsgTemplate(errorMessages);
            bool isBodyHtml = true;
            SendEmail(recipientArr, ccArr, bccArr, subject, body, isBodyHtml);
        }

        public void AlertTooManyMessagesInErrorQueue(int count, IErrorMessageDetails firstErrorMessage)
        {
            // Need to set the email recipient list, if we are using the smtp notifier.
            if (String.IsNullOrEmpty(RecipientList))
                throw new Exception("Recipient email list cannot be empty. Please add the list of recipients in the app settings separated by a semi-colon character");
            string subject = string.Format("Too many errors in the error queue (Count={0})",count);
            string body = this.GetBodyForTooManyErrorsMsgTemplate(count, firstErrorMessage);
            bool isBodyHtml = true;
            SendEmail(recipientArr, ccArr, bccArr, subject, body, isBodyHtml);
        }

        public void SendEmail(string[] recipientArr, string[] ccArr, string[] bccArr, string subject, string body, bool isHtml )
        {
            MailMessage message = new MailMessage();

            // Need to set the email recipient list, if we are using the smtp notifier.
            foreach (string recipient in recipientArr)
            {
                message.To.Add(recipient);
            }

            if (ccArr != null) // can be null
            {
                foreach (string recipient in ccArr)
                {
                    message.CC.Add(recipient);
                }
            }

            if (bccArr != null) // can be null
            {
                foreach (string recipient in bccArr)
                {
                    message.Bcc.Add(recipient);
                }
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            // Send the message
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Send(message);
        }
    }
}
