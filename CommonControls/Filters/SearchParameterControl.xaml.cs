/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/SearchParameterControl.xaml.cs
 * PURPOSE:     
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.Windows.Controls;

namespace CommonControls.Filters
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///    Search Parameter
    /// </summary>
    public sealed partial class SearchParameterControl
    {
        /// <summary>
        /// Define a delegate with an integer parameter
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="value">The value.</param>
        public delegate void DeleteLogicEventHandler(object sender, int value);


        /// <summary>
        /// Occurs when [delete logic].
        /// </summary>
        public event DeleteLogicEventHandler DeleteLogic;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        private int Id { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.Filters.SearchParameterControl" /> class.
        /// </summary>
        public SearchParameterControl()
        {
            InitializeComponent();
            View.Reference = this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.Filters.SearchParameterControl" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public SearchParameterControl(int id)
        {
            InitializeComponent();
            Id = id;
            View.Reference = this;
        }

        /// <summary>
        /// Deletes the clicked.
        /// </summary>
        internal void DeleteClicked()
        {
            DeleteLogic?.Invoke(this, Id);
        }
    }
}
