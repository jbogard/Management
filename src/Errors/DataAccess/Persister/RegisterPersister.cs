using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Config;
using Raven.Client.Document;

namespace NServiceBus.Management.Errors.DataAccess.Persister
{
    class RegisterPersister : INeedInitialization
    {
        public void Init()
        {
            PersistErrorsInRavenDB persister = PersistErrorsInRavenDB.Instance;
            DocumentStore documentStore = new DocumentStore { ConnectionStringName = "RavenDbConnectionString" };
            documentStore.Initialize();
            persister.DocumentStore = documentStore;
            NServiceBus.Configure.With().Configurer.RegisterSingleton<IPersistErrorMessages>(persister);
        }
    }
}
