/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Debugger
 * FILE:        ConfigWindow.xaml.cs
 * PURPOSE:     Configuration Window for Debugger
 * AUTHOR:      Peter Geinitz (Wayfarer)
 */

using System.Linq;
using System.Windows;

namespace Debugger
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Interaction logic for ConfigWindow.xaml.
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
        ///     Handles the reset button click event.
        ///     Resets configuration and updates UI elements.
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetConfiguration();
        }

        /// <summary>
        ///     Handles the save button click event.
        ///     Saves the current configuration.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();
        }

        /// <summary>
        ///     Handles the window loaded event.
        ///     Initializes color options and binds the DataContext.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeConfiguration();
        }

        /// <summary>
        ///     Handles the cancel button click event.
        ///     Closes the configuration window.
        /// </summary>
        private void BtnCnl_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Resets the configuration and updates UI elements.
        /// </summary>
        private void ResetConfiguration()
        {
            DebugRegister.Reset();
            ColorOptions.AddItemColors(DebugRegister.ColorOptions);
            DataContext = DebugRegister.Config;
        }

        /// <summary>
        ///     Saves the current configuration using XML serialization.
        /// </summary>
        private void SaveConfiguration()
        {
            var options = ColorOptions.GetColorOptions();
            DebugRegister.XmlSerializerObject(DataContext, options);
        }

        /// <summary>
        ///     Initializes color options and binds the DataContext.
        /// </summary>
        private void InitializeConfiguration()
        {
            var options = DebugRegister.ColorOptions;
            if (options?.Any() != true)
            {
                options = DebuggerResources.InitialOptions.ToList();
            }

            ColorOptions.AddItemColors(options);
            DataContext = DebugRegister.Config;
        }
    }
}
