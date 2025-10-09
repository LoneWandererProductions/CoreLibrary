/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/RelayCommand.cs
 * PURPOSE:     Part of the View Model, non-generic ICommand implementation.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows.Input;

namespace ViewModel
{
    /// <summary>
    ///     Simple ICommand implementation that relays Execute and CanExecute to delegates.
    ///     Can be used as a drop-in replacement for buttons and other command sources in WPF.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        /// <summary>
        ///     Delegate executed when the command is invoked.
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        ///     Delegate invoked to determine if the command can execute.
        /// </summary>
        private readonly Func<bool>? _canExecute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">Optional delegate to determine if execution is allowed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        ///     WPF command sources listen to this event to refresh their enabled state.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether this <see cref="RelayCommand"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter">Unused parameter (required by ICommand).</param>
        /// <returns>True if command can execute, otherwise false.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <inheritdoc />
        /// <summary>
        ///     Executes the <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="parameter">Unused parameter (required by ICommand).</param>
        public void Execute(object? parameter) => _execute();

        /// <summary>
        ///     Raises the <see cref="CanExecuteChanged"/> event to notify the UI to requery command state.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
