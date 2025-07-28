/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SQLiteGuiWindow.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, not yet completely cleared out into a View
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;

namespace SQLiteGui;

/// <inheritdoc cref="Window" />
/// <summary>
///     Starter Window, here we forward all interactions.
/// </summary>
public sealed partial class SqLiteGuiWindow
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:SQLiteGui.SQLiteGuiWindow" /> class.
    /// </summary>
    public SqLiteGuiWindow()
    {
        InitializeComponent();
        // No need for event subscriptions here
    }
}
