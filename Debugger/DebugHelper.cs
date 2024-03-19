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
        internal static void AddRange(TextRange textRange, string line)
        {
            //Todo make configurable in the future in config window

            textRange.Text = string.Concat(line, Environment.NewLine);

            if (line.Contains(nameof(ErCode.Error)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebugRegister.ErrorColor);
            }

            if (line.Contains(nameof(ErCode.Warning)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebugRegister.WarningColor);
            }

            if (line.Contains(nameof(ErCode.Information)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebugRegister.InformationColor);
            }

            if (line.Contains(nameof(ErCode.External)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebugRegister.ExternalColor);
            }

            if (!line.Contains(nameof(ErCode.External)) && !line.Contains(nameof(ErCode.Error)) &&
                !line.Contains(nameof(ErCode.Warning)) && !line.Contains(nameof(ErCode.Information)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebugRegister.StandardColor);
            }
        }
    }
}
