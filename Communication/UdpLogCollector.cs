/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        LogCollectorServer.cs
 * PURPOSE:     Central log collector server that listens for incoming log messages and processes them using a provided ILogProcessor implementation.
 *              The UDP Variation
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
    /// <summary>
    /// Similar to LogCollectorServer
    /// </summary>
    public class UdpLogCollector
    {
        /// <summary>
        /// The port
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// The processor
        /// </summary>
        private readonly ILogProcessor _processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpLogCollector"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="processor">The processor.</param>
        public UdpLogCollector(int port, ILogProcessor processor)
        {
            _port = port;
            _processor = processor;
        }

        /// <summary>
        /// Starts the asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task StartAsync(CancellationToken token)
        {
            using var udpClient = new UdpClient(_port);
            Trace.WriteLine(string.Concat("UDP Syslog Server started on port {Port}", _port));

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // UDP is much simpler: just receive the bytes
                    var result = await udpClient.ReceiveAsync(token);
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    Trace.WriteLine(string.Concat("Received UDP log from {Remote}: {Message}", result.RemoteEndPoint, message));

                    // Pass it to your existing Database processor!
                    await _processor.ProcessMessageAsync(message);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, "UDP Error");
                }
            }
        }
    }
}
