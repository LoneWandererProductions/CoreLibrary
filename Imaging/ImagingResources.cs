/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImagingResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Imaging
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    internal static class ImagingResources
    {
        /// <summary>
        ///     The error missing file (const). Value: "File not Found: ".
        /// </summary>
        internal const string ErrorMissingFile = "File not Found: ";

        /// <summary>
        ///     Error, wrong parameters (const). Value: "Wrong Arguments provided".
        /// </summary>
        internal const string ErrorWrongParameters = "Wrong Arguments provided: ";

        /// <summary>
        ///     Memory Error (const). Value: " used Memory: ".
        /// </summary>
        internal const string ErrorMemory = " used Memory: ";

        /// <summary>
        ///     The Spacing (const). Value:  " : ".
        /// </summary>
        internal const string Spacing = " : ";

        /// <summary>
        ///     The Separator (const). Value:  ','.
        /// </summary>
        internal const char Separator = ',';

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
    }
}
