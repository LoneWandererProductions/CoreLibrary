/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        WhoAmI.cs
 * PURPOSE:     Command to display local machine and network identity.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Registry;

namespace CoreBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Displays local machine identity (hostname, user, IP addresses) and allows extensions to fetch individual properties.
    /// </summary>
    public sealed class WhoAmI : ICommand
    {
        /// <summary>
        /// The variables
        /// </summary>
        internal IVariableRegistry Variables;

        /// <summary>
        /// Gets the last store key.
        /// </summary>
        /// <value>
        /// The last store key.
        /// </value>
        public string LastStoreKey { get; private set; } = "whoami";

        /// <summary>
        /// Initializes a new instance of the <see cref="WhoAmI"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public WhoAmI(IVariableRegistry variables)
        {
            Variables = variables;
        }

        /// <inheritdoc />
        public string Name => "WhoAmI";

        /// <inheritdoc />
        public string Description =>
            "Displays hostname, user and IP information. Supports the Who extension, Example: whoami().who(ip,hostname) or whoami().who(ip)";

        /// <inheritdoc />
        public string Namespace => "System";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => new Dictionary<string, int>
        {
            { "who", 1 } // "who" extension expects at least 1 parameter (or variable)
        };

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            // 1. Overload Logic: Use the first argument as the store key if provided
            LastStoreKey = (args is { Length: > 0 } && !string.IsNullOrWhiteSpace(args[0]))
                ? args[0]
                : "whoami";

            try
            {
                var hostname = Environment.MachineName;
                var username = Environment.UserName;
                var domain = Environment.UserDomainName;
                var os = Environment.OSVersion.ToString();

                var ips = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToList();

                string ipsJoined = ips.Any() ? string.Join(", ", ips) : "None";

                // 2. Prepare the data for the Heap
                var whoamiData = new Dictionary<string, VmValue>
                {
                    { "hostname", VmValue.FromString(hostname) },
                    { "username", VmValue.FromString(username) },
                    { "domain", VmValue.FromString(domain) },
                    { "ip", VmValue.FromString(ipsJoined) },
                    { "os", VmValue.FromString(os) },
                    { "64bitos", VmValue.FromBool(Environment.Is64BitOperatingSystem) },
                    { "64bitprocess", VmValue.FromBool(Environment.Is64BitProcess) },
                    { "processorcount", VmValue.FromInt(Environment.ProcessorCount) },
                    { "clrversion", VmValue.FromString(Environment.Version.ToString()) }
                };

                // 3. Store the Object in the Registry
                // This utilizes your SetObject logic to handle memory allocation/reuse
                Variables.SetObject(LastStoreKey, whoamiData);

                // 4. Generate the console output string
                var info =
                    "WhoAmI System Report\n" +
                    "------------------------\n" +
                    $"Hostname: {hostname}\n" +
                    $"Username: {username}\n" +
                    $"Domain: {domain}\n" +
                    $"IPv4 Addresses: {ipsJoined}\n" +
                    $"OS: {os}\n" +
                    $"Data stored in: ${LastStoreKey}\n" +
                    "------------------------";

                return CommandResult.Ok(info, LastStoreKey, EnumTypes.Wobject);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"WhoAmI failed: {ex.Message}");
            }
        }
    }
}
