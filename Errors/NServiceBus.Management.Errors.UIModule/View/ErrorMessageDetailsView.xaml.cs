using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NServiceBus.Management.Errors.UIModule.ViewModel;

namespace NServiceBus.Management.Errors.UIModule.View
{
    /// <summary>
    /// Interaction logic for ErrorMessagesView.xaml
    /// </summary>
    public partial class ErrorMessageDetailsView : UserControl
    {
        public ErrorMessageDetailsView(ErrorMessageDetailsViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
