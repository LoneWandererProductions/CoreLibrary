/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugHelper.cs
 * PURPOSE:     Helper Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
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
            textRange.Text = string.Concat(line, Environment.NewLine);

            if (found)
            {
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, DebugRegister.FoundColor);
            }

            ColorOption option;

            for (int i = 1; i < DebugRegister.ColorOptions.Count; i++)
            {
                option = DebugRegister.ColorOptions[i];

                if (!line.StartsWith(option.EntryText, StringComparison.Ordinal))
                {
                    continue;
                }

                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, option.ColorName);
                return;
            }

            option = DebugRegister.ColorOptions[0];

            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, option.ColorName);
        }
    }
}
