/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        MenuBand.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, User-control for the Table Part
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, can't be made internal since we need it public as User-control

using System.Windows;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     General Overview Window of the Tables inside the Database
    /// </summary>
    public sealed partial class TableOverview
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initiate Table Overview
        /// </summary>
        public TableOverview()
        {
            InitializeComponent();
        }
    }
}
