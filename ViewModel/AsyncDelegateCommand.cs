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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModel
{
    /// <inheritdoc />
    /// <summary>
    ///     An asynchronous delegate command class.
    /// </summary>
    public sealed class AsyncDelegateCommand<T> : ICommand
    {
        /// <summary>
        ///     The predicate to determine if the command can execute.
        /// </summary>
        private readonly Predicate<T> _canExecute;

        /// <summary>
        ///     The action to execute.
        /// </summary>
        private readonly Func<T, Task> _execute;

        /// <summary>
        /// The on exception
        /// </summary>
        private readonly Action<Exception>? _onException;

        /// <summary>
        /// The is executing
        /// </summary>
        private bool _isExecuting;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncDelegateCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute.</param>
        /// <param name="canExecute">
        ///     A predicate to determine if the command can execute. If null, the command is always
        ///     executable.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the execute action is null.</exception>
        public AsyncDelegateCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the action.</param>
        public async void Execute(object? parameter)
        {
            // 1. Safe Casting: Ensure parameter is actually T
            if (!IsValidParameter(parameter, out var validParam))
                return;

            // 2. Concurrency Lock
            if (_isExecuting) return;

            _isExecuting = true;
            RaiseCanExecuteChanged(); // Refreshes button state (disables it)

            try
            {
                // 3. Exception Handling wrapper
                await _execute(validParam);
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged(); // Re-enables button
            }
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
        /// Raises the <see cref="CanExecuteChanged" /> event to force WPF to re-query CanExecute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Safely casts the object to type T.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <c>true</c> if [is valid parameter] [the specified parameter]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidParameter(object? parameter, out T result)
        {
            result = default!;

            // Case 1: T is a reference type or Nullable<T>, and parameter is null
            if (parameter is null && (default(T) == null))
                return true;

            // Case 2: parameter is actually T
            if (parameter is T typedParam)
            {
                result = typedParam;
                return true;
            }

            return false;
        }
    }
}
