using System.IO.Compression;
using System.Text;

public static class Endpoints
{
    public const string Response200 = "HTTP/1.1 200 OK\r\n";

    public const string Response201 = "HTTP/1.1 201 Created\r\n";

    public const string Response400 = "HTTP/1.1 400 Bad Request\r\n";

    public const string Response404 = "HTTP/1.1 404 Not Found\r\n";

    public const string Response500 = "HTTP/1.1 500 Internal Server Error\r\n";

    public const string HeaderEnd = "\r\n";

    public static byte[] GetNotFound()
    {
        return Encoding.UTF8.GetBytes(Response404 + HeaderEnd);
    }

    public static byte[] GetEmptyOk()
    {
        return Encoding.UTF8.GetBytes(Response200 + HeaderEnd);
    }

    public static byte[] GetEcho(RequestDetails request)
    {
        if (request.Path is null)
        {
            return GetEmptyOk();
        }
        var body = Encoding.UTF8.GetBytes(string.Join("/", request.Path.Skip(1)));
        if (request.AcceptsGzip)
        {
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(body, 0, body.Length);
            }
            body = outputStream.ToArray();
        }
        var header = Encoding.UTF8.GetBytes(Response200 +
            $"Content-Type: text/plain\r\n" +
            (request.AcceptsGzip ? "Content-Encoding: gzip\r\n" : "") +
            $"Content-Length: {body?.Length ?? 0}\r\n" +
            HeaderEnd);
        return [.. header, .. body ?? []];
    }

    public static byte[] GetUserAgent(RequestDetails request)
    {
        return Encoding.UTF8.GetBytes(Response200 +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {request.Headers.GetValueOrDefault("User-Agent")?.Length ?? 0}\r\n" +
            HeaderEnd +
            request.Headers.GetValueOrDefault("User-Agent"));
    }

    public static byte[] GetFiles(RequestDetails request)
    {
        if (request.Path is null)
        {
            return Encoding.UTF8.GetBytes(Response500 + HeaderEnd);
        }
        var directory = CommandLineUtilities.GetFilesDirectory();
        var file = request.Path.Skip(1).FirstOrDefault();
        if (!File.Exists(directory + file))
        {
            return Encoding.UTF8.GetBytes(Response404 + HeaderEnd);
        }
        try
        {
            var bytes = File.ReadAllBytes(directory + file);
            return
            [
                .. Encoding.UTF8.GetBytes(Response200 +
                            $"Content-Type: application/octet-stream\r\n" +
                            $"Content-Length: {bytes.Length}\r\n" +
                            HeaderEnd),
                .. bytes,
            ];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Encoding.UTF8.GetBytes(Response500 + HeaderEnd);
        }
    }

    public static byte[] AddFiles(RequestDetails request)
    {
        if (request.Path is null)
        {
            return Encoding.UTF8.GetBytes(Response500 + HeaderEnd);
        }
        var directory = CommandLineUtilities.GetFilesDirectory();
        var file = request.Path.Skip(1).FirstOrDefault();
        if (File.Exists(directory + file) || request.Body is null)
        {
            return Encoding.UTF8.GetBytes(Response400 + HeaderEnd);
        }
        var bytes = Encoding.UTF8.GetBytes(request.Body);
        if (!request.Headers.TryGetValue("Content-Length", out var byteLengthString)
            || !int.TryParse(byteLengthString, out var byteLength))
        {
            return Encoding.UTF8.GetBytes(Response400 + HeaderEnd);
        }
        File.WriteAllBytes(directory + file, bytes[..byteLength]);
        return Encoding.UTF8.GetBytes(Response201 + HeaderEnd);
    }
}