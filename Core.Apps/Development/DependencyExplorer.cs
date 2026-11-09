/*
 * COPYRIGHT:    See COPYING in the top level directory
 * PROJECT:      Core.Apps.Development
 * FILE:         DependencyExplorer.cs
 * PURPOSE:      Maps project-to-project and project-to-library dependencies.
 * PROGRAMMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Registry;

namespace Core.Apps.Development
{
    /// <inheritdoc cref="ICommand" />
    /// <summary>
    /// Builds a map of project dependencies by scanning .csproj files in a given directory. It extracts both NuGet package references and project references, creating a structured representation of how projects depend on each other and on external libraries. The resulting map is stored in the registry for use in WPF visualizations or scripts, allowing developers to easily understand and analyze their project's dependency graph.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    /// <seealso cref="Weaver.Interfaces.IRegistryProducer" />
    public sealed class DependencyExplorer : ICommand, IRegistryProducer
    {
        /// <summary>
        /// The variables
        /// </summary>
        private readonly IVariableRegistry _variables;

        /// <inheritdoc />
        public string? CurrentRegistryKey => "project_map";

        /// <inheritdoc />
        public EnumTypes DataType => EnumTypes.Wobject;

        /// <inheritdoc />
        public IVariableRegistry Variables => _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyExplorer"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public DependencyExplorer(IVariableRegistry variables)
        {
            _variables = variables;
        }

        /// <inheritdoc />
        public string Name => "depexplore";

        /// <inheritdoc />
        public string Namespace => "Development";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public string Description => "Explores project dependencies and maps them.";

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
                return CommandResult.Fail("Usage: depexplore <root_folder>");

            var rootPath = args[0]!;
            if (!Directory.Exists(rootPath))
                return CommandResult.Fail("Folder not found.");

            var projectMap = new Dictionary<string?, VmValue>();
            var rawProjectRefs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var rawPackageRefs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // 1. Find all .csproj files
            var projects = Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var projFile in projects)
            {
                var projName = Path.GetFileNameWithoutExtension(projFile);
                if (string.IsNullOrWhiteSpace(projName)) continue;

                var doc = XDocument.Load(projFile);

                // 1. Create a list of VmValues for Packages (XML-namespace resilient & Central Package Management aware)
                var packages = doc.Descendants()
                    .Where(x => x.Name.LocalName.Equals("PackageReference", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Attribute("Include")?.Value ?? x.Attribute("Update")?.Value)
                    .Where(val => !string.IsNullOrWhiteSpace(val))
                    .Select(val => val!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var packageList = packages.Select(VmValue.FromString).ToList();

                // Store as a temporary list in the heap
                var pkgKey = $"{projName}_pkgs";
                _variables.SetList(pkgKey, packageList);
                rawPackageRefs[projName] = packages;

                // 2. Create a list of VmValues for References (extract clean project names without path or extension)
                var references = doc.Descendants()
                    .Where(x => x.Name.LocalName.Equals("ProjectReference", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Attribute("Include")?.Value)
                    .Where(val => !string.IsNullOrWhiteSpace(val))
                    .Select(path => Path.GetFileNameWithoutExtension(path!))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var refList = references.Select(VmValue.FromString).ToList();

                var refKey = $"{projName}_refs";
                _variables.SetList(refKey, refList);
                rawProjectRefs[projName] = references;

                // 3. Build the Object containing pointers to those lists
                var metaKey = $"{projName}_meta";
                var projDescriptor = new Dictionary<string?, VmValue>
                {
                    ["packages"] = VmValue.FromString(pkgKey), ["references"] = VmValue.FromString(refKey)
                };

                _variables.SetObject(metaKey, projDescriptor);

                // Store pointer to project metadata in root map
                projectMap[projName] = VmValue.FromString(metaKey);
            }

            // 5. Store in the Registry for WPF or Scripts to use
            _variables.SetObject(CurrentRegistryKey, projectMap);

            // 6. Calculate in-degrees (who references this project?)
            var inDegree = rawProjectRefs.Keys.ToDictionary(k => k, _ => 0, StringComparer.OrdinalIgnoreCase);
            foreach (var targets in rawProjectRefs.Values)
            {
                foreach (var target in targets.Where(t => inDegree.ContainsKey(t)))
                {
                    inDegree[target]++;
                }
            }

            // 7. Compose actionable architecture report
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                       PROJECT DEPENDENCY ANALYSIS REPORT                      ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Total Projects Scanned: {rawProjectRefs.Count}\n");

            // Categorization: Entry Points / Applications (Nothing depends on them)
            var entryPoints = rawProjectRefs.Keys
                .Where(k => inDegree[k] == 0 && rawProjectRefs[k].Count > 0)
                .OrderByDescending(k => rawProjectRefs[k].Count)
                .ToList();

            sb.AppendLine($"[ENTRY POINTS / ROOT CONSUMERS] ({entryPoints.Count})");
            sb.AppendLine("Projects that depend on others but are never referenced as a library:");
            foreach (var ep in entryPoints)
            {
                sb.AppendLine(
                    $"  * {ep,-25} -> {rawProjectRefs[ep].Count,2} Project Refs | {rawPackageRefs[ep].Count,2} NuGet Packages");
            }

            // Categorization: High-Coupling Architectural Hubs
            var hubs = rawProjectRefs.Keys
                .Where(k => inDegree[k] >= 3 && rawProjectRefs[k].Count >= 2)
                .OrderByDescending(k => inDegree[k] + rawProjectRefs[k].Count)
                .ToList();

            if (hubs.Count > 0)
            {
                sb.AppendLine($"\n[HIGH COUPLING HUBS - REFACTOR RISK] ({hubs.Count})");
                sb.AppendLine("Intermediate libraries with heavy incoming and outgoing dependencies:");
                foreach (var hub in hubs)
                {
                    sb.AppendLine(
                        $"  ! {hub,-25} (Used by {inDegree[hub]} projects | References {rawProjectRefs[hub].Count} projects)");
                }
            }

            // Categorization: Foundation / Leaf Libraries
            var foundations = rawProjectRefs.Keys
                .Where(k => inDegree[k] > 0 && rawProjectRefs[k].Count == 0)
                .OrderByDescending(k => inDegree[k])
                .ToList();

            sb.AppendLine($"\n[FOUNDATION LIBRARIES] ({foundations.Count})");
            sb.AppendLine("Clean leaf libraries (no downstream project dependencies):");
            foreach (var fl in foundations)
            {
                sb.AppendLine(
                    $"  * {fl,-25} (Inbound References: {inDegree[fl],2} | NuGet: {rawPackageRefs[fl].Count,2})");
            }

            // Categorization: Isolated / Orphan Projects
            var isolated = rawProjectRefs.Keys
                .Where(k => inDegree[k] == 0 && rawProjectRefs[k].Count == 0)
                .ToList();

            if (isolated.Count > 0)
            {
                sb.AppendLine($"\n[ISOLATED / ORPHAN CANDIDATES] ({isolated.Count})");
                sb.AppendLine("Projects with 0 incoming and 0 outgoing project dependencies:");
                foreach (var iso in isolated)
                {
                    sb.AppendLine($"  ? {iso,-25} (NuGet: {rawPackageRefs[iso].Count,2})");
                }
            }

            // 8. Visual Mermaid Diagram Export
            sb.AppendLine("\n================================================================================");
            sb.AppendLine("                             MERMAID DEPENDENCY GRAPH                           ");
            sb.AppendLine("================================================================================");
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph TD");

            foreach (var (source, targets) in rawProjectRefs.OrderBy(k => k.Key))
            {
                var cleanSource = SanitizeNodeId(source);
                if (targets.Count == 0 && inDegree[source] == 0)
                {
                    sb.AppendLine($"    {cleanSource}[\"{source}\"]");
                }
                else
                {
                    foreach (var target in targets)
                    {
                        var cleanTarget = SanitizeNodeId(target);
                        sb.AppendLine($"    {cleanSource}[\"{source}\"] --> {cleanTarget}[\"{target}\"]");
                    }
                }
            }

            sb.AppendLine("```");

            return CommandResult.Ok(sb.ToString(), CurrentRegistryKey, EnumTypes.Wobject);
        }

        /// <summary>
        /// Sanitizes a project name to ensure it forms a valid Mermaid node identifier.
        /// </summary>
        /// <param name="name">The project name.</param>
        /// <returns>A sanitized node identifier string.</returns>
        private static string SanitizeNodeId(string name) =>
            name.Replace(".", "_").Replace("-", "_").Replace(" ", "_");
    }
}
