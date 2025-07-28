/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugHelper.cs
 * PURPOSE:     Helper Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;

namespace Debugger;

/// <summary>
///     The debug helper class.
/// </summary>
internal static class DebugHelper
{
    /// <summary>
    ///     The directory where log files are stored.
    /// </summary>
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        DebuggerResources.LogPath);

    /// <summary>
    ///     Gets the log file.
    /// </summary>
    /// <param name="logFile">The log file.</param>
    /// <returns>The Content of the file</returns>
    internal static string GetLogFile(string logFile)
    {
        // Ensure log directory exists
        if (!Directory.Exists(LogDirectory))
        {
            _ = Directory.CreateDirectory(LogDirectory);
        }

        var logFilePath = Path.Combine(LogDirectory, $"{logFile}{DebuggerResources.LogFileExtension}");

        // Ensure the log file exists
        if (!File.Exists(logFilePath))
        {
            using (File.Create(logFilePath)) { } // Just create and close
        }

        // Check the file size and rotate if necessary
        if (new FileInfo(logFilePath).Length <= DebugRegister.MaxFileSize)
        {
            return logFilePath;
        }

        // Update to the new file path
        return RotateLogFiles(logFilePath);
    }

    /// <summary>
    ///     Add the range of text. With specific Format
    /// </summary>
    /// <param name="textRange">The textRange.</param>
    /// <param name="line">The line. as Text</param>
    /// <param name="found">The string was filtered</param>
    internal static void AddRange(TextRange textRange, string line, bool found)
    {
        // Get the current TextPointer for the end of the TextRange
        var endPointer = textRange.End;

        // Create a new TextRange starting from the current position of the TextPointer
        TextRange newRange = new(endPointer, endPointer)
        {
            // Append the new line (including newline)
            Text = line + Environment.NewLine
        };

        // Apply background color if needed
        if (found)
        {
            newRange.ApplyPropertyValue(TextElement.BackgroundProperty, DebugRegister.FoundColor);
        }

        // Determine the color option based on the line content
        var option =
            DebugRegister.ColorOptions.FirstOrDefault(opt =>
                line.StartsWith(opt.EntryText, StringComparison.Ordinal))
            ?? DebugRegister.ColorOptions[0];

        // Apply the foreground color
        newRange.ApplyPropertyValue(TextElement.ForegroundProperty, option.ColorName);
    }

    /// <summary>
    ///     Rotates the log files and returns the original file name if the maximum number of allowed files is reached.
    /// </summary>
    /// <param name="logFilePath">The path to the main log file (without a number).</param>
    /// <returns>The original log file name if the maximum number of log files is reached; otherwise, null.</returns>
    private static string RotateLogFiles(string logFilePath)
    {
        var maxBackupFiles = DebugRegister.MaxFileCount; // Configurable max backup count
        var logFileDirectory = Path.GetDirectoryName(logFilePath) ?? string.Empty;
        var logFileNameWithoutExtension = Path.GetFileNameWithoutExtension(logFilePath);
        var logFileExtension = Path.GetExtension(logFilePath);

        var originalFileName = logFilePath;

        // Rotate existing log files
        for (var i = maxBackupFiles - 1; i >= 1; i--)
        {
            var olderLogFile =
                Path.Combine(logFileDirectory, $"{logFileNameWithoutExtension}_{i}{logFileExtension}");
            var newerLogFile = Path.Combine(logFileDirectory,
                $"{logFileNameWithoutExtension}_{i + 1}{logFileExtension}");

            if (File.Exists(olderLogFile))
            {
                // Rename older logs to their new position
                File.Move(olderLogFile, newerLogFile);
            }
        }

        // Move the original log file to the first backup slot
        var firstBackupLogFile =
            Path.Combine(logFileDirectory, $"{logFileNameWithoutExtension}_1{logFileExtension}");
        if (File.Exists(originalFileName))
        {
            File.Move(originalFileName, firstBackupLogFile);
        }

        // If the maximum number of backups is reached, return the original file name
        return maxBackupFiles > 0 ? originalFileName : null;
    }
}
