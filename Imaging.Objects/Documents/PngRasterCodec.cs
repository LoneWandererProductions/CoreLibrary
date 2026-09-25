/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        PngRasterCodec.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;

namespace Imaging.Objects.Documents;

/// <summary>
///     PNG through GDI+. Layers can be opened in any image tool, alpha is preserved.
/// </summary>
public sealed class PngRasterCodec : IRasterCodec
{
    /// <inheritdoc />
    public string Extension => ".png";

    /// <inheritdoc />
    public bool IsCompressed => true;

    /// <inheritdoc />
    public void Write(UnmanagedImageBuffer buffer, Stream output)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(output);

        // GDI+ needs a seekable stream to encode PNG, a zip entry stream is not.
        using var bitmap = buffer.ToBitmap();
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);

        memory.Position = 0;
        memory.CopyTo(output);
    }

    /// <inheritdoc />
    public UnmanagedImageBuffer Read(Stream input, int expectedWidth, int expectedHeight)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var memory = new MemoryStream();
        input.CopyTo(memory);
        memory.Position = 0;

        try
        {
            using var bitmap = new Bitmap(memory);

            if (bitmap.Width != expectedWidth || bitmap.Height != expectedHeight)
            {
                throw new DocumentFormatException(
                    $"Raster layer is {bitmap.Width}x{bitmap.Height} but the document is {expectedWidth}x{expectedHeight}.");
            }

            return UnmanagedImageBuffer.FromBitmap(bitmap);
        }
        catch (ArgumentException ex)
        {
            // GDI+ reports undecodable data as ArgumentException.
            throw new DocumentFormatException("Raster layer is not a valid image.", ex);
        }
    }
}
