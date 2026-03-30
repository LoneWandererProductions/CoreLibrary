/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/SearchParameterControl.xaml.cs
 * PURPOSE:     Control for the Parameter, one Control, one Parameter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Controls;

namespace CommonFilter
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Search Parameter
    /// </summary>
    internal sealed partial class SearchParameterControl : UserControl
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.Filters.SearchParameterControl" /> class.
        /// </summary>
        public SearchParameterControl()
        {
            InitializeComponent();
        }
    }
}
