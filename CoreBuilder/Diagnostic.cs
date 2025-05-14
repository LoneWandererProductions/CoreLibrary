namespace CoreBuilder
{
    public sealed class Diagnostic
    {
        public string FilePath { get; }
        public int LineNumber { get; }
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostic"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="message">The message.</param>
        public Diagnostic(string filePath, int lineNumber, string message)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            Message = message;
        }

        public override string ToString() =>
            $"{FilePath}({LineNumber}): {Message}";
    }

}
