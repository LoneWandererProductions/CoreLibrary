/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/ObservableObject.cs
 * PURPOSE:     MVVM and Observable Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ViewModel
{
    /// <inheritdoc />
    /// <summary>
    ///     A base class that implements <see cref="INotifyPropertyChanged" /> to provide property change notifications.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
