using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessengerCoreLibrary.Models;

namespace MessengerClient.Network;

public class ChatUpdater
{
    private static readonly TimeSpan UpdatePeriod = TimeSpan.FromSeconds(1.0f);

    private readonly Func<Task<List<Message>>> _getMessagesMethod;
    private Timer _updateTimer;
    
    public event Action<List<Message>> OnUpdate;

    public ChatUpdater(Func<Task<List<Message>>> getMessagesMethod)
    {
        _getMessagesMethod = getMessagesMethod;
    }
    
    public async void Start(bool instantly = true)
    {
        _updateTimer = new Timer(UpdateChat, null, instantly ? TimeSpan.Zero : UpdatePeriod, UpdatePeriod);
    }
    
    private async void UpdateChat(object parameter)
    {
        List<Message> messages = await _getMessagesMethod.Invoke();
        OnUpdate?.Invoke(messages);
    }
    
    public async void Stop()
    {
        await _updateTimer.DisposeAsync();
    }
}