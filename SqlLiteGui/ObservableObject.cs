/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/ObservableObject.cs
 * PURPOSE:     Reused for SQLiteGui Project only
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     The observable object class.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        ///     The property changed event of the <see cref="PropertyChangedEventHandler" />.
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
