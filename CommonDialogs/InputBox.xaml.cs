/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     CommonDialogs
 * FILE:        InputBox.xaml.cs
 * PURPOSE:     FolderView Control, can be used independently of the FolderBrowser
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System.Windows;

namespace CommonDialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     A simple Input Box dialog for user input.
    /// </summary>
    public sealed partial class InputBox
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class with no header or text.
        /// </summary>
        public InputBox() : this(string.Empty, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class with a specified header.
        /// </summary>
        /// <param name="header">The header text for the InputBox title.</param>
        public InputBox(string header) : this(header, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputBox" /> class with a specified header and text.
        /// </summary>
        /// <param name="header">The header text for the InputBox title.</param>
        /// <param name="text">The content text displayed inside the InputBox.</param>
        public InputBox(string header, string text)
        {
            InitializeComponent();
            Title = header;
            LLblText.Content = text;
        }

        /// <summary>
        ///     Gets the input text entered by the user.
        /// </summary>
        public string InputText { get; private set; } = string.Empty;

        /// <summary>
        ///     Handles the click event of the "Okay" button.
        /// </summary>
        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            Close();
        }

        /// <summary>
        ///     Handles the click event of the "Close" button, closing the InputBox without saving any input.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
