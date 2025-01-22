public static class Endpoints
{
    public const string Response200 = "HTTP/1.1 200 OK\r\n";

    public const string Response201 = "HTTP/1.1 201 Created\r\n";

    public const string Response400 = "HTTP/1.1 400 Bad Request\r\n";

    public const string Response404 = "HTTP/1.1 404 Not Found\r\n";

    public const string Response500 = "HTTP/1.1 500 Internal Server Error\r\n";

    public const string HeaderEnd = "\r\n";

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
            return Response500 + HeaderEnd;
        }
        var directory = CommandLineUtilities.GetFilesDirectory();
        var file = request.Path.Skip(1).FirstOrDefault();
        if (!File.Exists(directory + file))
        {
            return Response404 + HeaderEnd;
        }
        try
        {
            var bytes = File.ReadAllBytes(directory + file);
            return Response200 +
            $"Content-Type: application/octet-stream\r\n" +
            $"Content-Length: {bytes.Length}\r\n" +
            HeaderEnd +
            string.Join("", System.Text.Encoding.UTF8.GetChars(bytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Response500 + HeaderEnd;
        }
    }

    public static string AddFiles(RequestDetails request)
    {
        if (request.Path is null)
        {
            return Response500 + HeaderEnd;
        }
        var directory = CommandLineUtilities.GetFilesDirectory();
        var file = request.Path.Skip(1).FirstOrDefault();
        if (File.Exists(directory + file) || request.Body is null)
        {
            return Response400 + HeaderEnd;
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(request.Body);
        if (!request.Headers.TryGetValue("Content-Length", out var byteLengthString)
            || !int.TryParse(byteLengthString, out var byteLength))
        {
            return Response400 + HeaderEnd;
        }
        File.WriteAllBytes(directory + file, bytes[..byteLength]);
        return Response201 + HeaderEnd;
    }
}