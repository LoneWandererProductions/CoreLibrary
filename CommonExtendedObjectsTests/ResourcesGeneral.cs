/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/ResourcesGeneral.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     The resources general class.
    /// </summary>
    internal static class ResourcesGeneral
    {
        /// <summary>
        ///     The data item one (readonly). Value: new DataItem { Number = 1, GenericText = DataItemOne, Other = 0.0 }.
        /// </summary>
        internal static readonly XmlItem DataItemOne = new() { Number = 1, GenericText = nameof(DataItemOne), Other = 0.0 };

        /// <summary>
        ///     The data item two (readonly). Value: new DataItem { Number = 1, GenericText = DataItemTwo, Other = 0.1 }.
        /// </summary>
        internal static readonly XmlItem DataItemTwo = new() { Number = 2, GenericText = nameof(DataItemTwo), Other = 0.1 };

        /// <summary>
        ///     The data item three (readonly). Value: new DataItem { Number = 1, GenericText = DataItemThree, Other = 0.2 }.
        /// </summary>
        internal static readonly XmlItem DataItemThree = new()
        {
            Number = 3, GenericText = nameof(DataItemThree), Other = 0.2
        };
    }
}
