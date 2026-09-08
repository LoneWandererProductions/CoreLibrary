/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        ErrorDialog.cs
 * PURPOSE:     Viewer for our Error Dialog
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Text;
using System.Windows;

namespace Common.Dialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The error window.
    /// </summary>
    public sealed partial class ErrorDialog
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorDialog" /> class.
        /// </summary>
        /// <param name="header">The main error header.</param>
        /// <param name="message">The descriptive message.</param>
        /// <param name="source">The originating source or component.</param>
        /// <param name="details">Extended stack trace or detail logs.</param>
        public ErrorDialog(string header, string message, string? source = null, string? details = null)
        {
            InitializeComponent();

            ErrorTitleText.Text = string.IsNullOrWhiteSpace(header) ? "An Error Occurred" : header;
            ErrorMessageText.Text = message ?? string.Empty;

            if (string.IsNullOrWhiteSpace(source))
            {
                ErrorSourceText.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorSourceText.Text = $"Source: {source}";
                ErrorSourceText.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(details))
            {
                DetailsExpander.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorDetailsText.Text = details;
                DetailsExpander.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        ///     Copies the formatted error details directly to the clipboard.
        /// </summary>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[Header] {ErrorTitleText.Text}");

            if (ErrorSourceText.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(ErrorSourceText.Text))
            {
                sb.AppendLine($"[{ErrorSourceText.Text}]");
            }

            if (!string.IsNullOrWhiteSpace(ErrorMessageText.Text))
            {
                sb.AppendLine($"[Message]\n{ErrorMessageText.Text}");
            }

            if (DetailsExpander.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(ErrorDetailsText.Text))
            {
                sb.AppendLine();
                sb.AppendLine("[Details]");
                sb.AppendLine(ErrorDetailsText.Text);
            }

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to copy to clipboard: {ex.Message}", "Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        ///     Handles the Click event of the CloseButton control.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}