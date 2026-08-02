/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.UI
 * FILE:        LogEntry.cs
 * PURPOSE:     Entry model for log messages, including optional timestamps for display in the UI.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.Apps.UI
{
    /// <summary>
    /// Entry model for log messages, including optional timestamps for display in the UI.
    /// </summary>
    public sealed class LogEntry
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public string Timestamp { get; init; } = string.Empty;

        /// <summary>
        /// For the UI to decide whether to show the timestamp
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.IsNullOrEmpty(Timestamp)
            ? Message
            : $"[{Timestamp}] {Message}";
    }
}
