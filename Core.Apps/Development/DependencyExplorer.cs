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

                // 1. Create a list of VmValues for Packages
                var packageList = doc.Descendants("PackageReference")
                    .Select(x => VmValue.FromString(x.Attribute("Include")?.Value))
                    .ToList();

                // Store as a temporary list in the heap
                _variables.SetList($"{projName}_pkgs", packageList);

                // 2. Create a list of VmValues for References
                var refList = doc.Descendants("ProjectReference")
                    .Select(x => VmValue.FromString(Path.GetFileNameWithoutExtension(x.Attribute("Include")?.Value)))
                    .ToList();

                _variables.SetList($"{projName}_refs", refList);

                // 3. Build the Object containing pointers to those lists
                projectMap[projName] = VmValue.FromObject(); // Or use Pointer/Object hybrid logic
            }

            // 5. Store in the Registry for WPF or Scripts to use
            _variables.SetObject(CurrentRegistryKey, projectMap);

            return CommandResult.Ok(sb.ToString(), CurrentRegistryKey, EnumTypes.Wobject);
        }
    }
}
