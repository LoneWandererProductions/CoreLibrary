/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Development
 * FILE:        DependencyExplorer.cs
 * PURPOSE:     Maps project-to-project and project-to-library dependencies.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
        public string CurrentRegistryKey => "project_map";

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
        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length < 1) return CommandResult.Fail("Usage: depexplore <root_folder>");

            var rootPath = args[0];
            if (!Directory.Exists(rootPath)) return CommandResult.Fail("Folder not found.");

            var projectMap = new Dictionary<string, VmValue>();
            var sb = new StringBuilder();

            // 1. Find all .csproj files
            var projects = Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var projFile in projects)
            {
                var projName = Path.GetFileNameWithoutExtension(projFile);
                var doc = XDocument.Load(projFile);

                // 2. Extract NuGet Packages
                var packages = doc.Descendants("PackageReference")
                    .Select(x => x.Attribute("Include")?.Value)
                    .Where(v => v != null)
                    .ToList();

                // 3. Extract Project References
                var refs = doc.Descendants("ProjectReference")
                    .Select(x => Path.GetFileNameWithoutExtension(x.Attribute("Include")?.Value ?? ""))
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                // 4. Flatten directly into projectMap using prefixed keys
                projectMap[$"{projName}.packages"] = VmValue.FromString(string.Join(", ", packages));
                projectMap[$"{projName}.references"] = VmValue.FromString(string.Join(", ", refs));

                sb.AppendLine($"Project: {projName}");
                sb.AppendLine($"  - Libraries: {string.Join(", ", packages)}");
                sb.AppendLine($"  - Projects:  {string.Join(", ", refs)}");
                sb.AppendLine();
            }

            // 5. Store in the Registry for WPF or Scripts to use
            _variables.SetObject(CurrentRegistryKey, projectMap);

            return CommandResult.Ok(sb.ToString(), CurrentRegistryKey, EnumTypes.Wobject);
        }
    }
}
