using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NServiceBus.Management.Errors.Messages;

namespace NServiceBus.Management.Errors
{
    public interface IQueryErrorPersistence
    {
        ReadOnlyCollection<IErrorMessageDetails> ErrorMessages { get; }
    }
}
