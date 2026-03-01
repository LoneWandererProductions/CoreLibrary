/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        PortCheckerServer.cs
 * PURPOSE:     Connects, sends a message, and disconnects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Connects, sends a message, and disconnects.
    /// </summary>
    public class PortCheckerServer : TcpServerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortCheckerServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public PortCheckerServer(int port) : base(port) { }

        /// <inheritdoc />
        /// <summary>
        /// Handles the client asynchronous.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        protected override async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            await using var stream = client.GetStream();
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(ComResource.AnswerMessage);
            await stream.WriteAsync(buffer, 0, buffer.Length, token);
            await stream.FlushAsync(token);
            Trace.WriteLine("Port check handled.");
        }
    }
}
