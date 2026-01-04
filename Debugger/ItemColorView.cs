/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        ItemColorView.cs
 * PURPOSE:     View for ItemColor Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Input;
using ViewModel;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    ///     View vor the ItemColor Control
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    internal sealed class ItemColorView : ViewModelBase
    {
        /// <summary>
        ///     The color name
        /// </summary>
        private string _colorName;

        /// <summary>
        ///     The entry text
        /// </summary>
        private string _entryText;

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal ColorOption Options => GetOptions();

        /// <summary>
        ///     Gets or sets the entry text.
        /// </summary>
        /// <value>
        ///     The entry text.
        /// </value>
        public string EntryText
        {
            get => _entryText;
            set
            {
                _entryText = value;
                OnPropertyChanged(nameof(EntryText));
            }
        }

        /// <summary>
        ///     Gets or sets the name of the color.
        /// </summary>
        /// <value>
        ///     The name of the color.
        /// </value>
        public string ColorName
        {
            get => _colorName;
            set
            {
                _colorName = value;
                //set Color of the Color Selection
                Reference.ColorPicker.StartColor = value;
                OnPropertyChanged(nameof(ColorName));
            }
        }

        /// <summary>
        ///     Gets or sets the reference.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public ItemColor Reference { get; set; }

        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand DeleteCommand =>
            new DelegateCommand<object>(DeleteAction, CanExecute);

        /// <summary>
        ///     Deletes the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DeleteAction(object obj)
        {
            // Perform delete action logic
            // Raise the event
            Reference.DeleteClicked();
        }

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <returns>ColorOption Container</returns>
        private ColorOption GetOptions()
        {
            return new ColorOption { ColorName = ColorName, EntryText = EntryText };
        }
    }
}
