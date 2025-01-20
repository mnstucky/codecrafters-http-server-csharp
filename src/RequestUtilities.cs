public static class RequestUtilities
{
    public static RequestDetails ParseRequest(string requestString)
    {
        var request = requestString.Split("\r\n");
        var requestLine = request.FirstOrDefault();
        var requestLineParts = requestLine?.Split(" ");
        var httpMethodString = requestLineParts?.FirstOrDefault();
        var httpMethod = httpMethodString switch
        {
            "GET" => HttpMethods.GET,
            _ => HttpMethods.Invalid
        };
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

        return new RequestDetails(httpMethod, requestTarget, headers);
    }
}