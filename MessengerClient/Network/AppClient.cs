using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessengerClient.Core.Infrastructure;
using MessengerClient.Core.Models;
using Microsoft.Extensions.Hosting;

namespace MessengerClient.Network;

public class AppClient : BackgroundService
{
    private readonly AppSharedOptions _sharedOptions;
    private readonly TcpClient _tcpClient;
    
    public ChatUpdater ChatUpdater { get; private set; }
    public bool IsConnected { get; private set; }
    
    public AppClient(AppSharedOptions sharedOptions)
    {
        _sharedOptions = sharedOptions;
        ChatUpdater = new ChatUpdater(GetMessagesAsync);
        _tcpClient = new TcpClient();
        _sharedOptions.AppClient = this;
    }
    
    public async Task<bool> TryConnectAsync()
    {
        try
        {
            await _tcpClient.ConnectAsync(_sharedOptions.RemoteEndPoint);
            Console.WriteLine("Connected to server");

            IsConnected = true;
        }
        catch (SocketException ex)
        {
            IsConnected = false;
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
    
    public async Task<bool> TrySignInAsync(User user)
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
    
    public async Task<bool> PostMessageAsync(Message message)
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
    
    public override void Dispose()
    {
        _tcpClient?.Close();
        base.Dispose();
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await TryConnectAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await QuitAsync();
        Dispose();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TryConnectAsync();
    }
}