using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using MessengerClient.Commands;
using MessengerClient.Network;
using MessengerClient.Views;
using MessengerCoreLibrary.Models;

namespace MessengerClient.ViewModels;

public class ChatViewModel : INotifyPropertyChanged, IDisposable
{
    private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
    private string _messageInputText;
    private bool _isSendMessageAllowed;
    private RelayCommand _sendMessageCommand;
    
    private readonly App _appInstance;

    private bool _polling; 
    private Dispatcher _dispatcher;
    //private readonly ChatWindow _window;
    private bool _messagesUpdatedOnce;

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
                if (!IsSendMessageAllowed)
                {
                    return;
                }

                Message message = new Message(_appInstance.CurrentUser.Nickname, null, 
                    _messageInputText, DateTime.UtcNow);
                
                _messages.Add(message);

                _appInstance.PostMessageAsync(message).ContinueWith(task =>
                {
                    MessageInputText = string.Empty;
            
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }
    }
    
    public ChatViewModel(App appInstance)
    {
        _appInstance = appInstance;
        
        /*_window = new ChatWindow();
        _window.DataContext = this;*/
    }

    public void StartPolling(Dispatcher dispatcher)
    {
        if (_polling)
        {
            return;
        }

        _dispatcher = dispatcher;
        _appInstance.ChatUpdater.Start();
        _appInstance.ChatUpdater.OnUpdate += UpdateMessagesList;
        
        _polling = true;
    }

    /*public void ShowWindow()
    {
        _window.Show();
        _window.Closed += OnWindowClosedByUser;
        _appInstance.ChatUpdater.Start();
        _appInstance.ChatUpdater.OnUpdate += UpdateMessagesList;
    }*/

    private void UpdateMessagesList(List<Message> actualMessages)
    {
        _dispatcher.Invoke(() =>
        {
            Messages = new ObservableCollection<Message>(actualMessages);

            if (!_messagesUpdatedOnce)
            {
                //_window.ScrollMessagesListToEnd();        //TODO ADD
                _messagesUpdatedOnce = true;
            }
        });
    }

    /*private void OnWindowClosedByUser(object sender, EventArgs e)
    {
        _appInstance.ChatUpdater.Stop();
        _appInstance.ChatUpdater.OnUpdate -= UpdateMessagesList;
        _appInstance.Shutdown();
    }*/
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (propertyName == nameof(MessageInputText))
        {
            IsSendMessageAllowed = !string.IsNullOrWhiteSpace(_messageInputText) && _messageInputText != string.Empty;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void StopPolling()
    {
        if (!_polling)
        {
            return;
        }

        _appInstance.ChatUpdater.Stop();
        _appInstance.ChatUpdater.OnUpdate -= UpdateMessagesList;
        _polling = false;
    }

    public void Dispose() => StopPolling();
}