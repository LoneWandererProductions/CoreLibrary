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
using System.Collections.Generic;
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
        /// The TCP log server
        /// </summary>
        private readonly LogCollectorServer _tcpServer;

        /// <summary>
        /// The UDP log server
        /// </summary>
        private readonly UdpLogCollector _udpServer;

        /// <summary>
        /// The protocol
        /// </summary>
        private readonly LogProtocol _protocol;

        /// <summary>
        /// The server tasks
        /// </summary>
        private readonly List<Task> _serverTasks = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogServer" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="onLogReceived">The on log received.</param>
        /// <param name="protocol">The protocol.</param>
        public SimpleLogServer(int port, Action<string> onLogReceived, LogProtocol protocol = LogProtocol.TCP)
                : this(port, new ActionLogProcessor(onLogReceived), protocol)
        {
            // This constructor just wraps the Action into our helper class
            // and chains to Constructor 2.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogServer" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="protocol">The protocol.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        /// <exception cref="ArgumentNullException">processor</exception>
        public SimpleLogServer(int port, ILogProcessor processor, LogProtocol protocol = LogProtocol.TCP)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _protocol = protocol;
            _cts = new CancellationTokenSource();

            // Initialize based on choice
            if (protocol == LogProtocol.TCP || protocol == LogProtocol.Both)
                _tcpServer = new LogCollectorServer(port, processor);

            if (protocol == LogProtocol.UDP || protocol == LogProtocol.Both)
                _udpServer = new UdpLogCollector(port, processor);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (_serverTasks.Count > 0) return;

            if (_tcpServer != null)
                _serverTasks.Add(Task.Run(() => _tcpServer.StartAsync(_cts.Token)));

            if (_udpServer != null)
                _serverTasks.Add(Task.Run(() => _udpServer.StartAsync(_cts.Token)));
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            try
            {
                // Wait for all active server tasks to complete their cleanup
                Task.WaitAll(_serverTasks.ToArray(), TimeSpan.FromSeconds(3));
            }
            catch (AggregateException) { /* Expected on cancellation */ }
            finally
            {
                _serverTasks.Clear();
            }
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
