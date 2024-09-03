using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Sever
{
    public class Program
    {
        private static TcpListener _server;
        private static List<TcpClient> _clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            _server = new TcpListener(IPAddress.Any, 5000);
            _server.Start();
            Console.WriteLine("Server started on port 5000.");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();

            Console.WriteLine("Press Enter to stop the server...");
            Console.ReadLine();
        }

        private static void AcceptClients()
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                _clients.Add(client);
                Console.WriteLine("Client connected.");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (message.Contains("Disconnected"))
                    {
                        Console.WriteLine("Client disconnected.");
                        ShowMessage (message);
                        break;
                    }
                    else 
                    {
                        Console.WriteLine("Received: " + message);
                        ShowMessage(message);
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            finally
            {
                _clients.Remove(client);
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        private static void ShowMessage(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            foreach (var client in _clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
    }
}