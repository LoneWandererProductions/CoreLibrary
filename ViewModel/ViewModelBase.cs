/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/ViewModelBase.cs
 * PURPOSE:     Basics for my View Model, can be reused as needed and extended
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ViewModel
{
    /// <summary>
    ///     Base class for all ViewModels.
    ///     Implements <see cref="INotifyPropertyChanged"/> and utility setters.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Sets a field and raises PropertyChanged if the value has changed.
        /// </summary>
        /// <typeparam name="T">Type of the field/property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Optional name of the property (auto-filled by compiler).</param>
        /// <returns>True if the value was changed, false if it was the same.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Sets a field, raises PropertyChanged, and invokes an optional callback.
        /// </summary>
        /// <typeparam name="T">Type of the field/property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="callback">Optional callback to invoke after setting the value.</param>
        /// <param name="propertyName">Optional name of the property (auto-filled by compiler).</param>
        /// <returns>True if the value was changed, false if it was the same.</returns>
        protected bool SetPropertyAndCallback<T>(
            ref T field,
            T value,
            Action<T>? callback = null,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            callback?.Invoke(value);
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Raises multiple dependent property change notifications.
        /// </summary>
        /// <param name="propertyNames">Array of property names to raise notifications for.</param>
        protected void RaisePropertyChangedFor(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                OnPropertyChanged(propertyName);
        }

        /// <summary>
        ///     Thread-safe version of <see cref="OnPropertyChanged"/> for async operations.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChangedAsync([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            SynchronizationContext? ctx = SynchronizationContext.Current;
            if (ctx != null)
            {
                ctx.Post(_ => handler(this, new PropertyChangedEventArgs(propertyName)), null);
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Determines if a command can execute (default always true).
        /// </summary>
        /// <param name="obj">Command parameter.</param>
        /// <returns>True if executable.</returns>
        protected bool CanExecute(object obj) => true;

        /// <summary>
        ///     Determines if a generic command can execute (default always true).
        /// </summary>
        /// <typeparam name="T">Type of the command parameter.</typeparam>
        /// <param name="obj">Command parameter.</param>
        /// <returns>True if executable.</returns>
        protected bool CanExecute<T>(T obj) => true;
    }
}
