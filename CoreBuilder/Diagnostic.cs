namespace CoreBuilder
{
    public class Diagnostic
    {
        public string FilePath { get; }
        public int LineNumber { get; }
        public string Message { get; }

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
