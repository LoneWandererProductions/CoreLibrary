/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DbInfo.xaml.cs
 * PURPOSE:     Display Database Infos
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, no not possible for User-control

using System.Windows.Controls;

namespace SQLiteGui;

/// <inheritdoc cref="UserControl" />
/// <summary>
///     The data base info class.
/// </summary>
internal sealed partial class DbInfo
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:SQLiteGui.DbInfo" /> class.
    /// </summary>
    public DbInfo()
    {
        InitializeComponent();
        ViewModel = new DbInfoViewModel();
        DataContext = ViewModel; // Set DataContext to ViewModel
    }

    /// <summary>
    ///     Gets the view model.
    /// </summary>
    /// <value>
    ///     The view model.
    /// </value>
    private DbInfoViewModel ViewModel { get; }

    /// <summary>
    ///     Called when [text box text changed].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        textBox.CaretIndex = textBox.Text.Length; // Move caret to the end
        textBox.ScrollToEnd(); // Scroll to the end
    }
}
