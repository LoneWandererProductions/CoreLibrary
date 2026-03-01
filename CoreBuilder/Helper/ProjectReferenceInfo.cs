/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        ProjectReferenceInfo.cs
 * PURPOSE:     Class to parse and analyze project references in a .csproj file.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CoreBuilder.Helper
{
    /// <summary>
    /// Class to parse and analyze project references in a .csproj file.
    /// </summary>
    internal sealed class ProjectReferenceInfo
    {
        /// <summary>
        /// Gets the project refs.
        /// </summary>
        /// <value>
        /// The project refs.
        /// </value>
        private List<string> ProjectRefs { get; } = new();

        /// <summary>
        /// Gets the package refs.
        /// </summary>
        /// <value>
        /// The package refs.
        /// </value>
        private List<string> PackageRefs { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReferenceInfo"/> class.
        /// </summary>
        /// <param name="xmlContent">Content of the XML.</param>
        internal ProjectReferenceInfo(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);

            ProjectRefs.AddRange(
                doc.Descendants("ProjectReference")
                    .Select(e => (string)e.Attribute("Include") ?? "")
            );

            PackageRefs.AddRange(
                doc.Descendants("PackageReference")
                    .Select(e => (string)e.Attribute("Include") ?? "")
            );
        }

        /// <summary>
        /// Gets the unused references.
        /// </summary>
        /// <returns>Possible unused References.</returns>
        internal IEnumerable<string> GetUnusedReferences()
        {
            // TODO: real symbol-based checking.
            // For now, we just treat all references as unused.
            foreach (var p in ProjectRefs)
                yield return p;

            foreach (var p in PackageRefs)
                yield return p;
        }
    }
}
