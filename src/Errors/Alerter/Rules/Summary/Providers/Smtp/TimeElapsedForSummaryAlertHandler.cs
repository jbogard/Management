using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.DomainEvents;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;
using HtmlAgilityPack;
using System.Reflection;
using NServiceBus.Management.Errors.Messages;
using System.Web;
using System.IO;
using NServiceBus.Management.Errors.Alerter.Messages;

namespace NServiceBus.Management.Errors.Alerter.Providers.SmtpAlertProvider
{
    public class TimeElapsedForSummaryAlertHandler : IHandleDomainEvents<TimeElapsedForSummaryAlert>
    {
        public IBus Bus { get; set; }
        
        private const string FieldValuePrefix = "$$";

        public void Handle(TimeElapsedForSummaryAlert args)
        {
            // Check to see if this provider is configured
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;
            AlertRule rule = ruleCollection[args.RuleId];
            AlertProviderCollection alertProviders = rule.AlertProviders;

            foreach (Provider provider in alertProviders)
            {
                if (provider.Type.Equals("Smtp"))
                {
                    if (String.IsNullOrEmpty(provider.RecipientList))
                        throw new Exception("Recipient email list cannot be empty. Please add the list of recipients in the app settings separated by a semi-colon character");
                    string subject = "Error Messages Received in the Queue";

                    string body = this.GetBodyForErrorMsgTemplate(args.ErrorMessages);
                    var recipientArr = provider.RecipientList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    bool isBodyHtml = true;

                    Bus.SendLocal<SendEmail>(m =>
                    {
                        m.Recipients = recipientArr;
                        m.CC = null;
                        m.Bcc = null;
                        m.Subject = subject;
                        m.Body = body;
                        m.IsBodyHtml = isBodyHtml;
                    });
                }
            }
        }

        private string GetBodyForErrorMsgTemplate(IErrorMessageDetails[] errorMessageList)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory;
            string emailTemplateHtmFile = Path.Combine(dirName, @"Providers\Smtp\ErrorMessageEmailTemplate.htm");

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
    }
}
