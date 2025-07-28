/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DataOverview.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, User-control for the Display Part
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;

// ReSharper disable MemberCanBeInternal, no not possible for User-control

namespace SQLiteGui;

/// <inheritdoc cref="Window" />
/// <summary>
///     The data overview class.
/// </summary>
public sealed partial class DataOverview
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:SQLiteGui.DataOverview" /> class.
    /// </summary>
    public DataOverview()
    {
        InitializeComponent();
    }
}
