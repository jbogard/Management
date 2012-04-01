using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Auditing.Persister.Raven;
using Raven.Client.Document;

namespace NServiceBus.Management.Auditing.Persister
{
    class RegisterRavenPersister : IWantCustomInitialization
    {
        public void Init()
        {
            var persister = PersistAuditMessagesInRaven.Instance;
            DocumentStore documentStore = new DocumentStore { ConnectionStringName = "AuditMessagesDatabase" };
            documentStore.Initialize();
            persister.DocumentStore = documentStore;

            Configure.Instance.Configurer.RegisterSingleton<IPersistAuditMessages>(persister);

        }
    }
}
