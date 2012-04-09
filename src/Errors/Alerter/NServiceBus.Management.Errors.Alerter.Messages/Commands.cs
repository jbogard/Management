using NServiceBus;
using System;
using NServiceBus.Management.Errors.Messages;
using System.Collections.Generic;

namespace NServiceBus.Management.Errors.Alerter.Messages
{
    public class ProcessErrorMessageReceived : ICommand
    {
        public Guid AlerterInstanceId { get; set; }
        public ErrorMessageReceived MessageDetails { get; set; }  
    }

    public class ProcessErrorMessageReprocessed : ICommand
    {
        public Guid AlerterInstanceId { get; set; }
        public ErrorMessageReprocessed MessageDetails { get; set; }
    }

    public class ProcessErrorMessageDeleted : ICommand
    {
        public Guid AlerterInstanceId { get; set; }
        public ErrorMessageDeleted MessageDetails { get; set; }
    }

    public class SendSummaryAlert : ICommand
    {
        public string RuleId { get; set; }
        public int DurationToWait { get; set; }
    }

    public class SendCriticalErrorLimitReachedAlert : ICommand
    {
        public string RuleId { get; set; }
        public int Count { get; set; }
    }
}
