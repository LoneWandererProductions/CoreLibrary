/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/SqlConnect.cs
 * PURPOSE:     Generic Sql Dialog for a connection String
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Windows;

namespace CommonDialogs;

/// <inheritdoc cref="Window" />
/// <summary>
///     Login Window for Sql Server
/// </summary>
/// <seealso cref="Window" />
public sealed partial class SqlLogin
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlLogin" /> class.
    /// </summary>
    public SqlLogin()
    {
        InitializeComponent();
    }
}
