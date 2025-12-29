/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/InputBinaryWindow.xaml.cs
 * PURPOSE:     Set custom Input, right now: where Clause,Copy Table
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The input binary window class.
    /// </summary>
    internal sealed partial class InputBinaryWindow
    {
        /// <summary>
        ///     The description (readonly).
        /// </summary>
        private readonly string _description;

        /// <summary>
        ///     The first (readonly).
        /// </summary>
        private readonly string _first;

        /// <summary>
        ///     The first value (readonly).
        /// </summary>
        private readonly string _firstValue;

        /// <summary>
        ///     The second (readonly).
        /// </summary>
        private readonly string _second;

        /// <summary>
        ///     The title (readonly).
        /// </summary>
        private readonly string _title;

        /// <summary>
        ///     The binary.
        /// </summary>
        private Binary _binary;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.InputBinaryWindow" /> class.
        /// </summary>
        internal InputBinaryWindow()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Custom Input Window
        /// </summary>
        /// <param name="title">Title of Window</param>
        /// <param name="description">Description Label</param>
        /// <param name="first">First Input Label</param>
        /// <param name="second">Second Input Label</param>
        internal InputBinaryWindow(string title, string description, string first, string second)
        {
            InitializeComponent();

            _title = title;
            _description = description;
            _first = first;
            _second = second;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Custom Input Window, in this case first Value is already set
        /// </summary>
        /// <param name="title">Title of Window</param>
        /// <param name="description">Description Label</param>
        /// <param name="first">First Input Label</param>
        /// <param name="second">Second Input Label</param>
        /// <param name="firstValue">Set First Value</param>
        internal InputBinaryWindow(string title, string description, string first, string second, string firstValue)
        {
            InitializeComponent();

            _title = title;
            _description = description;
            _first = first;
            _second = second;
            _firstValue = firstValue;
        }

        /// <summary>
        ///     Gets the Parameter clause.
        /// </summary>
        internal static Binary ParamsClause { get; private set; }

        /// <summary>
        ///     Get the basics in Place
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = _title;
            LblDescription.Content = _description;
            LblFirst.Content = _first;
            LblSecond.Content = _second;

            _binary = new Binary { Where = _firstValue };
            DataContext = _binary;
        }

        /// <summary>
        ///     Okay and Save
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            ParamsClause = _binary;
            Close();
        }

        /// <summary>
        ///     Cancel without Save
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ParamsClause = null;
            Close();
        }
    }
}
