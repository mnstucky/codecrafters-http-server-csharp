using System.Net;
using System.Net.Sockets;

TcpListener server = new(IPAddress.Any, 4221);

try
{
    server.Start();
    while (true)
    {
        var client = server.AcceptTcpClient();
        Task.Run(() => HandleRequest(client));
    }
}
finally
{
    server.Dispose();
}

void HandleRequest(TcpClient client)
{
    using var stream = client.GetStream();
    var requestBuffer = new byte[1024];
    stream.Read(requestBuffer);
    var request = System.Text.Encoding.UTF8.GetString(requestBuffer)
        .Split("\r\n");
    var requestLine = request.FirstOrDefault();
    var requestLineParts = requestLine?.Split(" ");
    var httpMethod = requestLineParts?.FirstOrDefault();
    var requestTarget = requestLineParts?.Skip(1)
        .FirstOrDefault()?
        .Split("/", StringSplitOptions.RemoveEmptyEntries);
    var headers = new Dictionary<string, string>();
    foreach (var header in request.Skip(1))
    {
        if (!header.Contains(": "))
        {
            continue;
        }
        headers.Add(header[..header.IndexOf(':')], header[(header.IndexOf(':') + 2)..]);
    }

    if (string.IsNullOrWhiteSpace(requestLine) || string.IsNullOrWhiteSpace(httpMethod)
        || httpMethod != "GET" || requestTarget is null)
    {
        var badRequest = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n");
        stream.Write(badRequest, 0, badRequest.Length);
    }

    var message = requestTarget?.FirstOrDefault() switch
    {
        null => "HTTP/1.1 200 OK\r\n\r\n",
        "echo" => $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {string.Join("/", requestTarget.Skip(1)).Length}\r\n\r\n" +
            string.Join("/", requestTarget.Skip(1)),
        "user-agent" => $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {headers.GetValueOrDefault("User-Agent")?.Length ?? 0}\r\n\r\n" +
            headers.GetValueOrDefault("User-Agent"),
        _ => "HTTP/1.1 404 Not Found\r\n\r\n"
    };
    var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
    stream.Write(messageBytes, 0, messageBytes.Length);
}