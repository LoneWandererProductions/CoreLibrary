/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DbInfo.xaml.cs
 * PURPOSE:     Display Database Infos
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, no not possible for User-control

using System;
using System.Diagnostics;
using System.Windows;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
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
        }

        /// <summary>
        ///     User Output
        /// </summary>
        /// <param name="message">Messages from the system</param>
        internal void SetData(string message)
        {
            Debug.WriteLine(string.Concat(message, Environment.NewLine));
            TxtBoxInfo.AppendText(string.Concat(message, Environment.NewLine));
        }
    }
}
