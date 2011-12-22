using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Saga;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors.Alerter
{
    public class AlerterSagaData : ISagaEntity
    {
        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }

        public Guid AlerterSagaId { get; set; }
        public bool IsTimeoutAlreadyRequested { get; set; }
        private List<ErrorAlertInfo> errorListToAlert;
        
        public AlerterSagaData()
        {
            errorListToAlert = new List<ErrorAlertInfo>();
        }

        public List<ErrorAlertInfo> ErrorListToAlert
        {
            get { return errorListToAlert; }
            set { errorListToAlert = value; }
        }
    }
}
