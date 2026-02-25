/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        SimpleLogServer.cs
 * PURPOSE:     Facade to simplify usage of LogCollectorServer for users who just want to provide a simple callback for log processing without needing to implement the ILogProcessor interface themselves.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * * TODO ARCHITECTURE:
 * 1. [Protocol] Implement Length-Prefixing (Header + Body) to support multi-line logs and binary data, replacing the fragile NewLine delimiter.
 * 2. [Performance] decouple Network Reads from Processing using a Producer/Consumer pattern (System.Threading.Channels) to prevent slow DB writes from blocking network sockets.
 * 3. [Security] Add TLS support (SslStream) via X509Certificate2 to encrypt log traffic.
 * 4. [Reliability] Add Application-Level Heartbeats (Ping/Pong) to detect and prune "zombie" connections that the TCP stack hasn't dropped yet.
 * 5. [Observability] Expose internal metrics (e.g., ConnectedClients, LogsPerSecond, DroppedPackets) for external monitoring.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
    /// <summary>
    /// A "Stupid Simple" wrapper for the LogCollector.
    /// No DI required. Just new it up and go.
    /// </summary>
    public class SimpleLogServer : IDisposable
    {
        /// <summary>
        /// The internal server
        /// </summary>
        private readonly LogCollectorServer _internalServer;

        /// <summary>
        /// The CTS
        /// </summary>
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// The server task
        /// </summary>
        private Task _serverTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="onLogReceived">The on log received.</param>
        public SimpleLogServer(int port, Action<string> onLogReceived)
            : this(port, new ActionLogProcessor(onLogReceived))
        {
            // This constructor just wraps the Action into our helper class
            // and chains to Constructor 2.
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="processor">The processor.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        public SimpleLogServer(int port, ILogProcessor processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _cts = new CancellationTokenSource();
            _internalServer = new LogCollectorServer(port, processor);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (_serverTask != null) return; // Already running

            // Run the server in a background task so it doesn't block the main thread
            _serverTask = Task.Run(() => _internalServer.StartAsync(_cts.Token));
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            try
            {
                _serverTask?.Wait(); // Wait for it to finish cleaning up
            }
            catch (AggregateException) { /* Ignore cancellation errors */ }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}
