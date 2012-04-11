using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap.Configuration.DSL;
using NServiceBus.Management.Errors.Alerter.DomainEvents;

namespace NServiceBus.Management.Errors.Alerter.Host
{
    public class DomainEventHandlerRegistry : Registry
    {
        public DomainEventHandlerRegistry()
        {
            Scan(x =>
            {
                x.AssembliesFromApplicationBaseDirectory();
                x.AddAllTypesOf(typeof(IHandleDomainEvents<>));
            });
        }
    }
}
