using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new(IPAddress.Any, 4221);
server.Start();
var client = server.AcceptTcpClient();
var stream = client.GetStream();
var request = "";
var requestBuffer = new byte[256];
var requestPosition = 0;
while (stream.Read(requestBuffer, requestPosition, 256) > 0)
{
    request += System.Text.Encoding.Default.GetString(requestBuffer);
    requestPosition += 256;
}
Console.WriteLine(request);
var message = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
stream.Write(message, 0, message.Length);
