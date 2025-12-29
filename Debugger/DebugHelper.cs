/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugHelper.cs
 * PURPOSE:     Helper Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Windows.Documents;

namespace Debugger
{
    /// <summary>
    ///     The debug helper class.
    /// </summary>
    internal static class DebugHelper
    {
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
    }
}
