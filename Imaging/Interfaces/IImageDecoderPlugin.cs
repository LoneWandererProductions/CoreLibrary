/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Interfaces
 * FILE:        IImageDecoderPlugin.cs
 * PURPOSE:     Contract for a plugin that decodes an image format the core
 *              Imaging pipeline doesn't natively understand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Drawing;

namespace Imaging.Interfaces
{
    /// <summary>
    ///     Implement this to add support for a new image type without touching
    ///     the core Imaging pipeline. Drop the compiled plugin DLL into the
    ///     Plugins folder next to the app; <see cref="ImageDecoderPluginRegistry" />
    ///     finds and loads it at startup.
    /// </summary>
    /// <remarks>
    ///     Shaped to match <c>ImageStream.GetOriginalBitmap(string)</c> - the
    ///     built-in decode path every plugin sits alongside - so writing one feels
    ///     the same as the code already decoding jpg/png/bmp/gif/tif today.
    ///     The Imaging.Cifs project's CIF format already does exactly this by
    ///     hand, as a hard-referenced special case; this interface is that same
    ///     shape, generalized so a new format doesn't need one.
    /// </remarks>
    public interface IImageDecoderPlugin
    {
        /// <summary>
        ///     Short, unique, human-readable name. Used only for diagnostics/logging
        ///     when a plugin fails to load or fails to decode a file.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     File extensions this plugin decodes, each including the leading dot
        ///     and lower-cased (e.g. ".webp"). These get merged into
        ///     <see cref="ImagingResources.Appendix" /> on load, so the rest of the
        ///     app - file dialogs, folder scans, the converter tool, thumbnail
        ///     generation - picks the format up automatically without changes.
        /// </summary>
        IReadOnlyCollection<string> SupportedExtensions { get; }

        /// <summary>
        ///     Decode the file at <paramref name="path" /> into a fully independent
        ///     Bitmap (no lingering reference to the source file or stream - same
        ///     contract as <c>ImageStream.GetOriginalBitmap</c>).
        /// </summary>
        /// <param name="path">Full path to the file to decode.</param>
        /// <returns>The decoded image.</returns>
        /// <exception cref="System.Exception">
        ///     Throw on failure rather than returning null - the caller logs and
        ///     handles it exactly like a native decode failure.
        /// </exception>
        Bitmap Decode(string path);
    }
}
