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
        _ = HandleRequestAsync(client);
    }
}
finally
{
    server.Dispose();
}

async Task HandleRequestAsync(TcpClient client)
{
    Console.WriteLine("Processing Request.");
    using var stream = client.GetStream();
    var requestBuffer = new byte[1024];
    await stream.ReadAsync(requestBuffer);
    var requestString = System.Text.Encoding.UTF8.GetString(requestBuffer);
    var request = RequestUtilities.ParseRequest(requestString);

    if (request.HttpMethods == HttpMethods.Invalid || request.Path is null)
    {
        var badRequest = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n");
        stream.Write(badRequest, 0, badRequest.Length);
    }

    var message = request.Path?.FirstOrDefault() switch
    {
        null => "HTTP/1.1 200 OK\r\n\r\n",
        "echo" => $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {string.Join("/", request.Path.Skip(1)).Length}\r\n\r\n" +
            string.Join("/", request.Path.Skip(1)),
        "user-agent" => $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {request.Headers.GetValueOrDefault("User-Agent")?.Length ?? 0}\r\n\r\n" +
            request.Headers.GetValueOrDefault("User-Agent"),
        _ => "HTTP/1.1 404 Not Found\r\n\r\n"
    };
    var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
    stream.Write(messageBytes, 0, messageBytes.Length);
}