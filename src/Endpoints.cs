public static class Endpoints
{
    const string Response200 = "HTTP/1.1 200 OK\r\n\r\n";

    const string Response404 = "HTTP/1.1 404 Not Found\r\n\r\n";

    const string Response500 = "HTTP/1.1 500 Internal Server Error\n\r\n";

    public static string GetNotFound()
    {
        return Response404;
    }

    public static string GetEmptyOk()
    {
        return Response200;
    }

    public static string GetEcho(RequestDetails request)
    {
        if (request.Path is null)
        {
            return GetEmptyOk();
        }
        return Response200 +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {string.Join("/", request.Path.Skip(1)).Length}\r\n\r\n" +
            string.Join("/", request.Path.Skip(1));
    }

    public static string GetUserAgent(RequestDetails request)
    {
        return Response200 +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {request.Headers.GetValueOrDefault("User-Agent")?.Length ?? 0}\r\n\r\n" +
            request.Headers.GetValueOrDefault("User-Agent");
    }

    public static string GetFiles(RequestDetails request)
    {
        var directory = CommandLineUtilities.GetFilesDirectory();
        if (!File.Exists(directory))
        {
            return Response404;
        }
        try
        {
            var bytes = File.ReadAllBytes(directory);
            return Response200 +
            $"Content-Type: application/octet-stream\r\n" +
            $"Content-Length: {bytes.Length}\r\n\r\n" +
            bytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Response500;
        }
    }
}