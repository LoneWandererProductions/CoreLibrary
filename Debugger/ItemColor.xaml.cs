/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        ItemColor.xaml.cs
 * PURPOSE:     UserControl ItemColor, that holds the Info about Color and Text
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows.Controls;
using CommonControls.Images;

namespace Debugger
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     ItemColor Item
    /// </summary>
    /// <seealso cref="T:System.Windows.Controls.UserControl" />
    /// <seealso cref="!:IComponentConnector" />
    internal sealed partial class ItemColor
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Debugger.ItemColor" /> class.
        /// </summary>
        public ItemColor()
        {
            InitializeComponent();
            View.Reference = this;
            ColorPicker.ColorChanged += ColorPicker_ColorChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Debugger.ItemColor" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ItemColor(int id)
        {
            InitializeComponent();
            Id = id;
            View.Reference = this;
            ColorPicker.ColorChanged += ColorPicker_ColorChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Debugger.ItemColor" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="option">The option for the parameter.</param>
        public ItemColor(int id, ColorOption option)
        {
            InitializeComponent();
            View.Reference = this;
            Id = id;
            View.EntryText = option.EntryText;
            View.ColorName = option.ColorName;
            ColorPicker.ColorChanged += ColorPicker_ColorChanged;
        }

        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        private int Id { get; }

        /// <summary>
        ///     Occurs when [delete logic].
        /// </summary>
        public event EventHandler<int>? DeleteLogic;


        /// <summary>
        ///     Gets the option.
        /// </summary>
        /// <returns>Return selected Color Options</returns>
        internal ColorOption GetOption()
        {
            return View.Options;
        }

        /// <summary>
        ///     Deletes the clicked.
        /// </summary>
        internal void DeleteClicked()
        {
            DeleteLogic(this, Id);
        }

        /// <summary>
        ///     Colors the picker color changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="color">The color.</param>
        private void ColorPicker_ColorChanged(object sender, string color)
        {
            View.ColorName = color;
        }
    }
}
