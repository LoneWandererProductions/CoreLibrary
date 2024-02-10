/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/SerialResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Serializer
{
    /// <summary>
    ///     The serial resources class.
    /// </summary>
    internal static class SerialResources
    {
        /// <summary>
        ///     Separator (const). Value: " , ".
        /// </summary>
        internal const string Separator = " , ";

        /// <summary>
        ///     Error property was not found (const). Value: "Could not find Property".
        /// </summary>
        internal const string ErrorPropertyNotFound = "Could not find Property";

        /// <summary>
        ///     Error in path (const). Value: "No File Found: ".
        /// </summary>
        internal const string ErrorPath = "No File Found: ";

        /// <summary>
        ///     Error File is empty (const). Value: "File was empty: ".
        /// </summary>
        internal const string ErrorFileEmpty = "File was empty: ";

        /// <summary>
        ///     Error could not File was empty (const). Value: "Object was Empty: ".
        /// </summary>
        internal const string ErrorSerializerEmpty = "Object was Empty: ";

        /// <summary>
        ///     Error string in serializer XML.
        /// </summary>
        internal const string ErrorSerializerXml = "Could not Serialize, Error in XML: ";
    }
}
