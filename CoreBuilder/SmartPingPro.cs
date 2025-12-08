/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        SmartPingPro.cs
 * PURPOSE:     Command to perform advanced network diagnostics (ping, traceroute, ports, DNS).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Command to perform advanced network diagnostics (ping, traceroute, ports, DNS).
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class SmartPingPro : ICommand
    {
        /// <inheritdoc />
        public string Name => "SmartPingPro";

        /// <inheritdoc />
        public string Description => "Advanced network diagnostic tool (ping, traceroute, ports, DNS).";

        /// <inheritdoc />
        public string Namespace => "Network";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: SmartPingPro [host] [options]");

            var host = args[0];
            var pingCount = args.Length > 1 && int.TryParse(args[1], out var n) ? n : 5;
            var doReverseDns = args.Length > 2 && args[2].Equals("true", StringComparison.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            sb.AppendLine($"SmartPingPro Report for {host}");
            sb.AppendLine("-----------------------------------");

            // 1. DNS Resolution
            var ips = System.Net.Dns.GetHostAddresses(host);
            sb.AppendLine("Resolved IPs: " + string.Join(", ", ips.Select(ip => ip.ToString())));

            // 2. Ping statistics
            var ping = new System.Net.NetworkInformation.Ping();
            var rttList = new List<long>();
            var received = 0;
            for (var i = 0; i < pingCount; i++)
            {
                var reply = ping.Send(host, 1000);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    rttList.Add(reply.RoundtripTime);
                    received++;
                }
            }

            var loss = (double)(pingCount - received) / pingCount * 100;
            sb.AppendLine(
                $"\nPing Statistics:\nPackets sent: {pingCount}, Received: {received}, Lost: {pingCount - received} ({loss:F0}%)");
            if (rttList.Any())
                sb.AppendLine(
                    $"Latency: Min/Avg/Max = {rttList.Min()}ms / {rttList.Average():F0}ms / {rttList.Max()}ms");

            // 3. Reverse DNS
            if (doReverseDns)
            {
                try
                {
                    var entry = System.Net.Dns.GetHostEntry(host);
                    sb.AppendLine($"\nReverse DNS: {entry.HostName}");
                }
                catch
                {
                    sb.AppendLine("\nReverse DNS: (not found)");
                }
            }

            // 4. Port Scan (common ports)
            int[] ports = { 22, 80, 443 };
            sb.AppendLine("\nOpen Ports:");
            foreach (var port in ports)
            {
                using var tcp = new System.Net.Sockets.TcpClient();

                try
                {
                    var task = tcp.ConnectAsync(host, port);
                    task.Wait(300); // timeout
                    sb.AppendLine($"{port} {(GetServiceName(port) ?? "")} Open");
                }
                catch
                {
                    sb.AppendLine($"{port} {(GetServiceName(port) ?? "")} Closed");
                }
            }

            // 5. Optional: Traceroute could be added here using Ping with TTL increment

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }


        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>Name of Serivce</returns>
        private string? GetServiceName(int port)
        {
            return port switch
            {
                22 => "SSH",
                80 => "HTTP",
                443 => "HTTPS",
                _ => null
            };
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}
