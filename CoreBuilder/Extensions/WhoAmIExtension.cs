/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Extensions
 * FILE:        WhoAmIExtension.cs
 * PURPOSE:     Extension for WhoAmI to return individual parameters like "ip" or "hostname". Specially designed for scripting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Net.NetworkInformation;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Extensions
{
    /// <inheritdoc />
    /// <summary>
    /// Extension for WhoAmI command to fetch individual parameters
    /// </summary>
    public sealed class WhoAmIExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "Who";

        /// <inheritdoc />
        public string Description =>
            "Returns specific information of WhoAmI command (e.g., ip, hostname, username), you can join them or return a single one.";

        /// <inheritdoc />
        public string Namespace => "System";

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            if (args.Length == 0)
            {
                return CommandResult.Fail(
                    "No parameter specified. Example: whoami().who(ip,hostname) or whoami().who(ip)");
            }

            try
            {
                var hostname = Environment.MachineName;
                var username = Environment.UserName;
                var domain = Environment.UserDomainName;

                var ips = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToList();

                string ipsJoined = ips.Any() ? string.Join(", ", ips) : "None";

                // Build a list of lines for requested fields
                var lines = args.Select(arg => arg.ToLowerInvariant())
                    .Select(field =>
                    {
                        return field switch
                        {
                            "hostname" => $"Hostname: {hostname}",
                            "username" => $"Username: {username}",
                            "domain" => $"Domain: {domain}",
                            "ip" => $"IP: {ipsJoined}",
                            "os" => $"OS: {Environment.OSVersion}",
                            "64bitos" => $"64-bit OS: {Environment.Is64BitOperatingSystem}",
                            "64bitprocess" => $"64-bit Process: {Environment.Is64BitProcess}",
                            "processorcount" => $"Processor Count: {Environment.ProcessorCount}",
                            "clrversion" => $"CLR Version: {Environment.Version}",
                            _ => $"Unknown parameter: {field}"
                        };
                    }).ToArray();

                // Join lines into a single message
                string message = string.Join(Environment.NewLine, lines);

                return CommandResult.Ok(message: message);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"WhoExtension failed: {ex.Message}");
            }
        }
    }
}
