/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/CustomErrorDialog.cs
 * PURPOSE:     Viewer for our Error Dialog
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.Windows;

namespace CommonDialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// The error window.
    /// </summary>
    /// <seealso cref="T:System.Windows.Window" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    public sealed partial class CustomErrorDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonDialogs.CustomErrorDialog" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        public CustomErrorDialog(string header, string message, string source = null!, string details = null!)
        {
            InitializeComponent();

            // Set default error title or use a localized string
            ErrorTitleText.Text = header;

            // Set error message, prepend source if provided
            ErrorMessageText.Text = string.IsNullOrWhiteSpace(source) ? message : $"{source}\n{message}";

            // Set error details if provided
            ErrorDetailsText.Text = details;
        }


        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
