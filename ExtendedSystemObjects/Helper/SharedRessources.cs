/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        ExtendedSystemObjects.Helper/SharedResources.cs
 * PURPOSE:     Generic System Functions for ListsCollection of Strings and constants.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace ExtendedSystemObjects.Helper
{
    /// <summary>
    ///     The extended system objects Resources class.
    /// </summary>
    internal static class SharedResources
    {
        /// <summary>
        ///     Error value not found (const). "Value not found in the dictionary".
        /// </summary>
        internal const string ErrorValueNotFound = "Value not found in the dictionary";

        /// <summary>
        ///     Error no value found (const). Value: "Values not found in the dictionary".
        /// </summary>
        internal const string ErrorNoValueFound = "Values not found in the dictionary";

        /// <summary>
        ///     Error no value found (const). Value: "Value is not sane".
        /// </summary>
        internal const string ErrorValueNotAllowed = "Value is not sane";

        /// <summary>
        ///     Error Key Exists (const). Value: "Key already exists: ".
        /// </summary>
        internal const string ErrorKeyExists = "Key already exists: ";

        /// <summary>
        ///     Error Value Exists (const). "Value already exists: ".
        /// </summary>
        internal const string ErrorValueExists = "Value already exists: ";

        /// <summary>
        ///     The error duplicate key (const). "Duplicate key detected: {key}".
        /// </summary>
        internal const string ErrorDuplicateKey = "Duplicate key detected: {key}";

        /// <summary>
        ///     Separator(const). Value: " , ".
        /// </summary>
        internal const string Separator = " , ";

        /// <summary>
        ///     The empty flag.
        /// </summary>
        internal const byte Empty = 0;

        /// <summary>
        ///     The occupied flag.
        /// </summary>
        internal const byte Occupied = 1;

        /// <summary>
        ///     The tombstone flag.
        /// </summary>
        internal const byte Tombstone = 2;

        /// <summary>
        ///     The small primes collection, is used in fast ImmutableLookupMap and ImmutableLookupMapUnmanaged.
        /// </summary>
        internal static readonly int[] SmallPrimes =
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101,
            103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199
        };
    }
}
