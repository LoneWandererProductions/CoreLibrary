/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        WhoAmI.cs
 * PURPOSE:     Command to display local machine and network identity.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Net.NetworkInformation;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Displays local machine identity (hostname, user, IP addresses) and allows extensions to fetch individual properties.
    /// </summary>
    public sealed class WhoAmI : ICommand
    {
        /// <inheritdoc />
        public string Name => "WhoAmI";

        /// <inheritdoc />
        public string Description => "Displays hostname, user and IP information.";

        /// <inheritdoc />
        public string Namespace => "System";

        /// <inheritdoc />
        public int ParameterCount => 0;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
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

                var info =
                    "WhoAmI System Report\n" +
                    "------------------------\n" +
                    $"Hostname: {hostname}\n" +
                    $"Username: {username}\n" +
                    $"Domain: {domain}\n" +
                    $"IPv4 Addresses: {(ips.Any() ? string.Join(", ", ips) : "None")}\n" +
                    $"OS: {Environment.OSVersion}\n" +
                    $"64-bit OS: {Environment.Is64BitOperatingSystem}\n" +
                    $"64-bit Process: {Environment.Is64BitProcess}\n" +
                    $"Processor Count: {Environment.ProcessorCount}\n" +
                    $"CLR Version: {Environment.Version}";

                return CommandResult.Ok(info, EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"WhoAmI failed: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}
