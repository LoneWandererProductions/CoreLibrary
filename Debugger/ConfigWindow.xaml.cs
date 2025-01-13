/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ConfigWindow.xaml.cs
 * PURPOSE:     Config Window
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;

namespace Debugger
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Interaction logic for Config.xaml
    /// </summary>
    internal sealed partial class ConfigWindow
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Debugger.ConfigWindow" /> class.
        /// </summary>
        internal ConfigWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the BtnReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            DebugRegister.Reset();
            ColorOptions.AddItemColors(DebugRegister.ColorOptions);
            DataContext = DebugRegister.Config;
        }

        /// <summary>
        ///     The Button save click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var options = ColorOptions.GetColorOptions();
            DebugRegister.XmlSerializerObject(DataContext, options);
        }

        /// <summary>
        ///     The window loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ColorOptions.AddItemColors(DebugRegister.ColorOptions);
            DataContext = DebugRegister.Config;
        }

        /// <summary>
        ///     The Button Cancel click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void BtnCnl_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
