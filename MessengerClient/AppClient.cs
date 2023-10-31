﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using MessengerClient.Core.Infrastructure;
using MessengerClient.Core.Models;

namespace MessengerClient;

public class AppClient : IDisposable
{
    private TcpClient _tcpClient;

    public event Action ErrorCaptured;
    
    public async void TryStartAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync("127.0.0.1", 8888);
            
            Console.WriteLine("Connected to server");
        }
        catch (SocketException ex)
        {
            ErrorCaptured?.Invoke();
        }
    }

    public async Task<bool> TryLoginAsync(User user)
    {
        NetworkStream networkStream = _tcpClient.GetStream();
        StreamReader reader = new StreamReader(networkStream);
        StreamWriter writer = new StreamWriter(networkStream);

        string jsonMessageBuffer = JsonSerializer.Serialize(user);
        Query query = new Query(QueryHeader.Login, jsonMessageBuffer);
        
        await writer.WriteAsync(query.ToString());
        await writer.FlushAsync();
        
        string rawLine = await reader.ReadLineAsync();
        Response response = Response.FromRawLine(rawLine);

        bool success = JsonSerializer.Deserialize<bool>(response.JsonDataString);
        return success;
    }


    /*public async Task Run()
    {
        try
        {
            using TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1", 8888);
            NetworkStream networkStream = tcpClient.GetStream();
        
            Console.WriteLine("Connected");

            
            StreamWriter writer = new StreamWriter(networkStream);
            
            string rawLine = JsonSerializer.Serialize(CreateTestMessage());
            
            Console.WriteLine("String: " + rawLine);

            Query query = new Query(QueryHeader.PostMessage, rawLine);
            
            await writer.WriteAsync(query.ToString());
            await writer.FlushAsync();
            
            
            Console.WriteLine("Wrote");
        
            StreamReader reader = new StreamReader(networkStream);
            Console.WriteLine("GetStream");
            
            rawLine = await reader.ReadLineAsync();
            Console.WriteLine("Read");
            
            Response response = Response.FromRawLine(rawLine);
            List<Message> chat = JsonSerializer.Deserialize<List<Message>>(response.JsonDataString);

            Console.WriteLine("Read");
        
            foreach (Message message in chat)
            {
                Console.WriteLine(message);
            }
            
            //tcpClient.Close();


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        
        
    }*/

    public void Dispose()
    {
        _tcpClient?.Close();
    }
}