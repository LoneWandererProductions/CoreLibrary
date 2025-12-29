/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        DataItem.cs
 * PURPOSE:     Basic Object needed for DataList
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ArrangeBraces_foreach
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System;
using ViewModel;

namespace CommonControls
{
    /// <inheritdoc cref="ObservableObject" />
    /// <summary>
    ///     Data Object
    /// </summary>
    public sealed class DataItem : ObservableObject, IEquatable<DataItem>
    {
        /// <summary>
        ///     The id.
        /// </summary>
        private int _id;

        /// <summary>
        ///     The name.
        /// </summary>
        private string _name;

        /// <summary>
        ///     Gets or sets the id.
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                {
                    return;
                }

                _id = value;
                RaisePropertyChangedEvent(nameof(Id));
            }
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChangedEvent(nameof(Name));
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(DataItem other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _id == other._id && _name == other._name;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is DataItem other && Equals(other));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_id);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{_id}{ComCtlResources.Separator}{_name}";
        }
    }
}
