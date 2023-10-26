using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessengerClient.Core.Models;

namespace MessengerClient;

public class Client
{
    private readonly IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

    public User CurrentUser { get; private set; }

    private TcpClient _tcpClient;

    public async Task Connect()
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(_endPoint);
        
        
    }
    
    
    /*public async Task TestLogin(User user)
    {
        


    }*/

    public void SendTestMessage()
    {
        try
        {
            


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }
    
    private Message CreateTestMessage()
    {
        return new Message()
        {
            Sender = new User()
            {
                Nickname = "Nick",
                Password = "111111"
            },

            Receiver = new User()
            {
                Nickname = "Mike",
                Password = "000000"
            },
            
            Text = "Just a test message.",
            
            PostDateTime = DateTime.UtcNow,
        };
        
    }

}