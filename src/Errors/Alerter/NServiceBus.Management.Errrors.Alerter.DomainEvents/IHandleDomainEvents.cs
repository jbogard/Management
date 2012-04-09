using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServiceBus.Management.Errrors.Alerter.DomainEvents
{
    public interface IHandleDomainEvents<T> where T : IDomainEvent
    {
        void Handle(T args);
    }
}
