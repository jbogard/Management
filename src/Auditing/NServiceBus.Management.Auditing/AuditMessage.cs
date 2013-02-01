using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Raven.Json.Linq;

namespace NServiceBus.Management.Auditing
{
    public class AuditMessage
    {
        public int Id { get; set; }
        public string MessageId { get; set; }
        public string OriginalMessageId { get; set; }
        public string MessageType { get; set; }
        public string OriginalBody { get; set; }
        public RavenJObject Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public DateTime ReceivedTime { get; set; }

        public string ReplyToAddress { get; set; }
    }
}
