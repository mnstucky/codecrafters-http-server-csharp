using System.Net.Sockets;

public static class RequestUtilities
{
    public static async Task HandleRequestAsync(TcpClient client)
    {
        Console.WriteLine("Processing Request.");
        using var stream = client.GetStream();
        var requestBuffer = new byte[1024];
        await stream.ReadAsync(requestBuffer);
        var requestString = System.Text.Encoding.UTF8.GetString(requestBuffer);
        var request = ParseRequest(requestString);

        if (request.HttpMethods == HttpMethods.Invalid || request.Path is null)
        {
            var badRequest = System.Text.Encoding.ASCII.GetBytes(Endpoints.Response400 + Endpoints.HeaderEnd);
            stream.Write(badRequest, 0, badRequest.Length);
        }

        var message = request.Path?.FirstOrDefault() switch
        {
            null => Endpoints.GetEmptyOk(),
            "echo" => Endpoints.GetEcho(request),
            "user-agent" => Endpoints.GetUserAgent(request),
            "files" =>
                request.HttpMethods switch
                {
                    HttpMethods.GET => Endpoints.GetFiles(request),
                    HttpMethods.POST => Endpoints.AddFiles(request),
                    _ => Endpoints.Response400 + Endpoints.HeaderEnd
                }
            ,
            _ => Endpoints.GetNotFound()
        };
        var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
    }

    public static RequestDetails ParseRequest(string requestString)
    {
        var request = requestString.Split("\r\n");
        var requestLine = request.FirstOrDefault();
        var requestLineParts = requestLine?.Split(" ");
        var httpMethodString = requestLineParts?.FirstOrDefault();
        var httpMethod = httpMethodString switch
        {
            "GET" => HttpMethods.GET,
            "POST" => HttpMethods.POST,
            _ => HttpMethods.Invalid
        };
        var requestTarget = requestLineParts?.Skip(1)
            .FirstOrDefault()?
            .Split("/", StringSplitOptions.RemoveEmptyEntries);
        var headers = new Dictionary<string, string>();
        var body = "";
        var bodyIsNext = false;
        foreach (var header in request.Skip(1))
        {
            if (header == Endpoints.HeaderEnd)
            {
                bodyIsNext = true;
                continue;
            }
            if (bodyIsNext)
            {
                body = header;
                continue;
            }
            if (!header.Contains(": "))
            {
                continue;
            }
            headers.Add(header[..header.IndexOf(':')], header[(header.IndexOf(':') + 2)..]);
        }

        return new RequestDetails(httpMethod, requestTarget, headers, body);
    }
}