/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/InputBox.xaml.cs
 * PURPOSE:     FolderView Control, can be used independent of the FolderBrowser
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System.Windows;

namespace CommonDialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     A Simple Input Box
    /// </summary>
    public sealed partial class InputBox
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class.
        /// </summary>
        public InputBox()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        public InputBox(string header)
        {
            InitializeComponent();
            Title = header;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="text">The text.</param>
        public InputBox(string header, string text)
        {
            InitializeComponent();
            Title = header;
            LLblText.Content = text;
        }

        /// <summary>
        ///     Gets the input text.
        /// </summary>
        /// <value>
        ///     The input text.
        /// </value>
        public string InputText { get; private set; }

        /// <summary>
        ///     Handles the Click event of the BtnOkay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            Close();
        }

        /// <summary>
        ///     Handles the Click event of the BtnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
