/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        LogCollectorServer.cs
 * PURPOSE:     Central log collector server that listens for incoming log messages and processes them using a provided ILogProcessor implementation.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
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

        /// <summary>
        /// Handles the client asynchronous.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        protected override async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (token.IsCancellationRequested) break;

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        await _processor.ProcessMessageAsync(line);
                    }
                }
            }
        }
    }
}
