using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Server
{
    public class ClientHandler
    {
        public TcpClient Client { get; }
        public SslStream Stream { get; }

        public ClientHandler(TcpClient client, X509Certificate certificate)
        {
            Client = client;
            Stream = new SslStream(client.GetStream(), false);
            // Authenticate as server using the provided certificate
            Stream.AuthenticateAsServer(certificate, false, SslProtocols.Tls12, false);
        }

        public async Task<string> ReadMessage()
        {
            // Read the 4-byte length prefix
            byte[] lengthBytes = new byte[4];
            await Stream.ReadAsync(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            // Read the message data
            byte[] messageBytes = new byte[length];
            await Stream.ReadAsync(messageBytes, 0, length);
            return Encoding.UTF8.GetString(messageBytes);
        }

        public async Task SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            // Send length prefix followed by message data
            await Stream.WriteAsync(lengthBytes, 0, 4);
            await Stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
    }
}
