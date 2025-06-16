/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/DataItem.cs
 * PURPOSE:     Basic Object needed for DataList, copy from CommonControls to safe a reference.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ArrangeBraces_foreach
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     Data Object
    /// </summary>
    internal sealed class DataItem
    {
        /// <summary>
        ///     Gets or sets the id.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        internal string Name { get; set; }

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
            return HashCode.Combine(Id);
        }
    }
}
