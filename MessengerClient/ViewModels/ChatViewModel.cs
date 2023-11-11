using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
    private string _messageInputText;
    private RelayCommand _sendMessageCommand;
    
    private AppClient _appClient;
    private ChatUpdater _chatUpdater;

    private ChatWindow _window;

    public event PropertyChangedEventHandler PropertyChanged;

    public string MessageInputText
    {
        get => _messageInputText;
        set
        {
            _messageInputText = value;
            OnPropertyChanged();
        }
    }
    
    public RelayCommand SendMessageCommand
    {
        get
        {
            return _sendMessageCommand ??= new RelayCommand(obj =>
            {
                if (_messageInputText == string.Empty || string.IsNullOrWhiteSpace(_messageInputText))
                {
                    return;                                 // TODO Disable button while it is true
                }

                Message message = new Message()
                {
                    SenderNickname = App.Instance.CurrentUser.Nickname,
                    ReceiverNickname = null,
                    Text = _messageInputText,
                    PostDateTime = DateTime.UtcNow
                };
                
                _messages.Add(message);

                _appClient.PostMessagesAsync(message, success =>
                {
                    MessageInputText = string.Empty;
                    //Console.WriteLine(success? "posted" : "not_posted");
                });
                
            });
        }
    }
    
    public ChatViewModel(AppClient appClient)
    {
        _appClient = appClient;
        _chatUpdater = new ChatUpdater(appClient, UpdateMessagesList);

        _window = new ChatWindow();
        _window.DataContext = this;
        _window.MessagesListViewSource = _messages;
    }

    public void ShowWindow()
    {
        _window.Show();
        _window.Closed += OnWindowClosedByUser;
        _chatUpdater.Start(true);
    }

    private void UpdateMessagesList(List<Message> actualMessages)
    {
        _window.Dispatcher.Invoke(() =>
        {
            _messages = new ObservableCollection<Message>(actualMessages);
            _window.MessagesListViewSource = _messages;
            Console.WriteLine("Updated");
        });
    }

    private static void OnWindowClosedByUser(object sender, EventArgs e)
    {
        App.Instance.Shutdown();
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}