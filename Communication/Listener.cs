using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Communication
{
    public class Listener
    {
        private System.Net.Sockets.TcpListener tcpListener;
        private bool isRunning;
        private readonly int _port = 12345;

        public Listener(int port)
        {
            _port = port;
            tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Any, port);
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            isRunning = true;
            tcpListener.Start();
            Console.WriteLine($"Listening on port {_port}...");

            while (isRunning)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopListening();
                    return;
                }

                try
                {
                    if (tcpListener.Pending()) // Non-blocking accept
                    {
                        var client = tcpListener.AcceptTcpClient();
                        ThreadPool.QueueUserWorkItem(HandleClient, client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting connection: {ex.Message}");
                }
            }
        }

        public void StopListening()
        {
            isRunning = false;
            tcpListener.Stop();
            Console.WriteLine("Server stopped.");
        }

        private static void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();

            // Respond with a simple message (acting as the "ping response")
            string response = "PONG";
            byte[] buffer = Encoding.ASCII.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);

            // Close the connection
            client.Close();
            Console.WriteLine("Responded to a ping.");
        }
    }
}
