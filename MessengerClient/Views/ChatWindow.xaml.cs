using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MessengerClient.Core.Models;

namespace MessengerClient.Views;

public partial class ChatWindow : Window
{
    public event Action OnHidden;
    
    public IEnumerable MessagesListViewSource
    {
        get => _messagesListView.ItemsSource;
        set => _messagesListView.ItemsSource = value;
    }
    
    public void Refresh()
    {
        _messagesListView.Items.Refresh();
    }

    public ChatWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        OnHidden?.Invoke();
    }
    
}