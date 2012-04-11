using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NServiceBus.Management.Errors.UIModule.Model;
using NServiceBus.Management.Errors.Messages;
using System.ComponentModel;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Commands;
using NServiceBus.Management.Errors.PrismEvents;

namespace NServiceBus.Management.Errors.UIModule.ViewModel
{
    public class ErrorMessageDetailsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly IQueryErrorPersistence errorPersistenceStore;
        private readonly IBus bus;
        private readonly IEventAggregator eventAggregator;

        private ObservableCollection<ErrorMessageDetails> errorMessages = new ObservableCollection<ErrorMessageDetails>();
        public  ObservableCollection<ErrorMessageDetails> ErrorMessages { get { return errorMessages; } }

        private DelegateCommand<string> reprocessCommand;
        public DelegateCommand<string> ReprocessCommand { get { return reprocessCommand; } }

        private DelegateCommand<string> deleteCommand;
        public DelegateCommand<string> DeleteCommand { get { return deleteCommand; } }

        public ErrorMessageDetailsViewModel(IBus bus, 
            IQueryErrorPersistence errorPersistenceStore,
            IEventAggregator eventAggregator)
        {
            if (errorPersistenceStore == null) throw new ArgumentNullException("ErrorPersistenceStore is not initialized");
            if (bus == null) throw new ArgumentNullException("NServiceBus is not initialized");
            if (eventAggregator == null) throw new ArgumentNullException("EventAggregator is not initialized");

            this.errorPersistenceStore = errorPersistenceStore;
            this.bus = bus;
            this.eventAggregator = eventAggregator;

            // Load error messages from the persistent store
            LoadErrorMessages();

            // Wire the reprocess command handler
            this.reprocessCommand = new DelegateCommand<string>(ReprocessCommandExecuted, CanCommandExecute);
            this.deleteCommand = new DelegateCommand<string>(DeleteCommandExecuted, CanCommandExecute);

            // Subscribe to new error messages received event
            eventAggregator.GetEvent<ErrorMessageReceivedPrismEvent>().Subscribe(OnNewErrorMessageReceived);
            eventAggregator.GetEvent<ErrorMessageDeletedPrismEvent>().Subscribe(OnErrorMessageDeleted);
            eventAggregator.GetEvent<ErrorMessageReprocessedPrismEvent>().Subscribe(OnErrorMessageReprocessed);

        }

        private void LoadErrorMessages()
        {
            // read from the persistent store and display the list of error messages.

            ReadOnlyCollection<IErrorMessageDetails> errorList = errorPersistenceStore.ErrorMessages;
            foreach (IErrorMessageDetails errorMessage in errorList)
            {
                ErrorMessageDetails detail = new ErrorMessageDetails
                {
                    MessageId = errorMessage.OriginalMessageId,
                    MessageXml = errorMessage.Body,
                    ProcessingFailedAddress = errorMessage.ProcessingFailedAddress,
                    TimeSent = errorMessage.ErrorReceivedTime,
                    WindowsIdentity = errorMessage.Identity,
                    ExceptionInformation = errorMessage.ExceptionInformation
                };

                errorMessages.Add(detail);

            }
        }

        private void OnNewErrorMessageReceived(ErrorMessageReceived message)
        {
            // Add this new error to the observable collection.
            ErrorMessageDetails newError = new ErrorMessageDetails()
            {
                MessageId = message.OriginalMessageId,
                MessageXml = message.Body,
                ProcessingFailedAddress = message.ProcessingFailedAddress,
                TimeSent = message.ErrorReceivedTime,
                WindowsIdentity = message.Identity,
                ExceptionInformation = message.ExceptionInformation
            };
            errorMessages.Add(newError);
        }

        private void ReprocessCommandExecuted(string messageId)
        {
            // We have the message we want to reprocess.
            ReprocessErrorMessage reprocessCommand = new ReprocessErrorMessage() { OriginalMessageId = messageId };
            bus.Send(reprocessCommand);

            // Remove item from the list.
            RemoveItem(messageId);
        }

        private void DeleteCommandExecuted(string messageId)
        {
            // We have the message we want to delete.
            DeleteErrorMessage deleteCommand = new DeleteErrorMessage() { OriginalMessageId = messageId };
            bus.Send(deleteCommand);

            // Remove item from the list.
            RemoveItem(messageId);
        }

        private bool CanCommandExecute(string parameter)
        {
            return true;
        }

        private void OnErrorMessageDeleted(ErrorMessageDeleted message)
        {
            RemoveItem(message.MessageId);
        }

        private void OnErrorMessageReprocessed(ErrorMessageReprocessed message)
        {
            RemoveItem(message.MessageId);
        }

        private void RemoveItem(string messageId)
        {
            // Remove item from the list.
            var errMsgItem = (from item in errorMessages
                              where item.MessageId.Equals(messageId)
                              select item).FirstOrDefault();
            if (errMsgItem != null)
                errorMessages.Remove(errMsgItem);
        }
    }
}
