using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessengerCoreLibrary.Infrastructure;
using MessengerCoreLibrary.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessengerClient.Network;

public class AppClient : BackgroundService
{
    private readonly AppSharedOptions _sharedOptions;
    private readonly TcpClient _tcpClient;
    private readonly ILogger<AppClient> _logger;
    
    public User CurrentUser { get; private set; }
    public ChatUpdater ChatUpdater { get; private set; }
    public bool IsConnected { get; private set; }
    
    public AppClient(AppSharedOptions sharedOptions, ILogger<AppClient> logger)
    {
        _sharedOptions = sharedOptions;
        _logger = logger;
        _tcpClient = new TcpClient();
        ChatUpdater = new ChatUpdater(GetMessagesAsync);
        
        _sharedOptions.AppClient = this;
    }
    
    public async Task<bool> TryConnectAsync()
    {
        try
        {
            _logger.LogInformation("Connecting to server...");
            await _tcpClient.ConnectAsync(_sharedOptions.RemoteEndPoint);
            _logger.LogInformation("The client is connected to the server.");
            IsConnected = true;
        }
        catch (SocketException)
        {
            IsConnected = false;
        }

        return IsConnected;
    }

    public async Task<bool> TrySignUpAsync(User user)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Attempt to sign up when the server is disconnected.");
            return false;
        }

        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignUp, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        bool result = JsonSerializer.Deserialize<bool>(response.JsonDataString);

        if (result)
        {
            CurrentUser = user;
        }

        return result;
    }
    
    public async Task<bool> TrySignInAsync(User user)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Attempt to sign in when the server is disconnected.");
            return false;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());
        
        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.SignIn, jsonMessageBuffer);
        await networkAdaptor.SendQueryAsync(query);
        
        Response response = await networkAdaptor.ReceiveResponseAsync();
        bool result = JsonSerializer.Deserialize<bool>(response.JsonDataString);
        
        if (result)
        {
            CurrentUser = user;
        }

        return result;
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Attempt to get messages when the server is disconnected.");
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
            _logger.LogWarning("Attempt to post a message when the server is disconnected.");
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
            _logger.LogWarning("Attempt to quit when the server is disconnected.");
            return;
        }
        
        NetworkAdaptor networkAdaptor = new NetworkAdaptor(_tcpClient.GetStream());

        Query query = new Query(QueryHeader.Quit);
        await networkAdaptor.SendQueryAsync(query);

        IsConnected = false;
    }
    
    public override void Dispose()
    {
        IsConnected = false;
        _tcpClient?.Close();
        
        _logger.LogInformation("The client is shut down.");
        
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