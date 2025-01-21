using System.Net;
using System.Net.Sockets;

Console.WriteLine("Starting Server.");
TcpListener server = new(IPAddress.Any, 4221);

try
{
    server.Start();
    Console.WriteLine("Listening on Port 4221.");
    while (true)
    {
        var client = await server.AcceptTcpClientAsync();
        _ = RequestUtilities.HandleRequestAsync(client);
    }
}
finally
{
    server.Dispose();
}
