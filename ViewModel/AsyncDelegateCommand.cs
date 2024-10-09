/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/AsyncDelegateCommand.cs
 * PURPOSE:     Part of the View Model, Async Version of the DelegateCommand
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://msdn.microsoft.com/de-de/library/system.windows.input.icommand%28v=vs.110%29.aspx
 *              https://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
 *              https://stackoverflow.com/questions/48527651/full-implementation-of-relay-command-can-it-be-applied-to-all-cases
 */

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModel
{
    /// <summary>
    ///     An asynchronous delegate command class.
    /// </summary>
    public sealed class AsyncDelegateCommand<T> : ICommand
    {
        /// <summary>
        /// The action to execute.
        /// </summary>
        private readonly Func<T, Task> _execute;

        /// <summary>
        ///     The predicate to determine if the command can execute.
        /// </summary>
        private readonly Predicate<T> _canExecute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncDelegateCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute.</param>
        /// <param name="canExecute">
        ///     A predicate to determine if the command can execute. If null, the command is always
        ///     executable.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the execute action is null.</exception>
        public AsyncDelegateCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the action.</param>
        public async void Execute(object parameter)
        {
            await _execute((T)parameter);
        }

        /// <summary>
        ///     Determines if the command can execute.
        /// </summary>
        /// <param name="parameter">The parameter for the predicate.</param>
        /// <returns>True if the command can execute, otherwise false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
