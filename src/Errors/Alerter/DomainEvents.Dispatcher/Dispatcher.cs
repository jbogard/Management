using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Alerter.DomainEvents;
using StructureMap;

namespace NServiceBus.Management.Errors.Alerter.DomainEvents
{
    public static class Dispatcher
    {
        public static IContainer Container { get; set; } //as before

        //Raises the given domain event
        public static void Raise<T>(T args) where T : IDomainEvent
        {
            if (Container != null)
            {
                foreach (var handler in Container.GetAllInstances<IHandleDomainEvents<T>>())
                {
                    handler.Handle(args);
                }
            }
        }
    }
}
