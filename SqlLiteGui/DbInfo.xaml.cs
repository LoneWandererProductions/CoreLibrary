/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DbInfo.xaml.cs
 * PURPOSE:     Display Database Infos
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, no not possible for User-control

namespace SQLiteGui
{
    /// <inheritdoc />
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
            ViewModel = new DbInfoViewModel();
            DataContext = ViewModel; // Set DataContext to ViewModel
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        private DbInfoViewModel ViewModel { get; }
    }
}
