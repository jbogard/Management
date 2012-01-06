using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Notify;
using NServiceBus.Management.Errors.Persister;
using System.Configuration;
using Raven.Client.Document;

namespace NServiceBus.Management.Errors.Monitor
{
    class RegisterPersister : IWantCustomInitialization
    {
        #region IWantCustomInitialization Members

        public void Init()
        {
            PersistErrorsInRavenDB persister = PersistErrorsInRavenDB.Instance;

            DocumentStore documentStore = new DocumentStore { ConnectionStringName = "RavenDbConnectionString" };
            documentStore.Initialize();
            persister.DocumentStore = documentStore;

            NServiceBus.Configure.With().Configurer.RegisterSingleton<IPersistErrorMessages>(persister);

            //Configure.Instance.Configurer.ConfigureComponent<PersistErrorsInRavenDB>(ObjectBuilder.ComponentCallModelEnum.Singleton);
            //Configure.Instance.Configurer.ConfigureProperty<PersistErrorsInRavenDB>(mt => mt.DocumentStore, documentStore);

            //NServiceBus.Configure.With().Configurer.RegisterSingleton<IPersistErrorMessages>(persister);            
        }

        #endregion
    }
}
