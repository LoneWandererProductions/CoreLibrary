/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreConsole
 * FILE:        CoreConsole/IResourceExtractor.cs
 * PURPOSE:     String Resource Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBeInternal

namespace CoreBuilder
{
    public interface IResourceExtractor
    {
        /// <summary>
        /// Processes the given project directory, extracting string literals
        /// and replacing them with resource references.
        /// </summary>
        /// <param name="projectPath">The root directory of the C# project.</param>
        /// <param name="outputResourceFile">Path to generate the resource file.</param>
        /// <param name="appendToExisting">Indicates whether to append to an existing file or create a new one.</param>
        void ProcessProject(string projectPath, string outputResourceFile, bool appendToExisting = false);
    }
}
