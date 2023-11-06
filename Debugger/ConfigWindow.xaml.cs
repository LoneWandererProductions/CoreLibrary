﻿/*
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
        ///     The Button save click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DebugRegister.ErrorColor = CombColorError.Color;
            DebugRegister.WarningColor = CombColorWarning.Color;
            DebugRegister.InformationColor = CombColorInformation.Color;
            DebugRegister.ExternalColor = CombColorExternal.Color;
            DebugRegister.StandardColor = CombColorStandard.Color;
            DebugRegister.XmlSerializerObject(DataContext);
        }

        /// <summary>
        ///     The window loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DebugRegister.Config == null)
            {
                DebugRegister.ReadConfigFile();
            }

            DataContext = DebugRegister.Config;
            CombColorError.Color = DebugRegister.ErrorColor;
            CombColorWarning.Color = DebugRegister.WarningColor;
            CombColorInformation.Color = DebugRegister.InformationColor;
            CombColorExternal.Color = DebugRegister.ExternalColor;
            CombColorStandard.Color = DebugRegister.StandardColor;
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
