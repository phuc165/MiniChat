using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class ChatClient
    {
        private TcpClient client;
        private SslStream stream;

        public async Task Connect(string serverIp, int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIp, port);
            stream = new SslStream(client.GetStream(), false, ValidateServerCertificate);
            // Authenticate as client; "serverName" should match the certificate's CN
            await stream.AuthenticateAsClientAsync("serverName");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For testing, accept any certificate
            return true;
            // In production, validate the certificate properly (e.g., check sslPolicyErrors)
        }

        public async Task SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            await stream.WriteAsync(lengthBytes, 0, 4);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        public async Task<string> ReceiveMessage()
        {
            byte[] lengthBytes = new byte[4];
            await stream.ReadAsync(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] messageBytes = new byte[length];
            await stream.ReadAsync(messageBytes, 0, length);
            return Encoding.UTF8.GetString(messageBytes);
        }

        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
