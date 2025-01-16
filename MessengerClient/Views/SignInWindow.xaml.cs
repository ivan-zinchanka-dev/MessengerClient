using System.Windows;
using MessengerClient.ViewModels;

namespace MessengerClient.Views
{
    public partial class SignInWindow : Window
    {
        private readonly AuthorizationViewModel _viewModel;
        
        public SignInWindow(AuthorizationViewModel viewModel)
        {
            _viewModel = viewModel;

            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}