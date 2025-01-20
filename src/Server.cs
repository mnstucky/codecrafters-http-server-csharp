using System.Net;
using System.Net.Sockets;

TcpListener server = new(IPAddress.Any, 4221);

try
{
    server.Start();
    while (true)
    {
        using var client = await server.AcceptTcpClientAsync();
        await RequestProcessor.Process(client);
    }
}
finally
{
    server.Dispose();
}
