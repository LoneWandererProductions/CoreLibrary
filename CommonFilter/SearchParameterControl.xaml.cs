/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/SearchParameterControl.xaml.cs
 * PURPOSE:     Control for the Parameter, one Control, one Parameter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Windows.Controls;

namespace CommonFilter
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Search Parameter
    /// </summary>
    internal sealed partial class SearchParameterControl
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.Filters.SearchParameterControl" /> class.
        /// </summary>
        public SearchParameterControl()
        {
            InitializeComponent();
            View.Reference = this;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.Filters.SearchParameterControl" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public SearchParameterControl(int id)
        {
            InitializeComponent();
            Id = id;
            View.Reference = this;
        }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        private int Id { get; }

        /// <summary>
        ///     Occurs when [delete logic].
        /// </summary>
        public event EventHandler<int> DeleteLogic;

        /// <summary>
        ///     Deletes the clicked.
        /// </summary>
        internal void DeleteClicked()
        {
            DeleteLogic(this, Id);
        }
    }
}
