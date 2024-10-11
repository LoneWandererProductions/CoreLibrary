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
    /// <summary>
    ///     The data base info class.
    /// </summary>
    internal sealed partial class DbInfo
    {
        public DbInfoViewModel ViewModel { get; }

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
        ///     User Output
        /// </summary>
        /// <param name="message">Messages from the system</param>
        internal void SetData(string message)
        {
            ViewModel.AppendInfo(message); // Use ViewModel to handle data
        }
    }
}
