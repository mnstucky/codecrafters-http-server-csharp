public record RequestDetails(HttpMethods HttpMethods, string[]? Path, Dictionary<string, string> Headers, string? Body)
{
    public bool AcceptsGzip => Headers.TryGetValue("Accept-Encoding", out var encoding) && encoding.Contains("gzip");
};