/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ItemColor.xaml.cs
 * PURPOSE:     Usercontrol ItemColor, that holds the Info about Color and Text
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


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
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        private int Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemColor"/> class.
        /// </summary>
        public ItemColor()
        {
            InitializeComponent();
            View.Reference = this;
            ColorPicker.ColorChanged += ColorPicker_ColorChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemColor"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
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

        /// <summary>
        /// Colors the picker color changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="color">The color.</param>
        private void ColorPicker_ColorChanged(object sender, string color)
        {
            View.ColorName = color;
        }
    }
}
