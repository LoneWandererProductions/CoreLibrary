/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebuggerResources.cs
 * PURPOSE:     Resource String
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Immutable;

namespace Debugger;

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
    ///     Base Text(const). Value: "Color for everything else".
    /// </summary>
    private const string BaseText = "<Color for everything else>";

    /// <summary>
    ///     The log Level Error (const). Value:  "Error: ".
    /// </summary>
    internal const string LogLvlError = "Error: ";

    /// <summary>
    ///     The log Level Warning (const). Value: "Warning: ".
    /// </summary>
    internal const string LogLvlWarning = "Warning: ";

    /// <summary>
    ///     The log Level Information (const). Value: "Information: "
    /// </summary>
    internal const string LogLvlInformation = "Information: ";

    /// <summary>
    ///     The log Level External (const). Value: "External Source: ".
    /// </summary>
    internal const string LogLvlExternal = "External Source: ";

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
    ///     The initial options for the colors
    /// </summary>
    internal static readonly ImmutableList<ColorOption> InitialOptions = ImmutableList.Create(
        new ColorOption { ColorName = DebugRegister.StandardColor, EntryText = BaseText },
        new ColorOption { ColorName = DebugRegister.ErrorColor, EntryText = LogLvlError },
        new ColorOption { ColorName = DebugRegister.WarningColor, EntryText = LogLvlWarning },
        new ColorOption { ColorName = DebugRegister.InformationColor, EntryText = LogLvlInformation },
        new ColorOption { ColorName = DebugRegister.ExternalColor, EntryText = LogLvlExternal }
    );
}
