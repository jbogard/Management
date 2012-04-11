using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Config;
using Raven.Client.Document;

namespace NServiceBus.Management.Errors.DataAccess.Query
{
    class RegisterQueryProvider : INeedInitialization
    {
        public void Init()
        {
            var queryProvider = QueryFromRavenDB.Instance;
            DocumentStore documentStore = new DocumentStore { ConnectionStringName = "RavenDbConnectionString" };
            documentStore.Initialize();
            queryProvider.DocumentStore = documentStore;

            Configure.Instance.Configurer.RegisterSingleton<IQueryErrorPersistence>(queryProvider);
        }
    }
}
