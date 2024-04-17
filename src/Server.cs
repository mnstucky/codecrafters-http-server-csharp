using System.Net;
using System.Net.Sockets;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();
var client = server.AcceptTcpClient();
var stream = client.GetStream();
var request = "";
var requestBuffer = new byte[256];
while (stream.Read(requestBuffer, 0, 256) > 0)
{
    request += System.Text.Encoding.Default.GetString(requestBuffer);
}
var message = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
stream.Write(message, 0, message.Length);
