/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/XMLItem.cs
 * PURPOSE:     Test Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeInternal, if we make it Internal Serialization will fail!

namespace CommonLibraryTests
{
    /// <summary>
    ///     The coordinates class.
    /// </summary>
    public sealed class XmlItem
    {
        /// <summary>
        ///     Row as X
        /// </summary>
        public int Number { get; init; }

        /// <summary>
        ///     Column as Y
        /// </summary>
        public string GenericText { get; init; }

        /// <summary>
        ///     Layer of Map
        /// </summary>
        public double Other { get; init; }
    }
}
