using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errrors.Alerter.DomainEvents;
using HtmlAgilityPack;
using NServiceBus.Management.Errors.Alerter.Config;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Web;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Alerter.Providers.SmtpAlertProvider
{
    public class MaxThresholdLimitReachedHandler : IHandleDomainEvents<MaxThresholdLimitReached>
    {
        public IBus Bus { get; set; }

        private const string FieldValuePrefix = "$$";
        public void Handle(MaxThresholdLimitReached args)
        {
            // Check to see if this provider is configured
            AlertRulesSection alertSection = (AlertRulesSection)ConfigurationManager.GetSection("AlertRulesConfig");
            AlertRuleCollection ruleCollection = alertSection.RuleCollection;
            AlertRule rule = ruleCollection[args.RuleId];
            AlertProviderCollection alertProviders = rule.AlertProviders;

            foreach (Provider provider in alertProviders)
            {
                if (provider.Type == "Smtp")
                {
                    if (String.IsNullOrEmpty(provider.RecipientList))
                        throw new Exception("Recipient email list cannot be empty. Please add the list of recipients in the app settings separated by a semi-colon character");
                    string subject = string.Format("Too many errors in the error queue (Count={0})", args.TotalErrorsInErrorQueue);

                    string body = this.GetBodyForTooManyErrorsMsgTemplate(args.TotalErrorsInErrorQueue, args.FirstErrorMessage);
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
    }
}
