using System.Windows;
using MessengerClient.ViewModels;

namespace MessengerClient.Views;

public partial class SignUpWindow : Window
{
    private readonly AuthorizationViewModel _viewModel;
    
    public SignUpWindow(AuthorizationViewModel viewModel)
    {
        _viewModel = viewModel;

        DataContext = _viewModel;
        InitializeComponent();
    }
}