﻿using System;
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
    private string _messageInputText = "Input yours";
    private RelayCommand _sendMessageCommand;
    
    private AppClient _appClient;
    private ChatUpdater _chatUpdater;
    
    public ChatWindow Window { get; private set; }

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
                    Console.WriteLine(success? "posted" : "not_posted");
                });
                
            });
        }
    }
    
    public ChatViewModel(AppClient appClient)
    {
        _appClient = appClient;
        _chatUpdater = new ChatUpdater(appClient, UpdateMessagesList);

        Window = new ChatWindow();
        Window.DataContext = this;
        Window.MessagesListViewSource = _messages;
    }
    
    public void Initialize()
    {
        _chatUpdater.Start(true);
    }

    private void UpdateMessagesList(List<Message> actualMessages)
    {
        Window.Dispatcher.Invoke(() =>
        {
            _messages = new ObservableCollection<Message>(actualMessages);
            Window.MessagesListViewSource = _messages;
            Console.WriteLine("Updated");
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}