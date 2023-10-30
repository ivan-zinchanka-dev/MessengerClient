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
    private string _messageInputText = "Input yours";
    private RelayCommand _sendMessageCommand;
    
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
                Console.WriteLine("Send");
                _messages.Add(new Message()
                {
                    Text = _messageInputText
                });
                
            });
        }
    }

    private void InitMessagesList()
    {
        _messages.Add(new Message()
        {
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        });

        _messages.Add(new Message()
        {
            Text = "Who?"
        });
        
        _messages.Add(new Message()
        {
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        });
        
        _messages.Add(new Message()
        {
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        });
        
        _messages.Add(new Message()
        {
            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        });
    }

    public ChatViewModel()
    {
        InitMessagesList();
        
        Window = new ChatWindow();
        Window.DataContext = this;
        Window.MessagesListViewSource = _messages;
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}