using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;
using NServiceBus.Management.Errors.Alerter.DomainEvents;

namespace NServiceBus.Management.Errors.Alerter.Host
{
    class EndpointConfig : IConfigureThisEndpoint, AsA_Publisher, IWantCustomInitialization
    {
        public void Init()
        {
            // Bootstrap StructureMap
            ObjectFactory.Initialize(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.LookForRegistries();
                    scanner.AssembliesFromApplicationBaseDirectory();
                });
            });

            // Bootstrap Nsb
            NServiceBus.Configure.With()
                .StructureMapBuilder(ObjectFactory.Container)
                .XmlSerializer();

            Dispatcher.Container = ObjectFactory.Container;
            
        }
    }
}
