using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NServiceBus.Management.Errors.UIModule.Model
{
    public class ErrorMessageDetails : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string messageId;
        public string MessageId
        {
            get {return messageId;}
            set 
            {
                if (messageId != value)
                {
                    messageId = value;
                    RaisePropertyChanged("MessageId");
                }
            }
        }

        private string processingFailedAddress;
        public string ProcessingFailedAddress
        {
            get {return processingFailedAddress;}
            set 
            {
                if (processingFailedAddress != value)
                {
                    processingFailedAddress = value;
                    RaisePropertyChanged("ProcessingFailedAddress");
                }
            }
        }

        private DateTime timeSent;
        public DateTime TimeSent
        {
            get {return timeSent;}
            set 
            {
                if (timeSent != value)
                {
                    timeSent = value;
                    RaisePropertyChanged("TimeSent");
                }
            }
        }

        private string messageXml;
        public string MessageXml
        {
            get {return messageXml;}
            set 
            {
                if (messageXml != value)
                {
                    messageXml = value;
                    RaisePropertyChanged("MessageXml");
                }
            }
        }

        private string windowsIdentity;
        public string WindowsIdentity
        {
            get {return windowsIdentity;}
            set 
            {
                if (windowsIdentity != value)
                {
                    windowsIdentity = value;
                    RaisePropertyChanged("WindowsIdentity");
                }
            }
        }

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

    }
}
