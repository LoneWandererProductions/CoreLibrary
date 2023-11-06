/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ViewModel
 * FILE:        ViewModel/ObservableObject.cs
 * PURPOSE:     MVVM and Observable Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;

namespace ViewModel
{
    /// <inheritdoc />
    /// <summary>
    ///     The observable object class.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        ///     The property changed event of the <see cref="T:System.ComponentModel.PropertyChangedEventHandler" />.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The raise property changed event.
        /// </summary>
        /// <param name="propertyName">The propertyName.</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
