using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using MessengerClient.Commands;
using MessengerClient.Network;
using MessengerCoreLibrary.Models;

namespace MessengerClient.ViewModels;

public class ChatViewModel : INotifyPropertyChanged, IDisposable
{
    private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
    private string _messageInputText;
    private bool _isSendMessageAllowed;
    private RelayCommand _sendMessageCommand;
    
    private readonly AppClient _appClient;
    private bool _polling; 
    
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

    public bool IsSendMessageAllowed
    {
        get => _isSendMessageAllowed;

        set
        {
            _isSendMessageAllowed = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Message> Messages
    {
        get => _messages;

        set
        {
            _messages = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SendMessageCommand
    {
        get
        {
            return _sendMessageCommand ??= new RelayCommand(obj =>
            {
                SendMessage();
            }, obj=> IsSendMessageAllowed);
        }
    }
    
    public ChatViewModel(AppSharedOptions sharedOptions)
    {
        _appClient = sharedOptions.AppClient;
    }

    public void StartPolling()
    {
        if (_polling)
        {
            return;
        }
        
        _appClient.ChatUpdater.Start();
        _appClient.ChatUpdater.OnUpdate += UpdateMessagesList;
        
        _polling = true;
    }
    
    public void StopPolling()
    {
        if (!_polling)
        {
            return;
        }

        _appClient.ChatUpdater.Stop();
        _appClient.ChatUpdater.OnUpdate -= UpdateMessagesList;
        _polling = false;
    }

    public void Dispose() => StopPolling();

    private void SendMessage()
    {
        Message message = new Message(_appClient.CurrentUser.Nickname, null, 
            _messageInputText, DateTime.UtcNow);
                
        _messages.Add(message);

        _appClient.PostMessageAsync(message).ContinueWith(task =>
        {
            MessageInputText = string.Empty;
            
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void UpdateMessagesList(List<Message> actualMessages)
    {
        Messages = new ObservableCollection<Message>(actualMessages);
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (propertyName == nameof(MessageInputText))
        {
            IsSendMessageAllowed = !string.IsNullOrWhiteSpace(_messageInputText) && _messageInputText != string.Empty;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}