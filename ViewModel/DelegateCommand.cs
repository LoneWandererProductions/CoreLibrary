/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/DelegateCommand.cs
 * PURPOSE:     Part of the View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://msdn.microsoft.com/de-de/library/system.windows.input.icommand%28v=vs.110%29.aspx
 *              https://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
 *              https://stackoverflow.com/questions/48527651/full-implementation-of-relay-command-can-it-be-applied-to-all-cases
 */

using System;
using System.Windows.Input;

namespace ViewModel
{
    /// <inheritdoc />
    /// <summary>
    ///     The delegate command class.
    /// </summary>
    public sealed class DelegateCommand<T> : ICommand
    {
        /// <summary>
        ///     The action (readonly).
        /// </summary>
        private readonly Action<T> _action;

        /// <summary>
        ///     The can execute
        /// </summary>
        private readonly Predicate<T> _canExecute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ICommand" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="canExecute">A boolean property to containing current permissions to execute the command</param>
        /// <exception cref="ArgumentNullException">action</exception>
        public DelegateCommand(Action<T> action, Predicate<T> canExecute)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        /// <summary>
        ///     The execute.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void Execute(object parameter)
        {
            _action((T)parameter);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Check if it can be executed
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The Execute Check<see cref="T:System.Boolean" />.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) != false;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Must be implemented
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
