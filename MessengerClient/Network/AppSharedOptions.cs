using System.Net;

namespace MessengerClient.Network;

public class AppSharedOptions
{
    public IPEndPoint RemoteEndPoint { get; private set; }
    public AppClient AppClient { get; set; }

    public AppSharedOptions(IPEndPoint remoteEndPoint)
    {
        RemoteEndPoint = remoteEndPoint;
    }
}