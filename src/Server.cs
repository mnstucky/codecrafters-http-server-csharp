using System.Net;
using System.Net.Sockets;

TcpListener server = new(IPAddress.Any, 4221);

try
{
    server.Start();
    while (true)
    {
        using var client = await server.AcceptTcpClientAsync();
        using var stream = client.GetStream();
        var requestBuffer = new byte[1024];
        await stream.ReadAsync(requestBuffer);
        var request = System.Text.Encoding.UTF8.GetString(requestBuffer);
        var requestLine = request.Split("\r\n").FirstOrDefault();
        var requestLineParts = requestLine?.Split(" ");
        var httpMethod = requestLineParts?.FirstOrDefault();
        var requestTarget = requestLineParts?.Skip(1)
            .FirstOrDefault()?
            .Split("/", StringSplitOptions.RemoveEmptyEntries);

        if (string.IsNullOrWhiteSpace(requestLine) || string.IsNullOrWhiteSpace(httpMethod)
            || httpMethod != "GET" || requestTarget is null)
        {
            var badRequest = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n");
            stream.Write(badRequest, 0, badRequest.Length);
            continue;
        }

        var message = requestTarget.FirstOrDefault() switch
        {
            null => "HTTP/1.1 200 OK\r\n\r\n",
            "echo" => $"HTTP/1.1 200 OK\r\n" +
                $"Content-Type: text/plain\r\n" +
                $"Content-Length: {string.Join("/", requestTarget.Skip(1)).Length}\r\n\r\n" +
                string.Join("/", requestTarget.Skip(1)),
            _ => "HTTP/1.1 404 Not Found\r\n\r\n"
        };
        var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
    }
}
finally
{
    server.Dispose();
}
