using System.Windows;
using MessengerClient.ViewModels;

namespace MessengerClient.Views;

public partial class ChatWindow : Window
{
    private readonly ChatViewModel _viewModel;
    
    public ChatWindow(ChatViewModel viewModel)
    {
        _viewModel = viewModel;
        
        DataContext = _viewModel;
        InitializeComponent();
        
        _viewModel.StartPolling(Dispatcher);
    }

    public void ScrollMessagesListToEnd()
    {
        if (_messagesListView.Items.Count > 0)
        {
            var last = _messagesListView.Items[_messagesListView.Items.Count - 1];
            _messagesListView.ScrollIntoView(last);
        }
    }
}