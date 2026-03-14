/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Extensions
 * FILE:        WhoAmIExtension.cs
 * PURPOSE:     Extension for WhoAmI to return individual parameters like "ip" or "hostname". Specially designed for scripting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Registry;

namespace Core.Apps.Extensions
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
        public CommandResult Invoke(ICommand command, string[] extensionArgs, Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            if (extensionArgs.Length == 0)
            {
                return CommandResult.Fail(
                    "No parameter specified. Example: whoami().who(ip,hostname) or whoami().who(ip)");
            }

            // 1. Logic check: If it doesn't implement IRegistryProducer, we can't save data.
            if (command is not IRegistryProducer producer)
            {
                return CommandResult.Fail(
                    $"Extension 'who' requires an IRegistryProducer, but {command.Name} does not implement it.");
            }

            // Use the registry and the key defined by the parent command
            var registry = producer.Variables;
            string storeKey = producer.CurrentRegistryKey;

            try
            {
                // 2. Fetch existing object from the registry to allow incremental updates
                // This ensures that whoami(x).who(ip).who(os) preserves the IP.
                var whoamiData = new Dictionary<string, VmValue>();
                if (registry.TryGetObject(storeKey, out var existingObj) && existingObj != null)
                {
                    foreach (var kvp in existingObj)
                    {
                        whoamiData[kvp.Key] = kvp.Value;
                    }
                }

                // 3. Gather System Data
                var hostname = Environment.MachineName;
                var username = Environment.UserName;
                var domain = Environment.UserDomainName;

                var ips = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToList();

                string ipsJoined = ips.Any() ? string.Join(", ", ips) : "None";

                // 4. Parse requested fields and update the dictionary
                var lines = new List<string>();
                foreach (var field in extensionArgs.Select(arg => arg.ToLowerInvariant()))
                {
                    switch (field)
                    {
                        case "hostname":
                            whoamiData["hostname"] = VmValue.FromString(hostname);
                            lines.Add($"Hostname: {hostname}");
                            break;
                        case "username":
                            whoamiData["username"] = VmValue.FromString(username);
                            lines.Add($"Username: {username}");
                            break;
                        case "domain":
                            whoamiData["domain"] = VmValue.FromString(domain);
                            lines.Add($"Domain: {domain}");
                            break;
                        case "ip":
                            whoamiData["ip"] = VmValue.FromString(ipsJoined);
                            lines.Add($"IP: {ipsJoined}");
                            break;
                        case "os":
                            whoamiData["os"] = VmValue.FromString(Environment.OSVersion.ToString());
                            lines.Add($"OS: {Environment.OSVersion}");
                            break;
                        case "64bitos":
                            whoamiData["64bitos"] = VmValue.FromBool(Environment.Is64BitOperatingSystem);
                            lines.Add($"64-bit OS: {Environment.Is64BitOperatingSystem}");
                            break;
                        case "64bitprocess":
                            whoamiData["64bitprocess"] = VmValue.FromBool(Environment.Is64BitProcess);
                            lines.Add($"64-bit Process: {Environment.Is64BitProcess}");
                            break;
                        case "processorcount":
                            whoamiData["processorcount"] = VmValue.FromInt(Environment.ProcessorCount);
                            lines.Add($"Processor Count: {Environment.ProcessorCount}");
                            break;
                        case "clrversion":
                            whoamiData["clrversion"] = VmValue.FromString(Environment.Version.ToString());
                            lines.Add($"CLR Version: {Environment.Version}");
                            break;
                        default:
                            lines.Add($"Unknown parameter: {field}");
                            break;
                    }
                }

                // 5. Store the updated dictionary back into the registry
                // Your VariableRegistry.SetObject handles the memory overwrite/reuse logic.
                registry.SetObject(storeKey, whoamiData);

                // 6. Return the status message
                string message = string.Join(Environment.NewLine, lines);
                return CommandResult.Ok(message, storeKey, EnumTypes.Wobject);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"WhoExtension failed: {ex.Message}");
            }
        }
    }
}