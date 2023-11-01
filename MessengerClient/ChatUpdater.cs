using System;
using System.Collections.Generic;
using System.Threading;
using MessengerClient.Core.Models;

namespace MessengerClient;

public class ChatUpdater
{
    private AppClient _appClient;
    private Timer _updateTimer;
    
    private static readonly TimeSpan UpdatePeriod = TimeSpan.FromSeconds(1.0f);

    private readonly Action<List<Message>> _updateMethod;
    
    public ChatUpdater(AppClient appClient, Action<List<Message>> updateMethod)
    {
        _appClient = appClient;
        _updateMethod = updateMethod;
    }
    
    public async void Start(bool instantly)
    {
        _updateTimer = new Timer(UpdateChat, null, instantly ? TimeSpan.Zero : UpdatePeriod, UpdatePeriod);
    }
    
    private void UpdateChat(object parameter)
    {
        Console.WriteLine("Time to update");
        
        _appClient.GetMessagesAsync(_updateMethod);
    }
    
    public async void Stop()
    {
        await _updateTimer.DisposeAsync();
    }
    
}