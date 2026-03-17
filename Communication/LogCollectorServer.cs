/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        LogCollectorServer.cs
 * PURPOSE:     Central log collector server that listens for incoming log messages and processes them using a provided ILogProcessor implementation.
 *              The TCP Variation
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Connects and listens for logs continuously.
    /// Central entry and processing point for logs, which can be extended to save logs to a database, write to files, or perform any other processing as needed.
    /// </summary>
    public class LogCollectorServer : TcpServerBase
    {
        /// <summary>
        /// The log processor, which can be implemented to save logs to a database, write to a file, or perform any other processing as needed.
        /// </summary>
        private readonly ILogProcessor _processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogCollectorServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="processor">The processor.</param>
        public LogCollectorServer(int port, ILogProcessor processor) : base(port)
        {
            _processor = processor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Handles the client asynchronous.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        protected override async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            // 1. Set a hard timeout on the socket itself. 
            // If no data arrives for 5 seconds, the socket throws an IOException.
            client.ReceiveTimeout = 5000;

            // 2. Use a specific cancellation token source for just this client
            // capable of cancelling if the global token cancels OR if we time out manually.
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // 3. The "Safe" Read. 
                    // StreamReader.ReadLineAsync() typically DOES NOT accept a CancellationToken directly
                    // in older .NET versions, and it doesn't respect ReceiveTimeout easily.
                    // We must handle the timeout manually or use a specific trick.

                    var lineTask = reader.ReadLineAsync();

                    // Wait for the line OR a timeout (e.g., 5 seconds idle)
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), linkedCts.Token);

                    var completedTask = await Task.WhenAny(lineTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        // Timeout occurred! Client is too slow or holding the connection open.
                        Trace.WriteLine("Client timed out. Disconnecting.");
                        break;
                    }

                    // If we got here, the line was read successfully
                    string line = await lineTask;

                    if (line == null) break; // Client disconnected gracefully

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        await _processor.ProcessMessageAsync(line);
                    }
                }
            }
            catch (IOException)
            {
                // Socket timeout or connection issue
            }
            catch (OperationCanceledException)
            {
                // Server is stopping
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error processing client: {ex.Message}");
            }
        }
    }
}
