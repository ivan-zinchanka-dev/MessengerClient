using System.Windows;

namespace MessengerClient.Views;

public partial class ChatWindow : Window
{
    public ChatWindow()
    {
        InitializeComponent();
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