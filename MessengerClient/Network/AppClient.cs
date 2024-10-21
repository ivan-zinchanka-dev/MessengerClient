using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using MessengerClient.Core.Infrastructure;
using MessengerClient.Core.Models;

namespace MessengerClient.Network;

public class AppClient : IDisposable
{
    private TcpClient _tcpClient;

    // TODO Reconnect logic
    
    public event Action ErrorCaptured;

    public bool IsConnected { get; private set; }

    public async Task<bool> TryConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync("127.0.0.1", 8888);
            
            Console.WriteLine("Connected to server");

            IsConnected = true;
        }
        catch (SocketException ex)
        {
            IsConnected = false;
            ErrorCaptured?.Invoke();
        }

        return IsConnected;
    }

    public async Task<bool> TrySignUpAsync(User user)
    {
        if (!IsConnected)
        {
            return false;
        }

        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignUp, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        return JsonSerializer.Deserialize<bool>(response.JsonDataString);
    }
    
    public async Task<bool> TryLoginAsync(User user)
    {
        if (!IsConnected)
        {
            return false;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignIn, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        return JsonSerializer.Deserialize<bool>(response.JsonDataString);
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        if (!IsConnected)
        {
            return new List<Message>();
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        Query query = new Query(QueryHeader.UpdateChat);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        return JsonSerializer.Deserialize<List<Message>>(response.JsonDataString);
    }
    
    public async Task<bool> PostMessagesAsync(Message message)
    {
        if (!IsConnected)
        {
            return false;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        Query query = new Query(QueryHeader.PostMessage, JsonSerializer.Serialize(message));
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        return JsonSerializer.Deserialize<bool>(response.JsonDataString);
    }
    
    public async Task QuitAsync()
    {
        if (!IsConnected)
        {
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());

        Query query = new Query(QueryHeader.Quit);
        await networkAdaptor.SendQueryAsync(query);
    }
    
    public void Dispose()
    {
        _tcpClient?.Close();
    }
}