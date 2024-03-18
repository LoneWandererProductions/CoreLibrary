/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebuggerResources.cs
 * PURPOSE:     Resource String
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace Debugger
{
    /// <summary>
    ///     The debug channel Resources class.
    /// </summary>
    internal static class DebuggerResources
    {
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
        internal const string FileExt= "Log Files (*.txt)|*.txt;";


        /// <summary>
        ///     The log Level one (const). Value: " , Error: ".
        /// </summary>
        internal const string LoglvlOne = " , Error: ";

        /// <summary>
        ///     The log Level two (const). Value: " , Warning: ".
        /// </summary>
        internal const string LoglvlTwo = " , Warning: ";

        /// <summary>
        ///     The log Level three (const). Value: " , Information: ".
        /// </summary>
        internal const string LoglvlThree = " , Information: ";

        /// <summary>
        ///     The log Level four (const). Value: "External Source: ".
        /// </summary>
        internal const string LoglvlFour = "External Source: ";

        /// <summary>
        ///     The error serializing (const). Value: "Unexpected Problems appeared while trying to serialize object: ".
        /// </summary>
        internal const string ErrorSerializing = "Unexpected Problems appeared while trying to serialize object: ";

        /// <summary>
        ///     The formating (const). Value: " : ".
        /// </summary>
        internal const string Formating = " : ";

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
        ///     The object formating (readonly). Value: string.Concat(Environment.NewLine, "Object:", Environment.NewLine).
        /// </summary>
        /// ss
        internal static readonly string ObjectFormating = string.Concat(Environment.NewLine, "Object:",
            Environment.NewLine);
    }
}
