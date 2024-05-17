/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebuggerResources.cs
 * PURPOSE:     Resource String
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace Debugger
{
    /// <summary>
    ///     The debug channel Resources class.
    /// </summary>
    internal static class DebuggerResources
    {
        /// <summary>
        ///     The Idle Timer (for delay in the Debug Log writer) (const). Value: 100.
        /// </summary>
        internal const int Idle = 100;

        /// <summary>
        ///     The config file (const). Value: "Config.xml".
        /// </summary>
        internal const string ConfigFile = "Config.Debug.xml";

        /// <summary>
        ///     The file name (const). Value: "DebugLog.txt".
        /// </summary>
        internal const string FileName = "DebugLog.txt";

        /// <summary>
        ///     The file extension (const). Value: "Log Files (*.txt)|*.txt;".
        /// </summary>
        internal const string FileExt = "Log Files (*.txt)|*.txt;";

        /// <summary>
        ///     Base Text(const). Value: "<Color for everything else>".
        /// </summary>
        internal const string BaseText = "<Color for everything else>";

        /// <summary>
        ///     The log Level one (const). Value: " , Error: ".
        /// </summary>
        internal const string LogLvlOne = "Error: ";

        /// <summary>
        ///     The log Level two (const). Value: " , Warning: ".
        /// </summary>
        internal const string LogLvlTwo = "Warning: ";

        /// <summary>
        ///     The log Level three (const). Value: " , Information: ".
        /// </summary>
        internal const string LogLvlThree = "Information: ";

        /// <summary>
        ///     The log Level four (const). Value: "External Source: ".
        /// </summary>
        internal const string LogLvlFour = "External Source: ";

        /// <summary>
        ///     The error serializing (const). Value: "Unexpected Problems appeared while trying to serialize object: ".
        /// </summary>
        internal const string ErrorSerializing = "Unexpected Problems appeared while trying to serialize object: ";

        /// <summary>
        ///     The error while Processing (const). Value: "Error processing message queue:".
        /// </summary>
        internal const string ErrorProcessing = "Error processing message queue:";

        /// <summary>
        ///     The formatting (const). Value: " : ".
        /// </summary>
        internal const string Formatting = " : ";

        /// <summary>
        /// The spacer (const). Value:  " , ".
        /// </summary>
        internal const string Spacer = " , ";

        /// <summary>
        ///     The caller (const). Value: "Method: ".
        /// </summary>
        internal const string Caller = "Method: ";

        /// <summary>
        ///     The Line Number (const). Value: "Line Number: ".
        /// </summary>
        internal const string LineNumber = " , Line Number: ";

        /// <summary>
        ///     The Location (const). Value: "Location: ".
        /// </summary>
        internal const string Location = "Location: ";

        /// <summary>
        ///     The Trail Window (const). Value: "Trail.exe".
        /// </summary>
        internal const string TrailWindow = "Debugger.exe";

        /// <summary>
        ///     The arguments none (const). Value: "/".
        /// </summary>
        internal const string ArgumentsNone = "/";

        /// <summary>
        ///     The error log file delete (const). Value: "Could not delete Log File : ".
        /// </summary>
        internal const string ErrorLogFileDelete = "Could not delete Log File : ";

        /// <summary>
        ///     The information create log file (const). Value: "Created a new Log File".
        /// </summary>
        internal const string InformationCreateLogFile = "Created a new Log File";

        /// <summary>
        ///     Manual Start of the Debugger (const). Value: "Manual Start for Debug started".
        /// </summary>
        internal const string ManualStart = "Manual Start for Debug started";

        /// <summary>
        ///     The object formatting (readonly). Value: string.Concat(Environment.NewLine, "Object:", Environment.NewLine).
        /// </summary>
        /// ss
        internal static readonly string ObjectFormatting = string.Concat(Environment.NewLine, "Object:",
            Environment.NewLine);

        /// <summary>
        ///     Gets or sets the error color.
        /// </summary>
        internal static string ErrorColor { get; set; } = "Red";

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        internal static string WarningColor { get; set; } = "Orange";

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        internal static string InformationColor { get; set; } = "Blue";

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        internal static string ExternalColor { get; set; } = "Green";

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        internal static string StandardColor { get; set; } = "Black";

        /// <summary>
        /// The initial options for the colors
        /// </summary>
        internal static readonly List<ColorOption> InitialOptions = new()
        {
            new ColorOption() {ColorName = StandardColor, EntryText = BaseText},
            new ColorOption() {ColorName = ErrorColor, EntryText = LogLvlOne},
            new ColorOption() {ColorName = WarningColor, EntryText = LogLvlTwo},
            new ColorOption() {ColorName = InformationColor, EntryText = LogLvlThree},
            new ColorOption() {ColorName = ExternalColor, EntryText = LogLvlFour},
        };
    }
}
