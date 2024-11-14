/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/ViewModelBase.cs
 * PURPOSE:     Basics for my View Model, can be reused as needed and extended
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.ComponentModel;

namespace ViewModel
{
    /// <inheritdoc />
    /// <summary>
    ///     Basic stuff for my View Models
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Sets the property and raises PropertyChanged if the value changes.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to the field storing the property value.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property (optional, automatically provided by caller).</param>
        protected void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
