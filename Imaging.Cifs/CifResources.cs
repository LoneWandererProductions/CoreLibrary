/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Cifs
 * FILE:        CifResources.cs
 * PURPOSE:     String Resources
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Imaging.Cifs
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    public static class CifResources
    {
        /// <summary>
        ///     The error missing file (const). Value: "File not Found: ".
        /// </summary>
        internal const string ErrorFileNotFound = "File not Found: ";

        /// <summary>
        ///     The Spacing (const). Value:  " : ".
        /// </summary>
        internal const string Spacing = " : ";

        /// <summary>
        ///     The Separator (const). Value:  ','.
        /// </summary>
        internal const char Separator = ',';

        /// <summary>
        ///     The Interval Splitter (const). Value: "-".
        /// </summary>
        internal const string IntervalSplitter = "-";

        /// <summary>
        ///     Separator (const). Value: " , ".
        /// </summary>
        internal const string Indexer = " , ";

        /// <summary>
        ///     Color string (const). Value: "Color: ".
        /// </summary>
        internal const string Color = "Color: ";

        /// <summary>
        ///     The flag that indicates that image is not compressed (const). Value:  "0".
        /// </summary>
        internal const string CifUnCompressed = "0";

        /// <summary>
        ///     The flag that indicates if image is compressed (const). Value:  "1".
        /// </summary>
        internal const string CifCompressed = "1";

        /// <summary>
        ///     The cif Separator used for compression (const). Value:  "-".
        /// </summary>
        internal const string CifSeparator = "-";

        /// <summary>
        ///     The error, interface is null (const). Value: "Error: Interface is Null."
        /// </summary>
        internal const string ErrorInterface = "Error: Interface is Null.";

        /// <summary>
        ///     The error, image is null (const). Value: "Error: Image is Null."
        /// </summary>
        internal const string ErrorImage = "Error: Image is Null.";

        /// <summary>
        ///     The error, Path is null (const). Value: "Error: Path is Null."
        /// </summary>
        internal const string ErrorPath = "Error: Path is Null.";
    }
}
