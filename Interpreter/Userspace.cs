/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/UserSpace.cs
 * PURPOSE:     Container for external Commands
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Interpreter;

/// <summary>
///     Command Handler Object
/// </summary>
public sealed class UserSpace
{
    /// <summary>
    ///     Gets or sets the name of the userSpace.
    /// </summary>
    /// <value>
    ///     The name of the userSpace.
    /// </value>
    internal string UserSpaceName { get; init; }

    /// <summary>
    ///     Gets or sets the commands.
    /// </summary>
    /// <value>
    ///     The commands.
    /// </value>
    internal Dictionary<int, InCommand> Commands { get; init; }

    /// <summary>
    ///     Gets or sets the extension commands.
    /// </summary>
    /// <value>
    ///     The extension commands.
    /// </value>
    internal Dictionary<int, InCommand> ExtensionCommands { get; init; }
}
