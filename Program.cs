using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting; // Add this using directive

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string certificatePath = "server.pfx";
            string certificatePassword = "12345";
            int port = 12345;

            try
            {
                Console.WriteLine("Initializing server...");
                Server server = new Server(certificatePath, certificatePassword);
                server.Start(port);
                Console.WriteLine("Server started. Press Enter to exit...");

                // This will keep the application running until Enter is pressed
                await Task.Run(() => Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
