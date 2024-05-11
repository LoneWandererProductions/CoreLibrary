﻿/*
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
            //Todo make configurable in the future in config window

            textRange.Text = string.Concat(line, Environment.NewLine);

            if (found)
            {
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, DebugRegister.FoundColor);
            }

            if (line.StartsWith(DebuggerResources.LogLvlOne, StringComparison.Ordinal))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebuggerResources.ErrorColor);
            }

            if (line.StartsWith(DebuggerResources.LogLvlTwo, StringComparison.Ordinal))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebuggerResources.WarningColor);
            }

            if (line.StartsWith(DebuggerResources.LogLvlThree, StringComparison.Ordinal))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebuggerResources.InformationColor);
            }

            if (line.StartsWith(DebuggerResources.LogLvlFour, StringComparison.Ordinal))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebuggerResources.ExternalColor);
            }

            if (!line.Contains(nameof(ErCode.External)) && !line.Contains(nameof(ErCode.Error)) &&
                !line.Contains(nameof(ErCode.Warning)) && !line.Contains(nameof(ErCode.Information)))
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, DebuggerResources.StandardColor);
            }
        }
    }
}
