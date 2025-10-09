/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/DelegateCommand.cs
 * PURPOSE:     Part of the View Model, Generic Variation of the ICommand Implementation.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://msdn.microsoft.com/de-de/library/system.windows.input.icommand%28v=vs.110%29.aspx
 *              https://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
 *              https://stackoverflow.com/questions/48527651/full-implementation-of-relay-command-can-it-be-applied-to-all-cases
 */

// ReSharper disable UnusedMember.Global

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
        ///     The action to execute.
        /// </summary>
        private readonly Action<T> _action;

        /// <summary>
        ///     The predicate to determine if the command can execute.
        /// </summary>
        private readonly Predicate<T>? _canExecute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateCommand{T}" /> class.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="canExecute">
        ///     A predicate to determine if the command can execute. If null, the command is always
        ///     executable.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
        public DelegateCommand(Action<T> action, Predicate<T>? canExecute = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter for the action.</param>
        public void Execute(object parameter)
        {
            _action((T)parameter);
        }

        /// <inheritdoc />
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
        ///     Raises the <see cref="CanExecuteChanged"/> event to force WPF to re-query CanExecute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <inheritdoc />
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
