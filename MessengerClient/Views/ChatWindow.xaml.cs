using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using MessengerClient.Core.Models;

namespace MessengerClient.Views;

public partial class ChatWindow : Window
{
    public IEnumerable MessagesListViewSource
    {
        get => _messagesListView.ItemsSource;
        set => _messagesListView.ItemsSource = value;
    }
    
    public ChatWindow()
    {
        InitializeComponent();
    }
    
}