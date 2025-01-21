public static class Endpoints
{
    const string Response200 = "HTTP/1.1 200 OK\r\n";

    const string Response404 = "HTTP/1.1 404 Not Found\r\n";

    const string Response500 = "HTTP/1.1 500 Internal Server Error\r\n";

    const string HeaderEnd = "\r\n";

    public static string GetNotFound()
    {
        return Response404 + HeaderEnd;
    }

    public static string GetEmptyOk()
    {
        return Response200 + HeaderEnd;
    }

    public static string GetEcho(RequestDetails request)
    {
        if (request.Path is null)
        {
            return GetEmptyOk();
        }
        return Response200 +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {string.Join("/", request.Path.Skip(1)).Length}\r\n" +
            HeaderEnd +
            string.Join("/", request.Path.Skip(1));
    }

    public static string GetUserAgent(RequestDetails request)
    {
        return Response200 +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {request.Headers.GetValueOrDefault("User-Agent")?.Length ?? 0}\r\n" +
            HeaderEnd +
            request.Headers.GetValueOrDefault("User-Agent");
    }

    public static string GetFiles(RequestDetails request)
    {
        if (request.Path is null)
        {
            return Response500;
        }
        var directory = CommandLineUtilities.GetFilesDirectory();
        var file = request.Path.Skip(1).FirstOrDefault();
        if (!File.Exists(directory + file))
        {
            return Response404;
        }
        try
        {
            var bytes = File.ReadAllBytes(directory + file);
            return Response200 +
            $"Content-Type: application/octet-stream\r\n" +
            $"Content-Length: {bytes.Length}\r\n" +
            HeaderEnd +
            System.Text.Encoding.UTF8.GetChars(bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Response500;
        }
    }
}