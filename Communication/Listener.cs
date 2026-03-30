/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Listener.cs
 * PURPOSE:     Simple port checker. Just listens to a port and answers with a fixed message.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    ///     Simple Port Listener
    /// </summary>
    public sealed class Listener
    {
        /// <summary>
        ///     The port
        /// </summary>
        private readonly int _port;

        /// <summary>
        ///     The TCP listener
        /// </summary>
        private readonly TcpListener _tcpListener;

        /// <summary>
        ///     The is running
        /// </summary>
        private bool _isRunning;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Listener" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public Listener(int port)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        ///     Starts the listening.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            _isRunning = true;
            _tcpListener.Start();
            Trace.WriteLine(ComResource.MessageListening, _port.ToString());

            try
            {
                // We use the CancellationToken to stop the loop gracefully
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    // This blocks asynchronously: 0% CPU usage while waiting
                    using var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);

                    // Fire and forget the handler so we can accept the next client immediately
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal exit when token is cancelled
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ComResource.ErrorAcceptingCommunication, ex.Message);
            }
            finally
            {
                StopListening();
            }
        }

        /// <summary>
        ///     Stops the listening.
        /// </summary>
        public void StopListening()
        {
            _isRunning = false;
            _tcpListener.Stop();
            Trace.WriteLine(ComResource.ServerStatusStop);
        }

        /// <summary>
        ///     Handles the client.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client) // Ensures the client is closed
                await using (var stream = client.GetStream()) // Ensures the stream is closed
                {
                    var buffer = Encoding.ASCII.GetBytes(ComResource.AnswerMessage);

                    // Use Async write
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();

                    Console.WriteLine(ComResource.MessageAnswer);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error handling client: " + ex.Message);
            }
        }
    }
}
