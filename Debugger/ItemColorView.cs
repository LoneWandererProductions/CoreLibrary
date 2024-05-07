/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ItemColorView.cs
 * PURPOSE:     View for ItemColor Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace Debugger
{
    /// <summary>
    /// View vor the ItemColor Control
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    internal sealed class ItemColorView : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal ColorOption Options => GetOptions();

        /// <summary>
        ///     The delete command
        /// </summary>
        private ICommand _deleteCommand;

        /// <summary>
        ///     The entry text
        /// </summary>
        private string _entryText;

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
        /// Gets or sets the name of the color.
        /// </summary>
        /// <value>
        /// The name of the color.
        /// </value>
        public string ColorName { get; internal set; }

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
            _deleteCommand = new DelegateCommand<object>(DeleteAction, CanExecute);

        /// <inheritdoc />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets a value indicating whether this instance can execute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///     <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </value>
        public bool CanExecute(object obj)
        {
            // check if executing is allowed, not used right now
            return true;
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
        /// Gets the options.
        /// </summary>
        /// <returns>ColorOption Container</returns>
        private ColorOption GetOptions()
        {
            return new ColorOption
            {
                ColorName = ColorName,
                EntryText = EntryText
            };
        }
    }
}
