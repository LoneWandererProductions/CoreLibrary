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
        void ProcessProject(string projectPath, string outputResourceFile);
    }
}
