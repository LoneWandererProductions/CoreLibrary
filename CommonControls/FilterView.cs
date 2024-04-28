using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace CommonControls
{
    internal sealed class FilterView : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        public Filter Reference { get; set; }

        /// <summary>
        ///     The add command
        /// </summary>
        private ICommand _addCommand;

        /// <summary>
        ///     The add command
        /// </summary>
        private ICommand _doneCommand;

        /// <summary>
        /// Gets the add command.
        /// </summary>
        /// <value>
        /// The add command.
        /// </value>
        public ICommand AddCommand =>
            _addCommand ??= new DelegateCommand<object>(AddAction, CanExecute);

        /// <summary>
        /// Gets the done command.
        /// </summary>
        /// <value>
        /// The done command.
        /// </value>
        public ICommand DoneCommand =>
            _doneCommand ??= new DelegateCommand<object>(DoneAction, CanExecute);

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Adds action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void AddAction(object obj)
        {
        }


        /// <summary>
        /// Done action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DoneAction(object obj)
        {
        }

    }
}
