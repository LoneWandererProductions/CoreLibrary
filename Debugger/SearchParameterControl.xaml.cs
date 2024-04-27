using System;
using System.Windows.Controls;

namespace Debugger
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///    
    /// </summary>
    public sealed partial class SearchParameterControl
    {
        public event EventHandler DeleteLogic;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchParameterControl"/> class.
        /// </summary>
        public SearchParameterControl()
        {
            InitializeComponent();
            View.Reference = this;
        }

        internal void DeleteClicked()
        {
            DeleteLogic?.Invoke(this, EventArgs.Empty);
        }
    }
}
