/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Plugins.Interface
 * FILE:        IImageEncoderPlugin.cs
 * PURPOSE:     Contract for a plugin that encodes (saves) an image format the
 *              core Imaging pipeline doesn't natively write.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;

namespace Imaging.Plugins.Interface
{
    /// <summary>
    ///     Implement this to add the ability to *save* a format, mirroring
    ///     <see cref="IImageDecoderPlugin" /> for the write side. Separate from
    ///     the decoder interface on purpose: a plugin can read a format,
    ///     write it, or both - a decode-only plugin (e.g. one that only ever
    ///     needs to *display* a legacy format) shouldn't be forced to also
    ///     implement writing it back out.
    /// </summary>
    /// <remarks>
    ///     A single class is free to implement both <see cref="IImageDecoderPlugin" />
    ///     and this interface - the registry checks each loaded instance against
    ///     both and registers it wherever it fits, so one plugin DLL can cover a
    ///     format's full read/write round-trip.
    /// </remarks>
    public interface IImageEncoderPlugin
    {
        /// <summary>
        ///     Short, unique, human-readable name. Used only for diagnostics/logging
        ///     when a plugin fails to load or fails to encode a file.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     File extensions this plugin can write, each including the leading dot
        ///     and lower-cased (e.g. ".webp"). These get merged into
        ///     <see cref="ImagingResources.Appendix" /> on load, same as a decoder
        ///     plugin's extensions - so a format that's write-only (or read+write)
        ///     still shows up everywhere Appendix drives the UI.
        /// </summary>
        IReadOnlyCollection<string?> SupportedExtensions { get; }

        /// <summary>
        ///     Encode <paramref name="bitmap" /> and write it to <paramref name="path" />.
        /// </summary>
        /// <param name="bitmap">The image to encode.</param>
        /// <param name="path">Full destination path, extension already set by the caller.</param>
        /// <exception cref="Exception">
        ///     Throw on failure rather than returning a bool - the caller logs and
        ///     handles it exactly like a built-in encode failure.
        /// </exception>
        void Encode(Bitmap? bitmap, string? path);
    }
}