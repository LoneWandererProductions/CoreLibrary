/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/SqlConnect.cs
 * PURPOSE:     Generic Sql Dialog for a connection String
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Windows;
using System.Windows.Markup;

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Login Window for Sql Server
    /// </summary>
    /// <seealso cref="Window" />
    /// <seealso cref="IComponentConnector" />
    public partial class SqlLogin
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.SqlLogin" /> class.
        /// </summary>
        public SqlLogin()
        {
            InitializeComponent();
        }
    }
}
