using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessengerClient.Core.Infrastructure;

namespace MessengerClient;

public class NetworkAdaptor
{
    private StreamReader _reader;
    private StreamWriter _writer;

    public NetworkAdaptor(NetworkStream stream)
    {
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
    }
    
    public async Task SendQueryAsync(Query query)
    {
        await _writer.WriteAsync(query.ToString());
        await _writer.FlushAsync();
    }
    
    public async Task<Response> ReceiveResponseAsync()
    {
        string rawLine = await _reader.ReadLineAsync();
        return Response.FromRawLine(rawLine);
    }
    
}