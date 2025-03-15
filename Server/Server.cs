using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Server
{
    public class Server
    {
        private TcpListener listener = null!;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private X509Certificate certificate;

        public Server(string certPath, string certPass)
        {
            // Load the SSL certificate (e.g., a .pfx file)
            certificate = new X509Certificate2(certPath, certPass);
        }

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server started on port {port}");
            // Begin accepting clients asynchronously
            listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        private void AcceptClient(IAsyncResult ar)
        {
            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                ClientHandler handler = new ClientHandler(client, certificate);
                lock (clients)
                {
                    clients.Add(handler);
                }
                Console.WriteLine("Client connected");
                // Handle this client in a separate task
                Task.Run(() => HandleClient(handler));
                // Continue accepting new clients
                listener.BeginAcceptTcpClient(AcceptClient, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }

        private async Task HandleClient(ClientHandler handler)
        {
            try
            {
                while (true)
                {
                    string message = await handler.ReadMessage();
                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }
                    Console.WriteLine($"Received: {message}");
                    // Broadcast the message to all other clients
                    List<ClientHandler> clientsToSend;
                    lock (clients)
                    {
                        clientsToSend = clients.Where(c => c != handler).ToList();
                    }
                    var tasks = clientsToSend.Select(async other =>
                    {
                        try
                        {
                            await other.SendMessage(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending to client: {ex.Message}");
                            lock (clients)
                            {
                                clients.Remove(other);
                            }
                            other.Client.Close();
                        }
                    }).ToList();
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected: {ex.Message}");
                lock (clients)
                {
                    clients.Remove(handler);
                }
                handler.Client.Close();
            }
        }
    }
}
