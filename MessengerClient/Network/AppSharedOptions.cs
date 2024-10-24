using System.Net;

namespace MessengerClient.Network;

public class AppSharedOptions
{
    public IPEndPoint RemoteEndPoint { get; set; }
    public AppClient AppClient { get; set; }
}