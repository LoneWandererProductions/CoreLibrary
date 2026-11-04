/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        FilterConfigControl.xaml.cs
 * PURPOSE:     Sidebar control for Filter Configuration
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging.Enums;

namespace Common.Images
{
    /// <summary>
    /// Configuration UserControl for Image Filters
    /// </summary>
    public partial class FilterConfigControl
    {
        /// <summary>
        /// The view model
        /// </summary>
        private readonly FilterConfigView _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConfigControl"/> class.
        /// </summary>
        public FilterConfigControl()
        {
            InitializeComponent();
            _viewModel = new FilterConfigView();
            DataContext = _viewModel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConfigControl" /> class.
        /// </summary>
        /// <param name="filter">The filter type to pre-select.</param>
        public FilterConfigControl(FiltersType filter) : this()
        {
            _viewModel.SelectedFilter = filter;
        }
    }
}
