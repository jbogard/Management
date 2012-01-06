using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.UnityExtensions;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using NServiceBus.Management.Errors.UIModule;
using NServiceBus.Unicast;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using NServiceBus.Management.Errors;
using NServiceBus.Management.Errors.Query;
using Raven.Client.Document;
using System.Configuration;


namespace NServiceBus.Management.UI
{
    class Bootstrapper : UnityBootstrapper
    {
        public UnicastBus Bus { get; set; }
        public QueryFromRavenDB ravenStore = QueryFromRavenDB.Instance;
        protected override System.Windows.DependencyObject CreateShell()
        {
            return new MainApplicationShell();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = (Window)this.Shell;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;
            moduleCatalog.AddModule(typeof(ErrorModule));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            var eventAggregator = this.Container.Resolve<IEventAggregator>();

            // Initialize the bus
            Bus = (UnicastBus)NServiceBus.Configure.With()
                .DefineEndpointName("NServiceBus.Management.UI")
            .DefaultBuilder()
            .XmlSerializer()
            .MsmqTransport()
                .IsTransactional(true)
                .PurgeOnStartup(false)
            .RunCustomAction(() => Configure.Instance.Configurer.RegisterSingleton<IEventAggregator>(eventAggregator))
            .UnicastBus()
                .ImpersonateSender(false)
                .LoadMessageHandlers()
            .CreateBus()
            .Start();

            this.RegisterTypeIfMissing(typeof(IBus), typeof(UnicastBus), true);
            this.Container.RegisterInstance<IBus>(Bus);

            // Register Raven as the IQuery for error persistence
            DocumentStore documentStore = new DocumentStore { ConnectionStringName = "RavenDbConnectionString" };
            documentStore.Initialize();
            ravenStore.DocumentStore = documentStore;

            this.RegisterTypeIfMissing(typeof(IQueryErrorPersistence), typeof(QueryFromRavenDB), true);
            this.Container.RegisterInstance<IQueryErrorPersistence>(ravenStore);

        }
    }
}
