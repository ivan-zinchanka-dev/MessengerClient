using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using MessengerClient.Core.Infrastructure;
using MessengerClient.Core.Models;

namespace MessengerClient.Network;

public class AppClient : IDisposable
{
    private TcpClient _tcpClient;

    // TODO Reconnect logic
    
    public event Action ErrorCaptured;

    public bool IsConnected { get; private set; }

    public async void TryConnectAsync(Action<bool> onCompleteCallback)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync("127.0.0.1", 8888);
            
            Console.WriteLine("Connected to server");

            IsConnected = true;
            onCompleteCallback?.Invoke(IsConnected);
        }
        catch (SocketException ex)
        {
            IsConnected = false;
            onCompleteCallback?.Invoke(IsConnected);
            ErrorCaptured?.Invoke();
        }
    }

    public async void TrySignUpAsync(User user, Action<bool> onCompleteCallback)
    {
        if (!IsConnected)
        {
            onCompleteCallback?.Invoke(IsConnected);
            return;
        }

        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignUp, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        bool success = JsonSerializer.Deserialize<bool>(response.JsonDataString);
        onCompleteCallback?.Invoke(success);
    }
    
    public async void TryLoginAsync(User user, Action<bool> onCompleteCallback)
    {
        if (!IsConnected)
        {
            onCompleteCallback?.Invoke(IsConnected);
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignIn, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        bool success = JsonSerializer.Deserialize<bool>(response.JsonDataString);
        onCompleteCallback?.Invoke(success);
    }

    public async void GetMessagesAsync(Action<List<Message>> onCompleteCallback)
    {
        if (!IsConnected)
        {
            onCompleteCallback?.Invoke(new List<Message>());
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        Query query = new Query(QueryHeader.UpdateChat);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        List<Message> messagesList = JsonSerializer.Deserialize<List<Message>>(response.JsonDataString);
        onCompleteCallback?.Invoke(messagesList);
    }
    
    public async void PostMessagesAsync(Message message, Action<bool> onCompleteCallback)
    {
        if (!IsConnected)
        {
            onCompleteCallback?.Invoke(IsConnected);
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        Query query = new Query(QueryHeader.PostMessage, JsonSerializer.Serialize(message));
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        bool success = JsonSerializer.Deserialize<bool>(response.JsonDataString);
        onCompleteCallback?.Invoke(success);
    }
    
    public async void QuitAsync(Action onCompleteCallback)
    {
        if (!IsConnected)
        {
            onCompleteCallback?.Invoke();
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());

        Query query = new Query(QueryHeader.Quit);
        await networkAdaptor.SendQueryAsync(query);
        
        onCompleteCallback?.Invoke();
    }
    
    public void Dispose()
    {
        _tcpClient?.Close();
    }
}