using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Auditing
{
    public interface IPersistAuditMessages
    {
        void Persist(AuditMessage message);
    }
}
