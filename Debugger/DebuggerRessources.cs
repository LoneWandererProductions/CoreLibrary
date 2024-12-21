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
        ///     Base Text(const). Value: "Color for everything else".
        /// </summary>
        private const string BaseText = "<Color for everything else>";

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
        ///     The spacer (const). Value:  " , ".
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
        ///     Manual Start of the Debugger (const). Value: "Manual Start of Trail.".
        /// </summary>
        internal const string ManualStart = "Manual Start of Trail.";

        /// <summary>
        ///     Gets or sets the error color.
        /// </summary>
        internal const string ErrorColor = "Red";

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        internal const string WarningColor = "Orange";

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        internal const string InformationColor = "Blue";

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        internal const string ExternalColor = "Green";

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        internal const string StandardColor = "Black";

        /// <summary>
        ///     Gets or sets the color of the found item in the line
        /// </summary>
        /// <value>
        ///     The color of the found.
        /// </value>
        internal const string FoundColor = "Yellow";

        /// <summary>
        ///     The log file extension (const). Value: "".log".
        /// </summary>
        internal const string LogFileExtension = ".log";

        /// <summary>
        ///     The log Path (const). Value: "Log".
        /// </summary>
        internal const string LogPath = "Log";


        /// <summary>
        ///     The object formatting (readonly). Value: string.Concat(Environment.NewLine, "Object:", Environment.NewLine).
        /// </summary>
        /// ss
        internal static readonly string ObjectFormatting = string.Concat(Environment.NewLine, "Object:",
            Environment.NewLine);

        /// <summary>
        ///     The initial options for the colors
        /// </summary>
        internal static readonly List<ColorOption> InitialOptions = new()
        {
            new ColorOption { ColorName = DebugRegister.StandardColor, EntryText = BaseText },
            new ColorOption { ColorName = DebugRegister.ErrorColor, EntryText = LogLvlOne },
            new ColorOption { ColorName = DebugRegister.WarningColor, EntryText = LogLvlTwo },
            new ColorOption { ColorName = DebugRegister.InformationColor, EntryText = LogLvlThree },
            new ColorOption { ColorName = DebugRegister.ExternalColor, EntryText = LogLvlFour }
        };
    }
}
