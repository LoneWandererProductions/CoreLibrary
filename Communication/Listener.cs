using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Communication
{
    public sealed class Listener
    {
        private readonly int _port = 12345;
        private readonly TcpListener _tcpListener;
        private bool isRunning;

        public Listener(int port)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            isRunning = true;
            _tcpListener.Start();
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
                    if (_tcpListener.Pending()) // Non-blocking accept
                    {
                        var client = _tcpListener.AcceptTcpClient();
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
            _tcpListener.Stop();
            Console.WriteLine("Server stopped.");
        }

        private static void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();

            // Respond with a simple message (acting as the "ping response")
            var response = "PONG";
            var buffer = Encoding.ASCII.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);

            // Close the connection
            client.Close();
            Console.WriteLine("Responded to a ping.");
        }
    }
}
