/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        TcpServerBase.cs
 * PURPOSE:     Base class for TCP servers, providing common functionality for starting, stopping, and accepting clients.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    /// Base class for TCP servers, providing common functionality for starting, stopping, and accepting clients.
    /// </summary>
    public abstract class TcpServerBase
    {
        /// <summary>
        /// The port
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// The TCP listener
        /// </summary>
        private TcpListener _tcpListener;

        /// <summary>
        /// The is running
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServerBase"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        protected TcpServerBase(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Starts the asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task StartAsync(CancellationToken token)
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();
            _isRunning = true;
            Trace.WriteLine($"Server started on port {_port}");

            try
            {
                while (!token.IsCancellationRequested && _isRunning)
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(token);
                    // Fire and forget, but handle the client specifically
                    _ = HandleClientInternalAsync(client, token);
                }
            }
            catch (OperationCanceledException)
            {
                /* Graceful shutdown */
            }
            finally { Stop(); }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _tcpListener?.Stop();
        }

        /// <summary>
        /// Handles the client internal asynchronous.
        /// Wrapper to ensure safety/logging around the abstract call
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        private async Task HandleClientInternalAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                {
                    await HandleClientAsync(client, token);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Client Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the client asynchronous.
        /// Plugin your specific client handling logic here. This is called for each accepted client connection.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        /// <returns>Handler for client</returns>
        protected abstract Task HandleClientAsync(TcpClient client, CancellationToken token);
    }
}
