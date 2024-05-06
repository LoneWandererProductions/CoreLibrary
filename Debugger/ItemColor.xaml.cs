using System;
using System.Windows.Controls;

namespace Debugger
{
    /// <summary>
    /// ItemColor Item
    /// </summary>
    /// <seealso cref="UserControl" />
    /// <seealso cref="IComponentConnector" />
    public partial class ItemColor : UserControl
    {
        private int Id { get; }

        public ItemColor()
        {
            InitializeComponent();
            View.Reference = this;
        }

        public ItemColor(int id)
        {
            InitializeComponent();
            Id = id;
            View.Reference = this;
        }

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
