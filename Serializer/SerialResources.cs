/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/SerialResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Serializer;

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
    ///     Error in path (const). Value: "File does not exist:".
    /// </summary>
    internal const string ErrorPath = "File does not exist:";

    /// <summary>
    ///     Error File is empty (const). Value: "File is empty: ".
    /// </summary>
    internal const string ErrorFileEmpty = "File is empty: ";

    /// <summary>
    ///     The error object null (const). Value: "Object cannot be null.".
    /// </summary>
    internal const string ErrorObjectNull = "Object cannot be null.";

    /// <summary>
    ///     The error dictionary null (const). Value: "Dictionary cannot be null.".
    /// </summary>
    internal const string ErrorDictionaryNull = "Dictionary cannot be null.";
}
