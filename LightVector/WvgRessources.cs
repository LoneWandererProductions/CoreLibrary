/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/VgProcessing.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace LightVector;

/// <summary>
///     The wvg resources class.
/// </summary>
internal static class WvgResources
{
    /// <summary>
    ///     The error path (const). Value: @"Empty Path provided".
    /// </summary>
    internal const string ErrorPath = "Empty Path provided";

    /// <summary>
    ///     The error serializer (const). Value: @"Could not Serialize: ".
    /// </summary>
    internal const string ErrorSerializer = "Could not Serialize: ";

    /// <summary>
    ///     The error serializer empty (const). Value: @"Object was Empty: ".
    /// </summary>
    internal const string ErrorSerializerEmpty = "Object was Empty: ";

    /// <summary>
    ///     The error de serializer (const). Value: @"Could not DeSerializer: ".
    /// </summary>
    internal const string ErrorDeSerializer = "Could not DeSerializer: ";

    /// <summary>
    ///     The error file empty (const). Value: @"File was empty: ".
    /// </summary>
    internal const string ErrorFileEmpty = "File was empty: ";
}
