/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        ILogFormatter.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreMemoryLog
{
    /// <summary>
    /// Defines how log entries are formatted for output (Trace, file, etc.).
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Formats the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Formattted Debug message.</returns>
        string Format(ILogEntry entry);
    }
}

