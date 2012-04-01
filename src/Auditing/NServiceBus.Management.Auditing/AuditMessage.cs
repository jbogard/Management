using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Auditing
{
    public class AuditMessage
    {
        public string MessageId { get; set; }
        public string OriginalMessageId { get; set; }
        public string MessageType { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public DateTime ReceivedTime { get; set; }
    }
}
