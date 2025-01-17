using System.ComponentModel;
using System.Windows;
using MessengerClient.ViewModels;

namespace MessengerClient.Views;

public partial class ChatWindow : Window
{
    private readonly ChatViewModel _viewModel;
    private bool _messagesUpdatedOnce;
    
    public ChatWindow(ChatViewModel viewModel)
    {
        _viewModel = viewModel;
        
        DataContext = _viewModel;
        InitializeComponent();
        
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.StartPolling();
        
        Unloaded += OnUnloaded;
    }
    
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(_viewModel.Messages) && !_messagesUpdatedOnce)
        {
            ScrollMessagesListToEnd();
            _messagesUpdatedOnce = true;
        }
    }

    private void ScrollMessagesListToEnd()
    {
        if (_messagesListView.Items.Count > 0)
        {
            var last = _messagesListView.Items[_messagesListView.Items.Count - 1];
            _messagesListView.ScrollIntoView(last);
        }
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
        _viewModel.StopPolling();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}