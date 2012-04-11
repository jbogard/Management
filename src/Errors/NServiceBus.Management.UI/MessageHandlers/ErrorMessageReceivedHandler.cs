using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.Management.Errors.Messages;
using Microsoft.Practices.Prism.Events;
using NServiceBus.Management.Errors.PrismEvents;
using System.Windows.Threading;

namespace NServiceBus.Management.UI.MessageHandlers
{
    class ErrorMessageReceivedHandler : IHandleMessages<ErrorMessageReceived>, 
        IHandleMessages<ErrorMessageDeleted>,
        IHandleMessages<ErrorMessageReprocessed>
    {
        public IEventAggregator eventAggregator { get; set; }
        public void Handle(ErrorMessageReceived message)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() => { eventAggregator.GetEvent<ErrorMessageReceivedPrismEvent>().Publish(message); }));
        }

        public void Handle(ErrorMessageDeleted message)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() => { eventAggregator.GetEvent<ErrorMessageDeletedPrismEvent>().Publish(message); }));
        }

        public void Handle(ErrorMessageReprocessed message)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() => { eventAggregator.GetEvent<ErrorMessageReprocessedPrismEvent>().Publish(message); }));
        }
    }
}
