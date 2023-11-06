/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/ScrollingTextBox.cs
 * PURPOSE:     Reused for SQLiteGui Project only
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows.Controls;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     The scrolling TextBox class.
    /// </summary>
    internal sealed class ScrollingTextBox : TextBox
    {
        /// <inheritdoc />
        /// <summary>
        ///     Set basic Attributes
        /// </summary>
        /// <param name="e">The Event Argument.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Change the standard behavior to scrolling down
        /// </summary>
        /// <param name="e">The Event Argument.</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            CaretIndex = Text.Length;
            ScrollToEnd();
        }
    }
}
