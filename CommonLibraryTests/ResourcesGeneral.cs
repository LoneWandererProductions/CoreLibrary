/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ResourcesGeneral.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace CommonLibraryTests
{
    /// <summary>
    ///     The resources general class.
    /// </summary>
    internal static class ResourcesGeneral
    {
        /// <summary>
        ///     Extension (const). Value: ".anp".
        /// </summary>
        private const string Anp = ".anp";

        /// <summary>
        ///     Extension (const). Value: ".evd".
        /// </summary>
        private const string Evd = ".evd";

        /// <summary>
        ///     Extension (const). Value: ".avd".
        /// </summary>
        private const string Avd = ".avd";

        /// <summary>
        ///     Extension (const). Value: ".xml".
        /// </summary>
        private const string Xml = ".xml";

        /// <summary>
        ///     Extension (const). Value: ".cpg".
        /// </summary>
        private const string Cpg = ".cpg";

        /// <summary>
        ///     Extension (const). Value: ".aci".
        /// </summary>
        private const string Aci = ".aci";

        /// <summary>
        ///     Extension (const). Value: ".txt".
        /// </summary>
        internal const string TstExt = ".txt";

        /// <summary>
        ///     Collection of Extensions
        /// </summary>
        internal static readonly List<string> FileExtList = new()
        {
            Anp,
            Evd,
            Avd,
            Xml,
            Cpg,
            Aci
        };

        /// <summary>
        ///     The data item one (readonly). Value: new DataItem { Number = 1, GenericText = DataItemOne, Other = 0.0 }.
        /// </summary>
        internal static readonly XmlItem DataItemOne = new()
        {
            Number = 1, GenericText = nameof(DataItemOne), Other = 0.0
        };

        /// <summary>
        ///     The data item two (readonly). Value: new DataItem { Number = 1, GenericText = DataItemTwo, Other = 0.1 }.
        /// </summary>
        internal static readonly XmlItem DataItemTwo = new()
        {
            Number = 2, GenericText = nameof(DataItemTwo), Other = 0.1
        };

        /// <summary>
        ///     The data item three (readonly). Value: new DataItem { Number = 1, GenericText = DataItemThree, Other = 0.2 }.
        /// </summary>
        internal static readonly XmlItem DataItemThree = new()
        {
            Number = 3, GenericText = nameof(DataItemThree), Other = 0.2
        };
    }
}
